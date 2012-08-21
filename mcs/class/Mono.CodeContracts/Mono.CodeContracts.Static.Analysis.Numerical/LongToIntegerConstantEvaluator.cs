// 
// LongToIntegerConstantEvaluator.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class LongToIntegerConstantEvaluator : ConstantEvaluatorVisitor<long, int> {
                protected override bool VisitAdd (long left, long right, out int result)
                {
                        return true.With ((int) left + (int) right, out result);
                }

                protected override bool VisitAnd (long left, long right, out int result)
                {
                        return true.With ((int) left & (int) right, out result);
                }

                protected override bool VisitOr (long left, long right, out int result)
                {
                        return true.With ((int) left | (int) right, out result);
                }

                protected override bool VisitXor (long left, long right, out int result)
                {
                        return true.With ((int) left ^ (int) right, out result);
                }

                protected override bool VisitEqual (long left, long right, out int result)
                {
                        return true.With ((int) left == (int) right ? 1 : 0, out result);
                }

                protected override bool VisitNotEqual (long left, long right, out int result)
                {
                        return true.With ((int) left != (int) right ? 1 : 0, out result);
                }

                protected override bool VisitLessThan (long left, long right, out int result)
                {
                        return true.With ((int) left < (int) right ? 1 : 0, out result);
                }

                protected override bool VisitLessEqualThan (long left, long right, out int result)
                {
                        return true.With ((int) left <= (int) right ? 1 : 0, out result);
                }

                protected override bool VisitGreaterThan (long left, long right, out int result)
                {
                        return true.With ((int) left > (int) right ? 1 : 0, out result);
                }

                protected override bool VisitGreaterEqualThan (long left, long right, out int result)
                {
                        return true.With ((int) left >= (int) right ? 1 : 0, out result);
                }

                protected override bool VisitSub (long left, long right, out int result)
                {
                        return true.With ((int) left - (int) right, out result);
                }

                protected override bool VisitMult (long left, long right, out int result)
                {
                        return true.With ((int) left * (int) right, out result);
                }

                protected override bool VisitDiv (long left, long right, out int result)
                {
                        if (right == 0)
                                return false.Without (out result);

                        return true.With ((int) left / (int) right, out result);
                }

                protected override bool VisitMod (long left, long right, out int result)
                {
                        if (right == 0)
                                return false.Without (out result);

                        return true.With ((int) left % (int) right, out result);
                }

                public static bool Evaluate (ExpressionOperator op, long left, long right, out int res)
                {
                        return new LongToIntegerConstantEvaluator ().VisitBinary (op, left, right, out res);
                }
        }
}