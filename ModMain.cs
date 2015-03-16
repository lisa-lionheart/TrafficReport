
using ICities;
using UnityEngine;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

using System.Reflection;
using System;

namespace TrafficReport
{
      
	public class TrafficReportMod : IUserMod
    {
		public TrafficReportMod() {
			Log.info("Mod Loaded");
		}
        
        public string Name
        {
            get { return "Traffic Report Tool"; }
        }
        public string Description
        {
            get { return "Display traffic information for a single vehicle, a section of road or a building"; }
        }
    }

    



    public class LoadingExtension : LoadingExtensionBase
    {

        //QueryTool queryTool;
        //UIButton button;
		//Thread: Main
		public override void OnCreated(ILoading loading) {
		
			Log.info ("onLoaded");
		}

		public override void OnReleased() {
			Log.info ("onReleased");
		}

        public override void OnLevelLoaded(LoadMode mode)
        {
            GameObject.FindWithTag("GameController").AddComponent<QueryTool>();
            ToolsModifierControl.SetTool<DefaultTool>();

            /*
            UIView uiView = UIView.GetAView();

          

            // Add a new button to the view.
            UIButton button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

            // Set the text to show on the button.
            button.text = "Traffic Report Tool";

            // Set the button dimensions.
            button.width = 150;
            button.height = 40;

            // Style the button to look like a menu button.
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);

            // Place the button.
            button.transformPosition = new Vector3(-1.65f, 0.97f);

            button.eventClick += toggleQueryTool;
             * */
        }

        void toggleQueryTool(UIComponent component, UIMouseEventParameter eventParam)
        {
			/*
            if(ToolsModifierControl.toolController.CurrentTool == queryTool) {
                ToolsModifierControl.SetTool<DefaultTool>();
                button.normalBgSprite = "ButtonMenu";
            }
            else
            {
                Log.info("Selecting query tool");
                ToolsModifierControl.toolController.CurrentTool = queryTool;
                button.normalBgSprite = "ButtonMenuPressed";
            }*/
        }
    }

 }

