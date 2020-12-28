// from otk
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Htk;

namespace GameTest
{
    /// <summary>
    /// Uses System.Drawing for 2d text rendering.
    /// </summary>
    public class TextRenderer : IDisposable
    {
        public static Font FontSerif = new Font(FontFamily.GenericSerif, 16);
        public static Font FontSans = new Font(FontFamily.GenericSansSerif, 16);
        public static Font FontMono = new Font(FontFamily.GenericMonospace, 12);

        string oldString = "";
        Bitmap bmp;
        Graphics gfx;
        int texture;
        Rectangle dirty_region;
        bool disposed;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="width">The width of the backing store in pixels.</param>
        /// <param name="height">The height of the backing store in pixels.</param>
        public TextRenderer(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height ");
            if (GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("No GraphicsContext is current on the calling thread.");

            bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(bmp);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            Clear(Color.Transparent);
        }

        /// <summary>
        /// Clears the backing store to the specified color.
        /// </summary>
        /// <param name="color">A <see cref="System.Drawing.Color"/>.</param>
        public void Clear(Color color)
        {
            gfx.Clear(color);
            dirty_region = new Rectangle(0, 0, bmp.Width, bmp.Height);
        }

        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to draw.</param>
        /// <param name="font">The <see cref="System.Drawing.Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="System.Drawing.Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            if (text == oldString)
            {
                dirty_region = Rectangle.Empty;
                return;
            }

            gfx.DrawString(text, font, brush, point);

            //SizeF size = gfx.MeasureString(text, font);
            //dirty_region = Rectangle.Round(RectangleF.Union(dirty_region, new RectangleF(point, size)));
            //dirty_region = Rectangle.Intersect(dirty_region, new Rectangle(0, 0, bmp.Width, bmp.Height));
        }

        // Uploads the dirty regions of the backing store to the OpenGL texture.
        void UploadBitmap()
        {
            if (dirty_region != RectangleF.Empty)
            {
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(dirty_region,
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    dirty_region.X, dirty_region.Y, dirty_region.Width, dirty_region.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);

                dirty_region = Rectangle.Empty;
            }
        }


        public void Render()
        {
            UploadBitmap();
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, 0.0, 4.0);
            GL.Translate(-0.5f, 0.5f, 0);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            float height = (float)bmp.Height / (float)Settings.Height;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(0, -height);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1f, -height);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1f, 0);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(0, 0);
            GL.End();
            GL.PopAttrib();
            GL.PopMatrix();
        }


        #region IDisposable Members

        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    bmp.Dispose();
                    gfx.Dispose();
                    if (GraphicsContext.CurrentContext != null)
                        GL.DeleteTexture(texture);
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TextRenderer()
        {
            Console.WriteLine("[Warning] Resource leaked: {0}.", typeof(TextRenderer));
        }

        #endregion
    }

}
