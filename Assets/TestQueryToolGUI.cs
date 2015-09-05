using UnityEngine;
using System.Collections;

using TrafficReport;

public class TestQueryToolGUI : QueryToolGUIBase {


	bool _toolActive;

	public override bool toolActive {
		get {
			return _toolActive;
		}
		set {
			_toolActive = value;
		}
	}


	// Use this for initialization
	void Start () {

		Report r = Report.Load ("Assets/report.xml");

        r.Save("Report2.xml");
	
		SetReport (r);
		SetSegmentHighlight (QueryToolGUIBase.HighlightType.Segment, 23080);

	}



}
