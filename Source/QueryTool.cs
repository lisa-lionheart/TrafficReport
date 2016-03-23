
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

		PathController paths;
        ReportUI ui;
        ReportButton toggleButton;

        Report currentReport;

        protected override void Awake()
        {
            Log.debug("Create Button");
            toggleButton = ReportButton.Create();
            toggleButton.eventClick += OnButtonToggled;

            Log.debug("Create UI...");
            ui = ReportUI.Create();
            ui.enabled = false;
            ui.absolutePosition = new Vector2(-1000, 0);
            ui.eventHighlightType += (String s) => { SetHighlight(s); };

            Log.debug("Create Analyzer...");
            analyzer = new TrafficAnalyzer();
            analyzer.OnReport += OnGotReport;

            Log.debug("Create Path Controller...");
            paths = new PathController(this);

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

        void HandleLeftClick()
        {
            try
            {
                Log.debug("You clicked on " + m_hoverInstance.ToString());
                Log.debug(m_hoverInstance.Type.ToString());

                paths.ActiveSegmentIndicator.gameObject.SetActive(false);


                paths.SetReport(null);
                currentReport = null;

                if (m_hoverInstance.Type == InstanceType.Vehicle)
                {
                    base.ToolCursor = loadingCursor;
                    analyzer.ReportOnVehicle(m_hoverInstance.Vehicle);
                }

                if (m_hoverInstance.Type == InstanceType.NetSegment)
                {
                    base.ToolCursor = loadingCursor;

                    paths.ActiveSegmentIndicator.gameObject.SetActive(true);

                    Vector3 pos = analyzer.GetSegmentMidPoint(m_hoverInstance.NetSegment) + Vector3.up * 20;
                    Log.debug("Segment pos: " + pos.ToString());
                    paths.ActiveSegmentIndicator.position = pos;
                    analyzer.ReportOnSegment(m_hoverInstance.NetSegment, NetInfo.Direction.Both);
                }

                if (m_hoverInstance.Type == InstanceType.Building)
                {
                    base.ToolCursor = loadingCursor;
                    analyzer.ReportOnBuilding(m_hoverInstance.Building);
                }


                if (m_hoverInstance.Type == InstanceType.CitizenInstance)
                {
                    base.ToolCursor = loadingCursor;
                    analyzer.ReportOnCitizen(m_hoverInstance.CitizenInstance);
                }

            }
            catch (Exception e)
            {
                Log.error(e.ToString());
                Log.error(e.StackTrace);
            }
        }

        void HandleRightClick()
        {
            try
            {
                Log.debug("You clicked on " + m_hoverInstance.ToString());
                Log.debug(m_hoverInstance.Type.ToString());

                if (m_hoverInstance.Type == InstanceType.NetSegment)
                {
                    paths.SetReport(null);
                    base.ToolCursor = loadingCursor;

                    if (Event.current.modifiers == EventModifiers.Shift)
                    {
                        analyzer.ReportOnSegment(m_hoverInstance.NetSegment, NetInfo.Direction.Forward);
                    }
                    else
                    {
                        analyzer.ReportOnSegment(m_hoverInstance.NetSegment, NetInfo.Direction.Backward);
                    }
                }
            }
            catch (Exception e)
            {
                Log.error(e.ToString());
                Log.error(e.StackTrace);
            }
        }

        void HandleHover()
        {
            if (m_hoverInstance != null) { 
                switch(m_hoverInstance.Type){
                    case InstanceType.NetSegment:
                        SetHighlight(HighlightType.Segment, (uint)m_hoverInstance.NetSegment);
                        return;
                    case InstanceType.Building:
                        SetHighlight(HighlightType.Building, (uint)m_hoverInstance.Building);
                        return;
                    case InstanceType.Vehicle:                         
                        SetHighlight(HighlightType.Vehicle, (uint)m_hoverInstance.Vehicle);
                        return;
                    case InstanceType.Citizen:
                        SetHighlight(HighlightType.Citizen, (uint)m_hoverInstance.Vehicle);
                        return;
                }                
            }
            SetHighlight(HighlightType.None, 0);
        }



        protected override void OnToolGUI(Event e)
        {
            if (Input.GetKeyUp(Config.instance.keyCode))
            {
                Log.info("Toggling tool");
                toolActive = !toolActive;
            }

            if (toolActive && Input.GetKeyUp(KeyCode.Escape))
            {
                toolActive = false;
                return;
            }


            if (paths != null)
            {
                paths.Update();
            }
            else
            {
                Log.debug("paths was NULL");
                base.OnToolGUI(e);
                return;
            }

            if (ToolsModifierControl.toolController.IsInsideUI)
                return;

            if (Event.current.type == EventType.MouseDown)
            {

                if (m_hoverInstance == null)
                {
                    currentReport = null;
                    ui.SetSelectedData(null);
                    ui.SetHighlightData(null,0);
                    paths.SetReport(null);
                }
                
                if (Event.current.button == 0)
                {
                    HandleLeftClick();
                }
                else if (Event.current.button == 1)
                {
                    HandleRightClick();
                }
            }
            else
            {
                HandleHover();
            }

        }

        public void SetHighlight(HighlightType type, uint thingId) {

            if (currentReport != null)
            {
                paths.SetHighlight(type, thingId);
                if (type == HighlightType.None)
                {
                    ui.SetHighlightData(null, 0);
                }
                else
                {
                    ui.SetHighlightData(currentReport.CountEntiesTypes(type, thingId), currentReport.allEntities.Length);
                }
            }
        }

        public void SetHighlight(String serviceType)
        {
            if (currentReport != null)
            {
                paths.SetHighlight(serviceType);
                ui.SetHighlightData(null, 0);
            }
        }
        
        public override NetSegment.Flags GetSegmentIgnoreFlags()
        {
            return NetSegment.Flags.None;
        }

        internal void OnGotReport(Report report)
        {
            currentReport = report;
            base.ToolCursor = m_cursor;
            paths.SetReport(report);

            if (report == null)
            {
                ui.SetSelectedData(null);
                ui.SetHighlightData(null, 0);
            }
            else
            {
                ui.SetSelectedData(report.CountEntiesTypes());

#if DEBUG
                report.Save("report.xml");
#endif
            }

            


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
            if (paths != null)
            {
                paths.ActiveSegmentIndicator.gameObject.SetActive(false);
                paths.SetReport(null);
            }

            if (ui != null)
            {
                ui.absolutePosition = new Vector2(-1000, 0);
                ui.enabled = false;
                ui.SetHighlightData(null, 0);
                ui.SetSelectedData(null);
            }

            base.OnDisable();
        }

        protected override void OnEnable()
        {
            InfoManager infoManger = GameObject.FindObjectOfType<InfoManager>();

            FieldInfo mode = typeof(InfoManager).GetField("m_currentMode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            toggleButton.ToggleState = true;
            ui.absolutePosition = new Vector2(0, 0);
            ui.enabled = true;

            UIView.library.Hide("CityInfoPanel");

            Log.debug("Type:" + mode.GetType().ToString());
            Log.debug("Mode is:" + mode.GetValue(infoManger).ToString());
            Log.debug("Changing info mode");
            mode.SetValue(infoManger, InfoManager.InfoMode.Traffic);
            Log.debug("Mode set to:" + infoManger.NextMode);
            infoManger.UpdateInfoMode();

            base.OnEnable();
        }

       
    }
}
