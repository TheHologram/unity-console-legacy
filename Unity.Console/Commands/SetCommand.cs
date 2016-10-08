using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class SetCommand : BaseMethodCommand, ICommand
    {
        public SetCommand(UnityCommandLine owner) : base(owner) {}

        protected override UnityCommandLine.TypeOptions TypeOptions => UnityCommandLine.TypeOptions.Field | UnityCommandLine.TypeOptions.GetProperty;

        public string Name => "set";
        public string Description => "Set field or property value";

        public string Help
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [class] [Member] [Value]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0} MMInternalClass Field 123", Name).AppendLine();
                sb.AppendFormat("   {0} $var1 StringProperty \"asdf\"", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var console = Owner.Console;
            if (args.Length <= 3)
            {
                console.Write("Insufficient arguments", Style.Error);
                return 0;
            }
            var typename = args[1];
            var membername = args[2];
            var valueString = args[3];
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

            foreach (PropertyInfo prop in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.GetProperty))
            {
                if (prop.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                {
                    var propList = Owner.GetMethodsSpecial(t, isVariable, "set_" + prop.Name);
                    if (propList.Count > 0)
                    {
                        var property = propList[0];
                        var value = Convert.ChangeType(valueString, prop.PropertyType);
                        property.Invoke(instance, new object[] { value });
                        Owner.PrintResult(value);
                        return 0;
                    }
                }
            }

            foreach (FieldInfo field in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.Field))
            {
                if (field.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                {
                    var value = Convert.ChangeType(valueString, field.FieldType);
                    field.SetValue(instance, value);
                    var result = field.GetValue(instance);
                    Owner.PrintResult(result);
                    return 0;
                }
            }

            console.Write("Unable to resolve find field or property: ", Style.Error);
            console.Write(membername, Style.Warning);
            console.WriteLine();
            return 0;
        }
        
        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            return TryGetOptions(args, 1, endswithspace, out options);
        }
    }
}