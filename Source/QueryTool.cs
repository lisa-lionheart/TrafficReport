
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace TrafficReport
{

	public class QueryToolGUI : QueryToolGUIBase {

		//Implent logic that interfaces with Colossal code
        public QueryTool queryTool;

		private Camera uiCamera;
        UIView ui;
        public QueryToolGUI()
        {
            ui = UnityEngine.Object.FindObjectOfType<UIView>();
			leftHandDrive = (Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True);

			//Ripped from the bowls of HideUI
			foreach (Camera c in UnityEngine.Object.FindObjectsOfType<Camera>()) {
				if (c.name == "UIView"){
					uiCamera = c;
					break;
				}
			}
        }

        public override bool toolActive
        {
            get
            {
                return ToolsModifierControl.toolController.CurrentTool == queryTool;
            }
            set
            {
                if (value)
                {
                    ToolsModifierControl.toolController.CurrentTool = queryTool;
                } else {
					SetReport(null);
					ToolsModifierControl.SetTool<DefaultTool>();
                }
            }
        }

        public override bool guiVisible
        {
            get { 
				if(!ui.enabled){
					return false;
				}

				if(!uiCamera.enabled){
					return false;
				}

				return true;
			}
        }


        public override Vector3 GetPositionForReportEntity(int i)
        {

            if (this.currentReport.allEntities[i].type == EntityType.Vehicle)
            {
                //return Singleton<VehicleManager>.instance.GetS
                uint id = currentReport.allEntities[i].id;
                //return Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id].GetLastFramePosition();                
                return Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id].GetSmoothPosition((ushort)id);  
            }


            if (this.currentReport.allEntities[i].type == EntityType.Citizen)
            {
                uint id = currentReport.allEntities[i].id;
                return Singleton<CitizenManager>.instance.m_instances.m_buffer[id].GetSmoothPosition((ushort)id);
            }
            return new Vector3();
        }

	}

    public class QueryTool : DefaultTool
    {

        TrafficAnalyzer analyzer;

		CursorInfo loadingCursor;
		QueryToolGUI gui;
        
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
                gui = new GameObject("QueryToolGUI").AddComponent<QueryToolGUI>();
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



                        gui.activeSegmentIndicator.gameObject.SetActive(false);

						if (hoverInstance.Type == InstanceType.Vehicle) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnVehicle(hoverInstance.Vehicle);
						}

						if (hoverInstance.Type == InstanceType.NetSegment) {
							gui.SetReport(null);
							base.ToolCursor = loadingCursor;

                            gui.activeSegmentIndicator.gameObject.SetActive(true);

                            Vector3 pos = analyzer.GetSegmentMidPoint(hoverInstance.NetSegment) + Vector3.up * 20;
                            Log.debug("Segment pos: " + pos.ToString());
                            gui.activeSegmentIndicator.position = pos;
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
					gui.SetSegmentHighlight (QueryToolGUIBase.HighlightType.Segment,(uint)hoverInstance.NetSegment);
				} else if (hoverInstance.Type == InstanceType.Building) {
					gui.SetSegmentHighlight (QueryToolGUIBase.HighlightType.Building,(uint)hoverInstance.Building);
				} else if (hoverInstance.Type == InstanceType.Vehicle) {
					gui.SetSegmentHighlight (QueryToolGUIBase.HighlightType.Vehicle,(uint)hoverInstance.Vehicle);
				} else if (hoverInstance.Type == InstanceType.Citizen) {
					gui.SetSegmentHighlight (QueryToolGUIBase.HighlightType.Citizen,(uint)hoverInstance.Vehicle);
				} else {
					gui.SetSegmentHighlight(QueryToolGUIBase.HighlightType.None, 0);
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
            gui.activeSegmentIndicator.gameObject.SetActive(false);
            gui.SetReport(null);
            base.OnDisable();
        }



       
    }
}
