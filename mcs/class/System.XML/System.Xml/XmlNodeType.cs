// XmlNodeType.cs
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
	/// <para>Specifies the type of node.</para>
	/// </summary>
	public enum XmlNodeType {

		/// <summary><para>This is returned by the <see cref="T:System.Xml.XmlReader" /> if a <see langword="Read" /> 
		/// method has not been called or if no more nodes
		/// are available to be read.</para></summary>
		None = 0,

		/// <summary><para> An element.
		///       </para><para> Example XML: <c>&lt;item&gt;</c></para>
		/// An <see langword="Element" /> node can have the
		/// following child node types: <see langword="Element" />, <see langword="Text" />,
		/// <see langword="Comment" />, <see langword="ProcessingInstruction" />, 
		/// <see langword="CDATA" />, and <see langword="EntityReference" />. It can be the 
		/// child of the <see langword="Document" />, <see langword="DocumentFragment" />,
		/// <see langword="EntityReference" />, and <see langword="Element" />
		/// nodes.
		/// </summary>
		Element = 1,

		/// <summary><para> An
		///  attribute.
		///  </para><para> Example XML: <c>id='123'</c></para><para> An <see langword="Attribute" /> node can have the
		/// following child node types: <see langword="Text" /> and
		/// <see langword="EntityReference" />. The <see langword="Attribute" /> node does not 
		/// appear as the child node of any other node type. It is not considered a
		/// child node of an <see langword="Element" />
		/// .</para></summary>
		Attribute = 2,

		/// <summary><para> The
		///       text content of a node.
		///       </para><para> A <see langword="Text" /> node cannot have any child nodes. 
		///    It can appear as the child node of the <see langword="Attribute" />,
		/// <see langword="DocumentFragment" />, <see langword="Element" />, and 
		/// <see langword="EntityReference" /> 
		/// nodes.
		/// </para></summary>
		Text = 3,

		/// <summary><para>A CDATA 
		///       
		///       section.</para><para>Example XML: <c>&lt;![CDATA[my 
		///       escaped text]]&gt;</c></para><para> 
		///       CDATA sections are used to escape blocks of text that would otherwise
		///       be recognized as markup. A <see langword="CDATA" /> node cannot have any child
		///       nodes. It can appear as the child of the <see langword="DocumentFragment" />,
		///    <see langword="EntityReference" />, and <see langword="Element" /> nodes.</para></summary>
		CDATA = 4,

		/// <summary><para>A reference to an entity.</para><para>Example XML: <c>&amp;num;</c></para><para> An <see langword="EntityReference" /> node can have the 
		///    following child node types: <see langword="Element" />,
		/// <see langword="ProcessingInstruction" />, <see langword="Comment" />, 
		/// <see langword="Text" />, <see langword="CDATA" />, and 
		/// <see langword="EntityReference" />. It can appear as the child of the 
		/// <see langword="Attribute" />, <see langword="DocumentFragment" />, 
		/// <see langword="Element" />, and <see langword="EntityReference" /> 
		/// 
		/// nodes.</para></summary>
		EntityReference = 5,

		/// <summary><para>An entity declaration.</para><para>Example XML: <c>&lt;!ENTITY ...&gt;</c></para><para>An <see langword="Entity" /> node can have child nodes 
		///    that represent the expanded entity (for example, <see langword="Text" /> and
		/// <see langword="EntityReference" /> nodes). It can appear as the child of the 
		/// <see langword="DocumentType" /> 
		/// node.</para></summary>
		Entity = 6,

		/// <summary><para> A processing instruction.
		///       </para><para> Example XML: <c>&lt;?pi test?&gt;</c></para><para> A <see langword="ProcessingInstruction" /> node cannot have 
		/// any child nodes. It can appear as the child of the <see langword="Document" />,
		/// <see langword="DocumentFragment" />, <see langword="Element" />, and 
		/// <see langword="EntityReference" /> 
		/// nodes.
		/// </para></summary>
		ProcessingInstruction = 7,

		/// <summary><para> A comment.
		///       </para><para> Example XML: <c>&lt;!-- my comment --&gt;</c></para><para> A <see langword="Comment" /> node cannot have any child 
		/// nodes. It can appear as the child of the <see langword="Document" />,
		/// <see langword="DocumentFragment" />, <see langword="Element" />, and 
		/// <see langword="EntityReference" /> 
		/// nodes.
		/// </para></summary>
		Comment = 8,

		/// <summary><para> A document object that, as the root of the document tree, provides access
		///  to the entire XML document.
		///  </para><para> A <see langword="Document" /> node 
		///  can have the following child node types:
		/// <see langword="XmlDeclaration" /> 
		/// , <see langword="Element" /> (maximum of one),
		/// <see langword="ProcessingInstruction" />, <see langword="Comment" />, and 
		/// <see langword="DocumentType" /> 
		/// . It cannot
		/// appear as the child of any node types.</para></summary>
		Document = 9,

		/// <summary><para> The document type declaration, indicated by the following tag.
		///       </para><para> Example XML: <c>&lt;!DOCTYPE ...&gt;</c></para><para> A <see langword="DocumentType" /> node can have the 
		/// following child node types: <see langword="Notation" /> and
		/// <see langword="Entity" />. It can appear as the child of the 
		/// <see langword="Document" /> 
		/// node.
		/// </para></summary>
		DocumentType = 10,

		/// <summary><para> A document fragment.
		///       </para><para> The <see langword="DocumentFragment" /> node associates a 
		///    node or subtree with a document without actually being contained within the
		///    document. A <see langword="DocumentFragment" /> node can have the following child
		///    node types: <see langword="Element" />, <see langword="ProcessingInstruction" />,
		/// <see langword="Comment" />, <see langword="Text" />, <see langword="CDATA" />, and 
		/// <see langword="EntityReference" /> 
		/// 
		/// . It
		/// cannot appear as the child of any node types.
		/// </para></summary>
		DocumentFragment = 11,

		/// <summary><para> A notation in the document type declaration.
		///       </para><para> Example XML: <c>&lt;!NOTATION ...&gt;</c></para><para> A <see langword="Notation" /> node cannot have any child 
		/// nodes. It can appear as the child of the <see langword="DocumentType" />
		/// node.
		/// </para></summary>
		Notation = 12,

		/// <summary><para>
		///        Whitespace between markup.
		///     </para></summary>
		Whitespace = 13,

		/// <summary><para> Whitespace between markup in a mixed content model or 
		///       whitespace within the <c>xml:space="preserve"</c> scope.
		///    </para></summary>
		SignificantWhitespace = 14,

		/// <summary><para> An end element tag.</para><para>Example XML: <c>&lt;/item&gt;</c></para><para>Returned when <see cref="T:System.Xml.XmlReader" /> gets to the end of an element.</para></summary>
		EndElement = 15,

		/// <summary><para>Returned when <see langword="XmlReader" /> gets to the end of the entity
		///    replacement as a result of a call to <see cref="M:System.Xml.XmlReader.ResolveEntity" />
		///    .</para></summary>
		EndEntity = 16,

		/// <summary><para> The XML declaration.
		///  </para><para> Example XML: <c>&lt;?xml version='1.0'?&gt;</c></para><para> The <see langword="XmlDeclaration" /> 
		/// node must be the first node in the document. It cannot have children. It is a
		/// child of the <see langword="Document" />
		/// node. It can have attributes that provide version
		/// and encoding information.</para></summary>
		XmlDeclaration = 17,
	} // XmlNodeType

} // System.Xml
