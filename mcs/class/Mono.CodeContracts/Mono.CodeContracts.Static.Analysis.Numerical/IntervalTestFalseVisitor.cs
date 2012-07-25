using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalTestFalseVisitor<TEnv, Var, Expr, TInterval, TNumeric> : TestFalseVisitor<TEnv, Var, Expr>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> {
        public IntervalTestFalseVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
        {
        }

        protected override TEnv DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, TEnv data)
        {
            data = cmp(left, right, original, data);
            return base.DispatchCompare(cmp, left, right, original, data);
        }
    }
}