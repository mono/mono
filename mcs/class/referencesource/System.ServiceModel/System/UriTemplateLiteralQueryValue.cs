//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Specialized;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Text;

    // thin wrapper around string; use type system to help ensure we
    // are doing canonicalization right/consistently
    class UriTemplateLiteralQueryValue : UriTemplateQueryValue, IComparable<UriTemplateLiteralQueryValue>
    {
        readonly string value; // an unescaped representation

        UriTemplateLiteralQueryValue(string value)
            : base(UriTemplatePartType.Literal)
        {
            Fx.Assert(value != null, "bad literal value");
            this.value = value;
        }
        public static UriTemplateLiteralQueryValue CreateFromUriTemplate(string value)
        {
            return new UriTemplateLiteralQueryValue(UrlUtility.UrlDecode(value, Encoding.UTF8));
        }

        public string AsEscapedString()
        {
            return UrlUtility.UrlEncode(this.value, Encoding.UTF8);
        }
        public string AsRawUnescapedString()
        {
            return this.value;
        }
        public override void Bind(string keyName, string[] values, ref int valueIndex, StringBuilder query)
        {
            query.AppendFormat("&{0}={1}", UrlUtility.UrlEncode(keyName, Encoding.UTF8), AsEscapedString());
        }

        public int CompareTo(UriTemplateLiteralQueryValue other)
        {
            return string.Compare(this.value, other.value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            UriTemplateLiteralQueryValue lqv = obj as UriTemplateLiteralQueryValue;
            if (lqv == null)
            {
                Fx.Assert("why would we ever call this?");
                return false;
            }
            else
            {
                return this.value == lqv.value;
            }
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override bool IsEquivalentTo(UriTemplateQueryValue other)
        {
            if (other == null)
            {
                Fx.Assert("why would we ever call this?");
                return false;
            }
            if (other.Nature != UriTemplatePartType.Literal)
            {
                return false;
            }
            UriTemplateLiteralQueryValue otherAsLiteral = other as UriTemplateLiteralQueryValue;
            Fx.Assert(otherAsLiteral != null, "The nature requires that this will be OK");
            return (CompareTo(otherAsLiteral) == 0);
        }
        public override void Lookup(string value, NameValueCollection boundParameters)
        {
            Fx.Assert(string.Compare(this.value, value, StringComparison.Ordinal) == 0, "How can that be?");
        }
    }
}
