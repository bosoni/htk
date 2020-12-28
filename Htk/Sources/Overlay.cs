/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using System.Collections.Generic;
using System;

namespace Htk
{
    public class Overlay
    {
        static List<Overlay> _overlays = new List<Overlay>();

        int matRes = -1;
        string textureName;
        public int Width, Height;
        public Vector4 Color = new Vector4(1, 1, 1, 1);

        public void Dispose()
        {
            if (matRes != -1)
                h3d.removeResource(matRes);
            matRes = -1;
        }

        public static Overlay Create(string textureName)
        {
            foreach (Overlay ov in _overlays)
                if (ov.textureName == textureName)
                    return ov;

            Overlay o = new Overlay();
            o.textureName = textureName;

            string str = "<Material>" +
                "<Shader source=\"shaders/overlay.shader\"/>" +
                "<Sampler name=\"albedoMap\" map=\"textures/" + textureName + "\"/>" +
                "</Material>";

            o.matRes = h3d.addResource((int)h3d.H3DResTypes.Material, textureName + ".material", 0);
            int texRes = h3d.addResource((int)h3d.H3DResTypes.Texture, "textures/" + textureName, 0);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            if (h3d.loadResource(o.matRes, encoding.GetBytes(str), str.Length) == false)
                Log.Error("Can't create material " + textureName);

            Util.LoadResourcesFromDisk();

            o.Width = h3d.getResParamI(texRes, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            o.Height = h3d.getResParamI(texRes, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgHeightI);

            _overlays.Add(o);
            return o;
        }

        public void Draw(float x, float y, float scaleX = 1, float scaleY = 1, float rotate = 0)
        {
            float s = (float)Settings.Width / (float)Settings.Height;
            x = s * x;
            float w = s * ((Width * scaleX) / (float)Settings.Width);
            float h = (Height * scaleY) / (float)Settings.Height;
            float x2 = x + w;
            float y2 = y + h;
            float[] xyuv = new float[] { 
                x, y, 0,1, 
                x, y2, 0,0,
                x2, y2, 1,0,
                x2, y, 1,1 };

            if (rotate != 0)
            {
                float rot = MathHelper.DegreesToRadians(rotate);
                float centerX = (x2 - x) / 2 + x;
                float centerY = (y2 - y) / 2 + y;
                rotatePoint(ref xyuv[0], ref xyuv[1], centerX, centerY, rot);
                rotatePoint(ref xyuv[4], ref xyuv[5], centerX, centerY, rot);
                rotatePoint(ref xyuv[8], ref xyuv[9], centerX, centerY, rot);
                rotatePoint(ref xyuv[12], ref xyuv[13], centerX, centerY, rot);
            }

            h3d.showOverlays(xyuv, 4, Color.X, Color.Y, Color.Z, Color.W, matRes, 0);
        }

        private void rotatePoint(ref float x, ref float y, float centerX, float centerY, float rot)
        {
            float newx = ((float)Math.Cos((float)rot) * (x - centerX)) + ((float)Math.Sin((float)rot) * (y - centerY));
            float newy = ((float)-Math.Sin((float)rot) * (x - centerX)) + ((float)Math.Cos((float)rot) * (y - centerY));
            x = newx + centerX;
            y = newy + centerY;
        }

        public void DrawToArea(float x, float y, float x2, float y2)
        {
            float s = (float)Settings.Width / (float)Settings.Height;
            x = s * x;
            x2 = s * x2;
            float[] xyuv = new float[] { 
                x, y, 0,1, 
                x, y2, 0,0,
                x2, y2, 1,0,
                x2, y, 1,1  };
            h3d.showOverlays(xyuv, 4, Color.X, Color.Y, Color.Z, Color.W, matRes, 0);
        }

        public void DrawFullScreen()
        {
            float x = 0, y = 0;
            float x2 = (float)Settings.Width / (float)Settings.Height;
            float y2 = 1;
            float[] xyuv = new float[] { 
                x, y, 0,1, 
                x, y2, 0,0,
                x2, y2, 1,0,
                x2, y, 1,1  };
            h3d.showOverlays(xyuv, 4, Color.X, Color.Y, Color.Z, Color.W, matRes, 0);
        }

    }
}
