// 
// DoubleImmutableMap.cs
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

namespace Mono.CodeContracts.Static.DataStructures {
	class DoubleImmutableMap<A, B, C>
		where A : IEquatable<A>
		where B : IEquatable<B> {
		private readonly B[] EmptyCache = new B[0];
		private readonly IImmutableMap<A, IImmutableMap<B, C>> map;

		private DoubleImmutableMap (IImmutableMap<A, IImmutableMap<B, C>> map)
		{
			this.map = map;
		}

		public C this [A key1, B key2]
		{
			get
			{
				IImmutableMap<B, C> inner = this.map [key1];
				if (inner == null)
					return default(C);
				return inner [key2];
			}
		}

		public int Keys1Count
		{
			get { return this.map.Count; }
		}

		public IEnumerable<A> Keys1
		{
			get { return this.map.Keys; }
		}

		public DoubleImmutableMap<A, B, C> Add (A key1, B key2, C value)
		{
			IImmutableMap<B, C> immutableMap = this.map [key1] ?? ImmutableMap<B, C>.Empty;
			return new DoubleImmutableMap<A, B, C> (this.map.Add (key1, immutableMap.Add (key2, value)));
		}

		public DoubleImmutableMap<A, B, C> RemoveAll (A key1)
		{
			return new DoubleImmutableMap<A, B, C> (this.map.Remove (key1));
		}

		public DoubleImmutableMap<A, B, C> Remove (A key1, B key2)
		{
			IImmutableMap<B, C> inner = this.map [key1];
			if (inner == null)
				return this;
			IImmutableMap<B, C> newInner = inner.Remove (key2);
			if (newInner == inner)
				return this;
			return new DoubleImmutableMap<A, B, C> (this.map.Add (key1, newInner));
		}

		public static DoubleImmutableMap<A, B, C> Empty (Func<A, int> uniqueIdGenerator)
		{
			return new DoubleImmutableMap<A, B, C> (ImmutableIntKeyMap<A, IImmutableMap<B, C>>.Empty (uniqueIdGenerator));
		}

		public bool Contains (A key1, B key2)
		{
			IImmutableMap<B, C> inner = this.map [key1];
			if (inner == null)
				return false;
			return inner.ContainsKey (key2);
		}

		public bool ContainsKey1 (A key1)
		{
			return this.map.ContainsKey (key1);
		}

		public int Keys2Count (A key1)
		{
			if (key1 == null)
				return 0;
			IImmutableMap<B, C> inner = this.map [key1];
			if (inner == null)
				return 0;
			return inner.Count;
		}

		public IEnumerable<B> Keys2 (A key1)
		{
			if (key1 == null)
				return this.EmptyCache;

			IImmutableMap<B, C> inner = this.map [key1];
			if (inner == null)
				return this.EmptyCache;
			return inner.Keys;
		}
	}
}
