// MethodImplAttributes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:39:42 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum MethodImplAttributes {

		/// <summary>
		/// </summary>
		CodeTypeMask = 3,

		/// <summary>
		/// </summary>
		IL = 0,

		/// <summary>
		/// </summary>
		Native = 1,

		/// <summary>
		/// </summary>
		OPTIL = 2,

		/// <summary>
		/// </summary>
		Runtime = 3,

		/// <summary>
		/// </summary>
		ManagedMask = 4,

		/// <summary>
		/// </summary>
		Unmanaged = 4,

		/// <summary>
		/// </summary>
		Managed = 0,

		/// <summary>
		/// </summary>
		ForwardRef = 16,

		/// <summary>
		/// </summary>
		PreserveSig = 128,

		/// <summary>
		/// </summary>
		InternalCall = 4096,

		/// <summary>
		/// </summary>
		Synchronized = 32,

		/// <summary>
		/// </summary>
		NoInlining = 8,

		/// <summary>
		/// </summary>
		MaxMethodImplVal = 65535,
	} // MethodImplAttributes

} // System.Reflection
