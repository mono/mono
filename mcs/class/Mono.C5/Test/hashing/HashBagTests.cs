#if NET_2_0
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
namespace nunit.hashtable.bag
{
	[TestFixture]
	public class Combined
	{
		private ICollection<KeyValuePair<int,int>> lst;


		[SetUp]
		public void Init()
		{
			lst = new HashBag<KeyValuePair<int,int>>();
			for (int i = 0; i < 10; i++)
				lst.Add(new KeyValuePair<int,int>(i, i + 30));
		}


		[TearDown]
		public void Dispose() { lst = null; }


		[Test]
		public void Find()
		{
			KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);

			Assert.IsTrue(lst.Find(ref p));
			Assert.AreEqual(3, p.key);
			Assert.AreEqual(33, p.value);
			p = new KeyValuePair<int,int>(13, 78);
			Assert.IsFalse(lst.Find(ref p));
		}


		[Test]
		public void FindOrAdd()
		{
			KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);
			KeyValuePair<int,int> q = new KeyValuePair<int,int>();

			Assert.IsTrue(lst.FindOrAdd(ref p));
			Assert.AreEqual(3, p.key);
			Assert.AreEqual(33, p.value);
			p = new KeyValuePair<int,int>(13, 79);
			Assert.IsFalse(lst.FindOrAdd(ref p));
			q.key = 13;
			Assert.IsTrue(lst.Find(ref q));
			Assert.AreEqual(13, q.key);
			Assert.AreEqual(79, q.value);
		}


		[Test]
		public void Update()
		{
			KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);
			KeyValuePair<int,int> q = new KeyValuePair<int,int>();

			Assert.IsTrue(lst.Update(p));
			q.key = 3;
			Assert.IsTrue(lst.Find(ref q));
			Assert.AreEqual(3, q.key);
			Assert.AreEqual(78, q.value);
			p = new KeyValuePair<int,int>(13, 78);
			Assert.IsFalse(lst.Update(p));
		}


		[Test]
		public void UpdateOrAdd()
		{
			KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);
			KeyValuePair<int,int> q = new KeyValuePair<int,int>();

			Assert.IsTrue(lst.UpdateOrAdd(p));
			q.key = 3;
			Assert.IsTrue(lst.Find(ref q));
			Assert.AreEqual(3, q.key);
			Assert.AreEqual(78, q.value);
			p = new KeyValuePair<int,int>(13, 79);
			Assert.IsFalse(lst.UpdateOrAdd(p));
			q.key = 13;
			Assert.IsTrue(lst.Find(ref q));
			Assert.AreEqual(13, q.key);
			Assert.AreEqual(79, q.value);
		}


		[Test]
		public void RemoveWithReturn()
		{
			KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);
			KeyValuePair<int,int> q = new KeyValuePair<int,int>();

			Assert.IsTrue(lst.RemoveWithReturn(ref p));
			Assert.AreEqual(3, p.key);
			Assert.AreEqual(33, p.value);
			p = new KeyValuePair<int,int>(13, 78);
			Assert.IsFalse(lst.RemoveWithReturn(ref p));
		}
	}



	[TestFixture]
	public class CollectionOrSink
	{
		private HashBag<int> hashbag;


		[SetUp]
		public void Init() { hashbag = new HashBag<int>(); }


		[Test]
		public void CountEtAl()
		{
			Assert.IsFalse(hashbag.IsReadOnly);
			Assert.IsFalse(hashbag.SyncRoot == null);
			Assert.AreEqual(0, hashbag.Count);
			Assert.IsTrue(hashbag.IsEmpty);
			Assert.IsTrue(hashbag.AllowsDuplicates);
			Assert.IsTrue(hashbag.Add(0));
			Assert.AreEqual(1, hashbag.Count);
			Assert.IsFalse(hashbag.IsEmpty);
			Assert.IsTrue(hashbag.Add(5));
			Assert.AreEqual(2, hashbag.Count);
			Assert.IsTrue(hashbag.Add(5));
			Assert.AreEqual(3, hashbag.Count);
			Assert.IsFalse(hashbag.IsEmpty);
			Assert.IsTrue(hashbag.Add(8));
			Assert.AreEqual(4, hashbag.Count);
			Assert.AreEqual(2, hashbag.ContainsCount(5));
			Assert.AreEqual(1, hashbag.ContainsCount(8));
			Assert.AreEqual(1, hashbag.ContainsCount(0));
		}


		[Test]
		public void AddAll()
		{
			hashbag.Add(3);hashbag.Add(4);hashbag.Add(4);hashbag.Add(5);hashbag.Add(4);

			HashBag<int> hashbag2 = new HashBag<int>();

			hashbag2.AddAll(hashbag);
			Assert.IsTrue(IC.seq(hashbag2, 3, 4, 4, 4, 5));
			hashbag.Add(9);
			hashbag.AddAll(hashbag2);
			Assert.IsTrue(IC.seq(hashbag2, 3, 4, 4, 4, 5));
			Assert.IsTrue(IC.seq(hashbag, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 9));
		}


		[Test]
		public void ContainsCount()
		{
			Assert.AreEqual(0, hashbag.ContainsCount(5));
			hashbag.Add(5);
			Assert.AreEqual(1, hashbag.ContainsCount(5));
			Assert.AreEqual(0, hashbag.ContainsCount(7));
			hashbag.Add(8);
			Assert.AreEqual(1, hashbag.ContainsCount(5));
			Assert.AreEqual(0, hashbag.ContainsCount(7));
			Assert.AreEqual(1, hashbag.ContainsCount(8));
			hashbag.Add(5);
			Assert.AreEqual(2, hashbag.ContainsCount(5));
			Assert.AreEqual(0, hashbag.ContainsCount(7));
			Assert.AreEqual(1, hashbag.ContainsCount(8));
		}


		[Test]
		public void RemoveAllCopies()
		{
			hashbag.Add(5);hashbag.Add(7);hashbag.Add(5);
			Assert.AreEqual(2, hashbag.ContainsCount(5));
			Assert.AreEqual(1, hashbag.ContainsCount(7));
			hashbag.RemoveAllCopies(5);
			Assert.AreEqual(0, hashbag.ContainsCount(5));
			Assert.AreEqual(1, hashbag.ContainsCount(7));
			hashbag.Add(5);hashbag.Add(8);hashbag.Add(5);
			hashbag.RemoveAllCopies(8);
			Assert.IsTrue(IC.eq(hashbag, 7, 5, 5));
		}


		[Test]
		public void ContainsAll()
		{
			HashBag<int> list2 = new HashBag<int>();

			Assert.IsTrue(hashbag.ContainsAll(list2));
			list2.Add(4);
			Assert.IsFalse(hashbag.ContainsAll(list2));
			hashbag.Add(4);
			Assert.IsTrue(hashbag.ContainsAll(list2));
			hashbag.Add(5);
			Assert.IsTrue(hashbag.ContainsAll(list2));
			list2.Add(20);
			Assert.IsFalse(hashbag.ContainsAll(list2));
			hashbag.Add(20);
			Assert.IsTrue(hashbag.ContainsAll(list2));
			list2.Add(4);
			Assert.IsFalse(hashbag.ContainsAll(list2));
			hashbag.Add(4);
			Assert.IsTrue(hashbag.ContainsAll(list2));
		}


		[Test]
		public void RetainAll()
		{
			HashBag<int> list2 = new HashBag<int>();

			hashbag.Add(4);hashbag.Add(5);hashbag.Add(4);hashbag.Add(6);hashbag.Add(4);
			list2.Add(5);list2.Add(4);list2.Add(7);list2.Add(4);
			hashbag.RetainAll(list2);
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 5));
			hashbag.Add(6);
			list2.Clear();
			list2.Add(7);list2.Add(8);list2.Add(9);
			hashbag.RetainAll(list2);
			Assert.IsTrue(IC.eq(hashbag));
		}


		[Test]
		public void RemoveAll()
		{
			HashBag<int> list2 = new HashBag<int>();

			hashbag.Add(4);hashbag.Add(5);hashbag.Add(6);hashbag.Add(4);hashbag.Add(5);
			list2.Add(5);list2.Add(4);list2.Add(7);list2.Add(4);
			hashbag.RemoveAll(list2);
			Assert.IsTrue(IC.seq(hashbag, 5, 6));
			hashbag.Add(5);hashbag.Add(4);
			list2.Clear();
			list2.Add(6);list2.Add(5);
			hashbag.RemoveAll(list2);
			Assert.IsTrue(IC.seq(hashbag, 4, 5));
			list2.Clear();
			list2.Add(7);list2.Add(8);list2.Add(9);
			hashbag.RemoveAll(list2);
			Assert.IsTrue(IC.seq(hashbag, 4, 5));
		}


		[Test]
		public void Remove()
		{
			hashbag.Add(4);hashbag.Add(4);hashbag.Add(5);hashbag.Add(4);hashbag.Add(6);
			Assert.IsFalse(hashbag.Remove(2));
			Assert.IsTrue(hashbag.Remove(4));
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 5, 6));
			hashbag.Add(7);
			hashbag.Add(21);hashbag.Add(37);hashbag.Add(53);hashbag.Add(69);hashbag.Add(53);hashbag.Add(85);
			Assert.IsTrue(hashbag.Remove(5));
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 6, 7, 21, 37, 53, 53, 69, 85));
			Assert.IsFalse(hashbag.Remove(165));
			Assert.IsTrue(hashbag.Check());
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 6, 7, 21, 37, 53, 53, 69, 85));
			Assert.IsTrue(hashbag.Remove(53));
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 6, 7, 21, 37, 53, 69, 85));
			Assert.IsTrue(hashbag.Remove(37));
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 6, 7, 21, 53, 69, 85));
			Assert.IsTrue(hashbag.Remove(85));
			Assert.IsTrue(IC.seq(hashbag, 4, 4, 6, 7, 21, 53, 69));
		}


		[TearDown]
		public void Dispose() { hashbag = null; }
	}



	[TestFixture]
	public class ArrayTest
	{
		private HashBag<int> hashbag;

		int[] a;


		[SetUp]
		public void Init()
		{
			hashbag = new HashBag<int>();
			a = new int[10];
			for (int i = 0; i < 10; i++)
				a[i] = 1000 + i;
		}


		[TearDown]
		public void Dispose() { hashbag = null; }


		private string aeq(int[] a, params int[] b)
		{
			if (a.Length != b.Length)
				return "Lengths differ: " + a.Length + " != " + b.Length;

			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i])
					return String.Format("{0}'th elements differ: {1} != {2}", i, a[i], b[i]);

			return "Alles klar";
		}


		[Test]
		public void ToArray()
		{
			Assert.AreEqual("Alles klar", aeq(hashbag.ToArray()));
			hashbag.Add(7);
			hashbag.Add(3);
			hashbag.Add(10);
			hashbag.Add(3);

			int[] r = hashbag.ToArray();

			Array.Sort(r);
			Assert.AreEqual("Alles klar", aeq(r, 3, 3, 7, 10));
		}


		[Test]
		public void CopyTo()
		{
			//Note: for small ints the itemhasher is the identity!
			hashbag.CopyTo(a, 1);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
			hashbag.Add(6);
			hashbag.CopyTo(a, 2);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
			hashbag.Add(4);
			hashbag.Add(6);
			hashbag.Add(9);
			hashbag.CopyTo(a, 4);

			//TODO: make independent of interhasher
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 6, 9, 4, 1008, 1009));
			hashbag.Clear();
			hashbag.Add(7);
			hashbag.CopyTo(a, 9);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 6, 9, 4, 1008, 7));
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyToBad()
		{
			hashbag.Add(3);
			hashbag.CopyTo(a, 10);
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CopyToBad2()
		{
			hashbag.CopyTo(a, -1);
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyToTooFar()
		{
			hashbag.Add(3);
			hashbag.Add(8);
			hashbag.CopyTo(a, 9);
		}
	}



	[TestFixture]
	public class HashingEquals
	{
		private ICollection<int> h1, h2;


		[SetUp]
		public void Init()
		{
			h1 = new HashBag<int>();
			h2 = new LinkedList<int>();
		}


		[TearDown]
		public void Dispose()
		{
			h1 = h2 = null;
		}


		[Test]
		public void Hashing()
		{
			Assert.AreEqual(h1.GetHashCode(), h2.GetHashCode());
			h1.Add(7);
			h2.Add(9);
			Assert.IsTrue(h1.GetHashCode() != h2.GetHashCode());
			h2.Add(7);
			h1.Add(9);
			Assert.IsTrue(h1.GetHashCode() == h2.GetHashCode());
		}


		[Test]
		public void Equals()
		{
			Assert.IsTrue(h1.Equals(h2));
			h1.Add(1);
			h1.Add(2);
			h1.Add(1);
			h1.Add(2);
			h2.Add(0);
			h2.Add(3);
			h2.Add(0);
			h2.Add(3);
			Assert.IsTrue(h1.GetHashCode() == h2.GetHashCode());
			Assert.IsTrue(!h1.Equals(h2));
			h1.Clear();
			h2.Clear();
			h1.Add(1);
			h1.Add(2);
			h2.Add(2);
			h2.Add(1);
			Assert.IsTrue(h1.GetHashCode() == h2.GetHashCode());
			Assert.IsTrue(h1.Equals(h2));
		}
	}
}
#endif
