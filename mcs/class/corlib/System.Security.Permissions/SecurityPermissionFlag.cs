// SecurityPermissionFlag.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:30:18 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// </summary>
	[Flags]
	public enum SecurityPermissionFlag {

		/// <summary>
		/// </summary>
		NoFlags = 0x00000000,

		/// <summary>
		/// </summary>
		Assertion = 0x00000001,

		/// <summary>
		/// </summary>
		UnmanagedCode = 0x00000002,

		/// <summary>
		/// </summary>
		SkipVerification = 0x00000004,

		/// <summary>
		/// </summary>
		Execution = 0x00000008,

		/// <summary>
		/// </summary>
		ControlThread = 0x00000010,

		/// <summary>
		/// </summary>
		AllFlags = Assertion | UnmanagedCode | SkipVerification | Execution | ControlThread,
	} // SecurityPermissionFlag

} // System.Security.Permissions
