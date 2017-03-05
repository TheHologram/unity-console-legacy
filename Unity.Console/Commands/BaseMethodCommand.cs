using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console.Commands
{
    public abstract class BaseMethodCommand
    {
        public BaseMethodCommand(UnityCommandLine owner)
        {
            Owner = owner;
        }

        public UnityCommandLine Owner { get; }

        protected abstract UnityCommandLine.TypeOptions TypeOptions { get; }

        public void SetResult(object arg)
        {
            if (arg == null)
                ClearResult();
            else
                Owner.Variables["$_"] = arg;
        }

        public void ClearResult()
        {
            Owner.Variables.Remove("$_");
        }

        public object GetArgument(string[] args, int offset)
        {
            if (offset < 0 || offset >= args.Length)
                return null;
            var arg = args[offset];
            Owner.Console.Write("Args: " + offset.ToString() + " " + arg + " = ", Style.Info);
            if (arg.StartsWith("$"))
            {
                object value;
                if (Owner.Variables.TryGetValue(arg, out value))
                {
                    Owner.Console.Write(value.GetType().ToString(), Style.Info);
                    Owner.Console.WriteLine();
                    return value;
                }

                Owner.Console.Write("Unable to locate variable: ", Style.Error);
                Owner.Console.WriteLine(arg, Style.Warning);
                return null;
            }
            Owner.Console.Write(arg, Style.Info);
            Owner.Console.WriteLine();
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

        public object ProcessValueArgs(object obj, string[] args, int offset)
        {
            object value;
            TryProcessValueArgs(obj, args, ref offset, out value);
            return value;
        }

        public bool TryGetInteger(object arg, out int result)
        {
            try
            {
                result = Convert.ToInt32(arg);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        public bool TryProcessValueArgs(object obj, string[] args, ref int index, out object value)
        {
            var console = Owner.Console;
            value = null;
            if (obj == null)
                return false;

            var arg = Owner.GetVariable(args[index]);
            if (arg is string && arg.ToString().StartsWith("$"))
            {
                if (!Owner.TryGetVariable(arg.ToString(), out value))
                {
                    console.Write("Unable to resolve variable: ", Style.Error);
                    console.Write(arg.ToString(), Style.Warning);
                    console.WriteLine();
                    return false;
                }
                arg = value;
            }

            var idx = 0;
            int aoff = 0, alast = -1;
            if (obj is ArraySegment)
            {
                var seg = (ArraySegment)obj;
                obj = seg.Array;
                aoff = seg.Offset;
                alast = seg.Count + seg.Offset;
            }
            if (obj is Array)
            {
                var arr = (Array)obj;
                if (arr.Rank == 1 && alast == -1)
                    alast = arr.Length - 1;
                if (arr.Rank == 1 && arg is string && arg.ToString().Contains(".."))
                {
                    var span = arg.ToString().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (span.Length != 2)
                        return false;
                    int first, last;
                    if (!int.TryParse(span[0], out first) || !int.TryParse(span[1], out last))
                        return false;
                    value = new ArraySegment(arr, first + aoff, last - first);
                    return true;
                }
                else if (arr.Rank <= args.Length - index)
                {
                    var len = index + arr.Rank;
                    var indices = new int[arr.Rank];
                    indices[0] = aoff;
                    for (var j = 0; j < arr.Rank && index < len; index++, j++)
                    {
                        arg = Owner.GetVariable(args[index]);
                        if (!TryGetInteger(arg, out idx))
                            return false;
                        indices[j] += idx;
                    }
                    if (indices[0] < aoff || indices[0] > alast) throw new IndexOutOfRangeException();
                    value = arr.GetValue(indices);
                    --index; // give back one index
                    return true;
                }
            }
            else if (obj is IList)
            {
                var list = (IList)obj;
                if (!TryGetInteger(arg, out idx))
                    return false;
                value = list[idx];
                return true;
            }
            else if (obj is IDictionary)
            {
                var dict = (IDictionary)obj;
                if (dict.Contains(arg))
                {
                    value = dict[arg];
                    return true;
                }
                else if (TryGetInteger(arg, out idx))
                {
                    idx = (int)arg;
                    if (!dict.Contains(idx))
                        return false;
                    value = dict[idx];
                    return true;
                }
            }
            else if (obj is ICollection)
            {
                if (!TryGetInteger(arg, out idx))
                    return false;
                int i = 0;
                foreach (var val in (ICollection)obj)
                {
                    if (i++ == idx)
                    {
                        value = val;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Process single argument of instance.  If instance is type then get static properties
        /// </summary>
        public bool ProcessProperty(object instance, string[] args, ref int offset, out object value)
        {
            value = null;
            var console = Owner.Console;
            try
            {
                Type t = (instance is Type) ? (Type)instance : instance?.GetType();
                var arg = args[offset];
                if (instance is ICollection)
                {
                    // processed at least one
                    if (TryProcessValueArgs(instance, args, ref offset, out value))
                        return true;
                    // fall-through and process normally
                }
                bool found = false;
                bool isInstance = !(instance is Type);
                var membername = arg;
                if (!found)
                {
                    foreach (PropertyInfo prop in Owner.GetMembers(t, isInstance, UnityCommandLine.TypeOptions.GetProperty))
                    {
                        if (prop.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var propList = Owner.GetMethodsSpecial(t, isInstance, "get_" + prop.Name);
                            if (propList.Count > 0)
                            {
                                var property = propList[0];
                                value = property.Invoke(instance, new object[0]);
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    foreach (FieldInfo field in Owner.GetMembers(t, isInstance, UnityCommandLine.TypeOptions.Field))
                    {
                        if (field.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                        {
                            value = field.GetValue(instance);
                            found = true;
                            break;
                        }
                    }
                }
                //if (!found)
                //{
                //    foreach (MethodInfo method in Owner.GetMembers(t, isInstance, UnityCommandLine.TypeOptions.Method))
                //    {
                //        if (method.Name.Equals(membername, StringComparison.CurrentCultureIgnoreCase))
                //        {
                //            int argCount = args.Length - offset;
                //            object[] methodargs = new object[argCount];
                //            var parameters = method.GetParameters();
                //            for (int index = 0; index < parameters.Length; index++)
                //            {
                //                var parameter = parameters[index];
                //                methodargs[index] = Convert.ChangeType(GetArgument(args, offset + index), parameter.ParameterType);
                //            }
                //            value = method.Invoke(instance, methodargs);
                //            found = true;
                //            break;
                //        }
                //    }
                //}
                if (!found)
                {
                    console.Write("Unable to resolve field or property: ", Style.Error);
                    console.Write(membername, Style.Warning);
                    console.WriteLine();
                    return false;
                }
            }
            catch (Exception ex)
            {
                console.Write("Exception: ", Style.Error);
                console.Write(ex.Message, Style.Warning);
                console.WriteLine();
                value = null;
                return false;
            }
            return true;
        }
    }
}