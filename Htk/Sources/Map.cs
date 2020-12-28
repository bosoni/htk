/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;
using Horde3DNET;
using Horde3DNET.Utils;

namespace Htk
{
    public class Map
    {
        public int MapWidth, MapHeight;
        int mapTexture;
        int[,] mapData;

        public void CreateMap(string mapFileName, string[] resNames)
        {
            int[] res = new int[resNames.Length];
            for (int q = 0; q < resNames.Length; q++) 
                res[q] = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, "models/" + resNames[q]+".scene.xml", 0);
            mapTexture = h3d.addResource((int)h3d.H3DResTypes.Texture, "textures/" + mapFileName, 0);
            Util.LoadResourcesFromDisk();

            MapWidth = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            MapHeight = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            mapData = new int[MapWidth, MapHeight];

            unsafe
            {
                IntPtr dataPtr = h3d.mapResStream(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgPixelStream, true, false);
                int* map = (int*)dataPtr.ToPointer();

                // käydään kartta läpi objekti kerrallaan
                for (int q = 0; q < res.Length; q++)
                {
                    int r = map[q];

                    for (int y = 1; y < MapHeight; y++)
                    {
                        for (int x = 0; x < MapWidth; x++)
                        {
                            int c = y * MapWidth + x;
                            if (map[c] == r)
                            {
                                int w = h3d.addNodes(h3d.H3DRootNode, res[q]);
                                h3d.setNodeTransform(w, x * 2, 0, y * 2, 0, 0, 0, 1, 1, 1);
                            }
                        }
                    }
                }
                h3d.unmapResStream(mapTexture);
            }
        }

    }
}
