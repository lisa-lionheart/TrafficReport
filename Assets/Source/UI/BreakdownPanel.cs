using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficReport.Util;
using UnityEngine;

namespace TrafficReport.Assets.Source.UI
{

    class BreakdownPanel : UIPanel
    {

        internal UILabel title;
        UILabel total;
        BreakdownPercent chart;
        internal event TypeHighlightEvent eventHighlightType;

        BreakdownElement[] breakdown;
        Dictionary<string,int> values;


        public override void Awake()
        {

            autoFitChildrenVertically = true;
            backgroundSprite = "MenuPanel";
            width = 200;
            height = 400;
            anchor = UIAnchorStyle.Top | UIAnchorStyle.Left;

            m_AutoLayoutPadding = new RectOffset(5, 5, 5, 0);
            m_AutoLayoutDirection = LayoutDirection.Vertical;

            m_AutoLayout = true;

            title = AddUIComponent<UILabel>();
            title.autoSize = false;
            title.textScale = 1.1f;
            title.verticalAlignment = UIVerticalAlignment.Middle;
            title.textAlignment = UIHorizontalAlignment.Center;
            title.size = new Vector2(width - 10, 40);

            chart = AddUIComponent<BreakdownPercent>();
            chart.size = new Vector2(190, 190);


            chart.spriteName = "PieChartWhiteBg";

            breakdown = new BreakdownElement[20];
            
            for(int i = 0; i < breakdown.Length; i++)
            {
                BreakdownElement view = AddUIComponent<BreakdownElement>();
                view.tooltip = "Click to toggle display";
                view.size = new Vector2(200, 15);

                view.eventMouseEnter += (UIComponent component, UIMouseEventParameter eventParam) =>
                {
                    if (eventHighlightType != null)
                        eventHighlightType(view.type);
                };
                view.eventMouseLeave += (UIComponent component, UIMouseEventParameter eventParam) =>
                {
                    if (eventHighlightType != null)
                        eventHighlightType(null);
                };

                breakdown[i] = view;
            }

            total = AddUIComponent<UILabel>();
            total.autoSize = false;
            total.text = "Total: 0";
            total.textColor = new Color32(206, 248, 0, 255);
            total.textScale = 0.9f;
            total.verticalAlignment = UIVerticalAlignment.Middle;
            total.size = new Vector2(190, 35);

            Invalidate();
        }

        internal void SetValues(Dictionary<string,int> counts)
        {            
            values = counts;
            Invalidate();
        }

        public int compare(KeyValuePair<String, int> a, KeyValuePair<String, int> b)
        {
            return b.Value - a.Value;
        }

        public override void Update()
        {
            if (!m_IsComponentInvalidated)
                return;

            if (values == null)
                return;

            

            int totalCount = 0;
            int hiddenCount = 0;

            List<KeyValuePair<string, int>> listValues = new List<KeyValuePair<string, int>>();
            List<KeyValuePair<string, int>> chartValues = new List<KeyValuePair<string, int>>();

            foreach(KeyValuePair<String,int> kv in values)
            {
                if (Config.instance.IsTypeVisible(kv.Key))
                {
                    totalCount += kv.Value;
                    if (kv.Value > 0)
                    {
                        chartValues.Add(kv);
                    }
                }
                else
                {
                    hiddenCount += kv.Value;
                }

                if (kv.Value > 0)
                {
                    listValues.Add(kv);
                }
                
            }

            listValues.Sort(compare);
            chartValues.Sort(compare);
            chartValues.Reverse();


            for (int i = 0; i < breakdown.Length; i++)
            {
                if (i > listValues.Count-1)
                {
                    breakdown[i].isVisible = false;
                }
                else
                {
                    breakdown[i].isVisible = true;
                    breakdown[i].SetVehicleDisplay(listValues[i].Key, listValues[i].Value, totalCount);                   
                }

            }

            if (totalCount == 0)
            {
                chart.enabled = false;

                if (hiddenCount > 0)
                {
                    total.text = "Hidden: " + hiddenCount;
                }
                else
                {
                    total.text = "No Traffic";
                }

            }
            else
            {
                chart.enabled = true;
                chart.SetValues(chartValues, totalCount);
                total.text = "Total: " + totalCount;
                if (hiddenCount > 0)
                    total.text += " Hidden: " + hiddenCount;
            }

            base.Update();
        }
    }


}
