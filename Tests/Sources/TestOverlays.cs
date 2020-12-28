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
    public class TestOverlays : BaseGame
    {
        Model man;
        Overlay logo, mbar, gear, colors, pointer;
        float col = 0;

        public override void Init()
        {
            man = Model.Load("man");
            man.Position = new OpenTK.Vector3(0, 0, -10);

            colors = Overlay.Create("colors.png");
            colors.Color = new Vector4(1, 1, 1, 0.2f);

            pointer = Overlay.Create("cursor.png");
            logo = Overlay.Create("logo.png");
            mbar = Overlay.Create("menubar.png");
            gear = Overlay.Create("gear.png");

            base.Init();
        }

        public override void Dispose()
        {
            logo.Dispose();
            mbar.Dispose();
            gear.Dispose();

            man.Dispose();
            base.Dispose();
        }

        public override void Update(float time)
        {
            col += 0.5f;
            mbar.Color = new Vector4(1, 1, 1, (float)System.Math.Abs(System.Math.Sin(0.01f * col)));
            base.Update(time);
        }

        float x = 0;
        public override void Render(float time)
        {
            // 3d object to the background
            man.Rotation.Y += 0.4f;
            man.Rotation.Z += 0.5f;
            man.Update(); // IMPORTANT!

            // when drawing 2d, coordinates are xy [0,1]  (so 0.5, 0.5 is center of the screen)

            colors.DrawFullScreen();
            mbar.DrawFullScreen();
            logo.Draw(0f, 0f); // upper left, original size
            mbar.DrawToArea(0, 0.9f, 1, 1);

            // rotating gear, col=angle
            gear.Draw(1f, 1f, -0.5f, -0.5f, col); // bottom right, half size


            // moving gear
            gear.Draw(x += 0.001f, 0f, 1, 1, -2 * man.Rotation.Z);
            if (x >= 1) x = 0;

            // mouse
            pointer.Draw((float)Mouse.X / (float)Settings.Width, (float)Mouse.Y / (float)Settings.Height);


            h3d.render(Camera.Node);
        }
    }
}
