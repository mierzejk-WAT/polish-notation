namespace Grammar.PolishNotation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Antlr4.Runtime.Misc;

    using Grammar.PolishNotation.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using Parser = PolishNotationParser;

    internal sealed class TreeCompilationListener : PolishNotationBaseListener
    {
        public readonly List<Instruction> Instructions = new List<Instruction>();
        private readonly Dictionary<string, byte> variables = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private readonly ILProcessor il;
        private readonly Lazy<MethodReference> min;
        private readonly Lazy<MethodReference> max;
        private readonly Lazy<MethodReference> pow;
        private readonly Lazy<MethodReference> floor;
        private readonly Lazy<MethodReference> round;
        private readonly Lazy<MethodReference> ceiling;
        private readonly Lazy<MethodReference> truncate;

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Local function.")]
        public TreeCompilationListener(ILProcessor processor)
        {
            this.il = processor;
            var module = processor.Body.Method.Module;

            Func<MethodReference> GetMathBinaryMethod(string name) => () =>
                module.ImportReference(CompilationUnit.TypeMath.GetMethod(name, new[] { CompilationUnit.TypeDouble, CompilationUnit.TypeDouble }));

            Func<MethodReference> GetMathUnaryMethod(string name) => () =>
                module.ImportReference(CompilationUnit.TypeMath.GetMethod(name, new[] { CompilationUnit.TypeDouble }));

            this.min = new Lazy<MethodReference>(GetMathBinaryMethod(nameof(this.Min)), LazyThreadSafetyMode.PublicationOnly);
            this.max = new Lazy<MethodReference>(GetMathBinaryMethod(nameof(this.Max)), LazyThreadSafetyMode.PublicationOnly);
            this.pow = new Lazy<MethodReference>(GetMathBinaryMethod(nameof(this.Pow)), LazyThreadSafetyMode.PublicationOnly);
            this.round = new Lazy<MethodReference>(GetMathUnaryMethod(nameof(this.Round)), LazyThreadSafetyMode.PublicationOnly);
            this.ceiling = new Lazy<MethodReference>(GetMathUnaryMethod(nameof(this.Ceiling)), LazyThreadSafetyMode.PublicationOnly);
            this.floor = new Lazy<MethodReference>(GetMathUnaryMethod(nameof(this.Floor)), LazyThreadSafetyMode.PublicationOnly);
            this.truncate = new Lazy<MethodReference>(GetMathUnaryMethod(nameof(this.Truncate)), LazyThreadSafetyMode.PublicationOnly);
        }

        public IReadOnlyDictionary<string, byte> Variables => this.variables;

        private MethodReference Min => this.min.Value;

        private MethodReference Max => this.max.Value;

        private MethodReference Pow => this.pow.Value;

        private MethodReference Round => this.round.Value;

        private MethodReference Ceiling => this.ceiling.Value;

        private MethodReference Floor => this.floor.Value;

        private MethodReference Truncate => this.truncate.Value;

        public override void EnterVarArg([NotNull] Parser.VarArgContext context)
        {
            var name = context.GetText();
            if (!this.variables.ContainsKey(name))
            {
                this.variables.Add(name, (byte)this.variables.Count);
            }
        }

        public override void ExitVarArg([NotNull] Parser.VarArgContext context)
        {
            var index = this.variables[context.GetText()];
            // simple local variable to stack instruction optimization
            switch (index)
            {
                case 0:
                    this.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    this.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    this.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    this.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    this.Emit(OpCodes.Ldloc_S, index);
                    break;
            }
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Correctly indented.")]
        public override void EnterNumber([NotNull] Parser.NumberContext ctx)
        {
            this.Emit(OpCodes.Ldc_R8, ctx is EnhancedNumberContext ectx ? ectx.Value : ctx.GetText().ParseDouble());
        }

        public override void ExitBinary([NotNull] Parser.BinaryContext ctx)
        {
            switch (ctx.op.Type)
            {
                case Parser.Add:
                    this.Emit(OpCodes.Add);
                    break;
                case Parser.Subtract:
                    this.Emit(OpCodes.Sub);
                    break;
                case Parser.Multiply:
                    this.Emit(OpCodes.Mul);
                    break;
                case Parser.Divide:
                    this.Emit(OpCodes.Div);
                    break;
                case Parser.FloorDivide:
                    this.Emit(OpCodes.Div);
                    this.Emit(OpCodes.Call, this.Floor);
                    break;
                case Parser.Reminder:
                    this.Emit(OpCodes.Rem);
                    break;
                case Parser.Power:
                    this.Emit(OpCodes.Call, this.Pow);
                    break;
                case Parser.Max:
                    this.Emit(OpCodes.Call, this.Max);
                    break;
                case Parser.Min:
                    this.Emit(OpCodes.Call, this.Min);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void ExitUnary([NotNull] Parser.UnaryContext ctx)
        {
            if (null != ctx.Op)
            {
                switch (ctx.Op.Type)
                {
                    case Parser.Round:
                        this.Emit(OpCodes.Call, this.Round);
                        break;
                    case Parser.Ceiling:
                        this.Emit(OpCodes.Call, this.Ceiling);
                        break;
                    case Parser.Floor:
                        this.Emit(OpCodes.Call, this.Floor);
                        break;
                    case Parser.Truncation:
                        this.Emit(OpCodes.Call, this.Truncate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // If the first child is not a token, it must be NegateContext
                this.Emit(OpCodes.Neg);
            }
        }

        #region Emit
        private void Emit(OpCode opcode)
        {
            this.Instructions.Add(this.il.Create(opcode));
        }

        private void Emit(OpCode opcode, byte value)
        {
            this.Instructions.Add(this.il.Create(opcode, value));
        }

        private void Emit(OpCode opcode, double value)
        {
            this.Instructions.Add(this.il.Create(opcode, value));
        }

        private void Emit(OpCode opcode, MethodReference method)
        {
            this.Instructions.Add(this.il.Create(opcode, method));
        }
        #endregion
    }
}
