using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;

namespace Unity.Console
{
    public class UnityCommandLine : CommandLine
    {
        #region CommandLine-like Parser
        static string[] ParseLine(string command)
        {
            var args = new List<string>();
            const char dquotechar = '\"';
            const char slashchar = '\\';
            var s = command;
            var i = 0;
            var n = s.Length;
            if (n == 0) return new string[0];

            var arg = new StringBuilder(n);
            for (; i < n;)
            {
                // ignore space between args
                var c = s[i];
                if (char.IsWhiteSpace(c)) { ++i; continue; }

                // handle next arg
                var inquote = false;
                for (; i < n; ++i)
                {
                    c = s[i];
                    var copychar = true;
                    var numslash = 0;
                    while (c == slashchar)
                    {
                        ++i;
                        if (++i >= n) break;
                        c = s[i];
                        ++numslash;
                    }
                    if (i >= n) break;
                    if (c == dquotechar)
                    {
                        if (numslash % 2 == 0)
                        {
                            if (inquote)
                            {
                                if ((i + 1) < n && s[i + 1] == dquotechar)
                                    c = s[i++];
                                else
                                    copychar = false;
                            }
                            else
                                copychar = false;
                            inquote = !inquote;
                        }
                        numslash /= 2;
                    }
                    while (numslash-- != 0)
                        arg.Append(slashchar);
                    if (!inquote && char.IsWhiteSpace(c))
                        break;
                    if (copychar)
                        arg.Append(c);
                }

                args.Add(arg.ToString());
                arg.Length = 0;
            }
            return args.ToArray();
        }
        #endregion

        string[] commands = { "call", "get", "set", "list", "clear", "help" };
        List<Type> types = new List<Type>();
        
