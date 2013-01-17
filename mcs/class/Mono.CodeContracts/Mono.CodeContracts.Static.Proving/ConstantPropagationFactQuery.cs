// 
// ConstantPropagationFactQuery.cs
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

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Proving
{
	class ConstantPropagationFactQuery<Variable> : IFactQuery<BoxedExpression, Variable> {
		#region Implementation of IFactBase<Variable>
        public bool IsUnreachable(APC pc)
        {
            return false;
        }

		public FlatDomain<bool> IsNull(APC pc, Variable variable)
		{
			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsNonNull(APC pc, Variable variable)
		{
			return ProofOutcome.Top;
		}
		#endregion

		#region Implementation of IFactQuery<BoxedExpression,Variable>
        public FlatDomain<bool> IsNull(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return (num == 0).ToTrueOrTop ();

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsNonNull(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return (num != 0).ToTrueOrTop ();

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsTrue(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return (num != 0).ToTrueOrTop ();

			return ConstantFact (expr);
		}

        public FlatDomain<bool> IsTrueImply(APC pc, Sequence<BoxedExpression> positiveAssumptions, Sequence<BoxedExpression> negativeAssumptions, BoxedExpression goal)
		{
			UnaryOperator op;
			BoxedExpression arg;
			while (goal.IsUnaryExpression (out op, out arg) && op.IsConversionOperator ())
				goal = arg;

			if (positiveAssumptions.Any(assumption => assumption.Equals (goal)))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsGreaterEqualToZero(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull(out num))
				return (num >= 0).ToTrueOrTop ();

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsLessThan(APC pc, BoxedExpression left, BoxedExpression right)
		{
			int l;
			int r;
			if (left.IsConstantIntOrNull(out l) && right.IsConstantIntOrNull(out r))
				return (l < r).ToTrueOrTop ();

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsNonZero(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull(out num))
				return (num != 0).ToTrueOrTop ();

			return ProofOutcome.Top;
		}

        private static FlatDomain<bool> ConstantFact(BoxedExpression expr)
		{
			BinaryOperator op;
			BoxedExpression left;
			BoxedExpression right;
			if (expr.IsBinaryExpression (out op, out left, out right)) {
				int l;
				var leftIsInt = left.IsConstantIntOrNull (out l);
				
				int r;
				var rightIsInt = right.IsConstantIntOrNull(out r);

				if (leftIsInt || rightIsInt) {
					if (leftIsInt && rightIsInt) {
						switch (op) {
							case BinaryOperator.Add:        return ((l + r) != 0).ToTrueOrTop ();
							case BinaryOperator.And:        return ((l & r) != 0).ToTrueOrTop ();
							case BinaryOperator.Ceq:        return (l == r).ToTrueOrTop ();
							case BinaryOperator.Cobjeq:     return ProofOutcome.Top;
							case BinaryOperator.Cne_Un:     return (l != r).ToTrueOrTop ();
							case BinaryOperator.Cge:        return (l >= r).ToTrueOrTop ();
							case BinaryOperator.Cge_Un:     return ((uint)l >= (uint)r).ToTrueOrTop ();
							case BinaryOperator.Cgt:        return (l > r).ToTrueOrTop ();
							case BinaryOperator.Cgt_Un:     return ((uint)l > (uint)r).ToTrueOrTop ();
							case BinaryOperator.Cle:        return (l <= r).ToTrueOrTop ();
							case BinaryOperator.Cle_Un:     return ((uint)l <= (uint)r).ToTrueOrTop ();
							case BinaryOperator.Clt:        return (l < r).ToTrueOrTop ();
							case BinaryOperator.Clt_Un:     return ((uint)l < (uint)r).ToTrueOrTop ();
							case BinaryOperator.Div:        return (r != 0 && ((l / r) != 0)).ToTrueOrTop ();
							case BinaryOperator.LogicalAnd: return (l != 0 && r != 0).ToTrueOrTop ();
							case BinaryOperator.LogicalOr:  return (l != 0 || r != 0).ToTrueOrTop ();
							case BinaryOperator.Mul:        return (l * r != 0).ToTrueOrTop ();
							case BinaryOperator.Or:         return ((l | r) != 0).ToTrueOrTop ();
							case BinaryOperator.Rem:        return (r != 0 && (l % r != 0)).ToTrueOrTop ();
							case BinaryOperator.Shl:        return (l << r != 0).ToTrueOrTop ();
							case BinaryOperator.Shr:        return (l >> r != 0).ToTrueOrTop ();
							case BinaryOperator.Sub:        return (l - r != 0).ToTrueOrTop ();
							case BinaryOperator.Xor:        return ((l ^ r) != 0).ToTrueOrTop ();
						}
					}
					if (op == BinaryOperator.Ceq && (leftIsInt && l == 0 || rightIsInt && r == 0))
						return ConstantFact (left).Negate ();
				}
				else if (left.IsConstant && right.IsConstant) {
					var lConst = left.Constant;
					var rConst = right.Constant;
					switch (op) {
						case BinaryOperator.Cobjeq:
							return lConst == null ? (rConst == null).ToTrueOrTop () : lConst.Equals (rConst).ToTrueOrTop ();
						case BinaryOperator.Cne_Un:
							return lConst == null ? (rConst != null).ToTrueOrTop () : (!lConst.Equals (rConst)).ToTrueOrTop ();
					}
				}
			}

			return ProofOutcome.Top;
		}
		#endregion
	}
}