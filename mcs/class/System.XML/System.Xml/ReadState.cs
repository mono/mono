// ReadState.cs
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
	/// <para>Specifies the state of an instance of a class derived 
	///       from the <see cref="T:System.Xml.XmlReader" /> class.</para>
	/// </summary>
	public enum ReadState {

		/// <summary><para>The <see cref="M:System.Xml.XmlReader.Read" /> method has not been called.</para></summary>
		Initial = 0,

		/// <summary><para>The <see cref="M:System.Xml.XmlReader.Read" /> method
		///    has been called. Additional methods may be called on the reader.</para></summary>
		Interactive = 1,

		/// <summary>
		///     An error occurred that prevents the
		///     read operation from continuing.
		///  </summary>
		Error = 2,

		/// <summary><para> The end of the file has been
		///       reached.</para></summary>
		EndOfFile = 3,

		/// <summary><para>The <see cref="M:System.Xml.XmlReader.Close" /> method has been called.</para></summary>
		Closed = 4,
	} // ReadState

} // System.Xml
