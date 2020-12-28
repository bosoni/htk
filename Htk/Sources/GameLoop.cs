/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using System.Collections.Generic;
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Htk
{
    public class GameLoop : GameWindow
    {
        public static string Extensions = "";

        public GameLoop(string name, bool hideMouseCursor)
            : base(Settings.Width, Settings.Height,
            new GraphicsMode(Settings.Bpp, 32, 0, Settings.FSAA, 0, 2, false), name)
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

            Extensions = GL.GetString(StringName.Extensions);

            h3d.setOption(h3d.H3DOptions.LoadTextures, 1);
            h3d.setOption(h3d.H3DOptions.TexCompression, 1);
            h3d.setOption(h3d.H3DOptions.FastAnimation, 1);
            h3d.setOption(h3d.H3DOptions.MaxAnisotropy, Settings.MaxAnisotropy);
            h3d.setOption(h3d.H3DOptions.ShadowMapSize, Settings.ShadowMapSize);
            h3d.setOption(h3d.H3DOptions.SampleCount, Settings.FSAA);
            BaseGame.Keyboard = Keyboard;
            BaseGame.Mouse = Mouse;

            if (hideMouseCursor)
            {
                //CursorVisible = false;
                System.Windows.Forms.Cursor.Hide();
            }
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
            if (StateManager.IsAnyStateActive() == false) return;
            BaseGame.Resize(Width, Height);
        }

        bool isF1 = false;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Escape])
            {
                StateManager.Clear();
                Exit();
                return;
            }

            if (Keyboard[Key.F1])
            {
                if (isF1 == false)
                {
                    isF1 = true;
                    Settings.ShowStats = !Settings.ShowStats;
                }
            }
            else isF1 = false;

            if (Keyboard[Key.AltLeft] && Keyboard[Key.Enter])
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;
            }

            if (StateManager.IsAnyStateActive() == false) return;
            StateManager.Update((float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (StateManager.IsAnyStateActive() == false) return;

            BaseGame.ShowFrameStats();

            StateManager.Render((float)e.Time);

            h3d.finalizeFrame();
            h3d.clearOverlays();
            SwapBuffers();

#if DEBUG
            Horde3DUtils.dumpMessages();
#endif
        }
    }

}
