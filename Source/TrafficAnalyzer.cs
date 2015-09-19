using ColossalFramework;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using TrafficReport.Util;
using UnityEngine;

namespace TrafficReport
{
    delegate void ReportGeneratedHandler(Report theReport);

    class TrafficAnalyzer 
    {
        VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
        NetManager netMan = Singleton<NetManager>.instance;


        CitizenInstance[] citizensInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;
        Citizen[] citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
        CitizenUnit[] citizenUnits = Singleton<CitizenManager>.instance.m_units.m_buffer;
        Vehicle[] vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;



        bool working;

        public event ReportGeneratedHandler OnReport;

        public TrafficAnalyzer()
        {
            working = false;
        }

        public void ReportOnVehicle(ushort id)
        {
            DispatchWorkAsync(() => DoReportOnVehicle(id));                        
        }

        internal void ReportOnCitizen(ushort id)
        {
            DispatchWorkAsync(() => DoReportOnCitizen(id));
        }

		public void ReportOnSegment(ushort segmentID, NetInfo.Direction dir)
        {
            DispatchWorkAsync(() => DoReportOnSegment(segmentID, dir));
        }

        public void ReportOnBuilding(ushort buildingID)
        {
            DispatchWorkAsync(() => DoReportOnBuilding(buildingID));
        }
        
        
        private void DispatchWorkAsync(Func<Report> work) {
            if (working)
            {
                Log.warn("Job in progress bailing!");
                return;
            }
            working = true;
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                Report report = null;
                try
                {
                    report = work();
                }
                catch (Exception e)
                {
                    Log.error(e.Message);
                    Log.error(e.StackTrace);
                }
                finally
                {    
                    ThreadHelper.dispatcher.Dispatch(() => OnReport(report));
                    working = false;
                }
            });
        }

        private Report DoReportOnVehicle(ushort id) {

            if (vehicles[id].m_leadingVehicle != 0)
                id = vehicles[id].m_leadingVehicle;


            if (vehicles[id].Info.m_vehicleType == VehicleInfo.VehicleType.Bicycle) {

                //Revisit when CO introduce tandum bicycles
                uint citizen = citizenUnits[vehicles[id].m_citizenUnits].GetCitizen(0);
                ushort citizenInstance = citizens[citizen].m_instance;
                Report r  = DoReportOnCitizen(citizenInstance);
                r.allEntities[0].id = id;
                return r;
            }

            if (!IsValid(ref vehicles[id]))
                return null;
                    
			EntityInfo info;
            info.type = EntityType.Vehicle;
			info.id = id;
            info.path = this.GatherPathVerticies(vehicles[id].m_path);
            info.serviceType = GetServiceType(ref vehicles[id]);					
			info.sourceBuilding = vehicles[id].m_sourceBuilding;
			info.targetBuilding = vehicles[id].m_targetBuilding;

            return new Report(info);
        }

        private Report DoReportOnCitizen(ushort id)
        {

            EntityInfo info;
            info.type = EntityType.Citizen;
            info.id = id;
            info.path = this.GatherPathVerticies(citizensInstances[id].m_path, true);
            info.serviceType = GetServiceType(ref citizensInstances[id]);
            info.sourceBuilding = citizensInstances[id].m_sourceBuilding;
            info.targetBuilding = citizensInstances[id].m_targetBuilding;

            return new Report(info);
        }

		private Report DoReportOnSegment(ushort segmentID, NetInfo.Direction dir)
        {

            List<EntityInfo> enities = new List<EntityInfo>();

            Log.debug("Looking at vehicles...");
            Vehicle[] vehicles = vehicleManager.m_vehicles.m_buffer;
            for (uint i = 0; i < vehicles.Length; i++)
            {
                
                if (!IsValid(ref vehicles[i])) continue;

                if (vehicles[i].m_leadingVehicle != 0) //This will be picked when it visits that vehicle
                    continue;

                //Log.info("Vehcile valid, checking if path intersects segment...");

				if (this.PathContainsSegment(vehicles[i].m_path, segmentID, dir))
                {
                    //Log.info("Found vehicle on segemnt, getting path....");

                    EntityInfo info;
                    info.type = EntityType.Vehicle;
					info.id = i;
                    info.path = this.GatherPathVerticies(vehicles[i].m_path);
					
					info.sourceBuilding = vehicles[i].m_sourceBuilding;
					info.targetBuilding = vehicles[i].m_targetBuilding;

					info.serviceType = GetServiceType(ref vehicles[i]);
                    enities.Add(info);
                }
            }

            Log.debug("found " + enities.Count + " entities");

            Log.debug("Looking at citizens...");
            for (uint i = 0; i < citizensInstances.Length; i++)
            {
                if(!IsValid(ref citizensInstances[i])) continue;                

				if (this.PathContainsSegment(citizensInstances[i].m_path, segmentID, dir))					
                {
                    //Log.info("Found citizen on segemnt, getting path....");

                    EntityInfo info;
                    info.type = EntityType.Citizen;
					info.id = i;
                    info.path = this.GatherPathVerticies(citizensInstances[i].m_path,true);
                    info.serviceType = GetServiceType(ref citizensInstances[i]);
					info.sourceBuilding = citizensInstances[i].m_sourceBuilding;
					info.targetBuilding = citizensInstances[i].m_targetBuilding;

                    enities.Add(info);
                }

            }

            Log.debug("found " + enities.Count + " entities");

            Log.debug("End DoReportOnSegment");

            return new Report(enities.ToArray());
            
        }

        private Report DoReportOnBuilding(ushort buildingID)
        {
            List<EntityInfo> enities = new List<EntityInfo>();

            for (uint i = 0; i < vehicles.Length; i++)
            {

                if (!IsValid(ref vehicles[i])) continue;

                //Log.info("Vehcile valid, checking if path intersects segment...");
                if (vehicles[i].m_sourceBuilding == buildingID || vehicles[i].m_targetBuilding == buildingID)
                {

                    EntityInfo info;
                    info.type = EntityType.Vehicle;
					info.id = i;
                    info.path = this.GatherPathVerticies(vehicles[i].m_path);
					info.sourceBuilding = vehicles[i].m_sourceBuilding;
					info.targetBuilding = vehicles[i].m_targetBuilding;

                    info.serviceType = GetServiceType(ref vehicles[i]);
                    enities.Add(info);
                }
            }

            for (uint i = 0; i < citizensInstances.Length; i++)
            {
                if (!IsValid(ref citizensInstances[i])) continue;
                               
                if(citizensInstances[i].m_sourceBuilding == buildingID || citizensInstances[i].m_targetBuilding == buildingID) {
                    EntityInfo info;
                    info.type = EntityType.Citizen;
                    info.id = i;
                    info.path = this.GatherPathVerticies(citizensInstances[i].m_path,true);
                    info.serviceType = GetServiceType(ref citizensInstances[i]);

                    info.sourceBuilding = citizensInstances[i].m_sourceBuilding;
                    info.targetBuilding = citizensInstances[i].m_targetBuilding;

                    enities.Add(info);
                }
            }

            //Find all citizens commuting to and from that building
            for (UInt32 i = 0; i < citizens.Length; i++)
            {
                //They are either a resident a, worker or a visitor to this building
                if(citizens[i].m_homeBuilding == buildingID || 
                    citizens[i].m_visitBuilding == buildingID || 
                    citizens[i].m_workBuilding == buildingID)
                {

                    ushort vId = citizens[i].m_vehicle;
                    if (vId != 0)
                    {
                        if (IsValid(ref vehicles[vId]))
                        {
                            EntityInfo info;
                            info.type = EntityType.Vehicle;
                            info.id = vId;
                            info.path = this.GatherPathVerticies(vehicles[vId].m_path);
                            info.sourceBuilding = vehicles[vId].m_sourceBuilding;
                            info.targetBuilding = vehicles[vId].m_targetBuilding;

                            info.serviceType = GetServiceType(ref vehicles[vId]);
                            enities.Add(info);
                        }
                    }

                    ushort instanceId = citizens[i].m_instance;
                    if (instanceId != 0)
                    {
                        if (IsValid(ref citizensInstances[instanceId]))
                        {
                            EntityInfo info;
                            info.type = EntityType.Citizen;
                            info.id = instanceId;
                            info.path = this.GatherPathVerticies(citizensInstances[instanceId].m_path, true);
                            info.serviceType = GetServiceType(ref citizensInstances[instanceId]);

                            info.sourceBuilding = citizensInstances[instanceId].m_sourceBuilding;
                            info.targetBuilding = citizensInstances[instanceId].m_targetBuilding;

                            enities.Add(info);
                        }
                    }

                }

            }


            return new Report(enities.ToArray());
        }
	

		private bool PathContainsSegment(uint pathID, ushort segmentID, NetInfo.Direction dir)
		{
			PathUnit path = this.getPath(pathID);
            
			while (true) {
                // HACK: Dont inlude last segment as it will show vechicles arriving for both directions
				for (int i = 0; i < path.m_positionCount-1; i++) {

                    PathUnit.Position p = path.GetPosition(i);
                    NetSegment segment = netMan.m_segments.m_buffer[(int)p.m_segment];

                    // Exclude paths with deleted segments
                    if (PathManager.GetLaneID(p) == 0 || (segment.m_flags & NetSegment.Flags.Deleted) != 0)
                    {
                        return false;
                    }

					if (p.m_segment == segmentID) {


						if (dir == NetInfo.Direction.Both) {
							return true;
						}

						NetInfo info = netMan.m_segments.m_buffer[(int)p.m_segment].Info;
						NetInfo.Direction laneDir = NetInfo.Direction.None;

						if (info.m_lanes.Length > (int)p.m_lane) {
							laneDir = info.m_lanes[(int)p.m_lane].m_finalDirection;
							if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) {
								laneDir = NetInfo.InvertDirection(laneDir);
							}
						} else {
                            Log.error("bad lane count");
						}

						if (laneDir == dir) {
							return true;
						}
//						} else if ((laneDir != NetInfo.Direction.Forward) && (laneDir != NetInfo.Direction.Backward)) {
		//					Log.debug("laneDir = " + laneDir);
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

        PathUnit.Position[] GatherPathPositions(uint pathID, bool isPed)
        {
            //Log.debug("Gathering path positions ...");

            PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
			List<PathUnit.Position> positions = new List<PathUnit.Position>();
            while (true)
            {
				for (int i = 0; i < paths[pathID].m_positionCount; i++)
                {
					PathUnit.Position p = paths[pathID].GetPosition(i);

                    //Exclude invalid lanes
                    if (PathManager.GetLaneID(p) != 0) {
                        positions.Add(p);
                    }
                    
                }

                if (paths[pathID].m_nextPathUnit == 0)
                {
                    //Log.debug("Done");
                    return positions.ToArray();
                }

                pathID = paths[pathID].m_nextPathUnit;

                //Bail once a pedestrian embarks on a bus
                if (isPed && (paths[pathID].m_vehicleTypes & (byte)VehicleInfo.VehicleType.Metro) > 0)
                {
                    return positions.ToArray();
                }
            }
        }

		PathPoint[] GatherPathVerticies(uint pathID, bool isPed = false)
        {
			List<PathPoint> path = new List<PathPoint>();

			NetSegment[] segments = netMan.m_segments.m_buffer;
			NetNode[] nodes = netMan.m_nodes.m_buffer;
            NetLane[] lanes = Singleton<NetManager>.instance.m_lanes.m_buffer;
            
            PathUnit[] paths = Singleton<PathManager>.instance.m_pathUnits.m_buffer;
			uint segment = paths[pathID].GetPosition(0).m_segment;
			uint startNode;
			startNode = segments[segment].m_startNode;


            PathUnit.Position[] positions = GatherPathPositions(pathID, isPed);

            //Log.debug("Generating verticies...");

            PathPoint lastPoint = new PathPoint();
            
            for (int i = 0; i < positions.Length; i++) {

                Vector3 pv, dir;
                PathPoint newPoint = new PathPoint();
                
                uint laneId = PathManager.GetLaneID(positions[i]);

                //Put the destination market on the road not the sidewalk
                if (i == positions.Length - 1) {
                    laneId = lastPoint.laneId;
                }
                    
                lanes[laneId].CalculatePositionAndDirection(positions[i].m_offset / 255.0f,out pv, out dir);

                newPoint.laneId = laneId;
                newPoint.guessed = false;
                newPoint.pos = pv;
                newPoint.forwards = (positions[i].m_offset < 128) ? -dir : dir;
                newPoint.backwards = -newPoint.forwards;

                //Point is contining a curve
                if ((lastPoint.pos - newPoint.pos).magnitude < 5.0f)
                {
                    newPoint.backwards = -lastPoint.forwards;
                }

                newPoint.segmentId = positions[i].m_segment;

				path.Add(newPoint);
                lastPoint = newPoint;

                if (i < positions.Length - 2) {
                    if (positions[i].m_segment != positions[i + 1].m_segment) {

                        newPoint = new PathPoint();

                        PathUnit.Position nextPos = positions[i + 1];
                        laneId = PathManager.GetLaneID(nextPos);
                                                
                        Vector3 pvA, dirA, pvB,dirB;
                        lanes[laneId].CalculatePositionAndDirection(0, out pvA, out dirA);
                        lanes[laneId].CalculatePositionAndDirection(1, out pvB, out dirB);

                        //Skip when not a junction;
                        if (pvA == pv || pvB == pv)
                            continue;

                        newPoint.guessed = true;
                        newPoint.laneId = laneId;
                      
                        //Find the closest lane end of the next segment
                        if ((pvA - pv).magnitude < (pvB - pv).magnitude)
                        {   
                            newPoint.pos = pvA;
                            newPoint.forwards = dirA;
                            newPoint.backwards = -dirA;
                        }
                        else
                        {
                            newPoint.pos = pvB;
                            newPoint.forwards = -dirB;
                            newPoint.backwards = dirB;
                        }                      
                        
                        newPoint.segmentId = positions[i + 1].m_segment;

                        path.Add(newPoint);
                        lastPoint = newPoint;
                        
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

        internal Vector3 GetSegmentMidPoint(ushort segmentId)
        {
            NetSegment segment = netMan.m_segments.m_buffer[segmentId];
            NetLane[] lanes = netMan.m_lanes.m_buffer;
            NetNode[] nodes = netMan.m_nodes.m_buffer;


            NetNode start = nodes[segment.m_startNode];
            NetNode end = nodes[segment.m_endNode];

            //return Vector3.Lerp(start.m_position, end.m_position, 0.5f);
            return Beizer.CalculateBezierPoint(0.5f, start.m_position, start.m_position + segment.m_startDirection/3.0f, end.m_position + segment.m_endDirection/3.0f, end.m_position);       
        }


        private string GetServiceType(ref Vehicle vehicle)
        {
            switch (vehicle.Info.GetService())
            {
                case ItemClass.Service.Residential:

                    if (vehicle.Info.name == "Scooter")
                        return "Citizen/Scooter";

                    return "Citizen/Car";

                default:
                    return vehicle.Info.GetService().ToString() + "/" + vehicle.Info.GetSubService().ToString();
            }

            
        }

        private string GetServiceType(ref CitizenInstance inst)
        {
            if ((inst.m_flags & CitizenInstance.Flags.RidingBicycle) == CitizenInstance.Flags.RidingBicycle)
            {
                return "Citizen/Cycle";
            }
            else
            {
                return "Citizen/Foot";
            }
        }

        private bool IsValid(ref Vehicle vehicle)
        {
            if ((vehicle.m_flags & Vehicle.Flags.Deleted) != Vehicle.Flags.None)
            {
                return false;
            }
            
            if (vehicle.m_path == 0)
            {
                return false;
            }

            return true;
        }

        private bool IsValid(ref CitizenInstance citizenInstance)
        {
            if ((citizenInstance.m_flags & CitizenInstance.Flags.Deleted) != CitizenInstance.Flags.None)
            {
                return false;
            }

            if (citizenInstance.m_path == 0)
            {
                return false;
            }

            return true;
        }

    }
}
