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


namespace nunit.linkedlists.hashed
{
	namespace Enumerable
	{
		[TestFixture]
		public class Multiops
		{
			private HashedLinkedList<int> list;

			private Filter<int> always, never, even;


			[SetUp]
			public void Init()
			{
				list = new HashedLinkedList<int>();
				always = delegate{return true;};
				never = delegate{return false;};
				even = delegate(int i){return i%2==0;};
			}


			[Test]
			public void All()
			{
				Assert.IsTrue(list.All(always));
				Assert.IsTrue(list.All(never));
				Assert.IsTrue(list.All(even));
				list.Add(8);
				Assert.IsTrue(list.All(always));
				Assert.IsFalse(list.All(never));
				Assert.IsTrue(list.All(even));
				list.Add(5);
				Assert.IsTrue(list.All(always));
				Assert.IsFalse(list.All(never));
				Assert.IsFalse(list.All(even));
			}


			[Test]
			public void Exists()
			{
				Assert.IsFalse(list.Exists(always));
				Assert.IsFalse(list.Exists(never));
				Assert.IsFalse(list.Exists(even));
				list.Add(5);
				Assert.IsTrue(list.Exists(always));
				Assert.IsFalse(list.Exists(never));
				Assert.IsFalse(list.Exists(even));
				list.Add(8);
				Assert.IsTrue(list.Exists(always));
				Assert.IsFalse(list.Exists(never));
				Assert.IsTrue(list.Exists(even));
			}


			[Test]
			public void Apply()
			{
				int sum = 0;
				Applier<int> a = delegate(int i){sum=i+10*sum;};

				list.Apply(a);
				Assert.AreEqual(0, sum);
				sum = 0;
				list.Add(5);list.Add(8);list.Add(7);list.Add(5);
				list.Apply(a);
				Assert.AreEqual(587, sum);
			}


			[TearDown]
			public void Dispose() { list = null; }
		}



		[TestFixture]
		public class GetEnumerator
		{
			private HashedLinkedList<int> list;


			[SetUp]
			public void Init() { list = new HashedLinkedList<int>(); }


			[Test]
			public void Empty()
			{
				MSG.IEnumerator<int> e = list.GetEnumerator();

				Assert.IsFalse(e.MoveNext());
			}


			[Test]
			public void Normal()
			{
				list.Add(5);
				list.Add(8);
				list.Add(5);
				list.Add(5);
				list.Add(10);
				list.Add(1);

				MSG.IEnumerator<int> e = list.GetEnumerator();

				Assert.IsTrue(e.MoveNext());
				Assert.AreEqual(5, e.Current);
				Assert.IsTrue(e.MoveNext());
				Assert.AreEqual(8, e.Current);
				Assert.IsTrue(e.MoveNext());
				Assert.AreEqual(10, e.Current);
				Assert.IsTrue(e.MoveNext());
				Assert.AreEqual(1, e.Current);
				Assert.IsFalse(e.MoveNext());
			}


			[Test]
			public void DoDispose()
			{
				list.Add(5);
				list.Add(8);
				list.Add(5);

				MSG.IEnumerator<int> e = list.GetEnumerator();

				e.MoveNext();
				e.MoveNext();
				e.Dispose();
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void MoveNextAfterUpdate()
			{
				list.Add(5);
				list.Add(8);
				list.Add(5);

				MSG.IEnumerator<int> e = list.GetEnumerator();

				e.MoveNext();
				list.Add(99);
				e.MoveNext();
			}



			[TearDown]
			public void Dispose() { list = null; }
		}
	}




	namespace CollectionOrSink
	{
		[TestFixture]
		public class CollectionOrSink
		{
			private HashedLinkedList<int> list;


			[SetUp]
			public void Init() { list = new HashedLinkedList<int>(); }


			[Test]
			public void CountEtAl()
			{
				Assert.AreEqual(0, list.Count);
				Assert.IsTrue(list.IsEmpty);
				Assert.IsFalse(list.AllowsDuplicates);
				Assert.IsTrue(list.Add(5));
				Assert.AreEqual(1, list.Count);
				Assert.IsFalse(list.IsEmpty);
				Assert.IsFalse(list.Add(5));
				Assert.AreEqual(1, list.Count);
				Assert.IsFalse(list.IsEmpty);
				Assert.IsTrue(list.Add(8));
				Assert.AreEqual(2, list.Count);
			}


			[Test]
			public void AddAll()
			{
				list.Add(3);list.Add(4);list.Add(5);

				HashedLinkedList<int> list2 = new HashedLinkedList<int>();

				list2.AddAll(list);
				Assert.IsTrue(IC.eq(list2, 3, 4, 5));
				list.AddAll(list2);
				Assert.IsTrue(IC.eq(list2, 3, 4, 5));
				Assert.IsTrue(IC.eq(list, 3, 4, 5));
			}


			[TearDown]
			public void Dispose() { list = null; }
		}



		[TestFixture]
		public class ArrayTest
		{
			private HashedLinkedList<int> list;

			int[] a;


			[SetUp]
			public void Init()
			{
				list = new HashedLinkedList<int>();
				a = new int[10];
				for (int i = 0; i < 10; i++)
					a[i] = 1000 + i;
			}


			[TearDown]
			public void Dispose() { list = null; }


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
				Assert.AreEqual("Alles klar", aeq(list.ToArray()));
				list.Add(7);
				list.Add(8);
				Assert.AreEqual("Alles klar", aeq(list.ToArray(), 7, 8));
			}


