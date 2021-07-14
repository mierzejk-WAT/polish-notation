namespace Grammar.PolishNotation.Metadata
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        public const string Separator = ",";

        private static readonly string[] VarSep = { Separator };

        public DescriptionAttribute(string variables)
        {
            this.Variables = variables.Split(VarSep, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] Variables { get; }

        public string Description { get; set; }

        public string Resource { get; set; }
    }
}
