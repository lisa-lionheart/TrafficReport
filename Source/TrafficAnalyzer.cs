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

           // Log.debug("End DoReportOnSegment");

            return new Report(enities.ToArray());
        }
		/*
		private bool PathContainsSegment(uint pathID, ushort segmentID, Vector3 dir)
		{
			PathUnit path = this.getPath(pathID);
			int positiveCount = 0;
			int zeroCount = 0;
			int negativeCount = 0;
			int lessThan60 = 0;
			int lessThan45 = 0;

			while (true) {

				for (int i = 0; i < path.m_positionCount; i++) {
					PathUnit.Position p = path.GetPosition(i);

					if (p.m_segment == segmentID) {

						if (dir == Vector3.zero) {
							return true;
						}

						Vector3 pathDirection = Singleton<NetManager>.instance.m_lanes.m_buffer[PathManager.GetLaneID(p)].CalculateDirection(p.m_offset);
						float dotProduct = Vector3.Dot(dir, pathDirection);

						if (dotProduct > 0) {
							positiveCount++;
							if (Math.Abs(Vector3.Angle(dir, pathDirection)) <= 60) {
								lessThan60++;
							}
							if (Math.Abs(Vector3.Angle(dir, pathDirection)) <= 45) {
								lessThan45++;
							}
						} else if (dotProduct == 0) {
							zeroCount++;
						} else {
							negativeCount++;
						}
					}
				}
					
				if (path.m_nextPathUnit == 0) {
					if (positiveCount > 0 || zeroCount > 0) {
						if (zeroCount > 0 || negativeCount > 0) {
							Debug.Log("positive = " + positiveCount + " zero = " + zeroCount + " negative = " + negativeCount);
						}
						if (lessThan60 > 0) {
							if (lessThan45 < lessThan60) {
								Debug.Log("lessThan60 = " + lessThan60 + " lessThan45 = " + lessThan45);
							}
							return (positiveCount > negativeCount);
						} else {
							Debug.Log("lessThan60 = " + lessThan60);
							return false;
						}
					} else {
						return false;
					}
				}

				path = this.getPath(path.m_nextPathUnit);
			}
		}
*/

		/*
				if (p.m_segment == segmentID) {
					pathDirection += Singleton<NetManager>.instance.m_lanes.m_buffer[PathManager.GetLaneID(p)].CalculateDirection(p.m_offset);
				}

				if (path.m_nextPathUnit == 0) {
					return (pathDirection.magnitude > 0 && Vector3.Dot(dir, pathDirection) >= 0);
				}

				*/

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

		PathPoint[] GatherPathVerticies(uint pathID)
        {
			List<PathPoint> path = new List<PathPoint>();

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
                    /*    segment = p.m_segment;
						startNode = segments[segment].m_startNode;
                        endNode = segments[segment].m_endNode;

                        Vector3 startPos = nodes[startNode].m_position;// +(Vector3.Cross(Vector3.up, segment.m_startDirection) * 5.0f); 
                        Vector3 endPos = nodes[endNode].m_position;// +(Vector3.Cross(Vector3.up, segment.m_endDirection) * -5.0f);

						Vector3 pv = Vector3.Lerp(startPos,endPos,(float)p.m_offset / 255.0f);
						*/

						Vector3 pv = PathManager.CalculatePosition(p);

						PathPoint newPoint;
						newPoint.x = pv.x;
						newPoint.y = pv.y;
						newPoint.z = pv.z;
						newPoint.segmentID = p.m_segment;

						path.Add(newPoint);

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
                    return path.ToArray();
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
