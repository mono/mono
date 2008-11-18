//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	cache.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

namespace System.Text.RegularExpressions {

	class FactoryCache {
		public FactoryCache (int capacity) {
			this.capacity = capacity;
			this.factories = new Hashtable (capacity);
			this.mru_list = new MRUList ();
		}

		public void Add (string pattern, RegexOptions options, IMachineFactory factory) {
			lock (this) {
				Key k = new Key (pattern, options);
				Cleanup ();
				factories[k] = factory;
				mru_list.Use (k);
			}
		}

		// lock must be held by the caller
		void Cleanup ()
		{
			while (factories.Count >= capacity && capacity > 0) {
				object victim = mru_list.Evict ();
				if (victim != null)
					factories.Remove ((Key) victim);
			}
		}

		public IMachineFactory Lookup (string pattern, RegexOptions options) {
			lock (this) {
				Key k = new Key (pattern, options);
				if (factories.Contains (k)) {
					mru_list.Use (k);
					return (IMachineFactory)factories[k];
				}
			}

			return null;
		}

		public int Capacity {
			get { return capacity; }
			set {
				// < 0 check done in the caller (Regex.CacheSize)
				lock (this) {
					capacity = value;
					Cleanup ();
				}
			}
		}

		private int capacity;
		private Hashtable factories;
		private MRUList mru_list;

		class Key {
			public string pattern;
			public RegexOptions options;

			public Key (string pattern, RegexOptions options) {
				this.pattern = pattern;
				this.options = options;
			}
			
			public override int GetHashCode () {
				return pattern.GetHashCode () ^ (int)options;
			}

			public override bool Equals (object o) {
				if (o == null || !(o is Key))
					return false;

				Key k = (Key)o;
				return options == k.options && pattern.Equals (k.pattern);
			}

			public override string ToString () {
				return "('" + pattern + "', [" + options + "])";
			}
		}
	}

	class MRUList {
		public MRUList () {
			head = tail = null;
		}

		public void Use (object o) {
			Node node;

			if (head == null) {
				node = new Node (o);
				head = tail = node;
				return;
			}

			node = head;
			while (node != null && !o.Equals (node.value))
				node = node.previous;

			if (node == null)
				node = new Node (o);
			else {
				if (node == head)
					return;

				if (node == tail)
					tail = node.next;
				else
					node.previous.next = node.next;

				node.next.previous = node.previous;
			}

			head.next = node;
			node.previous = head;
			node.next = null;
			head = node;
		}

		public object Evict () {
			if (tail == null)
				return null;

			object o = tail.value;
			tail = tail.next;

			if (tail == null)
				head = null;
			else
				tail.previous = null;

			return o;
		}

		private Node head, tail;

		private class Node {
			public object value;
			public Node previous, next;

			public Node (object value) {
				this.value = value;
			}
		}
	}
}
