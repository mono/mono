namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    public sealed class MvcHtmlString : HtmlString {
        private readonly string _value;

        public MvcHtmlString(string value)
            : base(value ?? String.Empty) {
            _value = value ?? String.Empty;
        }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "MvcHtmlString is immutable")]
        public static readonly MvcHtmlString Empty = Create(String.Empty);

        public static MvcHtmlString Create(string value) {
            return new MvcHtmlString(value);
        }

        public static bool IsNullOrEmpty(MvcHtmlString value) {
            return (value == null || value._value.Length == 0);
        }
    }
}
