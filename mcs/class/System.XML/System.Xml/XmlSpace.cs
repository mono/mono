// XmlSpace.cs
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
	/// <para>Specifies the white
	///       space attribute (xml:space), which indicates whether white space should be preserved in an element.</para>
	/// </summary>
	/// <remarks>
	/// <block subset="none" type="note">
	/// <para>This enumeration is used by instances of the <see cref="T:System.Xml.XmlParserContext" />, 
	///    <see cref="T:System.Xml.XmlTextReader" />, and <see cref="T:System.Xml.XmlTextWriter" /> classes.</para>
	/// </block>
	/// </remarks>
	public enum XmlSpace {

		/// <summary><para>No xml:space attribute is in scope.</para></summary>
		None = 0,

		/// <summary><para> xml:space = 'default' is in scope.</para></summary>
		Default = 1,

		/// <summary><para>xml:space = 'preserve' is in scope.</para></summary>
		Preserve = 2,
	} // XmlSpace

} // System.Xml
