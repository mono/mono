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

namespace nunit.arrays.sorted
{
	[TestFixture]
	public class Ranges
	{
		private SortedArray<int> array;

		private IComparer<int> c;


		[SetUp]
		public void Init()
		{
			c = new IC();
			array = new SortedArray<int>(c);
			for (int i = 1; i <= 10; i++)
			{
				array.Add(i * 2);
			}
		}


		[Test]
		public void Enumerator()
		{
			MSG.IEnumerator<int> e = array.RangeFromTo(5, 17).GetEnumerator();
			int i = 3;

			while (e.MoveNext())
			{
				Assert.AreEqual(2 * i++, e.Current);
			}

			Assert.AreEqual(9, i);
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Enumerator3()
		{
			MSG.IEnumerator<int> e = array.RangeFromTo(5, 17).GetEnumerator();

			e.MoveNext();
			array.Add(67);
			e.MoveNext();
		}


		[Test]
		public void Remove()
		{
			int[] all = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

			array.RemoveRangeFrom(18);
			Assert.IsTrue(IC.eq(array, new int[] { 2, 4, 6, 8, 10, 12, 14, 16 }));
			array.RemoveRangeFrom(28);
			Assert.IsTrue(IC.eq(array, new int[] { 2, 4, 6, 8, 10, 12, 14, 16 }));
			array.RemoveRangeFrom(13);
			Assert.IsTrue(IC.eq(array, new int[] { 2, 4, 6, 8, 10, 12 }));
			array.RemoveRangeFrom(2);
			Assert.IsTrue(IC.eq(array));
			foreach (int i in all) array.Add(i);

			array.RemoveRangeTo(10);
			Assert.IsTrue(IC.eq(array, new int[] { 10, 12, 14, 16, 18, 20 }));
			array.RemoveRangeTo(2);
			Assert.IsTrue(IC.eq(array, new int[] { 10, 12, 14, 16, 18, 20 }));
			array.RemoveRangeTo(21);
			Assert.IsTrue(IC.eq(array));
			foreach (int i in all) array.Add(i);

			array.RemoveRangeFromTo(4, 8);
			Assert.IsTrue(IC.eq(array, 2, 8, 10, 12, 14, 16, 18, 20));
			array.RemoveRangeFromTo(14, 28);
			Assert.IsTrue(IC.eq(array, 2, 8, 10, 12));
			array.RemoveRangeFromTo(0, 9);
			Assert.IsTrue(IC.eq(array, 10, 12));
			array.RemoveRangeFromTo(0, 81);
			Assert.IsTrue(IC.eq(array));
		}

		[Test]
		public void Normal()
		{
			int[] all = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };

			Assert.IsTrue(IC.eq(array, all));
			Assert.IsTrue(IC.eq(array.RangeAll(), all));
			Assert.AreEqual(10, array.RangeAll().Count);
			Assert.IsTrue(IC.eq(array.RangeFrom(11), new int[] { 12, 14, 16, 18, 20 }));
			Assert.AreEqual(5, array.RangeFrom(11).Count);
			Assert.IsTrue(IC.eq(array.RangeFrom(12), new int[] { 12, 14, 16, 18, 20 }));
			Assert.IsTrue(IC.eq(array.RangeFrom(2), all));
			Assert.IsTrue(IC.eq(array.RangeFrom(1), all));
			Assert.IsTrue(IC.eq(array.RangeFrom(21), new int[] { }));
			Assert.IsTrue(IC.eq(array.RangeFrom(20), new int[] { 20 }));
			Assert.IsTrue(IC.eq(array.RangeTo(8), new int[] { 2, 4, 6 }));
			Assert.IsTrue(IC.eq(array.RangeTo(7), new int[] { 2, 4, 6 }));
			Assert.AreEqual(3, array.RangeTo(7).Count);
			Assert.IsTrue(IC.eq(array.RangeTo(2), new int[] { }));
			Assert.IsTrue(IC.eq(array.RangeTo(1), new int[] {  }));
			Assert.IsTrue(IC.eq(array.RangeTo(3), new int[] { 2 }));
			Assert.IsTrue(IC.eq(array.RangeTo(20), new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18 }));
			Assert.IsTrue(IC.eq(array.RangeTo(21), all));
			Assert.IsTrue(IC.eq(array.RangeFromTo(7, 12), new int[] { 8, 10 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 11), new int[] { 6, 8, 10 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(1, 12), new int[] { 2, 4, 6, 8, 10 }));
			Assert.AreEqual(5, array.RangeFromTo(1, 12).Count);
			Assert.IsTrue(IC.eq(array.RangeFromTo(2, 12), new int[] { 2, 4, 6, 8, 10 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 21), new int[] { 6, 8, 10, 12, 14, 16, 18, 20 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 20), new int[] { 6, 8, 10, 12, 14, 16, 18 }));
		}


