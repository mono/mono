//------------------------------------------------------------------------------
// <copyright file="CapabilitiesState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Security.Permissions;

    //
    // Encapsulates the evaluation state used in computing capabilities
    //
    internal class CapabilitiesState {
        internal HttpRequest _request;
        internal IDictionary _values;
        internal ArrayList _matchlist;
        internal ArrayList _regexlist;
        internal bool _exit;
        internal bool _evaluateOnlyUserAgent;

        internal CapabilitiesState(HttpRequest request, IDictionary values) {
            _request = request;
            _values = values;
            _matchlist = new ArrayList();
            _regexlist = new ArrayList();
        }

        internal bool EvaluateOnlyUserAgent {
            get {
                return _evaluateOnlyUserAgent;
            }
            set {
                _evaluateOnlyUserAgent = value;
            }
        }

        internal virtual void ClearMatch() {
            if (_matchlist == null) {
                _regexlist = new ArrayList();
                _matchlist = new ArrayList();
            }
            else {
                _regexlist.Clear();
                _matchlist.Clear();
            }
        }

        internal virtual void AddMatch(DelayedRegex regex, Match match) {
            _regexlist.Add(regex);
            _matchlist.Add(match);
        }

        internal virtual void PopMatch() {
            _regexlist.RemoveAt(_regexlist.Count - 1);
            _matchlist.RemoveAt(_matchlist.Count - 1);
        }

        internal virtual String ResolveReference(String refname) {
            if (_matchlist == null)
                return String.Empty;

            int i = _matchlist.Count;

            while (i > 0) {
                i--;
                int groupnum = ((DelayedRegex)_regexlist[i]).GroupNumberFromName(refname);

                if (groupnum >= 0) {
                    Group group = ((Match)_matchlist[i]).Groups[groupnum];
                    if (group.Success) {
                        return group.ToString();
                    }
                }
            }

            return String.Empty;
        }

        [AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Low)]
        string ResolveServerVariableWithAssert(string varname) {
            string result = _request.ServerVariables[varname];
            if (result == null)
                return string.Empty;

            return result;
        }

        internal virtual String ResolveServerVariable(String varname) {
            if (varname.Length == 0 || varname == "HTTP_USER_AGENT")
                return HttpCapabilitiesDefaultProvider.GetUserAgent(_request);

            if (EvaluateOnlyUserAgent)
                return string.Empty;
            
            return ResolveServerVariableWithAssert(varname);
        }

        internal virtual String ResolveVariable(String varname) {
            String result;

            result = (String)_values[varname];

            if (result == null)
                return String.Empty;

            return result;
        }

        internal virtual void SetVariable(String varname, String value) {
            _values[varname] = value;
        }

        internal virtual bool Exit {
            get {
                return _exit;
            }
            set {
                _exit = value;
            }
        }
    }
}
