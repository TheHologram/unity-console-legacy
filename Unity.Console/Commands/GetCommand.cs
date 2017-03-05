using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("get")]
    class GetCommand : BaseMethodCommand, ICommand
    {
        public GetCommand(UnityCommandLine owner) : base(owner) { }

        protected override UnityCommandLine.TypeOptions TypeOptions => UnityCommandLine.TypeOptions.Field | UnityCommandLine.TypeOptions.GetProperty;

        public string Name => "get";
        public string Description => "Get field or property value";

        public string Help
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [class] [Member]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} MMInternalClass Field", Name).AppendLine();
                sb.AppendFormat("   {0} $var1 Property", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var console = Owner.Console;
            if (args.Length < 2)
            {
                console.WriteLine("Insufficient arguments", Style.Error);
                return 0;
            }
            var typename = args[1];
            bool isVariable = typename.StartsWith("$");

            object instance = null;
            Type t = null;
            if (!Owner.TryGetArgumentType(typename, out instance, out t))
            {
                console.Write(isVariable ? "Unable to resolve variable: " : "Unable to resolve type: ", Style.Error);
                console.Write(typename, Style.Warning);
                console.WriteLine();
                return 1;
            }
            if (args.Length == 2)
            {
                if (isVariable)
                {
                    Owner.PrintResult(instance);
                    return 0;
                }
                else
                {
                    console.WriteLine("Insufficient arguments", Style.Error);
                    return 0;
                }
            }

            if (instance == null)
                instance = t;
            bool found = false;
            object result = null;
            int idx = 2;
            var arg = args[idx];
            if (arg.StartsWith("$"))
            {
                idx++;
                if (!Owner.TryGetVariable(arg, out instance))
                {
                    console.Write("Unable to resolve variable: ", Style.Error);
                    console.Write(arg, Style.Warning);
                    console.WriteLine();
                    return 1;
                }
            }
            for (; idx < args.Length; ++idx)
            {
                if (ProcessProperty(instance, args, ref idx, out result))
                {
                    instance = result;
                    found = true;
                }
                else
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                Owner.PrintResult(result);
                SetResult(result);
                return 0;
            }
            return 0;
        }

        
        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            return TryGetOptions(args, 1, endswithspace, out options);
        }
    }
}