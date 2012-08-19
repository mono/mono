using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        interface INumericalEnvironmentDomain<TVar, TExpr> :
                IEnvironmentDomain<INumericalEnvironmentDomain<TVar, TExpr>, TVar, TExpr> {
                INumericalEnvironmentDomain<TVar, TExpr> AssumeVariableIn (TVar var, Interval interval);
                INumericalEnvironmentDomain<TVar, TExpr> AssumeLessEqualThan (TExpr left, TExpr right);
                }

        interface IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric> : INumericalEnvironmentDomain<TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                IntervalContextBase<TInterval, TNumeric> Context { get; }

                TInterval Eval (TExpr expr);
                TInterval Eval (TVar expr);

                bool TryGetValue (TVar rightVar, out TInterval intv);
                }

        static class NumericalEnvironmentDomainExtensions {
                public static INumericalEnvironmentDomain<TVar, TExpr> AssumeInInterval<TVar, TExpr> (
                        this INumericalEnvironmentDomain<TVar, TExpr> domain, TExpr expr, Interval intv,
                        IExpressionEncoder<TVar, TExpr> encoder)
                {
                        if (!domain.IsNormal ())
                                return domain;

                        if (intv.IsBottom)
                                return domain.Bottom;

                        if (!intv.LowerBound.IsInfinity)
                                domain = domain.AssumeLessEqualThan (intv.LowerBound.ToExpression (encoder), expr);

                        if (!intv.UpperBound.IsInfinity)
                                domain = domain.AssumeLessEqualThan (expr, intv.LowerBound.ToExpression (encoder));

                        return domain;
                }
        }
}