/*
 * HSceneEditor (c) mjt[matola@sci.fi], 2011-2013
 * aloitus: 10.12.11
 * 
 * 
 */

/*
 * TODO:
 * * load&save systeemiä pitää muuttaa että hierarkia-hommat toimii (koska scene-listassa voi drag'n'dropata)
 *   ---pitää suunnitella file-format että tukee hierarkiat
 * * kun treeviewiä muutettu ja poistaa yhden noden jolla childejä, niitä childerjä ei kumminkaa poisteta skenestä. 
 * 
 * * objektien värit voi muuttaa (colordialog tehty, ne arvot pitäis laittaa sit shaderille)
 *   
 * 
 * 
 * BUGS:
 * * logissa PALJON invalid handlea
 * * mrsniper taloa ei voi valita glcontrolista miks??  ehkei muitakaa sketchup modeleit?
 * * ku laittaa toisen valon, se ei liiku vaikka valo imagea liikuttaa.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        bool loaded = false;
        int selectedRes = -1, selectedNode = -1;
        static Obj curObj = new Obj();
        static bool moving = false;
        static int _index = 0, _camCount = 1, _lightCount = 1;
        static bool _update = true;
        static bool append = false;
        Keyboard keyb = new Keyboard();
        static Dictionary<string, string> fileInfos = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();

            {
                glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
                BaseGame.Camera.Speed = (float)numericUpDown13.Value;
            }
            Application.AddMessageFilter(keyb);

            if (Directory.Exists(Settings.ContentDir) == false) Directory.CreateDirectory(Settings.ContentDir);
            DataList_Refresh(checkBox1.Checked);
        }

        #region TOIMINTOJA

        /// <summary>
        /// scene objlistiin laitetaan objekteille nimien perään tietoja jos
        /// alkup asetuksia on muutettu, eli  [tiedot]
        ///  Scr  jos objekti käyttää scriptiä
        ///  -V   jos visible otettu pois päältä
        ///  -B   jos block otettu pois päältä
        ///  M=x  objektin massa jos asetettu (!=0.0)
        /// </summary>
        /// <returns></returns>
        string GetObjStatusStr()
        {
            string str = "  [";
            int len = str.Length;

            if (script_comboBox1.Text != "") str += "Scr";

            if (checkBox14.Checked == false)
            {
                if (str.Length != len) str += "/";
                str += "-V";
            }

            if (checkBox15.Checked == false)
            {
                if (str.Length != len) str += "/";
                str += "-B";
            }

            if ((float)numericUpDown23.Value != 0.0f)
            {
                if (str.Length != len) str += "/";
                str += " M=" + (float)numericUpDown23.Value;
            }

            if (str.Length == len) return "";

            str += "]";
            return str;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            loaded = false;
            Application.Idle -= Application_Idle;
            Clear();

            Horde3DUtils.dumpMessages();
            h3d.release();
            base.OnClosing(e);
        }

        private void ResizeLists(object sender, PaintEventArgs e)
        {
            treeView1.Size = new Size(splitContainer3.Panel1.Width, splitContainer3.Panel1.Height);
            treeView2.Size = new Size(splitContainer3.Panel2.Width, splitContainer3.Panel2.Height);
        }

        /// <summary>
        /// tarkista onko jotain tiedostoa muutettu. palauttaa true jos muokattu.
        /// tarkistetaan vain .xml, .jpg .png .dds .geo .anim .shader .glsl
        /// </summary>
        bool CheckFilesIfModified(string dir)
        {
            DirectoryInfo directory = new DirectoryInfo(dir);
            foreach (DirectoryInfo d in directory.GetDirectories()) if (CheckFilesIfModified(d.FullName) == true) return true;
            foreach (FileInfo f in directory.GetFiles())
            {
                string[] ext = new string[] { ".xml", ".jpg", ".png", ".dds", ".geo", ".anim", ".shader", ".glsl" };
                for (int c = 0; c < ext.Length; c++)
                    if (f.Name.Contains(ext[c]) == true)
                    {
                        string val;
                        if (fileInfos.TryGetValue(f.FullName, out val))
                        {
                            if (val != f.LastWriteTime.ToString()) // eri aika, muokattu!
                            {
                                return true;
                            }
                        }
                    }
            }
            return false;
        }

        /// <summary>
        /// värin valinta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Click(object sender, MouseEventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panel1.BackColor = colorDialog1.Color;
                if (curObj != null)
                    curObj.color = new Vector3(colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);
            }

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            // jos joku tiedosto muuttunut
            if (CheckFilesIfModified(Settings.ContentDir) == true)
            {
                Vector3 pos = BaseGame.Camera.Position;
                Vector3 rot = BaseGame.Camera.Rotation;
                Save("_tmp_.hse");
                newToolStripMenuItem_Click(null, null);
                Open("_tmp_.hse");
                BaseGame.Camera.Position = pos;
                BaseGame.Camera.Rotation = rot;
            }
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                Render();
            }
        }

        void RemoveSelection()
        {
            if (selectedNode == -1) return;
            h3d.removeNode(selectedNode);
            selectedNode = -1;
        }

        /// <summary>
        /// force Y
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown22_ValueChanged(object sender, EventArgs e)
        {
            if (checkBox12.Checked) // jos pakotetaan Y, aseta se arvo pos.Y:hyn
                numericUpDown2.Value = numericUpDown22.Value;
        }

        /// <summary>
        /// visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            curObj.visible = checkBox14.Checked;
        }

        /// <summary>
        /// block
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            curObj.block = checkBox15.Checked;
        }

        /// <summary>
        /// script valikko
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_Selected(object sender, EventArgs e)
        {
            if (curObj != null)
            {
                if ((string)script_comboBox1.SelectedItem == "")
                {
                    if (curObj.script != null) // jos otetaan scripti pois käytöstä
                    {
                        script_comboBox1.Text = "";
                        curObj.scriptName = "";
                        curObj.script = null;

                        // palauta orig
                        Scene.RestoreOrig(ref curObj);
                        SetTransform();
                    }
                }
                else
                {
                    curObj.scriptName = (string)script_comboBox1.SelectedItem;
                    PrepareScript(curObj.scriptName); // lataa scripti
                    Scene.AddOrig(curObj);
                }
            }
        }

        void PrepareScript(string scriptName)
        {
            if (scriptName == "" || scriptName == null) return;
            if (curObj.script == null) curObj.script = new Script();
            curObj.script.Load(Settings.ContentDir + "/scripts/" + scriptName);
        }

        /// <summary>
        /// scene objectlist refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label1_Click(object sender, EventArgs e)
        {
            Vector3 pos = BaseGame.Camera.Position;
            Vector3 rot = BaseGame.Camera.Rotation;
            Save("_tmp_.hse");
            newToolStripMenuItem_Click(null, null);
            Open("_tmp_.hse");
            BaseGame.Camera.Position = pos;
            BaseGame.Camera.Rotation = rot;
        }

        #endregion

        #region TREEVIEW_1
        void treeview1_ObjectSelected()
        {
            mode = lockAxis = 0;
            toolStripStatusLabel1.Text = "You can modify object's parameters. Drag'n'drop object to make it child of other object. G=Move, R=Rotate, S=Scale.";
            RemoveSelection();
            treeView2.SelectedNode = null;
            curObj = FindSelectedNodeInScene();

            // arvot näkyville
            if (curObj != null)
            {
                ObjectToParameters();
            }
        }

        /// <summary>
        /// sceneobject list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeview1_ObjectSelected();

        }

        /// <summary>
        /// tuplaklikkaamalla kamera siirretään objektin läheisyyteen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            curObj = FindSelectedNodeInScene();
            if (curObj != null)
            {
                BaseGame.Camera.Position = new OpenTK.Vector3(curObj.pos.X, curObj.pos.Y + 1, curObj.pos.Z + 5);

                if (curObj.name.Contains("camera") || curObj.name.Contains("Camera"))
                    BaseGame.Camera.Rotation = curObj.rot;
                else
                    BaseGame.Camera.Rotation = cameraFrontRot;
            }
        }

        private void scene_KeyUp(object sender, KeyEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (e.KeyData == Keys.Delete)
                {
                    if (treeView1.SelectedNode.Text.ToLower().Contains("default camera"))
                    {
                        MessageBox.Show("You can't delete the Default Camera.", "Info");
                        return;
                    }
                    Obj o = FindSelectedNodeInScene();
                    h3d.removeNode(o.node);
                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                    Scene.Objs.Remove(o);

                    // todo pitää poistaa noden childit myös oikein jos semmoisia on
                }
                else if (e.KeyData == Keys.F2) // rename
                {
                    treeView1.SelectedNode.BeginEdit();
                }
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Move the dragged node when the left mouse button is used.
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }

            // Copy the dragged node when the right mouse button is used.
            else if (e.Button == MouseButtons.Right)
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        // Set the target drop effect to the effect 
        // specified in the ItemDrag event handler.
        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        // Select the node under the mouse pointer to indicate the 
        // expected drop location.
        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.
            treeView1.SelectedNode = treeView1.GetNodeAt(targetPoint);
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = treeView1.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node that was dragged.
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Retrieve the node at the drop location.
            TreeNode targetNode = treeView1.GetNodeAt(targetPoint);

            // Confirm that the node at the drop location is not 
            // the dragged node or a descendant of the dragged node.
            if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
            {
                // If it is a move operation, remove the node from its current 
                // location and add it to the node at the drop location.
                if (e.Effect == DragDropEffects.Move)
                {
                    draggedNode.Remove();

                    if (targetNode == null) treeView1.Nodes.Add(draggedNode);
                    else
                        targetNode.Nodes.Add(draggedNode);

                    // TODO // hierarkia pitää muuttaa
                }

                // If it is a copy operation, clone the dragged node 
                // and add it to the node at the drop location.
                else if (e.Effect == DragDropEffects.Copy)
                {
                    if (targetNode == null) treeView1.Nodes.Add((TreeNode)draggedNode.Clone());
                    else
                        targetNode.Nodes.Add((TreeNode)draggedNode.Clone());

                    // TODO // kloonataan myös objekti joka nodeissa on
                }

                // Expand the node at the location 
                // to show the dropped node.
                if (targetNode != null) targetNode.Expand();
            }
        }

        // Determine whether one node is a parent 
        // or ancestor of a second node.
        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            if (node1 == null || node2 == null) return false;
            // Check the parent node of the second node.
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node, 
            // call the ContainsNode method recursively using the parent of 
            // the second node.
            return ContainsNode(node1, node2.Parent);
        }
        #endregion

        #region TREEVIEW_2
        /// <summary>
        /// data refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label2_Click(object sender, EventArgs e)
        {
            DataList_Refresh(checkBox1.Checked);
        }

        /// <summary>
        /// data listasta valinta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            mode = lockAxis = 0;
            label3.Text = "Parameters";
            script_comboBox1.Text = "";
            RemoveSelection();
            moving = false;

            if (treeView2.SelectedNode.Text.ToLower().Contains(".scene.xml"))
            {
                toolStripStatusLabel1.Text = treeView2.SelectedNode.Text + " selected.";
                string path = treeView2.SelectedNode.FullPath.Substring(Settings.ContentDir.Length);
                selectedRes = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, path, 0);
                Util.LoadResourcesFromDisk();

                selectedNode = h3d.addNodes(h3d.H3DRootNode, selectedRes);

                if (treeView2.SelectedNode.Text.ToLower().Contains("skybox"))
                {
                    h3d.setNodeTransform(selectedNode, 0, 0, 0, 0, 0, 0, 1000, 1000, 1000);
                    curObj = new Obj(0, 0, 0, 0, 0, 0, 1000, 1000, 1000, 255,255,255);
                    curObj.node = selectedNode;
                    curObj.fileName = treeView2.SelectedNode.FullPath;
                    curObj.name = treeView2.SelectedNode.Text;
                    Scene.Objs.Add(curObj);
                    treeView1.Nodes.Insert(0, curObj.node.ToString(), curObj.name);
                    selectedNode = -1;
                    toolStripStatusLabel1.Text = "Skybox added.";
                    MessageBox.Show("Skybox added.");
                }
                else
                {
                    curObj = new Obj();
                    treeView1.SelectedNode = null;
                    ObjectToParameters();
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "Double-click will open selected object at your default program.";
                selectedNode = -1;
            }
        }

        /// <summary>
        /// avaa tiedoston määrätyllä ohjelmalla, esim .blend blenderillä, .txt notepadilla..
        /// </summary>
        private void DataList_OpenObject(object sender, EventArgs e)
        {
            string dir = Directory.GetCurrentDirectory() + "/";
            if (Settings.ContentDir[0] == '/') dir = dir.Substring(0, 3); // ota kovalevytunnus
            if (treeView2.SelectedNode != null)
                Process.Start(dir + treeView2.SelectedNode.FullPath);
        }

        void DataList_Refresh(bool showOnlyScenes)
        {
            fileInfos.Clear();
            treeView2.Nodes.Clear();
            treeView2.Nodes.Add(Settings.ContentDir);
            script_comboBox1.Items.Clear(); // skriptit
            script_comboBox1.Items.Add("");
            AddFiles(Settings.ContentDir, treeView2.Nodes[0], showOnlyScenes, textBox1.Text);
            treeView2.ExpandAll();
        }

        // +
        private void label4_Click(object sender, EventArgs e)
        {
            treeView2.ExpandAll();
        }
        // -
        private void label5_Click(object sender, EventArgs e)
        {
            treeView2.CollapseAll();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DataList_Refresh(checkBox1.Checked);
        }

        /// <summary>
        /// objektilistan search, filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataList_Refresh(checkBox1.Checked);
        }

        /// <summary>
        /// laita treeviewiin data -hakemistossa olevat tiedostot (joista löytyy filter)
        /// </summary>
        void AddFiles(string dir, TreeNode node, bool showOnlyScenes, string filter)
        {
            DirectoryInfo directory = new DirectoryInfo(dir);
            foreach (DirectoryInfo d in directory.GetDirectories())
            {
                TreeNode t;

                if (showOnlyScenes == false || d.Name == "models")
                    t = new TreeNode(d.Name);
                else t = node;

                AddFiles(d.FullName, t, showOnlyScenes, filter);
                if (showOnlyScenes == false || d.Name == "models")
                    node.Nodes.Add(t);
            }
            foreach (FileInfo f in directory.GetFiles())
            {
                TreeNode t = new TreeNode(f.Name);
                fileInfos.Add(f.FullName, f.LastWriteTime.ToString());
                if (f.FullName.Contains(".cs")) // cs script
                    script_comboBox1.Items.Add((string)f.Name);

                if (showOnlyScenes == false || t.Text.ToLower().Contains(".scene.xml"))
                    if (t.Text.Contains(filter)) node.Nodes.Add(t);
            }
        }

        #endregion

        #region NUMERIC CONTROLS
        void ObjectToParameters()
        {
            _update = false; // seuraavat kutsuu numericUpDown*_ValueChanged metodeita, niin ei suoriteta updatetransformia vielä
            numericUpDown1.Value = (decimal)curObj.pos.X;
            numericUpDown2.Value = (decimal)curObj.pos.Y;
            numericUpDown3.Value = (decimal)curObj.pos.Z;
            numericUpDown4.Value = (decimal)curObj.rot.X;
            numericUpDown5.Value = (decimal)curObj.rot.Y;
            numericUpDown6.Value = (decimal)curObj.rot.Z;
            numericUpDown7.Value = (decimal)curObj.scale.X;
            numericUpDown8.Value = (decimal)curObj.scale.Y;
            numericUpDown9.Value = (decimal)curObj.scale.Z;
            _update = true; // nyt kun arvot on asetettu, suoritetaan update
            SetTransform();
            if (treeView1.SelectedNode != null) label3.Text = "Parameters: " + treeView1.SelectedNode.Text;

            if (script_comboBox1.Focused == false)
            {
                script_comboBox1.Text = curObj.scriptName;
            }

            checkBox14.Checked=curObj.visible;
            checkBox15.Checked=curObj.block;
            numericUpDown23.Value=(Decimal)curObj.mass;

            panel1.BackColor = colorDialog1.Color = Color.FromArgb(255, (int)curObj.color.X, (int)curObj.color.Y, (int)curObj.color.Z);
            
        }

        /// <summary>
        /// reset parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            _update = false; // seuraavat kutsuu numericUpDown*_ValueChanged metodeita, niin ei suoriteta updatetransformia vielä
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            numericUpDown4.Value = 0;
            numericUpDown5.Value = 0;
            numericUpDown6.Value = 0;
            numericUpDown7.Value = 1;
            numericUpDown8.Value = 1;
            numericUpDown9.Value = 1;
            _update = true; // nyt kun arvot on asetettu, suoritetaan update
            SetTransform();
            label3.Text = "Parameters";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            curObj.pos.X = (float)numericUpDown1.Value;
            SetTransform();
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown22.Value = numericUpDown2.Value;
            curObj.pos.Y = (float)numericUpDown2.Value;
            SetTransform();
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            curObj.pos.Z = (float)numericUpDown3.Value;
            SetTransform();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            curObj.rot.X = (float)numericUpDown4.Value;
            SetTransform();
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            curObj.rot.Y = (float)numericUpDown5.Value;
            SetTransform();
        }
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            curObj.rot.Z = (float)numericUpDown6.Value;
            SetTransform();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            curObj.scale.X = (float)numericUpDown7.Value;
            SetTransform();
        }
        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            curObj.scale.Y = (float)numericUpDown8.Value;
            SetTransform();
        }
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            curObj.scale.Z = (float)numericUpDown9.Value;
            SetTransform();
        }

        // camera speed
        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            BaseGame.Camera.Speed = (float)numericUpDown13.Value;
        }

        // position step
        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Increment = numericUpDown14.Value;
            numericUpDown2.Increment = numericUpDown14.Value;
            numericUpDown3.Increment = numericUpDown14.Value;
        }
        // rotation step
        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown4.Increment = numericUpDown15.Value;
            numericUpDown5.Increment = numericUpDown15.Value;
            numericUpDown6.Increment = numericUpDown15.Value;
        }
        // scale step
        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown7.Increment = numericUpDown16.Value;
            numericUpDown8.Increment = numericUpDown16.Value;
            numericUpDown9.Increment = numericUpDown16.Value;
        }
        #endregion

        #region MENU
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("HSceneEditor v0.2\n\n(c) mjt, 2011-2013 [matola@sci.fi]", "About");
        }
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Keys:\nG move, R rotate, S scale\nX,Y,Z locks axis\nESC cancel\n\n" +
                    "Right mouse button with A,S,W,D moves the camera.\n" +
                    "Middle button moves camera up/down." +
                    "Wheel moves camera forward/backward.\nCTRL+Wheel scales object.", "Help");
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = lockAxis = 0;
            fileInfos.Clear();
            _index = 0;
            _lightCount = _camCount = 1;
            Clear();
            CreateDefaultObjects();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK && openFileDialog1.FileName != "")
            {
                Open(openFileDialog1.FileName);
            }
        }

        private void appendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            append = true;
            openToolStripMenuItem1_Click(sender, e);
            append = false;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK && saveFileDialog1.FileName != "")
            {
                Save(saveFileDialog1.FileName);
            }
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCamera(1);
        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCamera(2);
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCamera(3);
        }

        /// <summary>
        /// lisää kamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            curObj = new Obj();
            curObj.name = "-Camera_" + (_camCount++) + " <)";
            curObj.node = BaseGame.Camera.Node;
            curObj.pos = BaseGame.Camera.Position;
            curObj.rot = BaseGame.Camera.Rotation;
            Scene.Objs.Add(curObj);
            treeView1.Nodes.Insert(0, curObj.node.ToString(), curObj.name);
            treeView1.SelectedNode = treeView1.Nodes[0];
            ObjectToParameters();
        }

        /// <summary>
        /// set current camera pos&rot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null && (treeView1.SelectedNode.Text.ToLower().Contains("camera")))
            {
                curObj = FindSelectedNodeInScene();
                if (curObj != null)
                {
                    curObj.pos = BaseGame.Camera.Position;
                    curObj.rot = BaseGame.Camera.Rotation;
                    ObjectToParameters();
                }
            }
            else MessageBox.Show("No camera selected ('Camera' must exist in its name).", "Info");
        }

        /// <summary>
        /// luodaan valo ja lisätään se skeneen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Light l = new Light();
            l.Create(h3d.H3DRootNode, new Vector3(0, 9, 0), new Vector3(-90, 0, 0));
            curObj = new Obj();
            curObj.fileName = "";
            curObj.name = "-Light_" + (_lightCount++) + " *";
            curObj.node = l.Node;
            curObj.pos = new Vector3(0, 9, 0);
            curObj.rot = new Vector3(-90, 0, 0);
            Scene.Objs.Add(curObj);
            treeView1.Nodes.Insert(0, l.Node.ToString(), curObj.name);
            toolStripStatusLabel1.Text = "Choose light from scene-list and set it to the right position. Hint: use 'Force Y' when moving.";
        }


        #endregion

        #region RANDOMIZE
        /// <summary>
        /// randomize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            // laske random alueet
            float posr = (float)(numericUpDown11.Value - numericUpDown10.Value); // max-min
            float rotr = (float)(numericUpDown17.Value - numericUpDown12.Value); // max-min
            float scaler = (float)(numericUpDown19.Value - numericUpDown18.Value); // max-min

            // tsekataan mitkä parametrit korvataan random-luvulla

            //pos
            if (checkBox2.Checked) numericUpDown1.Value = numericUpDown10.Value + (decimal)(posr * BaseGame.rnd.NextDouble());
            if (checkBox3.Checked) numericUpDown2.Value = numericUpDown10.Value + (decimal)(posr * BaseGame.rnd.NextDouble());
            if (checkBox4.Checked) numericUpDown3.Value = numericUpDown10.Value + (decimal)(posr * BaseGame.rnd.NextDouble());

            // rot
            if (checkBox5.Checked) numericUpDown4.Value = numericUpDown12.Value + (decimal)(rotr * BaseGame.rnd.NextDouble());
            if (checkBox6.Checked) numericUpDown5.Value = numericUpDown12.Value + (decimal)(rotr * BaseGame.rnd.NextDouble());
            if (checkBox7.Checked) numericUpDown6.Value = numericUpDown12.Value + (decimal)(rotr * BaseGame.rnd.NextDouble());

            // scale
            if (checkBox8.Checked) numericUpDown7.Value = numericUpDown18.Value + (decimal)(scaler * BaseGame.rnd.NextDouble());
            if (checkBox9.Checked) numericUpDown8.Value = numericUpDown18.Value + (decimal)(scaler * BaseGame.rnd.NextDouble());
            if (checkBox10.Checked) numericUpDown9.Value = numericUpDown18.Value + (decimal)(scaler * BaseGame.rnd.NextDouble());
        }

        /// <summary>
        /// clear (random täpit)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;
            checkBox8.Checked = false;
            checkBox9.Checked = false;
            checkBox10.Checked = false;
        }

        /// <summary>
        /// tyhjennä skene, putsaa kaikki
        /// </summary>
        void Clear()
        {
            foreach (Obj o in Scene.Objs)
            {
                h3d.removeNode(o.node);
                h3d.removeResource(o.res);
            }
            Scene.Objs.Clear();
            Scene.OrigObjs.Clear();
            treeView1.Nodes.Clear();
            selectedNode = -1;
            treeView1.SelectedNode = treeView2.SelectedNode = null;
            h3d.clear();
        }
        #endregion

    }
}
