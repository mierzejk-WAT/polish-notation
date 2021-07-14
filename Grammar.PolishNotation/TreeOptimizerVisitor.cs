namespace Grammar.PolishNotation
{
    using System.Collections.Generic;

    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using Grammar.PolishNotation.Utilities;
    using static PolishNotationParser;

    internal sealed class TreeOptimizerVisitor : PolishNotationBaseVisitor<(IParseTree ctx, double? value)>
    {
        public ArgumentContext Visit(ExpressionContext ctx)
        {
            return ((ExpressionContext)this.VisitExpression(ctx).ctx).tree;
        }

        public override (IParseTree ctx, double? value) VisitExpression(ExpressionContext ctx)
        {
            var value = this.Visit(ctx.tree).value;
            if (value.HasValue)
            {
                var argument = ctx.tree;
                argument.children = new List<IParseTree> { value.Value.ToNumberContext() };
            }

            return (ctx, null);
        }

        public override (IParseTree ctx, double? value) VisitEnclosedArg(EnclosedArgContext ctx)
        {
            return this.Visit(ctx.argument());
        }

        public override (IParseTree ctx, double? value) VisitBinary(BinaryContext ctx)
        {
            var value1 = this.Visit(ctx.argument(0)).value;
            var value2 = this.Visit(ctx.argument(1)).value;
            var value = ctx.GetBinaryValue(value1, value2);
            if (null != value)
            {
                return (null, value);
            }

            if (value1.HasValue)
            {
                ctx.children[1] = value1.Value.ToNumberContext();
            }
            else if (value2.HasValue)
            {
                ctx.children[2] = value2.Value.ToNumberContext();
            }

            return (ctx, null);
        }

        public override (IParseTree ctx, double? value) VisitUnary(UnaryContext ctx)
        {
            var arg = ctx.argument();
            var value = ctx.GetUnaryValue(this.Visit(arg).value);
            if (null != value)
            {
                return (null, value);
            }

            var innerUnary = arg.GetRuleContext<UnaryContext>(0);
            var innerBinary = arg.GetRuleContext<BinaryContext>(0);
            ParserRuleContext result = null;
            if (null != innerUnary)
            {
                if (ctx.ReturnsInteger && innerUnary.ReturnsInteger)
                {
                    result = innerUnary;
                }
                if (ctx.IsNegation && innerUnary.IsNegation)
                {
                    result = innerUnary.argument();
                }
                if (null != result)
                {
                    ((ArgumentContext)ctx.Parent).children[0] = result;
                }
            }
            if (null != innerBinary)
            {
                if (ctx.ReturnsInteger && innerBinary.ReturnsInteger)
                {
                    result = innerBinary;
                    ((ArgumentContext)ctx.Parent).children[0] = result;
                }
            }

            return (result ?? ctx, null);
        }

        public override (IParseTree ctx, double? value) VisitNumber(NumberContext ctx)
        {
            return (null, ctx.GetNumberValue());
        }
    }
}
