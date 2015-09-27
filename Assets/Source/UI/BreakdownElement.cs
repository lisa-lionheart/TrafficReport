using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficReport.Util;
using UnityEngine;

namespace TrafficReport.Assets.Source.UI
{
    class BreakdownElement : UIPanel
    {

        static Color32 grey = new Color32(128, 128, 128, 128);
        static Color32 textColor = new Color32(206, 248, 0, 255);
        static Color32 hoverColor = new Color32(0, 206, 248, 255);
               

        internal String type;

        internal UILabel lbl;
        internal UILabel val;
        internal UILabel percent;
        internal UISprite block;
        bool lastHover;

        public override void Awake()
        {
            m_AutoLayout = false;


            block = AddUIComponent<UISprite>();
            block.spriteName = "EmptySprite";


            block.area = new Vector4(0, 0, 15, 15);


            lbl = AddUIComponent<UILabel>();
            lbl.autoSize = false;
            lbl.textScale = 0.8f;
            lbl.area = new Vector4(20, 0, 180, 15);

            val = AddUIComponent<UILabel>();
            val.autoSize = false;
            val.text = "0";
            val.textScale = 0.8f;
            val.area = new Vector4(100, 0, 50, 15);
            val.textAlignment = UIHorizontalAlignment.Right;

            percent = AddUIComponent<UILabel>();
            percent.autoSize = false;
            percent.text = "- 10%";
            percent.textScale = 0.8f;
            percent.area = new Vector4(150, 0, 50, 15);

            eventClick += toggleVisibility;

            base.Awake();

        }
        String formatPercent(float f)
        {
            if (f < 0.01f)
            {
                return Math.Round(f * 100.0F, 1) + "%";
            }
            else
            {
                return Math.Floor(f * 100.0F) + "%";
            }
        }

        public override void Update()
        {
            if (type == null)
                return;

            if (m_IsMouseHovering != lastHover)
            {
                lastHover = m_IsMouseHovering;
                Invalidate();
            }

            if (m_IsComponentInvalidated) { 

                bool onOff = Config.instance.IsTypeVisible(type);
                Color32 currentTextColor = m_IsMouseHovering ? hoverColor : (onOff ? textColor : grey);

                Color32 typeColor = Config.instance.GetTypeColor(type);

                block.color = onOff ? typeColor : grey;
                percent.textColor = currentTextColor;
                val.textColor = currentTextColor;
                lbl.textColor = currentTextColor;
                parent.Invalidate();
            }

            base.Update();
        }



        internal void SetVehicleDisplay(String type, int count, int ofTotal)
        {
            this.type = type;
            lbl.text = Config.instance.GetTypeDisplay(type);;

            val.text = count.ToString();

            if (Config.instance.IsTypeVisible(type))
            {
                percent.text = " - " + formatPercent(count / (float)ofTotal);
            }
            else
            {
                percent.text = " -  ---";
            }

            Invalidate();
        }


        void toggleVisibility(UIComponent component, UIMouseEventParameter eventParam)
        {
            Log.debug("Click");
            Config.instance.ToggleVisibility(type);
            parent.Invalidate();
        }
    }

}
