//
// System.Xml.XmlAttributeCollection
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlAttributeCollection : XmlNamedNodeMap, ICollection
	{
		XmlElement ownerElement;

		internal XmlAttributeCollection (XmlNode parent) : base (parent)
		{
			ownerElement = parent as XmlElement;
			if(ownerElement == null)
				throw new XmlException ("invalid construction for XmlAttributeCollection.");
		}

		bool ICollection.IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IsReadOnly {
			get {
				return ownerElement.IsReadOnly;
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string name] {
			get {
				return (XmlAttribute) GetNamedItem (name);
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [int i] {
			get {
				return (XmlAttribute) Nodes [i];
			}
		}

		[MonoTODO]
		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string localName, string namespaceURI] {
			get {
				return (XmlAttribute) GetNamedItem (localName, namespaceURI);
			}
		}

		object ICollection.SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		
		public virtual XmlAttribute Append (XmlAttribute node) 
		{
			XmlNode xmlNode = this.SetNamedItem (node);
			return node;
		}	

		public void CopyTo (XmlAttribute[] array, int index)
		{
			// assuming that Nodes is a correct collection.
			for(int i=0; i<Nodes.Count; i++)
				array [index + i] = Nodes [i] as XmlAttribute;
		}

		[MonoTODO] // I don't know why this method is required...
		void ICollection.CopyTo (Array array, int index)
		{
			// assuming that Nodes is a correct collection.
			array.CopyTo (Nodes.ToArray (typeof(XmlAttribute)), index);
		}

		public virtual XmlAttribute InsertAfter (XmlAttribute newNode, XmlAttribute refNode)
		{
			if(newNode.OwnerDocument != this.ownerElement.OwnerDocument)
				throw new ArgumentException ("different document created this newNode.");

			ownerElement.OwnerDocument.onNodeInserting (newNode, null);

			int pos = Nodes.Count + 1;
			if(refNode != null)
			{
				for(int i=0; i<Nodes.Count; i++)
				{
					XmlNode n = Nodes [i] as XmlNode;
					if(n == refNode)
					{
						pos = i + 1;
						break;
					}
				}
				if(pos > Nodes.Count)
					throw new XmlException ("refNode not found in this collection.");
			}
			else
				pos = 0;
			SetNamedItem (newNode, pos);

			ownerElement.OwnerDocument.onNodeInserted (newNode, null);

			return newNode;
		}

		public virtual XmlAttribute InsertBefore (XmlAttribute newNode, XmlAttribute refNode)
		{
			if(newNode.OwnerDocument != this.ownerElement.OwnerDocument)
				throw new ArgumentException ("different document created this newNode.");

			ownerElement.OwnerDocument.onNodeInserting (newNode, null);

			int pos = Nodes.Count;
			if(refNode != null)
			{
				for(int i=0; i<Nodes.Count; i++)
				{
					XmlNode n = Nodes [i] as XmlNode;
					if(n == refNode)
					{
						pos = i;
						break;
					}
				}
				if(pos == Nodes.Count)
					throw new XmlException ("refNode not found in this collection.");
			}
			SetNamedItem (newNode, pos);

			ownerElement.OwnerDocument.onNodeInserted (newNode, null);

			return newNode;
		}

		public virtual XmlAttribute Prepend (XmlAttribute node) 
		{
			return this.InsertAfter (node, null);
		}

		public virtual XmlAttribute Remove (XmlAttribute node) 
		{
			if(node == null || node.OwnerDocument != this.ownerElement.OwnerDocument)
				throw new ArgumentException ("node is null or different document created this node.");

			XmlAttribute retAttr = null;
			foreach(XmlAttribute attr in Nodes)
			{
				if(attr == node)
				{
					retAttr = attr;
					break;
				}
			}

			if(retAttr != null)
			{
				ownerElement.OwnerDocument.onNodeRemoving (node, null);
				base.RemoveNamedItem (retAttr.LocalName, retAttr.NamespaceURI);
				ownerElement.OwnerDocument.onNodeRemoved (node, null);
			}
			return retAttr;
		}

		public virtual void RemoveAll () 
		{
			while(Count > 0)
				Remove ((XmlAttribute)Nodes [0]);
		}

		public virtual XmlAttribute RemoveAt (int i) 
		{
			if(Nodes.Count <= i)
				return null;
			return Remove ((XmlAttribute)Nodes [i]);
		}

		public override XmlNode SetNamedItem (XmlNode node)
		{
			return SetNamedItem(node, -1);
		}

		[MonoTODO("event handling")]
		internal new XmlNode SetNamedItem (XmlNode node, int pos)
		{
			if(IsReadOnly)
				throw new XmlException ("this AttributeCollection is read only.");

			return base.SetNamedItem (node, pos);
		}
	}
}
