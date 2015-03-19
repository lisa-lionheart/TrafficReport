using System;
using UnityEngine;

namespace TrafficReport
{
	public class QueryToolGUIBase : MonoBehaviour
	{
		Texture icon;
		Texture activeIcon;
		Rect buttonPos;
		int lastWidth;

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

            icon = ResourceLoader.loadTexture(80, 80, "Button");
            activeIcon = ResourceLoader.loadTexture(80, 80, "Button.active");
            gameObject.AddComponent<Canvas>();
        }

		void OnGUI()
		{
			if (!guiVisible)
			{
				return;
			}


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
				Graphics.DrawTexture(buttonPos, activeIcon);
				if(GUI.Button(buttonPos," ",GUIStyle.none)) {
					Log.info ("Selecting default tool");
					toolActive = false;
				}
			}
			else
			{
				Graphics.DrawTexture(buttonPos, icon);
				if (GUI.Button(buttonPos, " " , GUIStyle.none))
				{
					Log.info("Selecting query tool");
					toolActive = true;
				}
			}


			GUI.matrix = Matrix4x4.identity;

		}

	}
}

