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

using System.Linq;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving
{
	class ConstantPropagationFactQuery<Variable> : IFactQuery<BoxedExpression, Variable>
	{
		#region Implementation of IFactBase<Variable>
		public ProofOutcome IsNull(APC pc, Variable variable)
		{
			return ProofOutcome.Top;
		}

		public ProofOutcome IsNonNull(APC pc, Variable variable)
		{
			return ProofOutcome.Top;
		}

		public bool IsUnreachable(APC pc)
		{
			return false;
		}
		#endregion

		#region Implementation of IFactQuery<BoxedExpression,Variable>
		public ProofOutcome IsNull(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return IsTrueOrTop (num == 0);

			return ProofOutcome.Top;
		}

		public ProofOutcome IsNonNull(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return IsTrueOrTop (num != 0);

			return ProofOutcome.Top;
		}

		public ProofOutcome IsTrue(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull (out num))
				return IsTrueOrTop (num != 0);

			return ConstantFact (expr);
		}


		public ProofOutcome IsTrueImply(APC pc, LispList<BoxedExpression> positiveAssumptions, LispList<BoxedExpression> negativeAssumptions, BoxedExpression goal)
		{
			UnaryOperator op;
			BoxedExpression arg;
			while (goal.IsUnaryExpression (out op, out arg) && op.IsConversionOperator ())
				goal = arg;

			if (positiveAssumptions.AsEnumerable ().Any (positiveAssumption => positiveAssumptions.Equals (goal)))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

		public ProofOutcome IsGreaterEqualToZero(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull(out num))
				return IsTrueOrTop(num >= 0);

			return ProofOutcome.Top;
		}

		public ProofOutcome IsLessThan(APC pc, BoxedExpression left, BoxedExpression right)
		{
			int l;
			int r;
			if (left.IsConstantIntOrNull(out l) && right.IsConstantIntOrNull(out r))
				return IsTrueOrTop (l < r);

			return ProofOutcome.Top;
		}

		public ProofOutcome IsNonZero(APC pc, BoxedExpression expr)
		{
			int num;
			if (expr.IsConstantIntOrNull(out num))
				return IsTrueOrTop (num != 0);

			return ProofOutcome.Top;
		}

		private static ProofOutcome IsTrueOrTop(bool condition)
		{
			return condition ? ProofOutcome.True : ProofOutcome.Top;
		}

		private ProofOutcome ConstantFact(BoxedExpression expr)
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
							case BinaryOperator.Add:
								return IsTrueOrTop ((l + r) != 0);
							case BinaryOperator.And:
								return IsTrueOrTop((l & r) != 0);
							case BinaryOperator.Ceq:
								return IsTrueOrTop(l == r);
							case BinaryOperator.Cobjeq:
								return ProofOutcome.Top;
							case BinaryOperator.Cne_Un:
								return IsTrueOrTop(l != r);
							case BinaryOperator.Cge:
								return IsTrueOrTop(l >= r);
							case BinaryOperator.Cge_Un:
								return IsTrueOrTop((uint)l >= (uint)r);
							case BinaryOperator.Cgt:
								return IsTrueOrTop(l > r);
							case BinaryOperator.Cgt_Un:
								return IsTrueOrTop((uint)l > (uint)r);
							case BinaryOperator.Cle:
								return IsTrueOrTop(l <= r);
							case BinaryOperator.Cle_Un:
								return IsTrueOrTop((uint)l <= (uint)r);
							case BinaryOperator.Clt:
								return IsTrueOrTop(l < r);
							case BinaryOperator.Clt_Un:
								return IsTrueOrTop((uint)l < (uint)r);
							case BinaryOperator.Div:
								return IsTrueOrTop(r != 0 && ((l / r) != 0));
							case BinaryOperator.LogicalAnd:
								return IsTrueOrTop(l != 0 && r != 0);
							case BinaryOperator.LogicalOr:
								return IsTrueOrTop(l != 0 || r != 0);
							case BinaryOperator.Mul:
								return IsTrueOrTop(l * r != 0);
							case BinaryOperator.Or:
								return IsTrueOrTop((l | r) != 0);
							case BinaryOperator.Rem:
								return IsTrueOrTop(r != 0 && (l % r != 0));
							case BinaryOperator.Shl:
								return IsTrueOrTop(l << r != 0);
							case BinaryOperator.Shr:
								return IsTrueOrTop(l >> r != 0);
							case BinaryOperator.Sub:
								return IsTrueOrTop(l - r != 0);
							case BinaryOperator.Xor:
								return IsTrueOrTop((l ^ r) != 0);
						}
					}
					if (op == BinaryOperator.Ceq && (leftIsInt && l == 0 || rightIsInt && r == 0))
						return this.ConstantFact (left).Negate ();
				}
				else if (left.IsConstant && right.IsConstant) {
					var lConst = left.Constant;
					var rConst = right.Constant;
					switch (op) {
						case BinaryOperator.Cobjeq:
							return lConst == null ? IsTrueOrTop (rConst == null) : IsTrueOrTop (lConst.Equals (rConst));
						case BinaryOperator.Cne_Un:
							return lConst == null ? IsTrueOrTop (rConst != null) : IsTrueOrTop (!lConst.Equals (rConst));
					}
				}
			}

			return ProofOutcome.Top;
		}
		#endregion
	}
}