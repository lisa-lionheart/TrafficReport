using System;
using UnityEngine;
using TrafficReport;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace TrafficReport
{
	struct VehicleDisplay {
		public string id;
		public string display;
	}

	[Serializable]
	public class Config {
		public Rect buttonPos  = new Rect(80, 5, 80, 80);


		public static Config Load() {
			try {
				XmlSerializer xml = new XmlSerializer (typeof(Config));
				return xml.Deserialize(new FileStream("TrafficReport.xml", FileMode.Open, FileAccess.Read)) as Config;
			}catch(Exception e) {
				return new Config();
			}
		}

		public void Save() {
			try 
			{
				XmlSerializer xml = new XmlSerializer (GetType());
				xml.Serialize (new FileStream ("TrafficReport.xml", FileMode.OpenOrCreate, FileAccess.Write),this);
			} catch(Exception e) {
				Log.error("Error saving config" + e.ToString());
			}
		}
	}

	public class QueryToolGUIBase : MonoBehaviour
	{
		Texture icon;
		Texture activeIcon;

		Config config;
		//int lastWidth;

		GameObject[] visualizations;
		Material lineMaterial;
		Material lineMaterialHighlight;

		Dictionary<uint,HashSet<uint>> segmentMap; //Map segment to paths
		Dictionary<string, HashSet<uint>> typeMap;

		Report currentReport;
		uint currentSegment;

		public GUISkin uiSkin;
		public GUIStyle totalStyle;
		public GUIStyle buttonStyle;

		public bool leftHandDrive;

		Matrix4x4 guiScale;

		bool inDrag;
		Vector2 dragOffset;
		int lastScreenHeight;

		static VehicleDisplay[] vechicleTypes = {
			new VehicleDisplay { id =  "citizen", display = "Pedestrian" },

			new VehicleDisplay { id =  "Residential/ResidentialLow", display = "Car" },

			new VehicleDisplay { id =  "Industrial/IndustrialGeneric", display = "Cargo truck" },
			new VehicleDisplay { id =  "Industrial/IndustrialOil", display = "Oil Tanker" },
			new VehicleDisplay { id =  "Industrial/IndustrialOre", display = "Ore Truck" },
			new VehicleDisplay { id =  "Industrial/IndustrialForestry", display = "Log Truck" },
			new VehicleDisplay { id =  "Industrial/IndustrialFarming", display = "Tractor" },

			new VehicleDisplay { id =  "HealthCare/None", display = "Ambulance" },
			new VehicleDisplay { id =  "Garbage/None", display = "Garbage Truck" },
			new VehicleDisplay { id =  "PoliceDepartment/None", display = "Police Car" },
			new VehicleDisplay { id =  "FireDepartment/None", display = "Fire truck" },

			
			new VehicleDisplay { id =  "PublicTransport/PublicTransportBus", display = "Bus" },

		};


		public QueryToolGUIBase()
		{   
			config = Config.Load ();
		}

		public virtual bool toolActive {
			get { return true; }
			set  { 
				Log.error("Function not overidden");
			}
		}

        public virtual bool guiVisible
        {
			get { return true; }
		}

        public void Awake()
        {
			/*
			Log.debug ("Listing shaders");
			Shader[] shaders = (Shader[])Resources.FindObjectsOfTypeAll (typeof(Shader));
			foreach(Shader shader in shaders) {
				Log.debug (shader.name);
			}*/

            icon = ResourceLoader.loadTexture(80, 80, "Materials/Button.png");
            activeIcon = ResourceLoader.loadTexture(80, 80, "Materials/Button.active.png");

			Log.info("Load Line Material...");

			Color red = new Color (1, 0, 0);
			Color gold = new Color (1, 0.9f, 0);

			//string shader = ResourceLoader.loadResourceString ("Materials\\Shaders\\Normal-VertexLit.shader");
			string lineShader = ResourceLoader.loadResourceString ("Materials/Shaders/TransparentVertexLit.shader");
			/*Shader lineShader = null;

			try {
				lineShader = Shader.Find("Legacy Shaders/VertexLit");
			}catch(Exception e) {
				Log.error(e.Message);
			}

			try {
				if(lineShader == null) lineShader =  Shader.Find("Transparent/VertexLit");
			}catch(Exception e) {
				Log.error(e.Message);
			}*/

			lineMaterial = new Material (lineShader);
			lineMaterial.color = red;
			lineMaterial.SetColor("_Emission", red);
			lineMaterial.mainTexture = ResourceLoader.loadTexture(100, 200, "Materials/NewSkin.png");
			lineMaterial.renderQueue = 100;
			
			lineMaterialHighlight = new Material (lineMaterial);
			lineMaterialHighlight.color = gold;
			lineMaterialHighlight.SetColor("_Emission", gold);
			lineMaterial.renderQueue = 101;


			MakeSkin ();

			Log.debug ("Gui initialized");
        }

		void MakeSkin() {

			Color highlight = new Color (20.0f / 255, 207.0f / 255, 248.0f / 255);

			uiSkin = GUISkin.CreateInstance<GUISkin>();
			uiSkin.window.normal.background = ResourceLoader.loadTexture(32, 32, "Materials/UIbg.png");
			uiSkin.window.border = new RectOffset (16, 16, 16, 16);
			uiSkin.window.padding = new RectOffset (12, 8, 8, 12);

			uiSkin.window.normal.textColor = highlight;
			uiSkin.window.alignment = TextAnchor.UpperCenter;
			uiSkin.window.fontSize = 30;
			uiSkin.window.fontStyle = FontStyle.Bold;

			uiSkin.window.onNormal = uiSkin.window.normal;
			uiSkin.window.onFocused = uiSkin.window.onNormal;
			uiSkin.window.onHover = uiSkin.window.onNormal;
			uiSkin.window.onActive = uiSkin.window.onNormal;

			uiSkin.label.normal.textColor = Color.white;
			uiSkin.label.fontSize = 18;
			uiSkin.label.fontStyle = FontStyle.Bold;
			uiSkin.label.padding = new RectOffset (0, 0, 5, 5);

			totalStyle = new GUIStyle (uiSkin.label);
			totalStyle.normal.textColor = highlight;
			totalStyle.fontSize = 20;
			totalStyle.fontStyle = FontStyle.Bold;


		}

		void Update() {
			//Animate the traffic lines
			lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));
			lineMaterialHighlight.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));

			if (lastScreenHeight != Screen.height) {
				float s = Screen.height / 1440.0f;
				guiScale = Matrix4x4.Scale (new Vector3 (s, s, s));
				lastScreenHeight = Screen.height;
			}


			Vector2 pos = Input.mousePosition;
			pos.x = pos.x * 1440.0f / Screen.height;
			pos.y = (Screen.height - pos.y) * 1440.0f / Screen.height;

			if (!inDrag && Input.GetMouseButtonDown (1)) {
				inDrag = true;
				dragOffset = pos-config.buttonPos.position;
			} else if (inDrag) {
				config.buttonPos.position = pos-dragOffset;
				if(Input.GetMouseButtonUp(1)){
					inDrag=false;
					config.Save();
				}
			}
		}

		public void OnGUI()
		{
			if (!guiVisible)
				return;

			GUI.matrix = guiScale;
			GUI.skin = uiSkin;

			
			try 
			{


				GUI.DrawTexture (config.buttonPos, toolActive ? activeIcon : icon);//
				if(GUI.Button(config.buttonPos, "")) {
					Log.info ("Toggling tool");
					toolActive = !toolActive;
				}


				if (toolActive && currentReport != null) {

					Rect r = GUILayout.Window (50199, new Rect (20, 100, 200, 100), ReportSummary, "All Selected");
					if(segmentMap.ContainsKey(currentSegment)) {
						GUILayout.Window (50198, new Rect (240,100, 200, 100), HighlightSummary, "Highlighted");
					}
				}
			
			}catch(Exception e) {
				Log.error (e.Message);
				Log.error(e.StackTrace);
			}
			
			GUI.matrix = Matrix4x4.identity;
			GUI.skin = null;

		}

		void ReportSummary (int id)
		{			

			GUILayout.Space (35);
			
			int remaining = currentReport.allEntities.Length;
			foreach (VehicleDisplay t in vechicleTypes) {

				int count = 0;
				if(typeMap.ContainsKey(t.id))
					count = typeMap[t.id].Count;

				remaining -= count;
				if(count > 0) {
					GUILayout.Label (t.display + ": " + count);
				}
			}
			
			if(remaining > 0) {
				GUILayout.Label ("Other: " + remaining);
			}

			GUILayout.Label ("Total: " + currentReport.allEntities.Length,totalStyle);

		}

		void HighlightSummary (int id)
		{			
			try 
			{
			GUILayout.Space (35);

			if(!segmentMap.ContainsKey(currentSegment)) {
				GUILayout.Label("No data");
				GUILayout.Label("No data");
				GUILayout.Label("No data");
				GUILayout.Label("No data");
				return;
			}

						
			int remaining = segmentMap [currentSegment].Count;
			foreach (VehicleDisplay t in vechicleTypes) {
				
				int count = 0;
				foreach(uint e in  segmentMap[currentSegment]){
					if(currentReport.allEntities[e].serviceType == t.id) {
						count++;
						remaining--;
					}
				}
			
				if(count > 0) {
					GUILayout.Label (t.display + ": " + count);
				}
			}

			if(remaining > 0) {
				GUILayout.Label ("Other: " + remaining);
			}

			GUILayout.Label ("Total: " + segmentMap[currentSegment].Count,totalStyle);
		
			}catch(Exception e) {
				Log.error (e.Message);
				Log.error (e.StackTrace);
			}
		}

		public void SetReport(Report report) {

			if (visualizations != null) {
				RemoveAllPaths();
				currentReport = null;
				segmentMap = null;
				typeMap = null;
			}

			if (report == null || report.allEntities == null) {
				Log.debug ("Report NULL");
				return;
			}

			visualizations = new GameObject[report.allEntities.Length];
			for(int i=0; i < report.allEntities.Length; i++)
			{
				visualizations[i] =  CreatePathGameobject(report.allEntities[i].path);
			}
			
			float alpha = 30.0f / report.allEntities.Length;
			
			if (alpha > 1)
			{
				alpha = 1;
			}
			
			lineMaterial.color = new Color(1, 0, 0, alpha);

			GenerateMaps (report);

			currentReport = report;
		}

		private void GenerateMaps(Report report) {
			segmentMap = new Dictionary<uint,HashSet<uint>> ();
			typeMap = new Dictionary<string, HashSet<uint>> ();

			for (uint i =0; i < report.allEntities.Length; i++) {
				foreach(PathPoint p in report.allEntities[i].path) {

					if(!segmentMap.ContainsKey(p.segmentID)){
						segmentMap[p.segmentID] = new HashSet<uint>();
					}

					segmentMap[p.segmentID].Add(i);
				}

				string t = report.allEntities[i].serviceType;
				if(t == null){
					continue;
				}
				if(!typeMap.ContainsKey(t)){
					typeMap[t] = new HashSet<uint>();
				}
				typeMap[t].Add(i);
			}

		}

		public void SetSegmentHighlight(uint segmentID){

			if (currentReport == null) {
				return;
			}

			if (currentSegment == segmentID) {
				return;
			}

			if (segmentMap.ContainsKey(currentSegment)) {
				foreach (uint index in segmentMap[currentSegment]) {

					visualizations [index].GetComponent<Renderer> ().material = lineMaterial;
				}
			}
			
			if (segmentMap.ContainsKey (segmentID)) {
				foreach (uint index in segmentMap[segmentID]) {
					visualizations [index].GetComponent<Renderer> ().material = lineMaterialHighlight;
				}
			}

			currentSegment = segmentID;
		}
		
		private GameObject CreatePathGameobject(PathPoint[] positions) {
			
			lineMaterial.color = new Color(1, 0, 0, 1);
			
			PathMeshBuilder pb = new PathMeshBuilder();
			if (leftHandDrive)
			{
				pb.driveLane = -1;
			}
			
			
			Vector3[] points = new Vector3[positions.Length];
			for (int i=0; i < positions.Length; i++) {
				points[i] = new Vector3(positions[i].x, positions[i].y, positions[i].z);
			}
			
			
			pb.AddPoints(points);
			
			Mesh m = pb.GetMesh();
			GameObject go = new GameObject(); ;
			go.AddComponent<MeshFilter>();
			go.AddComponent<MeshRenderer>();
			go.GetComponent<MeshFilter>().mesh = m;
			go.GetComponent<MeshFilter>().sharedMesh = m;
			go.GetComponent<Renderer>().material = lineMaterial;
			go.transform.localPosition = new Vector3(0, 3, 0);
			
			return go;
		}
		
		
		void RemoveAllPaths()
		{
			if (visualizations == null) {
				return;
			}
			
			foreach (GameObject v in visualizations)
			{
				GameObject.Destroy(v);
			}
			
			visualizations = null;
		}
	}
}

