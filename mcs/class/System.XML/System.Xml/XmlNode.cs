//
// System.Xml.XmlNode
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Kral Ferch
// (C) 2002 Atsushi Enomoto
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
			get {
				// Isn't it conformant to W3C XML Base Recommendation?
				// As far as I tested, there are not...
				return (ParentNode != null) ? ParentNode.BaseURI : OwnerDocument.BaseURI;
			}
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
				switch (node.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
 					builder.Append (node.Value);
					break;
				}
				AppendChildValues (node, builder);
				node = node.NextSibling;
			}
		}

		public virtual string InnerXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteContentTo (xtw);

				return sw.GetStringBuilder ().ToString ();
			}

			set {
				throw new InvalidOperationException ("This node is readonly or doesn't have any children.");
			}
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
				throw new InvalidOperationException ();
			}
		}

		public virtual string OuterXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteTo (xtw);

				return sw.ToString ();
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

		internal virtual string XmlLang {
			get {
				if(Attributes != null)
					foreach(XmlAttribute attr in Attributes)
						if(attr.Name == "xml:lang")
							return attr.Value;
				return (ParentNode != null) ? ParentNode.XmlLang : OwnerDocument.XmlLang;
			}
		}

		internal virtual XmlSpace XmlSpace {
			get {
				if(Attributes != null) {
					foreach(XmlAttribute attr in Attributes) {
						if(attr.Name == "xml:space") {
							switch(attr.Value) {
							case "preserve": return XmlSpace.Preserve;
							case "default": return XmlSpace.Default;
							}
							break;
						}
					}
				}
				return (ParentNode != null) ? ParentNode.XmlSpace : OwnerDocument.XmlSpace;
			}
		}

		#endregion

		#region Methods

		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			// I assume that AppendChild(n) equals to InsertAfter(n, this.LastChild) or InsertBefore(n, null)
			return InsertBefore (newChild, null);
		}

		public virtual XmlNode Clone ()
		{
			// By MS document, it is equivalent to CloneNode(true).
			return this.CloneNode (true);
		}

		public abstract XmlNode CloneNode (bool deep);

		[MonoTODO]
		public XPathNavigator CreateNavigator ()
		{
			XmlDocument document = this.NodeType == XmlNodeType.Document ?
				this as XmlDocument : this.ownerDocument;
			return document.CreateNavigator (this);
		}

		public IEnumerator GetEnumerator ()
		{
			return new XmlNodeListChildren (this).GetEnumerator ();
		}

		public virtual string GetNamespaceOfPrefix (string prefix)
		{
			XmlNode node;
			switch (NodeType) {
			case XmlNodeType.Attribute:
				node = ((XmlAttribute) this).OwnerElement;
				break;
			case XmlNodeType.Element:
				node = this;
				break;
			default:
				node = ParentNode;
				break;
			}

			while (node.NodeType != XmlNodeType.Document) {
				foreach (XmlAttribute attr in node.Attributes) {
					if (prefix == attr.LocalName && attr.Prefix == "xmlns"
						|| attr.Name == "xmlns" && prefix == String.Empty)
						return attr.Value;
				}
				node = node.ParentNode;
			}
			return null;
		}

		public virtual string GetPrefixOfNamespace (string namespaceURI)
		{
			XmlNode node;
			switch (NodeType) {
			case XmlNodeType.Attribute:
				node = ((XmlAttribute) this).OwnerElement;
				break;
			case XmlNodeType.Element:
				node = this;
				break;
			default:
				node = ParentNode;
				break;
			}

			while (node.NodeType != XmlNodeType.Document) {
				foreach (XmlAttribute attr in node.Attributes) {
					if (attr.Prefix == "xmlns" && attr.Value == namespaceURI)
						return attr.LocalName;
					else if (attr.Name == "xmlns" && attr.Value == namespaceURI)
						return String.Empty;
				}
				node = node.ParentNode;
			}
			return String.Empty;
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

			// I took this way because current implementation 
			// Calling InsertAfter() from InsertBefore() is
			// subsequently to use 'NextSibling' which is
			// faster than 'PreviousSibling' (these children are 
			// forward-only linked list).
			XmlNode argNode = null;
			if(refChild != null)
				argNode = refChild.NextSibling;
			else if(ChildNodes.Count > 0)
				argNode = FirstChild;
			return InsertBefore (newChild, argNode);
		}

		[MonoTODO("If inserted node is entity reference, then check conforming entity. Wait for DTD implementation.")]
		public virtual XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

			if (NodeType != XmlNodeType.Element &&
			    NodeType != XmlNodeType.Attribute &&
			    NodeType != XmlNodeType.Document &&
			    NodeType != XmlNodeType.DocumentFragment)
				throw new InvalidOperationException (String.Format ("current node {0} is not allowed to have any children.", NodeType));

			switch (NodeType) {
			case XmlNodeType.Attribute:
				switch (newChild.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.EntityReference:
					break;
				default:
					throw new ArgumentException (String.Format (
						"Cannot insert specified type of node {0} as a child of this node {0}.", 
						newChild.NodeType, NodeType));
				}
				break;
			case XmlNodeType.Element:
				switch (newChild.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.Document:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Entity:
				case XmlNodeType.Notation:
				case XmlNodeType.XmlDeclaration:
					throw new ArgumentException ("Cannot insert specified type of node as a child of this node.");
				}
				break;
			}

			if (IsReadOnly)
				throw new ArgumentException ("The specified node is readonly.");

			if (newChild.OwnerDocument != ownerDoc)
				throw new ArgumentException ("Can't append a node created by another document.");

			if (refChild != null && newChild.OwnerDocument != refChild.OwnerDocument)
					throw new ArgumentException ("argument nodes are on the different documents.");

			// This check is done by MS.NET 1.0, but isn't done for MS.NET 1.1. 
			// Skip this check in the meantime...
