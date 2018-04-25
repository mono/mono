//------------------------------------------------------------------------------
// <copyright file="CheckPair.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Globalization;

    internal class CheckPair {
        private string _header;
        private string _match;
        private bool _nonMatch;
        internal CheckPair(string header, string match, bool nonMatch) {
            _header = header;
            _match = match;
            _nonMatch = nonMatch;
            //check validity of match string at parse time
            Regex regex = new Regex(match);
        }

        internal CheckPair(string header, string match) {
            _header = header;
            _match = match;
            _nonMatch = false;
            Regex regex = new Regex(match);
        }

        public string Header {
            get {
                return _header;
            }
        }

        public string MatchString {
            get {
                return _match;
            }
        }

        public bool NonMatch {
            get {
                return _nonMatch;
            }
        }
    }
}
