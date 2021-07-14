namespace Grammar.PolishNotation.Utilities
{
    using System;

    using static PolishNotationParser;

    internal static class TreeOptimizer
    {
        public static double GetNumberValue(this NumberContext ctx)
        {
            return ctx.GetText().ParseDouble();
        }

        public static double? GetBinaryValue(this BinaryContext ctx, double? value1, double? value2)
        {
            if (!value1.HasValue || !value2.HasValue)
            {
                return null;
            }

            var arg1 = value1.Value;
            var arg2 = value2.Value;
            switch (ctx.op.Type)
            {
                case Add:
                    return arg1 + arg2;
                case Subtract:
                    return arg1 - arg2;
                case Multiply:
                    return arg1 * arg2;
                case Divide:
                    return arg1 / arg2;
                case FloorDivide:
                    return Math.Floor(arg1 / arg2);
                case Reminder:
                    return arg1 % arg2;
                case Power:
                    return Math.Pow(arg1, arg2);
                case Max:
                    return Math.Max(arg1, arg2);
                case Min:
                    return Math.Min(arg1, arg2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static double? GetUnaryValue(this UnaryContext ctx, double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var arg = value.Value;
            if (null != ctx.Op)
            {
                switch (ctx.Op.Type)
                {
                    case Subtract:
                        return -arg;
                    case Round:
                        return Math.Round(arg);
                    case Ceiling:
                        return Math.Ceiling(arg);
                    case Floor:
                        return Math.Floor(arg);
                    case Truncation:
                        return Math.Truncate(arg);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // If the first child is not a token, it must be NegateContext
            return -arg;
        }

        public static EnhancedNumberContext ToNumberContext(this double d)
        {
            return new EnhancedNumberContext(d);
        }
    }
}
