using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("reset")]
    class ResetCommand : ICommand
    {
        public ResetCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        public string Name => "reset";

        public string Description => "Reset variables";

        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [var]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0}", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            if (args.Length == 1)
            {
                Owner.Variables.Clear();
            }
            for (int index = 1; index < args.Length; index++)
            {
                var arg = args[index];
                if (!Owner.Variables.Remove(arg))
                {
                    Owner.Console.Write("Unable to find variable: ", Style.Info);
                    Owner.Console.Write(arg, Style.Warning);
                }
            }
            return 0;
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            if (args.Length == 1)
            {
                if (endswithspace)
                {
                    var list = new List<string>();
                    list.AddRange(Owner.Variables.Keys);
                    options = list;
                    return true;
                }
                options = new[] {" "};
                return true;
            }
            if (endswithspace)
            {
                options = Owner.Variables.Keys;
                return true;
            }
            var argname = args[args.Length - 1];
            options = Owner.GetPartialStringList(argname, Owner.Variables.Keys);
            return false;
        }
    }

}