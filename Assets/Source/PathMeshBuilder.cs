using System;
using UnityEngine;
using System.Collections.Generic;

namespace TrafficReport
{
	public class PathMeshBuilder {


		public float width = 4f;
        public float endStopSize = 3.0f;
		public float laneOffset = -2.0f;

		public float curveRetractionFactor = 1.5f;
		public float lineScale = 0.5f;

        public float duplicatePointThreshold = 5.0f;


		List<Vector3> verts = new List<Vector3> ();
		List<int> triangles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();
		Vector3 lastPoint;
		float textureOffset = 0.0f;

        public List<PathPoint> FilterPoints(PathPoint[] points)
        {
            List<PathPoint> o = new List<PathPoint>();

            int j = 0;
            for (int i = 0; i < points.Length -1; i++)
            {
                if ((points[j].pos - points[i].pos).magnitude < duplicatePointThreshold)
                    continue;
                
                // KLUDGE: Filter out tight U-Turns
                if (
                    i > points.Length - 4 
                    && (Vector3.Angle(points[j].forwards, points[i].forwards) > 170.0f)
                    && (points[j].pos - points[i].pos).magnitude < duplicatePointThreshold
                )
                    continue;
                
                j = i;
                o.Add(points[i]);
            }


            
            points[points.Length - 1].forwards = Vector3.zero;// points[points.Length - 1].pos - points[j].pos;



            o.Add(points[points.Length - 1]);
            return o;
        }

		public void AddPoints(PathPoint[] _points){

            List<PathPoint> points = FilterPoints(_points);

            lastPoint = points[0].pos;


            for (int i = 0; i < points.Count - 1; i++)
            {

                Debug.DrawLine(points[i].pos, points[i].pos + Vector3.up, points[i].guessed ? Color.cyan : Color.blue, 20000);

                Debug.DrawLine(points[i].pos, points[i].pos + points[i].forwards, Color.green, 20000);
                Debug.DrawLine(points[i].pos, points[i].pos + points[i].backwards, Color.grey, 20000);

                

                PathPoint thisPoint = points[i];
                PathPoint nextPoint = points[i + 1];

                
               // Debug.Log(i + ":" + "Angle:" + Vector3.Angle(thisPoint.normal, nextPoint.normal) + " Magnitude: " + (thisPoint.pos - nextPoint.pos).magnitude);
                                

                Vector3 thisPointPos = thisPoint.pos;
                Vector3 nextPointPos = nextPoint.pos;


                float angle = Vector3.Angle(thisPoint.forwards, nextPoint.forwards);
                float segementLength = (thisPointPos - nextPointPos).magnitude;

                Vector3 p0 = thisPointPos;
                Vector3 p1 = thisPointPos + thisPoint.forwards / 3.0f;
                Vector3 p2 = nextPointPos + nextPoint.backwards / 3.0f;

                //Seperate logic for tight turns
                if (segementLength < 30.0f)
                {
                    p1 = thisPointPos + thisPoint.forwards.normalized * segementLength / 2.0f;
                    p2 = nextPointPos + nextPoint.backwards.normalized * segementLength / 2.0f;
                }

                Vector3 p3 = nextPointPos;

             //   Debug.DrawLine(p0, p1, Color.cyan, 2000);
             //   Debug.DrawLine(p2, p3, Color.cyan, 2000);
                                              
                
                //First calculate the real length of the spline with a rough calulation
                float realLength = 0;
                float maxAngle = 0;
                for (float a = 0; a < 1.0f; a += 0.1f)
                {
                    Vector3 pointA = Beizer.CalculateBezierPoint(a, p0, p1, p2, p3);
                    Vector3 pointB = Beizer.CalculateBezierPoint(a + 0.2f, p0, p1, p2, p3);

                    float ang = Vector3.Angle(pointB-p0, p1-p0);
                    if (ang > maxAngle) {
                        maxAngle = ang;
                    }

                    realLength += (pointA - pointB).magnitude;
                }

                // Make each segment tile a whole number of texture repeats so that
                // overlaping paths line up
                int textureRepeats = (int)Math.Floor(realLength / (width * 2));

                textureOffset = (float)Math.Round(textureOffset);

                //Aim to keep a fixed size for each quad, increaing the number the more it curves
                int steps = (int)Math.Floor(realLength / 20.0f + maxAngle / 3.0f) + 1;
                float step = 1.0f / steps;
                for (float a = 0; a < 1.0f; a += step)
                {
                    Vector3 point = Beizer.CalculateBezierPoint(a, p0, p1, p2, p3);
                    Vector3 pointB = Beizer.CalculateBezierPoint(a+step, p0, p1, p2, p3);
                    Vector3 fwd = (pointB - point).normalized;

                    Debug.DrawLine(point+ Vector3.up, point + Vector3.up + fwd, Color.green, 2000);

                    AddVertexPair(point, fwd);

                    textureOffset += step * textureRepeats; 

                }

            }


            //End the last segement
            AddVertexPair(points[points.Count - 1].pos, points[points.Count - 1].forwards.normalized);

			GenerateIndiciesAsLineStrip ();

            AddEndStop(points[0].pos, points[0].forwards.normalized, false);
            AddEndStop(points[points.Count - 1].pos, points[points.Count - 1].forwards.normalized, true);
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
				verts.Add(point + (p*endStopSize));
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
            m.Optimize();
			return m;
		}
	}
}

