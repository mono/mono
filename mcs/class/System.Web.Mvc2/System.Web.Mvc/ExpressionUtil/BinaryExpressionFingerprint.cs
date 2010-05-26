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

    // BinaryExpression fingerprint class
    // Useful for things like array[index]
    //
    // This particular fingerprint doesn't support the BinaryExpression.Conversion property,
    // which is used in some coalescing operations.
    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals",
        Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class BinaryExpressionFingerprint : ExpressionFingerprint {

        private BinaryExpressionFingerprint(BinaryExpression expression)
            : base(expression) {
            // don't care about UnaryExpression.IsLifted since it's not necessary to uniquely describe the expression,
            // but IsLiftedToNull *is* required

            IsLiftedToNull = expression.IsLiftedToNull;
            Method = expression.Method;
        }

        public bool IsLiftedToNull {
            get;
            private set;
        }

        public ExpressionFingerprint Left {
            get;
            private set;
        }

        public MethodInfo Method {
            get;
            private set;
        }

        public ExpressionFingerprint Right {
            get;
            private set;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            base.AddToHashCodeCombiner(combiner);

            combiner.AddInt32(IsLiftedToNull.GetHashCode());
            combiner.AddFingerprint(Left);
            combiner.AddObject(Method);
            combiner.AddFingerprint(Right);
        }

        public static BinaryExpressionFingerprint Create(BinaryExpression expression, ParserContext parserContext) {
            if (expression.Conversion != null) {
                // we don't support the Conversion property
                return null;
            }

            // if any fingerprinting fails, bail out
            ExpressionFingerprint left = Create(expression.Left, parserContext);
            if (left == null && expression.Left != null) {
                return null;
            }

            ExpressionFingerprint right = Create(expression.Right, parserContext);
            if (right == null && expression.Right != null) {
                return null;
            }

            return new BinaryExpressionFingerprint(expression) {
                Left = left,
                Right = right
            };
        }

        public override bool Equals(object obj) {
            BinaryExpressionFingerprint other = obj as BinaryExpressionFingerprint;
            if (other == null) {
                return false;
            }

            return (this.IsLiftedToNull == other.IsLiftedToNull
                && Object.Equals(this.Left, other.Left)
                && this.Method == other.Method
                && Object.Equals(this.Right, other.Right)
                && base.Equals(other));
        }

        public override Expression ToExpression(ParserContext parserContext) {
            Expression leftExpr = ToExpression(Left, parserContext);
            Expression rightExpr = ToExpression(Right, parserContext);
            return Expression.MakeBinary(NodeType, leftExpr, rightExpr, IsLiftedToNull, Method);
        }

    }
}
