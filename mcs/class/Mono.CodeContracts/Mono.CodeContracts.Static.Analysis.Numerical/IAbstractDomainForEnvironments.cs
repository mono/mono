using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal interface IAbstractDomainForEnvironments<TThis, Var, Expr> : IAbstractDomain<TThis>, IPureExpressionTest<TThis, Var, Expr>
        where TThis : IAbstractDomainForEnvironments<TThis, Var, Expr>
    {
        string ToString (Expr expr);
    }

    internal interface IPureExpressionTest<TThis, Var, Expr>
        where TThis : IAbstractDomainForEnvironments<TThis, Var, Expr>
    {
        TThis TestTrue  (Expr guard);
        TThis TestFalse (Expr guard);

        FlatDomain<bool> CheckIfHolds (Expr expr);
    }
}