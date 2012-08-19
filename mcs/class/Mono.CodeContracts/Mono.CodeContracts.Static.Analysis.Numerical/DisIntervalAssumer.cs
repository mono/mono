using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class DisIntervalAssumer<Var, Expr> : IntervalRationalAssumerBase<Var, Expr, DisInterval>
                where Var : IEquatable<Var> {
                public override IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeNotEqualToZero
                        (Var var, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        return AssumeEqualToDisInterval (var, DisInterval.NotZero, env);
                }

                static IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeEqualToDisInterval
                        (Var var, DisInterval intv, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        return env.RefineVariable (var, intv);
                }

                public override IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> AssumeNotEqual
                        (Expr left, Expr right, IntervalEnvironmentBase<Var, Expr, DisInterval, Rational> env)
                {
                        var result = env;

                        var rightIntv = env.Eval (right);
                        if (rightIntv.IsSinglePoint) {
                                var everythingExcept = DisInterval.EverythingExcept (rightIntv);
                                result = result.RefineVariable (env.Decoder.UnderlyingVariable (left), everythingExcept);
                        }

                        IntervalInference.InferenceResult<Var, DisInterval> resultLeft;
                        IntervalInference.InferenceResult<Var, DisInterval> resultRight;
                        IntervalInference.ConstraintsFor.NotEqual (left, right, env.Decoder, result, out resultLeft,
                                                                   out resultRight);

                        var join = resultLeft.Join (resultRight);
                        if (join.IsBottom)
                                return env.Bottom as IntervalEnvironmentBase<Var, Expr, DisInterval, Rational>;

                        return AssumeConstraints (join.Constraints, env);
                }
                }
}