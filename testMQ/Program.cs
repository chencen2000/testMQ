using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace testMQ
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            //Application.Run(new FormTest());
        }

        public static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("[{0}]: {1}", DateTime.Now.ToString("o"), msg));
        }
    }
}
