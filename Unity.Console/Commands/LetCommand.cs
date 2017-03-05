using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    [CommandAttribute("let")]
    class LetCommand : BaseMethodCommand, ICommand
    {
        public LetCommand(UnityCommandLine owner) : base(owner) {}

        protected override UnityCommandLine.TypeOptions TypeOptions => UnityCommandLine.TypeOptions.All;

        public string Name => "let";
        public string Description => "Set field or property value";

        public string Help
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [class] [Member] [Args]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} $var1 = MMInternalClass Field", Name).AppendLine();
                sb.AppendFormat("   {0} $var1 = MMInternalClass Method \"Arg1\" 2", Name).AppendLine();
                sb.AppendFormat("   {0} $var2 = $var1 StringProperty", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var console = Owner.Console;
            if (args.Length <= 1)
            {
                console.WriteLine("Insufficient arguments. Variable name is missing", Style.Error);
                return 0;
            }

            var valuename = args[1];
            if (!valuename.StartsWith("$"))
            { 
                console.WriteLine("Variable name must start with $", Style.Warning);
                return 0;
            }
            if (args.Length <= 2)
            {
                console.WriteLine("Insufficient arguments. Equals is missing", Style.Error);
                return 0;
            }
            var equals = args[2];
            if (!equals.StartsWith("="))
            {
                console.WriteLine("Equals after variable missing", Style.Error);
                return 0;
            }
            if (args.Length <= 3)
            {
                console.WriteLine("Insufficient arguments. Type or variable is missing", Style.Error);
                return 0;
            }
            var typename = args[3];
            if (typename == "call") // special bypass to call a method and use result
            {
                ICommand cmd;
                if (!Owner.Commands.TryGetValue(typename, out cmd))
                {
                    console.WriteLine("Call Command is missing", Style.Error);
                    return 0;
                }
                Owner.Variables.Remove("$_");
                var callcmd = cmd as CallCommand;
                if (callcmd != null)
                {
                    object callres;
                    var subargs = new string[args.Length - 3];
                    Array.Copy(args, 3, subargs, 0, subargs.Length);
                    callcmd.ExecuteLine(subargs, false);
                    if (Owner.TryGetVariable("$_", out callres))
                        Owner.Variables[valuename] = callres;
                    Owner.Variables.Remove("$_");
                }
                return 0;
            }
            bool isVariable = typename.StartsWith("$");

            object instance = null;
            Type t = null;
            if (!Owner.TryGetArgumentType(typename, out instance, out t))
            {
                Owner.Variables[valuename] = typename;
                Owner.Variables.Remove("$_");
                return 0;
            }

            if (args.Length <= 4)
            {
                if (isVariable)
                {
                    Owner.Variables[valuename] = instance;
                    Owner.Variables.Remove("$_");
                }
                else
                {
                    console.WriteLine("Insufficient arguments. Method, field or property name is missing", Style.Error);
                    return 0;
                }
            }
            if (instance == null)
                instance = t;
            bool found = false;
            object result = null;
            int idx = 4;
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
                //result = ProcessValueArgs(result, args, idx);
                Owner.Variables[valuename] = result;
                //Owner.PrintResult(result);
                Owner.Variables.Remove("$_");
            }
            return 0;
        }
        
        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            options = null;
            if (args.Length < 2)
            {
                options = new[] { "$" };
                return true;
            }
            if (args.Length == 2)
            {
                if (args[1].StartsWith("$") && args[1].Length > 1)
                {
                    options = new[] { endswithspace ? "=" : " =" } ;
                    return true;
                }
                return false;
            }
            if (args.Length == 3)
            {
                if (args[2] == "=")
                {
                    if (!endswithspace)
                    {
                        options = new[] {" "};
                        return true;
                    }
                }
            }
            if (args.Length > 3)
            {
                var arg3 = args[3];
                if (arg3 == "call") // special bypass to call a method and use result
                {
                    ICommand cmd;
                    if (!Owner.Commands.TryGetValue(arg3, out cmd))
                    {
                        Owner.Console.WriteLine("Call Command is missing", Style.Error);
                        return false;
                    }
                    var subargs = new string[args.Length - 3];
                    Array.Copy(args, 2, subargs, 0, subargs.Length);
                    return cmd.TryGetOptions(subargs, endswithspace, out options);
                }
            }

            return TryGetOptions(args, 3, endswithspace, out options);
        }
    }
}