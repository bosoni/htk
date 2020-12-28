// game-test (c) by mjt
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using Htk;

namespace GameTest
{
    public class Map
    {
        SpatialAStar.PathFinder pathFinder;
        public int MapWidth, MapHeight;
        int mapTexture;
        byte[, ,] mapData;
        public List<Treasure> Treasures = new List<Treasure>();

        Vector3 BBoxMin, BBoxMax, BBox;
        float scalingX, scalingY, scalingX2, scalingY2;
        public float Get3DX(int x)
        {
            return scalingX * (float)x + BBoxMin.X;
        }
        public float Get3DY(int y)
        {
            return scalingY * (float)y + BBoxMin.Z;
        }
        public int GetMapX(float x)
        {
            return (int)(scalingX2 * (x - BBoxMin.X));
        }
        public int GetMapY(float z)
        {
            return (int)(scalingY2 * (z - BBoxMin.Z));
        }


        public Point[] SearchPath(int x, int y, Vector3 endPos)
        {
            IEnumerable<SpatialAStar.MyPathNode> path = pathFinder.Search(x, y, (int)endPos.X, (int)endPos.Z);
            List<Point> list = new List<Point>();

            if (path == null) return null;

            foreach (SpatialAStar.MyPathNode node in path)
                list.Add(new Point(node.X, node.Y));

            return list.ToArray();
        }

        public int[] ReadXML(string fileName, out int[] materials)
        {
            List<int> res = new List<int>();

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode maptexture = doc.SelectSingleNode("//map/maptexture/text()");
            mapTexture = h3d.addResource((int)h3d.H3DResTypes.Texture, maptexture.Value, 0);

            XmlNodeList lst = doc.GetElementsByTagName("model");
            materials = new int[lst.Count];
            int c = 0;
            foreach (XmlNode node in lst)
            {
                int tempRes = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, node.FirstChild.Value, 0);
                if (node.LastChild.Name == "texture")
                {
                    string newTexName = node.LastChild.InnerText;

                    // luodaan materiaali joka käyttää <texture>
                    materials[c] = h3d.addResource((int)h3d.H3DResTypes.Material, newTexName + ".material", 0);
                    if (materials[c] == 0) throw new Exception("Cant create material " + newTexName + ".material");

                    string materialData = "<Material>\n<Shader source=\"shaders/model.shader\"/>\n";
                    //materialData += "<ShaderFlag name=\"_F01_Textured\"/>\n";
                    materialData += "<Sampler name=\"albedoMap\" map=\"" + newTexName + "\" allowCompression=\"true\" mipmaps=\"false\" />\n";
                    materialData += "</Material>\n\0";

                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    if (h3d.loadResource(materials[c], encoding.GetBytes(materialData), materialData.Length) == false)
                        throw new Exception("Cant load material " + c);
                }
                else materials[c] = 0; // käytetään objektin default materiaalia

                c++;
                res.Add(tempRes);
            }

            return res.ToArray();
        }

