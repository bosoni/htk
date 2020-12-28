/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */

// TODO animation blending
// animweightiä pitää käyttää ja sit 2 eri animaatio stagea, eli vanha anim ja uus anim


using System;
using Horde3DNET;
using OpenTK;
using System.IO;

namespace Htk
{
    public class AnimatedModel : Model
    {
        public float AnimTime = 0;
        string[] animNames;
        int[] anims;
        int animStage = 0;
        float animWeight = 1f;

        /// <summary>
        /// Loads model and animations (*.anim).
        /// Give name without extension ie "man".
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new static AnimatedModel Load(string name)
        {
            AnimatedModel m = new AnimatedModel();
            m.LoadModel(name);
            m.AnimTime = 0;
            m.animStage = 0;
            m.animWeight = 100;

            // find *.anim and load them
            m.animNames = Directory.GetFiles(Settings.ContentDir + "/models", name + "*.anim");
            if (m.animNames.Length > 0)
            {
                m.anims = new int[m.animNames.Length];
                for (int c = 0; c < m.anims.Length; c++)
                {
                    m.anims[c] = h3d.addResource((int)h3d.H3DResTypes.Animation, m.animNames[c], 0);
                    Util.LoadResourcesFromDisk();

                    // get anim name (ie  blob_walk.anim -> walk)
                    m.animNames[c].Replace('\\', '/');
                    m.animNames[c] = m.animNames[c].Substring(m.animNames[c].LastIndexOf(name));
                    m.animNames[c] = m.animNames[c].Substring(m.animNames[c].LastIndexOf('_') + 1);
                    m.animNames[c] = m.animNames[c].Substring(0, m.animNames[c].LastIndexOf('.'));
                    h3d.setupModelAnimStage(m.Node, c, m.anims[c], 0, "", false);

                    Log.WriteLine("* Load anim [" + c + "]: " + m.animNames[c]);
                }
            }
            return m;
        }

        public void SetAnim(int animId)
        {
            animStage = animId;
        }

        public void SetAnim(string animName)
        {
            for (int q = 0; q < animNames.Length; q++)
                if (animNames[q] == animName)
                {
                    animStage = q;
                    break;
                }
        }

        void UpdateAnim()
        {
            if (AnimTime == -1) return;
            h3d.setModelAnimParams(Node, animStage, AnimTime, animWeight);
            h3d.updateModel(Node, (int)(h3d.H3DModelUpdateFlags.Animation));
        }

        public void Update(float animTime)
        {
            AnimTime = animTime;
            UpdateAnim();
            base.Update();
        }

        public void UpdateMorphs(string[] targetNames, float[] morph)
        {
            for (int q = 0; q < morph.Length; q++)
                h3d.setModelMorpher(Node, targetNames[q], morph[q]);
            h3d.updateModel(Node, (int)(h3d.H3DModelUpdateFlags.Geometry));
        }

        public override void Dispose()
        {
            AnimTime = -1;
            base.Dispose();
        }
    }
}
