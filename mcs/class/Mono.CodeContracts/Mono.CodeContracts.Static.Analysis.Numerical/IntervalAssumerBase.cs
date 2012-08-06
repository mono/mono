using System;
using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    abstract class IntervalAssumerBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> 
    {
        public virtual TEnv AssumeEqual (Expr left, Expr right, TEnv env)
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
        public virtual TEnv AssumeEqualToZero (Var var, TEnv env)
        {
            return env.RefineVariable (var, env.Context.Zero);
        }

        public virtual TEnv AssumeNotEqual (Expr left, Expr right, TEnv env)
        {
            int value;
            if (env.Decoder.OperatorFor (left).IsRelational () && env.Decoder.IsConstantInt (right, out value))
                return value == 0 ? env.AssumeTrue (left) : env.AssumeFalse (left);

            var assumer = env.Assumer;
            return assumer.AssumeLessThan (left, right, env)
                .Join (assumer.AssumeLessThan (right, left, env));
        }

        public abstract TEnv AssumeLessThan       (Expr left, Expr right, TEnv env);
        public abstract TEnv AssumeLessEqualThan  (Expr left, Expr right, TEnv env);
                                                  
        public abstract TEnv AssumeNotEqualToZero (Var v, TEnv env);
        public abstract TEnv AssumeNotEqualToZero (Expr e, TEnv env);

        public abstract TEnv AssumeLessEqualThan (TInterval intv, Var right, TEnv env);
        public abstract TEnv AssumeLessThan      (TInterval intv, Var right, TEnv env);

        public abstract TEnv AssumeGreaterEqualThanZero (Expr expr, TEnv env);

        protected TEnv AssumeConstraints (IImmutableMap<Var, Sequence<TInterval>> constraints, TEnv env)
        {
            TEnv res = env;
            foreach (var v in constraints.Keys)
            {
                var seq = constraints[v];
                foreach (var intv in seq.AsEnumerable ())
                    res = res.RefineVariable (v, intv);
            }

            return res;
        }
        protected TEnv AssumeConstraints (IDictionary<Var, Sequence<TInterval>> constraints, TEnv env)
        {
            TEnv res = env;
            foreach (var v in constraints.Keys)
            {
                var seq = constraints[v];
                foreach (var intv in seq.AsEnumerable ())
                    res = res.RefineVariable (v, intv);
            }

            return res;
        }
    }
}