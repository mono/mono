// ArrayListTest.cs - NUnit Test Cases for the System.Collections.ArrayList class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell (http://www.novell.com)
// 

using System;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Collections
{
	[TestFixture]
	public class ArrayListTest
	{
		[Test]
		public void TestCtor ()
		{
			{
				ArrayList al1 = new ArrayList ();
				Assert.IsNotNull (al1, "no basic ArrayList");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "null icollection error not thrown");
			}
			{
				// what can I say?  I like chars.  [--DB]
				char [] coll = { 'a', 'b', 'c', 'd' };
				ArrayList al1 = new ArrayList (coll);
				Assert.IsNotNull (al1, "no icollection ArrayList");
				for (int i = 0; i < coll.Length; i++) {
					Assert.AreEqual (coll [i], al1 [i], i + " not ctor'ed properly.");
				}
			}
			{
				try {
					Char [,] c1 = new Char [2, 2];
					ArrayList al1 = new ArrayList (c1);
					Assert.Fail ("Should fail with multi-dimensional array in constructor.");
				} catch (RankException) {
				}
			}

			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (-1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative capacity error not thrown");
			}
		}

		[Test]
		public void TestCapacity ()
		{
#if NET_2_0
		int default_capacity = 4;
		int unspecified_capacity = 0;
#else
			int default_capacity = 16;
			int unspecified_capacity = 16;
#endif
			for (int i = 1; i < 100; i++) {
				ArrayList al1 = new ArrayList (i);
				Assert.AreEqual (i, al1.Capacity, "Bad capacity of " + i);
			}
			{
				ArrayList al1 = new ArrayList (0);
				// LAMESPEC: 
				// Assert.AreEqual (//	     16, al1.Capacity, "Bad capacity when set to 0");
				al1.Add ("?");
				Assert.AreEqual (default_capacity, al1.Capacity, "Bad capacity when set to 0");
			}
			{
				ArrayList al1 = new ArrayList ();
				Assert.AreEqual (unspecified_capacity, al1.Capacity, "Bad default capacity");
			}
		}

		[Test]
		public void TestCount ()
		{
			{
				ArrayList al1 = new ArrayList ();
				Assert.AreEqual (0, al1.Count, "Bad initial count");
				for (int i = 1; i <= 100; i++) {
					al1.Add (i);
					Assert.AreEqual (i, al1.Count, "Bad count " + i);
				}
			}
			for (int i = 0; i < 100; i++) {
				char [] coll = new Char [i];
				ArrayList al1 = new ArrayList (coll);
				Assert.AreEqual (i, al1.Count, "Bad count for " + i);
			}
		}

		[Test]
		public void TestIsFixed ()
		{
			ArrayList al1 = new ArrayList ();
			Assert.IsTrue (!al1.IsFixedSize, "should not be fixed by default");
			ArrayList al2 = ArrayList.FixedSize (al1);
			Assert.IsTrue (al2.IsFixedSize, "fixed-size wrapper not working");
		}

		[Test]
		public void TestIsReadOnly ()
		{
			ArrayList al1 = new ArrayList ();
			Assert.IsTrue (!al1.IsReadOnly, "should not be ReadOnly by default");
			ArrayList al2 = ArrayList.ReadOnly (al1);
			Assert.IsTrue (al2.IsReadOnly, "read-only wrapper not working");
		}

		[Test]
		public void TestIsSynchronized ()
		{
			ArrayList al1 = new ArrayList ();
			Assert.IsTrue (!al1.IsSynchronized, "should not be synchronized by default");
			ArrayList al2 = ArrayList.Synchronized (al1);
			Assert.IsTrue (al2.IsSynchronized, "synchronized wrapper not working");
		}

		[Test]
		public void TestItem ()
		{
			ArrayList al1 = new ArrayList ();
			{
				bool errorThrown = false;
				try {
					object o = al1 [-1];
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative item error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					object o = al1 [1];
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "past-end item error not thrown");
			}
			for (int i = 0; i <= 100; i++) {
				al1.Add (i);
			}
			for (int i = 0; i <= 100; i++) {
				Assert.AreEqual (i, al1 [i], "item not fetched for " + i);
			}
		}

		[Test]
		public void TestAdapter ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = ArrayList.Adapter (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "null adapter error not thrown");
			}
			{
				char [] list = { 'a', 'b', 'c', 'd' };
				ArrayList al1 = ArrayList.Adapter (list);
				Assert.IsNotNull (al1, "Couldn't get an adapter");
				for (int i = 0; i < list.Length; i++) {
					Assert.AreEqual (list [i], al1 [i], "adapter not adapting");
				}
				list [0] = 'z';
				for (int i = 0; i < list.Length; i++) {
					Assert.AreEqual (list [i], al1 [i], "adapter not adapting");
				}
			}
			// Test Binary Search
			{
				bool errorThrown = false;
				try {

					String [] s1 = { "This", "is", "a", "test" };
					ArrayList al1 = ArrayList.Adapter (s1);
					al1.BinarySearch (42);
				} catch (InvalidOperationException) {
					// this is what .NET throws
					errorThrown = true;
				} catch (ArgumentException) {
					// this is what the docs say it should throw
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "search-for-wrong-type error not thrown");
			}

			{
				char [] arr = { 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd' };
				ArrayList al1 = ArrayList.Adapter (arr);
				Assert.IsTrue (al1.BinarySearch ('c') >= 3, "couldn't find elem #1");
				Assert.IsTrue (al1.BinarySearch ('c') < 6, "couldn't find elem #2");
			}
			{
				char [] arr = { 'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e' };
				ArrayList al1 = ArrayList.Adapter (arr);
				Assert.AreEqual (-4, al1.BinarySearch ('c'), "couldn't find next-higher elem");
			}
			{
				char [] arr = { 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd' };
				ArrayList al1 = ArrayList.Adapter (arr);
				Assert.AreEqual (-9, al1.BinarySearch ('e'), "couldn't find end");
			}
			// Sort
			{
				char [] starter = { 'd', 'b', 'f', 'e', 'a', 'c' };
				ArrayList al1 = ArrayList.Adapter (starter);
				al1.Sort ();
				Assert.AreEqual ('a', al1 [0], "Should be sorted");
				Assert.AreEqual ('b', al1 [1], "Should be sorted");
				Assert.AreEqual ('c', al1 [2], "Should be sorted");
				Assert.AreEqual ('d', al1 [3], "Should be sorted");
				Assert.AreEqual ('e', al1 [4], "Should be sorted");
				Assert.AreEqual ('f', al1 [5], "Should be sorted");
			}

			// TODO - test other adapter types?
		}

		[Test]
		public void TestAdd ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList ());
					al1.Add ("Hi!");
				} catch (NotSupportedException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "add to fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					al1.Add ("Hi!");
				} catch (NotSupportedException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "add to read only error not thrown");
			}
			{
				ArrayList al1 = new ArrayList ();
				for (int i = 1; i <= 100; i++) {
					al1.Add (i);
					Assert.AreEqual (i, al1.Count, "add failed " + i);
					Assert.AreEqual (i, al1 [i - 1], "add failed " + i);

				}
			}
			{
				string [] strArray = new string [] { };
				ArrayList al1 = new ArrayList (strArray);
				al1.Add ("Hi!");
				al1.Add ("Hi!");
				Assert.AreEqual (2, al1.Count, "add failed");
			}
		}

		[Test]
		public void TestAddRange ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList ());
					String [] s1 = { "Hi!" };
					al1.AddRange (s1);
				} catch (NotSupportedException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "add to fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					String [] s1 = { "Hi!" };
					al1.AddRange (s1);
				} catch (NotSupportedException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "add to read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList ();
					al1.AddRange (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "add to read only error not thrown");
			}

			{
				ArrayList a1 = new ArrayList ();
				Assert.AreEqual (0, a1.Count, "ArrayList should start empty");
				char [] coll = { 'a', 'b', 'c' };
				a1.AddRange (coll);
				Assert.AreEqual (3, a1.Count, "ArrayList has wrong elements");
				a1.AddRange (coll);
				Assert.AreEqual (6, a1.Count, "ArrayList has wrong elements");
			}

			{
				ArrayList list = new ArrayList ();

				for (int i = 0; i < 100; i++) {
					list.Add (1);
				}

				Assert.AreEqual (49, list.BinarySearch (1), "BinarySearch off-by-one bug");
			}
		}

		[Test]
		public void TestBinarySearch ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList ();
					String [] s1 = { "This", "is", "a", "test" };
					al1.AddRange (s1);
					al1.BinarySearch (42);
				} catch (InvalidOperationException) {
					// this is what .NET throws
					errorThrown = true;
				} catch (ArgumentException) {
					// this is what the docs say it should throw
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "search-for-wrong-type error not thrown");
			}

			{
				char [] arr = { 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd' };
				ArrayList al1 = new ArrayList (arr);
				Assert.IsTrue (al1.BinarySearch ('c') >= 3, "couldn't find elem #1");
				Assert.IsTrue (al1.BinarySearch ('c') < 6, "couldn't find elem #2");
			}
			{
				char [] arr = { 'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e' };
				ArrayList al1 = new ArrayList (arr);
				Assert.AreEqual (-4, al1.BinarySearch ('c'), "couldn't find next-higher elem");
			}
			{
				char [] arr = { 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd' };
				ArrayList al1 = new ArrayList (arr);
				Assert.AreEqual (-9, al1.BinarySearch ('e'), "couldn't find end");
			}

		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BinarySearch_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.BinarySearch (Int32.MaxValue, 1, this, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BinarySearch_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.BinarySearch (1, Int32.MaxValue, this, null);
		}

		[Test]
		public void BinarySearch_Null ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			Assert.AreEqual (-1, al.BinarySearch (null), "null");
		}

		// TODO - BinarySearch with IComparer

		[Test]
		public void TestClear ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList ());
					al1.Clear ();
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "add to fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					al1.Clear ();
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "add to read only error not thrown");
			}
			{
				ArrayList al1 = new ArrayList ();
				al1.Add ('c');
				Assert.AreEqual (1, al1.Count, "should have one element");
				al1.Clear ();
				Assert.AreEqual (0, al1.Count, "should be empty");
			}
			{
				int [] i1 = { 1, 2, 3, 4 };
				ArrayList al1 = new ArrayList (i1);
				Assert.AreEqual (i1.Length, al1.Count, "should have elements");
				int capacity = al1.Capacity;
				al1.Clear ();
				Assert.AreEqual (0, al1.Count, "should be empty again");
				Assert.AreEqual (capacity, al1.Capacity, "capacity shouldn't have changed");
			}
		}

		[Test]
		public void TestClone ()
		{
			{
				char [] c1 = { 'a', 'b', 'c' };
				ArrayList al1 = new ArrayList (c1);
				ArrayList al2 = (ArrayList) al1.Clone ();
				Assert.AreEqual (al1 [0], al2 [0], "ArrayList match");
				Assert.AreEqual (al1 [1], al2 [1], "ArrayList match");
				Assert.AreEqual (al1 [2], al2 [2], "ArrayList match");
			}
			{
				char [] d10 = { 'a', 'b' };
				char [] d11 = { 'a', 'c' };
				char [] d12 = { 'b', 'c' };
				char [] [] d1 = { d10, d11, d12 };
				ArrayList al1 = new ArrayList (d1);
				ArrayList al2 = (ArrayList) al1.Clone ();
				Assert.AreEqual (al1 [0], al2 [0], "Array match");
				Assert.AreEqual (al1 [1], al2 [1], "Array match");
				Assert.AreEqual (al1 [2], al2 [2], "Array match");

				((char []) al1 [0]) [0] = 'z';
				Assert.AreEqual (al1 [0], al2 [0], "shallow copy");
			}
		}

		[Test]
		public void TestContains ()
		{
			char [] c1 = { 'a', 'b', 'c' };
			ArrayList al1 = new ArrayList (c1);
			Assert.IsTrue (!al1.Contains (null), "never find a null");
			Assert.IsTrue (al1.Contains ('b'), "can't find value");
			Assert.IsTrue (!al1.Contains ('?'), "shouldn't find value");
		}

		[Test]
		public void TestCopyTo ()
		{
			{
				bool errorThrown = false;
				try {
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					al1.CopyTo (null, 2);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 1");
			}
			{
				bool errorThrown = false;
				try {
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					Char [,] c2 = new Char [2, 2];
					al1.CopyTo (c2, 2);
				} catch (ArgumentException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 2");
			}
			{
				bool errorThrown = false;
				try {
					// This appears to be a bug in the ArrayList Constructor.
					// It throws a RankException if a multidimensional Array
					// is passed. The docs imply that an IEnumerator is used
					// to retrieve the items from the collection, so this should
					// work.  In anycase this test is for CopyTo, so use what
					// works on both platforms.
					//Char[,] c1 = new Char[2,2];
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					Char [] c2 = new Char [2];
					al1.CopyTo (c2, 2);
				} catch (ArgumentException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 3");
			}
			{
				bool errorThrown = false;
				try {
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					Char [] c2 = new Char [2];
					al1.CopyTo (c2, -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 4");
			}
			{
				bool errorThrown = false;
				try {
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					Char [] c2 = new Char [2];
					al1.CopyTo (c2, 3);
				} catch (ArgumentException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 5: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 5");
			}
			{
				bool errorThrown = false;
				try {
					Char [] c1 = new Char [2];
					ArrayList al1 = new ArrayList (c1);
					Char [] c2 = new Char [2];
					al1.CopyTo (c2, 1);
				} catch (ArgumentException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 6: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 6");
			}
			{
				bool errorThrown = false;
				try {
					String [] c1 = { "String", "array" };
					ArrayList al1 = new ArrayList (c1);
					Char [] c2 = new Char [2];
					al1.CopyTo (c2, 0);
				} catch (InvalidCastException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 7: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "error not thrown 7");
			}

			Char [] orig = { 'a', 'b', 'c', 'd' };
			ArrayList al = new ArrayList (orig);
			Char [] copy = new Char [10];
			Array.Clear (copy, 0, copy.Length);
			al.CopyTo (copy, 3);
			Assert.AreEqual ((char) 0, copy [0], "Wrong CopyTo 0");
			Assert.AreEqual ((char) 0, copy [1], "Wrong CopyTo 1");
			Assert.AreEqual ((char) 0, copy [2], "Wrong CopyTo 2");
			Assert.AreEqual (orig [0], copy [3], "Wrong CopyTo 3");
			Assert.AreEqual (orig [1], copy [4], "Wrong CopyTo 4");
			Assert.AreEqual (orig [2], copy [5], "Wrong CopyTo 5");
			Assert.AreEqual (orig [3], copy [6], "Wrong CopyTo 6");
			Assert.AreEqual ((char) 0, copy [7], "Wrong CopyTo 7");
			Assert.AreEqual ((char) 0, copy [8], "Wrong CopyTo 8");
			Assert.AreEqual ((char) 0, copy [9], "Wrong CopyTo 9");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.CopyTo (Int32.MaxValue, new byte [2], 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_ArrayIndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.CopyTo (0, new byte [2], Int32.MaxValue, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.CopyTo (0, new byte [2], 0, Int32.MaxValue);
		}

		[Test]
		public void TestFixedSize ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = ArrayList.FixedSize (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "null arg error not thrown");
			}
			{
				ArrayList al1 = new ArrayList ();
				Assert.AreEqual (false, al1.IsFixedSize, "arrays start un-fixed.");
				ArrayList al2 = ArrayList.FixedSize (al1);
				Assert.AreEqual (true, al2.IsFixedSize, "should be fixed.");
			}
		}

		[Test]
		public void TestEnumerator ()
		{
			String [] s1 = { "this", "is", "a", "test" };
			ArrayList al1 = new ArrayList (s1);
			IEnumerator en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Add ("something");
			try {
				en.MoveNext ();
				Assert.Fail ("Add() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.AddRange (al1);
			try {
				en.MoveNext ();
				Assert.Fail ("AddRange() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Clear ();
			try {
				en.MoveNext ();
				Assert.Fail ("Clear() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			al1 = new ArrayList (s1);
			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Insert (0, "new first");
			try {
				en.MoveNext ();
				Assert.Fail ("Insert() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.InsertRange (0, al1);
			try {
				en.MoveNext ();
				Assert.Fail ("InsertRange() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Remove ("this");
			try {
				en.MoveNext ();
				Assert.Fail ("Remove() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.RemoveAt (2);
			try {
				en.MoveNext ();
				Assert.Fail ("RemoveAt() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.RemoveRange (1, 1);
			try {
				en.MoveNext ();
				Assert.Fail ("RemoveRange() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Reverse ();
			try {
				en.MoveNext ();
				Assert.Fail ("Reverse() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}

			en = al1.GetEnumerator ();
			en.MoveNext ();
			al1.Sort ();
			try {
				en.MoveNext ();
				Assert.Fail ("Sort() didn't invalidate the enumerator");
			} catch (InvalidOperationException) {
				// do nothing...this is what we expect
			}
		}

		[Test]
		public void TestGetEnumerator ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					IEnumerator en = a.GetEnumerator (-1, 1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					IEnumerator en = a.GetEnumerator (1, -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					IEnumerator en = a.GetEnumerator (1, 1);
				} catch (ArgumentException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "out-of-range index error not thrown");
			}
			{
				String [] s1 = { "this", "is", "a", "test" };
				ArrayList al1 = new ArrayList (s1);
				IEnumerator en = al1.GetEnumerator ();
				Assert.IsNotNull (en, "No enumerator");

				for (int i = 0; i < s1.Length; i++) {
					en.MoveNext ();
					Assert.AreEqual (al1 [i], en.Current, "Not enumerating");
				}
			}
			{
				String [] s1 = { "this", "is", "a", "test" };
				ArrayList al1 = new ArrayList (s1);
				IEnumerator en = al1.GetEnumerator (1, 2);
				Assert.IsNotNull (en, "No enumerator");

				for (int i = 0; i < 2; i++) {
					en.MoveNext ();
					Assert.AreEqual (al1 [i + 1], en.Current, "Not enumerating");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.GetEnumerator (Int32.MaxValue, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEnumerator_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.GetEnumerator (0, Int32.MaxValue);
		}

		[Test]
		public void TestGetRange ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					ArrayList b = a.GetRange (-1, 1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					ArrayList b = a.GetRange (1, -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList ();
					ArrayList b = a.GetRange (1, 1);
				} catch (ArgumentException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "out-of-range index error not thrown");
			}
			{
				char [] chars = { 'a', 'b', 'c', 'd', 'e', 'f' };
				ArrayList a = new ArrayList (chars);
				ArrayList b = a.GetRange (1, 3);
				Assert.AreEqual (3, b.Count, "GetRange returned wrong size ArrayList");
				for (int i = 0; i < b.Count; i++) {
					Assert.AreEqual (chars [i + 1], b [i], "range didn't work");
				}

				a [2] = '?'; // should screw up ArrayList b.
				bool errorThrown = false;
				try {
					int i = b.Count;
				} catch (InvalidOperationException) {
					errorThrown = true;
				}
				Assert.AreEqual (true, errorThrown, "Munging 'a' should mess up 'b'");
			}
			{
				char [] chars = { 'a', 'b', 'c', 'd', 'e', 'f' };
				ArrayList a = new ArrayList (chars);
				ArrayList b = a.GetRange (3, 3);
				object [] obj_chars = b.ToArray ();
				for (int i = 0; i < 3; i++) {
					char c = (char) obj_chars [i];
					Assert.AreEqual (chars [i + 3], c, "range.ToArray didn't work");
				}
				char [] new_chars = (char []) b.ToArray (typeof (char));
				for (int i = 0; i < 3; i++) {
					Assert.AreEqual (chars [i + 3], new_chars [i], "range.ToArray with type didn't work");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetRange_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.GetRange (Int32.MaxValue, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetRange_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.GetRange (0, Int32.MaxValue);
		}

		[Test]
		public void TestIndexOf ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (1);
					int i = a.IndexOf ('a', -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative indexof error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (1);
					int i = a.IndexOf ('a', 2);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "past-end indexof error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (1);
					int i = a.IndexOf ('a', 0, -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative indexof error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (1);
					int i = a.IndexOf ('a', 0, 2);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "past-end indexof error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (2);
					int i = a.IndexOf ('a', 1, 2);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "past-end indexof error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c', 'd', 'e' };
				ArrayList a = new ArrayList (c);
				Assert.AreEqual (-1, a.IndexOf (null), "never find null");
				Assert.AreEqual (-1, a.IndexOf (null, 0), "never find null");
				Assert.AreEqual (-1, a.IndexOf (null, 0, 5), "never find null");
				Assert.AreEqual (2, a.IndexOf ('c'), "can't find elem");
				Assert.AreEqual (2, a.IndexOf ('c', 2), "can't find elem");
				Assert.AreEqual (2, a.IndexOf ('c', 2, 2), "can't find elem");
				Assert.AreEqual (-1, a.IndexOf ('c', 3, 2), "shouldn't find elem");
				Assert.AreEqual (-1, a.IndexOf ('?'), "shouldn't find");
				Assert.AreEqual (-1, a.IndexOf (3), "shouldn't find");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexOf_StartIndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.IndexOf ('a', Int32.MaxValue, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void IndexOf_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.IndexOf ('a', 1, Int32.MaxValue);
		}

		[Test]
		public void TestInsert ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList ());
					al1.Insert (0, "Hi!");
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					al1.Insert (0, "Hi!");
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.Insert (-1, "Hi!");
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.Insert (4, "Hi!");
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to read only error not thrown");
			}
			{
				ArrayList al1 = new ArrayList ();
				Assert.AreEqual (0, al1.Count, "arraylist starts empty");
				al1.Insert (0, 'a');
				al1.Insert (1, 'b');
				al1.Insert (0, 'c');
				Assert.AreEqual (3, al1.Count, "arraylist needs stuff");
				Assert.AreEqual ('c', al1 [0], "arraylist got stuff");
				Assert.AreEqual ('a', al1 [1], "arraylist got stuff");
				Assert.AreEqual ('b', al1 [2], "arraylist got stuff");
			}
		}

		[Test]
		public void TestInsertRange ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList ());
					string [] s = { "Hi!" };
					al1.InsertRange (0, s);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					string [] s = { "Hi!" };
					al1.InsertRange (0, s);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "insert to read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					string [] s = { "Hi!" };
					al1.InsertRange (-1, s);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "negative index insert error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					string [] s = { "Hi!" };
					al1.InsertRange (4, s);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "out-of-range insert error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.InsertRange (0, null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "null insert error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c' };
				ArrayList a = new ArrayList (c);
				a.InsertRange (1, c);
				Assert.AreEqual ('a', a [0], "bad insert 1");
				Assert.AreEqual ('a', a [1], "bad insert 2");
				Assert.AreEqual ('b', a [2], "bad insert 3");
				Assert.AreEqual ('c', a [3], "bad insert 4");
				Assert.AreEqual ('b', a [4], "bad insert 5");
				Assert.AreEqual ('c', a [5], "bad insert 6");
			}
		}

		[Test]
		public void TestLastIndexOf ()
		{
			//{
			//bool errorThrown = false;
			//try {
			//ArrayList a = new ArrayList(1);
			//int i = a.LastIndexOf('a', -1);
			//} catch (ArgumentOutOfRangeException) {
			//errorThrown = true;
			//}
			//Assert.IsTrue (//errorThrown, "first negative lastindexof error not thrown");
			//}
			{
				bool errorThrown = false;
				try {
					ArrayList a = new ArrayList (1);
					int i = a.LastIndexOf ('a', 2);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "past-end lastindexof error not thrown");
			}
			//{
			//bool errorThrown = false;
			//try {
			//ArrayList a = new ArrayList(1);
			//int i = a.LastIndexOf('a', 0, -1);
			//} catch (ArgumentOutOfRangeException) {
			//errorThrown = true;
			//}
			//Assert.IsTrue (//errorThrown, "second negative lastindexof error not thrown");
			//}
			//{
			//bool errorThrown = false;
			//try {
			//ArrayList a = new ArrayList(1);
			//int i = a.LastIndexOf('a', 0, 2);
			//} catch (ArgumentOutOfRangeException) {
			//errorThrown = true;
			//}
			//Assert.IsTrue (//errorThrown, "past-end lastindexof error not thrown");
			//}
			//{
			//bool errorThrown = false;
			//try {
			//ArrayList a = new ArrayList(2);
			//int i = a.LastIndexOf('a', 0, 2);
			//} catch (ArgumentOutOfRangeException) {
			//errorThrown = true;
			//}
			//Assert.IsTrue (//errorThrown, "past-end lastindexof error not thrown");
			//}
			int iTest = 0;
			try {
				char [] c = { 'a', 'b', 'c', 'd', 'e' };
				ArrayList a = new ArrayList (c);
				Assert.AreEqual (-1, a.LastIndexOf (null), "never find null");
				iTest++;
				Assert.AreEqual (-1, a.LastIndexOf (null, 4), "never find null");
				iTest++;
				Assert.AreEqual (-1, a.LastIndexOf (null, 4, 5), "never find null");
				iTest++;
				Assert.AreEqual (2, a.LastIndexOf ('c'), "can't find elem");
				iTest++;
				Assert.AreEqual (2, a.LastIndexOf ('c', 4), "can't find elem");
				iTest++;
				Assert.AreEqual (2, a.LastIndexOf ('c', 3, 2), "can't find elem");
				iTest++;
				Assert.AreEqual (-1, a.LastIndexOf ('c', 4, 2), "shouldn't find elem");
				iTest++;
				Assert.AreEqual (-1, a.LastIndexOf ('?'), "shouldn't find");
				iTest++;
				Assert.AreEqual (-1, a.LastIndexOf (1), "shouldn't find");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception caught when iTest=" + iTest + ". e=" + e);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void LastIndexOf_StartIndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.LastIndexOf ('a', Int32.MaxValue, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void LastIndexOf_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.LastIndexOf ('a', 1, Int32.MaxValue);
		}

		[Test]
		public void TestReadOnly ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = ArrayList.ReadOnly (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "null arg error not thrown");
			}
			{
				ArrayList al1 = new ArrayList ();
				Assert.AreEqual (false, al1.IsReadOnly, "arrays start writeable.");
				ArrayList al2 = ArrayList.ReadOnly (al1);
				Assert.AreEqual (true, al2.IsReadOnly, "should be readonly.");
			}
		}

		[Test]
		public void TestRemove ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList (3));
					al1.Remove (1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList (3));
					al1.Remove (1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove read only error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c' };
				ArrayList a = new ArrayList (c);
				a.Remove (1);
				a.Remove ('?');
				Assert.AreEqual (c.Length, a.Count, "should be unchanged");
				a.Remove ('a');
				Assert.AreEqual (2, a.Count, "should be changed");
				Assert.AreEqual ('b', a [0], "should have shifted");
				Assert.AreEqual ('c', a [1], "should have shifted");
			}
		}

		[Test]
		public void TestRemoveAt ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList (3));
					al1.RemoveAt (1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove from fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList (3));
					al1.RemoveAt (1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove from read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.RemoveAt (-1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove at negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.RemoveAt (4);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "remove at out-of-range index error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c' };
				ArrayList a = new ArrayList (c);
				a.RemoveAt (0);
				Assert.AreEqual (2, a.Count, "should be changed");
				Assert.AreEqual ('b', a [0], "should have shifted");
				Assert.AreEqual ('c', a [1], "should have shifted");
			}
		}

		[Test]
		public void TestRemoveRange ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.FixedSize (new ArrayList (3));
					al1.RemoveRange (0, 1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "removerange from fixed size error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList (3));
					al1.RemoveRange (0, 1);
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "removerange from read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.RemoveRange (-1, 1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "removerange at negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.RemoveRange (0, -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "removerange at negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.RemoveRange (2, 3);
				} catch (ArgumentException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "removerange at bad range error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c' };
				ArrayList a = new ArrayList (c);
				a.RemoveRange (1, 2);
				Assert.AreEqual (1, a.Count, "should be changed");
				Assert.AreEqual ('a', a [0], "should have shifted");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveRange_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.RemoveRange (Int32.MaxValue, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveRange_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.RemoveRange (1, Int32.MaxValue);
		}

		[Test]
		public void TestRepeat ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = ArrayList.Repeat ('c', -1);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "repeat negative copies error not thrown");
			}
			{
				ArrayList al1 = ArrayList.Repeat ("huh?", 0);
				Assert.AreEqual (0, al1.Count, "should be nothing in array");
			}
			{
				ArrayList al1 = ArrayList.Repeat ("huh?", 3);
				Assert.AreEqual (3, al1.Count, "should be something in array");
				Assert.AreEqual ("huh?", al1 [0], "array elem doesn't check");
				Assert.AreEqual ("huh?", al1 [1], "array elem doesn't check");
				Assert.AreEqual ("huh?", al1 [2], "array elem doesn't check");
			}
		}

		[Test]
		public void TestReverse ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					al1.Reverse ();
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "reverse on read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					char [] c = new Char [2];
					ArrayList al1 = new ArrayList (c);
					al1.Reverse (0, 3);
				} catch (ArgumentException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					char [] c = new Char [2];
					ArrayList al1 = new ArrayList (c);
					al1.Reverse (3, 0);
				} catch (ArgumentException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "error not thrown");
			}
			{
				char [] c = { 'a', 'b', 'c', 'd', 'e' };
				ArrayList al1 = new ArrayList (c);
				al1.Reverse (2, 1);
				for (int i = 0; i < al1.Count; i++) {
					Assert.AreEqual (c [i], al1 [i], "Should be no change yet");
				}
				al1.Reverse ();
				for (int i = 0; i < al1.Count; i++) {
					Assert.AreEqual (c [i], al1 [4 - i], "Should be reversed");
				}
				al1.Reverse ();
				for (int i = 0; i < al1.Count; i++) {
					Assert.AreEqual (c [i], al1 [i], "Should be back to normal");
				}
				al1.Reverse (1, 3);
				Assert.AreEqual (c [0], al1 [0], "Should be back to normal");
				Assert.AreEqual (c [3], al1 [1], "Should be back to normal");
				Assert.AreEqual (c [2], al1 [2], "Should be back to normal");
				Assert.AreEqual (c [1], al1 [3], "Should be back to normal");
				Assert.AreEqual (c [4], al1 [4], "Should be back to normal");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Reverse_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.Reverse (Int32.MaxValue, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Reverse_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.Reverse (1, Int32.MaxValue);
		}

		[Test]
		public void TestSetRange ()
		{
			{
				bool errorThrown = false;
				try {
					char [] c = { 'a', 'b', 'c' };
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList (3));
					al1.SetRange (0, c);
				} catch (NotSupportedException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 1: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "setrange on read only error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.SetRange (0, null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 2: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "setrange with null error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					char [] c = { 'a', 'b', 'c' };
					ArrayList al1 = new ArrayList (3);
					al1.SetRange (-1, c);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 3: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "setrange with negative index error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					char [] c = { 'a', 'b', 'c' };
					ArrayList al1 = new ArrayList (3);
					al1.SetRange (2, c);
				} catch (ArgumentOutOfRangeException) {
					errorThrown = true;
				} catch (Exception e) {
					Assert.Fail ("Incorrect exception thrown at 4: " + e.ToString ());
				}
				Assert.IsTrue (errorThrown, "setrange with too much error not thrown");
			}

			{
				char [] c = { 'a', 'b', 'c' };
				ArrayList al1 = ArrayList.Repeat ('?', 3);
				Assert.IsTrue (c [0] != (char) al1 [0], "no match yet");
				Assert.IsTrue (c [1] != (char) al1 [1], "no match yet");
				Assert.IsTrue (c [2] != (char) al1 [2], "no match yet");
				al1.SetRange (0, c);
				Assert.AreEqual (c [0], al1 [0], "should match");
				Assert.AreEqual (c [1], al1 [1], "should match");
				Assert.AreEqual (c [2], al1 [2], "should match");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetRange_Overflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.SetRange (Int32.MaxValue, new ArrayList ());
		}

		[Test]
		public void TestInsertRange_this ()
		{
			String [] s1 = { "this", "is", "a", "test" };
			ArrayList al = new ArrayList (s1);
			al.InsertRange (2, al);
			String [] s2 = { "this", "is", "this", "is", "a", "test", "a", "test" };
			for (int i = 0; i < al.Count; i++) {
				Assert.AreEqual (s2 [i], al [i], "at i=" + i);
			}
		}

		[Test]
		public void TestSort ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 =
						ArrayList.ReadOnly (new ArrayList ());
					al1.Sort ();
				} catch (NotSupportedException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "sort on read only error not thrown");
			}
			{
				char [] starter = { 'd', 'b', 'f', 'e', 'a', 'c' };
				ArrayList al1 = new ArrayList (starter);
				al1.Sort ();
				Assert.AreEqual ('a', al1 [0], "Should be sorted");
				Assert.AreEqual ('b', al1 [1], "Should be sorted");
				Assert.AreEqual ('c', al1 [2], "Should be sorted");
				Assert.AreEqual ('d', al1 [3], "Should be sorted");
				Assert.AreEqual ('e', al1 [4], "Should be sorted");
				Assert.AreEqual ('f', al1 [5], "Should be sorted");
			}
			{
				ArrayList al1 = new ArrayList ();
				al1.Add (null);
				al1.Add (null);
				al1.Add (32);
				al1.Add (33);
				al1.Add (null);
				al1.Add (null);

				al1.Sort ();
				Assert.AreEqual (null, al1 [0], "Should be null (0)");
				Assert.AreEqual (null, al1 [1], "Should be null (1)");
				Assert.AreEqual (null, al1 [2], "Should be null (2)");
				Assert.AreEqual (null, al1 [3], "Should be null (3)");
				Assert.AreEqual (32, al1 [4], "Should be 32");
				Assert.AreEqual (33, al1 [5], "Should be 33");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Sort_IndexOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.Sort (Int32.MaxValue, 1, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Sort_CountOverflow ()
		{
			ArrayList al = new ArrayList ();
			al.Add (this);
			al.Sort (1, Int32.MaxValue, null);
		}

		// TODO - Sort with IComparers

		// TODO - Synchronize

		[Test]
		public void TestToArray ()
		{
			{
				bool errorThrown = false;
				try {
					ArrayList al1 = new ArrayList (3);
					al1.ToArray (null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "toarray with null error not thrown");
			}
			{
				bool errorThrown = false;
				try {
					char [] c = { 'a', 'b', 'c' };
					string s = "huh?";
					ArrayList al1 = new ArrayList (c);
					al1.ToArray (s.GetType ());
				} catch (InvalidCastException) {
					errorThrown = true;
				}
				Assert.IsTrue (errorThrown, "toarray with bad type error not thrown");
			}
			{
				char [] c1 = { 'a', 'b', 'c', 'd', 'e' };
				ArrayList al1 = new ArrayList (c1);
				object [] o2 = al1.ToArray ();
				for (int i = 0; i < c1.Length; i++) {
					Assert.AreEqual (c1 [i], o2 [i], "should be copy");
				}
				Array c2 = al1.ToArray (c1 [0].GetType ());
				for (int i = 0; i < c1.Length; i++) {
					Assert.AreEqual (c1 [i], c2.GetValue (i), "should be copy");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TrimToSize_ReadOnly ()
		{
			ArrayList al1 = ArrayList.ReadOnly (new ArrayList ());
			al1.TrimToSize ();
		}

		[Test]
		public void TrimToSize ()
		{
			ArrayList al1 = new ArrayList ();
#if NET_2_0
		// Capacity is 0 under 2.0
		int capacity = 4;
#else
			int capacity = al1.Capacity;
#endif
			int size = capacity / 2;
			for (int i = 1; i <= size; i++) {
				al1.Add ('?');
			}
			al1.RemoveAt (0);
			al1.TrimToSize ();
			Assert.AreEqual (size - 1, al1.Capacity, "no capacity match");

			al1.Clear ();
			al1.TrimToSize ();
			Assert.AreEqual (capacity, al1.Capacity, "no default capacity");
		}

		class Comparer : IComparer
		{

			private bool called = false;

			public bool Called
			{
				get
				{
					bool result = called;
					called = false;
					return called;
				}
			}

			public int Compare (object x, object y)
			{
				called = true;
				return 0;
			}
		}

		[Test]
		public void BinarySearch1_EmptyList ()
		{
			ArrayList list = new ArrayList ();
			Assert.AreEqual (-1, list.BinarySearch (0), "BinarySearch");
		}

		[Test]
		public void BinarySearch2_EmptyList ()
		{
			Comparer comparer = new Comparer ();
			ArrayList list = new ArrayList ();
			Assert.AreEqual (-1, list.BinarySearch (0, comparer), "BinarySearch");
			// bug 77030 - the comparer isn't called for an empty array/list
			Assert.IsTrue (!comparer.Called, "Called");
		}

		[Test]
		public void BinarySearch3_EmptyList ()
		{
			Comparer comparer = new Comparer ();
			ArrayList list = new ArrayList ();
			Assert.AreEqual (-1, list.BinarySearch (0, 0, 0, comparer), "BinarySearch");
			// bug 77030 - the comparer isn't called for an empty array/list
			Assert.IsTrue (!comparer.Called, "Called");
		}

		[Test]
#if ONLY_1_1
	[Category ("NotDotNet")] // MS bug
#endif
		public void AddRange_GetRange ()
		{
			ArrayList source = ArrayList.Adapter (new object [] { "1", "2" });
			Assert.AreEqual (2, source.Count, "#1");
			Assert.AreEqual ("1", source [0], "#2");
			Assert.AreEqual ("2", source [1], "#3");
			ArrayList range = source.GetRange (1, 1);
			Assert.AreEqual (1, range.Count, "#4");
			Assert.AreEqual ("2", range [0], "#5");
			ArrayList target = new ArrayList ();
			target.AddRange (range);
			Assert.AreEqual (1, target.Count, "#6");
			Assert.AreEqual ("2", target [0], "#7");
		}

		[Test]
#if ONLY_1_1
	[Category ("NotDotNet")] // MS bug
#endif
		public void IterateSelf ()
		{
			ArrayList list = new ArrayList ();
			list.Add (list);
			IEnumerator enumerator = list.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.IsTrue (object.ReferenceEquals (list, enumerator.Current), "#2");
			Assert.IsTrue (!enumerator.MoveNext (), "#3");
		}
	}
}
