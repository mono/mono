//
// System.Collections.Generic.SortedDictionary
//
// Author:
//    Raja R Harinath <rharinath@novell.com>
//
// Authors of previous (superseded) version:
//    Kazuki Oikawa (kazuki@panicode.com)
//    Atsushi Enomoto (atsushi@ximian.com)
//

//
// Copyright (C) 2007, Novell, Inc.
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
using System.Diagnostics;

namespace System.Collections.Generic
{
	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
	public class SortedDictionary<TKey,TValue> : IDictionary<TKey,TValue>, ICollection<KeyValuePair<TKey,TValue>>, IEnumerable<KeyValuePair<TKey,TValue>>, IDictionary, ICollection, IEnumerable
	{
		class Node : RBTree.Node {
			public TKey key;
			public TValue value;

			public Node (TKey key)
			{
				this.key = key;
			}

			public Node (TKey key, TValue value)
			{
				this.key = key;
				this.value = value;
			}

			public override void SwapValue (RBTree.Node other)
			{
				Node o = (Node) other;
				TKey k = key; key = o.key; o.key = k;
				TValue v = value; value = o.value; o.value = v;
			}

			public KeyValuePair<TKey, TValue> AsKV ()
			{
				return new KeyValuePair<TKey, TValue> (key, value);
			}

			public DictionaryEntry AsDE ()
			{
				return new DictionaryEntry (key, value);
			}
		}

		class NodeHelper : RBTree.INodeHelper<TKey> {
			public IComparer<TKey> cmp;

			public int Compare (TKey key, RBTree.Node node)
			{
				return cmp.Compare (key, ((Node) node).key);
			}

			public RBTree.Node CreateNode (TKey key)
			{
				return new Node (key);
			}

			private NodeHelper (IComparer<TKey> cmp)
			{
				this.cmp = cmp;
			}
			static NodeHelper Default = new NodeHelper (Comparer<TKey>.Default);
			public static NodeHelper GetHelper (IComparer<TKey> cmp)
			{
				if (cmp == null || cmp == Comparer<TKey>.Default)
					return Default;
				return new NodeHelper (cmp);
			}
		}

		RBTree tree;
		NodeHelper hlp;

		#region Constructor
		public SortedDictionary () : this ((IComparer<TKey>) null)
		{
		}

		public SortedDictionary (IComparer<TKey> comparer)
		{
			hlp = NodeHelper.GetHelper (comparer);
			tree = new RBTree (hlp);
		}

		public SortedDictionary (IDictionary<TKey,TValue> dic) : this (dic, null)
		{
		}

		public SortedDictionary (IDictionary<TKey,TValue> dic, IComparer<TKey> comparer) : this (comparer)
		{
			if (dic == null)
				throw new ArgumentNullException ();
			foreach (KeyValuePair<TKey, TValue> entry in dic)
				Add (entry.Key, entry.Value);
		}
		#endregion

		#region PublicProperty

		public IComparer<TKey> Comparer {
			get { return hlp.cmp; }
		}

		public int Count {
			get { return (int) tree.Count; }
		}

		public TValue this [TKey key] {
			get {
				Node n = (Node) tree.Lookup (key);
				if (n == null)
					throw new KeyNotFoundException ();
				return n.value;
			}
			set {
				if (key == null)
					throw new ArgumentNullException ("key");
				Node n = (Node) tree.Intern (key, null);
				n.value = value;
			}
		}

		public KeyCollection Keys {
			get { return new KeyCollection (this); }
		}

		public ValueCollection Values {
			get { return new ValueCollection (this); }
		}
		#endregion

		#region PublicMethod

		public void Add (TKey key, TValue value)
		{
			if (key == null) 
				throw new ArgumentNullException ("key");

			RBTree.Node n = new Node (key, value);
			if (tree.Intern (key, n) != n)
				throw new ArgumentException ("key already present in dictionary", "key");
		}

		public void Clear ()
		{
			tree.Clear ();
		}

		public bool ContainsKey (TKey key)
		{
			return tree.Lookup (key) != null;
		}

		public bool ContainsValue (TValue value)
		{
			IEqualityComparer<TValue> vcmp = EqualityComparer<TValue>.Default;
			foreach (Node n in tree)
				if (vcmp.Equals (value, n.value))
					return true;
			return false;
		}

