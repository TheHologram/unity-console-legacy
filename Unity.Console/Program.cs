using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UC;
using Unity.Console.Commands;

namespace Unity.Console
{
    public class Program
    {
        private static UnityCommandLine cmdline;
        static readonly string _rootPath = null;
        static readonly string _iniPath = null;
        static readonly int _startdelay = 2000;
        static bool _enable = false;

        static Program()
        {
            var exeAsm = Assembly.GetExecutingAssembly();
            if (File.Exists(exeAsm.Location))
            {
                _rootPath = Path.GetDirectoryName(exeAsm.Location);
                _iniPath = Path.GetFullPath(Path.Combine(_rootPath, @"Console.ini"));
                if (File.Exists(_iniPath))
                {
                    _enable = GetPrivateProfileInt("Console", "Enable", 0, _iniPath) != 0;
                    _startdelay = GetPrivateProfileInt("Console", "StartDelay", 2000, _iniPath);
                }
            }
        }

        public static void Initialize()
        {
            Run(true, TimeSpan.FromMilliseconds(_startdelay));
        }

        public static void Close()
        {
            if (IntPtr.Zero != GetConsoleWindow())
            {
                FreeConsole();
            }
        }

        public static void Main()
        {
            Run(false, TimeSpan.Zero);
        }

        public static void Run(bool allocConsole, TimeSpan waitTime)
        {
            if (!_enable) return;

            System.Threading.Thread.Sleep(waitTime);

            if (allocConsole && IntPtr.Zero == GetConsoleWindow())
            {
                var hForeground = GetForegroundWindow();
                var hActiveHwnd = GetActiveWindow();
                var hFocusHwnd = GetFocus();

                AllocConsole();
                SetConsoleTitle("Unity Console");

                if (hForeground != IntPtr.Zero)
                    SetForegroundWindow(hForeground);
                if (hActiveHwnd != IntPtr.Zero)
                    SetActiveWindow(hActiveHwnd);
                if (hFocusHwnd != IntPtr.Zero)
                    SetFocus(hFocusHwnd);

            }

            if (Environment.GetEnvironmentVariable("TERM") == null)
                Environment.SetEnvironmentVariable("TERM", "dumb");

            var inStream = new InternalStream(StandardHandles.STD_INPUT);
            var outStream = new InternalStream(StandardHandles.STD_OUTPUT);
            var errStream = new InternalStream(StandardHandles.STD_ERROR);

            System.Console.SetIn(new StreamReader(inStream));
            System.Console.SetOut(new StreamWriter(outStream) {AutoFlush = true});
            System.Console.SetError(new StreamWriter(errStream) {AutoFlush = true});

            var stdwriter = new StreamWriter(new InternalStream(StandardHandles.STD_OUTPUT)) {AutoFlush = true};
            cmdline = new UnityCommandLine();

            //cmdline.RegisterAssembly(exeAsm);

            string[] lines;
            if (GetPrivateProfileSection("Images", _iniPath, out lines))
            {
                foreach (var line in lines)
                {
                    var scname = line.Trim();
                    if (string.IsNullOrEmpty(scname) || scname.StartsWith(";") || scname.StartsWith("#"))
                        continue;

                    var scpath = Path.IsPathRooted(scname) ? scname : Path.Combine(_rootPath, scname);
                    if (File.Exists(scpath))
                    {
                        try
                        {
                            var asm = System.AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(scpath));
                            cmdline.RegisterAssembly(asm);
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("ERROR: " + ex.Message);
                        }
                    }
                }
            }
            if (GetPrivateProfileSection("AutoCompleteAssemblies", _iniPath, out lines))
            {
                foreach (var line in lines)
                {
                    var asmname = line.Trim();
                    if (string.IsNullOrEmpty(asmname) || asmname.StartsWith(";") || asmname.StartsWith("#"))
                        continue;
                    if (!cmdline.AddAssembly(asmname))
                        stdwriter.WriteLine("Error adding assembly: " + asmname);
                }
            }

            var sb = new StringBuilder(4096);
            if (0 < GetPrivateProfileString("Console", "AutoCompleteAssemblies", "", sb, sb.Capacity, _iniPath))
            {
                foreach (var asmname in sb.ToString().Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    if (!cmdline.AddAssembly(asmname))
                        stdwriter.WriteLine("Error adding assembly: " + asmname);
            }
            sb.Length = 0;
            sb.Capacity = 4096;
            if (0 < GetPrivateProfileString("Console", "ScriptsFolders", ".", sb, sb.Capacity, _iniPath))
            {
                foreach (var scname in sb.ToString().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    var scpath = Path.IsPathRooted(scname) ? scname : Path.Combine(_rootPath, scname);
                    ExecCommand.ScriptFolders.Add(scpath);
                }
            }
            if (!GetPrivateProfileSection("Startup", _iniPath, out lines))
                lines = new string[0];

            var console = new UnityConsole(cmdline);
            cmdline.RunWithStartup(console, lines);
            cmdline = null;

            if (allocConsole)
                Close();
        }

        public static void Shutdown()
        {
            cmdline?.Stop();
            Close();
            _enable = false;
        }

        public static bool GetPrivateProfileSection(string appName, string fileName, out string[] section)
        {
            section = null;

            if (!System.IO.File.Exists(fileName))
                return false;

            int MAX_BUFFER = 32767;
            var bytes = new byte[MAX_BUFFER];
            int nbytes = GetPrivateProfileSection(appName, bytes, MAX_BUFFER, fileName);
            if ((nbytes == MAX_BUFFER - 2) || (nbytes == 0))
                return false;
            section = Encoding.ASCII.GetString(bytes, 0, nbytes).Trim('\0').Split('\0');
            return true;
        }

        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName,int nDefault, string lpFileName);

        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileSection(string lpAppName, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        static extern int GetPrivateProfileSection(string lpAppName, byte[] lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        private static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleTitle(string lpConsoleTitle);
    }
}