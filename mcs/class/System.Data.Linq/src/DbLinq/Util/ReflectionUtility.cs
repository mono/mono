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
using System.Reflection;

namespace DbLinq.Util
{
    /// <summary>
    /// What is extreme laziness? :)
    /// This class allows to get PropertyInfo or MethodInfo given a lambda (which allows to refactor members)
    /// </summary>
    public static class ReflectionUtility
    {
        public static MethodInfo GetMethodInfo(LambdaExpression lambdaExpression)
        {
            var methodCallExpression = lambdaExpression.Body as MethodCallExpression;
            if (methodCallExpression != null)
                return methodCallExpression.Method;
            throw new ArgumentException("Lambda expression is not correctly formated for MethodInfo extraction");
        }

        /// <summary>
        /// Returns a method info, providing a lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> lambdaExpression)
        {
            return GetMethodInfo((LambdaExpression)lambdaExpression);
        }

        /// <summary>
        /// Returns a method info, for example ReflectionUtility.GetMethodInfo(() => A.F()) would return "A.F()" MethodInfo
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this Expression<Action> lambdaExpression)
        {
            return GetMethodInfo((LambdaExpression)lambdaExpression);
        }

        /// <summary>
        /// Returns MemberInfo specified in the lambda, optional parameter expression constraint.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo(LambdaExpression expression)
        {
            var paramExpr = expression.Parameters.Count == 1 ? expression.Parameters[0] : null;
            return GetMemberInfo(paramExpr, expression.Body);
        }

        /// <summary>
        /// Returns MemberInfo specified in the lambda, optional parameter expression constraint.
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        private static MemberInfo GetMemberInfo(Expression optionalParamExpr, Expression expression)
        {
            switch (expression.NodeType)
            {
                // the ReflectionUtility Get** methods return the value as a object. If the value is a struct, we get a cast,
                // that we must unwrap
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return GetMemberInfo(optionalParamExpr, ((UnaryExpression)expression).Operand);
                case ExpressionType.MemberAccess:
                    var me = (MemberExpression)expression;
                    if (optionalParamExpr == null || me.Expression == optionalParamExpr)
                        return me.Member;
                    else
                        return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a PropertyInfo, given a lambda
        /// For example: ReflectionUtility.GetPropertyInfo&lt;A>(a => a.Prop) would return PropertyInfo "A.Prop"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> lambdaExpression)
        {
            return (PropertyInfo)GetMemberInfo((LambdaExpression) lambdaExpression);
        }

        /// <summary>
        /// Returns a PropertyInfo, given a lambda
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo(this Expression<Func<object>> lambdaExpression)
        {
            return (PropertyInfo)GetMemberInfo((LambdaExpression) lambdaExpression);
        }

        /// <summary>
        /// Returns a MemberInfo, given a lambda
        /// For example: ReflectionUtility.GetMemberInfo&lt;A>(a => a.Prop) would return MemberInfo "A.Prop"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object>> lambdaExpression)
        {
            return GetMemberInfo((LambdaExpression) lambdaExpression);
        }

        /// <summary>
        /// Returns a MemberInfo, given a lambda
        /// </summary>
        /// <param name="lambdaExpression"></param>
        /// <returns></returns>
        public static MemberInfo GetMemberInfo(this Expression<Func<object>> lambdaExpression)
        {
            return GetMemberInfo((LambdaExpression) lambdaExpression);
        }
    }
}
