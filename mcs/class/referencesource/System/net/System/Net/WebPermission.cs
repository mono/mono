//------------------------------------------------------------------------------
// <copyright file="WebPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net {

    using System.Collections;
    using System.Security;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Runtime.Serialization;

    //NOTE: While WebPermissionAttribute resides in System.DLL,
    //      no classes from that DLL are able to make declarative usage of WebPermission.


    // THE syntax of this attribute is as followed
    // [WebPermission(SecurityAction.Assert, Connect="http://hostname/path/url", Accept="http://localhost/path/url")]
    // [WebPermission(SecurityAction.Assert, ConnectPattern="http://hostname/www\.microsoft\.*/url/*", AcceptPattern="http://localhost/*")]

    // WHERE:
    //=======
    // - 'Connect' and 'Accept' keywords allow you to specify the final URI
    // - 'ConnectPattern' and 'AcceptPattern' keywords allow you to specify a set of URI in escaped Regex form
    // -           They take '.*' as special "everything" indicators, which are fast-pathed.

    [   AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor |
                        AttributeTargets.Class  | AttributeTargets.Struct      |
                        AttributeTargets.Assembly,
                        AllowMultiple = true, Inherited = false )]

    [Serializable()] sealed public class WebPermissionAttribute: CodeAccessSecurityAttribute
    {
        private object m_accept  = null;
        private object m_connect = null;

        public WebPermissionAttribute( SecurityAction action ): base( action )
        {
        }

        public string Connect {
            get { return m_connect as string;}
            set {
                if (m_connect != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "Connect", value), "value");
                }
                m_connect = value;
            }
        }

        public string Accept {
            get { return m_accept as string; }
            set {
                if (m_accept != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "Accept", value), "value");
                }
                m_accept = value;
            }
        }

        public string ConnectPattern {
            get
            {
                return m_connect is DelayedRegex ? m_connect.ToString() : m_connect is bool && (bool) m_connect ? WebPermission.MatchAll : null;
            }

            set {
                if (m_connect != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "ConnectPatern", value), "value");
                }
                if (value == WebPermission.MatchAll)
                {
                    m_connect = true;
                }
                else
                {
                    m_connect = new DelayedRegex(value);
                }
            }
        }

        public string AcceptPattern {
            get
            {
                return m_accept is DelayedRegex ? m_accept.ToString() : m_accept is bool && (bool) m_accept ? WebPermission.MatchAll : null;
            }

            set
            {
                if (m_accept != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "AcceptPattern", value), "value");
                }
                if (value == WebPermission.MatchAll)
                {
                    m_accept = true;
                }
                else
                {
                    m_accept = new DelayedRegex(value);
                }
            }
        }

