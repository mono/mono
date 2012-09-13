// 
// IntervalAssumeFalseVisitor.cs
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

using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class IntervalAssumeFalseVisitor<TVar, TExpr, TInterval, TNumeric> :
                AssumeFalseVisitor<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar> {
                public IntervalAssumeFalseVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Visit (TExpr expr,
                                                                                               IntervalEnvironmentBase
                                                                                                       <TVar, TExpr,
                                                                                                       TInterval,
                                                                                                       TNumeric> data)
                {
                        var res = base.Visit (expr, data);

                        if (!Decoder.IsBinaryExpression (expr))
                                return res;

                        var left = Decoder.LeftExpressionFor (expr);
                        var right = Decoder.RightExpressionFor (expr);

                        var intv = data.Eval (right);
                        if (intv.IsBottom)
                                return data.Bottom;
                        if (!intv.IsSinglePoint)
                                return res;

                        switch (Decoder.OperatorFor (expr)) {
                        case ExpressionOperator.LessThan: {
                                var leftVar = Decoder.UnderlyingVariable (left);
                                return res.Assumer.AssumeLessEqualThan (intv, leftVar, res);
                        }
                        case ExpressionOperator.LessEqualThan: {
                                var leftVar = Decoder.UnderlyingVariable (left);
                                return res.Assumer.AssumeLessThan (intv, leftVar, res);
                        }
                        }

                        return data;
                }

                protected override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> DispatchCompare (
                        CompareVisitor cmp, TExpr left, TExpr right, TExpr original,
                        IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> data)
                {
                        data = cmp (left, right, original, data);
                        return base.DispatchCompare (cmp, left, right, original, data);
                }
                }
}