        /// <summary>
        /// 
        /// testausta varten tää createmap_2
        /// 
        /// kartta ladataan .geo filusta, astar reitinhakua varten ladataan bitmap jossa seinät ym esteet
        /// 
        /// </summary>
        /// <param name="mapFileName"></param>
        /// <returns></returns>
        public Vector3 CreateMap_2(string mapFileName)
        {
            mapFileName = Settings.ContentDir + "/" + mapFileName;
            Vector3 startPos = new Vector3(5, 0, -15);

            int[] materials = null;
            int[] res = ReadXML(mapFileName, out materials);

            // lataa datat
            Util.LoadResourcesFromDisk();

            // kartan lev&kor
            MapWidth = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            MapHeight = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            mapData = new byte[MapWidth, MapHeight, 3];

            int mapNode = h3d.addNodes(h3d.H3DRootNode, res[0]);
            h3d.setNodeTransform(mapNode, 0, 0, 0, 0, 0, 0, 0.4f, 0.1f, 0.4f);

            h3d.getNodeAABB(mapNode,
                out BBoxMin.X, out BBoxMin.Y, out BBoxMin.Z,
                out BBoxMax.X, out BBoxMax.Y, out BBoxMax.Z);
            BBox = BBoxMax - BBoxMin;
            scalingX = BBox.X / (float)MapWidth;
            scalingY = BBox.Z / (float)MapHeight;
            scalingX2 = (float)MapWidth / BBox.X;
            scalingY2 = (float)MapHeight / BBox.Z;

            unsafe
            {
                IntPtr dataPtr = h3d.mapResStream(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgPixelStream, true, false);
                uint* map = (uint*)dataPtr.ToPointer();

                for (int y = 0; y < MapHeight; y++)
                {
                    for (int x = 0; x < MapWidth; x++)
                    {
                        uint col = map[y * MapWidth + x];
                        mapData[x, y, 0] = GetColor(0, col);
                        mapData[x, y, 1] = GetColor(1, col);
                        mapData[x, y, 2] = GetColor(2, col);
                        if (mapData[x, y, 0] == 255 && mapData[x, y, 1] == 255 && mapData[x, y, 2] == 255)
                            startPos = new Vector3(Get3DX(x), 0, Get3DY(y)); // aloituspaikka

//                        todo asetteles karttaan kaik muutki shitit,
  //                          aarteet ja aseet ym


                    }
                }
            }
            h3d.unmapResStream(mapTexture);
            pathFinder = new SpatialAStar.PathFinder(MapWidth, MapHeight, mapData);
            /*
             *resurssien asettamine kuuluis ylempään looppiin, todo fix  
            for (int q = 1; q < res.Length; q++)
            {
                int node = h3d.addNodes(h3d.H3DRootNode, res[q]);
            }
            */

            return startPos;
        }

