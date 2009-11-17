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
using Mono.Xml;

namespace System.Xml
{
#if NET_2_0
	public sealed class XmlAttributeCollection : XmlNamedNodeMap, ICollection
#else
	public class XmlAttributeCollection : XmlNamedNodeMap, ICollection
#endif
	{
		XmlElement ownerElement;
		XmlDocument ownerDocument;

		internal XmlAttributeCollection (XmlNode parent) : base (parent)
		{
			ownerElement = parent as XmlElement;
			ownerDocument = parent.OwnerDocument;
			if(ownerElement == null)
				throw new XmlException ("invalid construction for XmlAttributeCollection.");
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		bool IsReadOnly {
			get { return ownerElement.IsReadOnly; }
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
#if NET_2_0
		public XmlAttribute this [string name] {
#else
		public virtual XmlAttribute this [string name] {
#endif
			get {
				return (XmlAttribute) GetNamedItem (name);
			}
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
#if NET_2_0
		public XmlAttribute this [int i] {
#else
		public virtual XmlAttribute this [int i] {
#endif
			get {
				return (XmlAttribute) Nodes [i];
			}
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
#if NET_2_0
		public XmlAttribute this [string localName, string namespaceURI] {
#else
		public virtual XmlAttribute this [string localName, string namespaceURI] {
#endif
			get {
				return (XmlAttribute) GetNamedItem (localName, namespaceURI);
			}
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

#if NET_2_0
		public XmlAttribute Append (XmlAttribute node) 
#else
		public virtual XmlAttribute Append (XmlAttribute node) 
#endif
		{
			SetNamedItem (node);
			return node;
		}

		public void CopyTo (XmlAttribute[] array, int index)
		{
			// assuming that Nodes is a correct collection.
			for(int i=0; i<Count; i++)
				array [index + i] = Nodes [i] as XmlAttribute;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			// assuming that Nodes is a correct collection.
			array.CopyTo (Nodes.ToArray (typeof(XmlAttribute)), index);
		}

#if NET_2_0
		public XmlAttribute InsertAfter (XmlAttribute newNode, XmlAttribute refNode)
#else
		public virtual XmlAttribute InsertAfter (XmlAttribute newNode, XmlAttribute refNode)
#endif
		{
			if (refNode == null) {
				if (Count == 0)
					return InsertBefore (newNode, null);
				else
					return InsertBefore (newNode, this [0]);
			}
			for (int i = 0; i < Count; i++)
				if (refNode == Nodes [i])
					return InsertBefore (newNode, Count == i + 1 ? null : this [i + 1]);

			throw new ArgumentException ("refNode not found in this collection.");
		}

#if NET_2_0
		public XmlAttribute InsertBefore (XmlAttribute newNode, XmlAttribute refNode)
#else
		public virtual XmlAttribute InsertBefore (XmlAttribute newNode, XmlAttribute refNode)
#endif
		{
			if (newNode.OwnerDocument != ownerDocument)
				throw new ArgumentException ("different document created this newNode.");

			ownerDocument.onNodeInserting (newNode, null);

			int pos = Count;
			if (refNode != null) {
				for (int i = 0; i < Count; i++) {
					XmlNode n = Nodes [i] as XmlNode;
					if (n == refNode) {
						pos = i;
						break;
					}
				}
				if (pos == Count)
					throw new ArgumentException ("refNode not found in this collection.");
			}
			SetNamedItem (newNode, pos, false);

			ownerDocument.onNodeInserted (newNode, null);

			return newNode;
		}

#if NET_2_0
		public XmlAttribute Prepend (XmlAttribute node) 
#else
		public virtual XmlAttribute Prepend (XmlAttribute node) 
#endif
		{
			return this.InsertAfter (node, null);
		}

#if NET_2_0
		public XmlAttribute Remove (XmlAttribute node) 
#else
		public virtual XmlAttribute Remove (XmlAttribute node) 
#endif
		{
			if (IsReadOnly)
				throw new ArgumentException ("This attribute collection is read-only.");
			if (node == null)
				throw new ArgumentException ("Specified node is null.");
			if (node.OwnerDocument != ownerDocument)
				throw new ArgumentException ("Specified node is in a different document.");
			if (node.OwnerElement != this.ownerElement)
				throw new ArgumentException ("The specified attribute is not contained in the element.");

			XmlAttribute retAttr = null;
			for (int i = 0; i < Count; i++) {
				XmlAttribute attr = (XmlAttribute) Nodes [i];
				if (attr == node) {
					retAttr = attr;
					break;
				}
			}

			if(retAttr != null) {
				ownerDocument.onNodeRemoving (node, ownerElement);
				base.RemoveNamedItem (retAttr.LocalName, retAttr.NamespaceURI);
				RemoveIdenticalAttribute (retAttr);
				ownerDocument.onNodeRemoved (node, ownerElement);
			}
			// If it is default, then directly create new attribute.
			DTDAttributeDefinition def = retAttr.GetAttributeDefinition ();
			if (def != null && def.DefaultValue != null) {
				XmlAttribute attr = ownerDocument.CreateAttribute (
					retAttr.Prefix, retAttr.LocalName, retAttr.NamespaceURI, true, false);
				attr.Value = def.DefaultValue;
				attr.SetDefault ();
				this.SetNamedItem (attr);
			}
			retAttr.AttributeOwnerElement = null;
			return retAttr;
		}

#if NET_2_0
		public void RemoveAll () 
#else
		public virtual void RemoveAll () 
#endif
		{
			int current = 0;
			while (current < Count) {
				XmlAttribute attr = this [current];
				if (!attr.Specified)
					current++;
				// It is called for the purpose of event support.
				Remove (attr);
			}
		}

#if NET_2_0
		public XmlAttribute RemoveAt (int i) 
#else
		public virtual XmlAttribute RemoveAt (int i) 
#endif
		{
			if(Count <= i)
				return null;
			return Remove ((XmlAttribute)Nodes [i]);
		}

		public override XmlNode SetNamedItem (XmlNode node)
		{
			if(IsReadOnly)
				throw new ArgumentException ("this AttributeCollection is read only.");

			XmlAttribute attr = node as XmlAttribute;
			if (attr.OwnerElement == ownerElement)
				return node; // do nothing
			if (attr.OwnerElement != null)
				throw new ArgumentException ("This attribute is already set to another element.");

			ownerElement.OwnerDocument.onNodeInserting (node, ownerElement);

			attr.AttributeOwnerElement = ownerElement;
			XmlNode n = base.SetNamedItem (node, -1, false);
			AdjustIdenticalAttributes (node as XmlAttribute, n == node ? null : n);

			ownerElement.OwnerDocument.onNodeInserted (node, ownerElement);

			return n as XmlAttribute;
		}

		internal void AddIdenticalAttribute ()
		{
			SetIdenticalAttribute (false);
		}

		internal void RemoveIdenticalAttribute ()
		{
			SetIdenticalAttribute (true);
		}

		private void SetIdenticalAttribute (bool remove)
		{
			if (ownerElement == null)
				return;

			// Check if new attribute's datatype is ID.
			XmlDocumentType doctype = ownerDocument.DocumentType;
			if (doctype == null || doctype.DTD == null)
				return;
			DTDElementDeclaration elem = doctype.DTD.ElementDecls [ownerElement.Name];
			for (int i = 0; i < Count; i++) {
				XmlAttribute node = (XmlAttribute) Nodes [i];
				DTDAttributeDefinition attdef = elem == null ? null : elem.Attributes [node.Name];
				if (attdef == null || attdef.Datatype.TokenizedType != XmlTokenizedType.ID)
					continue;

				if (remove) {
					if (ownerDocument.GetIdenticalAttribute (node.Value) != null) {
						ownerDocument.RemoveIdenticalAttribute (node.Value);
						return;
					}
				} else {
					// adding new identical attribute, but 
					// MS.NET is pity for ID support, so I'm wondering how to correct it...
					if (ownerDocument.GetIdenticalAttribute (node.Value) != null)
						throw new XmlException (String.Format (
							"ID value {0} already exists in this document.", node.Value));
					ownerDocument.AddIdenticalAttribute (node);
					return;
				}
			}

		}

		private void AdjustIdenticalAttributes (XmlAttribute node, XmlNode existing)
		{
			// If owner element is not appended to the document,
			// ID table should not be filled.
			if (ownerElement == null)
				return;

			if (existing != null)
				RemoveIdenticalAttribute (existing);

			// Check if new attribute's datatype is ID.
			XmlDocumentType doctype = node.OwnerDocument.DocumentType;
			if (doctype == null || doctype.DTD == null)
				return;
			DTDAttListDeclaration attList = doctype.DTD.AttListDecls [ownerElement.Name];
			DTDAttributeDefinition attdef = attList == null ? null : attList.Get (node.Name);
			if (attdef == null || attdef.Datatype.TokenizedType != XmlTokenizedType.ID)
				return;

			ownerDocument.AddIdenticalAttribute (node);
		}

		private XmlNode RemoveIdenticalAttribute (XmlNode existing)
		{
			// If owner element is not appended to the document,
			// ID table should not be filled.
			if (ownerElement == null)
				return existing;

			if (existing != null) {
				// remove identical attribute (if it is).
				if (ownerDocument.GetIdenticalAttribute (existing.Value) != null)
					ownerDocument.RemoveIdenticalAttribute (existing.Value);
			}

			return existing;
		}
	}
}
