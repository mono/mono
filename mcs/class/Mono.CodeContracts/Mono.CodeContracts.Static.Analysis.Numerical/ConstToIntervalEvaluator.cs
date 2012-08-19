using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class ConstToIntervalEvaluator<TContext, TVar, TExpr, TInterval, TNumeric> :
                GenericTypeExpressionVisitor<TVar, TExpr, TContext, TInterval>
                where TContext : IntervalContextBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                public ConstToIntervalEvaluator (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override TInterval Visit (TExpr e, TContext ctx)
                {
                        if (!ReferenceEquals (e, null) && Decoder.IsConstant (e))
                                return base.Visit (e, ctx);

                        return Default (e, ctx);
                }

                protected override TInterval VisitBool (TExpr expr, TContext ctx)
                {
                        bool value;
                        if (Decoder.TryValueOf (expr, ExpressionType.Bool, out value))
                                return ctx.For (value ? 1L : 0L);

                        return Default (expr, ctx);
                }

                protected override TInterval VisitInt32 (TExpr expr, TContext ctx)
                {
                        int value;
                        if (Decoder.TryValueOf (expr, ExpressionType.Int32, out value))
                                return ctx.For (value);

                        return Default (expr, ctx);
                }

                protected override TInterval Default (TExpr expr, TContext ctx)
                {
                        return ctx.TopValue;
                }
                }
}