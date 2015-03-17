
using ICities;
using UnityEngine;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

using System.Reflection;
using System;

namespace TrafficReport
{
      
    public class TrafficReportMod : LoadingExtensionBase, IUserMod
    {
        public TrafficReportMod() {

            Assembly ass = Assembly.GetAssembly(typeof(TrafficReportMod));
			Log.info("Traffic Report Loaded " + DateTime.Now.ToString() + "  " + ass.ToString());

            RegisterTool();
		}
        
        public string Name
        {
            get { return "Traffic Report Tool"; }
        }
        public string Description
        {
            get { return "Display traffic information for a single vehicle, a section of road or a building"; }
        }


        void DeRegister()
        {
            
            GameObject gameController = GameObject.FindWithTag("GameController");
            if (gameController)
            {
                try
                {
                    Component existing = gameController.GetComponent("TrafficReportMod");
                    if (existing)
                    {
                        Log.info("Query tool already registed, removing");
                        GameObject.Destroy(existing);
                    }
                    
                    GameObject existinggui = GameObject.FindGameObjectWithTag("QueryToolGUI");
                    if (existinggui)
                    {
                        Log.info("Query tool already registed, removing");
                        GameObject.Destroy(existinggui);
                    }
                }
                catch (UnityException)
                {
                    Log.info("No gui exist yet");
                }

            }
        }


        void RegisterTool()
        {

            DeRegister(); 
            
            Log.info("Register badger");

            try {
                GameObject gameController = GameObject.FindWithTag("GameController");
                if (gameController)
                {
                    Log.debug(gameController.ToString());
                    QueryTool q = gameController.AddComponent<QueryTool>();

                }
            }
            catch (Exception e)
            {
                Log.error(e.ToString());
            }
        }


		public override void OnCreated(ILoading loading) {
            RegisterTool();
			Log.info ("onLoaded");
		}

		public override void OnReleased() {
			Log.info ("onReleased");
		}



        public override void OnLevelLoaded(LoadMode mode)
        {
            RegisterTool();

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

