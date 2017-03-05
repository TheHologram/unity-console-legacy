using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("sleep")]
    class SleepCommand : ICommand
    {
        public SleepCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        public string Name => "sleep";

        public string Description => "Sleep for a short period of time";

        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [seconds]", Name).AppendLine();
                sb.AppendLine("    Sleeps for a short period of time (in seconds). Defaults to 1 second");
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0}", Name).AppendLine();
                sb.AppendFormat("   {0} 5", Name).AppendLine();
                sb.AppendFormat("   {0} 0.5", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var ts = TimeSpan.FromSeconds(1);
            if (args.Length > 1)
            {
                string arg = args[1];
                double val;
                if (double.TryParse(arg, out val))
                    ts = TimeSpan.FromSeconds(val);
            }
            System.Threading.Thread.Sleep(ts);
            return 0;
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            if (args.Length == 1)
            {
                if (!endswithspace)
                {
                    options = new[] {" "};
                    return true;
                }
            }
            options = null;
            return false;
        }
    }

}