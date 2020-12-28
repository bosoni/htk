// game-test (c) by mjt
using System;
using Htk;

namespace GameTest
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Settings.ReadXML("settings.xml");
            Console.WriteLine("Small game-test (c) mjt\n\ndebug info: ");

            using (TestGameLoop game = new TestGameLoop())
            {
                game.Run(60.0);
            }
        }
    }
}
