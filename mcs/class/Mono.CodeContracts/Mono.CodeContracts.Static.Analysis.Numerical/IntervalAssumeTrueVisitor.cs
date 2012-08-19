using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class IntervalAssumeTrueVisitor<TVar, TExpr, TInterval, TNumeric> :
                AssumeTrueVisitor<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar> {
                public IntervalAssumeTrueVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                protected override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> DispatchCompare
                        (CompareVisitor cmp, TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = cmp (left, right, original, env);
                        return base.DispatchCompare (cmp, left, right, original, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitEqual
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return env.Assumer.AssumeEqual (left, right, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitLessThan
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return env.Assumer.AssumeLessThan (left, right, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitLessEqualThan
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return env.Assumer.AssumeLessEqualThan (left, right, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitAddition
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = base.VisitAddition (left, right, original, env);
                        return env.Assumer.AssumeNotEqualToZero (original, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitDivision
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = base.VisitDivision (left, right, original, env);
                        return env.Assumer.AssumeNotEqualToZero (original, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitMultiply
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = base.VisitMultiply (left, right, original, env);
                        return env.Assumer.AssumeNotEqualToZero (original, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitUnknown
                        (TExpr expr, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = base.VisitUnknown (expr, env);
                        return env.Assumer.AssumeNotEqualToZero (expr, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitNot
                        (TExpr expr, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return FalseVisitor.Visit (expr, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitNotEqual
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return env.Assumer.AssumeNotEqual (left, right, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitVariable
                        (TVar var, TExpr expr, IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        return env.Assumer.AssumeNotEqualToZero (expr, env);
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> VisitSubtraction
                        (TExpr left, TExpr right, TExpr original,
                         IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> env)
                {
                        env = base.VisitSubtraction (left, right, original, env);
                        return env.Assumer.AssumeNotEqualToZero (original, env);
                }
                }
}