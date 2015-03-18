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
			icon = ResourceLoader.loadTexture(80,80, "Materials.Button.png");
			activeIcon = ResourceLoader.loadTexture(80,80, "Materials.Button.active.png");

		}

		public bool toolActive {
			get { return false; }
			set  { 

			}
		}

		public bool guiVisible {
			get { return true; }
			set  { 

			}
		}

		void OnGUI()
		{
			if (!guiVisible)
			{
				return;
			}


			//GUI.Label(new Rect(70, 150, 100, 30), "This is a test label");

			if (lastWidth != Screen.width)
			{
				//Built for 144p scale up or down as appropriate
				float scale = Screen.width / 2560.0f;
				buttonPos = new Rect(80 * scale, 5 * scale, 80 * scale, 80 * scale);
				lastWidth = Screen.width;
			}

			if (toolActive)
			{
				Graphics.DrawTexture(buttonPos, activeIcon);
				if(GUI.Button(buttonPos," ",GUIStyle.none)) {
					toolActive = true;
				}
			}
			else
			{
				Graphics.DrawTexture(buttonPos, icon);
				if (GUI.Button(buttonPos, " " , GUIStyle.none))
				{
					Log.info("Selecting query tool");
					toolActive = false;
				}
			}


		}

	}
}

