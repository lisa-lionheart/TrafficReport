
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

		QueryTool queryTool;

        public TrafficReportMod() {

            Assembly ass = Assembly.GetAssembly(typeof(TrafficReportMod));
			Log.info("Traffic Report Loaded " + DateTime.Now.ToString() + "  " + ass.ToString());

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
            
            try {
                GameObject gameController = GameObject.FindWithTag("GameController");
                if (gameController)
                {
                    Log.debug(gameController.ToString());
                    queryTool = gameController.AddComponent<QueryTool>();
                    ToolsModifierControl.SetTool<DefaultTool>();

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


           
        }

       
    }

 }

