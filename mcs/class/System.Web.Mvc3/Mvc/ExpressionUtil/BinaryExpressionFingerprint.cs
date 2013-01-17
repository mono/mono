#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    // BinaryExpression fingerprint class
    // Useful for things like array[index]

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class BinaryExpressionFingerprint : ExpressionFingerprint {

        public BinaryExpressionFingerprint(ExpressionType nodeType, Type type, MethodInfo method)
            : base(nodeType, type) {

            // Other properties on BinaryExpression (like IsLifted / IsLiftedToNull) are simply derived
            // from Type and NodeType, so they're not necessary for inclusion in the fingerprint.

            Method = method;
        }

        // http://msdn.microsoft.com/en-us/library/system.linq.expressions.binaryexpression.method.aspx
        public MethodInfo Method { get; private set; }

        public override bool Equals(object obj) {
            BinaryExpressionFingerprint other = obj as BinaryExpressionFingerprint;
            return (other != null)
                && Equals(this.Method, other.Method)
                && this.Equals(other);
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            combiner.AddObject(Method);
            base.AddToHashCodeCombiner(combiner);
        }

    }
}
