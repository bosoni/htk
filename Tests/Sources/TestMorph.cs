/*
 * Htk example (c) mjt, 2011-2014
 * 
 * 
 */
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using OpenTK.Input;
using Htk;

namespace Test
{
    public class TestMorph : BaseGame
    {
        AnimatedModel face;
        Light light = new Light();
        const float SPD = 0.01f;
        string[] targetNames = { "yell", "happy", "eyesclosed", "eyesangry" };
        float[] morph = new float[4];
        float animTime = 0;

        public override void Init()
        {
            Camera.SetSkyBox("skybox");
            face = AnimatedModel.Load("t2");
            face.Rotation.Y = 10;
            face.Scale = new Vector3(0.01f, 0.01f, 0.01f);
            face.Update();

            Util.LoadResourcesFromDisk();


            // add light
            light.Create(h3d.H3DRootNode, new Vector3(0, 20, 10), new Vector3(-80, 0, 0));

            Camera.Position.Z = 5;

            System.Console.WriteLine("KEYS:\n Q,W,E,R   A,S,D,F,   space, up-arrow, right-control\n");


            base.Init();
        }


        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(float time)
        {
            base.Update(time);
        }

        public override void Render(float time)
        {
            Camera.Update();

            if (Keyboard[Key.Q]) if (morph[0] < 1) morph[0] += SPD;
            if (Keyboard[Key.W]) if (morph[1] < 1) morph[1] += SPD;
            if (Keyboard[Key.E]) if (morph[2] < 1) morph[2] += SPD;
            if (Keyboard[Key.R]) if (morph[3] < 1) morph[3] += SPD;

            if (Keyboard[Key.A]) if (morph[0] > 0) morph[0] -= SPD;
            if (Keyboard[Key.S]) if (morph[1] > 0) morph[1] -= SPD;
            if (Keyboard[Key.D]) if (morph[2] > 0) morph[2] -= SPD;
            if (Keyboard[Key.F]) if (morph[3] > 0) morph[3] -= SPD;


            if (Keyboard[Key.Up])
            {
                face.SetAnim(0);
            }
            else if (Keyboard[Key.Space])
            {
                face.SetAnim(1);
            }
            else if (Keyboard[Key.ControlRight])
            {
                face.SetAnim(2);
            }
            else
                animTime = 0;

            animTime += time * 20;

            face.UpdateMorphs(targetNames, morph);
            face.Update(animTime);

            h3d.render(Camera.Node);
        }
    }
}
