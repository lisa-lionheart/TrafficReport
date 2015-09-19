using System;
using UnityEngine;
using TrafficReport;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using TrafficReport.Util;
using TrafficReport.Assets.Source.UI;
using ColossalFramework.UI;
using ColossalFramework;
using System.Reflection;

namespace TrafficReport
{
	public class PathController
	{
        QueryTool queryTool;

		//In game visualizations
        GameObject[] pathsVisualizations;
        Billboard[] vehicleIcons;
        Billboard activeSegmentIndicator;

		Material lineMaterial;
		Material lineMaterialHighlight;
        Material vehicleIndicator, vehicleIndicatorHighlight;
        
		Report currentReport;
		uint currentHighlight;
		HighlightType currentHighlightType;
        
        public Billboard ActiveSegmentIndicator
        {
            get { return activeSegmentIndicator; }
        }

		public PathController(QueryTool tool)
		{
            queryTool = tool;
            currentHighlightType = HighlightType.None;

            Config.instance.eventConfigChanged += () => { OnConfigChanged(); };

            Log.info("Load Line Material...");

            Color red = new Color(1, 0, 0);
            Color gold = new Color(1, 0.9f, 0);

            string lineShader = ResourceLoader.loadResourceString("Materials/Shaders/TransparentVertexLit.shader");

            lineMaterial = new Material(lineShader);
            lineMaterial.color = red;
            lineMaterial.SetColor("_Emission", red);
            lineMaterial.SetColor("_SpecColor", Color.black); //Disable shine effect
            lineMaterial.mainTexture = ResourceLoader.loadTexture("Materials/NewSkin.png");
            lineMaterial.renderQueue = 100;

            lineMaterialHighlight = new Material(lineMaterial);
            lineMaterialHighlight.color = gold;
            lineMaterialHighlight.SetColor("_Emission", gold);
            lineMaterial.renderQueue = 101;


            Texture pin = ResourceLoader.loadTexture("Materials/Pin.png");

            activeSegmentIndicator = Billboard.Create(Billboard.CreateSpriteMaterial(pin, Color.green));
            vehicleIndicator = Billboard.CreateSpriteMaterial(pin, red);
            vehicleIndicatorHighlight = Billboard.CreateSpriteMaterial(pin, gold);

            Log.debug("PathController initialized");
		}
 
        void OnConfigChanged()
        {
            if (currentReport == null)
            {
                return;
            }

            for (int i = 0; i < currentReport.allEntities.Length; i++)
            {
                bool visible = Config.instance.IsTypeVisible(currentReport.allEntities[i].serviceType);
                vehicleIcons[i].gameObject.SetActive(visible);
                pathsVisualizations[i].SetActive(visible);
            }
        }
        
