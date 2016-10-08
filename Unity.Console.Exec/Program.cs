using System;
using System.Collections.Generic;
using System.IO;
using UC;

namespace Unity.Console.Exec
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("TERM") == null)
            {
                Environment.SetEnvironmentVariable("TERM", "dumb");
            }

            var inStream = new UC.InternalStream(StandardHandles.STD_INPUT);
            var outStream = new UC.InternalStream(StandardHandles.STD_OUTPUT);
            var errStream = new UC.InternalStream(StandardHandles.STD_ERROR);

            System.Console.SetIn(new StreamReader(inStream));
            System.Console.SetOut(new StreamWriter(outStream) { AutoFlush = true });
            System.Console.SetError(new StreamWriter(errStream) { AutoFlush = true });

            System.AppDomain.CurrentDomain.Load("Winmm.Test");
            System.AppDomain.CurrentDomain.Load("Winmm-Test-pass");
            UnityCommandLine cmdline = new UnityCommandLine();
            cmdline.AddAssembly("Winmm.Test");
            cmdline.AddAssembly("Winmm-Test-pass");
            UnityConsole console = new UnityConsole(cmdline);
            console.Clear();
            cmdline.Run(console);
        }
    }
}
