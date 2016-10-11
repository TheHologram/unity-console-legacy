using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
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
            if (args.Length <= 4)
            {
                console.WriteLine("Insufficient arguments. Method, field or property name is missing", Style.Error);
                return 0;
            }
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
            int ArgOffset = 5;
            bool found = false;
            object result = null;
            if (args.Length >= ArgOffset)
            {
                int idx;
                var membername = args[4];
                if (isVariable && instance is ICollection)
                {
                    --ArgOffset;
                    result = instance;
                    found = true;
                }
                if (!found)
                { 
                    foreach (MethodInfo method in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.Method))
                    {
                        if (method.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            int argCount = args.Length - ArgOffset;
                            object[] methodargs = new object[argCount];
                            var parameters = method.GetParameters();
                            for (int index = 0; index < parameters.Length; index++)
                            {
                                var parameter = parameters[index];
                                methodargs[index] = Convert.ChangeType(GetArgument(args, ArgOffset + index), parameter.ParameterType);
                            }
                            result = method.Invoke(instance, methodargs);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    foreach (
                        PropertyInfo prop in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.GetProperty))
                    {
                        if (prop.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var propList = Owner.GetMethodsSpecial(t, isVariable, "get_" + typename);
                            if (propList.Count > 0)
                            {
                                var property = propList[0];
                                result = property.Invoke(instance, new object[0]);
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    foreach (FieldInfo field in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.Field))
                    {
                        if (field.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            result = field.GetValue(instance);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    console.Write("Unable to resolve find field or property: ", Style.Error);
                    console.Write(membername, Style.Warning);
                    console.WriteLine();
                    return 0;
                }
            }
            if (found)
            {
                result = ProcessValueArgs(result, args, ArgOffset);
                Owner.Variables[valuename] = result;
                //Owner.PrintResult(result);
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
            return TryGetOptions(args, 3, endswithspace, out options);
        }
    }
}