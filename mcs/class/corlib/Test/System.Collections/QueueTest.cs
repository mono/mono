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
	public class QueueTest : Assertion {

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

		public void TestConstructorException1 () 
		{
			try 
			{
				Queue q = new Queue(-1, 2);
				Fail("Should throw an exception");
			} catch (ArgumentOutOfRangeException e) {
				AssertEquals("Exception's ParamName must be \"capacity\"", "capacity", e.ParamName);
			}
		}

		public void TestConstructorException2 () 
		{
			try 
			{
				Queue q = new Queue(10, 0);
				Fail("Should throw an exception because growFactor < 1");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				AssertEquals("Exception's ParamName must be \"growFactor\"", "growFactor", e.ParamName);
			}
		}

		public void TestConstructorException3 () 
		{
			try 
			{
				Queue q = new Queue(10, 11);
				Fail("Should throw an exception because growFactor > 10");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				AssertEquals("Exception's ParamName must be \"growFactor\"", "growFactor", e.ParamName);
			}
		}

		public void TestConstructorException4 () 
		{
			try 
			{
				Queue q = new Queue(null);
				Fail("Should throw an exception because col == null");
			} 
			catch (ArgumentNullException e) 
			{
				AssertEquals("Exception's ParamName must be \"col\"", "col", e.ParamName);
			}
		}

		public void TestICollectionConstructor () 
		{
			Queue q = new Queue(new int[] {1, 2, 3, 4, 5});
			AssertEquals("count", 5, q.Count);
			for (int i=1; i <=5; i++) 
			{
				AssertEquals(i,	q.Dequeue());
			}
                }

		public void TestConstructors () 
		{
			SetUp();
			Assert (q1.Count == 100);
			Assert (q2.Count == 50);
			Assert (emptyQueue.Count == 0);
		}

		public void TestCount() 
		{
			SetUp();

			AssertEquals("Count #1", 100, q1.Count);
			for (int i = 1; i <=50; i ++) 
			{
				q1.Dequeue();
			}
			AssertEquals("Count #2", 50, q1.Count);
			for (int i = 1; i <=50; i ++) 
			{
				q1.Enqueue(i);
			}
			AssertEquals("Count #3", 100, q1.Count);

			AssertEquals("Count #4", 50, q2.Count);

			AssertEquals("Count #5", 0, emptyQueue.Count);
		}

		public void TestIsSynchronized() 
		{
			SetUp();
			Assert("IsSynchronized should be false", !q1.IsSynchronized);
			Assert("IsSynchronized should be false", !q2.IsSynchronized);
			Assert("IsSynchronized should be false", !emptyQueue.IsSynchronized);
		}

		public void TestSyncRoot() 
		{
			SetUp();
#if !NET_2_0 // umm, why on earth do you expect SyncRoot is the Queue itself?
			AssertEquals("SyncRoot q1", q1, q1.SyncRoot);
			AssertEquals("SyncRoot q2", q2, q2.SyncRoot);
			AssertEquals("SyncRoot emptyQueue", emptyQueue, emptyQueue.SyncRoot);
#endif

			Queue q1sync = Queue.Synchronized(q1);
			AssertEquals("SyncRoot value of a synchronized queue", q1, q1sync.SyncRoot);
		}

		public void TestCopyToException1 () 
		{
			SetUp();
			try 
			{
				q1.CopyTo(null, 1);
				Fail("must throw ArgumentNullException");
			} catch (ArgumentNullException e) {
				AssertEquals("Exception's ParamName must be \"array\"", "array", e.ParamName);
			}
		}


		public void TestCopyToException2 () 
		{
			SetUp();
			try 
			{
				q1.CopyTo(new int[2,2], 1);
				Fail("must throw ArgumentException");
			} 
			catch (ArgumentException) 
			{
			}
		}

		public void TestCopyToException3 () 
		{
			SetUp();
			try 
			{
				q1.CopyTo(new int[3], -1);
				Fail("must throw ArgumentOutOfRangeException");
			} 
			catch (ArgumentOutOfRangeException e) 
			{
				AssertEquals("Exception's ParamName must be \"index\"", "index", e.ParamName);
			}
		}

		public void TestCopyToException4 () 
		{
			SetUp();
			try 
			{
				q1.CopyTo(new int[3], 1);
				Fail("must throw ArgumentException");
			} 
			catch (ArgumentException) {}
		}


		public void TestCopyTo () 
		{
			SetUp();

			int[] a1 = new int[100];
			int[] a2 = new int[60];

			string progress_marker = "";
			try {
				progress_marker = "before first CopyTo";
				q1.CopyTo (a1, 0);
				for (int i = 0; i < 100; i++)
					AssertEquals (i, a1[i]);

				// Remove some items from q2 and add other 
				// items, to avoid having  an "easy" just created
				// Queue
				for (int i = 50; i < 60; i++)
					Assert (i == (int) q2.Dequeue ());
				for (int i = 100; i < 110; i++)
					q2.Enqueue (i);
				
				progress_marker = "before second CopyTo";
				q2.CopyTo (a2, 10);
				for (int i = 60; i < 110; i++)
					Assert (i == a2[i - 60 + 10]);
				
				// Copying an empty Queue should not modify the array
				progress_marker = "before third CopyTo";
				emptyQueue.CopyTo (a2, 10);
				for (int i = 60; i < 110; i++)
					Assert (i == a2[i - 60 + 10]);
			} catch (Exception e) {
				Fail ("Unexpected exception at marker <" + progress_marker + ">: e = " + e);
			}

		}

		public void TestEnumerator () {
			SetUp();
			int i;
			IEnumerator e;
			e = q1.GetEnumerator ();
			i = 0;
			while (e.MoveNext ()) {
				AssertEquals ("q1 at i=" + i, i, ((int) e.Current));
				i++;
			}
			e = q2.GetEnumerator ();
			i = 50;
			while (e.MoveNext ()) {
				AssertEquals (i, ((int) e.Current));
				i++;
			}
			e = emptyQueue.GetEnumerator ();
			if (e.MoveNext ())
				Fail ("Empty Queue enumerator returning elements!");

			e = q1.GetEnumerator ();
			try {
				e.MoveNext ();
				q1.Enqueue (0);
				e.MoveNext ();
				Fail ("#1 Should have thrown InvalidOperationException");
			} catch	(InvalidOperationException) { }
			e = q1.GetEnumerator ();
		}

		public void TestEnumeratorException1 () 
		{
			SetUp();
			IEnumerator e;

			e = q1.GetEnumerator();
			q1.Enqueue(6);
			try {
				e.MoveNext();
				Fail("MoveNext must throw InvalidOperationException after Enqueue");
			} catch (InvalidOperationException) {}


			e = q1.GetEnumerator();
			q1.Enqueue(6);
			try 
			{
				e.Reset();
				Fail("Reset must throw InvalidOperationException after Enqueue");
			} 
			catch (InvalidOperationException) {}

			e = q1.GetEnumerator();
			q1.TrimToSize();
			try 
			{
				e.Reset();
				Fail("Reset must throw InvalidOperationException after TrimToSize");
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
			AssertNotNull (e.Current);
		}

		[Test]
		public void EnumeratorFalseAfterMoveNextAll () 
		{
			IEnumerator e = q1.GetEnumerator();
			while (e.MoveNext ()) {
			}
			Assert (!e.MoveNext ());
		}

		public void TestClone () {
			SetUp();
			Queue q3 = (Queue) q2.Clone ();
			Assert (q3.Count == q2.Count);
			for (int i = 0; i < 50; i++)
				Assert (q2.Dequeue ().Equals (q3.Dequeue ()));
			Assert (q3.Count == 0);
			Assert (q2.Count == 0);
		}

		public void TestClear () {
			SetUp();
			q1.Clear ();
			Assert (q1.Count == 0);
			q2.Clear ();
			Assert (q2.Count == 0);
			emptyQueue.Clear ();
			Assert (emptyQueue.Count == 0);
		}

		public void TestContains () {
			SetUp();
			for (int i = 0; i < 100; i++) {
				Assert (q1.Contains (i));
				Assert (!emptyQueue.Contains (i));
				if (i < 50)
					Assert (!q2.Contains (i));
				else
					Assert (q2.Contains (i));
			}
			
			Assert("q1 does not contain null", !q1.Contains(null));
			q1.Enqueue(null);
			Assert("q1 contains null", q1.Contains(null));
		}
		
		public void TestEnqueueDequeuePeek () {
			SetUp();
			int q1size = q1.Count;
			int q2size = q2.Count;
			q2.Enqueue (null);
			Assert (q2.Count == ++q2size);
			for (int i = 0; i < 50; i++) {
				int k = (int) q1.Peek ();
				Assert (q1.Count == q1size);
				int j = (int) q1.Dequeue ();
				Assert (q1.Count == --q1size);
				Assert (i == j);
				Assert (j == k);
				q2.Enqueue (j);
				Assert (q2.Count == ++q2size);
			}
			for (int i = 50; i < 100; i++) {
				Assert (((int) q2.Dequeue ()) == i);
				Assert (q2.Count == --q2size);
			}
			Assert (q2.Peek () == null);
			Assert (q2.Dequeue () == null);
			Assert (q2.Count == --q2size);
			for (int i = 0; i < 50; i++) {
				Assert (((int) q2.Dequeue ()) == i);
				Assert (q2.Count == --q2size);
			}
		}
		
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
				AssertEquals (tmp[i], tmp[i], z);
				i++;
			}
		}

		[ExpectedException(typeof(InvalidOperationException))]
		public void TestDequeueEmpty() 
		{
			Queue q= new Queue();
			q.Dequeue();
		}

		public void TestToArray() 
		{
			SetUp();
			object[] a = q1.ToArray();
			for (int i = 0; i < 100; i++) 
			{
				AssertEquals("Queue-Array mismatch",q1.Dequeue(),(int) a[i]);
			}

			object[] b = emptyQueue.ToArray();
			AssertEquals("b should be a zero-lenght array", 0, b.Length); 
		}

		public void TestTrimToSize() 
		{
			SetUp();
			for (int i=0; i < 50; i++) 
			{
				q1.Dequeue();
			}
			q1.TrimToSize();
			// FIXME: I can't figure out how to test if TrimToSize actually worked!
		}

		// TODO: test Syncronized operation

		public void TestSynchronizedException() 
		{
			try 
			{
				Queue.Synchronized(null);
				Fail("Must throw ArgumentNullException");
			} 
			catch (ArgumentNullException e)
			{
				AssertEquals("Exception's ParamName must be \"queue\"", "queue", e.ParamName);
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
			Assert ("q1sync.IsSyncronized", q1sync.IsSynchronized); 
			AssertEquals ("q1sync.Count", q1.Count, q1sync.Count);

			Queue q1syncsync = Queue.Synchronized (q1sync);
			Assert ("q1syncsync must be synchronized too", q1syncsync.IsSynchronized);
			AssertEquals ("q1syncsync.Count", q1.Count, q1syncsync.Count);

			Queue q1syncclone = (Queue) q1sync.Clone();
			Assert ("clone must be synchronized too", q1syncclone.IsSynchronized);
			AssertEquals ("q1syncclone.Count", q1.Count, q1syncclone.Count);
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
			AssertEquals ("0", 0, queue.Dequeue ());
			queue.Enqueue (411);

			AssertEquals ("Count-1", 31, queue.Count);
			for (int i = 1; i < 31; i++) {
				AssertEquals ("Peek" + i.ToString (), i, queue.Peek ());
				AssertEquals ("Dequeue" + i.ToString (), i, queue.Dequeue ());
			}
			AssertEquals ("Count-2", 1, queue.Count);
			AssertEquals ("411", 411, queue.Dequeue ());
		}

		[Test]
		public void TrimToSize_Enqueue_Dequeue ()
		{
			Queue queue = new Queue (32, 1.0F);
			for (int i = 0; i < 31; i++)
				queue.Enqueue (i);

			queue.TrimToSize ();
			queue.Enqueue (411);
			AssertEquals ("Count-1", 32, queue.Count);
			AssertEquals ("0", 0, queue.Dequeue ());
		}
	}
}

