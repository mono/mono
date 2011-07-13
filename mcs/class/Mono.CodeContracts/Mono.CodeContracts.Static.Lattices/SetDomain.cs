// 
// SetDomain.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using System.IO;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Lattices {
	struct SetDomain<T> : IAbstractDomain<SetDomain<T>>
		where T : IEquatable<T> {
		public static readonly SetDomain<T> TopValue = new SetDomain<T> (ImmutableSet<T>.Empty ());
		public static readonly SetDomain<T> BottomValue = new SetDomain<T> ((IImmutableSet<T>) null);

		private readonly IImmutableSet<T> set;

		private SetDomain (IImmutableSet<T> set)
		{
			this.set = set;
		}

		public SetDomain (Func<T, int> keyConverter)
		{
			this.set = ImmutableSet<T>.Empty (keyConverter);
		}

		public SetDomain<T> Top
		{
			get { return TopValue; }
		}

		public SetDomain<T> Bottom
		{
			get { return BottomValue; }
		}

		public bool IsTop
		{
			get
			{
				if (this.set != null)
					return this.set.Count == 0;

				return false;
			}
		}

		public bool IsBottom
		{
			get { return this.set == null; }
		}

		public SetDomain<T> Join (SetDomain<T> that, bool widening, out bool weaker)
		{
			if (this.set == that.set) {
				weaker = false;
				return this;
			}
			if (IsBottom) {
				weaker = !that.IsBottom;
				return that;
			}
			if (that.IsBottom || IsTop) {
				weaker = false;
				return this;
			}
			if (that.IsTop) {
				weaker = !IsTop;
				return that;
			}

			IImmutableSet<T> join = this.set.Intersect (that.set);

			weaker = join.Count < this.set.Count;
			return new SetDomain<T> (join);
		}

		public SetDomain<T> Meet (SetDomain<T> that)
		{
			if (this.set == that.set || IsBottom || that.IsTop)
				return this;
			if (that.IsBottom || IsTop)
				return that;

			return new SetDomain<T> (this.set.Union (that.set));
		}

		public bool LessEqual (SetDomain<T> that)
		{
			if (IsBottom)
				return true;
			if (that.IsBottom)
				return false;

			return that.set.IsContainedIn (this.set);
		}

		public SetDomain<T> ImmutableVersion ()
		{
			return this;
		}

		public SetDomain<T> Clone ()
		{
			return this;
		}

		public SetDomain<T> Add (T elem)
		{
			return new SetDomain<T> (this.set.Add (elem));
		}

		public SetDomain<T> Remove (T elem)
		{
			return new SetDomain<T> (this.set.Remove (elem));
		}

		public bool Contains (T item)
		{
			return this.set.Contains (item);
		}

		public void Dump (TextWriter tw)
		{
			if (IsBottom)
				tw.WriteLine ("Bot");
			else if (IsTop)
				tw.WriteLine ("Top");
			else
				this.set.Dump (tw);
		}
	}
}
