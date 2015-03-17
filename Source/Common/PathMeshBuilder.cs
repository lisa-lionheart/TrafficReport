using System;
using UnityEngine;
using System.Collections.Generic;

namespace TrafficReport
{
	public class PathMeshBuilder {


		public float width = 5f;
		public float laneOffset = 5f; // set -5 for lefthand drive

		public Vector3[] p;
		public Material material;

		private float textureOffset = 0.0f;

		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		Vector3 lastPoint;

		//Generates a new Mesh and applies it to the gameObject
		public void AddPoints(Vector3[] points){

			float o = width / 2;
			Vector3 offset = new Vector3 (o, 0, o);
			lastPoint = points[0];

			for (int i = 0; i < points.Length - 1; i++) {

				Vector3 start = points [i];
				Vector3 end = points [i+1];

				if((end-start).magnitude < width * 3) {
					continue;
				}

				if(i != 0) {
					start += (end-start).normalized * (width*1.0f);

				}
				
				if(i != points.Length - 1) {
					end += (start-end).normalized * (width*1.0f);
				}

				
				textureOffset += (start-lastPoint).magnitude / width;
				AddSegment(start, end);


				if(i < points.Length - 2) {

					Vector3 cornerPoint = points[i+1];

					Vector3 nextStart = cornerPoint;
					Vector3 nextEnd = points[i+2];
					nextStart -= (nextStart-nextEnd).normalized * (width*1.0f);


					Vector3 p0 = end;
					Vector3 p1 = Vector3.Lerp (end,cornerPoint, 0.5f);
					Vector3 p2 = Vector3.Lerp (nextStart,cornerPoint, 0.5f);
					Vector3 p3 = nextStart;

					Vector3 startDir = (end-start).normalized;
					Vector3 endDir = (nextEnd-nextStart).normalized;

					float step = 0.2;
					for(float a = step ; a < 1.0f; a += step) {
						Vector3 point = Beizer.CalculateBezierPoint(0.25f, p0,p1,p2,p3);
						Vector3 fwd = Vector3.Lerp(
					}



				} 
			}

			GenerateIndiciesAsLineStrip ();
		}

		void GenerateIndiciesAsLineStrip(){

			for(int i = 0;  i < verts.Count-2; i +=2) {
				
				triangles.Add (i);
				triangles.Add (i + 2);
				triangles.Add (i + 1);
				
				triangles.Add (i + 1);
				triangles.Add (i + 2);
				triangles.Add (i);
				
				triangles.Add (i + 1);
				triangles.Add (i + 2);
				triangles.Add (i + 3);
				
				triangles.Add (i + 3);
				triangles.Add (i + 2);
				triangles.Add (i + 1);
			}

		}

		void AddSegment(Vector3 start, Vector3 end) {

			Vector3 fwd = (end - start).normalized;

			AddVertexPair (start, fwd);
			AddVertexPair (end, fwd);

		}

		void AddVertexPair(Vector3 point, Vector3 fwd) {
			Vector3 offset = Vector3.Cross(fwd, Vector3.up).normalized * width /2;
			
			//Add the vertices and UVs (4 for every segement of the line)
			verts.Add (start - offset);              
			uvs.Add (new Vector2 (textureOffset, 1.0f));

		
			verts.Add (start + offset);
			uvs.Add (new Vector2 (textureOffset, 0.5f));

			lastPoint = point;
		}

		public Mesh GetMesh() {

			//Creating the Mesh and assigning it to the gameObject
			Mesh m = new Mesh();
			m.name = "line";
			m.vertices = verts.ToArray();
			m.triangles = triangles.ToArray();
			m.uv = uvs.ToArray();
			m.RecalculateNormals();

			return m;

		
		}
	}
}

