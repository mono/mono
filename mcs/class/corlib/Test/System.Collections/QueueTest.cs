//
// System.Collections.QueueTest
// Test suite for System.Collections.Queue
//
// Author:
//    Ricardo Fernández Pascual
//
// (C) 2001 Ricardo Fernández Pascual
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections {

	[TestFixture]
	public class QueueTest {

		protected Queue q1;
		protected Queue q2;
		protected Queue emptyQueue;

		[SetUp]
		protected void SetUp () 
		{
			q1 = new Queue (10);
			for (int i = 0; i < 100; i++)
				q1.Enqueue (i);
			
			q2 = new Queue (50, 1.5f);
			for (int i = 50; i < 100; i++)
				q2.Enqueue (i);

			emptyQueue = new Queue ();
		}

		[Test]
		public void TestConstructorException1 () 
		{
			try 
			{
				Queue q = new Queue(-1, 2);
				Assert.Fail ("Should throw an exception");
			} catch (ArgumentOutOfRangeException e) {
				Assert.AreEqual ("capacity", e.ParamName, "Exception's ParamName must be \"capacity\"");
			}
		}

		[Test]
		public void TestConstructorException2 () 
		{
			try 
			{
				Queue q = new Queue(10, 0);
				Assert.Fail ("Should throw an exception because growFactor < 1");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				Assert.AreEqual ("growFactor", e.ParamName, "Exception's ParamName must be \"growFactor\"");
			}
		}

		[Test]
		public void TestConstructorException3 () 
		{
			try 
			{
				Queue q = new Queue(10, 11);
				Assert.Fail ("Should throw an exception because growFactor > 10");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				Assert.AreEqual ("growFactor", e.ParamName, "Exception's ParamName must be \"growFactor\"");
			}
		}

		[Test]
		public void TestConstructorException4 () 
		{
			try 
			{
				Queue q = new Queue(null);
				Assert.Fail ("Should throw an exception because col == null");
			} 
			catch (ArgumentNullException e) 
			{
				Assert.AreEqual ("col", e.ParamName, "Exception's ParamName must be \"col\"");
			}
		}

		[Test]
		public void TestICollectionConstructor () 
		{
			Queue q = new Queue(new int[] {1, 2, 3, 4, 5});
			Assert.AreEqual (5, q.Count, "count");
			for (int i=1; i <=5; i++) 
			{
				Assert.AreEqual (q.Dequeue(), i);
			}
                }

		[Test]
		public void TestConstructors () 
		{
			Assert.IsTrue (q1.Count == 100);
			Assert.IsTrue (q2.Count == 50);
			Assert.IsTrue (emptyQueue.Count == 0);
		}

		[Test]
		public void TestCount() 
		{
			Assert.AreEqual (100, q1.Count, "Count #1");
			for (int i = 1; i <=50; i ++) 
			{
				q1.Dequeue();
			}
			Assert.AreEqual (50, q1.Count, "Count #2");
			for (int i = 1; i <=50; i ++) 
			{
				q1.Enqueue(i);
			}
			Assert.AreEqual (100, q1.Count, "Count #3");

			Assert.AreEqual (50, q2.Count, "Count #4");

			Assert.AreEqual (0, emptyQueue.Count, "Count #5");
		}

		[Test]
		public void TestIsSynchronized() 
		{
			Assert.IsTrue (!q1.IsSynchronized, "IsSynchronized should be false");
			Assert.IsTrue (!q2.IsSynchronized, "IsSynchronized should be false");
			Assert.IsTrue (!emptyQueue.IsSynchronized, "IsSynchronized should be false");
		}

		[Test]
		public void TestSyncRoot() 
		{
#if !NET_2_0 // umm, why on earth do you expect SyncRoot is the Queue itself?
			Assert.AreEqual (q1, q1.SyncRoot, "SyncRoot q1");
			Assert.AreEqual (q2, q2.SyncRoot, "SyncRoot q2");
			Assert.AreEqual (emptyQueue, emptyQueue.SyncRoot, "SyncRoot emptyQueue");
#endif

			Queue q1sync = Queue.Synchronized(q1);
			Assert.AreEqual (q1, q1sync.SyncRoot, "SyncRoot value of a synchronized queue");
		}

		[Test]
		public void TestCopyToException1 () 
		{
			try 
			{
				q1.CopyTo(null, 1);
				Assert.Fail ("must throw ArgumentNullException");
			} catch (ArgumentNullException e) {
				Assert.AreEqual ("array", e.ParamName, "Exception's ParamName must be \"array\"");
			}
		}


		[Test]
		public void TestCopyToException2 () 
		{
			try 
			{
				q1.CopyTo(new int[2,2], 1);
				Assert.Fail ("must throw ArgumentException");
			} 
			catch (ArgumentException) 
			{
			}
		}

		[Test]
		public void TestCopyToException3 () 
		{
			try 
			{
				q1.CopyTo(new int[3], -1);
				Assert.Fail ("must throw ArgumentOutOfRangeException");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				Assert.AreEqual ("index", e.ParamName, "Exception's ParamName must be \"index\"");
			}
		}

		[Test]
		public void TestCopyToException4 () 
		{
			try 
			{
				q1.CopyTo(new int[3], 1);
				Assert.Fail ("must throw ArgumentException");
			} 
			catch (ArgumentException) {}
		}


		[Test]
		public void TestCopyTo () 
		{
			int[] a1 = new int[100];
			int[] a2 = new int[60];

			string progress_marker = "";
			try {
				progress_marker = "before first CopyTo";
				q1.CopyTo (a1, 0);
				for (int i = 0; i < 100; i++)
					Assert.AreEqual (a1[i], i);

				// Remove some items from q2 and add other 
				// items, to avoid having  an "easy" just created
				// Queue
				for (int i = 50; i < 60; i++)
					Assert.IsTrue (i == (int) q2.Dequeue ());
				for (int i = 100; i < 110; i++)
					q2.Enqueue (i);
				
				progress_marker = "before second CopyTo";
				q2.CopyTo (a2, 10);
				for (int i = 60; i < 110; i++)
					Assert.IsTrue (i == a2[i - 60 + 10]);
				
				// Copying an empty Queue should not modify the array
				progress_marker = "before third CopyTo";
				emptyQueue.CopyTo (a2, 10);
				for (int i = 60; i < 110; i++)
					Assert.IsTrue (i == a2[i - 60 + 10]);
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at marker <" + progress_marker + ">: e = " + e);
			}

		}

		[Test]
		public void TestEnumerator () {
			int i;
			IEnumerator e;
			e = q1.GetEnumerator ();
			i = 0;
			while (e.MoveNext ()) {
				Assert.AreEqual (i, ((int) e.Current), "q1 at i=" + i);
				i++;
			}
			e = q2.GetEnumerator ();
			i = 50;
			while (e.MoveNext ()) {
				Assert.AreEqual (((int) e.Current), i);
				i++;
			}
			e = emptyQueue.GetEnumerator ();
			if (e.MoveNext ())
				Assert.Fail ("Empty Queue enumerator returning elements!");

			e = q1.GetEnumerator ();
			try {
				e.MoveNext ();
				q1.Enqueue (0);
				e.MoveNext ();
				Assert.Fail ("#1 Should have thrown InvalidOperationException");
			} catch	(InvalidOperationException) { }
			e = q1.GetEnumerator ();
		}

		[Test]
		public void TestEnumeratorException1 () 
		{
			IEnumerator e;

			e = q1.GetEnumerator();
			q1.Enqueue(6);
			try {
				e.MoveNext();
				Assert.Fail ("MoveNext must throw InvalidOperationException after Enqueue");
			} catch (InvalidOperationException) {}


			e = q1.GetEnumerator();
			q1.Enqueue(6);
			try 
			{
				e.Reset();
				Assert.Fail ("Reset must throw InvalidOperationException after Enqueue");
			} 
			catch (InvalidOperationException) {}

			e = q1.GetEnumerator();
			q1.TrimToSize();
			try 
			{
				e.Reset();
				Assert.Fail ("Reset must throw InvalidOperationException after TrimToSize");
			} 
			catch (InvalidOperationException) {}

		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EnumeratorCurrentAfterMoveNextAll () 
		{
			IEnumerator e = q1.GetEnumerator();
			while (e.MoveNext ()) {
			}
			Assert.IsNotNull (e.Current);
		}

		[Test]
		public void EnumeratorFalseAfterMoveNextAll () 
		{
			IEnumerator e = q1.GetEnumerator();
			while (e.MoveNext ()) {
			}
			Assert.IsTrue (!e.MoveNext ());
		}

		[Test]
		public void TestClone () {
			Queue q3 = (Queue) q2.Clone ();
			Assert.IsTrue (q3.Count == q2.Count);
			for (int i = 0; i < 50; i++)
				Assert.IsTrue (q2.Dequeue ().Equals (q3.Dequeue ()));
			Assert.IsTrue (q3.Count == 0);
			Assert.IsTrue (q2.Count == 0);
		}

		[Test]
		public void TestClear () {
			q1.Clear ();
			Assert.IsTrue (q1.Count == 0);
			q2.Clear ();
			Assert.IsTrue (q2.Count == 0);
			emptyQueue.Clear ();
			Assert.IsTrue (emptyQueue.Count == 0);
		}

		[Test]
		public void TestContains () {
			for (int i = 0; i < 100; i++) {
				Assert.IsTrue (q1.Contains (i));
				Assert.IsTrue (!emptyQueue.Contains (i));
				if (i < 50)
					Assert.IsTrue (!q2.Contains (i));
				else
					Assert.IsTrue (q2.Contains (i));
			}
			
			Assert.IsTrue (!q1.Contains(null), "q1 does not contain null");
			q1.Enqueue(null);
			Assert.IsTrue (q1.Contains(null), "q1 contains null");
		}
		
		[Test]
		public void TestEnqueueDequeuePeek () {
			int q1size = q1.Count;
			int q2size = q2.Count;
			q2.Enqueue (null);
			Assert.IsTrue (q2.Count == ++q2size);
			for (int i = 0; i < 50; i++) {
				int k = (int) q1.Peek ();
				Assert.IsTrue (q1.Count == q1size);
				int j = (int) q1.Dequeue ();
				Assert.IsTrue (q1.Count == --q1size);
				Assert.IsTrue (i == j);
				Assert.IsTrue (j == k);
				q2.Enqueue (j);
				Assert.IsTrue (q2.Count == ++q2size);
			}
			for (int i = 50; i < 100; i++) {
				Assert.IsTrue (((int) q2.Dequeue ()) == i);
				Assert.IsTrue (q2.Count == --q2size);
			}
			Assert.IsTrue (q2.Peek () == null);
			Assert.IsTrue (q2.Dequeue () == null);
			Assert.IsTrue (q2.Count == --q2size);
			for (int i = 0; i < 50; i++) {
				Assert.IsTrue (((int) q2.Dequeue ()) == i);
				Assert.IsTrue (q2.Count == --q2size);
			}
		}
		
		[Test]
		public void TestDequeue() {
			Queue queue = new Queue();
			string[] tmp = new string[50];
			int i;
			for (i=0;i<50;i++) {
				tmp[i] = "Data #" + i;
				queue.Enqueue(tmp[i]);
			}
			
			i = 0;
			while(queue.Count>0){
				string z = (string) queue.Dequeue();
				Assert.AreEqual (tmp[i], z, tmp[i]);
				i++;
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestDequeueEmpty() 
		{
			Queue q= new Queue();
			q.Dequeue();
		}

		[Test]
		public void TestToArray() 
		{
			object[] a = q1.ToArray();
			for (int i = 0; i < 100; i++) 
			{
				Assert.AreEqual (q1.Dequeue(), (int) a[i], "Queue-Array mismatch");
			}

			object[] b = emptyQueue.ToArray();
			Assert.AreEqual (0, b.Length, "b should be a zero-lenght array"); 
		}

		[Test]
		public void TestTrimToSize() 
		{
			for (int i=0; i < 50; i++) 
			{
				q1.Dequeue();
			}
			q1.TrimToSize();
			// FIXME: I can't figure out how to test if TrimToSize actually worked!
		}

		// TODO: test Syncronized operation

		[Test]
		public void TestSynchronizedException() 
		{
			try 
			{
				Queue.Synchronized(null);
				Assert.Fail ("Must throw ArgumentNullException");
			} 
			catch (ArgumentNullException e)
			{
				Assert.AreEqual ("queue", e.ParamName, "Exception's ParamName must be \"queue\"");
			}
		}
		
		[Test]
		public void TestAlwaysGrows() 
		{
			// In bug #61919 the grow () method might not always grow (if the size
			// was 0, or due to rounding).
			Queue queue = new Queue (new Queue());
			queue.Enqueue(1);
		}

		[Test]
		public void SynchronizedClone () 
		{
			Queue q1sync = Queue.Synchronized (q1);
			Assert.IsTrue (q1sync.IsSynchronized, "q1sync.IsSyncronized"); 
			Assert.AreEqual (q1.Count, q1sync.Count, "q1sync.Count");

			Queue q1syncsync = Queue.Synchronized (q1sync);
			Assert.IsTrue (q1syncsync.IsSynchronized, "q1syncsync must be synchronized too");
			Assert.AreEqual (q1.Count, q1syncsync.Count, "q1syncsync.Count");

			Queue q1syncclone = (Queue) q1sync.Clone();
			Assert.IsTrue (q1syncclone.IsSynchronized, "clone must be synchronized too");
			Assert.AreEqual (q1.Count, q1syncclone.Count, "q1syncclone.Count");
		}

		[Test]		
		public void TestICollectionCtorUsesEnum ()
		{
			BitArray x = new BitArray (10, true);
			Queue s = new Queue (x);
		}

		// https://bugzilla.novell.com/show_bug.cgi?id=321657
		[Test]
		public void TrimToSize_Dequeue_Enqueue ()
		{
			Queue queue = new Queue (32, 1.0F);
			for (int i = 0; i < 31; i++)
				queue.Enqueue (i);

			queue.TrimToSize ();
			Assert.AreEqual (0, queue.Dequeue (), "0");
			queue.Enqueue (411);

			Assert.AreEqual (31, queue.Count, "Count-1");
			for (int i = 1; i < 31; i++) {
				Assert.AreEqual (i, queue.Peek (), "Peek" + i.ToString ());
				Assert.AreEqual (i, queue.Dequeue (), "Dequeue" + i.ToString ());
			}
			Assert.AreEqual (1, queue.Count, "Count-2");
			Assert.AreEqual (411, queue.Dequeue (), "411");
		}

		[Test]
		public void TrimToSize_Enqueue_Dequeue ()
		{
			Queue queue = new Queue (32, 1.0F);
			for (int i = 0; i < 31; i++)
				queue.Enqueue (i);

			queue.TrimToSize ();
			queue.Enqueue (411);
			Assert.AreEqual (32, queue.Count, "Count-1");
			Assert.AreEqual (0, queue.Dequeue (), "0");
		}
	}
}

