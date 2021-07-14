namespace Grammar.Explorer.Hosts
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using IronPython.Hosting;
    using JetBrains.Annotations;
    using Microsoft.Scripting.Hosting;

    internal sealed class PythonHost
    {
        private readonly ScriptEngine scriptEngine = Python.CreateEngine();
        private readonly ScriptScope scriptScope;

        public PythonHost([CanBeNull] TextWriter textWriter = null)
        {
            if (null != textWriter)
            {
                this.scriptEngine.Runtime.IO.SetOutput(Stream.Null, textWriter);
            }

            this.scriptScope = this.scriptEngine.CreateScope();
            this.scriptScope.SetVariable("modules", this.Modules);
        }

        public IDictionary<string, object> Modules { get; } = new ExpandoObject();

        public Task<string> ExecuteCodeAsync([CanBeNull] string code, CancellationToken? cancellationToken = null)
        {
            return Task.Run(
                () =>
                    {
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            return null;
                        }

                        var scriptSource = this.scriptEngine.CreateScriptSourceFromString(
                            code,
                            Microsoft.Scripting.SourceCodeKind.Statements);
                        var result = scriptSource.ExecuteAndWrap(this.scriptScope, out var exception);
                        cancellationToken?.ThrowIfCancellationRequested();
                        if (null != exception)
                        {
                            return (exception.Unwrap() as Exception)?.Message;
                        }

                        return result.Unwrap()?.ToString() ?? string.Empty;
                    },
                cancellationToken ?? CancellationToken.None);
        }
    }
}
