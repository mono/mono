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
using System.Linq;
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
    internal class BinaryExpressionMutator : IMutableExpression
    {
        protected BinaryExpression BinaryExpression { get; private set; }

        public Expression Mutate(IList<Expression> operands)
        {
            switch (BinaryExpression.NodeType)
            {
                case ExpressionType.Add:
                    if (BinaryExpression.Method != null)
                        return Expression.Add(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Add(operands[0], operands[1]);

                case ExpressionType.AddChecked:
                    if (BinaryExpression.Method != null)
                        return Expression.AddChecked(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.AddChecked(operands[0], operands[1]);

                case ExpressionType.Divide:
                    if (BinaryExpression.Method != null)
                        return Expression.Divide(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Divide(operands[0], operands[1]);

                case ExpressionType.Modulo:
                    if (BinaryExpression.Method != null)
                        return Expression.Modulo(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Modulo(operands[0], operands[1]);

                case ExpressionType.Multiply:
                    if (BinaryExpression.Method != null)
                        return Expression.Multiply(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Multiply(operands[0], operands[1]);

                case ExpressionType.MultiplyChecked:
                    if (BinaryExpression.Method != null)
                        return Expression.MultiplyChecked(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.MultiplyChecked(operands[0], operands[1]);

                case ExpressionType.Power:
                    if (BinaryExpression.Method != null)
                        return Expression.Power(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Power(operands[0], operands[1]);

                case ExpressionType.Subtract:
                    if (BinaryExpression.Method != null)
                        return Expression.Subtract(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Subtract(operands[0], operands[1]);

                case ExpressionType.SubtractChecked:
                    if (BinaryExpression.Method != null)
                        return Expression.SubtractChecked(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.SubtractChecked(operands[0], operands[1]);

                case ExpressionType.And:
                    if (BinaryExpression.Method != null)
                        return Expression.And(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.And(operands[0], operands[1]);

                case ExpressionType.Or:
                    if (BinaryExpression.Method != null)
                        return Expression.Or(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.Or(operands[0], operands[1]);

                case ExpressionType.ExclusiveOr:
                    if (BinaryExpression.Method != null)
                        return Expression.ExclusiveOr(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.ExclusiveOr(operands[0], operands[1]);


                case ExpressionType.LeftShift:
                    if (BinaryExpression.Method != null)
                        return Expression.LeftShift(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.LeftShift(operands[0], operands[1]);

                case ExpressionType.RightShift:
                    if (BinaryExpression.Method != null)
                        return Expression.RightShift(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.RightShift(operands[0], operands[1]);


                case ExpressionType.AndAlso:
                    if (BinaryExpression.Method != null)
                        return Expression.AndAlso(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.AndAlso(operands[0], operands[1]);

                case ExpressionType.OrElse:
                    if (BinaryExpression.Method != null)
                        return Expression.OrElse(operands[0], operands[1], BinaryExpression.Method);
                    return Expression.OrElse(operands[0], operands[1]);

                case ExpressionType.Equal:
                    if (BinaryExpression.Method != null)
                        return Expression.Equal(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.Equal(operands[0], operands[1]);

                case ExpressionType.NotEqual:
                    if (BinaryExpression.Method != null)
                        return Expression.NotEqual(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.NotEqual(operands[0], operands[1]);

                case ExpressionType.GreaterThanOrEqual:
                    if (BinaryExpression.Method != null)
                        return Expression.GreaterThanOrEqual(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.GreaterThanOrEqual(operands[0], operands[1]);

                case ExpressionType.GreaterThan:
                    if (BinaryExpression.Method != null)
                        return Expression.GreaterThan(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.GreaterThan(operands[0], operands[1]);

                case ExpressionType.LessThan:
                    if (BinaryExpression.Method != null)
                        return Expression.LessThan(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.LessThan(operands[0], operands[1]);

                case ExpressionType.LessThanOrEqual:
                    if (BinaryExpression.Method != null)
                        return Expression.LessThanOrEqual(operands[0], operands[1], BinaryExpression.IsLiftedToNull, BinaryExpression.Method);
                    return Expression.LessThanOrEqual(operands[0], operands[1]);


                case ExpressionType.Coalesce:
                    if (BinaryExpression.Conversion != null)
                        return Expression.Coalesce(operands[0], operands[1], BinaryExpression.Conversion);
                    return Expression.Coalesce(operands[0], operands[1]);

                case ExpressionType.ArrayIndex:
                    return Expression.ArrayIndex(operands[0], operands.Skip(1));
            }
            throw new Exception();
        }

        public IEnumerable<Expression> Operands
        {
            get 
            {
                yield return BinaryExpression.Left;
                yield return BinaryExpression.Right;
            }
        }

        public BinaryExpressionMutator(BinaryExpression expression)
        {
            BinaryExpression = expression;
        }
    }
}