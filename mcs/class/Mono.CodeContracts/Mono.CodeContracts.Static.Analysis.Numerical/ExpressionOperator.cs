using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        enum ExpressionOperator {
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
                Unknown,

                ConvertToInt32
        }

        static class ExpressionOperatorExtensions {
                static readonly HashSet<ExpressionOperator> relationalOperators = new HashSet<ExpressionOperator> ();

                static ExpressionOperatorExtensions ()
                {
                        relationalOperators.Add (ExpressionOperator.LessThan);
                        relationalOperators.Add (ExpressionOperator.LessEqualThan);
                        relationalOperators.Add (ExpressionOperator.GreaterThan);
                        relationalOperators.Add (ExpressionOperator.GreaterEqualThan);
                        relationalOperators.Add (ExpressionOperator.Equal);
                        relationalOperators.Add (ExpressionOperator.Equal_Obj);
                        relationalOperators.Add (ExpressionOperator.NotEqual);
                }

                public static bool IsUnary (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.UnaryMinus || op == ExpressionOperator.Not;
                }

                public static bool IsZerary (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.Constant || op == ExpressionOperator.Variable;
                }

                public static bool IsBinary (this ExpressionOperator op)
                {
                        return !op.IsUnary () && !op.IsZerary ();
                }

                public static bool IsGreaterThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.GreaterThan;
                }

                public static bool IsGreaterEqualThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.GreaterEqualThan;
                }

                public static bool IsLessThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.LessThan;
                }

                public static bool IsLessEqualThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.LessEqualThan;
                }

                public static bool IsRelational (this ExpressionOperator op)
                {
                        return relationalOperators.Contains (op);
                }
        }
}