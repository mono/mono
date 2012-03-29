#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    // MemberExpression fingerprint class
    // Expression of form xxx.FieldOrProperty

    [SuppressMessage("Microsoft.Usage", "CA2218:OverrideGetHashCodeOnOverridingEquals", Justification = "Overrides AddToHashCodeCombiner() instead.")]
    internal sealed class MemberExpressionFingerprint : ExpressionFingerprint {

        public MemberExpressionFingerprint(ExpressionType nodeType, Type type, MemberInfo member)
            : base(nodeType, type) {

            Member = member;
        }

        // http://msdn.microsoft.com/en-us/library/system.linq.expressions.memberexpression.member.aspx
        public MemberInfo Member { get; private set; }

        public override bool Equals(object obj) {
            MemberExpressionFingerprint other = obj as MemberExpressionFingerprint;
            return (other != null)
                && Equals(this.Member, other.Member)
                && this.Equals(other);
        }

        internal override void AddToHashCodeCombiner(HashCodeCombiner combiner) {
            combiner.AddObject(Member);
            base.AddToHashCodeCombiner(combiner);
        }

    }
}
