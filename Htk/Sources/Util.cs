/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using System.IO;
using Horde3DNET;
using SharpFS;

namespace Htk
{
    public class Util
    {
        /// <summary>
        /// Returns filename with correct directory (depends on filename's extension).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static string RealDir(string fileName)
        {
            string dir = "";
            fileName = fileName.ToLower();
            if (fileName.Contains(".jpg") || fileName.Contains(".png") || fileName.Contains(".tga") || fileName.Contains(".dds"))
                dir = "textures/";
            if (fileName.Contains(".geo") || fileName.Contains(".scene.xml") || fileName.Contains(".anim"))
                dir = "models/";
            if (fileName.Contains(".material.xml"))
            {
                dir = "models/";
                if (Settings.UseNormalMaps)
                {
                    string newfile = fileName.Replace('\\', '/');
                    newfile = newfile.Substring(newfile.LastIndexOf('/') + 1);
                    newfile = newfile.Substring(0, newfile.IndexOf('.')) + "_nm.material.xml";
                    if (File.Exists(Settings.ContentDir + "/" + dir + newfile))
                        fileName = newfile;
                }
            }
            if (fileName.Contains(".shader") || fileName.Contains(".glsl"))
                dir = "shaders/";
            if (fileName.Contains(".pipeline.xml"))
                dir = "pipelines/";
            if (fileName.Contains(".particle.xml"))
                dir = "particles/";

            fileName = fileName.Replace('\\', '/');
            dir += fileName.Substring(fileName.LastIndexOf('/') + 1);

            return dir;
        }

        /// <summary>
        /// Load resources from disk.
        /// </summary>
        /// <returns></returns>
        public static bool LoadResourcesFromDisk()
        {
            return LoadResourcesFromDisk(Settings.ContentDir, "");
        }

        /// <summary>
        /// Loads resources.
        /// Can read from zip file too.
        /// </summary>
        /// <param name="contentDir"></param>
        /// <param name="zipFile"></param>
        /// <returns></returns>
        public static bool LoadResourcesFromDisk(string contentDir, string zipFile = "")
        {
            VirtualFS vfs = null;
            VirtualFile vfile = null;
            bool result = true;
            byte[] dataBuf;
            contentDir += "/";

            if (zipFile != null && zipFile != "")
            {
                //First, create a new virtual filesystem
                //we'll mount our current directory to the root, with read and write access
                vfs = new VirtualFS(Directory.GetCurrentDirectory(), true, typeof(DirectoryArchiver));
                vfs.RegisterArchiver(typeof(ZipArchiver));
                vfs.Mount("zip", zipFile);
            }

            // Get the first resource that needs to be loaded
            int res = h3d.queryUnloadedResource(0);
            while (res != 0)
            {
                string file = contentDir + RealDir(h3d.getResName(res));
                bool fileOK = false;

                if (vfs != null)
                {
                    foreach (string f in vfs.ListFiles("zip"))
                        if (f == file)
                        {
                            vfile = vfs.OpenFile("zip/" + file, FileMode.Open, FileAccess.Read);
                            fileOK = true;
                            break;
                        }
                }
                else
                    if (File.Exists(file)) fileOK = true;

                if (fileOK)
                {
                    // read resource file to memory
                    if (vfile != null)
                    {
                        dataBuf = new byte[vfile.Size];
                        vfile.Stream.Read(dataBuf, 0, (int)vfile.Size);
                    }
                    else
                        dataBuf = File.ReadAllBytes(file);

                    if (dataBuf != null && dataBuf.Length != 0)
                    {
                        // Send resource data to engine
                        result &= h3d.loadResource(res, dataBuf, dataBuf.Length);
                    }
                }
                else // Resource file not found
                {
                    dataBuf = new byte[1];
                    h3d.loadResource(res, dataBuf, 1);
                    Log.WriteLine("File not found: " + file);
                    result = false;
                }

                // Get next unloaded resource
                res = h3d.queryUnloadedResource(0);
            }
            if (vfs != null) vfs.Unmount("/zip");

            return result;
        }
    }
}
