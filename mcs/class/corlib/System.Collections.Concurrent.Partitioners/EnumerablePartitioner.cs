// 
// EnumerablePartitioner.cs
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

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Collections.Concurrent.Partitioners
{
	// Represent a chunk partitioner
	internal class EnumerablePartitioner<T> : OrderablePartitioner<T>
	{
		IEnumerable<T> source;
		
		const int InitialPartitionSize = 1;
		const int PartitionMultiplier = 2;
		
		int initialPartitionSize;
		int partitionMultiplier;

		public EnumerablePartitioner (IEnumerable<T> source)
			: this (source, InitialPartitionSize, PartitionMultiplier)
		{

		}
		
		// This is used to get striped partitionning (for Take and Skip for instance
		public EnumerablePartitioner (IEnumerable<T> source, int initialPartitionSize, int partitionMultiplier)
			 : base (true, false, true)
		{
			this.source = source;
			this.initialPartitionSize = initialPartitionSize;
			this.partitionMultiplier = partitionMultiplier;
		}
		
		public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions (int partitionCount)
		{
			if (partitionCount <= 0)
				throw new ArgumentOutOfRangeException ("partitionCount");
			
			IEnumerator<KeyValuePair<long, T>>[] enumerators
				= new IEnumerator<KeyValuePair<long, T>>[partitionCount];

			PartitionerState state = new PartitionerState ();
			IEnumerator<T> src = source.GetEnumerator ();
			bool isSimple = initialPartitionSize == 1 && partitionMultiplier == 1;

			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = isSimple ? GetPartitionEnumeratorSimple (src, state, i == enumerators.Length - 1) : GetPartitionEnumerator (src, state);
			}
			
			return enumerators;
		}

		// This partitioner that is simpler than the general case (don't use a list) is called in the case
		// of initialPartitionSize == partitionMultiplier == 1
		IEnumerator<KeyValuePair<long, T>> GetPartitionEnumeratorSimple (IEnumerator<T> src,
		                                                                 PartitionerState state,
		                                                                 bool last)
		{
			long index = -1;
			var value = default (T);

			try {
				do {
					lock (state.SyncLock) {
						if (!src.MoveNext ())
							break;

						index = state.Index++;
						value = src.Current;
					}

					yield return new KeyValuePair<long, T> (index, value);
				} while (true);
			} finally {
				if (last)
					src.Dispose ();
			}
		}
		
		IEnumerator<KeyValuePair<long, T>> GetPartitionEnumerator (IEnumerator<T> src, PartitionerState state)
		{
			int count = initialPartitionSize;
			List<T> list = new List<T> ();
			
			while (true) {
				list.Clear ();
				long ind = -1;
				
				lock (state.SyncLock) {
					ind = state.Index;
					
					for (int i = 0; i < count; i++) {
						if (!src.MoveNext ()) {
							if (list.Count == 0)
								yield break;
							else
								break;
						}
						
						list.Add (src.Current);
						state.Index++;
					}					
				}
				
				for (int i = 0; i < list.Count; i++)
					yield return new KeyValuePair<long, T> (ind + i, list[i]);
				
				count *= partitionMultiplier;
			}
		}

		class PartitionerState
		{
			public long Index = 0;
			public readonly object SyncLock = new object ();
		}
	}
}
#endif
