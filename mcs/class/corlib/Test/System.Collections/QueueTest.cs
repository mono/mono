//
// System.Collections.QueueTest
// Test suite for System.Collections.Queue
//
// Author:
//    Ricardo Fernández Pascual
//
// (C) 2001 Ricardo Fernández Pascual
//



using System;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections {

	public class QueueTest : TestCase {

		protected Queue q1;
		protected Queue q2;
		protected Queue emptyQueue;

		protected override void SetUp () {
			q1 = new Queue (10);
			for (int i = 0; i < 100; i++)
				q1.Enqueue (i);
			
			q2 = new Queue (50, 1.5f);
			for (int i = 50; i < 100; i++)
				q2.Enqueue (i);

			emptyQueue = new Queue ();
		}

		public void TestConstructors () {
			Assert (q1.Count == 100);
			Assert (q2.Count == 50);
			Assert (emptyQueue.Count == 0);
			// TODO: Test Queue (ICollection)
		}

		// TODO: should test all methods from ICollection, 
		// but it should be done in ICollectionTest.cs... ??

		public void TestCopyTo () {
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

		public void TestClone () {
			Queue q3 = (Queue) q2.Clone ();
			Assert (q3.Count == q2.Count);
			for (int i = 0; i < 50; i++)
				Assert (q2.Dequeue ().Equals (q3.Dequeue ()));
			Assert (q3.Count == 0);
			Assert (q2.Count == 0);
		}

		public void ClearTest () {
			q1.Clear ();
			Assert (q1.Count == 0);
			q2.Clear ();
			Assert (q2.Count == 0);
			emptyQueue.Clear ();
			Assert (emptyQueue.Count == 0);
		}

		public void ContainsTest () {
			for (int i = 0; i < 100; i++) {
				Assert (q1.Contains (i));
				Assert (!emptyQueue.Contains (i));
				if (i < 50)
					Assert (!q2.Contains (i));
				else
					Assert (q2.Contains (i));
			}
		}
		
		public void EnqueueDequeuePeekTest () {
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
		
		public void DequeueTest() {
			Queue queue = new Queue();
			string[] tmp = new string[50];
			int i;
			for (i=0;i<50;i++) {
				tmp[i] = "Data #" + i;
				queue.Enqueue(tmp[i]);
			}

			while(queue.Count>0){
				i--;
				string z = (string) queue.Dequeue();
				AssertEquals (tmp[i], tmp[i], z);
			}
		}

		// TODO: test Syncronized operation

	}
}

