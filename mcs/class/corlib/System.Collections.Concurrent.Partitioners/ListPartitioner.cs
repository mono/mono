// 
// ListPartitioner.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.Concurrent.Partitioners
{
	// Represent a Range partitioner
	internal class ListPartitioner<T> : OrderablePartitioner<T>
	{
		IList<T> source;
		
		public ListPartitioner (IList<T> source) : base (true, true, true)
		{
			this.source = source;
		}
		
		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			IEnumerator<KeyValuePair<long, T>>[] enumerators
				= new IEnumerator<KeyValuePair<long, T>>[partitionCount];
			
			int count = source.Count / partitionCount;
			int extra = 0;

			if (source.Count < partitionCount) {
				count = 1;
			} else {
				extra = source.Count % partitionCount;
				if (extra > 0)
					++count;
			}

			int currentIndex = 0;

			Range[] ranges = new Range[enumerators.Length];
			for (int i = 0; i < ranges.Length; i++) {
				ranges[i] = new Range (currentIndex,
				                       currentIndex + count);
				currentIndex += count;
				if (--extra == 0)
					--count;
			}
			
			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = GetEnumeratorForRange (ranges, i);
			}
			
			return enumerators;
		}

		class Range
		{
			public int Actual;
			public readonly int LastIndex;

			public Range (int frm, int lastIndex)
			{
				Actual = frm;
				LastIndex = lastIndex;
			}
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRange (Range[] ranges, int workerIndex)
		{
			if (ranges[workerIndex].Actual >= source.Count)
			  return GetEmpty ();
			
			return GetEnumeratorForRangeInternal (ranges, workerIndex);
		}

		IEnumerator<KeyValuePair<long, T>> GetEmpty ()
		{
			yield break;
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRangeInternal (Range[] ranges, int workerIndex)
		{
			Range range = ranges[workerIndex];
			int lastIndex = range.LastIndex;
			int index = range.Actual;

			for (int i = index; i < lastIndex; i = ++range.Actual) {
				yield return new KeyValuePair<long, T> (i, source[i]);
			}
		}
	}
}
#endif
