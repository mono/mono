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
//

using System;
using System.IO;
using System.Text;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections;
using Mono.Xml;
using Mono.Xml.Native;

namespace System.Xml
{
	public class XmlDocument : XmlNode
	{
		#region Fields

		XmlLinkedNode lastLinkedChild;
		XmlNameTable nameTable;
		string baseURI = String.Empty;
		XmlImplementation implementation;
		bool preserveWhitespace = false;
		XmlResolver resolver;
		Hashtable idTable = new Hashtable ();

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
			implementation = (impl != null) ? impl : new XmlImplementation ();
			nameTable = (nt != null) ? nt : implementation.internalNameTable;
			AddDefaultNameTableKeys ();
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

		[MonoTODO("It doesn't have internal subset object model.")]
		public virtual XmlDocumentType DocumentType {
			get {
				foreach(XmlNode n in this.ChildNodes) {
					if(n.NodeType == XmlNodeType.DocumentType)
						return (XmlDocumentType)n;
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

		internal override XmlLinkedNode LastLinkedChild {
			get	{
				return lastLinkedChild;
			}

			set {
				lastLinkedChild = value;
			}
		}
		
		public override string LocalName {
			get { return "#document"; }
		}

		public override string Name {
			get { return "#document"; }
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

		#endregion

		#region Methods
		internal void AddIdenticalAttribute (XmlAttribute attr)
		{
			idTable [attr.Value] = attr;
		}

		public override XmlNode CloneNode (bool deep)
		{
			XmlDocument doc = implementation.CreateDocument ();
			doc.baseURI = baseURI;

			doc.PreserveWhitespace = PreserveWhitespace;	// required?
			if(deep)
			{
				foreach(XmlNode n in ChildNodes)
					doc.AppendChild (doc.ImportNode (n, deep));
			}
			return doc;
		}

		public XmlAttribute CreateAttribute (string name)
		{
			return CreateAttribute (name,
				name == "xmlns" ? "http://www.w3.org/2000/xmlns/" : String.Empty);
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

			return new XmlAttribute (prefix, localName, namespaceURI, this);
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

		public virtual XmlDocumentType CreateDocumentType (string name, string publicId,
								   string systemId, string internalSubset)
		{
			return new XmlDocumentType (name, publicId, systemId, internalSubset, this);
		}

		private XmlDocumentType CreateDocumentType (XmlTextReader reader)
		{
			return new XmlDocumentType (reader, this);
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
			if ((localName == null) || (localName == String.Empty))
				throw new ArgumentException ("The local name for elements or attributes cannot be null or an empty string.");
			CheckName (localName);
			return new XmlElement (prefix != null ? prefix : String.Empty, localName, namespaceURI != null ? namespaceURI : String.Empty, this);
		}

		public virtual XmlEntityReference CreateEntityReference (string name)
		{
			return new XmlEntityReference (name, this);
		}

		protected internal virtual XPathNavigator CreateNavigator (XmlNode node)
		{
			return new XmlDocumentNavigator (node);
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
				case XmlNodeType.Document: return new XmlDocument (); // TODO - test to see which constructor to use, i.e. use existing NameTable or not.
				case XmlNodeType.DocumentFragment: return CreateDocumentFragment ();
				case XmlNodeType.DocumentType: return CreateDocumentType (null, null, null, null);
				case XmlNodeType.Element: return CreateElement (prefix, name, namespaceURI);
				case XmlNodeType.EntityReference: return CreateEntityReference (null);
				case XmlNodeType.ProcessingInstruction: return CreateProcessingInstruction (null, null);
				case XmlNodeType.SignificantWhitespace: return CreateSignificantWhitespace (String.Empty);
				case XmlNodeType.Text: return CreateTextNode (null);
				case XmlNodeType.Whitespace: return CreateWhitespace (String.Empty);
				case XmlNodeType.XmlDeclaration: return CreateXmlDeclaration ("1.0", null, null);
				default: throw new ArgumentOutOfRangeException(String.Format("{0}\nParameter name: {1}",
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
			foreach (char c in text)
				if ((c != ' ') && (c != '\r') && (c != '\n') && (c != '\t'))
				    throw new ArgumentException ("Invalid whitespace characters.");
			 
			return new XmlSignificantWhitespace (text, this);
		}

		public virtual XmlText CreateTextNode (string text)
		{
			return new XmlText (text, this);
		}

		public virtual XmlWhitespace CreateWhitespace (string text)
		{
			foreach (char c in text)
				if ((c != ' ') && (c != '\r') && (c != '\n') && (c != '\t'))
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

		[MonoTODO]
		// FIXME: Currently XmlAttributeCollection.SetNamedItem() does
		// add to the identity table, but in fact I delayed identity
		// check on GetIdenticalAttribute. To make such way complete,
		// we have to use MultiMap, not Hashtable.
		public virtual XmlElement GetElementById (string elementId)
		{
			XmlAttribute attr = GetIdenticalAttribute (elementId);
			return attr != null ? attr.OwnerElement : null;
		}

		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.searchNodesRecursively (this, name, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList){
				if (node.Name.Equals (argName))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argArrayList);
			}
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, string argNamespaceURI, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList){
				if (node.LocalName.Equals (argName) && node.NamespaceURI.Equals (argNamespaceURI))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argNamespaceURI, argArrayList);
			}
		}

		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.searchNodesRecursively (this, localName, namespaceURI, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private XmlNodeType GetNodeTypeFromString (string nodeTypeString)
		{
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

		[MonoTODO("default attributes (of imported doc); Entity; Notation")]
		public virtual XmlNode ImportNode (XmlNode node, bool deep)
		{
			switch(node.NodeType)
			{
				case XmlNodeType.Attribute:
					{
						XmlAttribute src_att = node as XmlAttribute;
						XmlAttribute dst_att = this.CreateAttribute (src_att.Prefix, src_att.LocalName, src_att.NamespaceURI);
						dst_att.Value = src_att.Value;	// always explicitly specified (whether source is specified or not)
						return dst_att;
					}

				case XmlNodeType.CDATA:
					return this.CreateCDataSection (node.Value);

				case XmlNodeType.Comment:
					return this.CreateComment (node.Value);

				case XmlNodeType.Document:
					throw new XmlException ("Document cannot be imported.");

				case XmlNodeType.DocumentFragment:
					{
						XmlDocumentFragment df = this.CreateDocumentFragment ();
						if(deep)
						{
							foreach(XmlNode n in node.ChildNodes)
							{
								df.AppendChild (this.ImportNode (n, deep));
							}
						}
						return df;
					}

				case XmlNodeType.DocumentType:
					throw new XmlException ("DocumentType cannot be imported.");

				case XmlNodeType.Element:
					{
						XmlElement src = (XmlElement)node;
						XmlElement dst = this.CreateElement (src.Prefix, src.LocalName, src.NamespaceURI);
						foreach(XmlAttribute attr in src.Attributes)
						{
							if(attr.Specified)	// copies only specified attributes
								dst.SetAttributeNode ((XmlAttribute)this.ImportNode (attr, deep));
							if(DocumentType != null)
							{
								// TODO: create default attribute values
							}
						}
						if(deep)
						{
							foreach(XmlNode n in src.ChildNodes)
								dst.AppendChild (this.ImportNode (n, deep));
						}
						return dst;
					}

				case XmlNodeType.EndElement:
					throw new XmlException ("Illegal ImportNode call for NodeType.EndElement");
				case XmlNodeType.EndEntity:
					throw new XmlException ("Illegal ImportNode call for NodeType.EndEntity");

				case XmlNodeType.Entity:
					throw new NotImplementedException ();	// TODO

				case XmlNodeType.EntityReference:
					return this.CreateEntityReference (node.Name);

				case XmlNodeType.None:
					throw new XmlException ("Illegal ImportNode call for NodeType.None");

				case XmlNodeType.Notation:
					throw new NotImplementedException ();	// TODO

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
					throw new NotImplementedException ();
			}
		}

		public virtual void Load (Stream inStream)
		{
			Load (new XmlTextReader (inStream));
		}

		public virtual void Load (string filename)
		{
			XmlReader xr = new XmlTextReader (filename);
			Load (xr);
			xr.Close ();
		}

		public virtual void Load (TextReader txtReader)
		{
			Load (new XmlTextReader (txtReader));
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
			do {
				XmlNode n = ReadNode (xmlReader);
				if(n == null) break;
				AppendChild (n);
			} while (true);
		}

		public virtual void LoadXml (string xml)
		{
			XmlReader xmlReader = new XmlTextReader (
				xml, XmlNodeType.Document, null);
			Load (xmlReader);
		}

		internal void onNodeChanged (XmlNode node, XmlNode Parent)
		{
			if (NodeChanged != null)
				NodeChanged (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Change,
					node, Parent, Parent));
		}

		internal void onNodeChanging(XmlNode node, XmlNode Parent)
		{
			if (NodeChanging != null)
				NodeChanging (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Change,
					node, Parent, Parent));
		}

		internal void onNodeInserted (XmlNode node, XmlNode newParent)
		{
			if (NodeInserted != null)
				NodeInserted (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Insert,
					node, null, newParent));
		}

		internal void onNodeInserting (XmlNode node, XmlNode newParent)
		{
			if (NodeInserting != null)
				NodeInserting (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Insert,
					node, null, newParent));
		}

		internal void onNodeRemoved (XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoved != null)
				NodeRemoved (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Remove,
					node, oldParent, null));
		}

