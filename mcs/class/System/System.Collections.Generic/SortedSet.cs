//
// SortedSet.cs
//
// Authors:
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;

// SortedSet is basically implemented as a reduction of SortedDictionary<K, V>

#if NET_4_0

namespace System.Collections.Generic {

	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	public class SortedSet<T> : ISet<T>, ICollection, ISerializable, IDeserializationCallback
	{
		class Node : RBTree.Node {

			public T item;

			public Node (T item)
			{
				this.item = item;
			}

			public override void SwapValue (RBTree.Node other)
			{
				var o = (Node) other;
				var i = this.item;
				this.item = o.item;
				o.item = i;
			}
		}

		class NodeHelper : RBTree.INodeHelper<T> {

			static NodeHelper Default = new NodeHelper (Comparer<T>.Default);

			public IComparer<T> comparer;

			public int Compare (T item, RBTree.Node node)
			{
				return comparer.Compare (item, ((Node) node).item);
			}

			public RBTree.Node CreateNode (T item)
			{
				return new Node (item);
			}

			NodeHelper (IComparer<T> comparer)
			{
				this.comparer = comparer;
			}

			public static NodeHelper GetHelper (IComparer<T> comparer)
			{
				if (comparer == null || comparer == Comparer<T>.Default)
					return Default;

				return new NodeHelper (comparer);
			}
		}

		RBTree tree;
		NodeHelper helper;
		SerializationInfo si;

		public SortedSet ()
			: this (Comparer<T>.Default)
		{
		}

		public SortedSet (IEnumerable<T> collection)
			: this (collection, Comparer<T>.Default)
		{
		}

		public SortedSet (IEnumerable<T> collection, IComparer<T> comparer)
			: this (comparer)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			foreach (var item in collection)
				Add (item);
		}

		public SortedSet (IComparer<T> comparer)
		{
			this.helper = NodeHelper.GetHelper (comparer);
			this.tree = new RBTree (this.helper);
		}

		protected SortedSet (SerializationInfo info, StreamingContext context)
		{
			this.si = info;
		}

		public IComparer<T> Comparer {
			get { return helper.comparer; }
		}

		public int Count {
			get { return GetCount (); }
		}

		public T Max {
			get { return GetMax (); }
		}

		public T Min {
			get {  return GetMin (); }
		}

		internal virtual T GetMax ()
		{
			if (tree.Count == 0)
				return default (T);

			return GetItem (tree.Count - 1);
		}

		internal virtual T GetMin ()
		{
			if (tree.Count == 0)
				return default (T);

			return GetItem (0);
		}

		internal virtual int GetCount ()
		{
			return tree.Count;
		}

		T GetItem (int index)
		{
			return ((Node) tree [index]).item;
		}

		public bool Add (T item)
		{
			return TryAdd (item);
		}

		internal virtual bool TryAdd (T item)
		{
			var node = new Node (item);
			return tree.Intern (item, node) == node;
		}

		public virtual void Clear ()
		{
			tree.Clear ();
		}

		public virtual bool Contains (T item)
		{
			return tree.Lookup (item) != null;
		}

		public void CopyTo (T [] array)
		{
			CopyTo (array, 0, Count);
		}

		public void CopyTo (T [] array, int index)
		{
			CopyTo (array, index, Count);
		}

		public void CopyTo (T [] array, int index, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index > array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < count)
				throw new ArgumentException ("destination array cannot hold the requested elements");

