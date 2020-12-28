/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK.Input;
using OpenTK;

namespace Htk
{
    public class BaseGame
    {
        public static bool Running = true;
        public static Random rnd = new Random();
        public static KeyboardDevice Keyboard;
        public static MouseDevice Mouse;
        public static Camera Camera = new Camera();

        protected TextRenderer text = null;
        protected int oldMouseX, oldMouseY;
        protected bool mouseLeftPressed = false, mouseRightPressed = false, mouseMiddlePressed = false;

        static int fontMatResDEBUG, panelMatResDEBUG;

        /// <summary>
        /// pipelineName can be:
        ///    forward
        ///    deferred
        ///    hdr
        /// </summary>
        protected string pipelineName = "forward";

        public virtual void Dispose()
        {
            if (text != null)
            {
                text.Dispose();
                text = null;
            }
            h3d.clear();

            Running = false;
        }

        public virtual void Init()
        {
            Camera.Create(h3d.H3DRootNode, Camera.Position, Camera.Rotation, pipelineName);
            Resize();

            // Overlays
            fontMatResDEBUG = h3d.addResource((int)h3d.H3DResTypes.Material, "overlays/font.material.xml", 0);
            panelMatResDEBUG = h3d.addResource((int)h3d.H3DResTypes.Material, "overlays/panel.material.xml", 0);
            Util.LoadResourcesFromDisk();
        }

        public virtual void Update(float time)
        {
            if (Mouse[MouseButton.Left]) mouseLeftPressed = true; else mouseLeftPressed = false;
            if (Mouse[MouseButton.Right]) mouseRightPressed = true; else mouseRightPressed = false;
            if (Mouse[MouseButton.Middle]) mouseMiddlePressed = true; else mouseMiddlePressed = false;

            oldMouseX = Mouse.X;
            oldMouseY = Mouse.Y;
        }

        public virtual void Render(float time)
        {
        }

        static void Resize()
        {
            h3d.setNodeParamI(Camera.Node, (int)h3d.H3DCamera.ViewportXI, 0);
            h3d.setNodeParamI(Camera.Node, (int)h3d.H3DCamera.ViewportYI, 0);
            h3d.setNodeParamI(Camera.Node, (int)h3d.H3DCamera.ViewportWidthI, Settings.Width);
            h3d.setNodeParamI(Camera.Node, (int)h3d.H3DCamera.ViewportHeightI, Settings.Height);
            h3d.resizePipelineBuffers(Camera.Pipeline, Settings.Width, Settings.Height);
            h3d.setupCameraView(Camera.Node, Camera.Fov, (float)Settings.Width / Settings.Height, Camera.Near, Camera.Far);
        }

        public static void Resize(int width, int height)
        {
            Settings.Width = width;
            Settings.Height = height;
            Resize();
        }

        public static void ShowFrameStats()
        {
            if (Settings.ShowStats)
                Horde3DUtils.showFrameStats(fontMatResDEBUG, panelMatResDEBUG, Horde3DUtils.MaxStatMode);
        }
    }
}
