//
// System.Xml.XmlNamedNodeMap.cs
//
// Author: Daniel Weber (daniel-weber@austin.rr.com)
//
// Implementation of abstract System.Xml.XmlNamedNodeMap class
//
// Credit for source code to Open XML 2.3.17

using System;
using System.Collections;

namespace System.Xml
{

	public class XmlNamedNodeMap : IEnumerable
	{
		//=============== Class data structures ====================================
		// Use weak references for owners to prevent circular linkage
		protected XmlNode FOwner;
		protected XmlNode FOwnerNode;

		protected ArrayList FnodeList;
		private bool FNamespaceAware;
		private bool FIsReadonly;

		// ============ Public Properties =====================================
		//=====================================================================
		/// <summary>
		/// Get the count of nodes in the map
		/// </summary>
		public virtual int Count 
		{
			get
			{
				return FnodeList.Count;
			}
		}
		
		// ============ Public Methods    =====================================
		/// <summary>
		/// Get the node at the given index.  Index is 0-based, top element is Count - 1
		/// </summary>
		/// <param name="index">index into array of XmlNode to get</param>
		/// <returns>XmlNode at index, or null if index out of range</returns>
		public virtual XmlNode Item(int index)
		{
			try
			{
				return FnodeList[index] as XmlNode;   //FnodeList.Item(index);
			}
			catch (ArgumentOutOfRangeException)
			{
				return null;
			}
		}

		/// <summary>
		/// Get the node with the given name
		/// If the nodes have an associated namespace, use (localName, namespaceURI) instead
		/// </summary>
		/// <param name="name">name of the node to return</param>
		/// <returns>XmlNode, or null if node not found</returns>
		public virtual XmlNode GetNamedItem(string name)
		{
			// Can't return without a namespace to lookup
			// SDK doesn't specify an exception to throw, so just return null
			if (FNamespaceAware) 
				return null;

			foreach (XmlNode cur in FnodeList)
			{
				if (cur.Name == name)
					return cur;
			}

			return null;

		}
		
		/// <summary>
		/// Get the node with localName in given namespaceURI
		/// </summary>
		/// <param name="localName">localName of node</param>
		/// <param name="namespaceURI">namespace of node</param>
		/// <returns>XmlNode at location, or null</returns>
		public virtual XmlNode GetNamedItem(string localName, string namespaceURI)
		{
			// No namespace data in objects, can't lookup
			// SDK doesn't specify an exception to throw, so just return null
			if (! FNamespaceAware)
				return null;

			foreach (XmlNode cur in FnodeList)
			{
				if ((cur.Name == localName) & (cur.NamespaceURI == namespaceURI))
					return cur;
			}
			return null;

		}

		/// <summary>
		/// Get the enumerator for the node map
		/// </summary>
		/// <returns>Enumerator</returns>
		public virtual IEnumerator GetEnumerator()
		{
			return FnodeList.GetEnumerator();
		}

		
		/// <summary>
		/// Removes the node with given name from the Map and returns it.
		/// If the node is namespace aware, use RemoveNamedItem(localName, namespaceURI) instead
		/// </summary>
		/// <param name="name">node name</param>
		/// <returns>Removed node, or null if node not found</returns>
		public virtual XmlNode RemoveNamedItem(string name)
		{
			// Can't return without a namespace to lookup
			// SDK doesn't specify an exception to throw, so just return null
			if (FNamespaceAware) 
				return null;

			for (int i = 0; i < FnodeList.Count; i++)
			{
				XmlNode cur = FnodeList[i] as XmlNode;
				if (cur.Name == name)
				{
					FnodeList.RemoveAt(i);
					return cur;
				}
			}
			return null;
		}

