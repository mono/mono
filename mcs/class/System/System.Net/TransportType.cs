// TransportType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {


	/// <summary>
	/// <para>
	///                   Defines the transport type allowed for the socket.
	///                </para>
	/// </summary>
	public enum TransportType {

		/// <summary>
		/// <para>
		///                   Udp connections are allowed.
		///                </para>
		/// </summary>
		Udp = 1,

		/// <summary>
		/// <para>
		///                   TCP connections are allowed.
		///                </para>
		/// </summary>
		Tcp = 2,

		/// <summary>
		/// <para>
		///                   Any connection is allowed.
		///                </para>
		/// </summary>
		All = 3,
	} // TransportType

} // System.Net
