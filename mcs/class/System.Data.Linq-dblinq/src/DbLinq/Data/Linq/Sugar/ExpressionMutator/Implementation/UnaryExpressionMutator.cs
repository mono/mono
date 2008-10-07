#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.ExpressionMutator.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.ExpressionMutator.Implementation
#endif
{
    internal class UnaryExpressionMutator : IMutableExpression
    {
        protected UnaryExpression UnaryExpression { get; private set; }

        public Expression Mutate(IList<Expression> operands)
        {
            switch (UnaryExpression.NodeType)
            {
                case ExpressionType.ArrayLength:
                    return Expression.ArrayLength(operands[0]);

                case ExpressionType.Convert:
                    if (UnaryExpression.Method != null)
                        return Expression.Convert(operands[0], UnaryExpression.Type, UnaryExpression.Method);
                    return Expression.Convert(operands[0], UnaryExpression.Type);

                case ExpressionType.ConvertChecked:
                    if (UnaryExpression.Method != null)
                        return Expression.ConvertChecked(operands[0], UnaryExpression.Type, UnaryExpression.Method);
                    return Expression.ConvertChecked(operands[0], UnaryExpression.Type);

                case ExpressionType.Negate:
                    if (UnaryExpression.Method != null)
                        return Expression.Negate(operands[0], UnaryExpression.Method);
                    return Expression.Negate(operands[0]);

                case ExpressionType.NegateChecked:
                    if (UnaryExpression.Method != null)
                        return Expression.NegateChecked(operands[0], UnaryExpression.Method);
                    return Expression.NegateChecked(operands[0]);

                case ExpressionType.Not:
                    if (UnaryExpression.Method != null)
                        return Expression.Not(operands[0], UnaryExpression.Method);
                    return Expression.Not(operands[0]);

                case ExpressionType.Quote:
                    return Expression.Quote(operands[0]);

                case ExpressionType.TypeAs:
                    return Expression.TypeAs(operands[0], UnaryExpression.Type);

                case ExpressionType.UnaryPlus:
                    if (UnaryExpression.Method != null)
                        return Expression.UnaryPlus(operands[0], UnaryExpression.Method);
                    return Expression.UnaryPlus(operands[0]);

            }
            throw new Exception();
        }

        public IEnumerable<Expression> Operands
        {
            get
            {
                yield return UnaryExpression.Operand;
            }
        }

        public UnaryExpressionMutator(UnaryExpression expression)
        {
            UnaryExpression = expression;
        }
    }
}