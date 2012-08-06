using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalAssumeTrueVisitor<TEnv, Var, Expr, TInterval, TNumeric> : AssumeTrueVisitor<TEnv, Var, Expr>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric> 
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> 
    {
        public IntervalAssumeTrueVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
        {
        }

        protected override TEnv DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, TEnv env)
        {
            env = cmp (left, right, original, env);
            return base.DispatchCompare (cmp, left, right, original, env);
        }

        public override TEnv VisitEqual(Expr left, Expr right, Expr original, TEnv env)
        {
            return env.Assumer.AssumeEqual (left, right, env);
        }

        public override TEnv VisitLessThan(Expr left, Expr right, Expr original, TEnv env)
        {
            return env.Assumer.AssumeLessThan (left, right, env);
        }

        public override TEnv VisitLessEqualThan(Expr left, Expr right, Expr original, TEnv env)
        {
            return env.Assumer.AssumeLessEqualThan (left, right, env);
        }

        public override TEnv VisitAddition(Expr left, Expr right, Expr original, TEnv env)
        {
            env = base.VisitAddition (left, right, original, env);
            return env.Assumer.AssumeNotEqualToZero (original, env);
        }

        public override TEnv VisitDivision(Expr left, Expr right, Expr original, TEnv env)
        {
            env = base.VisitDivision (left, right, original, env);
            return env.Assumer.AssumeNotEqualToZero (original, env);
        }

        public override TEnv VisitMultiply(Expr left, Expr right, Expr original, TEnv env)
        {
            env = base.VisitMultiply (left, right, original, env);
            return env.Assumer.AssumeNotEqualToZero (original, env);
        }

        public override TEnv VisitUnknown(Expr expr, TEnv env)
        {
            env = base.VisitUnknown(expr, env);
            return env.Assumer.AssumeNotEqualToZero (expr, env);
        }

        public override TEnv VisitNot(Expr expr, TEnv env)
        {
            return FalseVisitor.Visit (expr, env);
        }

        public override TEnv VisitNotEqual(Expr left, Expr right, Expr original, TEnv env)
        {
            return env.Assumer.AssumeNotEqual (left, right, env);
        }

        public override TEnv VisitVariable(Var var, Expr expr, TEnv env)
        {
            return env.Assumer.AssumeNotEqualToZero (expr, env);
        }

        public override TEnv VisitSubtraction(Expr left, Expr right, Expr original, TEnv env)
        {
            env = base.VisitSubtraction(left, right, original, env);
            return env.Assumer.AssumeNotEqualToZero (original, env);
        }
    }
}