#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    // ConditionalExpression fingerprint class
    // Expression of form (test) ? ifTrue : ifFalse

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class ConditionalExpressionFingerprint : ExpressionFingerprint {

        public ConditionalExpressionFingerprint(ExpressionType nodeType, Type type)
            : base(nodeType, type) {

            // There are no properties on ConditionalExpression that are worth including in
            // the fingerprint.
        }

        public override bool Equals(object obj) {
            ConditionalExpressionFingerprint other = obj as ConditionalExpressionFingerprint;
            return (other != null)
                && this.Equals(other);
        }

    }
}
