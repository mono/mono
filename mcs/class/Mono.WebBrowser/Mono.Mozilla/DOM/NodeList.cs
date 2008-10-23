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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class NodeList : DOMObject, INodeList
	{
		protected nsIDOMNodeList unmanagedNodes;
		protected INode [] nodes;
		protected int nodeCount;
		
		public NodeList(WebBrowser control, nsIDOMNodeList nodeList) : base (control)
		{
			if (control.platform != control.enginePlatform)
				unmanagedNodes = nsDOMNodeList.GetProxy (control, nodeList);
			else
				unmanagedNodes = nodeList;
		}

		public NodeList (WebBrowser control) : base (control)
		{
			nodes = new Node[0];
		}
		
		public NodeList (WebBrowser control, bool loaded) : base (control)
		{
		}
		
		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Clear ();
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region Helpers
		protected void Clear () 
		{
			if (nodes != null) {
				for (int i = 0; i < nodeCount; i++) {
					nodes[i] = null;
				}
				nodeCount = 0;
				unmanagedNodes = null;
				nodes = null;
			}
		}
		
		internal virtual void Load ()
		{
			if (unmanagedNodes == null) return;
			Clear ();
			uint count;
			unmanagedNodes.getLength (out count);
			nodeCount = (int) count; // hmm.... not good
			nodes = new Node[nodeCount];
			for (int i = 0; i < nodeCount; i++) {
				nsIDOMNode node;
				unmanagedNodes.item ((uint)i, out node);
				ushort type;
				node.getNodeType (out type);
				nodes[i] = GetTypedNode (node);
//				switch (type) {
//					case (ushort)NodeType.Element:
//						nodes[i] = new HTMLElement (control, node as nsIDOMHTMLElement);
//						break;
//					default:
//						nodes[i] = new Node (control, node);
//						break;
//				}				
			}
		}
		#endregion
		
		#region IEnumerable members
		public IEnumerator GetEnumerator () 
		{
			return new NodeListEnumerator (this);
		}
		#endregion
		
		#region ICollection members
		public void CopyTo (Array dest, int index) 
		{
			if (nodes != null) {
				Array.Copy (nodes, 0, dest, index, Count);
			}
		}
	
		public virtual int Count {
			get {
				if (unmanagedNodes != null && nodes == null)
					Load ();
				return nodeCount; 
			}
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}

		#endregion
		
		#region IList members
		public bool IsReadOnly 
		{
			get { return false;}
		}

		bool IList.IsFixedSize 
		{
			get { return false;}
		}

		void IList.RemoveAt  (int index) 
		{
			RemoveAt (index);			
		}
		
		public void RemoveAt (int index)
		{
			if (index > Count || index < 0)
				return;			
			Array.Copy (nodes, index + 1, nodes, index, (nodeCount - index) - 1);
			nodeCount--;
			nodes[nodeCount] = null;
		}
		
		public void Remove (INode node) 
		{
			this.RemoveAt (IndexOf (node));
		}

		void IList.Remove (object node) 
		{
			Remove (node as INode);
		}
		
		public void Insert (int index, INode value) 
		{
			if (index > Count)
				index = nodeCount;
			INode[] tmp = new Node[nodeCount+1];
			if (index > 0)
				Array.Copy (nodes, 0, tmp, 0, index);
			tmp[index] = value;
			if (index < nodeCount)
				Array.Copy (nodes, index, tmp, index + 1, (nodeCount - index));
			nodes = tmp;
			nodeCount++;
		}

		void IList.Insert (int index, object value) 
		{
			this.Insert (index, value as INode);
		}
		
		public int IndexOf (INode node) 
		{
			return Array.IndexOf (nodes, node);
		}

		int IList.IndexOf (object node) 
		{
			return IndexOf (node as INode);
		}
		
		
		public bool Contains (INode node)
		{
			return this.IndexOf (node) != -1;
		}
		
		bool IList.Contains (object node)
		{
			return Contains (node as INode);			
		}
		
		void IList.Clear () 
		{
			this.Clear ();
		}
		
		public int Add (INode node) 
		{
			this.Insert (Count + 1, node as INode);
			return nodeCount - 1;
		}
		
		int IList.Add (object node) 
		{
			return Add (node as INode);
		}
		
		object IList.this [int index] {
			get { 
				return this [index]; 
			}
			set { 
				this [index] = value as INode; 
			}
		}
		
		public INode this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				return nodes [index];								
			}
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				nodes [index] = value as INode;
			}
		}
		
		#endregion
		
		public override int GetHashCode () {
			if (this.unmanagedNodes != null)
				return this.unmanagedNodes.GetHashCode ();
			return base.GetHashCode ();
		}		

		internal class NodeListEnumerator : IEnumerator {

			private NodeList collection;
			private int index = -1;

			public NodeListEnumerator (NodeList collection)
			{
				this.collection = collection;
			}

			public object Current {
				get {
					if (index == -1)
						return null;
					return collection [index];
				}
			}

			public bool MoveNext ()
			{
				if (index + 1 >= collection.Count)
					return false;
				index++;
				return true;
			}

			public void Reset ()
			{
				index = -1;
			}
		}
		
	}	
}
