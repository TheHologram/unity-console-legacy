using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    abstract class BaseMethodCommand 
    {
        public BaseMethodCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; private set; }

        protected abstract UnityCommandLine.TypeOptions TypeOptions { get; }

        public object GetArgument(string[] args, int offset)
        {
            if (offset < 0 || offset >= args.Length)
                return null;
            var arg = args[offset];
            if (arg.StartsWith("$"))
            {
                object value;
                if (Owner.Variables.TryGetValue(arg, out value))
                    return value;
                Owner.Console.Write("Unable to locate variable: ", Style.Error);
                Owner.Console.WriteLine(arg, Style.Warning);
                return null;
            }
            return arg;
        }

        public bool TryGetOptions(string[] args, int offset, bool endswithspace, out IEnumerable<string> options)
        {
            options = null;
            if (args.Length == offset)
            {
                var list = new List<string>();
                list.AddRange(Owner.GetTypeNames());
                list.AddRange(Owner.Variables.Keys);
                options = list;
                return true;
            }

            string arg1 = args[offset];
            bool isVariable = arg1.StartsWith("$");
            object value;
            Type t;
            if (!Owner.TryGetArgumentType(arg1, out value, out t))
            {
                var list = new List<string>();
                options = isVariable 
                    ? Owner.GetPartialStringList(arg1, Owner.Variables.Keys) 
                    : Owner.GetPartialStringList(arg1, Owner.GetTypeNames(arg1));
                return true;
            }

            // handle variable or types names in commands
            if (args.Length == offset+1)
            {
                var methods = isVariable
                    ? Owner.GetInstanceMembers(t, TypeOptions)
                    : Owner.GetStaticMembers(t, TypeOptions);
                options = Owner.GetMemberNames(methods);
                return true;
            }

            // handle method names in commands
            if (args.Length == offset+2)
            {
                var membername = args[offset+1];

                var methods = isVariable
                    ? Owner.GetInstanceMembers(t, TypeOptions)
                    : Owner.GetStaticMembers(t, TypeOptions);
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