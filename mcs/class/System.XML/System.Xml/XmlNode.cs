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

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.XPath;
#if NET_2_0
using System.Diagnostics;
using System.Xml.Schema;
#endif

namespace System.Xml
{
	public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable
	{
		static EmptyNodeList emptyList = new EmptyNodeList ();

		class EmptyNodeList : XmlNodeList
		{
			static IEnumerator emptyEnumerator = new object [0].GetEnumerator ();

			public override int Count {
				get { return 0; }
			}

			public override IEnumerator GetEnumerator ()
			{
				return emptyEnumerator;
			}

			public override XmlNode Item (int index)
			{
				return null;
			}
		}

		#region Fields

		XmlDocument ownerDocument;
		XmlNode parentNode;
		XmlNodeListChildren childNodes;

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
				return (ParentNode != null) ? ParentNode.ChildrenBaseURI : String.Empty;
			}
		}

		internal virtual string ChildrenBaseURI {
			get {
				return BaseURI;
			}
		}

		public virtual XmlNodeList ChildNodes {
			get {
				IHasXmlChildNode l = this as IHasXmlChildNode;
				if (l == null)
					return emptyList;
				if (childNodes == null)
					childNodes = new XmlNodeListChildren (l);
				return childNodes;
			}
		}

		public virtual XmlNode FirstChild {
			get {
				IHasXmlChildNode l = this as IHasXmlChildNode;
				XmlLinkedNode c = (l == null) ?
					null : l.LastLinkedChild;
				return c == null ? null : c.NextLinkedSibling;
			}
		}

		public virtual bool HasChildNodes {
			get { return LastChild != null; }
		}

		public virtual string InnerText {
			get {
				switch (NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
 					return Value;
				}
				if (FirstChild == null)
					return String.Empty;
				if (FirstChild == LastChild)
					return FirstChild.NodeType != XmlNodeType.Comment ?
						FirstChild.InnerText :
						String.Empty;

				StringBuilder builder = null;
				AppendChildValues (ref builder);
				return builder == null ? String.Empty : builder.ToString ();
			}

			set {
				if (! (this is XmlDocumentFragment))
					throw new InvalidOperationException ("This node is read only. Cannot be modified.");
				RemoveAll ();
				AppendChild (OwnerDocument.CreateTextNode (value));
			}
		}

		private void AppendChildValues (ref StringBuilder builder)
		{
			XmlNode node = FirstChild;

			while (node != null) {
				switch (node.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
					if (builder == null)
						builder = new StringBuilder ();
 					builder.Append (node.Value);
					break;
				}
				node.AppendChildValues (ref builder);
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
			get 
			{
				XmlNode curNode = this;
				do
				{
					switch (curNode.NodeType) 
					{
						case XmlNodeType.EntityReference:
						case XmlNodeType.Entity:
							return true;

						case XmlNodeType.Attribute:
							curNode = ((XmlAttribute)curNode).OwnerElement;
							break;

						default:
							curNode = curNode.ParentNode;
							break;
					}
				}
				while (curNode != null) ;

				return false;
			}
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public virtual XmlElement this [string name] {
			get { 
				for (int i = 0; i < ChildNodes.Count; i++) {
					XmlNode node = ChildNodes [i];
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
				for (int i = 0; i < ChildNodes.Count; i++) {
					XmlNode node = ChildNodes [i];
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
			get {
				IHasXmlChildNode l = this as IHasXmlChildNode;
				return l == null ? null : l.LastLinkedChild;
			}
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
				throw new InvalidOperationException ("Can not get XPath node type from " + this.GetType ().ToString ());
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
					for (int i = 0; i < Attributes.Count; i++) {
						XmlAttribute attr = Attributes [i];
						if(attr.Name == "xml:lang")
							return attr.Value;
					}
				return (ParentNode != null) ? ParentNode.XmlLang : OwnerDocument.XmlLang;
			}
		}

		internal virtual XmlSpace XmlSpace {
			get {
				if(Attributes != null) {
					for (int i = 0; i < Attributes.Count; i++) {
						XmlAttribute attr = Attributes [i];
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

#if NET_2_0
		public virtual IXmlSchemaInfo SchemaInfo {
			get { return null; }
			internal set { }
		}
#endif

		#endregion

		#region Methods

		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			// AppendChild(n) is equivalent to InsertBefore(n, null)
			return InsertBefore (newChild, null);
		}

		internal XmlNode AppendChild (XmlNode newChild, bool checkNodeType)
		{
			return InsertBefore (newChild, null, checkNodeType, true);
		}

		public virtual XmlNode Clone ()
		{
			// By MS document, it is equivalent to CloneNode(true).
			return this.CloneNode (true);
		}

		public abstract XmlNode CloneNode (bool deep);

#if NET_2_0
		public virtual XPathNavigator CreateNavigator ()
		{
			// XmlDocument has overriden definition, so it is safe
			// to use OwnerDocument here.
			return OwnerDocument.CreateNavigator (this);
		}
#else
		public XPathNavigator CreateNavigator ()
		{
			XmlDocument document = this.NodeType == XmlNodeType.Document ?
				this as XmlDocument : this.ownerDocument;
			return document.CreateNavigator (this);
		}
#endif

		public IEnumerator GetEnumerator ()
		{
			return ChildNodes.GetEnumerator ();
		}

		public virtual string GetNamespaceOfPrefix (string prefix)
		{
			switch (prefix) {
			case null:
				throw new ArgumentNullException ("prefix");
#if NET_2_0
			case "xml":
				return XmlNamespaceManager.XmlnsXml;
			case "xmlns":
				return XmlNamespaceManager.XmlnsXmlns;
#endif
			}

			XmlNode node;
			switch (NodeType) {
			case XmlNodeType.Attribute:
				node = ((XmlAttribute) this).OwnerElement;
				if (node == null)
					return String.Empty;
				break;
			case XmlNodeType.Element:
				node = this;
				break;
			default:
				node = ParentNode;
				break;
			}

			while (node != null) {
				if (node.Prefix == prefix)
					return node.NamespaceURI;
				if (node.NodeType == XmlNodeType.Element &&
				    ((XmlElement) node).HasAttributes) {
					int count = node.Attributes.Count;
					for (int i = 0; i < count; i++) {
						XmlAttribute attr = node.Attributes [i];
						if (prefix == attr.LocalName && attr.Prefix == "xmlns"
							|| attr.Name == "xmlns" && prefix == String.Empty)
							return attr.Value;
					}
				}
				node = node.ParentNode;
			}
			return String.Empty;
		}

		public virtual string GetPrefixOfNamespace (string namespaceURI)
		{
#if NET_2_0
			switch (namespaceURI) {
			case XmlNamespaceManager.XmlnsXml:
				return XmlNamespaceManager.PrefixXml;
			case XmlNamespaceManager.XmlnsXmlns:
				return XmlNamespaceManager.PrefixXmlns;
			}
#endif

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

			while (node != null) {
				if (node.NodeType == XmlNodeType.Element &&
				    ((XmlElement) node).HasAttributes) {
					for (int i = 0; i < node.Attributes.Count; i++) {
						XmlAttribute attr = node.Attributes [i];
						if (attr.Prefix == "xmlns" && attr.Value == namespaceURI)
							return attr.LocalName;
						else if (attr.Name == "xmlns" && attr.Value == namespaceURI)
							return String.Empty;
					}
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
			// InsertAfter(n1, n2) is equivalent to InsertBefore(n1, n2.PreviousSibling).

			// I took this way because current implementation 
			// Calling InsertBefore() in this method is faster than
			// the counterpart, since NextSibling is faster than 
			// PreviousSibling (these children are forward-only list).
			XmlNode argNode = null;
			if (refChild != null)
				argNode = refChild.NextSibling;
			else if (FirstChild != null)
				argNode = FirstChild;
			return InsertBefore (newChild, argNode);
		}

		public virtual XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			return InsertBefore (newChild, refChild, true, true);
		}

		// check for the node to be one of node ancestors
		internal bool IsAncestor (XmlNode newChild)
		{
			XmlNode currNode = this.ParentNode;
			while(currNode != null)
			{
				if(currNode == newChild)
					return true;
				currNode = currNode.ParentNode;
			}
			return false;
		}

		internal XmlNode InsertBefore (XmlNode newChild, XmlNode refChild, bool checkNodeType, bool raiseEvent)
		{
			if (checkNodeType)
				CheckNodeInsertion (newChild, refChild);

			if (newChild == refChild)
				return newChild;

			IHasXmlChildNode l = (IHasXmlChildNode) this;

			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument) this : OwnerDocument;

			if (raiseEvent)
				ownerDoc.onNodeInserting (newChild, this);

			if (newChild.ParentNode != null)
				newChild.ParentNode.RemoveChild (newChild, checkNodeType);

			if (newChild.NodeType == XmlNodeType.DocumentFragment) {
				// This recursively invokes events. (It is compatible with MS implementation.)
				while (newChild.FirstChild != null)
					this.InsertBefore (newChild.FirstChild, refChild);
			}
			else {
				XmlLinkedNode newLinkedChild = (XmlLinkedNode) newChild;
				newLinkedChild.parentNode = this;

				if (refChild == null) {
					// newChild is the last child:
					// * set newChild as NextSibling of the existing lastchild
					// * set LastChild = newChild
					// * set NextSibling of newChild as FirstChild
					if (l.LastLinkedChild != null) {
						XmlLinkedNode formerFirst = (XmlLinkedNode) FirstChild;
						l.LastLinkedChild.NextLinkedSibling = newLinkedChild;
						l.LastLinkedChild = newLinkedChild;
						newLinkedChild.NextLinkedSibling = formerFirst;
					} else {
						l.LastLinkedChild = newLinkedChild;
						l.LastLinkedChild.NextLinkedSibling = newLinkedChild;	// FirstChild
					}
				} else {
					// newChild is not the last child:
					// * if newchild is first, then set next of lastchild is newChild.
					//   otherwise, set next of previous sibling to newChild
					// * set next of newChild to refChild
					XmlLinkedNode prev = refChild.PreviousSibling as XmlLinkedNode;
					if (prev == null)
						l.LastLinkedChild.NextLinkedSibling = newLinkedChild;
					else
						prev.NextLinkedSibling = newLinkedChild;
					newLinkedChild.NextLinkedSibling = refChild as XmlLinkedNode;
				}
				switch (newChild.NodeType) {
				case XmlNodeType.EntityReference:
					((XmlEntityReference) newChild).SetReferencedEntityContent ();
					break;
				case XmlNodeType.Entity:
					break;
				case XmlNodeType.DocumentType:
					break;
				}

				if (raiseEvent)
					ownerDoc.onNodeInserted (newChild, newChild.ParentNode);
			}
			return newChild;
		}

		private void CheckNodeInsertion (XmlNode newChild, XmlNode refChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument) this : OwnerDocument;

			if (NodeType != XmlNodeType.Element &&
			    NodeType != XmlNodeType.Attribute &&
			    NodeType != XmlNodeType.Document &&
			    NodeType != XmlNodeType.DocumentFragment)
				throw new InvalidOperationException (String.Format ("Node cannot be appended to current node {0}.", NodeType));

			switch (NodeType) {
			case XmlNodeType.Attribute:
				switch (newChild.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.EntityReference:
					break;
				default:
					throw new InvalidOperationException (String.Format (
						"Cannot insert specified type of node {0} as a child of this node {1}.", 
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
					throw new InvalidOperationException ("Cannot insert specified type of node as a child of this node.");
				}
				break;
			}

			if (IsReadOnly)
				throw new InvalidOperationException ("The node is readonly.");

			if (newChild.OwnerDocument != ownerDoc)
				throw new ArgumentException ("Can't append a node created by another document.");

			if (refChild != null) {
				if (refChild.ParentNode != this)
					throw new ArgumentException ("The reference node is not a child of this node.");
			}

			if(this == ownerDoc && ownerDoc.DocumentElement != null && (newChild is XmlElement) && newChild != ownerDoc.DocumentElement)
				throw new XmlException ("multiple document element not allowed.");

			// checking validity finished. then appending...

			
			if (newChild == this || IsAncestor (newChild))
				throw new ArgumentException("Cannot insert a node or any ancestor of that node as a child of itself.");

		}

		public virtual void Normalize ()
		{
			StringBuilder tmpBuilder = new StringBuilder ();
			int count = this.ChildNodes.Count;
			int start = 0;
			for (int i = 0; i < count; i++) {
				XmlNode c = ChildNodes [i];
				switch (c.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					tmpBuilder.Append (c.Value);
					break;
				default:
					c.Normalize ();
					NormalizeRange (start, i, tmpBuilder);
					// Continue to normalize from next node.
					start = i + 1;
					break;
				}
			}
			if (start < count) {
				NormalizeRange (start, count, tmpBuilder);
			}
		}

		private void NormalizeRange (int start, int i, StringBuilder tmpBuilder)
		{
			int keepPos = -1;
			// If Texts and Whitespaces are mixed, Text takes precedence to remain.
			// i.e. Whitespace should be removed.
			for (int j = start; j < i; j++) {
				XmlNode keep = ChildNodes [j];
				if (keep.NodeType == XmlNodeType.Text) {
					keepPos = j;
					break;
				}
				else if (keep.NodeType == XmlNodeType.SignificantWhitespace)
					keepPos = j;
					// but don't break up to find Text nodes.
			}

			if (keepPos >= 0) {
				for (int del = start; del < keepPos; del++)
					RemoveChild (ChildNodes [start]);
				int rest = i - keepPos - 1;
				for (int del = 0; del < rest; del++) {
					RemoveChild (ChildNodes [start + 1]);
}
			}

			if (keepPos >= 0)
				ChildNodes [start].Value = tmpBuilder.ToString ();
			// otherwise nothing to be normalized

			tmpBuilder.Length = 0;
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
			return RemoveChild (oldChild, true);
		}

		private void CheckNodeRemoval ()
		{
			if (NodeType != XmlNodeType.Attribute && 
				NodeType != XmlNodeType.Element && 
				NodeType != XmlNodeType.Document && 
				NodeType != XmlNodeType.DocumentFragment)
				throw new ArgumentException (String.Format ("This {0} node cannot remove its child.", NodeType));

			if (IsReadOnly)
				throw new ArgumentException (String.Format ("This {0} node is read only.", NodeType));
		}

		internal XmlNode RemoveChild (XmlNode oldChild, bool checkNodeType)
		{
			if (oldChild == null)
				throw new NullReferenceException ();
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;
			if(oldChild.ParentNode != this)
				throw new ArgumentException ("The node to be removed is not a child of this node.");

			if (checkNodeType)
				ownerDoc.onNodeRemoving (oldChild, oldChild.ParentNode);

			if (checkNodeType)
				CheckNodeRemoval ();

			IHasXmlChildNode l = (IHasXmlChildNode) this;

			if (Object.ReferenceEquals (l.LastLinkedChild, l.LastLinkedChild.NextLinkedSibling) && Object.ReferenceEquals (l.LastLinkedChild, oldChild))
				// If there is only one children, simply clear.
				l.LastLinkedChild = null;
			else {
				XmlLinkedNode oldLinkedChild = (XmlLinkedNode) oldChild;
				XmlLinkedNode beforeLinkedChild = l.LastLinkedChild;
				XmlLinkedNode firstChild = (XmlLinkedNode) FirstChild;
				
				while (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, l.LastLinkedChild) == false && 
					Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					beforeLinkedChild = beforeLinkedChild.NextLinkedSibling;

				if (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					throw new ArgumentException ();

				beforeLinkedChild.NextLinkedSibling = oldLinkedChild.NextLinkedSibling;

				// Each derived class may have its own l.LastLinkedChild, so we must set it explicitly.
				if (oldLinkedChild.NextLinkedSibling == firstChild)
					l.LastLinkedChild = beforeLinkedChild;

				oldLinkedChild.NextLinkedSibling = null;
				}

			if (checkNodeType)
				ownerDoc.onNodeRemoved (oldChild, oldChild.ParentNode);
			oldChild.parentNode = null;	// clear parent 'after' above logic.

			return oldChild;
		}

		public virtual XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			if(oldChild.ParentNode != this)
				throw new ArgumentException ("The node to be removed is not a child of this node.");
			
			if (newChild == this || IsAncestor (newChild))
				throw new InvalidOperationException("Cannot insert a node or any ancestor of that node as a child of itself.");
			
			XmlNode next = oldChild.NextSibling;
			RemoveChild (oldChild);
			InsertBefore (newChild, next);
			return oldChild;
		}

		// WARNING: don't use this member outside XmlAttribute nodes.
		internal XmlElement AttributeOwnerElement {
			get { return (XmlElement) parentNode; }
			set { parentNode = value; }
		}

		internal void SearchDescendantElements (string name, bool matchAll, ArrayList list)
		{
			for (XmlNode n = FirstChild; n != null; n = n.NextSibling) {
				if (n.NodeType != XmlNodeType.Element)
					continue;
				if (matchAll || n.Name == name)
					list.Add (n);
				n.SearchDescendantElements (name, matchAll, list);
			}
		}

		internal void SearchDescendantElements (string name, bool matchAllName, string ns, bool matchAllNS, ArrayList list)
		{
			for (XmlNode n = FirstChild; n != null; n = n.NextSibling) {
				if (n.NodeType != XmlNodeType.Element)
					continue;
				if ((matchAllName || n.LocalName == name)
					&& (matchAllNS || n.NamespaceURI == ns))
					list.Add (n);
				n.SearchDescendantElements (name, matchAllName, ns, matchAllNS, list);
			}
		}

		public XmlNodeList SelectNodes (string xpath)
		{
			return SelectNodes (xpath, null);
		}

		public XmlNodeList SelectNodes (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			/*
			ArrayList rgNodes = new ArrayList ();
			while (iter.MoveNext ())
			{
				rgNodes.Add (((IHasXmlNode) iter.Current).GetNode ());
			}
			return new XmlNodeArrayList (rgNodes);
			*/
			return new XmlIteratorNodeList (iter);
		}

		public XmlNode SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		public XmlNode SelectSingleNode (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			if (!iter.MoveNext ())
				return null;
			return (iter.Current as IHasXmlNode).GetNode ();
		}

		public virtual bool Supports (string feature, string version)
		{
			if (String.Compare (feature, "xml", true, CultureInfo.InvariantCulture) == 0 // not case-sensitive
			    && (String.Compare (version, "1.0", true, CultureInfo.InvariantCulture) == 0
				|| String.Compare (version, "2.0", true, CultureInfo.InvariantCulture) == 0))
				return true;
			else
				return false;
		}

		public abstract void WriteContentTo (XmlWriter w);

		public abstract void WriteTo (XmlWriter w);

		// It parses this and all the ancestor elements,
		// find 'xmlns' declarations, stores and then return them.
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

			while (el != null) {
				for (int i = 0; i < el.Attributes.Count; i++) {
					XmlAttribute attr = el.Attributes [i];
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
