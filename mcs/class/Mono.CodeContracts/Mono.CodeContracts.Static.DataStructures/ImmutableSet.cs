// 
// ImmutableSet.cs
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
using System.IO;

namespace Mono.CodeContracts.Static.DataStructures {
	class ImmutableSet<T> : IImmutableSet<T>
		where T : IEquatable<T> {
		private readonly IImmutableMap<T, Dummy> underlying;

	    private ImmutableSet (IImmutableMap<T, Dummy> immutableMap)
		{
			this.underlying = immutableMap;
		}

		#region Implementation of IImmutableSet<T>
		public T Any
		{
			get { return this.underlying.AnyKey; }
		}

		public int Count
		{
			get { return this.underlying.Count; }
		}

		public IEnumerable<T> Elements
		{
			get { return this.underlying.Keys; }
		}

		public IImmutableSet<T> Add (T item)
		{
			return new ImmutableSet<T> (this.underlying.Add (item, Dummy.Value));
		}

	    public IImmutableSet<T> AddRange(IEnumerable<T> seq)
	    {
	        var map = this.underlying;
	        foreach (var item in seq)
	            map = map.Add(item, Dummy.Value);

            return new ImmutableSet<T>(map);
	    }

	    public IImmutableSet<T> Remove (T item)
		{
			return new ImmutableSet<T> (this.underlying.Remove (item));
		}

		public bool Contains (T item)
		{
			return this.underlying.ContainsKey (item);
		}

		public bool IsContainedIn (IImmutableSet<T> that)
		{
			if (Count > that.Count)
				return false;
			bool result = true;
			this.underlying.Visit ((e, dummy) => {
			                       	if (that.Contains (e))
			                       		return VisitStatus.ContinueVisit;

			                       	result = false;
			                       	return VisitStatus.StopVisit;
			                       });
			return result;
		}

		public IImmutableSet<T> Intersect (IImmutableSet<T> that)
		{
			if (this == that)
				return this;
			if (Count == 0)
				return this;
			IImmutableSet<T> set;
			IImmutableSet<T> larger;
			if (Count < that.Count) {
				set = this;
				larger = that;
			} else {
				if (that.Count == 0)
					return that;
				set = that;
				larger = this;
			}
			IImmutableSet<T> result = set;
			set.Visit ((e) => {
			           	if (!larger.Contains (e))
			           		result = result.Remove (e);
			           });
			return result;
		}

		public IImmutableSet<T> Union (IImmutableSet<T> that)
		{
			if (this == that)
				return this;
			if (Count == 0)
				return that;
			IImmutableSet<T> smaller;
			IImmutableSet<T> larger;
			if (Count < that.Count) {
				smaller = this;
				larger = that;
			} else {
				if (that.Count == 0)
					return this;
				smaller = that;
				larger = this;
			}
			IImmutableSet<T> result = larger;
			smaller.Visit (e => { result = result.Add (e); });
			return result;
		}

		public void Visit (Action<T> visitor)
		{
			this.underlying.Visit ((elem, dummy) => {
			                       	visitor (elem);
			                       	return VisitStatus.ContinueVisit;
			                       });
		}

		public void Dump (TextWriter tw)
		{
			tw.Write ("{");
			bool first = true;
			foreach (T key in this.underlying.Keys) {
				if (!first)
					tw.Write (", ");
				else
					first = false;
				tw.Write ("{0}", key);
			}
			tw.WriteLine ("}");
		}

		public static IImmutableSet<T> Empty ()
		{
			return new ImmutableSet<T> (ImmutableMap<T, Dummy>.Empty);
		}

		public static IImmutableSet<T> Empty (Func<T, int> keyConverter)
		{
			return new ImmutableSet<T> (ImmutableIntKeyMap<T, Dummy>.Empty (keyConverter));
		}
		#endregion
	}
}