        public Vector3 CreateMap(string mapFileName)
        {
            mapFileName = Settings.ContentDir + "/" + mapFileName;
            Vector3 startPos = new Vector3(0, 0, 0);

            int[] materials = null;
            int[] res = ReadXML(mapFileName, out materials);

            // lataa kaikki
            Util.LoadResourcesFromDisk();

            // kartan lev&kor
            MapWidth = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            MapHeight = h3d.getResParamI(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgWidthI);
            mapData = new byte[MapWidth, MapHeight, 4];

            //int transparentWallRes = -1;

            unsafe
            {
                IntPtr dataPtr = h3d.mapResStream(mapTexture, (int)h3d.H3DTexRes.ImageElem, 0, (int)h3d.H3DTexRes.ImgPixelStream, true, false);
                uint* map = (uint*)dataPtr.ToPointer();

                // kartta läpi taso kerrallaan (R,G,B)
                for (int layer = 0; layer < 3; layer++) // note: jos pistää 4 (rgba),ruutu täyttyy laatikoista
                {
                    // kartta läpi objekti kerrallaan
                    for (int objNum = 0; objNum < res.Length; objNum++)
                    {
                        for (int y = 1; y < MapHeight; y++)
                        {
                            for (int x = 0; x < MapWidth; x++)
                            {
                                int index = y * MapWidth + x;
                                mapData[x, y, layer] = GetColor(layer, map[index]);
                                if (layer == 2 && mapData[x, y, layer] == 255) // B-layeri 255 == pelaajan aloituspaikka
                                {
                                    startPos = new Vector3(x * 2, 5, y * 2);
                                }

                                byte curObjColor = GetColor(layer, map[objNum]);
                                if (curObjColor == 0) continue;
                                if (GetColor(layer, map[index]) == curObjColor)
                                {
                                    int w = h3d.addNodes(h3d.H3DRootNode, res[objNum]);
                                    if (w != 0)
                                    {
                                        if (layer == 2)
                                        {
                                            Treasure treas = new Treasure();
                                            treas.Node = w;
                                            treas.Position = new Vector3(x * 2, 0, y * 2);
                                            //treasures.Add(treas);
                                        }

                                        // muutetaan objektin materiaali jos tarvii
                                        if (materials[objNum] != 0)
                                        {
                                            if (h3d.findNodes(w, "", (int)h3d.H3DNodeTypes.Mesh) != 0)
                                            {
                                                int mesh = h3d.getNodeFindResult(0);
                                                h3d.setNodeParamI(mesh, (int)h3d.H3DMesh.MatResI, materials[objNum]);
                                            }
                                        }

                                        // lattia 
                                        if (layer == 0)
                                        {
                                            //floorNodeMap.Add(w);
                                        }

                                        if (layer != 1) // jos ei seinät, aseta objekti skeneen
                                        {
                                            //h3d.setNodeTransform(w, -1 + x * 2, 0, -1 + y * 2, 0, 0, 0, 1, 1, 1);
                                            h3d.setNodeTransform(w, x * 2, 0, y * 2, 0, 0, 0, 1, 1, 1);
                                        }
                                        else // seinät
                                        {
                                            float scale = 1;
                                            float sy = 1;

                                            // tsekkaa onko alareuna-seinä tai oikeanpuoleinen seinä
                                            if (mapData[x, y + 1, 0] == 0 || mapData[x + 1, y, 0] == 0)
                                            {

                                                /*  // jos halutaan muuttaa seinä läpikuultavaksi
                                                if (transparentWallRes == -1)
                                                {
                                                    transparentWallRes = h3d.addResource((int)h3d.H3DResTypes.Material, "models/1/transparent_wall.material.xml", 0);
                                                      Util.LoadResourcesFromDisk();
                                                }

                                                if (h3d.findNodes(w, "Rectangle01", (int)h3d.H3DNodeTypes.Mesh) != 0)
                                                 // tai if (h3d.findNodes(w, "", (int)h3d.H3DNodeTypes.Mesh) != 0)
                                                {
                                                    int mesh = h3d.getNodeFindResult(0);
                                                    h3d.setNodeParamI(mesh, (int)h3d.H3DMesh.MatResI, transparentWallRes);
                                                }
                                                */

                                                sy = 0.4f;
                                                scale = 0.5f;
                                            }

                                            // kavennetaan seiniä ellei nurkkaseinäpala
                                            float sx = 1, sz = 1;
                                            bool horz = false, vert = false;
                                            if (x > 0 && x < MapWidth - 1 && y > 0 && y < MapHeight - 1)
                                            {
                                                if (mapData[x - 1, y, layer] != 0 && mapData[x + 1, y, layer] != 0) horz = true;
                                                if (mapData[x, y - 1, layer] != 0 && mapData[x, y + 1, layer] != 0) vert = true;
                                            }
                                            if (horz || vert)
                                                if (horz) sz = 0.5f * scale;
                                                else if (vert) sx = 0.5f * scale;

                                            h3d.setNodeTransform(w, 1 + x * 2, 0, 1 + y * 2, 0, 0, 0, sx, sy, sz);
                                            //h3d.setNodeTransform(w, x * 2, 0, y * 2, 0, 0, 0, sx, sy, sz);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                h3d.unmapResStream(mapTexture);

                pathFinder = new SpatialAStar.PathFinder(MapWidth, MapHeight, mapData);

                return startPos;
            }
        }

        /// <summary>
        /// layer 0,1,2 -> R,G,B
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        byte GetColor(int layer, uint color)
        {
            switch (layer)
            {
                case 0: // R
                    return (byte)(color >> 16);
                case 1: // G
                    return (byte)(color >> 8);
                case 2: // B
                    return (byte)color;
                case 3: // A
                    return (byte)(color >> 24);
            }
            return 0;
        }

        public bool IsFreePlace(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight) return false;
            if (mapData[x, y, 1] == 0 && mapData[x, y, 2] == 0)
            {
                return true;
            }
            return false;
        }

    }

    public class Treasure
    {
        public string Name = "Treasure chest";
        public int Value;
        public Vector3 Position;
        public int Node;

        public Treasure()
        {
            Value = BaseGame.rnd.Next(100) + 100;
        }
    }
}
