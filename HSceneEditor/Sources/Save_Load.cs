/*
 * .hsc Fileformat:
 * 
 * int num of objects 
 * {
 *   string filename 
 *   string name
 *   string scriptname
 *   bool visible
 *   bool block
 *   vector3 pos
 *   vector3 rot
 *   vector3 scale
 *   vector3 color    
 *   float mass
 * }
 * float kameran nopeus
 * 
 */

// todo uus fileformat hierarkioineen

/*
 * jos objektiin lisätään  
 *    string parent
 *    
 * ni sillo ainaki childi tietää parentin.
 * tuon nimen saa kun tallentaa ja tsekataan joka
 * objekti  että onko se treeview1:ssä rootti vai onko sillä parent.
 * 
 * ladatessa sitten treeviewistä etitään parent ja pistetää childnode sille.
 * 
 * 
 * ressit pysyy muistissa, mutta scenegraph pitänee luoda joka kerta ku
 * treeviewiä muutellaan. 
 * kun lisätään obuja, ne menee roottiin, ok.
 * 
 * kun hiiren vasemmalla siirretään node paikasta A paikkaan B, pittää..
 * n1   B 
 * n2
 * n3   A
 *    ->>
 *       n1
 *        \n3
 *       n2
 * 
 * (nodea ei vissii voi "kiinnittää" toiseen nodeen, vaan pitää käyttää ressiä)
 * mistä: n3 joten se poistetaan, h3d.removeNode( n3.nodeID )
 * mihin: n1 joten h3d.addNodes( n1.nodeID, n3.res );
 * 
 * ei kai se ton kummempaa oo. kunhan noi id:t ja ressit o talles ni kaik pitäis mennä ok..?
 * 
 * 
 * 
 * 
 * */

using System.IO;
using System.Windows.Forms;
using Horde3DNET;
using Horde3DNET.Utils;
using Htk;
using OpenTK;

namespace HSceneEditor
{
    public partial class Form1 : Form
    {
        public void Open(string filename)
        {
            if (append == false) Clear();

            using (TextReader tr = new StreamReader(filename))
            {
                string[] data = tr.ReadToEnd().Split('\n');
                int c = 0;
                int objs = int.Parse(data[c++]);
                for (int q = 0; q < objs; q++)
                {
                    curObj = new Obj();
                    curObj.fileName = data[c++];
                    curObj.name = data[c++];
                    curObj.scriptName = data[c++];
                    curObj.visible = bool.Parse(data[c++]);
                    curObj.block = bool.Parse(data[c++]);
                    curObj.pos = new Vector3(float.Parse(data[c++]), float.Parse(data[c++]), float.Parse(data[c++]));
                    curObj.rot = new Vector3(float.Parse(data[c++]), float.Parse(data[c++]), float.Parse(data[c++]));
                    curObj.scale = new Vector3(float.Parse(data[c++]), float.Parse(data[c++]), float.Parse(data[c++]));
                    curObj.color = new Vector3(float.Parse(data[c++]), float.Parse(data[c++]), float.Parse(data[c++]));
                    curObj.mass = float.Parse(data[c++]);
                    
                    if (curObj.name.Contains("light") || curObj.name.Contains("Light"))
                    {
                        Light l = new Light();
                        l.Create(h3d.H3DRootNode, curObj.pos, curObj.rot);
                        curObj.node = l.Node;
                        treeView1.Nodes.Insert(0, curObj.node.ToString(), curObj.name);
                    }
                    else if (curObj.name.Contains("camera") || curObj.name.Contains("Camera"))
                    {
                        glControl1_Resize(this, null);   // Ensure the Viewport is set up correctly
                        BaseGame.Camera.Position = curObj.pos;
                        BaseGame.Camera.Rotation = curObj.rot;

                        treeView1.Nodes.Insert(0, curObj.node.ToString(), curObj.name);
                    }
                    else if (curObj.fileName != "") // esim valoilla ja kameroilla ei ole tiedostonimeä
                    {
                        string path = curObj.fileName.Substring(Settings.ContentDir.Length);
                        int res = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, path, 0);
                        Util.LoadResourcesFromDisk();

                        curObj.node = h3d.addNodes(h3d.H3DRootNode, res);
                        selectedNode = curObj.node;
                        SetTransform(); // objekti paikoilleen

                        if (curObj.name.Contains("skybox") || curObj.name.Contains("Skybox")) treeView1.Nodes.Insert(0, curObj.node.ToString(), curObj.name);
                        else treeView1.Nodes.Add(curObj.node.ToString(), curObj.name);
                    }

                    Scene.Objs.Add(curObj);

                    if (curObj.scriptName.Contains(".cs") || curObj.scriptName.Contains(".dll"))
                    {
                        Scene.AddOrig(curObj);
                        PrepareScript(curObj.scriptName);
                    }

                }
                BaseGame.Camera.Speed = float.Parse(data[c++]);
                selectedNode = -1;

                treeView1.SelectedNode = treeView1.Nodes[0]; // valitaan joku, ei väliä mikä (ettei curObj ole null)
                curObj = FindSelectedNodeInScene();
            }
        }

        public void Save(string filename)
        {
            if (filename.Contains(".hse") == false) filename += ".hse";

            using (TextWriter tw = new StreamWriter(filename))
            {
                string data = "" + Scene.Objs.Count + "\n";

                foreach (Obj ob in Scene.Objs)
                {
                    // jos scenen orig listassa on sama obu, tallennetaan sen alkup arvot eikä skriptin muokkaamia
                    Obj o = Scene.FindOrig(ob.name);
                    if (o == null) o = ob;

                    data += ob.fileName + "\n"; // filename
                    data += ob.name + "\n"; // nimi sceneobj treeviewis
                    data += ob.scriptName + "\n"; // scriptin nimi
                    data += o.visible + "\n";
                    data += o.block + "\n";
                    data += o.pos.X.ToString() + "\n"; data += o.pos.Y.ToString() + "\n"; data += o.pos.Z.ToString() + "\n";
                    data += o.rot.X.ToString() + "\n"; data += o.rot.Y.ToString() + "\n"; data += o.rot.Z.ToString() + "\n";
                    data += o.scale.X.ToString() + "\n"; data += o.scale.Y.ToString() + "\n"; data += o.scale.Z.ToString() + "\n";
                    data += o.color.X.ToString() + "\n"; data += o.color.Y.ToString() + "\n"; data += o.color.Z.ToString() + "\n";
                    data += o.mass.ToString() + "\n";
                }
                data += BaseGame.Camera.Speed.ToString() + "\n";
                tw.WriteLine(data);
            }
        }
    }
}
