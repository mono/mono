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

		private void AppendChildValues(XmlNode parent, StringBuilder builder)
		{
			XmlNode node = parent.FirstChild;

			while (node != null) {
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

			ownerDoc.onNodeInserting (newChild, this);

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute) {
				
				if (newChild.OwnerDocument != ownerDoc)
					throw new ArgumentException ("Can't append a node created by another document.");

				XmlLinkedNode newLinkedChild = (XmlLinkedNode) newChild;
				XmlLinkedNode lastLinkedChild = LastLinkedChild;

				newLinkedChild.parentNode = this;
				
				if (lastLinkedChild != null) {
					newLinkedChild.NextLinkedSibling = lastLinkedChild.NextLinkedSibling;
					lastLinkedChild.NextLinkedSibling = newLinkedChild;
				} else
					newLinkedChild.NextLinkedSibling = newLinkedChild;
				
				LastLinkedChild = newLinkedChild;

				ownerDoc.onNodeInserted (newChild, newChild.ParentNode);

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
		public virtual XPathNavigator CreateNavigator ()
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

			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute) 
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

		#endregion
	}
}
