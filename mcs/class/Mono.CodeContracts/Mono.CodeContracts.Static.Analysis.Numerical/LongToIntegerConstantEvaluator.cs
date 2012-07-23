namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal class LongToIntegerConstantEvaluator : ConstantEvaluatorVisitor<long, int>
    {
        protected override bool VisitAdd (long left, long right, out int result)
        {
            return true.With ((int)left + (int)right, out result);
        }

        protected override bool VisitAnd (long left, long right, out int result)
        {
            return true.With ((int)left & (int)right, out result);
        }

        protected override bool VisitOr (long left, long right, out int result)
        {
            return true.With ((int)left | (int)right, out result);
        }

        protected override bool VisitXor (long left, long right, out int result)
        {
            return true.With ((int)left ^ (int)right, out result);
        }

        protected override bool VisitEqual (long left, long right, out int result)
        {
            return true.With ((int)left == (int)right ? 1 : 0, out result);
        }

        protected override bool VisitNotEqual (long left, long right, out int result)
        {
            return true.With ((int)left != (int)right ? 1 : 0, out result);
        }

        protected override bool VisitLessThan (long left, long right, out int result)
        {
            return true.With ((int)left < (int)right ? 1 : 0, out result);
        }

        protected override bool VisitLessEqualThan (long left, long right, out int result)
        {
            return true.With ((int)left <= (int)right ? 1 : 0, out result);
        }

        protected override bool VisitGreaterThan (long left, long right, out int result)
        {
            return true.With ((int)left > (int)right ? 1 : 0, out result);
        }

        protected override bool VisitGreaterEqualThan (long left, long right, out int result)
        {
            return true.With ((int)left >= (int)right ? 1 : 0, out result);
        }

        protected override bool VisitSub (long left, long right, out int result)
        {
            return true.With ((int)left - (int)right, out result);
        }

        protected override bool VisitMult (long left, long right, out int result)
        {
            return true.With ((int)left * (int)right, out result);
        }

        protected override bool VisitDiv (long left, long right, out int result)
        {
            if (right == 0)
                return false.Without (out result);

            return true.With ((int)left / (int)right, out result);
        }

        protected override bool VisitMod (long left, long right, out int result)
        {
            if (right == 0)
                return false.Without (out result);

            return true.With ((int)left % (int)right, out result);
        }

        public static bool Evaluate(ExpressionOperator op, long left, long right, out int res)
        {
            return new LongToIntegerConstantEvaluator ().VisitBinary (op, left, right, out res);
        }
    }
}