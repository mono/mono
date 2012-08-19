using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class IntervalAssumer<TVar, TExpr> : IntervalRationalAssumerBase<TVar, TExpr, Interval>
                where TVar : IEquatable<TVar> {
                public override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> AssumeNotEqual
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> env)
                {
                        IntervalInference.InferenceResult<TVar, Interval> resultLeft;
                        IntervalInference.InferenceResult<TVar, Interval> resultRight;
                        IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, env, out resultLeft,
                                                                   out resultRight);

                        IntervalInference.InferenceResult<TVar, Interval> join = resultLeft.Join (resultRight);
                        if (join.IsBottom)
                                return env.Bottom;

                        return this.AssumeConstraints (join.Constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> AssumeNotEqualToZero
                        (TVar v, IntervalEnvironmentBase<TVar, TExpr, Interval, Rational> env)
                {
                        //do nothing, we can't exclude one point
                        return env;
                }
         }
}