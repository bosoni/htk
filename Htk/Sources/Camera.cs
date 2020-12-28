/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;

namespace Htk
{
    public class Camera : Movable
    {
        public int Pipeline;
        public float Near = 0.1f, Far = 10000, Fov = 45;
        public float Speed = 0.005f;
        public float DeltaX = 0, DeltaY = 0;
        Model skybox;

        /// <summary>
        /// pipelineName can be:
        ///    forward
        ///    deferred
        ///    hdr
        /// </summary>
        public string Create(int parent, Vector3 position, Vector3 rotation, string pipelineName = "forward")
        {
            Position = position;
            Rotation = rotation;

            name = "Camera" + _count++;
            Pipeline = h3d.addResource((int)h3d.H3DResTypes.Pipeline, "pipelines/" + pipelineName + ".pipeline.xml", 0);
            Node = h3d.addCameraNode(h3d.H3DRootNode, name, Pipeline);
            return name;
        }

        public void SetSkyBox(string name)
        {
            skybox = Model.Load(name);
            skybox.Scale = new Vector3(100, 100, 100);
            skybox.Update();
        }

        public override void Update()
        {
            if (skybox != null)
            {
                skybox.Position = Position;
                skybox.Update();
            }

            base.Update();
        }
    }
}
