//
// System.Xml.XmlDocument
//
// Authors:
//   Daniel Weber (daniel-weber@austin.rr.com)
//   Kral Ferch <kral_ferch@hotmail.com>
//   Jason Diamond <jason@injektilo.org>
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Daniel Weber
// (C) 2002 Kral Ferch, Jason Diamond, Miguel de Icaza, Duncan Mak
//

using System;
using System.IO;
using System.Text;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections;

namespace System.Xml
{
	public class XmlDocument : XmlNode
	{
		#region Fields

		XmlLinkedNode lastLinkedChild;
		XmlNameTable nameTable;
		string baseURI = String.Empty;

		#endregion

		#region Constructors

		public XmlDocument () : base (null)
		{
			System.Xml.NameTable nt = new NameTable();
			// keys below are default of MS .NET Framework
			nt.Add("#text");
			nt.Add("xml");
			nt.Add("xmlns");
			nt.Add("#entity");
			nt.Add("#document-fragment");
			nt.Add("#comment");
			nt.Add("space");
			nt.Add("id");
			nt.Add("#whitespace");
			nt.Add("http://www.w3.org/2000/xmlns/");
			nt.Add("#cdata-section");
			nt.Add("lang");

			nameTable = nt;
		}

		[MonoTODO]
		protected internal XmlDocument (XmlImplementation imp) : base (null)
		{
			throw new NotImplementedException ();
		}

