using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal interface IAbstractDomainForEnvironments<TThis, Var, Expr> : IAbstractDomain<TThis>, IPureExpressionTest<TThis, Var, Expr>
        where TThis : IAbstractDomainForEnvironments<TThis, Var, Expr>
    {
        string ToString (Expr expr);
    }

    internal interface IPureExpressionTest<TThis, Var, Expr>
        where TThis : IPureExpressionTest<TThis, Var, Expr>
    {
        TThis AssumeTrue  (Expr guard);
        TThis AssumeFalse (Expr guard);

        FlatDomain<bool> CheckIfHolds (Expr expr);
    }
}