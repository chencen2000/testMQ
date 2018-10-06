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
        static int Main(string [] args)
        {
            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if(_args.IsParameterTrue("debug"))
            {
                MessageBox.Show("wait for debug");
            }
            Program.args = _args.Parameters;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            //Application.Run(new FormTest());
            return exit_code;
        }
        public static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("[{0}]: {1}", DateTime.Now.ToString("o"), msg));
        }
        public static System.Collections.Specialized.StringDictionary args = null;
        public static int exit_code = 1;
    }
}
