using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class DisIntervalAssumer<TEnv, Var, Expr> : IntervalRationalAssumerBase<TEnv, Var, Expr, DisInterval>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, DisInterval, Rational>
        where Var : IEquatable<Var>
    {
        public override TEnv AssumeNotEqualToZero (Var var, TEnv env)
        {
            return AssumeEqualToDisInterval (var, DisInterval.NotZero, env);
        }

        private static TEnv AssumeEqualToDisInterval (Var var, DisInterval intv, TEnv env)
        {
            return env.RefineVariable (var, intv);
        }

        public override TEnv AssumeNotEqual (Expr left, Expr right, TEnv env)
        {
            TEnv result = env;

            var rightIntv = env.Eval (right);
            if (rightIntv.IsSinglePoint)
            {
                var everythingExcept = DisInterval.EverythingExcept (rightIntv);
                result = result.RefineVariable (env.Decoder.UnderlyingVariable (left), everythingExcept);
            }

            IntervalInference.InferenceResult<Var, DisInterval> resultLeft;
            IntervalInference.InferenceResult<Var, DisInterval> resultRight;
            IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, result, out resultLeft, out resultRight);

            var join = resultLeft.Join (resultRight);
            if (join.IsBottom)
                return env.Bottom;

            return this.AssumeConstraints (join.Constraints, env);
        }
    }
}