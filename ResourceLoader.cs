using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        public static string BaseDir {  
            get {

                Log.debug(ResourceAssembly.Location);

                try
                {
                    return Path.GetDirectoryName(ResourceAssembly.Location) + Path.PathSeparator;
                }catch(Exception e) {
                    //return InvalidProgramException.
                    return "";
                }
            }
        }


        public static byte[] loadResourceData(string name)
        {
            name = "TrafficReport.resources." + name;

            UnmanagedMemoryStream stream  = (UnmanagedMemoryStream)ResourceAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                Log.error("Could not find resource: " + name);
                return null;
            }

            BinaryReader read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
        }


        public static Texture2D loadTexture(int x, int y, string filename)
        {
            try
            {
                Texture2D texture = new Texture2D(x,y);
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                Log.error("The file could not be read:" + e.Message);                
            }

            return null;
        }

    }
}
