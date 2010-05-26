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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // MethodCallExpression fingerprint class
    // Expression of form xxx.Foo(...), xxx[...] (get_Item()), etc.
    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals",
        Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class MethodCallExpressionFingerprint : ExpressionFingerprint {

        private MethodCallExpressionFingerprint(MethodCallExpression expression)
            : base(expression) {

            Method = expression.Method;
        }

        public ReadOnlyCollection<ExpressionFingerprint> Arguments {
            get;
            private set;
        }

        public MethodInfo Method {
            get;
            private set;
        }

        public ExpressionFingerprint Target {
            get;
            private set;
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            base.AddToHashCodeCombiner(combiner);

            combiner.AddEnumerable(Arguments);
            combiner.AddObject(Method);
            combiner.AddFingerprint(Target);
        }

        public static MethodCallExpressionFingerprint Create(MethodCallExpression expression, ParserContext parserContext) {
            ReadOnlyCollection<ExpressionFingerprint> arguments = Create(expression.Arguments, parserContext);
            if (arguments == null) {
                return null;
            }

            ExpressionFingerprint target = Create(expression.Object, parserContext);
            if (target == null && expression.Object != null) {
                return null;
            }

            return new MethodCallExpressionFingerprint(expression) {
                Arguments = arguments,
                Target = target
            };
        }

        public override bool Equals(object obj) {
            MethodCallExpressionFingerprint other = obj as MethodCallExpressionFingerprint;
            if (other == null) {
                return false;
            }

            return (this.Arguments.SequenceEqual(other.Arguments)
                && this.Method == other.Method
                && Object.Equals(this.Target, other.Target)
                && base.Equals(other));
        }

        public override Expression ToExpression(ParserContext parserContext) {
            Expression targetExpr = ToExpression(Target, parserContext);
            IEnumerable<Expression> argumentsExpr = ToExpression(Arguments, parserContext);
            return Expression.Call(targetExpr, Method, argumentsExpr);
        }

    }
}