			[Test]
			public void CopyTo()
			{
				list.CopyTo(a, 1);
				Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
				list.Add(6);
				list.CopyTo(a, 2);
				Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
				list.Add(4);
				list.Add(5);
				list.Add(9);
				list.CopyTo(a, 4);
				Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 4, 5, 9, 1008, 1009));
				list.Clear();
				list.Add(7);
				list.CopyTo(a, 9);
				Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 6, 4, 5, 9, 1008, 7));
			}


			[Test]
			[ExpectedException(typeof(ArgumentException))]
			public void CopyToBad()
			{
				list.Add(3);
				list.CopyTo(a, 10);
			}


			[Test]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void CopyToBad2()
			{
				list.CopyTo(a, -1);
			}


			[Test]
			[ExpectedException(typeof(ArgumentException))]
			public void CopyToTooFar()
			{
				list.Add(3);
				list.Add(4);
				list.CopyTo(a, 9);
			}
		}



		[TestFixture]
		public class Sync
		{
			private HashedLinkedList<int> list;


			[SetUp]
			public void Init()
			{
				list = new HashedLinkedList<int>();
			}


			[TearDown]
			public void Dispose() { list = null; }


			[Test]
			public void Get()
			{
				Assert.IsNotNull(list.SyncRoot);
			}
		}
	}




	namespace EditableCollection
	{
#if FIXME
		[TestFixture]
		public class Searching
		{
			private HashedLinkedList<int> list;


			[SetUp]
			public void Init() { list = new HashedLinkedList<int>(); }


			[Test]
			public void Contains()
			{
				Assert.IsFalse(list.Contains(5));
				list.Add(5);
				Assert.IsTrue(list.Contains(5));
				Assert.IsFalse(list.Contains(7));
				list.Add(8);
				list.Add(10);
				Assert.IsTrue(list.Contains(5));
				Assert.IsFalse(list.Contains(7));
				Assert.IsTrue(list.Contains(8));
				Assert.IsTrue(list.Contains(10));
				list.Remove(8);
				Assert.IsTrue(list.Contains(5));
				Assert.IsFalse(list.Contains(7));
				Assert.IsFalse(list.Contains(8));
				Assert.IsTrue(list.Contains(10));
			}

			[Test]
			public void BadAdd()
			{
				Assert.IsTrue(list.Add(5));
				Assert.IsTrue(list.Add(8));
				Assert.IsFalse(list.Add(5));
			}


			[Test]
			public void ContainsCount()
			{
				Assert.AreEqual(0, list.ContainsCount(5));
				list.Add(5);
				Assert.AreEqual(1, list.ContainsCount(5));
				Assert.AreEqual(0, list.ContainsCount(7));
				list.Add(8);
				Assert.AreEqual(1, list.ContainsCount(5));
				Assert.AreEqual(0, list.ContainsCount(7));
				Assert.AreEqual(1, list.ContainsCount(8));
			}


			[Test]
			public void RemoveAllCopies()
			{
				list.Add(5);list.Add(7);
				Assert.AreEqual(1, list.ContainsCount(5));
				Assert.AreEqual(1, list.ContainsCount(7));
				list.RemoveAllCopies(5);
				Assert.IsTrue(list.Check());
				Assert.AreEqual(0, list.ContainsCount(5));
				Assert.AreEqual(1, list.ContainsCount(7));
				list.Add(5);list.Add(8);
				list.RemoveAllCopies(8);
				Assert.IsTrue(IC.eq(list, 7, 5));
			}

			[Test]
			public void FindAll()
			{
				Filter<int> f = delegate(int i){return i%2==0;};

				Assert.IsTrue(list.FindAll(f).IsEmpty);
				list.Add(5);list.Add(8);list.Add(10);
				Assert.IsTrue(((LinkedList<int>)list.FindAll(f)).Check());
				Assert.IsTrue(IC.eq(list.FindAll(f), 8, 10));
			}

			[Test]
			public void ContainsAll()
			{
				HashedLinkedList<int> list2 = new HashedLinkedList<int>();

				Assert.IsTrue(list.ContainsAll(list2));
				list2.Add(4);
				Assert.IsFalse(list.ContainsAll(list2));
				list.Add(4);
				Assert.IsTrue(list.ContainsAll(list2));
				list.Add(5);
				Assert.IsTrue(list.ContainsAll(list2));
			}


			[Test]
			public void RetainAll()
			{
				HashedLinkedList<int> list2 = new HashedLinkedList<int>();

				list.Add(4);list.Add(5);list.Add(6);
				list2.Add(5);list2.Add(4);list2.Add(7);
				list.RetainAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 4, 5));
				list.Add(5);list.Add(4);list.Add(6);
				list2.Clear();
				list2.Add(5);list2.Add(5);list2.Add(6);
				list.RetainAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 5, 6));
				list2.Clear();
				list2.Add(7);list2.Add(8);list2.Add(9);
				list.RetainAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list));
			}


			[Test]
			public void RemoveAll()
			{
				HashedLinkedList<int> list2 = new HashedLinkedList<int>();

				list.Add(4);list.Add(5);list.Add(6);
				list2.Add(5);list2.Add(4);list2.Add(7);
				list.RemoveAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 6));
				list.Add(5);list.Add(4);list.Add(6);
				list2.Clear();
				list2.Add(6);list2.Add(5);
				list.RemoveAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 4));
				list2.Clear();
				list2.Add(7);list2.Add(8);list2.Add(9);
				list.RemoveAll(list2);
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 4));
			}

			[Test]
			public void Remove()
			{
				list.Add(4);list.Add(5);list.Add(6);
				Assert.IsFalse(list.Remove(2));
				Assert.IsTrue(list.Check());
				Assert.IsTrue(list.Remove(4));
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 5, 6));
				Assert.AreEqual(6, list.RemoveLast());
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 5));
				list.Add(7);
				Assert.AreEqual(5, list.RemoveFirst());
				Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 7));
			}


			[Test]
			public void Clear()
			{
				list.Add(7);list.Add(6);
				list.Clear();
				Assert.IsTrue(list.IsEmpty);
			}


			[TearDown]
			public void Dispose() { list = null; }
		}
