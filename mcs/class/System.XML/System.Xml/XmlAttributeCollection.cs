// System.Xml.XmlAttributeCollection.cs
//
// Author: Daniel Weber (daniel-weber@austin.rr.com)
//
// Implementation of abstract Xml.XmlAttributeCollection class
//

using System;
using System.Collections;

namespace System.Xml
{
	/// <summary>
	/// A collection of Attributes that can be accessed by index or name(space)
	/// Derived from XmlNamedNodeMap
	/// <seealso cref="XmlNamedNodeMap"/>
	/// </summary>
	public class XmlAttributeCollection : XmlNamedNodeMap, ICollection
	{

		// =====  ICollection interface elements  ===================================
		/// <summary>
		/// Private class to provide Synchronzed Access to the attribute list.
		/// </summary>
		private class SyncAttributes : XmlAttributeCollection
		{
			private XmlAttributeCollection _attributes;

			public SyncAttributes ( XmlAttributeCollection attributes )
			{
				_attributes = attributes;
			}

			public override bool IsSynchronized 
			{
				get {return true; }
			}

			// Override all properties/methods that modify/read items
			//	and lock them so they are thread-safe
			public override void CopyTo(Array array, int index)
			{
				lock (_attributes )
				{ _attributes.CopyTo(array, index); }
			}
			
			public override XmlAttribute this[string name] 
			{
				get  {
					lock (_attributes ) { return _attributes[name]; }
				}
			}

			public override XmlAttribute this[int i] 
			{
				get {
					lock (_attributes) { return _attributes[i]; }
				}
			}

			public override XmlAttribute Append( XmlAttribute node )
			{
				lock (_attributes)
				{ return _attributes.Append( node ); }
			}
			
			public override void CopyTo(XmlAttribute[] array, int index)
			{
				lock (_attributes)
				{ _attributes.CopyTo(array, index); }
			}

			public override XmlAttribute InsertAfter( 
				XmlAttribute newNode, 
				XmlAttribute refNode)
			{
				lock (_attributes)
				{ return _attributes.InsertAfter( newNode, refNode ); }
			}

			public override XmlAttribute Prepend(XmlAttribute node)
			{
				lock (_attributes)
				{ return _attributes.Prepend(node); }
			}

			public override XmlAttribute Remove(XmlAttribute node)
			{
				lock (_attributes)
				{ return _attributes.Remove( node ); }
			}

			public override void RemoveAll()
			{
				lock (_attributes)
				{ _attributes.RemoveAll(); }
			}

			public override XmlAttribute RemoveAt(int i)
			{
				lock (_attributes)
				{ return _attributes.RemoveAt(i); }
			}

			public override XmlNode SetNamedItem(XmlNode node)
			{
				lock (_attributes)
				{ return _attributes.SetNamedItem(node); }
			}

			// Even ToString, since someone could come along and blow away an
			//	attribute while we're iterating...
			public override string ToString()
			{
				lock (_attributes) 
				{ return _attributes.ToString(); }
			}

		}  // SynchAttributes

