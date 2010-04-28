//
// System.Xml.XmlDocument
//
// Authors:
//   Daniel Weber (daniel-weber@austin.rr.com)
//   Kral Ferch <kral_ferch@hotmail.com>
//   Jason Diamond <jason@injektilo.org>
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2001 Daniel Weber
// (C) 2002 Kral Ferch, Jason Diamond, Miguel de Icaza, Duncan Mak,
//   Atsushi Enomoto
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections;
using Mono.Xml;
#if NET_2_0
using System.Xml.Schema;
using Mono.Xml.XPath;
#endif

namespace System.Xml
{
	public class XmlDocument : XmlNode, IHasXmlChildNode
	{
		#region Fields
		static readonly Type [] optimal_create_types = new Type [] {typeof (string), typeof (string), typeof (string)};
		bool optimal_create_element, optimal_create_attribute;

		XmlNameTable nameTable;
		string baseURI = String.Empty;
		XmlImplementation implementation;
		bool preserveWhitespace = false;
		XmlResolver resolver;
		Hashtable idTable = new Hashtable ();
		XmlNameEntryCache nameCache;
		XmlLinkedNode lastLinkedChild;
		XmlAttribute nsNodeXml;
#if NET_2_0
		XmlSchemaSet schemas;
		IXmlSchemaInfo schemaInfo;
#endif

		// MS.NET rejects undeclared entities _only_ during Load(),
		// while ReadNode() never rejects such node. So it signs
		// whether we are on Load() or not (MS.NET uses Loader class,
		// but we don't have to implement Load() as such)
		bool loadMode;

		#endregion

		#region Constructors

		public XmlDocument () : this (null, null)
		{
		}

		protected internal XmlDocument (XmlImplementation imp) : this (imp, null)
		{
		}

		public XmlDocument (XmlNameTable nt) : this (null, nt)
		{
		}

		XmlDocument (XmlImplementation impl, XmlNameTable nt) : base (null)
		{
			if (impl == null)
				implementation = new XmlImplementation ();
			else
				implementation = impl;

			nameTable = (nt != null) ? nt : implementation.InternalNameTable;
			nameCache = new XmlNameEntryCache (nameTable);
			AddDefaultNameTableKeys ();
			resolver = new XmlUrlResolver ();
			
			Type type = GetType ();
			optimal_create_element = type.GetMethod ("CreateElement", optimal_create_types).DeclaringType == typeof (XmlDocument);
			optimal_create_attribute = type.GetMethod ("CreateAttribute", optimal_create_types).DeclaringType == typeof (XmlDocument);
		}
		#endregion

		#region Events

		public event XmlNodeChangedEventHandler NodeChanged;

		public event XmlNodeChangedEventHandler NodeChanging;

		public event XmlNodeChangedEventHandler NodeInserted;

		public event XmlNodeChangedEventHandler NodeInserting;

		public event XmlNodeChangedEventHandler NodeRemoved;

		public event XmlNodeChangedEventHandler NodeRemoving;

		#endregion

		#region Properties

		internal XmlAttribute NsNodeXml {
			get {
				if (nsNodeXml == null) {
					nsNodeXml = CreateAttribute ("xmlns", "xml", "http://www.w3.org/2000/xmlns/");
					nsNodeXml.Value = "http://www.w3.org/XML/1998/namespace";
				}
				return nsNodeXml;
			}
		}

		XmlLinkedNode IHasXmlChildNode.LastLinkedChild {
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
		}

		public override string BaseURI {
			get {
				return baseURI;
			}
		}

		public XmlElement DocumentElement {
			get {
				XmlNode node = FirstChild;

				while (node != null) {
					if (node is XmlElement)
						break;
					node = node.NextSibling;
				}

				return node != null ? node as XmlElement : null;
			}
		}

		public virtual XmlDocumentType DocumentType {
			get {
				for (XmlNode n = FirstChild; n != null; n = n.NextSibling) {
					if (n.NodeType == XmlNodeType.DocumentType)
						return (XmlDocumentType) n;
					else if (n.NodeType == XmlNodeType.Element) // document element
						return null;
				}

				return null;
			}
		}

		public XmlImplementation Implementation {
			get { return implementation; }
		}

		public override string InnerXml {
			get {
				return base.InnerXml;
			}
			set {	// reason for overriding
				this.LoadXml (value);
			}
		}

