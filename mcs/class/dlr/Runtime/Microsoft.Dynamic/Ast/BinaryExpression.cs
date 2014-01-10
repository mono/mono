/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
#if !MONO_INTERPRETER
        /// <summary>
        /// Null coalescing expression
        /// {result} ::= ((tmp = {_left}) == null) ? {right} : tmp
        /// '??' operator in C#.
        /// </summary>
        public static Expression Coalesce(Expression left, Expression right, out ParameterExpression temp) {
            return CoalesceInternal(left, right, null, false, out temp);
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(Expression left, Expression right, MethodInfo isTrue, out ParameterExpression temp) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(left, right, isTrue, false, out temp);
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(Expression left, Expression right, MethodInfo isTrue, out ParameterExpression temp) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            return CoalesceInternal(left, right, isTrue, true, out temp);
        }

        private static Expression CoalesceInternal(Expression left, Expression right, MethodInfo isTrue, bool isReverse, out ParameterExpression temp) {
            ContractUtils.RequiresNotNull(left, "left");
            ContractUtils.RequiresNotNull(right, "right");

            // A bit too strict, but on a safe side.
            ContractUtils.Requires(left.Type == right.Type, "Expression types must match");

            temp = Expression.Variable(left.Type, "tmp_left");

            Expression condition;
            if (isTrue != null) {
                ContractUtils.Requires(isTrue.ReturnType == typeof(bool), "isTrue", "Predicate must return bool.");
                ParameterInfo[] parameters = isTrue.GetParameters();
                ContractUtils.Requires(parameters.Length == 1, "isTrue", "Predicate must take one parameter.");
                ContractUtils.Requires(isTrue.IsStatic && isTrue.IsPublic, "isTrue", "Predicate must be public and static.");

                Type pt = parameters[0].ParameterType;
                ContractUtils.Requires(TypeUtils.CanAssign(pt, left.Type), "left", "Incorrect left expression type");
                condition = Expression.Call(isTrue, Expression.Assign(temp, left));
            } else {
                ContractUtils.Requires(TypeUtils.CanCompareToNull(left.Type), "left", "Incorrect left expression type");
                condition = Expression.Equal(Expression.Assign(temp, left), AstUtils.Constant(null, left.Type));
            }

            Expression t, f;
            if (isReverse) {
                t = temp;
                f = right;
            } else {
                t = right;
                f = temp;
            }

            return Expression.Condition(condition, t, f);
        }

        public static Expression Coalesce(LambdaBuilder builder, Expression left, Expression right) {
            ParameterExpression temp;
            Expression result = Coalesce(left, right, out temp);
            builder.AddHiddenVariable(temp);
            return result;
        }

        /// <summary>
        /// True coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? {right} : tmp
        /// Generalized AND semantics.
        /// </summary>
        public static Expression CoalesceTrue(LambdaBuilder builder, Expression left, Expression right, MethodInfo isTrue) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            ParameterExpression temp;
            Expression result = CoalesceTrue(left, right, isTrue, out temp);
            builder.AddHiddenVariable(temp);
            return result;
        }

        /// <summary>
        /// False coalescing expression.
        /// {result} ::= IsTrue(tmp = {left}) ? tmp : {right}
        /// Generalized OR semantics.
        /// </summary>
        public static Expression CoalesceFalse(LambdaBuilder builder, Expression left, Expression right, MethodInfo isTrue) {
            ContractUtils.RequiresNotNull(isTrue, "isTrue");
            ParameterExpression temp;
            Expression result = CoalesceFalse(left, right, isTrue, out temp);
            builder.AddHiddenVariable(temp);
            return result;
        }
#endif
        public static BinaryExpression Update(this BinaryExpression expression, Expression left, Expression right) {
            return expression.Update(left, expression.Conversion, right);
        }
    }
}
