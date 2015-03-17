using System;
using UnityEngine;
using System.Collections.Generic;

namespace TrafficReport
{
	public class PathMeshBuilder {


		public float width = 5f;
		public float laneOffset = -2.0f;

		public int driveLane = 1; // 1=rhd, -1 =lhd, 0=down the middle


		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		Vector3 lastPoint;
		float textureOffset = 0.0f;


		public void AddPoints(Vector3[] points){

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

					float step = 0.2f;
					for(float a = step ; a < 1.0f; a += step) {
						Vector3 point = Beizer.CalculateBezierPoint(a, p0,p1,p2,p3);
						Vector3 fwd = Vector3.Lerp(startDir,endDir, a);
						AddVertexPair(point,fwd);
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
			Vector3 offset = Vector3.Cross(fwd, Vector3.up).normalized ;

			point += offset * ((float)driveLane * laneOffset);
			
			textureOffset += (point-lastPoint).magnitude / width;

			verts.Add (point - offset * (width /2));              
			verts.Add (point + offset * (width /2));
			
			uvs.Add (new Vector2 (textureOffset, 0.6f));
			uvs.Add (new Vector2 (textureOffset, 0.9f));

			lastPoint = point;
		}

		public Mesh GetMesh() {
			Mesh m = new Mesh();
			m.vertices = verts.ToArray();
			m.triangles = triangles.ToArray();
			m.uv = uvs.ToArray();
			m.RecalculateNormals();
			return m;
		}
	}
}

