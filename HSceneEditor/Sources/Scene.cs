using System.Collections.Generic;
using OpenTK;
using Htk;

namespace HSceneEditor
{
    public class Obj
    {
        public string name, fileName, scriptName;
        public int res, node;
        public Vector3 pos, rot, scale, color;
        public bool visible = true, block = true;
        public float mass = 0;
        public Script script;

        void Init()
        {
            name = "";
            fileName = "";
            scriptName = "";
            res = node = -1;
            pos = Vector3.Zero;
            rot = Vector3.Zero;
            scale = Vector3.One;
            color = new Vector3(255, 255, 255);
            visible = true;
            block = true;
            script = null;
            mass = 0;
        }

        public Obj()
        {
            Init();
        }
        public Obj(float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz, float r, float g, float b)
        {
            Init();
            pos = new Vector3(x, y, z);
            rot = new Vector3(rx, ry, rz);
            scale = new Vector3(sx, sy, sz);
            color = new Vector3(r, g, b);
        }
    }

    public static class Scene
    {
        public static List<Obj> Objs = new List<Obj>();
        public static List<Obj> OrigObjs = new List<Obj>();

        public static Obj FindOrig(string name)
        {
            foreach (Obj orig in OrigObjs)
                if (orig.name == name)
                    return orig;
            return null;
        }

        public static void RemoveOrig(Obj o)
        {
            while (true)
            {
                Obj ob = FindOrig(o.name);
                if (ob == null) break;
                OrigObjs.Remove(ob);
            }
        }

        /// <summary>
        /// otetaan originaalilistasta alkup arvot ja laitetaan ne curObj:iin
        /// </summary>
        /// <param name="cur"></param>
        public static void RestoreOrig(ref Obj cur)
        {
            Obj orig = FindOrig(cur.name);
            if (orig == null) return;

            cur.name = orig.name;
            cur.fileName = orig.fileName;
            cur.res = orig.res;
            cur.node = orig.node;
            cur.pos = orig.pos;
            cur.rot = orig.rot;
            cur.scale = orig.scale;
            cur.color = orig.color;
            cur.visible = orig.visible;
            cur.block = orig.block;
            cur.mass = orig.mass;
            OrigObjs.Remove(orig);
        }

        /// <summary>
        /// luo kopio, lisää se originaalilistaan. jos listassa on jo samanniminen objekti, korvataan se
        /// </summary>
        /// <param name="o"></param>
        public static void AddOrig(Obj o)
        {
            Obj newo = FindOrig(o.name);
            if (newo == null)
                newo = new Obj();
            else OrigObjs.Remove(newo);

            newo.name = o.name;
            newo.fileName = o.fileName;
            newo.res = o.res;
            newo.node = o.node;
            newo.pos = o.pos;
            newo.rot = o.rot;
            newo.scale = o.scale;
            newo.color = o.color;
            newo.visible = o.visible;
            newo.block = o.block;
            newo.mass = o.mass;
            OrigObjs.Add(newo);
        }
    }
}
