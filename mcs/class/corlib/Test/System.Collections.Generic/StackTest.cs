//
// StackTest.cs
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
	public class StackTest: Assertion
	{
		[Test]
		public void TestCtor ()
		{
			Stack <int> a = new Stack <int> ();
			Stack <int> b = new Stack <int> (1);
			Stack <object> c = new Stack <object> ();
			Stack <object> d = new Stack <object> (1);
			Stack <object> e = new Stack <object> (0);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestCtorEx ()
		{
			Stack <int> a = new Stack <int> (-1);
		}
		
		[Test]
		public void TestCtorEnum ()
		{
			List <int> l = new List <int> ();
			l.Add (1);
			l.Add (2);
			l.Add (3);
			
			Stack <int> s = new Stack <int> (l);
			
			// Things get pop'd in reverse
			AssertPop (s, 3);
			AssertPop (s, 2);
			AssertPop (s, 1);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtorEnumNull ()
		{
			Stack <int> s = new Stack <int> (null);
		}
		
		[Test]
		public void TestClear()
		{
			Stack <int> s = new Stack <int> ();
			s.Clear ();
			
			AssertEquals (s.Count, 0);
			
			s.Push (1);
			s.Push (2);
			
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
			Stack <int> z = new Stack <int> ();
			z.Push (1);
			z.Push (2);
			x [0] = 10;
			z.CopyTo (x, 1);
			
			AssertEquals (x [0], 10);
			AssertEquals (x [1], 2);
			AssertEquals (x [2], 1);
		}
		
		[Test]
		public void TestPeek ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			
			AssertEquals (s.Peek (), 1);
			AssertEquals (s.Count, 1);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPeekEx ()
		{
			Stack <int> s = new Stack <int> ();
			s.Peek ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPeekEx2 ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			s.Pop ();			
			s.Peek ();
		}
		
		[Test]
		public void TestPop ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			
			AssertEquals (s.Pop (), 1);
			AssertEquals (s.Count, 0);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPopEx ()
		{
			Stack <int> s = new Stack <int> ();
			s.Pop ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestPopEx2 ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			s.Pop ();			
			s.Pop ();
		}
		
		[Test]
		public void TestPush ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			AssertEquals (s.Count, 1);
			s.Push (2);
			AssertEquals (s.Count, 2);
			
			for (int i = 0; i < 100; i ++)
				s.Push (i);
			
			AssertEquals (s.Count, 102);
		}
		
		[Test]
		public void TestToArray ()
		{
			Stack <int> s = new Stack <int> ();
			
			int [] x = s.ToArray ();
			
			AssertEquals (x.Length, 0);
			
			s.Push (1);
			x = s.ToArray ();
			AssertEquals (x.Length, 1);
			AssertEquals (x [0], 1);
		}
		
		[Test]
		public void TestTrimToSize ()
		{
			Stack <int> s = new Stack <int> ();
			s.TrimToSize ();
			s.Push (1);
			s.TrimToSize ();
		}
		
		[Test]
		public void TestEnumerator ()
		{
			Stack <int> s = new Stack <int> ();
			
			foreach (int x in s)
				Fail ();
			
			s.Push (1);
			
			int i = 0;
			
			foreach (int x in s) {
				AssertEquals (i, 0);
				AssertEquals (x, 1);
				i ++;
			}
			
			i = 0;
			
			s.Push (2);
			s.Push (3);
			
			foreach (int x in s) {
				AssertEquals (x, 3 - i);
				Assert (i < 3);
				i ++;
			}
		}
		
		void AssertPop <T> (Stack <T> s, T t)
		{
			AssertEquals (s.Pop (), t);
		}
        }
}
#endif