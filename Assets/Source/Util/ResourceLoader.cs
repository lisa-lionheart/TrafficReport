using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TrafficReport.Util
{
    public class ResourceLoader
    {
        private static string m_savedModPath = null;

        public static Assembly ResourceAssembly
        {
            get {
                //return null;
                return Assembly.GetAssembly(typeof(ResourceLoader));
            }
        }

        public static string GetModPath()
        {
            if (m_savedModPath == null)
            {
                PluginManager pluginManager = PluginManager.instance;

                var pluginInfo = PluginManager.instance.FindPluginInfo(ResourceAssembly);
                m_savedModPath = pluginInfo.modPath;
            }

            return m_savedModPath;
        }

        public static byte[] loadResourceData(string name)
        {
#if BuildingModDll
            name = "TrafficReport.Assets." + name.Replace("/",".");

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
            string resolvedName = "Assets/" + name;
            Log.info("Loading: " + resolvedName);
            return File.ReadAllBytes(resolvedName);
#endif
        }

        public static string loadResourceString(string name)
        {
           return System.Text.Encoding.UTF8.GetString(loadResourceData(name));
        }

        public static Shader loadShader()
        {
            try
            {
                string absUri = "file:///" + GetModPath().Replace("\\", "/") + "/vertexlit";
                WWW www = new WWW(absUri);
                AssetBundle bundle = www.assetBundle;

                Log.debug("bundle loading " + ((bundle == null) ? "failed " + www.error : "succeeded"));
                String[] allAssets = bundle.GetAllAssetNames();
                foreach (String asset in allAssets)
                {
                    Log.debug("asset is: " + asset);
                }
                Shader shader = bundle.LoadAsset("VertexLit.shader") as Shader;

                if (shader == null)
                {
                    throw new Exception();
                }
                bundle.Unload(false);
                return shader;
            }
            catch (Exception e)
            {
                Debug.Log("Exception trying to load bundle file!" + e.ToString());
                return null;
            }
        }

        public static Texture2D loadTexture(string filename)
        {
            try
            {
                Texture2D texture = new Texture2D(100,100, TextureFormat.ARGB32, true); //These values make no difference
                texture.filterMode = FilterMode.Trilinear;
                texture.LoadImage(loadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                Log.error("LoadTexture() The file could not be read:" + e.Message);                
            }

            return null;
        }

    }
}
