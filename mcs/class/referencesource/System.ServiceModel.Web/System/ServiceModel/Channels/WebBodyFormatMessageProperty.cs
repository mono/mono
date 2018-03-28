//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
using System.Globalization;
namespace System.ServiceModel.Channels
{
    public sealed class WebBodyFormatMessageProperty : IMessageProperty
    {
        WebContentFormat format;
        static WebBodyFormatMessageProperty jsonProperty;
        public const string Name = "WebBodyFormatMessageProperty";
        static WebBodyFormatMessageProperty xmlProperty;
        static WebBodyFormatMessageProperty rawProperty;

        public WebBodyFormatMessageProperty(WebContentFormat format)
        {
            if (format == WebContentFormat.Default)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR2.GetString(SR2.DefaultContentFormatNotAllowedInProperty)));
            }
            this.format = format;
        }

        public WebContentFormat Format
        {
            get { return this.format; }
        }

        internal static WebBodyFormatMessageProperty JsonProperty
        {
            get
            {
                if (jsonProperty == null)
                {
                    jsonProperty = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                }
                return jsonProperty;
            }
        }

        internal static WebBodyFormatMessageProperty XmlProperty
        {
            get
            {
                if (xmlProperty == null)
                {
                    xmlProperty = new WebBodyFormatMessageProperty(WebContentFormat.Xml);
                }
                return xmlProperty;
            }
        }

        internal static WebBodyFormatMessageProperty RawProperty
        {
            get
            {
                if (rawProperty == null)
                {
                    rawProperty = new WebBodyFormatMessageProperty(WebContentFormat.Raw);
                }
                return rawProperty;
            }
        }

        public IMessageProperty CreateCopy()
        {
            return this;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, SR2.GetString(SR2.WebBodyFormatPropertyToString, this.Format.ToString()));
        }
    }
}
