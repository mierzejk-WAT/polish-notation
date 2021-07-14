#pragma warning disable 1584,1711,1572,1581,1580
namespace Grammar.PolishNotation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using Metadata;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using static Mono.Cecil.Cil.OpCodes;
    using static Mono.Cecil.ModuleAttributes;
    using static Mono.Cecil.TypeAttributes;
    using Method = Mono.Cecil.MethodAttributes;
    using Parser = PolishNotationParser;
    using Reflection = System.Reflection;

    public static class CompilationUnit
    {
        internal static readonly Encoding ResourceEncoding = Encoding.UTF8;
        internal static readonly NumberFormatInfo FORMAT = new NumberFormatInfo
        {
            NegativeSign = "-",
            NumberDecimalSeparator = ".",
            NumberGroupSeparator = string.Empty
        };

        internal static readonly Type TypeObject = typeof(object);
        internal static readonly Type TypeDouble = typeof(double);
        internal static readonly Type TypeMath = typeof(Math);
        internal static readonly Type TypeString = typeof(string);
        internal static readonly Type TypeParam = typeof(ParamArrayAttribute);
        internal static readonly Type TypeDoubleParam = typeof(double[]);
        private static readonly Type TypeICalculate = typeof(ICalculate);
        private static readonly Type TypeDescription = typeof(DescriptionAttribute);
        private static readonly ConcurrentDictionary<string, ICalculate> Types = new ConcurrentDictionary<string, ICalculate>(StringComparer.OrdinalIgnoreCase);

        internal static ValueTuple<string, ICalculate, ICollection<string>, string, byte[]> GetType(string input, TextWriter output = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(input), "Source code is null or empty.");
            }

            var tree = GetTree(input, out var errors, out var name, out var description, output);
            if (errors > 0)
            {
                throw new ArgumentException($"Syntax errors: {errors} errors.", nameof(input));
            }

            if (!Types.ContainsKey(name))
            {
                var(instance, variables, rawAssembly) = LoadType(name, description, tree, input);
                if (Types.TryAdd(name, instance))
                {
                    return (name, instance, variables, description, rawAssembly);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(input), $"The module '{name}' is already loaded.");
        }

        internal static bool TryLoadType(Type type, string name, out ICalculate loaded, TextWriter output)
        {
            if (!Types.ContainsKey(name))
            {
                loaded = (ICalculate)Activator.CreateInstance(type);
                if (Types.TryAdd(name, loaded))
                {
                    return true;
                }
            }

            output?.WriteLine("The module '{0}' is already loaded.", name);
            loaded = default(ICalculate);
            return false;
        }

        private static IParseTree GetTree(string input, out int errors, out string name, out string description, TextWriter output)
        {
            var charStream = new AntlrInputStream(input);
            var lexer = new PolishNotationLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Parser(tokenStream, Console.Out, output ?? Console.Error)
            {
                BuildParseTree = true
            };

            if (null == output)
            {
                parser.RemoveErrorListeners();
            }

            var expression = parser.expression();
            errors = parser.NumberOfSyntaxErrors;
            name = parser.Name;
            description = parser.Description;
            if (errors > 0)
            {
                return expression.tree;
            }

            var optimizer = new TreeOptimizerVisitor();
            return optimizer.Visit(expression);
        }

        private static (ICalculate instance, ICollection<string> variables, byte[] RawAssembly)
            LoadType(string name, string description, IParseTree tree, string sourceCode)
        {
            var typeDef = CreateNewTypeDefinition(name, out var body);
            var il = body.GetILProcessor();

            var walker = new ParseTreeWalker();
            var listener = new TreeCompilationListener(il);
            walker.Walk(listener, tree);

            var variables = listener.Variables;
            il.AddLocalVariable(variables)
                .ToList()
                .ForEach(body.Variables.Add);
            body.InitLocals = true;

            listener.Instructions.ForEach(il.Append);
            il.Emit(Ret);

            typeDef
                .AddStringResource(sourceCode)
                .SetDescriptionAttribute(variables.Keys, description);

            byte[] asm;
            using (var stream = new MemoryStream())
            {
                typeDef.Module.Write(stream);
                asm = stream.ToArray();
            }

            var assembly = Reflection.Assembly.Load(asm);
            var instance = (ICalculate)Activator.CreateInstance(assembly.GetTypes()[0]);
            return (instance, (ICollection<string>)variables.Keys, asm);
        }

        private static TypeDefinition CreateNewTypeDefinition(string unitName, out MethodBody body)
        {
            var module = ModuleDefinition.CreateModule(unitName, ModuleKind.Dll);
            module.Attributes = ILOnly | Required32Bit | Preferred32Bit;

            return module
                .CreateCalcTypeDefinition(unitName)
                .AddDefaultConstructor()
                .AddCalculateMethod(out body);
        }

        private static TypeDefinition CreateCalcTypeDefinition(this ModuleDefinition module, string unitName)
        {
            // TypeAttributes.AnsiClass, TypeAttributes.UnicodeClass, TypeAttributes.AutoClass - exclusive LPTSTR mrshalling
            var calc = new TypeDefinition(
                unitName,
                "Calc",
                Public | Sealed | Class | AutoLayout | AnsiClass | BeforeFieldInit,
                module.TypeSystem.Object);
            calc.Interfaces.Add(module.ImportReference(TypeICalculate));

            module.Types.Add(calc);
            return calc;
        }

        private static TypeDefinition AddDefaultConstructor(this TypeDefinition typeDef)
        {
            var module = typeDef.Module;
            var ctor = new MethodDefinition(".ctor", Method.Public | Method.SpecialName | Method.RTSpecialName | Method.HideBySig, module.TypeSystem.Void);
            var il = ctor.Body.GetILProcessor();
            il.Emit(Ldarg_0);
            il.Emit(Call, module.ImportReference(TypeObject.GetConstructor(Type.EmptyTypes)));
            il.Emit(Ret);

            typeDef.Methods.Add(ctor);
            return typeDef;
        }

        private static TypeDefinition AddCalculateMethod(this TypeDefinition typeDef, out MethodBody body)
        {
            var module = typeDef.Module;
            const string CalculateMethodName = nameof(ICalculate.Calculate);
            var calculateMethod = new MethodDefinition(
                $"{TypeICalculate.FullName}.{CalculateMethodName}",
                Method.Private | Method.NewSlot | Method.Virtual | Method.Final | Method.HideBySig, // MethodAttributes.HideBySig -> hide by name AND SIGNATUR; !MethodAttributes.HideBySig -> hide by name only
                module.TypeSystem.Double);
            calculateMethod.Overrides.Add(module.ImportReference(TypeICalculate.GetMethod(CalculateMethodName)));

            var param = new ParameterDefinition("args", ParameterAttributes.None, module.ImportReference(TypeDoubleParam));
            var paramsAttribute = new CustomAttribute(module.ImportReference(TypeParam.GetConstructor(Type.EmptyTypes)));
            param.CustomAttributes.Add(paramsAttribute);
            calculateMethod.Parameters.Add(param);
            typeDef.Methods.Add(calculateMethod);

            body = calculateMethod.Body;
            return typeDef;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static TypeDefinition SetDescriptionAttribute(this TypeDefinition typeDef, IEnumerable<string> variables, string description = null)
        {
            var module = typeDef.Module;
            var attr = new CustomAttribute(module.ImportReference(TypeDescription.GetConstructor(new[] { TypeString })));
            attr.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.String, string.Join(DescriptionAttribute.Separator, variables)));
            var resProperty = new CustomAttributeNamedArgument(nameof(DescriptionAttribute.Resource), new CustomAttributeArgument(module.TypeSystem.String, typeDef.FullName));
            attr.Properties.Add(resProperty);
            if (!string.IsNullOrWhiteSpace(description))
            {
                var descProperty = new CustomAttributeNamedArgument(nameof(DescriptionAttribute.Description), new CustomAttributeArgument(module.TypeSystem.String, description));
                attr.Properties.Add(descProperty);
            }

            typeDef.CustomAttributes.Add(attr);
            return typeDef;
        }

        private static TypeDefinition AddStringResource(this TypeDefinition typeDef, string resource)
        {
            var embeddedResource = new EmbeddedResource(typeDef.FullName, ManifestResourceAttributes.Public, ResourceEncoding.GetBytes(resource));
            typeDef.Module.Resources.Add(embeddedResource);
            return typeDef;
        }

        private static IEnumerable<VariableDefinition> AddLocalVariable(this ILProcessor il, IReadOnlyCollection<KeyValuePair<string, byte>> variables)
        {
            var doubleReference = il.Body.Method.Module.TypeSystem.Double;
            foreach (var var in variables)
            {
                il.Emit(Ldarg_1); // params double[] args to the stack
                il.PushByte(var.Value); // (var.Value) index to the stack
                il.Emit(Ldelem_R8); // args[index] to the stack
                il.PopToVariable(var.Value); // args[index] from the stack the local variable at index = var.Value
                yield return new VariableDefinition(var.Key, doubleReference);
            }
        }

        /// <summary>
        /// Simple byte to stack instruction optimization.
        /// </summary>
        /// <param name="il"><typeparamref name="ILProcessor"/> used to emit the instruction.</param>
        /// <param name="value"><typeparamref name="Byte"/> to be pushed to the stack.</param>
        private static void PushByte(this ILProcessor il, byte value)
        {
            switch (value)
            {
                case 0:
                    il.Emit(Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(Ldc_I4_8);
                    break;
                default:
                    il.Emit(Ldc_I4_S, value);
                    break;
            }
        }

        /// <summary>
        /// Simple pop to local variable instruction optimization.
        /// </summary>
        /// <param name="il"><typeparamref name="ILProcessor"/> used to emit the instruction.</param>
        /// <param name="index">Local variable index.</param>
        private static void PopToVariable(this ILProcessor il, byte index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(Stloc_0);
                    break;
                case 1:
                    il.Emit(Stloc_1);
                    break;
                case 2:
                    il.Emit(Stloc_2);
                    break;
                case 3:
                    il.Emit(Stloc_3);
                    break;
                default:
                    il.Emit(Stloc_S, index);
                    break;
            }
        }
    }
}