using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("clear")]
    class ClearCommand : ICommand
    {
        public ClearCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        public string Name => "clear";

        public string Description => "Clear Screen";

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
            Owner.Console.Clear();
            return 0;
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            options = null;
            return false;
        }
    }

}