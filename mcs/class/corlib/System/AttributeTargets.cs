// AttributeTargets.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:31:48 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	/// </summary>
	[Flags]
	public enum AttributeTargets {

		/// <summary>
		/// </summary>
		Assembly = 0x00000001,

		/// <summary>
		/// </summary>
		Module = 0x00000002,

		/// <summary>
		/// </summary>
		Class = 0x00000004,

		/// <summary>
		/// </summary>
		Struct = 0x00000008,

		/// <summary>
		/// </summary>
		Enum = 0x00000010,

		/// <summary>
		/// </summary>
		Constructor = 0x00000020,

		/// <summary>
		/// </summary>
		Method = 0x00000040,

		/// <summary>
		/// </summary>
		Property = 0x00000080,

		/// <summary>
		/// </summary>
		Field = 0x00000100,

		/// <summary>
		/// </summary>
		Event = 0x00000200,

		/// <summary>
		/// </summary>
		Interface = 0x00000400,

		/// <summary>
		/// </summary>
		Parameter = 0x00000800,

		/// <summary>
		/// </summary>
		Delegate = 0x00001000,

		/// <summary>
		/// </summary>
		ReturnValue = 0x00002000,

		/// <summary>
		/// </summary>
		All = Assembly | Module | Class | Struct | Enum | Constructor | Method | Property | Field | Event | Interface | Parameter | Delegate | ReturnValue,
	} // AttributeTargets

} // System
