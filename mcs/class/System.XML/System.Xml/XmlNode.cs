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

		protected internal XmlNode(XmlDocument ownerDocument)
		{
			this.ownerDocument = ownerDocument;
		}

		#endregion

		#region Properties

		public virtual XmlAttributeCollection Attributes
		{
			get { return null; }
		}

		[MonoTODO]
		public virtual string BaseURI
		{
			get { throw new NotImplementedException (); }
		}

		public virtual XmlNodeList ChildNodes {
			get {
				return new XmlNodeListChildren(LastLinkedChild);
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
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string InnerXml {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool IsReadOnly {
			get { throw new NotImplementedException (); }
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

		[MonoTODO]
		public abstract string LocalName { get;	}

		[MonoTODO]
		public abstract string Name	{ get; }

		[MonoTODO]
		public virtual string NamespaceURI {
			get { throw new NotImplementedException (); }
		}

		public virtual XmlNode NextSibling {
			get { return null; }
		}

		[MonoTODO]
		public abstract XmlNodeType NodeType { get;	}

		[MonoTODO]
		public virtual string OuterXml {
			get { throw new NotImplementedException (); }
		}

		public virtual XmlDocument OwnerDocument {
			get { return ownerDocument; }
		}

		public virtual XmlNode ParentNode {
			get { return parentNode; }
		}

		[MonoTODO]
		public virtual string Prefix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public virtual XmlNode PreviousSibling {
			get { return null; }
		}

		[MonoTODO]
		public virtual string Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			if (NodeType == XmlNodeType.Document || NodeType == XmlNodeType.Element || NodeType == XmlNodeType.Attribute) {
				XmlLinkedNode newLinkedChild = (XmlLinkedNode)newChild;
				XmlLinkedNode lastLinkedChild = LastLinkedChild;
				if (lastLinkedChild != null) {
					newLinkedChild.NextLinkedSibling = lastLinkedChild.NextLinkedSibling;
					lastLinkedChild.NextLinkedSibling = newLinkedChild;
				} else
					newLinkedChild.NextLinkedSibling = newLinkedChild;
				LastLinkedChild = newLinkedChild;
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
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
			LastLinkedChild = null;
		}

		[MonoTODO]
		public virtual XmlNode RemoveChild (XmlNode oldChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList SelectNodes (string xpath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNodeList SelectNodes (string xpath, XmlNamespaceManager nsmgr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode SelectSingleNode (string xpath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XmlNode SelectSingleNode (string xpath, XmlNamespaceManager nsmgr)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public abstract void WriteContentTo (XmlWriter w);

		[MonoTODO]
		public abstract void WriteTo (XmlWriter w);

		#endregion
	}
}
