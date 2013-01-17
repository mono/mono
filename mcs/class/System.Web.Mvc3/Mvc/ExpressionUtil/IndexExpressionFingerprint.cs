#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    // IndexExpression fingerprint class
    // Represents certain forms of array access or indexer property access

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class IndexExpressionFingerprint : ExpressionFingerprint {

        public IndexExpressionFingerprint(ExpressionType nodeType, Type type, PropertyInfo indexer)
            : base(nodeType, type) {

            // Other properties on IndexExpression (like the argument count) are simply derived
            // from Type and Indexer, so they're not necessary for inclusion in the fingerprint.

            Indexer = indexer;
        }

        // http://msdn.microsoft.com/en-us/library/system.linq.expressions.indexexpression.indexer.aspx
        public PropertyInfo Indexer { get; private set; }

        public override bool Equals(object obj) {
            IndexExpressionFingerprint other = obj as IndexExpressionFingerprint;
            return (other != null)
                && Equals(this.Indexer, other.Indexer)
                && this.Equals(other);
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            combiner.AddObject(Indexer);
            base.AddToHashCodeCombiner(combiner);
        }

    }
}
