using ColossalFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


namespace TrafficReport
{
    public class QueryTool : DefaultTool
    {


        NetManager netMan = Singleton<NetManager>.instance;
        
        List<GameObject> visualizations;

        protected override void Awake()
        {
            visualizations  = new List<GameObject>();
            m_cursor = new CursorInfo();
            m_cursor.m_texture = ResourceLoader.loadTexture(128, 128, "Cursor.png");
            m_cursor.m_hotspot = new Vector2(42, 41);
            base.Awake();
        }

        protected override void OnToolGUI()
        {
            if (this.m_toolController.IsInsideUI)
            {
                return;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseDown)
            {
                if (current.button == 0)
                {
                    InstanceID hoverInstance = this.m_hoverInstance;

                    //Log.info(m_mousePosition.ToString());

                    try
                    {
                        Log.debug("You clicked on " + hoverInstance.ToString());
                        Log.debug(hoverInstance.Type.ToString());
                        
                        if (hoverInstance.Type == InstanceType.Vehicle)
                        {
                            this.ReportOnVehicle(hoverInstance.Vehicle);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.error(e.ToString());
                        Log.error(e.StackTrace);
                    }
                }
            }

            base.OnToolGUI();
        }

        void ReportOnVehicle(ushort id)
        {
            Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id];

            Log.info("Vechicle Name: " + vehicle.Info.name);

            Vector3[] path = this.GatherPathVerticies(vehicle.m_path);

            this.HideAllVisualizations();
            this.DumpPath(path);
            this.VisualizePath(path);
        }

        void DumpPath(Vector3[] path)
        {
            string filename = ResourceLoader.BaseDir + "path.txt";
            Log.debug("Dumping path to " + filename);

            StreamWriter fs = new StreamWriter(filename);
            for (int i = 0; i < path.Length; i++)
            {
                fs.WriteLine(path[i].x + " " + path[i].y + " " + path[i].z);
            }
            fs.Close();
        }

        Vector3[] GatherPathVerticies(uint pathID) 
        {
            List<Vector3> verts = new List<Vector3>();

            Log.debug("Gathering path...");

            PathUnit path = this.getPath(pathID);
            NetSegment segment = netMan.m_segments.m_buffer[path.GetPosition(0).m_segment];
            NetNode startNode, endNode;
            Vector3 lastPoint;
            startNode = netMan.m_nodes.m_buffer[segment.m_startNode];
            lastPoint = startNode.m_position;
            verts.Add(lastPoint);
            while (true)
            {
                for (int i = 0; i < path.m_positionCount; i++)
                {
                    PathUnit.Position p = path.GetPosition(i);

                    if (p.m_segment != 0)
                    {
                        segment = netMan.m_segments.m_buffer[p.m_segment];

                        startNode = netMan.m_nodes.m_buffer[segment.m_startNode];
                        endNode = netMan.m_nodes.m_buffer[segment.m_endNode];


                        
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
                        verts.Add(endNode.m_position);
                    }
                }

                if (path.m_nextPathUnit == 0)
                {
                    Log.debug("Done");
                    return verts.ToArray();
                }
                path = this.getPath(path.m_nextPathUnit);
            }
        }


        void VisualizePath(Vector3[] positions)
        {

            Vector3 offset = new Vector3(0, 10, 0);

            Material lineMaterial = new Material(Shader.Find("Diffuse"));
            lineMaterial.color = Color.red;
                

            for (int i = 0; i < positions.Length - 1 ; i++)
            {
                GameObject lineGameObject = new GameObject();
                LineRenderer theLine = lineGameObject.AddComponent<LineRenderer>();
                theLine.material = lineMaterial;
                theLine.enabled = true;
                theLine.SetWidth(5, 5);
                theLine.SetVertexCount(2);
                theLine.SetPosition(0, positions[i + 0] + offset);
                theLine.SetPosition(1, positions[i + 1] + offset);

                visualizations.Add(lineGameObject);
            }          

        }

        PathUnit getPath(uint id)
        {
            return Singleton<PathManager>.instance.m_pathUnits.m_buffer[id];
        }

        void ReportBuilding(uint id)
        {
            Building b = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
            Log.info(id + " = " + b.Info.GetLocalizedTitle());
        }


        void HideAllVisualizations()
        {

            foreach (GameObject v in visualizations)
            {
                GameObject.Destroy(v);
            }

            visualizations = new List<GameObject>();
        }
        
        protected override void OnDisable()
        {
            this.HideAllVisualizations();
            base.OnDisable();
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
 	         base.RenderOverlay(cameraInfo);
        }
    }
}
