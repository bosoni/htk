/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using Horde3DNET;
using OpenTK;
using System.IO;

namespace Htk
{
    public class Model : Movable
    {
        /// <summary>
        /// Loads model (no animations, use AnimatedModel instead). 
        /// Give name without extension ie "platform".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Model Load(string name)
        {
            Model m = new Model();
            return m.LoadModel(name);
        }

        protected Model LoadModel(string name)
        {
            int resource = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, name + ".scene.xml", 0);
            Util.LoadResourcesFromDisk();
            Node = h3d.addNodes(h3d.H3DRootNode, resource);
            return this;
        }

    }
}
