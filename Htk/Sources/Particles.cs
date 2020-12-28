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
    public class Particles : Model
    {

        /// <summary>
        /// Loads particle file.
        /// Give name without extension ie "particleSys1".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new static Particles Load(string name)
        {
            Particles m = new Particles();
            m.LoadModel(name);
            return m;
        }

        /// <summary>
        /// Update particles.
        /// </summary>
        /// <param name="timeDelta"></param>
        public void Update(float timeDelta)
        {
            int cnt = h3d.findNodes(Node, "", (int)h3d.H3DNodeTypes.Emitter);
            for (int i = 0; i < cnt; ++i)
                h3d.updateEmitter(h3d.getNodeFindResult(i), timeDelta);

            base.Update();
        }
    }
}
