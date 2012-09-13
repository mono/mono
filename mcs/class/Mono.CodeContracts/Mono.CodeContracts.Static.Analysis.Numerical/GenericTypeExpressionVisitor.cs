// 
// GenericTypeExpressionVisitor.cs
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
        abstract class GenericTypeExpressionVisitor<TVariable, TExpression, TIn, TOut> {
                protected readonly IExpressionDecoder<TVariable, TExpression> Decoder;

                protected GenericTypeExpressionVisitor (IExpressionDecoder<TVariable, TExpression> decoder)
                {
                        Decoder = decoder;
                }

                public virtual TOut Visit (TExpression e, TIn input)
                {
                        switch (Decoder.TypeOf (e)) {
                        case ExpressionType.Unknown:
                                return Default (e, input);
                        case ExpressionType.Int32:
                                return VisitInt32 (e, input);
                        case ExpressionType.Bool:
                                return VisitBool (e, input);
                        default:
                                throw new AbstractInterpretationException ("Unknown type for expressions " +
                                                                           Decoder.TypeOf (e));
                        }
                }

                protected virtual TOut VisitBool (TExpression expr, TIn input)
                {
                        return Default (expr, input);
                }

                protected virtual TOut VisitInt32 (TExpression expr, TIn input)
                {
                        return Default (expr, input);
                }

                protected abstract TOut Default (TExpression expr, TIn input);
        }
}