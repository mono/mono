//
// System.Xml.XmlNode
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable
	{
		#region Fields

		XmlDocument ownerDocument;
		XmlNode parentNode;

		#endregion

		#region Constructors

		internal XmlNode (XmlDocument ownerDocument)
		{
			this.ownerDocument = ownerDocument;
		}

		#endregion

		#region Properties

		public virtual XmlAttributeCollection Attributes
		{
			get { return null; }
		}

		public virtual string BaseURI
		{
			get { return ParentNode.BaseURI; }
		}

		public virtual XmlNodeList ChildNodes {
			get {
				return new XmlNodeListChildren(this);
			}
		}

		public virtual XmlNode FirstChild {
			get {
				if (LastChild != null) {
					return LastLinkedChild.NextLinkedSibling;
				}
				else {
					return null;
				}
			}
		}

		public virtual bool HasChildNodes {
			get { return LastChild != null; }
		}

		[MonoTODO]
		public virtual string InnerText {
			get {
				StringBuilder builder = new StringBuilder ();
				AppendChildValues (this, builder);
				return builder.ToString ();
			}

			set { throw new NotImplementedException (); }
		}

		private void AppendChildValues (XmlNode parent, StringBuilder builder)
		{
			XmlNode node = parent.FirstChild;

			while (node != null) {
				if (node.NodeType == XmlNodeType.Text)
					builder.Append (node.Value);
				AppendChildValues (node, builder);
				node = node.NextSibling;
			}
		}

		[MonoTODO("Setter.")]
		public virtual string InnerXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteContentTo(xtw);

				return sw.GetStringBuilder().ToString();
			}

			set { throw new NotImplementedException (); }
		}

		public virtual bool IsReadOnly {
			get { return false; }
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public virtual XmlElement this [string name] {
			get { 
				foreach (XmlNode node in ChildNodes) {
					if ((node.NodeType == XmlNodeType.Element) &&
					    (node.Name == name)) {
						return (XmlElement) node;
					}
				}

				return null;
			}
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public virtual XmlElement this [string localname, string ns] {
			get { 
				foreach (XmlNode node in ChildNodes) {
					if ((node.NodeType == XmlNodeType.Element) &&
					    (node.LocalName == localname) && 
					    (node.NamespaceURI == ns)) {
						return (XmlElement) node;
					}
				}

				return null;
			}
		}

		public virtual XmlNode LastChild {
			get { return LastLinkedChild; }
		}

		internal virtual XmlLinkedNode LastLinkedChild {
			get { return null; }
			set { }
		}

		public abstract string LocalName { get;	}

		public abstract string Name	{ get; }

		public virtual string NamespaceURI {
			get { return String.Empty; }
		}

		public virtual XmlNode NextSibling {
			get { return null; }
		}

		public abstract XmlNodeType NodeType { get;	}

		internal virtual XPathNodeType XPathNodeType {
			get {
				return (XPathNodeType) (-1);
			}
		}

		public virtual string OuterXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteTo(xtw);

				return sw.GetStringBuilder().ToString();
			}
		}

		public virtual XmlDocument OwnerDocument {
			get { return ownerDocument; }
		}

		public virtual XmlNode ParentNode {
			get { return parentNode; }
		}

		public virtual string Prefix {
			get { return String.Empty; }
			set {}
		}

		public virtual XmlNode PreviousSibling {
			get { return null; }
		}

		public virtual string Value {
			get { return null; }
			set { throw new InvalidOperationException ("This node does not have a value"); }
		}

		#endregion

		#region Methods

		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute || NodeType == XmlNodeType.DocumentFragment) {
				
				ownerDoc.onNodeInserting (newChild, this);

				// If newChild is already on the tree, then it was removed from current position.
				// But test fails, so kept alive in the meantime;)
				if(newChild.ParentNode != null)
					newChild.ParentNode.RemoveChild(newChild);

				if (newChild.OwnerDocument != ownerDoc)
					throw new ArgumentException ("Can't append a node created by another document.");

				if(newChild.NodeType == XmlNodeType.DocumentFragment)
				{
					int x = newChild.ChildNodes.Count;
					for(int i=0; i<x; i++)
					{
						// When this logic became to remove children in order, then index will have never to increments.
						XmlNode n = newChild.ChildNodes[0];
						this.AppendChild(n);	// recursively invokes events. (It is compatible with MS implementation.)
					}
				}
				else
				{
					XmlLinkedNode newLinkedChild = (XmlLinkedNode) newChild;
					XmlLinkedNode lastLinkedChild = LastLinkedChild;

					newLinkedChild.parentNode = this;
				
					if (lastLinkedChild != null) 
					{
						newLinkedChild.NextLinkedSibling = lastLinkedChild.NextLinkedSibling;
						lastLinkedChild.NextLinkedSibling = newLinkedChild;
					} 
					else
						newLinkedChild.NextLinkedSibling = newLinkedChild;
				
					LastLinkedChild = newLinkedChild;

					ownerDoc.onNodeInserted (newChild, newChild.ParentNode);

				}
				return newChild;
			} else
				throw new InvalidOperationException();
		}

		[MonoTODO]
		public virtual XmlNode Clone ()
		{
			throw new NotImplementedException ();
		}

		public abstract XmlNode CloneNode (bool deep);

		[MonoTODO]
		public XPathNavigator CreateNavigator ()
		{
			return new XmlDocumentNavigator(this);
		}

		public IEnumerator GetEnumerator ()
		{
			return new XmlNodeListChildren(this).GetEnumerator();
		}

		[MonoTODO]
		public virtual string GetNamespaceOfPrefix (string prefix)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetPrefixOfNamespace (string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		[MonoTODO]
		public virtual XmlNode InsertAfter (XmlNode newChild, XmlNode refChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Normalize ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode PrependChild (XmlNode newChild)
		{
			throw new NotImplementedException ();
		}

		public virtual void RemoveAll ()
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

			ownerDoc.onNodeRemoving (this, this.ParentNode);
			LastLinkedChild = null;
			ownerDoc.onNodeRemoved (this, this.ParentNode);
		}

		public virtual XmlNode RemoveChild (XmlNode oldChild)
		{
			OwnerDocument.onNodeRemoving (oldChild, oldChild.ParentNode);

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute || NodeType == XmlNodeType.DocumentFragment) 
			{
				if (IsReadOnly)
					throw new ArgumentException();

				if (Object.ReferenceEquals(LastLinkedChild, LastLinkedChild.NextLinkedSibling) && Object.ReferenceEquals(LastLinkedChild, oldChild))
					LastLinkedChild = null;
				else {
					XmlLinkedNode oldLinkedChild = (XmlLinkedNode)oldChild;
					XmlLinkedNode beforeLinkedChild = LastLinkedChild;
					
					while (!Object.ReferenceEquals(beforeLinkedChild.NextLinkedSibling, LastLinkedChild) && !Object.ReferenceEquals(beforeLinkedChild.NextLinkedSibling, oldLinkedChild))
						beforeLinkedChild = beforeLinkedChild.NextLinkedSibling;

					if (!Object.ReferenceEquals(beforeLinkedChild.NextLinkedSibling, oldLinkedChild))
						throw new ArgumentException();

					beforeLinkedChild.NextLinkedSibling = oldLinkedChild.NextLinkedSibling;
					oldLinkedChild.NextLinkedSibling = null;
				 }

				OwnerDocument.onNodeRemoved (oldChild, oldChild.ParentNode);
				oldChild.parentNode = null;	// clear parent 'after' above logic.

				return oldChild;
			} 
			else
				throw new ArgumentException();
		}

		[MonoTODO]
		public virtual XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			throw new NotImplementedException ();
		}

		public XmlNodeList SelectNodes (string xpath)
		{
			return SelectNodes (xpath, null);
		}

		[MonoTODO]
		public XmlNodeList SelectNodes (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			ArrayList rgNodes = new ArrayList ();
			while (iter.MoveNext ())
			{
				rgNodes.Add (((XmlDocumentNavigator) iter.Current).Node);
			}
			return new XmlNodeArrayList (rgNodes);
		}

		public XmlNode SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		[MonoTODO]
		public XmlNode SelectSingleNode (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			if (!iter.MoveNext ())
				return null;
			return ((XmlDocumentNavigator) iter.Current).Node;
		}

		internal void SetParentNode (XmlNode parent)
		{
			parentNode = parent;
		}

		[MonoTODO]
		public virtual bool Supports (string feature, string version)
		{
			throw new NotImplementedException ();
		}

		public abstract void WriteContentTo (XmlWriter w);

		public abstract void WriteTo (XmlWriter w);

		// It parses with XmlReader and then construct DOM of the parsed contents.
		internal void ConstructDOM(XmlReader xmlReader, XmlNode currentNode)
		{
			// I am not confident whether this method should be placed in this class or not...
			// Please verify its validity and then erase this comment;-)
			XmlNode newNode;
			XmlDocument doc = currentNode is XmlDocument ? (XmlDocument)currentNode : currentNode.OwnerDocument;
			// Below are 'almost' copied from XmlDocument.Load(XmlReader xmlReader)
			while (xmlReader.Read ()) 
			{
				switch (xmlReader.NodeType) 
				{
					case XmlNodeType.CDATA:
						newNode = doc.CreateCDataSection(xmlReader.Value);
						currentNode.AppendChild (newNode);
						break;

					case XmlNodeType.Comment:
						newNode = doc.CreateComment (xmlReader.Value);
						currentNode.AppendChild (newNode);
						break;

					case XmlNodeType.Element:
						XmlElement element = doc.CreateElement (xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
						currentNode.AppendChild (element);

						// set the element's attributes.
						while (xmlReader.MoveToNextAttribute ()) 
						{
							XmlAttribute attribute = doc.CreateAttribute (xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
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
						newNode = doc.CreateProcessingInstruction (xmlReader.Name, xmlReader.Value);
						currentNode.AppendChild (newNode);
						break;

					case XmlNodeType.Text:
						newNode = doc.CreateTextNode (xmlReader.Value);
						currentNode.AppendChild (newNode);
						break;

					case XmlNodeType.XmlDeclaration:
						// empty strings are dummy, then gives over setting value contents to setter.
						newNode = doc.CreateXmlDeclaration("1.0" , String.Empty, String.Empty);
						((XmlDeclaration)newNode).Value = xmlReader.Value;
						this.AppendChild(newNode);
						break;

					case XmlNodeType.DocumentType:
						XmlTextReader xmlTextReader = xmlReader as XmlTextReader;
						if(xmlTextReader != null)
						{
							XmlDocumentType dtdNode = doc.CreateDocumentType(xmlTextReader.Name, xmlTextReader.publicId, xmlTextReader.systemId, xmlTextReader.Value);
							this.AppendChild(dtdNode);
						}
						else
							throw new XmlException("construction of DocumentType node from this XmlReader is not supported.");
						break;
				}
			}
		}

		// It parses this and all the ancestor elements,
		// find 'xmlns' declarations, stores and then return them.
		// TODO: tests
		internal protected XmlNamespaceManager ConstructNamespaceManager()
		{
			XmlDocument doc = this is XmlDocument ? (XmlDocument)this : this.OwnerDocument;
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			XmlElement el = null;
			switch(this.NodeType)
			{
				case XmlNodeType.Attribute:
					el = ((XmlAttribute)this).OwnerElement;
					break;
				case XmlNodeType.Element:
					el = this as XmlElement;
					break;
				default:
					el = this.ParentNode as XmlElement;
					break;
			}

			while(el != null)
			{			
				foreach(XmlAttribute attr in el.Attributes)
				{
					if(attr.Prefix == "xmlns")
					{
						if(nsmgr.LookupNamespace(attr.LocalName) == null )
						{
							nsmgr.AddNamespace(attr.LocalName, attr.Value);
						}
					}
				}
				// When reached to document, then it will set null value :)
				el = el.ParentNode as XmlElement;
			}
			return nsmgr;
		}
		#endregion
	}
}
