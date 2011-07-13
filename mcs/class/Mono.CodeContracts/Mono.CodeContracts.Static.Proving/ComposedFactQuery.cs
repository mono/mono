// 
// ComposedFactQuery.cs
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
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving
{
	class ComposedFactQuery<Variable> : IFactQuery<BoxedExpression, Variable>
	{
		private List<IFactQuery<BoxedExpression, Variable>> elements = new List<IFactQuery<BoxedExpression, Variable>> ();
		private Predicate<APC> isUnreachable;

		public ComposedFactQuery(Predicate<APC> isUnreachable )
		{
			this.isUnreachable = isUnreachable;
		}

		public void Add(IFactQuery<BoxedExpression, Variable> item )
		{
			if (item == null)
				return;
			this.elements.Add (item);
		}

		#region Implementation of IFactBase<Variable>
		public ProofOutcome IsNull(APC pc, Variable variable)
		{
			return elements.Select(fact => fact.IsNull(pc, variable)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsNonNull(APC pc, Variable variable)
		{
			return elements.Select(fact => fact.IsNonNull(pc, variable)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public bool IsUnreachable(APC pc)
		{
			if (this.isUnreachable != null && this.isUnreachable(pc))
				return true;
			return elements.Any (factQuery => factQuery.IsUnreachable (pc));
		}
		#endregion

		#region Implementation of IFactQuery<BoxedExpression,Variable>
		public ProofOutcome IsNull(APC pc, BoxedExpression expr)
		{
			return elements.Select (fact => fact.IsNull (pc, expr)).FirstOrDefault (factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsNonNull(APC pc, BoxedExpression expr)
		{
			return elements.Select(fact => fact.IsNonNull(pc, expr)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsTrue(APC pc, BoxedExpression expr)
		{
			ProofOutcome res = ProofOutcome.Top;
			foreach (var factQuery in elements) {
				var outcome = factQuery.IsTrue (pc, expr);
				switch (outcome) {
					case ProofOutcome.True:
					case ProofOutcome.Bottom:
						return outcome;
					case ProofOutcome.False:
						res = outcome;
						continue;
					default:
						continue;
				}
			}
			if (res != ProofOutcome.Top)
				return res;

			BinaryOperator op;
			BoxedExpression left;
			BoxedExpression right;
			if (expr.IsBinaryExpression (out op, out left, out right)) {
				if ((op == BinaryOperator.Ceq || op == BinaryOperator.Cobjeq) && this.IsRelational (left) && this.IsNull (pc, right) == ProofOutcome.True) {
					var outcome = this.IsTrue (pc, left);
					switch (outcome) {
						case ProofOutcome.False:
							return ProofOutcome.True;
						case ProofOutcome.True:
							return ProofOutcome.False;
						default:
							return outcome;
					}
				}
				int leftInt;
				int rightInt;
				if (op == BinaryOperator.Ceq && left.IsConstantIntOrNull (out leftInt) && right.IsConstantIntOrNull (out rightInt))
					return leftInt == rightInt ? ProofOutcome.True : ProofOutcome.False;
			}

			if (expr.IsUnary && expr.UnaryOperator == UnaryOperator.Not) {
				var outcome = this.IsTrue (pc, expr.UnaryArgument);
				switch (outcome) {
					case ProofOutcome.False:
						return ProofOutcome.True;
					case ProofOutcome.True:
						return ProofOutcome.False;
					default:
						return outcome;
				}
			} 

			return ProofOutcome.Top;
		}

		private bool IsRelational(BoxedExpression e)
		{
			BinaryOperator op;
			BoxedExpression left;
			BoxedExpression right;
			if (e.IsBinaryExpression (out op, out left, out right))
				switch (op) {
					case BinaryOperator.Ceq:
					case BinaryOperator.Cobjeq:
					case BinaryOperator.Cne_Un:
					case BinaryOperator.Cge:
					case BinaryOperator.Cge_Un:
					case BinaryOperator.Cgt:
					case BinaryOperator.Cgt_Un:
					case BinaryOperator.Cle:
					case BinaryOperator.Cle_Un:
					case BinaryOperator.Clt:
					case BinaryOperator.Clt_Un:
						return true;
				}
			return false;
		}

		public ProofOutcome IsTrueImply(APC pc, LispList<BoxedExpression> positiveAssumptions, LispList<BoxedExpression> negativeAssumptions, BoxedExpression goal)
		{
			return elements.Select(fact => fact.IsTrueImply(pc, positiveAssumptions, negativeAssumptions, goal)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsGreaterEqualToZero(APC pc, BoxedExpression expr)
		{
			return elements.Select(fact => fact.IsGreaterEqualToZero(pc, expr)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsLessThan(APC pc, BoxedExpression expr, BoxedExpression right)
		{
			return elements.Select(fact => fact.IsLessThan(pc, expr, right)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}

		public ProofOutcome IsNonZero(APC pc, BoxedExpression expr)
		{
			return elements.Select(fact => fact.IsNonZero(pc, expr)).FirstOrDefault(factResult => factResult != ProofOutcome.Top);
		}
		#endregion
	}
}