// SocketShutdown.cs
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
	/// <para>Specifies whether the ability to send or receive data is terminated when the <see cref="M:System.Net.Sockets.Socket.Shutdown(System.Net.Sockets.SocketShutdown)" />
	/// method is called on a connected <see cref="T:System.Net.Sockets.Socket" /> instance.</para>
	/// </summary>
	public enum SocketShutdown {

		/// <summary><para> Specifies to terminate the ability to receive data on a 
		///  <see cref="T:System.Net.Sockets.Socket" /> 
		///  instance.</para></summary>
		Receive = 0,

		/// <summary><para>Specifies to terminate the ability to send data from a 
		///  <see cref="T:System.Net.Sockets.Socket" /> 
		///  instance.</para></summary>
		Send = 1,

		/// <summary><para> Specifies to terminate the ability to send and receive data 
		///  on a <see cref="T:System.Net.Sockets.Socket" /> instance.</para></summary>
		Both = 2,
	} // SocketShutdown

} // System.Net.Sockets
