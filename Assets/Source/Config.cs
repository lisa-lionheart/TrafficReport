using System;
using UnityEngine;
using TrafficReport;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace TrafficReport
{	
	[Serializable]
	public class Config {

		public Vector2 buttonPosition  = new Vector2(80, 5);
		public KeyCode keyCode1 = KeyCode.Question;
		public KeyCode keyCode2 = KeyCode.Slash;

		[XmlIgnore]
		public Rect buttonRect {
			get {
				return new Rect(buttonPosition.x,buttonPosition.y,80,80);
			}
		}
		
		public static Config Load() {
			try {
				XmlSerializer xml = new XmlSerializer (typeof(Config));
				FileStream fs = new FileStream("TrafficReport.xml", FileMode.Open, FileAccess.Read);
				Config config =  xml.Deserialize(fs) as Config;
				fs.Close();
				return config;
			}catch(Exception e) {
				return new Config();
			} 
		}
		
		public void Save() {
			try 
			{
				XmlSerializer xml = new XmlSerializer (GetType());
				FileStream fs = new FileStream ("TrafficReport.xml", FileMode.Create, FileAccess.Write);
				xml.Serialize (fs,this);
				fs.Close();
			} catch(Exception e) {
				Log.error("Error saving config" + e.ToString());

			}
		}
	}
}

