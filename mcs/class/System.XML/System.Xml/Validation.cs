// Validation.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Xml {


	/// <summary>
	///                Specifies the type of validation to perform.
	///             </summary>
	public enum Validation {

		/// <summary>
		/// <para>
		///                   The Auto member does the following:
		///                </para>
		/// <list type="number">
		/// <item>
		/// <term>
		///                         If there is no DTD or schema, it will parse the XML
		///                         without validation.
		///                      </term>
		/// </item>
		/// <item>
		/// <term>
		///                         If there is a DTD defined in a &lt;!DOCTYPE ...&gt;
		///                         declaration, it will load the DTD and process the DTD declarations such that
		///                         default attributes and general entities will be made available. General
		///                         entities are only loaded and parsed if they are used (expanded).
		///                      </term>
		/// </item>
		/// <item>
		/// <term>
		///                         If there is no &lt;!DOCTYPE ...&gt; declaration but
		///                         there is an XSD "schemaLocation" attribute, it will load and process those XSD
		///                         schemas and it will return any default attributes defined in those schemas.
		///                      </term>
		/// </item>
		/// <item>
		/// <term>
		///                         If there is no &lt;!DOCTYPE ...&gt; declaration and no XSD
		///                         "schemaLocation" attribute but there are some namespaces using the MSXML
		///                         "x-schema:" URN prefix, it will load and process those schemas and it will
		///                         return any default attributes defined in those schemas.
		///                      </term>
		/// </item>
		/// </list>
		/// </summary>
		Auto = 0,

		/// <summary>
		/// <para>
		///                   No validation.
		///                </para>
		/// </summary>
		None = 1,

		/// <summary>
		/// <para>
		///                   Validate according to DTD.
		///                </para>
		/// </summary>
		DTD = 2,

		/// <summary>
		/// <para>
		///                   Validate according to XDR and XSD schemas, including inline schemas. An error
		///                   is returned if both XDR and XSD schemas are referenced from the same
		///                   document.
		///                </para>
		/// </summary>
		Schema = 3,
	} // Validation

} // System.Xml