#endif
	}




	namespace IIndexed
	{
		[TestFixture]
		public class Searching
		{
			private IIndexed<int> dit;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
			}


			[Test]
			public void IndexOf()
			{
				Assert.AreEqual(-1, dit.IndexOf(6));
				dit.Add(7);
				Assert.AreEqual(-1, dit.IndexOf(6));
				Assert.AreEqual(-1, dit.LastIndexOf(6));
				Assert.AreEqual(0, dit.IndexOf(7));
				dit.Add(5);dit.Add(7);dit.Add(8);dit.Add(7);
				Assert.AreEqual(-1, dit.IndexOf(6));
				Assert.AreEqual(0, dit.IndexOf(7));
				Assert.AreEqual(0, dit.LastIndexOf(7));
				Assert.AreEqual(2, dit.IndexOf(8));
				Assert.AreEqual(1, dit.LastIndexOf(5));
			}


			[TearDown]
			public void Dispose()
			{
				dit = null;
			}
		}



		[TestFixture]
		public class Removing
		{
			private IIndexed<int> dit;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
			}


			[Test]
			public void RemoveAt()
			{
				dit.Add(5);dit.Add(7);dit.Add(9);dit.Add(1);dit.Add(2);
				Assert.AreEqual(7, dit.RemoveAt(1));
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 5, 9, 1, 2));
				Assert.AreEqual(5, dit.RemoveAt(0));
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 9, 1, 2));
				Assert.AreEqual(2, dit.RemoveAt(2));
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 9, 1));
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void RemoveAtBad0()
			{
				dit.RemoveAt(0);
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void RemoveAtBadM1()
			{
				dit.RemoveAt(-1);
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void RemoveAtBad1()
			{
				dit.Add(8);
				dit.RemoveAt(1);
			}


			[Test]
			public void RemoveInterval()
			{
				dit.RemoveInterval(0, 0);
				dit.Add(10);dit.Add(20);dit.Add(30);dit.Add(40);dit.Add(50);dit.Add(60);
				dit.RemoveInterval(3, 0);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 10, 20, 30, 40, 50, 60));
				dit.RemoveInterval(3, 1);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 10, 20, 30, 50, 60));
				dit.RemoveInterval(1, 3);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 10, 60));
				dit.RemoveInterval(0, 2);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit));
				dit.Add(10);dit.Add(20);dit.Add(30);dit.Add(40);dit.Add(50);dit.Add(60);
				dit.RemoveInterval(0, 2);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 30, 40, 50, 60));
				dit.RemoveInterval(2, 2);
				Assert.IsTrue(((HashedLinkedList<int>)dit).Check());
				Assert.IsTrue(IC.eq(dit, 30, 40));
			}


			[TearDown]
			public void Dispose()
			{
				dit = null;
			}
		}
	}




	namespace IList
	{
		[TestFixture]
		public class Searching
		{
			private IList<int> lst;


			[SetUp]
			public void Init() { lst = new HashedLinkedList<int>(); }


			[TearDown]
			public void Dispose() { lst = null; }


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "List is empty")]
			public void FirstBad()
			{
				int f = lst.First;
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "List is empty")]
			public void LastBad()
			{
				int f = lst.Last;
			}


			[Test]
			public void FirstLast()
			{
				lst.Add(19);
				Assert.AreEqual(19, lst.First);
				Assert.AreEqual(19, lst.Last);
				lst.Add(34);lst.InsertFirst(12);
				Assert.AreEqual(12, lst.First);
				Assert.AreEqual(34, lst.Last);
			}


			[Test]
			public void This()
			{
				lst.Add(34);
				Assert.AreEqual(34, lst[0]);
				lst[0] = 56;
				Assert.AreEqual(56, lst.First);
				lst.Add(7);lst.Add(77);lst.Add(777);lst.Add(7777);
				lst[0] = 45;lst[2] = 78;lst[4] = 101;
				Assert.IsTrue(IC.eq(lst, 45, 7, 78, 777, 101));
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadEmptyGet()
			{
				int f = lst[0];
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadLowGet()
			{
				lst.Add(7);

				int f = lst[-1];
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadHiGet()
			{
				lst.Add(6);

				int f = lst[1];
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadEmptySet()
			{
				lst[0] = 4;
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadLowSet()
			{
				lst.Add(7);
				lst[-1] = 9;
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void ThisBadHiSet()
			{
				lst.Add(6);
				lst[1] = 11;
			}
		}


#if FIXME
		[TestFixture]
		public class Combined
		{
			private IList<KeyValuePair<int,int>> lst;


			[SetUp]
			public void Init()
			{
				lst = new HashedLinkedList<KeyValuePair<int,int>>();
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

				Assert.IsTrue(lst.FindOrAdd(ref p));
				Assert.AreEqual(3, p.key);
				Assert.AreEqual(33, p.value);
				p = new KeyValuePair<int,int>(13, 79);
				Assert.IsFalse(lst.FindOrAdd(ref p));
				Assert.AreEqual(13, lst[10].key);
				Assert.AreEqual(79, lst[10].value);
			}


			[Test]
			public void Update()
			{
				KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);

				Assert.IsTrue(lst.Update(p));
				Assert.AreEqual(3, lst[3].key);
				Assert.AreEqual(78, lst[3].value);
				p = new KeyValuePair<int,int>(13, 78);
				Assert.IsFalse(lst.Update(p));
			}


			[Test]
			public void UpdateOrAdd()
			{
				KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);

				Assert.IsTrue(lst.UpdateOrAdd(p));
				Assert.AreEqual(3, lst[3].key);
				Assert.AreEqual(78, lst[3].value);
				p = new KeyValuePair<int,int>(13, 79);
				Assert.IsFalse(lst.UpdateOrAdd(p));
				Assert.AreEqual(13, lst[10].key);
				Assert.AreEqual(79, lst[10].value);
			}


			[Test]
			public void RemoveWithReturn()
			{
				KeyValuePair<int,int> p = new KeyValuePair<int,int>(3, 78);

				Assert.IsTrue(lst.RemoveWithReturn(ref p));
				Assert.AreEqual(3, p.key);
				Assert.AreEqual(33, p.value);
				Assert.AreEqual(4, lst[3].key);
				Assert.AreEqual(34, lst[3].value);
				p = new KeyValuePair<int,int>(13, 78);
				Assert.IsFalse(lst.RemoveWithReturn(ref p));
			}
		}
#endif

		[TestFixture]
		public class Inserting
		{
			private IList<int> lst;


			[SetUp]
			public void Init() { lst = new HashedLinkedList<int>(); }


			[TearDown]
			public void Dispose() { lst = null; }


			[Test]
			public void Insert()
			{
				lst.Insert(0, 5);
				Assert.IsTrue(IC.eq(lst, 5));
				lst.Insert(0, 7);
				Assert.IsTrue(IC.eq(lst, 7, 5));
				lst.Insert(1, 4);
				Assert.IsTrue(IC.eq(lst, 7, 4, 5));
				lst.Insert(3, 2);
				Assert.IsTrue(IC.eq(lst, 7, 4, 5, 2));
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void BadInsertLow()
			{
				lst.Add(7);
				lst.Insert(-1, 9);
			}


			[Test]
			[ExpectedException(typeof(IndexOutOfRangeException))]
			public void BadInsertHi()
			{
				lst.Add(6);
				lst.Insert(2, 11);
			}


			[Test]
			public void FIFO()
			{
				for (int i = 0; i < 7; i++)
					lst.Add(2 * i);

				Assert.IsTrue(lst.FIFO);
				Assert.AreEqual(0, lst.Remove());
				Assert.AreEqual(2, lst.Remove());
				lst.FIFO = false;
				Assert.AreEqual(12, lst.Remove());
				Assert.AreEqual(10, lst.Remove());
				lst.FIFO = true;
				Assert.AreEqual(4, lst.Remove());
				Assert.AreEqual(6, lst.Remove());
			}


			[Test]
			public void InsertFirstLast()
			{
				lst.InsertFirst(4);
				lst.InsertLast(5);
				lst.InsertFirst(14);
				lst.InsertLast(15);
				lst.InsertFirst(24);
				lst.InsertLast(25);
				lst.InsertFirst(34);
				lst.InsertLast(55);
				Assert.IsTrue(IC.eq(lst, 34, 24, 14, 4, 5, 15, 25, 55));
			}


#if FIXME
			[Test]
			public void InsertBefore()
			{
				lst.Add(2);
				lst.Add(3);
				lst.Add(4);
				lst.Add(5);
				lst.InsertBefore(7, 2);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 2, 3, 4, 5));
				lst.InsertBefore(8, 3);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 2, 8, 3, 4, 5));
				lst.InsertBefore(9, 5);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 2, 8, 3, 4, 9, 5));
			}
