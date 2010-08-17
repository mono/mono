#if NET_4_0
//
// StripPartitioner.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	internal class StripPartitioner<T> : OrderablePartitioner<T>
	{
		IList<T> source;

		public StripPartitioner (IList<T> source) : base (true, false, true)
		{
			this.source = source;
		}

		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			IEnumerator<KeyValuePair<long, T>>[] array = new IEnumerator<KeyValuePair<long, T>>[partitionCount];
			for (int i = 0; i < array.Length; i++)
				array[i] = GetStripEnumerator (i, partitionCount);

			return array;
		}

		IEnumerator<KeyValuePair<long, T>> GetStripEnumerator (int start, int partitionCount)
		{
			for (int i = start; i < source.Count; i += partitionCount) {
				//Console.WriteLine ("Num {0} yielding [{1} : {2}]", start, i, source[i]);
				yield return new KeyValuePair<long, T> (i, source [i]);
			}
		}
	}
}
#endif
