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

    // ConditionalExpression fingerprint class
    // Expression of form (test) ? ifTrue : ifFalse
    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals",
        Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class ConditionalExpressionFingerprint : ExpressionFingerprint {

        private ConditionalExpressionFingerprint(ConditionalExpression expression)
            : base(expression) {
        }

        public ExpressionFingerprint Test {
            get;
            private set;
        }

        public ExpressionFingerprint IfTrue {
            get;
            private set;
        }

        public ExpressionFingerprint IfFalse {
            get;
            private set;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            base.AddToHashCodeCombiner(combiner);

            combiner.AddFingerprint(Test);
            combiner.AddFingerprint(IfTrue);
            combiner.AddFingerprint(IfFalse);
        }

        public static ConditionalExpressionFingerprint Create(ConditionalExpression expression, ParserContext parserContext) {
            // if any fingerprinting fails, bail out
            ExpressionFingerprint test = Create(expression.Test, parserContext);
            if (test == null && expression.Test != null) {
                return null;
            }

            ExpressionFingerprint ifTrue = Create(expression.IfTrue, parserContext);
            if (ifTrue == null && expression.IfTrue != null) {
                return null;
            }

            ExpressionFingerprint ifFalse = Create(expression.IfFalse, parserContext);
            if (ifFalse == null && expression.IfFalse != null) {
                return null;
            }

            return new ConditionalExpressionFingerprint(expression) {
                Test = test,
                IfTrue = ifTrue,
                IfFalse = ifFalse
            };
        }

        public override bool Equals(object obj) {
            ConditionalExpressionFingerprint other = obj as ConditionalExpressionFingerprint;
            if (other == null) {
                return false;
            }

            return (Object.Equals(this.Test, other.Test)
                && Object.Equals(this.IfTrue, other.IfTrue)
                && Object.Equals(this.IfFalse, other.IfFalse)
                && base.Equals(other));
        }

        public override Expression ToExpression(ParserContext parserContext) {
            Expression testExpr = ToExpression(Test, parserContext);
            Expression ifTrueExpr = ToExpression(IfTrue, parserContext);
            Expression ifFalseExpr = ToExpression(IfFalse, parserContext);
            return Expression.Condition(testExpr, ifTrueExpr, ifFalseExpr);
        }

    }
}
