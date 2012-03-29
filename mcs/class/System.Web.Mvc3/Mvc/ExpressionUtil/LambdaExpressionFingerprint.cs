#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    // LambdaExpression fingerprint class
    // Represents a lambda expression (root element in Expression<T>)

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class LambdaExpressionFingerprint : ExpressionFingerprint {

        public LambdaExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type) {

            // There are no properties on LambdaExpression that are worth including in
            // the fingerprint.
        }

        public override bool Equals(object obj) {
            LambdaExpressionFingerprint other = obj as LambdaExpressionFingerprint;
            return (other != null)
                && this.Equals(other);
        }

    }
}
