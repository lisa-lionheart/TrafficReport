using System;
using UnityEngine;
using TrafficReport;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace TrafficReport
{	
	
	[Serializable]
	public struct VehicleDisplay {
		public string id;
		public string display;
		public bool onOff;
	}

	[Serializable]
	public class Config {

		public Vector2 buttonPosition  = new Vector2(80, 5);
		public KeyCode keyCode = KeyCode.Slash;

		public VehicleDisplay[] vehicleTypes = {
			new VehicleDisplay { id =  "citizen", display = "Pedestrian", onOff=true },
			
			new VehicleDisplay { id =  "Residential/ResidentialLow", display = "Car", onOff=true },
			
			new VehicleDisplay { id =  "Industrial/IndustrialGeneric", display = "Cargo truck", onOff=true },
			new VehicleDisplay { id =  "Industrial/IndustrialOil", display = "Oil Tanker", onOff=true },
			new VehicleDisplay { id =  "Industrial/IndustrialOre", display = "Ore Truck", onOff=true },
			new VehicleDisplay { id =  "Industrial/IndustrialForestry", display = "Log Truck", onOff=true },
			new VehicleDisplay { id =  "Industrial/IndustrialFarming", display = "Tractor", onOff=true },
			
			new VehicleDisplay { id =  "HealthCare/None", display = "Ambulance", onOff=true },
			new VehicleDisplay { id =  "Garbage/None", display = "Garbage Truck", onOff=true },
			new VehicleDisplay { id =  "PoliceDepartment/None", display = "Police Car", onOff=true },
			new VehicleDisplay { id =  "FireDepartment/None", display = "Fire truck", onOff=true },
			
			
			new VehicleDisplay { id =  "PublicTransport/PublicTransportBus", display = "Bus", onOff=true },
			
		};


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

