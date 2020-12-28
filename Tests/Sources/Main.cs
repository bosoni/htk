/*
 * Htk example (c) mjt, 2011-2014
 * 
 * 
 */
using System;
using System.Diagnostics;
using Htk;

namespace Test
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.IO.File.Delete("Horde3D_Log.html");
            System.IO.File.Delete("log.txt");

            Settings.ReadXML("settings.xml");

            Console.WriteLine("Choose test (1-6): ");
            ConsoleKeyInfo k = Console.ReadKey();

            using (GameLoop game = new GameLoop("Htk example", true))
            {
                switch (k.KeyChar)
                {
                    case '1':
                        StateManager.Add(new TestOverlays(), true);
                        break;

                    case '2':
                        StateManager.Add(new Test_Walking(), true);
                        break;

                    case '3':
                        StateManager.Add(new TestTiledMap(), true);
                        break;

                    case '4':
                        StateManager.Add(new TestMorph(), true);
                        break;

                    case '5':
                        StateManager.Add(new TestParticles(), true);
                        break;

                    case '6':
                        StateManager.Add(new Test_GL(), true);
                        break;

                    default:
                        return;
                }
                game.Run(120.0);
            }
        }
    }
}
