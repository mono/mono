// ConcurrentQueueTest.cs
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
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using NUnit.Framework;
using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Collections.Concurrent
{
	
	
	[TestFixture()]
	public class ConcurrentQueueTests
	{
		ConcurrentQueue<int> queue;
		
		[SetUpAttribute]
		public void Setup()
		{
			queue = new ConcurrentQueue<int>();
			for (int i = 0; i < 10; i++) {
				queue.Enqueue(i);
			}
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void StressEnqueueTestCase ()
		{
			/*ParallelTestHelper.Repeat (delegate {
				queue = new ConcurrentQueue<int> ();
				int amount = -1;
				const int count = 10;
				const int threads = 5;
				
				ParallelTestHelper.ParallelStressTest (queue, (q) => {
					int t = Interlocked.Increment (ref amount);
					for (int i = 0; i < count; i++)
						queue.Enqueue (t);
				}, threads);
				
				Assert.AreEqual (threads * count, queue.Count, "#-1");
				int[] values = new int[threads];
				int temp;
				while (queue.TryDequeue (out temp)) {
					values[temp]++;
				}
				
				for (int i = 0; i < threads; i++)
					Assert.AreEqual (count, values[i], "#" + i);
			});*/
			
			CollectionStressTestHelper.AddStressTest (new ConcurrentQueue<int> ());
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void StressDequeueTestCase ()
		{
			/*ParallelTestHelper.Repeat (delegate {
				queue = new ConcurrentQueue<int> ();
				const int count = 10;
				const int threads = 5;
				const int delta = 5;
				
				for (int i = 0; i < (count + delta) * threads; i++)
					queue.Enqueue (i);
				
				bool state = true;
				
				ParallelTestHelper.ParallelStressTest (queue, (q) => {
					int t;
					for (int i = 0; i < count; i++)
						state &= queue.TryDequeue (out t);
				}, threads);
				
				Assert.IsTrue (state, "#1");
				Assert.AreEqual (delta * threads, queue.Count, "#2");
				
				string actual = string.Empty;
				int temp;
				while (queue.TryDequeue (out temp)) {
					actual += temp;
				}
				string expected = Enumerable.Range (count * threads, delta * threads)
					.Aggregate (string.Empty, (acc, v) => acc + v);
				
				Assert.AreEqual (expected, actual, "#3");
			});*/
			
			CollectionStressTestHelper.RemoveStressTest (new ConcurrentQueue<int> (), CheckOrderingType.InOrder);
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void StressTryPeekTestCase ()
		{
			ParallelTestHelper.Repeat (delegate {
				var queue = new ConcurrentQueue<object> ();
				queue.Enqueue (new object());
				
				const int threads = 10;
				int threadCounter = 0;
				bool success = true;
				
				ParallelTestHelper.ParallelStressTest (queue, (q) => {
					int threadId = Interlocked.Increment (ref threadCounter);
					object temp;
					if (threadId < threads)
					{
						while (queue.TryPeek (out temp))
							if (temp == null)
								success = false;
					} else {
						queue.TryDequeue (out temp);
					}
				}, threads);
				
				Assert.IsTrue (success, "TryPeek returned unexpected null value.");
			}, 10);
		}
		
		[Test]
		public void CountTestCase()
		{
			Assert.AreEqual(10, queue.Count, "#1");
			int value;
			queue.TryPeek(out value);
			queue.TryDequeue(out value);
			queue.TryDequeue(out value);
			Assert.AreEqual(8, queue.Count, "#2");
		}
		
		//[Ignore]
		[Test]
		public void EnumerateTestCase()
		{
			string s = string.Empty;
			foreach (int i in queue) {
				s += i;
			}
			Assert.AreEqual("0123456789", s, "#1 : " + s);
		}
		
		[Test()]
		public void TryPeekTestCase()
		{
			int value;
			queue.TryPeek(out value);
			Assert.AreEqual(0, value, "#1 : " + value);
			queue.TryDequeue(out value);
			Assert.AreEqual(0, value, "#2 : " + value);
			queue.TryDequeue(out value);
			Assert.AreEqual(1, value, "#3 : " + value);
			queue.TryPeek(out value);
			Assert.AreEqual(2, value, "#4 : " + value);
			queue.TryPeek(out value);
			Assert.AreEqual(2, value, "#5 : " + value);
		}
		
		[Test()]
		public void TryDequeueTestCase()
		{
			int value;
			queue.TryPeek(out value);
			Assert.AreEqual(0, value, "#1");
			Assert.IsTrue(queue.TryDequeue(out value), "#2");
			Assert.IsTrue(queue.TryDequeue(out value), "#3");
			Assert.AreEqual(1, value, "#4");
		}
		
		[Test()]
		public void TryDequeueEmptyTestCase()
		{
			int value;
			queue = new ConcurrentQueue<int> ();
			queue.Enqueue(1);
			Assert.IsTrue(queue.TryDequeue(out value), "#1");
			Assert.IsFalse(queue.TryDequeue(out value), "#2");
			Assert.IsTrue(queue.IsEmpty, "#3");
		}
		
		[Test]
		public void ToArrayTest()
		{
			int[] array = queue.ToArray();
			string s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.AreEqual("0123456789", s, "#1 : " + s);
			queue.CopyTo(array, 0);
			s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.AreEqual("0123456789", s, "#2 : " + s);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToExistingArray_Null ()
		{
			queue.CopyTo (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToExistingArray_OutOfRange ()
		{
			queue.CopyTo (new int[3], -1);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ToExistingArray_IndexOverflow ()
		{
			queue.CopyTo (new int[3], 4);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ToExistingArray_Overflow ()
		{
			queue.CopyTo (new int[3], 0);
		}

		static WeakReference CreateWeakReference (object obj)
		{
			return new WeakReference (obj);
		}

		[Test]
		// This depends on precise stack scanning
		[Category ("NotWorking")]
		public void TryDequeueReferenceTest ()
		{
			var obj = new Object ();
			var weakReference = CreateWeakReference(obj);
			var queue = new ConcurrentQueue<object> ();

			queue.Enqueue (obj);
			queue.TryDequeue (out obj);
			obj = null;

			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			Assert.IsFalse (weakReference.IsAlive);
		}
	}
}
