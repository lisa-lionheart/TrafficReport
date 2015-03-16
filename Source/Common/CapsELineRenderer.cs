using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CapsELineRenderer : MonoBehaviour {

	public float width = 1f;
	public Material material;

	// Use this for initialization
	void Start () {

		List<Vector3> points = new List<Vector3> ();
		
		char[] sep = new char[]{' '};
		
		foreach(string line in File.ReadAllLines ("Assets/path1.txt")) { 
			string[] parts = line.Split (sep);
			Vector3 p = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
			points.Add(p);
		}
		
					                        

		RenderLine (points.ToArray());
	}
	
	// Calculates the distance from the given Points to the edges of the line you want to render
	private float[] RotateVector(Vector3 a, Vector3 b, float width){
		Vector3 delta = a - b;	//Get the X,Y and Z Distances between point A and B
		Debug.Log (delta);
		float dist = Mathf.Sqrt(delta[0]*delta[0] + delta[2] * delta[2]); //Calculate the distance in 2D (Pythagoras)
		float alpha = Mathf.Asin(delta[2] / dist) + Mathf.PI; //Calculate the angle between the two points
		if (delta [0] > 0) {
			return new float[]{Mathf.Sin (alpha) * width * -1, Mathf.Cos (alpha) * width * -1, dist};
		} else {
			return new float[]{Mathf.Sin (alpha) * width, Mathf.Cos (alpha) * width, dist}; //Calculate the X and Z distance from the points
		}
	}


	//Generates a new Mesh and applies it to the gameObject
	void RenderLine(Vector3[] points){
		float o = width / 2;
		Vector3 offset = new Vector3 (o, 0, o);

		List<Vector3> verts = new List<Vector3>();
		
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();

		for(int i = 0;i < points.Length - 1; i++) {
			Vector3 a = points[i];
			Vector3 b = points[i + 1];

			float[] output = RotateVector(a, b, o); //Returns an array with [x,y, distance]
			offset = new Vector3(output[0], 0, output[1]); 	//Calculate offset to give the line a width
			float uvRepeat = output[2]/width;

			//Add the vertices and UVs (4 for every segement of the line)
			verts.Add(a - offset);				
			uvs.Add(new Vector2(0,0.5f));
			verts.Add(a + offset);
			uvs.Add(new Vector2(0,1));
			verts.Add(b - offset);
			uvs.Add(new Vector2(uvRepeat,0.5f));
			verts.Add(b + offset);
			uvs.Add(new Vector2(uvRepeat,1));

			//Add Vertices and UVs for the graphic on every corner
			offset = new Vector3(o, 0, 0);
			Vector3 inverseOffset = new Vector3(0,0,o);
			verts.Add(a - offset);				
			uvs.Add(new Vector2(0,0.5f));
			verts.Add(a + offset);
			uvs.Add(new Vector2(1,0));
			verts.Add(a - inverseOffset);				
			uvs.Add(new Vector2(0,0));
			verts.Add(a + inverseOffset);
			uvs.Add(new Vector2(1,0.5f));

			//Setup the connections between the vertices. We have to connect all 4 vertices we just created counterclockwise to two triangles
			int c = i*8;
			triangles.Add(c);
			triangles.Add(c + 2);
			triangles.Add(c + 1);

			triangles.Add(c + 1);
			triangles.Add(c + 2);
			triangles.Add(c + 3);

			//Setup connections for the corners
			triangles.Add(c + 7);
			triangles.Add(c + 6);
			triangles.Add(c + 4);
			
			triangles.Add(c + 7);
			triangles.Add(c + 5);
			triangles.Add(c + 6);
		}


		GameObject go = gameObject;
		go.AddComponent<MeshFilter> ();
		go.AddComponent<MeshRenderer> ();


		//Creating the Mesh and assigning it to the gameObject
		Mesh m = new Mesh();
		m.name = "line";
		
		m.vertices = verts.ToArray();

		m.triangles = triangles.ToArray();
		m.uv = uvs.ToArray();
		m.RecalculateNormals();

		go.GetComponent<MeshFilter>().mesh = m;
		go.GetComponent<MeshFilter>().sharedMesh = m;
		go.GetComponent<Renderer>().material = material;
	}
}
