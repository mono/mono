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
		WeakReference reusableXmlTextReader;

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

		// Used to read 'InnerXml's for its descendants at any place.
		internal XmlTextReader ReusableReader {
			get {
				if(reusableXmlTextReader == null)
					reusableXmlTextReader = new WeakReference (null);
				if(!reusableXmlTextReader.IsAlive) {
					XmlTextReader reader = new XmlTextReader ((TextReader)null);
					reusableXmlTextReader.Target = reader;
				}
				return (XmlTextReader)reusableXmlTextReader.Target;
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

		[MonoTODO("check its behavior")]
		public bool PreserveWhitespace {
			get { return preserveWhitespace; }
			set { preserveWhitespace = value; }
		}

		internal override string XmlLang {
			get { return String.Empty; }
		}

		[MonoTODO]
		public virtual XmlResolver XmlResolver {
			set { throw new NotImplementedException (); }
		}

		internal override XmlSpace XmlSpace {
			get {
				return XmlSpace.None;
			}
		}

		#endregion

		#region Methods

		[MonoTODO("Should BaseURI be cloned?")]
		public override XmlNode CloneNode (bool deep)
		{
			XmlDocument doc = implementation.CreateDocument ();
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
			return new XmlComment (data, this);
		}

		[MonoTODO]
		protected internal virtual XmlAttribute CreateDefaultAttribute (string prefix, string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
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

		public virtual XmlEntityReference CreateEntityReference (string name)
		{
			return new XmlEntityReference (name, this);
		}

		[MonoTODO]
		internal protected virtual XPathNavigator CreateNavigator (XmlNode node)
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

			if  ((standalone != null && standalone != String.Empty) && !((standalone == "yes") || (standalone == "no")))
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

			// This method of XmlNode is previously written here.
			// Then I(ginga) moved them to use this logic with XmlElement.
			this.ConstructDOM(xmlReader, currentNode);
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

		public virtual void Save(Stream outStream)
		{
			XmlTextWriter xmlWriter = new XmlTextWriter (outStream, Encoding.UTF8);
			WriteContentTo (xmlWriter);
			xmlWriter.Close ();
		}

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
				if(!PreserveWhitespace) {
					w.WriteRaw ("\n");
				}
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
