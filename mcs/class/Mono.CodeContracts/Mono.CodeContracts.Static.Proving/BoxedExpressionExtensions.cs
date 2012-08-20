// 
// BoxedExpressionExtensions.cs
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
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving
{
	static class BoxedExpressionExtensions
	{
		public static bool IsConstantIntOrNull(this BoxedExpression e, out int res)
		{
			res = 0;
			if (e.IsConstant) {
				IConvertible convertible = e.Constant as IConvertible;
				if (convertible != null) {
					if (e.Constant is string || e.Constant is float || e.Constant is double)
						return false;

					try {
						res = convertible.ToInt32 (null);
						return true;
					} catch {
					}
				}
				if (e.Constant == null)
					return true;
			}
			return false;
		}

                public static bool IsConstantBool(this BoxedExpression e, out bool result)
                {
                        if (e.IsConstant && e.Constant is bool)
                                return true.With ((bool) e.Constant, out result);

                        return false.Without (out result);
                }

                public static bool IsTrivialCondition(this BoxedExpression e, out bool result)
                {
                        int value;
                        if (e.IsConstantIntOrNull (out value))
                                return true.With (value != 0, out result);
                        if (e.IsConstantBool (out result))
                                return true;

                        BinaryOperator bop;
                        BoxedExpression left;
                        BoxedExpression right;
                        if (e.IsBinaryExpression (out bop, out left, out right)) {
                                int l, r;
                                var isLeftInt = left.IsConstantIntOrNull (out l);
                                var isRightInt = right.IsConstantIntOrNull (out r);
                                if (bop == BinaryOperator.Ceq) {
                                        if (isLeftInt && isRightInt)
                                                return true.With (l == r, out result);
                                        bool leftResult;
                                        if (isRightInt && r == 0 && left.IsTrivialCondition (out leftResult))
                                                return true.With (!leftResult, out result);

                                        bool rightResult;
                                        if (isLeftInt && l == 0 && right.IsTrivialCondition (out rightResult))
                                                return true.With (!rightResult, out result);

                                        return false.Without (out result);
                                }

                                switch (bop) {
                                        case BinaryOperator.Add:
                                        case BinaryOperator.Add_Ovf:
                                        case BinaryOperator.Add_Ovf_Un:
                                                return true.With (l + r != 0, out result);
                                        case BinaryOperator.And:
                                                return true.With ((l & r) != 0, out result);
                                        case BinaryOperator.Cne_Un:
                                                return true.With (l != r, out result);
                                        case BinaryOperator.Cge:
                                                return true.With (l >= r, out result);
                                        case BinaryOperator.Cge_Un:
                                                return true.With ((uint) l >= (uint) r, out result);
                                        case BinaryOperator.Cgt:
                                                return true.With (l > r, out result);
                                        case BinaryOperator.Cgt_Un:
                                                return true.With ((uint) l > (uint) r, out result);
                                        case BinaryOperator.Cle:
                                                return true.With (l <= r, out result);
                                        case BinaryOperator.Cle_Un:
                                                return true.With ((uint) l <= (uint) r, out result);
                                        case BinaryOperator.Clt:
                                                return true.With (l < r, out result);
                                        case BinaryOperator.Clt_Un:
                                                return true.With ((uint) l < (uint) r, out result);
                                        case BinaryOperator.Div:
                                                if (r == 0)
                                                        return false.Without (out result);
                                                return true.With (l / r != 0, out result);
                                        case BinaryOperator.Div_Un:
                                                if (r == 0)
                                                        return false.Without (out result);
                                                return true.With ((uint) l / (uint) r != 0, out result);
                                        case BinaryOperator.LogicalAnd:
                                                return true.With (l != 0 && r != 0, out result);
                                        case BinaryOperator.LogicalOr:
                                                return true.With (l != 0 || r != 0, out result);
                                        case BinaryOperator.Mul:
                                        case BinaryOperator.Mul_Ovf:
                                        case BinaryOperator.Mul_Ovf_Un:
                                                return true.With (l * r != 0, out result);
                                        case BinaryOperator.Or:
                                                return true.With ((l | r) != 0, out result);
                                        case BinaryOperator.Rem:
                                                if (r == 0)
                                                        return false.Without (out result);
                                                return true.With (l % r != 0, out result);
                                        case BinaryOperator.Rem_Un:
                                                if (r == 0)
                                                        return false.Without (out result);
                                                return true.With (((uint) l) % ((uint) r) != 0, out result);
                                        case BinaryOperator.Shl:
                                                return true.With (l << r != 0, out result);
                                        case BinaryOperator.Shr:
                                                return true.With (l >> r != 0, out result);
                                        case BinaryOperator.Shr_Un:
                                                return true.With (((uint) l) >> r != 0, out result);
                                        case BinaryOperator.Sub:
                                        case BinaryOperator.Sub_Ovf:
                                        case BinaryOperator.Sub_Ovf_Un:
                                                return true.With (l - r != 0, out result);
                                        case BinaryOperator.Xor:
                                                return true.With ((l ^ r) != 0, out result);
                                        default:
                                                return false.Without (out result);
                                }
                        }

                        return false.Without (out result);
                }
	}
}