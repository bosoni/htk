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
    public class TestTiledMap : BaseGame
    {
        Light light = new Light();
        Map map = new Map();

        public override void Init()
        {
            map.CreateMap("map.png", new string[] { "wall", "floor" });

            Camera.Position = new Vector3(10, 10, 10);
            Camera.Rotation = new Vector3(0, -120, 0);

            // add light
            light.Create(h3d.H3DRootNode, new Vector3(50, 50, 50), new Vector3(-90, 0, 0));

            base.Init();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(float time)
        {
            float spd = (float)time * 10;
            if (Keyboard[Key.ShiftLeft]) spd *= 2;
            if (Keyboard[Key.W]) Camera.Move(spd);
            if (Keyboard[Key.S]) Camera.Move(-spd);
            if (Keyboard[Key.A]) Camera.Strafe(-spd);
            if (Keyboard[Key.D]) Camera.Strafe(spd);
            if (Mouse[MouseButton.Left])
            {
                Camera.Rotation.Y -= Mouse.X - oldMouseX;
                Camera.Rotation.X -= Mouse.Y - oldMouseY;
            }


            base.Update(time);
        }

        public override void Render(float time)
        {
            Camera.Update();
            h3d.render(Camera.Node);
        }
    }
}
