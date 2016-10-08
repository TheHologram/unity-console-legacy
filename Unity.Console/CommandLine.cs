/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UC;

namespace Microsoft.Scripting.Hosting.Shell {

    /// <summary>
    /// Command line hosting service.
    /// </summary>
    public abstract class CommandLine {
        protected IConsole _console;

        protected virtual string Prompt { get { return ">>> "; } }
        protected virtual string PromptContinuation { get { return "... "; } }
        protected virtual string Logo { get { return null; } }

        public CommandLine() {
        }

        protected virtual void Initialize() {
        }

        protected virtual void Shutdown() {
            try {
                //_language.Shutdown();
            } catch (Exception e) {
                UnhandledException(e);
            }
        }
        protected void PrintLogo() {
            _console.Write(Logo, Style.Out);
        }

        #region Interactivity

        public int Run(IConsole console)
        {
            _console = console;
            Initialize();

            try
            {
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

        /// <summary>
        /// Starts the interactive loop.  Performs any initialization necessary before
        /// starting the loop and then calls RunInteractiveLoop to start the loop.
        /// 
        /// Returns the exit code when the interactive loop is completed.
        /// </summary>
        protected virtual int RunInteractive() {
            PrintLogo();
            return RunInteractiveLoop();
        }

        /// <summary>
        /// Runs the interactive loop.  Repeatedly parse and run interactive actions
        /// until an exit code is received.  If any exceptions are unhandled displays
        /// them to the console
        /// </summary>
        protected int RunInteractiveLoop() {
            //if (_scope == null) {
            //    _scope = CreateScope();
            //}

            int? res = null;
            do {
                try {
                    res = TryInteractiveAction();
                } catch (ShutdownConsoleException) {
                    break;
                } catch (Exception e) {
                    // There should be no unhandled exceptions in the interactive session
                    // We catch all exceptions here, and just display it,
                    // and keep on going
                    UnhandledException(e);
                }
            } while (res == null);

            return res.Value;
        }

        protected virtual void UnhandledException(Exception e) {
            _console.WriteLine(e.ToString(), Style.Error);
        }

        /// <summary>
        /// Attempts to run a single interaction and handle any language-specific
        /// exceptions.  Base classes can override this and call the base implementation
        /// surrounded with their own exception handling.
        /// 
        /// Returns null if successful and execution should continue, or an exit code.
        /// </summary>
        protected virtual int? TryInteractiveAction() {
            int? result = null;

            try {
                result = RunOneInteraction();
            } catch (ThreadAbortException) {
                //KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                //if (pki != null) {
                //    UnhandledException(tae);
                //    Thread.ResetAbort();
                //}
            }

            return result;
        }

        /// <summary>
        /// Parses a single interactive command and executes it.  
        /// 
        /// Returns null if successful and execution should continue, or the appropiate exit code.
        /// </summary>
        private int? RunOneInteraction() {
            bool continueInteraction;
            string s = ReadStatement(out continueInteraction);

            if (continueInteraction == false)
                return 0;

            if (String.IsNullOrEmpty(s)) {
                // Is it an empty line?
                _console.Write(String.Empty, Style.Out);
                return null;
            }

            ExecuteLine( s );
            return null;
        }

        protected abstract int? ExecuteLine(string s);

        /// <summary>
        /// Private helper function to see if we should treat the current input as a blank link.
        /// 
        /// We do this if we only have auto-indent text.
        /// </summary>
        private static bool TreatAsBlankLine(string line, int autoIndentSize) {
            if (line.Length == 0) return true;
            if (autoIndentSize != 0 && line.Trim().Length == 0 && line.Length == autoIndentSize) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Read a statement, which can potentially be a multiple-line statement suite (like a class declaration).
        /// </summary>
        /// <param name="continueInteraction">Should the console session continue, or did the user indicate 
        /// that it should be terminated?</param>
        /// <returns>Expression to evaluate. null for empty input</returns>
        protected string ReadStatement(out bool continueInteraction) {
            StringBuilder b = new StringBuilder();
            int autoIndentSize = 0;

            _console.Write(Prompt, Style.Prompt);

            while (true) {
                string line = ReadLine(autoIndentSize);
                continueInteraction = true;

                if (line == null) {
                    continueInteraction = false;
                    return null;
                }

                bool allowIncompleteStatement = TreatAsBlankLine(line, autoIndentSize);
                b.Append(line);
                b.Append("\n");

                string code = b.ToString();

                if (IsCompleteOrInvalid(code))
                    return code;

                //if (_options.AutoIndent && _options.AutoIndentSize != 0) {
                //    autoIndentSize = GetNextAutoIndentSize(code);
                //}

                // Keep on reading input
                _console.Write(PromptContinuation, Style.Prompt);
            }
        }

        protected abstract bool IsCompleteOrInvalid(string code);

        /// <summary>
        /// Gets the next level for auto-indentation
        /// </summary>
        protected virtual int GetNextAutoIndentSize(string text) {
            return 0;
        }

        protected virtual string ReadLine(int autoIndentSize) {
            return _console.ReadLine(autoIndentSize);
        }

        internal protected virtual TextWriter GetOutputWriter(bool isErrorOutput) {
            return isErrorOutput ? System.Console.Error : System.Console.Out;
        }

        //private static DynamicSite<object, IList<string>>  _memberCompletionSite =
        //    new DynamicSite<object, IList<string>>(OldDoOperationAction.Make(Operators.GetMemberNames));

        public virtual bool TryGetOptions(StringBuilder line, out IEnumerable<string> options)
        {
            options = null;
            return false;
        }

        public IList<string> GetMemberNames(string code) {
            List<string> res = new List<string>();
            //object value = _language.CreateSnippet(code, SourceCodeKind.Expression).Execute(_scope);
            //return _engine.Operations.GetMemberNames(value);
            // TODO: why doesn't this work ???
            //return _memberCompletionSite.Invoke(new CodeContext(_scope, _language), value);
            return res;
        }

        public virtual IList<string> GetGlobals(string name) {
            List<string> res = new List<string>();
            //foreach (SymbolId scopeName in _scope.Keys) {
            //    string strName = SymbolTable.IdToString(scopeName);
            //    if (strName.StartsWith(name)) {
            //        res.Add(strName);
            //    }
            //}
            return res;
        }

#endregion
    }


}
