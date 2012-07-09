using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class ExpressionViaStringComparer<Var> : IComparer<Var>
    {
        public int Compare(Var x, Var y)
        {
            return string.Compare(x.ToString(), y.ToString());
        }
    }
}