#if NET_4_0
// ParallelTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NUnit;
using NUnit.Core;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	
	[TestFixture()]
	public class ParallelTests
	{
		
		[Test]
		public void ParallelForTestCase ()
		{
			int[] expected = Enumerable.Range (1, 1000).Select ((e) => e * 2).ToArray ();
			
			ParallelTestHelper.Repeat (() => {	
				int[] actual = Enumerable.Range (1, 1000).ToArray ();
				SpinWait sw = new SpinWait ();
				
				Parallel.For (0, actual.Length, (i) => { actual[i] *= 2; sw.SpinOnce (); });
				
				CollectionAssert.AreEquivalent (expected, actual, "#1, same");
				CollectionAssert.AreEqual (expected, actual, "#2, in order");
			});
		}

		[Test, ExpectedException (typeof (AggregateException))]
		public void ParallelForExceptionTestCase ()
		{
			Parallel.For(1, 100, delegate (int i) { throw new Exception("foo"); });
		}
		
		[Test]
		public void ParallelForSmallRangeTest ()
		{
			ParallelTestHelper.Repeat (() => {
				int test = -1;
				Parallel.For (0, 1, (i) => test = i);
				
				Assert.AreEqual (0, test, "#1");
			});
		}
		
		[Test]
		public void ParallelForNoOperationTest ()
		{
			bool launched = false;
			Parallel.For (4, 1, (i) => launched = true);
			Assert.IsFalse (launched, "#1");
		}

		[Test]
		public void ParallelForNestedTest ()
		{
			bool[] launched = new bool[6 * 20 * 10];
			Parallel.For (0, 6, delegate (int i) {
				Parallel.For (0, 20, delegate (int j) {
					Parallel.For (0, 10, delegate (int k) {
							launched[i * 20 * 10 + j * 10 + k] = true;
					});
				});
		    });

			Assert.IsTrue (launched.All ((_) => _), "All true");
		}
		
		[Test]
		public void ParallelForEachTestCase ()
		{
			ParallelTestHelper.Repeat (() => {
				IEnumerable<int> e = Enumerable.Repeat(1, 500);
				ConcurrentQueue<int> queue = new ConcurrentQueue<int> ();
				SpinWait sw = new SpinWait ();
				int count = 0;
				
				Parallel.ForEach (e, (element) => { Interlocked.Increment (ref count); queue.Enqueue (element); sw.SpinOnce (); });
				
				Assert.AreEqual (500, count, "#1");
				CollectionAssert.AreEquivalent (e, queue, "#2");
			});
		}
		
		[Test, ExpectedException (typeof (AggregateException))]
		public void ParallelForEachExceptionTestCase ()
		{
			IEnumerable<int> e = Enumerable.Repeat (1, 10);
			Parallel.ForEach (e, delegate (int element) { throw new Exception ("foo"); });
		}
	}
}
#endif
