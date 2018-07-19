//------------------------------------------------------------------------------
// <copyright file="AntiXssEncoder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Util;

    public class AntiXssEncoder : HttpEncoder {

        #region HttpEncoder Methods

        // NOTE: No Anti-XSS equivalents for HtmlDecode and HeaderNameValueEncode

        protected internal override void HtmlAttributeEncode(string value, TextWriter output) {
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            output.Write(UnicodeCharacterEncoder.HtmlAttributeEncode(value));
        }

        protected internal override void HtmlEncode(string value, TextWriter output) {
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            output.Write(HtmlEncode(value, false));
        }

        protected internal override byte[] UrlEncode(byte[] bytes, int offset, int count) {
            if (!HttpEncoder.ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            string utf8String = Encoding.UTF8.GetString(bytes, offset, count);
            string result = UrlEncode(utf8String, Encoding.UTF8);
            return Encoding.UTF8.GetBytes(result);
        }

        protected internal override string UrlPathEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return value;
            }

            // DevDiv #211105: We should make the UrlPathEncode method encode only the path portion of URLs.

            string schemeAndAuthority;
            string path;
            string queryAndFragment;
            bool isValidUrl = UriUtil.TrySplitUriForPathEncode(value, out schemeAndAuthority, out path, out queryAndFragment, checkScheme: false);

            if (!isValidUrl) {
                // treat as a relative URL, so we might still need to chop off the query / fragment components
                schemeAndAuthority = null;
                UriUtil.ExtractQueryAndFragment(value, out path, out queryAndFragment);
            }

            return schemeAndAuthority + HtmlParameterEncoder.UrlPathEncode(path, Encoding.UTF8) + queryAndFragment;
        }

        #endregion

        public static void MarkAsSafe(LowerCodeCharts lowerCodeCharts, LowerMidCodeCharts lowerMidCodeCharts,
            MidCodeCharts midCodeCharts, UpperMidCodeCharts upperMidCodeCharts, UpperCodeCharts upperCodeCharts) {

            // should be callable from console apps
            if (HostingEnvironment.IsHosted) {
                HttpApplicationFactory.ThrowIfApplicationOnStartCalled();
            }

            UnicodeCharacterEncoder.MarkAsSafe(lowerCodeCharts, lowerMidCodeCharts, midCodeCharts, upperMidCodeCharts, upperCodeCharts);
        }

        public static string CssEncode(string input) {
            return CssEncoder.Encode(input);
        }

        public static string HtmlEncode(string input, bool useNamedEntities) {
            return UnicodeCharacterEncoder.HtmlEncode(input, useNamedEntities);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "As this is meant as a replacement for HttpUility.Encode we must keep the same return type.")]
        public static string UrlEncode(string input) {
            return UrlEncode(input, Encoding.UTF8);
        }
        
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This does not return a URL so the return type can be a string.")]
        public static string HtmlFormUrlEncode(string input) {
            return HtmlFormUrlEncode(input, Encoding.UTF8);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This does not return a URL so the return type can be a string.")]
        public static string UrlEncode(string input, int codePage) {
            return UrlEncode(input, Encoding.GetEncoding(codePage));
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This does not return a URL so the return type can be a string.")]
        public static string HtmlFormUrlEncode(string input, int codePage) {
            return HtmlFormUrlEncode(input, Encoding.GetEncoding(codePage));
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This does not return a URL so the return type can be a string.")]
        public static string UrlEncode(string input, Encoding inputEncoding) {
            // Assuming the default to be UTF-8
            if (inputEncoding == null) {
                inputEncoding = Encoding.UTF8;
            }

            return HtmlParameterEncoder.QueryStringParameterEncode(input, inputEncoding);
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "This does not return a URL so the return type can be a string.")]
        public static string HtmlFormUrlEncode(string input, Encoding inputEncoding) {
            // Assuming the default to be UTF-8
            if (inputEncoding == null) {
                inputEncoding = Encoding.UTF8;
            }

            return HtmlParameterEncoder.FormStringParameterEncode(input, inputEncoding);
        }

        public static string XmlEncode(string input) {
            return UnicodeCharacterEncoder.XmlEncode(input);
        }

        public static string XmlAttributeEncode(string input) {
            // HtmlEncodeAttribute will handle input
            return UnicodeCharacterEncoder.XmlAttributeEncode(input);
        }

    }
}