		internal void onNodeRemoving (XmlNode node, XmlNode oldParent)
		{
			if (NodeRemoving != null)
				NodeRemoving (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Remove,
					node, oldParent, null));
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

		// Checks that Element's name is valid
		private void CheckName (String name)
		{
			// TODO: others validations?
			if (name.IndexOf (" ") >= 0)
				throw new XmlException ("The ' ' characted cannot be included in a name");
		}

		// Reads XmlReader and creates Attribute Node.
		private XmlAttribute ReadAttributeNode(XmlReader reader)
		{
			if(reader.NodeType == XmlNodeType.Element)
				reader.MoveToFirstAttribute ();
			else if(reader.NodeType != XmlNodeType.Attribute)
				throw new InvalidOperationException (MakeReaderErrorMessage ("bad position to read attribute.", reader));
			XmlAttribute attribute = CreateAttribute (reader.Prefix, reader.LocalName, reader.NamespaceURI);
			ReadAttributeNodeValue (reader, attribute);

			// Keep the current reader position
			bool res;
			if (attribute.NamespaceURI == string.Empty || attribute.NamespaceURI == null)
				res = reader.MoveToAttribute (attribute.Name);
			else 
				res = reader.MoveToAttribute (attribute.LocalName, attribute.NamespaceURI);
			if (reader.IsDefault)
				attribute.SetDefault ();
			return attribute;
		}

