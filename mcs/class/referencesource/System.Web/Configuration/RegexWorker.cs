//------------------------------------------------------------------------------
// <copyright file="RegexWorker.cs" company="Microsoft">
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
using System.Web.RegularExpressions;
using System.Web.Util;

namespace System.Web.Configuration {

    //
    public class RegexWorker {
        internal static readonly Regex RefPat = new BrowserCapsRefRegex();
        private Hashtable _groups;
        private HttpBrowserCapabilities _browserCaps;

        public RegexWorker(HttpBrowserCapabilities browserCaps) {
            _browserCaps = browserCaps;
        }

        private string Lookup(string from) {
            MatchCollection matches = RefPat.Matches(from);

            // shortcut for no reference case
            if (matches.Count == 0) {
                return from;
            }

            StringBuilder sb = new StringBuilder();
            int startIndex = 0;

            foreach (Match match in matches) {
                int length = match.Index - startIndex;
                sb.Append(from.Substring(startIndex, length));
                startIndex = match.Index + match.Length;

                string groupName = match.Groups["name"].Value;

                string result = null;
                if (_groups != null) {
                    result = (String)_groups[groupName];
                }

                if (result == null) {
                    result = _browserCaps[groupName];
                }

                sb.Append(result);
            }

            sb.Append(from, startIndex, from.Length - startIndex);
            string output = sb.ToString();

            // Return null instead of empty string since empty string is used to override values.
            if (output.Length == 0) {
                return null;
            }

            return output;
        }

        public string this[string key] {
            get {
                return Lookup(key);
            }
        }

        public bool ProcessRegex(string target, string regexExpression) {
            if(target == null) {
                target = String.Empty;
            }

            // Adding timeout for Regex in case of malicious string causing DoS
            Regex regex = RegexUtil.CreateRegex(regexExpression, RegexOptions.ExplicitCapture);
            Match match = regex.Match(target);
            if(match.Success == false) {
                return false;
            }

            string[] groups = regex.GetGroupNames();

            if (groups.Length > 0) {
                if (_groups == null) {
                    _groups = new Hashtable();
                }

                for (int i = 0; i < groups.Length; i++) {
                    _groups[groups[i]] = match.Groups[i].Value;
                }
            }

            return true;
        }
    }
}
