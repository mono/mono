// SocketType.cs
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
	/// <para> Specifies the type of socket an instance of the <see cref="T:System.Net.Sockets.Socket" /> class represents.
	///  </para>
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="T:System.Net.Sockets.SocketType" /> member 
	///  is required when instantiating the <see cref="T:System.Net.Sockets.Socket" />
	///  class and specifies the functionality the instance supports. </para>
	/// </remarks>
	public enum SocketType {

		/// <summary><para> Supports reliable, two-way, connection-based byte
		///       streams with an out-of-band (OOB) data transmission mechanism. Uses the Transmission
		///       Control Protocol (<see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" />) protocol and the <see cref="!:System.Net.Sockets.Socket.ProtocolFamily.InterNetwork" qualify="true" /> address family.</para></summary>
		Stream = 1,

		/// <summary><para> Supports datagrams, which are connectionless, unreliable
		///       messages of a fixed (typically small) maximum length. Uses the User Datagram
		///       Protocol (<see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" /> ) protocol
		///       and the <see cref="!:System.Net.Sockets.Socket.ProtocolFamily.InterNetwork" qualify="true" /> address family.</para></summary>
		Dgram = 2,

		/// <summary><para> 
		///       Supports access to the underlying
		///       transport protocol. Can communication through protocols other than <see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" /> and <see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" />
		///       
		///       such as, Internet Control Message Protocol (<see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" />) and Internet Group Management
		///       Protocol (<see cref="P:System.Net.Sockets.Socket.ProtocolType" qualify="true" />).</para></summary>
		Raw = 3,

		/// <summary><para>Supports message-oriented, reliably delivered messages, and preserves message 
		///       boundaries in data. </para></summary>
		Rdm = 4,

		/// <summary><para> Supports message-oriented, sequenced packets.</para></summary>
		Seqpacket = 5,

		/// <summary><para> Unknown socket type.</para></summary>
		Unknown = -1,
	} // SocketType

} // System.Net.Sockets
