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

        TrafficAnalyzer analyzer;
        List<GameObject> visualizations;
        Material lineMaterial;

        public QueryTool()
        {
            analyzer = new TrafficAnalyzer(this);
        }

        protected override void Awake()
        {
            visualizations  = new List<GameObject>();
            m_cursor = new CursorInfo();
            m_cursor.m_texture = ResourceLoader.loadTexture(128, 128, "Cursor.png");
            m_cursor.m_hotspot = new Vector2(42, 41);


            lineMaterial = new Material(Shader.Find("VertexLit"));
            lineMaterial.color = Color.red;
            lineMaterial.mainTexture = ResourceLoader.loadTexture(80, 90, "Arrow.png");
            lineMaterial.SetTextureScale("_MainTex", new Vector2(10, 1));
            base.Awake();
        }

        protected override void OnToolGUI()
        {
            if (this.m_toolController.IsInsideUI)
            {
                return;
            }

            //Animate the traffic lines
	        lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f,0));


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
                            HideAllVisualizations();
                            analyzer.ReportOnVehicle(hoverInstance.Vehicle);
                        }

                        if (hoverInstance.Type == InstanceType.NetSegment)
                        {
                            HideAllVisualizations();
                            analyzer.ReportOnSegment(hoverInstance.NetSegment);
                        }

                        if (hoverInstance.Type == InstanceType.Building)
                        {
                            HideAllVisualizations();
                            analyzer.ReportOnBuilding(hoverInstance.Building);
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

        
        public override NetSegment.Flags GetSegmentIgnoreFlags()
        {
            return NetSegment.Flags.None;
        }

        internal void OnGotBuildingReport(TrafficAnalyzer.Report report)
        {
            OnGotSegmentReport(report);
        }

        internal void OnGotSegmentReport(TrafficAnalyzer.Report report)
        {
            foreach(Vector3[] path in report.paths) {
                OnGotVehiclePath(path);
            }
        }

        internal void OnGotVehiclePath(Vector3[] positions)
        {

            Vector3 offset = new Vector3(0, 10, 0);

                

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
