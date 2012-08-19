using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class ExpressionViaStringComparer<TVar> : IComparer<TVar> {
                public int Compare (TVar x, TVar y)
                {
                        return System.String.CompareOrdinal(x.ToString (), y.ToString ());
                }
        }
}