using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

// ReSharper disable once CheckNamespace
public partial class PolishNotationParser
{
    public partial class UnaryContext
    {
        /* private static readonly int[] IntegerOperations =
            {
                PolishNotationParser.Truncation,
                PolishNotationParser.Round,
                PolishNotationParser.Ceiling,
                PolishNotationParser.Floor
            }; */

        public IToken Op => this.GetChild<ITerminalNode>(0)?.Symbol;

        // All tokens are integer operations; negation is a context, not a token.
        // public bool ReturnsInteger => Array.IndexOf(IntegerOperations, this.Op?.Type) > -1;
        public bool ReturnsInteger => null != this.Op;

        // If it is not a token, it must be negation.
        public bool IsNegation => null == this.Op;
    }

    public partial class BinaryContext
    {
        public bool ReturnsInteger => this.op.Type == PolishNotationParser.FloorDivide;
    }
}
