using System;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    abstract class IntervalAssumerBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> 
    {
        protected readonly TEnv env;

        protected IntervalAssumerBase(TEnv env)
        {
            this.env = env;
        }

        public virtual TEnv AssumeEqual         (Expr left, Expr right)
        {
            var leftVar = env.Decoder.UnderlyingVariable(left);
            var rightVar = env.Decoder.UnderlyingVariable(right);

            if (env.Contains(leftVar))
            {
                var res = env;
                var interval = env.Eval(left).Meet(env.Eval(right));

                res = res.With(leftVar, interval);
                res = res.With(rightVar, interval);

                return res;
            }

            if (env.Decoder.IsConstant(left) && env.Decoder.IsConstant(right) && env.Eval(left).Meet(env.Eval(right)).IsBottom)
                return env.Bottom;

            return env;
        }

        public virtual TEnv AssumeEqualToZero(Var var)
        {
            return env.RefineVariable(var, env.Context.Zero);
        }

        public abstract TEnv AssumeNotEqual      (Expr left, Expr right);
        public abstract TEnv AssumeLessThan      (Expr left, Expr right);
        public abstract TEnv AssumeLessEqualThan (Expr left, Expr right);

        public abstract TEnv AssumeNotEqualToZero(Var v);

        public abstract TEnv AssumeNotEqualToZero (Expr v);

        public abstract TEnv AssumeLessEqualThan (TInterval intv, Var right);
        public abstract TEnv AssumeLessThan (TInterval intv, Var right);

        protected TEnv AssumeConstraints(IImmutableMap<Var, Sequence<TInterval>> constraints)
        {
            TEnv res = env;
            foreach (var v in constraints.Keys)
            {
                var seq = constraints[v];
                foreach (var intv in seq.AsEnumerable())
                    res = res.RefineVariable(v, intv);
            }

            return res;
        }
    }
}