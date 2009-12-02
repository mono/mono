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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {
	[TestFixture]
	public class QueueTest
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
			
			Assert.AreEqual (0, s.Count, "#1");
			
			s.Enqueue (1);
			s.Enqueue (2);
			
			Assert.AreEqual (2, s.Count, "#2");
			
			s.Clear ();
			
			Assert.AreEqual (0, s.Count, "#3");

			IEnumerator enumerator = s.GetEnumerator();
			s.Clear();

			try {
				enumerator.Reset();
				Assert.Fail ("#4");
			} catch(InvalidOperationException) {
			}
		}
		
		[Test]
		public void TestContains ()
		{
			Stack <int> s = new Stack <int> ();
			
			Assert.IsFalse (s.Contains (1), "#1");
			
			s.Push (1);
			
			Assert.IsTrue (s.Contains (1), "#2");
			Assert.IsFalse (s.Contains (0), "#3");
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
			
			Assert.AreEqual (10, x [0], "#1");
			Assert.AreEqual (1, x [1], "#2");
			Assert.AreEqual (2, x [2], "#3");
			
			z = new Queue <int> ();
			x = new int [z.Count];
			z.CopyTo (x, 0);
		}

		[Test]
		public void TestICollectionCopyTo ()
		{
			var queue = new Queue<int> ();

			((ICollection) queue).CopyTo (new int [0], 0);

			queue.Enqueue (1);
			queue.Enqueue (2);

			var array = new int [queue.Count];

			((ICollection) queue).CopyTo (array, 0);

			Assert.AreEqual (1, array [0]);
			Assert.AreEqual (2, array [1]);

			array = new int [queue.Count + 1];
			array [0] = 42;

			((ICollection) queue).CopyTo (array, 1);

			Assert.AreEqual (42, array [0]);
			Assert.AreEqual (1, array [1]);
			Assert.AreEqual (2, array [2]);
		}

		[Test]
		public void TestPeek ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			
			Assert.AreEqual (1, s.Peek (), "#1");
			Assert.AreEqual (1, s.Count, "#2");
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
			
			Assert.AreEqual (1, s.Dequeue (), "#1");
			Assert.AreEqual (0, s.Count, "#2");
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
			Assert.AreEqual (1, s.Count, "#1");
			s.Enqueue (2);
			Assert.AreEqual (2, s.Count, "#2");
			
			for (int i = 0; i < 100; i ++)
				s.Enqueue (i);
			
			Assert.AreEqual (102, s.Count, "#3");
		}
		
		[Test]
		public void TestToArray ()
		{
			Queue <int> s = new Queue <int> ();
			
			int [] x = s.ToArray ();
			
			Assert.AreEqual (0, x.Length, "#1");
			
			s.Enqueue (1);
			x = s.ToArray ();
			Assert.AreEqual (1, x.Length, "#2");
			Assert.AreEqual (1, x [0], "#3");
		}

		[Test]
		public void TestEnumerator ()
		{
			Queue <int> s = new Queue <int> ();
			
			foreach (int x in s)
				Assert.Fail ("#1" + x);
			
			s.Enqueue (1);
			
			int i = 0;
			
			foreach (int x in s) {
				Assert.AreEqual  (0, i, "#2");
				Assert.AreEqual  (1, x, "#3");
				i ++;
			}
			
			for (i = 2; i < 100; i ++)
				s.Enqueue (i);
			
			i = 1;
			
			foreach (int x in s) {
				Assert.AreEqual (i, x, "#4");
				i ++;
			}
		}

		[Test]
		public void TrimExcessTest ()
		{
			Queue <int> s = new Queue <int> ();
			s.TrimExcess ();
			Assert.AreEqual (0, s.Count, "#1");

			s.Enqueue (1);
			s.Enqueue (3);
			Assert.AreEqual (1, s.Dequeue (), "#2");
			Assert.AreEqual (3, s.Peek (), "#3");

			s.TrimExcess ();
			Assert.AreEqual (1, s.Count, "#4");
			Assert.AreEqual (3, s.Peek (), "#5");

			s.Enqueue (2);
			Assert.AreEqual (3, s.Dequeue (), "#6");
			Assert.AreEqual (2, s.Dequeue (), "#7");

			s.TrimExcess ();
			Assert.AreEqual (0, s.Count, "#8");
		}

		[Test]
		public void TrimExcessDequeueEnqueue ()
		{
			var queue = new Queue<int> ();
			queue.Enqueue (1);
			queue.Enqueue (2);
			queue.Enqueue (3);

			queue.TrimExcess ();

			Assert.AreEqual (1, queue.Dequeue ());

			queue.Enqueue (4);

			Assert.AreEqual (2, queue.Dequeue ());
			Assert.AreEqual (3, queue.Dequeue ());
			Assert.AreEqual (4, queue.Dequeue ());

			Assert.AreEqual (0, queue.Count);
		}

		[Test]
		[Category ("NotWorking")] // bug #80649
		public void SerializeTest ()
		{
			Queue <int> s = new Queue <int> ();
			s.Enqueue (1);
			s.Enqueue (3);
			s.Enqueue (2);
			s.Dequeue ();

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, s);

			byte [] buffer = new byte [ms.Length];
			ms.Position = 0;
			ms.Read (buffer, 0, buffer.Length);

			Assert.AreEqual (_serializedQueue, buffer);
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void DeserializeTest ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serializedQueue, 0, _serializedQueue.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			Queue<int> s = (Queue<int>) bf.Deserialize (ms);
			Assert.AreEqual (2, s.Count, "#1");
			Assert.AreEqual (3, s.Dequeue (), "#2");
			Assert.AreEqual (2, s.Dequeue (), "#3");
		}

		void AssertDequeue <T> (Queue <T> s, T t)
		{
			Assert.AreEqual (t, s.Dequeue ());
		}

		static byte [] _serializedQueue = new byte [] {
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
			0x47, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x2e, 0x51, 0x75, 0x65,
			0x75, 0x65, 0x60, 0x31, 0x5b, 0x5b, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x49, 0x6e, 0x74, 0x33, 0x32, 0x2c, 0x20, 0x6d, 0x73,
			0x63, 0x6f, 0x72, 0x6c, 0x69, 0x62, 0x2c, 0x20, 0x56, 0x65, 0x72,
			0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30, 0x2e,
			0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d,
			0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75,
			0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65,
			0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31,
			0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x5d, 0x5d, 0x05, 0x00,
			0x00, 0x00, 0x06, 0x5f, 0x61, 0x72, 0x72, 0x61, 0x79, 0x05, 0x5f,
			0x68, 0x65, 0x61, 0x64, 0x05, 0x5f, 0x74, 0x61, 0x69, 0x6c, 0x05,
			0x5f, 0x73, 0x69, 0x7a, 0x65, 0x08, 0x5f, 0x76, 0x65, 0x72, 0x73,
			0x69, 0x6f, 0x6e, 0x07, 0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x08,
			0x08, 0x08, 0x02, 0x00, 0x00, 0x00, 0x09, 0x03, 0x00, 0x00, 0x00,
			0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x05, 0x00, 0x00, 0x00, 0x0f, 0x03, 0x00, 0x00, 0x00, 0x04,
			0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00,
			0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0b };
	}
}
#endif
