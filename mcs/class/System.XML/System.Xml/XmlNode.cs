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

		public virtual XmlAttributeCollection Attributes {
			get { return null; }
		}

		public virtual string BaseURI {
			get { return ParentNode.BaseURI; }
		}

		public virtual XmlNodeList ChildNodes {
			get {
				return new XmlNodeListChildren (this);
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

		[MonoTODO("confirm whether this way is right for each not-overriden types.")]
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

				WriteContentTo (xtw);

				return sw.GetStringBuilder ().ToString ();
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

				WriteTo (xtw);

				return sw.GetStringBuilder ().ToString ();
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
			// I assume that AppendChild(n) equals to InsertAfter(n, this.LastChild) or InsertBefore(n, null)
			return InsertBefore (newChild, null);

			// Below are formerly used logic.
/*			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute || NodeType == XmlNodeType.DocumentFragment) {
				
				if (IsReadOnly)
					throw new ArgumentException ("The specified node is readonly.");

				if (newChild.OwnerDocument != ownerDoc)
					throw new ArgumentException ("Can't append a node created by another document.");

				// checking validity finished. then appending...

				ownerDoc.onNodeInserting (newChild, this);

				if(newChild.ParentNode != null)
					newChild.ParentNode.RemoveChild(newChild);

				if(newChild.NodeType == XmlNodeType.DocumentFragment)
				{
					int x = newChild.ChildNodes.Count;
					for(int i=0; i<x; i++)
					{
						// When this logic became to remove children in order, then index will have never to increments.
						XmlNode n = newChild.ChildNodes [0];
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
*/		}

		public virtual XmlNode Clone ()
		{
			// By MS document, it is equivalent to CloneNode(true).
			return this.CloneNode (true);
		}

		public abstract XmlNode CloneNode (bool deep);

		[MonoTODO]
		public XPathNavigator CreateNavigator ()
		{
			return new XmlDocumentNavigator (this);
		}

		public IEnumerator GetEnumerator ()
		{
			return new XmlNodeListChildren (this).GetEnumerator ();
		}

		[MonoTODO("performance problem.")]
		public virtual string GetNamespaceOfPrefix (string prefix)
		{
			XmlNamespaceManager nsmgr = ConstructNamespaceManager ();
			return nsmgr.LookupNamespace (prefix);
		}

		[MonoTODO("performance problem.")]
		public virtual string GetPrefixOfNamespace (string namespaceURI)
		{
			XmlNamespaceManager nsmgr = ConstructNamespaceManager ();
			return nsmgr.LookupPrefix (namespaceURI);
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public virtual XmlNode InsertAfter (XmlNode newChild, XmlNode refChild)
		{
			// I assume that insertAfter(n1, n2) equals to InsertBefore(n1, n2.PreviousSibling).

			// I took this way because rather than calling InsertAfter() from InsertBefore()
			//   because current implementation of 'NextSibling' looks faster than 'PreviousSibling'.
			XmlNode argNode = null;
			if(refChild != null)
				argNode = refChild.NextSibling;
			else if(ChildNodes.Count > 0)
				argNode = FirstChild;
			return InsertBefore (newChild, argNode);
		}

		public virtual XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

			if (NodeType == XmlNodeType.Document ||
			    NodeType == XmlNodeType.Element ||
			    NodeType == XmlNodeType.Attribute ||
			    NodeType == XmlNodeType.DocumentFragment) {			
				if (IsReadOnly)
					throw new ArgumentException ("The specified node is readonly.");

				if (newChild.OwnerDocument != ownerDoc)
					throw new ArgumentException ("Can't append a node created by another document.");

				if (refChild != null && newChild.OwnerDocument != refChild.OwnerDocument)
						throw new ArgumentException ("argument nodes are on the different documents.");

				if (refChild != null && this == ownerDoc &&
				    ownerDoc.DocumentElement != null &&
				    (newChild is XmlElement ||
				     newChild is XmlCharacterData ||
				     newChild is XmlEntityReference))
					throw new XmlException ("cannot insert this node to this position.");

				// checking validity finished. then appending...

				ownerDoc.onNodeInserting (newChild, this);

				if(newChild.ParentNode != null)
					newChild.ParentNode.RemoveChild (newChild);

				if(newChild.NodeType == XmlNodeType.DocumentFragment) {
					int x = newChild.ChildNodes.Count;
					for(int i=0; i<x; i++) {
						XmlNode n = newChild.ChildNodes [0];
						this.InsertBefore (n, refChild);	// recursively invokes events. (It is compatible with MS implementation.)
					}
				}
				else {
					XmlLinkedNode newLinkedChild = (XmlLinkedNode) newChild;
					XmlLinkedNode lastLinkedChild = LastLinkedChild;

					newLinkedChild.parentNode = this;

					if(refChild == null) {
						// append last, so:
						// * set nextSibling of previous lastchild to newChild
						// * set lastchild = newChild
						// * set next of newChild to firstChild
						if(LastLinkedChild != null) {
							XmlLinkedNode formerFirst = FirstChild as XmlLinkedNode;
							LastLinkedChild.NextLinkedSibling = newLinkedChild;
							LastLinkedChild = newLinkedChild;
							newLinkedChild.NextLinkedSibling = formerFirst;
						}
						else {
							LastLinkedChild = newLinkedChild;
							LastLinkedChild.NextLinkedSibling = newLinkedChild;	// FirstChild
						}
					}
					else {
						// append not last, so:
						// * if newchild is first, then set next of lastchild is newChild.
						//   otherwise, set next of previous sibling to newChild
						// * set next of newChild to refChild
						XmlLinkedNode prev = refChild.PreviousSibling as XmlLinkedNode;
						if(prev == null)
							LastLinkedChild.NextLinkedSibling = newLinkedChild;
						else
							prev.NextLinkedSibling = newLinkedChild;
						newLinkedChild.NextLinkedSibling = refChild as XmlLinkedNode;
					}
					ownerDoc.onNodeInserted (newChild, newChild.ParentNode);
				}
				return newChild;
			} 
			else
				throw new InvalidOperationException ();
		}

		[MonoTODO]
		public virtual void Normalize ()
		{
			throw new NotImplementedException ();
		}

		public virtual XmlNode PrependChild (XmlNode newChild)
		{
			return InsertAfter (newChild, null);
		}

		public virtual void RemoveAll ()
		{
			XmlNode next = null;
			for (XmlNode node = FirstChild; node != null; node = next) {
				next = node.NextSibling;
				RemoveChild (node);
			}
		}

		public virtual XmlNode RemoveChild (XmlNode oldChild)
		{
			if(oldChild.ParentNode != this)
				throw new XmlException ("specified child is not child of this node.");

			OwnerDocument.onNodeRemoving (oldChild, oldChild.ParentNode);

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute || NodeType == XmlNodeType.DocumentFragment) {
				if (IsReadOnly)
					throw new ArgumentException ();

				if (Object.ReferenceEquals (LastLinkedChild, LastLinkedChild.NextLinkedSibling) && Object.ReferenceEquals (LastLinkedChild, oldChild))
					LastLinkedChild = null;
				else {
					XmlLinkedNode oldLinkedChild = (XmlLinkedNode)oldChild;
					XmlLinkedNode beforeLinkedChild = LastLinkedChild;
					
					while (!Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, LastLinkedChild) && !Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild))
						beforeLinkedChild = beforeLinkedChild.NextLinkedSibling;

					if (!Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild))
						throw new ArgumentException ();

					beforeLinkedChild.NextLinkedSibling = oldLinkedChild.NextLinkedSibling;
					oldLinkedChild.NextLinkedSibling = null;
				 }

				OwnerDocument.onNodeRemoved (oldChild, oldChild.ParentNode);
				oldChild.parentNode = null;	// clear parent 'after' above logic.

				return oldChild;
			} 
			else
				throw new ArgumentException ();
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
		internal void ConstructDOM (XmlReader xmlReader, XmlNode currentNode)
		{
			// I am not confident whether this method should be placed in this class or not...
			// Please verify its validity and then erase this comment;-)
			XmlNode newNode;
			XmlDocument doc = currentNode is XmlDocument ? (XmlDocument)currentNode : currentNode.OwnerDocument;
			// Below are 'almost' copied from XmlDocument.Load(XmlReader xmlReader)
			while (xmlReader.Read ()) {
				switch (xmlReader.NodeType) {
				case XmlNodeType.CDATA:
					newNode = doc.CreateCDataSection (xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Comment:
					newNode = doc.CreateComment (xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Element:
					XmlElement element = doc.CreateElement (xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
					element.IsEmpty = xmlReader.IsEmptyElement;
					currentNode.AppendChild (element);

					// set the element's attributes.
					while (xmlReader.MoveToNextAttribute ()) {
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
					newNode = doc.CreateXmlDeclaration ("1.0" , String.Empty, String.Empty);
					((XmlDeclaration)newNode).Value = xmlReader.Value;
					this.AppendChild (newNode);
					break;

				case XmlNodeType.DocumentType:
					XmlTextReader xmlTextReader = xmlReader as XmlTextReader;
					if(xmlTextReader != null) {
						XmlDocumentType dtdNode = doc.CreateDocumentType (xmlTextReader.Name, xmlTextReader.publicId, xmlTextReader.systemId, xmlTextReader.Value);
						this.AppendChild (dtdNode);
					}
					else
						throw new XmlException ("construction of DocumentType node from this XmlReader is not supported.");
					break;

				case XmlNodeType.EntityReference:
					newNode = doc.CreateEntityReference (xmlReader.Name);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.SignificantWhitespace:
					newNode = doc.CreateSignificantWhitespace (xmlReader.Value);
					currentNode.AppendChild (newNode);
					break;

				case XmlNodeType.Whitespace:
					if(doc.PreserveWhitespace) {
						newNode = doc.CreateWhitespace (xmlReader.Value);
						currentNode.AppendChild (newNode);
					}
					break;
				}
			}
		}

		// It parses this and all the ancestor elements,
		// find 'xmlns' declarations, stores and then return them.
		// TODO: tests
		internal XmlNamespaceManager ConstructNamespaceManager ()
		{
			XmlDocument doc = this is XmlDocument ? (XmlDocument)this : this.OwnerDocument;
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			XmlElement el = null;
			switch(this.NodeType) {
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

			while(el != null) {
				foreach(XmlAttribute attr in el.Attributes) {
					if(attr.Prefix == "xmlns" || (attr.Name == "xmlns" && attr.Prefix == String.Empty)) {
						if(nsmgr.LookupNamespace (attr.LocalName) == null )
							nsmgr.AddNamespace (attr.LocalName, attr.Value);
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
