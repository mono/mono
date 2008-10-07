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
using System.Linq;
using System.Linq.Expressions;

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.ExpressionMutator;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    /// <summary>
    /// Optimizes expressions (such as constant chains)
    /// </summary>
    internal class ExpressionOptimizer : IExpressionOptimizer
    {
        public virtual Expression Optimize(Expression expression, BuilderContext builderContext)
        {
            return expression.Recurse(e => Analyze(e, builderContext));
        }

        protected Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            // small optimization
            if (expression is ConstantExpression)
                return expression;

            expression = AnalyzeNull(expression, builderContext);
            expression = AnalyzeNot(expression, builderContext);
            // constant optimization at last, because the previous optimizations may generate constant expressions
            expression = AnalyzeConstant(expression, builderContext);
            return expression;
        }

        protected virtual Expression AnalyzeConstant(Expression expression, BuilderContext builderContext)
        {
            // we try to find a non-constant operand, and if we do, we won't change this expression
            foreach (var operand in expression.GetOperands())
            {
                if (!(operand is ConstantExpression))
                    return expression;
            }
            // now, we just simply return a constant with new value
            try
            {
                var optimizedExpression = Expression.Constant(expression.Evaluate());
                // sometimes, optimizing an expression changes its type, and we just can't allow this.
                if (optimizedExpression.Type == expression.Type)
                    return optimizedExpression;
            }
                // if we fail to evaluate the expression, then just return it
            catch (ArgumentException) { }
            return expression;
        }

        protected virtual Expression AnalyzeNot(Expression expression, BuilderContext builderContext)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                var notExpression = expression as UnaryExpression;
                var subExpression = notExpression.Operand;
                var subOperands = subExpression.GetOperands().ToList();
                switch (subExpression.NodeType)
                {
                    case ExpressionType.Equal:
                        return Expression.NotEqual(subOperands[0], subOperands[1]);
                    case ExpressionType.GreaterThan:
                        return Expression.LessThanOrEqual(subOperands[0], subOperands[1]);
                    case ExpressionType.GreaterThanOrEqual:
                        return Expression.LessThan(subOperands[0], subOperands[1]);
                    case ExpressionType.LessThan:
                        return Expression.GreaterThanOrEqual(subOperands[0], subOperands[1]);
                    case ExpressionType.LessThanOrEqual:
                        return Expression.GreaterThan(subOperands[0], subOperands[1]);
                    case ExpressionType.Not:
                        return subOperands[0]; // not not x -> x :)
                    case ExpressionType.NotEqual:
                        return Expression.Equal(subOperands[0], subOperands[1]);
                    case (ExpressionType)SpecialExpressionType.IsNotNull: // is this dirty work?
                        return new SpecialExpression(SpecialExpressionType.IsNull, subOperands);
                    case (ExpressionType)SpecialExpressionType.IsNull:
                        return new SpecialExpression(SpecialExpressionType.IsNotNull, subOperands);
                }
            }
            return expression;
        }

        protected virtual Expression AnalyzeNull(Expression expression, BuilderContext builderContext)
        {
            // this first test only to speed up things a little
            if (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual)
            {
                var operands = expression.GetOperands().ToList();
                var nullComparison = GetNullComparison(expression.NodeType, operands[0], operands[1]);
                if (nullComparison == null)
                    nullComparison = GetNullComparison(expression.NodeType, operands[1], operands[0]);
                if (nullComparison != null)
                    return nullComparison;
                return expression;
            }
            return expression;
        }

        protected virtual Expression GetNullComparison(ExpressionType nodeType, Expression columnExpression, Expression nullExpression)
        {
            if (columnExpression is ColumnExpression || columnExpression is InputParameterExpression)
            {
                if (nullExpression is ConstantExpression && ((ConstantExpression)nullExpression).Value == null)
                {
                    switch (nodeType)
                    {
                        case ExpressionType.Equal:
                            return new SpecialExpression(SpecialExpressionType.IsNull, columnExpression);
                        case ExpressionType.NotEqual:
                            return new SpecialExpression(SpecialExpressionType.IsNotNull, columnExpression);
                    }
                }
            }
            return null;
        }
    }
}