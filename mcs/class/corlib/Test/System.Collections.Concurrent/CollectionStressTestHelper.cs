#if NET_4_0
// 
// CollectionStressTestHelper.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Threading;
using System.Linq;
using MonoTests.System.Threading.Tasks;

using NUnit;
using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	public enum CheckOrderingType {
		InOrder,
		Reversed,
		DontCare
	}
	
	public static class CollectionStressTestHelper
	{
		public static void AddStressTest (IProducerConsumerCollection<int> coll)
		{
			ParallelTestHelper.Repeat (delegate {
				int amount = -1;
				const int count = 10;
				const int threads = 5;
				
				ParallelTestHelper.ParallelStressTest (coll, (q) => {
					int t = Interlocked.Increment (ref amount);
					for (int i = 0; i < count; i++)
						coll.TryAdd (t);
				}, threads);
				
				Assert.AreEqual (threads * count, coll.Count, "#-1");
				int[] values = new int[threads];
				int temp;
				while (coll.TryTake (out temp)) {
					values[temp]++;
				}
				
				for (int i = 0; i < threads; i++)
					Assert.AreEqual (count, values[i], "#" + i);
			});
		}
		
		public static void RemoveStressTest (IProducerConsumerCollection<int> coll, CheckOrderingType order)
		{
			ParallelTestHelper.Repeat (delegate {
				
				const int count = 10;
				const int threads = 5;
				const int delta = 5;
				
				for (int i = 0; i < (count + delta) * threads; i++)
					coll.TryAdd (i);
				
				bool state = true;
				
				Assert.AreEqual ((count + delta) * threads, coll.Count, "#0");
				
				ParallelTestHelper.ParallelStressTest (coll, (q) => {
					bool s = true;
					int t;
					
					for (int i = 0; i < count; i++)
						s &= coll.TryTake (out t);
					
					if (!s)
						state = false;
				}, threads);
				
				Assert.IsTrue (state, "#1");
				Assert.AreEqual (delta * threads, coll.Count, "#2");
				
				string actual = string.Empty;
				int temp;
				while (coll.TryTake (out temp)) {
					actual += temp.ToString ();;
				}
				
				IEnumerable<int> range = Enumerable.Range (order == CheckOrderingType.Reversed ? 0 : count * threads, delta * threads);
				if (order == CheckOrderingType.Reversed)
					range = range.Reverse ();
				
				string expected = range.Aggregate (string.Empty, (acc, v) => acc + v);
				
				if (order == CheckOrderingType.DontCare)
					CollectionAssert.AreEquivalent (expected, actual, "#3");
				else 
					Assert.AreEqual (expected, actual, "#3");
			}, 1000);
		}
	}
}
#endif
