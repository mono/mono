//
// TemporaryArea.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2012 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq.Parallel
{
	internal class TemporaryArea<TKey, TValue>
	{
		Dictionary<TKey, TValue> dict;

		public TemporaryArea () : this (EqualityComparer<TKey>.Default)
		{

		}

		public TemporaryArea (IEqualityComparer<TKey> comparer)
		{
			this.dict = new Dictionary<TKey, TValue> (comparer);
		}

		public bool TryAdd (TKey index, TValue value)
		{
			lock (dict) {
				if (dict.ContainsKey (index))
					return false;
				dict.Add (index, value);
				return true;
			}
		}

		public bool TryRemove (TKey index, out TValue value)
		{
			lock (dict) {
				if (!dict.TryGetValue (index, out value))
					return false;
				return dict.Remove (index);
			}
		}
	}
}

#endif
