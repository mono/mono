//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	cache.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

				while (factories.Count >= capacity) {
					object victim = mru_list.Evict ();
					if (victim != null)
						factories.Remove ((Key)victim);
				}
				
				factories[k] = factory;
				mru_list.Use (k);
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

		private int capacity;
		private Hashtable factories;
		private MRUList mru_list;

		struct Key {
			public string pattern;
			public RegexOptions options;

			public Key (string pattern, RegexOptions options) {
				this.pattern = pattern;
				this.options = options;
			}
			
			public new int GetHashCode () {
				return pattern.GetHashCode () ^ (int)options;
			}

			public new bool Equals (object o) {
				if (o == null || o.GetType () != this.GetType ())
					return false;

				Key k = (Key)o;
				return options == k.options && pattern.Equals (k.pattern);
			}

			public new string ToString () {
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
