// ProtocolType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:32:24 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net.Sockets {


	/// <summary>
	/// </summary>
	public enum ProtocolType {

		/// <summary>
		/// </summary>
		IP = 0,

		/// <summary>
		/// </summary>
		Icmp = 1,

		/// <summary>
		/// </summary>
		Igmp = 2,

		/// <summary>
		/// </summary>
		Ggp = 3,

		/// <summary>
		/// </summary>
		Tcp = 6,

		/// <summary>
		/// </summary>
		Pup = 12,

		/// <summary>
		/// </summary>
		Udp = 17,

		/// <summary>
		/// </summary>
		Idp = 22,

#if NET_1_1
		/// <summary>
		/// </summary>
		IPv6 = 41,
#endif

		/// <summary>
		/// </summary>
		ND = 77,

		/// <summary>
		/// </summary>
		Raw = 255,

		/// <summary>
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// </summary>
		Ipx = 1000,

		/// <summary>
		/// </summary>
		Spx = 1256,

		/// <summary>
		/// </summary>
		SpxII = 1257,

		/// <summary>
		/// </summary>
		Unknown = -1,
	} // ProtocolType

} // System.Net.Sockets