/*
        public bool ConnectAll
        {
            get
            {
                return m_connect is bool ? (bool) m_connect : false;
            }

            set
            {
                if (m_connect != null)
                {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "ConnectAll", value), "value");
                }
                m_connect = value;
            }
        }

        public bool AcceptAll
        {
            get
            {
                return m_accept is bool ? (bool) m_accept : false;
            }

            set
            {
                if (m_accept != null)
                {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, "AcceptAll", value), "value");
                }
                m_accept = value;
            }
        }
*/

        public override IPermission CreatePermission()
        {
            WebPermission perm = null;
            if (Unrestricted) {
                perm = new WebPermission( PermissionState.Unrestricted);
            }
            else {
                NetworkAccess access = (NetworkAccess) 0;
                if (m_connect is bool)
                {
                    if ((bool) m_connect)
                    {
                        access |= NetworkAccess.Connect;
                    }
                    m_connect = null;
                }
                if (m_accept is bool)
                {
                    if ((bool) m_accept)
                    {
                        access |= NetworkAccess.Accept;
                    }
                    m_accept = null;
                }
                perm = new WebPermission(access);
                if (m_accept != null) {
                    if (m_accept is DelayedRegex) {
                        perm.AddAsPattern(NetworkAccess.Accept, (DelayedRegex)m_accept);
                    }
                    else {
                        perm.AddPermission(NetworkAccess.Accept, (string)m_accept);
                    }
                }
                if (m_connect != null) {
                    if (m_connect is DelayedRegex) {
                        perm.AddAsPattern(NetworkAccess.Connect, (DelayedRegex)m_connect);
                    }
                    else {
                        perm.AddPermission(NetworkAccess.Connect, (string)m_connect);
                    }
                }
            }
            return perm;
        }

    }

    [Serializable]
    internal class DelayedRegex
    {
        private Regex   _AsRegex;
        private string  _AsString;

        internal DelayedRegex(string regexString)
        {
            if (regexString == null)
                throw new ArgumentNullException("regexString");

            _AsString = regexString;
        }

        internal DelayedRegex(Regex regex)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");

            _AsRegex = regex;
        }

        internal Regex AsRegex
        {
            get
            {
                if (_AsRegex == null)
                {
                    _AsRegex = new Regex(_AsString + "[/]?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
                }
                return _AsRegex;
            }
        }

        public override string ToString()
        {
            return _AsString != null ? _AsString : (_AsString = _AsRegex.ToString());
        }
    }

    /// <devdoc>
    ///    <para>
    ///       Controls rights to make or accept connections on a Web address.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public sealed class WebPermission : CodeAccessPermission, IUnrestrictedPermission {

        private bool m_noRestriction;
        [OptionalField] private bool m_UnrestrictedConnect;
        [OptionalField] private bool m_UnrestrictedAccept;
        private ArrayList m_connectList = new ArrayList();
        private ArrayList m_acceptList = new ArrayList();

        internal const string MatchAll = ".*";
        private static volatile Regex s_MatchAllRegex;
        internal static Regex MatchAllRegex
        {
            get
            {
                if (s_MatchAllRegex == null)
                {
                    s_MatchAllRegex = new Regex(".*");
                }
                return s_MatchAllRegex;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the enumeration of permissions to connect a remote URI.
        ///    </para>
        /// </devdoc>
        public IEnumerator ConnectList {
            get {
                if (m_UnrestrictedConnect)
                {
                    return (new Regex[] { MatchAllRegex }).GetEnumerator();
                }

                ArrayList cloned = new ArrayList(m_connectList.Count);

                for (int i = 0; i < m_connectList.Count; ++i)
                    cloned.Add(m_connectList[i] is DelayedRegex? (object)((DelayedRegex)m_connectList[i]).AsRegex :
                    m_connectList[i] is Uri? (object)((Uri)m_connectList[i]).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped) :
                    m_connectList[i]);

                return cloned.GetEnumerator();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the enumeration of permissions to export a local URI.
        ///    </para>
        /// </devdoc>
        public IEnumerator AcceptList {
            get {
                if (m_UnrestrictedAccept)
                {
                    return (new Regex[] { MatchAllRegex }).GetEnumerator();
                }

                ArrayList cloned = new ArrayList(m_acceptList.Count);

                for (int i = 0; i < m_acceptList.Count; ++i)
                    cloned.Add(m_acceptList[i] is DelayedRegex? (object)((DelayedRegex)m_acceptList[i]).AsRegex :
                    m_acceptList[i] is Uri? (object)((Uri)m_acceptList[i]).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped) :
                    m_acceptList[i]);

                return cloned.GetEnumerator();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebPermission'/>
        ///       class that passes all demands or
        ///       that fails all demands.
        ///    </para>
        /// </devdoc>
        public WebPermission(PermissionState state) {
            m_noRestriction = (state == PermissionState.Unrestricted);
        }

        internal WebPermission(bool unrestricted) {
            m_noRestriction = unrestricted;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebPermission'/> class.
        ///    </para>
        /// </devdoc>
        public WebPermission() {
        }

        internal WebPermission(NetworkAccess access)
        {
            m_UnrestrictedConnect = (access & NetworkAccess.Connect) != 0;
            m_UnrestrictedAccept = (access & NetworkAccess.Accept) != 0;
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebPermission'/>
        ///       class with the specified access rights for
        ///       the specified URI Pattern.
        ///       Suitable only for WebPermission policy object construction
        ///    </para>
        /// </devdoc>
        public WebPermission(NetworkAccess access, Regex uriRegex) {
            AddPermission(access, uriRegex);
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.WebPermission'/>
        ///       class with the specified access rights for
        ///       the specified Uniform Resource Identifier .
        ///       Suitable for requesting particular WebPermission
        ///    </para>
        /// </devdoc>
        // <
        public WebPermission(NetworkAccess access, String uriString) {
            AddPermission(access, uriString);
        }
        //
        // <
        internal WebPermission(NetworkAccess access, Uri uri) {
            AddPermission(access, uri);
        }

        // Methods specific to this class
        /// <devdoc>
        ///   <para>
        ///      Adds a new instance of the WebPermission
        ///      class with the specified access rights for the particular Uniform Resource Identifier.
        ///    </para>
        /// </devdoc>
        // <
        public void AddPermission(NetworkAccess access, String  uriString) {
            if (uriString == null) {
                throw new ArgumentNullException("uriString");
            }

            if (m_noRestriction)
            {
                return;
            }

            Uri uri;
            if (Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                AddPermission(access, uri);
            else
            {
                ArrayList lists = new ArrayList();
                if ((access & NetworkAccess.Connect) != 0 && !m_UnrestrictedConnect)
                    lists.Add(m_connectList);
                if ((access & NetworkAccess.Accept) != 0 && !m_UnrestrictedAccept)
                    lists.Add(m_acceptList);

                foreach (ArrayList list in lists)
                {
                    // avoid duplicated uris in the list
                    bool found = false;
                    foreach (object obj in list) {
                        string str = obj as string;
                        if (str != null && string.Compare(str, uriString, StringComparison.OrdinalIgnoreCase ) == 0)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        list.Add(uriString);
                    }
                }
            }
        }

        // <
        internal void AddPermission(NetworkAccess access, Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            if (m_noRestriction)
            {
                return;
            }

            ArrayList lists = new ArrayList();
            if ((access & NetworkAccess.Connect) != 0 && !m_UnrestrictedConnect)
                lists.Add(m_connectList);
            if ((access & NetworkAccess.Accept) != 0 && !m_UnrestrictedAccept)
                lists.Add(m_acceptList);

            foreach (ArrayList list in lists)
            {
                // avoid duplicated uris in the list
                bool found = false;
                foreach (object permObj in list) {
                    if ((permObj is Uri) && uri.Equals(permObj))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    list.Add(uri);
                }
            }
        }

        /// <devdoc>
        /// <para>Adds a new instance of the <see cref='System.Net.WebPermission'/>
        /// class with the specified access rights for the specified URI Pattern.
        /// Should be used during a policy object creation and not for particular URI permission check</para>
        /// </devdoc>
        public void AddPermission(NetworkAccess access, Regex uriRegex) {
            if (uriRegex == null) {
                throw new ArgumentNullException("uriRegex");
            }

            if (m_noRestriction)
            {
                return;
            }

            if (uriRegex.ToString() == MatchAll)
            {
                if (!m_UnrestrictedConnect && (access & NetworkAccess.Connect) != 0)
                {
                    m_UnrestrictedConnect = true;
                    m_connectList.Clear();
                }
                if (!m_UnrestrictedAccept && (access & NetworkAccess.Accept) != 0)
                {
                    m_UnrestrictedAccept = true;
                    m_acceptList.Clear();
                }
                return;
            }

            AddAsPattern(access, new DelayedRegex(uriRegex));
        }

        //  Overloaded form using string inputs
        //  Enforces case-insensitive matching
        /// Adds a new instance of the System.Net.WebPermission
        /// class with the specified access rights for the specified URI Pattern
        internal void AddAsPattern(NetworkAccess access, DelayedRegex uriRegexPattern)
        {
            ArrayList lists = new ArrayList();
            if ((access & NetworkAccess.Connect) != 0 && !m_UnrestrictedConnect)
                lists.Add(m_connectList);
            if ((access & NetworkAccess.Accept) != 0 && !m_UnrestrictedAccept)
                lists.Add(m_acceptList);

            foreach (ArrayList list in lists)
            {
                // avoid duplicated regexes in the list
                bool found = false;
                foreach (object obj in list) {
                    if ((obj is DelayedRegex) && (string.Compare(uriRegexPattern.ToString(), obj.ToString(), StringComparison.OrdinalIgnoreCase ) == 0)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    list.Add(uriRegexPattern);
                }
            }
        }

        // IUnrestrictedPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Checks the overall permisison state of the object.
        ///    </para>
        /// </devdoc>
        public bool IsUnrestricted() {
            return m_noRestriction;
        }

        // IPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Creates a copy of a <see cref='System.Net.WebPermission'/> instance.
        ///    </para>
        /// </devdoc>
        public override IPermission Copy() {
            if (m_noRestriction)
            {
                return new WebPermission(true);
            }

            WebPermission wp = new WebPermission((m_UnrestrictedConnect ? NetworkAccess.Connect : (NetworkAccess) 0) |
                (m_UnrestrictedAccept ? NetworkAccess.Accept : (NetworkAccess)0));
            wp.m_acceptList = (ArrayList)m_acceptList.Clone();
            wp.m_connectList = (ArrayList)m_connectList.Clone();
            return wp;
        }

        /// <devdoc>
        /// <para>Compares two <see cref='System.Net.WebPermission'/> instances.</para>
        /// </devdoc>
        public override bool IsSubsetOf(IPermission target) {
            // Pattern suggested by security engine
            if (target == null) {
                return !m_noRestriction && !m_UnrestrictedConnect && !m_UnrestrictedAccept && m_connectList.Count == 0 && m_acceptList.Count == 0;
            }

            WebPermission other = target as WebPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if (other.m_noRestriction)
            {
                return true;
            }
            else if (m_noRestriction)
            {
                return false;
            }

            //
            // Besides SPECIAL case, this method is restricted to only final URIs (strings) on
            // the current object.
            // The restriction comes from the problem of finding a Regex to be a subset of another Regex
            //
            DelayedRegex regex = null;

            if (!other.m_UnrestrictedAccept)
            {
                if (m_UnrestrictedAccept)
                {
                    return false;
                }
                else if (m_acceptList.Count != 0)
                {
                    if (other.m_acceptList.Count == 0)
                    {
                        return false;
                    }
                    foreach(object obj in this.m_acceptList) {
                        regex = obj as DelayedRegex;
                        if(regex != null) {
                            if(isSpecialSubsetCase(obj.ToString(), other.m_acceptList))
                                continue;
                            throw new NotSupportedException(SR.GetString(SR.net_perm_both_regex));
                        }
                        if(!isMatchedURI(obj, other.m_acceptList))
                            return false;
                    }
                }
            }

            if (!other.m_UnrestrictedConnect)
            {
                if (m_UnrestrictedConnect)
                {
                    return false;
                }
                else if (m_connectList.Count != 0)
                {
                    if (other.m_connectList.Count == 0)
                    {
                        return false;
                    }
                    foreach(object obj in this.m_connectList) {
                        regex = obj as DelayedRegex;
                        if(regex != null) {
                            if(isSpecialSubsetCase(obj.ToString(), other.m_connectList))
                                continue;
                            throw new NotSupportedException(SR.GetString(SR.net_perm_both_regex));
                        }
                        if(!isMatchedURI(obj, other.m_connectList))
                            return false;
                    }
                }
            }

            return true;
        }

        //Checks special case when testing Regex to be a subset of other Regex
        //Support only the case when  both Regexes are identical as strings.
        private static bool isSpecialSubsetCase(String regexToCheck, ArrayList permList) {

            Uri uri;
            foreach(object uriPattern in permList) {
                DelayedRegex regex = uriPattern as DelayedRegex;
                if(regex != null) {
                    //regex parameter against regex permission
                    if (String.Compare(regexToCheck, regex.ToString(), StringComparison.OrdinalIgnoreCase ) == 0)
                        return true;
                }
                else if ((uri = uriPattern as Uri) != null) {
                    //regex parameter against Uri permission
                    if (String.Compare(regexToCheck, Regex.Escape(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped)), StringComparison.OrdinalIgnoreCase ) == 0)
                        return true;
                }
                else if (String.Compare(regexToCheck, Regex.Escape(uriPattern.ToString()), StringComparison.OrdinalIgnoreCase ) == 0) {
                   //regex parameter against string permission
                   return true;
                }

            }

            return false;
       }

        // The union of two web permissions is formed by concatenating
        // the list of allowed regular expressions. There is no check
        // for duplicates/overlaps
        /// <devdoc>
        /// <para>Returns the logical union between two <see cref='System.Net.WebPermission'/> instances.</para>
        /// </devdoc>
        public override IPermission Union(IPermission target) {
            // Pattern suggested by Security engine
            if (target==null) {
                return this.Copy();
            }
            WebPermission other = target as WebPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if (m_noRestriction || other.m_noRestriction)
            {
                return new WebPermission(true);
            }

            WebPermission result = new WebPermission();

            if (m_UnrestrictedConnect || other.m_UnrestrictedConnect)
            {
                result.m_UnrestrictedConnect = true;
            }
            else
            {
                result.m_connectList = (ArrayList) other.m_connectList.Clone();

                for (int i = 0; i < m_connectList.Count; i++) {
                    DelayedRegex uriPattern = m_connectList[i] as DelayedRegex;
                    if(uriPattern == null)
                        if (m_connectList[i] is string)
                            result.AddPermission(NetworkAccess.Connect, (string)m_connectList[i]);
                        else
                            result.AddPermission(NetworkAccess.Connect, (Uri)m_connectList[i]);
                    else
                        result.AddAsPattern(NetworkAccess.Connect, uriPattern);
                }
            }

            if (m_UnrestrictedAccept || other.m_UnrestrictedAccept)
            {
                result.m_UnrestrictedAccept = true;
            }
            else
            {
                result.m_acceptList = (ArrayList) other.m_acceptList.Clone();

                for (int i = 0; i < m_acceptList.Count; i++) {
                    DelayedRegex uriPattern = m_acceptList[i] as DelayedRegex;
                    if(uriPattern == null)
                        if (m_acceptList[i] is string)
                            result.AddPermission(NetworkAccess.Accept, (string)m_acceptList[i]);
                        else
                            result.AddPermission(NetworkAccess.Accept, (Uri)m_acceptList[i]);
                    else
                        result.AddAsPattern(NetworkAccess.Accept, uriPattern);
                }
            }

            return result;
        }

        /// <devdoc>
        /// <para>Returns the logical intersection between two <see cref='System.Net.WebPermission'/> instances.</para>
        /// </devdoc>
        public override IPermission Intersect(IPermission target) {
            // Pattern suggested by Security engine
            if (target == null) {
                return null;
            }

            WebPermission other = target as WebPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if (m_noRestriction)
            {
                return other.Copy();
            }
            if (other.m_noRestriction)
            {
                return Copy();
            }

            WebPermission result = new WebPermission();

            if (m_UnrestrictedConnect && other.m_UnrestrictedConnect)
            {
                result.m_UnrestrictedConnect = true;
            }
            else if (m_UnrestrictedConnect || other.m_UnrestrictedConnect)
            {
                result.m_connectList = (ArrayList) (m_UnrestrictedConnect ? other : this).m_connectList.Clone();
            }
            else
            {
                intersectList(m_connectList, other.m_connectList, result.m_connectList);
            }

            if (m_UnrestrictedAccept && other.m_UnrestrictedAccept)
            {
                result.m_UnrestrictedAccept = true;
            }
            else if (m_UnrestrictedAccept || other.m_UnrestrictedAccept)
            {
                result.m_acceptList = (ArrayList) (m_UnrestrictedAccept ? other : this).m_acceptList.Clone();
            }
            else
            {
                intersectList(m_acceptList, other.m_acceptList, result.m_acceptList);
            }

            // return null if resulting permission is restricted and empty
            if (!result.m_UnrestrictedConnect && !result.m_UnrestrictedAccept &&
                result.m_connectList.Count == 0 && result.m_acceptList.Count == 0) {
                return null;
            }
            return result;
        }

        /// <devdoc>
        /// </devdoc>
        public override void FromXml(SecurityElement securityElement) {
            if (securityElement == null) {

                //
                // null SecurityElement
                //

                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission")) {

                //
                // SecurityElement must be a permission element
                //

                throw new ArgumentException(SR.GetString(SR.net_not_ipermission), "securityElement");
            }

            string className = securityElement.Attribute("class");

            if (className == null) {

                //
                // SecurityElement must be a permission element for this type
                //

                throw new ArgumentException(SR.GetString(SR.net_no_classname), "securityElement");
            }
            if (className.IndexOf(this.GetType().FullName) < 0) {

                //
                // SecurityElement must be a permission element for this type
                //

                throw new ArgumentException(SR.GetString(SR.net_no_typename), "securityElement");
            }

            String str = securityElement.Attribute("Unrestricted");

            m_connectList = new ArrayList();
            m_acceptList = new ArrayList();
            m_UnrestrictedAccept = m_UnrestrictedConnect = false;

            if (str != null && string.Compare(str, "true", StringComparison.OrdinalIgnoreCase ) == 0)
            {
                m_noRestriction = true;
                return;
            }

            m_noRestriction = false;

            SecurityElement et = securityElement.SearchForChildByTag("ConnectAccess");
            string uriPattern;

            if (et != null) {

                foreach(SecurityElement uriElem in et.Children) {
                    //NOTE: Any stuff coming from XML is treated as URI PATTERN!
                    if (uriElem.Tag.Equals("URI")) {
                        try {
                            uriPattern = uriElem.Attribute("uri");
                        }
                        catch {
                            uriPattern = null;
                        }
                        if (uriPattern == null) {
                            throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val_in_element), "ConnectAccess");
                        }
                        if (uriPattern == MatchAll)
                        {
                            m_UnrestrictedConnect = true;
                            m_connectList = new ArrayList();
                            break;
                        }
                        else
                        {
                            AddAsPattern(NetworkAccess.Connect, new DelayedRegex(uriPattern));
                        }
                    }
                    else {
                        // improper tag found, just ignore
                    }
                }
            }

            et = securityElement.SearchForChildByTag("AcceptAccess");
            if (et != null) {

                foreach(SecurityElement uriElem in et.Children) {
                    //NOTE: Any stuff coming from XML is treated as URI PATTERN!
                    if (uriElem.Tag.Equals("URI")) {
                        try {
                            uriPattern = uriElem.Attribute("uri");
                        }
                        catch {
                            uriPattern = null;
                        }
                        if (uriPattern == null) {
                            throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val_in_element), "AcceptAccess");
                        }
                        if (uriPattern == MatchAll)
                        {
                            m_UnrestrictedAccept = true;
                            m_acceptList = new ArrayList();
                            break;
                        }
                        else
                        {
                            AddAsPattern(NetworkAccess.Accept, new DelayedRegex(uriPattern));
                        }
                    }
                    else {
                        // improper tag found, just ignore
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override SecurityElement ToXml() {

            SecurityElement securityElement = new SecurityElement("IPermission");

            securityElement.AddAttribute( "class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ) );
            securityElement.AddAttribute( "version", "1" );

            if (!IsUnrestricted()) {
                String tempStr=null;

                if (m_UnrestrictedConnect || m_connectList.Count > 0)
                {
                    SecurityElement connectElement = new SecurityElement( "ConnectAccess" );

                    if (m_UnrestrictedConnect)
                    {
                        SecurityElement uripattern = new SecurityElement("URI");
                        uripattern.AddAttribute("uri", SecurityElement.Escape(MatchAll));
                        connectElement.AddChild(uripattern);
                    }
                    else
                    {
                        //NOTE All strings going to XML will become URI PATTERNS i.e. escaped to Regex
                        foreach(object obj in m_connectList) {
                            Uri uri = obj as Uri;
                            if(uri != null)
                                tempStr=Regex.Escape(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
                            else
                                tempStr=obj.ToString();

                            if (obj is string)
                                tempStr = Regex.Escape(tempStr);

                            SecurityElement uripattern = new SecurityElement("URI");
                            uripattern.AddAttribute("uri", SecurityElement.Escape(tempStr));
                            connectElement.AddChild(uripattern);
                        }
                    }

                    securityElement.AddChild( connectElement );
                }

                if (m_UnrestrictedAccept || m_acceptList.Count > 0)
                {
                    SecurityElement acceptElement = new SecurityElement("AcceptAccess");

                    if (m_UnrestrictedAccept)
                    {
                        SecurityElement uripattern = new SecurityElement("URI");
                        uripattern.AddAttribute("uri", SecurityElement.Escape(MatchAll));
                        acceptElement.AddChild(uripattern);
                    }
                    else
                    {
                        //NOTE All strings going to XML will become URI PATTERNS i.e. escaped to Regex
                        foreach(object obj in m_acceptList) {
                            Uri  uri = obj as Uri;
                            if(uri != null)
                                tempStr=Regex.Escape(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
                            else
                                tempStr=obj.ToString();

                            if (obj is string)
                                tempStr = Regex.Escape(tempStr);

                            SecurityElement uripattern = new SecurityElement("URI");
                            uripattern.AddAttribute("uri", SecurityElement.Escape(tempStr));
                            acceptElement.AddChild(uripattern);
                        }
                    }

                    securityElement.AddChild( acceptElement );
                }
            }
            else {
                securityElement.AddAttribute( "Unrestricted", "true" );
            }
            return securityElement;
        }

        // Verifies a single Uri against a set of regular expressions
        private static bool isMatchedURI(object uriToCheck, ArrayList uriPatternList) {

            string stringUri = uriToCheck as string;

            foreach(object uriPattern in uriPatternList) {
                DelayedRegex R = uriPattern as DelayedRegex;

                //perform case insensitive comparison of final URIs or strings, a Uri is never equal compares a string (strings are invalid Uris)
                if(R == null) {
                    if (uriToCheck.GetType() == uriPattern.GetType())
                    {
                        if (stringUri != null && string.Compare(stringUri, (string)uriPattern, StringComparison.OrdinalIgnoreCase ) == 0) {
                            return true;
                        }
                        else if(stringUri == null && uriToCheck.Equals(uriPattern)) {
                            return true;
                        }
                    }
                    continue;
                }

                //Otherwise trying match final URI against given Regex pattern
                string s = stringUri != null? stringUri: ((Uri)uriToCheck).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
                Match M = R.AsRegex.Match(s);
                if ((M != null)                             // Found match for the regular expression?
                    && (M.Index == 0)                       // ... which starts at the begining
                    && (M.Length == s.Length)) {            // ... and the whole string matched
                    return true;
                }

                if (stringUri != null)
                    continue;
                //
                // check if the URI was presented in non-canonical form
                //
                s = ((Uri)uriToCheck).GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
                M = R.AsRegex.Match(s);
                if ((M != null)                             // Found match for the regular expression?
                    && (M.Index == 0)                       // ... which starts at the begining
                    && (M.Length == s.Length)) {   // ... and the whole string matched
                    return true;
                }
            }
            return false;
        }

        // We should keep the result as compact as possible since otherwise even
        // simple scenarios in Policy Wizard won;t work due to repeated Union/Intersect calls
        // The issue comes from the "hard" Regex.IsSubsetOf(Regex) problem.
        private static void intersectList(ArrayList A, ArrayList B, ArrayList result) {
            bool[]  aDone = new bool[A.Count];
            bool[]  bDone = new bool[B.Count];
            int     ia=0, ib;

            // The optimization is done according to the following truth
            // (A|B|C) intersect (B|C|E|D)) == B|C|(A inter E)|(A inter D)
            //
            // We also check on any duplicates in the result

            // Round 1st
            // Getting rid of same permissons in the input arrays (assuming X /\ X = X)
            foreach (object a in  A) {
                ib = 0;
                foreach (object b in  B) {

                    // check to see if b is in the result already
                    if (!bDone[ib]) {

                        //if both are regexes or both are Uris or both are strings
                        if (a.GetType() == b.GetType())
                        {
                            if (a is Uri)
                            {
                                // both are uris
                                if (a.Equals(b))
                                {
                                    result.Add(a);
                                    aDone[ia]=bDone[ib]=true;
                                    //since permissions are ORed we can break and go to the next A
                                    break;
                                }
                            }
                            else
                            {
                                // regexes and strings uses ToString() output
                                if (string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase ) == 0)
                                {
                                    result.Add(a);
                                    aDone[ia]=bDone[ib]=true;
                                    //since permissions are ORed we can break and go to the next A
                                    break;
                                }
                            }
                        }
                    }
                    ++ib;
                } //foreach b in B
                ++ia;
            } //foreach a in A

            ia = 0;
            // Round second
            // Grab only intersections of objects not found in both A and B
            foreach (object a in  A) {

                if (!aDone[ia]) {
                    ib = 0;
                    foreach(object b in B) {

                        if (!bDone[ib]) {
                            bool resultUri;
                            object intesection = intersectPair(a, b, out resultUri);

                            if (intesection != null) {
                                bool found = false;
                                // check to see if we already have the same result
                                foreach (object obj in result) {
                                    if (resultUri == (obj is Uri))
                                    {
                                        if(resultUri
                                           ? intesection.Equals(obj)
                                           : string.Compare(obj.ToString(), intesection.ToString(), StringComparison.OrdinalIgnoreCase ) == 0)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }

                                if (!found) {
                                    result.Add(intesection);
                                }
                            }
                        }
                        ++ib;
                    }
                }
                ++ia;
            }
        }

        private static object intersectPair(object L, object R, out bool isUri) {

            //VERY OLD OPTION:  return new Regex("(?=(" + ((Regex)X[i]).ToString()+ "))(" + ((Regex)Y[j]).ToString() + ")","i");
            //STILL OLD OPTION: return new Regex("(?=.*?(" + L.ToString() + "))" + "(?=.*?(" + R.ToString() + "))");
            // check RegexSpec.doc
            //CURRENT OPTION:   return new Regex("(?=(" + L.ToString() + "))(" + R.ToString() + ")", RegexOptions.IgnoreCase );
            isUri = false;
            DelayedRegex L_Pattern =L as DelayedRegex;
            DelayedRegex R_Pattern =R as DelayedRegex;

            if(L_Pattern != null && R_Pattern != null)  {       //both are Regex
                return new DelayedRegex("(?=(" + L_Pattern.ToString() + "))(" + R_Pattern.ToString() + ")");
            }
            else if(L_Pattern != null && R_Pattern == null) {   //only L is a Regex
                    isUri = R is Uri;
                    string uriString = isUri? ((Uri)R).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped): R.ToString();

                    Match M = L_Pattern.AsRegex.Match(uriString);
                    if ((M != null)                             // Found match for the regular expression?
                        && (M.Index == 0)                       // ... which starts at the begining
                        && (M.Length == uriString.Length)) { // ... and the whole string matched
                        return R;
                    }
                    return null;
            }
            else if(L_Pattern == null && R_Pattern != null) {   //only R is a Regex
                    isUri = L is Uri;
                    string uriString = isUri? ((Uri)L).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped):  L.ToString();
                    Match M = R_Pattern.AsRegex.Match(uriString);
                    if ((M != null)                             // Found match for the regular expression?
                        && (M.Index == 0)                       // ... which starts at the begining
                        && (M.Length == uriString.Length)) { // ... and the whole string matched
                        return L;
                    }
                    return null;
           }
           //both are Uris or strings
           isUri = L is Uri;
           if (isUri)
               return L.Equals(R)? L : null;
           else
               return string.Compare(L.ToString(), R.ToString(), StringComparison.OrdinalIgnoreCase ) == 0? L : null;
        }
    } // class WebPermission
} // namespace System.Net