		// Reads attribute from XmlReader and then creates attribute value children. XmlAttribute also uses this.
		internal void ReadAttributeNodeValue(XmlReader reader, XmlAttribute attribute)
		{
			while(reader.ReadAttributeValue ()) {
				if(reader.NodeType == XmlNodeType.EntityReference)
					// FIXME: if DocumentType is available, then try to resolve it.
					attribute.AppendChild (CreateEntityReference (reader.Name));
				// FIXME: else if(NodeType == EndEntity) -- reset BaseURI and so on -- ;
				else
					// Children of Attribute is restricted to CharacterData and EntityReference (Comment is not allowed).
					attribute.AppendChild (CreateTextNode (reader.Value));
			}
		}

		[MonoTODO ("Child of entity is not simple Value string;Get prefix of NotationDecl")]
		public virtual XmlNode ReadNode(XmlReader reader)
		{
			XmlNode resultNode = null;
			XmlNode newNode = null;
			XmlNode currentNode = null;

			switch (reader.ReadState) {
			case ReadState.Interactive:
				break;
			case ReadState.Initial:
				reader.Read ();
				break;
			default:
				return null;
			}

			int startDepth = reader.Depth;
			bool ignoredWhitespace;
			bool reachedEOF = false;

			do {
				ignoredWhitespace = false;
				if (reader.ReadState != ReadState.Interactive)
					if (reachedEOF)
						throw new Exception ("XML Reader reached to end while reading node.");
					else
						reachedEOF = true;
				switch (reader.NodeType) {

				case XmlNodeType.Attribute:
					newNode = ReadAttributeNode (reader);
					break;

				case XmlNodeType.CDATA:
					newNode = CreateCDataSection (reader.Value);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Comment:
					newNode = CreateComment (reader.Value);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Element:
					XmlElement element = CreateElement (reader.Prefix, reader.LocalName, reader.NamespaceURI);
					element.IsEmpty = reader.IsEmptyElement;
					if(currentNode != null)
						currentNode.AppendChild (element);
					else
						resultNode = element;

					// set the element's attributes.
					while (reader.MoveToNextAttribute ()) {
						element.SetAttributeNode (ReadAttributeNode (reader));
					}

					reader.MoveToElement ();

					if (!reader.IsEmptyElement)
						currentNode = element;

					break;

				case XmlNodeType.EndElement:
					if (currentNode == null)
						throw new XmlException ("Unexpected end element.");
					else if (currentNode.Name != reader.Name)
						throw new XmlException (reader as IXmlLineInfo, String.Format ("mismatch end tag. Expected {0} but found {1}", currentNode.Name, reader.Name));
					currentNode = currentNode.ParentNode;
					break;

				case XmlNodeType.EndEntity:
					break;	// no operation

				case XmlNodeType.ProcessingInstruction:
					newNode = CreateProcessingInstruction (reader.Name, reader.Value);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Text:
					newNode = CreateTextNode (reader.Value);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.XmlDeclaration:
					// empty strings are dummy, then gives over setting value contents to setter.
					newNode = CreateXmlDeclaration ("1.0" , String.Empty, String.Empty);
					((XmlDeclaration)newNode).Value = reader.Value;
					if(currentNode != null)
						throw new XmlException (reader as IXmlLineInfo, "XmlDeclaration at invalid position.");
					break;

				case XmlNodeType.DocumentType:
					// hack ;-)
					XmlTextReader xtReader = reader as XmlTextReader;
					if(xtReader == null)
						newNode = CreateDocumentType (reader.Name, reader ["PUBLIC"], reader ["SYSTEM"], reader.Value);
					else
						newNode = CreateDocumentType (xtReader);

					if(currentNode != null)
						throw new XmlException (reader as IXmlLineInfo, "XmlDocumentType at invalid position.");
					break;

				case XmlNodeType.EntityReference:
					newNode = CreateEntityReference (reader.Name);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.SignificantWhitespace:
					newNode = CreateSignificantWhitespace (reader.Value);
					if(currentNode != null)
						currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Whitespace:
					if(PreserveWhitespace) {
						newNode = CreateWhitespace (reader.Value);
						if(currentNode != null)
							currentNode.AppendChild (newNode);
					}
					else
						ignoredWhitespace = true;
					break;
				}
				// Read next, except for reading attribute node.
				if (!(newNode is XmlAttribute) && !reader.Read ())
					break;
			} while (ignoredWhitespace || reader.Depth > startDepth ||
				(reader.Depth == startDepth && reader.NodeType == XmlNodeType.EndElement));
			if (startDepth != reader.Depth && reader.EOF)
				throw new XmlException ("Unexpected end of xml reader.");
			return resultNode != null ? resultNode : newNode;
		}

