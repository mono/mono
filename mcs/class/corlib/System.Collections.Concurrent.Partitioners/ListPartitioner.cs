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
			if (count <= 1)
				count = 1;

			int extra = count == 1 ? 0 : source.Count % partitionCount;
			if (extra > 0)
				++count;

			int currentIndex = 0;

			StealRange[] ranges = new StealRange[enumerators.Length];
			for (int i = 0; i < ranges.Length; i++) {
				ranges[i] = new StealRange (currentIndex,
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

		[StructLayout(LayoutKind.Explicit)]
		struct StealValue {
			[FieldOffset(0)]
			public long Value;
			[FieldOffset(0)]
			public int Actual;
			[FieldOffset(4)]
			public int Stolen;
		}

		class StealRange
		{
			public StealValue V = new StealValue ();
			public readonly int LastIndex;

			public StealRange (int frm, int lastIndex)
			{
				V.Actual = frm;
				LastIndex = lastIndex;
			}
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRange (StealRange[] ranges, int workerIndex)
		{
			if (ranges[workerIndex].V.Actual >= source.Count)
			  return GetEmpty ();
			
			return GetEnumeratorForRangeInternal (ranges, workerIndex);
		}

		IEnumerator<KeyValuePair<long, T>> GetEmpty ()
		{
			yield break;
		}
		
		IEnumerator<KeyValuePair<long, T>> GetEnumeratorForRangeInternal (StealRange[] ranges, int workerIndex)
		{
			StealRange range = ranges[workerIndex];
			int lastIndex = range.LastIndex;
			int index = range.V.Actual;

			// HACK: The algorithm here shouldn't need the following Int.Incr call (or a Thread.MemoryBarrier)
			// but for weird reasons it does.
			for (int i = index; i < lastIndex; i = Interlocked.Increment (ref range.V.Actual)) {
				if (i >= lastIndex - range.V.Stolen)
					break;

				yield return new KeyValuePair<long, T> (i, source[i]);

				if (i + 1 >= lastIndex - range.V.Stolen)
					break;
			}

			int num = ranges.Length;
			int len = num + workerIndex;

			for (int sIndex = workerIndex + 1; sIndex < len; ++sIndex) {
				int extWorker = sIndex % num;
				range = ranges[extWorker];
				lastIndex = range.LastIndex;

				StealValue val;
				long old;
				int stolen;

				do {
					do {
						val = range.V;
						old = val.Value;

						if (val.Actual >= lastIndex - val.Stolen - 1)
							goto next;
						val.Stolen += 1;
					} while (Interlocked.CompareExchange (ref range.V.Value, val.Value, old) != old);

					stolen = lastIndex - val.Stolen;

					if (stolen > range.V.Actual || (stolen == range.V.Actual && range.V.Actual == val.Actual + 1))
						yield return new KeyValuePair<long, T> (stolen, source[stolen]);
				} while (stolen >= 0);

				next:
				continue;
			}
		}
	}
}
#endif
