// 
// SimpleLogicInference.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Proving {
	class SimpleLogicInference<Expression, Variable> : BasicFacts<Expression, Variable> {
		public SimpleLogicInference (IExpressionContextProvider<Expression, Variable> contextProvider, IFactBase<Variable> factBase, Predicate<APC> isUnreachable)
			: base (contextProvider, factBase, isUnreachable)
		{
		}

		public override ProofOutcome IsNull (APC pc, BoxedExpression expr)
		{
			Variable v;
			if (TryVariable (expr, out v)) {
				ProofOutcome proofOutcome = this.FactBase.IsNull (pc, v);
				if (proofOutcome != ProofOutcome.Top)
					return proofOutcome;
			}

			if (expr.IsConstant) {
				object constant = expr.Constant;
				if (constant == null)
					return ProofOutcome.True;
				if (constant is string)
					return ProofOutcome.False;
				var convertible = constant as IConvertible;
				if (convertible != null) {
					try {
						return (convertible.ToInt64 (null) == 0) ? ProofOutcome.True : ProofOutcome.False;
					} catch {
						return ProofOutcome.Top;
					}
				}
			}

			BinaryOperator op;
			BoxedExpression left;
			BoxedExpression right;
			if (expr.IsBinaryExpression (out op, out left, out right)) {
				if ((op == BinaryOperator.Ceq || op == BinaryOperator.Cobjeq) && IsNull (pc, right) == ProofOutcome.True)
					return IsNonNull (pc, left);
				if (op == BinaryOperator.Cne_Un && IsNull (pc, right) == ProofOutcome.True)
					return IsNull (pc, left);
			}
			return ProofOutcome.Top;
		}

		public override ProofOutcome IsNonNull (APC pc, BoxedExpression expr)
		{
			Variable v;
			if (TryVariable (expr, out v)) {
				ProofOutcome proofOutcome = this.FactBase.IsNonNull (pc, v);
				if (proofOutcome != ProofOutcome.Top)
					return proofOutcome;
			}

			if (expr.IsConstant) {
				object constant = expr.Constant;
				if (constant == null)
					return ProofOutcome.False;
				if (constant is string)
					return ProofOutcome.True;
				var convertible = constant as IConvertible;
				if (convertible != null) {
					try {
						return (convertible.ToInt64 (null) == 0) ? ProofOutcome.False : ProofOutcome.True;
					} catch {
						return ProofOutcome.Top;
					}
				}
			}

			BinaryOperator op;
			BoxedExpression left;
			BoxedExpression right;
			if (expr.IsBinaryExpression (out op, out left, out right)) {
				if ((op == BinaryOperator.Ceq || op == BinaryOperator.Cobjeq) && IsNull (pc, right) == ProofOutcome.True)
					return IsNull (pc, left);
				if (op == BinaryOperator.Cne_Un && IsNull (pc, right) == ProofOutcome.True)
					return IsNonNull (pc, left);
			}
			return ProofOutcome.Top;
		}
	}
}
