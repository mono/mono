// 
// ConstToIntervalEvaluator.cs
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
        class ConstToIntervalEvaluator<TContext, TVar, TExpr, TInterval, TNumeric> :
                GenericTypeExpressionVisitor<TVar, TExpr, TContext, TInterval>
                where TContext : IntervalContextBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                public ConstToIntervalEvaluator (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override TInterval Visit (TExpr e, TContext ctx)
                {
                        if (!ReferenceEquals (e, null) && Decoder.IsConstant (e))
                                return base.Visit (e, ctx);

                        return Default (e, ctx);
                }

                protected override TInterval VisitBool (TExpr expr, TContext ctx)
                {
                        bool value;
                        if (Decoder.TryValueOf (expr, ExpressionType.Bool, out value))
                                return ctx.For (value ? 1L : 0L);

                        return Default (expr, ctx);
                }

                protected override TInterval VisitInt32 (TExpr expr, TContext ctx)
                {
                        int value;
                        if (Decoder.TryValueOf (expr, ExpressionType.Int32, out value))
                                return ctx.For (value);

                        return Default (expr, ctx);
                }

                protected override TInterval Default (TExpr expr, TContext ctx)
                {
                        return ctx.TopValue;
                }
                }
}