namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public sealed class UrlParameter {

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly UrlParameter Optional = new UrlParameter();

        // singleton constructor
        private UrlParameter() { }

        public override string ToString() {
            return String.Empty;
        }
    }
}
