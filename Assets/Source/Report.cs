using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

namespace TrafficReport
{
	
	[Serializable]
	public enum EntityType {
		Citzen,
		Vehicle
	}

	[Serializable]
	public struct PathPoint {
		public float x,y,z, nx, ny, nz;
		public uint segmentID;

        public Vector3 pos
        {
            get
            {
                return new Vector3(x, y, z);
            }
            set {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }

        public Vector3 normal
        {
            get
            {
                return new Vector3(nx, ny, nz);
            }
            set
            {
                nx = value.x;
                ny = value.y;
                nz = value.z;
            }
        }
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
			XmlSerializer xml = new XmlSerializer (GetType());
			xml.Serialize (new FileStream (name, FileMode.Create, FileAccess.Write),this);
			} catch(Exception e) {
				Log.error("Error saving report" + e.ToString());
			}
        }

        public static Report Load(string name)
        {
			XmlSerializer xml = new XmlSerializer (typeof(Report));
			return xml.Deserialize(new FileStream(name, FileMode.Open, FileAccess.Read)) as Report;
        }

    }
}
