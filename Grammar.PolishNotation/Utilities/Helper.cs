namespace Grammar.PolishNotation.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Metadata;
    using static System.Globalization.NumberStyles;
    using Module = Interfaces.Module;

    internal static class Helper
    {
        public static string GetResourceString(this Assembly assembly, Encoding encoding, params string[] resourceNames)
        {
            StreamReader reader = null;
            var stream = resourceNames.Where(NotNull).Select(assembly.GetManifestResourceStream).FirstOrDefault(NotNull);
            if (stream is null)
            {
                return null;
            }

            try
            {
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                reader?.Dispose();
                stream.Close();
            }
        }

        public static Module CreateModule(string name, ICalculate instance, ICollection<string> variables, string description, byte[] rawAssembly, string sourceCode)
        {
            return new Module(
                name: name,
                @delegate: PolishNotationManager.GetCalc(instance, variables),
                getArg: PolishNotationManager.GetArgValidator(variables.Count),
                arguments: PolishNotationManager.GetArguments(variables),
                description: description,
                rawAssembly: rawAssembly,
                sourceCode: sourceCode,
                scriptObject: PolishNotationManager.GetFunction(instance, variables));
        }

        public static Module CreateModule(string name, ICalculate instance, DescriptionAttribute description, Type type, byte[] rawAssembly = null)
        {
            return CreateModule(
                name,
                instance,
                description.Variables,
                description.Description,
                rawAssembly,
                GetResourceString(type.Assembly, CompilationUnit.ResourceEncoding, description.Resource, type.FullName));
        }

        public static Module CreateModule(this (string name, ICalculate instance, ICollection<string> variables, string description, byte[] rawAssembly) tuple, string sourceCode)
        {
            return CreateModule(tuple.name, tuple.instance, tuple.variables, tuple.description, tuple.rawAssembly, sourceCode);
        }

        public static double ParseDouble(this string s)
        {
            return double.Parse(s, AllowDecimalPoint | AllowLeadingSign | AllowLeadingWhite | AllowTrailingWhite, CompilationUnit.FORMAT);
        }

        public static string ToFormattedString(this double d)
        {
            return d.ToString("G", CompilationUnit.FORMAT);
        }

        private static bool NotNull<T>(this T item)
            where T : class
        {
            return null != item;
        }
    }
}
