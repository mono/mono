// ParameterAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:52 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum ParameterAttributes {

		/// <summary>
		/// </summary>
		None = 0,

		/// <summary>
		/// </summary>
		In = 1,

		/// <summary>
		/// </summary>
		Out = 2,

		/// <summary>
		/// </summary>
		Lcid = 4,

		/// <summary>
		/// </summary>
		Retval = 8,

		/// <summary>
		/// </summary>
		Optional = 16,

		/// <summary>
		/// </summary>
		ReservedMask = 61440,

		/// <summary>
		/// </summary>
		HasDefault = 4096,

		/// <summary>
		/// </summary>
		HasFieldMarshal = 8192,

		/// <summary>
		/// </summary>
		Reserved3 = 16384,

		/// <summary>
		/// </summary>
		Reserved4 = 32768,
	} // ParameterAttributes

} // System.Reflection