//				if(this == ownerDoc && ownerDoc.DocumentElement != null && (newChild is XmlElement))
//					throw new XmlException ("multiple document element not allowed.");

			// checking validity finished. then appending...

			return insertBeforeIntern (newChild, refChild);
		}

		internal XmlNode insertBeforeIntern (XmlNode newChild, XmlNode refChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;

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
			if (Attributes != null)
				Attributes.RemoveAll ();
			XmlNode next = null;
			for (XmlNode node = FirstChild; node != null; node = next) {
				next = node.NextSibling;
				RemoveChild (node);
			}
		}

		public virtual XmlNode RemoveChild (XmlNode oldChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;
			if(oldChild.ParentNode != this)
				throw new XmlException ("specified child is not child of this node.");

			ownerDoc.onNodeRemoving (oldChild, oldChild.ParentNode);

			if (NodeType != XmlNodeType.Attribute && 
				NodeType != XmlNodeType.Element && 
				NodeType != XmlNodeType.Document && 
				NodeType != XmlNodeType.DocumentFragment)
				throw new ArgumentException (String.Format ("This {0} node cannot remove child.", NodeType));

			if (IsReadOnly)
				throw new ArgumentException (String.Format ("This {0} node is read only.", NodeType));

			if (Object.ReferenceEquals (LastLinkedChild, LastLinkedChild.NextLinkedSibling) && Object.ReferenceEquals (LastLinkedChild, oldChild))
				// If there is only one children, simply clear.
				LastLinkedChild = null;
			else {
				XmlLinkedNode oldLinkedChild = (XmlLinkedNode) oldChild;
				XmlLinkedNode beforeLinkedChild = LastLinkedChild;
				XmlLinkedNode firstChild = (XmlLinkedNode) FirstChild;
				
				while (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, LastLinkedChild) == false && 
					Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					beforeLinkedChild = beforeLinkedChild.NextLinkedSibling;

				if (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					throw new ArgumentException ();

				beforeLinkedChild.NextLinkedSibling = oldLinkedChild.NextLinkedSibling;

				// Each derived class may have its own LastLinkedChild, so we must set it explicitly.
				if (oldLinkedChild.NextLinkedSibling == firstChild)
					this.LastLinkedChild = beforeLinkedChild;

				oldLinkedChild.NextLinkedSibling = null;
				}

			ownerDoc.onNodeRemoved (oldChild, oldChild.ParentNode);
			oldChild.parentNode = null;	// clear parent 'after' above logic.

			return oldChild;
		}

		public virtual XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			if(oldChild.ParentNode != this)
				throw new InvalidOperationException ("oldChild is not a child of this node.");
			XmlNode parent = this.ParentNode;
			while(parent != null) {
				if(newChild == parent)
					throw new InvalidOperationException ("newChild is ancestor of this node.");
				parent = parent.ParentNode;
			}
			foreach(XmlNode n in ChildNodes) {
				if(n == oldChild) {
					XmlNode prev = oldChild.PreviousSibling;
					RemoveChild (oldChild);
					InsertAfter (newChild, prev);
					break;
				}
			}
			return oldChild;
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

//		internal void SetParentNode (XmlNode parent)
//		{
//			parentNode = parent;
//		}

		[MonoTODO]
		public virtual bool Supports (string feature, string version)
		{
			throw new NotImplementedException ();
		}

		public abstract void WriteContentTo (XmlWriter w);

		public abstract void WriteTo (XmlWriter w);

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
					if(attr.Prefix == "xmlns") {
						if (nsmgr.LookupNamespace (attr.LocalName) != attr.Value)
							nsmgr.AddNamespace (attr.LocalName, attr.Value);
					} else if(attr.Name == "xmlns") {
						if(nsmgr.LookupNamespace (String.Empty) != attr.Value)
							nsmgr.AddNamespace (String.Empty, attr.Value);
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
