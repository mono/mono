/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using C5;
using NUnit.Framework;
using MSG = System.Collections.Generic;

namespace nunit.heaps
{

	[TestFixture]
	public class IntervalHeapTests
	{
		IPriorityQueue<int> queue;


		[SetUp]
		public void Init() { queue = new IntervalHeap<int>(); }


		[TearDown]
		public void Dispose() { queue = null; }


        [Test]
        public void Simple()
        {
            Assert.IsTrue(queue.AllowsDuplicates);
            Assert.AreEqual(0, queue.Count);
            queue.Add(8); queue.Add(18); queue.Add(8); queue.Add(3);
            Assert.AreEqual(4, queue.Count);
            Assert.AreEqual(18, queue.DeleteMax());
            Assert.AreEqual(3, queue.Count);
            Assert.AreEqual(3, queue.DeleteMin());
            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(8, queue.FindMax());
            Assert.AreEqual(8, queue.DeleteMax());
            Assert.AreEqual(8, queue.FindMax());
            queue.Add(15);
            Assert.AreEqual(15, queue.FindMax());
            Assert.AreEqual(8, queue.FindMin());
            Assert.IsNotNull(queue.SyncRoot);
            Assert.IsTrue(queue.Comparer.Compare(2, 3) < 0);
            Assert.IsTrue(queue.Comparer.Compare(4, 3) > 0);
            Assert.IsTrue(queue.Comparer.Compare(3, 3) == 0);

        }


        [Test]
        public void Enumerate()
        {
            int[] a = new int[4];
            int siz = 0;
            foreach (int i in queue)
                siz++;
            Assert.AreEqual(0, siz);

            queue.Add(8); queue.Add(18); queue.Add(8); queue.Add(3);

            foreach (int i in queue)
                a[siz++] = i;
            Assert.AreEqual(4, siz);
            Array.Sort(a,0,siz);
            Assert.AreEqual(3, a[0]);
            Assert.AreEqual(8, a[1]);
            Assert.AreEqual(8, a[2]);
            Assert.AreEqual(18, a[3]);

            siz = 0;
            Assert.AreEqual(18, queue.DeleteMax());
            foreach (int i in queue)
                a[siz++] = i;
            Assert.AreEqual(3, siz);
            Array.Sort(a, 0, siz);
            Assert.AreEqual(3, a[0]);
            Assert.AreEqual(8, a[1]);
            Assert.AreEqual(8, a[2]);

            siz = 0;
            Assert.AreEqual(8, queue.DeleteMax());
            foreach (int i in queue)
                a[siz++] = i;
            Assert.AreEqual(2, siz);
            Array.Sort(a, 0, siz);
            Assert.AreEqual(3, a[0]);
            Assert.AreEqual(8, a[1]);

            siz = 0;
            Assert.AreEqual(8, queue.DeleteMax());
            foreach (int i in queue)
                a[siz++] = i;
            Assert.AreEqual(1, siz);
            Assert.AreEqual(3, a[0]);
        }

        [Test]
        public void Random()
		{
			int length = 1000;
			int[] a = new int[length];
			Random ran = new Random(6754);

			for (int i = 0; i < length; i++)
				queue.Add(a[i] = ran.Next());

			Assert.IsTrue(queue.Check());
			Array.Sort(a);
			for (int i = 0; i < length / 2; i++)
			{
				Assert.AreEqual(a[length - i - 1], queue.DeleteMax());
				Assert.IsTrue(queue.Check());
				Assert.AreEqual(a[i], queue.DeleteMin());
				Assert.IsTrue(queue.Check());
			}

			Assert.IsTrue(queue.IsEmpty);
		}


		[Test]
		public void RandomDuplicates()
		{
			int length = 1000;
			int s;
			int[] a = new int[length];
			Random ran = new Random(6754);

			for (int i = 0; i < length; i++)
				queue.Add(a[i] = ran.Next(3, 13));
			Assert.IsTrue(queue.Check());

			Array.Sort(a);

			for (int i = 0; i < length/2; i++)
			{
				Assert.AreEqual(a[i], queue.DeleteMin());
				Assert.IsTrue(queue.Check());
				Assert.AreEqual(a[length - i - 1], s = queue.DeleteMax());
				Assert.IsTrue(queue.Check());
			}

			Assert.IsTrue(queue.IsEmpty);
		}


		[Test]
		public void AddAll()
		{
			int length = 1000;
			int[] a = new int[length];
			Random ran = new Random(6754);

			LinkedList<int> lst = new LinkedList<int>();
			for (int i = 0; i < length; i++)
				lst.Add(a[i] = ran.Next());

			queue.AddAll(lst);
			Assert.IsTrue(queue.Check());
			Array.Sort(a);
			for (int i = 0; i < length / 2; i++)
			{
				Assert.AreEqual(a[length - i - 1], queue.DeleteMax());
				Assert.IsTrue(queue.Check());
				Assert.AreEqual(a[i], queue.DeleteMin());
				Assert.IsTrue(queue.Check());
			}

			Assert.IsTrue(queue.IsEmpty);
		}
	}
}