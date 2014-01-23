// 
// Partitioner.cs
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
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
	using Partitioners;

	public static class Partitioner
	{
		public static OrderablePartitioner<TSource> Create<TSource> (IEnumerable<TSource> source)
		{
			IList<TSource> tempIList = source as IList<TSource>;
			if (tempIList != null)
				return Create (tempIList, true);
			
			return new EnumerablePartitioner<TSource> (source);
		}
		
		public static OrderablePartitioner<TSource> Create<TSource> (TSource[] array, bool loadBalance)
		{
			return Create ((IList<TSource>)array, loadBalance);
		}
		
		public static OrderablePartitioner<TSource> Create<TSource> (IList<TSource> list, bool loadBalance)
		{
			return new ListPartitioner<TSource> (list);
		}
		
		public static OrderablePartitioner<Tuple<int, int>> Create (int fromInclusive,
		                                                            int toExclusive)
		{
			// This formula that is somewhat non-straighforward was guessed based on MS output
			int rangeSize = (toExclusive - fromInclusive) / (Environment.ProcessorCount * 3);
			if (rangeSize < 1)
				rangeSize = 1;

			return Create (fromInclusive, toExclusive, rangeSize);
		}

		public static OrderablePartitioner<Tuple<int, int>> Create (int fromInclusive,
		                                                            int toExclusive,
		                                                            int rangeSize)
		{
			if (fromInclusive >= toExclusive)
				throw new ArgumentOutOfRangeException ("toExclusive");
			if (rangeSize <= 0)
				throw new ArgumentOutOfRangeException ("rangeSize");

			return new UserRangePartitioner (fromInclusive, toExclusive, rangeSize);
		}

		public static OrderablePartitioner<Tuple<long, long>> Create (long fromInclusive,
		                                                              long toExclusive)
		{
			long rangeSize = (toExclusive - fromInclusive) / (Environment.ProcessorCount * 3);
			if (rangeSize < 1)
				rangeSize = 1;

			return Create (fromInclusive, toExclusive, rangeSize);
		}

		public static OrderablePartitioner<Tuple<long, long>> Create (long fromInclusive,
		                                                              long toExclusive,
		                                                              long rangeSize)
		{
			if (rangeSize <= 0)
				throw new ArgumentOutOfRangeException ("rangeSize");
			if (fromInclusive >= toExclusive)
				throw new ArgumentOutOfRangeException ("toExclusive");

			return new UserLongRangePartitioner (fromInclusive, toExclusive, rangeSize);
		}
		
#if NET_4_5
		[MonoTODO]
		public static OrderablePartitioner<TSource> Create<TSource> (IEnumerable<TSource> source,
									     EnumerablePartitionerOptions partitionerOptions)
		{
			throw new NotImplementedException ();
		}
#endif
	}
	
	public abstract class Partitioner<TSource>
	{
		protected Partitioner ()
		{
			
		}
		
		public virtual IEnumerable<TSource> GetDynamicPartitions ()
		{
			if (!SupportsDynamicPartitions)
				throw new NotSupportedException ();
			
			return null;
		}
		
		public abstract IList<IEnumerator<TSource>> GetPartitions (int partitionCount);
		
		public virtual bool SupportsDynamicPartitions {
			get {
				return false;
			}
		}
	}
}
#endif
