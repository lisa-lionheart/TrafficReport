using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using TrafficReport.Util;



public enum HighlightType
{
    None,
    All,
    Segment,
    Vehicle,
    Building,
    Citizen
}


namespace TrafficReport
{
	
	[Serializable]
	public enum EntityType {
		Citizen,
		Vehicle
	}

	[Serializable]
	public struct PathPoint {
        public Vector3 pos, forwards, backwards;
        public uint segmentId, laneId;
        public bool guessed;
	}
	
	[Serializable]
	public struct EntityInfo
	{
		public EntityType type;
		public PathPoint[] path;
		public uint id;

		public uint sourceBuilding;
		public uint targetBuilding;

		public string serviceType;


        public bool MatchesHighlight(HighlightType highlightType, uint thingId)
        {
            switch (highlightType)
            {
                case HighlightType.None:
                    return false;
                case HighlightType.All:
                    return true;
                case HighlightType.Segment:

                    foreach (PathPoint p in path)
                    {
                        if (p.segmentId == thingId)
                            return true;
                    }

                    break;
                case HighlightType.Building:
                    if (sourceBuilding == thingId || targetBuilding == id)
                        return true;
                    break;
                case HighlightType.Vehicle:
                    if (id == thingId && type == EntityType.Vehicle)
                        return true;
                    break;
                case HighlightType.Citizen:

                    if (id == thingId && type == EntityType.Citizen)
                        return true;
                    break;
            }
            return false;
        }

	}

    [Serializable]
    public class Report
    {
        public EntityInfo[] allEntities;

		public Report() {
		}

		public Report(EntityInfo _info)
		{
			allEntities = new EntityInfo[]{_info};
		}

        public Report(EntityInfo[] _info)
        {
            allEntities = _info;
        }
		       

        public void Save(string name)
        {
			Log.info ("Saving report");
			try 
			    {
			    XmlSerializer xml = new XmlSerializer (typeof(Report));
                FileStream fs = new FileStream(name, FileMode.Create, FileAccess.Write);
			    xml.Serialize (fs,this);
                fs.Close();
			} catch(Exception e) {
				Log.error("Error saving report" + e.ToString());
            }
            finally {
            }
            
        }

        public static Report Load(string name)
        {
			XmlSerializer xml = new XmlSerializer (typeof(Report));
			return xml.Deserialize(new FileStream(name, FileMode.Open, FileAccess.Read)) as Report;
        }

        public Dictionary<string, int> CountEntiesTypes(HighlightType type = HighlightType.All, uint id = 0)
        {
            Dictionary<string,int> typeCounts = new Dictionary<string,int>();
            for (int i = 0; i < allEntities.Length; i++)
            {
                if (!allEntities[i].MatchesHighlight(type, id))
                    continue;

                int val = 0;
                typeCounts.TryGetValue(allEntities[i].serviceType, out val);
                typeCounts[allEntities[i].serviceType] = val+1;
            }

            return typeCounts;
        }

    }
}
