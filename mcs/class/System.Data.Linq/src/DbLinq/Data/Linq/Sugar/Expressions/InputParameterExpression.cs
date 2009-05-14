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
using System.Diagnostics;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
    [DebuggerDisplay("InputParameterExpression (current value={GetValue()})")]
#if !MONO_STRICT
    public
#endif
    class InputParameterExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.InputParameter;

        public Expression Expression { get; private set; }

        public string Alias { get; set; }

        private readonly Delegate getValueDelegate;
        /// <summary>
        /// Returns the outer parameter value
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            return getValueDelegate.DynamicInvoke();
        }

        public InputParameterExpression(Expression expression, string alias)
            : base(ExpressionType, expression.Type)
        {
            Expression = expression;
            var lambda = Expression.Lambda(expression);
            getValueDelegate = lambda.Compile();
            Alias = alias;
        }
    }
}