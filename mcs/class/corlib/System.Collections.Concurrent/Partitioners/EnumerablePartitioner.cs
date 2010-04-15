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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	// Represent a chunk partitioner
	internal class EnumerablePartitioner<T> : OrderablePartitioner<T>
	{
		IEnumerable<T> source;
		
		const int InitialPartitionSize = 1;
		const int PartitionMultiplier = 2;
		
		int initialPartitionSize;
		int partitionMultiplier;
		
		int index = 0;
		readonly object syncLock = new object ();
		
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
			
			IEnumerator<T> src = source.GetEnumerator ();
			
			for (int i = 0; i < enumerators.Length; i++) {
				enumerators[i] = GetPartitionEnumerator (src);
			}
			
			return enumerators;
		}
		
		IEnumerator<KeyValuePair<long, T>> GetPartitionEnumerator (IEnumerator<T> src)
		{
			int count = initialPartitionSize;
			List<T> list = new List<T> ();
			
			while (true) {
				list.Clear ();
				int ind = -1;
				
				lock (syncLock) {
					ind = index;
					
					for (int i = 0; i < count; i++) {
						if (!src.MoveNext ()) {
							if (list.Count == 0)
								yield break;
							else
								break;
						}
						
						list.Add (src.Current);
						index++;
					}					
				}
				
				
				
				for (int i = 0; i < list.Count; i++)
					yield return new KeyValuePair<long, T> (ind + i, list[i]);
				
				count *= partitionMultiplier;
			}
		}                                  
	}
}
#endif