		[Test]
		public void Backwards()
		{
			int[] all = new int[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20 };
			int[] lla = new int[] { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2 };

			Assert.IsTrue(IC.eq(array, all));
			Assert.IsTrue(IC.eq(array.RangeAll().Backwards(), lla));
			Assert.IsTrue(IC.eq(array.RangeFrom(11).Backwards(), new int[] { 20, 18, 16, 14, 12 }));
            Assert.IsTrue(IC.eq(array.RangeFrom(12).Backwards(), new int[] { 20, 18, 16, 14, 12 }));
			Assert.IsTrue(IC.eq(array.RangeFrom(2).Backwards(), lla));
			Assert.IsTrue(IC.eq(array.RangeFrom(1).Backwards(), lla));
			Assert.IsTrue(IC.eq(array.RangeFrom(21).Backwards(), new int[] { }));
			Assert.IsTrue(IC.eq(array.RangeFrom(20).Backwards(), new int[] { 20 }));
			Assert.IsTrue(IC.eq(array.RangeTo(8).Backwards(), new int[] { 6, 4, 2 }));
			Assert.IsTrue(IC.eq(array.RangeTo(7).Backwards(), new int[] { 6, 4, 2 }));
			Assert.IsTrue(IC.eq(array.RangeTo(2).Backwards(), new int[] { }));
			Assert.IsTrue(IC.eq(array.RangeTo(1).Backwards(), new int[] {  }));
			Assert.IsTrue(IC.eq(array.RangeTo(3).Backwards(), new int[] { 2 }));
			Assert.IsTrue(IC.eq(array.RangeTo(20).Backwards(), new int[] { 18, 16, 14, 12, 10, 8, 6, 4, 2}));
			Assert.IsTrue(IC.eq(array.RangeTo(21).Backwards(), lla));
			Assert.IsTrue(IC.eq(array.RangeFromTo(7, 12).Backwards(), new int[] { 10, 8 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 11).Backwards(), new int[] { 10, 8, 6 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(1, 12).Backwards(), new int[] { 10, 8, 6, 4, 2 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(2, 12).Backwards(), new int[] { 10, 8, 6, 4, 2 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 21).Backwards(), new int[] { 20, 18, 16, 14, 12, 10, 8, 6 }));
			Assert.IsTrue(IC.eq(array.RangeFromTo(6, 20).Backwards(), new int[] { 18, 16, 14, 12, 10, 8, 6 }));
		}

		[Test]
		public void Direction()
		{
			Assert.AreEqual(EnumerationDirection.Forwards, array.Direction);
			Assert.AreEqual(EnumerationDirection.Forwards, array.RangeFrom(20).Direction);
			Assert.AreEqual(EnumerationDirection.Forwards, array.RangeTo(7).Direction);
			Assert.AreEqual(EnumerationDirection.Forwards, array.RangeFromTo(1, 12).Direction);
			Assert.AreEqual(EnumerationDirection.Forwards, array.RangeAll().Direction);
			Assert.AreEqual(EnumerationDirection.Backwards, array.Backwards().Direction);
			Assert.AreEqual(EnumerationDirection.Backwards, array.RangeFrom(20).Backwards().Direction);
			Assert.AreEqual(EnumerationDirection.Backwards, array.RangeTo(7).Backwards().Direction);
			Assert.AreEqual(EnumerationDirection.Backwards, array.RangeFromTo(1, 12).Backwards().Direction);
			Assert.AreEqual(EnumerationDirection.Backwards, array.RangeAll().Backwards().Direction);
		}


		[TearDown]
		public void Dispose()
		{
			array = null;
			c = null;
		}
	}

	[TestFixture]
	public class BagItf
	{
		private SortedArray<int> array;


		[SetUp]
		public void Init()
		{
			array = new SortedArray<int>(new IC());
			for (int i = 10; i < 20; i++)
			{
				array.Add(i);
				array.Add(i + 10);
			}
		}


		[Test]
		public void Both()
		{
			Assert.AreEqual(0, array.ContainsCount(7));
			Assert.AreEqual(1, array.ContainsCount(10));
			array.RemoveAllCopies(10);
			Assert.AreEqual(0, array.ContainsCount(10));
			array.RemoveAllCopies(7);
		}


		[TearDown]
		public void Dispose()
		{
			array = null;
		}
	}


	[TestFixture]
	public class Div
	{
		private SortedArray<int> array;


		[SetUp]
		public void Init()
		{
			array = new SortedArray<int>(new IC());
		}


		private void loadup()
		{
			for (int i = 10; i < 20; i++)
			{
				array.Add(i);
				array.Add(i + 10);
			}
		}


		[Test]
		public void NoDuplicatesEtc()
		{
			Assert.IsFalse(array.AllowsDuplicates);
			loadup();
			Assert.IsFalse(array.AllowsDuplicates);
			Assert.AreEqual(Speed.Log, array.ContainsSpeed);
			Assert.IsTrue(array.Comparer.Compare(2, 3) < 0);
            Assert.IsTrue(array.Comparer.Compare(4, 3) > 0);
            Assert.IsTrue(array.Comparer.Compare(3, 3) == 0);
        }

		[Test]
		public void Add()
		{
			Assert.IsTrue(array.Add(17));
			Assert.IsFalse(array.Add(17));
			Assert.IsTrue(array.Add(18));
			Assert.IsFalse(array.Add(18));
			Assert.AreEqual(2, array.Count);
		}


		[TearDown]
		public void Dispose()
		{
			array = null;
		}
	}


	[TestFixture]
	public class FindOrAdd
	{
		private SortedArray<KeyValuePair<int,string>> bag;


		[SetUp]
		public void Init()
		{
			bag = new SortedArray<KeyValuePair<int,string>>(new KeyValuePairComparer<int,string>(new IC()));
		}


		[TearDown]
		public void Dispose()
		{
			bag = null;
		}


		[Test]
		public void Test()
		{
			KeyValuePair<int,string> p = new KeyValuePair<int,string>(3, "tre");

			Assert.IsFalse(bag.FindOrAdd(ref p));
			p.value = "drei";
			Assert.IsTrue(bag.FindOrAdd(ref p));
			Assert.AreEqual("tre", p.value);
			p.value = "three";
			Assert.AreEqual(1, bag.ContainsCount(p));
			Assert.AreEqual("tre", bag[0].value);
		}
	}



	[TestFixture]
	public class ArrayTest
	{
		private SortedArray<int> tree;

		int[] a;


		[SetUp]
		public void Init()
		{
			tree = new SortedArray<int>(new IC());
			a = new int[10];
			for (int i = 0; i < 10; i++)
				a[i] = 1000 + i;
		}


		[TearDown]
		public void Dispose() { tree = null; }


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
			Assert.AreEqual("Alles klar", aeq(tree.ToArray()));
			tree.Add(7);
			tree.Add(4);
			Assert.AreEqual("Alles klar", aeq(tree.ToArray(), 4, 7));
		}


		[Test]
		public void CopyTo()
		{
			tree.CopyTo(a, 1);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
			tree.Add(6);
			tree.CopyTo(a, 2);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 1004, 1005, 1006, 1007, 1008, 1009));
			tree.Add(4);
			tree.Add(9);
			tree.CopyTo(a, 4);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 4, 6, 9, 1007, 1008, 1009));
			tree.Clear();
			tree.Add(7);
			tree.CopyTo(a, 9);
			Assert.AreEqual("Alles klar", aeq(a, 1000, 1001, 6, 1003, 4, 6, 9, 1007, 1008, 7));
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyToBad()
		{
			tree.Add(3);
			tree.CopyTo(a, 10);
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CopyToBad2()
		{
			tree.CopyTo(a, -1);
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyToTooFar()
		{
			tree.Add(3);
			tree.Add(4);
			tree.CopyTo(a, 9);
		}
	}


	[TestFixture]
	public class Combined
	{
		private IIndexedSorted<KeyValuePair<int,int>> lst;


		[SetUp]
		public void Init()
		{
			lst = new SortedArray<KeyValuePair<int,int>>(new KeyValuePairComparer<int,int>(new IC()));
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


	[TestFixture]
	public class Remove
	{
		private SortedArray<int> array;


		[SetUp]
		public void Init()
		{
			array = new SortedArray<int>(new IC());
			for (int i = 10; i < 20; i++)
			{
				array.Add(i);
				array.Add(i + 10);
			}
		}


		[Test]
		public void SmallTrees()
		{
			array.Clear();
			array.Add(7);
			array.Add(9);
			Assert.IsTrue(array.Remove(7));
			Assert.IsTrue(array.Check());
		}


		[Test]
		public void ByIndex()
		{
			//Remove root!
			int n = array.Count;
			int i = array[10];

			array.RemoveAt(10);
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Contains(i));
			Assert.AreEqual(n - 1, array.Count);

			//Low end
			i = array.FindMin();
			array.RemoveAt(0);
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Contains(i));
			Assert.AreEqual(n - 2, array.Count);

			//high end
			i = array.FindMax();
			array.RemoveAt(array.Count - 1);
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Contains(i));
			Assert.AreEqual(n - 3, array.Count);

			//Some leaf
			i = 18;
			array.RemoveAt(7);
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Contains(i));
			Assert.AreEqual(n - 4, array.Count);
		}


		[Test]
		public void AlmostEmpty()
		{
			//Almost empty
			array.Clear();
			array.Add(3);
			array.RemoveAt(0);
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Contains(3));
			Assert.AreEqual(0, array.Count);
		}


		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException), "Index out of range for sequenced collection")]
		public void Empty()
		{
			array.Clear();
			array.RemoveAt(0);
		}


		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException), "Index out of range for sequenced collection")]
		public void HighIndex()
		{
			array.RemoveAt(array.Count);
		}


		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException), "Index out of range for sequenced collection")]
		public void LowIndex()
		{
			array.RemoveAt(-1);
		}


		[Test]
		public void Normal()
		{
			Assert.IsFalse(array.Remove(-20));

			//No demote case, with move_item
			Assert.IsTrue(array.Remove(20));
			Assert.IsTrue(array.Check());
			Assert.IsFalse(array.Remove(20));

			//plain case 2
			Assert.IsTrue(array.Remove(14));
			Assert.IsTrue(array.Check(), "Bad tree");

			//case 1b
			Assert.IsTrue(array.Remove(25));
			Assert.IsTrue(array.Check(), "Bad tree");

			//case 1c
			Assert.IsTrue(array.Remove(29));
			Assert.IsTrue(array.Check(), "Bad tree");

			//1a (terminating)
			Assert.IsTrue(array.Remove(10));
			Assert.IsTrue(array.Check(), "Bad tree");

			//2+1b
			Assert.IsTrue(array.Remove(12));
			Assert.IsTrue(array.Remove(11));

			//1a+1b
			Assert.IsTrue(array.Remove(18));
			Assert.IsTrue(array.Remove(13));
			Assert.IsTrue(array.Remove(15));

			//2+1c
			for (int i = 0; i < 10; i++)
				array.Add(50 - 2 * i);

			Assert.IsTrue(array.Remove(42));
			Assert.IsTrue(array.Remove(38));
			Assert.IsTrue(array.Remove(28));
			Assert.IsTrue(array.Remove(40));

			//
			Assert.IsTrue(array.Remove(16));
			Assert.IsTrue(array.Remove(23));
			Assert.IsTrue(array.Remove(17));
			Assert.IsTrue(array.Remove(19));
			Assert.IsTrue(array.Remove(50));
			Assert.IsTrue(array.Remove(26));
			Assert.IsTrue(array.Remove(21));
			Assert.IsTrue(array.Remove(22));
			Assert.IsTrue(array.Remove(24));
			for (int i = 0; i < 48; i++)
				array.Remove(i);

			//Almost empty tree:
			Assert.IsFalse(array.Remove(26));
			Assert.IsTrue(array.Remove(48));
			Assert.IsTrue(array.Check(), "Bad tree");

			//Empty tree:
			Assert.IsFalse(array.Remove(26));
			Assert.IsTrue(array.Check(), "Bad tree");
		}


		[TearDown]
		public void Dispose()
		{
			array = null;
		}
	}



	[TestFixture]
	public class PredecessorStructure
	{
		private SortedArray<int> tree;


		[SetUp]
		public void Init()
		{
			tree = new SortedArray<int>(new IC());
		}


		private void loadup()
		{
			for (int i = 0; i < 20; i++)
				tree.Add(2 * i);
		}


		[Test]
		public void Predecessor()
		{
			loadup();
			Assert.AreEqual(6, tree.Predecessor(7));
			Assert.AreEqual(6, tree.Predecessor(8));

			//The bottom
			Assert.AreEqual(0, tree.Predecessor(1));

			//The top
			Assert.AreEqual(38, tree.Predecessor(39));
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Below minimum of set\r\nParameter name: item\r\nActual value was -2.")]
		public void PredecessorTooLow1()
		{
			tree.Predecessor(-2);
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Below minimum of set\r\nParameter name: item\r\nActual value was 0.")]
		public void PredecessorTooLow2()
		{
			tree.Predecessor(0);
		}


		[Test]
		public void WeakPredecessor()
		{
			loadup();
			Assert.AreEqual(6, tree.WeakPredecessor(7));
			Assert.AreEqual(8, tree.WeakPredecessor(8));

			//The bottom
			Assert.AreEqual(0, tree.WeakPredecessor(1));
			Assert.AreEqual(0, tree.WeakPredecessor(0));

			//The top
			Assert.AreEqual(38, tree.WeakPredecessor(39));
			Assert.AreEqual(38, tree.WeakPredecessor(38));
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Below minimum of set\r\nParameter name: item\r\nActual value was -1.")]
		public void WeakPredecessorTooLow1()
		{
			tree.WeakPredecessor(-1);
		}


		[Test]
		public void Successor()
		{
			loadup();
			Assert.AreEqual(8, tree.Successor(7));
			Assert.AreEqual(10, tree.Successor(8));

			//The bottom
			Assert.AreEqual(2, tree.Successor(0));
			Assert.AreEqual(0, tree.Successor(-1));

			//The top
			Assert.AreEqual(38, tree.Successor(37));
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Above maximum of set\r\nParameter name: item\r\nActual value was 38.")]
		public void SuccessorTooHigh1()
		{
			tree.Successor(38);
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Above maximum of set\r\nParameter name: item\r\nActual value was 39.")]
		public void SuccessorTooHigh2()
		{
			tree.Successor(39);
		}


		[Test]
		public void WeakSuccessor()
		{
			loadup();
			Assert.AreEqual(6, tree.WeakSuccessor(6));
			Assert.AreEqual(8, tree.WeakSuccessor(7));

			//The bottom
			Assert.AreEqual(0, tree.WeakSuccessor(-1));
			Assert.AreEqual(0, tree.WeakSuccessor(0));

			//The top
			Assert.AreEqual(38, tree.WeakSuccessor(37));
			Assert.AreEqual(38, tree.WeakSuccessor(38));
		}


		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException), "Above maximum of set\r\nParameter name: item\r\nActual value was 39.")]
		public void WeakSuccessorTooHigh1()
		{
			tree.WeakSuccessor(39);
		}


		[TearDown]
		public void Dispose()
		{
			tree = null;
		}
	}



	[TestFixture]
	public class PriorityQueue
	{
		private SortedArray<int> tree;


		[SetUp]
		public void Init()
		{
			tree = new SortedArray<int>(new IC());
		}


		private void loadup()
		{
			foreach (int i in new int[] { 1, 2, 3, 4 })
				tree.Add(i);
		}


		[Test]
		public void Normal()
		{
			loadup();
			Assert.AreEqual(1, tree.FindMin());
			Assert.AreEqual(4, tree.FindMax());
			Assert.AreEqual(1, tree.DeleteMin());
			Assert.AreEqual(4, tree.DeleteMax());
			Assert.IsTrue(tree.Check(), "Bad tree");
			Assert.AreEqual(2, tree.FindMin());
			Assert.AreEqual(3, tree.FindMax());
			Assert.AreEqual(2, tree.DeleteMin());
			Assert.AreEqual(3, tree.DeleteMax());
			Assert.IsTrue(tree.Check(), "Bad tree");
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException), "Priority queue is empty")]
		public void Empty1()
		{
			tree.FindMin();
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException), "Priority queue is empty")]
		public void Empty2()
		{
			tree.FindMax();
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException), "Priority queue is empty")]
		public void Empty3()
		{
			tree.DeleteMin();
		}


		[Test]
		[ExpectedException(typeof(InvalidOperationException), "Priority queue is empty")]
		public void Empty4()
		{
			tree.DeleteMax();
		}


		[TearDown]
		public void Dispose()
		{
			tree = null;
		}
	}



	[TestFixture]
	public class IndexingAndCounting
	{
		private SortedArray<int> array;


		[SetUp]
		public void Init()
		{
			array = new SortedArray<int>(new IC());
		}


		private void populate()
		{
			array.Add(30);
			array.Add(50);
			array.Add(10);
			array.Add(70);
		}


		[Test]
		public void ToArray()
		{
			populate();

			int[] a = array.ToArray();

			Assert.AreEqual(4, a.Length);
			Assert.AreEqual(10, a[0]);
			Assert.AreEqual(30, a[1]);
			Assert.AreEqual(50, a[2]);
			Assert.AreEqual(70, a[3]);
		}


		[Test]
		public void GoodIndex()
		{
			Assert.AreEqual(-1, array.IndexOf(20));
			Assert.AreEqual(-1, array.LastIndexOf(20));
			populate();
			Assert.AreEqual(10, array[0]);
			Assert.AreEqual(30, array[1]);
			Assert.AreEqual(50, array[2]);
			Assert.AreEqual(70, array[3]);
			Assert.AreEqual(0, array.IndexOf(10));
			Assert.AreEqual(1, array.IndexOf(30));
			Assert.AreEqual(2, array.IndexOf(50));
			Assert.AreEqual(3, array.IndexOf(70));
			Assert.AreEqual(-1, array.IndexOf(20));
			Assert.AreEqual(-1, array.IndexOf(0));
			Assert.AreEqual(-1, array.IndexOf(90));
			Assert.AreEqual(0, array.LastIndexOf(10));
			Assert.AreEqual(1, array.LastIndexOf(30));
			Assert.AreEqual(2, array.LastIndexOf(50));
			Assert.AreEqual(3, array.LastIndexOf(70));
			Assert.AreEqual(-1, array.LastIndexOf(20));
			Assert.AreEqual(-1, array.LastIndexOf(0));
			Assert.AreEqual(-1, array.LastIndexOf(90));
		}


		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void IndexTooLarge()
		{
			populate();
			Console.WriteLine(array[4]);
		}


		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void IndexTooSmall()
		{
			populate();
			Console.WriteLine(array[-1]);
		}


		[Test]
		public void FilledTreeOutsideInput()
		{
			populate();
			Assert.AreEqual(0, array.CountFrom(90));
			Assert.AreEqual(0, array.CountFromTo(-20, 0));
			Assert.AreEqual(0, array.CountFromTo(80, 100));
			Assert.AreEqual(0, array.CountTo(0));
			Assert.AreEqual(4, array.CountTo(90));
			Assert.AreEqual(4, array.CountFromTo(-20, 90));
			Assert.AreEqual(4, array.CountFrom(0));
		}


		[Test]
		public void FilledTreeIntermediateInput()
		{
			populate();
			Assert.AreEqual(3, array.CountFrom(20));
			Assert.AreEqual(1, array.CountFromTo(20, 40));
			Assert.AreEqual(2, array.CountTo(40));
		}


		[Test]
		public void FilledTreeMatchingInput()
		{
			populate();
			Assert.AreEqual(3, array.CountFrom(30));
			Assert.AreEqual(2, array.CountFromTo(30, 70));
			Assert.AreEqual(0, array.CountFromTo(50, 30));
			Assert.AreEqual(0, array.CountFromTo(50, 50));
			Assert.AreEqual(0, array.CountTo(10));
			Assert.AreEqual(2, array.CountTo(50));
		}


		[Test]
		public void CountEmptyTree()
		{
			Assert.AreEqual(0, array.CountFrom(20));
			Assert.AreEqual(0, array.CountFromTo(20, 40));
			Assert.AreEqual(0, array.CountTo(40));
		}


		[TearDown]
		public void Dispose()
		{
			array = null;
		}
	}




	namespace ModificationCheck
	{
		[TestFixture]
		public class Enumerator
		{
			private SortedArray<int> tree;

			private MSG.IEnumerator<int> e;


			[SetUp]
			public void Init()
			{
				tree = new SortedArray<int>(new IC());
				for (int i = 0; i < 10; i++)
					tree.Add(i);

				e = tree.GetEnumerator();
			}


			[Test]
			public void CurrentAfterModification()
			{
				e.MoveNext();
				tree.Add(34);
				Assert.AreEqual(0, e.Current);
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterAdd()
			{
				e.MoveNext();
				tree.Add(34);
				e.MoveNext();
			}




			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterRemove()
			{
				e.MoveNext();
				tree.Remove(34);
				e.MoveNext();
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterClear()
			{
				e.MoveNext();
				tree.Clear();
				e.MoveNext();
			}


			[TearDown]
			public void Dispose()
			{
				tree = null;
				e = null;
			}
		}



		[TestFixture]
		public class RangeEnumerator
		{
			private SortedArray<int> tree;

			private MSG.IEnumerator<int> e;


			[SetUp]
			public void Init()
			{
				tree = new SortedArray<int>(new IC());
				for (int i = 0; i < 10; i++)
					tree.Add(i);

				e = tree.RangeFromTo(3, 7).GetEnumerator();
			}


			[Test]
			public void CurrentAfterModification()
			{
				e.MoveNext();
				tree.Add(34);
				Assert.AreEqual(3, e.Current);
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterAdd()
			{
				tree.Add(34);
				e.MoveNext();
			}




			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterRemove()
			{
				tree.Remove(34);
				e.MoveNext();
			}


			[Test]
			[ExpectedException(typeof(InvalidOperationException), "Collection was modified")]
			public void MoveNextAfterClear()
			{
				tree.Clear();
				e.MoveNext();
			}


			[TearDown]
			public void Dispose()
			{
				tree = null;
				e = null;
			}
		}
	}

	namespace HigherOrder
	{
		internal class CubeRoot: IComparable<int>
		{
			private int c;


			internal CubeRoot(int c) { this.c = c; }


			public int CompareTo(int that) { return c - that * that * that; }

            public bool Equals(int that) { return c == that * that * that; }

		}



		class Interval: IComparable<int>
		{
			private int b, t;


			internal Interval(int b, int t) { this.b = b; this.t = t; }


			public int CompareTo(int that) { return that < b ? 1 : that > t ? -1 : 0; }

            public bool Equals(int that) { return that >= b && that <= t; }
        }



		[TestFixture]
		public class Simple
		{
			private SortedArray<int> array;

			private IComparer<int> ic;


			[SetUp]
			public void Init()
			{
				ic = new IC();
				array = new SortedArray<int>(ic);
			}


			private bool never(int i) { return false; }


			private bool always(int i) { return true; }


			private bool even(int i) { return i % 2 == 0; }


			private string themap(int i) { return String.Format("AA {0,4} BB", i); }


			private string badmap(int i) { return String.Format("AA {0} BB", i); }


			private int appfield1;

			private int appfield2;


			private void apply(int i) { appfield1++; appfield2 += i * i; }


			[Test]
			public void Apply()
			{
				Simple simple1 = new Simple();

				array.Apply(new Applier<int>(simple1.apply));
				Assert.AreEqual(0, simple1.appfield1);
				Assert.AreEqual(0, simple1.appfield2);

				Simple simple2 = new Simple();

				for (int i = 0; i < 10; i++) array.Add(i);

				array.Apply(new Applier<int>(simple2.apply));
				Assert.AreEqual(10, simple2.appfield1);
				Assert.AreEqual(285, simple2.appfield2);
			}


			[Test]
			public void All()
			{
				Assert.IsTrue(array.All(new Filter<int>(never)));
				Assert.IsTrue(array.All(new Filter<int>(even)));
				Assert.IsTrue(array.All(new Filter<int>(always)));
				for (int i = 0; i < 10; i++)					array.Add(i);

				Assert.IsFalse(array.All(new Filter<int>(never)));
				Assert.IsFalse(array.All(new Filter<int>(even)));
				Assert.IsTrue(array.All(new Filter<int>(always)));
				array.Clear();
				for (int i = 0; i < 10; i++)					array.Add(i * 2);

				Assert.IsFalse(array.All(new Filter<int>(never)));
				Assert.IsTrue(array.All(new Filter<int>(even)));
				Assert.IsTrue(array.All(new Filter<int>(always)));
				array.Clear();
				for (int i = 0; i < 10; i++)					array.Add(i * 2 + 1);

				Assert.IsFalse(array.All(new Filter<int>(never)));
				Assert.IsFalse(array.All(new Filter<int>(even)));
				Assert.IsTrue(array.All(new Filter<int>(always)));
			}


			[Test]
			public void Exists()
			{
				Assert.IsFalse(array.Exists(new Filter<int>(never)));
				Assert.IsFalse(array.Exists(new Filter<int>(even)));
				Assert.IsFalse(array.Exists(new Filter<int>(always)));
				for (int i = 0; i < 10; i++)					array.Add(i);

				Assert.IsFalse(array.Exists(new Filter<int>(never)));
				Assert.IsTrue(array.Exists(new Filter<int>(even)));
				Assert.IsTrue(array.Exists(new Filter<int>(always)));
				array.Clear();
				for (int i = 0; i < 10; i++)					array.Add(i * 2);

				Assert.IsFalse(array.Exists(new Filter<int>(never)));
				Assert.IsTrue(array.Exists(new Filter<int>(even)));
				Assert.IsTrue(array.Exists(new Filter<int>(always)));
				array.Clear();
				for (int i = 0; i < 10; i++)					array.Add(i * 2 + 1);

				Assert.IsFalse(array.Exists(new Filter<int>(never)));
				Assert.IsFalse(array.Exists(new Filter<int>(even)));
				Assert.IsTrue(array.Exists(new Filter<int>(always)));
			}


			[Test]
			public void FindAll()
			{
				Assert.AreEqual(0, array.FindAll(new Filter<int>(never)).Count);
				for (int i = 0; i < 10; i++)
					array.Add(i);

				Assert.AreEqual(0, array.FindAll(new Filter<int>(never)).Count);
				Assert.AreEqual(10, array.FindAll(new Filter<int>(always)).Count);
				Assert.AreEqual(5, array.FindAll(new Filter<int>(even)).Count);
				Assert.IsTrue(((SortedArray<int>)array.FindAll(new Filter<int>(even))).Check());
			}


			[Test]
			public void Map()
			{
				Assert.AreEqual(0, array.Map(new Mapper<int,string>(themap), new SC()).Count);
				for (int i = 0; i < 11; i++)
					array.Add(i * i * i);

				IIndexedSorted<string> res = array.Map(new Mapper<int,string>(themap), new SC());

				Assert.IsTrue(((SortedArray<string>)res).Check());
				Assert.AreEqual(11, res.Count);
				Assert.AreEqual("AA    0 BB", res[0]);
				Assert.AreEqual("AA   27 BB", res[3]);
				Assert.AreEqual("AA  125 BB", res[5]);
				Assert.AreEqual("AA 1000 BB", res[10]);
			}


			[Test]
			[ExpectedException(typeof(ArgumentException), "mapper not monotonic")]
			public void BadMap()
			{
				for (int i = 0; i < 11; i++)
					array.Add(i * i * i);

				ISorted<string> res = array.Map(new Mapper<int,string>(badmap), new SC());
			}


			[Test]
			public void Cut()
			{
				for (int i = 0; i < 10; i++)
					array.Add(i);

				int low, high;
				bool lval, hval;

				Assert.IsTrue(array.Cut(new CubeRoot(27), out low, out lval, out high, out hval));
				Assert.IsTrue(lval && hval);
				Assert.AreEqual(4, high);
				Assert.AreEqual(2, low);
				Assert.IsFalse(array.Cut(new CubeRoot(30), out low, out lval, out high, out hval));
				Assert.IsTrue(lval && hval);
				Assert.AreEqual(4, high);
				Assert.AreEqual(3, low);
			}


			[Test]
			public void CutInt()
			{
				for (int i = 0; i < 10; i++)
					array.Add(2 * i);

				int low, high;
				bool lval, hval;

				Assert.IsFalse(array.Cut(new IC(3), out low, out lval, out high, out hval));
				Assert.IsTrue(lval && hval);
				Assert.AreEqual(4, high);
				Assert.AreEqual(2, low);
				Assert.IsTrue(array.Cut(new IC(6), out low, out lval, out high, out hval));
				Assert.IsTrue(lval && hval);
				Assert.AreEqual(8, high);
				Assert.AreEqual(4, low);
			}


			[Test]
			public void CutInterval()
			{
				for (int i = 0; i < 10; i++)
					array.Add(2 * i);

				int lo, hi;
				bool lv, hv;

				Assert.IsTrue(array.Cut(new Interval(5, 9), out lo, out lv, out hi, out hv));
				Assert.IsTrue(lv && hv);
				Assert.AreEqual(10, hi);
				Assert.AreEqual(4, lo);
				Assert.IsTrue(array.Cut(new Interval(6, 10), out lo, out lv, out hi, out hv));
				Assert.IsTrue(lv && hv);
				Assert.AreEqual(12, hi);
				Assert.AreEqual(4, lo);
				for (int i = 0; i < 100; i++)
					array.Add(2 * i);

				array.Cut(new Interval(77, 105), out lo, out lv, out hi, out hv);
				Assert.IsTrue(lv && hv);
				Assert.AreEqual(106, hi);
				Assert.AreEqual(76, lo);
				array.Cut(new Interval(5, 7), out lo, out lv, out hi, out hv);
				Assert.IsTrue(lv && hv);
				Assert.AreEqual(8, hi);
				Assert.AreEqual(4, lo);
				array.Cut(new Interval(80, 110), out lo, out lv, out hi, out hv);
				Assert.IsTrue(lv && hv);
				Assert.AreEqual(112, hi);
				Assert.AreEqual(78, lo);
			}


			[Test]
			public void UpperCut()
			{
				for (int i = 0; i < 10; i++)
					array.Add(i);

				int l, h;
				bool lv, hv;

				Assert.IsFalse(array.Cut(new CubeRoot(1000), out l, out lv, out h, out hv));
				Assert.IsTrue(lv && !hv);
				Assert.AreEqual(9, l);
				Assert.IsFalse(array.Cut(new CubeRoot(-50), out l, out lv, out h, out hv));
				Assert.IsTrue(!lv && hv);
				Assert.AreEqual(0, h);
			}


			[TearDown]
			public void Dispose() { ic = null; array = null; }
		}
	}




	namespace MultiOps
	{
		[TestFixture]
		public class AddAll
		{
			private int sqr(int i) { return i * i; }


			SortedArray<int> array;


			[SetUp]
			public void Init() { array = new SortedArray<int>(new IC()); }


			[Test]
			public void EmptyEmpty()
			{
				array.AddAll(new FunEnumerable(0, new Int2Int(sqr)));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
			}


			[Test]
			public void SomeEmpty()
			{
				for (int i = 4; i < 9; i++) array.Add(i);

				array.AddAll(new FunEnumerable(0, new Int2Int(sqr)));
				Assert.AreEqual(5, array.Count);
				Assert.IsTrue(array.Check());
			}


			[Test]
			public void EmptySome()
			{
				array.AddAll(new FunEnumerable(4, new Int2Int(sqr)));
				Assert.AreEqual(4, array.Count);
				Assert.IsTrue(array.Check());
				Assert.AreEqual(0, array[0]);
				Assert.AreEqual(1, array[1]);
				Assert.AreEqual(4, array[2]);
				Assert.AreEqual(9, array[3]);
			}


			[Test]
			public void SomeSome()
			{
				for (int i = 3; i < 9; i++) array.Add(i);

				array.AddAll(new FunEnumerable(4, new Int2Int(sqr)));
				Assert.AreEqual(9, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 3,4, 5, 6, 7, 8, 9));
			}


			[TearDown]
			public void Dispose() { array = null; }
		}



		[TestFixture]
		public class AddSorted
		{
			private int sqr(int i) { return i * i; }


			private int bad(int i) { return i * (5 - i); }


			SortedArray<int> array;


			[SetUp]
			public void Init() { array = new SortedArray<int>(new IC()); }


			[Test]
			public void EmptyEmpty()
			{
				array.AddSorted(new FunEnumerable(0, new Int2Int(sqr)));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
			}



			[Test]
			public void SomeEmpty()
			{
				for (int i = 4; i < 9; i++) array.Add(i);

				array.AddSorted(new FunEnumerable(0, new Int2Int(sqr)));
				Assert.AreEqual(5, array.Count);
				Assert.IsTrue(array.Check());
			}



			[Test]
			public void EmptySome()
			{
				array.AddSorted(new FunEnumerable(4, new Int2Int(sqr)));
				Assert.AreEqual(4, array.Count);
				Assert.IsTrue(array.Check());
				Assert.AreEqual(0, array[0]);
				Assert.AreEqual(1, array[1]);
				Assert.AreEqual(4, array[2]);
				Assert.AreEqual(9, array[3]);
			}



			[Test]
			public void SomeSome()
			{
				for (int i = 3; i < 9; i++) array.Add(i);

				array.AddSorted(new FunEnumerable(4, new Int2Int(sqr)));
				Assert.AreEqual(9, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 3, 4, 5, 6, 7, 8, 9));
			}

			[Test]
			[ExpectedException(typeof(ArgumentException), "Argument not sorted")]
			public void EmptyBad()
			{
				array.AddSorted(new FunEnumerable(9, new Int2Int(bad)));
			}


			[TearDown]
			public void Dispose() { array = null; }
		}

		[TestFixture]
		public class Rest
		{
			SortedArray<int> array, array2;


			[SetUp]
			public void Init()
			{
				array = new SortedArray<int>(new IC());
				array2 = new SortedArray<int>(new IC());
				for (int i = 0; i < 10; i++)
					array.Add(i);

				for (int i = 0; i < 10; i++)
					array2.Add(2 * i);
			}


			[Test]
			public void RemoveAll()
			{
				array.RemoveAll(array2.RangeFromTo(3, 7));
				Assert.AreEqual(8, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 2, 3, 5, 7, 8, 9));
				array.RemoveAll(array2.RangeFromTo(3, 7));
				Assert.AreEqual(8, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 2, 3, 5, 7, 8, 9));
				array.RemoveAll(array2.RangeFromTo(13, 17));
				Assert.AreEqual(8, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 2, 3, 5, 7, 8, 9));
				array.RemoveAll(array2.RangeFromTo(3, 17));
				Assert.AreEqual(7, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 0, 1, 2, 3, 5, 7, 9));
				for (int i = 0; i < 10; i++) array2.Add(i);

				array.RemoveAll(array2.RangeFromTo(-1, 10));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array));
			}


			[Test]
			public void RetainAll()
			{
				array.RetainAll(array2.RangeFromTo(3, 17));
				Assert.AreEqual(3, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 4, 6, 8));
				array.RetainAll(array2.RangeFromTo(1, 17));
				Assert.AreEqual(3, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 4, 6, 8));
				array.RetainAll(array2.RangeFromTo(3, 5));
				Assert.AreEqual(1, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array, 4));
				array.RetainAll(array2.RangeFromTo(7, 17));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array));
				for (int i = 0; i < 10; i++) array.Add(i);

				array.RetainAll(array2.RangeFromTo(5, 5));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array));
				for (int i = 0; i < 10; i++) array.Add(i);

				array.RetainAll(array2.RangeFromTo(15, 25));
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(array.Check());
				Assert.IsTrue(IC.eq(array));
			}


			[Test]
			public void ContainsAll()
			{
				Assert.IsFalse(array.ContainsAll(array2));
				Assert.IsTrue(array.ContainsAll(array));
				array2.Clear();
				Assert.IsTrue(array.ContainsAll(array2));
				array.Clear();
				Assert.IsTrue(array.ContainsAll(array2));
				array2.Add(8);
				Assert.IsFalse(array.ContainsAll(array2));
			}


			[Test]
			public void RemoveInterval()
			{
				array.RemoveInterval(3, 4);
				Assert.IsTrue(array.Check());
				Assert.AreEqual(6, array.Count);
				Assert.IsTrue(IC.eq(array, 0, 1, 2, 7, 8, 9));
				array.RemoveInterval(2, 3);
				Assert.IsTrue(array.Check());
				Assert.AreEqual(3, array.Count);
				Assert.IsTrue(IC.eq(array, 0, 1, 9));
				array.RemoveInterval(0, 3);
				Assert.IsTrue(array.Check());
				Assert.AreEqual(0, array.Count);
				Assert.IsTrue(IC.eq(array));
			}


			[Test]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void RemoveRangeBad1()
			{
				array.RemoveInterval(-3, 8);
			}


			[Test]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void RemoveRangeBad2()
			{
				array.RemoveInterval(3, -8);
			}


			[Test]
			[ExpectedException(typeof(ArgumentException))]
			public void RemoveRangeBad3()
			{
				array.RemoveInterval(3, 8);
			}


			[Test]
			public void GetRange()
			{
				MSG.IEnumerable<int> e = array[3, 3];

				Assert.IsTrue(IC.eq(e, 3, 4, 5));
				e = array[3, 0];
				Assert.IsTrue(IC.eq(e));
			}


			[Test]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void GetRangeBad1()
			{
				object foo = array[-3, 0];
			}


			[Test]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void GetRangeBad2()
			{
				object foo = array[3, -1];
			}


			[Test]
			[ExpectedException(typeof(ArgumentException))]
			public void GetRangeBad3()
			{
				object foo = array[3, 8];
			}


			[TearDown]
			public void Dispose() { array = null; array2 = null; }
		}
	}




	namespace Sync
	{
		[TestFixture]
		public class SyncRoot
		{
			private SortedArray<int> tree;

			int sz = 5000;


			[Test]
			public void Safe()
			{
				System.Threading.Thread t1 = new System.Threading.Thread(new System.Threading.ThreadStart(safe1));
				System.Threading.Thread t2 = new System.Threading.Thread(new System.Threading.ThreadStart(safe2));

				t1.Start();
				t2.Start();
				t1.Join();
				t2.Join();
				Assert.AreEqual(2 * sz + 1, tree.Count);
				Assert.IsTrue(tree.Check());
			}


			//[Test]
			public void UnSafe()
			{
				bool bad = false;

				for (int i = 0; i < 10; i++)
				{
					System.Threading.Thread t1 = new System.Threading.Thread(new System.Threading.ThreadStart(unsafe1));
					System.Threading.Thread t2 = new System.Threading.Thread(new System.Threading.ThreadStart(unsafe2));

					t1.Start();
					t2.Start();
					t1.Join();
					t2.Join();
					if (bad = 2 * sz + 1 != tree.Count)
					{
						Console.WriteLine("{0}::Unsafe(): bad at {1}", GetType(), i);
						break;
					}
				}

				Assert.IsTrue(bad, "No sync problems!");
			}


			[Test]
			public void SafeUnSafe()
			{
				System.Threading.Thread t1 = new System.Threading.Thread(new System.Threading.ThreadStart(unsafe1));
				System.Threading.Thread t2 = new System.Threading.Thread(new System.Threading.ThreadStart(unsafe2));

				t1.Start();
				t1.Join();
				t2.Start();
				t2.Join();
				Assert.AreEqual(2 * sz + 1, tree.Count);
			}


			[SetUp]
			public void Init() { tree = new SortedArray<int>(new IC()); }


			private void unsafe1()
			{
				for (int i = 0; i < 2 * sz; i++)
					tree.Add(i * 2);

				for (int i = 1; i < sz; i++)
					tree.Remove(i * 4);
			}


			private void safe1()
			{
				for (int i = 0; i < 2 * sz; i++)
					lock (tree.SyncRoot)
						tree.Add(i * 2);

				for (int i = 1; i < sz; i++)
					lock (tree.SyncRoot)
						tree.Remove(i * 4);
			}


			private void unsafe2()
			{
				for (int i = 2 * sz; i > 0; i--)
					tree.Add(i * 2 + 1);

				for (int i = sz; i > 0; i--)
					tree.Remove(i * 4 + 1);
			}


			private void safe2()
			{
				for (int i = 2 * sz; i > 0; i--)
					lock (tree.SyncRoot)
						tree.Add(i * 2 + 1);

				for (int i = sz; i > 0; i--)
					lock (tree.SyncRoot)
						tree.Remove(i * 4 + 1);
			}


			[TearDown]
			public void Dispose() { tree = null; }
		}



		//[TestFixture]
		public class ConcurrentQueries
		{
			private SortedArray<int> tree;

			int sz = 500000;


			[SetUp]
			public void Init()
			{
				tree = new SortedArray<int>(new IC());
				for (int i = 0; i < sz; i++)
				{
					tree.Add(i);
				}
			}



			class A
			{
				public int count = 0;

				SortedArray<int> t;


				public A(SortedArray<int> t) { this.t = t; }


				public void a(int i) { count++; }


				public void traverse() { t.Apply(new Applier<int>(a)); }
			}




			[Test]
			public void Safe()
			{
				A a = new A(tree);

				a.traverse();
				Assert.AreEqual(sz, a.count);
			}


			[Test]
			public void RegrettablyUnsafe()
			{
				System.Threading.Thread[] t = new System.Threading.Thread[10];
				A[] a = new A[10];
				for (int i = 0; i < 10; i++)
				{
					a[i] = new A(tree);
					t[i] = new System.Threading.Thread(new System.Threading.ThreadStart(a[i].traverse));
				}

				for (int i = 0; i < 10; i++)
					t[i].Start();
				for (int i = 0; i < 10; i++)
					t[i].Join();
				for (int i = 0; i < 10; i++)
					Assert.AreEqual(sz,a[i].count);

			}


			[TearDown]
			public void Dispose() { tree = null; }
		}
	}




	namespace Hashing
	{
		[TestFixture]
		public class ISequenced
		{
			private ISequenced<int> dit, dat, dut;


			[SetUp]
			public void Init()
			{
				dit = new SortedArray<int>(new IC());
				dat = new SortedArray<int>(new IC());
				dut = new SortedArray<int>(new RevIC());
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
				dut.Add(3);
				Assert.AreEqual(hasher(3), dut.GetHashCode());
				dut.Add(7);
				Assert.AreEqual(hasher(7, 3), dut.GetHashCode());
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
				dit = new SortedArray<int>(new IC());
				dat = new SortedArray<int>(new IC());
				dut = new SortedArray<int>(new RevIC());
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


			public int hasher(int count,params int[] items)
			{
				int retval = 0;

				foreach (int i in items)
					retval ^= i;

				return (count<<16)+retval;
			}


			[Test]
			public void HashVal()
			{
				Assert.AreEqual(hasher(0), dit.GetHashCode());
				dit.Add(3);
				Assert.AreEqual(hasher(1,3), dit.GetHashCode());
				dit.Add(7);
				Assert.AreEqual(hasher(2,3, 7), dit.GetHashCode());
				Assert.AreEqual(hasher(0), dut.GetHashCode());
				dut.Add(3);
				Assert.AreEqual(hasher(1,3), dut.GetHashCode());
				dut.Add(7);
				Assert.AreEqual(hasher(2,7, 3), dut.GetHashCode());
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

	}
}
#endif
