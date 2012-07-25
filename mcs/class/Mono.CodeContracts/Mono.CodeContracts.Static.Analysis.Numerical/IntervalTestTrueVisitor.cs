using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalTestTrueVisitor<TEnv, Var, Expr, TInterval, TNumeric> : TestTrueVisitor<TEnv, Var, Expr>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric> 
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> 
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
            return data.Assumer.AssumeEqual (left, right);
        }

        public override TEnv VisitLessThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeLessThan (left, right);
        }

        public override TEnv VisitLessEqualThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeLessEqualThan (left, right);
        }

        public override TEnv VisitAddition(Expr left, Expr right, Expr original, TEnv data)
        {
            throw new NotImplementedException();
            //return data.Assumer.AssumeNotEqualToZero (original);
        }
    }
}