		public override bool IsReadOnly {
			get { return false; }
		}

		internal bool IsStandalone {
			get {
				return FirstChild != null &&
					FirstChild.NodeType == XmlNodeType.XmlDeclaration &&
					((XmlDeclaration) this.FirstChild).Standalone == "yes";
			}
		}

		public override string LocalName {
			get { return "#document"; }
		}

		public override string Name {
			get { return "#document"; }
		}

		internal XmlNameEntryCache NameCache {
			get { return nameCache; }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Document; }
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Root;
			}
		}

		public override XmlDocument OwnerDocument {
			get { return null; }
		}

		public bool PreserveWhitespace {
			get { return preserveWhitespace; }
			set { preserveWhitespace = value; }
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}

		internal override string XmlLang {
			get { return String.Empty; }
		}

		public virtual XmlResolver XmlResolver {
			set { resolver = value; }
		}

		internal override XmlSpace XmlSpace {
			get {
				return XmlSpace.None;
			}
		}
		
		internal Encoding TextEncoding {
			get {
				XmlDeclaration dec = FirstChild as XmlDeclaration;
			
				if (dec == null || dec.Encoding == "")
					return null;
				
				return Encoding.GetEncoding (dec.Encoding);
			}
		}

#if NET_2_0
		public override XmlNode ParentNode {
			get { return null; }
		}

		public XmlSchemaSet Schemas {
			get {
				if (schemas == null)
					schemas = new XmlSchemaSet ();
				return schemas;
			}
			set { schemas = value; }
		}

		public override IXmlSchemaInfo SchemaInfo {
			get { return schemaInfo; }
			internal set { schemaInfo = value; }
		}
