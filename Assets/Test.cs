using UnityEngine;
using System.Collections;

using TrafficReport;
using System.IO;
using System.Collections.Generic;

public class Test : MonoBehaviour {

	Material material;
	List<Vector3> points = new List<Vector3> ();

	void OnPreRender() {
	}


	void Update() {

		
		Debug.DrawLine(points[0], points[0] + Vector3.up* 10.0f, Color.blue);
		
		material.SetTextureOffset ("_MainTex", new Vector2 (Time.time * -0.5f, 0));
		foreach (Vector3 p in points) {
			Debug.DrawLine(p, p + Vector3.up* 5.0f, Color.red);
		}
	}

	// Use this for initialization
	void Start () {

		material = Resources.Load ("Materials/Line") as Material;



		char[] sep = new char[]{' '};

	
	
		
		foreach(string line in File.ReadAllLines ("Assets/path1.txt")) { 
			string[] parts = line.Split (sep);
			Vector3 p = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
			points.Add(p);
		}
		

		PathMeshBuilder pb = new PathMeshBuilder ();
		
		pb.driveLane = -1;
		
		pb.AddPoints (points.ToArray());

		Mesh m = pb.GetMesh ();
		GameObject go = new GameObject ();;
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		go.GetComponent<MeshFilter> ().mesh = m;
		go.GetComponent<MeshFilter>().sharedMesh = m;
		go.GetComponent<Renderer> ().material = material;

		pb = new PathMeshBuilder ();
		pb.driveLane = -1;

		/*points.Reverse ();

		pb.AddPoints (points.ToArray());
		
		m = pb.GetMesh ();
		go = new GameObject ();;
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		go.GetComponent<MeshFilter> ().mesh = m;
		go.GetComponent<MeshFilter>().sharedMesh = m;
		go.GetComponent<Renderer> ().material = material;*/
	
	}

}
