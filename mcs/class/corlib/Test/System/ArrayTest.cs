// ArrayTest.cs - NUnit Test Cases for the System.Array class
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Collections;
using System.Globalization;

namespace MonoTests.System
{

public class ArrayTest : TestCase
{
	public ArrayTest() : base ("MonoTests.System.ArrayTest testsuite") {}
	public ArrayTest(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(ArrayTest)); 
		}
	}
    
	public void TestIsFixedSize() {
		char[] a1 = {'a'};
		Assert("All arrays are fixed", a1.IsFixedSize);
	}

	public void TestIsReadOnly() {
		char[] a1 = {'a'};
		Assert("No array is readonly", !a1.IsReadOnly);
	}

	public void TestIsSynchronized() {
		char[] a1 = {'a'};
		Assert("No array is synchronized", !a1.IsSynchronized);
	}

	public void TestLength() {
		{
			char[] a1 = { };
			AssertEquals("Zero length array", 0, a1.Length);
		}
		{
			char[] a1 = {'c'};
			AssertEquals("One-length array", 1, a1.Length);
		}
		{
			char[] a1 = {'c', 'c'};
			AssertEquals("Two-length array", 2, a1.Length);
		}
	}

	public void TestRank() {
		char[] a1 = { 'c', 'd', 'e' };
		AssertEquals("Rank one", 1, a1.Rank);

		char[,] a2 = new Char[3,3];
		AssertEquals("Rank two", 2, a2.Rank);

		char[,,] a3 = new Char[3,3,3];
		AssertEquals("Rank three", 3, a3.Rank);
	}

	public void TestBinarySearch1() {
		bool errorThrown = false;
		try {
			Array.BinarySearch(null, "blue");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);

		{
			char[] bad = {'d', 'a', 'd', 'a', 'c', 'a'};
			AssertEquals("shouldn't find elem in badly-sorted array", -1, Array.BinarySearch(bad, 'c'));
		}
		{
			char[] bad = {'a', 'd', 'a', 'd', 'a', 'c', 'a'};
			AssertEquals("shouldn't find elem in badly-sorted array", -2, Array.BinarySearch(bad, 'c'));
		}
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert("couldn't find elem", 
			       Array.BinarySearch(arr, 'c') >= 3);
			Assert("couldn't find elem", 
			       Array.BinarySearch(arr, 'c') < 6);
		}
		{
			char[] arr = {'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			AssertEquals("couldn't find next-higher elem", 
				     -4, Array.BinarySearch(arr, 'c'));
		}
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			AssertEquals("couldn't find end", 
				     -9, Array.BinarySearch(arr, 'e'));
		}
	}
	public void TestBinarySearch2() {
		bool errorThrown = false;
		try {
			Array.BinarySearch(null, 0, 1, "blue");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, 0, 1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, -1, 1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, -1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, 4, 'a');
		} catch (ArgumentException) {
			errorThrown = true;
		}
		Assert("Error not thrown", errorThrown);

		// FIXME - see commented-out tests in TestBinarySearch1, above
		
		{
			char[] bad = {'z', 'z', 'd', 'a', 'd', 'a', 'c', 'a'};
			AssertEquals("shouldn't find elem in badly-sorted array", -3, Array.BinarySearch(bad, 2, 6, 'c'));
		}
		{
			char[] bad = {'z', 'z', 'a', 'd', 'a', 'd', 'a', 'c', 'a'};
			AssertEquals("shouldn't find elem in badly-sorted array", -4, Array.BinarySearch(bad, 2, 7, 'c'));
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert("couldn't find elem", 
			       Array.BinarySearch(arr, 2, 8, 'c') >= 5);
			Assert("couldn't find elem", 
			       Array.BinarySearch(arr, 2, 8, 'c') < 8);
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			AssertEquals("couldn't find next-higher elem", 
				     -6, Array.BinarySearch(arr, 2, 8, 'c'));
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			AssertEquals("couldn't find end", 
				     -11, Array.BinarySearch(arr, 2, 8, 'e'));
		}
	}

	// TODO - testBinarySearch with explicit IComparer args

	public void TestClear() {
		bool errorThrown = false;
		try {
			Array.Clear(null, 0, 1);
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("error not thrown", errorThrown);

		int[] i1 = { 1, 2, 3, 4 };
		{
			int[] compare = {1,2,3,4};
			AssertEquals("array match", compare[0], i1[0]);
			AssertEquals("array match", compare[1], i1[1]);
			AssertEquals("array match", compare[2], i1[2]);
			AssertEquals("array match", compare[3], i1[3]);
		}
		Array.Clear(i1, 3, 1);
		{
			int[] compare = {1,2,3,0};
			AssertEquals("array match", compare[0], i1[0]);
			AssertEquals("array match", compare[1], i1[1]);
			AssertEquals("array match", compare[2], i1[2]);
			AssertEquals("array match", compare[3], i1[3]);
		}
		Array.Clear(i1, 1, 1);
		{
			int[] compare = {1,0,3,0};
			AssertEquals("array match", compare[0], i1[0]);
			AssertEquals("array match", compare[1], i1[1]);
			AssertEquals("array match", compare[2], i1[2]);
			AssertEquals("array match", compare[3], i1[3]);
		}
		Array.Clear(i1, 1, 3);
		{
			int[] compare = {1,0,0,0};
			AssertEquals("array match", compare[0], i1[0]);
			AssertEquals("array match", compare[1], i1[1]);
			AssertEquals("array match", compare[2], i1[2]);
			AssertEquals("array match", compare[3], i1[3]);
		}

		string[] s1 = { "red", "green", "blue" };
		Array.Clear(s1, 0, 3);
		{
			string[] compare = {null, null, null};
			AssertEquals("array match", compare[0], s1[0]);
			AssertEquals("array match", compare[1], s1[1]);
			AssertEquals("array match", compare[2], s1[2]);
		}
	}

	public void TestClone() {
		char[] c1 = {'a', 'b', 'c'};
		char[] c2 = (char[])c1.Clone();
		AssertEquals("Array match", c1[0], c2[0]);
		AssertEquals("Array match", c1[1], c2[1]);
		AssertEquals("Array match", c1[2], c2[2]);

		char[] d10 = {'a', 'b'};
		char[] d11 = {'a', 'c'};
		char[] d12 = {'b', 'c'};
		char[][] d1 = {d10, d11, d12};
		char[][] d2 = (char[][])d1.Clone();
		AssertEquals("Array match", d1[0], d2[0]);
		AssertEquals("Array match", d1[1], d2[1]);
		AssertEquals("Array match", d1[2], d2[2]);

		d1[0][0] = 'z';
		AssertEquals("shallow copy", d1[0], d2[0]);
	}

	public void TestCopy() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = {};
				Array.Copy(c1, null, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = {};
				Array.Copy(null, c1, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				Char[,] c2 = new Char[1,1];
				Array.Copy(c1, c2, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				string[] s1 = new String[1];
				Array.Copy(c1, s1, 1);
			} catch (ArrayTypeMismatchException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				Object[] o1 = new Object[1];
				o1[0] = "hello";
				Array.Copy(o1, c1, 1);
			} catch (InvalidCastException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				Char[] c2 = new Char[1];
				Array.Copy(c1, c2, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				Char[] c2 = new Char[2];
				Array.Copy(c1, c2, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[1];
				Char[] c2 = new Char[2];
				Array.Copy(c2, c1, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, copy, 4);
		for (int i = 0; i < orig.Length; i++) {
			AssertEquals("copy unsuccessful " + i,
				     orig[i], copy[i]);
		}
		Array.Clear(copy, 0, copy.Length);
		for (int i = 0; i < orig.Length; i++) {
			AssertEquals("clear unsuccessful " + i,
				     (char)0, copy[i]);
		}
		Array.Copy(orig, copy, 2);
		AssertEquals("copy unsuccessful 1", orig[0], copy[0]);
		AssertEquals("copy unsuccessful 2", orig[1], copy[1]);
		Assert("copy unsuccessful 3", orig[2] != copy[2]);
		Assert("copy unsuccessful 4", orig[3] != copy[3]);
	}
	public void TestCopy2() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[] c2 = new Char[2];
				Array.Copy(c2, 1, c1, 0, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[] c2 = new Char[2];
				Array.Copy(c2, 0, c1, 1, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		
		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, 1, copy, 1, 3);
		Assert("copy unsuccessful", copy[0] != orig[0]);
		for (int i = 1; i < orig.Length; i++) {
			AssertEquals("copy unsuccessful " + i,
				     orig[i], copy[i]);
		}
		Array.Clear(copy, 0, copy.Length);
		Array.Copy(orig, 1, copy, 0, 2);
		AssertEquals("copy unsuccessful", orig[1], copy[0]);
		AssertEquals("copy unsuccessful", orig[2], copy[1]);
		Assert("copy unsuccessful", copy[2] != orig[2]);
		Assert("copy unsuccessful", copy[3] != orig[3]);
	}

	public void TestCopyTo() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				c1.CopyTo(null, 2);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[,] c2 = new Char[2,2];
				c1.CopyTo(c2, 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[,] c1 = new Char[2,2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, 2);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, 3);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, 1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				String[] c1 = new String[2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, 0);
			} catch (ArrayTypeMismatchException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		Char[] orig = {'a', 'b', 'c', 'd'};
		Char[] copy = new Char[10];
		Array.Clear(copy, 0, copy.Length);
		orig.CopyTo(copy, 3);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[0]);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[1]);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[2]);
		AssertEquals("Wrong CopyTo 0", orig[0], copy[3]);
		AssertEquals("Wrong CopyTo 0", orig[1], copy[4]);
		AssertEquals("Wrong CopyTo 0", orig[2], copy[5]);
		AssertEquals("Wrong CopyTo 0", orig[3], copy[6]);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[7]);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[8]);
		AssertEquals("Wrong CopyTo 0", (char)0, copy[9]);
	}

	public void TestCreateInstance() {
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(null, 12);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), -3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c1 = (char[])Array.CreateInstance(Type.GetType("System.Char"), 12);
		AssertEquals("Array wrong size", 12, c1.Length);
	}

	public void TestGetEnumerator() {
		String[] s1 = {"this", "is", "a", "test"};
		IEnumerator en = s1.GetEnumerator();
		AssertNotNull("No enumerator", en);

		for (int i = 0; i < s1.Length; i++) {
			en.MoveNext();
			AssertEquals("Not enumerating", s1[i], en.Current);
		}
	}

	public void TestGetLength() {
		{
			bool errorThrown = false;
			try {
				char[] c1 = {'a', 'b', 'c'};
				c1.GetLength(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = {'a', 'b', 'c'};
				c1.GetLength(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c2 = new Char[5];
		AssertEquals("wrong single dimension length", 
			     5, c2.GetLength(0));

		char[,] c3 = new Char[6,7];
		AssertEquals("wrong single dimension length", 
			     6, c3.GetLength(0));
		AssertEquals("wrong single dimension length", 
			     7, c3.GetLength(1));
	}

	public void TestGetLowerBound() {
		// I have no idea what the point of this function is.
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetLowerBound(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetLowerBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c1 = new Char[5];
		AssertEquals("single-dimensional lower bound", 
			     0, c1.GetLowerBound(0));

		char[,] c2 = new Char[4,4];
		AssertEquals("multiple-dimensional lower bound", 
			     0, c2.GetLowerBound(0));
		AssertEquals("multiple-dimensional lower bound", 
			     0, c2.GetLowerBound(1));
	}

	public void TestGetUpperBound() {
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetUpperBound(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetUpperBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c1 = new Char[5];
		AssertEquals("single-dimensional lower bound", 
			     4, c1.GetUpperBound(0));

		char[,] c2 = new Char[4,6];
		AssertEquals("multiple-dimensional lower bound", 
			     3, c2.GetUpperBound(0));
		AssertEquals("multiple-dimensional lower bound", 
			     5, c2.GetUpperBound(1));
	}

	public void TestGetValue1() {
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		for (int i = 0; i < c1.Length; i++) {
			AssertEquals("Bad GetValue", c1[i], c1.GetValue(i));
		}
	}
	public void TestGetValue2() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue(1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(-1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,] c1 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				AssertEquals("Bad GetValue", 
					     c1[i,j], c1.GetValue(i, j));
			}
		}
	}
	public void TestGetValue3() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue(1,1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(-1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,,] c1 = new Char[4,2,3];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int remains = i % 6;
			int second = remains / 3;
			int third = remains % 3;
			c1[first,second, third] = (char)(((int)'a')+i);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				for (int k = 0; k < c1.GetLength(2); k++) {
				AssertEquals("Bad GetValue", 
					     c1[i,j,k], c1.GetValue(i,j,k));
				}
			}
		}
	}
	public void TestGetValueN() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				int[] coords = {1, 1};
				c.GetValue(coords);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				int[] coords = {-1, 1};
				c.GetValue(coords);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				int[] coords = {4, 1};
				c.GetValue(coords);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,] c1 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				int[] coords = {i, j};
				AssertEquals("Bad GetValue", 
					     c1[i,j], c1.GetValue(coords));
			}
		}
	}

	public void TestIndexOf1() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?");
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "a", "test"};
		AssertEquals("No null here", -1, Array.IndexOf(s1, null));
		AssertEquals("No nothing here", -1, Array.IndexOf(s1, "nothing"));
		AssertEquals("Found first", 0, Array.IndexOf(s1, "this"));
		AssertEquals("Found last", 3, Array.IndexOf(s1, "test"));
	}
	public void TestIndexOf2() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?", 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("No null here", -1, Array.IndexOf(s1, null, 1));
		AssertEquals("No nothing here", -1, Array.IndexOf(s1, "nothing", 1));
		AssertEquals("Didn't find first", -1, Array.IndexOf(s1, "this", 1));
		AssertEquals("Found first", 1, Array.IndexOf(s1, "is", 1));
		AssertEquals("Found last", 4, Array.IndexOf(s1, "test", 1));
	}
	public void TestIndexOf3() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?", 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("No null here", -1, Array.IndexOf(s1, null, 1, 3));
		AssertEquals("No nothing here", -1, Array.IndexOf(s1, "nothing", 1, 3));
		AssertEquals("Didn't find first", -1, Array.IndexOf(s1, "this", 1, 3));
		AssertEquals("Found first", 1, Array.IndexOf(s1, "is", 1, 3));
		AssertEquals("Didn't find last", -1, Array.IndexOf(s1, "test", 1, 3));
		AssertEquals("Found last", 3, Array.IndexOf(s1, "a", 1, 3));
	}
	
	public void TestLastIndexOf1() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?");
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "a", "a", "test"};
		AssertEquals("No null here", -1, Array.LastIndexOf(s1, null));
		AssertEquals("No nothing here", -1, Array.LastIndexOf(s1, "nothing"));
		AssertEquals("Found first", 0, Array.LastIndexOf(s1, "this"));
		AssertEquals("Found last", 4, Array.LastIndexOf(s1, "test"));
		AssertEquals("Found repeat", 3, Array.LastIndexOf(s1, "a"));
	}
	public void TestLastIndexOf2() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?", 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("No null here", -1, Array.LastIndexOf(s1, null, 3));
		AssertEquals("No nothing here", -1, Array.LastIndexOf(s1, "nothing", 3));
		AssertEquals("Didn't find larst", -1, Array.LastIndexOf(s1, "test", 3));
		AssertEquals("Found last", 3, Array.LastIndexOf(s1, "a", 3));
		AssertEquals("Found first", 0, Array.LastIndexOf(s1, "this", 3));
	}
	public void TestLastIndexOf3() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?", 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("No null here", 
			     -1, Array.LastIndexOf(s1, null, 3, 3));
		AssertEquals("No nothing here", 
			     -1, Array.LastIndexOf(s1, "nothing", 3, 3));
		AssertEquals("Didn't find first", 
			     -1, Array.LastIndexOf(s1, "this", 3, 3));
		AssertEquals("Found first", 
			     1, Array.LastIndexOf(s1, "is", 3, 3));
		AssertEquals("Didn't find last", 
			     -1, Array.LastIndexOf(s1, "test", 3, 3));
		AssertEquals("Found last", 
			     3, Array.LastIndexOf(s1, "a", 3, 3));
	}

	public void TestReverse() {
		{
			bool errorThrown = false;
			try {
				Array.Reverse(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		
		char[] c1 = {'a', 'b', 'c', 'd'};
		Array.Reverse(c1);
		AssertEquals("Reverse not working", 'd', c1[0]);
		AssertEquals("Reverse not working", 'c', c1[1]);
		AssertEquals("Reverse not working", 'b', c1[2]);
		AssertEquals("Reverse not working", 'a', c1[3]);

		{
			bool errorThrown = false;
			try {
				Array.Reverse(null, 0, 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c, 0, 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 0, 3);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert("error not thrown", errorThrown);
		//}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 3, 0);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert("error not thrown", errorThrown);
		//}

		char[] c2 = { 'a', 'b', 'c', 'd'};
		Array.Reverse(c2, 1, 2);
		AssertEquals("Reverse not working", 'a', c2[0]);
		AssertEquals("Reverse not working", 'c', c2[1]);
		AssertEquals("Reverse not working", 'b', c2[2]);
		AssertEquals("Reverse not working", 'd', c2[3]);
	}

	public void TestSetValue1() {
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", 1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", -1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", 4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		char[] c2 = new char[4];
		for (int i = 0; i < c1.Length; i++) {
			c2.SetValue(c1[i], i);
		}
		for (int i = 0; i < c1.Length; i++) {
			AssertEquals("Bad SetValue", c1[i], c2[i]);
		}
	}
	public void TestSetValue2() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", 1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", -1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", 4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,] c1 = new Char[4,6];
		char[,] c2 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
			c2.SetValue(c1[first,second], first, second);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				AssertEquals("Bad SetValue", 
					     c1[i,j], c2[i, j]);
			}
		}
	}
	public void TestSetValue3() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", 1,1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", -1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", 4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,,] c1 = new Char[4,2,3];
		char[,,] c2 = new Char[4,2,3];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int remains = i % 6;
			int second = remains / 3;
			int third = remains % 3;
			c1[first,second, third] = (char)(((int)'a')+i);
			c2.SetValue(c1[first, second, third], first, second, third);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				for (int k = 0; k < c1.GetLength(2); k++) {
				AssertEquals("Bad SetValue", 
					     c1[i,j,k], c2[i,j,k]);
				}
			}
		}
	}
	public void TestSetValueN() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				int[] coords = {1, 1};
				c.SetValue("buh", coords);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				int[] coords = {-1, 1};
				c.SetValue("buh", coords);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				int[] coords = {4, 1};
				c.SetValue("buh", coords);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("error not thrown", errorThrown);
		}

		char[,] c1 = new Char[4,6];
		char[,] c2 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
			int[] coords = {first, second};
			c2.SetValue(c1[first,second], coords);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				AssertEquals("Bad SetValue", 
					     c1[i,j], c2[i,j]);
			}
		}
	}

	public void TestSort() {
		{
			bool errorThrown = false;
			try {
				Array.Sort(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown 1", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Array.Sort(null, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown 2", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown 5", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("error not thrown 6", errorThrown);
		}

		// note: null second array => just sort first array
		char[] starter = {'d', 'b', 'f', 'e', 'a', 'c'};
		int[] starter1 = {1,2,3,4,5,6};
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1);
			AssertEquals("Basic sort problem", 'a', c1[0]);
			AssertEquals("Basic sort problem", 'b', c1[1]);
			AssertEquals("Basic sort problem", 'c', c1[2]);
			AssertEquals("Basic sort problem", 'd', c1[3]);
			AssertEquals("Basic sort problem", 'e', c1[4]);
			AssertEquals("Basic sort problem", 'f', c1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1);
			AssertEquals("Keyed sort problem", 'a', c1[0]);
			AssertEquals("Keyed sort problem", 'b', c1[1]);
			AssertEquals("Keyed sort problem", 'c', c1[2]);
			AssertEquals("Keyed sort problem", 'd', c1[3]);
			AssertEquals("Keyed sort problem", 'e', c1[4]);
			AssertEquals("Keyed sort problem", 'f', c1[5]);
			AssertEquals("Keyed sort problem", 5, i1[0]);
			AssertEquals("Keyed sort problem", 2, i1[1]);
			AssertEquals("Keyed sort problem", 6, i1[2]);
			AssertEquals("Keyed sort problem", 1, i1[3]);
			AssertEquals("Keyed sort problem", 4, i1[4]);
			AssertEquals("Keyed sort problem", 3, i1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1, 1, 4);
			AssertEquals("Basic sort chunk problem", 'd', c1[0]);
			AssertEquals("Basic sort chunk problem", 'a', c1[1]);
			AssertEquals("Basic sort chunk problem", 'b', c1[2]);
			AssertEquals("Basic sort chunk problem", 'e', c1[3]);
			AssertEquals("Basic sort chunk problem", 'f', c1[4]);
			AssertEquals("Basic sort chunk problem", 'c', c1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1, 1, 4);
			AssertEquals("Keyed sort chunk problem", 'd', c1[0]);
			AssertEquals("Keyed sort chunk problem", 'a', c1[1]);
			AssertEquals("Keyed sort chunk problem", 'b', c1[2]);
			AssertEquals("Keyed sort chunk problem", 'e', c1[3]);
			AssertEquals("Keyed sort chunk problem", 'f', c1[4]);
			AssertEquals("Keyed sort chunk problem", 'c', c1[5]);
			AssertEquals("Keyed sort chunk problem", 1, i1[0]);
			AssertEquals("Keyed sort chunk problem", 5, i1[1]);
			AssertEquals("Keyed sort chunk problem", 2, i1[2]);
			AssertEquals("Keyed sort chunk problem", 4, i1[3]);
			AssertEquals("Keyed sort chunk problem", 3, i1[4]);
			AssertEquals("Keyed sort chunk problem", 6, i1[5]);
		}
	}

	// TODO - TestSort passed-in IComparable versions

}

}
