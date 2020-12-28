// game-test (c) by mjt
using System;
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Htk;

namespace GameTest
{
    public class TestGameLoop : GameWindow
    {
        public static string NextClass = "";
        BaseGame game;

        public TestGameLoop()
            : base(Settings.Width, Settings.Height, new GraphicsMode(Settings.Bpp), "Unnamed game project")
        {
            VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
            Settings.Device = DisplayDevice.Default;
            if (Settings.FullScreen)
            {
                Settings.Device.ChangeResolution(Settings.Device.SelectResolution(Settings.Width, Settings.Height, Settings.Bpp, 60f));
                WindowState = OpenTK.WindowState.Fullscreen;
            }

            if (!h3d.init())
            {
                Horde3DUtils.dumpMessages();
                throw new Exception();
            }

            h3d.setOption(h3d.H3DOptions.LoadTextures, 1);
            h3d.setOption(h3d.H3DOptions.TexCompression, 1);
            h3d.setOption(h3d.H3DOptions.FastAnimation, 1);
            h3d.setOption(h3d.H3DOptions.MaxAnisotropy, Settings.MaxAnisotropy);
            h3d.setOption(h3d.H3DOptions.ShadowMapSize, Settings.ShadowMapSize);
            h3d.setOption(h3d.H3DOptions.SampleCount, Settings.FSAA);

            game = new Game();
            game.Init();
            BaseGame.Keyboard = Keyboard;
            BaseGame.Mouse = Mouse;

            //CursorVisible = false; otk bugi todo ota käyttöön taas tämä kunhan otk korjattu
            System.Windows.Forms.Cursor.Hide();

        }

        public override void Dispose()
        {
            if (Settings.FullScreen) Settings.Device.RestoreResolution();
            base.Dispose();

            // Write all messages to log file
            Horde3DUtils.dumpMessages();
            h3d.release();
        }

        protected override void OnResize(EventArgs e)
        {
            if (game == null) return;
            game.Resize(Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (game == null) return;

            game.Update((float)e.Time);

            switch (NextClass)
            {
                case "Exit":
                    game.Dispose();
                    game = null;
                    this.Exit();
                    break;

                case "Start":
                    break;
            }

            if (Keyboard[Key.AltLeft] && Keyboard[Key.Enter])
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;
            }

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (game == null) return;
            game.Render();

            h3d.finalizeFrame();
            h3d.clearOverlays();
            SwapBuffers();
        }
    }

}
