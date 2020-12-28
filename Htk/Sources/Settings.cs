/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System.Xml;
using OpenTK;

namespace Htk
{
    public class Settings
    {
        public static DisplayDevice Device;
        public static int Width = 1024, Height = 768, Bpp = 32;
        public static int FSAA = 0, MaxAnisotropy = 0, ShadowMapSize = 512;
        public static bool FullScreen = false, VSync = false;
        public static string ContentDir = "../../../Data";
        public static bool UseNormalMaps = false;
        public static bool ShowStats = true;

        public static void ReadXML(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode resolution = doc.SelectSingleNode("//settings/resolution/text()");
            XmlNode fsaa = doc.SelectSingleNode("//settings/fsaa/text()");
            XmlNode maxAnisotropy = doc.SelectSingleNode("//settings/maxAnisotropy/text()");
            XmlNode shadowMapSize = doc.SelectSingleNode("//settings/shadowMapSize/text()");
            XmlNode fullscreen = doc.SelectSingleNode("//settings/fullscreen/text()");
            XmlNode vsync = doc.SelectSingleNode("//settings/vsync/text()");

            string[] res = resolution.Value.Split('x');
            Width = int.Parse(res[0]);
            Height = int.Parse(res[1]);
            Bpp = int.Parse(res[2]);
            FSAA = int.Parse(fsaa.Value);
            MaxAnisotropy = int.Parse(maxAnisotropy.Value);
            ShadowMapSize = int.Parse(shadowMapSize.Value);
            FullScreen = fullscreen.Value == "true";
            VSync = vsync.Value == "true";
        }
    }
}
