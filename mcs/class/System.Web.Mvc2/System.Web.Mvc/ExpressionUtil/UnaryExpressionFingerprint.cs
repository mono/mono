#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    // UnaryExpression fingerprint class
    // The most common appearance of a UnaryExpression is a cast or other conversion operator
    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals",
        Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class UnaryExpressionFingerprint : ExpressionFingerprint {

        private UnaryExpressionFingerprint(UnaryExpression expression)
            : base(expression) {
            // don't care about UnaryExpression.IsLifted / IsLiftedToNull since they're not necessary to uniquely describe the expression

            Method = expression.Method;
        }

        public MethodInfo Method {
            get;
            private set;
        }

        public ExpressionFingerprint Operand {
            get;
            private set;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            base.AddToHashCodeCombiner(combiner);

            combiner.AddObject(Method);
            combiner.AddFingerprint(Operand);
        }

        public static UnaryExpressionFingerprint Create(UnaryExpression expression, ParserContext parserContext) {
            ExpressionFingerprint operand = Create(expression.Operand, parserContext);
            if (operand == null && expression.Operand != null) {
                // couldn't convert the operand, so bail
                return null;
            }

            return new UnaryExpressionFingerprint(expression) {
                Operand = operand
            };
        }

        public override bool Equals(object obj) {
            UnaryExpressionFingerprint other = obj as UnaryExpressionFingerprint;
            if (other == null) {
                return false;
            }

            return (this.Method == other.Method
                && Object.Equals(this.Operand, other.Operand)
                && base.Equals(other));
        }

        public override Expression ToExpression(ParserContext parserContext) {
            Expression operandExpr = ToExpression(Operand, parserContext);

            // in .NET 3.5 SP1, Expression.MakeUnary() throws if NodeType is UnaryPlus, so special-case
            if (NodeType == ExpressionType.UnaryPlus) {
                return Expression.UnaryPlus(operandExpr, Method);
            }
            else {
                return Expression.MakeUnary(NodeType, operandExpr, Type, Method);
            }
        }

    }
}
