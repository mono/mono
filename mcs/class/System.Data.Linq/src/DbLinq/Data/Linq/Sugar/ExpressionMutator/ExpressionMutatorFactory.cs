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
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar.ExpressionMutator.Implementation;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.ExpressionMutator.Implementation;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.ExpressionMutator
#else
namespace DbLinq.Data.Linq.Sugar.ExpressionMutator
#endif
{
    internal static class ExpressionMutatorFactory
    {
        public static IMutableExpression GetMutator(Expression expression)
        {
            if (expression is BinaryExpression)
                return new BinaryExpressionMutator((BinaryExpression)expression);
            if (expression is ConditionalExpression)
                return new ConditionalExpressionMutator((ConditionalExpression)expression);
            if (expression is ConstantExpression)
                return new ConstantExpressionMutator((ConstantExpression)expression);
            if (expression is InvocationExpression)
                return new InvocationExpressionMutator((InvocationExpression)expression);
            if (expression is LambdaExpression)
                return new LambdaExpressionMutator((LambdaExpression)expression);
            if (expression is MemberExpression)
                return new MemberExpressionMutator((MemberExpression)expression);
            if (expression is MethodCallExpression)
                return new MethodCallExpressionMutator((MethodCallExpression)expression);
            if (expression is NewExpression)
                return new NewExpressionMutator((NewExpression)expression);
            if (expression is NewArrayExpression)
                return new NewArrayExpressionMutator((NewArrayExpression)expression);
            if (expression is MemberInitExpression)
                return new MemberInitExpressionMutator((MemberInitExpression)expression);
            if (expression is ListInitExpression)
                return new ListInitExpressionMutator((ListInitExpression)expression);
            if (expression is ParameterExpression)
                return new ParameterExpressionMutator((ParameterExpression)expression);
            if (expression is TypeBinaryExpression)
                return new TypeBinaryExpressionMutator((TypeBinaryExpression)expression);
            if (expression is UnaryExpression)
                return new UnaryExpressionMutator((UnaryExpression)expression);
            throw Error.BadArgument("S0064: Unknown Expression Type '{0}'", expression.GetType());
        }
    }
}