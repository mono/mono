// 
// UserRangePartitioner.cs
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

#if NET_4_0

using System;
using System.Threading;
using System.Collections.Generic;

namespace System.Collections.Concurrent.Partitioners
{
	internal class UserRangePartitioner : OrderablePartitioner<Tuple<int,  int>>
	{
		readonly int start;
		readonly int end;
		readonly int rangeSize;

		public UserRangePartitioner (int start, int end, int rangeSize) : base (true, true, true)
		{
			this.start = start;
			this.end = end;
			this.rangeSize = rangeSize;
		}
		
		public override IList<IEnumerator<KeyValuePair<long, Tuple<int, int>>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			int currentIndex = 0;
			Func<int> getNextIndex = () => Interlocked.Increment (ref currentIndex) - 1;

			var enumerators = new IEnumerator<KeyValuePair<long, Tuple<int, int>>>[partitionCount];
			for (int i = 0; i < partitionCount; i++)
				enumerators[i] = GetEnumerator (getNextIndex);

			return enumerators;
		}
		
		IEnumerator<KeyValuePair<long, Tuple<int, int>>> GetEnumerator (Func<int> getNextIndex)
		{
			while (true) {
				int index = getNextIndex ();
				int sliceStart = index * rangeSize + start;

				if (sliceStart >= end)
					break;

				yield return new KeyValuePair<long, Tuple<int, int>> (index, Tuple.Create (sliceStart, Math.Min (end, sliceStart + rangeSize)));
				sliceStart += rangeSize;
			}
		}
	}

	internal class UserLongRangePartitioner : OrderablePartitioner<Tuple<long,  long>>
	{
		readonly long start;
		readonly long end;
		readonly long rangeSize;

		public UserLongRangePartitioner (long start, long end, long rangeSize) : base (true, true, true)
		{
			this.start = start;
			this.end = end;
			this.rangeSize = rangeSize;
		}
		
		public override IList<IEnumerator<KeyValuePair<long, Tuple<long, long>>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			long currentIndex = 0;
			Func<long> getNextIndex = () => Interlocked.Increment (ref currentIndex) - 1;

			var enumerators = new IEnumerator<KeyValuePair<long, Tuple<long, long>>>[partitionCount];
			for (int i = 0; i < partitionCount; i++)
				enumerators[i] = GetEnumerator (getNextIndex);

			return enumerators;
		}
		
		IEnumerator<KeyValuePair<long, Tuple<long, long>>> GetEnumerator (Func<long> getNextIndex)
		{
			while (true) {
				long index = getNextIndex ();
				long sliceStart = index * rangeSize + start;

				if (sliceStart >= end)
					break;

				yield return new KeyValuePair<long, Tuple<long, long>> (index, Tuple.Create (sliceStart, Math.Min (end, sliceStart + rangeSize)));
				sliceStart += rangeSize;
			}
		}
	}
}
#endif
