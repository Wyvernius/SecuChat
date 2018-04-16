using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SecuChat
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Logger.rerouteConsole();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                Logger.CloseLogFile();
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Only one instance of this program per machine is allowed!");
            }
        }
    }

    class Logger
    {
        static FileStream ostrm;
        static StreamWriter writer;

        [Conditional("RELEASE")]
        public static void CloseLogFile()
        {
            writer.Close();
            ostrm.Close();
        }

        [Conditional("RELEASE")]
        public static void rerouteConsole()
        {
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("./ChatAppLog"+ DateTime.Now.ToFileTime().ToString()+".txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
        }
    }
}