		private string MakeReaderErrorMessage (string message, XmlReader reader)
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			if (li != null)
				return String.Format ("{0} Line number = {1}, Inline position = {2}.", message, li.LineNumber, li.LinePosition);
			else
				return message;
		}

		internal void RemoveIdenticalAttribute (string id)
		{
			idTable.Remove (id);
		}

		public virtual void Save(Stream outStream)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (outStream, Encoding.UTF8);
			xmlWriter.Formatting = Formatting.Indented;
			WriteContentTo (xmlWriter);
			xmlWriter.Close ();
		}

		public virtual void Save (string filename)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (filename, Encoding.UTF8);
			xmlWriter.Formatting = Formatting.Indented;
			WriteContentTo (xmlWriter);
			xmlWriter.Close ();
		}

		public virtual void Save (TextWriter writer)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (writer);
			xmlWriter.Formatting = Formatting.Indented;
			WriteContentTo (xmlWriter);
			xmlWriter.Flush ();
		}

		public virtual void Save (XmlWriter xmlWriter)
		{
			//
			// This should preserve white space if PreserveWhiteSpace is true
			//
			WriteContentTo (xmlWriter);
			xmlWriter.Flush ();
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach(XmlNode childNode in ChildNodes) {
				childNode.WriteTo (w);
			}
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
		#endregion
	}
}
