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
		public float x,y,z;
		public uint segmentID;
	}
	
	[Serializable]
	public struct EntityInfo
	{
		public EntityType type;
		public PathPoint[] path;
		//Mesh mesh; 
		
	}

    [Serializable]
    class Report
    {
        public EntityInfo[] allEntities;

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
			XmlSerializer xml = new XmlSerializer (typeof(Report));
			xml.Serialize (new FileStream (name, FileMode.OpenOrCreate, FileAccess.Write),this);
        }

        public static Report Load(string name)
        {
			XmlSerializer xml = new XmlSerializer (typeof(Report));
			return xml.Deserialize(new FileStream(name, FileMode.Open, FileAccess.Read)) as Report;
        }

    }
}
