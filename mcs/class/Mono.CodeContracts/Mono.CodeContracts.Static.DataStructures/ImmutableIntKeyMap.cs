// 
// ImmutableIntKeyMap.cs
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
	class ImmutableIntKeyMap<K, V> : IImmutableMap<K, V>, IEquatable<IImmutableMap<K, V>> {
		private readonly IImmutableIntMap<Pair<K, V>> immutable_int_map;
		private readonly Func<K, int> keyConverter;

		private ImmutableIntKeyMap (IImmutableIntMap<Pair<K, V>> map, Func<K, int> converter)
		{
			this.immutable_int_map = map;
			this.keyConverter = converter;
		}

		#region Implementation of IImmutableMap<K,V>
		public V this [K key]
		{
			get
			{
				Pair<K, V> pair = this.immutable_int_map [this.keyConverter (key)];
				if (pair != null)
					return pair.Value;
				return default(V);
			}
		}

		public K AnyKey
		{
			get { return Keys.First (); }
		}

		public IEnumerable<K> Keys
		{
			get
			{
				var res = new List<K> ();
				this.immutable_int_map.Visit (data => res.Add (data.Key));
				return res;
			}
		}

		public int Count
		{
			get { return this.immutable_int_map.Count; }
		}

		public IImmutableMap<K, V> EmptyMap
		{
			get { return Empty (this.keyConverter); }
		}

		public IImmutableMap<K, V> Add (K key, V value)
		{
			return new ImmutableIntKeyMap<K, V> (this.immutable_int_map.Add (this.keyConverter (key), new Pair<K, V> (key, value)), this.keyConverter);
		}

		public IImmutableMap<K, V> Remove (K key)
		{
			return new ImmutableIntKeyMap<K, V> (this.immutable_int_map.Remove (this.keyConverter (key)), this.keyConverter);
		}

		public bool ContainsKey (K key)
		{
			return this.immutable_int_map.Contains (this.keyConverter (key));
		}

		public void Visit (Func<K, V, VisitStatus> func)
		{
			this.immutable_int_map.Visit (data => func (data.Key, data.Value));
		}
		#endregion

		#region Implementation of IEquatable<IImmutableMap<K,V>>
		public bool Equals (IImmutableMap<K, V> other)
		{
			return this == other;
		}
		#endregion

		public static IImmutableMap<K, V> Empty (Func<K, int> keyConverter)
		{
			return new ImmutableIntKeyMap<K, V> (ImmutableIntMap<Pair<K, V>>.Empty (), keyConverter);
		}
	}
}
