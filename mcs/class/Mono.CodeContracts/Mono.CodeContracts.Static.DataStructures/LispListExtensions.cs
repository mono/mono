// 
// LispListExtensions.cs
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
	static class LispListExtensions {
		public static LispList<T> Cons<T> (this LispList<T> rest, T elem)
		{
			return LispList<T>.Cons (elem, rest);
		}

		public static LispList<T> Append<T> (this LispList<T> list, LispList<T> append)
		{
			if (list == null)
				return append;
			if (append == null)
				return list;

			return Cons (list.Tail.Append (append), list.Head);
		}

		public static LispList<T> Where<T> (this LispList<T> list, Predicate<T> keep)
		{
			if (list == null)
				return null;
			LispList<T> rest = list.Tail.Where (keep);
			if (!keep (list.Head))
				return rest;

			if (rest == list.Tail)
				return list;

			return Cons (rest, list.Head);
		}

		public static void Apply<T> (this LispList<T> list, Action<T> action)
		{
			LispList<T>.Apply (list, action);
		}

		public static IEnumerable<T> AsEnumerable<T> (this LispList<T> list)
		{
			return LispList<T>.PrivateGetEnumerable (list);
		}

		public static bool Any<T> (this LispList<T> list, Predicate<T> predicate)
		{
			if (list == null)
				return false;

			if (predicate (list.Head))
				return true;

			return list.Tail.Any (predicate);
		}

		public static int Length<T> (this LispList<T> list)
		{
			return LispList<T>.LengthOf (list);
		}

		public static bool IsEmpty<T> (this LispList<T> list)
		{
			return list == null;
		}

		public static LispList<S> Select<T, S> (this LispList<T> list, Func<T, S> selector)
		{
			return LispList<T>.Select (list, selector);
		}

		public static T Last<T> (this LispList<T> list)
		{
			if (list == null)
				return default(T);

			while (LispList<T>.LengthOf (list) > 1)
				list = list.Tail;

			return list.Head;
		}

		public static LispList<T> Coerce<S, T> (this LispList<S> list)
			where S : T
		{
			return list.Select (l => (T) l);
		}
	}
}
