// Formatting.cs
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
	/// <para> Specifies formatting options for an instance of the <see cref="T:System.Xml.XmlTextWriter" /> class.
	///    </para>
	/// </summary>
	public enum Formatting {

		/// <summary><para> No special formatting is applied. This is the default.
		///       </para></summary>
		None = 0,

		/// <summary><para> Causes child elements to be indented
		///       according to the <see cref="P:System.Xml.XmlTextWriter.Indentation" /> and <see cref="P:System.Xml.XmlTextWriter.IndentChar" />
		///       settings.
		///       This option
		///       
		///       indents element content only; mixed content is not affected.</para><block subset="none" type="note"><para>For the XML 1.0 definitions of these terms, see the W3C documentation 
		///          (http://www.w3.org/TR/1998/REC-xml-19980210#sec-element-content and http://www.w3.org/TR/1998/REC-xml-19980210#sec-mixed-content).</para></block></summary>
		Indented = 1,
	} // Formatting

} // System.Xml
