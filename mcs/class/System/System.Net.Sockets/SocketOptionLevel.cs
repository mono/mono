// SocketOptionLevel.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:32:55 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net.Sockets {


	/// <summary>
	/// </summary>
	public enum SocketOptionLevel {

		/// <summary>
		/// </summary>
		Socket = 65535,

		/// <summary>
		/// </summary>
		IP = 0,

#if NET_1_1
		/// <summary>
		/// </summary>
		IPv6 = 41,
#endif

		/// <summary>
		/// </summary>
		Tcp = 6,

		/// <summary>
		/// </summary>
		Udp = 17,
	} // SocketOptionLevel

} // System.Net.Sockets