#endif


			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertBefore()
			{
				lst.Add(2);
				lst.Add(3);
				lst.Add(2);
				lst.Add(5);
				lst.InsertBefore(7, 4);
			}


#if FIXME
			[Test]
			public void InsertAfter()
			{
				lst.Add(1);
				lst.Add(2);
				lst.Add(3);
				lst.Add(4);
				lst.Add(5);
				lst.InsertAfter(7, 2);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 1, 2, 7, 3, 4, 5));
				lst.InsertAfter(8, 1);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 1, 8, 2, 7, 3, 4, 5));
				lst.InsertAfter(9, 5);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 1, 8, 2, 7, 3, 4, 5, 9));
			}
#endif


			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertAfter()
			{
				lst.Add(2);
				lst.Add(3);
				lst.Add(6);
				lst.Add(5);
				lst.InsertAfter(7, 4);
			}


			[Test]
			public void InsertAll()
			{
				lst.Add(1);
				lst.Add(2);
				lst.Add(3);
				lst.Add(4);

				IList<int> lst2 = new HashedLinkedList<int>();

				lst2.Add(7);lst2.Add(8);lst2.Add(9);
				lst.InsertAll(0, lst2);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 8, 9, 1, 2, 3, 4));
				lst.RemoveAll(lst2);
				lst.InsertAll(4, lst2);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 1, 2, 3, 4, 7, 8, 9));
				lst.RemoveAll(lst2);
				lst.InsertAll(2, lst2);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 1, 2, 7, 8, 9, 3, 4));
			}


#if FIXME
			[Test]
			public void Map()
			{
				Mapper<int,string> m = delegate(int i){return "<<"+i+">>";};
				IList<string> r = lst.Map(m);

				Assert.IsTrue(((LinkedList<string>)r).Check());
				Assert.IsTrue(r.IsEmpty);
				lst.Add(1);
				lst.Add(2);
				lst.Add(3);
				lst.Add(4);
				r = lst.Map(m);
				Assert.IsTrue(((LinkedList<string>)r).Check());
				Assert.AreEqual(4, r.Count);
				for (int i = 0; i < 4; i++)
					Assert.AreEqual("<<" + (i + 1) + ">>", r[i]);
			}
