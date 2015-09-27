
using ICities;
using UnityEngine;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

using System.Reflection;
using System;
using TrafficReport.Util;
using TrafficReport.Assets.Source.UI;
using System.Collections.Generic;

namespace TrafficReport
{
      
    public class TrafficReportMod : LoadingExtensionBase, IUserMod
    {

		QueryTool queryTool;
    
        public string Name
        {
            get { return "Traffic Report Tool 2.0"; }
        }
        public string Description
        {
            get {

                //ReportButton btn = ReportButton.Create();

                
                //btn.eventClick += (UIComponent c, UIMouseEventParameter e) =>
                //{
                //    btn.ToggleState = !btn.ToggleState;
                //};
#if DEBUG
                ReportUI ui = ReportUI.Create();

                Dictionary<String, int> vals = new Dictionary<String, int>();

                vals["citizen"] = 10;
                vals["Residential/ResidentialLow"] = 20;
                vals["Industrial/IndustrialOre"] = 30;
                vals["Industrial/IndustrialOil"] = 35;

                vals["something"] = 0;

                ui.SetSelectedData(vals);
                ////ReportButton btn = ReportButton.Create();
                
#endif       
                return "Display traffic information for a single vehicle, a section of road or a building"; 
            
            }
        }

        public override void OnCreated(ILoading loading) {
        	Log.info ("onLoaded");
		}

		public override void OnReleased() {
			Log.info ("onReleased");
		}

        public override void OnLevelLoaded(LoadMode mode)
        {
            GameObject gameController = GameObject.FindWithTag("GameController");
            if (gameController)
            {
                Log.debug(gameController.ToString());
                queryTool = gameController.AddComponent<QueryTool>();
                queryTool.enabled = false;

            }
                              
        }


    }

 }

