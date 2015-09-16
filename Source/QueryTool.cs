
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
		ReportVisualizer visualization;

        ReportButton toggleButton;
        
        protected override void Awake()
        {

            toggleButton = ReportButton.Create();
            toggleButton.eventClick += OnButtonToggled;

            analyzer = new TrafficAnalyzer(this);
            visualization = new ReportVisualizer(this);

            Debug.Log("Init vis");
            visualization.Init();

            Log.info("Load Cursor...");
			m_cursor = CursorInfo.CreateInstance<CursorInfo>();
            m_cursor.m_texture = ResourceLoader.loadTexture("Materials/Cursor.png");
            m_cursor.m_hotspot = new Vector2(0, 0);

			loadingCursor = CursorInfo.CreateInstance<CursorInfo>();
            loadingCursor.m_texture = ResourceLoader.loadTexture("Materials/Hourglass.png");

            Log.info("QueryTool awoken");
            base.Awake();
        }


        void OnButtonToggled(ColossalFramework.UI.UIComponent component, ColossalFramework.UI.UIMouseEventParameter eventParam)
        {
            toolActive = !toolActive;
        }

        protected override void OnToolGUI()
        {


            if (Input.GetKeyUp(Config.instance.keyCode))
            {
                Log.info("Toggling tool");
                toolActive = !toolActive;
            }

            if (toolActive && Input.GetKeyUp(KeyCode.Escape))
            {
                toolActive = false;
            }

            visualization.Update();
                
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



                        visualization.ActiveSegmentIndicator.gameObject.SetActive(false);

						if (hoverInstance.Type == InstanceType.Vehicle) {
							visualization.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnVehicle(hoverInstance.Vehicle);
						}

						if (hoverInstance.Type == InstanceType.NetSegment) {
							visualization.SetReport(null);
							base.ToolCursor = loadingCursor;

                            visualization.ActiveSegmentIndicator.gameObject.SetActive(true);

                            Vector3 pos = analyzer.GetSegmentMidPoint(hoverInstance.NetSegment) + Vector3.up * 20;
                            Log.debug("Segment pos: " + pos.ToString());
                            visualization.ActiveSegmentIndicator.position = pos;
							analyzer.ReportOnSegment(hoverInstance.NetSegment, NetInfo.Direction.Both);
						}

						if (hoverInstance.Type == InstanceType.Building) {
							visualization.SetReport(null);
							base.ToolCursor = loadingCursor;
							analyzer.ReportOnBuilding(hoverInstance.Building);
						}


						if (hoverInstance.Type == InstanceType.CitizenInstance) {
							visualization.SetReport(null);
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
							visualization.SetReport(null);
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
					visualization.SetSegmentHighlight (ReportVisualizer.HighlightType.Segment,(uint)hoverInstance.NetSegment);
				} else if (hoverInstance.Type == InstanceType.Building) {
					visualization.SetSegmentHighlight (ReportVisualizer.HighlightType.Building,(uint)hoverInstance.Building);
				} else if (hoverInstance.Type == InstanceType.Vehicle) {
					visualization.SetSegmentHighlight (ReportVisualizer.HighlightType.Vehicle,(uint)hoverInstance.Vehicle);
				} else if (hoverInstance.Type == InstanceType.Citizen) {
					visualization.SetSegmentHighlight (ReportVisualizer.HighlightType.Citizen,(uint)hoverInstance.Vehicle);
				} else {
					visualization.SetSegmentHighlight(ReportVisualizer.HighlightType.None, 0);
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
			visualization.SetReport (report);
            			
			#if DEBUG
			report.Save ("report.xml");
			#endif
        }

        public bool toolActive
        {
            get
            {   
                return ToolsModifierControl.toolController.CurrentTool == this;
            }
            set
            {
                ToolsModifierControl.toolController.CurrentTool = value ? this : ToolsModifierControl.toolController.Tools[0];
            }
        }

        
        protected override void OnDisable()
        {

            toggleButton.ToggleState = false;
            visualization.ActiveSegmentIndicator.gameObject.SetActive(false);
            visualization.SetReport(null);
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            InfoManager infoManger = GameObject.FindObjectOfType<InfoManager>();

            FieldInfo mode = typeof(InfoManager).GetField("m_currentMode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
              
            toggleButton.ToggleState = true;

            UIView.library.Hide("CityInfoPanel");

            Debug.Log("Type:" + mode.GetType().ToString());
            Debug.Log("Mode is:" + mode.GetValue(infoManger).ToString());
            Debug.Log("Changing info mode");
            mode.SetValue(infoManger, InfoManager.InfoMode.Traffic);
            Debug.Log("Mode set to:" + infoManger.NextMode);
            infoManger.UpdateInfoMode();
                        
            base.OnEnable();
        }

       
    }
}
