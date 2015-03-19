    using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TrafficReport
{
    public class ResourceLoader
    {

        public static Assembly ResourceAssembly
        {
            get {
                //return null;
                return Assembly.GetAssembly(typeof(ResourceLoader));
            }
        }

        public static byte[] loadResourceData(string name)
        {
#if BuildingModDll
            name = "TrafficReport.Assets." + name;

            UnmanagedMemoryStream stream  = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                Log.error("Could not find resource: " + name);
                return null;
            }

            Log.debug("Found resource: " + name);
            BinaryReader read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
#else
			return null;
#endif
        }

        public static string loadResourceString(string name)
        {
            
#if BuildingModDll
            name = "TrafficReport.Assets." + name;

            UnmanagedMemoryStream stream = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                Log.error("Could not find resource: " + name);
                return null;
            }

            StreamReader read = new StreamReader(stream);
            return read.ReadToEnd();
#else	
			return null;
#endif
        }




        public static Texture2D loadTexture(int x, int y, string filename)
        {
            
#if BuildingModDll
            try
            {
                Texture2D texture = new Texture2D(x,y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                Log.error("LoadTexture() The file could not be read:" + e.Message);                
            }

            return null;
#else	
			return null;
#endif
        }

    }
}
