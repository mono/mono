using System;
using System.Security.Permissions;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace System.Net
{
    internal abstract class WebProxyDataBuilder
    {
        private const char addressListDelimiter = ';';
        private const char addressListSchemeValueDelimiter = '=';
        private const char bypassListDelimiter = ';';

        private WebProxyData m_Result;

        public WebProxyData Build()
        {
            m_Result = new WebProxyData();
            BuildInternal();
            return m_Result;
        }

        protected abstract void BuildInternal();

        protected void SetProxyAndBypassList(string addressString, string bypassListString)
        {
            if (addressString != null)
            {
                addressString = addressString.Trim(); // Ignore white spaces in the proxy address string.

                if (addressString != string.Empty)
                {
                    if (addressString.IndexOf(addressListSchemeValueDelimiter) == -1)
                    {
                        // One single proxy (for all schemes): addressString looks like "proxyhost:8080"
                        m_Result.proxyAddress = ParseProxyUri(addressString);
                    }
                    else
                    {
                        // Different proxy settings for different schemes: addressString looks like
                        // "http=httpproxy:80;ftp=ftpproxy:80;https=httpsproxy:80"
                        m_Result.proxyHostAddresses = ParseProtocolProxies(addressString);
                    }

                    // Can't get here without proxyAddress or proxyHostAddresses, should have thrown a FormatException
                    Debug.Assert( (m_Result.proxyAddress != null || m_Result.proxyHostAddresses != null),
                        "Failed parsing proxy settings string");

                    if (bypassListString != null)
                    {
                        bypassListString = bypassListString.Trim(); // Ignore white spaces in the bypass string.

                        if (bypassListString != string.Empty)
                        {
                            bool bypassOnLocal = false;
                            m_Result.bypassList = ParseBypassList(bypassListString, out bypassOnLocal);
                            m_Result.bypassOnLocal = bypassOnLocal;
                        }
                    }
                }
            }
        }

        protected void SetAutoProxyUrl(string autoConfigUrl)
        {
            if (!string.IsNullOrEmpty(autoConfigUrl))
            {
                Uri scriptLocation = null;
                if (Uri.TryCreate(autoConfigUrl, UriKind.Absolute, out scriptLocation))
                {
                    m_Result.scriptLocation = scriptLocation;
                }
            }
        }

        protected void SetAutoDetectSettings(bool value)
        {
            m_Result.automaticallyDetectSettings = value;
        }

        //
        // Parses out a string from IE and turns it into a URI
        //
        private static Uri ParseProxyUri(string proxyString) {
            Debug.Assert(!string.IsNullOrEmpty(proxyString));

            if (proxyString.IndexOf("://") == -1) {
                proxyString = "http://" + proxyString;
            }

            try {
                return new Uri(proxyString);
            }
            catch (UriFormatException e) {
                if (Logging.On) Logging.PrintError(Logging.Web, e.Message);
                throw CreateInvalidProxyStringException(proxyString);
            }
        }

        //
        // Builds a hashtable containing the protocol and proxy URI to use for it.
        //
        private static Hashtable ParseProtocolProxies(string proxyListString) {
            Debug.Assert(!string.IsNullOrEmpty(proxyListString));

            // get a list of "scheme=url" pairs
            string[] proxyListStrings = proxyListString.Split(addressListDelimiter);

            Hashtable proxyListHashTable = new Hashtable(CaseInsensitiveAscii.StaticInstance);

            for (int i = 0; i < proxyListStrings.Length; i++) {

                string schemeValue = proxyListStrings[i].Trim();

                if (schemeValue == string.Empty) {
                    // We ignore empty sections, i.e. initial, final semicolons or sequences of semicolons,
                    // e.g. ";http=httpproxy;;ftp=ftpproxy;". Applications like Fiddler setting the Registry
                    // keys directly, may add final semicolons. To not introduce regressions we just ignore such
                    // empty sections.
                    continue;
                }

                string[] schemeValueStrings = schemeValue.Split(addressListSchemeValueDelimiter);

                if (schemeValueStrings.Length != 2) {
                    throw CreateInvalidProxyStringException(proxyListString);
                }

                schemeValueStrings[0] = schemeValueStrings[0].Trim();
                schemeValueStrings[1] = schemeValueStrings[1].Trim();

                if ((schemeValueStrings[0] == string.Empty) || (schemeValueStrings[1] == string.Empty)) {
                    throw CreateInvalidProxyStringException(proxyListString);
                }

                proxyListHashTable[schemeValueStrings[0]] = ParseProxyUri(schemeValueStrings[1]);
            }

            Debug.Assert(proxyListHashTable.Count > 0);
            return proxyListHashTable;
        }

        private static FormatException CreateInvalidProxyStringException(string originalProxyString) {
            string message = SR.GetString(SR.net_proxy_invalid_url_format, originalProxyString);
            if (Logging.On) Logging.PrintError(Logging.Web, message);
            return new FormatException(message);
        }

        //
        // Converts a simple IE regular expresion string into one
        //  that is compatible with Regex escape sequences.
        //
        private static string BypassStringEscape(string rawString) {

            Debug.Assert(rawString != null);

            // Break up raw string into scheme, host, port.
            // This regular expression is used to get the three components.
            // Scheme and port are optional.
            // If Match fails just assume whole string is the host.
            Regex parser = new
                Regex("^(?<scheme>.*://)?(?<host>[^:]*)(?<port>:[0-9]{1,5})?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            Match results = parser.Match(rawString);
            string scheme, host, port;
            if (results.Success) {
                scheme = results.Groups["scheme"].Value;
                host = results.Groups["host"].Value;
                port = results.Groups["port"].Value;
            } else {
                // Match method failed - set host to whole bypass string
                scheme = string.Empty;
                host = rawString;
                port = string.Empty;
            }

            // Escape any regex reserved chars before constructing final regex.
            scheme = ConvertRegexReservedChars(scheme);
            host = ConvertRegexReservedChars(host);
            port = ConvertRegexReservedChars(port);


            // If scheme or port not specified use regular
            // expression "wildcards" for them.
            if (scheme == string.Empty) {
                // match any leading scheme plus separator
                // but don't require it
                scheme = "(?:.*://)?";
            }
            if (port == string.Empty) {
                // match a port but don't require it
                port = "(?::[0-9]{1,5})?";
            }

            // Construct and return final regular expression
            // with start-of-line and end-of-line anchors.
            return "^" + scheme + host + port + "$";
        }


        private const string regexReserved = "#$()+.?[\\^{|";

        private static string ConvertRegexReservedChars(string rawString) {

            Debug.Assert(rawString != null);

            // Regular expressions reserve
            //   (1) "#$()+.?[\^{|" as special chars.
            //   (2) "*" as a special char.
            //   (3) whitespace as a special char.
            // Convert any char "c" in above list to "\c".
            // Convert reserved char "*" to ".*".
            // Leave whitespace as-is.
            if (rawString.Length == 0)
                return rawString;
            StringBuilder builder = new StringBuilder();
            foreach (char c in rawString) {
                if (regexReserved.IndexOf(c) != -1) {
                    builder.Append('\\');
                } else if (c == '*') {
                    builder.Append('.');
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        //
        // Parses out a string of bypass list entries and coverts it to Regex's that can be used
        //   to match against.
        //
        private static ArrayList ParseBypassList(string bypassListString, out bool bypassOnLocal) {
            string[] bypassListStrings = bypassListString.Split(bypassListDelimiter);
            bypassOnLocal = false;
            if (bypassListStrings.Length == 0) {
                return null;
            }
            ArrayList bypassList = null;
            foreach (string bypassString in bypassListStrings) {
                if (bypassString!=null) {
                    string trimmedBypassString = bypassString.Trim();
                    if (trimmedBypassString.Length>0) {
                        if (string.Compare(trimmedBypassString, "<local>", StringComparison.OrdinalIgnoreCase)==0) {
                            bypassOnLocal = true;
                        }
                        else {
                            trimmedBypassString = BypassStringEscape(trimmedBypassString);
                            if (bypassList==null) {
                                bypassList = new ArrayList();
                            }
                            GlobalLog.Print("WebProxyDataBuilder::ParseBypassList() bypassList.Count:" + bypassList.Count + " adding:" + ValidationHelper.ToString(trimmedBypassString));
                            if (!bypassList.Contains(trimmedBypassString)) {
                                bypassList.Add(trimmedBypassString);
                                GlobalLog.Print("WebProxyDataBuilder::ParseBypassList() bypassList.Count:" + bypassList.Count + " added:" + ValidationHelper.ToString(trimmedBypassString));
                            }
                        }
                    }
                }
            }
            return bypassList;
        }

    }
}
