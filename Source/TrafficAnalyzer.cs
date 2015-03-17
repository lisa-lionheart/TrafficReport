using ColossalFramework;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TrafficReport
{
    class TrafficAnalyzer 
    {
        public struct Report
        {
            public List<Vector3[]> paths;
        }


        VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
        NetManager netMan = Singleton<NetManager>.instance;
        QueryTool tool;
        bool working;

        public TrafficAnalyzer(QueryTool _tool)
        {
            working = false;
            tool = _tool;
        }

        public void ReportOnVehicle(ushort id)
        {
            if (working)
            {
                Log.warn("Job in progress bailing!");
                return;
            }
            Log.info("badger");

            working = true;

            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                try
                {
                    Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;

                    Log.info("Vechicle Name: " + vehicles[id].Info.name);

					Vector3[] path = this.GatherPathVerticies(vehicles[id].m_path);

                    this.DumpPath(path);


                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotSinglePathReport(path));

                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
            });
            
        }

        internal void ReportOnCitizen(ushort id)
        {

            if (working)
            {
                Log.warn("Job in progress bailing!");
                return;
            }
            Log.info("badger");

            working = true;

            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                try
                {
                    CitizenInstance[] citzens =  Singleton<CitizenManager>.instance.m_instances.m_buffer;

                    Log.info("Vechicle Name: " + citzens[id].Info.name);

                    Vector3[] path = this.GatherPathVerticies(citzens[id].m_path);

                    this.DumpPath(path);

                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotSinglePathReport(path));

                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
            });
        }

        public void ReportOnSegment(ushort segmentID)
        {
            if (working)
            {
                Log.warn("Job in progress bailing!");
                return;
            }

            working = true;

            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                try //
                {
                    Report report = this.DoReportOnSegment(segmentID);
                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotSegmentReport(report));
                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }                
            });
        }

        public void ReportOnBuilding(ushort buildingID)
        {
            if (working)
            {
                Log.warn("Job in progress bailing!");
                return;
            }

            working = true;

            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                try
                {
                    Report report = this.DoReportOnBuilding(buildingID);
                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotBuildingReport(report));
                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
            });
        }

        private Report DoReportOnSegment(ushort segmentID)
        {
            Report report;
            report.paths = new List<Vector3[]>();

            Log.debug("Looking at vehicles...");
            Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;
            for (int i = 0; i < vehicles.Length; i++)
            {

                if ((vehicles[i].m_flags & Vehicle.Flags.Deleted) != Vehicle.Flags.None)
                {
                    continue;
                }

                //Log.debug("Analyzing vehicle" + vehicles[i].Info.GetLocalizedDescriptionShort());

                if (vehicles[i].m_path == 0)
                {
                    continue;
                }

                //Log.info("Vehcile valid, checking if path intersects segment...");

                if (this.PathContainsSegment(vehicles[i].m_path, segmentID))
                {
                    Log.info("Found vehicle on segemnt, getting path....");
                    Vector3[] path = this.GatherPathVerticies(vehicles[i].m_path);
                    report.paths.Add(path);
                    Log.info("Got Path");
                }
            }

            Log.debug("Looking at citzens...");
            CitizenInstance[] citzens = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            for (int i = 0; i < citzens.Length; i++)
            {
                if((citzens[i].m_flags & CitizenInstance.Flags.Deleted) != CitizenInstance.Flags.None) {
                    continue;
                }

                if (citzens[i].m_path == 0)
                {
                    continue;
                }

                if (this.PathContainsSegment(citzens[i].m_path, segmentID))
                {
                    Log.info("Found citizen on segemnt, getting path....");
                    Vector3[] path = this.GatherPathVerticies(citzens[i].m_path);
                    report.paths.Add(path);
                    Log.info("Got Path");
                }

            }

            Log.debug("End DoReportOnSegment");

            return report;
        }

        private Report DoReportOnBuilding(ushort buildingID)
        {
            Report report;
            report.paths = new List<Vector3[]>();

            Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;

            for (int i = 0; i < vehicles.Length; i++)
            {

                if ((vehicles[i].m_flags & Vehicle.Flags.Deleted) != Vehicle.Flags.None)
                {
                    continue;
                }
                
                if (vehicles[i].m_path == 0)
                {
                    continue;
                }

                //Log.info("Vehcile valid, checking if path intersects segment...");


                if (vehicles[i].m_sourceBuilding == buildingID || vehicles[i].m_targetBuilding == buildingID)
                {

                    //Log.info("Found vehicle associated with building, getting path....");
                    Vector3[] path = this.GatherPathVerticies(vehicles[i].m_path);
                    report.paths.Add(path);
                    //Log.info("Got Path");
                }
            }

           // Log.debug("End DoReportOnSegment");

            return report;
        }

        private bool PathContainsSegment(uint pathID, ushort segmentID)
        {
            PathUnit path = this.getPath(pathID);
           
            while (true)
            {
                for (int i = 0; i < path.m_positionCount; i++)
                {
                    PathUnit.Position p = path.GetPosition(i);
                    if (p.m_segment == segmentID)
                    {
                        return true;
                    }
                }

                if (path.m_nextPathUnit == 0)
                {
                    return false;
                }
                path = this.getPath(path.m_nextPathUnit);
            }
        }

        PathUnit getPath(uint id)
        {
            return Singleton<PathManager>.instance.m_pathUnits.m_buffer[id];
        }

        Vector3[] GatherPathVerticies(uint pathID)
        {
            List<Vector3> verts = new List<Vector3>();

			PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
			NetSegment[] segments = netMan.m_segments.m_buffer;
			NetNode[] nodes = netMan.m_nodes.m_buffer;

            //Log.debug("Gathering path...");

			uint segment = paths[pathID].GetPosition(0).m_segment;
			uint startNode, endNode;
			startNode = segments[segment].m_startNode;
            //verts.Add(lastPoint);
            while (true)
            {
				for (int i = 0; i < paths[pathID].m_positionCount; i++)
                {
					PathUnit.Position p = paths[pathID].GetPosition(i);

                    if (p.m_segment != 0)
                    {
                        segment = p.m_segment;
						startNode = segments[segment].m_startNode;
                        endNode = segments[segment].m_endNode;

                        Vector3 startPos = nodes[startNode].m_position;// +(Vector3.Cross(Vector3.up, segment.m_startDirection) * 5.0f); 
                        Vector3 endPos = nodes[endNode].m_position;// +(Vector3.Cross(Vector3.up, segment.m_endDirection) * -5.0f);

                        verts.Add(Vector3.Lerp(startPos,endPos,(float)p.m_offset / 255.0f)); 
                       // verts.Add(endPos);
                        //List<Vector3> segmentVerts = new List<Vector3>();

                        //verts.Add(startNode.m_position);
                        /*
                        if (!NetSegment.IsStraight(startNode.m_position, segment.m_startDirection, endNode.m_position, segment.m_endDirection))
                        {
                            Vector3 mp1, mp2;
                            NetSegment.CalculateMiddlePoints(
                                    startNode.m_position, segment.m_startDirection, 
                                    endNode.m_position, segment.m_endDirection, 
                                    true, true, out mp1, out mp2);
                            verts.Add(mp1);
                            verts.Add(mp2);
                        }*/
                        //verts.Add(endNode.m_position);
                    }
                }

				if (paths[pathID].m_nextPathUnit == 0)
                {
                    //Log.debug("Done");
                    return verts.ToArray();
                }

				pathID = paths[pathID].m_nextPathUnit;
            }
        }

        void DumpPath(Vector3[] path)
        {
            string filename = "path.txt";
            //Log.debug("Dumping path to " + filename);

            StreamWriter fs = new StreamWriter(filename);
            for (int i = 0; i < path.Length; i++)
            {
                fs.WriteLine(path[i].x + " " + path[i].y + " " + path[i].z);
            }
            fs.Close();
        }


       
    }
}
