/*
 * Htk framework (c) mjt, 2011-2012
 * [matola@sci.fi]
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Htk
{
    public class Texture2D
    {
        public int Width, Height, RealWidth, RealHeight;
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        string textureName = "";
        protected uint textureID = 0;
        TextureTarget target;
        VBO Vbo = null;

        public static Texture2D Load(string fileName)
        {
            fileName = Settings.ContentDir + "/" + fileName;

            Texture2D tex;
            // jos texture on jo ladattu, palauta se
            textures.TryGetValue(fileName, out tex);
            if (tex != null)
            {
                Log.WriteLine("Info: texture " + fileName + " already loaded.");
                return tex;
            }

            tex = new Texture2D();
            tex.textureName = fileName;

            try
            {
                if (fileName.Contains(".dds")) // jos dds texture
                {
                    ImageDDS.LoadFromDisk(fileName, out tex.textureID, out tex.target);
                }
                else
                {
                    ImageGDI.LoadFromDisk(fileName, out tex.textureID, out tex.target);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception(e.ToString());
            }

            float[] pwidth = new float[1];
            float[] pheight = new float[1];
            GL.BindTexture(tex.target, tex.textureID);
            GL.GetTexLevelParameter(tex.target, 0, GetTextureParameter.TextureWidth, pwidth);
            GL.GetTexLevelParameter(tex.target, 0, GetTextureParameter.TextureHeight, pheight);
            tex.Width = (int)pwidth[0];
            tex.Height = (int)pheight[0];
            if (fileName.Contains(".dds"))
            {
                tex.RealWidth = tex.Width;
                tex.RealHeight = tex.Height;
            }
            else
            {
                tex.RealWidth = ImageGDI.RealWidth;
                tex.RealHeight = ImageGDI.RealHeight;
            }
            tex.CreateVBO();

            textures.Add(tex.textureName, tex);
            return tex;
        }

        void CreateVBO()
        {
            if (Vbo != null) Vbo.Dispose();
            ushort[] ind = new ushort[] { 0, 1, 3, 1, 2, 3 };
            float w = 0.5f, h = 0.5f;
            Vertex[] vert =
            {
                new Vertex(new Vector3(-w, -h, 0), new Vector3(0, 0, 1), new Vector2(0, 0)),
                new Vertex(new Vector3(w, -h, 0), new Vector3(0, 0, 1), new Vector2(1, 0)),
                new Vertex(new Vector3(w, h, 0), new Vector3(0, 0, 1), new Vector2(1, 1)),
                new Vertex(new Vector3(-w, h, 0), new Vector3(0, 0, 1), new Vector2(0, 1))
            };
            Vbo = new VBO();
            Vbo.DataToVBO(vert, ind);
        }

        public void Dispose()
        {
            if (textureID != 0)
            {
                GL.DeleteTextures(1, ref textureID);
                textureID = 0;
                Vbo.Dispose();
                Vbo = null;

                textures.Remove(textureName);
                textureName = "";
            }
        }
        public static void DisposeAll()
        {
            List<string> tex = new List<string>();
            foreach (KeyValuePair<string, Texture2D> dta in textures) tex.Add(dta.Key);
            for (int q = 0; q < tex.Count; q++) textures[tex[q]].Dispose();
            textures.Clear();
        }

        static void Set2DMode()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Viewport(0, 0, Settings.Width, Settings.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, 0.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Draw(float x, float y, float rotate, float scaleX, float scaleY, bool blend)
        {
            scaleX *= (float)Width / (float)Settings.Width;
            scaleY *= (float)Height / (float)Settings.Height;
            x += 0.5f * scaleX;
            y += 0.5f * scaleY;
            Vector4 color = new Vector4(1, 1, 1, 1);
            Draw(x - 0.5f, 0.5f - y, rotate, scaleX, scaleY, blend, color);
        }
        public void DrawFullScreen(bool blend)
        {
            Vector4 color = new Vector4(1, 1, 1, 1);
            Draw(0, 0, 0, 1, 1, blend, color);
        }
        public void DrawFullScreen(bool blend, Vector4 color)
        {
            Draw(0, 0, 0, 1, 1, blend, color);
        }

        public void Draw(float x, float y, float rotate, float scaleX, float scaleY, bool blend, Vector4 color)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.PushMatrix();
            GL.UseProgram(0);
            Set2DMode();
            GL.Translate(x, y, 0);
            GL.Scale(scaleX, scaleY, 0);
            GL.Rotate(rotate, 0, 0, 1);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            if (blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.Color4((float)color.X, color.Y, color.Z, color.W);
            Vbo.Render();

            GL.PopAttrib();
            GL.PopMatrix();
        }

        public void RenderBillboard(float x, float y, float z, float zrot, float size, bool blend)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.UseProgram(0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            if (blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One); // BlendingFactorDest.OneMinusSrcAlpha);
            }

            GL.PushMatrix();
            {
                GL.Translate(x, y, z);
                size *= 10;
                GL.Scale(size, size, size);
                GL.Rotate(zrot, 0, 0, 1);

                float[] matrix=new float[16];
                GL.GetFloat(GetPName.ModelviewMatrix, matrix);
                Matrix4 mat=Matrix4.Identity;
                mat.Row3.X = matrix[12];
                mat.Row3.Y = matrix[13];
                mat.Row3.Z = matrix[14];
                mat.Row3.W = matrix[15];
                                
                GL.LoadMatrix(ref mat);
                
                Vbo.Render();
            }
            GL.PopMatrix();
            GL.PopAttrib();
        }

        public void RenderBillboard(Vector3 pos, float zrot, float size, bool blend)
        {
            RenderBillboard(pos.X, pos.Y, pos.Z, zrot, size, blend);
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position, Normal;
        public Vector2 UV;
        public static int Size;
        public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV.X = uv.X;
            this.UV.Y = uv.Y;
        }
    }

    public class VBO
    {
        int vertexID = -1, indexID = -1;
        int numOfIndices = 0;

        public void Dispose()
        {
            if (vertexID != -1) GL.DeleteBuffers(1, ref vertexID);
            if (indexID != -1) GL.DeleteBuffers(1, ref indexID);
            vertexID = indexID =  -1;
            numOfIndices = 0;
        }

        public void DataToVBO(Vertex[] vertices, ushort[] indices)
        {
            if (numOfIndices > 0) Dispose();
            numOfIndices = indices.Length;
            Vertex.Size = BlittableValueType.StrideOf(vertices);
            int size;

            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vertex.Size), vertices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size) Log.Error("DataToVBO: Vertex data not uploaded correctly.");

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(short) != size) Log.Error("DataToVBO: Element data not uploaded correctly.");
        }

        public void Render()
        {
            if (vertexID == -1 || indexID == -1) Log.Error("VBO destroyed!");
            GL.UseProgram(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, Vertex.Size, (IntPtr)Vector3.SizeInBytes);
            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Size, (IntPtr)(2 * Vector3.SizeInBytes));
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, Vertex.Size, (IntPtr)(0));

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }
    }
}
