using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Threading;
using System.IO;
using System.Runtime.CompilerServices;

namespace TrafficReport
{
    public class Log
    {
        private static StreamWriter _logFile;
        public static StreamWriter logFile {
            get {

                if(_logFile == null) {
                    _logFile = new StreamWriter(new FileStream("TrafficReport.log", FileMode.Append, FileAccess.Write, FileShare.Read));
                    _logFile.WriteLine("Loggin started at" + DateTime.Now.ToString());
                }
                return _logFile;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void error(string message)
        {
            logFile.WriteLine("ERROR: " + message);
            logFile.Flush();
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void info(string message)
        {
            logFile.WriteLine("INFO: " + message);
            logFile.Flush();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void debug(string message)
        {
            logFile.WriteLine("DEBUG: " + message);
            logFile.Flush();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void warn(string message)
        {
            logFile.WriteLine("WARN: " + message);
            logFile.Flush();
        }
    }
}
