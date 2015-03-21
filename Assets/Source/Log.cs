using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

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
        public static void Write(string level, string message)
        {

#if BuildingModDll
            logFile.WriteLine(level + ": " + message);
            logFile.Flush();
#else
            Debug.Log(message);
#endif
        }

        
        public static void error(string message)
        {
            Write("ERROR", message);
        }
        
        public static void info(string message)
        {
            Write("INFO", message);
        }

        public static void debug(string message)
        {
            Write("DEBUG", message);
        }

        public static void warn(string message)
        {
            Write("WARN", message);
        }
    }
}
