// game-test (c) by mjt
using System;
using Horde3DNET;
using OpenTK.Input;
using Htk;

namespace GameTest
{

    public class BaseGame
    {
        public static Random rnd = new Random();
        public static KeyboardDevice Keyboard;
        public static MouseDevice Mouse;

        protected Camera camera = new Camera();
        protected TextRenderer text = null;
        protected int oldMouseX, oldMouseY;
        protected bool mouseLeftPressed = false, mouseRightPressed = false, mouseMiddlePressed = false;

        public virtual void Dispose()
        {
            if (text != null) text.Dispose();
            text = null;
        }

        public virtual void Init()
        {
        }

        public virtual void Update(float time)
        {
            if (Mouse[MouseButton.Left]) mouseLeftPressed = true; else mouseLeftPressed = false;
            if (Mouse[MouseButton.Right]) mouseRightPressed = true; else mouseRightPressed = false;
            if (Mouse[MouseButton.Middle]) mouseMiddlePressed = true; else mouseMiddlePressed = false;

            oldMouseX = Mouse.X;
            oldMouseY = Mouse.Y;
        }

        public virtual void Render()
        {
        }

        void Resize()
        {
            h3d.setNodeParamI(camera.Node, (int)h3d.H3DCamera.ViewportXI, 0);
            h3d.setNodeParamI(camera.Node, (int)h3d.H3DCamera.ViewportYI, 0);
            h3d.setNodeParamI(camera.Node, (int)h3d.H3DCamera.ViewportWidthI, Settings.Width);
            h3d.setNodeParamI(camera.Node, (int)h3d.H3DCamera.ViewportHeightI, Settings.Height);
            h3d.resizePipelineBuffers(camera.Pipeline, Settings.Width, Settings.Height);

            // Set virtual camera parameters
            h3d.setupCameraView(camera.Node, camera.Fov, (float)Settings.Width / Settings.Height, camera.Near, camera.Far);
        }

        public void Resize(int width, int height)
        {
            Settings.Width = width;
            Settings.Height = height;
            Resize();
        }
    }

}
