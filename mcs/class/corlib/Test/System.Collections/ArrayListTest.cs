// ArrayListTest.cs - NUnit Test Cases for the System.Collections.ArrayList class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


/// <summary>ArrayList test.</summary>
public class ArrayListTest : TestCase {
	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	public void TestCtor() {
		{
			ArrayList al1 = new ArrayList();
			AssertNotNull("no basic ArrayList", al1);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null icollection error not thrown", 
			       errorThrown);
		}
		{
			// what can I say?  I like chars.  [--DB]
			char[] coll = {'a', 'b', 'c', 'd'};
			ArrayList al1 = new ArrayList(coll);
			AssertNotNull("no icollection ArrayList", al1);
			for (int i = 0; i < coll.Length; i++) {
				AssertEquals(i + " not ctor'ed properly.",
					     coll[i], al1[i]);
			}
		}
		{
			try {
				Char[,] c1 = new Char[2,2];
				ArrayList al1 = new ArrayList(c1);
				Fail ("Should fail with multi-dimensional array in constructor.");
			} catch (RankException) {
			}
		}

		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(-1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative capacity error not thrown", 
			       errorThrown);
		}
	}

	public void TestCapacity() {
		for (int i = 1; i < 100; i++) {
			ArrayList al1 = new ArrayList(i);
			AssertEquals("Bad capacity of " + i,
				     i, al1.Capacity);
		}
		{
			ArrayList al1 = new ArrayList(0);
			// LAMESPEC: 
			// AssertEquals("Bad capacity when set to 0",
			//	     16, al1.Capacity);
			al1.Add ("?");			
			AssertEquals("Bad capacity when set to 0",
				     16, al1.Capacity);
		}		
		{
			ArrayList al1 = new ArrayList();
			AssertEquals("Bad default capacity",
				     16, al1.Capacity);
		}
	}
	
	public void TestCount() {
		{
			ArrayList al1 = new ArrayList();
			AssertEquals("Bad initial count",
				     0, al1.Count);
			for (int i = 1; i <= 100; i++) {
				al1.Add(i);
				AssertEquals("Bad count " + i,
					     i, al1.Count);
			}
		}
		for (int i = 0; i < 100; i++) {
			char[] coll = new Char[i];
			ArrayList al1 = new ArrayList(coll);
			AssertEquals("Bad count for " + i,
				     i, al1.Count);
		}
	}

	public void TestIsFixed() {
		ArrayList al1 = new ArrayList();
		Assert("should not be fixed by default", !al1.IsFixedSize);
		ArrayList al2 = ArrayList.FixedSize(al1);
		Assert("fixed-size wrapper not working", al2.IsFixedSize);
	}

	public void TestIsReadOnly() {
		ArrayList al1 = new ArrayList();
		Assert("should not be ReadOnly by default", !al1.IsReadOnly);
		ArrayList al2 = ArrayList.ReadOnly(al1);
		Assert("read-only wrapper not working", al2.IsReadOnly);
	}

	public void TestIsSynchronized() {
		ArrayList al1 = new ArrayList();
		Assert("should not be synchronized by default", 
		       !al1.IsSynchronized);
		ArrayList al2 = ArrayList.Synchronized(al1);
		Assert("synchronized wrapper not working", al2.IsSynchronized);
	}

	public void TestItem() {
		ArrayList al1 = new ArrayList();
		{
			bool errorThrown = false;
			try {
				object o = al1[-1];
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative item error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				object o = al1[1];
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("past-end item error not thrown", 
			       errorThrown);
		}
		for (int i = 0; i <= 100; i++) {
			al1.Add(i);
		}
		for (int i = 0; i <= 100; i++) {
			AssertEquals("item not fetched for " + i,
				     i, al1[i]);
		}
	}

	public void TestAdapter() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = ArrayList.Adapter(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("null adapter error not thrown", 
			       errorThrown);
		}
		{		
			char[] list = {'a', 'b', 'c', 'd'};
			ArrayList al1 = ArrayList.Adapter(list);
			AssertNotNull("Couldn't get an adapter", al1);
			for (int i = 0; i < list.Length; i++) {
				AssertEquals("adapter not adapting", list[i], al1[i]);
			}
			list[0] = 'z';
			for (int i = 0; i < list.Length; i++) {
				AssertEquals("adapter not adapting", list[i], al1[i]);
			}
		}
		// TODO - test other adapter types?
	}

	public void TestAdd() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList());
				al1.Add("Hi!");
			} catch (NotSupportedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("add to fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.Add("Hi!");
			} catch (NotSupportedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("add to read only error not thrown", 
			       errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			for (int i = 1; i <= 100; i++) {
				al1.Add(i);
				AssertEquals("add failed " + i,
					     i, al1.Count);
				AssertEquals("add failed " + i,
					     i, al1[i-1]);
				
			}
		}
		{
			string [] strArray = new string [] {};
			ArrayList al1 = new ArrayList (strArray);
			al1.Add ("Hi!");
			al1.Add ("Hi!");
			AssertEquals ("add failed", 2, al1.Count);
		}
	}
	
	public void TestAddRange() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList());
				String[] s1 = {"Hi!"};
				al1.AddRange(s1);
			} catch (NotSupportedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("add to fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				String[] s1 = {"Hi!"};
				al1.AddRange(s1);
			} catch (NotSupportedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("add to read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList();
				al1.AddRange(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("add to read only error not thrown", 
			       errorThrown);
		}

		{
			ArrayList a1 = new ArrayList();
			AssertEquals("ArrayList should start empty",
				     0, a1.Count);
			char[] coll = {'a', 'b', 'c'};
			a1.AddRange(coll);
			AssertEquals("ArrayList has wrong elements",
				     3, a1.Count);
			a1.AddRange(coll);
			AssertEquals("ArrayList has wrong elements",
				     6, a1.Count);
		}
	}

	public void TestBinarySearch() {
		{
		bool errorThrown = false;
		try {
			ArrayList al1 = new ArrayList();
			String[] s1 = {"This", "is", "a", "test"};
			al1.AddRange(s1);
			al1.BinarySearch(42);
		} catch (InvalidOperationException) {
			// this is what .NET throws
			errorThrown = true;
		} catch (ArgumentException) {
			// this is what the docs say it should throw
			errorThrown = true;
		} catch (Exception e) {
			Fail ("Incorrect exception thrown at 1: " + e.ToString());
		}
		Assert("search-for-wrong-type error not thrown", 
		       errorThrown);
		}
		
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			ArrayList al1 = new ArrayList(arr);
			Assert("couldn't find elem #1", 
			       al1.BinarySearch('c') >= 3);
			Assert("couldn't find elem #2", 
			       al1.BinarySearch('c') < 6);
		}
		{
			char[] arr = {'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			ArrayList al1 = new ArrayList(arr);
			AssertEquals("couldn't find next-higher elem", 
				     -4, al1.BinarySearch('c'));
		}
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			ArrayList al1 = new ArrayList(arr);
			AssertEquals("couldn't find end", 
				     -9, al1.BinarySearch('e'));
		}
		
	}

	// TODO - BinarySearch with IComparer

	public void TestClear() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList());
				al1.Clear();
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("add to fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.Clear();
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("add to read only error not thrown", 
			       errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			al1.Add('c');
			AssertEquals("should have one element",
				     1, al1.Count);
			al1.Clear();
			AssertEquals("should be empty",
				     0, al1.Count);
		}
		{
			int[] i1 = {1,2,3,4};
			ArrayList al1 = new ArrayList(i1);
			AssertEquals("should have elements",
				     i1.Length, al1.Count);
			int capacity = al1.Capacity;
			al1.Clear();
			AssertEquals("should be empty again",
				     0, al1.Count);
			AssertEquals("capacity shouldn't have changed",
				     capacity, al1.Capacity);
		}
	}

	public void TestClone() {
		{
			char[] c1 = {'a', 'b', 'c'};
			ArrayList al1 = new ArrayList(c1);
			ArrayList al2 = (ArrayList)al1.Clone();
			AssertEquals("ArrayList match", al1[0], al2[0]);
			AssertEquals("ArrayList match", al1[1], al2[1]);
			AssertEquals("ArrayList match", al1[2], al2[2]);
		}
		{
			char[] d10 = {'a', 'b'};
			char[] d11 = {'a', 'c'};
			char[] d12 = {'b', 'c'};
			char[][] d1 = {d10, d11, d12};
			ArrayList al1 = new ArrayList(d1);
			ArrayList al2 = (ArrayList)al1.Clone();
			AssertEquals("Array match", al1[0], al2[0]);
			AssertEquals("Array match", al1[1], al2[1]);
			AssertEquals("Array match", al1[2], al2[2]);
			
			((char[])al1[0])[0] = 'z';
			AssertEquals("shallow copy", al1[0], al2[0]);
		}
	}

	public void TestContains() {
		char[] c1 = {'a', 'b', 'c'};
		ArrayList al1 = new ArrayList(c1);
		Assert("never find a null", !al1.Contains(null));
		Assert("can't find value", al1.Contains('b'));
		Assert("shouldn't find value", !al1.Contains('?'));
	}
	
	public void TestCopyTo() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				al1.CopyTo(null, 2);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("error not thrown 1", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				Char[,] c2 = new Char[2,2];
				al1.CopyTo(c2, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("error not thrown 2", errorThrown);
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
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				Char[] c2 = new Char[2];
				al1.CopyTo(c2, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("error not thrown 3", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				Char[] c2 = new Char[2];
				al1.CopyTo(c2, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("error not thrown 4", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				Char[] c2 = new Char[2];
				al1.CopyTo(c2, 3);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 5: " + e.ToString());
			}
			Assert("error not thrown 5", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				ArrayList al1 = new ArrayList(c1);
				Char[] c2 = new Char[2];
				al1.CopyTo(c2, 1);
			} catch (ArgumentException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 6: " + e.ToString());
			}
			Assert("error not thrown 6", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				String[] c1 = {"String", "array"};
				ArrayList al1 = new ArrayList(c1);
				Char[] c2 = new Char[2];
				al1.CopyTo(c2, 0);
			} catch (InvalidCastException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 7: " + e.ToString());
			}
			Assert("error not thrown 7", errorThrown);
		}

		Char[] orig = {'a', 'b', 'c', 'd'};
		ArrayList al = new ArrayList(orig);
		Char[] copy = new Char[10];
		Array.Clear(copy, 0, copy.Length);
		al.CopyTo(copy, 3);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[0]);
		AssertEquals("Wrong CopyTo 1", (char)0, copy[1]);
		AssertEquals("Wrong CopyTo 2", (char)0, copy[2]);
		AssertEquals("Wrong CopyTo 3", orig[0], copy[3]);
		AssertEquals("Wrong CopyTo 4", orig[1], copy[4]);
		AssertEquals("Wrong CopyTo 5", orig[2], copy[5]);
		AssertEquals("Wrong CopyTo 6", orig[3], copy[6]);
		AssertEquals("Wrong CopyTo 7", (char)0, copy[7]);
		AssertEquals("Wrong CopyTo 8", (char)0, copy[8]);
		AssertEquals("Wrong CopyTo 9", (char)0, copy[9]);
	}

	public void TestFixedSize() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = ArrayList.FixedSize(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null arg error not thrown", errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			AssertEquals("arrays start un-fixed.",
				     false, al1.IsFixedSize);
			ArrayList al2 = ArrayList.FixedSize(al1);
			AssertEquals("should be fixed.",
				     true, al2.IsFixedSize);
		}
	}

	public void TestEnumerator() {
		String[] s1 = {"this", "is", "a", "test"};
		ArrayList al1 = new ArrayList(s1);
		IEnumerator en = al1.GetEnumerator();
		en.MoveNext();
		al1.Add("something");
		try {
			en.MoveNext();
			Fail("Add() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		en = al1.GetEnumerator();
		en.MoveNext();
		al1.AddRange(al1);
		try {
			en.MoveNext();
			Fail("AddRange() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		en = al1.GetEnumerator();
		en.MoveNext();
		al1.Clear();
		try {
			en.MoveNext();
			Fail("Clear() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		al1 = new ArrayList(s1);
		en = al1.GetEnumerator();
		en.MoveNext();
		al1.Insert(0, "new first");
		try {
			en.MoveNext();
			Fail("Insert() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		en = al1.GetEnumerator();
		en.MoveNext();
		al1.InsertRange(0, al1);
		try {
			en.MoveNext();
			Fail("InsertRange() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		en = al1.GetEnumerator();
		en.MoveNext();
		al1.Remove("this");
		try {
			en.MoveNext();
			Fail("Remove() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}

		en = al1.GetEnumerator();
		en.MoveNext();
		al1.RemoveAt(2);
		try {
			en.MoveNext();
			Fail("RemoveAt() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}
		
		en = al1.GetEnumerator();
		en.MoveNext();
		al1.RemoveRange(1, 1);
		try {
			en.MoveNext();
			Fail("RemoveRange() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}
		
		en = al1.GetEnumerator();
		en.MoveNext();
		al1.Reverse();
		try {
			en.MoveNext();
			Fail("Reverse() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}
		
		en = al1.GetEnumerator();
		en.MoveNext();
		al1.Sort();
		try {
			en.MoveNext();
			Fail("Sort() didn't invalidate the enumerator");
		} catch (InvalidOperationException) {
			// do nothing...this is what we expect
		}
	}

	public void TestGetEnumerator() {
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				IEnumerator en = a.GetEnumerator(-1,1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				IEnumerator en = a.GetEnumerator(1,-1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				IEnumerator en = a.GetEnumerator(1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("out-of-range index error not thrown", 
			       errorThrown);
		}
		{
			String[] s1 = {"this", "is", "a", "test"};
			ArrayList al1 = new ArrayList(s1);
			IEnumerator en = al1.GetEnumerator();
			AssertNotNull("No enumerator", en);
			
			for (int i = 0; i < s1.Length; i++) {
				en.MoveNext();
				AssertEquals("Not enumerating", 
					     al1[i], en.Current);
			}
		}
		{
			String[] s1 = {"this", "is", "a", "test"};
			ArrayList al1 = new ArrayList(s1);
			IEnumerator en = al1.GetEnumerator(1,2);
			AssertNotNull("No enumerator", en);
			
			for (int i = 0; i < 2; i++) {
				en.MoveNext();
				AssertEquals("Not enumerating", 
					     al1[i+1], en.Current);
			}
		}
	}

	public void TestGetRange() {
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				ArrayList b = a.GetRange(-1,1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				ArrayList b = a.GetRange(1,-1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList();
				ArrayList b = a.GetRange(1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("out-of-range index error not thrown", 
			       errorThrown);
		}
		{
			char[] chars = {'a', 'b', 'c', 'd', 'e', 'f'};
			ArrayList a = new ArrayList(chars);
			ArrayList b = a.GetRange(1, 3);
			AssertEquals("GetRange returned wrong size ArrayList", 3, b.Count);
			for (int i = 0; i < b.Count; i++) {
				AssertEquals("range didn't work",
					     chars[i+1], b[i]);
			}

			a[2] = '?'; // should screw up ArrayList b.
			bool errorThrown = false;
			try {
				int i = b.Count;
			} catch (InvalidOperationException) {
				errorThrown = true;
			}
			AssertEquals("Munging 'a' should mess up 'b'",
				     true, errorThrown);
		}
	}

	public void TestIndexOf() {
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(1);
				int i = a.IndexOf('a', -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative indexof error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(1);
				int i = a.IndexOf('a', 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("past-end indexof error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(1);
				int i = a.IndexOf('a', 0, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative indexof error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(1);
				int i = a.IndexOf('a', 0, 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("past-end indexof error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(2);
				int i = a.IndexOf('a', 1, 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("past-end indexof error not thrown", 
			       errorThrown);
		}
		{
			char[] c = {'a', 'b', 'c', 'd', 'e'};
			ArrayList a = new ArrayList(c);
			AssertEquals("never find null", 
				     -1, a.IndexOf(null));
			AssertEquals("never find null", 
				     -1, a.IndexOf(null, 0));
			AssertEquals("never find null", 
				     -1, a.IndexOf(null, 0, 5));
			AssertEquals("can't find elem", 
				     2, a.IndexOf('c'));
			AssertEquals("can't find elem", 
				     2, a.IndexOf('c', 2));
			AssertEquals("can't find elem", 
				     2, a.IndexOf('c', 2, 2));
			AssertEquals("shouldn't find elem", 
				     -1, a.IndexOf('c', 3, 2));
			AssertEquals("shouldn't find", -1, a.IndexOf('?'));
			AssertEquals("shouldn't find", -1, a.IndexOf(3));
		}
	}

	public void TestInsert() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList());
				al1.Insert(0, "Hi!");
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("insert to fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.Insert(0, "Hi!");
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("insert to read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.Insert(-1, "Hi!");
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("insert to read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.Insert(4, "Hi!");
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("insert to read only error not thrown", 
			       errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			AssertEquals("arraylist starts empty", 0, al1.Count);
			al1.Insert(0, 'a');
			al1.Insert(1, 'b');
			al1.Insert(0, 'c');
			AssertEquals("arraylist needs stuff", 3, al1.Count);
			AssertEquals("arraylist got stuff", 'c', al1[0]);
			AssertEquals("arraylist got stuff", 'a', al1[1]);
			AssertEquals("arraylist got stuff", 'b', al1[2]);
		}
	}

	public void TestInsertRange() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList());
				string[] s = {"Hi!"};
				al1.InsertRange(0, s);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("insert to fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				string[] s = {"Hi!"};
				al1.InsertRange(0, s);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("insert to read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				string[] s = {"Hi!"};
				al1.InsertRange(-1, s);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("negative index insert error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				string[] s = {"Hi!"};
				al1.InsertRange(4, s);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("out-of-range insert error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.InsertRange(0, null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null insert error not thrown", 
			       errorThrown);
		}
		{
			char[] c = {'a', 'b', 'c'};
			ArrayList a = new ArrayList(c);
			a.InsertRange(1, c);
			AssertEquals("bad insert 1", 'a', a[0]);
			AssertEquals("bad insert 2", 'a', a[1]);
			AssertEquals("bad insert 3", 'b', a[2]);
			AssertEquals("bad insert 4", 'c', a[3]);
			AssertEquals("bad insert 5", 'b', a[4]);
			AssertEquals("bad insert 6", 'c', a[5]);
		}
	}

	public void TestLastIndexOf() {
		//{
		//bool errorThrown = false;
		//try {
		//ArrayList a = new ArrayList(1);
		//int i = a.LastIndexOf('a', -1);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("first negative lastindexof error not thrown", 
		//errorThrown);
		//}
		{
			bool errorThrown = false;
			try {
				ArrayList a = new ArrayList(1);
				int i = a.LastIndexOf('a', 2);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("past-end lastindexof error not thrown", 
			       errorThrown);
		}
		//{
		//bool errorThrown = false;
		//try {
		//ArrayList a = new ArrayList(1);
		//int i = a.LastIndexOf('a', 0, -1);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("second negative lastindexof error not thrown", 
		//errorThrown);
		//}
		//{
		//bool errorThrown = false;
		//try {
		//ArrayList a = new ArrayList(1);
		//int i = a.LastIndexOf('a', 0, 2);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("past-end lastindexof error not thrown", 
		//errorThrown);
		//}
		//{
		//bool errorThrown = false;
		//try {
		//ArrayList a = new ArrayList(2);
		//int i = a.LastIndexOf('a', 0, 2);
		//} catch (ArgumentOutOfRangeException) {
		//errorThrown = true;
		//}
		//Assert("past-end lastindexof error not thrown", 
		//errorThrown);
		//}
		int iTest = 0;
		try {
			char[] c = {'a', 'b', 'c', 'd', 'e'};
			ArrayList a = new ArrayList(c);
			AssertEquals("never find null", 
				     -1, a.LastIndexOf(null));
			iTest++;
			AssertEquals("never find null", 
				     -1, a.LastIndexOf(null, 4));
			iTest++;
			AssertEquals("never find null", 
				     -1, a.LastIndexOf(null, 4, 5));
			iTest++;
			AssertEquals("can't find elem", 
				     2, a.LastIndexOf('c'));
			iTest++;
			AssertEquals("can't find elem", 
				     2, a.LastIndexOf('c', 4));
			iTest++;
			AssertEquals("can't find elem", 
				     2, a.LastIndexOf('c', 3, 2));
			iTest++;
			AssertEquals("shouldn't find elem", 
				     -1, a.LastIndexOf('c', 4, 2));
			iTest++;
			AssertEquals("shouldn't find", -1, a.LastIndexOf('?'));
			iTest++;
			AssertEquals("shouldn't find", -1, a.LastIndexOf(1));
		} catch (Exception e) {
			Fail ("Unexpected exception caught when iTest=" + iTest + ". e=" + e);
		}
	}

	public void TestReadOnly() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = ArrayList.ReadOnly(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("null arg error not thrown", errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			AssertEquals("arrays start writeable.",
				     false, al1.IsReadOnly);
			ArrayList al2 = ArrayList.ReadOnly(al1);
			AssertEquals("should be readonly.",
				     true, al2.IsReadOnly);
		}
	}

	public void TestRemove() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList(3));
				al1.Remove(1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("remove fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList(3));
				al1.Remove(1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("remove read only error not thrown", 
			       errorThrown);
		}
		{
			char[] c = {'a','b','c'};
			ArrayList a = new ArrayList(c);
			a.Remove(1);
			a.Remove('?');
			AssertEquals("should be unchanged", c.Length, a.Count);
			a.Remove('a');
			AssertEquals("should be changed", 2, a.Count);
			AssertEquals("should have shifted", 'b', a[0]);
			AssertEquals("should have shifted", 'c', a[1]);
		}
	}

	public void TestRemoveAt() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList(3));
				al1.RemoveAt(1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("remove from fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList(3));
				al1.RemoveAt(1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("remove from read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.RemoveAt(-1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("remove at negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.RemoveAt(4);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("remove at out-of-range index error not thrown", 
			       errorThrown);
		}
		{
			char[] c = {'a','b','c'};
			ArrayList a = new ArrayList(c);
			a.RemoveAt(0);
			AssertEquals("should be changed", 2, a.Count);
			AssertEquals("should have shifted", 'b', a[0]);
			AssertEquals("should have shifted", 'c', a[1]);
		}
	}

	public void TestRemoveRange() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.FixedSize(new ArrayList(3));
				al1.RemoveRange(0, 1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("removerange from fixed size error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList(3));
				al1.RemoveRange(0, 1);
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("removerange from read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.RemoveRange(-1, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("removerange at negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.RemoveRange(0, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("removerange at negative index error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.RemoveRange(2, 3);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("removerange at bad range error not thrown", 
			       errorThrown);
		}
		{
			char[] c = {'a','b','c'};
			ArrayList a = new ArrayList(c);
			a.RemoveRange(1,2);
			AssertEquals("should be changed", 1, a.Count);
			AssertEquals("should have shifted", 'a', a[0]);
		}
	}

	public void TestRepeat() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = ArrayList.Repeat('c', -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("repeat negative copies error not thrown", 
			       errorThrown);
		}
		{
			ArrayList al1 = ArrayList.Repeat("huh?", 0);
			AssertEquals("should be nothing in array", 
				     0, al1.Count);
		}		
		{
			ArrayList al1 = ArrayList.Repeat("huh?", 3);
			AssertEquals("should be something in array", 
				     3, al1.Count);
			AssertEquals("array elem doesn't check",
				     "huh?", al1[0]);
			AssertEquals("array elem doesn't check",
				     "huh?", al1[1]);
			AssertEquals("array elem doesn't check",
				     "huh?", al1[2]);
		}
	}

	public void TestReverse() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.Reverse();
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("reverse on read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				ArrayList al1 = new ArrayList(c);
				al1.Reverse(0, 3);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				ArrayList al1 = new ArrayList(c);
				al1.Reverse(3, 0);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			char[] c = {'a', 'b', 'c', 'd', 'e'};
			ArrayList al1 = new ArrayList(c);
			al1.Reverse(2,1);
			for (int i = 0; i < al1.Count; i++) {
				AssertEquals("Should be no change yet",
					     c[i], al1[i]);
			}
			al1.Reverse();
			for (int i = 0; i < al1.Count; i++) {
				AssertEquals("Should be reversed",
					     c[i], al1[4-i]);
			}
			al1.Reverse();
			for (int i = 0; i < al1.Count; i++) {
				AssertEquals("Should be back to normal",
					     c[i], al1[i]);
			}
			al1.Reverse(1,3);
			AssertEquals("Should be back to normal", c[0], al1[0]);
			AssertEquals("Should be back to normal", c[3], al1[1]);
			AssertEquals("Should be back to normal", c[2], al1[2]);
			AssertEquals("Should be back to normal", c[1], al1[3]);
			AssertEquals("Should be back to normal", c[4], al1[4]);
		}
	}

	public void TestSetRange() {
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList(3));
				al1.SetRange(0, c);
			} catch (NotSupportedException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 1: " + e.ToString());
			}
			Assert("setrange on read only error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.SetRange(0, null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 2: " + e.ToString());
			}
			Assert("setrange with null error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				ArrayList al1 = new ArrayList(3);
				al1.SetRange(-1, c);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 3: " + e.ToString());
			}
			Assert("setrange with negative index error not thrown",
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				ArrayList al1 = new ArrayList(3);
				al1.SetRange(2, c);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			} catch (Exception e) {
				Fail ("Incorrect exception thrown at 4: " + e.ToString());
			}
			Assert("setrange with too much error not thrown",
			       errorThrown);
		}

		{
			char[] c = {'a', 'b', 'c'};
			ArrayList al1 = ArrayList.Repeat('?', 3);
			Assert("no match yet", c[0] != (char)al1[0]);
			Assert("no match yet", c[1] != (char)al1[1]);
			Assert("no match yet", c[2] != (char)al1[2]);
			al1.SetRange(0, c);
			AssertEquals("should match", c[0], al1[0]);
			AssertEquals("should match", c[1], al1[1]);
			AssertEquals("should match", c[2], al1[2]);
		}
	}

	public void TestInsertRange_this() {
		String[] s1 = {"this", "is", "a", "test"};
		ArrayList al = new ArrayList(s1);
		al.InsertRange(2, al);
		String[] s2 = {"this", "is", "this", "is", "a", "test", "a", "test"};
		for (int i=0; i < al.Count; i++) {
			AssertEquals("at i=" + i, s2[i], al[i]);
		}
	}

	public void TestSort() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.Sort();
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("sort on read only error not thrown", 
			       errorThrown);
		}
		{
			char[] starter = {'d', 'b', 'f', 'e', 'a', 'c'};
			ArrayList al1 = new ArrayList(starter);
			al1.Sort();
			AssertEquals("Should be sorted", 'a', al1[0]);
			AssertEquals("Should be sorted", 'b', al1[1]);
			AssertEquals("Should be sorted", 'c', al1[2]);
			AssertEquals("Should be sorted", 'd', al1[3]);
			AssertEquals("Should be sorted", 'e', al1[4]);
			AssertEquals("Should be sorted", 'f', al1[5]);
		}
	}

	// TODO - Sort with IComparers

	// TODO - Synchronize

	public void TestToArray() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = new ArrayList(3);
				al1.ToArray(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("toarray with null error not thrown", 
			       errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				string s = "huh?";
				ArrayList al1 = new ArrayList(c);
				al1.ToArray(s.GetType());
			} catch (InvalidCastException) {
				errorThrown = true;
			}
			Assert("toarray with bad type error not thrown", 
			       errorThrown);
		}
		{
			char[] c1 = {'a', 'b', 'c', 'd', 'e'};
			ArrayList al1 = new ArrayList(c1);
			object[] o2 = al1.ToArray();
			for (int i = 0; i < c1.Length; i++) {
				AssertEquals("should be copy", c1[i], o2[i]);
			}
			Array c2 = al1.ToArray(c1[0].GetType());
			for (int i = 0; i < c1.Length; i++) {
				AssertEquals("should be copy", 
					     c1[i], c2.GetValue(i));
			}
		}
	}

	public void TestTrimToSize() {
		{
			bool errorThrown = false;
			try {
				ArrayList al1 = 
					ArrayList.ReadOnly(new ArrayList());
				al1.TrimToSize();
			} catch (NotSupportedException) {
				errorThrown = true;
			}
			Assert("trim read only error not thrown", 
			       errorThrown);
		}
		{
			ArrayList al1 = new ArrayList();
			int capacity = al1.Capacity;
			int size = capacity / 2;
			for (int i = 1; i <=size; i++) {
				al1.Add('?');
			}
			al1.RemoveAt(0);
			al1.TrimToSize();
			AssertEquals("no capacity match", 
				     size - 1, al1.Capacity);

			al1.Clear();
			al1.TrimToSize();
			AssertEquals("no default capacity", 
				     capacity, al1.Capacity);
		}
	}

}
 
}
