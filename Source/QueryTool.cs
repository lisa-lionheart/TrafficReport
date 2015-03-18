
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace TrafficReport
{

	public class QueryToolGUI: QueryToolGUIBase {

		//Implent logic that interfaces with Colossal code

	}

    public class QueryTool : DefaultTool
    {

        TrafficAnalyzer analyzer;
        List<GameObject> visualizations;
        Material lineMaterial;

        CursorInfo loadingCursor;

        public QueryTool()
        {
            Log.info("Badger");
            analyzer = new TrafficAnalyzer(this);
        }

        protected override void Awake()
        {

            try
            {


                visualizations = new List<GameObject>();

                m_cursor = new CursorInfo();
                m_cursor.m_texture = ResourceLoader.loadTexture(128, 128, "Cursor.png");
                m_cursor.m_hotspot = new Vector2(42, 41);

                loadingCursor = new CursorInfo();
                loadingCursor.m_texture = ResourceLoader.loadTexture(64, 64, "Hourglass.png");

                lineMaterial = new Material(ResourceLoader.loadResourceString("TransparentVertexLit.shader"));
                lineMaterial.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
                lineMaterial.SetColor("_Emission", new Color(1, 0, 0));
                lineMaterial.mainTexture = ResourceLoader.loadTexture(100, 200, "NewSkin.png");


                GameObject go = new GameObject();
                QueryToolGUI gui = go.AddComponent<QueryToolGUI>();
                gui.queryTool = this;
                go.tag = "QueryToolGUI";

            }
            catch (Exception e)
            {
                Log.error(e.ToString());
            }
            base.Awake();
        }


        protected override void OnToolGUI()
        {

            //Animate the traffic lines
            lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));


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
                            HideAllVisualizations();
                            base.ToolCursor = loadingCursor;
                            analyzer.ReportOnVehicle(hoverInstance.Vehicle);
                        }

                        if (hoverInstance.Type == InstanceType.NetSegment)
                        {
                            HideAllVisualizations();
                            base.ToolCursor = loadingCursor;
                            analyzer.ReportOnSegment(hoverInstance.NetSegment);
                        }

                        if (hoverInstance.Type == InstanceType.Building)
                        {
                            HideAllVisualizations();
                            base.ToolCursor = loadingCursor;
                            analyzer.ReportOnBuilding(hoverInstance.Building);
                        }


                        if (hoverInstance.Type == InstanceType.CitizenInstance)
                        {
                            HideAllVisualizations();
                            base.ToolCursor = loadingCursor;
                            analyzer.ReportOnCitizen(hoverInstance.CitizenInstance);
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
            base.ToolCursor = m_cursor;

            OnGotSegmentReport(report);
        }

        internal void OnGotSegmentReport(TrafficAnalyzer.Report report)
        {

            base.ToolCursor = m_cursor;

            foreach(Vector3[] path in report.paths) {
                VisualizePath(path);
            }

            float alpha = 30.0f / report.paths.Count;

            if (alpha > 1)
            {
                alpha = 1;
            }

            lineMaterial.color = new Color(1, 0, 0, alpha);
        }

        internal void OnGotSinglePathReport(Vector3[] positions)
        {
            base.ToolCursor = m_cursor;
            VisualizePath(positions);
        }

        private void VisualizePath(Vector3[] positions) {

            lineMaterial.color = new Color(1, 0, 0, 1);

            PathMeshBuilder pb = new PathMeshBuilder();


            if (Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True)
            {
                pb.driveLane = -1;
            }

            pb.AddPoints(positions);

            Mesh m = pb.GetMesh();
            GameObject go = new GameObject(); ;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshFilter>().mesh = m;
            go.GetComponent<MeshFilter>().sharedMesh = m;
            go.GetComponent<Renderer>().material = lineMaterial;

            go.transform.localPosition = new Vector3(0, 3, 0);

            visualizations.Add(go);
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
