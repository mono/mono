//
// System.Xml.XmlDocument
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

using System;
using System.IO;
using System.Xml.XPath;
using System.Diagnostics;

namespace System.Xml
{
	public class XmlDocument : XmlNode
	{
		#region Fields

		XmlLinkedNode lastLinkedChild;
		XmlNameTable nameTable;

		#endregion

		#region Constructors

		public XmlDocument () : base (null) { }

		[MonoTODO]
		protected internal XmlDocument (XmlImplementation imp) : base (null)
		{
			throw new NotImplementedException ();
		}

		public XmlDocument (NameTable nt) : base (null)
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

		[MonoTODO]
		public override string BaseURI {
			get { throw new NotImplementedException(); }
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

		[MonoTODO]
		public override string InnerXml {
			get { throw new NotImplementedException(); }
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

		public override XmlDocument OwnerDocument {
			get { return null; }
		}

		[MonoTODO]
		public bool PreserveWhitespace {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public XmlResolver XmlResolver {
			set { throw new NotImplementedException(); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override XmlNode CloneNode (bool deep)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlAttribute CreateAttribute (string name)
		{
			int indexOfColon = name.IndexOf (':');
			
			if (indexOfColon == -1)
				return CreateAttribute (String.Empty, name, String.Empty);

			string prefix = name.Substring (0, indexOfColon);
			string localName = name.Substring (indexOfColon + 1);

			return CreateAttribute (prefix, localName, String.Empty);
		}

		[MonoTODO]
		public XmlAttribute CreateAttribute (string qualifiedName, string namespaceURI)
		{
			int indexOfColon = qualifiedName.IndexOf (':');
			
			if (indexOfColon == -1)
				return CreateAttribute (String.Empty, qualifiedName, String.Empty);

			string prefix = qualifiedName.Substring (0, indexOfColon);
			string localName = qualifiedName.Substring (indexOfColon + 1);

			return CreateAttribute (prefix, localName, String.Empty);
		}

		public virtual XmlAttribute CreateAttribute (string prefix, string localName, string namespaceURI)
		{
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

		[MonoTODO]
		public virtual XmlDocumentType CreateDocumentType (
			string name,
			string publicId,
			string systemId,
			string internalSubset)
		{
			throw new NotImplementedException ();
		}

		public XmlElement CreateElement (string name)
		{
			int indexOfColon = name.IndexOf (':');
			
			if (indexOfColon == -1)
				return CreateElement (String.Empty, name, String.Empty);

			string prefix = name.Substring (0, indexOfColon);
			string localName = name.Substring (indexOfColon + 1);

			return CreateElement (prefix, localName, String.Empty);
		}

		[MonoTODO]
		public XmlElement CreateElement (
			string qualifiedName, 
			string namespaceURI)
		{
			int indexOfColon = qualifiedName.IndexOf (':');
			
			if (indexOfColon == -1)
				return CreateElement (String.Empty, qualifiedName, namespaceURI);

			string prefix = qualifiedName.Substring (0, indexOfColon);
			string localName = qualifiedName.Substring (indexOfColon + 1);

			return CreateElement (prefix, localName, namespaceURI);
		}

		public virtual XmlElement CreateElement (
			string prefix,
			string localName,
			string namespaceURI)
		{
			return new XmlElement (prefix, localName, namespaceURI, this);
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

		[MonoTODO]
		public virtual XmlNode CreateNode (
			string nodeTypeString,
			string name,
			string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode CreateNode (
			XmlNodeType type,
			string name,
			string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode CreateNode (
			XmlNodeType type,
			string prefix,
			string name,
			string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		public virtual XmlProcessingInstruction CreateProcessingInstruction (
			string target,
			string data)
		{
			return new XmlProcessingInstruction (target, data, this);
		}

		[MonoTODO]
		public virtual XmlSignificantWhitespace CreateSignificantWhitespace (string text)
		{
			throw new NotImplementedException ();
		}

		public virtual XmlText CreateTextNode (string text)
		{
			return new XmlText (text, this);
		}

		[MonoTODO]
		public virtual XmlWhitespace CreateWhitespace (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlDeclaration CreateXmlDeclaration (
			string version,
			string encoding,
			string standalone)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual XmlElement GetElementById (string elementId)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual XmlNode ImportNode (XmlNode node, bool deep)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Load (Stream inStream)
		{
			throw new NotImplementedException ();
		}

		public virtual void Load (string filename)
		{
			XmlReader xmlReader = new XmlTextReader (new StreamReader (filename));
			Load (xmlReader);
		}

		[MonoTODO]
		public virtual void Load (TextReader txtReader)
		{
			throw new NotImplementedException ();
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
					XmlElement element = CreateElement (xmlReader.Name, xmlReader.LocalName, xmlReader.NamespaceURI);
					currentNode.AppendChild (element);

					// set the element's attributes.
					while (xmlReader.MoveToNextAttribute ())
						element.SetAttribute (xmlReader.Name, xmlReader.Value);

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
		}

		public virtual void LoadXml (string xml)
		{
			XmlReader xmlReader = new XmlTextReader (new StringReader (xml));
			Load (xmlReader);
		}

		internal void onNodeChanged (XmlNode node, XmlNode Parent)
		{
			if (NodeChanged != null)
				NodeInserted (node, new XmlNodeChangedEventArgs
					(XmlNodeChangedAction.Change,
					node, Parent, Parent));
		}

		internal void onNodeChanging(XmlNode node, XmlNode Parent)
		{
			if (NodeInserting != null)
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

		[MonoTODO]
		public virtual XmlNode ReadNode(XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Save(Stream outStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Save (string filename)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Save (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Save (XmlWriter writer)
		{
			throw new NotImplementedException ();
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
