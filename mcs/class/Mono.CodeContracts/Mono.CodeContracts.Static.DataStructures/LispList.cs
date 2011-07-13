// 
// LispList.cs
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
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures {
	class LispList<T> {
		public static readonly LispList<T> Empty = null;
		private readonly int count;
		private readonly T element;
		private readonly LispList<T> tail;

		private LispList (T elem, LispList<T> tail)
		{
			this.element = elem;
			this.tail = tail;
			this.count = LengthOf (tail) + 1;
		}

		public T Head
		{
			get { return this.element; }
		}

		public LispList<T> Tail
		{
			get { return this.tail; }
		}

		public static LispList<T> Cons (T elem, LispList<T> tail)
		{
			return new LispList<T> (elem, tail);
		}

		public static LispList<T> Reverse (LispList<T> list)
		{
			LispList<T> rest = null;
			for (; list != null; list = list.tail)
				rest = rest.Cons (list.element);
			return rest;
		}

		public static bool Contains (LispList<T> l, T o)
		{
			if (l == null)
				return false;
			var equatable = o as IEquatable<T>;
			if (equatable != null) {
				if (equatable.Equals (l.element))
					return true;
			} else if (o.Equals (l.element))
				return true;

			return Contains (l.tail, o);
		}

		public static int LengthOf (LispList<T> list)
		{
			if (list == null)
				return 0;
			return list.count;
		}

		public static void Apply (LispList<T> list, Action<T> action)
		{
			for (; list != null; list = list.tail)
				action (list.Head);
		}

		public static IEnumerable<T> PrivateGetEnumerable (LispList<T> list)
		{
			LispList<T> current = list;
			while (current != null) {
				T next = current.Head;
				current = current.tail;
				yield return next;
			}
		}

		public static LispList<S> Select<S> (LispList<T> list, Func<T, S> selector)
		{
			if (list == null)
				return null;
			return list.tail.Select (selector).Cons (selector (list.Head));
		}

		public override string ToString ()
		{
			var sb = new StringBuilder ();
			BuildString (sb);

			return sb.ToString ();
		}

		private void BuildString (StringBuilder sb)
		{
			sb.Append (this.element == null ? "<null>" : this.element.ToString ());
			if (this.tail != null) {
				sb.Append (",");
				this.tail.BuildString (sb);
			}
		}
	}
}
