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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class StackTest
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
			new Stack <int> (-1);
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
			new Stack <int> (null);
		}
		
		[Test]
		public void TestClear()
		{
			Stack <int> s = new Stack <int> ();
			s.Clear ();
			
			Assert.AreEqual (0, s.Count, "#1");
			
			s.Push (1);
			s.Push (2);
			
			Assert.AreEqual (2, s.Count, "#2");
			
			s.Clear ();
			
			Assert.AreEqual (0, s.Count, "#3");
		}
		
		[Test]
		public void TestContains ()
		{
			Stack <int> s = new Stack <int> ();
			
			Assert.IsFalse (s.Contains (1), "#1");
			
			s.Push (1);
			
			Assert.IsTrue  (s.Contains (1), "#2");
			Assert.IsFalse (s.Contains (0), "#3");
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
			
			Assert.AreEqual  (10, x [0], "#1");
			Assert.AreEqual (2, x [1], "#2");
			Assert.AreEqual (1, x [2], "#3");
			
			z = new Stack <int> ();
			x = new int [z.Count];
			z.CopyTo (x, 0);			
			
			ICollection c = new Stack <int> ();
			x = new int [c.Count];
			c.CopyTo (x, 0);
		}

		[Test]
		public void TestPeek ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			
			Assert.AreEqual (1, s.Peek (), "#1");
			Assert.AreEqual (1, s.Count, "#2");

			IEnumerator enumerator = s.GetEnumerator();
			s.Peek();
			enumerator.Reset();
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
			
			Assert.AreEqual (1, s.Pop (), "#1");
			Assert.AreEqual (0, s.Count, "#2");
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
			Assert.AreEqual (1, s.Count, "#1");
			s.Push (2);
			Assert.AreEqual (2, s.Count, "#2");
			
			for (int i = 0; i < 100; i ++)
				s.Push (i);
			
			Assert.AreEqual (102, s.Count, "#3");
		}
		
		[Test]
		public void TestToArray ()
		{
			Stack <int> s = new Stack <int> ();
			
			int [] x = s.ToArray ();
			
			Assert.AreEqual (0, x.Length, "#1");
			
			s.Push (1);
			x = s.ToArray ();
			Assert.AreEqual (1, x.Length, "#2");
			Assert.AreEqual (1, x [0], "#3");
		}
		
		[Test]
		public void TestEnumerator ()
		{
			Stack <int> s = new Stack <int> ();
			
			foreach (int x in s)
				Assert.Fail ("#1:" + x);
			
			s.Push (1);
			
			int i = 0;
			
			foreach (int x in s) {
				Assert.AreEqual (0, i, "#2");
				Assert.AreEqual (1, x, "#3");
				i ++;
			}
			
			i = 0;
			
			s.Push (2);
			s.Push (3);
			
			foreach (int x in s) {
				Assert.AreEqual (3 - i, x, "#4");
				Assert.IsTrue (i < 3, "#5");
				i ++;
			}
		}

		[Test]
		public void TrimExcessTest ()
		{
			Stack <int> s = new Stack <int> ();
			s.TrimExcess ();
			Assert.AreEqual (0, s.Count, "#1");

			s.Push (1);
			s.Push (3);
			Assert.AreEqual (3, s.Pop (), "#2");
			Assert.AreEqual (1, s.Peek (), "#3");

			s.TrimExcess ();
			Assert.AreEqual (1, s.Count, "#4");
			Assert.AreEqual (1, s.Peek (), "#5");

			s.Push (2);
			Assert.AreEqual (2, s.Pop (), "#6");
			Assert.AreEqual (1, s.Pop (), "#7");

			s.TrimExcess ();
			Assert.AreEqual (0, s.Count, "#8");
		}

		[Test]
		[Category ("NotWorking")] // bug #80649
		public void SerializeTest ()
		{
			Stack <int> s = new Stack <int> ();
			s.Push (1);
			s.Push (3);
			s.Push (2);
			s.Pop ();

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, s);

			byte [] buffer = new byte [ms.Length];
			ms.Position = 0;
			ms.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (_serializedStack, buffer);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif // TARGET_JVM
		public void DeserializeTest ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serializedStack, 0, _serializedStack.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			Stack<int> s = (Stack<int>) bf.Deserialize (ms);
			Assert.AreEqual (2, s.Count, "#1");
			Assert.AreEqual (3, s.Pop (), "#2");
			Assert.AreEqual (1, s.Pop (), "#3");
		}

		void AssertPop <T> (Stack <T> s, T t)
		{
			Assert.AreEqual  (t, s.Pop ());
		}

		static byte [] _serializedStack = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x02, 0x00, 0x00, 0x00,
			0x49, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2c, 0x20, 0x56, 0x65,
			0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30,
			0x2e, 0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65,
			0x3d, 0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50,
			0x75, 0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b,
			0x65, 0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36,
			0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00,
			0x00, 0x00, 0x7f, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43,
			0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e,
			0x47, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x2e, 0x53, 0x74, 0x61,
			0x63, 0x6b, 0x60, 0x31, 0x5b, 0x5b, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x49, 0x6e, 0x74, 0x33, 0x32, 0x2c, 0x20, 0x6d, 0x73,
			0x63, 0x6f, 0x72, 0x6c, 0x69, 0x62, 0x2c, 0x20, 0x56, 0x65, 0x72,
			0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e,
			0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d,
			0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75,
			0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65,
			0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31,
			0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x5d, 0x5d, 0x03, 0x00,
			0x00, 0x00, 0x06, 0x5f, 0x61, 0x72, 0x72, 0x61, 0x79, 0x05, 0x5f,
			0x73, 0x69, 0x7a, 0x65, 0x08, 0x5f, 0x76, 0x65, 0x72, 0x73, 0x69,
			0x6f, 0x6e, 0x07, 0x00, 0x00, 0x08, 0x08, 0x08, 0x02, 0x00, 0x00,
			0x00, 0x09, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04,
			0x00, 0x00, 0x00, 0x0f, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00,
			0x00, 0x08, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0b };
	}
}
#endif