        protected override int? ExecuteLine(string s)
        {
            string[] args = ParseLine(s);
            if (args.Length == 0) return 0;
            var arg0 = args[0].ToLower();
            if (arg0.StartsWith("help") || arg0.StartsWith("?"))
            {
                _console.WriteLine("help  - Display this help", Style.Info);
                _console.WriteLine("call  - Call Object Static method", Style.Info);
                _console.WriteLine("get   - Get field or property value", Style.Info);
                _console.WriteLine("set   - Set field or property value", Style.Info);
                _console.WriteLine("list  - List Properties and Methods of class", Style.Info);
                _console.WriteLine("clear - Clear screen", Style.Info);
                _console.WriteLine("^Z    - Close Console (Control-Z + Enter)", Style.Info);
                return 0;
            }
            if (Array.IndexOf(commands, arg0) < 0)
            {
                _console.WriteLine("Unknown command: " + arg0, Style.Error);
                return 0;
            }
            if (arg0 == "exit")
            {
                _console.WriteLine("Use ^Z (Control-Z) + Enter to close console. ", Style.Warning);
                _console.WriteLine("  - Careful though as it may not be possible open console again", Style.Info);
                return 0;
                //throw new ShutdownConsoleException();
            }
            if (arg0 == "cls" || arg0 == "clear" || arg0 == "clr")
            {
                _console.Clear();
                return 0;
            }
            if (arg0 == "list" && args.Length == 1)
            {
                foreach (var type in types)
                    _console.WriteLine("  " + type.FullName, Style.Info);
                return 0;
            }
            if (args.Length < 2)
            {
                _console.WriteLine("Insufficient arguments", Style.Error);
                return 0;
            }
            var arg1 = args[1];
            var arg2 = args.Length >=3 ? args[2] : "";
            Type t;
            if (!TryGetType(arg1, out t))
            {
                _console.WriteLine("Unable to resolve type: " + arg1, Style.Error);
                return 0;
            }
            var argCount = args.Length - 3;
            switch (arg0)
            {
                case "call":
                    {
                        var memberList = GetMethodsExact(t, arg2);
                        if (memberList.Count == 0)
                        {
                            _console.WriteLine("Unable to resolve method: " + arg2, Style.Error);
                            return 0;
                        }
                        bool found = false;
                        MethodInfo member = null;
                        foreach (MethodInfo method in memberList)
                        {
                            if (method.GetParameters().Length == argCount)
                            {
                                member = method;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            _console.WriteLine("Unable to resolve method arguments: " + arg2, Style.Error);
                            return 0;
                        }
                        object[] methodargs = new object[argCount];
                        var parameters = member.GetParameters();
                        for (int index = 0; index < parameters.Length; index++)
                        {
                            var parameter = parameters[index];
                            methodargs[index] = Convert.ChangeType(args[3 + index], parameter.ParameterType);
                        }
                        var result = member.Invoke(null, methodargs);
                        PrintResult(result);
                        break;
                    }

                case "get":
                    {
                        if (argCount != 0)
                        {
                            _console.WriteLine("Too many arguments for get request", Style.Error);
                            return 0;
                        }
                        bool found = false;
                        object result = null;
                        var propList = GetMethodsSpecial(t, "get_" + arg2);
                        if (propList.Count > 0)
                        {
                            var property = propList[0];
                            result = property.Invoke(null, new object[0]);
                            found = true;
                        }
                        var fieldList = GetFieldsExact(t, arg2, BindingFlags.GetField);
                        if (fieldList.Count > 0)
                        {
                            var field = fieldList[0];
                            result = field.GetValue(null);
                            found = true;
                        }
                        if (found)
                        {
                            PrintResult(result);
                            return 0;
                        }
                        _console.WriteLine("Unable to resolve property: " + arg2, Style.Error);
                        return 0;
                    }
                case "set":
                    {
                        if (argCount != 1)
                        {
                            _console.WriteLine("Insufficient arguments for set request", Style.Error);
                            return 0;
                        }
                        var valueString = args[3];
                        var methodList = GetMethodsSpecial(t, "set_" + arg2);
                        if (methodList.Count > 0)
                        {
                            var propList = GetPropertiesExact(t, arg2, BindingFlags.SetProperty);
                            if (propList.Count > 0)
                            {
                                var property = propList[0];
                                var method = methodList[0];
                                var value = Convert.ChangeType(valueString, property.PropertyType);
                                method.Invoke(null, new object[] { value });
                                PrintResult(value);
                                return 0;
                            }
                        }
                        var fieldList = GetFieldsExact(t, arg2, BindingFlags.GetField);
                        if (fieldList.Count > 0)
                        {
                            var field = fieldList[0];
                            var value = Convert.ChangeType(valueString, field.FieldType);
                            field.SetValue(null, value);
                            PrintResult(value);
                            return 0;
                        }
                        _console.WriteLine("Unable to resolve property: " + arg2, Style.Error);
                        return 0;
                    }

                case "list":
                    {
                        const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
                        foreach (var method in t.GetMethods(defaultFlags | BindingFlags.InvokeMethod))
                            if (!method.IsSpecialName && !method.IsGenericMethod)
                            {
                                _console.Write(string.Format("call {0} {1,-16}", t.FullName, method.Name), Style.Info);
                                foreach ( var parameter in method.GetParameters())
                                    _console.Write(string.Format("\t {0} [{1}]", parameter.Name, parameter.ParameterType.Name), Style.Info);
                                _console.WriteLine();
                            }
                        foreach (var field in t.GetFields(defaultFlags | BindingFlags.GetField))
                            if (!field.IsSpecialName)
                                _console.WriteLine(string.Format("get  {0} {1,-16}\t {2}", t.FullName, field.Name, field.FieldType.Name), Style.Info);
                        foreach (var property in t.GetProperties(defaultFlags | BindingFlags.GetProperty))
                            if (!property.IsSpecialName)
                                _console.WriteLine(string.Format("get  {0} {1,-16}\t {2}", t.FullName, property.Name, property.PropertyType.Name), Style.Info);
                        return 0;
                    }

            }
            return 0;
        }

        static string EscapeString(string s)
        {
            return s.Replace("\"", "\\\"");
        }

        static void EscapeChar(TextWriter output, char c)
        {
            if (c == '\'')
            {
                output.Write("'\\''");
                return;
            }
            if (c > 32)
            {
                output.Write("'{0}'", c);
                return;
            }
            switch (c)
            {
                case '\a': output.Write("'\\a'"); break;
                case '\b': output.Write("'\\b'"); break;
                case '\n': output.Write("'\\n'"); break;
                case '\v': output.Write("'\\v'"); break;
                case '\r': output.Write("'\\r'"); break;
                case '\f': output.Write("'\\f'"); break;
                case '\t': output.Write("'\\t"); break;
                default: output.Write("'\\x{0:x}", (int)c); break;
            }
        }

        internal void PrettyPrintArrayHeader(TextWriter output, object result)
        {
            if (result == null) return;

            var t = result.GetType();
            if (t.IsClass)
            {
                int width = _console.Width - 5;
                var fi = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                int colWidth = Math.Min(16, Math.Max(5, ((width / fi.Length) + 1)));
                string format = string.Format("{{0,-{0}}} ", colWidth);
                foreach (var f in fi)
                    output.Write(format, f.Name);
            }
        }
        internal void PrettyPrintArrayItem(TextWriter output, int i, object result)
        {
            if (result == null) return;

            output.Write("[{0,2}] ", i);
            var t = result.GetType();
            if (t.IsClass)
            {
                int width = _console.Width - 5;
                var fi = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                int colWidth = Math.Min(16, Math.Max(5, ((width / fi.Length) + 1)));
                string format = string.Format("{{0,-{0}}} ", colWidth);
                foreach (var f in fi)
                    output.Write(format, f.GetValue(result));
                output.WriteLine();
            }
            else
            {
                PrettyPrint(output, result);
                output.Write('\t');
            }
        }

        internal void PrettyPrint(TextWriter output, object result)
        {
            if (result == null)
            {
                output.Write("null");
                return;
            }

            if (result is Array)
            {
                Array a = (Array)result;
                int lower = a.GetLowerBound(0);
                int top = a.GetUpperBound(0);
                for (int i = lower; i <= top; i++)
                {
                    var value = a.GetValue(i);
                    if (value == null) continue;

                    if (i == lower)
                    {
                        output.Write(new string(' ', 5));
                        PrettyPrintArrayHeader(output, value);
                        output.WriteLine();
                    }
                    PrettyPrintArrayItem(output, i, value);
                }

            }
            else if (result is bool)
            {
                if ((bool)result)
                    output.Write("true");
                else
                    output.Write("false");
            }
            else if (result is string)
            {
                output.Write("\"{0}\"", EscapeString((string)result));
            }
            else if (result is IDictionary)
            {
                IDictionary dict = (IDictionary)result;
                int top = dict.Count, count = 0;

                output.Write("{");
                foreach (DictionaryEntry entry in dict)
                {
                    count++;
                    output.Write("{ ");
                    PrettyPrint(output, entry.Key);
                    output.Write(", ");
                    PrettyPrint(output, entry.Value);
                    if (count != top)
                        output.Write(" }, ");
                    else
                        output.Write(" }");
                }
                output.Write("}");
            }
            else if (result is IEnumerable)
            {
                int i = 0;
                output.Write("{ ");
                foreach (object item in (IEnumerable)result)
                {
                    if (i++ != 0)
                        output.Write(", ");
                    PrettyPrint(output, item);
                }
                output.Write(" }");
            }
            else if (result is char)
            {
                EscapeChar(output, (char)result);
            }
            else
            {
                output.Write(result.ToString());
            }
        }
        private void PrintResult(object result)
        {
            if (result != null)
            {
                _console.WriteLine("Result: ", Style.Out);
                var sw = new StringWriter();
                PrettyPrint(sw, result);
                _console.WriteLine(sw.ToString(), Style.Info);
            }
        }

        public List<string> GetTypeNames(string prefix = null)
        {
            var options = new List<string>();
            foreach (var type in types)
            {
                var name = type.FullName;
                if (string.IsNullOrEmpty(prefix) || name.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                    options.Add(name);
            }
            return options;
        }

        public bool TryGetType(string name, out Type t)
        {
            foreach (var type in types)
            {
                if (type.FullName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    t = type;
                    return true;
                }
            }
            t = null;
            return false;
        }

        private List<string> GetPartialStringList(string startswith, IEnumerable<string> options)
        {
            var list = new List<string>();
            foreach (var option in options)
            {
                if (option.StartsWith(startswith, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(option.Substring(startswith.Length));
            }
            return list;
        }

        public override bool TryGetOptions(StringBuilder input, out IEnumerable<string> options)
        {
            options = null;
            string line = input.ToString();
            string[] args = ParseLine(line);
            // just show commands
            if (args.Length == 0)
            {
                options = commands;
                return true;
            }
            bool endswithspace = line.EndsWith(" ");
            var arg0 = args[0];
            if (arg0.StartsWith("help"))
                return false;
            if (args.Length == 1)
            {
                // try partial match of commands
                var list = GetPartialStringList(arg0, commands);
                if (list.Count == 0) return false;
                if (list.Count == 1)
                {
                    if (string.IsNullOrEmpty(list[0]))
                    {
                        options = !endswithspace ? new[] { " " } : GetTypeNames().ToArray();
                        return true;
                    }
                }
                options = list;
                return true;
            }
            string arg1 = args[1];
            if (args.Length == 2) // handle types in commands
            {
                var list = GetPartialStringList(arg1, GetTypeNames(arg1));
                if (list.Count == 0) return false;
                if (list.Count == 1)
                {
                    if (string.IsNullOrEmpty(list[0]))
                    {
                        if (!endswithspace)
                        {
                            options = new[] { " " };
                            return true;
                        }
                        Type t;
                        if (TryGetType(arg1, out t))
                        {
                            options = GetMemberNameList(t, arg0);
                            return true;
                        }
                        return false;
                    }
                }
                options = list;
                return true;
            }
            if (args.Length == 3)
            {
                var arg2 = args[2];
                Type t;
                if (TryGetType(arg1, out t))
                {
                    var list = GetPartialStringList(arg2, GetMemberNameList(t, arg0));
                    if (list.Count == 0) return false;
                    options = list;
                    return true;
                }
            }
            return false;
        }

        private static IList<MethodInfo> GetMethodsExact(Type t, string arg0)
        {
            const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var list = new List<MethodInfo>();
            foreach (var method in t.GetMethods(defaultFlags | BindingFlags.InvokeMethod))
                if (!method.IsSpecialName && !method.IsGenericMethod && method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(method);
            return list;
        }
        private static IList<MethodInfo> GetMethodsSpecial(Type t, string arg0)
        {
            const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var list = new List<MethodInfo>();
            foreach (var method in t.GetMethods(defaultFlags | BindingFlags.InvokeMethod))
                if (method.IsSpecialName && !method.IsGenericMethod && method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(method);
            return list;
        }

        private static IList<FieldInfo> GetFieldsExact(Type t, string arg0, BindingFlags flags)
        {
            const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var list = new List<FieldInfo>();
            foreach (var method in t.GetFields(defaultFlags | flags))
                if (!method.IsSpecialName && method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(method);
            return list;
        }

        private static IList<PropertyInfo> GetPropertiesExact(Type t, string arg0, BindingFlags flags)
        {
            const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var list = new List<PropertyInfo>();
            foreach (var method in t.GetProperties(defaultFlags | flags))
                if (!method.IsSpecialName && method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(method);
            return list;
        }

        private static IEnumerable<MemberInfo> GetMemberList(Type t, string arg0)
        {
            const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var list = new List<MemberInfo>();
            switch (arg0)
            {
                case "call":
                    foreach (var method in t.GetMethods(defaultFlags | BindingFlags.InvokeMethod))
                        if (!method.IsSpecialName && !method.IsGenericMethod)
                            list.Add(method);
                    break;
                case "set":
                    foreach (var method in t.GetFields(defaultFlags | BindingFlags.SetField))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    foreach (var method in t.GetProperties(defaultFlags | BindingFlags.SetProperty))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    break;
                case "get":
                    foreach (var method in t.GetFields(defaultFlags | BindingFlags.GetField))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    foreach (var method in t.GetProperties(defaultFlags | BindingFlags.GetProperty))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    break;
                case "list":
                    foreach (var method in t.GetMethods(defaultFlags | BindingFlags.InvokeMethod))
                        if (!method.IsSpecialName && !method.IsGenericMethod)
                            list.Add(method);
                    foreach (var method in t.GetFields(defaultFlags | BindingFlags.GetField))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    foreach (var method in t.GetProperties(defaultFlags | BindingFlags.GetProperty))
                        if (!method.IsSpecialName)
                            list.Add(method);
                    break;
            }
            return list;
        }
        private static IEnumerable<string> GetMemberNameList(Type t, string arg0)
        {
            var list = new List<string>();
            foreach (var x in GetMemberList(t, arg0))
                list.Add(x.Name);
            return list;
        }

        protected override bool IsCompleteOrInvalid(string code)
        {
            return true;
        }

        public bool AddAssembly(string name)
        {
            try
            {
                var asmlist = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asmlist)
                {
                    var nm = asm.GetName();
                    if (nm.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        types.AddRange(asm.GetTypes());
                }
                return true;
            }
            catch
            {
            }
            return false;
        }
    }
}