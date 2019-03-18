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

using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Implementation
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
            expression = AnalyzeBinaryBoolean(expression, builderContext);
            // constant optimization at last, because the previous optimizations may generate constant expressions
            expression = AnalyzeConstant(expression, builderContext);
            return expression;
        }

        private Expression AnalyzeBinaryBoolean(Expression expression, BuilderContext builderContext)
        {
            if (expression.Type != typeof(bool))
                return expression;
            var bin = expression as BinaryExpression;
            if (bin == null)
                return expression;
            bool canOptimizeLeft = bin.Left.NodeType == ExpressionType.Constant && bin.Left.Type == typeof(bool);
            bool canOptimizeRight = bin.Right.NodeType == ExpressionType.Constant && bin.Right.Type == typeof(bool);
            if (canOptimizeLeft && canOptimizeRight)
                return Expression.Constant(expression.Evaluate());
            if (canOptimizeLeft || canOptimizeRight)
                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        if (canOptimizeLeft)
                            if ((bool)bin.Left.Evaluate())
                                return bin.Right;   // (TRUE and X) == X 
                            else
                                return bin.Left;    // (FALSE and X) == FALSE 
                        if (canOptimizeRight)
                            if ((bool)bin.Right.Evaluate())
                                return bin.Left;    // (X and TRUE) == X 
                            else
                                return bin.Right;   // (X and FALSE) == FALSE
                        break;
                    case ExpressionType.OrElse:
                        if (canOptimizeLeft)
                            if ((bool)bin.Left.Evaluate())
                                return bin.Left;    // (TRUE or X) == TRUE 
                            else
                                return bin.Right;   // (FALSE or X) == X 
                        if (canOptimizeRight)
                            if ((bool)bin.Right.Evaluate())
                                return bin.Right;   // (X or TRUE) == TRUE 
                            else
                                return bin.Left;    // (X or FALSE) == X
                        break;
                    case ExpressionType.Equal:
                        // TODO: this optimization should work for Unary Expression Too
                        // this actually produce errors becouse of string based Sql generation
                        canOptimizeLeft = canOptimizeLeft && bin.Right is BinaryExpression;
                        if (canOptimizeLeft)
                            if ((bool)bin.Left.Evaluate())
                                return bin.Right;                   // (TRUE == X) == X 
                            else
                                return Expression.Not(bin.Right);   // (FALSE == X) == not X 
                        canOptimizeRight = canOptimizeRight && bin.Left is BinaryExpression;
                        // TODO: this optimization should work for Unary Expression Too
                        // this actually produce errors becouse of string based Sql generation
                        if (canOptimizeRight)
                            if ((bool)bin.Right.Evaluate())
                                return bin.Left;                    // (X == TRUE) == X 
                            else
                                return Expression.Not(bin.Left);    // (X == FALSE) == not X
                        break;
                    case ExpressionType.NotEqual:
                        canOptimizeLeft = canOptimizeLeft && bin.Right is BinaryExpression;
                        // TODO: this optimization should work for Unary Expression Too
                        // this actually produce errors becouse of string based Sql generation
                        if (canOptimizeLeft)
                            if ((bool)bin.Left.Evaluate())
                                return Expression.Not(bin.Right);   // (TRUE != X) == not X 
                            else
                                return bin.Right;                   // (FALSE != X) == X 
                        canOptimizeRight = canOptimizeRight && bin.Left is BinaryExpression;
                        // TODO: this optimization should work for Unary Expression Too
                        // this actually produce errors becouse of string based Sql generation
                        if (canOptimizeRight)
                            if ((bool)bin.Right.Evaluate())
                                return Expression.Not(bin.Left);    // (X != TRUE) == not X 
                            else
                                return bin.Left;                    // (X != FALSE) == X
                        break;
                }
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
            if (expression.NodeType == ExpressionType.Parameter)
                return expression;
            if (expression.NodeType == (ExpressionType)SpecialExpressionType.Like)
                return expression;
            // SETuse
            // If the value of the first SpecialExpressionType change this 999 should change too
            if ((short)expression.NodeType > 999)
                return expression;
            // now, we just simply return a constant with new value
            try
            {
                var optimizedExpression = Expression.Constant(expression.Evaluate());
                // sometimes, optimizing an expression changes its type, and we just can't allow this.
                if (optimizedExpression.Type == expression.Type)
                    return optimizedExpression;
            }
                // if we fail to evaluate the expression, then just return it
            catch (ArgumentException) 
            {
                return expression;
            }
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