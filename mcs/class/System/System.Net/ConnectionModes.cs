// ConnectionModes.cs
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
	///                   Specifies the mode used to establish a connection with a server.
	///                </para>
	/// </summary>
	public enum ConnectionModes {

		/// <summary>
		/// <para>
		///                   Non-persistent, one request per connection.
		///                </para>
		/// </summary>
		Single = 0,

		/// <summary>
		/// <para>
		///                   Persistent connection, one request/response at a time.
		///                </para>
		/// </summary>
		Persistent = 1,

		/// <summary>
		/// <para>
		///                   Persistent connection, many requests/responses in order.
		///                </para>
		/// </summary>
		Pipeline = 2,

		/// <summary>
		/// <para>
		///                   Persistent connection, many requests/responses out of order.
		///                </para>
		/// </summary>
		Mux = 3,
	} // ConnectionModes

} // System.Net
