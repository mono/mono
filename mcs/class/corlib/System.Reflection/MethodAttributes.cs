// MethodAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:32 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum MethodAttributes {

		/// <summary>
		/// </summary>
		MemberAccessMask = 7,

		/// <summary>
		/// </summary>
		PrivateScope = 0,

		/// <summary>
		/// </summary>
		Private = 1,

		/// <summary>
		/// </summary>
		FamANDAssem = 2,

		/// <summary>
		/// </summary>
		Assembly = 3,

		/// <summary>
		/// </summary>
		Family = 4,

		/// <summary>
		/// </summary>
		FamORAssem = 5,

		/// <summary>
		/// </summary>
		Public = 6,

		/// <summary>
		/// </summary>
		Static = 16,

		/// <summary>
		/// </summary>
		Final = 32,

		/// <summary>
		/// </summary>
		Virtual = 64,

		/// <summary>
		/// </summary>
		HideBySig = 128,

		/// <summary>
		/// </summary>
		VtableLayoutMask = 256,

#if NET_1_1
		/// <summary>
		/// </summary>		
		CheckAccessOnOverride = 512,
#endif

		/// <summary>
		/// </summary>
		ReuseSlot = 0,

		/// <summary>
		/// </summary>
		NewSlot = 256,

		/// <summary>
		/// </summary>
		Abstract = 1024,

		/// <summary>
		/// </summary>
		SpecialName = 2048,

		/// <summary>
		/// </summary>
		PinvokeImpl = 8192,

		/// <summary>
		/// </summary>
		UnmanagedExport = 8,

		/// <summary>
		/// </summary>
		RTSpecialName = 4096,

		/// <summary>
		/// </summary>
		ReservedMask = 53248,

		/// <summary>
		/// </summary>
		HasSecurity = 16384,

		/// <summary>
		/// </summary>
		RequireSecObject = 32768,
	} // MethodAttributes

} // System.Reflection
