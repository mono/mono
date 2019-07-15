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
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.ExpressionMutator
{
    /// <summary>
    /// Extensions to Expression, to enumerate and dynamically change operands in a uniformized way
    /// </summary>
    internal static class ExpressionMutatorExtensions
    {
        /// <summary>
        /// Enumerates all subexpressions related to this one
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IEnumerable<Expression> GetOperands(this Expression expression)
        {
            if (expression is MutableExpression)
                return new List<Expression>(((MutableExpression)expression).Operands);
            return ExpressionMutatorFactory.GetMutator(expression).Operands;
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <param name="checkForChanges"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, IList<Expression> operands, bool checkForChanges)
            where T : Expression
        {
            bool haveOperandsChanged = checkForChanges && HaveOperandsChanged(expression, operands);
            if (!haveOperandsChanged)
                return expression;
            var mutableExpression = expression as IMutableExpression;
            if (mutableExpression != null)
                return (T)mutableExpression.Mutate(operands);
            return (T)ExpressionMutatorFactory.GetMutator(expression).Mutate(operands);
        }

        /// <summary>
        /// Determines if operands have changed for a given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns></returns>
        private static bool HaveOperandsChanged<T>(T expression, IList<Expression> operands)
            where T : Expression
        {
            var oldOperands = GetOperands(expression).ToList();
            if (operands.Count != oldOperands.Count)
                return true;

            for (int operandIndex = 0; operandIndex < operands.Count; operandIndex++)
            {
                if (operands[operandIndex] != oldOperands[operandIndex])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, IList<Expression> operands)
            where T : Expression
        {
            return ChangeOperands(expression, operands, true);
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, params Expression[] operands)
            where T : Expression
        {
            return ChangeOperands(expression, operands, true);
        }

        /// <summary>
        /// Returns the expression result
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static object Evaluate(this Expression expression)
        {
            var executableExpression = expression as IExecutableExpression;
            if (executableExpression != null)
                return executableExpression.Execute();
            try
            {
                // here, we may have non-evaluable expressions, so we "try"/"catch"
                // (maybe should we find something better)
                var lambda = Expression.Lambda(expression);
                var compiled = lambda.Compile();
                var value = compiled.DynamicInvoke();
                return value;
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Down-top pattern analysis.
        /// </summary>
        /// <param name="expression">The original expression</param>
        /// <param name="analyzer"></param>
        /// <returns>A new QueryExpression or the original one</returns>
        public static Expression Recurse(this Expression expression, Func<Expression, Expression> analyzer)
        {
            var newOperands = new List<Expression>();
            // first, work on children (down)
            foreach (var operand in GetOperands(expression))
            {
                if (operand != null)
                    newOperands.Add(Recurse(operand, analyzer));
                else
                    newOperands.Add(null);
            }
            // then on expression itself (top)
            return analyzer(expression.ChangeOperands(newOperands));
        }
    }
}