#endif


			[Test]
			public void RemoveFirstLast()
			{
				lst.Add(1);
				lst.Add(2);
				lst.Add(3);
				lst.Add(4);
				Assert.AreEqual(1, lst.RemoveFirst());
				Assert.AreEqual(4, lst.RemoveLast());
				Assert.AreEqual(2, lst.RemoveFirst());
				Assert.AreEqual(3, lst.RemoveLast());
				Assert.IsTrue(lst.IsEmpty);
			}


			[Test]
			public void Reverse()
			{
				for (int i = 0; i < 10; i++)
					lst.Add(i);

				lst.Reverse();
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
				lst.Reverse(0, 3);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 2, 1, 0));
				lst.Reverse(7, 0);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 2, 1, 0));
				lst.Reverse(7, 3);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 0, 1, 2));
				lst.Reverse(5, 1);
				Assert.IsTrue(lst.Check());
				Assert.IsTrue(IC.eq(lst, 7, 8, 9, 6, 5, 4, 3, 0, 1, 2));
			}


			[Test]
			[ExpectedException(typeof(ArgumentException))]
			public void BadReverse()
			{
				for (int i = 0; i < 10; i++)
					lst.Add(i);

				lst.Reverse(8, 3);
			}
		}



		[TestFixture]
		public class Sorting
		{
			private IList<int> lst;


			[SetUp]
			public void Init() { lst = new HashedLinkedList<int>(); }


			[TearDown]
			public void Dispose() { lst = null; }


			[Test]
			public void Sort()
			{
				lst.Add(5);lst.Add(6);lst.Add(55);lst.Add(7);lst.Add(3);
				Assert.IsFalse(lst.IsSorted(new IC()));
				lst.Sort(new IC());
				Assert.IsTrue(lst.IsSorted(new IC()));
				Assert.IsTrue(IC.eq(lst, 3, 5, 6, 7, 55));
			}
		}
	}




	namespace Range
	{
		[TestFixture]
		public class Range
		{
			private IList<int> lst;


			[SetUp]
			public void Init() { lst = new HashedLinkedList<int>(); }


			[TearDown]
			public void Dispose() { lst = null; }


			[Test]
			public void GetRange()
			{
				//Assert.IsTrue(IC.eq(lst[0, 0)));
				for (int i = 0; i < 10; i++) lst.Add(i);

				Assert.IsTrue(IC.eq(lst[0, 3], 0, 1, 2));
				Assert.IsTrue(IC.eq(lst[3, 3], 3, 4, 5));
				Assert.IsTrue(IC.eq(lst[6, 3], 6, 7, 8));
				Assert.IsTrue(IC.eq(lst[6, 4], 6, 7, 8, 9));
			}


			[Test]
			public void Backwards()
			{
				for (int i = 0; i < 10; i++) lst.Add(i);

				Assert.IsTrue(IC.eq(lst.Backwards(), 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
				Assert.IsTrue(IC.eq(lst[0, 3].Backwards(), 2, 1, 0));
				Assert.IsTrue(IC.eq(lst[3, 3].Backwards(),  5, 4, 3));
				Assert.IsTrue(IC.eq(lst[6, 4].Backwards(),  9, 8, 7, 6));
			}


			[Test]
			public void DirectionAndCount()
			{
				for (int i = 0; i < 10; i++) lst.Add(i);

				Assert.AreEqual(EnumerationDirection.Forwards, lst.Direction);
				Assert.AreEqual(EnumerationDirection.Forwards, lst[3, 7].Direction);
				Assert.AreEqual(EnumerationDirection.Backwards, lst[3, 7].Backwards().Direction);
				Assert.AreEqual(EnumerationDirection.Backwards, lst.Backwards().Direction);
				Assert.AreEqual(4, lst[3, 4].Count);
				Assert.AreEqual(4, lst[3, 4].Backwards().Count);
				Assert.AreEqual(10, lst.Backwards().Count);
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException))]
			public void MoveNextAfterUpdate()
			{
				for (int i = 0; i < 10; i++) lst.Add(i);

				foreach (int i in lst)
				{
					lst.Add(45 + i);
				}
			}
		}
	}




	namespace View
	{
		[TestFixture]
		public class Simple
		{
			HashedLinkedList<int> list;
			LinkedList<int> view;


			[SetUp]
			public void Init()
			{
				list = new HashedLinkedList<int>();
				list.Add(0);list.Add(1);list.Add(2);list.Add(3);
				view = (LinkedList<int>)list.View(1, 2);
			}


			[TearDown]
			public void Dispose()
			{
				list = null;
				view = null;
			}


			void check()
			{
				Assert.IsTrue(list.Check());
				Assert.IsTrue(view.Check());
			}

			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertAfterLow()
			{
				view.InsertAfter(876, 0);
			}


			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertAfterHigh()
			{
				view.InsertAfter(876, 0);
			}

			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertBeforeLow()
			{
				view.InsertBefore(876, 0);
			}


			[Test]
			[ExpectedException(typeof(ArgumentException), "Target item not found")]
			public void BadInsertBeforeHigh()
			{
				view.InsertBefore(876, 0);
			}


			[Test]
			public void Add()
			{
				check();
				Assert.IsTrue(IC.eq(list, 0, 1, 2, 3));
				Assert.IsTrue(IC.eq(view, 1, 2));
				view.InsertFirst(10);
				check();
				Assert.IsTrue(IC.eq(list, 0, 10, 1, 2, 3));
				Assert.IsTrue(IC.eq(view, 10, 1, 2));
				view.Clear();
				Assert.IsFalse(view.IsReadOnly);
				Assert.IsFalse(view.AllowsDuplicates);
				Assert.IsTrue(view.IsEmpty);
				check();
				Assert.IsTrue(IC.eq(list, 0, 3));
				Assert.IsTrue(IC.eq(view));
				view.Add(8);
				Assert.IsFalse(view.IsEmpty);
				Assert.IsFalse(view.AllowsDuplicates);
				Assert.IsFalse(view.IsReadOnly);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 3));
				Assert.IsTrue(IC.eq(view, 8));
				view.Add(12);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 12, 3));
				Assert.IsTrue(IC.eq(view, 8, 12));
				view.InsertAfter(15, 12);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 12, 15, 3));
				Assert.IsTrue(IC.eq(view, 8, 12, 15));
				view.InsertBefore(18, 12);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 3));
				Assert.IsTrue(IC.eq(view, 8, 18, 12, 15));

				HashedLinkedList<int> lst2 = new HashedLinkedList<int>();

				lst2.Add(90);lst2.Add(92);
				view.AddAll(lst2);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 90, 92, 3));
				Assert.IsTrue(IC.eq(view, 8, 18, 12, 15, 90, 92));
				view.InsertLast(66);
				check();
				Assert.IsTrue(IC.eq(list, 0, 8, 18, 12, 15, 90, 92, 66, 3));
				Assert.IsTrue(IC.eq(view, 8, 18, 12, 15, 90, 92, 66));
			}


			[Test]
			public void Bxxx()
			{
				Assert.IsTrue(IC.eq(view.Backwards(), 2, 1));
                Assert.AreSame(list, view.Underlying);
                Assert.IsNull(list.Underlying);
                Assert.AreEqual(EnumerationDirection.Forwards, view.Direction);
				Assert.AreEqual(EnumerationDirection.Backwards, view.Backwards().Direction);
				Assert.AreEqual(0,list.Offset);
				Assert.AreEqual(1,view.Offset);
			}


			[Test]
			public void Contains()
			{
				Assert.IsTrue(view.Contains(1));
				Assert.IsFalse(view.Contains(0));

				HashedLinkedList<int> lst2 = new HashedLinkedList<int>();

				lst2.Add(2);
				Assert.IsTrue(view.ContainsAll(lst2));
				lst2.Add(3);
				Assert.IsFalse(view.ContainsAll(lst2));
				Assert.AreEqual(Speed.Constant, view.ContainsSpeed);
				Assert.AreEqual(2, view.Count);
				view.Add(1);
				Assert.AreEqual(1, view.ContainsCount(2));
				Assert.AreEqual(1, view.ContainsCount(1));
				Assert.AreEqual(2, view.Count);
			}


			[Test]
			public void CreateView()
			{
				LinkedList<int> view2 = (LinkedList<int>)view.View(1, 0);

                Assert.AreSame(list, view2.Underlying);
            }


			[Test]
			public void FIFO()
			{
				Assert.IsTrue(view.FIFO);
				view.Add(23);view.Add(24);view.Add(25);
				check();
				Assert.IsTrue(IC.eq(view, 1, 2, 23, 24, 25));
				Assert.AreEqual(1, view.Remove());
				check();
				Assert.IsTrue(IC.eq(view, 2, 23, 24, 25));
				view.FIFO = false;
				Assert.IsFalse(view.FIFO);
				Assert.AreEqual(25, view.Remove());
				check();
				Assert.IsTrue(IC.eq(view, 2, 23, 24));
			}


			[Test]
			public void MapEtc()
			{
#if FIXME
				LinkedList<double> dbl = (LinkedList<double>)view.Map(new Mapper<int,double>(delegate(int i){return i/10.0;}));

				Assert.IsTrue(dbl.Check());
				Assert.AreEqual(0.1, dbl[0]);
				Assert.AreEqual(0.2, dbl[1]);
				for (int i = 0; i < 10; i++) view.Add(i);

				LinkedList<int> list2 = (LinkedList<int>)view.FindAll(new Filter<int>(delegate(int i){return i%4==1;}));

				Assert.IsTrue(list2.Check());
				Assert.IsTrue(IC.eq(list2, 1, 5, 9));
#endif
			}


			[Test]
			public void FL()
			{
				Assert.AreEqual(1, view.First);
				Assert.AreEqual(2, view.Last);
			}


			[Test]
			public void Indexing()
			{
				list.Clear();
				for (int i = 0; i < 20; i++) list.Add(i);

				view = (LinkedList<int>)list.View(5, 7);
				for (int i = 0; i < 7; i++) Assert.AreEqual(i + 5, view[i]);

				for (int i = 0; i < 7; i++) Assert.AreEqual(i, view.IndexOf(i + 5));

				for (int i = 0; i < 7; i++) Assert.AreEqual(i, view.LastIndexOf(i + 5));
			}


			[Test]
			public void Insert()
			{
				view.Insert(0, 34);
				view.Insert(1, 35);
				view.Insert(4, 36);
				Assert.IsTrue(view.Check());
				Assert.IsTrue(IC.eq(view, 34, 35, 1, 2, 36));

				IList<int> list2 = new HashedLinkedList<int>();

				list2.Add(40);list2.Add(41);
				view.InsertAll(3, list2);
				Assert.IsTrue(view.Check());
				Assert.IsTrue(IC.eq(view, 34, 35, 1, 40, 41, 2, 36));
			}


			[Test]
			public void Sort()
			{
				view.Add(45);view.Add(47);view.Add(46);view.Add(48);
				Assert.IsFalse(view.IsSorted(new IC()));
				view.Sort(new IC());
				check();
				Assert.IsTrue(IC.eq(list, 0, 1, 2, 45, 46, 47, 48, 3));
				Assert.IsTrue(IC.eq(view, 1, 2, 45, 46, 47, 48));
			}


			[Test]
			public void Remove()
			{
				view.Add(1);view.Add(5);view.Add(3);view.Add(1);view.Add(3);view.Add(0);
				Assert.IsTrue(IC.eq(view, 1, 2, 5));
				Assert.IsTrue(view.Remove(1));
				check();
				Assert.IsTrue(IC.eq(view, 2, 5));
				Assert.IsFalse(view.Remove(1));
				check();
				Assert.IsTrue(IC.eq(view, 2, 5));
				Assert.IsFalse(view.Remove(0));
				check();
				Assert.IsTrue(IC.eq(view, 2, 5));
				view.RemoveAllCopies(3);
				check();
				Assert.IsTrue(IC.eq(view, 2, 5));
				Assert.IsTrue(IC.eq(list, 0, 2, 5, 3));
				view.Add(1);view.Add(5);view.Add(3);view.Add(1);view.Add(3);view.Add(0);
				Assert.IsTrue(IC.eq(view, 2, 5, 1));

				HashedLinkedList<int> l2 = new HashedLinkedList<int>();

				l2.Add(1);l2.Add(2);l2.Add(2);l2.Add(3);l2.Add(1);
				view.RemoveAll(l2);
				check();
				Assert.IsTrue(IC.eq(view, 5));
				view.RetainAll(l2);
				check();
				Assert.IsTrue(IC.eq(view));
				view.Add(2);view.Add(4);view.Add(5);
				Assert.AreEqual(2, view.RemoveAt(0));
				Assert.AreEqual(5, view.RemoveAt(1));
				Assert.AreEqual(4, view.RemoveAt(0));
				check();
				Assert.IsTrue(IC.eq(view));
				view.Add(8);view.Add(6);view.Add(78);
				Assert.AreEqual(8, view.RemoveFirst());
				Assert.AreEqual(78, view.RemoveLast());
				view.Add(2);view.Add(5);view.Add(3);view.Add(1);
				view.RemoveInterval(1, 2);
				check();
				Assert.IsTrue(IC.eq(view, 6, 1));
			}


			[Test]
			public void Reverse()
			{
				view.Clear();
				for (int i = 0; i < 10; i++) view.Add(10 + i);

				view.Reverse(3, 4);
				check();
				Assert.IsTrue(IC.eq(view, 10, 11, 12, 16, 15, 14, 13, 17, 18, 19));
				view.Reverse();
				Assert.IsTrue(IC.eq(view, 19, 18, 17, 13, 14, 15, 16, 12, 11, 10));
				Assert.IsTrue(IC.eq(list, 0, 19, 18, 17, 13, 14, 15, 16, 12, 11, 10, 3));
			}


			[Test]
			public void Slide()
			{
				view.Slide(1);
				check();
				Assert.IsTrue(IC.eq(view, 2, 3));
				view.Slide(-2);
				check();
				Assert.IsTrue(IC.eq(view, 0, 1));
				view.Slide(0, 3);
				check();
				Assert.IsTrue(IC.eq(view, 0, 1, 2));
				view.Slide(2, 1);
				check();
				Assert.IsTrue(IC.eq(view, 2));
				view.Slide(-1, 0);
				check();
				Assert.IsTrue(IC.eq(view));
				view.Add(28);
				Assert.IsTrue(IC.eq(list, 0, 28, 1, 2, 3));
			}
			[Test]
			public void Iterate()
			{
				list.Clear();
				view = null;
				foreach (int i in new int[] { 2, 4, 8, 13, 6, 1, 10, 11 }) list.Add(i);

				view = (LinkedList<int>)list.View(list.Count - 2, 2);
				int j = 666;
				while (true)
				{
					//Console.WriteLine("View: {0}:  {1} --> {2}", view.Count, view.First, view.Last);
					if ((view.Last - view.First) % 2 == 1)
						view.Insert(1, j++);
					check();
					if (view.Offset == 0)
						break;
					else
						view.Slide(-1,2);
				}
				//foreach (int i in list) Console.Write(" " + i);
				//Assert.IsTrue(list.Check());
				Assert.IsTrue(IC.eq(list, 2, 4, 8, 668, 13, 6, 1, 667, 10, 666, 11));
			}


			[Test]
			public void SyncRoot()
			{
				Assert.AreSame(view.SyncRoot, list.SyncRoot);
			}
		}
	}

	namespace HashingAndEquals
	{
		[TestFixture]
		public class ISequenced
		{
			private ISequenced<int> dit, dat, dut;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
			}


			[Test]
			public void EmptyEmpty()
			{
				Assert.IsTrue(dit.Equals(dat));
			}


			[Test]
			public void EmptyNonEmpty()
			{
				dit.Add(3);
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsFalse(dat.Equals(dit));
			}


			public int hasher(params int[] items)
			{
				int retval = 0;

				foreach (int i in items)
					retval = retval * 31 + i;

				return retval;
			}


			[Test]
			public void HashVal()
			{
				Assert.AreEqual(hasher(), dit.GetHashCode());
				dit.Add(3);
				Assert.AreEqual(hasher(3), dit.GetHashCode());
				dit.Add(7);
				Assert.AreEqual(hasher(3, 7), dit.GetHashCode());
				Assert.AreEqual(hasher(), dut.GetHashCode());
				dut.Add(7);
				Assert.AreEqual(hasher(7), dut.GetHashCode());
				dut.Add(3);
				Assert.AreEqual(hasher(7, 3), dut.GetHashCode());
			}


			[Test]
			public void EqualHashButDifferent()
			{
				dit.Add(0);dit.Add(31);
				dat.Add(1);dat.Add(0);
				Assert.AreEqual(dit.GetHashCode(), dat.GetHashCode());
				Assert.IsFalse(dit.Equals(dat));
			}


			[Test]
			public void Normal()
			{
				dit.Add(3);
				dit.Add(7);
				dat.Add(3);
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsFalse(dat.Equals(dit));
				dat.Add(7);
				Assert.IsTrue(dit.Equals(dat));
				Assert.IsTrue(dat.Equals(dit));
			}


			[Test]
			public void WrongOrder()
			{
				dit.Add(3);
				dut.Add(3);
				Assert.IsTrue(dit.Equals(dut));
				Assert.IsTrue(dut.Equals(dit));
				dit.Add(7);
				((HashedLinkedList<int>)dut).InsertFirst(7);
				Assert.IsFalse(dit.Equals(dut));
				Assert.IsFalse(dut.Equals(dit));
			}


			[Test]
			public void Reflexive()
			{
				Assert.IsTrue(dit.Equals(dit));
				dit.Add(3);
				Assert.IsTrue(dit.Equals(dit));
				dit.Add(7);
				Assert.IsTrue(dit.Equals(dit));
			}


			[TearDown]
			public void Dispose()
			{
				dit = null;
				dat = null;
				dut = null;
			}
		}



		[TestFixture]
		public class IEditableCollection
		{
			private ICollection<int> dit, dat, dut;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
			}


			[Test]
			public void EmptyEmpty()
			{
				Assert.IsTrue(dit.Equals(dat));
			}


			[Test]
			public void EmptyNonEmpty()
			{
				dit.Add(3);
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsFalse(dat.Equals(dit));
			}


			public int hasher(int count, params int[] items)
			{
				int retval = 0;

				foreach (int i in items)
					retval ^= i;

				return (count << 16) + retval;
			}


			[Test]
			public void HashVal()
			{
				Assert.AreEqual(hasher(0), dit.GetHashCode());
				dit.Add(3);
				Assert.AreEqual(hasher(1, 3), dit.GetHashCode());
				dit.Add(7);
				Assert.AreEqual(hasher(2, 3, 7), dit.GetHashCode());
				Assert.AreEqual(hasher(0), dut.GetHashCode());
				dut.Add(3);
				Assert.AreEqual(hasher(1, 3), dut.GetHashCode());
				dut.Add(7);
				Assert.AreEqual(hasher(2, 7, 3), dut.GetHashCode());
			}


			[Test]
			public void EqualHashButDifferent()
			{
				dit.Add(2);dit.Add(1);
				dat.Add(3);dat.Add(0);
				Assert.AreEqual(dit.GetHashCode(), dat.GetHashCode());
				Assert.IsFalse(dit.Equals(dat));
			}


			[Test]
			public void Normal()
			{
				dit.Add(3);
				dit.Add(7);
				dat.Add(3);
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsFalse(dat.Equals(dit));
				dat.Add(7);
				Assert.IsTrue(dit.Equals(dat));
				Assert.IsTrue(dat.Equals(dit));
			}


			[Test]
			public void WrongOrder()
			{
				dit.Add(3);
				dut.Add(3);
				Assert.IsTrue(dit.Equals(dut));
				Assert.IsTrue(dut.Equals(dit));
				dit.Add(7);
				dut.Add(7);
				Assert.IsTrue(dit.Equals(dut));
				Assert.IsTrue(dut.Equals(dit));
			}


			[Test]
			public void Reflexive()
			{
				Assert.IsTrue(dit.Equals(dit));
				dit.Add(3);
				Assert.IsTrue(dit.Equals(dit));
				dit.Add(7);
				Assert.IsTrue(dit.Equals(dit));
			}


			[TearDown]
			public void Dispose()
			{
				dit = null;
				dat = null;
				dut = null;
			}
		}



		[TestFixture]
		public class MultiLevelUnorderedOfUnOrdered
		{
			private ICollection<int> dit, dat, dut;

			private ICollection<ICollection<int>> Dit, Dat, Dut;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
				dit.Add(2);dit.Add(1);
				dat.Add(1);dat.Add(2);
				dut.Add(3);
				Dit = new HashedLinkedList<ICollection<int>>();
				Dat = new HashedLinkedList<ICollection<int>>();
				Dut = new HashedLinkedList<ICollection<int>>();
			}


			[Test]
			public void Check()
			{
				Assert.IsTrue(dit.Equals(dat));
				Assert.IsFalse(dit.Equals(dut));
			}


			[Test]
			public void Multi()
			{
				Dit.Add(dit);Dit.Add(dut);Dit.Add(dit);
				Dat.Add(dut);Dat.Add(dit);Dat.Add(dat);
				Assert.IsTrue(Dit.Equals(Dat));
				Assert.IsFalse(Dit.Equals(Dut));
			}


			[TearDown]
			public void Dispose()
			{
				dit = dat = dut = null;
				Dit = Dat = Dut = null;
			}
		}



		[TestFixture]
		public class MultiLevelOrderedOfUnOrdered
		{
			private ICollection<int> dit, dat, dut;

			private ISequenced<ICollection<int>> Dit, Dat, Dut;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
				dit.Add(2);dit.Add(1);
				dat.Add(1);dat.Add(2);
				dut.Add(3);
				Dit = new HashedLinkedList<ICollection<int>>();
				Dat = new HashedLinkedList<ICollection<int>>();
				Dut = new HashedLinkedList<ICollection<int>>();
			}


			[Test]
			public void Check()
			{
				Assert.IsTrue(dit.Equals(dat));
				Assert.IsFalse(dit.Equals(dut));
			}


			[Test]
			public void Multi()
			{
				Dit.Add(dit);Dit.Add(dut);Dit.Add(dit);
				Dat.Add(dut);Dat.Add(dit);Dat.Add(dat);
				Dut.Add(dit);Dut.Add(dut);Dut.Add(dat);
				Assert.IsFalse(Dit.Equals(Dat));
				Assert.IsTrue(Dit.Equals(Dut));
			}


			[TearDown]
			public void Dispose()
			{
				dit = dat = dut = null;
				Dit = Dat = Dut = null;
			}
		}



		[TestFixture]
		public class MultiLevelUnOrderedOfOrdered
		{
			private ISequenced<int> dit, dat, dut, dot;

			private ICollection<ISequenced<int>> Dit, Dat, Dut, Dot;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
				dot = new HashedLinkedList<int>();
				dit.Add(2);dit.Add(1);
				dat.Add(1);dat.Add(2);
				dut.Add(3);
				dot.Add(2);dot.Add(1);
				Dit = new HashedLinkedList<ISequenced<int>>();
				Dat = new HashedLinkedList<ISequenced<int>>();
				Dut = new HashedLinkedList<ISequenced<int>>();
				Dot = new HashedLinkedList<ISequenced<int>>();
			}


			[Test]
			public void Check()
			{
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsTrue(dit.Equals(dot));
				Assert.IsFalse(dit.Equals(dut));
			}


			[Test]
			public void Multi()
			{
				Dit.Add(dit);Dit.Add(dut);Dit.Add(dit);
				Dat.Add(dut);Dat.Add(dit);Dat.Add(dat);
				Dut.Add(dot);Dut.Add(dut);Dut.Add(dit);
				Dot.Add(dit);Dot.Add(dit);Dot.Add(dut);
				Assert.IsTrue(Dit.Equals(Dut));
				Assert.IsFalse(Dit.Equals(Dat));
				Assert.IsTrue(Dit.Equals(Dot));
			}


			[TearDown]
			public void Dispose()
			{
				dit = dat = dut = dot = null;
				Dit = Dat = Dut = Dot = null;
			}
		}



		[TestFixture]
		public class MultiLevelOrderedOfOrdered
		{
			private ISequenced<int> dit, dat, dut, dot;

			private ISequenced<ISequenced<int>> Dit, Dat, Dut, Dot;


			[SetUp]
			public void Init()
			{
				dit = new HashedLinkedList<int>();
				dat = new HashedLinkedList<int>();
				dut = new HashedLinkedList<int>();
				dot = new HashedLinkedList<int>();
				dit.Add(2);dit.Add(1); //{2,1}
				dat.Add(1);dat.Add(2); //{1,2}
				dut.Add(3);            //{3}
				dot.Add(2);dot.Add(1); //{2,1}
				Dit = new HashedLinkedList<ISequenced<int>>();
				Dat = new HashedLinkedList<ISequenced<int>>();
				Dut = new HashedLinkedList<ISequenced<int>>();
				Dot = new HashedLinkedList<ISequenced<int>>();
			}


			[Test]
			public void Check()
			{
				Assert.IsFalse(dit.Equals(dat));
				Assert.IsTrue(dit.Equals(dot));
				Assert.IsFalse(dit.Equals(dut));
			}


			[Test]
			public void Multi()
			{
				Dit.Add(dit);Dit.Add(dut);Dit.Add(dit); // {{2,1},{3}}
				Dat.Add(dut);Dat.Add(dit);Dat.Add(dat); // {{3},{2,1},{1,2}}
				Dut.Add(dot);Dut.Add(dut);Dut.Add(dit); // {{2,1},{3}}
				Dot.Add(dit);Dot.Add(dit);Dot.Add(dut); // {{2,1},{3}}
				Assert.IsTrue(Dit.Equals(Dut));
				Assert.IsFalse(Dit.Equals(Dat));
				Assert.IsTrue(Dit.Equals(Dot));
			}


			[TearDown]
			public void Dispose()
			{
				dit = dat = dut = dot = null;
				Dit = Dat = Dut = Dot = null;
			}
		}
	}
}
#endif
