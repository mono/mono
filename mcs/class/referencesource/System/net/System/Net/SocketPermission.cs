//------------------------------------------------------------------------------
// <copyright file="SocketPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System.Collections;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Threading;

    //NOTE: While SocketPermissionAttribute resides in System.DLL,
    //      no classes from that DLL are able to make declarative usage of SocketPermission.


    // THE syntax of this attribute is as followed
    // [SocketPermsion(SecurityAction.Assert, Access=Connect, Host=hostname, Transport=Tcp/Udp/All, port=portN/All)]
    // [SocketPermsion(SecurityAction.Assert, Access=Accept, Host=localname, Transport=Tcp/Udp/All, port=portN/All)]
    //
    // WHERE:
    //=======
    // - hostname is either a DNS hostname OR an IP address 1.2.3.4 or an IP wildcard 1.2.*.*
    // - protocol is either Tcp, Udp or All
    // - port is a numeric value or -1 that means "All Ports"
    //
    //  All the properites Host, Protocol and Port must be specified.
    // "localIP" means that you put here a valid address or DNS name or the localhost
    //
    //  NetworkAccess specifies the scope of permission, i.e. for connecting to remote peer,
    //  or for accepting data on the local resources.
    //

    [   AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor |
                        AttributeTargets.Class  | AttributeTargets.Struct      |
                        AttributeTargets.Assembly,
                        AllowMultiple = true, Inherited = false )]

    [Serializable]
    public sealed class SocketPermissionAttribute: CodeAccessSecurityAttribute
    {
        private string  m_access = null;
        private string  m_host   = null;
        private string  m_port   = null;
        private string  m_transport  = null;

        private const string strAccess     = "Access";
        private const string strConnect    = "Connect";
        private const string strAccept     = "Accept";
        private const string strHost       = "Host";
        private const string strTransport  = "Transport";
        private const string strPort       = "Port";

        public SocketPermissionAttribute( SecurityAction action ): base( action )
        {
        }

        public string Access {
            get { return m_access; }
            set {
                if (m_access != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, strAccess, value), "value");
                }
                m_access = value;
            }
        }

        public string Host {
            get { return m_host; }
            set {
                if (m_host != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, strHost, value), "value");
                }
                m_host = value;
            }
        }

        public string Transport {
            get { return m_transport;}
            set {
                if (m_transport != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, strTransport, value), "value");
                }
                m_transport = value;
            }
        }

        public string Port {
            get { return m_port;}
            set {
                if (m_port != null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_multi, strPort, value), "value");
                }
                m_port = value;
            }
        }

        public override IPermission CreatePermission()
        {
            SocketPermission perm = null;
            if (Unrestricted) {
                perm = new SocketPermission( PermissionState.Unrestricted);
            }
            else {
                perm = new SocketPermission(PermissionState.None);
                if (m_access == null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_count, strAccess));
                }
                if (m_host == null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_count, strHost));
                }
                if (m_transport == null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_count, strTransport));
                }
                if (m_port == null) {
                    throw new ArgumentException(SR.GetString(SR.net_perm_attrib_count, strPort));
                }
                ParseAddPermissions(perm);
            }
            return perm;
        }


        private void ParseAddPermissions(SocketPermission perm) {

            NetworkAccess access;
            if (0 == string.Compare(m_access, strConnect, StringComparison.OrdinalIgnoreCase )) {
                access = NetworkAccess.Connect;
            }
            else
            if (0 == string.Compare(m_access, strAccept, StringComparison.OrdinalIgnoreCase )) {
                access = NetworkAccess.Accept;
            }
            else {
                throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, strAccess, m_access));
            }

            TransportType transport;
            try {
                transport = (TransportType) Enum.Parse(typeof(TransportType), m_transport, true);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {                                       
		            throw;
	            }
                throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, strTransport, m_transport), e);
            }

            int port;
            if (string.Compare(m_port, "All", StringComparison.OrdinalIgnoreCase ) == 0) {
                m_port = "-1";
            }
            try {
                port = Int32.Parse(m_port, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {                                       
		            throw;
	            }
                throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, strPort, m_port), e);
            }

            if (!ValidationHelper.ValidateTcpPort(port) && port != SocketPermission.AllPorts) {
                throw new ArgumentOutOfRangeException("port", port, SR.GetString(SR.net_perm_invalid_val, strPort, m_port));
            }
            perm.AddPermission(access, transport, m_host, port);
        }

    }


    /// <devdoc>
    ///    <para>
    ///       Controls rights to make or accept connections on a transport address.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public sealed class SocketPermission : CodeAccessPermission, IUnrestrictedPermission {

        private ArrayList m_connectList;
        private ArrayList m_acceptList;
        private bool m_noRestriction;


        /// <devdoc>
        ///    <para>
        ///       Returns the enumeration of permissions to connect a remote peer.
        ///    </para>
        /// </devdoc>
        public IEnumerator ConnectList    {get {return m_connectList.GetEnumerator();}}

        /// <devdoc>
        ///    <para>
        ///       Returns the enumeration of permissions to accept incoming connections.
        ///    </para>
        /// </devdoc>
        public IEnumerator AcceptList     {get {return m_acceptList.GetEnumerator();}}


        /// <devdoc>
        ///    <para>
        ///       Defines a constant representing all ports.
        ///    </para>
        /// </devdoc>
        public const int AllPorts = unchecked((int)0xFFFFFFFF);

        //<
        internal const int AnyPort = unchecked((int)0);

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.SocketPermission'/>
        ///       class that passes all demands
        ///       or that fails all demands.
        ///    </para>
        /// </devdoc>
        public SocketPermission(PermissionState state) {
            initialize();
            m_noRestriction = (state == PermissionState.Unrestricted);
        }

        internal SocketPermission(bool free) {
            initialize();
            m_noRestriction = free;
        }


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the SocketPermissions class for the given transport address with the specified permission.
        ///    </para>
        /// </devdoc>
        public SocketPermission(NetworkAccess access, TransportType transport, String hostName, int portNumber) {
            initialize();
            m_noRestriction = false;
            AddPermission(access, transport, hostName, portNumber);
        }

        /// <devdoc>
        ///    <para>
        ///       Adds a permission to the set of permissions for a transport address.
        ///    </para>
        /// </devdoc>
        public void AddPermission(NetworkAccess access, TransportType transport, string hostName, int portNumber) {
            if (hostName == null) {
                throw new ArgumentNullException("hostName");
            }

            EndpointPermission endPoint = new EndpointPermission(hostName, portNumber, transport);

            AddPermission(access, endPoint);
        }

        internal void AddPermission(NetworkAccess access, EndpointPermission endPoint) {
            if (m_noRestriction) {    // Is the permission unrestricted?
                return;             // YES-- then additional endpoints have no effect
            }
            if ((access & NetworkAccess.Connect) != 0)
                    m_connectList.Add(endPoint);
            if ((access & NetworkAccess.Accept) != 0)
                    m_acceptList.Add(endPoint);
        }

        // IUnrestrictedPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Checks the overall permission state of the object.
        ///    </para>
        /// </devdoc>
        public bool IsUnrestricted() {
            return m_noRestriction;
        }

        // IPermission interface methods
        /// <devdoc>
        ///    <para>
        ///       Creates
        ///       a copy of a <see cref='System.Net.SocketPermission'/> instance.
        ///    </para>
        /// </devdoc>
        public override IPermission Copy() {

            SocketPermission sp = new SocketPermission(m_noRestriction);

            sp.m_connectList = (ArrayList)m_connectList.Clone();
            sp.m_acceptList = (ArrayList)m_acceptList.Clone();
                        return sp;
        }

        private bool FindSubset(ArrayList source, ArrayList target) {
            foreach (EndpointPermission e in source) {

                bool found = false;

                foreach (EndpointPermission ee in target) {
                    if (e.SubsetMatch(ee)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    return false;
                }
            }
            return true;
        }

        /// <devdoc>
        /// <para>Returns the logical union between two <see cref='System.Net.SocketPermission'/> instances.</para>
        /// </devdoc>
        public override IPermission Union(IPermission target) {
            // Pattern suggested by Security engine
            if (target==null) {
                return this.Copy();
            }
            SocketPermission other = target as SocketPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }
            if (m_noRestriction || other.m_noRestriction) {
                return new SocketPermission(true);
            }
            SocketPermission result = (SocketPermission)other.Copy();

            for (int i = 0; i < m_connectList.Count; i++) {
                result.AddPermission(NetworkAccess.Connect, (EndpointPermission)m_connectList[i]);
            }
            for (int i = 0; i < m_acceptList.Count; i++) {
                result.AddPermission(NetworkAccess.Accept, (EndpointPermission)m_acceptList[i]);
            }
            return result;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the logical intersection between two <see cref='System.Net.SocketPermission'/> instances.
        ///    </para>
        /// </devdoc>
        public override IPermission Intersect(IPermission target) {
            // Pattern suggested by Security engine
            if (target == null) {
                return null;
            }

            SocketPermission other = target as SocketPermission;
            if(other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            SocketPermission result;
            if (m_noRestriction) {
                result = (SocketPermission)(other.Copy());
            }
            else if (other.m_noRestriction) {
                result = (SocketPermission)(this.Copy());
            }
            else {
                result = new SocketPermission(false);
                intersectLists(m_connectList, other.m_connectList, result.m_connectList);
                intersectLists(m_acceptList, other.m_acceptList, result.m_acceptList);
            }

            // return null if resulting permission is restricted and empty
            if (!result.m_noRestriction &&
                result.m_connectList.Count == 0 && result.m_acceptList.Count == 0) {
                return null;
            }
            return result;
        }

        /// <devdoc>
        /// <para>Compares two <see cref='System.Net.SocketPermission'/> instances.</para>
        /// </devdoc>
        public override bool IsSubsetOf(IPermission target) {
            // Pattern suggested by security engine
            if (target == null) {
                return (m_noRestriction == false && m_connectList.Count == 0 && m_acceptList.Count == 0);
            }

            SocketPermission other = target as SocketPermission;
            if (other == null) {
                throw new ArgumentException(SR.GetString(SR.net_perm_target), "target");
            }

            if (other.IsUnrestricted()) {
                return true;
            } else if (this.IsUnrestricted()) {
                return false;
            } else if (this.m_acceptList.Count + this.m_connectList.Count ==0) {
                return true;
            } else if (other.m_acceptList.Count + other.m_connectList.Count ==0) {
                return false;
            }

            bool result = false;
            try {
                if (FindSubset(m_connectList, other.m_connectList) &&
                    FindSubset(m_acceptList, other.m_acceptList)) {
                    result = true;
                }
            }
            finally {
                //  This is around a back door into DNS
                //  Security engine will call isSubsetOf and probably have
                //  DNS permission asserted. We call DNS resolve.
                //  Before return do cleanup of DNS results.

                //  Only "this" needs cleanup, the policy object is not available for
                //  an application to look at.
                this.CleanupDNS();
            }

            return result;
        }

        //
        //This is to cleanup DNS resolution results
        //
        private void CleanupDNS() {
            foreach(EndpointPermission e in m_connectList) {
                //DNS hostnames never produce 'cached=true'
                if (e.cached) {
                    continue;
                }
                e.address = null;
            }

            foreach(EndpointPermission e in m_acceptList) {
                //DNS hostnames never produce 'cached=true'
                if (e.cached) {
                    continue;
                }
                e.address = null;
            }
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

            //
            // Start recovering the state from XML encoding
            //

            initialize();


            String str = securityElement.Attribute("Unrestricted");

            if (str != null) {
                m_noRestriction = (0 == string.Compare( str, "true", StringComparison.OrdinalIgnoreCase ));
                if(m_noRestriction)
                    return;
            }

            m_noRestriction = false;
            m_connectList = new ArrayList();
            m_acceptList = new ArrayList();

            SecurityElement et = securityElement.SearchForChildByTag("ConnectAccess");
            if (et != null) {
                ParseAddXmlElement(et, m_connectList, "ConnectAccess, ");
            }
            et = securityElement.SearchForChildByTag("AcceptAccess");
            if (et != null) {
                ParseAddXmlElement(et, m_acceptList, "AcceptAccess, ");
            }
        }

        private static void ParseAddXmlElement(SecurityElement et, ArrayList listToAdd, string accessStr) {

            foreach(SecurityElement uriElem in et.Children) {
                if (uriElem.Tag.Equals("ENDPOINT")) {
                    Hashtable attributes = uriElem.Attributes;
                    string tmpStr;

                    try {
                        tmpStr = attributes["host"] as string;
                    }
                    catch{
                        tmpStr = null;
                    }

                    if (tmpStr == null) {
                        throw new ArgumentNullException(accessStr + "host");
                    }
                    string host = tmpStr;

                    try {
                        tmpStr = attributes["transport"] as string;
                    }
                    catch{
                        tmpStr = null;
                    }
                    if (tmpStr == null) {
                        throw new ArgumentNullException(accessStr + "transport");
                    }
                    TransportType transport;
                    try {
                        transport = (TransportType) Enum.Parse(typeof(TransportType), tmpStr, true);
                    }
                    catch (Exception exception) {
                        if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
		                    throw;
	                    }
                        throw new ArgumentException(accessStr + "transport", exception);
                    }

                    try {
                        tmpStr = attributes["port"] as string;
                    }
                    catch{
                        tmpStr = null;
                    }
                    if (tmpStr == null) {
                        throw new  ArgumentNullException(accessStr + "port");
                    }
                    if (string.Compare(tmpStr, "All", StringComparison.OrdinalIgnoreCase ) == 0) {
                        tmpStr = "-1";
                    }
                    int port;
                    try {
                        port = Int32.Parse(tmpStr, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception exception) {
                        if (exception is ThreadAbortException || exception is StackOverflowException || exception is OutOfMemoryException) {                                       
		                    throw;
	                    }
                        throw new ArgumentException(SR.GetString(SR.net_perm_invalid_val, accessStr + "port", tmpStr), exception);
                    }

                    if (!ValidationHelper.ValidateTcpPort(port) && port != SocketPermission.AllPorts) {
                        throw new ArgumentOutOfRangeException("port", port, SR.GetString(SR.net_perm_invalid_val, accessStr + "port", tmpStr));
                    }


                    listToAdd.Add(new EndpointPermission(host, port , transport));
                }
                else {
                    // improper tag found, just ignore
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override SecurityElement ToXml() {

            SecurityElement securityElement = new SecurityElement( "IPermission" );

            securityElement.AddAttribute("class", this.GetType().FullName + ", " + this.GetType().Module.Assembly.FullName.Replace( '\"', '\'' ));
            securityElement.AddAttribute("version", "1");

            if (!IsUnrestricted()) {
                if (m_connectList.Count > 0) {

                    SecurityElement permList = new SecurityElement("ConnectAccess");
                    foreach(EndpointPermission permission in m_connectList) {
                        SecurityElement endpoint = new SecurityElement("ENDPOINT");
                        endpoint.AddAttribute("host", permission.Hostname);
                        endpoint.AddAttribute("transport", permission.Transport.ToString());
                        endpoint.AddAttribute("port",   permission.Port != AllPorts?
                                                        permission.Port.ToString(NumberFormatInfo.InvariantInfo): "All");
                        permList.AddChild(endpoint);
                    }
                    securityElement.AddChild(permList);
                }

                if (m_acceptList.Count > 0) {

                    SecurityElement permList = new SecurityElement("AcceptAccess");
                    foreach(EndpointPermission permission in m_acceptList) {
                        SecurityElement endpoint = new SecurityElement("ENDPOINT");
                        endpoint.AddAttribute("host", permission.Hostname);
                        endpoint.AddAttribute("transport", permission.Transport.ToString());
                        endpoint.AddAttribute("port",   permission.Port != AllPorts?
                                                        permission.Port.ToString(NumberFormatInfo.InvariantInfo): "All");
                        permList.AddChild(endpoint);
                    }
                    securityElement.AddChild(permList);
                }
            }
            else {
                securityElement.AddAttribute("Unrestricted", "true");
            }
            return securityElement;
        }

        private void initialize() {
            m_noRestriction = false;
            m_connectList = new ArrayList();
            m_acceptList = new ArrayList();
        }

        private static void intersectLists(ArrayList A, ArrayList B, ArrayList result) {
            // The optimization is done according to the following truth
            // (A|B|C) intersect (B|C|E|D)) == B|C|(A inter E)|(A inter D)
            //
            // We also check on any duplicates in the result


            bool[] aDone=new bool[A.Count];            //used to avoid duplicates in result
            bool[] bDone=new bool[B.Count];
            int ia=0;
            int ib=0;
            // Round 1st
            // Getting rid of same permissons in the input arrays (assuming X /\ X = X)
            foreach (EndpointPermission a in  A) {
                ib = 0;
                foreach (EndpointPermission b in  B) {
                    // check to see if b is in the result already
                    if (!bDone[ib]) {
                        //if both elements are the same, copy it into result
                        if (a.Equals(b)) {
                            result.Add(a);
                            aDone[ia]=bDone[ib]=true;
                            //since permissions are ORed we can break and go to the next A
                            break;
                        }
                    }
                    ++ib;
                } //foreach b in B
                ++ia;
            } //foreach a in A

            ia = 0;
            // Round second
            // Grab only intersections of objects not found in both A and B
            foreach (EndpointPermission a in  A) {

                if (!aDone[ia]) {
                    ib = 0;
                    foreach(EndpointPermission b in B) {
                        if (!bDone[ib]) {
                            EndpointPermission intesection = a.Intersect(b);
                            if (intesection != null) {
                                bool found = false;
                                // check to see if we already have the same result
                                foreach (EndpointPermission  res in result) {
                                    if (res.Equals(intesection)) {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) {
                                    result.Add(intesection);
                                }
                            }
                        } //!Done[ib]
                        ++ib;
                    } //foreach b in B
                } //!Done[ia]
                ++ia;
            } //foreach a in A
        }

    }// class SocketPermission


    /// <devdoc>
    ///       Represents an element of SocketPermission object contents.
    /// </devdoc>
    [Serializable]
    public class EndpointPermission {

        //
        // <





        internal String hostname;
        internal int port;
        internal TransportType transport;
        internal bool wildcard;
        internal IPAddress[] address;
        internal bool cached = false;

        private static char[] DotSeparator = new char[] {'.'};
        private const String encSeperator = "#";

        /// <devdoc>
        ///    <para>
        ///       Returns the hostname part of EndpointPermission object
        ///    </para>
        /// </devdoc>
        public String           Hostname        { get {return hostname;}}

        /// <devdoc>
        ///    <para>
        ///       Returns the transport of EndpointPermission object
        ///    </para>
        /// </devdoc>
        public TransportType    Transport       { get {return transport;}}

        /// <devdoc>
        ///    <para>
        ///       Returns the Port part of EndpointPermission object
        ///    </para>
        /// </devdoc>
        public int              Port            { get {return port;}}
        //
        // <





        internal EndpointPermission(String epname, int port, TransportType trtype) {

            if (CheckEndPointName(epname) == EndPointType.Invalid) {
                throw new ArgumentException(SR.GetString(SR.net_perm_epname, epname), "epname");
            }
            if (!ValidationHelper.ValidateTcpPort(port) && port != SocketPermission.AllPorts) {
                throw new ArgumentOutOfRangeException("port", SR.GetString(SR.net_perm_invalid_val, "Port", port.ToString(NumberFormatInfo.InvariantInfo)));
            }

            hostname = epname;
            this.port = port;
            transport = trtype;
            wildcard = false;
        }

        //
        // This is ONLY a syntatic check on equality, hostnames are compared as strings!
        //
        public override bool Equals(object obj) {

            EndpointPermission ep = (EndpointPermission)obj;

            if (String.Compare(hostname, ep.hostname, StringComparison.OrdinalIgnoreCase ) != 0) {
                return false;
            }
            if (port != ep.port) {
                return false;
            }
            if (transport != ep.transport) {
                return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        //
        // <


        internal bool IsDns {
            get {
                if (IsValidWildcard) {
                    return false;
                }
                return CheckEndPointName(hostname) == EndPointType.DnsOrWildcard;
            }
        }


        //
        // In this version wildcards are only allowed over IP ranges
        // not DNS names. For example "*.microsoft.com" is not allowed
        // A valid wildcard will have exactly three periods
        //IPv6 wildcards are NOT supported
        //
        private bool IsValidWildcard {
            get {

                int len = hostname.Length;

                //
                // Check minimum length
                //

                if (len < 3) {
                    return false;
                }

                //
                // First and last characters cannot be periods
                //

                if ((hostname[0] == '.') || (hostname[len - 1] == '.')) {
                    return false;
                }

                int dotCount = 0;
                int anyCount = 0;

                for (int i = 0; i < hostname.Length; i++) {
                    if (hostname[i] == '.') {
                        dotCount++;
                    }
                    else if (hostname[i] == '*') {
                        ++anyCount;
                    }
                    else if (!Char.IsDigit(hostname[i])) {  // Not a digit?
                        return false;                       // Reject wildcard
                    }
                }
                return (dotCount == 3) && (anyCount > 0);
            }
        }

        internal bool MatchAddress(EndpointPermission e) {

            // For Asp.Net config we made it valid empty string in a hostname,
            // but it will match to nothing.
            if(this.Hostname.Length == 0 || e.Hostname.Length == 0) {
                return false;
            }

            //
            // This is a fix for INADDR_ANY in Bind()
            // if this.Hostname == "0.0.0.0" then it matches only to e.Hostname="*.*.*.*"
            //
            // The reason is to not pass "0.0.0.0" into Resolve()
            if(this.Hostname.Equals("0.0.0.0"))
            {
                if(e.Hostname.Equals("*.*.*.*") || e.Hostname.Equals("0.0.0.0"))
                    return true;
                return false;
            }

            if (IsDns && e.IsDns) {

                //
                // <



                return (String.Compare(hostname, e.hostname, StringComparison.OrdinalIgnoreCase ) == 0);
            }
            Resolve();
            e.Resolve();

            //
            // if Resolve() didn't work for some reason then we're out of luck
            //

            if (((address == null) && !wildcard) || ((e.address == null) && !e.wildcard)) {
                return false;
            }

            //
            // try matching IP addresses against other wildcard address(es) or
            // wildcard
            //

            if (this.wildcard && !e.wildcard) {
                return false;                           // as a wildcard I cannot be subset of a host.

            }
            else if (e.wildcard) {
                if (this.wildcard) {
                    // check against my _wildcard_
                    if (MatchWildcard(e.hostname)) {
                        return true;
                    }
                }
                else {
                    // check against my _addresses_
                    for (int i = 0; i < address.Length; ++i) {
                        if (e.MatchWildcard(address[i].ToString())) {
                            return true;
                        }
                    }
                }
            } else {
                //both are _not_ wildcards
                for (int i = 0; i < address.Length; ++i) {
                    for (int j = 0; j < e.address.Length; ++j) {
                        if (address[i].Equals(e.address[j])) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal bool MatchWildcard(string str) {

            string [] wcPieces = hostname.Split(DotSeparator);
            string [] strPieces = str.Split(DotSeparator);

            if ((strPieces.Length != 4) || (wcPieces.Length != 4)) {
                return false;
            }
            for (int i = 0; i < 4; i++) {
                if ((strPieces[i] != wcPieces[i]) && (wcPieces[i] != "*")) {
                    return false;
                }
            }
            return true;
        }

        internal void Resolve() {

            //
            // if we already resolved this name then don't do it again
            //

            if (cached) {
                return;
            }

            //
            // IP wildcards are not resolved
            //

            if (wildcard) {
                return;
            }

            //
            // IP addresses with wildcards are allowed in permissions
            //

            if (IsValidWildcard) {
                wildcard = true;
                cached = true;
                return;
            }

            //
            // Check if the permission was specified as numeric IP.
            //
            IPAddress ipaddr;
            if (IPAddress.TryParse(hostname, out ipaddr))
            {
                address = new IPAddress[1];
                address[0] = ipaddr;
                cached = true;
                return;
            }

            //
            // Not numeric: use GetHostByName to determine addresses
            //
            try {
                IPHostEntry ipHostEntry;
                if (Dns.TryInternalResolve(hostname, out ipHostEntry)) {
                    address = ipHostEntry.AddressList;
                }

                // NB: It never caches DNS responses
                //

            }
            catch (SecurityException) {
                throw;
            }
            catch {
                // ignore second exception
            }
        }

        internal bool SubsetMatch(EndpointPermission e) {
            return ((transport == e.transport) || (e.transport == TransportType.All))
                    && ((port == e.port) || (e.port == SocketPermission.AllPorts) || port == SocketPermission.AnyPort)
                    && MatchAddress(e);
        }

        public override String ToString() {
            return hostname + encSeperator + port + encSeperator + ((int)transport).ToString(NumberFormatInfo.InvariantInfo);
        }

        internal EndpointPermission Intersect(EndpointPermission E) {

            String commonName=null;
            TransportType commonTransport;
            int commonPort;

            //
            // Look at the transport
            //

            if (transport == E.transport) {           // same transport
                commonTransport = transport;
            }
            // NO: check if one of the permissions authorize all transports
            else if (transport == TransportType.All) {
                commonTransport = E.transport;
            }
            else if (E.transport == TransportType.All) {
                commonTransport = transport;
            }
            else {   // transport dont match-- intersection is empty
                return null;
            }

            //
            // Determine common port
            //

            if (port == E.port) {
                commonPort = port;
            }
            else if (port == SocketPermission.AllPorts) {
                commonPort = E.port;
            }
            else if (E.port == SocketPermission.AllPorts) {
                commonPort = port;
            }
            else {
                return null;
            }

            //Work out common hostname part
            //
            // This is a fix for INADDR_ANY in Bind()
            // if this.Hostname == "0.0.0.0" then it matches only to e.Hostname="*.*.*.*"
            //
            // The reason is to not pass "0.0.0.0" into Resolve()
            if(this.Hostname.Equals("0.0.0.0"))
            {
                if(E.Hostname.Equals("*.*.*.*") || E.Hostname.Equals("0.0.0.0"))
                    commonName = this.Hostname;//i.e. 0.0.0.0
                else
                    return null;
            }
            else if(E.Hostname.Equals("0.0.0.0"))
            {
                if(this.Hostname.Equals("*.*.*.*") || this.Hostname.Equals("0.0.0.0"))
                    commonName = E.Hostname; //i.e. 0.0.0.0
                else
                    return null;
            }
            else if (IsDns && E.IsDns) {
                //
                // If both are DNS names we compare names as strings
                //
                if(String.Compare(hostname, E.hostname, StringComparison.OrdinalIgnoreCase ) != 0) {
                    return null;
                }
                else {
                    commonName = hostname;
                }
            }
            else
            {
                Resolve();
                E.Resolve();
                //after this step we got both clases updated with valid
                //wildcard and address members. It's safe now to access those members directly

                //
                // if Resolve() didn't work for some reason then we're out of luck
                //

                if (((address == null) && !wildcard) || ((E.address == null) && !E.wildcard)) {
                    return null;
                }


                //
                // Find intersection of address lists
                if(wildcard && E.wildcard) {
                    string [] wcPieces = hostname.Split(DotSeparator);
                    string [] strPieces = E.hostname.Split(DotSeparator);
                    string  result="";

                    if ((strPieces.Length != 4) || (wcPieces.Length != 4)) {
                        return null;
                    }
                    for (int i = 0; i < 4; i++) {
                        if(i != 0) {
                            result+=".";
                        }
                        if (strPieces[i] == wcPieces[i]) {
                            result+=strPieces[i];
                        }
                        else
                        if (strPieces[i] == "*") {
                            result+=wcPieces[i];
                        }
                        else
                        if (wcPieces[i] == "*") {
                            result+=strPieces[i];
                        }
                        else
                            return null;
                    }
                    commonName = result;
                }else
                if (wildcard) {                                                 //if ME is a wildcard
                    //
                    //
                    // Check for wildcard IP matching
                    //
                    for (int i = 0; i < E.address.Length; ++i) {
                        if (MatchWildcard(E.address[i].ToString())) {
                            commonName = E.hostname;    //SHE fits into my wildcard
                            break;
                        }
                    }
                }
                else if (E.wildcard) {                                   //if SHE is a wildcard
                    for (int i = 0; i < address.Length; ++i) {
                        if (E.MatchWildcard(address[i].ToString())) {
                            commonName = hostname;      //ME fit  into her wildcard
                            break;
                        }
                    }
                }
                else
                {
                    //
                    // Not wildcard: check aginst  IP addresses list
                    //

                    if (address == E.address) {                 // they both are NOT null (already checked)
                        commonName = hostname;
                    }

                    //
                    // Search the IP addresses for match
                    //
                    for (int i = 0; commonName == null && i < address.Length; i++) {
                        for (int k = 0; k < E.address.Length; k++) {
                            if (address[i].Equals(E.address[k])) {
                                commonName = hostname;
                                break;
                            }
                        }
                    }
                }
                if(commonName == null) {
                    return null;
                }
            }

            return new EndpointPermission(commonName, commonPort, commonTransport);
        }
/*
FROM RFC 952
------------
ASSUMPTIONS
1   A "name" (Net, Host, Gateway, or Domain name) is a text string up
    to 24 characters drawn from the alphabet (A-Z), digits (0-9), minus sign (-), and period (.).
    Note that periods are only allowed when they serve to delimit components of "domain style names".
    (See RFC-921, "Domain Name System Implementation Schedule", for background).
    No blank or space characters are permitted as part of a name.
    No distinction is made between upper and lower case.
    The first character must be an alpha character.
    The last character must not be a minus sign or period.
    Single character names or nicknames are not allowed.

    Implementaion below is relaxed in terms of:
    - Hostname may start with a digit (as per RFC1123 )
    - Hostname may contain '_' character (historical Inet issue)
    - Hostname may be a single-character string (historical Inet issue)
    - Hostname may contain '*' as a wildcard for an EndPointPermission
    - Hostname may be empty (to support config templates)
    - Hostname may be an IPv6 string comprised of A-F, 0-9, '.', ':', and '%' chars
*/
    private enum EndPointType {
            Invalid,
            IPv6,
            DnsOrWildcard,
            IPv4
    };

    private static EndPointType CheckEndPointName(string name) {
        if (name == null) {
            return EndPointType.Invalid;
        }
        bool isIPv6       = false;
        bool isDnsOrWC    = false;
        bool isHexLetter  = false;
        for(int i=0; i < name.Length; ++i) {
            char ch = name[i];
            switch(ch) {
            case '.':   //note _all_ dots name is an error
                        continue;
            case '-':   //if _all_ chars are those we call Dns (to confirm error)
            case '_':
            case '*':   isDnsOrWC = true;
                        continue;
            case ':':
            case '%':   isIPv6 = true;
                        continue;
            default:    break;
            }

            //Check on letters but NOT hex digits
            if ((ch > 'f' && ch <= 'z') || (ch > 'F' && ch <= 'Z')) {
                isDnsOrWC = true;
                continue;
            }
            //Check on HEX letters
            if((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')) {
                isHexLetter = true;
                continue;
            }
            //Here only digits left (others are invalid)
            if (!(ch >= '0' && ch <= '9'))
                return EndPointType.Invalid;
        }

        // The logic is (solely for the purpose of SocketPermssion class)
        //  isIPv6 && isDnsOrWC   = EndPointType.Invalid
        //  isIPv6 && !isDnsOrWC  = EndPointType.IPv6
        //  !isIPv6 && isDnsOrWC  = EndPointType.DnsOrWildcard
        //  !isIPv6 && !isDnsOrWC && isHexLetter = EndPointType.DnsOrWildcard;
        //  else = EndPointType.IPv4
        return isIPv6 ? (isDnsOrWC? EndPointType.Invalid: EndPointType.IPv6)
                      : (isDnsOrWC? EndPointType.DnsOrWildcard :
                                    isHexLetter? EndPointType.DnsOrWildcard :EndPointType.IPv4);
    }


    } // class EndpointPermission


} // namespace System.Net
