namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalTestTrueVisitor<TEnv, Var, Expr, TInterval, TNumeric> : TestTrueVisitor<TEnv, Var, Expr>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric> 
        where TInterval : IntervalBase<TInterval, TNumeric>
    {
        public IntervalTestTrueVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
        {
        }

        protected override TEnv DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, TEnv data)
        {
            data = cmp (left, right, original, data);
            return base.DispatchCompare (cmp, left, right, original, data);
        }

        public override TEnv VisitEqual(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.TestTrueEqual (left, right);
        }

        public override TEnv VisitLessThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.TestTrueLessThan (left, right);
        }

        public override TEnv VisitLessEqualThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.TestTrueLessEqualThan (left, right);
        }
    }
}