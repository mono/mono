/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using MSG = System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A sorted generic dictionary based on a red-black tree set.
	/// </summary>
	public class TreeDictionary<K,V>: DictionaryBase<K,V>, IDictionary<K,V>, ISortedDictionary<K,V>
	{
		#region Fields

		private TreeSet<KeyValuePair<K,V>> sortedpairs;

		#endregion

		#region Constructors

		/// <summary>
		/// Create a red-black tree dictionary using the natural comparer for keys.
		/// <exception cref="ArgumentException"/> if the key type K is not comparable.
		/// </summary>
		public TreeDictionary() : this(ComparerBuilder.FromComparable<K>.Examine()) { }

		/// <summary>
		/// Create a red-black tree dictionary using an external comparer for keys.
		/// </summary>
		/// <param name="c">The external comparer</param>
		public TreeDictionary(IComparer<K> c)
		{
			pairs = sortedpairs = new TreeSet<KeyValuePair<K,V>>(new KeyValuePairComparer<K,V>(c));
		}

		#endregion

		#region ISortedDictionary<K,V> Members

		/// <summary>
		/// Get the entry in the dictionary whose key is the
		/// predecessor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		[Tested]
		public KeyValuePair<K,V> Predecessor(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return sortedpairs.Predecessor(p);
		}


		/// <summary>
		/// Get the entry in the dictionary whose key is the
		/// weak predecessor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		[Tested]
		public KeyValuePair<K,V> WeakPredecessor(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return sortedpairs.WeakPredecessor(p);
		}


		/// <summary>
		/// Get the entry in the dictionary whose key is the
		/// successor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		[Tested]
		public KeyValuePair<K,V> Successor(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return sortedpairs.Successor(p);
		}


		/// <summary>
		/// Get the entry in the dictionary whose key is the
		/// weak successor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		[Tested]
		public KeyValuePair<K,V> WeakSuccessor(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return sortedpairs.WeakSuccessor(p);
		}

		#endregion

		//TODO: put in interface
		/// <summary>
		/// Make a snapshot of the current state of this dictionary
		/// </summary>
		/// <returns>The snapshot</returns>
		[Tested]
		public MSG.IEnumerable<KeyValuePair<K,V>> Snapshot()
		{
			TreeDictionary<K,V> res = (TreeDictionary<K,V>)MemberwiseClone();

			res.pairs = (TreeSet<KeyValuePair<K,V>>)sortedpairs.Snapshot();
			return res;
		}
	}
}