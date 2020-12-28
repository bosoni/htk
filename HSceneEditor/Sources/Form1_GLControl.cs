/// glcontrol koodit

using System;
using System.Drawing;
using System.Windows.Forms;
using Horde3DNET;
using Horde3DNET.Utils;
using Htk;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace HSceneEditor
{
    public partial class Form1 : Form
    {
        Camera editorCamera = new Camera();
        bool mouseRightPressed = false, mouseMiddlePressed = false;
        int oldMouseX = 0, oldMouseY = 0;
        /// <summary>
        /// 1 = x
        /// 2 = y
        /// 3 = z
        /// </summary>
        byte lockAxis = 0;

        /// <summary>
        /// 0 = moving
        /// 1 = rotating
        /// 2 = scaling
        /// </summary>
        byte mode = 0;

        // presets
        static Vector3 cameraTopPos = new Vector3(0, 20, 0), cameraTopRot = new Vector3(-90, 0, 0);
        static Vector3 cameraFrontPos = new Vector3(0, 2, 20), cameraFrontRot = new Vector3(0, 0, 0);
        static Vector3 cameraRightPos = new Vector3(20, 2, 0), cameraRightRot = new Vector3(0, 90, 0);

        Texture2D imgCam, imgSound, imgLight;

        private void glControl1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!h3d.init())
                {
                    Horde3DUtils.dumpMessages();
                    return;
                }
            }
            catch (Exception ee)
            {
                Log.WriteLine(ee.ToString());
                return;
            }

            h3d.setOption(h3d.H3DOptions.LoadTextures, 1);
            h3d.setOption(h3d.H3DOptions.TexCompression, 1);
            h3d.setOption(h3d.H3DOptions.FastAnimation, 1);
            h3d.setOption(h3d.H3DOptions.MaxAnisotropy, Settings.MaxAnisotropy);
            h3d.setOption(h3d.H3DOptions.ShadowMapSize, Settings.ShadowMapSize);
            h3d.setOption(h3d.H3DOptions.SampleCount, Settings.FSAA);
            Application.Idle += Application_Idle;
            imgCam = Texture2D.Load("editor_data/cam_img.png");
            imgLight = Texture2D.Load("editor_data/light_img.png");
            imgSound = Texture2D.Load("editor_data/audio_img.png");

            loaded = true;

            CreateDefaultObjects();
        }

        private void glControl1_MouseEnter(object sender, EventArgs e)
        {
            glControl1.Focus();
        }


        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;
            Render();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (!loaded) return;
            glControl1.MakeCurrent();

            glControl1.ClientSize = new Size(splitContainer1.Panel2.Width, splitContainer1.Panel2.Height);
            if (glControl1.ClientSize.Height == 0) glControl1.ClientSize = new System.Drawing.Size(glControl1.ClientSize.Width, 1);
            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);

            BaseGame.Resize(glControl1.ClientSize.Width, glControl1.ClientSize.Height);
        }

        /// <summary>
        /// mouse move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (selectedNode != -1 || treeView1.SelectedNode != null)
            {
                if (mode == 0) // move
                {
                    if (lockAxis == 1) curObj.pos.X += BaseGame.Camera.DeltaX * 0.02f; // X
                    else if (lockAxis == 2) curObj.pos.Y -= BaseGame.Camera.DeltaY * 0.02f; // Y
                    else if (lockAxis == 3) curObj.pos.Z += BaseGame.Camera.DeltaY * 0.02f; // Z
                    if (lockAxis != 0) toolStripStatusLabel1.Text = "Moving: " + curObj.pos.ToString();
                }
                else if (mode == 1) // rotation
                {
                    if (lockAxis == 0)
                    {
                        curObj.rot.X += BaseGame.Camera.DeltaY * 1f; // X
                        curObj.rot.Y += BaseGame.Camera.DeltaX * 1f; // Y
                    }
                    else if (lockAxis == 1) curObj.rot.X += BaseGame.Camera.DeltaY * 1f; // X
                    else if (lockAxis == 2) curObj.rot.Y += BaseGame.Camera.DeltaX * 1f; // Y
                    else if (lockAxis == 3) curObj.rot.Z += BaseGame.Camera.DeltaY * 1f; // Z
                    toolStripStatusLabel1.Text = "Rotating: " + curObj.rot.ToString();
                }
                else
                    if (mode == 2) // scale
                    {
                        if (lockAxis == 0)
                        {
                            curObj.scale.X += BaseGame.Camera.DeltaX * 0.05f; // X
                            curObj.scale.Y += BaseGame.Camera.DeltaX * 0.05f; // Y
                            curObj.scale.Z += BaseGame.Camera.DeltaX * 0.05f; // Z
                        }
                        else if (lockAxis == 1) curObj.scale.X += BaseGame.Camera.DeltaX * 0.05f; // X
                        else if (lockAxis == 2) curObj.scale.Y -= BaseGame.Camera.DeltaY * 0.05f; // Y
                        else if (lockAxis == 3) curObj.scale.Z += BaseGame.Camera.DeltaY * 0.05f; // Z
                        toolStripStatusLabel1.Text = "Scaling: " + curObj.scale.ToString();
                    }

                ObjectToParameters();
            }

            if (selectedNode != -1)
            {
                if (lockAxis == 0 && mode == 0)
                {
                    toolStripStatusLabel1.Text = "Left mouse button: puts object to the scene. G=Move, R=Rotate, S=Scale. X,Y,Z locks axis. Esc cancel. Right mouse button with A,D,S,W Right mouse button with A,S,W,D moves the camera.";

                    // hiirenkoordinaatit 0-1 floattina
                    float mx = (float)e.X / (float)Settings.Width, my = (float)(Settings.Height - e.Y) / (float)Settings.Height;

                    // piilota valittu obj
                    Vector3 scaleTmp = curObj.pos;
                    curObj.pos = new Vector3(-100000, 100000, -100000);
                    SetTransform();

                    // lasketaan 3d-koordinaatit hiiren koordinaateista
                    float ox, oy, oz, dx, dy, dz, dist = 0;
                    float[] intersectionPoint = new float[3];
                    int node = 0;
                    Horde3DUtils.pickRay(BaseGame.Camera.Node, mx, my, out ox, out oy, out oz, out dx, out dy, out dz);

                    if (h3d.castRay(h3d.H3DRootNode, ox, oy, oz, dx * 10000, dy * 10000, dz * 10000, 0) != 0)
                    {
                        if (h3d.getCastRayResult(0, out node, out dist, intersectionPoint))
                        {
                            if (checkBox12.Checked) // jos snapataan Y:hyn, aseta se arvo pos.Y:hyn
                            {
                                numericUpDown2.Value = numericUpDown22.Value;
                                intersectionPoint[1] = (float)numericUpDown22.Value;
                            }
                            curObj.pos = new Vector3(intersectionPoint[0], intersectionPoint[1], intersectionPoint[2]);
                            SetTransform();

                            numericUpDown1.Value = (decimal)curObj.pos.X;
                            numericUpDown2.Value = (decimal)curObj.pos.Y;
                            numericUpDown3.Value = (decimal)curObj.pos.Z;
                        }
                    }
                    else
                    {
                        curObj.pos = new Vector3(0, (float)numericUpDown2.Value, 0);
                        SetTransform();
                    }
                }
            }

            BaseGame.Camera.DeltaX = e.X - oldMouseX;
            BaseGame.Camera.DeltaY = e.Y - oldMouseY;
            if (mouseRightPressed)
            {
                BaseGame.Camera.Rotation.Y -= BaseGame.Camera.DeltaX;
                BaseGame.Camera.Rotation.X -= BaseGame.Camera.DeltaY;
            }
            else if (mouseMiddlePressed)
            {
                BaseGame.Camera.Strafe(BaseGame.Camera.DeltaX * 0.1f);
                BaseGame.Camera.Position.Y -= BaseGame.Camera.DeltaY * 0.1f;
            }
            oldMouseX = e.X;
            oldMouseY = e.Y;
        }

        private void glControl1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int wheel = e.Delta;

            // jos painaa samalla ctrl, objekti skaalautuu
            if (Keyboard.IsKeyPressed(Keys.ControlKey))
            {
                float r = (float)wheel * 0.001f;
                numericUpDown7.Value += (decimal)r;
                numericUpDown8.Value += (decimal)r;
                numericUpDown9.Value += (decimal)r;
                SetTransform();
            }
            else
                BaseGame.Camera.Move(0.01f * (float)wheel);
        }


        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseRightPressed = false;
            mouseMiddlePressed = false;
        }

        /// <summary>
        /// mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mouseRightPressed = true;
                return;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                mouseMiddlePressed = true;
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mode = lockAxis = 0;
                if (selectedNode != -1) // laitetaan objekti skeneen
                {
                    // jos autorand eli joka klikkaus aiheuttaa myös randomizen
                    if (checkBox13.Checked) button2_Click(null, null); // kutsu Randomizea
                    SetTransform();

                    // luodaanko uusi objekti (vai vai liikutettu jo luotua jolloin moving==true)
                    if (moving == false)
                    {
                        curObj.name = (treeView2.SelectedNode.Text.Substring(0, treeView2.SelectedNode.Text.Length - ".scene.xml".Length) + "_" + _index++);
                        curObj.fileName = treeView2.SelectedNode.FullPath;
                        curObj.res = selectedRes;
                        curObj.node = selectedNode;

                        if (script_comboBox1.SelectedItem != null && (string)script_comboBox1.SelectedItem != "")
                        {
                            curObj.scriptName = (string)script_comboBox1.SelectedItem;
                            PrepareScript(curObj.scriptName); // lataa scripti
                            Scene.AddOrig(curObj);
                        }

                        Scene.Objs.Add(curObj);
                        treeView1.Nodes.Add(curObj.node.ToString(), curObj.name + GetObjStatusStr());

                        // uusi objekti käyttää asetettuja arvoja
                        curObj = new Obj(
                            (float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value,
                            (float)numericUpDown4.Value, (float)numericUpDown5.Value, (float)numericUpDown6.Value,
                            (float)numericUpDown7.Value, (float)numericUpDown8.Value, (float)numericUpDown9.Value,
                            curObj.color.X, curObj.color.Y, curObj.color.Z);

                        // luodaan uusi node, vanha jää paikoilleen
                        selectedNode = h3d.addNodes(h3d.H3DRootNode, selectedRes);
                        SetTransform();

                        // jos käytetään skriptiä, asetetaan se nimi jo uuteen objektiin
                        if (script_comboBox1.SelectedItem != null && (string)script_comboBox1.SelectedItem != "")
                        {
                            curObj.scriptName = (string)script_comboBox1.SelectedItem;
                            curObj.script = new Script();
                        }
                    }
                    else // luotua objektia muokattu
                    {
                        selectedNode = -1;
                        moving = false;

                        Scene.RemoveOrig(curObj);

                    }
                }
                else // selection (picking)
                {
                    // hiirenkoordinaatit 0-1 floattina
                    float mx = (float)e.X / (float)Settings.Width, my = (float)(Settings.Height - e.Y) / (float)Settings.Height;

                    int node = Horde3DUtils.pickNode(BaseGame.Camera.Node, mx, my);
                    node = h3d.getNodeParent(node);

                    for (int q = 0; q < treeView1.Nodes.Count; q++)
                    {
                        if (treeView1.Nodes[q].Name == node.ToString())
                        {
                            treeView1.SelectedNode = treeView1.Nodes[q];
                            treeview1_ObjectSelected();
                            break;
                        }
                    }
                }
            }
        }

        void SetTransform()
        {
            // jos käytetään snappia xz gridiin
            if (checkBox11.Checked)
            {
                int x = (int)(curObj.pos.X / (float)numericUpDown20.Value);
                int z = (int)(curObj.pos.Z / (float)numericUpDown21.Value);
                curObj.pos.X = (float)x * (float)numericUpDown20.Value;
                curObj.pos.Z = (float)z * (float)numericUpDown20.Value;
            }

            // jos pakotetaan Y:hyn, aseta se arvo pos.Y:hyn
            if (checkBox12.Checked) numericUpDown2.Value = numericUpDown22.Value;

            // aseta objektin paikka
            if (_update)
            {
                bool selection = false;
                if (treeView1.SelectedNode == null) selection = true;

                if (selection)
                    h3d.setNodeTransform(selectedNode,
                        curObj.pos.X, curObj.pos.Y, curObj.pos.Z, // pos
                        curObj.rot.X, curObj.rot.Y, curObj.rot.Z, // rot
                        curObj.scale.X, curObj.scale.Y, curObj.scale.Z); // scale
                else
                    h3d.setNodeTransform(curObj.node,
                        curObj.pos.X, curObj.pos.Y, curObj.pos.Z, // pos
                        curObj.rot.X, curObj.rot.Y, curObj.rot.Z, // rot
                        curObj.scale.X, curObj.scale.Y, curObj.scale.Z); // scale
            }
        }

        void CreateDefaultObjects()
        {
            // lisää default kamera
            BaseGame.Camera.Position = new Vector3(0, 2, 10);
            BaseGame.Camera.Rotation = Vector3.Zero;
            BaseGame.Camera.Create(h3d.H3DRootNode, BaseGame.Camera.Position, BaseGame.Camera.Rotation, "forward");
            glControl1_Resize(this, EventArgs.Empty);   // Ensure the Viewport is set up correctly

            Obj obj = new Obj();
            obj.name = "-Default Camera <)";
            obj.node = BaseGame.Camera.Node;
            obj.pos = BaseGame.Camera.Position;
            obj.rot = BaseGame.Camera.Rotation;
            Scene.Objs.Add(obj);
            treeView1.Nodes.Add(obj.node.ToString(), obj.name);

            // skeneen default plane, jonka päälle voi kasailla muut objektit. tän voi tietty poistaa sit myös.
            int res = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, "/models/xz.scene.xml", 0);
            Util.LoadResourcesFromDisk();
            int node = h3d.addNodes(h3d.H3DRootNode, res);
            h3d.setNodeTransform(node, 0, 0, 0f, 0, 0, 0, 100, 100, 100);
            obj = new Obj();
            obj.fileName = Settings.ContentDir + "/models/xz.scene.xml";
            obj.name = "-Default Plane <>";
            obj.res = res;
            obj.node = node;
            obj.pos = Vector3.Zero;
            obj.rot = Vector3.Zero;
            obj.scale = new Vector3(100, 100, 100);
            Scene.Objs.Add(obj);
            treeView1.Nodes.Add(obj.node.ToString(), obj.name);

            // luo default valo
            Light l = new Light();
            l.Create(h3d.H3DRootNode, new Vector3(0, 10, 0), new Vector3(-90, 0, 0));
            obj = new Obj();
            obj.fileName = "";
            obj.name = "-Default Light *";
            obj.node = l.Node;
            obj.pos = new Vector3(0, 10, 0);
            obj.rot = new Vector3(-90, 0, 0);
            Scene.Objs.Add(obj);
            treeView1.Nodes.Add(l.Node.ToString(), obj.name);
        }

        /// <summary>
        /// mode:  1=top  2=front  3=right
        /// </summary>
        /// <param name="mode"></param>
        void SetCamera(int mode)
        {
            switch (mode)
            {
                case 1: // top
                    BaseGame.Camera.Position = cameraTopPos;
                    BaseGame.Camera.Rotation = cameraTopRot;
                    break;
                case 2: // front
                    BaseGame.Camera.Position = cameraFrontPos;
                    BaseGame.Camera.Rotation = cameraFrontRot;
                    break;
                case 3: // right
                    BaseGame.Camera.Position = cameraRightPos;
                    BaseGame.Camera.Rotation = cameraRightRot;
                    break;
            }

        }

        /// <summary>
        /// palauttaa treeview1 listasta valitun objektin
        /// </summary>
        Obj FindSelectedNodeInScene()
        {
            if (treeView1.SelectedNode == null) return null;

            if (treeView1.SelectedNode.Text.ToLower().Contains("camera"))
            {
                foreach (Obj o in Scene.Objs)
                    if (o.name == treeView1.SelectedNode.Text)
                        return o;
            }
            else
                foreach (Obj o in Scene.Objs)
                    if (o.node.ToString() == treeView1.SelectedNode.Name)
                        return o;

            return null;
        }

        #region RENDER
        void Render()
        {
            if (!loaded) return;
            glControl1.MakeCurrent();
            Input();

            // suorita skripti jos asetettu. animointi näkyy vain valitulla objektilla
            if (curObj != null && curObj.script != null && treeView1.SelectedNode != null)
            {
                curObj.script.Run("Update", new object[] { curObj });
                ObjectToParameters();
            }

            foreach (Obj o in Scene.Objs)
            {
                if (o.name.Contains("particle"))
                {
                    // Animate particle system                                
                    int cnt = h3d.findNodes(o.node, "", (int)h3d.H3DNodeTypes.Emitter);

                    h3d.setNodeTransform(o.node, o.pos.X, o.pos.Y, o.pos.Z, // pos
                                                o.rot.X, o.rot.Y, o.rot.Z, // rot
                                                o.scale.X, o.scale.Y, o.scale.Z); // scale
                    for (int i = 0; i < cnt; i++)
                        h3d.updateEmitter(h3d.getNodeFindResult(i), 1f / 60f);
                }
            }

            BaseGame.Camera.Update();
            h3d.render(BaseGame.Camera.Node);
            h3d.finalizeFrame();
            h3d.clearOverlays();

            GL.PushMatrix();
            {
                GL.PushAttrib(AttribMask.AllAttribBits);
                GL.UseProgram(0);
                float[] matproj = new float[16];
                h3d.getCameraProjMat(BaseGame.Camera.Node, matproj);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(matproj);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.Rotate(-BaseGame.Camera.Rotation.X, 1, 0, 0);
                GL.Rotate(-BaseGame.Camera.Rotation.Y, 0, 1, 0);
                GL.Rotate(-BaseGame.Camera.Rotation.Z, 0, 0, 1);
                GL.Translate(-BaseGame.Camera.Position);
                GL.Begin(BeginMode.Lines);
                {
                    GL.Color4(1f, 0, 0, 1f); // red
                    GL.Vertex3(-100, 0, 0);
                    GL.Vertex3(100, 0, 0);
                    GL.Color4(0, 0, 1f, 1f); // blue
                    GL.Vertex3(0, -100, 0);
                    GL.Vertex3(0, 100, 0);
                    GL.Color4(0, 1f, 0, 1f); // green
                    GL.Vertex3(0, 0, -100);
                    GL.Vertex3(0, 0, 100);
                    // jos grid enabled
                    if (checkBox11.Checked)
                    {
                        GL.Color4(0.5f, 0.5f, 0.5f, 1f); // harmaa grid
                        for (float z = 0; z < 100; z += (float)numericUpDown21.Value)
                        {
                            GL.Vertex3(-100, 0, z);
                            GL.Vertex3(100, 0, z);
                            GL.Vertex3(-100, 0, -z);
                            GL.Vertex3(100, 0, -z);
                        }

                        for (float x = 0; x < 100; x += (float)numericUpDown20.Value)
                        {
                            GL.Vertex3(x, 0, -100);
                            GL.Vertex3(x, 0, 100);
                            GL.Vertex3(-x, 0, -100);
                            GL.Vertex3(-x, 0, 100);
                        }
                    }
                    GL.Color4(1f, 1f, 1f, 1f);

                    if (selectedNode != -1)
                    {
                        if (lockAxis == 1) // x
                        {
                            GL.Vertex3(-100, curObj.pos.Y, curObj.pos.Z);
                            GL.Vertex3(100, curObj.pos.Y, curObj.pos.Z);
                        }
                        if (lockAxis == 2) // y
                        {
                            GL.Vertex3(curObj.pos.X, -100, curObj.pos.Z);
                            GL.Vertex3(curObj.pos.X, 100, curObj.pos.Z);
                        }
                        if (lockAxis == 3) // z
                        {
                            GL.Vertex3(curObj.pos.X, curObj.pos.Y, -100);
                            GL.Vertex3(curObj.pos.X, curObj.pos.Y, 100);
                        }
                    }
                }
                GL.End();

                if (treeView1.SelectedNode != null)
                {
                    float minX, minY, minZ, maxX, maxY, maxZ;
                    h3d.getNodeAABB(int.Parse(treeView1.SelectedNode.Name), out minX, out minY, out minZ, out maxX, out maxY, out maxZ);
                    GL.Color4(0.7f, 0.7f, 0.7f, 1f);
                    GL.Begin(BeginMode.LineStrip);
                    GL.Vertex3(minX, minY, minZ);
                    GL.Vertex3(maxX, minY, minZ);
                    GL.Vertex3(maxX, maxY, minZ);
                    GL.Vertex3(minX, maxY, minZ);
                    GL.Vertex3(minX, minY, minZ);
                    GL.Vertex3(minX, minY, maxZ);
                    GL.Vertex3(maxX, minY, maxZ);
                    GL.Vertex3(maxX, maxY, maxZ);
                    GL.Vertex3(minX, maxY, maxZ);
                    GL.Vertex3(minX, minY, maxZ);
                    GL.End();
                    GL.Begin(BeginMode.Lines);
                    GL.Vertex3(minX, maxY, minZ);
                    GL.Vertex3(minX, maxY, maxZ);
                    GL.Vertex3(maxX, minY, minZ);
                    GL.Vertex3(maxX, minY, maxZ);
                    GL.Vertex3(maxX, maxY, minZ);
                    GL.Vertex3(maxX, maxY, maxZ);
                    GL.End();
                }

                foreach (Obj o in Scene.Objs)
                {
                    if (o.name.ToLower().Contains("camera"))
                        imgCam.RenderBillboard(o.pos, 0, 1, true);

                    if (o.name.ToLower().Contains("light"))
                        imgLight.RenderBillboard(o.pos, 0, 1, true);

                    if (o.name.ToLower().Contains("audio"))
                        imgSound.RenderBillboard(o.pos, 0, 1, true);
                }
                GL.PopAttrib();
            }
            GL.PopMatrix();

            glControl1.SwapBuffers();
        }
        #endregion

        #region INPUT
        void Input()
        {
            if (Keyboard.IsKeyPressed(Keys.Escape))
            {
                mode = lockAxis = 0;
                // jos objekti ei ole scene obj listassa, poista se (hiirikursorista)
                if (treeView1.Nodes.ContainsKey(selectedNode.ToString()) == false) RemoveSelection();
                else
                {
                    // obj on scene obj listassa, niin otetaan originaali takas koska esc on cancel
                    if (curObj != null)
                    {
                        Scene.RestoreOrig(ref curObj);
                        SetTransform();
                        selectedNode = -1;
                    }
                }
                treeView2.SelectedNode = null;
                toolStripStatusLabel1.Text = "";
            }
            if (mouseRightPressed)
            {
                float mul = 1;
                if (Keyboard.IsKeyPressed(Keys.ShiftKey)) mul = 5;
                if (Keyboard.IsKeyPressed(Keys.W)) BaseGame.Camera.Move(BaseGame.Camera.Speed * mul);
                if (Keyboard.IsKeyPressed(Keys.S)) BaseGame.Camera.Move(-BaseGame.Camera.Speed * mul);
                if (Keyboard.IsKeyPressed(Keys.A)) BaseGame.Camera.Strafe(-BaseGame.Camera.Speed * mul);
                if (Keyboard.IsKeyPressed(Keys.D)) BaseGame.Camera.Strafe(BaseGame.Camera.Speed * mul);
            }
            else
            {
                if (curObj != null && curObj.name.Contains("Default Plane")) return;

                // jos muokataan jo asetettua objektia, otetaan originaali talteen siltä varalta että painetaan esc (joka peruu muutokset)
                if (treeView1.SelectedNode != null)
                    if (Keyboard.IsKeyPressed(Keys.G) ||
                        Keyboard.IsKeyPressed(Keys.R) ||
                        Keyboard.IsKeyPressed(Keys.S))
                    {
                        moving = true; // muokataan jo asetettua
                        if (Scene.FindOrig(curObj.name) == null) Scene.AddOrig(curObj);
                        curObj = FindSelectedNodeInScene();
                        selectedNode = curObj.node;
                    }

                if (Keyboard.IsKeyPressed(Keys.G)) mode = 0; // move with mouse
                if (Keyboard.IsKeyPressed(Keys.R)) mode = 1;  // rotation
                if (Keyboard.IsKeyPressed(Keys.S)) mode = 2;  // scale
                if (Keyboard.IsKeyPressed(Keys.X)) lockAxis = 1; // lock X
                if (Keyboard.IsKeyPressed(Keys.Y)) lockAxis = 2; // lock Y
                if (Keyboard.IsKeyPressed(Keys.Z)) lockAxis = 3; // lock Z

                // blender näppäimet: numpadin 7, 1 ja 3 vaihtaa kameran paikkaa top,front,right. 
                if (Keyboard.IsKeyPressed(Keys.NumPad7))
                {
                    BaseGame.Camera.Position = cameraTopPos;
                    BaseGame.Camera.Rotation = cameraTopRot;
                }
                if (Keyboard.IsKeyPressed(Keys.NumPad1))
                {
                    BaseGame.Camera.Position = cameraFrontPos;
                    BaseGame.Camera.Rotation = cameraFrontRot;
                }
                if (Keyboard.IsKeyPressed(Keys.NumPad3))
                {
                    BaseGame.Camera.Position = cameraRightPos;
                    BaseGame.Camera.Rotation = cameraRightRot;
                }
            }

        }
        #endregion

    }
}
