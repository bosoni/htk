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
    public class Light : Movable
    {
        static int lightCount = 0;
        static int lightMatRes;
        public Vector3 Color = new Vector3(0.8f, 0.8f, 0.8f);
        public float Radius = 200, Fov = 90;
        public int ShadowMapCountI = 3;
        public float ShadowSplitLambdaF = 0.9f, ShadowMapBiasF = 0.001f;
        public bool CreateShadow = true;

        public string Create(int parent, Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;

            lightMatRes = h3d.addResource((int)h3d.H3DResTypes.Material, "shaders/light.material.xml", 0);

            name = "Light" + lightCount++;
            Node = h3d.addLightNode(parent, name, lightMatRes, "LIGHTING", "SHADOWMAP");
            h3d.setNodeTransform(Node, position.X, position.Y, position.Z, rotation.X, rotation.Y, rotation.Z, 1, 1, 1);
            h3d.setNodeParamF(Node, (int)h3d.H3DLight.RadiusF, 0, Radius);
            h3d.setNodeParamF(Node, (int)h3d.H3DLight.FovF, 0, Fov);
            h3d.setNodeParamF(Node, (int)h3d.H3DLight.ColorF3, 0, Color.X);
            h3d.setNodeParamF(Node, (int)h3d.H3DLight.ColorF3, 1, Color.Y);
            h3d.setNodeParamF(Node, (int)h3d.H3DLight.ColorF3, 2, Color.Z);

            if (CreateShadow)
            {
                h3d.setNodeParamI(Node, (int)h3d.H3DLight.ShadowMapCountI, ShadowMapCountI);
                h3d.setNodeParamF(Node, (int)h3d.H3DLight.ShadowSplitLambdaF, 0, ShadowSplitLambdaF);
                h3d.setNodeParamF(Node, (int)h3d.H3DLight.ShadowMapBiasF, 0, ShadowMapBiasF);
            }

            return name;
        }
    }
}
