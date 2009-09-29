//
// CSharpInvokeMemberBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.CSharp.RuntimeBinder
{
	static class Extensions
	{
		public static IList<T> ToReadOnly<T> (this IEnumerable<T> col)
		{
			return col == null ?
				new ReadOnlyCollectionBuilder<T> (0) :
				new ReadOnlyCollectionBuilder<T> (col);
		}

		public static int HashCode (int h1, int h2, int h3)
		{
			const int FNV_prime = 16777619;
			int hash = unchecked ((int) 2166136261);

			hash = (hash ^ h1) * FNV_prime;
			hash = (hash ^ h2) * FNV_prime;
			hash = (hash ^ h3) * FNV_prime;

			hash += hash << 13;
			hash ^= hash >> 7;
			hash += hash << 3;
			hash ^= hash >> 17;
			hash += hash << 5;

			return hash;
		}

		public static int HashCode (int h1, int h2, int h3, int h4, int h5)
		{
			const int FNV_prime = 16777619;
			int hash = unchecked ((int) 2166136261);

			hash = (hash ^ h1) * FNV_prime;
			hash = (hash ^ h2) * FNV_prime;
			hash = (hash ^ h3) * FNV_prime;
			hash = (hash ^ h4) * FNV_prime;
			hash = (hash ^ h5) * FNV_prime;

			hash += hash << 13;
			hash ^= hash >> 7;
			hash += hash << 3;
			hash ^= hash >> 17;
			hash += hash << 5;

			return hash;
		}
	}
}
