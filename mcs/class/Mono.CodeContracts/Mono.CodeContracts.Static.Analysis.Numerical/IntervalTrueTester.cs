using System;
using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    abstract class IntervalRationalAssumerBase<TEnv, Var, Expr, TInterval> : IntervalAssumerBase<TEnv, Var, Expr, TInterval, Rational>
        where TInterval : IntervalBase<TInterval, Rational> 
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, Rational> 
        where Var : IEquatable<Var> {
        
        protected IntervalRationalAssumerBase (TEnv env)
            : base (env)
        {
        }

        public override TEnv AssumeLessThan(TInterval intv, Var right)
        {
            TInterval refined;
            if (!IntervalInference.ConstraintsFor.TryRefineLessEqualThan (intv, right, env, out refined))
                return env;

            return env.With(right, refined);
        }

        public override TEnv AssumeLessEqualThan(TInterval intv, Var right)
        {
            TInterval refined;
            if (!IntervalInference.ConstraintsFor.TryRefineLessEqualThan(intv, right, env, out refined))
                return env;

            return env.With(right, refined);
        }

        public override TEnv AssumeNotEqualToZero(Expr e)
        {
            Var variable = this.env.Decoder.UnderlyingVariable(e);

            var current = this.env.Eval(e);
            
            TInterval refinement;
            if (current.LowerBound.IsZero)
                refinement = env.Context.For(1L, current.UpperBound);
            else if (current.UpperBound.IsZero)
                refinement = env.Context.For(current.LowerBound, -1L);
            else
                refinement = env.Context.TopValue;

            return this.env.With(variable, current.Meet(refinement));
        }

        public override TEnv AssumeNotEqual(Expr left, Expr right)
        {
            IntervalInference.InferenceResult<Var, TInterval> resultLeft;
            IntervalInference.InferenceResult<Var, TInterval> resultRight;
            IntervalInference.ConstraintsFor.NotEqual(left, right, env.Decoder, env, out resultLeft, out resultRight);

            var join = resultLeft.Join(resultRight);
            if (join.IsBottom)
                return env.Bottom;

            return this.AssumeConstraints(join.Constraints);
        }
    }

    class IntervalAssumer<TEnv, Var, Expr> : IntervalRationalAssumerBase<TEnv, Var, Expr, Interval>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, Interval, Rational> 
        where Var : IEquatable<Var> {

        public IntervalAssumer(TEnv env)
            : base (env)
        {
        }

        public override TEnv AssumeLessThan (Expr left, Expr right)
        {
            throw new NotImplementedException ();
        }

        public override TEnv AssumeLessEqualThan (Expr left, Expr right)
        {
            throw new NotImplementedException ();
        }

        public override TEnv AssumeNotEqualToZero (Var var)
        {
            //do nothing, we can't exclude one point
            return env;
        }
    }
}