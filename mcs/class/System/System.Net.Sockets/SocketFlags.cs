// SocketFlags.cs
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
	/// <para> Controls the transfer behavior when sending and 
	///  receiving data on a <see cref="T:System.Net.Sockets.Socket" /> instance.</para>
	/// </summary>
	/// <remarks>
	/// <para>The following methods use this enumeration:</para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.BeginReceive(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.BeginReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@,System.AsyncCallback,System.Object)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.BeginSendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint,System.AsyncCallback,System.Object)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.Receive(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.ReceiveFrom(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint@)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.Send(System.Byte[],System.Int32,System.Net.Sockets.SocketFlags)" />
	/// </para>
	/// <para>
	/// <see cref="M:System.Net.Sockets.Socket.SendTo(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.Net.EndPoint)" />
	/// </para>
	/// </remarks>
	[Flags]
	public enum SocketFlags {

		/// <summary><para> No flags are specified. 
		///  </para></summary>
		None = 0x00000000,

		/// <summary><para> Specifies to send or receive out-of-band (OOB) data. OOB
		///       data is specially marked data that can be received independently of unmarked data.
		///       </para><block subset="none" type="note"><para>Used only with a connection-oriented protocol. </para></block></summary>
		OutOfBand = 0x00000001,

		/// <summary><para> Specifies to peek at the incoming data. This copies data 
		///  to the input buffer but does not remove it from the input queue.
		///  </para></summary>
		Peek = 0x00000002,

		/// <summary><para> Specifies not to use routing tables to transmit the data. If there is a router 
		///  between the local and destination addresses, the data will be lost.
		///  </para></summary>
		DontRoute = 0x00000004,

		/// <summary><para> Specifies that a partial message has been received.
		///       </para><block subset="none" type="note"><para>Used only with a message-oriented protocol. </para></block></summary>
		Partial = 0x00008000,
	} // SocketFlags

} // System.Net.Sockets
