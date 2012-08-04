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

        protected override TEnv DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, TEnv data)
        {
            data = cmp (left, right, original, data);
            return base.DispatchCompare (cmp, left, right, original, data);
        }

        public override TEnv VisitEqual(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeEqual (left, right);
        }

        public override TEnv VisitLessThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeLessThan (left, right);
        }

        public override TEnv VisitLessEqualThan(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeLessEqualThan (left, right);
        }

        public override TEnv VisitAddition(Expr left, Expr right, Expr original, TEnv data)
        {
            data = base.VisitAddition (left, right, original, data);
            return data.Assumer.AssumeNotEqualToZero (original);
        }

        public override TEnv VisitDivision(Expr left, Expr right, Expr original, TEnv data)
        {
            data = base.VisitDivision (left, right, original, data);
            return data.Assumer.AssumeNotEqualToZero (original);
        }

        public override TEnv VisitMultiply(Expr left, Expr right, Expr original, TEnv data)
        {
            data = base.VisitMultiply (left, right, original, data);
            return data.Assumer.AssumeNotEqualToZero (original);
        }

        public override TEnv VisitUnknown(Expr expr, TEnv data)
        {
            data = base.VisitUnknown(expr, data);
            return data.Assumer.AssumeNotEqualToZero (expr);
        }

        public override TEnv VisitNot(Expr expr, TEnv data)
        {
            return FalseVisitor.Visit (expr, data);
        }

        public override TEnv VisitNotEqual(Expr left, Expr right, Expr original, TEnv data)
        {
            return data.Assumer.AssumeNotEqual (left, right);
        }

        public override TEnv VisitVariable(Var var, Expr expr, TEnv data)
        {
            return data.Assumer.AssumeNotEqualToZero (expr);
        }

        public override TEnv VisitSubtraction(Expr left, Expr right, Expr original, TEnv data)
        {
            data = base.VisitSubtraction(left, right, original, data);
            return data.Assumer.AssumeNotEqualToZero (original);
        }
    }
}