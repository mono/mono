// TransportType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {


	/// <summary>
	/// <para> Specifies transport types.
	///       </para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">The <see cref="T:System.Net.TransportType" /> enumeration defines transport types
	///    for the <see cref="T:System.Net.SocketPermission" /> and <see cref="T:System.Net.Sockets.Socket" /> classes.</block>
	/// </para>
	/// </remarks>
	public enum TransportType {

		/// <summary><para> Specifies the User Datagram Protocol (UDP) transport as defined by IETF RFC 768.
		///       </para></summary>
		Udp = 1,

		/// <summary><para> Specifies any connectionless transport, such as User Datagram Protocol (UDP).</para></summary>
		Connectionless = 1,

		/// <summary><para> Specifies the Transmission Control Protocol (TCP) transport as defined by IETF RFC 793.
		///       </para></summary>
		Tcp = 2,

		/// <summary><para> Specifies any connection-oriented transport, such as Transmission Control Protocol (TCP).</para></summary>
		ConnectionOriented = 2,

		/// <summary><para> Specifies any transport type.
		///  </para></summary>
		All = 3,
	} // TransportType

} // System.Net
