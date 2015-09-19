using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrafficReport.Util;
using UnityEngine;

namespace TrafficReport.Assets.Source.UI
{

    

    delegate void TypeHighlightEvent(String type);

   

    class ReportUI : UIPanel
    {

        static Color32 grey = new Color32(128, 128, 128, 128);
        static Color32 textColor = new Color32(206, 248, 0, 255);
        static Color32 hoverColor = new Color32(0, 206, 248, 255);

        UILabel usageText;

        UIPanel helpBg;

        BreakdownPanel reportBreakDown;
        BreakdownPanel highlightBreakDown;

        public event TypeHighlightEvent eventHighlightType;

        public static ReportUI Create()
        {
            GameObject go = new GameObject("TrafficReportUI");
            go.transform.localPosition = Vector3.zero;
            ReportUI reportUI = go.AddComponent<ReportUI>();
            UIView.GetAView().AttachUIComponent(go);
            return reportUI;
        }

        public override void Awake()
        {
            size = new Vector2(5, 5);
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
                "going through that location.\n";                


            reportBreakDown = AddUIComponent<BreakdownPanel>();
            reportBreakDown.title.text = "Selected";
            reportBreakDown.title.tooltip = "A breakdown of all traffic going through the selected road segement";
            reportBreakDown.isVisible = false;
            reportBreakDown.relativePosition = new Vector2(10, 150);
            reportBreakDown.eventHighlightType += (String s) =>
            {
                if (eventHighlightType != null)
                    eventHighlightType(s);
            };

            highlightBreakDown = AddUIComponent<BreakdownPanel>();
            highlightBreakDown.title.text = "...highlighted";
            highlightBreakDown.isVisible = false;
            highlightBreakDown.relativePosition = new Vector2(220, 150);

            base.Awake();
        }


        public void SetSelectedData(Dictionary<string,int> counts) {
            if(counts == null) {
                reportBreakDown.isVisible = false;
            }else {
                reportBreakDown.SetValues(counts);
                reportBreakDown.isVisible = true;
            }
        }

        public void SetHighlightData(Dictionary<String,int> counts, int totalCount)
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
