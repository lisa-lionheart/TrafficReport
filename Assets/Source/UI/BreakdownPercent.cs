using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TrafficReport.Assets.Source.UI
{
    class BreakdownPercent : UIRadialChart
    {
        public void SetValues(List<KeyValuePair<string, int>> values, int ofTotal)
        {

            

            m_Slices = new List<SliceSettings>(values.Count);

            float tally = 0.0f;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].Value == 0)
                    continue;

                SliceSettings slice = new SliceSettings();
                slice.startValue = tally;
                tally += values[i].Value / (float)ofTotal;
                if (tally > 1.0f) {
                    Debug.Log("SHOULD NEVER HAPPEN");
                    tally = 1.0f;
                }
                slice.endValue = tally;

                Color32 c = Config.instance.GetTypeColor(values[i].Key);

                slice.innerColor = c;
                slice.outterColor = c;

                m_Slices.Add(slice);
            }

            Invalidate();

        }
    }
}
