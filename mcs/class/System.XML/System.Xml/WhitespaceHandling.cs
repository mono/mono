// WhitespaceHandling.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Xml {


	/// <summary>
	/// <para> Specifies the type of white space returned by instances of 
	///       the <see cref="T:System.Xml.XmlTextReader" /> class.
	///       </para>
	/// </summary>
	/// <remarks>
	/// <para>Significant white space is white space between markup in 
	///       a mixed content model, or white space within an element that has the xml:space =
	///       "preserve" attribute. Insignificant
	///       white space is any other white space between markup.</para>
	/// </remarks>
	public enum WhitespaceHandling {

		/// <summary><para>Return both significant and insignificant white space. This is the default. </para></summary>
		All = 0,

		/// <summary><para> Return significant white space only.
		///       </para></summary>
		Significant = 1,

		/// <summary><para>Return neither significant nor insignificant white
		///       space.</para></summary>
		None = 2,
	} // WhitespaceHandling

} // System.Xml