		/// <summary>
		/// Return true if access is synchronized (thread-safe)
		/// </summary>
		public virtual bool IsSynchronized 
		{
			// This version of the class is not synchronized
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Return object used for synchronous access to class
		/// </summary>
		public object SyncRoot 
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Returns a thread-safe version of the attribute collection.
		/// </summary>
		/// <param name="attributes">Attribute collection to make thread-safe.</param>
		/// <returns>Thread-safe XmlAttributeCollection.</returns>
		public static XmlAttributeCollection Synchronized(XmlAttributeCollection attributes) 
		{
			if (attributes == null) 
			{
				throw new ArgumentNullException("Null XmlAttributeCollection passed to Synchronized()");
			}

			return new SyncAttributes(attributes);
		}

		/// <summary>
		/// Copy the XmlAttributeCollection into the passed array.  Index is zero-based.
		/// </summary>
		/// <param name="array">Array to copy into</param>
		/// <param name="index">Index to start copying from</param>
		public virtual void CopyTo(Array array, int index)
		{
			// Let the Array handle all the errors, there's no risk to us

			// TODO - should we set OwnerElement to null in clone() in CopyTo(Array, int)? (yes, using setOwnerElement())
			int arrayIndex = 0;
			for (int i = index; i < FnodeList.Count; i++)
			{
				XmlAttribute e = FnodeList[i] as XmlAttribute;
				XmlAttribute theClone = e.Clone() as XmlAttribute;
				theClone.setOwnerElement(null);
				array.SetValue(theClone, arrayIndex);
				arrayIndex++;
			}
		}
		// XmlAttributeCollection Properties =================================
		/// <summary>
		/// Get the attribute with the specified name
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("ItemOf")]
		public virtual XmlAttribute this[string name] 
		{
			get
			{
				return GetNamedItem(name) as XmlAttribute;
			}
		}

		/// <summary>
		/// Get the attribute at the specified index.  The Collection is zero-based.
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("ItemOf")]
		public virtual XmlAttribute this[int i] 
		{
			get
			{
				return base.Item(i) as XmlAttribute;
			}
		}

