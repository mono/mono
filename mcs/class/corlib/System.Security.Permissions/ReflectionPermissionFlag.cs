// ReflectionPermissionFlag.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:31:14 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// </summary>
	[Flags]
	public enum ReflectionPermissionFlag {

		/// <summary>
		/// </summary>
		NoFlags = 0x00000000,

		/// <summary>
		/// </summary>
		TypeInformation = 0x00000001,

		/// <summary>
		/// </summary>
		MemberAccess = 0x00000002,

		/// <summary>
		/// </summary>
		ReflectionEmit = 0x4,

		/// <summary>
		/// </summary>
		AllFlags = TypeInformation | MemberAccess | ReflectionEmit,
	} // ReflectionPermissionFlag

} // System.Security.Permissions