		public XmlDocument (XmlNameTable nt) : base (null)
		{
			nameTable = nt;
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

		[MonoTODO]
		public virtual XmlDocumentType DocumentType {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public XmlImplementation Implementation {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO ("Setter.")]
		public override string InnerXml {
			get {
				// Not sure why this is an override.  Passing through for now.
				return base.InnerXml;
			}
			set { throw new NotImplementedException(); }
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

		[MonoTODO]
		public bool PreserveWhitespace {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public virtual XmlResolver XmlResolver {
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			throw new NotImplementedException ();
		}

		public XmlAttribute CreateAttribute (string name)
		{
			return CreateAttribute (name, String.Empty);
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
			return new XmlComment(data, this);
		}

		[MonoTODO]
		protected internal virtual XmlAttribute CreateDefaultAttribute (string prefix, string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlDocumentFragment CreateDocumentFragment ()
		{
			throw new NotImplementedException ();
		}

		public virtual XmlDocumentType CreateDocumentType (string name, string publicId,
								   string systemId, string internalSubset)
		{
			return new XmlDocumentType (name, publicId, systemId, internalSubset, this);
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

			return new XmlElement (prefix != null ? prefix : String.Empty, localName, namespaceURI != null ? namespaceURI : String.Empty, this);
		}

		[MonoTODO]
		public virtual XmlEntityReference CreateEntityReference (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual XPathNavigator CreateNavigator (XmlNode node)
		{
			throw new NotImplementedException ();
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

			if  ((standalone != null) && !((standalone == "yes") || (standalone == "no")))
				throw new ArgumentException ("standalone string is not correct.");
			
			return new XmlDeclaration (version, encoding, standalone, this);
		}

		[MonoTODO]
		public virtual XmlElement GetElementById (string elementId)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual XmlNode ImportNode (XmlNode node, bool deep)
		{
			// How to resolve default attribute values?
			switch(node.NodeType)
			{
				case XmlNodeType.Attribute:
					{
						XmlAttribute src_att = node as XmlAttribute;
						XmlAttribute dst_att = this.CreateAttribute(src_att.Prefix, src_att.LocalName, src_att.NamespaceURI);
						// TODO: resolve default attribute values
						dst_att.Value = src_att.Value;
						return dst_att;
					}

				case XmlNodeType.CDATA:
					return this.CreateCDataSection(node.Value);

				case XmlNodeType.Comment:
					return this.CreateComment(node.Value);

				case XmlNodeType.Document:
					throw new XmlException("Document cannot be imported.");

				case XmlNodeType.DocumentFragment:
					{
						XmlDocumentFragment df = this.CreateDocumentFragment();
						if(deep)
						{
							foreach(XmlNode n in node.ChildNodes)
							{
								df.AppendChild(this.ImportNode(n, deep));
							}
						}
						return df;
					}

				case XmlNodeType.DocumentType:
					throw new XmlException("DocumentType cannot be imported.");

				case XmlNodeType.Element:
					{
						XmlElement src = (XmlElement)node;
						XmlElement dst = this.CreateElement(src.Prefix, src.LocalName, src.NamespaceURI);
						foreach(XmlAttribute attr in src.Attributes)
						{
							// TODO: create default attribute values
							dst.SetAttributeNode((XmlAttribute)this.ImportNode(attr, deep));
						}
						if(deep)
						{
							foreach(XmlNode n in src.ChildNodes)
								dst.AppendChild(this.ImportNode(n, deep));
						}
						return dst;
					}

				case XmlNodeType.EndElement:
					throw new XmlException ("Illegal ImportNode call for NodeType.EndElement");
				case XmlNodeType.EndEntity:
					throw new XmlException ("Illegal ImportNode call for NodeType.EndEntity");
				case XmlNodeType.Entity:
					throw new NotImplementedException ();

				// [2002.10.14] CreateEntityReference not implemented.
				case XmlNodeType.EntityReference:
					throw new NotImplementedException("ImportNode of EntityReference not implemented mainly because CreateEntityReference was implemented in the meantime.");
//					return this.CreateEntityReference(node.Name);

				case XmlNodeType.None:
					throw new XmlException ("Illegal ImportNode call for NodeType.None");
				case XmlNodeType.Notation:
					throw new NotImplementedException ();

				case XmlNodeType.ProcessingInstruction:
					XmlProcessingInstruction pi = node as XmlProcessingInstruction;
					return this.CreateProcessingInstruction(pi.Target, pi.Data);

				case XmlNodeType.SignificantWhitespace:
					return this.CreateSignificantWhitespace(node.Value);

				case XmlNodeType.Text:
					return this.CreateTextNode(node.Value);

				case XmlNodeType.Whitespace:
					return this.CreateWhitespace(node.Value);

				// I don't know how to test it...
				case XmlNodeType.XmlDeclaration:
				//	return this.CreateNode(XmlNodeType.XmlDeclaration, String.Empty, node.Value);
					throw new NotImplementedException ();

				default:
					throw new NotImplementedException ();
			}
		}

		public virtual void Load (Stream inStream)
		{
			XmlReader xmlReader = new XmlTextReader (inStream);
			Load (xmlReader);
		}

		public virtual void Load (string filename)
		{
			baseURI = filename;
			XmlReader xmlReader = new XmlTextReader (new StreamReader (filename));
			Load (xmlReader);
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

			XmlNode currentNode = this;
			XmlNode newNode;

#if true
			this.ConstructDOM(xmlReader, currentNode);
#else
			// Below are copied to XmlNode.Construct(currentNode, xmlReader)
			while (xmlReader.Read ()) 
			{
				switch (xmlReader.NodeType) {

				case XmlNodeType.CDATA:
					newNode = CreateCDataSection(xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Comment:
					newNode = CreateComment (xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Element:
					XmlElement element = CreateElement (xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
					currentNode.AppendChild (element);

					// set the element's attributes.
					while (xmlReader.MoveToNextAttribute ()) {
						XmlAttribute attribute = CreateAttribute (xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
						attribute.Value = xmlReader.Value;
						element.SetAttributeNode (attribute);
					}

					xmlReader.MoveToElement ();

					// if this element isn't empty, push it onto our "stack".
					if (!xmlReader.IsEmptyElement)
						currentNode = element;

					break;

				case XmlNodeType.EndElement:
					currentNode = currentNode.ParentNode;
					break;

				case XmlNodeType.ProcessingInstruction:
					newNode = CreateProcessingInstruction (xmlReader.Name, xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Text:
					newNode = CreateTextNode (xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;
				}
			}
#endif
		}

		public virtual void LoadXml (string xml)
		{
			XmlReader xmlReader = new XmlTextReader (new StringReader (xml));
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

		[MonoTODO]
		public virtual XmlNode ReadNode(XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Verify what encoding is used by default;  Should use PreserveWhiteSpace")]
		public virtual void Save(Stream outStream)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (outStream, Encoding.UTF8);
			WriteContentTo (xmlWriter);
			xmlWriter.Close ();
		}

		[MonoTODO ("Verify what encoding is used by default; Should use PreseveWhiteSpace")]
		public virtual void Save (string filename)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (filename, Encoding.UTF8);
			WriteContentTo (xmlWriter);
			xmlWriter.Close ();
		}

		[MonoTODO]
		public virtual void Save (TextWriter writer)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (writer);
			WriteContentTo (xmlWriter);
			xmlWriter.Flush ();
		}

		[MonoTODO ("Should preserve white space if PreserveWhisspace is set")]
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
			foreach(XmlNode childNode in ChildNodes)
				childNode.WriteTo(w);
		}

		public override void WriteTo (XmlWriter w)
		{
			WriteContentTo(w);
		}

		#endregion
	}
}
