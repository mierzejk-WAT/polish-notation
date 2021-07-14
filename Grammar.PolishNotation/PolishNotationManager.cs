namespace Grammar.PolishNotation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Grammar.Interfaces;
    using Metadata;
    using Utilities;
    using static System.Linq.Expressions.Expression;
    using Module = Interfaces.Module;

    public sealed class PolishNotationManager : IGrammarManager
    {
        private static readonly Regex Separator = new Regex(@",\s*", RegexOptions.Compiled | RegexOptions.ECMAScript);
        private static readonly Type Double = typeof(double);

        Module IGrammarManager.GetModule(string input, TextWriter output)
        {
            return CompilationUnit.GetType(input, output).CreateModule(input);
        }

        void IGrammarManager.AddModulesTo(ConcurrentBag<Module> bag, IEnumerable<Type> types, TextWriter output)
        {
            string GetName(string ns, bool single, Type type)
            {
                return string.IsNullOrWhiteSpace(ns)
                           ? type.Name
                           : single
                               ? ns
                               : $"{ns}_{type.Name}";
            }

            var discovered = from t in types
                             let desc = t.GetCustomAttribute<DescriptionAttribute>()
                             where typeof(ICalculate).IsAssignableFrom(t) && null != desc
                             group new { Type = t, Description = desc } by t.Namespace?.Replace('.', '_')
                             into grouping
                             let nspace = grouping.ToList()
                                from type in nspace
                                select new { type.Type, Name = GetName(grouping.Key, 1 == nspace.Count, type.Type), type.Description };
            Parallel.ForEach(
                discovered,
                type =>
                    {
                        if (!CompilationUnit.TryLoadType(type.Type, type.Name, out var instance, output))
                        {
                            return;
                        }

                        bag.Add(Helper.CreateModule(type.Name, instance, type.Description, type.Type));
                    });
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        internal static GetArg GetArgValidator(int varNumber)
        {
            object Parse(string expression)
            {
                var args = (from s in Separator.Split(expression)
                            where !string.IsNullOrWhiteSpace(s)
                            select s.ParseDouble()).ToList();
                if (args.Count != varNumber)
                {
                    throw new FormatException();
                }

                return args;
            }

            return Parse;
        }

        internal static Eval GetCalc(ICalculate instance, ICollection<string> variables)
        {
            string Calc(object argument)
            {
                if (argument is List<double> args && args.Count == variables.Count)
                {
                    return instance.Calculate(args.ToArray()).ToString("0.#####");
                }

                throw new FormatException();
            }

            return Calc;
        }

        internal static string GetArguments(IEnumerable<string> variables)
        {
            var parameters = string.Join(", ", variables);
            return string.IsNullOrEmpty(parameters) ? @"{no arguments}" : parameters;
        }

        internal static object GetFunction(ICalculate instance, IEnumerable<string> variables)
        {
            var parameters = (from v in variables select Parameter(Double, v)).ToArray();
            var wrapper = Lambda(
                Invoke(
                    Constant((Func<double[], double>)instance.Calculate),
                    // ReSharper disable once CoVariantArrayConversion
                    NewArrayInit(Double, parameters)),
                parameters);
            return wrapper.Compile();
        }
    }
}
