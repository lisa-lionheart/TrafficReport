using ICities;
using UnityEngine;
using ColossalFramework.UI;

namespace TrafficReport
{
    public class TestPanel : UIPanel
    {

        public override void Start ()
        {
            //this makes the panel "visible", I don't know what sprites are available, but found this value to work
            this.backgroundSprite = "GenericPanel";
            this.color = new Color32(255,0,0,100);
            this.width = 100;
            this.height = 200;

        // and then something like
            UILabel l = this.AddUIComponent<UILabel> ();
            l.text = "I am a label";
            l.eventClick += new MouseEventHandler(thingClicked);
                
        }

        internal void thingClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            Log.info("You clicked the thing");
        }
    }
}
