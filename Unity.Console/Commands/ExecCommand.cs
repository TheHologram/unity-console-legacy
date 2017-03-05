using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("exec")]
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
                sb.AppendFormat("Usage:   {0} [Arg1] [Arg2] [Arg3]", Name).AppendLine();
                sb.AppendLine(" Notes:");
                sb.AppendLine("   Arguments are passed in as $1, $2, $3 ... variables and are reset");
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} script.sc", Name).AppendLine();
                sb.AppendFormat("   {0} script.sc $var1 2 \"arg3\" ", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            if (args.Length >= 2)
            {
                if (ScriptFolders.Count == 0)
                {
                    ScriptFolders.Add(Directory.GetCurrentDirectory());
                }

                var scriptname = args[1];
                foreach (var path in ScriptFolders)
                {
                    if (Directory.Exists(path))
                    {
                        foreach (var file in Directory.GetFiles(path, Path.ChangeExtension(scriptname,".sc"), SearchOption.TopDirectoryOnly))
                        {
                            Dictionary<string, object> olddict = null;
                            Dictionary<string, object> newdict = null;
                            if (args.Length > 2)
                            {
                                olddict = new Dictionary<string, object>();
                                newdict = new Dictionary<string, object>();
                                // backup old values and fetch would be new values
                                for (int i = 2, j = 1; i < args.Length; ++i, ++j)
                                {
                                    object value = null;
                                    object oldvalue = null;
                                    var arg = args[i];
                                    var argname = "$" + j.ToString();
                                    if (Owner.Variables.TryGetValue(argname, out oldvalue))
                                        olddict[argname] = oldvalue;
                                    if (arg.StartsWith("$"))
                                        Owner.Variables.TryGetValue(arg, out value);
                                    else
                                        value = arg;
                                    newdict[argname] = value;
                                }
                                // must be 2 pass to avoid issues with sub scripts using $# variables
                                foreach (var kvp in newdict)
                                    Owner.Variables[kvp.Key] = kvp.Value;                                
                            }
                            using (var sr = File.OpenText(file))
                                Owner.ExecuteScript(sr, false);

                            // clear the current value variable
                            Owner.Variables.Remove("$_");
                            // remove created arguments
                            if (newdict != null)
                                foreach (var key in newdict.Keys)
                                    Owner.Variables.Remove(key);
                            // reset old values
                            if (olddict != null)
                                foreach (var kvp in olddict)
                                    Owner.Variables[kvp.Key] = kvp.Value;
                            return 0;
                        }
                    }
                }               
            }
            Owner.Variables.Remove("$_");
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