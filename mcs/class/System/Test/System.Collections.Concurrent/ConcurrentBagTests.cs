// 
// ConcurrentBagTests.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Threading;
using System.Linq;

using NUnit;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture]
	public class ConcurrentBagTests
	{
		ConcurrentBag<int> bag;
		
		[SetUp]
		public void Setup ()
		{
			bag = new ConcurrentBag<int> ();
		}

		[Test]
		public void BasicAddTakeTest ()
		{
			bag.Add (1);
			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (1, bag.Count);

			var array = bag.ToArray ();
			Assert.AreEqual (1, array.Length);
			Assert.AreEqual (1, array[0]);

			int result;
			Assert.IsTrue (bag.TryTake (out result));
			Assert.AreEqual (1, result);
			Assert.IsTrue (bag.IsEmpty);
		}

		[Test]
		public void BasicAddTakeFromOtherThread ()
		{
			var t = new Thread (() => bag.Add (1));
			t.Start ();
			Assert.IsTrue (t.Join (300));

			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (1, bag.Count);

			var array = bag.ToArray ();
			Assert.AreEqual (1, array.Length);
			Assert.AreEqual (1, array[0]);

			int result;
			Assert.IsTrue (bag.TryTake (out result));
			Assert.AreEqual (1, result);
			Assert.IsTrue (bag.IsEmpty);
		}

		[Test]
		public void AddFromMultipleThreadTakeFromOneThread ()
		{
			var threads = new Thread[10];
			for (int i = 0; i < threads.Length; i++) {
				threads[i] = new Thread (() => bag.Add (1));
				threads[i].Start ();
			}
			foreach (var t in threads)
				Assert.IsTrue (t.Join (2000));

			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (threads.Length, bag.Count);

			var array = bag.ToArray ();
			Assert.AreEqual (threads.Length, array.Length);

			Assert.That (array, new CollectionEquivalentConstraint (Enumerable.Repeat (1, 10).ToArray ()), "#1, same");

			int result;
			for (int i = 0; i < threads.Length; i++) {
				Assert.IsTrue (bag.TryTake (out result));
				Assert.AreEqual (1, result);
			}
			Assert.IsTrue (bag.IsEmpty);
		}

		[Test]
		public void AddFromOneThreadTakeFromMultiple ()
		{
			var threads = new Thread[10];
			for (int i = 0; i < threads.Length; i++)
				bag.Add (1);

			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (threads.Length, bag.Count);

			bool valid = true;

			for (int i = 0; i < threads.Length; i++) {
				int result;
				threads[i] = new Thread (() => valid &= bag.TryTake (out result) && result == 1);
				threads[i].Start ();
			}

			foreach (var t in threads)
				Assert.IsTrue (t.Join (200));

			Assert.IsTrue (valid, "Aggregate test");
		}

		[Test]
		public void BasicAddPeekTest ()
		{
			bag.Add (1);
			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (1, bag.Count);

			int result;
			Assert.IsTrue (bag.TryPeek (out result));
			Assert.AreEqual (1, result);
			Assert.IsFalse (bag.IsEmpty);
		}

		[Test]
		public void BasicAddPeekFromOtherThread ()
		{
			var t = new Thread (() => bag.Add (1));
			t.Start ();
			Assert.IsTrue (t.Join (300));

			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (1, bag.Count);

			int result;
			Assert.IsTrue (bag.TryPeek (out result));
			Assert.AreEqual (1, result);
			Assert.IsFalse (bag.IsEmpty);
		}

		[Test]
		public void AddFromOneThreadPeekFromMultiple ()
		{
			var threads = new Thread[10];
			for (int i = 0; i < threads.Length; i++)
				bag.Add (1);

			Assert.IsFalse (bag.IsEmpty);
			Assert.AreEqual (threads.Length, bag.Count);

			bool valid = true;

			for (int i = 0; i < threads.Length; i++) {
				int result;
				threads[i] = new Thread (() => valid &= bag.TryPeek (out result) && result == 1);
				threads[i].Start ();
			}

			foreach (var t in threads)
				Assert.IsTrue (t.Join (200));

			Assert.IsTrue (valid, "Aggregate test");
		}

        [Test]
        public void BasicRemoveEmptyTest ()
        {
            int result;
            Assert.IsTrue(bag.IsEmpty);
            Assert.IsFalse(bag.TryTake(out result));
        }

        [Test]
        public void BasicRemoveTwiceTest()
        {
            bag.Add (1);
            Assert.IsFalse (bag.IsEmpty);
            Assert.AreEqual (1, bag.Count);

            int result;
            Assert.IsTrue (bag.TryTake (out result));
            Assert.AreEqual (1, result);
            Assert.IsTrue (bag.IsEmpty);
            Assert.IsFalse (bag.TryTake (out result));
            Assert.IsFalse (bag.TryTake (out result));
        }

        [Test]
        public void AddRemoveAddTest()
        {
            bag.Add (1);
            Assert.IsFalse (bag.IsEmpty);
            Assert.AreEqual (1, bag.Count);

            int result;
            Assert.IsTrue (bag.TryTake (out result));
            Assert.AreEqual (1, result);
            Assert.IsTrue (bag.IsEmpty);

            bag.Add (1);
            Assert.IsFalse (bag.IsEmpty);
            Assert.AreEqual (1, bag.Count);

            Assert.IsTrue (bag.TryTake (out result));
            Assert.AreEqual (1, result);
            Assert.IsTrue (bag.IsEmpty);
        }
		
		[Test]
		public void AddStressTest ()
		{
			CollectionStressTestHelper.AddStressTest (bag);
		}
		
		[Test]
		public void RemoveStressTest ()
		{
			CollectionStressTestHelper.RemoveStressTest (bag, CheckOrderingType.DontCare);
		}

		[Test]
		public void Bug24213 ()
		{
			var size = 2049;
			var bag = new ConcurrentBag<int> ();
			for (int i = 0; i < size; i++)
				bag.Add (i);

			var array = bag.ToArray ();

			Assert.AreEqual (size, array.Length);

			for (int i = 0; i < size; i++)
				Assert.AreEqual (i, array [i]);
		}
	}
}
#endif
