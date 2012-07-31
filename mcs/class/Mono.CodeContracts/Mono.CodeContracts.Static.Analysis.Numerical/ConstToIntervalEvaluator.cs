using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    class ConstToIntervalEvaluator<TEnv, TVar, TExpr, TInterval, TNumeric> : GenericTypeExpressionVisitor<TVar, TExpr, TEnv, TInterval> 
        where TEnv : IntervalEnvironmentBase<TEnv, TVar, TExpr, TInterval, TNumeric> 
        where TVar : IEquatable<TVar> 
        where TInterval : IntervalBase<TInterval, TNumeric> 
    {
        public ConstToIntervalEvaluator (IExpressionDecoder<TVar, TExpr> decoder)
            : base (decoder)
        {
        }

        public override TInterval Visit(TExpr e, TEnv env)
        {
            if (!ReferenceEquals(e, null) && this.Decoder.IsConstant(e))
                return base.Visit (e, env);

            return this.Default (e, env);
        }

        protected override TInterval VisitBool(TExpr expr, TEnv env)
        {
            bool value;
            if (Decoder.TryValueOf(expr, ExpressionType.Bool, out value))
                return env.Context.For (value ? 1L : 0L);

            return this.Default (expr, env);
        }

        protected override TInterval VisitInt32(TExpr expr, TEnv env)
        {
            int value;
            if (Decoder.TryValueOf(expr, ExpressionType.Int32, out value))
                return env.Context.For(value);

            return this.Default(expr, env);
        }

        protected override TInterval Default (TExpr expr, TEnv env)
        {
            return env.Context.TopValue;
        }
    }
}