		public void Update() {

			//Animate the traffic lines
			lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));
			lineMaterialHighlight.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));

            if (currentReport != null)
            {
                for (int i = 0; i < currentReport.allEntities.Length; i++)
                {
                    vehicleIcons[i].position = GetPositionForReportEntity(i) + Vector3.up * 10.0f;
                }
            }
		}
	

		public void SetReport(Report report) {

			if (pathsVisualizations != null) {
                
				RemoveAllPaths();
				currentReport = null;          
			}

			if (report == null || report.allEntities == null) {
				Log.debug ("Report NULL");             
				return;
			}

            
			pathsVisualizations = new GameObject[report.allEntities.Length];
            vehicleIcons = new Billboard[report.allEntities.Length];
			for(int i=0; i < report.allEntities.Length; i++)
			{
                //if (i != 34)  continue;

                bool visible = Config.instance.IsTypeVisible(report.allEntities[i].serviceType);

				pathsVisualizations[i] =  CreatePathGameobject(report.allEntities[i].serviceType, report.allEntities[i].path);
                pathsVisualizations[i].name = "Path " + i;
                pathsVisualizations[i].SetActive(visible);

                vehicleIcons[i] = Billboard.Create(vehicleIndicator);
                vehicleIcons[i].gameObject.SetActive(visible);

			}
			
			float alpha = 30.0f / report.allEntities.Length;
			
			if (alpha > 1)
			{
				alpha = 1;
			}
			

			currentReport = report;            
			SetHighlight(HighlightType.None, 0);

		}

        private  Vector3 GetPositionForReportEntity(int i)
        {

            if (this.currentReport.allEntities[i].type == EntityType.Vehicle)
            {
                //return Singleton<VehicleManager>.instance.GetS
                uint id = currentReport.allEntities[i].id;
                //return Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id].GetLastFramePosition();                
                return Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id].GetSmoothPosition((ushort)id);
            }


            if (this.currentReport.allEntities[i].type == EntityType.Citizen)
            {
                uint id = currentReport.allEntities[i].id;
                return Singleton<CitizenManager>.instance.m_instances.m_buffer[id].GetSmoothPosition((ushort)id);
            }
            return new Vector3();
        }

		
        public void SetHighlight(String serviceType)
        {

            if (currentReport == null)
            {
                return;
            }

            for (int i = 0; i < currentReport.allEntities.Length; i++)
            {
                bool highlighted = currentReport.allEntities[i].serviceType.Equals(serviceType);
                pathsVisualizations[i].GetComponent<Renderer>().material = highlighted ? lineMaterialHighlight : lineMaterial;
                vehicleIcons[i].GetComponent<Renderer>().material = highlighted ? vehicleIndicatorHighlight : vehicleIndicator;                
            }
        }

		public void SetHighlight(HighlightType type, uint id){

			if (currentReport == null) {
				return;
			}

			if (currentHighlight == id && currentHighlightType == type) {
				return;
			}

			for(int index=0; index < currentReport.allEntities.Length; index++) {
				bool match = currentReport.allEntities[index].MatchesHighlight(type,id);
                pathsVisualizations[index].GetComponent<Renderer>().material = match ? lineMaterialHighlight : lineMaterial;
                vehicleIcons[index].GetComponent<Renderer>().material = match ? vehicleIndicatorHighlight : vehicleIndicator;				
			}
            
			currentHighlight = id;
			currentHighlightType = type;
		}
		
		private GameObject CreatePathGameobject(string type, PathPoint[] positions) {
			
			lineMaterial.color = new Color(1, 0, 0, 1);
			
			PathMeshBuilder pb = new PathMeshBuilder();

            if (type == "Citizen/Foot" || type == "Citizen/Cycle")
            {
                //Citizens have much tighter paths, to remove duplicate points so much
                pb.duplicatePointThreshold = 1.0f;
                //pb.normalScaleFactor = 0.01f;
                pb.tightNormalScaleFactor = 0.05f;
                pb.pathBreakThreshold = 150.0f;  //If a path segemnt is longer than this they are riding a bus/metro/train
            }

			pb.AddPoints(positions);
			
			Mesh m = pb.GetMesh();
			GameObject go = new GameObject(); ;
            go.AddComponent<MeshFilter>();
			go.AddComponent<MeshRenderer>();
			go.GetComponent<MeshFilter>().mesh = m;
			go.GetComponent<MeshFilter>().sharedMesh = m;
			go.GetComponent<MeshRenderer>().material = lineMaterial;
			go.transform.localPosition = new Vector3(0, 3, 0);

			go.SetActive(Config.instance.IsTypeVisible(type));
			
			return go;
		}
		
		
		void RemoveAllPaths()
		{
			if (pathsVisualizations == null) {
				return;
			}
			
			foreach (GameObject v in pathsVisualizations)
			{
				GameObject.Destroy(v);
			}

            foreach (Billboard v in vehicleIcons)
            {
                GameObject.Destroy(v.gameObject);
            }

            vehicleIcons = null;
			pathsVisualizations = null;
		}

    }
}

