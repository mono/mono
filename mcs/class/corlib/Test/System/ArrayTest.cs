// ArrayTest.cs - NUnit Test Cases for the System.Array class
//
// David Brandt (bucky@keystreams.com)
// Eduardo Garcia (kiwnix@yahoo.es)
// 
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.Collections;
using System.Globalization;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace MonoTests.System
{
	//Auxiliary Things
	enum enua  {hola,adios,mas,menos};

	class AClass
	{
		public AClass()
		{

		}
	}

	class BClass : AClass
	{
	}

	class CClass : AClass
	{
	}

	struct AStruct
	{
		public string s;
		public string a;
	}
	
	class DataEqual
	{
		public override bool Equals (object obj)
		{
			return true;
		}
	}
		
	//End Auxiliary Things

[TestFixture]
public class ArrayTest : Assertion
{
	char [] arrsort = {'d', 'b', 'f', 'e', 'a', 'c'};

	public ArrayTest() {}

	[Test]
	public void TestIsFixedSize() {
		char[] a1 = {'a'};
		Assert("All arrays are fixed", a1.IsFixedSize);
	}
	
	[Test]
	public void TestIsReadOnly() {
		char[] a1 = {'a'};
		Assert("No array is readonly", !a1.IsReadOnly);
	}

	[Test]
	public void TestIsSynchronized() {
		char[] a1 = {'a'};
		Assert("No array is synchronized", !a1.IsSynchronized);
	}

	[Test]
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

	[Test]
	public void TestRank() {
		char[] a1 = { 'c', 'd', 'e' };
		AssertEquals("Rank one", 1, a1.Rank);

		char[,] a2 = new Char[3,3];
		AssertEquals("Rank two", 2, a2.Rank);

		char[,,] a3 = new Char[3,3,3];
		AssertEquals("Rank three", 3, a3.Rank);
	}

	[Test]
	public void TestBinarySearch1() {
		bool errorThrown = false;
		try {
			Array.BinarySearch(null, "blue");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("#B01", errorThrown);
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert("#B02", errorThrown);

		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert("#B05", Array.BinarySearch(arr, 'c') >= 3);
			Assert("#B06", Array.BinarySearch(arr, 'c') < 6);
		}
		{
			char[] arr = {'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			AssertEquals("#B07", -4, Array.BinarySearch(arr, 'c'));
		}
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			AssertEquals("#B08", -9, Array.BinarySearch(arr, 'e'));
		}
	}

	[Test]
	public void TestBinarySearch2() {
		bool errorThrown = false;
		try {
			Array.BinarySearch(null, 0, 1, "blue");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("#B20", errorThrown);
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, 0, 1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert("#B21", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, -1, 1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert("#B22", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, -1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert("#B23", errorThrown);
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, 4, 'a');
		} catch (ArgumentException) {
			errorThrown = true;
		}
		Assert("#B24", errorThrown);

		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert("#B26", Array.BinarySearch(arr, 2, 8, 'c') >= 5);
			Assert("#B27", Array.BinarySearch(arr, 2, 8, 'c') < 8);
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			AssertEquals("#B28", -6, Array.BinarySearch(arr, 2, 8, 'c'));
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			AssertEquals("#B29", -11, Array.BinarySearch(arr, 2, 8, 'e'));
		}
	}

	public void TestBinarySearch3()
	{
		int[] array = new int[100];

		for (int i = 0; i < 100; i++)
			array[i] = 10;

		AssertEquals("#B30", 49, Array.BinarySearch(array, 10));
	}

	[Test]
	public void BinarySearch_NullValue () 
	{
		int[] array = new int[1];
		AssertEquals ("I=a,o", -1, Array.BinarySearch (array, null));
		AssertEquals ("I=a,o,c", -1, Array.BinarySearch (array, null, null));
		AssertEquals ("I=a,i,i,o", -1, Array.BinarySearch (array, 0, 1, null));
		AssertEquals ("I=a,i,i,o,c", -1, Array.BinarySearch (array, 0, 1, null,null));

		object[] o = new object [3] { this, this, null };
		AssertEquals ("O=a,o", -1, Array.BinarySearch (o, null));
		AssertEquals ("O=a,o,c", -1, Array.BinarySearch (o, null, null));
		AssertEquals ("O=a,i,i,o", -1, Array.BinarySearch (o, 0, 3, null));
		AssertEquals ("O=a,i,i,o,c", -1, Array.BinarySearch (o, 0, 3, null, null));
	}

	// TODO - testBinarySearch with explicit IComparer args

	[Test]
	public void TestClear() {
		bool errorThrown = false;
		try {
			Array.Clear(null, 0, 1);
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("#C01", errorThrown);

		int[] i1 = { 1, 2, 3, 4 };
		{
			int[] compare = {1,2,3,4};
			AssertEquals("#C02", compare[0], i1[0]);
			AssertEquals("#C03", compare[1], i1[1]);
			AssertEquals("#C04", compare[2], i1[2]);
			AssertEquals("#C05", compare[3], i1[3]);
		}
		Array.Clear(i1, 3, 1);
		{
			int[] compare = {1,2,3,0};
			AssertEquals("#C06", compare[0], i1[0]);
			AssertEquals("#C07", compare[1], i1[1]);
			AssertEquals("#C08", compare[2], i1[2]);
			AssertEquals("#C09", compare[3], i1[3]);
		}
		Array.Clear(i1, 1, 1);
		{
			int[] compare = {1,0,3,0};
			AssertEquals("#C10", compare[0], i1[0]);
			AssertEquals("#C11", compare[1], i1[1]);
			AssertEquals("#C12", compare[2], i1[2]);
			AssertEquals("#C13", compare[3], i1[3]);
		}
		Array.Clear(i1, 1, 3);
		{
			int[] compare = {1,0,0,0};
			AssertEquals("#C14", compare[0], i1[0]);
			AssertEquals("#C15", compare[1], i1[1]);
			AssertEquals("#C16", compare[2], i1[2]);
			AssertEquals("#C17", compare[3], i1[3]);
		}

		string[] s1 = { "red", "green", "blue" };
		Array.Clear(s1, 0, 3);
		{
			string[] compare = {null, null, null};
			AssertEquals("#C18", compare[0], s1[0]);
			AssertEquals("#C19", compare[1], s1[1]);
			AssertEquals("#C20", compare[2], s1[2]);
		}
	}

	[Test]
	public void TestClone() {
		char[] c1 = {'a', 'b', 'c'};
		char[] c2 = (char[])c1.Clone();
		AssertEquals("#D01", c1[0], c2[0]);
		AssertEquals("#D02", c1[1], c2[1]);
		AssertEquals("#D03", c1[2], c2[2]);

		char[] d10 = {'a', 'b'};
		char[] d11 = {'a', 'c'};
		char[] d12 = {'b', 'c'};
		char[][] d1 = {d10, d11, d12};
		char[][] d2 = (char[][])d1.Clone();
		AssertEquals("#D04", d1[0], d2[0]);
		AssertEquals("#D05", d1[1], d2[1]);
		AssertEquals("#D06", d1[2], d2[2]);

		d1[0][0] = 'z';
		AssertEquals("#D07", d1[0], d2[0]);
	}

	[Test] public void TestIndexer ()
	{
		int [] a = new int [10];
		IList b = a;
		try {
			object c = b [-1];
			Fail ("IList.this [-1] should throw");
		} catch (IndexOutOfRangeException) {
			// Good
		} catch (Exception){
			Fail ("Should have thrown an IndexOutOfRangeException");
		}
	}
		
	[Test]
	public void TestCopy() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = {};
				Array.Copy(c1, null, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#E01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = {};
				Array.Copy(null, c1, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#E02", errorThrown);
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
			Assert("#E03", errorThrown);
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
			Assert("#E04", errorThrown);
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
			Assert("#E05", errorThrown);
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
			Assert("#E06", errorThrown);
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
			Assert("#E07", errorThrown);
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
			Assert("#E08", errorThrown);
		}

		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, copy, 4);
		for (int i = 0; i < orig.Length; i++) {
			AssertEquals("#E09(" + i + ")", orig[i], copy[i]);
		}
		Array.Clear(copy, 0, copy.Length);
		for (int i = 0; i < orig.Length; i++) {
			AssertEquals("#E10(" + i + ")", (char)0, copy[i]);
		}
		Array.Copy(orig, copy, 2);
		AssertEquals("#E11", orig[0], copy[0]);
		AssertEquals("#E12", orig[1], copy[1]);
		Assert("#E13", orig[2] != copy[2]);
		Assert("#E14", orig[3] != copy[3]);
	}

	[Test]
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
			Assert("#E31", errorThrown);
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
			Assert("#E32", errorThrown);
		}
		
		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, 1, copy, 1, 3);
		Assert("#E33", copy[0] != orig[0]);
		for (int i = 1; i < orig.Length; i++) {
			AssertEquals("#E34(" + i + ")", orig[i], copy[i]);
		}
		Array.Clear(copy, 0, copy.Length);
		Array.Copy(orig, 1, copy, 0, 2);
		AssertEquals("#E35", orig[1], copy[0]);
		AssertEquals("#E36", orig[2], copy[1]);
		Assert("#E37", copy[2] != orig[2]);
		Assert("#E38", copy[3] != orig[3]);
	}

	[Test]
	[ExpectedException (typeof (InvalidCastException))]
	public void Copy_InvalidCast () {
		object[] arr1 = new object [10];
		Type[] arr2 = new Type [10];

		arr1 [0] = new object ();

		Array.Copy (arr1, 0, arr2, 0, 10);
	}

	[Test]
	public void TestCopyTo() {
		{
			bool errorThrown = false;
			try {
				Char[] c1 = new Char[2];
				c1.CopyTo(null, 2);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#E61", errorThrown);
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
#if TARGET_JVM // This is really implementation dependent behaviour.
			catch (RankException) {
				errorThrown = true;
			}
#endif // TARGET_JVM
			Assert("#E62", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Char[,] c1 = new Char[2,2];
				Char[] c2 = new Char[2];
				c1.CopyTo(c2, -1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#E63", errorThrown);
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
			Assert("#E64", errorThrown);
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
			Assert("#E65", errorThrown);
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
			Assert("#E66", errorThrown);
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
			Assert("#E67", errorThrown);
		}

		{
			bool errorThrown = false;
			try {
				String[] c1 = new String[2];
				// TODO: this crashes mono if there are null
				// values in the array.
				c1[1] = "hey";
				c1[0] = "you";
				Char[] c2 = new Char[2];
				c2[1] = 'a';
				c2[0] = 'z';
				c1.CopyTo(c2, 0);
			} catch (ArrayTypeMismatchException) {
				errorThrown = true;
			}
			Assert("#E68", errorThrown);
		}

		Char[] orig = {'a', 'b', 'c', 'd'};
		Char[] copy = new Char[10];
		Array.Clear(copy, 0, copy.Length);
		orig.CopyTo(copy, 3);
		AssertEquals("#E69", (char)0, copy[0]);
		AssertEquals("#E70", (char)0, copy[1]);
		AssertEquals("#E71", (char)0, copy[2]);
		AssertEquals("#E72", orig[0], copy[3]);
		AssertEquals("#E73", orig[1], copy[4]);
		AssertEquals("#E74", orig[2], copy[5]);
		AssertEquals("#E75", orig[3], copy[6]);
		AssertEquals("#E76", (char)0, copy[7]);
		AssertEquals("#E77", (char)0, copy[8]);
		AssertEquals("#E78", (char)0, copy[9]);

		{
			// The following is valid and must not throw an exception.
			bool errorThrown = false;
			try {
				int[] src = new int [0];
				int[] dest = new int [0];
				src.CopyTo (dest, 0);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#E79", !errorThrown);
		}

		{
			// bug #38812
			bool errorThrown = false;
			try {
				CClass[] src = new CClass [] { new CClass () };
				BClass[] dest = new BClass [1];

				src.CopyTo (dest, 0);

			} catch (ArrayTypeMismatchException) {
				errorThrown = true;
			}
			Assert("#E80", errorThrown);
		}
	}

	[Test]
	public void TestCreateInstance() {
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(null, 12);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#F01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), -3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#F02", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), (int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#F03a", errorThrown);
		}
#if NET_1_1
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), (int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#F03b", errorThrown);
		}
#endif
#if !TARGET_JVM // Arrays lower bounds are not supported for TARGET_JVM
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), null, null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#F04", errorThrown);
		}
#endif // TARGET_JVM
		{
			bool errorThrown = false;
			try {
				int[] lengths = new int [0];
				Array.CreateInstance(Type.GetType("System.Char"), lengths);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#F05", errorThrown);
		}
#if !TARGET_JVM // CreateInstance with lower bounds not supported for TARGET_JVM
		{
			bool errorThrown = false;
			try {
				int[] lengths = new int [1];
				int[] bounds = new int [2];
				Array.CreateInstance(Type.GetType("System.Char"), lengths, bounds);
				errorThrown = true;
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#F06", errorThrown);
		}

		char[] c1 = (char[])Array.CreateInstance(Type.GetType("System.Char"), 12);
		AssertEquals("#F07", 12, c1.Length);

		Array c2 = Array.CreateInstance(Type.GetType("System.Char"), 12, 5);
		AssertEquals("#F08", 2, c2.Rank);
		AssertEquals("#F09", 60, c2.Length);


		{
			int[] lengths = { 3 };
			int[] bounds = { 5 };
			int[] src = { 512, 718, 912 };
			Array array = Array.CreateInstance(typeof(int), lengths, bounds);

			AssertEquals("#F10", 3, array.Length);
			AssertEquals("#F11", 5, array.GetLowerBound(0));
			AssertEquals("#F12", 7, array.GetUpperBound(0));

			src.CopyTo (array, 5);

			for (int i = 0; i < src.Length; i++)
				AssertEquals("#F13(" + i + ")", src[i], array.GetValue(i+5));
		}

		// Test that a 1 dimensional array with 0 lower bound is the
		// same as an szarray
		Type szarrayType = new int [10].GetType ();
		Assert (szarrayType == (Array.CreateInstance (typeof (int), new int[] {1}, new int[] {0})).GetType ());
		Assert (szarrayType != (Array.CreateInstance (typeof (int), new int[] {1}, new int[] {1})).GetType ());
#endif // TARGET_JVM
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestCreateInstance2 ()
	{
		Array.CreateInstance (typeof (Int32), (int[])null);
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#else
	[ExpectedException (typeof (NullReferenceException))]
#endif
	public void TestCreateInstance2b ()
	{
		Array.CreateInstance (typeof (Int32), (long[])null);
	}

	[Test]
	public void TestGetEnumerator() {
		String[] s1 = {"this", "is", "a", "test"};
		IEnumerator en = s1.GetEnumerator ();
		AssertNotNull ("#G01", en);

		Assert ("#G02", en.MoveNext ());
		AssertEquals ("#G03", "this", en.Current);
		Assert ("#G04", en.MoveNext ());
		AssertEquals ("#G05", "is", en.Current);
		Assert ("#G06", en.MoveNext ());
		AssertEquals ("#G07", "a", en.Current);
		Assert ("#G08", en.MoveNext ());
		AssertEquals ("#G09", "test", en.Current);
		Assert ("#G10", !en.MoveNext ());

		en.Reset ();
		Assert("#G11", en.MoveNext ());
		AssertEquals ("#G12", "this", en.Current);

		// mutation does not invalidate array enumerator!
		s1.SetValue ("change", 1);
		Assert ("#G13", en.MoveNext ());
		AssertEquals ("#G14", "change", en.Current);
	}

	[Test]
	public void TestGetEnumeratorMultipleDimension() {
		String[,] s1 = {{"this", "is"}, {"a", "test"}};
		IEnumerator en = s1.GetEnumerator ();
		AssertNotNull ("#AA01", en);

		Assert ("#AA02", en.MoveNext ());
		AssertEquals ("#AA03", "this", en.Current);
		Assert ("#AA04", en.MoveNext ());
		AssertEquals ("#AA05", "is", en.Current);
		Assert ("#AA06", en.MoveNext ());
		AssertEquals ("#AA07", "a", en.Current);
		Assert ("#AA08", en.MoveNext ());
		AssertEquals ("#AA09", "test", en.Current);
		Assert ("#AA10", !en.MoveNext ());

		en.Reset ();
		Assert("#AA11", en.MoveNext ());
		AssertEquals ("#AA12", "this", en.Current);

		int[] idxs = {0,1};
		// mutation does not invalidate array enumerator!
		s1.SetValue ("change", idxs);
		Assert ("#AA13", en.MoveNext ());
		AssertEquals ("#AA14", "change", en.Current);
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestGetEnumeratorNonZeroLowerBounds() {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance( typeof(String), myLengthsArray, myBoundsArray );
		for ( int i = myArray.GetLowerBound(0); i <= myArray.GetUpperBound(0); i++ )
			for ( int j = myArray.GetLowerBound(1); j <= myArray.GetUpperBound(1); j++ )  {
				int[] myIndicesArray = new int[2] { i, j };
				myArray.SetValue( Convert.ToString(i) + j, myIndicesArray );
			}
		IEnumerator en = myArray.GetEnumerator ();
		AssertNotNull ("#AB01", en);

		// check the first couple of values
		Assert ("#AB02", en.MoveNext ());
		AssertEquals ("#AB03", "23", en.Current);
		Assert ("#AB04", en.MoveNext ());
		AssertEquals ("#AB05", "24", en.Current);

		// then check the last element's value
		string lastElement;
		do {  
			lastElement = (string)en.Current;
		} while (en.MoveNext());
		AssertEquals ("#AB06", "47", lastElement);
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Add () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Add ("can not");
			Fail ("IList.Add should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Fail ("IList.Add threw wrong exception type");
		}

		Fail("IList.Add shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Insert () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Insert (0, "can not");
			Fail ("IList.Insert should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Fail ("IList.Insert threw wrong exception type");
		}

		Fail("IList.Insert shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Remove () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Remove ("can not");
			Fail ("IList.Remove should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Fail ("IList.Remove threw wrong exception type");
		}

		Fail("IList.Remove shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_RemoveAt () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).RemoveAt (0);
			Fail ("IList.RemoveAt should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Fail ("IList.RemoveAt threw wrong exception type");
		}

		Fail("IList.RemoveAt shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Contains () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );

		try {
			bool b = ((IList)myArray).Contains ("23");
			Fail("IList.Contains should throw with multi-dimensional arrays");
		}
		catch (RankException) {
			int[] iArr = new int[3] { 1, 2, 3};
			// check the first and last items
			Assert("AC01", ((IList)iArr).Contains (1));
			Assert("AC02", ((IList)iArr).Contains (3));

			// and one that is definately not there
			Assert("AC03", !((IList)iArr).Contains (42));
			return;
		}

		Fail("Should not get here");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_IndexOf () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );

		try {
			bool b = ((IList)myArray).Contains ("23");
			Fail("IList.Contains should throw with multi-dimensional arrays");
		}
		catch (RankException) {
			int[] iArr = new int[3] { 1, 2, 3};
			// check the first and last items
			AssertEquals("AD01", 0, ((IList)iArr).IndexOf (1));
			AssertEquals("AD02", 2, ((IList)iArr).IndexOf (3));

			// and one that is definately not there
			AssertEquals("AD03", -1, ((IList)iArr).IndexOf (42));
		}
		catch (Exception e) {
			Fail("Unexpected exception: " + e.ToString());
		}

		// check that wierd case whem lowerbound is Int32.MinValue,
		// so that IndexOf() needs to return Int32.MaxValue when it cannot find the object
		int[] myLengthArray = new int[1] { 3 };
		int[] myBoundArray = new int[1] { Int32.MinValue };
		Array myExtremeArray=Array.CreateInstance ( typeof(String), myLengthArray, myBoundArray );
		AssertEquals("AD04", Int32.MaxValue, ((IList)myExtremeArray).IndexOf (42));

	}

	[Test]
	public void TestGetLength() {
		{
			bool errorThrown = false;
			try {
				char[] c1 = {'a', 'b', 'c'};
				c1.GetLength(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = {'a', 'b', 'c'};
				c1.GetLength(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H02", errorThrown);
		}

		char[] c2 = new Char[5];
		AssertEquals("#H03", 5, c2.GetLength(0));

		char[,] c3 = new Char[6,7];
		AssertEquals("#H04", 6, c3.GetLength(0));
		AssertEquals("#H05", 7, c3.GetLength(1));
	}

	[Test]
	public void TestGetLowerBound() {
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetLowerBound(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H31", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetLowerBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H32", errorThrown);
		}

		char[] c1 = new Char[5];
		AssertEquals("#H33", 0, c1.GetLowerBound(0));

		char[,] c2 = new Char[4,4];
		AssertEquals("#H34", 0, c2.GetLowerBound(0));
		AssertEquals("#H35", 0, c2.GetLowerBound(1));
	}

	[Test]
	public void TestGetUpperBound() {
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetUpperBound(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H61", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetUpperBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#H62", errorThrown);
		}

		char[] c1 = new Char[5];
		AssertEquals("#H63", 4, c1.GetUpperBound(0));

		char[,] c2 = new Char[4,6];
		AssertEquals("#H64", 3, c2.GetUpperBound(0));
		AssertEquals("#H65", 5, c2.GetUpperBound(1));
	}

	[Test]
	public void TestGetValue1() {
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#I01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I02", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I03", errorThrown);
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		for (int i = 0; i < c1.Length; i++) {
			AssertEquals("#I04(" + i + ")", c1[i], c1.GetValue(i));
		}
	}

	[Test]
	public void TestGetValue2() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue(1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#I21", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(-1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I22", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I23", errorThrown);
		}

		char[,] c1 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				AssertEquals("#I24(" + i + "," + j + ")",
					c1[i,j], c1.GetValue(i, j));
			}
		}
	}

	[Test]
	public void TestGetValue3() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue(1,1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#I41", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(-1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I42", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#I43", errorThrown);
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
					AssertEquals("#I44(" + i + "," + j + ")",
						c1[i,j,k], c1.GetValue(i,j,k));
				}
			}
		}
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#else
	[ExpectedException (typeof (NullReferenceException))]
#endif
	public void TestGetValueLongArray ()
	{
		char[] c = new Char[2];
		c.GetValue((long [])null);
	}

	[Test]
	public void TestGetValueN() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.GetValue((int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#I61a", errorThrown);
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
			Assert("#I62", errorThrown);
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
			Assert("#I63", errorThrown);
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
			Assert("#I64", errorThrown);
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
				AssertEquals("#I65(" + i + "," + j + ")",
					c1[i,j], c1.GetValue(coords));
			}
		}
	}

	[Test]
	public void TestIndexOf1() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?");
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#J01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#J02", errorThrown);
		}

		String[] s1 = {"this", "is", "a", "test"};
		AssertEquals("#J03", -1, Array.IndexOf(s1, null));
		AssertEquals("#J04", -1, Array.IndexOf(s1, "nothing"));
		AssertEquals("#J05", 0, Array.IndexOf(s1, "this"));
		AssertEquals("#J06", 3, Array.IndexOf(s1, "test"));
	}

	[Test]
	public void TestIndexOf2() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?", 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#J21", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#J22", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#J23", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("#J24", -1, Array.IndexOf(s1, null, 1));
		AssertEquals("#J25", -1, Array.IndexOf(s1, "nothing", 1));
		AssertEquals("#J26", -1, Array.IndexOf(s1, "this", 1));
		AssertEquals("#J27", 1, Array.IndexOf(s1, "is", 1));
		AssertEquals("#J28", 4, Array.IndexOf(s1, "test", 1));
	}

	[Test]
	public void TestIndexOf3() {
		{
			bool errorThrown = false;
			try {
				Array.IndexOf(null, "huh?", 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#J41", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#J42", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#J43", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#J44", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("#J45", -1, Array.IndexOf(s1, null, 1, 3));
		AssertEquals("#J46", -1, Array.IndexOf(s1, "nothing", 1, 3));
		AssertEquals("#J47", -1, Array.IndexOf(s1, "this", 1, 3));
		AssertEquals("#J48", 1, Array.IndexOf(s1, "is", 1, 3));
		AssertEquals("#J49", -1, Array.IndexOf(s1, "test", 1, 3));
		AssertEquals("#J50", 3, Array.IndexOf(s1, "a", 1, 3));
	}
	
	[Test]
	public void TestIndexOf_CustomEqual ()
	{
		DataEqual[] test = new DataEqual [] { new DataEqual () };
		AssertEquals (0, Array.IndexOf (test, "asdfas", 0));
		
		IList array = (IList)test;
		AssertEquals (0, array.IndexOf ("asdfas"));
	}
	
	[Test]
	public void TestLastIndexOf1() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?");
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#K01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#K02", errorThrown);
		}

		String[] s1 = {"this", "is", "a", "a", "test"};
		AssertEquals("#K03", -1, Array.LastIndexOf(s1, null));
		AssertEquals("#K04", -1, Array.LastIndexOf(s1, "nothing"));
		AssertEquals("#K05", 0, Array.LastIndexOf(s1, "this"));
		AssertEquals("#K06", 4, Array.LastIndexOf(s1, "test"));
		AssertEquals("#K07", 3, Array.LastIndexOf(s1, "a"));

		AssertEquals (-1, Array.LastIndexOf (new String [0], "foo"));
	}

	[Test]
	public void TestLastIndexOf2() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?", 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#K21", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#K22", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#K23", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("#K24", -1, Array.LastIndexOf(s1, null, 3));
		AssertEquals("#K25", -1, Array.LastIndexOf(s1, "nothing", 3));
		AssertEquals("#K26", -1, Array.LastIndexOf(s1, "test", 3));
		AssertEquals("#K27", 3, Array.LastIndexOf(s1, "a", 3));
		AssertEquals("#K28", 0, Array.LastIndexOf(s1, "this", 3));
	}

	[Test]
	public void TestLastIndexOf3() {
		{
			bool errorThrown = false;
			try {
				Array.LastIndexOf(null, "huh?", 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#K41", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#K42", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#K43", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#K44", errorThrown);
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		AssertEquals("#K45", -1, Array.LastIndexOf(s1, null, 3, 3));
		AssertEquals("#K46", -1, Array.LastIndexOf(s1, "nothing", 3, 3));
		AssertEquals("#K47", -1, Array.LastIndexOf(s1, "this", 3, 3));
		AssertEquals("#K48", 1, Array.LastIndexOf(s1, "is", 3, 3));
		AssertEquals("#K49", -1, Array.LastIndexOf(s1, "test", 3, 3));
		AssertEquals("#K50", 3, Array.LastIndexOf(s1, "a", 3, 3));
	}

	[Test]
	public void TestLastIndexOf4 ()
	{
		short [] a = new short [] { 19, 238, 317, 6, 565, 0, -52, 60, -563, 753, 238, 238};
		try {
			Array.LastIndexOf (a, (object)16, -1);
			NUnit.Framework.Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException) { }
		
#if NET_2_0		
		try {
			Array.LastIndexOf<short> (a, 16, -1);
			NUnit.Framework.Assert.Fail ("#2");
		} catch (ArgumentOutOfRangeException) { }
#endif		
	}

	[Test]
	public void TestLastIndexOf5 ()
	{
		char [] a = new char [] {'j', 'i', 'h', 'g', 'f', 'e', 'd', 'c', 'b', 'a', 'j', 'i', 'h'};
		string s;
		int retval;
		bool error = false;

		for (int i = a.Length - 1; i >= 0 ; i--) {
			s = i.ToString ();
			retval = Array.LastIndexOf(a, a [i], i, i + 1);
			if (retval != i)
				error = true;
		}
		Assert (!error);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOf_StartIndexOverflow ()
	{
		// legal - no exception
		byte[] array = new byte [16];
		Array.LastIndexOf (array, this, Int32.MaxValue, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOf_CountOverflow ()
	{
		// legal - no exception
		byte[] array = new byte [16];
		Array.LastIndexOf (array, this, 1, Int32.MaxValue);
	}

	[Test]
	public void LastIndexOf_0LengthArray ()
	{
		Array array = Array.CreateInstance (typeof (char), 0);
		int idx = Array.LastIndexOf (array, (object) null, -1, 0);
		NUnit.Framework.Assert.IsTrue (idx == -1, "#01");
		idx = Array.LastIndexOf (array, (object) null, -1, 10);
		NUnit.Framework.Assert.IsTrue (idx == -1, "#02");
		idx = Array.LastIndexOf (array, (object) null, -100, 10);
		NUnit.Framework.Assert.IsTrue (idx == -1, "#02");

		array = Array.CreateInstance (typeof (char), 1);
		try {
			Array.LastIndexOf (array, (object) null, -1, 0);
			NUnit.Framework.Assert.Fail ("#04");
		} catch (ArgumentOutOfRangeException e) {
		}
		try {
			Array.LastIndexOf (array, (object) null, -1, 10);
			NUnit.Framework.Assert.Fail ("#05");
		} catch (ArgumentOutOfRangeException e) {
		}
		try {
			Array.LastIndexOf (array, (object) null, -100, 10);
			NUnit.Framework.Assert.Fail ("#06");
		} catch (ArgumentOutOfRangeException e) {
		}
	}

	[Test]
	public void TestReverse() {
		{
			bool errorThrown = false;
			try {
				Array.Reverse(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#L01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#L02", errorThrown);
		}
		
		char[] c1 = {'a', 'b', 'c', 'd'};
		Array.Reverse(c1);
		AssertEquals("#L03", 'd', c1[0]);
		AssertEquals("#L04", 'c', c1[1]);
		AssertEquals("#L05", 'b', c1[2]);
		AssertEquals("#L06", 'a', c1[3]);

		{
			bool errorThrown = false;
			try {
				Array.Reverse(null, 0, 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#L07", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c, 0, 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert("#L08", errorThrown);
		}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 0, 3);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert("#L09", errorThrown);
		//}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 3, 0);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert("#L10", errorThrown);
		//}

		char[] c2 = { 'a', 'b', 'c', 'd'};
		Array.Reverse(c2, 1, 2);
		AssertEquals("#L11", 'a', c2[0]);
		AssertEquals("#L12", 'c', c2[1]);
		AssertEquals("#L13", 'b', c2[2]);
		AssertEquals("#L14", 'd', c2[3]);
	}

	[Test]
	public void TestSetValue1() {
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", 1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#M01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", -1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M02", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", 4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M03", errorThrown);
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		char[] c2 = new char[4];
		for (int i = 0; i < c1.Length; i++) {
			c2.SetValue(c1[i], i);
		}
		for (int i = 0; i < c1.Length; i++) {
			AssertEquals("#M04(" + i + ")", c1[i], c2[i]);
		}

		int[] c3 = { 1, 2, 3 };
		long[] c4 = new long [3];

		for (int i = 0; i < c3.Length; i++)
			c4.SetValue (c3 [i], i);

		try {
			c3.CopyTo (c4, 0);
		} catch (Exception e) {
			Fail ("c3.CopyTo(): e=" + e);
		}
		for (int i = 0; i < c3.Length; i++)
			Assert ("#M05(" + i + ")", c3[i] == c4[i]);

		Object[] c5 = new Object [3];
		long[] c6 = new long [3];

		try {
			c4.CopyTo (c5, 0);
		} catch (Exception e) {
			Fail ("c4.CopyTo(): e=" + e);
		}

		try {
			c5.CopyTo (c6, 0);
		} catch (Exception e) {
			Fail ("c5.CopyTo(): e=" + e);
		}
		// for (int i = 0; i < c5.Length; i++)
		// Assert ("#M06(" + i + ")", c5[i] == c6[i]);
	}

	[Test]
	public void TestSetValue2() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", 1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#M21", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", -1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M22", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", 4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M23", errorThrown);
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
				AssertEquals("#M24(" + i + "," + j + ")",
					c1[i,j], c2[i, j]);
			}
		}
	}

	[Test]
	public void TestSetValue3() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", 1,1,1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#M41", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", -1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M42", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", 4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert("#M43", errorThrown);
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
					AssertEquals("#M44(" + i + "," + j + " )",
						c1[i,j,k], c2[i,j,k]);
				}
			}
		}
	}

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#else
	[ExpectedException (typeof (NullReferenceException))]
#endif
	public void TestSetValueLongArray ()
	{
		char[] c = new Char[2];
		c.SetValue("buh", (long [])null);
	}

	[Test]
	public void TestSetValueN() {
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				c.SetValue("buh", (int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#M61a", errorThrown);
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
			Assert("#M62", errorThrown);
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
			Assert("#M63", errorThrown);
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
			Assert("#M64", errorThrown);
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
				AssertEquals("#M65(" + i + "," + j + ")",
					c1[i,j], c2[i,j]);
			}
		}
	}

	[Test]
	public void TestSetValue4() {
		{
			int[] c1 = { 1, 2, 3 };
			long[] c2 = new long [3];

			for (int i = 0; i < c1.Length; i++)
				c2.SetValue (c1 [i], i);

			for (int i = 0; i < c1.Length; i++) {
				Assert ("#M81(" + i + ")", c1[i] == c2[i]);
				AssertEquals ("#M82(" + i + ")", typeof (long), c2[i].GetType ());
			}
		}
		{
			long[] c1 = { 1, 2, 3 };
			int[] c2 = new int [3];
			bool errorThrown = false;
			try {
				c2.SetValue (c1 [0], 0);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert("#M83", errorThrown);
		}
		{
			int[] c1 = { 1, 2, 3 };
			Object[] c2 = new Object [3];

			for (int i = 0; i < c1.Length; i++)
				c2.SetValue (c1 [i], i);

			for (int i = 0; i < c1.Length; i++)
				AssertEquals ("#M84(" + i + ")", c1[i], Convert.ToInt32 (c2[i]));
		}
		{
			Object[] c1 = new Object [3];
			Object[] c2 = new Object [3];
			c1[0] = new Object ();

			for (int i = 0; i < c1.Length; i++)
				c2.SetValue (c1 [i], i);

			for (int i = 0; i < c1.Length; i++)
				AssertEquals ("#M85(" + i + ")", c1[i], c2[i]);
		}
		{
			Object[] c1 = new Object [3];
			string[] c2 = new String [3];
			string test = "hello";
			c1[0] = test;

			c2.SetValue (c1 [0], 0);
			AssertEquals ("#M86", c1[0], c2[0]);
			AssertEquals ("#M87", "hello", c2[0]);
		}
		{
			char[] c1 = { 'a', 'b', 'c' };
			string[] c2 = new string [3];
			try {
				c2.SetValue (c1 [0], 0);
				Fail ("#M88");
			} catch (InvalidCastException) {}
		}
		{
			Single[] c1 = { 1.2F, 2.3F, 3.4F, 4.5F };
			long[] c2 = new long [3];
			try {
				c2.SetValue (c1 [0], 0);
				Fail ("#M89");
			} catch (ArgumentException) {}
		}
		{
			Type[] types = {
				typeof (Boolean),
				typeof (Byte),
				typeof (Char),
				typeof (Double),
				typeof (Int16),
				typeof (Int32),
				typeof (Int64),
				typeof (SByte),
				typeof (Single),
				typeof (UInt16),
				typeof (UInt32),
				typeof (UInt64)
			};

			bool v1 = true;
			Byte v2 = 1;
			Char v3 = 'a';
			Double v4 = -1.2;
			Int16 v5 = -32;
			Int32 v6 = -234;
			Int64 v7 = -34523;
			SByte v8 = -1;
			Single v9 = -4.8F;
			UInt16 v10 = 24234;
			UInt32 v11 = 235354;
			UInt64 v12 = 234552;

			Object[] va1 = { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 };
			Object[] va2 = { "true", "1", "a", "-1.2", "-32", "-234", "-34523", "-1",
					 "-4.8F", "24234", "235354", "234552" };

			Object[][] vt = { va1, va1, va1, va1, va1, va1, va1, va1, va1, va1, va1, va1 };

			int[] arg_ex = {
				0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
				1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0,
				1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1,
				1, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1,
				1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 1,
				1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1,
				1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1,
				1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0,
				1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 0, 0,
				1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0
			};

			// SetValue

			for (int i = 0; i < types.Length; i++) {
				for (int j = 0; j < types.Length; j++) {
					Array array = Array.CreateInstance (types [j], 2);

					Object value = vt[j][i];

					bool errorThrown = false;
					try {
						array.SetValue (value, 0);
					} catch (ArgumentException) {
						errorThrown = true;
					}

					int ex_index = (i * types.Length) + j;

					AssertEquals ("#M90(" + types [i] + "," + types [j] + ")",
						errorThrown, arg_ex [ex_index] == 1);
				}
			}

			for (int i = 0; i < types.Length; i++) {
				String[] array = new String [2];

				Object value = va1 [i];

				bool errorThrown = false;
				try {
					array.SetValue (value, 0);
				} catch (InvalidCastException) {
					errorThrown = true;
				}

				Assert ("#M91(" + types [i] + ")", errorThrown);
			}

			for (int i = 0; i < types.Length; i++) {
				Array array = Array.CreateInstance (types [i], 2);

				Object value = va2 [i];

				bool errorThrown = false;
				try {
					array.SetValue (value, 0);
				} catch (InvalidCastException) {
					errorThrown = true;
				}

				Assert ("#M92(" + types [i] + ")", errorThrown);
			}

			for (int i = 0; i < types.Length; i++) {
				Array array = Array.CreateInstance (types [i], 2);

				Object value = null;

				bool errorThrown = false;
				try {
					array.SetValue (value, 0);
				} catch (InvalidCastException) {
					errorThrown = true;
				}

				Assert ("#M93(" + types [i] + ")", !errorThrown);
			}

			// Copy

			for (int i = 0; i < types.Length; i++) {
				for (int j = 0; j < types.Length; j++) {
					Array source = Array.CreateInstance (types [i], 2);
					Array array = Array.CreateInstance (types [j], 2);

					source.SetValue (vt[j][i], 0);
					source.SetValue (vt[j][i], 1);

					bool errorThrown = false;
					try {
						Array.Copy (source, array, 2);
					} catch (ArrayTypeMismatchException) {
						errorThrown = true;
					}

					int ex_index = (i * types.Length) + j;

					AssertEquals ("#M94(" + types [i] + "," + types [j] + ")",
						errorThrown, arg_ex [ex_index] == 1);
				}
			}

			for (int i = 0; i < types.Length; i++) {
				Array source = Array.CreateInstance (types [i], 2);
				String[] array = new String [2];

				source.SetValue (va1 [i], 0);
				source.SetValue (va1 [i], 1);

				bool errorThrown = false;
				try {
					Array.Copy (source, array, 2);
				} catch (ArrayTypeMismatchException) {
					errorThrown = true;
				}

				Assert ("#M95(" + types [i] + ")", errorThrown);
			}

			for (int i = 0; i < types.Length; i++) {
				String[] source = new String [2];
				Array array = Array.CreateInstance (types [i], 2);

				source.SetValue (va2 [i], 0);
				source.SetValue (va2 [i], 1);

				bool errorThrown = false;
				try {
					Array.Copy (source, array, 2);
				} catch (ArrayTypeMismatchException) {
					errorThrown = true;
				}

				Assert ("#M96(" + types [i] + ")", errorThrown);
			}
		}
	}

	[Test]
	public void TestSort() {
		{
			bool errorThrown = false;
			try {
				Array.Sort(null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#N01", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				Array.Sort(null, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#N02", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#N03", errorThrown);
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert("#N04", errorThrown);
		}
		{
			int tc = 5;
			char[] arr = {'d', 'b', 'f', 'e', 'a', 'c'};
			
			try {
				Array.Sort (null, 0, 1);
				Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, -1, 3);
				Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, 1, -3);
				Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, arr.Length, arr.Length + 2);
				Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Fail ("#N" + tc.ToString ()); }
		}
		
		// note: null second array => just sort first array
		char[] starter = {'d', 'b', 'f', 'e', 'a', 'c'};
		int[] starter1 = {1,2,3,4,5,6};
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1);
			AssertEquals("#N21", 'a', c1[0]);
			AssertEquals("#N22", 'b', c1[1]);
			AssertEquals("#N23", 'c', c1[2]);
			AssertEquals("#N24", 'd', c1[3]);
			AssertEquals("#N25", 'e', c1[4]);
			AssertEquals("#N26", 'f', c1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1);
			AssertEquals("#N41", 'a', c1[0]);
			AssertEquals("#N42", 'b', c1[1]);
			AssertEquals("#N43", 'c', c1[2]);
			AssertEquals("#N44", 'd', c1[3]);
			AssertEquals("#N45", 'e', c1[4]);
			AssertEquals("#N46", 'f', c1[5]);
			AssertEquals("#N47", 5, i1[0]);
			AssertEquals("#N48", 2, i1[1]);
			AssertEquals("#N49", 6, i1[2]);
			AssertEquals("#N50", 1, i1[3]);
			AssertEquals("#N51", 4, i1[4]);
			AssertEquals("#N52", 3, i1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1, 1, 4);
			AssertEquals("#N61", 'd', c1[0]);
			AssertEquals("#N62", 'a', c1[1]);
			AssertEquals("#N63", 'b', c1[2]);
			AssertEquals("#N64", 'e', c1[3]);
			AssertEquals("#N65", 'f', c1[4]);
			AssertEquals("#N66", 'c', c1[5]);
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1, 1, 4);
			AssertEquals("#N81", 'd', c1[0]);
			AssertEquals("#N82", 'a', c1[1]);
			AssertEquals("#N83", 'b', c1[2]);
			AssertEquals("#N84", 'e', c1[3]);
			AssertEquals("#N85", 'f', c1[4]);
			AssertEquals("#N86", 'c', c1[5]);
			AssertEquals("#N87", 1, i1[0]);
			AssertEquals("#N88", 5, i1[1]);
			AssertEquals("#N89", 2, i1[2]);
			AssertEquals("#N90", 4, i1[3]);
			AssertEquals("#N91", 3, i1[4]);
			AssertEquals("#N92", 6, i1[5]);
		}
	}

	[Test]
	public void TestInitializeEmpty()
	{
		bool catched=false;
		int[] a = {};
		try
		{
			a.Initialize();
		}
		catch(Exception)
		{
			catched=true;
		}
		Assert("#TI01",!catched);
	}

	[Test]
	public void TestInitializeInt()
	{
		int[] a = {1,2,0};
		a.Initialize();
		int[] b = {1,2,0};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI02 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeDouble()
	{
		double[] a = {1.0,2.0,0.0};
		a.Initialize();
		double[] b = {1.0,2.0,0.0};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI03 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeFloat()
	{
		float[] a = {1.0F,2.0F,0.0F};
		a.Initialize();
		float[] b = {1.0F,2.0F,0.0F};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI04 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeChar()
	{
		char[] a = {'1','.','0','F','2','.','0','F'};
		a.Initialize();
		char[] b = {'1','.','0','F','2','.','0','F'};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI05 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeString()
	{
		string[] a = {"hola","adios","menos","mas"};
		a.Initialize();
		string[] b = {"hola","adios","menos","mas"};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI06 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeEnum()
	{
		enua[] a = {enua.hola,enua.adios,enua.menos,enua.mas};
		a.Initialize();
		enua[] b = {enua.hola,enua.adios,enua.menos,enua.mas};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI07 " + i ,a[i],b[i]);
		}
	}
	
	[Test]
	public void TestInitializeIntNI()
	{
		int[] a = new int[20];
		a.Initialize();
		foreach(int b in a)
		{
			AssertEquals("#TI08",b,0);
		}
	}
	
	[Test]
	public void TestInitializeCharNI()
	{
		char[] a = new char[20];
		a.Initialize();
		foreach(char b in a)
		{
			AssertEquals("#TI09",b,0);
		}
	}
	
	[Test]
	public void TestInitializeDoubleNI()
	{
		double[] a = new double[20];
		a.Initialize();
		foreach(double b in a)
		{
			AssertEquals("#TI09",b,0.0);
		}
	}
	
	[Test]
	public void TestInitializeStringNI()
	{
		string[] a = new string[20];
		a.Initialize();
		foreach(string b in a)
		{
			AssertEquals("#TI10",b,null);
		}
	}
	
	[Test]
	public void TestInitializeObjectNI()
	{
		object[] a = new object[20];
		a.Initialize();
		foreach(object b in a)
		{
			AssertEquals("#TI11",b,null);
		}
	}

	[Test]
	public void TestInitializeAClassNI()
	{
		AClass[] a = new AClass[20];
		a.Initialize();
		foreach(AClass b in a)
		{
			AssertEquals("#TI12",b,null);
		}
	}


	[Test]
	public void TestInitializeAStructNI()
	{
		AStruct[] a = new AStruct[20];
		a.Initialize();
		foreach(AStruct b in a)
		{
			AssertEquals("#TI14",b,new AStruct());
		}
	}

	[Test]
	public void TestInitializeAStruct()
	{
		AStruct[] a = new AStruct[3];
		a[1].a = "ADIOS";
		a[1].s = "HOLA";
		a.Initialize();
		AStruct[] b = new AStruct[3];
		b[1].a = "ADIOS";
		b[1].s = "HOLA";
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			AssertEquals("#TI15 " + i ,a[i],b[i]);
		}
	}

	[Test]
	public void TestInitializeDateTimeNI()
	{
		DateTime[] a = new DateTime[20];
		a.Initialize();
		foreach(DateTime b in a)
		{
			AssertEquals("#TI16",b,new DateTime());
		}
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void MoreSort1 ()
	{
		Array.Sort (null, 0, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MoreSort2 ()
	{
		Array.Sort (arrsort, -1, 3);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void MoreSort3 ()
	{
		Array.Sort (arrsort, 1, -3);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void MoreSort4 ()
	{
		Array.Sort (arrsort, arrsort.Length, arrsort.Length + 2);
	}

	[Test]
	[ExpectedException (typeof (RankException))]
	public void MoreSort5 ()
	{
		char [,] arr = new char [,] {{'a'}, {'b'}};
		Array.Sort (arr, 0, 1);
	}

	[Test]
	public void MoreSort6 ()
	{
		Array.Sort (arrsort, 0, 0);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void MoreSort7 ()
	{
		Array.Sort (arrsort, arrsort.Length - 1, 2);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void MoreSort8 ()
	{
		Array.Sort (arrsort, 0, arrsort.Length + 1);
	}

	[Test]
	public void MoreSort9 ()
	{
		Array.Sort (arrsort, null, 0, arrsort.Length, null);
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void MoreSort10 ()
	{
		object [] array = {true, 'k', SByte.MinValue, Byte.MinValue, (short) 2, 634, (long) 436, (float) 1.1, 1.23, "Hello World"};
		Array.Sort (array, (IComparer) null);
	}

	[Test] // bug #81941
	public void Sort ()
	{
		double [] a = new double [2] { 0.9, 0.3 };
		uint [] b = new uint [2] { 4, 7 };
		Array.Sort (a, b);
		AssertEquals ("#1", 0.3, a [0]);
		AssertEquals ("#2", 0.9, a [1]);
		AssertEquals ("#3", 7, b [0]);
		AssertEquals ("#4", 4, b [1]);
	}

	[Test]
	public void ClearJaggedArray () 
	{
		byte[][] matrix = new byte [8][];
		for (int i=0; i < 8; i++) {
			matrix [i] = new byte [8];
			for (int j=0; j < 8; j++) {
				matrix [i][j] = 1;
			}
		}
		Array.Clear (matrix, 0, 8);
		for (int i=0; i < 8; i++) {
			AssertNull (i.ToString (), matrix [i]);
		}
	}

	[Test]
	public void ClearMultidimentionalArray () 
	{
		byte[,] matrix = new byte [2,2] { {1, 1}, {2, 2} };
		Array.Clear (matrix, 0, 2);
		AssertEquals ("0,0", 0, matrix [0,0]);
		AssertEquals ("0,1", 0, matrix [0,1]);
		AssertEquals ("1,0", 2, matrix [1,0]);
		AssertEquals ("1,1", 2, matrix [1,1]);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void ClearOutsideMultidimentionalArray () 
	{
		byte[,] matrix = new byte [2,2] { {1, 1}, {2, 2} };
		Array.Clear (matrix, 0, 5);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void Clear_IndexOverflow () 
	{
		byte[] array = new byte [16];
		Array.Clear (array, 4, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void Clear_LengthOverflow () 
	{
		byte[] array = new byte [16];
		Array.Clear (array, Int32.MaxValue, 4);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Copy_SourceIndexOverflow () 
	{
		byte[] array = new byte [16];
		Array.Copy (array, Int32.MaxValue, array, 8, 8);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Copy_DestinationIndexOverflow () 
	{
		byte[] array = new byte [16];
		Array.Copy (array, 8, array, Int32.MaxValue, 8);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Copy_LengthOverflow () 
	{
		byte[] array = new byte [16];
		Array.Copy (array, 8, array, 8, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Reverse_IndexOverflow () 
	{
		byte[] array = new byte [16];
		Array.Reverse (array, Int32.MaxValue, 8);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Reverse_LengthOverflow () 
	{
		byte[] array = new byte [16];
		Array.Reverse (array, 8, Int32.MaxValue);
	}
	
	public struct CharX : IComparable {
		public char c;
	
		public CharX (char c)
		{
			this.c = c;
		}
	
		public int CompareTo (object obj)
		{
			if (obj is CharX)
				return c.CompareTo (((CharX) obj).c);
			else
				return c.CompareTo (obj);
		}
	}

	[Test]
	public void BinarySearch_ArgPassingOrder ()
	{
		//
		// This tests that arguments are passed to the comprer in the correct
		// order. The IComparable of the *array* elements must get called, not
		// that of the search object.
		//
		CharX [] x = { new CharX ('a'), new CharX ('b'), new CharX ('c') };
		AssertEquals (1, Array.BinarySearch (x, 'b'));
	}

	class Comparer: IComparer {

		private bool called = false;

		public bool Called {
			get {
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
		int[] array = new int[0];
		AssertEquals ("BinarySearch", - 1, Array.BinarySearch (array, 0));
	}

	[Test]
	public void BinarySearch2_EmptyList ()
	{
		int[] array = new int[0];
		AssertEquals ("BinarySearch", -1, Array.BinarySearch (array, 0, 0, 0));
	}

	[Test]
	public void BinarySearch3_EmptyList ()
	{
		Comparer comparer = new Comparer ();
		int[] array = new int[0];
		AssertEquals ("BinarySearch", -1, Array.BinarySearch (array, 0, comparer));
		// bug 77030 - the comparer isn't called for an empty array/list
		Assert ("Called", !comparer.Called);
	}

	[Test]
	public void BinarySearch4_EmptyList ()
	{
		Comparer comparer = new Comparer ();
		int[] array = new int[0];
		AssertEquals ("BinarySearch", -1, Array.BinarySearch (array, 0, 0, comparer));
		// bug 77030 - the comparer isn't called for an empty array/list
		Assert ("Called", !comparer.Called);
	}

#if NET_2_0
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void AsReadOnly_NullArray ()
	{
		Array.AsReadOnly <int> (null);
	}

	[Test]
	public void ReadOnly_Count ()
	{
		AssertEquals (10, Array.AsReadOnly (new int [10]).Count);
	}

	[Test]
	public void ReadOnly_Contains ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		Assert (a.Contains (3));
		Assert (!a.Contains (6));
	}

	[Test]
	public void ReadOnly_IndexOf ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		AssertEquals (0, a.IndexOf (3));
		AssertEquals (1, a.IndexOf (5));
		AssertEquals (-1, a.IndexOf (6));
	}

	[Test]
	public void ReadOnly_Indexer ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		AssertEquals (3, a [0]);
		AssertEquals (5, a [1]);

		/* Check that modifications to the original array are visible */
		arr [0] = 6;
		AssertEquals (6, a [0]);
	}

	[Test]
	public void ReadOnly_Enumerator ()
	{
		int[] arr = new int [10];

		for (int i = 0; i < 10; ++i)
			arr [i] = i;

		int sum = 0;
		foreach (int i in Array.AsReadOnly (arr))
			sum += i;

		AssertEquals (45, sum);
	}

	[Test]
	public void Resize ()
	{
		int [] arr = new int [] { 1, 3, 5 };
		Array.Resize <int> (ref arr, 3);
		AssertEquals ("#A1", 3, arr.Length);
		AssertEquals ("#A2", 1, arr [0]);
		AssertEquals ("#A3", 3, arr [1]);
		AssertEquals ("#A4", 5, arr [2]);

		Array.Resize <int> (ref arr, 2);
		AssertEquals ("#B1", 2, arr.Length);
		AssertEquals ("#B2", 1, arr [0]);
		AssertEquals ("#B3", 3, arr [1]);

		Array.Resize <int> (ref arr, 4);
		AssertEquals ("#C1", 4, arr.Length);
		AssertEquals ("#C2", 1, arr [0]);
		AssertEquals ("#C3", 3, arr [1]);
		AssertEquals ("#C4", 0, arr [2]);
		AssertEquals ("#C5", 0, arr [3]);
	}

	[Test]
	public void Resize_null ()
	{
		int [] arr = null;
		Array.Resize (ref arr, 10);
		AssertEquals (arr.Length, 10);
	}

	[Test]
	public void Test_ContainsAndIndexOf_EquatableItem ()
	{
		EquatableClass[] list = new EquatableClass[] {new EquatableClass (0), new EquatableClass (1), new EquatableClass (0)};

		AssertEquals ("#0", 0, Array.IndexOf<EquatableClass> (list, list[0]));
		AssertEquals ("#1", 0, Array.IndexOf<EquatableClass> (list, new EquatableClass (0)));
		AssertEquals ("#2", 2, Array.LastIndexOf<EquatableClass> (list, list[0]));
		AssertEquals ("#3", 2, Array.LastIndexOf<EquatableClass> (list, new EquatableClass (0)));
	}

	public class EquatableClass : IEquatable<EquatableClass>
	{
		int _x;
		public EquatableClass (int x)
		{
			_x = x;
		}

		public bool Equals (EquatableClass other)
		{
			return this._x == other._x;
		}
	}

	[Test]
	public void AsIList ()
	{
		IList<int> arr = new int [10];
		arr [0] = 5;
		AssertEquals (5, arr [0]);

		IList<FooStruct> arr2 = new FooStruct [10];
		FooStruct s = new FooStruct ();
		s.i = 11;
		s.j = 22;
		arr2 [5] = s;
		s = arr2 [5];
		AssertEquals (11, s.i);
		AssertEquals (22, s.j);

		IList<string> arr3 = new string [10];
		arr3 [5] = "ABC";
		AssertEquals ("ABC", arr3 [5]);
	}

	struct FooStruct {
		public int i, j;
	}

#if !TARGET_JVM // BugBUG: T[] is not yet ICollection<T> under TARGET_JVM
	[Test]
	// From bug #80563
	public void ICollectionNull ()
	{
		ICollection<object> test;
		
		test = new List<object>();
		AssertEquals ("list<o>", test.Contains (null), false);

		test = new object[] {};
		AssertEquals ("empty array", test.Contains (null), false);

		test = new object[] {null};
		AssertEquals ("array with null", test.Contains (null), true);

		test = new List<object>(test);
		AssertEquals ("List<object> with test", test.Contains (null), true);
		
		test = new object[] {new object()};
		AssertEquals ("array with object", test.Contains (null), false);

		test = new List<object>(test);
		AssertEquals ("array with test", test.Contains (null), false);
	}
#endif // TARGET_JVM
#endif

	#region Bug 80299

	enum ByteEnum : byte {}
	enum IntEnum : int {}

	[Test]
	public void TestByteEnumArrayToByteArray ()
	{
		ByteEnum[] a = new ByteEnum[] {(ByteEnum) 1, (ByteEnum) 2};
		byte[] b = new byte[a.Length];
		a.CopyTo (b, 0);
	}

	[Test]
	public void TestByteEnumArrayToIntArray ()
	{
		ByteEnum[] a = new ByteEnum[] {(ByteEnum) 1, (ByteEnum) 2};
		int[] b = new int[a.Length];
		a.CopyTo (b, 0);
	}

	[Test]
	[ExpectedException (typeof (ArrayTypeMismatchException))]
	public void TestIntEnumArrayToByteArray ()
	{
		IntEnum[] a = new IntEnum[] {(IntEnum) 1, (IntEnum) 2};
		byte[] b = new byte[a.Length];
		a.CopyTo (b, 0);
	}

	[Test]
	public void TestIntEnumArrayToIntArray ()
	{
		IntEnum[] a = new IntEnum[] {(IntEnum) 1, (IntEnum) 2};
		int[] b = new int[a.Length];
		a.CopyTo (b, 0);
	}

	#endregion

#if NET_2_0
	[Test] // bug #322248
	public void IEnumerator_Reset ()
	{
		int[] array = new int[] { 1, 2, 3};
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();
		Assert ("#A1", e.MoveNext ());
		AssertEquals ("#A2", 1, e.Current);
		Assert ("#A3", e.MoveNext ());
		AssertEquals ("#A4", 2, e.Current);

		e.Reset ();

		Assert ("#C1", e.MoveNext ());
		AssertEquals ("#C2", 1, e.Current);
	}

	[Test]
	public void IEnumerator_Current_Finished ()
	{
		int[] array = new int[] { 1, 2, 3 };
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();
		Assert ("#A1", e.MoveNext ());
		AssertEquals ("#A2", 1, e.Current);
		Assert ("#A3", e.MoveNext ());
		AssertEquals ("#A4", 2, e.Current);
		Assert ("#A5", e.MoveNext ());
		AssertEquals ("#A6", 3, e.Current);
		Assert ("#A6", !e.MoveNext ());

		try {
			Fail ("#B1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration already finished
			AssertEquals ("#B2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
		}
	}

	[Test]
	public void IEnumerator_Current_NotStarted ()
	{
		int[] array = new int[] { 1, 2, 3 };
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();

		try {
			Fail ("#A1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration has not started. Call MoveNext
			AssertEquals ("#A2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
		}
	}

	[Test]
	public void IEnumerator_Current_Reset ()
	{
		int[] array = new int[] { 1, 2, 3 };
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();
		e.MoveNext ();
		e.Reset ();

		try {
			Fail ("#B1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration has not started. Call MoveNext
			AssertEquals ("#B2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
		}
	}
#endif
}
}
