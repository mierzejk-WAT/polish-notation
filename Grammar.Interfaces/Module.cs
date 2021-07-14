namespace Grammar.Interfaces
{
    public struct Module
    {
        public readonly Eval Delegate;
        public readonly GetArg GetArg;
        public readonly string Arguments;
        public readonly string Description;
        public readonly byte[] RawAssembly;
        public readonly string SourceCode;
        public readonly object ScriptObject;

        public Module(
            string name,
            Eval @delegate,
            GetArg getArg,
            string arguments,
            string description,
            byte[] rawAssembly,
            string sourceCode,
            object scriptObject)
        {
            this.Name = name;
            this.Delegate = @delegate;
            this.GetArg = getArg;
            this.Arguments = arguments;
            this.Description = description;
            this.SourceCode = sourceCode;
            this.RawAssembly = rawAssembly;
            this.ScriptObject = scriptObject;
        }

        public string Name { get; }
    }
}
