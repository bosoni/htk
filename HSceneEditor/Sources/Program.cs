using System;
using System.Windows.Forms;
using System.Diagnostics;
using Htk;

namespace HSceneEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.IO.File.Delete("Horde3D_Log.html");
            System.IO.File.Delete("log_debug.txt");
            System.IO.File.Delete("log.txt");

            //Settings.ContentDir = "../../../Data";

#if DEBUG
            // OTK DEBUG / To capture this output, add the following lines to the start of your Main() function:
            TextWriterTraceListener tl = new TextWriterTraceListener("log_debug.txt");
            Debug.Listeners.Add(tl);
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