		/// <summary>
		/// Removes the node with given localName in namespaceURI
		/// If this XmlNamedNodeMap is not namespace aware, use RemoveNamedItem(name) instead.
		/// </summary>
		/// <param name="localName">local node name</param>
		/// <param name="namespaceURI">namespace node is in</param>
		/// <returns>Node that was removed, or null if no node found</returns>
		public virtual XmlNode RemoveNamedItem(string localName, string namespaceURI)
		{
			// No namespace data in objects, can't lookup
			// SDK doesn't specify an exception to throw, so just return null
			if (! FNamespaceAware)  // NOT _namespaceAware
				return null;

			for (int i = 0; i < FnodeList.Count; i++)
			{
				XmlNode cur = FnodeList[i] as XmlNode;
				if ((cur.Name == localName) & (cur.NamespaceURI == namespaceURI))
				{
					FnodeList.RemoveAt(i);
					return cur;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds the passed node using the name property
		/// If a node with this name exists, it is returned, otherwise null is returned.
		/// </summary>
		/// <exception cref="ArgumentException">Raised if node was created from another docuement, or XmlNamedNodeMap is read-only</exception>
		/// <exception cref="InvalidOperationException">Node is an XmlAttribute of another XmlElement</exception>
		/// <param name="node"></param>
		/// <returns></returns>
		public virtual XmlNode SetNamedItem(XmlNode node)
		{
			XmlNode retValue ;		// Return value of method

			// Can't add to read-only Map
			if (FIsReadonly)
				throw new ArgumentException("Attempt to add node to read-only Node Map");
			
			//if FOwner.OwnerDocument <> arg.OwnerDocument then raise EWrong_Document_Err
			if (! FOwner.OwnerDocument.Equals(node.OwnerDocument))
				throw new ArgumentException("Cannot add node from another document");
		
			// if FNamespaceAware then raise ENamespace_Err.create('Namespace error.');
			if (FNamespaceAware)
				throw new InvalidOperationException("Invalid Operation: Can't add node by name to namespace aware node list");

			// Can't assign node that has a parent
			// TODO - is this check required/valid in the .NET API?
			//if assigned(arg.parentNode) then raise EInuse_Node_Err.create('In use node error.');
			if (node.ParentNode != null)
				throw new ArgumentException("In use node error");

			// XmlAttribute cannot be assigned to an element
			//if arg.NodeType = ntAttribute_Node
			//	then if assigned((arg as TdomAttr).OwnerElement)
			//			 then if (arg as TdomAttr).OwnerElement <> FOwnerNode then raise EInuse_Attribute_Err.create('Inuse attribute error.');

			if (node is XmlAttribute)
			{
				if ((node as XmlAttribute).OwnerElement != null)
				{
					if (! FOwnerNode.Equals( (node as XmlAttribute).OwnerElement ))
						throw new InvalidOperationException("XmlAttribute is assigned to another element");
				}
			}

			/* 
			if not (arg.NodeType in FAllowedNodeTypes) then raise EHierarchy_Request_Err.create('Hierarchy request error.');
			*/

			if ( GetNamedItem(node.Name) != null)
			{ 
				retValue = RemoveNamedItem(node.Name); 
			}
			else
			{ 
				retValue = null;
			}

			FnodeList.Add(node);

			// TODO - check that owner is set properly on adding an attribute node

			return retValue;
		}

		// ============ Constructors  =========================================
		internal XmlNamedNodeMap(XmlNode aOwner, XmlNode aOwnerNode, ArrayList nodeList)
		{
			if (nodeList == null)
				nodeList = new ArrayList();
			else
				FnodeList = nodeList;
			FOwner = aOwner;
			FOwnerNode = aOwnerNode;
			FIsReadonly = false;
			FNamespaceAware = false;
		}

		// Add a default costructor to satisfy Visual Studio....
		// TODO - figure out a way to get rid of default constructor on XmlNamedNodeMap()
		internal XmlNamedNodeMap()
		{
			FnodeList = new ArrayList();
			FOwner = null;
			FOwnerNode = null;
			FIsReadonly = false;
			FNamespaceAware = false;
		}

		// ============ Internal Properties ===================================
		//=====================================================================
		internal bool IsReadOnly
		{
			get 
			{ 
				return FIsReadonly;
			}
			set 
			{ 
				FIsReadonly = value;
			}
		}

		internal bool NamespaceAware
		{
			get 
			{ 
				return FNamespaceAware; 
			}
			set 
			{ 
				FNamespaceAware = value;
			}
		}

	}
}