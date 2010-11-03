// 
// PartitionerTests.cs
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
using System.Collections.Concurrent;

using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture]
	public class PartitionerTests
	{
		[Test]
		public void PartitionerCreateIntegerWithExplicitRange ()
		{
			OrderablePartitioner<Tuple<int, int>> partitioner = Partitioner.Create (1, 20, 5);
			var partitions = partitioner.GetOrderablePartitions (3);
			Assert.AreEqual (3, partitions.Count);
			CollectionAssert.AllItemsAreNotNull (partitions);
			var iterator = partitions[0];			
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (Create (0, 1, 6)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (Create (1, 6, 11)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (Create (2, 11, 16)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (Create (3, 16, 20)));
			
			Assert.IsFalse (partitions[1].MoveNext ());
			Assert.IsFalse (partitions[2].MoveNext ());
		}

		[Test]
		public void PartitionerCreateLongWithExplicitRange ()
		{
			OrderablePartitioner<Tuple<long, long>> partitioner = Partitioner.Create ((long)1, (long)20, (long)5);
			var partitions = partitioner.GetOrderablePartitions (3);
			Assert.AreEqual (3, partitions.Count);
			CollectionAssert.AllItemsAreNotNull (partitions);
			var iterator = partitions[0];			
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (CreateL (0, 1, 6)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (CreateL (1, 6, 11)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (CreateL (2, 11, 16)));
			Assert.IsTrue (iterator.MoveNext ());
			Assert.IsTrue (iterator.Current.Equals (CreateL (3, 16, 20)));
			
			Assert.IsFalse (partitions[1].MoveNext ());
			Assert.IsFalse (partitions[2].MoveNext ());
		}

		static KeyValuePair<long, Tuple<int, int>> Create (long ind, int i1, int i2)
		{
			return new KeyValuePair<long, Tuple<int, int>> (ind, Tuple.Create (i1, i2));
		}

		static KeyValuePair<long, Tuple<long, long>> CreateL (long ind, long i1, long i2)
		{
			return new KeyValuePair<long, Tuple<long, long>> (ind, Tuple.Create (i1, i2));
		}
	}
}
#endif
