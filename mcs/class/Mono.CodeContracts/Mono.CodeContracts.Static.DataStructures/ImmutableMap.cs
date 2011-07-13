// 
// ImmutableMap.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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
using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.Static.DataStructures {
	class ImmutableMap<K, V> : IImmutableMap<K, V>
		where K : IEquatable<K> {
		public static ImmutableMap<K, V> Empty = new ImmutableMap<K, V> (ImmutableIntMap<LispList<Pair<K, V>>>.Empty (), 0, null);
		private readonly int count;
		private readonly IImmutableIntMap<LispList<Pair<K, V>>> immutable_int_map;
		private readonly LispList<K> keys;

		private ImmutableMap (IImmutableIntMap<LispList<Pair<K, V>>> map, int count, LispList<K> keys)
		{
			this.keys = keys;
			this.count = count;
			this.immutable_int_map = map;
		}

		#region Implementation of IImmutableMap<K,V>
		public V this [K key]
		{
			get
			{
				for (LispList<Pair<K, V>> list = this.immutable_int_map [key.GetHashCode ()]; list != null; list = list.Tail) {
					K k = list.Head.Key;
					if (key.Equals (k))
						return list.Head.Value;
				}
				return default(V);
			}
		}

		public K AnyKey
		{
			get { return this.keys.Head; }
		}

		public IEnumerable<K> Keys
		{
			get { return this.keys.AsEnumerable (); }
		}

		public int Count
		{
			get { return this.immutable_int_map.Count; }
		}

		public IImmutableMap<K, V> EmptyMap
		{
			get { return Empty; }
		}

		public IImmutableMap<K, V> Add (K key, V value)
		{
			int hashCode = key.GetHashCode ();
			LispList<Pair<K, V>> list1 = this.immutable_int_map [hashCode];
			LispList<Pair<K, V>> newList = Remove (list1, key).Cons (new Pair<K, V> (key, value));
			int diff = newList.Length () - list1.Length ();
			LispList<K> newKeys = diff == 0 ? this.keys : this.keys.Cons (key);

			return new ImmutableMap<K, V> (this.immutable_int_map.Add (hashCode, newList), this.count + diff, newKeys);
		}

		public IImmutableMap<K, V> Remove (K key)
		{
			int hashCode = key.GetHashCode ();
			LispList<Pair<K, V>> from = this.immutable_int_map [hashCode];
			if (from == null)
				return this;
			LispList<Pair<K, V>> newList = Remove (from, key);
			if (newList == from)
				return this;
			LispList<K> newKeys = RemoveKey (key, this.keys);
			if (newList == null)
				return new ImmutableMap<K, V> (this.immutable_int_map.Remove (hashCode), this.count - 1, newKeys);
			return new ImmutableMap<K, V> (this.immutable_int_map.Add (hashCode, newList), this.count - 1, newKeys);
		}

		public bool ContainsKey (K key)
		{
			return this.immutable_int_map [key.GetHashCode ()].AsEnumerable ().Any (pair => key.Equals (pair.Key));
		}

		public void Visit (Func<K, V, VisitStatus> func)
		{
			this.immutable_int_map.Visit (list => {
			                              	foreach (var pair in list.AsEnumerable ())
			                              		func (pair.Key, pair.Value);
			                              });
		}

		private LispList<Pair<K, V>> Remove (LispList<Pair<K, V>> from, K key)
		{
			if (from == null)
				return null;
			if (key.Equals (from.Head.Key))
				return from.Tail;
			LispList<Pair<K, V>> tail = Remove (from.Tail, key);
			return tail == from.Tail ? from : tail.Cons (from.Head);
		}

		private static LispList<K> RemoveKey (K key, LispList<K> keys)
		{
			if (keys == null)
				throw new InvalidOperationException ();

			if (key.Equals (keys.Head))
				return keys.Tail;

			return RemoveKey (key, keys.Tail).Cons (keys.Head);
		}
		#endregion
	}
}
