//
// MemberInfoList.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
using System.Reflection;
using System.Collections;

namespace Microsoft.JScript {

	internal class Node {
		private MemberInfo elem;
		private Node next;
		private string name;

		internal MemberInfo Element {
			get { return elem; }
		}

		internal Node Next {
			get { return next; }
			set { next = value; }
		}

		internal Node (string name, MemberInfo value, Node node)
		{
			this.name = name;
			elem = value;
			next = node;
		}
	}

	public sealed class MemberInfoList {
		Node head;
		int size;

		internal int Size {
			get { return size; }
		}

		internal Node Head {
			get { return head; }
		}

		internal MemberInfoList ()
		{
			head = new Node (String.Empty, null, null);
			size = 0;
		}

		internal void Insert (string name, MemberInfo elem)
		{
			Node second = head.Next;
			head.Next = new Node (name, elem, second);
			size++;
		}

		internal bool Delete (object elem)
		{
			Node marker;
			for (marker = head; marker.Next != null && !marker.Next.Element.Equals (elem); marker = marker.Next)
				;

			if (marker.Next != null && marker.Next.Element.Equals (elem)) {
				marker.Next = marker.Next.Next;
				size--;
				return true;
			} else return false;
		}

		internal Node Find (object value)
		{
			Node result = null;
			Node current = head.Next;

			for (int i = 0; i < size; i++) {
				if (current != null) 
					if (current.Element.Equals (value))
						result = current;
					else
						current = current.Next;
			}
			return result;
		}
	}

	internal class ListIter {
		Node current;

		internal Node Current {
			get { return current; }
		}

		internal ListIter (MemberInfoList list)
		{
			current = list.Head;
		}

		internal Node Next ()
		{
			if (!End)
				current = current.Next;
			if (!End)
				return current;
			else
				throw new Exception ("Attemp to access beyond end of list.");
		}

		internal bool End {
			get { return current == null; }
		}
	}

	internal class ChainHash : IEnumerable {

		MemberInfoList [] bucket;

		internal ChainHash (int size)
		{
			bucket = new MemberInfoList [size];

			for (int i = 0; i < size; i++)
				bucket [i] = new MemberInfoList ();		
		}

		internal void Insert (string name, MemberInfo value)
		{
			bucket [Hash (name)].Insert (name, value);
		}

		internal object Retrieve (object value)
		{
			int n = bucket.Length;
			object result = null;

			for (int i = 0; i < n; i++) {
				result = bucket [i].Find (value);
				if (result == null)
					continue;
				else
					break;
			}
			return result;
		}

		internal MemberInfoList Retrieve (string name)
		{
			return bucket [Hash (name)];
		}
		
		internal void Delete (object value)
		{
			if (value == null)
				throw new Exception ("Delete.Error, key can't be null.");

			int n = bucket.Length;
			bool deleted = false;

			for (int i = 0; i < n; i++) {
				deleted = bucket [i].Delete (value);
				if (deleted)
					break;
			}
		}

		private int Hash (string name)
		{
			return name.GetHashCode () % bucket.Length;
		}

		public IEnumerator GetEnumerator ()
		{
			ArrayList elems = new ArrayList ();
			ListIter iter;
			int i, n;

			foreach (MemberInfoList list in bucket) {
				n = list.Size;
				iter = new ListIter (list);
				for (i = 0; i < n; i++)
					elems.Add (iter.Next ());
			}
			return elems.GetEnumerator ();
		}
	}
}
