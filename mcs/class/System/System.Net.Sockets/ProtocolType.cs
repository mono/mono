// ProtocolType.cs
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
	/// <para> Specifies the protocols used by the <see cref="T:System.Net.Sockets.Socket" /> class.
	///    </para>
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="T:System.Net.Sockets.ProtocolType" /> enumeration is used with the
	/// <see cref="T:System.Net.Sockets.Socket" /> class. This enumeration specifies the 
	///    protocols that a socket instance can use to transport
	///    data. </para>
	/// </remarks>
	public enum ProtocolType {

		/// <summary><para>Specifies the Internet Protocol (IP) as defined by IETF RFC 791, 792, 919, 
		///       922, and 1112.</para><block subset="none" type="note"><para>Multiple names are defined for this value based on prior art.
		///          This value is identical to <see cref="F:System.Net.Sockets.ProtocolType.Unspecified" />.</para></block></summary>
		IP = 0,

		/// <summary><para>Specifies the Internet Control Message Protocol (ICMP) as defined by IETF RFC 1792.</para></summary>
		Icmp = 1,

		/// <summary><para>Specifies the Internet Group Management Protocol (IGMP) as defined by IETF RFC 2236.</para></summary>
		Igmp = 2,

		/// <summary><para>Specifies the Gateway To Gateway Protocol.</para></summary>
		Ggp = 3,

		/// <summary><para>Specifies the Transmission Control Protocol (TCP) as defined by IETF RFC 793. </para></summary>
		Tcp = 6,

		/// <summary><para> Specifies the Xerox Post Office Update Protocol.</para></summary>
		Pup = 12,

		/// <summary><para>Specifies the User Datagram Protocol (UDP) as defined by IETF RFC 768. </para></summary>
		Udp = 17,

		/// <summary><para> Specifies the Inter-Domain Policy Protocol (IDP) as defined by IETF RFC 1764.</para></summary>
		Idp = 22,

		/// <summary><para> Specifies the Net Disk Protocol.</para></summary>
		ND = 77,

		/// <summary><para>Specifies the Raw IP packet protocol.</para></summary>
		Raw = 255,

		/// <summary><para>Unspecified protocol.</para><block subset="none" type="note"><para>Multiple names are defined for this value based on prior art. 
		///          This value is identical to <see cref="F:System.Net.Sockets.ProtocolType.IP" />.</para></block></summary>
		Unspecified = 0,

		/// <summary><para> Specifies the Internetwork Packet Exchange Protocol.</para></summary>
		Ipx = 1000,

		/// <summary><para> Specifies the Sequenced Packet Exchange Protocol.</para></summary>
		Spx = 1256,

		/// <summary><para>Specifies the Sequenced Packet Exchange Version 2 Protocol.</para></summary>
		SpxII = 1257,

		/// <summary><para>Used to indicate an uninitialized state. This member is 
		///       not to be used when instantiating the <see cref="T:System.Net.Sockets.Socket" /> class.</para></summary>
		Unknown = -1,
	} // ProtocolType

} // System.Net.Sockets
