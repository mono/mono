#if NET_2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Collections.Generic
{
	[TestFixture]
	public class LinkedListTest
	{
		LinkedList <int> intlist;
		LinkedList <string> strings;

		[SetUp]
		public void Setup ()
		{
			intlist = new LinkedList <int> ();
			
			// 2 3 4
			intlist.AddLast (3);
			intlist.AddLast (4);
			intlist.AddFirst (2);

			string [] tmpStrings = new string [] { "foo", "bar", "baz" };
			// FIXME workaround for 74953
			
			List <string> workaround = new List <string> ();
			foreach (string s in tmpStrings)
				workaround.Add (s);

			// strings = new LinkedList <string> (tmpStrings);
			strings = new LinkedList <string> (workaround);
		}

		[Test]
		public void AddedTest ()
		{
			int i = 2;
			foreach (int current in intlist)
			{
				Assert.AreEqual (i, current);
				i++;
			}
			Assert.AreEqual (5, i);
		}

		[Test]
		public void CreatedTest ()
		{
			string [] values = new string [] { "foo", "bar", "baz" };
			int i = 0;
			foreach (string current in strings)
			{
				Assert.AreEqual (values [i], current);
				i++;
			}
			Assert.AreEqual (3, i);
		}

		[Test]
		public void NonCircularNodeTest ()
		{
			LinkedListNode <int> node = intlist.First;
			Assert.AreEqual (2, node.Value);
			LinkedListNode <int> previous = node.Previous;
			Assert.IsNull (previous);

			node = node.Next;
			Assert.IsNotNull (node);
			Assert.AreEqual (3, node.Value);

			node = node.Next;
			Assert.IsNotNull (node);
			Assert.AreEqual (4, node.Value);

			node = node.Next;
			Assert.IsNull (node);
		}

		[Test]
		public void ClearTest ()
		{
			LinkedListNode <int> node = intlist.First;
			intlist.Clear ();

			Assert.AreEqual (0, intlist.Count);
			Assert.AreEqual (2, node.Value);
			Assert.IsNull (node.Next);
			Assert.IsNull (node.Previous);
		}

		[Test]
		public void ContainsTest ()
		{
			Assert.IsTrue (intlist.Contains (3));
			Assert.IsFalse (intlist.Contains (5));
		}

		[Test]
		public void AddBeforeAndAfterTest ()
		{
			LinkedListNode <int> node = intlist.Find (3);
			intlist.AddAfter (node, new LinkedListNode <int> (5));
			LinkedListNode <int> sixNode = intlist.AddAfter (node, 6);
			LinkedListNode <int> sevenNode = intlist.AddBefore (node, 7);
			intlist.AddBefore (node, new LinkedListNode <int> (8));

			Assert.AreEqual (6, sixNode.Value);
			Assert.AreEqual (7, sevenNode.Value);

			// 2 7 8 3 6 5 4
			int [] values = new int [] { 2, 7, 8, 3, 6, 5, 4 };
			int i = 0;
			foreach (int current in intlist)
			{
				Assert.AreEqual (values [i], current);
				i++;
			}
			for (LinkedListNode <int> current = intlist.First; current != null; current = current.Next )
				Assert.AreSame (intlist, current.List);
		}

		[Test]
		public void CopyToTest ()
		{
			int [] values = new int [] { 2, 3, 4 };
			int [] output = new int [3];
			intlist.CopyTo (output, 0);
			for (int i = 0; i < 3; i++)
				Assert.AreEqual (values [i], output [i]);
			
			LinkedList <int> l = new LinkedList <int> ();
			values = new int [l.Count];
			l.CopyTo (values, 0);
		}

		[Test]
		public void FindTest ()
		{
			intlist.AddFirst (4);

			LinkedListNode <int> head, tail;
			head = intlist.Find (4);
			tail = intlist.FindLast (4);
			Assert.AreEqual (intlist.First, head);
			Assert.AreEqual (intlist.Last, tail);
		}

		[Test]
		public void RemoveTest ()
		{
			Assert.IsTrue (intlist.Remove (3));
			Assert.AreEqual (2, intlist.Count);

			int [] values = { 2, 4 };
			int i = 0;
			foreach (int current in intlist)
			{
				Assert.AreEqual (values [i], current);
				i++;
			}
			Assert.IsFalse (intlist.Remove (5));

			LinkedListNode <string> node = strings.Find ("baz");
			strings.Remove (node);

			Assert.IsNull (node.List);
			Assert.IsNull (node.Previous);
			Assert.IsNull (node.Next);

			string [] values2 = { "foo", "bar" };
			i = 0;
			foreach (string current in strings)
			{
				Assert.AreEqual (values2 [i], current);
				i++;
			}
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNullNodeTest ()
		{
			intlist.Remove (null);
		}
		
		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void RemoveInvalidNodeTest ()
		{
			intlist.Remove (new LinkedListNode <int> (4));
		}

		[Test]
		public void RemoveFirstLastTest ()
		{
			strings.RemoveFirst ();
			strings.RemoveLast ();
			Assert.AreEqual (1, strings.Count);
			Assert.AreEqual ("bar", strings.First.Value);
		}

		[Test]
		public void ListSerializationTest ()
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream();
			formatter.Serialize(stream, intlist);

			stream.Position = 0;
			object deserialized = formatter.Deserialize(stream);

			Assert.IsTrue(deserialized is LinkedList <int>);

			LinkedList <int> dlist = deserialized as LinkedList <int>;

			int [] values = { 2, 3, 4 };
			int i = 0;
			foreach (int value in dlist)
			{
				Assert.AreEqual (values [i], value);
				i++;
			}
			Assert.AreEqual(3, i);
		}

		/* FIXME: disabled pending fix for #75299
		[Test]
		*/
		public void EnumeratorSerializationTest ()
		{
			BinaryFormatter formatter = new BinaryFormatter ();
			MemoryStream stream = new MemoryStream ();
			LinkedList<int>.Enumerator e = intlist.GetEnumerator ();
			formatter.Serialize (stream, e);

			stream.Position = 0;
			object deserialized = formatter.Deserialize(stream);

			Assert.IsTrue(deserialized is LinkedList <int>.Enumerator);

			LinkedList <int>.Enumerator d = (LinkedList <int>.Enumerator) deserialized;

			int [] values = { 2, 3, 4 };
			int i = 0;
			while (d.MoveNext ())
			{
				Assert.AreEqual (values [i], d.Current);
				i++;
			}
			Assert.AreEqual(3, i);
		}
		
		[Test] //bug 481621
		public void PlayWithNullValues ()
		{
			LinkedList <string> li = new LinkedList <string> ();
			li.AddLast ((string)null);
			li.AddLast ("abcd");
			li.AddLast ((string)null);
			li.AddLast ("efgh");
			Assert.AreEqual (4, li.Count);
			Assert.AreEqual ("efgh", li.Last.Value);
			Assert.IsNull (li.First.Value);

			Assert.IsTrue (li.Remove ((string)null));
			Assert.AreEqual (3, li.Count);
			Assert.AreEqual ("efgh", li.Last.Value);
			Assert.AreEqual ("abcd", li.First.Value);

			Assert.IsTrue (li.Remove ((string)null));
			Assert.AreEqual (2, li.Count);
			Assert.AreEqual ("efgh", li.Last.Value);
			Assert.AreEqual ("abcd", li.First.Value);

			Assert.IsFalse (li.Remove ((string)null));
			Assert.AreEqual (2, li.Count);
			Assert.AreEqual ("efgh", li.Last.Value);
			Assert.AreEqual ("abcd", li.First.Value);
		}
	}
}

#endif
