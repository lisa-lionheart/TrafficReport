using System;
using UnityEngine;
using System.Collections.Generic;

namespace TrafficReport
{
	public class PathMeshBuilder {


		public float width = 4f;
		public float laneOffset = -2.0f;

		public float curveRetractionFactor = 1.5f;
		public float lineScale = 0.5f;


		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		Vector3 lastPoint;
		float textureOffset = 0.0f;


		public void AddPoints(PathPoint[] points){

            lastPoint = points[0].pos;

            for (int i = 0; i < points.Length; i++)
            {
                
                Debug.DrawLine(points[i].pos, points[i].pos + Vector3.up, Color.blue, 20000);
                Debug.DrawLine(points[i].pos, points[i].pos + points[i].normal, Color.green, 20000);

            }

            for (int i = 0; i < points.Length - 1; i++)
            {
                PathPoint thisPoint = points[i];
                PathPoint nextPoint = points[i + 1];
                             
                if ((thisPoint.pos-nextPoint.pos).magnitude < 10.0f)
                {
                    i++;
                    nextPoint = points[i + 1];
                }


                Vector3 thisPointPos = thisPoint.pos;// +thisPoint.normal.normalized * 2.0f;
                Vector3 nextPointPos = nextPoint.pos;// -nextPoint.normal.normalized * 2.0f;

                //AddVertexPair(thisPoint.pos, thisPoint.normal.normalized);

                float segementLength =(thisPointPos-nextPointPos).magnitude;

                Vector3 p0 = thisPointPos;
                Vector3 p1 = thisPointPos + thisPoint.normal.normalized * segementLength/2.0f;
                Vector3 p2 = nextPointPos - nextPoint.normal.normalized * segementLength/2.0f;
                Vector3 p3 = nextPointPos;

                float step = 0.1f;
                int textureRepeats = (int)Math.Floor(segementLength / (width*2));

                //Todo change step based on curve factor

                for (float a = 0; a < 1.0f; a += step)
                {
                    Vector3 point = Beizer.CalculateBezierPoint(a, p0, p1, p2, p3);
                    Vector3 pointB = Beizer.CalculateBezierPoint(a+step, p0, p1, p2, p3);
                    Vector3 fwd = (pointB - point).normalized;

                    Debug.DrawLine(point, point + fwd, Color.green, 2000);

                    AddVertexPair(point, fwd);

                    textureOffset += step * textureRepeats; //(a * width * curveRetractionFactor * lineScale) / 2.0f;

                }

            }


            //End the last segement
            AddVertexPair(points[points.Length - 1].pos, points[points.Length - 1].normal.normalized);


			GenerateIndiciesAsLineStrip ();

            Vector3 startFwd = (points[0].pos - points[1].pos);
            //			if (startFwd.magnitude < width * 3.0f) {
            //				startFwd = (points[0] - points [2]);
            //			}
            AddEndStop(points[0].pos, startFwd.normalized, false);

            Vector3 endFwd = (points[points.Length - 1].pos - points[points.Length - 2].pos);
            if (endFwd.magnitude < width * 3.0f)
            {
                endFwd = (points[points.Length - 1].pos - points[points.Length - 3].pos);
            }
            AddEndStop(points[points.Length - 1].pos, endFwd.normalized, true);
		}

		void AddEndStop(Vector3 point, Vector3 fwd, bool isEnd) {

			float v1 = 0.5f;
			float v2 = 0.26f;

			if(isEnd){
				v1 = 0.25f;
				v2 = 0.0f;
			}

			Vector3 offset = Vector3.Cross(fwd, Vector3.up);

			int baseVert = verts.Count;

			verts.Add (point);
			uvs.Add (new Vector2 (0.5f, v1));

			int steps = 32;
			float anglePerStep = ((float)Math.PI * 2) / steps;
			for (int step = 0; step <= steps; step++) {

				float angle = (step * anglePerStep);

				Vector3 p = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
				verts.Add(point + (p*width*2));
				uvs.Add (new Vector2 (0.5f, v2));
			}

			
			for (int i = baseVert; i < baseVert + steps; i++) {
				triangles.Add (baseVert);
				triangles.Add (i + 2);
				triangles.Add (i + 1);

				triangles.Add (baseVert);
				triangles.Add (i + 1);
				triangles.Add (i + 2);
			}
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

			textureOffset = 0;
			AddVertexPair (start, fwd);
			textureOffset = Mathf.Floor ((end - start).magnitude * lineScale / width);
			AddVertexPair (end, fwd);

		}

		void AddVertexPair(Vector3 point, Vector3 fwd) {
			Vector3 offset = Vector3.Cross(fwd, Vector3.up).normalized ;

			verts.Add (point - offset * (width /2));              
			verts.Add (point + offset * (width /2));
			
			uvs.Add (new Vector2 (textureOffset, 0.6f));
			uvs.Add (new Vector2 (textureOffset, 0.9f));

			lastPoint = point;

            //Debug.DrawLine(point, point + fwd,Color.green,20000);
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