#endif

		#endregion

		#region Methods
		internal void AddIdenticalAttribute (XmlAttribute attr)
		{
			idTable [attr.Value] = attr;
		}

		public override XmlNode CloneNode (bool deep)
		{
			XmlDocument doc = implementation != null ? implementation.CreateDocument () : new XmlDocument ();
			doc.baseURI = baseURI;

			if(deep)
			{
				for (XmlNode n = FirstChild; n != null; n = n.NextSibling)
					doc.AppendChild (doc.ImportNode (n, deep), false);
			}
			return doc;
		}

		public XmlAttribute CreateAttribute (string name)
		{
			string prefix;
			string localName;
			string namespaceURI = String.Empty;

			ParseName (name, out prefix, out localName);

			if (prefix == "xmlns" || (prefix == "" && localName == "xmlns"))
				namespaceURI = XmlNamespaceManager.XmlnsXmlns;
			else if (prefix == "xml")
				namespaceURI = XmlNamespaceManager.XmlnsXml;

			return CreateAttribute (prefix, localName, namespaceURI );
		}

		public XmlAttribute CreateAttribute (string qualifiedName, string namespaceURI)
		{
			string prefix;
			string localName;

			ParseName (qualifiedName, out prefix, out localName);

			return CreateAttribute (prefix, localName, namespaceURI);
		}

		public virtual XmlAttribute CreateAttribute (string prefix, string localName, string namespaceURI)
		{
			if ((localName == null) || (localName == String.Empty))
				throw new ArgumentException ("The attribute local name cannot be empty.");

			return new XmlAttribute (prefix, localName, namespaceURI, this, false, true);
		}

		internal XmlAttribute CreateAttribute (string prefix, string localName, string namespaceURI, bool atomizedNames, bool checkNamespace)
		{
			if (optimal_create_attribute)
				return new XmlAttribute (prefix, localName, namespaceURI, this, atomizedNames, checkNamespace);
			else
				return CreateAttribute (prefix, localName, namespaceURI);
		}

		public virtual XmlCDataSection CreateCDataSection (string data)
		{
			return new XmlCDataSection (data, this);
		}

		public virtual XmlComment CreateComment (string data)
		{
			return new XmlComment (data, this);
		}

		protected internal virtual XmlAttribute CreateDefaultAttribute (string prefix, string localName, string namespaceURI)
		{
			XmlAttribute attr = CreateAttribute (prefix, localName, namespaceURI);
			attr.isDefault = true;
			return attr;
		}

		public virtual XmlDocumentFragment CreateDocumentFragment ()
		{
			return new XmlDocumentFragment (this);
		}

		[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual XmlDocumentType CreateDocumentType (string name, string publicId,
								   string systemId, string internalSubset)
		{
			return new XmlDocumentType (name, publicId, systemId, internalSubset, this);
		}

		private XmlDocumentType CreateDocumentType (DTDObjectModel dtd)
		{
			return new XmlDocumentType (dtd, this);
		}

		public XmlElement CreateElement (string name)
		{
			return CreateElement (name, String.Empty);
		}

		public XmlElement CreateElement (
			string qualifiedName, 
			string namespaceURI)
		{
			string prefix;
			string localName;

			ParseName (qualifiedName, out prefix, out localName);
			
			return CreateElement (prefix, localName, namespaceURI);
		}

		public virtual XmlElement CreateElement (
			string prefix,
			string localName,
			string namespaceURI)
		{
			// LAMESPEC: MS.NET has a weird behavior that they can Load() from XmlTextReader 
			// whose Namespaces = false, but their CreateElement() never allows qualified name.
			// I leave it as it is.
			return new XmlElement (prefix != null ? prefix : String.Empty, localName, namespaceURI != null ? namespaceURI : String.Empty, this, false);
		}

		internal XmlElement CreateElement (
			string prefix,
			string localName,
			string namespaceURI,
			bool nameAtomized)
		{
			if ((localName == null) || (localName == String.Empty))
				throw new ArgumentException ("The local name for elements or attributes cannot be null or an empty string.");
			if (optimal_create_element)
				// LAMESPEC: MS.NET has a weird behavior that they can Load() from XmlTextReader 
				// whose Namespaces = false, but their CreateElement() never allows qualified name.
				// I leave it as it is.
				return new XmlElement (prefix != null ? prefix : String.Empty, localName, namespaceURI != null ? namespaceURI : String.Empty, this, nameAtomized);
			else
				return CreateElement (prefix, localName, namespaceURI);
		}

		public virtual XmlEntityReference CreateEntityReference (string name)
		{
			return new XmlEntityReference (name, this);
		}

#if NET_2_0
		public override XPathNavigator CreateNavigator ()
		{
			return CreateNavigator (this);
		}
#endif

		protected internal virtual XPathNavigator CreateNavigator (XmlNode node)
		{
#if NET_2_0
			return new XPathEditableDocument (node).CreateNavigator ();
#else
			return new XmlDocumentNavigator (node);
#endif
		}

		public virtual XmlNode CreateNode (
			string nodeTypeString,
			string name,
			string namespaceURI)
		{
			return CreateNode (GetNodeTypeFromString (nodeTypeString), name, namespaceURI);
		}

		public virtual XmlNode CreateNode (
			XmlNodeType type,
			string name,
			string namespaceURI)
		{
			string prefix = null;
			string localName = name;

			if ((type == XmlNodeType.Attribute) || (type == XmlNodeType.Element) || (type == XmlNodeType.EntityReference))
				ParseName (name, out prefix, out localName);
			
			return CreateNode (type, prefix, localName, namespaceURI);
		}

		public virtual XmlNode CreateNode (
			XmlNodeType type,
			string prefix,
			string name,
			string namespaceURI)
		{
			switch (type) {
				case XmlNodeType.Attribute: return CreateAttribute (prefix, name, namespaceURI);
				case XmlNodeType.CDATA: return CreateCDataSection (null);
				case XmlNodeType.Comment: return CreateComment (null);
				case XmlNodeType.Document: return new XmlDocument ();
				case XmlNodeType.DocumentFragment: return CreateDocumentFragment ();
				case XmlNodeType.DocumentType: return CreateDocumentType (null, null, null, null);
				case XmlNodeType.Element: return CreateElement (prefix, name, namespaceURI);
				case XmlNodeType.EntityReference: return CreateEntityReference (null);
				case XmlNodeType.ProcessingInstruction: return CreateProcessingInstruction (null, null);
				case XmlNodeType.SignificantWhitespace: return CreateSignificantWhitespace (String.Empty);
				case XmlNodeType.Text: return CreateTextNode (null);
				case XmlNodeType.Whitespace: return CreateWhitespace (String.Empty);
				case XmlNodeType.XmlDeclaration: return CreateXmlDeclaration ("1.0", null, null);
				default:
#if NET_2_0
					throw new ArgumentException (
#else // makes less sense
					throw new ArgumentOutOfRangeException (
#endif
						String.Format("{0}\nParameter name: {1}",
							 "Specified argument was out of the range of valid values", type.ToString ()));
			}
		}

		public virtual XmlProcessingInstruction CreateProcessingInstruction (
			string target,
			string data)
		{
			return new XmlProcessingInstruction (target, data, this);
		}

		public virtual XmlSignificantWhitespace CreateSignificantWhitespace (string text)
		{
			if (!XmlChar.IsWhitespace (text))
				    throw new ArgumentException ("Invalid whitespace characters.");
			 
			return new XmlSignificantWhitespace (text, this);
		}

		public virtual XmlText CreateTextNode (string text)
		{
			return new XmlText (text, this);
		}

		public virtual XmlWhitespace CreateWhitespace (string text)
		{
			if (!XmlChar.IsWhitespace (text))
			    throw new ArgumentException ("Invalid whitespace characters.");
			 
			return new XmlWhitespace (text, this);
		}

		public virtual XmlDeclaration CreateXmlDeclaration (string version, string encoding,
								    string standalone)
		{
			if (version != "1.0")
				throw new ArgumentException ("version string is not correct.");

			if  ((standalone != null && standalone != String.Empty) && !((standalone == "yes") || (standalone == "no")))
				throw new ArgumentException ("standalone string is not correct.");

			return new XmlDeclaration (version, encoding, standalone, this);
		}

		// FIXME: Currently XmlAttributeCollection.SetNamedItem() does
		// add to the identity table, but in fact I delayed identity
		// check on GetIdenticalAttribute. To make such way complete,
		// we have to use MultiMap, not Hashtable.
		//
		// Well, MS.NET is also fragile around here.
		public virtual XmlElement GetElementById (string elementId)
		{
			XmlAttribute attr = GetIdenticalAttribute (elementId);
			return attr != null ? attr.OwnerElement : null;
		}

		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.SearchDescendantElements (name, name == "*", nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.SearchDescendantElements (localName, localName == "*", namespaceURI, namespaceURI == "*", nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private XmlNodeType GetNodeTypeFromString (string nodeTypeString)
		{
#if NET_2_0 // actually should be done in any version
			if (nodeTypeString == null)
				throw new ArgumentNullException ("nodeTypeString");
#endif
			switch (nodeTypeString) {
				case "attribute": return XmlNodeType.Attribute;
				case "cdatasection": return XmlNodeType.CDATA;
				case "comment": return XmlNodeType.Comment;
				case "document": return XmlNodeType.Document;
				case "documentfragment": return XmlNodeType.DocumentFragment;
				case "documenttype": return XmlNodeType.DocumentType;
				case "element": return XmlNodeType.Element;
				case "entityreference": return XmlNodeType.EntityReference;
				case "processinginstruction": return XmlNodeType.ProcessingInstruction;
				case "significantwhitespace": return XmlNodeType.SignificantWhitespace;
				case "text": return XmlNodeType.Text;
				case "whitespace": return XmlNodeType.Whitespace;
				default:
					throw new ArgumentException(String.Format("The string doesn't represent any node type : {0}.", nodeTypeString));
			}
		}

		internal XmlAttribute GetIdenticalAttribute (string id)
		{
			XmlAttribute attr = this.idTable [id] as XmlAttribute;
			if (attr == null)
				return null;
			if (attr.OwnerElement == null || !attr.OwnerElement.IsRooted) {
//				idTable.Remove (id);
				return null;
			}
			return attr;
		}

		public virtual XmlNode ImportNode (XmlNode node, bool deep)
		{
			if (node == null)
				throw new NullReferenceException ("Null node cannot be imported.");

			switch (node.NodeType) {
			case XmlNodeType.Attribute:
				XmlAttribute srcAtt = node as XmlAttribute;
				XmlAttribute dstAtt = this.CreateAttribute (srcAtt.Prefix, srcAtt.LocalName, srcAtt.NamespaceURI);
				for (XmlNode n = srcAtt.FirstChild; n != null; n = n.NextSibling)
					dstAtt.AppendChild (this.ImportNode (n, deep));
				return dstAtt;

			case XmlNodeType.CDATA:
				return this.CreateCDataSection (node.Value);

			case XmlNodeType.Comment:
				return this.CreateComment (node.Value);

			case XmlNodeType.Document:
				throw new XmlException ("Document cannot be imported.");

			case XmlNodeType.DocumentFragment:
				XmlDocumentFragment df = this.CreateDocumentFragment ();
				if(deep)
					for (XmlNode n = node.FirstChild; n != null; n = n.NextSibling)
						df.AppendChild (this.ImportNode (n, deep));
				return df;

			case XmlNodeType.DocumentType:
				throw new XmlException ("DocumentType cannot be imported.");

			case XmlNodeType.Element:
				XmlElement src = (XmlElement)node;
				XmlElement dst = this.CreateElement (src.Prefix, src.LocalName, src.NamespaceURI);
				for (int i = 0; i < src.Attributes.Count; i++) {
					XmlAttribute attr = src.Attributes [i];
					if(attr.Specified)	// copies only specified attributes
						dst.SetAttributeNode ((XmlAttribute) this.ImportNode (attr, deep));
				}
				if(deep)
					for (XmlNode n = src.FirstChild; n != null; n = n.NextSibling)
						dst.AppendChild (this.ImportNode (n, deep));
				return dst;

			case XmlNodeType.EndElement:
				throw new XmlException ("Illegal ImportNode call for NodeType.EndElement");
			case XmlNodeType.EndEntity:
				throw new XmlException ("Illegal ImportNode call for NodeType.EndEntity");

			case XmlNodeType.EntityReference:
				return this.CreateEntityReference (node.Name);

			case XmlNodeType.None:
				throw new XmlException ("Illegal ImportNode call for NodeType.None");

			case XmlNodeType.ProcessingInstruction:
				XmlProcessingInstruction pi = node as XmlProcessingInstruction;
				return this.CreateProcessingInstruction (pi.Target, pi.Data);

			case XmlNodeType.SignificantWhitespace:
				return this.CreateSignificantWhitespace (node.Value);

			case XmlNodeType.Text:
				return this.CreateTextNode (node.Value);

			case XmlNodeType.Whitespace:
				return this.CreateWhitespace (node.Value);

			case XmlNodeType.XmlDeclaration:
				XmlDeclaration srcDecl = node as XmlDeclaration;
				return this.CreateXmlDeclaration (srcDecl.Version, srcDecl.Encoding, srcDecl.Standalone);

			default:
				throw new InvalidOperationException ("Cannot import specified node type: " + node.NodeType);
			}
		}

		public virtual void Load (Stream inStream)
		{
			XmlTextReader reader = new XmlTextReader (inStream, NameTable);
			reader.XmlResolver = resolver;
			XmlValidatingReader vr = new XmlValidatingReader (reader);
			vr.EntityHandling = EntityHandling.ExpandCharEntities;
			vr.ValidationType = ValidationType.None;
			Load (vr);
		}

		public virtual void Load (string filename)
		{
			XmlTextReader xr = null;
			try {
				xr = new XmlTextReader (filename, NameTable);
				xr.XmlResolver = resolver;
				XmlValidatingReader vr = new XmlValidatingReader (xr);
				vr.EntityHandling = EntityHandling.ExpandCharEntities;
				vr.ValidationType = ValidationType.None;
				Load (vr);
			} finally {
				if (xr != null)
					xr.Close ();
			}
		}

		public virtual void Load (TextReader txtReader)
		{
			XmlTextReader xr = new XmlTextReader (txtReader, NameTable);
			XmlValidatingReader vr = new XmlValidatingReader (xr);
			vr.EntityHandling = EntityHandling.ExpandCharEntities;
			vr.ValidationType = ValidationType.None;
			xr.XmlResolver = resolver;
			Load (vr);
		}

		public virtual void Load (XmlReader xmlReader)
		{
			// Reset our document
			// For now this just means removing all our children but later this
			// may turn out o need to call a private method that resets other things
			// like properties we have, etc.
			RemoveAll ();

			this.baseURI = xmlReader.BaseURI;
			// create all contents with use of ReadNode()
			try {
				loadMode = true;
				do {
					XmlNode n = ReadNode (xmlReader);
					if (n == null)
						break;
					if (preserveWhitespace || n.NodeType != XmlNodeType.Whitespace)
						AppendChild (n, false);
				} while (xmlReader.NodeType != XmlNodeType.EndElement);
#if NET_2_0
				if (xmlReader.Settings != null)
					schemas = xmlReader.Settings.Schemas;
#endif
			} finally {
				loadMode = false;
			}
		}

		public virtual void LoadXml (string xml)
		{
			XmlTextReader xmlReader = new XmlTextReader (
				xml,
				XmlNodeType.Document,
				new XmlParserContext (NameTable, new XmlNamespaceManager (NameTable), null, XmlSpace.None));
			try {
				xmlReader.XmlResolver = resolver;
				Load (xmlReader);
			} finally {
				xmlReader.Close ();
			}
		}

		internal void onNodeChanged (XmlNode node, XmlNode parent, string oldValue, string newValue)
		{
			if (NodeChanged != null)
				NodeChanged (node, new XmlNodeChangedEventArgs
					(node, parent, parent, oldValue, newValue, XmlNodeChangedAction.Change));
		}

		internal void onNodeChanging(XmlNode node, XmlNode parent, string oldValue, string newValue)
		{
			if (node.IsReadOnly)
				throw new ArgumentException ("Node is read-only.");
			if (NodeChanging != null)
				NodeChanging (node, new XmlNodeChangedEventArgs
					(node, parent, parent, oldValue, newValue, XmlNodeChangedAction.Change));
		}

		internal void onNodeInserted (XmlNode node, XmlNode newParent)
		{
			if (NodeInserted != null)
				NodeInserted (node, new XmlNodeChangedEventArgs
					(node, null, newParent, null, null, XmlNodeChangedAction.Insert));
		}

		internal void onNodeInserting (XmlNode node, XmlNode newParent)
		{
			if (NodeInserting != null)
				NodeInserting (node, new XmlNodeChangedEventArgs
					(node, null, newParent, null, null, XmlNodeChangedAction.Insert));
		}

		internal void onNodeRemoved (XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoved != null)
				NodeRemoved (node, new XmlNodeChangedEventArgs
					(node, oldParent, null, null, null, XmlNodeChangedAction.Remove));
		}

		internal void onNodeRemoving (XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoving != null)
				NodeRemoving (node, new XmlNodeChangedEventArgs
					(node, oldParent, null, null, null, XmlNodeChangedAction.Remove));
		}

		private void ParseName (string name, out string prefix, out string localName)
		{
			int indexOfColon = name.IndexOf (':');
			
			if (indexOfColon != -1) {
				prefix = name.Substring (0, indexOfColon);
				localName = name.Substring (indexOfColon + 1);
			} else {
				prefix = "";
				localName = name;
			}
		}

		// Reads XmlReader and creates Attribute Node.
		private XmlAttribute ReadAttributeNode (XmlReader reader)
		{
			if (reader.NodeType == XmlNodeType.Element)
				reader.MoveToFirstAttribute ();
			else if (reader.NodeType != XmlNodeType.Attribute)
				throw new InvalidOperationException (MakeReaderErrorMessage ("bad position to read attribute.", reader));
			XmlAttribute attribute = CreateAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI);
#if NET_2_0
			if (reader.SchemaInfo != null)
				SchemaInfo = new XmlSchemaInfo (reader.SchemaInfo);
#endif
			bool isDefault = reader.IsDefault;

			ReadAttributeNodeValue (reader, attribute);

			if (isDefault)
				attribute.SetDefault ();

			return attribute;
		}

		// Reads attribute from XmlReader and then creates attribute value children. XmlAttribute also uses this.
		internal void ReadAttributeNodeValue (XmlReader reader, XmlAttribute attribute)
		{
			while (reader.ReadAttributeValue ()) {
				if (reader.NodeType == XmlNodeType.EntityReference)
					attribute.AppendChild (CreateEntityReference (reader.Name), false); // omit node type check
				else
					// Children of Attribute is restricted to CharacterData and EntityReference (Comment is not allowed).
					attribute.AppendChild (CreateTextNode (reader.Value), false); // omit node type check
			}
		}

		[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual XmlNode ReadNode (XmlReader reader)
		{
			if (PreserveWhitespace)
				return ReadNodeCore (reader);
			XmlTextReader xtr = reader as XmlTextReader;
			if (xtr != null && xtr.WhitespaceHandling ==
			    WhitespaceHandling.All) {
				try {
					xtr.WhitespaceHandling = WhitespaceHandling.Significant;
					return ReadNodeCore (reader);
				} finally {
					xtr.WhitespaceHandling = WhitespaceHandling.All;
				}
			}
			else
				return ReadNodeCore (reader);
		}

		XmlNode ReadNodeCore (XmlReader reader)
		{
			switch (reader.ReadState) {
			case ReadState.Interactive:
				break;
			case ReadState.Initial:
#if NET_2_0
				if (reader.SchemaInfo != null)
					this.SchemaInfo = new XmlSchemaInfo (reader.SchemaInfo);
#endif
				reader.Read ();
				break;
			default:
				return null;
			}

			XmlNode n;
			switch (reader.NodeType) {

			case XmlNodeType.Attribute:
				string localName = reader.LocalName;
				string ns = reader.NamespaceURI;
				n = ReadAttributeNode (reader);
				// Keep the current reader position on attribute.
				reader.MoveToAttribute (localName, ns);
				return n;

			case XmlNodeType.CDATA:
				n = CreateCDataSection (reader.Value);
				break;

			case XmlNodeType.Comment:
				n = CreateComment (reader.Value);
				break;

			case XmlNodeType.Element:
				XmlElement element = CreateElement (reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.NameTable == this.NameTable);
#if NET_2_0
				if (reader.SchemaInfo != null)
					SchemaInfo = new XmlSchemaInfo (reader.SchemaInfo);
#endif
				element.IsEmpty = reader.IsEmptyElement;

				// set the element's attributes.
				for (int i = 0; i < reader.AttributeCount; i++) {
					reader.MoveToAttribute (i);
					element.SetAttributeNode (
						ReadAttributeNode (reader));
					reader.MoveToElement ();
				}
				// FIXME: the code below should be fine and
				// in some XmlReaders it is much faster, but
				// caused some breakage in sys.data test.
				/*
				if (reader.MoveToFirstAttribute ()) {
					do {
						element.SetAttributeNode (ReadAttributeNode (reader));
					} while (reader.MoveToNextAttribute ());
					reader.MoveToElement ();
				}
				*/
				reader.MoveToElement ();

				int depth = reader.Depth;

				if (reader.IsEmptyElement) {
					n = element;
					break;
				}

				reader.Read ();
				while (reader.Depth > depth) {
					n = ReadNodeCore (reader);
					if (preserveWhitespace || n.NodeType != XmlNodeType.Whitespace)
						element.AppendChild (n, false);
				}
				n = element;
				break;

			case XmlNodeType.ProcessingInstruction:
				n = CreateProcessingInstruction (reader.Name, reader.Value);
				break;

			case XmlNodeType.Text:
				n = CreateTextNode (reader.Value);
				break;

			case XmlNodeType.XmlDeclaration:
				n = CreateXmlDeclaration ("1.0" , String.Empty, String.Empty);
				n.Value = reader.Value;
				break;

			case XmlNodeType.DocumentType:
				DTDObjectModel dtd = null;
				IHasXmlParserContext ctxReader = reader as IHasXmlParserContext;
				if (ctxReader != null)
					dtd = ctxReader.ParserContext.Dtd;

				if (dtd != null)
					n = CreateDocumentType (dtd);
				else
					n = CreateDocumentType (reader.Name, reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
				break;

			case XmlNodeType.EntityReference:
				if (this.loadMode && this.DocumentType != null &&
					DocumentType.Entities.GetNamedItem (reader.Name) == null)
					throw new XmlException ("Reference to undeclared entity was found.");

				n = CreateEntityReference (reader.Name);
				// IF argument XmlReader can resolve entity,
				// ReadNode() also fills children _from it_.
				// In this case, it is not from doctype node.
				// (it is kind of sucky design, but it happens
				// anyways when we modify doctype node).
				//
				// It does not happen when !CanResolveEntity.
				// (In such case AppendChild() will resolve
				// entity content, as XmlEntityReference does.)
				if (reader.CanResolveEntity)
				{
					reader.ResolveEntity ();
					reader.Read ();
					for (XmlNode child; reader.NodeType != XmlNodeType.EndEntity && (child = ReadNode (reader)) != null;)
						n.InsertBefore (child, null, false, false);
				}
				break;

			case XmlNodeType.SignificantWhitespace:
				n = CreateSignificantWhitespace (reader.Value);
				break;

			case XmlNodeType.Whitespace:
				n = CreateWhitespace (reader.Value);
				break;

			case XmlNodeType.None:
				return null;

			default:
				// No idea why MS does throw NullReferenceException ;-P
				throw new NullReferenceException ("Unexpected node type " + reader.NodeType + ".");
			}

			reader.Read ();
			return n;
		}

		private string MakeReaderErrorMessage (string message, XmlReader reader)
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			if (li != null)
				return String.Format (CultureInfo.InvariantCulture, "{0} Line number = {1}, Inline position = {2}.", message, li.LineNumber, li.LinePosition);
			else
				return message;
		}

		internal void RemoveIdenticalAttribute (string id)
		{
			idTable.Remove (id);
		}

		public virtual void Save (Stream outStream)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (outStream, TextEncoding);
			if (!PreserveWhitespace)
				xmlWriter.Formatting = Formatting.Indented;
			WriteContentTo (xmlWriter);
			xmlWriter.Flush ();
		}

		public virtual void Save (string filename)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (filename, TextEncoding);
			try {
				if (!PreserveWhitespace)
					xmlWriter.Formatting = Formatting.Indented;
				WriteContentTo (xmlWriter);
			} finally {
				xmlWriter.Close ();
			}
		}

		public virtual void Save (TextWriter writer)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (writer);
			if (!PreserveWhitespace)
				xmlWriter.Formatting = Formatting.Indented;
			if (FirstChild != null && FirstChild.NodeType != XmlNodeType.XmlDeclaration)
				xmlWriter.WriteStartDocument ();
			WriteContentTo (xmlWriter);
			xmlWriter.WriteEndDocument ();
			xmlWriter.Flush ();
		}

		public virtual void Save (XmlWriter xmlWriter)
		{
			//
			// This should preserve white space if PreserveWhiteSpace is true
			//
			bool autoXmlDecl = FirstChild != null && FirstChild.NodeType != XmlNodeType.XmlDeclaration;
			if (autoXmlDecl)
				xmlWriter.WriteStartDocument ();
			WriteContentTo (xmlWriter);
			if (autoXmlDecl)
				xmlWriter.WriteEndDocument ();
			xmlWriter.Flush ();
		}

		public override void WriteContentTo (XmlWriter w)
		{
			for (XmlNode n = FirstChild; n != null; n = n.NextSibling)
				n.WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			WriteContentTo (w);
		}

		private void AddDefaultNameTableKeys ()
		{
			// The following keys are default of MS .NET Framework
			nameTable.Add ("#text");
			nameTable.Add ("xml");
			nameTable.Add ("xmlns");
			nameTable.Add ("#entity");
			nameTable.Add ("#document-fragment");
			nameTable.Add ("#comment");
			nameTable.Add ("space");
			nameTable.Add ("id");
			nameTable.Add ("#whitespace");
			nameTable.Add ("http://www.w3.org/2000/xmlns/");
			nameTable.Add ("#cdata-section");
			nameTable.Add ("lang");
			nameTable.Add ("#document");
			nameTable.Add ("#significant-whitespace");
		}

		internal void CheckIdTableUpdate (XmlAttribute attr, string oldValue, string newValue)
		{
			if (idTable [oldValue] == attr) {
				idTable.Remove (oldValue);
				idTable [newValue] = attr;
			}
		}

#if NET_2_0
		public void Validate (ValidationEventHandler handler)
		{
			Validate (handler, this,
				XmlSchemaValidationFlags.ProcessIdentityConstraints);
		}

		public void Validate (ValidationEventHandler handler,
			XmlNode node)
		{
			Validate (handler, node,
				XmlSchemaValidationFlags.ProcessIdentityConstraints);
		}

		private void Validate (ValidationEventHandler handler,
			XmlNode node, XmlSchemaValidationFlags flags)
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.NameTable = NameTable;
			settings.Schemas = schemas;
			settings.Schemas.XmlResolver = resolver;
			settings.XmlResolver = resolver;
			settings.ValidationFlags = flags;
			settings.ValidationType = ValidationType.Schema;
			XmlReader r = XmlReader.Create (
				new XmlNodeReader (node), settings);
			while (!r.EOF)
				r.Read ();
		}
#endif

		#endregion
	}
}
