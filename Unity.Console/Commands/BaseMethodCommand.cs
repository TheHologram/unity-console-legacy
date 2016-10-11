using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    internal abstract class BaseMethodCommand
    {
        public BaseMethodCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

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

            var arg1 = args[offset];
            var isVariable = arg1.StartsWith("$");
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
            if (args.Length == offset + 1)
            {
                var methods = isVariable
                    ? Owner.GetInstanceMembers(t, TypeOptions)
                    : Owner.GetStaticMembers(t, TypeOptions);
                options = Owner.GetMemberNames(methods);
                return true;
            }

            // handle method names in commands
            if (args.Length == offset + 2)
            {
                var membername = args[offset + 1];

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

        internal static object ProcessValueArgs(object obj, string[] args, int offset)
        {
            for (var index = offset; index < args.Length; index++)
            {
                var arg = args[index];
                if (obj == null)
                    break;
                var idx = 0;
                int aoff = 0, alast = -1;
                if (obj is ArraySegment)
                {
                    var seg = (ArraySegment) obj;
                    obj = seg.Array;
                    aoff = seg.Offset;
                    alast = seg.Count + seg.Offset;
                }
                if (obj is Array)
                {
                    var arr = (Array) obj;
                    if (arr.Rank == 1 && alast == -1)
                        alast = arr.Length - 1;
                    if (arr.Rank == 1 && arg.Contains(".."))
                    {
                        var span = arg.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                        if (span.Length == 2)
                        {
                            int first, last;
                            if (int.TryParse(span[0], out first) && int.TryParse(span[1], out last))
                            {
                                obj = new ArraySegment(arr, first + aoff, last - first);
                            }
                        }
                    }
                    else if (arr.Rank == args.Length - index)
                    {
                        var indices = new int[arr.Rank];
                        indices[0] = aoff;
                        for (var j = 0; j < arr.Rank && index < args.Length; index++, j++)
                        {
                            arg = args[index];
                            if (int.TryParse(arg, out idx))
                                indices[j] += idx;
                        }
                        if (indices[0]<aoff || indices[0] > alast) throw new IndexOutOfRangeException();
                        obj = arr.GetValue(indices);
                    }
                }
                else if (obj is IList)
                {
                    var list = (IList) obj;
                    if (int.TryParse(arg, out idx))
                    {
                        obj = list[idx];
                    }
                }
                else if (obj is IDictionary)
                {
                    var dict = (IDictionary)obj;
                    if ( dict.Contains(arg) )
                        obj = dict[arg];
                    else if (int.TryParse(arg, out idx))
                    {
                        if (dict.Contains(idx))
                            obj = dict[idx];
                    }
                }
                else if (obj is ICollection)
                {
                    if (int.TryParse(arg, out idx))
                    {
                        int i = 0;
                        foreach (var val in (ICollection) obj)
                        {
                            if (i++ == idx)
                            {
                                obj = val;
                                break;
                            }
                        }
                    }
                }
            }
            return obj;
        }
    }
}