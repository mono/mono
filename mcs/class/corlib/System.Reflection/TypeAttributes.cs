// TypeAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:40:22 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum TypeAttributes {

		/// <summary>
		/// </summary>
		VisibilityMask = 7,

		/// <summary>
		/// </summary>
		NotPublic = 0,

		/// <summary>
		/// </summary>
		Public = 1,

		/// <summary>
		/// </summary>
		NestedPublic = 2,

		/// <summary>
		/// </summary>
		NestedPrivate = 3,

		/// <summary>
		/// </summary>
		NestedFamily = 4,

		/// <summary>
		/// </summary>
		NestedAssembly = 5,

		/// <summary>
		/// </summary>
		NestedFamANDAssem = 6,

		/// <summary>
		/// </summary>
		NestedFamORAssem = 7,

		/// <summary>
		/// </summary>
		LayoutMask = 24,

		/// <summary>
		/// </summary>
		AutoLayout = 0,

		/// <summary>
		/// </summary>
		LayoutSequential = 8,

		/// <summary>
		/// </summary>
		ExplicitLayout = 16,

		/// <summary>
		/// </summary>
		ClassSemanticsMask = 32,

		/// <summary>
		/// </summary>
		Class = 0,

		/// <summary>
		/// </summary>
		Interface = 32,

		/// <summary>
		/// </summary>
		Abstract = 128,

		/// <summary>
		/// </summary>
		Sealed = 256,

		/// <summary>
		/// </summary>
		SpecialName = 1024,

		/// <summary>
		/// </summary>
		Import = 4096,

		/// <summary>
		/// </summary>
		Serializable = 8192,

		/// <summary>
		/// </summary>
		StringFormatMask = 196608,

		/// <summary>
		/// </summary>
		AnsiClass = 0,

		/// <summary>
		/// </summary>
		UnicodeClass = 65536,

		/// <summary>
		/// </summary>
		AutoClass = 131072,

		/// <summary>
		/// </summary>
		BeforeFieldInit = 1048576,

		/// <summary>
		/// </summary>
		ReservedMask = 264192,

		/// <summary>
		/// </summary>
		RTSpecialName = 2048,

		/// <summary>
		/// </summary>
		HasSecurity = 262144,
	} // TypeAttributes

} // System.Reflection
