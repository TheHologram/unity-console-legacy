using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class ExecCommand : ICommand
    {
        public ExecCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public static List<string> ScriptFolders { get; } = new List<string>();

        public UnityCommandLine Owner { get; }

        public string Name => "exec";

        public string Description => "Execute script";

        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} ", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0}", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            if (args.Length == 2)
            {
                if (ScriptFolders.Count == 0)
                {
                    ScriptFolders.Add(Directory.GetCurrentDirectory());
                }

                var sname = args[1];
                foreach (var path in ScriptFolders)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var file in Directory.GetFiles(path, Path.ChangeExtension(sname,".sc"), SearchOption.TopDirectoryOnly))
                        {
                            using (var sr = File.OpenText(file))
                                Owner.ExecuteScript(sr, false);
                            return 0;
                        }
                    }
                }
            }
            return 0;
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            if (ScriptFolders.Count == 0)
            {
                ScriptFolders.Add(Directory.GetCurrentDirectory());
            }
            var names = new List<string>();
            foreach (var path in ScriptFolders)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path, "*.sc", SearchOption.TopDirectoryOnly))
                    {
                        names.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
            }
            if (args.Length == 1 && endswithspace)
            {
                options = names;
                return true;
            }
            var name = args[1];
            options = Owner.GetPartialStringList(name, names);
            return true;
        }
    }

}