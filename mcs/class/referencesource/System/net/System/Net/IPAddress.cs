//------------------------------------------------------------------------------
// <copyright file="IPAddress.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Net.Sockets;
    using System.Globalization;
    using System.Text;

    /// <devdoc>
    ///    <para>Provides an internet protocol (IP) address.</para>
    /// </devdoc>
    [Serializable]
    public class IPAddress {

        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);
        public static readonly  IPAddress Loopback = new IPAddress(0x000000000100007F);
        public static readonly  IPAddress Broadcast = new IPAddress(0x00000000FFFFFFFF);
        public static readonly  IPAddress None = Broadcast;

        internal const long LoopbackMask = 0x00000000000000FF;

        //
        // IPv6 Changes: make this internal so other NCL classes that understand about
        //               IPv4 and IPv4 can still access it rather than the obsolete property.
        //
        internal long m_Address;

        [NonSerialized]
        internal string m_ToString;

        public static readonly IPAddress IPv6Any      = new IPAddress(new byte[]{ 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },0);
        public static readonly IPAddress IPv6Loopback = new IPAddress(new byte[]{ 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1 },0);
        public static readonly IPAddress IPv6None     = new IPAddress(new byte[]{ 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },0);

        /// <devdoc>
        ///   <para>
        ///     Default to IPv4 address
        ///   </para>
        /// </devdoc>
        private AddressFamily m_Family       = AddressFamily.InterNetwork;
        private ushort[]      m_Numbers      = new ushort[NumberOfLabels];
        private long          m_ScopeId      = 0;                             // really uint !
        private int           m_HashCode     = 0;

        internal const int IPv4AddressBytes =  4;
        internal const int IPv6AddressBytes = 16;

        internal const int NumberOfLabels = IPv6AddressBytes / 2;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.IPAddress'/>
        ///       class with the specified
        ///       address.
        ///    </para>
        /// </devdoc>
        public IPAddress(long newAddress) {
            if (newAddress<0 || newAddress>0x00000000FFFFFFFF) {
                throw new ArgumentOutOfRangeException("newAddress");
            }
            m_Address = newAddress;
        }


        /// <devdoc>
        ///    <para>
        ///       Constructor for an IPv6 Address with a specified Scope.
        ///    </para>
        /// </devdoc>
        public IPAddress(byte[] address,long scopeid) {

            if (address==null) {
                throw new ArgumentNullException("address");
            }

            if(address.Length != IPv6AddressBytes){
                throw new ArgumentException(SR.GetString(SR.dns_bad_ip_address), "address");
            }

            m_Family = AddressFamily.InterNetworkV6;

            for (int i = 0; i < NumberOfLabels; i++) {
                m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
            }

            //
            // Consider: Since scope is only valid for link-local and site-local
            //           addresses we could implement some more robust checking here
            //
            if ( scopeid < 0 || scopeid > 0x00000000FFFFFFFF ) {
                throw new ArgumentOutOfRangeException("scopeid");
            }

            m_ScopeId = scopeid;
        }
        //
        private IPAddress(ushort[] address, uint scopeid)
        {
            m_Family = AddressFamily.InterNetworkV6;
            m_Numbers = address;
            m_ScopeId = scopeid;
        }


        /// <devdoc>
        ///    <para>
        ///       Constructor for IPv4 and IPv6 Address.
        ///    </para>
        /// </devdoc>
        public IPAddress(byte[] address){
            if (address==null) {
                throw new ArgumentNullException("address");
            }
            if (address.Length != IPv4AddressBytes && address.Length != IPv6AddressBytes)
            {
                throw new ArgumentException(SR.GetString(SR.dns_bad_ip_address), "address");
            }

            if (address.Length == IPv4AddressBytes)
            {
                m_Family = AddressFamily.InterNetwork;
                m_Address = ((address[3] << 24 | address[2] <<16 | address[1] << 8| address[0]) & 0x0FFFFFFFF);
            }
            else {
                m_Family = AddressFamily.InterNetworkV6;

                for (int i = 0; i < NumberOfLabels; i++) {
                    m_Numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
                }
            }
        }

        //
        // we need this internally since we need to interface with winsock
        // and winsock only understands Int32
        //
        internal IPAddress(int newAddress) {
            m_Address = (long)newAddress & 0x00000000FFFFFFFF;
        }



        /// <devdoc>
        /// <para>Converts an IP address string to an <see cref='System.Net.IPAddress'/>
        /// instance.</para>
        /// </devdoc>
        public static bool TryParse(string ipString, out IPAddress address) {
            address = InternalParse(ipString,true);
            return (address != null);
        } 
        
        public static IPAddress Parse(string ipString) {
            return InternalParse(ipString,false);
        }
        
        private static IPAddress InternalParse(string ipString, bool tryParse) {
            if (ipString == null) {
                if (tryParse) {
                    return null;
                }
                throw new ArgumentNullException("ipString");
            }

            //
            // IPv6 Changes: Detect probable IPv6 addresses and use separate
            //               parse method.
            //
            if (ipString.IndexOf(':') != -1 ) {

#if !FEATURE_PAL
                //
                // If the address string contains the colon character
                // then it can only be an IPv6 address. Use a separate
                // parse method to unpick it all. Note: we don't support
                // port specification at the end of address and so can
                // make this decision.
                //
                // We need to make sure that Socket is initialized for this
                // call !
                //
                SocketException e = null;
                long   scope = 0;
                if(Socket.OSSupportsIPv6)
                {
                    byte[] bytes = new byte[IPv6AddressBytes];
                    SocketAddress saddr = new SocketAddress(AddressFamily.InterNetworkV6, SocketAddress.IPv6AddressSize);

                    SocketError errorCode =
                        UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(
                            ipString,
                            AddressFamily.InterNetworkV6,
                            IntPtr.Zero,
                            saddr.m_Buffer,
                            ref saddr.m_Size );

                    if (errorCode ==SocketError.Success)
                    {
                        //
                        // We have to set up the address by hand now, to avoid initialization
                        // recursion problems that we get if we use IPEndPoint.Create.
                        //
                        for (int i=0; i < IPv6AddressBytes; i++)
                        {
                            bytes[i] = saddr[i + 8];
                        }
                        //
                        // Scope
                        //
                        scope = (long)((saddr[27]<<24) + (saddr[26]<<16) + (saddr[25]<<8 ) + (saddr[24]));
                        return new IPAddress(bytes,scope);
                    }
                    
                    if(tryParse){
                        return null;
                    }
                    
                    e = new SocketException();
                }
                else
                {
                    unsafe
                    {
                        int offset = 0;
                        if (ipString[0] != '[')
                            ipString = ipString + ']'; //for Uri parser to find the terminator.
                        else
                            offset = 1;

                        int end = ipString.Length;
                        fixed (char *name = ipString)
                        {
                            if (IPv6AddressHelper.IsValidStrict(name, offset, ref end) || (end != ipString.Length))
                            {
                                ushort[] numbers = new ushort[NumberOfLabels];
                                string scopeId = null;
                                fixed (ushort *numbPtr = numbers)
                                {
                                    IPv6AddressHelper.Parse(ipString, numbPtr, 0, ref scopeId);
                                }
                                //
                                // Scope
                                //
                                if (scopeId == null || scopeId.Length == 0)
                                    return new IPAddress(numbers, 0);

                                uint result;
                                scopeId = scopeId.Substring(1);
                                if (UInt32.TryParse(scopeId, NumberStyles.None, null, out result))
                                    return new IPAddress(numbers, result);

                            }
                        }
                    }
                    if(tryParse){
                        return null;
                    }
                    e = new SocketException(SocketError.InvalidArgument);
                }

                throw new FormatException(SR.GetString(SR.dns_bad_ip_address), e);

#else // !FEATURE_PAL
                // IPv6 addresses not supported for FEATURE_PAL
                throw new SocketException(SocketError.OperationNotSupported);
#endif // !FEATURE_PAL
            }
            else
            // The new IPv4 parser is better than the native one, it can parse 0xFFFFFFFF. (It's faster too).
            {
                // App-Compat: The .NET 4.0 parser used Winsock.  When we removed this initialization in 4.5 it 
                // uncovered bugs in IIS's management APIs where they failed to initialize Winsock themselves.
                // DDCC says we need to keep this for an in place release, but to remove it in the next SxS release.
                Socket.InitializeSockets();
                ///////////////////////////

                int end = ipString.Length;
                long result;
                unsafe
                {
                    fixed (char *name = ipString)
                    {
                        result = IPv4AddressHelper.ParseNonCanonical(name, 0, ref end, true);
                    }
                }

                if (result == IPv4AddressHelper.Invalid || end != ipString.Length)
                {
                    if (tryParse)
                    {
                        return null;
                    }

                    throw new FormatException(SR.GetString(SR.dns_bad_ip_address));
                }

                // IPv4AddressHelper always returns IP address in a format that we need to reverse.
                result =  (((result & 0x000000FF) << 24) | (((result & 0x0000FF00) << 8)
                    | (((result & 0x00FF0000) >> 8) | ((result & 0xFF000000) >> 24))));

                return new IPAddress(result);
            }
        } // Parse



        /**
         * @deprecated IPAddress.Address is address family dependant, use Equals method for comparison.
         */
        /// <devdoc>
        ///     <para>
        ///         Mark this as deprecated.
        ///     </para>
        /// </devdoc>
        [Obsolete("This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons. http://go.microsoft.com/fwlink/?linkid=14202")]
        public long Address {
            get {
                //
                // IPv6 Changes: Can't do this for IPv6, so throw an exception.
                //
                //
                if ( m_Family == AddressFamily.InterNetworkV6 ) {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                else {
                    return m_Address;
                }
            }
            set {
                //
                // IPv6 Changes: Can't do this for IPv6 addresses
                if ( m_Family == AddressFamily.InterNetworkV6 ) {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                else {
                    if (m_Address!=value) {
                        m_ToString = null;
                        m_Address = value;
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>
        /// Provides a copy of the IPAddress internals as an array of bytes.
        /// </para>
        /// </devdoc>
        public byte[] GetAddressBytes() {
            byte[] bytes;
            if (m_Family == AddressFamily.InterNetworkV6 ) {
                bytes = new byte[NumberOfLabels * 2];

                int j = 0;
                for ( int i = 0; i < NumberOfLabels; i++) {
                    bytes[j++] = (byte)((this.m_Numbers[i] >> 8) & 0xFF);
                    bytes[j++] = (byte)((this.m_Numbers[i]     ) & 0xFF);
                }
            }
            else {
                bytes = new byte[IPv4AddressBytes];
                bytes[0] = (byte)(m_Address);
                bytes[1] = (byte)(m_Address >> 8);
                bytes[2] = (byte)(m_Address >> 16);
                bytes[3] = (byte)(m_Address >> 24);
            }
            return bytes;
        }

        public AddressFamily AddressFamily {
            get {
                return m_Family;
            }
        }

        /// <devdoc>
        ///    <para>
        ///        IPv6 Scope identifier. This is really a uint32, but that isn't CLS compliant
        ///    </para>
        /// </devdoc>
        public long ScopeId {
            get {
                //
                // Not valid for IPv4 addresses
                //
                if ( m_Family == AddressFamily.InterNetwork ) {
                    throw new SocketException(SocketError.OperationNotSupported);
                }

                return m_ScopeId;
            }
            set {
                //
                // Not valid for IPv4 addresses
                //
                if ( m_Family == AddressFamily.InterNetwork ) {
                    throw new SocketException(SocketError.OperationNotSupported);
                }

                //
                // Consider: Since scope is only valid for link-local and site-local
                //           addresses we could implement some more robust checking here
                //
                if ( value < 0 || value > 0x00000000FFFFFFFF) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (m_ScopeId!=value) {
                    m_Address = value;
                    m_ScopeId = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Converts the Internet address to either standard dotted quad format
        ///       or standard IPv6 representation.
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            if (m_ToString==null) {
                //
                // IPv6 Changes: generate the IPV6 representation
                //
                if ( m_Family == AddressFamily.InterNetworkV6 ) {

#if !FEATURE_PAL
                    int addressStringLength = 256;
                    StringBuilder addressString = new StringBuilder(addressStringLength);

                    if(Socket.OSSupportsIPv6)
                    {
                        SocketAddress saddr = new SocketAddress(AddressFamily.InterNetworkV6, SocketAddress.IPv6AddressSize);
                        int j = 8;
                        for ( int i = 0; i < NumberOfLabels; i++) {
                            saddr[j++] = (byte)(this.m_Numbers[i] >> 8);
                            saddr[j++] = (byte)(this.m_Numbers[i]);
                        }
                        if (m_ScopeId >0) {
                            saddr[24] = (byte)m_ScopeId;
                            saddr[25] = (byte)(m_ScopeId >> 8);
                            saddr[26] = (byte)(m_ScopeId >> 16);
                            saddr[27] = (byte)(m_ScopeId >> 24);
                        }
                        SocketError errorCode =
                            UnsafeNclNativeMethods.OSSOCK.WSAAddressToString(
                                saddr.m_Buffer,
                                saddr.m_Size,
                                IntPtr.Zero,
                                addressString,
                                ref addressStringLength);
                        if (errorCode!=SocketError.Success) {
                            throw new SocketException();
                        }
                    }
                    else
                    {
                        const string numberFormat = "{0:x4}:{1:x4}:{2:x4}:{3:x4}:{4:x4}:{5:x4}:{6}.{7}.{8}.{9}";
                        string address = String.Format(CultureInfo.InvariantCulture, numberFormat, 
                            m_Numbers[0], m_Numbers[1], m_Numbers[2], m_Numbers[3], m_Numbers[4], m_Numbers[5], 
                            ((m_Numbers[6] >> 8) & 0xFF), (m_Numbers[6] & 0xFF), 
                            ((m_Numbers[7] >> 8) & 0xFF), (m_Numbers[7] & 0xFF));
                        addressString.Append(address);

                        if (m_ScopeId != 0)
                        {
                            addressString.Append('%').Append((uint)m_ScopeId);
                        }
                    }

                    m_ToString = addressString.ToString();
#else // !FEATURE_PAL
                    // IPv6 addresses not supported for FEATURE_PAL
                    throw new SocketException(SocketError.OperationNotSupported);
#endif // !FEATURE_PAL
                }
                else {
                    unsafe {
                        const int MaxSize = 15;
                        int offset = MaxSize;
                        char* addressString = stackalloc char[MaxSize];
                        int number = (int)((m_Address>>24)&0xFF);
                        do {
                            addressString[--offset] = (char)('0' + number%10);
                            number = number/10;
                        } while (number>0);
                        addressString[--offset] = '.';
                        number = (int)((m_Address>>16)&0xFF);
                        do {
                            addressString[--offset] = (char)('0' + number%10);
                            number = number/10;
                        } while (number>0);
                        addressString[--offset] = '.';
                        number = (int)((m_Address>>8)&0xFF);
                        do {
                            addressString[--offset] = (char)('0' + number%10);
                            number = number/10;
                        } while (number>0);
                        addressString[--offset] = '.';
                        number = (int)(m_Address&0xFF);
                        do {
                            addressString[--offset] = (char)('0' + number%10);
                            number = number/10;
                        } while (number>0);
                        m_ToString = new string(addressString, offset, MaxSize-offset);
                    }
                }
            }
            return m_ToString;
        }

        public static long HostToNetworkOrder(long host) {
#if BIGENDIAN
            return host;
#else
            return (((long)HostToNetworkOrder((int)host) & 0xFFFFFFFF) << 32)
                    | ((long)HostToNetworkOrder((int)(host >> 32)) & 0xFFFFFFFF);
#endif
        }
        public static int HostToNetworkOrder(int host) {
#if BIGENDIAN
            return host;
#else
            return (((int)HostToNetworkOrder((short)host) & 0xFFFF) << 16)
                    | ((int)HostToNetworkOrder((short)(host >> 16)) & 0xFFFF);
#endif
        }
        public static short HostToNetworkOrder(short host) {
#if BIGENDIAN
            return host;
#else
            return (short)((((int)host & 0xFF) << 8) | (int)((host >> 8) & 0xFF));
#endif
        }
        public static long NetworkToHostOrder(long network) {
            return HostToNetworkOrder(network);
        }
        public static int NetworkToHostOrder(int network) {
            return HostToNetworkOrder(network);
        }
        public static short NetworkToHostOrder(short network) {
            return HostToNetworkOrder(network);
        }

        public static bool IsLoopback(IPAddress address) {
            if (address == null) {
                throw new ArgumentNullException("address");
            }
            if ( address.m_Family == AddressFamily.InterNetworkV6 ) {
                //
                // Do Equals test for IPv6 addresses
                //
                return address.Equals(IPv6Loopback);
            }
            else {
                return ((address.m_Address & IPAddress.LoopbackMask) == (IPAddress.Loopback.m_Address & IPAddress.LoopbackMask));
            }
        }

        internal bool IsBroadcast {
            get {
                if ( m_Family == AddressFamily.InterNetworkV6 ) {
                    //
                    // No such thing as a broadcast address for IPv6
                    //
                    return false;
                }
                else {
                    return m_Address==Broadcast.m_Address;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       V.Next: Determines if an address is an IPv6 Multicast address
        ///    </para>
        /// </devdoc>
        public bool IsIPv6Multicast{
            get{
                return ( m_Family == AddressFamily.InterNetworkV6 ) &&
                    ( ( this.m_Numbers[0] & 0xFF00 ) == 0xFF00 );
            }

        }

        /// <devdoc>
        ///    <para>
        ///       V.Next: Determines if an address is an IPv6 Link Local address
        ///    </para>
        /// </devdoc>
        public bool IsIPv6LinkLocal {
            get{
                return ( m_Family == AddressFamily.InterNetworkV6 ) &&
                   ( ( this.m_Numbers[0] & 0xFFC0 ) == 0xFE80 );
            }
        }

        /// <devdoc>
        ///    <para>
        ///       V.Next: Determines if an address is an IPv6 Site Local address
        ///    </para>
        /// </devdoc>
        public bool IsIPv6SiteLocal {
            get{
                return ( m_Family == AddressFamily.InterNetworkV6 ) &&
                   ( ( this.m_Numbers[0] & 0xFFC0 ) == 0xFEC0 );
            }
        }

        public bool IsIPv6Teredo {
            get {
                return ( m_Family == AddressFamily.InterNetworkV6 ) &&
                       ( this.m_Numbers[0] == 0x2001 ) &&
                       ( this.m_Numbers[1] == 0 );
            }
        }
        
        // 0:0:0:0:0:FFFF:x.x.x.x
        public bool IsIPv4MappedToIPv6 {
            get {
                if (AddressFamily != AddressFamily.InterNetworkV6) {
                    return false;
                }
                for (int i = 0; i < 5; i++) {
                    if (m_Numbers[i] != 0) {
                        return false;
                    }
                }
                return (m_Numbers[5] == 0xFFFF);
            }
        }

        internal bool Equals(object comparandObj, bool compareScopeId) {
            IPAddress comparand = comparandObj as IPAddress;

            if (comparand == null) {
                return false;
            }
            //
            // Compare families before address representations
            //
            if (m_Family != comparand.m_Family) {
                return false;
            }
            if ( m_Family == AddressFamily.InterNetworkV6 ) {
                //
                // For IPv6 addresses, we must compare the full 128bit
                // representation.
                //
                for ( int i = 0; i < NumberOfLabels; i++) {
                    if (comparand.m_Numbers[i] != this.m_Numbers[i])
                        return false;
                }
                //
                // In addition, the scope id's must match as well
                //
                if (comparand.m_ScopeId == this.m_ScopeId)
                    return true;
                else
                    return (compareScopeId? false : true);
            }
            else {
                //
                // For IPv4 addresses, compare the integer representation.
                //
                return comparand.m_Address == this.m_Address;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Compares two IP addresses.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object comparand) {
            return Equals(comparand, true);
        }

        public override int GetHashCode() {
            //
            // For IPv6 addresses, we cannot simply return the integer
            // representation as the hashcode. Instead, we calculate
            // the hashcode from the string representation of the address.
            //
            if ( m_Family == AddressFamily.InterNetworkV6 ) {
                if ( m_HashCode == 0 )
                    m_HashCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(ToString());

                return m_HashCode;
            }
            else {
                //
                // For IPv4 addresses, we can simply use the integer
                // representation.
                //
                return unchecked((int)m_Address);
            }
        }

        // For security, we need to be able to take an IPAddress and make a copy that's immutable and not derived.
        internal IPAddress Snapshot()
        {
            switch (m_Family)
            {
                case AddressFamily.InterNetwork:
                    return new IPAddress(m_Address);

                case AddressFamily.InterNetworkV6:
                    return new IPAddress(m_Numbers, (uint) m_ScopeId);
            }

            throw new InternalException();
        }

        // IPv4 192.168.1.1 maps as ::FFFF:192.168.1.1
        public IPAddress MapToIPv6()
        {
            if (AddressFamily == AddressFamily.InterNetworkV6)
            {
                return this;
            }

            ushort[] labels = new ushort[IPAddress.NumberOfLabels];
            labels[5] = 0xFFFF;
            labels[6] = (ushort)(((m_Address & 0x0000FF00) >> 8) | ((m_Address & 0x000000FF) << 8));
            labels[7] = (ushort)(((m_Address & 0xFF000000) >> 24) | ((m_Address & 0x00FF0000) >> 8));
            return new IPAddress(labels, 0);
        }

        // Takes the last 4 bytes of an IPv6 address and converts it to an IPv4 address.
        // This does not restrict to address with the ::FFFF: prefix because other types of 
        // addresses display the tail segments as IPv4 like Terado.
        public IPAddress MapToIPv4()
        {
            if (AddressFamily == AddressFamily.InterNetwork)
            {
                return this;
            }

            // Cast the ushort values to a uint and mask with unsigned literal before bit shifting.
            // Otherwise, we can end up getting a negative value for any IPv4 address that ends with
            // a byte higher than 127 due to sign extension of the most significant 1 bit.
            long address = ((((uint)m_Numbers[6] & 0x0000FF00u) >> 8) | (((uint)m_Numbers[6] & 0x000000FFu) << 8)) |
                    (((((uint)m_Numbers[7] & 0x0000FF00u) >> 8) | (((uint)m_Numbers[7] & 0x000000FFu) << 8)) << 16);

            return new IPAddress(address);
        }
    } // class IPAddress
} // namespace System.Net
