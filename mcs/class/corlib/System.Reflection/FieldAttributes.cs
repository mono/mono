// FieldAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:12 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum FieldAttributes {

		/// <summary>
		/// </summary>
		FieldAccessMask = 7,

		/// <summary>
		/// </summary>
		PrivateScope = 0x0,

		/// <summary>
		/// </summary>
		Private = 0x1,

		/// <summary>
		/// </summary>
		FamANDAssem = 0x2,

		/// <summary>
		/// </summary>
		Assembly = 0x3,

		/// <summary>
		/// </summary>
		Family = 0x4,

		/// <summary>
		/// </summary>
		FamORAssem = 0x5,

		/// <summary>
		/// </summary>
		Public = 0x6,

		/// <summary>
		/// </summary>
		Static = 0x10,

		/// <summary>
		/// </summary>
		InitOnly = 0x20,

		/// <summary>
		/// </summary>
		Literal = 0x40,

		/// <summary>
		/// </summary>
		NotSerialized = 0x80,

		/// <summary>
		/// </summary>
		HasFieldRVA = 0x100,

		/// <summary>
		/// </summary>
		SpecialName = 0x200,

		/// <summary>
		/// </summary>
		RTSpecialName = 0x400,

		/// <summary>
		/// </summary>
		HasFieldMarshal = 0x1000,	

		/// <summary>
		/// </summary>
		PinvokeImpl = 0x2000,

		/// <summary>
		/// </summary>
		// HasSecurity = 0x4000,

		/// <summary>
		/// </summary>
		HasDefault = 0x8000,

		/// <summary>
		/// </summary>
		ReservedMask = HasDefault | HasFieldMarshal | RTSpecialName | HasFieldRVA,

	} // FieldAttributes

} // System.Reflection
