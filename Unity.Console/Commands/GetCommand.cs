using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class GetCommand : BaseMethodCommand, ICommand
    {
        public GetCommand(UnityCommandLine owner) : base(owner) {}

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
            int ArgOffset = 3;
            bool found = false;
            object result = null;
            if (args.Length >= ArgOffset)
            {
                int idx;
                var membername = args[2];
                if (isVariable && instance is Array && (membername.StartsWith("$") || int.TryParse(membername, out idx)))
                {
                    --ArgOffset;
                    result = instance;
                    found = true;
                }
                else
                {
                    foreach (PropertyInfo prop in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.GetProperty))
                    {
                        if (prop.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var propList = Owner.GetMethodsSpecial(t, isVariable, "get_" + prop.Name);
                            if (propList.Count > 0)
                            {
                                var property = propList[0];
                                result = property.Invoke(instance, new object[0]);
                                found = true;
                                break;
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
            }
            if (found)
            {
                // possible array
                for (int index = ArgOffset; index < args.Length; index++)
                {
                    var arg = args[index];
                    if (result == null)
                        break;
                    int idx = 0;
                    if (result is Array && int.TryParse(arg, out idx))
                    {
                        var arr = (Array) result;
                        if (arr.Rank == 1)
                            result = arr.GetValue(idx);
                    }
                }
                Owner.PrintResult(result);
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