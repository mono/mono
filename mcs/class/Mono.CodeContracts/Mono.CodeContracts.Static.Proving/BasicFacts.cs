// 
// BasicFacts.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Proving {
	class BasicFacts<Expression, Variable> : IFactQuery<BoxedExpression, Variable> {
		protected IExpressionContextProvider<Expression, Variable> ContextProvider;
		protected IFactBase<Variable> FactBase;
		protected Predicate<APC> isUnreachable;

		public BasicFacts (IExpressionContextProvider<Expression, Variable> contextProvider, IFactBase<Variable> factBase, Predicate<APC> isUnreachable)
		{
			this.ContextProvider = contextProvider;
			this.FactBase = factBase;
			this.isUnreachable = isUnreachable;
		}

		#region Implementation of IFactBase<Variable>
        public FlatDomain<bool> IsNull(APC pc, Variable variable)
		{
			return this.FactBase.IsNull (pc, variable);
		}

        public FlatDomain<bool> IsNonNull(APC pc, Variable variable)
		{
			return this.FactBase.IsNonNull (pc, variable);
		}

		public bool IsUnreachable (APC pc)
		{
			if (this.isUnreachable != null && this.isUnreachable (pc))
				return true;

			return this.FactBase.IsUnreachable (pc);
		}

		protected static bool TryVariable (BoxedExpression e, out Variable v)
		{
			object underlyingVariable = e.UnderlyingVariable;
			if (underlyingVariable is Variable) {
				v = (Variable) underlyingVariable;
				return true;
			}

			v = default(Variable);
			return false;
		}
		#endregion

		#region Implementation of IFactQuery<BoxedExpression,Variable>
        public virtual FlatDomain<bool> IsNull(APC pc, BoxedExpression expr)
		{
			Variable v;
			if (TryVariable (expr, out v)) {
                FlatDomain<bool> outcome = this.FactBase.IsNull(pc, v);
				if (!outcome.IsTop)
					return outcome;
			}
			return ProofOutcome.Top;
		}

        public virtual FlatDomain<bool> IsNonNull(APC pc, BoxedExpression expr)
		{
			Variable v;
			if (TryVariable (expr, out v)) {
                FlatDomain<bool> outcome = this.FactBase.IsNonNull(pc, v);
				if (!outcome.IsTop)
					return outcome;
			}
			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsTrue(APC pc, BoxedExpression expr)
		{
			return IsNonZero (pc, expr);
		}

        public FlatDomain<bool> IsTrueImply(APC pc, Sequence<BoxedExpression> positiveAssumptions, Sequence<BoxedExpression> negativeAssumptions, BoxedExpression goal)
		{
            FlatDomain<bool> outcome = IsTrue(pc, goal);
            if (outcome.IsTrue() || outcome.IsBottom)
                return outcome;

            return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsGreaterEqualToZero(APC pc, BoxedExpression expr)
		{
			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsLessThan(APC pc, BoxedExpression expr, BoxedExpression right)
		{
			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsNonZero(APC pc, BoxedExpression expr)
		{
			return IsNonNull (pc, expr);
		}
		#endregion
	}
}
