using System.Collections.Generic;

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

    static class ExpressionOperatorExtensions
    {
        private static readonly HashSet<ExpressionOperator> relationalOperators = new HashSet<ExpressionOperator> (); 

        static ExpressionOperatorExtensions()
        {
            relationalOperators.Add (ExpressionOperator.LessThan);
            relationalOperators.Add (ExpressionOperator.LessEqualThan);
            relationalOperators.Add (ExpressionOperator.GreaterThan);
            relationalOperators.Add (ExpressionOperator.GreaterEqualThan);
            relationalOperators.Add (ExpressionOperator.Equal);
            relationalOperators.Add (ExpressionOperator.Equal_Obj);
            relationalOperators.Add (ExpressionOperator.NotEqual);
        }

        internal static bool IsUnary(this ExpressionOperator op)
        {
            return op == ExpressionOperator.UnaryMinus || op == ExpressionOperator.Not;
        }

        internal static bool IsZerary(this ExpressionOperator op)
        {
            return op == ExpressionOperator.Constant || op == ExpressionOperator.Variable;
        }

        internal static bool IsBinary(this ExpressionOperator op)
        {
            return !op.IsUnary () && !op.IsZerary();
        }

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

        internal static bool IsRelational(this ExpressionOperator op)
        {
            return relationalOperators.Contains (op);
        }
    }
}