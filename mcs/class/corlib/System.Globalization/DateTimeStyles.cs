// DateTimeStyles.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Globalization {


	/// <summary>
	/// <para>Defines the formatting options that customize how the
	///    <see cref="M:System.DateTime.Parse(System.String)" />
	///    and <see cref="M:System.DateTime.ParseExact(System.String,System.String,System.IFormatProvider)" /> methods parse a string.</para>
	/// </summary>
	/// <remarks>
	/// <block subset="none" type="note">
	/// <para>See the <see cref="T:System.String" /> class for the list of white space characters.</para>
	/// <para>Only the <see cref="F:System.Globalization.DateTimeStyles.NoCurrentDateDefault" /> option affects the <see cref="M:System.DateTime.Parse(System.String)" /> method.
	/// <see cref="M:System.DateTime.Parse(System.String)" /> always 
	///    ignores leading, inner, and trailing white spaces.</para>
	/// <para>This enumeration has a <see cref="T:System.FlagsAttribute" /> that
	/// allows a bitwise combination of its member values.</para>
	/// </block>
	/// </remarks>
	[Flags]
	public enum DateTimeStyles {

		/// <summary><para>Indicates that the default formatting options should be used.</para></summary>
		None = 0x00000000,

		/// <summary><para>Indicates that leading white space characters are allowed.</para></summary>
		AllowLeadingWhite = 0x00000001,

		/// <summary><para>Indicates that trailing white space characters are allowed.</para></summary>
		AllowTrailingWhite = 0x00000002,

		/// <summary><para>Indicates that extra white space characters not specified 
		///       in the <see cref="T:System.Globalization.DateTimeFormatInfo" /> format patterns are allowed.</para></summary>
		AllowInnerWhite = 0x00000004,

		/// <summary><para>Indicates that white space characters anywhere in the 
		///       string are allowed.</para></summary>
		AllowWhiteSpaces = AllowLeadingWhite | AllowTrailingWhite | AllowInnerWhite,

		/// <summary><para>Indicates that there is no current date default. If a 
		///       string contains only the time and not the date, and this option is used with the
		///    <see cref="M:System.DateTime.Parse(System.String)" /> or <see cref="M:System.DateTime.ParseExact(System.String,System.String,System.IFormatProvider)" /> methods, a Gregorian year 1, month 1, day 
		///       1 date is assumed. In all other cases the methods assume the current local system date.</para></summary>
		NoCurrentDateDefault = 0x00000008,

		AdjustToUniversal = 0x00000010,
	} // DateTimeStyles

} // System.Globalization
