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
using System.Reflection;

using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;

namespace DbLinq.Data.Linq.Sugar.Implementation
{
    /// <summary>
    /// Analyzes language patterns and replace them with standard expressions
    /// </summary>
    internal class ExpressionLanguageParser : IExpressionLanguageParser
    {
        public virtual Expression Parse(Expression expression, BuilderContext builderContext)
        {
            return expression.Recurse(e => Analyze(e, builderContext));
        }

        protected delegate Expression Analyzer(Expression expression);

        protected IEnumerable<Analyzer> Analyzers;
        private readonly object analyzersLock = new object();

        protected virtual IEnumerable<Analyzer> GetAnalyzers()
        {
            lock (analyzersLock)
            {
                if (Analyzers == null)
                {
                    // man, this is the kind of line I'm proud of :)
                    Analyzers = from method in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                let m = (Analyzer)Delegate.CreateDelegate(typeof(Analyzer), this, method, false)
                                where m != null
                                select m;
                    Analyzers = Analyzers.ToList(); // result is faster from here
                }
                return Analyzers;
            }
        }

        protected virtual Expression Analyze(Expression expression, BuilderContext builderContext)
        {
            foreach (var analyze in GetAnalyzers())
            {
                var e = analyze(expression);
                if (e != null)
                    return e;
            }
            return expression;
        }

        /// <summary>
        /// Tests for Convert.ToBoolean()
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeConvertToBoolean(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.DeclaringType.Name == "Convert")
                {
                    if (methodCallExpression.Method.Name == "ToBoolean")
                        return Expression.Convert(methodCallExpression.Arguments[0], methodCallExpression.Type);
                }
            }
            return null;
        }

        /// <summary>
        /// Used to determine if the Expression is a VB CompareString
        /// Returns an equivalent Expression if true
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected virtual Expression AnalyzeCompareString(Expression expression)
        {
            bool equals;
            var testedExpression = GetComparedToZero(expression, out equals);
            if (testedExpression != null)
            {
                var methodExpression = testedExpression as MethodCallExpression;
                if (methodExpression != null
                    && methodExpression.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators"
                    && methodExpression.Method.Name == "CompareString")
                {
                    return Expression.Equal(methodExpression.Arguments[0], methodExpression.Arguments[1]);
                }
            }
            return null;
        }

        protected virtual Expression AnalyzeLikeString(Expression expression)
        {
            var methodExpression = expression as MethodCallExpression;
            if (methodExpression != null
                && methodExpression.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.LikeOperator"
                && methodExpression.Method.Name == "LikeString")
            {
                var lambda = (Expression<Func<string, string, bool>>)((a, b) => a.StartsWith(b));
                return Expression.Invoke(lambda, methodExpression.Arguments[0], methodExpression.Arguments[1]);
            }
            return null;
        }

        /// <summary>
        /// Determines if an expression is a comparison to 0
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="equals">True if ==, False if !=</param>
        /// <returns>The compared Expression or null</returns>
        protected static Expression GetComparedToZero(Expression expression, out bool equals)
        {
            equals = expression.NodeType == ExpressionType.Equal;
            if (equals || expression.NodeType == ExpressionType.NotEqual)
            {
                var binaryExpression = (BinaryExpression)expression;
                if (IsZero(binaryExpression.Right))
                    return binaryExpression.Left;
                if (IsZero(binaryExpression.Left))
                    return binaryExpression.Right;
            }
            return null;
        }

        /// <summary>
        /// Determines if an expression is constant value 0
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected static bool IsZero(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                var unaryExpression = (ConstantExpression)expression;
                return (unaryExpression.Value as int? ?? 0) == 0; // you too, have fun with C# operators
            }
            return false;
        }
    }
}
