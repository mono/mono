// FormatterTypeStyle.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.Serialization.Formatters {


	/// <summary>
	/// <para>
	///                   Specifies options to the XML and Binary formatters.
	///                </para>
	/// </summary>
	public enum FormatterTypeStyle {

		/// <summary>
		/// <para>
		///                   Types are outputted only for <see langword="VARIANT " /> and <see cref="T:System.Array" />.
		///                </para>
		/// </summary>
		TypesWhenNeeded = 0,

		/// <summary>
		/// <para>
		///                   Types are outputted for all <see cref="T:System.Object" />
		///                   fields
		///                </para>
		/// </summary>
		TypesAlways = 1,
		XsdString = 2,
	} // FormatterTypeStyle

} // System.Runtime.Serialization.Formatters
