// FileIOPermissionAccess.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:30:11 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// </summary>
	[Flags]
	public enum FileIOPermissionAccess {

		/// <summary>
		/// </summary>
		NoAccess = 0x00000000,

		/// <summary>
		/// </summary>
		Read = 0x00000001,

		/// <summary>
		/// </summary>
		Write = 0x00000002,

		/// <summary>
		/// </summary>
		Append = 0x00000004,

		/// <summary>
		/// </summary>
		PathDiscovery = 0x00000008,

		/// <summary>
		/// </summary>
		AllAccess = Read | Write | Append | PathDiscovery,
	} // FileIOPermissionAccess

} // System.Security.Permissions
