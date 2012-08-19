using System;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class IntervalRationalAssumerBase<TVar, TExpr, TInterval> :
                IntervalAssumerBase<TVar, TExpr, TInterval, Rational>
                where TInterval : IntervalBase<TInterval, Rational>
                where TVar : IEquatable<TVar> {
                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessThan
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        bool isBottom;
                        var constraints =
                                IntervalInference.ConstraintsFor.LessThan<IIntervalEnvironment<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (left, right, env.Decoder, env, out isBottom);
                        if (isBottom)
                                return env.Bottom;

                        var res = env;
                        foreach (var v in constraints.Keys) {
                                var intervals = constraints[v].AsEnumerable ();
                                foreach (var intv in intervals)
                                        res = res.RefineVariable (v, intv);
                        }

                        return res;
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessEqualThan
                        (TExpr left, TExpr right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        bool isBottom;
                        var constraints =
                                IntervalInference.ConstraintsFor.LessEqualThan
                                        <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (left, right, env.Decoder, env,
                                         out isBottom);
                        if (isBottom)
                                return env.Bottom;

                        return AssumeConstraints (constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeGreaterEqualThanZero
                        (TExpr expr,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        var constraints =
                                IntervalInference.ConstraintsFor.GreaterEqualThanZero
                                        <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                        (expr, env.Decoder, env);
                        return AssumeConstraints (constraints, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessThan
                        (TInterval intv,
                         TVar right,
                         IntervalEnvironmentBase
                                 <TVar,
                                 TExpr,
                                 TInterval
                                 ,
                                 Rational
                                 > env)
                {
                        TInterval refined;
                        if (
                                !IntervalInference.ConstraintsFor.TryRefineLessEqualThan
                                         <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                         (intv, right, env, out refined))
                                return env;

                        return env.With (right, refined);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeLessEqualThan
                        (
                        TInterval intv, TVar right, IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> env)
                {
                        TInterval refined;
                        if (
                                !IntervalInference.ConstraintsFor.TryRefineLessEqualThan
                                         <IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational>, TVar, TExpr, TInterval>
                                         (intv, right, env, out refined))
                                return env;

                        return env.With (right, refined);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, Rational> AssumeNotEqualToZero
                        (TExpr e,
                         IntervalEnvironmentBase
                                 <
                                 TVar
                                 ,
                                 TExpr
                                 ,
                                 TInterval
                                 ,
                                 Rational
                                 >
                                 env)
                {
                        var variable = env.Decoder.UnderlyingVariable (e);

                        var intv = env.Eval (e);

                        TInterval refinement;
                        if (intv.LowerBound.IsZero)
                                refinement = env.Context.For (1L, intv.UpperBound);
                        else if (intv.UpperBound.IsZero)
                                refinement = env.Context.For (intv.LowerBound, -1L);
                        else
                                refinement = env.Context.TopValue;

                        return env.With (variable, intv.Meet (refinement));
                }
                }
}