//------------------------------------------------------------------------------
// <copyright file="DelayedRegex.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Xml;

    using Pair = System.Web.UI.Pair;

    internal class DelayedRegex {

        private String _regstring;
        private Regex _regex;

        internal DelayedRegex(String s) {
            _regex = null;
            _regstring = s;
        }

        internal Match Match(String s) {
            EnsureRegex();
            return _regex.Match(s);
        }

        internal int GroupNumberFromName(String name) {
            EnsureRegex();
            return _regex.GroupNumberFromName(name);
        } 

        internal void EnsureRegex() {
            string regstring = _regstring;
            if(_regex == null) {
                _regex = new Regex(regstring);
                //free original
                _regstring = null;
            }
            return;
        }
    }
}
