using System;
using UnityEngine;
using TrafficReport;
using System.Collections.Generic;

namespace TrafficReport
{
	public class QueryToolGUIBase : MonoBehaviour
	{
		Texture icon;
		Texture activeIcon;
		Rect buttonPos  = new Rect(80, 5, 80, 80);
		//int lastWidth;

		GameObject[] visualizations;
		Material lineMaterial;
		Material lineMaterialHighlight;

		Dictionary<uint,HashSet<uint>> segmentMap; //Map segment to paths

		Report currentReport;
		uint currentSegment;

		public bool leftHandDrive;

		public QueryToolGUIBase()
		{    
		}

		public virtual bool toolActive {
			get { return false; }
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

            icon = ResourceLoader.loadTexture(80, 80, "Materials\\Button.png");
            activeIcon = ResourceLoader.loadTexture(80, 80, "Materials\\Button.active.png");

			Log.info("Load Line Material...");

			Color red = new Color (1, 0, 0);
			Color gold = new Color (1, 0.9f, 0);

			lineMaterial = new Material(ResourceLoader.loadResourceString("Materials\\Shaders\\TransparentVertexLit.shader"));
			lineMaterial.color = red;
			lineMaterial.SetColor("_Emission", red);
			lineMaterial.mainTexture = ResourceLoader.loadTexture(100, 200, "Materials\\NewSkin.png");

			
			lineMaterialHighlight = new Material(ResourceLoader.loadResourceString("Materials\\Shaders\\TransparentVertexLit.shader"));
			lineMaterialHighlight.color = gold;
			lineMaterialHighlight.SetColor("_Emission", gold);
			lineMaterialHighlight.mainTexture = ResourceLoader.loadTexture(100, 200, "Materials\\NewSkin.png");

			Log.debug ("Gui initialized");
        }

		void OnGUI()
		{
			if (!guiVisible)
			{
				return;
			}

			//Animate the traffic lines
			lineMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * -0.5f, 0));


			//GUI.Label(new Rect(70, 150, 100, 30), "This is a test label");
			/*
			if (lastWidth != Screen.width)
			{
				//Built for 144p scale up or down as appropriate
				float scale = Screen.width / 2560.0f;
				buttonPos = new Rect(80 * scale, 5 * scale, 80 * scale, 80 * scale);
				lastWidth = Screen.width;
			}*/

			GUI.matrix = Matrix4x4.Scale (Vector3.one * Screen.width / 2560.0f);

			if (toolActive)
			{
				GUI.DrawTexture(buttonPos, activeIcon);
                if(GUI.Button(buttonPos," ",GUIStyle.none)) {
					Log.info ("Selecting default tool");
					toolActive = false;
				}
			}
			else
			{
				GUI.DrawTexture(buttonPos, icon);
				if (GUI.Button(buttonPos, " " , GUIStyle.none))
				{
					Log.info("Selecting query tool");
					toolActive = true;
				}
			}


			GUI.matrix = Matrix4x4.identity;

		}


		public void SetReport(Report report) {

			if (visualizations != null) {
				RemoveAllPaths();
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
			
			//lineMaterial.color = new Color(1, 0, 0, alpha);

			GenerateSegmentMap (report);

			currentReport = report;
		}

		private void GenerateSegmentMap(Report report) {
			segmentMap = new Dictionary<uint,HashSet<uint>> ();

			for (uint i =0; i < report.allEntities.Length; i++) {
				foreach(PathPoint p in report.allEntities[i].path) {

					if(!segmentMap.ContainsKey(p.segmentID)){
						segmentMap[p.segmentID] = new HashSet<uint>();
					}

					segmentMap[p.segmentID].Add(i);
				}
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
					visualizations [index].transform.localPosition = new Vector3 (0, 3, 0);
					visualizations [index].GetComponent<Renderer> ().material = lineMaterial;
				}
			}
			
			if (segmentMap.ContainsKey (segmentID)) {
				foreach (uint index in segmentMap[segmentID]) {
					visualizations [index].transform.localPosition = new Vector3 (0, 15, 0);
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

