// WriteState.cs
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
	/// <para> Specifies the state of an instance of the <see cref="T:System.Xml.XmlTextWriter" /> class.
	///    </para>
	/// </summary>
	public enum WriteState {

		/// <summary><para> An instance of the <see cref="!:System.Xml.XmlTextWriter.XmlTextWriter" qualify="true" /> class is initialized 
		///    but none of the writing methods nor the <see cref="M:System.Xml.XmlTextWriter.Close" qualify="true" />
		///    method have been called.
		///    </para><para> The <see cref="M:System.Xml.XmlTextWriter.WriteEndDocument" qualify="true" /> method resets
		/// the <see cref="P:System.Xml.XmlTextWriter.WriteState" qualify="true" /> to this
		/// value. </para></summary>
		Start = 0,

		/// <summary><para> The XML declaration is being written.
		///       </para></summary>
		Prolog = 1,

		/// <summary><para> An element start tag is being written.
		///       </para></summary>
		Element = 2,

		/// <summary><para> An attribute value is being written.
		///       </para></summary>
		Attribute = 3,

		/// <summary><para> Element content is being written.
		///       </para></summary>
		Content = 4,

		/// <summary><para> The <see cref="M:System.Xml.XmlTextWriter.Close" /> method has been called.
		///    </para></summary>
		Closed = 5,
	} // WriteState

} // System.Xml
