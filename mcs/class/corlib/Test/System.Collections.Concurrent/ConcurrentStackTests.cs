// ConcurrentStackTests.cs
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
using System.Threading;
using System.Linq;
using System.Collections.Concurrent;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture()]
	public class ConcurrentStackTests
	{
		ConcurrentStack<int> stack;
		
		[SetUpAttribute]
		public void Setup()
		{
			stack = new ConcurrentStack<int>();
			for (int i = 0; i < 10; i++) {
				stack.Push(i);
			}
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void StressPushTestCase ()
		{
			/*ParallelTestHelper.Repeat (delegate {
				stack = new ConcurrentStack<int> ();
				int amount = -1;
				const int count = 10;
				const int threads = 5;
				
				ParallelTestHelper.ParallelStressTest (stack, (q) => {
					int t = Interlocked.Increment (ref amount);
					for (int i = 0; i < count; i++)
						stack.Push (t);
				}, threads);
				
				Assert.AreEqual (threads * count, stack.Count, "#-1");
				int[] values = new int[threads];
				int temp;
				while (stack.TryPop (out temp)) {
					values[temp]++;
				}
				
				for (int i = 0; i < threads; i++)
					Assert.AreEqual (count, values[i], "#" + i);
			});*/
			CollectionStressTestHelper.AddStressTest (new ConcurrentStack<int> ());
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void StressPopTestCase ()
		{
			/*ParallelTestHelper.Repeat (delegate {
				stack = new ConcurrentStack<int> ();
				const int count = 10;
				const int threads = 5;
				const int delta = 5;
				
				for (int i = 0; i < (count + delta) * threads; i++)
					stack.Push (i);
				
				bool state = true;
				
				ParallelTestHelper.ParallelStressTest (stack, (q) => {
					int t;
					for (int i = 0; i < count; i++)
						state &= stack.TryPop (out t);
				}, threads);
				
				Assert.IsTrue (state, "#1");
				Assert.AreEqual (delta * threads, stack.Count, "#2");
				
				string actual = string.Empty;
				int temp;
				while (stack.TryPop (out temp)) {
					actual += temp;
				}
				string expected = Enumerable.Range (0, delta * threads).Reverse()
					.Aggregate (string.Empty, (acc, v) => acc + v);
				
				Assert.AreEqual (expected, actual, "#3");
			});*/
			
			CollectionStressTestHelper.RemoveStressTest (new ConcurrentStack<int> (), CheckOrderingType.Reversed);
		}
		
		[Test]
		public void CountTestCase()
		{
			Assert.IsTrue(stack.Count == 10, "#1");
			int value;
			stack.TryPeek(out value);
			stack.TryPop(out value);
			stack.TryPop(out value);
			Assert.IsTrue(stack.Count == 8, "#2");
			stack.Clear();
			Assert.IsTrue(stack.Count == 0, "#3");
			Assert.IsTrue(stack.IsEmpty, "#4");
		}
		
		[Test()]
		public void EnumerateTestCase()
		{
			string s = string.Empty;
			foreach (int i in stack) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
		}
		
		[Test()]
		public void TryPeekTestCase()
		{
			int value;
			stack.TryPeek(out value);
			Assert.IsTrue(value == 9, "#1 : " + value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 9, "#2 : " + value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 8, "#3 : " + value);
			stack.TryPeek(out value);
			Assert.IsTrue(value == 7, "#4 : " + value);
			stack.TryPeek(out value);
			Assert.IsTrue(value == 7, "#5 : " + value);
		}
		
		[Test()]
		public void TryPopTestCase()
		{
			int value;
			stack.TryPeek(out value);
			Assert.IsTrue(value == 9, "#1");
			stack.TryPop(out value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 8, "#2 : " + value);
		}
		
		[Test()]
		public void TryPopEmptyTestCase()
		{
			int value;
			stack.Clear();
			stack.Push(1);
			Assert.IsTrue(stack.TryPop(out value), "#1");
			Assert.IsFalse(stack.TryPop(out value), "#2");
			Assert.IsTrue(stack.IsEmpty, "#3");
		}
		
		[Test]
		public void ToArrayTest()
		{
			int[] array = stack.ToArray();
			string s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
			stack.CopyTo(array, 0);
			s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ToExistingArray_Null ()
		{
			stack.CopyTo (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToExistingArray_OutOfRange ()
		{
			stack.CopyTo (new int[3], -1);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ToExistingArray_IndexOverflow ()
		{
			stack.CopyTo (new int[3], 4);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ToExistingArray_Overflow ()
		{
			stack.CopyTo (new int[3], 0);
		}

		[Test]
		public void TryPopRangeTest ()
		{
			int[] values = new int[3];
			Assert.AreEqual (3, stack.TryPopRange (values));
			Assert.That (values, new CollectionEquivalentConstraint (new int[] { 9, 8, 7 }));
			Assert.AreEqual (10 - values.Length, stack.Count);
			for (int i = 9 - values.Length; i >= 0; i--) {
				int outValue;
				Assert.IsTrue (stack.TryPop (out outValue));
				Assert.AreEqual (i, outValue);
			}
		}

		[Test]
		public void TryPopRangeEmpty ()
		{
			stack = new ConcurrentStack<int>();
			Assert.AreEqual (0, stack.TryPopRange (new int [1]));
		}

		[Test]
		public void TryPopRangeTestWithOneElement ()
		{
			int[] values = new int[1];
			Assert.AreEqual (1, stack.TryPopRange (values));
			Assert.That (values, new CollectionEquivalentConstraint (new int[] { 9 }));
			Assert.AreEqual (10 - values.Length, stack.Count);
			for (int i = 9 - values.Length; i >= 0; i--) {
				int outValue;
				Assert.IsTrue (stack.TryPop (out outValue));
				Assert.AreEqual (i, outValue);
			}
		}

		[Test]
		public void TryPopRangeFullTest ()
		{
			int[] values = new int[10];
			Assert.AreEqual (10, stack.TryPopRange (values));
			Assert.That (values, new CollectionEquivalentConstraint (Enumerable.Range (0, 10).Reverse ()));
			Assert.AreEqual (0, stack.Count);
		}

		[Test]
		public void TryPopRangePartialFillTest ()
		{
			int[] values = new int[5];
			Assert.AreEqual (2, stack.TryPopRange (values, 3, 2));
			Assert.That (values, new CollectionEquivalentConstraint (new int[] { 0, 0, 0, 9, 8 }));
			Assert.AreEqual (8, stack.Count);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryPopRange_NegativeIndex ()
		{
			stack.TryPopRange (new int[3], -2, 3);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryPopRange_LargeIndex ()
		{
			stack.TryPopRange (new int[3], 200, 3);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void TryPopRange_LargeCount ()
		{
			stack.TryPopRange (new int[3], 2, 5);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TryPopRange_NullArray ()
		{
			stack.TryPopRange (null);
		}
		
		[Test]
		public void PushRangeTestCase()
		{
			var testStack = new ConcurrentStack<int>();
			
			var testData = new int[] { 1, 2, 3, 4, 5 };			
			testStack.PushRange (testData);
			
			Assert.AreEqual (testData.Length, testStack.Count);
		}
	}
}
