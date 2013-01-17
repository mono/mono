// 
// EvaluateArithmeticWithOverflow.cs
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
        static class EvaluateArithmeticWithOverflow {
                public static bool TryBinary (ExpressionOperator op, long left, long right, out int res)
                {
                        return LongToIntegerConstantEvaluator.Evaluate (op, left, right, out res);
                }

                public static bool TryBinary (ExpressionOperator op, long left, long right, out uint res)
                {
                        return false.Without (out res);
                }

                public static bool TryBinary<In> (ExpressionType targetType, ExpressionOperator op, In left, In right,
                                                  out object res)
                        where In : struct
                {
                        switch (targetType) {
                        case ExpressionType.Unknown:
                        case ExpressionType.Bool:
                                return false.Without (out res);

                        case ExpressionType.Int32:
                                var l = left.ConvertToLong ();
                                var r = right.ConvertToLong ();

                                int intValue;
                                if (l.HasValue && r.HasValue && TryBinary (op, l.Value, r.Value, out intValue))
                                        return true.With (intValue, out res);

                                break;
                        }

                        return false.Without (out res);
                }
        }
}