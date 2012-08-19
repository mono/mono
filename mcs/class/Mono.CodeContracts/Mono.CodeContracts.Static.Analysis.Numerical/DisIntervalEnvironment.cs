using System;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class DisIntervalEnvironment<TVar, TExpr> :
                IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> where TVar : IEquatable<TVar> {
                static DisIntervalAssumer<TVar, TExpr> cached_assumer;

                DisIntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder,
                                        EnvironmentDomain<TVar, DisInterval> varsToInterval)
                        : base (decoder, varsToInterval)
                {
                }

                public DisIntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override IntervalAssumerBase<TVar, TExpr, DisInterval, Rational> Assumer
                {
                        get
                        {
                                return cached_assumer ??
                                       (cached_assumer = new DisIntervalAssumer<TVar, TExpr> ());
                        }
                }

                public override IntervalContextBase<DisInterval, Rational> Context { get { return DisIntervalContext.Instance; } }

                protected override IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> NewInstance (
                        EnvironmentDomain<TVar, DisInterval> varsToIntervals)
                {
                        return new DisIntervalEnvironment<TVar, TExpr> (Decoder, varsToIntervals);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, DisInterval, Rational> AssumeVariableIn (TVar var,
                                                                                                              Interval
                                                                                                                      interval)
                {
                        return RefineVariable (var, DisInterval.For (interval));
                }
                }
}