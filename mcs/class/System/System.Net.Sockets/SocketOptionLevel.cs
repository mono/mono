// SocketOptionLevel.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net.Sockets {


	/// <summary>
	/// <para> Specifies the option level associated with
	///  the <see cref="T:System.Net.Sockets.SocketOptionName" /> used in the <see cref="M:System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Int32)" /> and <see cref="M:System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName)" /> methods
	///  of the <see cref="T:System.Net.Sockets.Socket" /> class.
	///  </para>
	/// </summary>
	/// <remarks>
	/// <para>Some socket options apply only to specific
	///  protocols while others apply to all types. Members of this enumeration specify which protocol applies to
	///  a specific socket option. </para>
	/// </remarks>
	public enum SocketOptionLevel {

		/// <summary><para>Specifies that members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration are not specific to a particular protocol. </para><para>The following table lists these members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration.</para><list type="table"><listheader><term>SocketOptionName</term><description>Description</description></listheader><item><term> Broadcast</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates broadcast messages are allowed to be sent to the socket. </description></item><item><term> Debug</term><description>A <see cref="T:System.Boolean" />
		///    where <see langword="true" /> indicates to record debugging information.</description></item><item><term> DontLinger</term><description>A <see cref="T:System.Boolean" />
		/// where <see langword="true" /> indicates to close the socket without lingering.</description></item><item><term> DontRoute</term><description>A <see cref="T:System.Boolean" />
		/// where <see langword="true" /> indicates not to route
		/// data; <see langword="false" /> indicates to send data directly to interface addresses.</description></item><item><term> Error</term><description>A
		///    <see cref="T:System.Int32" /> that
		///       contains the error code associated with the last socket error. The error
		///       code is cleared by this option. This option is read-only.</description></item><item><term> KeepAlive</term><description>A <see cref="T:System.Boolean" />
		/// where <see langword="true" /> (the default) indicates to enable keep-alives, which allows a connection to remain open after a request.</description></item><item><term> OutOfBandInline</term><description>A <see cref="T:System.Boolean" />
		/// where <see langword="true" /> indicates to receive out-of-band data in the normal data stream.</description></item><item><term> ReceiveBuffer</term><description>A <see cref="T:System.Int32" /> that specifies the
		///    total per-socket buffer space reserved for receives. This is unrelated to
		///    the maximum message size or the size of a TCP window.</description></item><item><term> ReceiveTimeout</term><description>A
		///    <see cref="T:System.Int32" /> that
		///       specifies the maximum time, in milliseconds, the <see cref="M:System.Net.Sockets.Socket.Receive(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" /> and <see cref="M:System.Net.Sockets.Socket.ReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@)" /> methods will block when attempting to
		///       receive data. If data is not received within this
		///       time, a <see cref="T:System.Net.Sockets.SocketException" /> exception is thrown.</description></item><item><term> ReuseAddress</term><description>A <see cref="T:System.Boolean" />
		/// where <see langword="true" /> allows the socket to be bound to an address that is already in use.</description></item><item><term> SendBuffer</term><description>A <see cref="T:System.Int32" /> that specifies the total per-socket buffer space reserved for sends. This is unrelated to the maximum message size or the size of a TCP window.</description></item><item><term> SendTimeout</term><description>A
		///    <see cref="T:System.Int32" /> that
		///       specifies the maximum time, in milliseconds, the <see cref="M:System.Net.Sockets.Socket.Send(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" /> and <see cref="M:System.Net.Sockets.Socket.SendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint)" /> methods will block when attempting to
		///       send data. If data is not sent within this time, a <see cref="T:System.Net.Sockets.SocketException" /> exception is thrown.</description></item><item><term> Type</term><description><para>One of the values defined in the <see cref="T:System.Net.Sockets.SocketType" /> enumeration. This option is 
		///       read-only.</para></description></item></list></summary>
		Socket = 65535,

		/// <summary><para> Specifies that members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration apply to
		///    Internet Protocol (IP).
		///    </para><para>The following table lists these members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration.</para><list type="table"><listheader><term>SocketOptionName</term><description>Description</description></listheader><item><term> HeaderIncluded</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates
		///          the application is providing the IP header for outgoing
		///          datagrams. </description></item><item><term> IPOptions</term><description>Specifies IP options to be inserted into outgoing
		///          datagrams.</description></item><item><term> IpTimeToLive</term><description>A <see cref="T:System.Int32" /> that specifies the
		///       time-to-live for datagrams. The time-to-live designates the number of
		///       networks on which the datagram is allowed to travel
		///       before being discarded by a
		///       router.</description></item><item><term> MulticastInterface</term><description>Sets the interface for outgoing multicast
		///       packets.</description></item><item><term> MulticastLoopback</term><description>A
		///    <see cref="T:System.Boolean" /> where <see langword="true" /> enables multicast
		///       loopback. </description></item><item><term> MulticastTimeToLive</term><description>A <see cref="T:System.Int32" /> that specifies the
		///    time-to-live for multicast datagrams. </description></item><item><term> TypeOfService</term><description>A <see cref="T:System.Int32" /> that specifies the
		///    type of service field in the IP header. </description></item><item><term> UseLoopback</term><description>A
		///    <see cref="T:System.Boolean" /> where <see langword="true" /> indicates Bypass hardware when
		///       possible. </description></item></list></summary>
		IP = 0,

		/// <summary><para>Specifies that members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration apply to Transmission Control Protocol (TCP). </para><para>The following table lists these members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration.</para><list type="table"><listheader><term>SocketOptionName</term><description>Description</description></listheader><item><term> BsdUrgent</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates to use urgent data as
		///          defined in RFC-1222. Once set, this option cannot be turned off.</description></item><item><term> Expedited</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates to use expedited data as defined in RFC-1222.
		///          Once set, this option cannot be turned off.</description></item><item><term> NoDelay</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates to disable the Nagle algorithm for send
		///          coalescing.</description></item></list></summary>
		Tcp = 6,

		/// <summary><para>Specifies that members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration apply to User Datagram Protocol
		///    (UDP). </para><para>The following table lists these members of the <see cref="T:System.Net.Sockets.SocketOptionName" /> enumeration.</para><list type="table"><listheader><term>SocketOptionName</term><description>Description</description></listheader><item><term> ChecksumCoverage</term><description>UDP checksum coverage.</description></item><item><term> NoChecksum</term><description>A
		///       <see cref="T:System.Boolean" /> where <see langword="true" /> indicates to send UDP datagrams with the checksum set to zero.</description></item></list></summary>
		Udp = 17,
	} // SocketOptionLevel

} // System.Net.Sockets
