// RegistryPermissionAccess.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:42:13 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// </summary>
	[Flags]
	public enum RegistryPermissionAccess {

		/// <summary>
		/// </summary>
		NoAccess = 0,

		/// <summary>
		/// </summary>
		Read = 1,

		/// <summary>
		/// </summary>
		Write = 2,

		/// <summary>
		/// </summary>
		Create = 4,

		/// <summary>
		/// </summary>
		AllAccess = 7,
	} // RegistryPermissionAccess

} // System.Security.Permissions
