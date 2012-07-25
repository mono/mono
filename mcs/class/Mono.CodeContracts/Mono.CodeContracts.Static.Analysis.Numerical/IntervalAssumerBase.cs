using System;

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

        public abstract TEnv AssumeEqual         (Expr left, Expr right);
        public abstract TEnv AssumeNotEqual      (Expr left, Expr right);
        public abstract TEnv AssumeLessThan      (Expr left, Expr right);
        public abstract TEnv AssumeLessEqualThan (Expr left, Expr right);

        public abstract TEnv AssumeEqualToZero   (Var v);
        public abstract TEnv AssumeNotEqualToZero(Var v);
    }

    class IntervalTrueTester<TEnv, Var, Expr> : IntervalAssumerBase<TEnv, Var, Expr, Interval, Rational>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, Interval, Rational> 
        where Var : IEquatable<Var> {
        
        public IntervalTrueTester (TEnv env)
            : base (env)
        {
        }

        public override TEnv AssumeEqual (Expr left, Expr right)
        {
            throw new NotImplementedException ();
        }

        public override TEnv AssumeNotEqual (Expr left, Expr right)
        {
            throw new NotImplementedException ();
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

        public override TEnv AssumeEqualToZero(Var var)
        {
            Interval a = Interval.For (0);
            
            return env.RefineVariable (var, a);
            
        }
    }
}