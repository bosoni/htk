// game-test (c) by mjt
using System;
using System.Threading;
using System.Collections.Generic;
using Horde3DNET;
using Horde3DNET.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System.IO;
using System.Drawing;
using Htk;

namespace GameTest
{
    public class Game : BaseGame
    {
        const int MAX_ENEMIES = 100;

        GameLog gameLog = new GameLog();
        List<Actor> actors = new List<Actor>();
        Actor self;
        Map map = new Map();

        int lightNode, cursor3DNode;
        float cur3Dangle = 0;

        bool alive = true, mouseBottom = false;

        Overlay texAlapalkki;
        Overlay texEnergy;
        Overlay[] texCursors = new Overlay[3];
        /// <summary>
        /// 0=default, 1=attack, 2=talk
        /// </summary>
        int curCursor = 2;

        /// <summary>
        /// tämä metodi suoritetaan toisessa threadissa
        /// </summary>
        /// <param name="actorObj"></param>
        public void SetPath(object actorObj)
        {
            Actor actor = (Actor)actorObj;

            // luo enemyille random reitit
            actor.Anim.Clear();

            if (rnd.Next(100) < 50)
            {
                actor.Anim.Add(actor.Position);
                int x = rnd.Next(map.MapWidth), y = rnd.Next(map.MapHeight);
                actor.Anim.Add(new Vector3(map.Get3DX(x), 0, map.Get3DY(y)));
                Console.Write("!"); // debug
            }
            else
                while (true)
                {
                    int x = rnd.Next(map.MapWidth), y = rnd.Next(map.MapHeight);
                    if (map.IsFreePlace(x, y))
                    {
                        if (CreatePath(map.GetMapX(actor.Position.X), map.GetMapY(actor.Position.Z), ref actor, new Vector3(x, 0, y)))
                        {
                            Console.Write("."); // debug
                            break;
                        }
                    }
                }
        }

        bool CreatePath(int x, int y, ref Actor actor, Vector3 newPosition)
        {
            Point[] p = map.SearchPath(x, y, newPosition);
            if (p != null)
            {
                for (int q = 0; q < p.Length; q++)
                {
                    actor.Anim.Add(new Vector3(map.Get3DX(p[q].X), 0, map.Get3DY(p[q].Y)));
                }
                return true;
            }
            return false;
        }



        public override void Init()
        {
            camera.Pipeline = h3d.addResource((int)h3d.H3DResTypes.Pipeline, "pipelines/forward.pipeline.xml", 0);
            int lightMatRes = h3d.addResource((int)h3d.H3DResTypes.Material, "shaders/light.material.xml", 0);
            int mouseCursor = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, "cursor3d.scene.xml", 0);
            int characterRes = h3d.addResource((int)h3d.H3DResTypes.SceneGraph, "man.scene.xml", 0);
            int characterWalkRes = h3d.addResource((int)h3d.H3DResTypes.Animation, "man.anim", 0);

            Util.LoadResourcesFromDisk();

            cursor3DNode = h3d.addNodes(h3d.H3DRootNode, mouseCursor);
            texCursors[0] = Overlay.Create("textures/cursor_default.png");
            texCursors[1] = Overlay.Create("textures/cursor_attack.png");
            texCursors[2] = Overlay.Create("textures/cursor_talk.png");
            texAlapalkki = Overlay.Create("textures/alapalkki.png");
            texEnergy = Overlay.Create("textures/energy.png");

            self = new Actor();
            self.Node = h3d.addNodes(h3d.H3DRootNode, characterRes);
            h3d.setupModelAnimStage(self.Node, 0, characterWalkRes, 0, string.Empty, false);
            self.Position = map.CreateMap_2("maps/karttatest3.xml");
            self.Position.Y = -0.01f;
            self.Update();
            actors.Add(self);

            // enemyt
            int enemyNum = 0;
            while (enemyNum < MAX_ENEMIES)
            {
                int x = rnd.Next(map.MapWidth);
                int y = rnd.Next(map.MapHeight);

                // tsekkaa ettei mene päällekäin
                for (int c = 0; c < enemyNum; c++)
                    if ((int)actors[c].Position.X == (int)map.Get3DX(x) && (int)actors[c].Position.Z == (int)map.Get3DY(y))
                    {
                        continue;
                    }

                if (map.IsFreePlace(x, y))
                {
                    enemyNum++;

                    Actor tmp = new Actor();

                    // aseta erilaisia arvoja enemyille
                    tmp.Energy = BaseGame.rnd.Next(50) + 50; // energy 50-100
                    tmp.Weight = (float)BaseGame.rnd.Next(50) + 70; // 70-120
                    tmp.Strength = (float)BaseGame.rnd.NextDouble() + 0.4f;
                    tmp.Skill = (float)BaseGame.rnd.NextDouble() + 0.5f;
                    tmp.Skill = (float)BaseGame.rnd.NextDouble() * 0.01f;

                    tmp.Node = h3d.addNodes(h3d.H3DRootNode, characterRes);
                    h3d.setupModelAnimStage(tmp.Node, 0, characterWalkRes, 0, string.Empty, false);
                    tmp.Position = new Vector3(map.Get3DX(x), 0, map.Get3DY(y));
                    tmp.Update();
                    actors.Add(tmp);
                }
            }

