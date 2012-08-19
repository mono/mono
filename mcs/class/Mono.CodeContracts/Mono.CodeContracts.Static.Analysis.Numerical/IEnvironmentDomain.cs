using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        interface IEnvironmentDomain<TDomain, TVar, TExpr> : IAbstractDomain<TDomain> {
                TDomain AssumeTrue (TExpr guard);
                TDomain AssumeFalse (TExpr guard);

                FlatDomain<bool> CheckIfHolds (TExpr expr);
                string ToString (TExpr expr);
        }
}