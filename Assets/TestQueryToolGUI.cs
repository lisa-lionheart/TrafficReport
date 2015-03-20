using UnityEngine;
using System.Collections;

using TrafficReport;

public class TestQueryToolGUI : QueryToolGUIBase {

	// Use this for initialization
	void Start () {

		Report r = Report.Load ("Assets/report.xml");
	
		SetReport (r);
		SetSegmentHighlight (23080);
	}

}
