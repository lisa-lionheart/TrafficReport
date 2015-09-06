using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficReport 
{

    public class Billboard : MonoBehaviour
    {

        public Vector3 position;

        void Start()
        {

        }

        void Update()
        {
            Vector3 currentCameraPos = Camera.main.transform.position;
            Vector3 fwd = (position - currentCameraPos).normalized;
            fwd.y = 0;
            gameObject.transform.position = position;
            gameObject.transform.LookAt(position + fwd, Vector3.up);          
  
        }
        

        public static Material CreateSpriteMaterial(Texture t, Color c)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = c;
            mat.mainTexture = t;

            return mat;
        }

        public static Billboard Create(Material icon)
        {
            GameObject newSprite = GameObject.CreatePrimitive(PrimitiveType.Quad);
            
            newSprite.transform.localScale = new Vector3(10, 10, 10);
            newSprite.name = "sprite";
            newSprite.GetComponent<MeshRenderer>().material = icon;

            return newSprite.AddComponent<Billboard>(); ;
        }

        
        public Material material { 
            get {
                return gameObject.GetComponent<Renderer>().material;
            }
            set {
                gameObject.GetComponent<Renderer>().material= value;
            }
        }
    }
}
