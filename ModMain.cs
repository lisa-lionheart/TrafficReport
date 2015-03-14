
using ICities;
using UnityEngine;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

using System.Reflection;
using System;

namespace TrafficReport
{
      
    public class TestMod : IUserMod
    {
        System.Version verison;

        public TestMod()
        {
            Assembly asembly = Assembly.GetAssembly(typeof(TestMod));
            this.verison = asembly.GetName().Version;
            Log.info("Mod class created " + this.verison);
        }
        
        public string Name
        {
            get { return "Traffic Query Tool"; }
        }
        public string Description
        {
            get { return "Select vehicle with this tool active to see path drawn to their destination"; }
        }
    }


    public class LoadingExtension : LoadingExtensionBase
    {

        QueryTool queryTool;
        UIButton button;
        
        public override void OnLevelLoaded(LoadMode mode)
        {
            queryTool = GameObject.FindWithTag("GameController").AddComponent<QueryTool>();
            ToolsModifierControl.SetTool<DefaultTool>();

            UIView uiView = UIView.GetAView();

          

            // Add a new button to the view.
            button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

            // Set the text to show on the button.
            button.text = "Traffic Query Tool";

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
        }

        void toggleQueryTool(UIComponent component, UIMouseEventParameter eventParam)
        {

            if(ToolsModifierControl.toolController.CurrentTool == queryTool) {
                ToolsModifierControl.SetTool<DefaultTool>();
                button.normalBgSprite = "ButtonMenu";
            }
            else
            {
                Log.info("Selecting query tool");
                ToolsModifierControl.toolController.CurrentTool = queryTool;
                button.normalBgSprite = "ButtonMenuPressed";
            }
        }
    }

 }

