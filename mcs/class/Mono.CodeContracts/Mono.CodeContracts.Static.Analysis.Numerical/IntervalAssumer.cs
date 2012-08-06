using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalAssumer<TEnv, Var, Expr> : IntervalRationalAssumerBase<TEnv, Var, Expr, Interval>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, Interval, Rational>
        where Var : IEquatable<Var>
    {
        public override TEnv AssumeNotEqualToZero (Var var, TEnv env)
        {
            //do nothing, we can't exclude one point
            return env;
        }

        public override TEnv AssumeNotEqual (Expr left, Expr right, TEnv env)
        {
            IntervalInference.InferenceResult<Var, Interval> resultLeft;
            IntervalInference.InferenceResult<Var, Interval> resultRight;
            IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, env, out resultLeft, out resultRight);

            var join = resultLeft.Join (resultRight);
            if (join.IsBottom)
                return env.Bottom;

            return this.AssumeConstraints (join.Constraints, env);
        }
    }

}