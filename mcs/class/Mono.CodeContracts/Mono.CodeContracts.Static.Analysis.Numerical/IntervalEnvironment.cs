using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class IntervalEnvironment<TVar, TExpr> :
                IntervalEnvironmentBase<TVar, TExpr, Interval, Rational>
                where TVar : IEquatable<TVar> {
                static IntervalAssumer<TVar, TExpr> cachedAssumer;

                public IntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                IntervalEnvironment
                        (IExpressionDecoder<TVar, TExpr> decoder,
                         EnvironmentDomain<TVar, Interval> varsToInterval)
                        : base (decoder, varsToInterval)
                {
                }

                public override IntervalAssumerBase<TVar, TExpr, Interval, Rational>
                        Assumer { get { return cachedAssumer ?? (cachedAssumer = new IntervalAssumer<TVar, TExpr> ()); } }

                public override IntervalContextBase<Interval, Rational> Context { get { return IntervalContext.Instance; } }

                protected override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> NewInstance
                        (EnvironmentDomain<TVar, Interval> varsToIntervals)
                {
                        return new IntervalEnvironment<TVar, TExpr> (Decoder, varsToIntervals);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> AssumeVariableIn
                        (TVar var, Interval interval)
                {
                        return RefineVariable (var, interval);
                }
        }
}