using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;
using Unity.Console.Behaviors;
using UnityEngine.Assertions.Must;

namespace Unity.Console.Commands
{
    [CommandAttribute("sync")]
    class SyncCommand : ICommand
    {
        public SyncCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        public string Name => "sync";

        public string Description => "Synchronize commands via Unity Coroutine";

        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} <command|on|off> [Arg1] [Arg2] [Arg3]", Name).AppendLine();
                sb.AppendLine(" Notes:");
                sb.AppendLine("   Sync wraps other commands and executes them in a Unity Coroutine for safer thread context");
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} on", Name).AppendLine();
                sb.AppendFormat("   {0} off", Name).AppendLine();
                sb.AppendFormat("   {0} call MMInternalClass Method", Name).AppendLine();
                sb.AppendFormat("   {0} exec script.sc $var1 2 \"arg3\" ", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            if (args.Length <= 1)
            {
                Owner.Console.WriteLine("Insufficient arguments", Style.Error);
                return 0;
            }
            // special handling for auto synchronize
            if (args.Length > 1)
            {
                switch (args[1].ToLower())
                {
                    case "off":
                        Owner.Synchronize = false;
                        return 0;
                    case "on":
                        Owner.Synchronize = true;
                        return 0;
                }
            }
            var subargs = new string[args.Length - 1];
            Array.Copy(args, 1, subargs, 0, args.Length-1);

            var arg0 = subargs[0];
            ICommand cmd;
            if (!Owner.Commands.TryGetValue(arg0, out cmd))
            {
                Owner.Console.WriteLine("Unknown command: " + arg0, Style.Error);
                return 0;
            }
            return FuncBehavior.Execute<string[], int?>(cmd.ExecuteLine, subargs);
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            if (args.Length <= 1)
            {
                if (endswithspace)
                {
                    options = Owner.Commands.Keys;
                    return true;
                }
                else
                {
                    options = new[] { " " };
                    return false;
                }
            }
            var subargs = new string[args.Length - 1];
            Array.Copy(args, 1, subargs, 0, args.Length - 1);

            var arg0 = subargs[0];
            ICommand cmd;
            if (!Owner.Commands.TryGetValue(arg0, out cmd))
            {
                options = Owner.GetPartialStringList(arg0, Owner.Commands.Keys);
                return true;
            }
            // request space after first commands
            if (subargs.Length == 1 && !endswithspace)
            {
                options = new[] { " " };
                return false;
            }
            return cmd.TryGetOptions(subargs, endswithspace, out options);
        }
    }

}