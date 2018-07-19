//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.Specialized;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    // This represents a Query value, which can either be Empty, a Literal or a Variable
    abstract class UriTemplateQueryValue
    {

        readonly UriTemplatePartType nature;
        static UriTemplateQueryValue empty = new EmptyUriTemplateQueryValue();

        protected UriTemplateQueryValue(UriTemplatePartType nature)
        {
            this.nature = nature;
        }

        public static UriTemplateQueryValue Empty
        {
            get
            {
                return UriTemplateQueryValue.empty;
            }
        }

        public UriTemplatePartType Nature
        {
            get
            {
                return this.nature;
            }
        }
        public static UriTemplateQueryValue CreateFromUriTemplate(string value, UriTemplate template)
        {
            // Checking for empty value
            if (value == null)
            {
                return UriTemplateQueryValue.Empty;
            }
            // Identifying the type of value - Literal|Compound|Variable
            switch (UriTemplateHelpers.IdentifyPartType(value))
            {
                case UriTemplatePartType.Literal:
                    return UriTemplateLiteralQueryValue.CreateFromUriTemplate(value);

                case UriTemplatePartType.Compound:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                        SR.UTQueryCannotHaveCompoundValue, template.originalTemplate)));

                case UriTemplatePartType.Variable:
                    return new UriTemplateVariableQueryValue(template.AddQueryVariable(value.Substring(1, value.Length - 2)));

                default:
                    Fx.Assert("Invalid value from IdentifyStringNature");
                    return null;
            }
        }

        public static bool IsNullOrEmpty(UriTemplateQueryValue utqv)
        {
            if (utqv == null)
            {
                return true;
            }
            if (utqv == UriTemplateQueryValue.Empty)
            {
                return true;
            }
            return false;
        }
        public abstract void Bind(string keyName, string[] values, ref int valueIndex, StringBuilder query);

        public abstract bool IsEquivalentTo(UriTemplateQueryValue other);
        public abstract void Lookup(string value, NameValueCollection boundParameters);

        class EmptyUriTemplateQueryValue : UriTemplateQueryValue
        {
            public EmptyUriTemplateQueryValue()
                : base(UriTemplatePartType.Literal)
            {
            }
            public override void Bind(string keyName, string[] values, ref int valueIndex, StringBuilder query)
            {
                query.AppendFormat("&{0}", UrlUtility.UrlEncode(keyName, Encoding.UTF8));
            }

            public override bool IsEquivalentTo(UriTemplateQueryValue other)
            {
                return (other == UriTemplateQueryValue.Empty);
            }
            public override void Lookup(string value, NameValueCollection boundParameters)
            {
                Fx.Assert(string.IsNullOrEmpty(value), "shouldn't have a value");
            }
        }
    }

}