            lightNode = h3d.addLightNode(h3d.H3DRootNode, "Light1", lightMatRes, "LIGHTING", "SHADOWMAP");
            camera.Node = h3d.addCameraNode(h3d.H3DRootNode, "Camera", camera.Pipeline);
            camera.Position = new Vector3(self.Position);
            camera.Position.X += 35;
            camera.Position.Y += 40;
            camera.Position.Z += 35;
            camera.Rotation = new Vector3(-40, 45, 0);
            camera.Fov = 10;
            camera.Near = 1f;
            camera.Far = 1000;

            SetLight();

            text = new TextRenderer(Settings.Width, 200);
        }

        void SetLight()
        {
            h3d.setNodeTransform(lightNode, 0, 0, 0, 0, 0, 0, 1, 1, 1);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.RadiusF, 0, 200);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.FovF, 0, 90);
            h3d.setNodeParamI(lightNode, (int)h3d.H3DLight.ShadowMapCountI, 3);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.ShadowSplitLambdaF, 0, 0.9f);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.ShadowMapBiasF, 0, 0.001f);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.ColorF3, 0, 0.9f);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.ColorF3, 1, 0.7f);
            h3d.setNodeParamF(lightNode, (int)h3d.H3DLight.ColorF3, 2, 0.75f);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(float time)
        {
            // hiirenkoordinaatit 0-1 floattina
            float mx = (float)Mouse.X / (float)Settings.Width, my = (float)Mouse.Y / (float)Settings.Height;

            if (Keyboard[Key.Escape])
            {
                TestGameLoop.NextClass = "Exit";
            }
            float spd = (float)time * 10;
            if (Keyboard[Key.Up] || Mouse.Y < 15) camera.Strafe(spd, 0);
            if (Keyboard[Key.Down] || Mouse.Y > Settings.Height - 15) camera.Strafe(-spd, 0);
            if (Keyboard[Key.Left] || Mouse.X < 15) camera.Strafe(-spd);
            if (Keyboard[Key.Right] || Mouse.X > Settings.Width - 15) camera.Strafe(spd);

            // tarkista onko hiiri alapalkissa
            if (my > 0.82f || alive == false)
            {
                mouseBottom = true;
                h3d.setNodeTransform(cursor3DNode, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }
            else
            {
                mouseBottom = false;

                // Zoom
                int wheel = Mouse.WheelDelta;
                if (wheel > 0) camera.Move(time * 100);
                else if (wheel < 0) camera.Move(-time * 100);

                if (Mouse[MouseButton.Middle])
                {
                    // Panning
                    if (Mouse[MouseButton.Right] == false)
                    {
                        camera.Strafe((Mouse.X - oldMouseX) * 0.2f);
                        camera.Strafe(-(Mouse.Y - oldMouseY) * 0.2f, 0);

                    }
                    else // Rotating
                    {
                        // todo
                        /*
                        eka pitäis varmaa ottaa 3d-koordinaatit siitä kohdasta joka 
                            o suoraan kamerasta eteenpäin
                                (castray ja kameran direction)
                        sit lasketaa deltat ((Mouse.X - oldMouseX) * 0.2f jne)

                        intersectionin ympäri pitäis kameraa pyöräyttää.
                        */

                    }


                }


                // lasketaan 3d-koordinaatit hiiren koordinaateista
                float ox, oy, oz, dx, dy, dz, dist = 0;
                float[] intersectionPoint = new float[3];
                int picked = 0;
                Horde3DUtils.pickRay(camera.Node, mx, (float)(Settings.Height - Mouse.Y) / (float)Settings.Height,
                    out ox, out oy, out oz, out dx, out dy, out dz);
                if (h3d.castRay(h3d.H3DRootNode, ox, oy, oz, dx * 10000, dy * 10000, dz * 10000, 0) != 0)
                {
                    if (h3d.getCastRayResult(0, out picked, out dist, intersectionPoint))
                    {
                        if (picked > 0) picked = h3d.getNodeParent(picked);

                        // näyttää 3d-kursorin
                        h3d.setNodeTransform(cursor3DNode, intersectionPoint[0], 0, intersectionPoint[2], 0, cur3Dangle -= 2, 0, 0.7f, 0.7f, 0.7f);
                    }
                }

                // liikutus haluttuun kohtaan / hyökkäys / aarteen ottaminen / puhuminen
                if (Mouse[MouseButton.Left])
                {
                    self.Anim.Clear();
                    if (picked > 0)
                    {
                        Vector3 intersect = new Vector3(intersectionPoint[0], 0, intersectionPoint[2]);
                        Vector3 len = self.Position - intersect;
                        if (len.LengthFast > 1f)
                        {
                            self.Anim.Add(self.Position);
                            self.Anim.Add(intersect);
                        }

                        for (int q = 1; q < actors.Count; q++)
                        {
                            if (picked == actors[q].Node)
                            {
                                len = actors[q].Position - self.Position;
                                if (len.LengthFast < 1.0f) // jos ollaan toisen vieressä
                                {
                                    // lyödäänkö vai puhutaanko?

                                    if (curCursor == 1) // lyö
                                    {
                                        string hitStr = self.Hit(actors[q]);
                                        if (hitStr != "") gameLog.Add(hitStr, Brushes.White);
                                        if (actors[q].Energy <= 0) // kuolema!
                                        {
                                            h3d.removeNode(picked);
                                            gameLog.Add("You killed " + actors[q].Name + "!", Brushes.DeepPink);
                                            self.Skill += 2;
                                            actors.RemoveAt(q);
                                        }
                                        picked = 0;
                                    }
                                    else if (curCursor == 2 && mouseLeftPressed == false) // puhu
                                    {
                                        gameLog.Add("You talked to " + actors[q].Name + ": Hi you ugly shit!", Brushes.Green);
                                    }

                                }
                                picked = 0;
                                break;
                            }
                        }
                    }

                    /*
                    // jos klikattu aarretta
                    if (picked > 0)
                    {
                        int treasureFound = map.GetTreasure(picked);
                        if (treasureFound != -1)
                        {
                            Vector3 len = self.Position - map.GetTreasures()[treasureFound].Position;
                            if (len.LengthFast < 2f)
                            {
                                h3d.removeNode(picked);
                                map.GetTreasures()[treasureFound].Position = Vector3.Zero;
                                gameLog.Add("Found treasure: " + map.GetTreasures()[treasureFound].Value, Brushes.Gold);
                                gold += map.GetTreasures()[treasureFound].Value;
                            }
                        }
                    }*/
                }

                if (Mouse[MouseButton.Right])
                {
                    if (mouseRightPressed == false)
                    {
                        curCursor++;
                        if (curCursor == texCursors.Length) curCursor = 1;
                    }
                }
            }

            if (self.Energy <= 0)
            {
                alive = false;
                self.Energy = 0;
                gameLog.Add("You died!", Brushes.Red);
                h3d.removeNode(self.Node);
            }

            self.Anim.MoveActorInMap(time * 5, 20, ref map, ref actors);
            self.Update(time);

            counter++; counter %= 2; // 0 tai 1
            for (int q = 1 + counter; q < actors.Count - counter; q += 2) // joka toinen actor joka framella
            {
                actors[q].Update(time);
                if (actors[q].Anim.Update(time * 10, 20, map, ref actors) == false)
                {
                    if (rnd.Next(1000) < 1)
                    {
                        Actor tmp = actors[q];
                        Thread th = new Thread(new ParameterizedThreadStart(SetPath));
                        th.Start(tmp);
                    }
                }
            }

            base.Update(time);
        }
        int counter = 0;



        Vector3 lp = new Vector3(0, 50, 100), lr = new Vector3(0, 0, 0); // todo, säädä valo näillä

        public override void Render()
        {
            h3d.setNodeTransform(lightNode, lp.X, lp.Y, lp.Z, lr.X, lr.Y, lr.Z, 1, 1, 1);

            /*
            h3d.setNodeTransform(lightNode,
                self.Position.X, self.Position.Y + 1, self.Position.Z,
                0, 180 + self.Rotation.Y, 0, 1, 1, 1);
            */


            texAlapalkki.Draw(0, 0.95f, (float)Settings.Width / 1600f, (float)Settings.Height / 1200);

            //TODO FIX texEnergy.Draw(0.905f, 0.94f, (0.088f / 100) * self.Energy, 0.037f); //, 1f - (1.0f / 100 * self.Energy), (1.0f / 100 * self.Energy), 0.5f)); sfdojsrophjwero

            // piirrä hiiripointteri
            if (mouseBottom == false)
                texCursors[curCursor].Draw((float)Mouse.X / (float)Settings.Width, (float)Mouse.Y / (float)Settings.Height);
            else texCursors[0].Draw((float)Mouse.X / (float)Settings.Width, (float)Mouse.Y / (float)Settings.Height);

            camera.Update();
            h3d.render(camera.Node);


            if (gameLog.Updated == true)
            {
                text.Clear(Color.Transparent);
                for (int q = 0, y = 5; q < gameLog.log.Count; q++, y += TextRenderer.FontMono.Height)
                    text.DrawString(gameLog.log[q], TextRenderer.FontMono, gameLog.brush[q], new PointF(0, y));
                gameLog.ClearOlder(5);
                gameLog.Updated = false;
            }
            text.Render();

        }
    }
}
