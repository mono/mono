// DateTimeStyles.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:32:07 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Globalization {


	/// <summary>
	/// </summary>
	[Flags]
	public enum DateTimeStyles {

		/// <summary>
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// </summary>
		AllowLeadingWhite = 0x00000001,

		/// <summary>
		/// </summary>
		AllowTrailingWhite = 0x00000002,

		/// <summary>
		/// </summary>
		AllowInnerWhite = 0x00000004,

		/// <summary>
		/// </summary>
		AllowWhiteSpaces = AllowLeadingWhite | AllowTrailingWhite | AllowInnerWhite,

		/// <summary>
		/// </summary>
		NoCurrentDateDefault = 0x00000008,

		/// <summary>
		/// </summary>
		AdjustToUniversal = 0x00000010,
	} // DateTimeStyles

} // System.Globalization
