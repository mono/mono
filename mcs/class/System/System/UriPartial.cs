// UriPartial.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	///  Specifies URI components.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">The <see cref="T:System.UriPartial" /> enumeration defines the values that are passed to the
	/// <see cref="M:System.Uri.GetLeftPart(System.UriPartial)" qualify="true" /> method.</block>
	/// </para>
	/// </remarks>
	public enum UriPartial {

		/// <summary><para> Specifies the scheme component of a URI.</para></summary>
		Scheme = 0,

		/// <summary>
		///  Specifies the authority component of a URI.
		/// </summary>
		Authority = 1,

		/// <summary>
		///  Specifies the path component of a URI.
		/// </summary>
		Path = 2,
	} // UriPartial

} // System