			foreach (Node node in tree) {
				if (count-- == 0)
					break;

				array [index++] = node.item;
			}
		}

		public bool Remove (T item)
		{
			return TryRemove (item);
		}

		internal virtual bool TryRemove (T item)
		{
			return tree.Remove (item) != null;
		}

		public int RemoveWhere (Predicate<T> match)
		{
			var array = ToArray ();

			int count = 0;
			foreach (var item in array) {
				if (!match (item))
					continue;

				Remove (item);
				count++;
			}

			return count;
		}

		public IEnumerable<T> Reverse ()
		{
			for (int i = tree.Count - 1; i >= 0; i--)
				yield return GetItem (i);
		}

		T [] ToArray ()
		{
			var array = new T [this.Count];
			CopyTo (array);
			return array;
		}

		public Enumerator GetEnumerator ()
		{
			return TryGetEnumerator ();
		}

		internal virtual Enumerator TryGetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public static IEqualityComparer<SortedSet<T>> CreateSetComparer ()
		{
			return CreateSetComparer (EqualityComparer<T>.Default);
		}

		[MonoTODO]
		public static IEqualityComparer<SortedSet<T>> CreateSetComparer (IEqualityComparer<T> memberEqualityComparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}

		[MonoTODO]
		protected virtual void OnDeserialization (object sender)
		{
			if (si == null)
				return;

			throw new NotImplementedException ();
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			OnDeserialization (sender);
		}

		[MonoLimitation ("Isn't O(n) when other is SortedSet<T>")]
		public void ExceptWith (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");
			foreach (T item in other)
				Remove (item);
		}

		public virtual SortedSet<T> GetViewBetween (T lowerValue, T upperValue)
		{
			if (Comparer.Compare (lowerValue, upperValue) > 0)
				throw new ArgumentException ("The lowerValue is bigger than upperValue");

			return new SortedSubSet (this, lowerValue, upperValue);
		}

		[MonoLimitation ("Isn't O(n) when other is SortedSet<T>")]
		public virtual void IntersectWith (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			RBTree newtree = new RBTree (helper);
			foreach (T item in other) {
				var node = tree.Remove (item);
				if (node != null)
					newtree.Intern (item, node);
			}
			tree = newtree;
		}

		public bool IsProperSubsetOf (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0) {
				foreach (T item in other)
					return true; // this idiom means: if 'other' is non-empty, return true
				return false;
			}

			return is_subset_of (other, true);
		}

		public bool IsProperSupersetOf (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0)
				return false;

			return is_superset_of (other, true);
		}

		public bool IsSubsetOf (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0)
				return true;

			return is_subset_of (other, false);
		}

		public bool IsSupersetOf (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0) {
				foreach (T item in other)
					return false; // this idiom means: if 'other' is non-empty, return false
				return true;
			}

			return is_superset_of (other, false);
		}

		// Precondition: Count != 0, other != null
		bool is_subset_of (IEnumerable<T> other, bool proper)
		{
			SortedSet<T> that = nodups (other);

			if (Count > that.Count)
				return false;
			// Count != 0 && Count <= that.Count => that.Count != 0
			if (proper && Count == that.Count)
				return false;
			return that.covers (this);
		}

		// Precondition: Count != 0, other != null
		bool is_superset_of (IEnumerable<T> other, bool proper)
		{
			SortedSet<T> that = nodups (other);

			if (that.Count == 0)
				return true;
			if (Count < that.Count)
				return false;
			if (proper && Count == that.Count)
				return false;
			return this.covers (that);
		}

		public bool Overlaps (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0)
				return false;

			// Don't use 'nodups' here.  Only optimize the SortedSet<T> case
			SortedSet<T> that = other as SortedSet<T>;
			if (that != null && that.Comparer != Comparer)
				that = null;

			if (that != null)
				return that.Count != 0 && overlaps (that);

			foreach (T item in other)
				if (Contains (item))
					return true;
			return false;
		}

		public bool SetEquals (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			if (Count == 0) {
				foreach (T item in other)
					return false;
				return true;
			}

			SortedSet<T> that = nodups (other);

			if (Count != that.Count)
				return false;

			using (var t = that.GetEnumerator ()) {
				foreach (T item in this) {
					if (!t.MoveNext ())
						throw new SystemException ("count wrong somewhere: this longer than that");
					if (Comparer.Compare (item, t.Current) != 0)
						return false;
				}
				if (t.MoveNext ())
					throw new SystemException ("count wrong somewhere: this shorter than that");
				return true;
			}
		}

		SortedSet<T> nodups (IEnumerable<T> other)
		{
			SortedSet<T> that = other as SortedSet<T>;
			if (that != null && that.Comparer == Comparer)
				return that;
			return new SortedSet<T> (other, Comparer);
		}

		bool covers (SortedSet<T> that)
		{
			using (var t = that.GetEnumerator ()) {
				if (!t.MoveNext ())
					return true;
				foreach (T item in this) {
					int cmp = Comparer.Compare (item, t.Current);
					if (cmp > 0)
						return false;
					if (cmp == 0 && !t.MoveNext ())
						return true;
				}
				return false;
			}
		}

		bool overlaps (SortedSet<T> that)
		{
			using (var t = that.GetEnumerator ()) {
				if (!t.MoveNext ())
					return false;
				foreach (T item in this) {
					int cmp;
					while ((cmp = Comparer.Compare (item, t.Current)) > 0) {
						if (!t.MoveNext ())
							return false;
					}
					if (cmp == 0)
						return true;
				}
				return false;
			}
		}

		[MonoLimitation ("Isn't O(n) when other is SortedSet<T>")]
		public void SymmetricExceptWith (IEnumerable<T> other)
		{
			SortedSet<T> that_minus_this = new SortedSet<T> (Comparer);

			// compute this - that and that - this in parallel
			foreach (T item in nodups (other))
				if (!Remove (item))
					that_minus_this.Add (item);

			UnionWith (that_minus_this);
		}

		[MonoLimitation ("Isn't O(n) when other is SortedSet<T>")]
		public void UnionWith (IEnumerable<T> other)
		{
			CheckArgumentNotNull (other, "other");

			foreach (T item in other)
				Add (item);
		}

		static void CheckArgumentNotNull (object arg, string name)
		{
			if (arg == null)
				throw new ArgumentNullException (name);
		}

		void ICollection<T>.Add (T item)
		{
			Add (item);
		}

		void ICollection<T>.CopyTo (T [] array, int index)
		{
			CopyTo (array, index, Count);
		}

		bool ICollection<T>.IsReadOnly {
			get { return false; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			if (Count == 0)
				return;
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0 || array.Length <= index)
				throw new ArgumentOutOfRangeException ("index");
			if (array.Length - index < Count)
				throw new ArgumentException ();

			foreach (Node node in tree)
				array.SetValue (node.item, index++);
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		// TODO:Is this correct? If this is wrong,please fix.
		object ICollection.SyncRoot {
			get { return this; }
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable {

			RBTree.NodeEnumerator host;

			IComparer<T> comparer;

			T current;
			T upper;

			internal Enumerator (SortedSet<T> set)
				: this ()
			{
				host = set.tree.GetEnumerator ();
			}

			internal Enumerator (SortedSet<T> set, T lower, T upper)
				: this ()
			{
				host = set.tree.GetSuffixEnumerator (lower);
				comparer = set.Comparer;
				this.upper = upper;
			}

			public T Current {
				get { return current; }
			}

			object IEnumerator.Current {
				get {
					host.check_current ();
					return ((Node) host.Current).item;
				}
			}

			public bool MoveNext ()
			{
				if (!host.MoveNext ())
					return false;

				current = ((Node) host.Current).item;
				return comparer == null || comparer.Compare (upper, current) >= 0;
			}

			public void Dispose ()
			{
				host.Dispose ();
			}

			void IEnumerator.Reset ()
			{
				host.Reset ();
			}
		}

		[Serializable]
		sealed class SortedSubSet : SortedSet<T>, IEnumerable<T>, IEnumerable {

			SortedSet<T> set;
			T lower;
			T upper;

			public SortedSubSet (SortedSet<T> set, T lower, T upper)
				: base (set.Comparer)
			{
				this.set = set;
				this.lower = lower;
				this.upper = upper;

			}

			internal override T GetMin ()
			{
				RBTree.Node lb = null, ub = null;
				set.tree.Bound (lower, ref lb, ref ub);

				if (ub == null || set.helper.Compare (upper, ub) < 0)
					return default (T);

				return ((Node) ub).item;
			}

			internal override T GetMax ()
			{
				RBTree.Node lb = null, ub = null;
				set.tree.Bound (upper, ref lb, ref ub);

				if (lb == null || set.helper.Compare (lower, lb) > 0)
					return default (T);

				return ((Node) lb).item;
			}

			internal override int GetCount ()
			{
				int count = 0;
				using (var e = set.tree.GetSuffixEnumerator (lower)) {
					while (e.MoveNext () && set.helper.Compare (upper, e.Current) >= 0)
						++count;
				}
				return count;
			}

			internal override bool TryAdd (T item)
			{
				if (!InRange (item))
					throw new ArgumentOutOfRangeException ("item");

				return set.TryAdd (item);
			}

			internal override bool TryRemove (T item)
			{
				if (!InRange (item))
					return false;

				return set.TryRemove (item);
			}

			public override bool Contains (T item)
			{
				if (!InRange (item))
					return false;

				return set.Contains (item);
			}

			public override void Clear ()
			{
				set.RemoveWhere (InRange);
			}

			bool InRange (T item)
			{
				return Comparer.Compare (item, lower) >= 0
					&& Comparer.Compare (item, upper) <= 0;
			}

			public override SortedSet<T> GetViewBetween (T lowerValue, T upperValue)
			{
				if (Comparer.Compare (lowerValue, upperValue) > 0)
					throw new ArgumentException ("The lowerValue is bigger than upperValue");
				if (!InRange (lowerValue))
					throw new ArgumentOutOfRangeException ("lowerValue");
				if (!InRange (upperValue))
					throw new ArgumentOutOfRangeException ("upperValue");

				return new SortedSubSet (set, lowerValue, upperValue);
			}

			internal override Enumerator TryGetEnumerator ()
			{
				return new Enumerator (set, lower, upper);
			}

			public override void IntersectWith (IEnumerable<T> other)
			{
				CheckArgumentNotNull (other, "other");

				var slice = new SortedSet<T> (this);
				slice.IntersectWith (other);

				Clear ();
				set.UnionWith (slice);
			}
		}
	}
}

#endif
