// BindingFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:33:54 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	public enum BindingFlags {

		Default = 0,

		/// <summary>
		/// </summary>
		IgnoreCase = 0x00000001,

		/// <summary>
		/// </summary>
		DeclaredOnly = 0x00000002,

		/// <summary>
		/// </summary>
		Instance = 0x00000004,

		/// <summary>
		/// </summary>
		Static = 0x00000008,

		/// <summary>
		/// </summary>
		Public = 0x00000010,

		/// <summary>
		/// </summary>
		NonPublic = 0x00000020,

		FlattenHierarchy = 0x00000040,
		
		/// <summary>
		/// </summary>
		InvokeMethod = 0x00000100,

		/// <summary>
		/// </summary>
		CreateInstance = 0x00000200,

		/// <summary>
		/// </summary>
		GetField = 0x00000400,

		/// <summary>
		/// </summary>
		SetField = 0x00000800,

		/// <summary>
		/// </summary>
		GetProperty = 0x00001000,

		/// <summary>
		/// </summary>
		SetProperty = 0x00002000,

		PutDispProperty = 0x00004000,

		/// <summary>
		/// </summary>
		ExactBinding = 0x00010000,

		/// <summary>
		/// </summary>
		SuppressChangeType = 0x00020000,

		/// <summary>
		/// </summary>
		OptionalParamBinding = 0x00040000,

		IgnoreReturn = 0x01000000
	} // BindingFlags

} // System.Reflection
