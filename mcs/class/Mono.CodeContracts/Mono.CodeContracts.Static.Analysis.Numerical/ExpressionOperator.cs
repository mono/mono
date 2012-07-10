namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    enum ExpressionOperator
    {
        Constant,
        Variable,
        Not,
        And,
        Or,
        Xor,
        LogicalAnd,
        LogicalOr,
        LogicalNot,
        Equal,
        Equal_Obj,
        NotEqual,
        LessThan,
        LessEqualThan,
        GreaterThan,
        GreaterEqualThan,
        Add,
        Sub,
        Mult,
        Div,
        Mod,
        UnaryMinus,
        SizeOf,
        Unknown
    }

    public static class ExpressionOperatorExtensions
    {
        internal static bool IsGreaterThan(this ExpressionOperator op)
        {
            return op == ExpressionOperator.GreaterThan;
        }

        internal static bool IsGreaterEqualThan(this ExpressionOperator op)
        {
            return op == ExpressionOperator.GreaterEqualThan;
        }

        internal static bool IsLessThan(this ExpressionOperator op)
        {
            return op == ExpressionOperator.LessThan;
        }

        internal static bool IsLessEqualThan(this ExpressionOperator op)
        {
            return op == ExpressionOperator.LessEqualThan;
        }
    }
}