		public void CopyTo (KeyValuePair<TKey,TValue>[] array, int arrayIndex)
		{
			if (Count == 0)
				return;
			if (array == null)
				throw new ArgumentNullException ();
			if (arrayIndex < 0 || array.Length <= arrayIndex)
				throw new ArgumentOutOfRangeException ();
			if (array.Length - arrayIndex < Count)
				throw new ArgumentException ();

			foreach (Node n in tree)
				array [arrayIndex ++] = n.AsKV ();
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		public bool Remove (TKey key)
		{
			return tree.Remove (key) != null;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			Node n = (Node) tree.Lookup (key);
			value = n == null ? default (TValue) : n.value;
			return n != null;
		}

		#endregion

		#region PrivateMethod
		TKey ToKey (object key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (!(key is TKey))
				throw new ArgumentException (String.Format ("Key \"{0}\" cannot be converted to the key type {1}.", key, typeof (TKey)));
			return (TKey) key;
		}

		TValue ToValue (object value)
		{
			if (!(value is TValue) && (value != null || typeof (TValue).IsValueType))
				throw new ArgumentException (String.Format ("Value \"{0}\" cannot be converted to the value type {1}.", value, typeof (TValue)));
			return (TValue) value;
		}
		#endregion

		#region IDictionary<TKey,TValue> Member

		ICollection<TKey> IDictionary<TKey,TValue>.Keys {
			get { return new KeyCollection (this); }
		}

		ICollection<TValue> IDictionary<TKey,TValue>.Values {
			get { return new ValueCollection (this); }
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Member

		void ICollection<KeyValuePair<TKey,TValue>>.Add (KeyValuePair<TKey,TValue> item)
		{
			Add (item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.Contains (KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			return TryGetValue (item.Key, out value) &&
				EqualityComparer<TValue>.Default.Equals (item.Value, value);
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.IsReadOnly {
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.Remove (KeyValuePair<TKey,TValue> item)
		{
			TValue value;
			return TryGetValue (item.Key, out value) &&
 				EqualityComparer<TValue>.Default.Equals (item.Value, value) &&
				Remove (item.Key);
		}

		#endregion

		#region IDictionary Member

		void IDictionary.Add (object key, object value)
		{
			Add (ToKey (key), ToValue (value));
		}

		bool IDictionary.Contains (object key)
		{
			return ContainsKey (ToKey (key));
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		bool IDictionary.IsFixedSize {
			get { return false; }
		}

		bool IDictionary.IsReadOnly {
			get { return false; }
		}

		ICollection IDictionary.Keys  {
			get { return new KeyCollection (this); }
		}

		void IDictionary.Remove (object key)
		{
			Remove (ToKey (key));
		}

		ICollection IDictionary.Values {
			get { return new ValueCollection (this); }
		}

		object IDictionary.this [object key] {
			get { return this [ToKey (key)]; }
			set { this [ToKey (key)] = ToValue (value); }
		}

		#endregion

		#region ICollection Member

		void ICollection.CopyTo (Array array, int index)
		{
			if (Count == 0)
				return;
			if (array == null)
				throw new ArgumentNullException ();
			if (index < 0 || array.Length <= index)
				throw new ArgumentOutOfRangeException ();
			if (array.Length - index < Count)
				throw new ArgumentException ();

			foreach (Node n in tree)
				array.SetValue (n.AsDE (), index++);
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		// TODO:Is this correct? If this is wrong,please fix.
		object ICollection.SyncRoot {
			get { return this; }
		}

		#endregion

		#region IEnumerable Member

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		#endregion

		#region IEnumerable<TKey> Member

		IEnumerator<KeyValuePair<TKey,TValue>> IEnumerable<KeyValuePair<TKey,TValue>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		#endregion

		[Serializable]
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
		public sealed class ValueCollection : ICollection<TValue>,
			IEnumerable<TValue>, ICollection, IEnumerable
		{
			SortedDictionary<TKey,TValue> _dic;

			public ValueCollection (SortedDictionary<TKey,TValue> dic)
			{
				_dic = dic;
			}

			void ICollection<TValue>.Add (TValue item)
			{
				throw new NotSupportedException ();
			}

			void ICollection<TValue>.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool ICollection<TValue>.Contains (TValue item)
			{
				return _dic.ContainsValue (item);
			}

			public void CopyTo (TValue [] array, int arrayIndex)
			{
				if (Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ();
				if (arrayIndex < 0 || array.Length <= arrayIndex)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - arrayIndex < Count)
					throw new ArgumentException ();
				foreach (Node n in _dic.tree)
					array [arrayIndex++] = n.value;
			}

			public int Count {
				get { return _dic.Count; }
			}

			bool ICollection<TValue>.IsReadOnly {
				get { return true; }
			}

			bool ICollection<TValue>.Remove (TValue item)
			{
				throw new NotSupportedException ();
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dic);
			}
		
			void ICollection.CopyTo (Array array, int index)
			{
				if (Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || array.Length <= index)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - index < Count)
					throw new ArgumentException ();
				foreach (Node n in _dic.tree)
					array.SetValue (n.value, index++);
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return _dic; }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
 				return new Enumerator (_dic);
			}

			public struct Enumerator : IEnumerator<TValue>,IEnumerator, IDisposable
			{
				RBTree.NodeEnumerator host;

				TValue current;

				internal Enumerator (SortedDictionary<TKey,TValue> dic)
					: this ()
				{
					host = dic.tree.GetEnumerator ();
				}

				public TValue Current {
					get { return current; }
				}

				public bool MoveNext ()
				{
					if (!host.MoveNext ())
						return false;
					current = ((Node) host.Current).value;
					return true;
				}

				public void Dispose ()
				{
					host.Dispose ();
				}

				object IEnumerator.Current {
					get {
						host.check_current ();
						return current;
					}
				}

				void IEnumerator.Reset ()
				{
					host.Reset ();
				}
			}
		}

		[Serializable]
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
		public sealed class KeyCollection : ICollection<TKey>,
			IEnumerable<TKey>, ICollection, IEnumerable
		{
			SortedDictionary<TKey,TValue> _dic;

			public KeyCollection (SortedDictionary<TKey,TValue> dic)
			{
				_dic = dic;
			}

			void ICollection<TKey>.Add (TKey item)
			{
				throw new NotSupportedException ();
			}

			void ICollection<TKey>.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool ICollection<TKey>.Contains (TKey item)
			{
				return _dic.ContainsKey (item);
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public void CopyTo (TKey [] array, int arrayIndex)
			{
				if (Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ();
				if (arrayIndex < 0 || array.Length <= arrayIndex)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - arrayIndex < Count)
					throw new ArgumentException ();
				foreach (Node n in _dic.tree)
					array [arrayIndex++] = n.key;
			}

			public int Count {
				get { return _dic.Count; }
			}

			bool ICollection<TKey>.IsReadOnly {
				get { return true; }
			}

			bool ICollection<TKey>.Remove (TKey item)
			{
				throw new NotSupportedException ();
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dic);
			}

			void ICollection.CopyTo (Array array, int index)
			{
				if (Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || array.Length <= index)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - index < Count)
					throw new ArgumentException ();
				foreach (Node n in _dic.tree)
					array.SetValue (n.key, index++);
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return _dic; }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
 				return new Enumerator (_dic);
			}

			public struct Enumerator : IEnumerator<TKey>, IEnumerator, IDisposable
			{
				RBTree.NodeEnumerator host;

				TKey current;

				internal Enumerator (SortedDictionary<TKey,TValue> dic)
					: this ()
				{
					host = dic.tree.GetEnumerator ();
				}

				public TKey Current {
					get { return current; }
				}

				public bool MoveNext ()
				{
					if (!host.MoveNext ())
						return false;
					current = ((Node) host.Current).key;
					return true;
				}

				public void Dispose ()
				{
					host.Dispose ();
				}

				object IEnumerator.Current {
					get {
						host.check_current ();
						return current;
					}
				}

				void IEnumerator.Reset ()
				{
					host.Reset ();
				}
			}
		}

		public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			RBTree.NodeEnumerator host;

			KeyValuePair<TKey, TValue> current;

			internal Enumerator (SortedDictionary<TKey,TValue> dic)
				: this ()
			{
				host = dic.tree.GetEnumerator ();
			}

			public KeyValuePair<TKey,TValue> Current {
				get { return current; }
			}

			public bool MoveNext ()
			{
				if (!host.MoveNext ())
					return false;
				current = ((Node) host.Current).AsKV ();
				return true;
			}

			public void Dispose ()
			{
				host.Dispose ();
			}

			Node CurrentNode {
				get {
					host.check_current ();
					return (Node) host.Current;
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return CurrentNode.AsDE (); }
			}

			object IDictionaryEnumerator.Key {
				get { return CurrentNode.key; }
			}

			object IDictionaryEnumerator.Value {
				get { return CurrentNode.value; }
			}

			object IEnumerator.Current {
				get { return CurrentNode.AsDE (); }
			}

			void IEnumerator.Reset ()
			{
				host.Reset ();
			}
		}
	}
}
