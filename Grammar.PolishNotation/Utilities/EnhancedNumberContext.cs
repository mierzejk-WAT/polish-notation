namespace Grammar.PolishNotation.Utilities
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    using Parser = PolishNotationParser;

    public sealed class EnhancedNumberContext : Parser.NumberContext
    {
        public EnhancedNumberContext(double d)
            : base(ParserRuleContext.EmptyContext, -1)
        {
            this.Value = d;
            this.AddChild(new TerminalNodeImpl(new CommonToken(0, d.ToFormattedString())));
        }

        public double Value { get; }
    }
}
