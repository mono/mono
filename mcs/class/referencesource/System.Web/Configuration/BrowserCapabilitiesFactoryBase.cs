//------------------------------------------------------------------------------
// <copyright file="BrowserCapabilitiesFactoryBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Base class for browser capabilities object: just a read-only dictionary
 * holder that supports Init()
 *
 * 


*/

using System.Web.UI;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Util;

namespace System.Web.Configuration {

    public class BrowserCapabilitiesFactoryBase {

        private IDictionary _matchedHeaders;
        private IDictionary _browserElements;
        private object _lock = new object();

        public BrowserCapabilitiesFactoryBase() {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IDictionary BrowserElements {
            get {
                if (_browserElements == null)
                    lock (_lock) {
                        if (_browserElements == null) {
                            Hashtable browserElements = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
                            PopulateBrowserElements(browserElements);
                            _browserElements = browserElements;
                        }
                    }

                return _browserElements;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void PopulateBrowserElements(IDictionary dictionary) {
        }

        internal IDictionary InternalGetMatchedHeaders() {
            return MatchedHeaders;
        }

        internal IDictionary InternalGetBrowserElements() {
            return BrowserElements;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IDictionary MatchedHeaders {
            get {
                if (_matchedHeaders == null)
                    lock (_lock) {
                        if (_matchedHeaders == null) {
                            Hashtable matchedHeaders = Hashtable.Synchronized(new Hashtable(24, StringComparer.OrdinalIgnoreCase));
                            PopulateMatchedHeaders(matchedHeaders);
                            _matchedHeaders = matchedHeaders;
                        }
                    }

                return _matchedHeaders;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void PopulateMatchedHeaders(IDictionary dictionary) {
        }

        internal int CompareFilters(string filter1, string filter2) {
            bool isFilter1DefaultFilter = String.IsNullOrEmpty(filter1);
            bool isFilter2DefaultFilter = String.IsNullOrEmpty(filter2);

            IDictionary browsers = BrowserElements;
            bool filter1Exists = (browsers.Contains(filter1)) || isFilter1DefaultFilter;
            bool filter2Exists = (browsers.Contains(filter2)) || isFilter2DefaultFilter;

            if (!filter1Exists) {
                if (!filter2Exists) {
                    return 0;
                }
                else {
                    return -1;
                }
            }
            else {
                if (!filter2Exists) {
                    return 1;
                }
            }

            if (isFilter1DefaultFilter && !isFilter2DefaultFilter) {
                return 1;
            }

            if (isFilter2DefaultFilter && !isFilter1DefaultFilter) {
                return -1;
            }

            if (isFilter1DefaultFilter && isFilter2DefaultFilter) {
                return 0;
            }

            int filter1Depth = (int)((Triplet)BrowserElements[filter1]).Third;
            int filter2Depth = (int)((Triplet)BrowserElements[filter2]).Third;

            return filter2Depth - filter1Depth;
        }

        public virtual void ConfigureBrowserCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps) {
        }

        // CodeGenerator will override this function to declare custom browser capabilities
        public virtual void ConfigureCustomCapabilities(NameValueCollection headers, HttpBrowserCapabilities browserCaps) {
        }

        internal static string GetBrowserCapKey(IDictionary headers, HttpRequest request) {
            StringBuilder sb = new StringBuilder();
            foreach(String key in headers.Keys) {
                if (key.Length == 0) {
                    sb.Append(HttpCapabilitiesDefaultProvider.GetUserAgent(request));
                }
                else {
                    sb.Append(request.Headers[key]);
                }

                sb.Append("\n");
            }

            return sb.ToString();
        }

        internal HttpBrowserCapabilities GetHttpBrowserCapabilities(HttpRequest request) {
            if (request == null)
                throw new ArgumentNullException("request");

            NameValueCollection headers = request.Headers;
            HttpBrowserCapabilities browserCaps = new HttpBrowserCapabilities();
            Hashtable values = new Hashtable(180, StringComparer.OrdinalIgnoreCase);
            values[String.Empty] = HttpCapabilitiesDefaultProvider.GetUserAgent(request);
            browserCaps.Capabilities = values;
            ConfigureBrowserCapabilities(headers, browserCaps);
            ConfigureCustomCapabilities(headers, browserCaps);

            return browserCaps;
        }

        protected bool IsBrowserUnknown(HttpCapabilitiesBase browserCaps) {
            // We want to ignore the "Default" node, which will also be matched.
            if(browserCaps.Browsers == null || browserCaps.Browsers.Count <= 1) {
                return true;
            }

            return false;
        }
    }
}
