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
    public class Test_Walking : BaseGame
    {
        const int CHARACTERS = 10;

        AnimatedModel player;
        AnimatedModel[] characters = new AnimatedModel[CHARACTERS];

        Light light1 = new Light(), light2 = new Light();
        float animTime;

        public override void Init()
        {
            base.Init();

            Camera.SetSkyBox("skybox");
            Camera.Position.Y = 2;
            Camera.Position.Z = 10;

            for (int q = 0; q < CHARACTERS; q++)
            {
                characters[q] = AnimatedModel.Load("man");
                characters[q].Position = new Vector3(q, 0, 0);
                characters[q].Update();


                // TODO FIX: tämä ei tee mitään.
                float c = (float)q * 0.05f;
                float[] data = { c, c, 0, 1 };
                h3d.setNodeUniforms(characters[q].Node, data, 4);
            }
            player = characters[0];

            Model floor = Model.Load("platform");

            // Add light source to player
            light1.Create(player.Node, new Vector3(0, 2f, 0), new Vector3(-10, 180, 0));

            // add second light
            light2.Create(h3d.H3DRootNode, new Vector3(0, 10, 0), new Vector3(-90, 0, 0));

            System.Console.WriteLine("Use arrowkeys to move character.\n");
        }

        public override void Dispose()
        {
            player.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// very simple collision detection
        /// </summary>
        /// <param name="spd"></param>
        /// <returns></returns>
        bool CanMove(float spd)
        {
            Vector3 dir = player.GetDirection(spd * 10);
            int i = h3d.castRay(h3d.H3DRootNode, player.Position.X, player.Position.Y + 0.1f, player.Position.Z, dir.X, dir.Y, dir.Z, 0);
            if (i > 0)
            {
                int node = 0;
                float dist = 0;
                float[] intersectionPoint = new float[3];
                for (int q = 0; q < i; q++)
                    if (h3d.getCastRayResult(q, out node, out dist, intersectionPoint))
                    {
                        if (node != player.Node) return false;
                    }
            }
            return true;
        }

        public override void Update(float time)
        {
            float spd = (float)time * 10;
            if (Keyboard[Key.ShiftLeft] || Keyboard[Key.ShiftRight]) spd *= 2;
            if (Keyboard[Key.W]) Camera.Move(spd);
            if (Keyboard[Key.S]) Camera.Move(-spd);
            if (Keyboard[Key.A]) Camera.Strafe(-spd);
            if (Keyboard[Key.D]) Camera.Strafe(spd);
            if (Mouse[MouseButton.Left])
            {
                Camera.Rotation.Y -= Mouse.X - oldMouseX;
                Camera.Rotation.X -= Mouse.Y - oldMouseY;
            }
            Camera.Position.Y = 2;

            if (Keyboard[Key.Up])
            {
                if (CanMove(-spd * 0.2f))
                {
                    player.Move(-spd * 0.2f);
                    animTime += 0.7f;
                }
            }
            if (Keyboard[Key.Down])
            {
                if (CanMove(spd * 0.2f))
                {
                    player.Move(spd * 0.2f);
                    animTime -= 0.7f;
                }
            }
            if (Keyboard[Key.Left]) player.Rotation.Y += spd * 10;
            if (Keyboard[Key.Right]) player.Rotation.Y -= spd * 10;

            base.Update(time);
        }

        public override void Render(float time)
        {
            Camera.Update();
            player.Update(animTime);

            h3d.render(Camera.Node);
        }
    }
}
