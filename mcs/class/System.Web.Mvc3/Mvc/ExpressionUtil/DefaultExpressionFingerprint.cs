#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    // DefaultExpression fingerprint class
    // Expression of form default(T)

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class DefaultExpressionFingerprint : ExpressionFingerprint {

        public DefaultExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type) {

            // There are no properties on DefaultExpression that are worth including in
            // the fingerprint.
        }

        public override bool Equals(object obj) {
            DefaultExpressionFingerprint other = obj as DefaultExpressionFingerprint;
            return (other != null)
                && this.Equals(other);
        }

    }
}
