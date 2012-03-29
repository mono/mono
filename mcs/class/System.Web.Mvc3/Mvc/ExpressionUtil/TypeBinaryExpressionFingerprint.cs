#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    // TypeBinary fingerprint class
    // Expression of form "obj is T"

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class TypeBinaryExpressionFingerprint : ExpressionFingerprint {

        public TypeBinaryExpressionFingerprint(ExpressionType nodeType, Type type, Type typeOperand)
            : base(nodeType, type) {

            TypeOperand = typeOperand;
        }

        // http://msdn.microsoft.com/en-us/library/system.linq.expressions.typebinaryexpression.typeoperand.aspx
        public Type TypeOperand { get; private set; }

        public override bool Equals(object obj) {
            TypeBinaryExpressionFingerprint other = obj as TypeBinaryExpressionFingerprint;
            return (other != null)
                && Equals(this.TypeOperand, other.TypeOperand)
                && this.Equals(other);
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            combiner.AddObject(TypeOperand);
            base.AddToHashCodeCombiner(combiner);
        }

    }
}
