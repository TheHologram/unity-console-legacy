using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    class ListCommand : ICommand
    {
        public ListCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; private set; }

        public string Name => "list";

        public string Description => "List Properties and Methods of class";

        public string Help
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                sb.AppendFormat("Usage:   {0} [class]", Name).AppendLine();
                sb.AppendLine(" Examples:");
                sb.AppendFormat("   {0}", Name).AppendLine();
                sb.AppendFormat("   {0} MMInternalClass", Name).AppendLine();
                sb.AppendFormat("   {0} $var1", Name).AppendLine();
                return sb.ToString();
            }
        }

        public int? ExecuteLine(string[] args)
        {
            var console = Owner.Console;
            if (args.Length <= 1)
            {
                foreach (var type in Owner.GetTypeNames())
                    console.WriteLine(string.Format("  {0} {1}",Name,type), Style.Info);
                foreach (var type in Owner.Variables.Keys)
                    console.WriteLine(string.Format("  {0} {1}", Name, type), Style.Info);
                return 0;
            }
            var typename = args[1];
            bool isVariable = typename.StartsWith("$");

            if (args.Length == 2)
            {
                IEnumerable<string> options = isVariable 
                    ? (IEnumerable<string>)Owner.Variables.Keys
                    : (IEnumerable<string>)Owner.GetTypeNames(typename)
                    ;
                var partialNames = Owner.GetPartialStringList(typename, options);
                if (partialNames.Count == 1 && string.IsNullOrEmpty(partialNames[0]))
                {
                    WriteInfo(typename, isVariable);
                }
                else
                {
                    foreach (var part in partialNames)
                        console.WriteLine(string.Format("  {0} {1}{2}", Name, typename, part), Style.Info);
                }
                return 0;
            }
            var methodname = args[2];
            if (args.Length >= 3)
            {
                object value;
                Type t;
                if (!Owner.TryGetArgumentType(typename, out value, out t))
                {
                    console.WriteLine("Unable to resolve type: " + typename, Style.Error);
                    return 0;
                }
                WriteInfo(typename, methodname, isVariable);
            }
            return null;
        }

        private void WriteInfo(string typename, bool isVariable)
        {
            WriteInfo(typename, "", isVariable);
        }

        private void WriteInfo(string typename, string methodName, bool isVariable)
        {
            var isPartial = !string.IsNullOrEmpty(methodName);
            var console = Owner.Console;
            Type t;
            object value;
            if (Owner.TryGetArgumentType(typename, out value, out t))
            {
                var partialmatch = Owner.BuildRegexFromWildcard(methodName);
                foreach (MethodInfo method in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.Method))
                {
                    if (method.IsSpecialName) continue;
                    if (!isPartial || partialmatch.IsMatch(method.Name))
                    {
                        console.Write(string.Format("call {0} {1}", typename, method.Name), Style.Info);
                        foreach (var parameter in method.GetParameters())
                            console.Write(string.Format(" <{0} [{1}]>", parameter.Name, parameter.ParameterType.Name),Style.Info);
                        console.WriteLine();
                    }
                }
                foreach (FieldInfo field in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.Field))
                {
                    if (field.IsSpecialName) continue;
                    if (!isPartial || partialmatch.IsMatch(field.Name))
                        console.WriteLine(string.Format("get {0} {1,-16} [{2}]", typename, field.Name, field.FieldType.Name),Style.Info);
                }
                foreach (PropertyInfo property in Owner.GetMembers(t, isVariable, UnityCommandLine.TypeOptions.GetProperty))
                {
                    if (property.IsSpecialName) continue;
                    if (!isPartial || partialmatch.IsMatch(property.Name))
                        console.WriteLine(string.Format("get {0} {1,-16} [{2}]", typename, property.Name, property.PropertyType.Name), Style.Info);
                }
            }
        }

        public bool TryGetOptions(string[] args, bool endswithspace, out IEnumerable<string> options)
        {
            options = null;
            if (args.Length == 1)
            {
                var list = new List<string>();
                list.AddRange(Owner.GetTypeNames());
                list.AddRange(Owner.Variables.Keys);
                options = list;
                return true;
            }

            string arg1 = args[1];
            bool isVariable = arg1.StartsWith("$");

            Type t;
            object value;
            if (!Owner.TryGetArgumentType(arg1, out value, out t))
            {
                var list = new List<string>();
                options = isVariable
                    ? Owner.GetPartialStringList(arg1, Owner.Variables.Keys)
                    : Owner.GetPartialStringList(arg1, Owner.GetTypeNames(arg1));
                return true;
            }


            // handle variable or types names in commands
            if (args.Length == 2)
            {
                var methods = isVariable
                    ? Owner.GetInstanceMembers(t, UnityCommandLine.TypeOptions.All)
                    : Owner.GetStaticMembers(t, UnityCommandLine.TypeOptions.All);
                options = Owner.GetMemberNames(methods);
                return true;
            }

            // handle method names in commands
            if (args.Length == 3)
            {
                var membername = args[2];

                var methods = isVariable
                    ? Owner.GetInstanceMembers(t, UnityCommandLine.TypeOptions.All)
                    : Owner.GetStaticMembers(t, UnityCommandLine.TypeOptions.All);
                var memberNames = Owner.GetMemberNames(methods);
                if (!endswithspace)
                {
                    var list = Owner.GetPartialStringList(membername, memberNames);
                    if (list.Count == 0) return false;
                    options = list;
                    return true;
                }
            }
            return false;
        }
    }
}