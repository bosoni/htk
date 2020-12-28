/*
 * Htk framework (c) mjt, 2011-2014
 * 
 * 
 * Using Horde3D.Net+OpenTK.
 */
using System;

namespace Htk
{
    public static class Log
    {
        private static System.IO.StreamWriter logWriter = null;

        public static void Open(string filename)
        {
            if (logWriter == null) logWriter = new System.IO.StreamWriter(filename);
        }
        public static void Close()
        {
            if (logWriter == null) return;
            logWriter.Close();
            logWriter = null;
        }
        public static void WriteToFile(string str)
        {
            if (logWriter == null)
            {
                Open("log.txt");
            }

            logWriter.WriteLine("[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]: " + str);
            logWriter.Flush();
        }

        public static void WriteLine(string str)
        {

#if DEBUG   // jos DEBUG, kirjoita myös konsoliin
            System.Diagnostics.Trace.WriteLine(str);
            Console.WriteLine(str);
#endif
            WriteToFile(str);
        }

        public static void Error(string str)
        {
            Log.WriteLine(str);
            //System.Windows.Forms.MessageBox.Show(str, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            throw new Exception(str);
        }

    }

}
