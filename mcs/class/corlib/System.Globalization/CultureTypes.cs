// CultureTypes.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Globalization {

	public enum CultureTypes {

		/// <summary>
		/// <para>Describes cultures which have no attached sub-tag, e.g. "de", "en", "ja".</para>
		/// </summary>
		NeutralCultures = 1,

		/// <summary>
		/// <para>Describes cultures which have an attached sub-tag, e.g. "de-CH", 
		///                   "en-US", "ja-JP".</para>
		/// </summary>
		SpecificCultures = 2,

		/// <summary>
		/// <para>Describes a union of neutral and specific cultures, i.e. all cultures.</para>
		/// </summary>
		AllCultures = 3,

		/// <summary>
		/// <para>In case of a Win32 system describes all installed cultures.</para>
		/// </summary>
		InstalledWin32Cultures = 4,
	} // CultureTypes

} // System.Globalization
