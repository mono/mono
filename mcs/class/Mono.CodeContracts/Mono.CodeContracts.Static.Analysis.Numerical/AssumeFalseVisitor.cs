// 
// AssumeFalseVisitor.cs
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

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class AssumeFalseVisitor<TDomain, TVar, TExpr> :
                GenericExpressionVisitor<TDomain, TDomain, TVar, TExpr>
                where TDomain : IEnvironmentDomain<TDomain, TVar, TExpr> {
                protected AssumeFalseVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public AssumeTrueVisitor<TDomain, TVar, TExpr> TrueVisitor { get; set; }

                protected override TDomain Default (TDomain data)
                {
                        return data;
                }

                public override TDomain VisitConstant (TExpr left, TDomain data)
                {
                        bool boolValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Bool, out boolValue))
                                return boolValue ? data : data.Bottom;

                        int intValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Int32, out intValue))
                                return intValue != 0 ? data : data.Bottom;

                        return data;
                }

                public override TDomain VisitNot (TExpr expr, TDomain data)
                {
                        return TrueVisitor.Visit (expr, data);
                }

                public override TDomain VisitEqual (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        int value;
                        if (Decoder.TryValueOf (right, ExpressionType.Int32, out value) && value == 0)
                                // test (left :neq: 0) ==> test (left)
                                return TrueVisitor.Visit (left, data);

                        return TrueVisitor.VisitNotEqual (left, right, original, data);
                }

                public override TDomain VisitLessThan (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        // !(left < right) ==> right <= left
                        return TrueVisitor.VisitLessEqualThan (right, left, original, data);
                }

                public override TDomain VisitLessEqualThan (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        // !(left <= right) ==> right < left
                        return TrueVisitor.VisitLessThan (right, left, original, data);
                }
                }
}