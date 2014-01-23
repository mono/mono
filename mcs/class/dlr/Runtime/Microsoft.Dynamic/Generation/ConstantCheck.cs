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

using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {

    public static class ConstantCheck {

        /// <summary>
        /// Tests to see if the expression is a constant with the given value.
        /// </summary>
        /// <param name="expression">The expression to examine</param>
        /// <param name="value">The constant value to check for.</param>
        /// <returns>true/false</returns>
        public static bool Check(Expression expression, object value) {
            ContractUtils.RequiresNotNull(expression, "expression");
            return IsConstant(expression, value);
        }


        /// <summary>
        /// Tests to see if the expression is a constant with the given value.
        /// </summary>
        /// <param name="e">The expression to examine</param>
        /// <param name="value">The constant value to check for.</param>
        /// <returns>true/false</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static bool IsConstant(Expression e, object value) {
            switch (e.NodeType) {
                case ExpressionType.AndAlso:
                    return CheckAndAlso((BinaryExpression)e, value);

                case ExpressionType.OrElse:
                    return CheckOrElse((BinaryExpression)e, value);

                case ExpressionType.Constant:
                    return CheckConstant((ConstantExpression)e, value);

                case ExpressionType.TypeIs:
                    return Check((TypeBinaryExpression)e, value);

                default:
                    return false;
            }
        }

        //CONFORMING
        internal static bool IsNull(Expression e) {
            return IsConstant(e, null);
        }


        private static bool CheckAndAlso(BinaryExpression node, object value) {
            Debug.Assert(node.NodeType == ExpressionType.AndAlso);

            if (node.Method != null) {
                return false;
            }
            //TODO: we can propagate through conversion, but it may not worth it.
            if (node.Conversion != null) {
                return false;
            }
    
            if (value is bool) {
                if ((bool)value) {
                    return IsConstant(node.Left, true) && IsConstant(node.Right, true);
                } else {
                    // if left isn't a constant it has to be evaluated
                    return IsConstant(node.Left, false);
                }
            }
            return false;
        }

        private static bool CheckOrElse(BinaryExpression node, object value) {
            Debug.Assert(node.NodeType == ExpressionType.OrElse);

            if (node.Method != null) {
                return false;
            }

            if (value is bool) {
                if ((bool)value) {
                    return IsConstant(node.Left, true);
                } else {
                    return IsConstant(node.Left, false) && IsConstant(node.Right, false);
                }
            }
            return false;
        }

        private static bool CheckConstant(ConstantExpression node, object value) {
            if (value == null) {
                return node.Value == null;
            } else {
                return value.Equals(node.Value);
            }
        }

        private static bool Check(TypeBinaryExpression node, object value) {
            // allow constant TypeIs expressions to be optimized away
            if (value is bool && ((bool)value) == true) {
                return node.TypeOperand.IsAssignableFrom(node.Expression.Type);
            }
            return false;
        }
    }
}
