using System;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    internal abstract class IntervalRationalAssumerBase<TEnv, Var, Expr, TInterval> :
        IntervalAssumerBase<TEnv, Var, Expr, TInterval, Rational>
        where TInterval : IntervalBase<TInterval, Rational>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, Rational>
        where Var : IEquatable<Var>
    {
        public override TEnv AssumeLessThan (Expr left, Expr right, TEnv env)
        {
            bool isBottom;
            var constraints = IntervalInference.ConstraintsFor.LessThan (left, right, env.Decoder, env, out isBottom);
            if (isBottom)
                return env.Bottom;

            TEnv res = env;
            foreach (var v in constraints.Keys)
            {
                var intervals = constraints[v].AsEnumerable ();
                foreach (var intv in intervals)
                    res = res.RefineVariable (v, intv);
            }

            return res;
        }

        public override TEnv AssumeLessEqualThan (Expr left, Expr right, TEnv env)
        {
            bool isBottom;
            var constraints = IntervalInference.ConstraintsFor.LessEqualThan (left, right, env.Decoder, env,
                                                                              out isBottom);
            if (isBottom)
                return env.Bottom;

            return AssumeConstraints (constraints, env);
        }

        public override TEnv AssumeGreaterEqualThanZero (Expr expr, TEnv env)
        {
            var constraints = IntervalInference.ConstraintsFor.GreaterEqualThanZero (expr, env.Decoder, env);
            return AssumeConstraints (constraints, env);
        }

        public override TEnv AssumeLessThan (TInterval intv, Var right, TEnv env)
        {
            TInterval refined;
            if (!IntervalInference.ConstraintsFor.TryRefineLessEqualThan (intv, right, env, out refined))
                return env;

            return env.With (right, refined);
        }

        public override TEnv AssumeLessEqualThan (TInterval intv, Var right, TEnv env)
        {
            TInterval refined;
            if (!IntervalInference.ConstraintsFor.TryRefineLessEqualThan (intv, right, env, out refined))
                return env;

            return env.With (right, refined);
        }

        public override TEnv AssumeNotEqualToZero (Expr e, TEnv env)
        {
            Var variable = env.Decoder.UnderlyingVariable (e);

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