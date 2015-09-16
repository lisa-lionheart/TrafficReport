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
	public class ReportVisualizer : MonoBehaviour
	{
		public enum HighlightType {
			None,
			Segment,
			Vehicle,
			Building,
			Citizen
		}

        //Implent logic that interfaces with Colossal code
        public QueryTool queryTool;
        ReportButton toggleButton;
        ReportUI reportUi;

		//In game visualizations
        GameObject[] pathsVisualizations;
        Billboard[] vehicleIcons;
        Billboard activeSegmentIndicator;

		Material lineMaterial;
		Material lineMaterialHighlight;
        Material vehicleIndicator, vehicleIndicatorHighlight;
        
		protected Report currentReport;
		uint currentHighlight;
		HighlightType currentHighlightType;

		public bool leftHandDrive;


        public Billboard ActiveSegmentIndicator
        {
            get { return activeSegmentIndicator; }
        }

		public ReportVisualizer()
		{   
			currentHighlightType = HighlightType.None;
		}

		public bool toolActive {
            get
            {
                return ToolsModifierControl.toolController.CurrentTool == queryTool;
            }
            set
            {
                InfoManager infoManger = GameObject.FindObjectOfType<InfoManager>();


                FieldInfo mode = typeof(InfoManager).GetField("m_currentMode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                //MethodInfo SetInfoMode = typeof(InfoManager).GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);


                if (value)
                {
                    UIView.library.Hide("CityInfoPanel");

                    Debug.Log("Type:" + mode.GetType().ToString());
                    Debug.Log("Mode is:" + mode.GetValue(infoManger).ToString());
                    Debug.Log("Changing info mode");
                    mode.SetValue(infoManger, InfoManager.InfoMode.Traffic);
                    Debug.Log("Mode set to:" + infoManger.NextMode);
                    infoManger.UpdateInfoMode();

                    //  SetInfoMode.Invoke(infoManger, new object[] { InfoManager.InfoMode.Traffic, InfoManager.SubInfoMode.Default });

                    ToolsModifierControl.toolController.CurrentTool = queryTool;
                }
                else
                {
                    SetReport(null);
                    infoManger.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
            }
		}


        public virtual bool guiVisible
        {
			get { return true; }
		}

        public void Awake()
        {
            toggleButton = ReportButton.Create();
            toggleButton.eventClick += toggleButton_eventClick;

            Config.instance.eventConfigChanged += instance_eventConfigChanged;

            GameObject go = new GameObject();
            go.transform.localPosition = Vector3.zero;

            reportUi = go.AddComponent<ReportUI>();
            UIView.GetAView().AttachUIComponent(go);

            reportUi.eventHighlightType += (String s) => { SetTypeHighlight(s); };
                        
			Log.info("Load Line Material...");

			Color red = new Color (1, 0, 0);
			Color gold = new Color (1, 0.9f, 0);

			string lineShader = ResourceLoader.loadResourceString ("Materials/Shaders/TransparentVertexLit.shader");

			lineMaterial = new Material (lineShader);
			lineMaterial.color = red;
			lineMaterial.SetColor("_Emission", red);
            lineMaterial.SetColor("_SpecColor", Color.black); //Disable shine effect
			lineMaterial.mainTexture = ResourceLoader.loadTexture("Materials/NewSkin.png");
			lineMaterial.renderQueue = 100;
			
			lineMaterialHighlight = new Material (lineMaterial);
			lineMaterialHighlight.color = gold;
			lineMaterialHighlight.SetColor("_Emission", gold);
			lineMaterial.renderQueue = 101;


            Texture pin = ResourceLoader.loadTexture("Materials/Pin.png");

            activeSegmentIndicator = Billboard.Create(Billboard.CreateSpriteMaterial(pin, Color.green));
            vehicleIndicator = Billboard.CreateSpriteMaterial(pin, red);
            vehicleIndicatorHighlight = Billboard.CreateSpriteMaterial(pin, gold);

			Log.debug ("Gui initialized");
        }

        void instance_eventConfigChanged()
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

        void toggleButton_eventClick(ColossalFramework.UI.UIComponent component, ColossalFramework.UI.UIMouseEventParameter eventParam)
        {
            toolActive = !toolActive;
            
        }



		void Update() {

            toggleButton.ToggleState = !toolActive;
            reportUi.isVisible = toolActive;
            
			//Animate the traffic lines
			lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));
			lineMaterialHighlight.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));

			
			if (Input.GetKeyUp(Config.instance.keyCode)){
				Log.info ("Toggling tool");
				toolActive = !toolActive;
			}

			if (toolActive && Input.GetKeyUp(KeyCode.Escape)) {
				toolActive = false;
			}

            if (currentReport != null)
            {
                for (int i = 0; i < currentReport.allEntities.Length; i++)
                {
                    vehicleIcons[i].position = GetPositionForReportEntity(i) + Vector3.up * 10.0f;
                }
            }
		}
	
		void OnRenderObject() {

		}

		public void SetReport(Report report) {

			if (pathsVisualizations != null) {
				RemoveAllPaths();
				currentReport = null;          
			}

			if (report == null || report.allEntities == null) {
				Log.debug ("Report NULL");
                reportUi.SetSelectedData(null);
                reportUi.SetHighlightData(null,0);
				return;
			}

            int[] typeCounts = new int[Config.instance.vehicleTypes.Length];

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

                for (int j = 0; j < typeCounts.Length; j++)
                {
                    if (Config.instance.vehicleTypes[j].id == report.allEntities[i].serviceType)
                    {
                        typeCounts[j]++;
                    }
                }
			}
			
			float alpha = 30.0f / report.allEntities.Length;
			
			if (alpha > 1)
			{
				alpha = 1;
			}
			

			currentReport = report;

            reportUi.SetSelectedData(typeCounts);
			SetSegmentHighlight(HighlightType.None, 0);

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

		
        public void SetTypeHighlight(String serviceType)
        {

            if (currentReport == null)
            {
                return;
            }

            for (int i = 0; i < currentReport.allEntities.Length; i++)
            {
                bool highlighted = currentReport.allEntities[i].serviceType.Equals(serviceType);

                if (highlighted)
                {
                    pathsVisualizations[i].GetComponent<Renderer>().material = lineMaterialHighlight;
                    vehicleIcons[i].GetComponent<Renderer>().material = vehicleIndicatorHighlight;
                }
                else
                {
                    pathsVisualizations[i].GetComponent<Renderer>().material = lineMaterial;
                    vehicleIcons[i].GetComponent<Renderer>().material = vehicleIndicator;
                }

            }
        }

		public void SetSegmentHighlight(HighlightType type, uint id){

			if (currentReport == null) {
				return;
			}

			if (currentHighlight == id && currentHighlightType == type) {
				return;
			}

			foreach (GameObject go in pathsVisualizations) {
				go.GetComponent<Renderer> ().material = lineMaterial;
			}


            foreach (Billboard bb in vehicleIcons)
            {
                bb.material = vehicleIndicator;
            }

			int total = 0;

            int[] typeCount = new int[Config.instance.vehicleTypes.Length];
			
			for(int index=0; index < currentReport.allEntities.Length; index++) {

				bool highlighted = false;
				switch(type){
				case HighlightType.Segment:
				
					foreach(PathPoint p in currentReport.allEntities[index].path) {
						if(p.segmentId == id) {
							highlighted = true;
							break;
						}
					}

					break;
				case HighlightType.Building:
					if(currentReport.allEntities[index].sourceBuilding == id || currentReport.allEntities[index].targetBuilding == id) {

						highlighted = true;
					}
					break;
				case HighlightType.Vehicle:
					if(currentReport.allEntities[index].id == id && currentReport.allEntities[index].type == EntityType.Vehicle) {
						highlighted = true;
					}
					break;
				case HighlightType.Citizen:
					
					if(currentReport.allEntities[index].id == id && currentReport.allEntities[index].type == EntityType.Citizen) {
						highlighted = true;
					}
					break;
				}

				if(highlighted){
					pathsVisualizations [index].GetComponent<Renderer> ().material = lineMaterialHighlight;
                    vehicleIcons[index].GetComponent<Renderer>().material = vehicleIndicatorHighlight;
					string t = currentReport.allEntities[index].serviceType;
					total++;

                    for (int j = 0; j < typeCount.Length; j++)
                    {
                        if (Config.instance.vehicleTypes[j].id == currentReport.allEntities[index].serviceType)
                        {
                            typeCount[j]++;
                        }
                    }
				}
			}

            reportUi.SetHighlightData(typeCount, total);

			currentHighlight = id;
			currentHighlightType = type;
		}
		
		private GameObject CreatePathGameobject(string type, PathPoint[] positions) {
			
			lineMaterial.color = new Color(1, 0, 0, 1);
			
			PathMeshBuilder pb = new PathMeshBuilder();

            if (type == "citizen")
            {
                //Citizens have much tighter paths, to remove duplicate points so much
                pb.duplicatePointThreshold = 1.0f;
                //pb.normalScaleFactor = 0.01f;
                pb.tightNormalScaleFactor = 0.05f;
                pb.pathBreakThreshold = 150.0f;
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

			foreach(VehicleDisplay t in Config.instance.vehicleTypes) {

				if(t.id == type) {
					go.SetActive(t.onOff);
				}
			}
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

