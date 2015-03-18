using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrafficReport
{
 //   [Serializable]
    class Report
    {
        public enum EntityType {
            Citzen,
            Vehicle
        }

       // [Serializable]
        public struct EntityInfo
        {
            public EntityType type;
            public Vector3[] path;
            //Mesh mesh; 

        }

        //public HashSet<uint> touchedSegments;

        public EntityInfo[] allEntities;


        public Report(EntityInfo[] _info)
        {
            allEntities = _info;
        }


        public Report(EntityInfo _info)
        {
            allEntities = new EntityInfo[]{_info};
        }

        static void save(string name)
        {

        }

        static Report load(string name)
        {

            return null;
        }

    }
}
