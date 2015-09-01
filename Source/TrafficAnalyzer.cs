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


					EntityInfo info;
                    info.type = EntityType.Vehicle;
					info.id = id;
                    info.path = this.GatherPathVerticies(vehicles[id].m_path);
					info.serviceType = vehicles[id].Info.GetService().ToString() + "/" +  vehicles[id].Info.GetSubService().ToString();
					
					info.sourceBuilding = vehicles[id].m_sourceBuilding;
					info.targetBuilding = vehicles[id].m_targetBuilding;

                    Report report = new Report(info);

                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotReport(report));

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


                    EntityInfo info;
                    info.type = EntityType.Citzen;
					info.id = id;
                    info.path = this.GatherPathVerticies(citzens[id].m_path);
					info.serviceType = "citzen";
					
					info.sourceBuilding = citzens[id].m_sourceBuilding;
					info.targetBuilding = citzens[id].m_targetBuilding;

                    Report report = new Report(info);

                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotReport(report));

                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
            });
        }

		public void ReportOnSegment(ushort segmentID, NetInfo.Direction dir)
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
                    Report report = this.DoReportOnSegment(segmentID, dir);
                    working = false;
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotReport(report));
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
                    ThreadHelper.dispatcher.Dispatch(() => tool.OnGotReport(report));
                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
            });
        }

		private Report DoReportOnSegment(ushort segmentID, NetInfo.Direction dir)
        {

            List<EntityInfo> enities = new List<EntityInfo>();

            Log.debug("Looking at vehicles...");
            Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;
            for (uint i = 0; i < vehicles.Length; i++)
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

				if (this.PathContainsSegment(vehicles[i].m_path, segmentID, dir))
                {
                    Log.info("Found vehicle on segemnt, getting path....");

                    EntityInfo info;
                    info.type = EntityType.Vehicle;
					info.id = i;
                    info.path = this.GatherPathVerticies(vehicles[i].m_path);
					
					info.sourceBuilding = vehicles[i].m_sourceBuilding;
					info.targetBuilding = vehicles[i].m_targetBuilding;

					info.serviceType = vehicles[i].Info.GetService().ToString() + "/" +  vehicles[i].Info.GetSubService().ToString();
                    enities.Add(info);
                }
            }

            Log.debug("found " + enities.Count + " entities");

            Log.debug("Looking at citzens...");
            CitizenInstance[] citzens = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            for (uint i = 0; i < citzens.Length; i++)
            {
                if((citzens[i].m_flags & CitizenInstance.Flags.Deleted) != CitizenInstance.Flags.None) {
                    continue;
                }

                if (citzens[i].m_path == 0)
                {
                    continue;
                }

				if (this.PathContainsSegment(citzens[i].m_path, segmentID, dir))					
                {
                    Log.info("Found citizen on segemnt, getting path....");

                    EntityInfo info;
                    info.type = EntityType.Citzen;
					info.id = i;
                    info.path = this.GatherPathVerticies(citzens[i].m_path);
					info.serviceType = "citizen";
					info.sourceBuilding = citzens[i].m_sourceBuilding;
					info.targetBuilding = citzens[i].m_targetBuilding;

                    enities.Add(info);
                    Log.info("Got Path");
                }

            }

            Log.debug("found " + enities.Count + " entities");

            Log.debug("End DoReportOnSegment");

            return new Report(enities.ToArray());
            
        }

        private Report DoReportOnBuilding(ushort buildingID)
        {
            List<EntityInfo> enities = new List<EntityInfo>();

            Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;

            for (uint i = 0; i < vehicles.Length; i++)
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

                    EntityInfo info;
                    info.type = EntityType.Vehicle;
					info.id = i;
                    info.path = this.GatherPathVerticies(vehicles[i].m_path);
					info.sourceBuilding = vehicles[i].m_sourceBuilding;
					info.targetBuilding = vehicles[i].m_targetBuilding;
					
					info.serviceType = vehicles[i].Info.GetService().ToString() + "/" +  vehicles[i].Info.GetSubService().ToString();
                    enities.Add(info);
                }
            }

            CitizenInstance[] citzens = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            for (uint i = 0; i < citzens.Length; i++)
            {
                if ((citzens[i].m_flags & CitizenInstance.Flags.Deleted) != CitizenInstance.Flags.None)
                {
                    continue;
                }

                if (citzens[i].m_path == 0)
                {
                    continue;
                }

                if(citzens[i].m_sourceBuilding == buildingID || citzens[i].m_targetBuilding == buildingID) {
                    EntityInfo info;
                    info.type = EntityType.Citzen;
                    info.id = i;
                    info.path = this.GatherPathVerticies(citzens[i].m_path);
                    info.serviceType = "citzen";

                    info.sourceBuilding = citzens[i].m_sourceBuilding;
                    info.targetBuilding = citzens[i].m_targetBuilding;

                    enities.Add(info);
                }

            }

           // Log.debug("End DoReportOnSegment");

            return new Report(enities.ToArray());
        }
		
		

		private bool PathContainsSegment(uint pathID, ushort segmentID, NetInfo.Direction dir)
		{
			NetManager instance = Singleton<NetManager>.instance;
			PathUnit path = this.getPath(pathID);

			while (true) {
				for (int i = 0; i < path.m_positionCount; i++) {
					PathUnit.Position p = path.GetPosition(i);
					if (p.m_segment == segmentID) {

						if (dir == NetInfo.Direction.Both) {
							return true;
						}

						NetInfo info = instance.m_segments.m_buffer[(int)p.m_segment].Info;
						NetInfo.Direction laneDir = NetInfo.Direction.None;

						if (info.m_lanes.Length > (int)p.m_lane) {
							laneDir = info.m_lanes[(int)p.m_lane].m_finalDirection;
							if ((instance.m_segments.m_buffer[(int)p.m_segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) {
								laneDir = NetInfo.InvertDirection(laneDir);
							}
						} else {
                                                        Log.debug("bad lane count");
						}

						if (laneDir == dir) {
							return true;
						}
//						} else if ((laneDir != NetInfo.Direction.Forward) && (laneDir != NetInfo.Direction.Backward)) {
		//					Debug.Log("laneDir = " + laneDir);
		//				}
					}
				}

				if (path.m_nextPathUnit == 0) {
					return false;
				}

				path = this.getPath(path.m_nextPathUnit);
			}
		}

		PathUnit getPath(uint id)
        {
            return Singleton<PathManager>.instance.m_pathUnits.m_buffer[id];
        }

        PathUnit.Position[] GatherPathPositions(uint pathID)
        {
            Log.debug("Gathering path positions ...");

            PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
			List<PathUnit.Position> positions = new List<PathUnit.Position>();
            while (true)
            {
				for (int i = 0; i < paths[pathID].m_positionCount; i++)
                {
					PathUnit.Position p = paths[pathID].GetPosition(i);
                    positions.Add(p);
                }

                if (paths[pathID].m_nextPathUnit == 0)
                {
                    Log.debug("Done");
                    return positions.ToArray();
                }

                pathID = paths[pathID].m_nextPathUnit;
            }
        }

        //PathPoint[] GatherPathVerticies(uint pathID)
        //{
        //    List<PathPoint> path = new List<PathPoint>();

        //    NetSegment[] segments = netMan.m_segments.m_buffer;
        //    NetNode[] nodes = netMan.m_nodes.m_buffer;
        //    PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
        //    NetLane[] lanes = Singleton<NetManager>.instance.m_lanes.m_buffer;


        //    PathUnit.Position[] positions = GatherPathPositions(pathID);

            
        //    Vector3 pos, direction;
        //    PathPoint newPoint;

        //    pos = PathManager.CalculatePosition(positions[0]);

        //    for (int i = 0; i < positions.Length; i++)
        //    {

        //        uint laneID = PathManager.GetLaneID(positions[i]);
        //        Vector3 laneStart, laneEnd;

        //        lanes[laneID].CalculatePositionAndDirection(0, out laneStart, out direction);
        //        lanes[laneID].CalculatePositionAndDirection(0, out laneEnd, out direction);

        //        float startOfset = positions[i].m_offset / 255.0f;
                
        //        float step = 0.2f;
        //        if((laneStart-pos).magnitude < (laneEnd-pos).magnitude) {
        //            step = - 0.2f;

        //            if (i != 0) {
        //                startOfset = 1;
        //            }
        //        }
        //        else
        //        {
        //            if (i != 0)
        //            {
        //                startOfset = 0;
        //            }
        //        }

                
        //        for(float j=startOfset; j <= 1 && j >= 0; j += step) {

        //            lanes[laneID].CalculatePositionAndDirection(j, out pos, out direction);
                                   
        //            newPoint.x = pos.x;
        //            newPoint.y = pos.y;
        //            newPoint.z = pos.z;
        //            newPoint.segmentID = positions[i].m_segment;

        //            path.Add(newPoint);
        //        }

        //    }

        //    return path.ToArray();
        //}

		PathPoint[] GatherPathVerticies(uint pathID)
        {
			List<PathPoint> path = new List<PathPoint>();

			NetSegment[] segments = netMan.m_segments.m_buffer;
			NetNode[] nodes = netMan.m_nodes.m_buffer;
            NetLane[] lanes = Singleton<NetManager>.instance.m_lanes.m_buffer;
            
            PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
			uint segment = paths[pathID].GetPosition(0).m_segment;
			uint startNode, endNode;
			startNode = segments[segment].m_startNode;


            PathUnit.Position[] positions = GatherPathPositions(pathID);

            Log.debug("Generating verticies...");
            
            for (int i = 0; i < positions.Length; i++) {

                Vector3 pv, dir;
                PathPoint newPoint = new PathPoint();
                
                uint laneID = PathManager.GetLaneID(positions[i]);
                    
                lanes[laneID].CalculatePositionAndDirection(positions[i].m_offset / 255.0f,out pv, out dir);

                newPoint.pos = pv;
                newPoint.normal = (positions[i].m_offset < 128) ? -dir : dir;
                newPoint.segmentID = positions[i].m_segment;

				path.Add(newPoint);

                if (i < positions.Length - 2) {
                    if (positions[i].m_segment != positions[i + 1].m_segment) {

                        newPoint = new PathPoint();

                        PathUnit.Position nextPos = positions[i + 1];
                        laneID = PathManager.GetLaneID(nextPos);

                        
                        Vector3 pvA, dirA, pvB,dirB;
                        lanes[laneID].CalculatePositionAndDirection(0, out pvA, out dirA);
                        lanes[laneID].CalculatePositionAndDirection(1, out pvB, out dirB);

                        if (pvA == pv || pvB == pv)
                            continue;

                        newPoint.normal = Vector3.down;

                        //Find the closest lane end of the next segment
                        if ((pvA - pv).magnitude < (pvB - pv).magnitude)
                        {
                            newPoint.pos = pvA;
                            newPoint.normal = dirA;
                            
                        }
                        else
                        {
                            newPoint.pos = pvB;
                            newPoint.normal = -dirB;
                        }                      
                        
                        newPoint.segmentID = positions[i + 1].m_segment;

                        path.Add(newPoint);
                        
                    }
                }

            }

            return path.ToArray();
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
