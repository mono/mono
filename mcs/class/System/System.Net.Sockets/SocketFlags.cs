// SocketFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:32:49 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net.Sockets {


	/// <summary>
	/// </summary>
	[Flags]
	public enum SocketFlags {

		/// <summary>
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// </summary>
		OutOfBand = 0x00000001,

		/// <summary>
		/// </summary>
		MaxIOVectorLength = 0x00000010,
		
		/// <summary>
		/// </summary>
		Peek = 0x00000002,

		/// <summary>
		/// </summary>
		DontRoute = 0x00000004,

		/// <summary>
		/// </summary>
		Partial = 0x00008000,
	} // SocketFlags

} // System.Net.Sockets
