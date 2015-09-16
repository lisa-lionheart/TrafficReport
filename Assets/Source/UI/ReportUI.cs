using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficReport.Assets.Source.UI
{


    delegate void TypeHighlightEvent(String type);
   

    class BreakdownPannel : UIPanel
    {
        static Color32 grey = new Color32(128, 128, 128, 128);
        static Color32 textColor = new Color32(206, 248, 0, 255);
        static Color32 hoverColor = new Color32(0,206, 248, 255);


        internal UILabel title;
        UILabel total;
        UIRadialChart chart;
        internal bool interactive;
        internal event TypeHighlightEvent eventHighlightType;

        class BreakdownElement : UIPanel
        {
            internal VehicleDisplay vd;

            internal UILabel lbl;
            internal UILabel val;
            internal UILabel percent;
            internal UISprite block;
            internal UIRadialChart.SliceSettings linkedSlice;
            internal UIRadialChart linkedChart;


            bool lastOnOff;
            bool lastHover;

            public void Awake()
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
                val.area = new Vector4(100,0, 50, 15);
                val.textAlignment = UIHorizontalAlignment.Right;

                percent = AddUIComponent<UILabel>();
                percent.autoSize = false;
                percent.text = "- 10%";
                percent.textScale = 0.8f;
                percent.area = new Vector4(150, 0, 50, 15);

                eventClick += toggleVisibility;

            }

            public void Update()
            {
                if (vd == null)
                    return;

                if (vd.onOff != lastOnOff || m_IsMouseHovering != lastHover)
                {
                    lastOnOff = vd.onOff;
                    lastHover = m_IsMouseHovering;
                    UpdateColors();
                    Config.instance.NotifyChange();
                }
            }

            internal void SetVehicleDisplay(VehicleDisplay vd) {
                this.vd = vd;
                lbl.text = vd.display;
                UpdateColors();
            }

            private void UpdateColors()
            {
                Color32 currentTextColor = m_IsMouseHovering ? hoverColor : (vd.onOff ? textColor : grey); 

                block.color = vd.onOff ? vd.color : grey;
                lbl.textColor = currentTextColor;
                val.textColor = currentTextColor;
                percent.textColor = currentTextColor;
                linkedSlice.innerColor = vd.onOff ? vd.color : grey;
                linkedSlice.outterColor = m_IsMouseHovering ? hoverColor : (vd.onOff ? vd.color : grey);
                linkedChart.Invalidate();
            }

            void toggleVisibility(UIComponent component, UIMouseEventParameter eventParam)
            {
                Debug.Log("Click");
                vd.onOff = !vd.onOff;
                UpdateColors();
            }
        }


        BreakdownElement[] breakdown;
        float[] percents;


        void Awake()
        {

            autoFitChildrenVertically = true;
            backgroundSprite = "MenuPanel";
            width = 200;
            height = 400;
            anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;


            m_AutoLayoutPadding = new RectOffset(5,5,5,0);
            m_AutoLayoutDirection = LayoutDirection.Vertical;
            
            m_AutoLayout = true;

            title = AddUIComponent<UILabel>();
            title.autoSize = false;
            title.text = "Breakdown Panel";
            title.verticalAlignment = UIVerticalAlignment.Middle;
            title.textAlignment = UIHorizontalAlignment.Center;
            title.size = new Vector2(width-10, 30);
            
            chart = AddUIComponent<UIRadialChart>();
            chart.size = new Vector2(190,190);
            
            
            chart.spriteName = "PieChartWhiteBg";

            breakdown = new BreakdownElement[Config.instance.vehicleTypes.Length];
            percents = new float[breakdown.Length];
            
            int i = 0;
            foreach(VehicleDisplay v in Config.instance.vehicleTypes) {


                chart.AddSlice();

                BreakdownElement view = AddUIComponent<BreakdownElement>();
                view.linkedChart = chart;
                view.linkedSlice = chart.GetSlice(i);
                view.SetVehicleDisplay(v);
                view.size = new Vector2(200, 15);
                breakdown[i] = view;

                view.eventMouseEnter += (UIComponent component, UIMouseEventParameter eventParam) =>
                {
                    if (eventHighlightType != null)
                        eventHighlightType(v.id);
                };
                view.eventMouseLeave += (UIComponent component, UIMouseEventParameter eventParam) =>
                {
                    if (eventHighlightType != null)
                        eventHighlightType(null);
                };
                i++;

            }


            
            total = AddUIComponent<UILabel>();
            total.autoSize = false;
            total.text = "Total: 0";
            total.textColor = new Color32(206, 248, 0, 255);
            total.textScale = 0.9f;
            total.verticalAlignment = UIVerticalAlignment.Middle;
            total.size = new Vector2(190, 35);

            LateUpdate();
            FitChildrenVertically();
        }

        String formatPercent(float f)
        {
            if (f < 0.01f)
            {
                return Math.Round(f*100.0F, 1) + "%";
            }
            else
            {
                return Math.Floor(f*100.0F) + "%";
            }
        }

        internal void SetValues(int[] counts)
        {
            if (counts.Length != breakdown.Length)
            {
                Debug.Log("SetValues: expected: " + breakdown.Length + ", got:" + counts.Length);
                return;
            }


            int totalCount = 0;
            foreach (int val in counts)
                totalCount += val;

            for (int i = 0; i < breakdown.Length; i++)
            {

                percents[i] = counts[i] / (float)totalCount;

                if (counts[i] == 0)
                {
                    breakdown[i].isVisible = false;
                }
                else
                {
                    breakdown[i].isVisible = true;
                    breakdown[i].val.text = counts[i].ToString();
                    breakdown[i].percent.text = " - " + formatPercent(percents[i]);
                }

            }

            if (totalCount == 0)
            {
                chart.enabled = false;
                total.text = "No Traffic";
            }
            else
            {
                chart.enabled = true; 
                chart.SetValues(percents);
                total.text = "Total: " + totalCount;
            }
            
            LateUpdate();

            //height = total.position.y + total.height + 5;
            FitChildren();
            

        }
    }



    class ReportUI : UIPanel
    {

        UILabel usageText;

        UIPanel helpBg;

        BreakdownPannel reportBreakDown;
        BreakdownPannel highlightBreakDown;

        public event TypeHighlightEvent eventHighlightType;

        void Awake()
        {


            //backgroundSprite = "MenuPanel";
            area = new Vector4(0, 0, 100, 100);
            anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;

            helpBg = AddUIComponent<UIPanel>();
            helpBg.backgroundSprite = "GenericPanel";
            helpBg.color = new Color32(0, 0, 120, 200);
            helpBg.area = new Vector4(10, 65, 230, 70);
            

            usageText =  AddUIComponent<UILabel>();
            usageText.autoSize = false;
            usageText.area = new Vector4(15, 70, 250, 60);
            usageText.textScale = 0.6f;
            usageText.text =
                "Left Click to see all Trafic\n" +
                "Right Click to see traffic for one direction\n" +
                "Shift + Right Click for the other direction\n" + 
                "Hover over other roads to see how much is \n" +
                "going through that location";


            reportBreakDown = AddUIComponent<BreakdownPannel>();
            reportBreakDown.interactive = true;
            reportBreakDown.title.text = "Selected";
            reportBreakDown.isVisible = false;
            reportBreakDown.relativePosition = new Vector2(10, 150);
            reportBreakDown.eventHighlightType += (String s) =>
            {
                if (eventHighlightType != null)
                    eventHighlightType(s);
            };

            highlightBreakDown = AddUIComponent<BreakdownPannel>();
            highlightBreakDown.title.text = "...highlighted";
            highlightBreakDown.isVisible = false;
            highlightBreakDown.relativePosition = new Vector2(220, 150);


        }


        public void SetSelectedData(int[] counts) {
            if(counts == null) {
                reportBreakDown.isVisible = false;
            }else {
                reportBreakDown.SetValues(counts);
                reportBreakDown.isVisible = true;
            }
        }

        public void SetHighlightData(int[] counts, int totalCount)
        {
            if (counts == null || totalCount == 0)
            {
                highlightBreakDown.isVisible = false;
            }
            else
            {
                highlightBreakDown.SetValues(counts);
                highlightBreakDown.isVisible = true;
            }
        }
    }
}
