using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal interface IAbstractDomainForEnvironments<TThis, Var, Expr> : IAbstractDomain<TThis>
        where TThis : IAbstractDomainForEnvironments<TThis, Var, Expr>
    {
        string ToString (Expr expr);
    }
}