using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Scripting.Hosting.Shell;
using Unity.Console.Behaviors;

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

        List<Type> types = new List<Type>();

        public UnityCommandLine()
        {
            Commands["help"] = new Commands.HelpCommand(this);
            Commands["list"] = new Commands.ListCommand(this);
            Commands["call"] = new Commands.CallCommand(this);
            Commands["get"] = new Commands.GetCommand(this);
            Commands["set"] = new Commands.SetCommand(this);
            Commands["let"] = new Commands.LetCommand(this);
            Commands["reset"] = new Commands.ResetCommand(this);
            Commands["exec"] = new Commands.ExecCommand(this);
            Commands["sync"] = new Commands.SyncCommand(this);
            Commands["sleep"] = new Commands.SleepCommand(this);
            Commands["clear"] = new Commands.ClearCommand(this);
        }

        public void RegisterAssembly(Assembly asm)
        {
            if (asm == null) return;
            try
            {
                foreach (var type in asm.GetTypes())
                {
                    foreach (CommandAttribute attr in type.GetCustomAttributes(typeof(CommandAttribute), false))
                    {
                        ICommand inst = null;
                        var ci = type.GetConstructor(new[] { typeof(UnityCommandLine) });
                        inst = ci.Invoke(new object[] { this }) as ICommand;
                        if (inst != null)
                            Commands[attr.Description] = inst;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        public IConsole Console => _console;
        public Dictionary<string, object> Variables { get; } = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, ICommand> Commands { get; } = new Dictionary<string, ICommand>(StringComparer.CurrentCultureIgnoreCase);

        public bool Synchronize { get; set; }

        public void ExecuteScript(StreamReader reader, bool showOutput)
        {
            bool suppressed = Console.SuppressOutput;
            try
            {
                Console.SuppressOutput = !showOutput;

                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    ExecuteLine(line.Trim());
            }
            finally
            {
                Console.SuppressOutput = suppressed;
            }
        }
        public void ExecuteScript(IEnumerable<string> lines, bool showOutput)
        {
            bool suppressed = Console.SuppressOutput;
            try
            {
                Console.SuppressOutput = !showOutput;
                foreach(var line in lines)
                    ExecuteLine(line.Trim());
            }
            finally
            {
                Console.SuppressOutput = suppressed;
            }
        }

        #region Interactivity

        public int RunWithStartup(IConsole console, string[] lines)
        {
            _console = console;
            Initialize();

            try
            {
                if (lines != null && lines.Length > 0)
                    ExecuteScript(lines, true);
                return RunInteractive();
            }
            catch (System.Threading.ThreadAbortException)
            {
                //if (tae.ExceptionState is KeyboardInterruptException)
                //{
                //    Thread.ResetAbort();
                //}
                return -1;
            }
            finally
            {
                Shutdown();
                _console = null;
            }
        }

        #endregion

        protected override int? ExecuteLine(string s)
        {
            if (s.StartsWith(";") || s.StartsWith("#")) return 0;

            string[] args = ParseLine(s);
            if (args.Length == 0) return 0;

            var arg0 = args[0].ToLower();

            switch (arg0)
            {
                case "?":
                    arg0 = "help";
                    break;

                case "logout":
                case "quit":
                case "exit":
                {
                    _console.WriteLine("Use ^Z (Control-Z) + Enter to close console. ", Style.Warning);
                    _console.WriteLine("  - Careful though as it may not be possible open console again", Style.Info);
                    return 0;
                    //throw new ShutdownConsoleException();
                }
                case "cls":
                case "clr":
                    arg0 = "clear";
                    break;
            }

            ICommand cmd;
            if (!Commands.TryGetValue(arg0, out cmd))
            {
                _console.WriteLine("Unknown command: " + arg0, Style.Error);
                return 0;
            }
            if (Synchronize)
            {
                return FuncBehavior.Execute(cmd.ExecuteLine, args);
            }
            else
            {
                return cmd.ExecuteLine(args);
            }
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

        public void PrettyPrintArrayHeader(TextWriter output, object result)
        {
            if (result == null) return;

            var t = result.GetType();
            if (t.IsClass)
            {
                int width = _console.Width - 5;
                var fi = t.GetFields(DefaultBindingFlags | BindingFlags.Instance);
                int colWidth = Math.Min(16, Math.Max(5, ((width / fi.Length) + 1)));
                string format = string.Format("{{0,-{0}}} ", colWidth);
                foreach (var f in fi)
                    output.Write(format, f.Name);
            }
        }
        public void PrettyPrintArrayItem(TextWriter output, int i, object result)
        {
            if (result == null) return;

            output.Write("[{0,2}] ", i);
            var t = result.GetType();
            if (t.IsClass)
            {
                int width = _console.Width - 5;
                var fi = t.GetFields(DefaultBindingFlags | BindingFlags.Instance);
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

        public void PrettyPrint(TextWriter output, object result)
        {
            if (result == null)
            {
                output.Write("null");
                return;
            }
            Type t = result.GetType();

            if (result is Array)
            {
                PrettyPrintArray(output, (Array)result);
            }
            else if (result is ArraySegment)
            {
                var seg = (ArraySegment) result;
                PrettyPrintArray(output, seg.Array, seg.Offset, seg.Count);
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
                output.Write("\"{0}\"", result);
            }
            else if (result is IDictionary)
            {
                IDictionary dict = (IDictionary)result;
                int top = dict.Count;
                foreach (DictionaryEntry entry in dict)
                {
                    PrettyPrint(output, entry.Key);
                    output.Write(": ");
                    PrettyPrint(output, entry.Value);
                }
            }
            else if (result is IEnumerable)
            {
                foreach (object item in (IEnumerable) result) {
                    PrettyPrint(output, item);
                }
            }
            else if (result is char)
            {
                EscapeChar(output, (char)result);
            }
            else if (t.IsPrimitive || t.IsValueType)
            {
                output.Write(result.ToString());
            }
            else if (t.IsClass)
            {
                PrettyPrintClass(output, result);
            }
            else
            {
                output.Write(result.ToString());
            }
        }

        private void PrettyPrintArray(TextWriter output, Array a)
        {
            if (a.Rank == 1)
            {
                PrettyPrintArray(output, a, 0, a.Length);
            }
            else
            {
                PrettyPrintArray(output, a, 0, -1);
            }            
        }

        private void PrettyPrintArray(TextWriter output, Array a, int offset, int count)
        {
            bool first = true;
            if (a.Rank == 1)
            {
                var lower = a.GetLowerBound(0) + offset;
                var top = Math.Min(a.GetUpperBound(0), lower + count);
                for (var i = lower; i <= top; i++)
                {
                    var value = a.GetValue(i);
                    if (value == null) continue;

                    if (first)
                    {
                        first = false;
                        output.Write(new string(' ', 5));
                        PrettyPrintArrayHeader(output, value);
                        output.WriteLine();
                    }
                    PrettyPrintArrayItem(output, i-offset, value);
                }
            }
            else
            {
                PrintResult(a);
            }
        }

        private void PrettyPrintClass(TextWriter output, object result)
        {
            Type t = result.GetType();

            output.WriteLine(t.FullName);
            int width = 5;
            var fi = t.GetFields(DefaultBindingFlags | BindingFlags.Instance);
            var pi = t.GetProperties(DefaultBindingFlags | BindingFlags.Instance | BindingFlags.GetProperty);
            foreach (var f in fi)
                width = Math.Max(width, f.Name.Length);
            foreach (var p in pi)
                width = Math.Max(width, p.Name.Length);
            string format = string.Format("{{0,-{0}}} : ", width);
            foreach (var p in pi)
            {
                MethodInfo mi;
                if (TryGetMethodSpecial(t, true, "get_" + p.Name, out mi))
                {
                    output.Write(format, p.Name);
                    var val = mi.Invoke(result, new object[0]);
                    if (val != null)
                    {
                        var vt = val.GetType();
                        if (vt.IsPrimitive)
                            PrettyPrint(output, val);
                        else
                            PrettyPrint(output, val.ToString());
                    }
                    output.WriteLine();
                }
            }
            foreach (var f in fi)
            {
                output.Write(format, f.Name);
                var val = f.GetValue(result);
                if (val != null)
                {
                    var vt = val.GetType();
                    if (vt.IsPrimitive)
                        PrettyPrint(output, val);
                    else
                        PrettyPrint(output, val.ToString());
                }
                output.WriteLine();
            }
        }

        public void PrintResult(object result)
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
            try
            {
                t = Type.GetType(name, true, true);
                return true;
            }
            catch
            {
                t = null;
                return false;
            }
        }

        public List<string> GetPartialStringList(string startswith, IEnumerable<string> options)
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
            string line = input.ToString();
            string[] args = ParseLine(line);
            // just show commands
            if (args.Length == 0)
            {
                options = Commands.Keys;
                return true;
            }
            bool endswithspace = line.EndsWith(" ");
            var arg0 = args[0];
            ICommand cmd;
            if (!Commands.TryGetValue(arg0, out cmd))
            {
                options = GetPartialStringList(arg0, Commands.Keys);
                return true;
            }
            // request space after first commands
            if (args.Length == 1 && !endswithspace)
            {
                options = new[] { " " };
                return false;
            }
            return cmd.TryGetOptions(args, endswithspace, out options);
        }

        public IList<MethodInfo> GetMethodsSpecial(Type t, bool isInstance, string arg0)
        {
            BindingFlags flags = isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var list = new List<MethodInfo>();
            foreach (var method in t.GetMethods(DefaultBindingFlags | BindingFlags.InvokeMethod | flags))
                if (method.IsSpecialName && !method.IsGenericMethod && method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                    list.Add(method);
            return list;
        }

        public bool TryGetMethodSpecial(Type t, bool isInstance, string arg0, out MethodInfo mi)
        {
            BindingFlags flags = isInstance ? BindingFlags.Instance : BindingFlags.Static;
            var list = new List<MethodInfo>();
            foreach (var method in t.GetMethods(DefaultBindingFlags | BindingFlags.InvokeMethod | flags))
                if (method.IsSpecialName && !method.IsGenericMethod &&
                    method.Name.Equals(arg0, StringComparison.CurrentCultureIgnoreCase))
                {
                    mi = method;
                    return true;
                }
            mi = null;
            return false;
        }

        const BindingFlags DefaultBindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public;

        public void AppendMethods(List<MemberInfo> list, Type t, BindingFlags flags)
        {
            foreach (var method in t.GetMethods(DefaultBindingFlags | flags | BindingFlags.InvokeMethod))
                if (!method.IsSpecialName && !method.IsGenericMethod)
                    list.Add(method);
        }

        public void AppendProperties(List<MemberInfo> list, Type t, BindingFlags flags)
        {
            foreach (var method in t.GetProperties(DefaultBindingFlags | flags))
                if (!method.IsSpecialName)
                    list.Add(method);
        }

        public void AppendFields(List<MemberInfo> list, Type t, BindingFlags flags)
        {
            foreach (var method in t.GetFields(DefaultBindingFlags | flags))
                if (!method.IsSpecialName)
                    list.Add(method);
        }

        public IEnumerable<string> GetMemberNames(IEnumerable<MemberInfo> members)
        {
            if (members != null)
                foreach (var x in members)
                    yield return x.Name;
        }

        protected override bool IsCompleteOrInvalid(string code)
        {
            return true;
        }

        public bool AddAssembly(string name)
        {
            try
            {
                var asmlist = AppDomain.CurrentDomain.GetAssemblies();
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
                // ignored
            }
            return false;
        }

        [Flags]
        public enum TypeOptions
        {
            Method = 0x1,
            Field = 0x2,
            GetProperty = 0x4,
            SetProperty = 0x8,
            All = Method|Field|GetProperty,
        }

        public IEnumerable<MemberInfo> GetMembers(Type t, bool instance, TypeOptions flags)
        {
            return instance ? GetInstanceMembers(t, flags) : GetStaticMembers(t, flags);
        }

        public IEnumerable<MemberInfo> GetStaticMembers(Type t, TypeOptions flags)
        {

            var members = new List<MemberInfo>();
            if ((flags & TypeOptions.Method) != 0)
                AppendMethods(members, t, BindingFlags.Static);
            if ((flags & TypeOptions.Field) != 0)
                AppendFields(members, t, BindingFlags.Static | BindingFlags.GetField);
            if ((flags & TypeOptions.GetProperty) != 0)
                AppendProperties(members, t, BindingFlags.Static | BindingFlags.GetProperty);
            if ((flags & TypeOptions.SetProperty) != 0)
                AppendProperties(members, t, BindingFlags.Static | BindingFlags.SetProperty);
            return members;
        }

        public IEnumerable<MemberInfo> GetInstanceMembers(Type t, TypeOptions flags)
        {
            var members = new List<MemberInfo>();
            if ((flags & TypeOptions.Method) != 0)
                AppendMethods(members, t, BindingFlags.Instance);
            if ((flags & TypeOptions.Field) != 0)
                AppendFields(members, t, BindingFlags.Instance | BindingFlags.GetField);
            if ((flags & TypeOptions.GetProperty) != 0)
                AppendProperties(members, t, BindingFlags.Instance | BindingFlags.GetProperty);
            if ((flags & TypeOptions.SetProperty) != 0)
                AppendProperties(members, t, BindingFlags.Instance | BindingFlags.SetProperty);
            return members;
        }

        public bool TryGetArgumentType(string typename, out object value, out Type t)
        {
            var isVariable = typename.StartsWith("$");
            if (isVariable)
            {
                if (Variables.TryGetValue(typename, out value))
                {
                    t = value.GetType();
                    return true;
                }
            }
            else
            {
                value = null;
                return TryGetType(typename, out t);
            }
            t = null;
            return false;
        }

        public object GetVariable(string name)
        {
            object value = null;
            bool isVariable = name.StartsWith("$");
            if (isVariable)
            {
                if (!this.Variables.TryGetValue(name, out value))
                {
                    this.Console.Write("Unable to resolve variable: ", Style.Error);
                    this.Console.Write(name, Style.Warning);
                    this.Console.WriteLine();
                    return null;
                }
            }
            else
            {
                value = name;
            }
            return value;
        }

        public bool TryGetVariable(string name, out object value)
        {
            bool isVariable = name.StartsWith("$");
            if (isVariable)
            {
                if (!this.Variables.TryGetValue(name, out value))
                {
                    this.Console.Write("Unable to resolve variable: ", Style.Error);
                    this.Console.Write(name, Style.Warning);
                    this.Console.WriteLine();
                    return false;
                }
            }
            else
            {
                value = name;
            }
            return true;
        }

        public bool AppendRegexFromWildcard(StringBuilder sb, string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            bool special = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '*':
                        sb.Append(".*");
                        special = true;
                        break;
                    case '?':
                        sb.Append('.');
                        special = true;
                        break;
                    case '\\':
                        if (i < value.Length - 1)
                            sb.Append(System.Text.RegularExpressions.Regex.Escape(value[++i].ToString()));
                        break;
                    default:
                        sb.Append(System.Text.RegularExpressions.Regex.Escape(value[i].ToString()));
                        break;
                }
            }
            return special;
        }

        public System.Text.RegularExpressions.Regex BuildRegexFromWildcard(string wildcard, bool caseInsensitive)
        {
            var sb = new StringBuilder();
            AppendRegexFromWildcard(sb, wildcard);
            var options = System.Text.RegularExpressions.RegexOptions.Singleline;
            if (caseInsensitive) options |= System.Text.RegularExpressions.RegexOptions.IgnoreCase;
            return new System.Text.RegularExpressions.Regex(sb.ToString(), options);
        }

        public System.Text.RegularExpressions.Regex BuildRegexFromWildcard(string wildcard)
        {
            return BuildRegexFromWildcard(wildcard, true);
        }
    }
}