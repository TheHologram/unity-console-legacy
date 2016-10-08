using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class CallCommand : BaseMethodCommand, ICommand
    {
        public CallCommand(UnityCommandLine owner) : base(owner) {}

        protected override UnityCommandLine.TypeOptions TypeOptions => UnityCommandLine.TypeOptions.Method;

        public string Name => "call";
        public string Description => "Call Method on Static class or Variable Instance";

        public string Help
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [class] [Method] [args]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} MMInternalClass Method", Name).AppendLine();
                sb.AppendFormat("   {0} $var1 Method", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var console = Owner.Console;
            if (args.Length <= 2)
            {
                console.Write("Insufficient arguments", Style.Error);
                return 0;
            }
            var typename = args[1];
            var membername = args[2];
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
            const int ArgOffset = 3;
            var argCount = args.Length - ArgOffset;

            var methods = isVariable
                ? Owner.GetInstanceMembers(t, TypeOptions)
                : Owner.GetStaticMembers(t, TypeOptions);

            bool foundMember = false;
            bool foundExact = false;
            MethodInfo member = null;
            foreach (MethodInfo method in methods)
            {
                if (method.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                {
                    foundMember = true;
                    if (method.GetParameters().Length == argCount)
                    {
                        member = method;
                        foundExact = true;
                        break;
                    }
                }
            }
            if (!foundMember)
            {
                console.Write("Unable to resolve find method: ", Style.Error);
                console.Write(membername, Style.Warning);
                console.WriteLine();
                return 0;
            }
            else if (!foundExact)
            {
                console.Write("Unable to resolve find method ", Style.Error);
                console.Write(membername, Style.Warning);
                console.Write(" with ", Style.Error);
                console.Write(argCount.ToString(), Style.Warning);
                console.Write(" arguments", Style.Error);
                console.WriteLine();
                return 0;
            }

            object[] methodargs = new object[argCount];
            var parameters = member.GetParameters();
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodargs[index] = Convert.ChangeType(GetArgument(args, ArgOffset + index), parameter.ParameterType);
            }
            var result = member.Invoke(instance, methodargs);
            Owner.PrintResult(result);
            return 0;
        }
        
        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            return base.TryGetOptions(args, 1, endswithspace, out options);
        }
    }
}