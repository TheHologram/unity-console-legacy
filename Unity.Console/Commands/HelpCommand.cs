using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class HelpCommand : ICommand
    {
        public HelpCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        public string Name => "help";

        public string Description => "Display Help with commands";

        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [command]", Name).AppendLine();

                int length = 5;
                foreach (var kvp in Owner.Commands)
                {
                    if (kvp.Key.Length > length)
                        length = kvp.Key.Length;
                }
                string format = string.Format("{{0,-{0}}} - {{1}}", length);
                foreach (var kvp in Owner.Commands)
                    sb.AppendFormat(format, kvp.Key, kvp.Value.Description).AppendLine();
                sb.AppendFormat(format, "^Z","Close Console (Control-Z + Enter)").AppendLine();
                sb.AppendLine();

                sb.AppendLine(" Examples:");
                sb.AppendLine("   help");
                sb.AppendLine("   help list");
                sb.AppendLine("   help call");
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            if (args.Length > 1)
            {
                ICommand cmd;
                if (Owner.Commands.TryGetValue(args[1], out cmd))
                {
                    Owner.Console.WriteLine(cmd.Help, Style.Info);
                    return 0;
                }
            }
            Owner.Console.WriteLine(Help, Style.Info);
            return 0;
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            if (args.Length == 1)
            {
                if (endswithspace)
                    options = Owner.Commands.Keys;
                else
                    options = new[] { " " };
                return true;
            }

            options = null;
            return false;
        }
    }

}