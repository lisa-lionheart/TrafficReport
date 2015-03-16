using System;
using UnityEngine;
using System.Collections.Generic;

namespace TrafficReport
{
	public class PathMeshBuilder {


		public float width = 1f;
		public Vector3[] p;
		public Material material;

		private float textureOffset = 0.0f;

		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();

		// Calculates the distance from the given Points to the edges of the line you want to render
		private Vector3 RotateVector(Vector3 a, Vector3 b, float width){
			Vector3 delta = a - b;  //Get the X,Y and Z Distances between point A and B
			float dist = Mathf.Sqrt(delta[0]*delta[0] + delta[2] * delta[2]); //Calculate the distance in 2D (Pythagoras)
			float alpha = Mathf.Asin(delta[2] / dist) - Mathf.PI; //Calculate the angle between the two points
			return new Vector3(Mathf.Sin (alpha)* width, 0, Mathf.Cos(alpha)*width); //Calculate the X and Z distance from the points
		}


		//Generates a new Mesh and applies it to the gameObject
		void AddPoints(Vector3[] points){

			float o = width / 2;
			Vector3 offset = new Vector3 (o, 0, o);

			float alpha = 0;
			for (int i = 0; i < points.Length - 1; i++) {
				AddSegment(points [i], points [i+1]);
			}
		}

		void AddSegment(Vector3 start, Vector3 end) {

			offset = Vector3.Cross ((end-start), Vector3.up).normalized * width/2;

			//Add the vertices and UVs (4 for every segement of the line)
			verts.Add (start - offset);              
			uvs.Add (new Vector2 (textureOffset, 0));

			verts.Add (start + offset);
			uvs.Add (new Vector2 (textureOffset, 1));

			textureOffset += (send - start).magnitude / width;

			verts.Add (end - offset);
			uvs.Add (new Vector2 (textureOffset, 0));
			verts.Add (end + offset);
			uvs.Add (new Vector2 (textureOffset, 1));

			//Setup the connections between the vertices. We have to connect all 4 vertices we just created counterclockwise to two triangles
			int c = i * 4;
			triangles.Add (c);
			triangles.Add (c + 2);
			triangles.Add (c + 1);

			triangles.Add (c + 1);
			triangles.Add (c + 2);
			triangles.Add (c + 3);
		}

		Mesh GetMesh() {

			//Creating the Mesh and assigning it to the gameObject
			Mesh m = new Mesh();
			m.name = "line";
			m.vertices = verts.ToArray();
			m.triangles = triangles.ToArray();
			m.uv = uvs.ToArray();
			m.RecalculateNormals();

			return m;

			/*
			GameObject go = gameObject;
			go.GetComponent<MeshFilter>().mesh = m;
			go.GetComponent<MeshFilter>().sharedMesh = m;
			go.GetComponent<Renderer> ().material = material;*/
			*/
		}
	}
}

