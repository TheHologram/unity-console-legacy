using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UC;

namespace Unity.Console
{
    class Program
    {

        public static void Main()
        {
            if (Environment.GetEnvironmentVariable("TERM") == null)
                Environment.SetEnvironmentVariable("TERM", "dumb");

            var inStream = new UC.InternalStream(StandardHandles.STD_INPUT);
            var outStream = new UC.InternalStream(StandardHandles.STD_OUTPUT);
            var errStream = new UC.InternalStream(StandardHandles.STD_ERROR);

            System.Console.SetIn(new StreamReader(inStream));
            System.Console.SetOut(new StreamWriter(outStream) { AutoFlush = true });
            System.Console.SetError(new StreamWriter(errStream) { AutoFlush = true });

            var stdwriter = new StreamWriter(new UC.InternalStream(StandardHandles.STD_OUTPUT)) { AutoFlush = true };
            UnityCommandLine cmdline = new UnityCommandLine();

            var path = new StringBuilder(260);
            GetModuleFileName(IntPtr.Zero, path, path.Capacity);
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var rootPath = Path.GetFullPath(Path.GetDirectoryName(path.ToString()));
            var iniPath = Path.GetFullPath( Path.Combine(rootPath, @"Console.ini"));

            StringBuilder sb = new StringBuilder(4096);
            if (0 < GetPrivateProfileString("Mono", "AutoCompleteAssemblies", "", sb, sb.Capacity, iniPath))
            {
                foreach (var asmname in sb.ToString().Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    if (!cmdline.AddAssembly(asmname))
                        stdwriter.WriteLine("Error adding assembly: " + asmname);
            }
            sb.Length = 0;
            sb.Capacity = 4096;
            if (0 < GetPrivateProfileString("Mono", "ScriptsFolders", ".", sb, sb.Capacity, iniPath))
            {
                foreach (var scname in sb.ToString().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    var scpath = Path.IsPathRooted(scname) ? scname : Path.Combine(rootPath, scname);
                    Commands.ExecCommand.ScriptFolders.Add(scpath);
                }
            }

            var console = new UnityConsole(cmdline);
            cmdline.Run(console);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(
               string lpAppName,
               string lpKeyName,
               string lpDefault,
               StringBuilder lpReturnedString,
               int nSize,
               string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        static extern uint GetModuleFileName([In]IntPtr hModule, [Out]StringBuilder lpFilename, [In][MarshalAs(UnmanagedType.U4)]int nSize);
    }
}
