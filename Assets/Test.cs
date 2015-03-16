using UnityEngine;
using System.Collections;

using TrafficReport;
using System.IO;
using System.Collections.Generic;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {

		List<Vector3> points = new List<Vector3> ();

		char[] sep = new char[]{' '};

	
	
		
		foreach(string line in File.ReadAllLines ("Assets/path1.txt")) { 
			string[] parts = line.Split (sep);
			Vector3 p = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
			points.Add(p);
		}
		

		PathMeshBuilder pb = new PathMeshBuilder ();
		
		pb.AddPoints (points.ToArray());

		Mesh m = pb.GetMesh ();
		GameObject go = gameObject;
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();
		go.GetComponent<MeshFilter> ().mesh = m;
		go.GetComponent<MeshFilter>().sharedMesh = m;
		//go.GetComponent<Renderer>().material = material;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
