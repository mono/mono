//------------------------------------------------------------------------------
// <copyright file="uribuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System {

    using System.Text;
    using System.Globalization;
    using System.Threading;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class UriBuilder {

    // fields

        private bool m_changed = true;
        private string m_fragment = String.Empty;
        private string m_host = "localhost";
        private string m_password = String.Empty;
        private string m_path = "/";
        private int m_port = -1;
        private string m_query = String.Empty;
        private string m_scheme = "http";
        private string m_schemeDelimiter = Uri.SchemeDelimiter;
        private Uri m_uri;
        private string m_username = String.Empty;

    // constructors

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(string uri) {

            // setting allowRelative=true for a string like www.acme.org
            Uri tryUri = new Uri(uri, UriKind.RelativeOrAbsolute);

            if (tryUri.IsAbsoluteUri) {
                Init(tryUri);
            }
            else {
                uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
                Init(new Uri(uri));
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(Uri uri) {

            if ((object)uri == null)
                throw new ArgumentNullException("uri");

            Init(uri);
        }

        private void Init(Uri uri) {
            m_fragment = uri.Fragment;
            m_query = uri.Query;
            m_host = uri.Host;
            m_path = uri.AbsolutePath;
            m_port = uri.Port;
            m_scheme = uri.Scheme;
            m_schemeDelimiter = uri.HasAuthority? Uri.SchemeDelimiter: ":";

            string userInfo = uri.UserInfo;

            if (!string.IsNullOrEmpty(userInfo)) {

                int index = userInfo.IndexOf(':');

                if (index != -1) {
                    m_password = userInfo.Substring(index + 1);
                    m_username = userInfo.Substring(0, index);
                }
                else {
                    m_username = userInfo;
                }
            }
            SetFieldsFromUri(uri);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(string schemeName, string hostName) {
            Scheme = schemeName;
            Host = hostName;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(string scheme, string host, int portNumber) : this(scheme, host) {
            Port = portNumber;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(string scheme,
                          string host,
                          int port,
                          string pathValue
                          ) : this(scheme, host, port)
        {
            Path = pathValue;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public UriBuilder(string scheme,
                          string host,
                          int port,
                          string path,
                          string extraValue
                          ) : this(scheme, host, port, path)
        {
            try {
                Extra = extraValue;
            }
            catch (Exception exception) {
                if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {
                    throw;
                }

                throw new ArgumentException("extraValue");
            }
        }

    // properties

        private string Extra {
            set {
                if (value == null) {
                    value = String.Empty;
                }
                if (value.Length > 0) {
                    if (value[0] == '#') {
                        Fragment = value.Substring(1);
                    }
                    else if (value[0] == '?') {
                        int end = value.IndexOf('#');
                        if (end == -1) {
                            end = value.Length;
                        }
                        else {
                            Fragment = value.Substring(end+1);
                        }
                        Query = value.Substring(1, end-1);
                    } else {
                        throw new ArgumentException("value");
                    }
                }
                else {
                    Fragment = String.Empty;
                    Query = String.Empty;
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Fragment {
            get {
                return m_fragment;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }
                if (value.Length > 0) {
                    value = '#' + value;
                }
                m_fragment = value;
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Host {
            get {
                return m_host;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }
                m_host = value;
                //probable ipv6 address - 
                if (m_host.IndexOf(':') >= 0) {
                    //set brackets
                    if (m_host[0] != '[')
                        m_host = "[" + m_host + "]";
                }
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Password {
            get {
                return m_password;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }
                m_password = value;
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Path {
            get {
                return m_path;
            }
            set {
                if ((value == null) || (value.Length == 0)) {
                    value = "/";
                }
                //if ((value[0] != '/') && (value[0] != '\\')) {
                //    value = '/' + value;
                //}
                m_path = Uri.InternalEscapeString(ConvertSlashes(value));
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Port {
            get {
                return m_port;
            }
            set {
                if (value < -1 || value > 0xFFFF) {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_port = value;
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Query {
            get {
                return m_query;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }
                if (value.Length > 0) {
                    value = '?' + value;
                }
                m_query = value;
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Scheme {
            get {
                return m_scheme;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }

                int index = value.IndexOf(':');
                if (index != -1) {
                    value = value.Substring(0, index);
                }

                if (value.Length != 0)
                {
                    if (!Uri.CheckSchemeName(value)) {
                        throw new ArgumentException("value");
                    }
                    value = value.ToLower(CultureInfo.InvariantCulture);
                }
                m_scheme  = value;
                m_changed = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Uri Uri {
            get {
                if (m_changed) {
                    m_uri = new Uri(ToString());
                    SetFieldsFromUri(m_uri);
                    m_changed = false;
                }
                return m_uri;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string UserName {
            get {
                return m_username;
            }
            set {
                if (value == null) {
                    value = String.Empty;
                }
                m_username = value;
                m_changed = true;
            }
        }

    // methods

        private string ConvertSlashes(string path) {

            StringBuilder sb = new StringBuilder(path.Length);
            char ch;

            for (int i = 0; i < path.Length; ++i) {
                ch = path[i];
                if (ch == '\\') {
                    ch = '/';
                }
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(object rparam) {
            if (rparam == null) {
                return false;
            }
            return Uri.Equals(rparam.ToString());
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return Uri.GetHashCode();
        }

        private void SetFieldsFromUri(Uri uri) {
            m_fragment = uri.Fragment;
            m_query = uri.Query;
            m_host = uri.Host;
            m_path = uri.AbsolutePath;
            m_port = uri.Port;
            m_scheme = uri.Scheme;
            m_schemeDelimiter = uri.HasAuthority? Uri.SchemeDelimiter: ":";

            string userInfo = uri.UserInfo;

            if (userInfo.Length > 0) {

                int index = userInfo.IndexOf(':');

                if (index != -1) {
                    m_password = userInfo.Substring(index + 1);
                    m_username = userInfo.Substring(0, index);
                }
                else {
                    m_username = userInfo;
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ToString() {

            if (m_username.Length == 0 && m_password.Length > 0) {
                throw new UriFormatException(SR.GetString(SR.net_uri_BadUserPassword));
            }

            if (m_scheme.Length != 0)
            {
                UriParser syntax = UriParser.GetSyntax(m_scheme);
                if (syntax != null)
                    m_schemeDelimiter = syntax.InFact(UriSyntaxFlags.MustHaveAuthority) ||
                                        (m_host.Length != 0 && syntax.NotAny(UriSyntaxFlags.MailToLikeUri) && syntax.InFact(UriSyntaxFlags.OptionalAuthority ))
                            ? Uri.SchemeDelimiter
                            : ":";
                else
                    m_schemeDelimiter = m_host.Length != 0? Uri.SchemeDelimiter: ":";
            }

            string result = m_scheme.Length != 0? (m_scheme + m_schemeDelimiter): string.Empty;
            return result
                    + m_username
                    + ((m_password.Length > 0) ? (":" + m_password) : String.Empty)
                    + ((m_username.Length > 0) ? "@" : String.Empty)
                    + m_host
                    + (((m_port != -1) && (m_host.Length > 0)) ? (":" + m_port) : String.Empty)
                    + (((m_host.Length > 0) && (m_path.Length != 0) && (m_path[0] != '/')) ? "/" : String.Empty) + m_path
                    + m_query
                    + m_fragment;
        }
    }
}
