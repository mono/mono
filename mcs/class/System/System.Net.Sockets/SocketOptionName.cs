// SocketOptionName.cs
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
	/// <para> Specifies option names for use in the <see cref="M:System.Net.Sockets.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName,System.Int32)" qualify="true" /> and <see cref="M:System.Net.Sockets.Socket.GetSocketOption(System.Net.Sockets.SocketOptionLevel,System.Net.Sockets.SocketOptionName)" qualify="true" /> 
	/// methods of the <see cref="T:System.Net.Sockets.Socket" /> class. </para>
	/// </summary>
	/// <remarks>
	/// <para>Socket options determine the behavior of an instance of
	///       the <see cref="T:System.Net.Sockets.Socket" /> class. Some socket options apply only to specific protocols while others apply to
	///       all types. Members of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration specify
	///       which protocol applies to a specific socket option. </para>
	/// </remarks>
	public enum SocketOptionName {

		/// <summary><para>Record debugging information when available.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Debug = 1,

		/// <summary><para><see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" qualify="true" /> has been called on the socket.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> 
		/// data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		AcceptConnection = 2,

		/// <summary><para> Allow the socket to be bound to an address that is already in use.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		ReuseAddress = 4,

		/// <summary><para> Send keep-alives.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		KeepAlive = 8,

		/// <summary><para> Do not route; send directly to interface addresses.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		DontRoute = 16,

		/// <summary><para> Permit sending broadcast messages on the socket.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Broadcast = 32,

		/// <summary><para> Bypass hardware when possible.
		///       </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		UseLoopback = 64,

		/// <summary><para> Linger on close if unsent data is present.
		///  </para><para>The value associated with this option is an instance of 
		///  the <see cref="T:System.Net.Sockets.LingerOption" qualify="true" /> class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Linger = 128,

		/// <summary><para> Receive out-of-band data in the normal data stream.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		OutOfBandInline = 256,

		/// <summary><para> Close socket gracefully without lingering.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		DontLinger = -129,

		/// <summary><para> Enable a socket to be bound for exclusive access.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para></summary>
		ExclusiveAddressUse = -5,

		/// <summary><para> Specifies the total per-socket buffer space reserved for sends. This is
		///  unrelated to the maximum message size or the size of a TCP window.
		///  </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		SendBuffer = 4097,

		/// <summary><para>Specifies the total per-socket buffer space reserved for
		///  receives. This is unrelated to the maximum message size or the size of a TCP window. </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		ReceiveBuffer = 4098,

		/// <summary><para> Send low water mark.
		///  </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para></summary>
		SendLowWater = 4099,

		/// <summary><para> Receive low water mark.
		///  </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para></summary>
		ReceiveLowWater = 4100,

		/// <summary><para> Specifies the 
		///       maximum time, in milliseconds, the <see cref="M:System.Net.Sockets.Socket.Send(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" /> and <see cref="M:System.Net.Sockets.Socket.SendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint)" /> methods will block when attempting to
		///       send data. If data is not sent within this time, a <see cref="T:System.Net.Sockets.SocketException" /> exception is thrown.
		///       </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> member of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		SendTimeout = 4101,

		/// <summary><para> Specifies the 
		///       maximum time, in milliseconds, the <see cref="M:System.Net.Sockets.Socket.Receive(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" /> and <see cref="M:System.Net.Sockets.Socket.ReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@)" /> methods will block when attempting to
		///       receive data. If data is not received within this time, a <see cref="T:System.Net.Sockets.SocketException" /> exception is thrown.
		///       </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		ReceiveTimeout = 4102,

		/// <summary><para> Get the error status code, then clear the code.
		///       </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Error = 4103,

		/// <summary><para> Get the socket type, one of the members of 
		///       the <see cref="T:System.Net.Sockets.SocketType" qualify="true" /> enumeration.
		///       </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Socket" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Type = 4104,

		/// <summary><para> Maximum queue length that can be specified by <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" />.
		///  </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para></summary>
		MaxConnections = 2147483647,

		/// <summary><para>Specifies IP options to be inserted into outgoing datagrams.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		IPOptions = 1,

		/// <summary><para> Application is providing the IP header for
		///  outgoing datagrams.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		HeaderIncluded = 2,

		/// <summary><para>Change the IP header type of service field.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		TypeOfService = 3,

		/// <summary><para>Set the IP header time-to-live field.</para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		IpTimeToLive = 4,

		/// <summary><para>Set the interface for outgoing multicast packets.</para><para>The value associated with this option is an instance of the <see cref="!:System.Net.Sockets.IPAddress" qualify="true" /> 
		/// class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		MulticastInterface = 9,

		/// <summary><para> IP multicast time to live.
		///  </para><para>The value associated with this option is a <see cref="T:System.Int32" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		MulticastTimeToLive = 10,

		/// <summary><para>IP multicast loopback.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		MulticastLoopback = 11,

		/// <summary><para> Add an IP group membership.
		///  </para><para>The value associated with this option is an instance of the <see cref="T:System.Net.Sockets.MulticastOption" qualify="true" /> class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		AddMembership = 12,

		/// <summary><para> Drop an IP group membership.
		///  </para><para>The value associated with this option is an instance of the <see cref="T:System.Net.Sockets.MulticastOption" qualify="true" /> class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		DropMembership = 13,

		/// <summary><para>Do not fragment IP datagrams.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para></summary>
		DontFragment = 14,

		/// <summary><para>Join a source group.</para><para>The value associated with this option is an instance of the <see cref="!:System.Net.Sockets.IPAddress" qualify="true" />
		/// class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		AddSourceMembership = 15,

		/// <summary><para>Drop a source group.</para><para>The value associated with this option is an instance of the <see cref="!:System.Net.Sockets.IPAddress" qualify="true" />
		/// class. </para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.IP" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		DropSourceMembership = 16,

		/// <summary><para>Block data from a source.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para></summary>
		BlockSource = 17,

		/// <summary><para>Unblock a previously blocked source.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para></summary>
		UnblockSource = 18,

		/// <summary><para>Return information about received packets.</para></summary>
		PacketInformation = 19,

		/// <summary><para> Disable the Nagle algorithm for send coalescing.
		///  </para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data 
		///  type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Tcp" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		NoDelay = 1,

		/// <summary><para>Use urgent data as defined in RFC-1222. This option can
		///  be set only once, and once set, cannot be turned off.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Tcp" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		BsdUrgent = 2,

		/// <summary><para>Use expedited data as defined in RFC-1222. This option
		///  can be set only once, and once set, cannot be turned off.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Tcp" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		Expedited = 2,

		/// <summary><para>Send UDP datagrams with checksum set to zero.</para><para>The value associated with this option is a <see cref="T:System.Boolean" qualify="true" /> data type.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Udp" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		NoChecksum = 1,

		/// <summary><para>Set or get UDP checksum coverage.</para><para>The <see cref="F:System.Net.Sockets.SocketOptionLevel.Udp" /> value of the <see cref="T:System.Net.Sockets.SocketOptionLevel" /> enumeration applies to this option.</para></summary>
		ChecksumCoverage = 20,
	} // SocketOptionName

} // System.Net.Sockets
