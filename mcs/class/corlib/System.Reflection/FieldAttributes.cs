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
	public enum FieldAttributes {

		/// <summary>
		/// </summary>
		FieldAccessMask = 7,

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
		InitOnly = 32,

		/// <summary>
		/// </summary>
		Literal = 64,

		/// <summary>
		/// </summary>
		NotSerialized = 128,

		/// <summary>
		/// </summary>
		SpecialName = 512,

		/// <summary>
		/// </summary>
		PinvokeImpl = 8192,

		/// <summary>
		/// </summary>
		ReservedMask = 54528,

		/// <summary>
		/// </summary>
		RTSpecialName = 1024,

		/// <summary>
		/// </summary>
		HasFieldMarshal = 4096,	

		/// <summary>
		/// </summary>
		// HasSecurity = 16384,

		/// <summary>
		/// </summary>
		HasDefault = 32768,

		/// <summary>
		/// </summary>
		HasFieldRVA = 256,
	} // FieldAttributes

} // System.Reflection
