using System;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalEnvironment<TVar, TExpr> : IntervalEnvironmentBase<IntervalEnvironment<TVar, TExpr>, TVar, TExpr, Interval, Rational> 
        where TVar : IEquatable<TVar> 
    {
        private static IntervalAssumer<IntervalEnvironment<TVar, TExpr>, TVar, TExpr> cachedAssumer;

        public IntervalEnvironment(IExpressionDecoder<TVar, TExpr> decoder) 
            : base(decoder)
        {
        }

        private IntervalEnvironment (IExpressionDecoder<TVar, TExpr> decoder, EnvironmentDomain<TVar, Interval> varsToInterval)
            : base (decoder, varsToInterval)
        {
        }
        
        public override IntervalAssumerBase<IntervalEnvironment<TVar, TExpr>, TVar, TExpr, Interval, Rational> Assumer
        {
            get
            {
                return cachedAssumer ??
                       (cachedAssumer = new IntervalAssumer<IntervalEnvironment<TVar, TExpr>, TVar, TExpr> ());
            }
        }

        public override IntervalContextBase<Interval, Rational> Context { get { return IntervalContext.Instance;} }

        protected override IntervalEnvironment<TVar, TExpr> NewInstance(EnvironmentDomain<TVar, Interval> varsToIntervals)
        {
            return new IntervalEnvironment<TVar, TExpr>(this.Decoder, varsToIntervals);
        }
    }
}