		/// <summary>
		/// Get the attribute with the specifed (localName, URI)
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("ItemOf")]
		public virtual XmlAttribute this[string localName, string namespaceURI] 
		{
			get
			{
				return GetNamedItem(localName, namespaceURI) as XmlAttribute;
			}
		}

		// ============= Public methods =====================================
		/// <summary>
		/// Appends the specified node to the attribute list
		/// If the node is already in the list, it is moved to the end.
		/// If a node is in the list with the same name, the node is removed and the new node is added.
		/// </summary>
		/// <param name="node">Attribute node to append to the collection</param>
		/// <exception cref="ArgumentException">Node was created from a differant document or node is null</exception>
		/// <returns></returns>
		public virtual XmlAttribute Append( XmlAttribute node )
		{
			// TODO - node validation? (no)
			
			XmlAttribute retval = null;

			System.Diagnostics.Debug.Assert(node != null, "Null node passed to Append()");

			if (! FOwner.OwnerDocument.Equals(node.OwnerDocument))
				throw new ArgumentException("Cannot append node from another document");

			if (node.OwnerElement != null) 
				throw new ArgumentException("Cannot append node from another document");

			foreach (XmlAttribute cur in FnodeList)
			{
				// If node is already in the collection, it is moved to the last position. 
				if (cur.Equals(node))
				{
					retval = cur;
					FnodeList.Remove(cur);
				}

				//If an attribute with the same name is already present in the collection, 
				//   the original attribute is removed from the collection and 
				//   node is added to the end of the collection.
				if (cur.Name == node.Name)
					FnodeList.Remove(cur);
			}

			// add the new node to the end of the collection
			// set attribute owner element? (yes)
			node.setOwnerElement(FOwnerNode as XmlElement);
			FnodeList.Add(node);

			// return the removed item
			return retval;
		}

		/// <summary>
		/// Copies all attributes in collection into the array, starting at index.
		/// attribute index is zero-based.
		/// </summary>
		/// <exception cref="OverflowException">Thrown if insufficient room to copy all elements</exception>
		/// <param name="array">Array to copy XlmAttributes into</param>
		/// <param name="index">index to start copy</param>
		public virtual void CopyTo(XmlAttribute[] array, int index)
		{
			// Let the array handle all the errors, there's no risk to us

			// TODO - should we set OwnerElement to null in clone() in CopyTo(XmlAttribute[], int)? (yes, using setOwnerElement())
			int arrayIndex = 0;
			for (int i = index; i < FnodeList.Count; i++)
			{
				XmlAttribute e = FnodeList[i] as XmlAttribute;
				XmlAttribute theClone = e.Clone() as XmlAttribute;
				theClone.setOwnerElement(null);
				array[arrayIndex] = theClone;
				arrayIndex++;
			}
		}

		/// <summary>
		/// Helper function since InsertBefore/After use exact same algorithm
		/// </summary>
		/// <param name="refNode"></param>
		/// <param name="newNode"></param>
		/// <param name="offset">offset to add to Insert (0 or 1)</param>
		/// <returns>Deleted attribute</returns>
		private XmlAttribute InsertHelper(XmlAttribute newNode, XmlAttribute refNode, int offset)
		{
			// TODO - validation? (no)
			if (refNode == null)
				throw new ArgumentNullException("Null refNode passed to InsertAfter()");
			if (newNode == null)
				throw new ArgumentNullException("Null newNode passed to InsertAfter()");

			if (! newNode.OwnerDocument.Equals(refNode.OwnerDocument) )
				throw new ArgumentException("Node to insert does not have same owner document as reference");

			// Logically, it makes no sense to insert node "A" after node "A",
			//	since only one node "A" can be in the list - flag it as an error
			if (newNode.Name == refNode.Name)
				throw new ArgumentException("Node to insert has same name as reference node");

			// Other bizarre case is if refNode.Equals(newNode)
			//	We'll flag this error after we check that refNode is in the list
		
			int refNodeIndex = -1;
			// Note that if newNode is in the list, then we'll get a name match
			int SameNameIndex = -1;

			for (int i = 0; i < FnodeList.Count; i++)
			{
				XmlAttribute curListNode = Item(i) as XmlAttribute;
				if (curListNode.Name == refNode.Name)
					refNodeIndex = i;
				
				if (curListNode.Name == newNode.Name)
					SameNameIndex = i;
			}

			if ( refNodeIndex == -1 )
				throw new ArgumentException("Attribute [" + refNode.Name + "] is not in Collection for InsertAfter()");

			// Check the obvious, InsertAfter( attr1, attr1);
			if (refNode.Equals( newNode ) )
				return newNode;

			XmlAttribute retval = null;

			if (SameNameIndex != -1)
			{
				// If this is newNode in the list, we'll insert it back in the right spot
				// If this is another node, just remove it
				retval = FnodeList[SameNameIndex] as XmlAttribute;
				FnodeList.RemoveAt(SameNameIndex);
				if ( SameNameIndex < refNodeIndex )
					refNodeIndex--;
			}

			FnodeList.Insert(refNodeIndex + offset, newNode);

			// TODO - set OwnerElement? (no)
			//node.setOwnerElement(FOwnerNode as XmlElement);

			// TODO - determine which node to return (deleted node)
			return retval;
		}

		/// <summary>
		/// Insert the specifed attribute immediately after the reference node.
		/// If an attribute with the same name is already in the collection, that attribute is removed and the new attribute inserted
		/// </summary>
		/// <exception cref="ArgumentException">Raised if newNode OwnerDocument differs from nodelist owner or refNode is not a member of the collection</exception>
		/// <param name="newNode">New attribute to insert</param>
		/// <param name="refNode">Reference node to insert new node after</param>
		/// <returns>Inserted node</returns>
		public virtual XmlAttribute InsertAfter( XmlAttribute newNode, XmlAttribute refNode)
		{
			return InsertHelper(newNode, refNode, 1);
		}

		/// <summary>
		/// Inserts newNode into the collection just before refNode.
		/// If a node with newNode.Name is already in the list, it is removed.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if inserted attribute created from different document, refNode not found in collection or
		/// refNode.Name == newNode.Name.</exception>
		/// <param name="newNode">Node to insert into list</param>
		/// <param name="refNode">Node to insert before</param>
		/// <returns>Deleted node, or null if no node was deleted.</returns>
		public virtual XmlAttribute InsertBefore(
			XmlAttribute newNode,
			XmlAttribute refNode
			)
		{
			return InsertHelper(newNode, refNode, 0);
		}

		/// <summary>
		/// Inserts the specified node as the first node in the collection
		/// </summary>
		/// <param name="node">XmlAttribute to insert</param>
		/// <exception cref="ArgumentException">If node is null, or owner document does not match collection.</exception>
		/// <returns>Node that was removed, or null if no node deleted.</returns>
		public virtual XmlAttribute Prepend(XmlAttribute node)
		{
			//TODO - node validation? (no)
			// TODO - set attribute owner element? (no)
			//node.setOwnerElement(FOwnerNode as XmlElement);

			if (FnodeList.Count > 0)
			{
				return InsertBefore(node, Item(0) as XmlAttribute);
			}

			if (node == null)
				throw new ArgumentException("Cannot prepend null node");

			if (! node.OwnerDocument.Equals(FOwner.OwnerDocument) )
				throw new ArgumentException("Node to prepend does not have same owner document as reference");

			FnodeList.Add(node);			
			return node;
		}

		/// <summary>
		/// Removes the requested node from the collection.
		/// </summary>
		/// <param name="node">Node to remove</param>
		/// <returns>The node removed, or null if the node was not found in the collection.</returns>
		public virtual XmlAttribute Remove(XmlAttribute node)
		{
			for (int i = 0; i < FnodeList.Count; i++)
			{
				XmlAttribute e = FnodeList[i] as XmlAttribute;
			
				if (e.Equals(node))
				{
					FnodeList.RemoveAt(i);
					return node;
				}
			}
			// TODO - if node is a default, should we add it back with a default value? (no)
			return null;
		}

		/// <summary>
		/// Removes all attributes from the collection
		/// </summary>
		public virtual void RemoveAll()
		{
			// Can this be this easy?
			for (int i = FnodeList.Count - 1; i > 0; i--)
			{
				XmlAttribute e = FnodeList[i] as XmlAttribute;
				e.setOwnerElement(null);
				FnodeList.RemoveAt(i);
			}

			// TODO - Add default attributes back in in RemoveAll()?  (no)
		}

		/// <summary>
		/// Removes the attribute at the specified index
		/// </summary>
		/// <param name="i">index of attribute to remove.</param>
		/// <returns>Removed node, or null if node not in collection</returns>
		public virtual XmlAttribute RemoveAt(int i)
		{
			if ((i < 0) | ( i >= FnodeList.Count))
				return null;

			// TODO - if default attribute removed in RemoveAt(), add it back? (no)
			XmlAttribute e = FnodeList[i] as XmlAttribute;
			FnodeList.RemoveAt(i);
			e.setOwnerElement(null);
			return e;
		}

		/// <summary>
		/// Adds a node to the collection using it's name.  If a node with the same name exists,
		/// it is removed.
		/// </summary>
		/// <param name="node">XmlAttribute to add.</param>
		/// <returns>If a node replaces a named node, the replaced node is deleted and returned. 
		/// Otherwise, null.</returns>
		public override XmlNode SetNamedItem(XmlNode node)
		{
			return base.SetNamedItem(node);
		}

		public override string ToString()
		{
			string retval = "System.Xml.XmlAttributeCollection ";
			if (FOwnerNode != null)
				retval += "OwnerElement: " + FOwnerNode.Name;

			foreach (XmlAttribute o in FnodeList)
			{
				retval += o.Name + "=" + o.Value;
			}
			return retval;

		}
		// ============= Constructors ========================================
		// TODO - change constructor to pass in IEnumerator?
		internal XmlAttributeCollection(XmlNode aOwner, XmlNode aOwnerNode, ArrayList nodeList) :
			base(aOwner, aOwnerNode, nodeList)
		{
			// Makes no sense to have namespace aware on attributes
			NamespaceAware = false;
		}

		internal XmlAttributeCollection ()
		{
		}

	}
}