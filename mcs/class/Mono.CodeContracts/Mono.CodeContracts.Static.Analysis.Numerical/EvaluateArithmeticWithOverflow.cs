using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static class EvaluateArithmeticWithOverflow {
                public static bool TryBinary (ExpressionOperator op, long left, long right, out int res)
                {
                        return LongToIntegerConstantEvaluator.Evaluate (op, left, right, out res);
                }

                public static bool TryBinary (ExpressionOperator op, long left, long right, out uint res)
                {
                        return false.Without (out res);
                }

                public static bool TryBinary<In> (ExpressionType targetType, ExpressionOperator op, In left, In right,
                                                  out object res)
                        where In : struct
                {
                        switch (targetType) {
                        case ExpressionType.Unknown:
                        case ExpressionType.Bool:
                                return false.Without (out res);

                        case ExpressionType.Int32:
                                var l = left.ConvertToLong ();
                                var r = right.ConvertToLong ();

                                int intValue;
                                if (l.HasValue && r.HasValue && TryBinary (op, l.Value, r.Value, out intValue))
                                        return true.With (intValue, out res);

                                break;
                        }

                        return false.Without (out res);
                }
        }
}