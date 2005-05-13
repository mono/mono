//
// Queue.cs
//
// Author:
//  Ben Maurer (bmaurer@ximian.com)
//

#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {
	[TestFixture]
	public class QueueTest : Assertion
	{
		[Test]
		public void TestCtor ()
		{
			Queue <int> a = new Queue <int> ();
			Queue <int> b = new Queue <int> (1);
			Queue <object> c = new Queue <object> ();
			Queue <object> d = new Queue <object> (1);
			Queue <object> e = new Queue <object> (0);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestCtorEx ()
		{
			Queue <int> a = new Queue <int> (-1);
		}
		
		[Test]
		public void TestCtorEnum ()
		{
			List <int> l = new List <int> ();
			l.Add (1);
			l.Add (2);
			l.Add (3);
			
			Queue <int> s = new Queue <int> (l);
			
			AssertDequeue (s, 1);
			AssertDequeue (s, 2);
			AssertDequeue (s, 3);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorEnumNull ()
		{
			Queue <int> s = new Queue <int> (null);
		}
		
		[Test]
		public void TestClear()
		{
			Queue <int> s = new Queue <int> ();
			s.Clear ();
			
			AssertEquals (s.Count, 0);
			
			s.Enqueue (1);
			s.Enqueue (2);
			
			AssertEquals (s.Count, 2);
			
			s.Clear ();
			
			AssertEquals (s.Count, 0);
		}
		
		[Test]
		public void TestContains ()
		{
			Stack <int> s = new Stack <int> ();
			
			AssertEquals (s.Contains (1), false);
			
			s.Push (1);
			
			AssertEquals (s.Contains (1), true);
			AssertEquals (s.Contains (0), false);
		}
		
		[Test]
		public void TestCopyTo ()
		{
			int [] x = new int [3];
			Queue <int> z = new Queue <int> ();
			z.Enqueue (1);
			z.Enqueue (2);
			x [0] = 10;
			z.CopyTo (x, 1);
			
			AssertEquals (x [0], 10);
			AssertEquals (x [1], 1);
			AssertEquals (x [2], 2);
		}
		
		[Test]
		public void TestPeek ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			
			AssertEquals (s.Peek (), 1);
			AssertEquals (s.Count, 1);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPeekEx ()
		{
			Queue <int> s = new Queue <int> ();
			s.Peek ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPeekEx2 ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			s.Dequeue ();			
			s.Peek ();
		}
		
		[Test]
		public void TestDequeue ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			
			AssertEquals (s.Dequeue (), 1);
			AssertEquals (s.Count, 0);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDequeueEx ()
		{
			Queue <int> s = new Queue <int> ();
			s.Dequeue ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDequeueEx2 ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			s.Dequeue ();			
			s.Dequeue ();
		}
		
		[Test]
		public void TestEnqueue ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			AssertEquals (s.Count, 1);
			s.Enqueue (2);
			AssertEquals (s.Count, 2);
			
			for (int i = 0; i < 100; i ++)
				s.Enqueue (i);
			
			AssertEquals (s.Count, 102);
		}
		
		[Test]
		public void TestToArray ()
		{
			Queue <int> s = new Queue <int> ();
			
			int [] x = s.ToArray ();
			
			AssertEquals (x.Length, 0);
			
			s.Enqueue (1);
			x = s.ToArray ();
			AssertEquals (x.Length, 1);
			AssertEquals (x [0], 1);
		}
		
		[Test]
		public void TestTrimToSize ()
		{
			Queue <int> s = new Queue <int> ();
			s.TrimToSize ();
			s.Enqueue (1);
			s.TrimToSize ();
		}
		
		[Test]
		public void TestEnumerator ()
		{
			Queue <int> s = new Queue <int> ();
			
			foreach (int x in s)
				Fail ();
			
			s.Enqueue (1);
			
			int i = 0;
			
			foreach (int x in s) {
				AssertEquals (i, 0);
				AssertEquals (x, 1);
				i ++;
			}
			
			for (i = 2; i < 100; i ++)
				s.Enqueue (i);
			
			i = 1;
			
			foreach (int x in s) {
				AssertEquals (x, i);
				i ++;
			}
		}
		
		void AssertDequeue <T> (Queue <T> s, T t)
		{
			AssertEquals (s.Dequeue (), t);
		}
        }
}
#endif