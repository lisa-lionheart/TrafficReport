
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TrafficReport.Assets.Source.UI;
using TrafficReport.Util;
using UnityEngine;



namespace TrafficReport
{


    public class QueryTool : DefaultTool
    {

        TrafficAnalyzer analyzer;

		CursorInfo loadingCursor;
		ReportVisualizer gui;
        
        protected override void Awake()
        {   
            try
            {
                analyzer = new TrafficAnalyzer(this);

                Log.info("Load Cursor...");
				m_cursor = CursorInfo.CreateInstance<CursorInfo>();
                m_cursor.m_texture = ResourceLoader.loadTexture("Materials/Cursor.png");
                m_cursor.m_hotspot = new Vector2(0, 0);

				loadingCursor = CursorInfo.CreateInstance<CursorInfo>();
                loadingCursor.m_texture = ResourceLoader.loadTexture("Materials/Hourglass.png");

                Log.info("Create GUI...");

                //gui = new GameObject("ReportVisualizer").AddComponent<ReportVisualizer>();

                gui = gameObject.AddComponent<ReportVisualizer>();
                gui.queryTool = this;

                Log.info("QueryTool awoken");
            }
            catch (Exception e)
            {
                Log.error(e.ToString());
            }
            base.Awake();
        }


        protected override void OnToolGUI()
        {
			if (this.m_toolController.IsInsideUI)
            {
                return;
            }
			
			InstanceID hoverInstance = this.m_hoverInstance;			
            Event current = Event.current; 

            if (current.type == EventType.MouseDown) {
				if (current.button == 0) {

					//Log.info(m_mousePosition.ToString());

					try {
						Log.debug("You clicked on " + hoverInstance.ToString());
						Log.debug(hoverInstance.Type.ToString());



                        gui.ActiveSegmentIndicator.gameObject.SetActive(false);

						if (hoverInstance.Type == InstanceType.Vehicle) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnVehicle(hoverInstance.Vehicle);
						}

						if (hoverInstance.Type == InstanceType.NetSegment) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;

                            gui.ActiveSegmentIndicator.gameObject.SetActive(true);

                            Vector3 pos = analyzer.GetSegmentMidPoint(hoverInstance.NetSegment) + Vector3.up * 20;
                            Log.debug("Segment pos: " + pos.ToString());
                            gui.ActiveSegmentIndicator.position = pos;
							analyzer.ReportOnSegment(hoverInstance.NetSegment, NetInfo.Direction.Both);
						}

						if (hoverInstance.Type == InstanceType.Building) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnBuilding(hoverInstance.Building);
						}


						if (hoverInstance.Type == InstanceType.CitizenInstance) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnCitizen(hoverInstance.CitizenInstance);
						}

					} catch (Exception e) {
						Log.error(e.ToString());
						Log.error(e.StackTrace);
					}						
				} else if (current.button == 1) {
					try {
						Log.debug("You clicked on " + hoverInstance.ToString());
						Log.debug(hoverInstance.Type.ToString());

						if (hoverInstance.Type == InstanceType.NetSegment) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;

                            if (current.modifiers == EventModifiers.Shift)
                            {
                                analyzer.ReportOnSegment(hoverInstance.NetSegment, NetInfo.Direction.Forward);
                            }
                            else
                            {
                                analyzer.ReportOnSegment(hoverInstance.NetSegment, NetInfo.Direction.Backward);
                            }
						}
					} catch (Exception e) {
						Log.error(e.ToString());
						Log.error(e.StackTrace);
					}
				}
			} else {
				
				if (hoverInstance.Type == InstanceType.NetSegment) {
					gui.SetSegmentHighlight (ReportVisualizer.HighlightType.Segment,(uint)hoverInstance.NetSegment);
				} else if (hoverInstance.Type == InstanceType.Building) {
					gui.SetSegmentHighlight (ReportVisualizer.HighlightType.Building,(uint)hoverInstance.Building);
				} else if (hoverInstance.Type == InstanceType.Vehicle) {
					gui.SetSegmentHighlight (ReportVisualizer.HighlightType.Vehicle,(uint)hoverInstance.Vehicle);
				} else if (hoverInstance.Type == InstanceType.Citizen) {
					gui.SetSegmentHighlight (ReportVisualizer.HighlightType.Citizen,(uint)hoverInstance.Vehicle);
				} else {
					gui.SetSegmentHighlight(ReportVisualizer.HighlightType.None, 0);
				}
			}
            base.OnToolGUI();
        }

        
        public override NetSegment.Flags GetSegmentIgnoreFlags()
        {
            return NetSegment.Flags.None;
        }

        internal void OnGotReport(Report report)
        {

            base.ToolCursor = m_cursor;
			gui.SetReport (report);

			
			#if DEBUG
			report.Save ("report.xml");
			#endif
        }


        
        protected override void OnDisable()
        {
            gui.ActiveSegmentIndicator.gameObject.SetActive(false);
            gui.SetReport(null);
            base.OnDisable();
        }



       
    }
}
