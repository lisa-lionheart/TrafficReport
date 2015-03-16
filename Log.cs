using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

namespace TrafficReport
{
    public class Log
    {
        public static void error(string message)
        {
            //DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, message);
            Console.WriteLine("ERROR: " + message);
        }


        public static void info(string message)
        {
            //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, message);
            Console.WriteLine("INFO: " + message);
        }

        public static void debug(string message)
        {
          //  DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, message);
            Console.WriteLine("DEBUG: " + message);
        }

        public static void warn(string message)
        {
            //DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, message);
            Console.WriteLine("WARN: " + message);
        }
    }
}
