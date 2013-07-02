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
using System.Reflection;
using System.Collections.Generic;

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

		public override int GetHashCode ()
		{
			return 0;
		}
	}
		
	//End Auxiliary Things

[TestFixture]
public class ArrayTest
{
	char [] arrsort = {'d', 'b', 'f', 'e', 'a', 'c'};

	public ArrayTest() {}

	[Test]
	public void TestIsFixedSize() {
		char[] a1 = {'a'};
		Assert.IsTrue (a1.IsFixedSize, "All arrays are fixed");
	}
	
	[Test]
	public void TestIsReadOnly() {
		char[] a1 = {'a'};
		Assert.IsTrue (!a1.IsReadOnly, "No array is readonly");
	}

	[Test]
	public void TestIsSynchronized() {
		char[] a1 = {'a'};
		Assert.IsTrue (!a1.IsSynchronized, "No array is synchronized");
	}

	[Test]
	public void TestLength() {
		{
			char[] a1 = { };
			Assert.AreEqual (0, a1.Length, "Zero length array");
		}
		{
			char[] a1 = {'c'};
			Assert.AreEqual (1, a1.Length, "One-length array");
		}
		{
			char[] a1 = {'c', 'c'};
			Assert.AreEqual (2, a1.Length, "Two-length array");
		}
	}

	[Test]
	public void TestRank() {
		char[] a1 = { 'c', 'd', 'e' };
		Assert.AreEqual (1, a1.Rank, "Rank one");

		char[,] a2 = new Char[3,3];
		Assert.AreEqual (2, a2.Rank, "Rank two");

		char[,,] a3 = new Char[3,3,3];
		Assert.AreEqual (3, a3.Rank, "Rank three");
	}

	[Test]
	public void TestBinarySearch1() {
		bool errorThrown = false;
		try {
			Array.BinarySearch(null, "blue");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B01");
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B02");

		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert.IsTrue (Array.BinarySearch(arr, 'c') >= 3, "#B05");
			Assert.IsTrue (Array.BinarySearch(arr, 'c') < 6, "#B06");
		}
		{
			char[] arr = {'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			Assert.AreEqual (-4, Array.BinarySearch(arr, 'c'), "#B07");
		}
		{
			char[] arr = {'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert.AreEqual (-9, Array.BinarySearch(arr, 'e'), "#B08");
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
		Assert.IsTrue (errorThrown, "#B20");
		errorThrown = false;
		try {
			char[,] c1 = new Char[2,2];
			Array.BinarySearch(c1, 0, 1, "needle");
		} catch (RankException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B21");
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, -1, 1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B22");
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, -1, 'a');
		} catch (ArgumentOutOfRangeException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B23");
		errorThrown = false;
		try {
			char[] c1 = {'a'};
			Array.BinarySearch(c1, 0, 4, 'a');
		} catch (ArgumentException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#B24");

		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert.IsTrue (Array.BinarySearch(arr, 2, 8, 'c') >= 5, "#B26");
			Assert.IsTrue (Array.BinarySearch(arr, 2, 8, 'c') < 8, "#B27");
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'd', 'd', 'd', 'e', 'e'};
			Assert.AreEqual (-6, Array.BinarySearch(arr, 2, 8, 'c'), "#B28");
		}
		{
			char[] arr = {'z', 'z', 'a', 'b', 'b', 'c', 'c', 'c', 'd', 'd'};
			Assert.AreEqual (-11, Array.BinarySearch(arr, 2, 8, 'e'), "#B29");
		}
	}

	public void TestBinarySearch3()
	{
		int[] array = new int[100];

		for (int i = 0; i < 100; i++)
			array[i] = 10;

		Assert.AreEqual (49, Array.BinarySearch(array, 10), "#B30");
	}

	[Test]
	public void BinarySearch_NullValue () 
	{
		int[] array = new int[1];
		Assert.AreEqual (-1, Array.BinarySearch (array, null), "I=a,o");
		Assert.AreEqual (-1, Array.BinarySearch (array, null, null), "I=a,o,c");
		Assert.AreEqual (-1, Array.BinarySearch (array, 0, 1, null), "I=a,i,i,o");
		Assert.AreEqual (-1, Array.BinarySearch (array, 0, 1, null, null), "I=a,i,i,o,c");

		object[] o = new object [3] { this, this, null };
		Assert.AreEqual (-1, Array.BinarySearch (o, null), "O=a,o");
		Assert.AreEqual (-1, Array.BinarySearch (o, null, null), "O=a,o,c");
		Assert.AreEqual (-1, Array.BinarySearch (o, 0, 3, null), "O=a,i,i,o");
		Assert.AreEqual (-1, Array.BinarySearch (o, 0, 3, null, null), "O=a,i,i,o,c");
	}

	class TestComparer7 : IComparer<int>
	{
		public int Compare (int x, int y)
		{
			if (y != 7)
				throw new ApplicationException ();

			return x.CompareTo (y);
		}
	}

	[Test]
	public void BinarySearch_WithComparer ()
	{
		var a = new int[] { 2, 6, 9 };
		Assert.AreEqual (-3, Array.BinarySearch (a, 7, new TestComparer7 ()));
	}

	[Test]
	public void TestClear() {
		bool errorThrown = false;
		try {
			Array.Clear(null, 0, 1);
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "#C01");

		int[] i1 = { 1, 2, 3, 4 };
		{
			int[] compare = {1,2,3,4};
			Assert.AreEqual (compare[0], i1[0], "#C02");
			Assert.AreEqual (compare[1], i1[1], "#C03");
			Assert.AreEqual (compare[2], i1[2], "#C04");
			Assert.AreEqual (compare[3], i1[3], "#C05");
		}
		Array.Clear(i1, 3, 1);
		{
			int[] compare = {1,2,3,0};
			Assert.AreEqual (compare[0], i1[0], "#C06");
			Assert.AreEqual (compare[1], i1[1], "#C07");
			Assert.AreEqual (compare[2], i1[2], "#C08");
			Assert.AreEqual (compare[3], i1[3], "#C09");
		}
		Array.Clear(i1, 1, 1);
		{
			int[] compare = {1,0,3,0};
			Assert.AreEqual (compare[0], i1[0], "#C10");
			Assert.AreEqual (compare[1], i1[1], "#C11");
			Assert.AreEqual (compare[2], i1[2], "#C12");
			Assert.AreEqual (compare[3], i1[3], "#C13");
		}
		Array.Clear(i1, 1, 3);
		{
			int[] compare = {1,0,0,0};
			Assert.AreEqual (compare[0], i1[0], "#C14");
			Assert.AreEqual (compare[1], i1[1], "#C15");
			Assert.AreEqual (compare[2], i1[2], "#C16");
			Assert.AreEqual (compare[3], i1[3], "#C17");
		}

		string[] s1 = { "red", "green", "blue" };
		Array.Clear(s1, 0, 3);
		{
			string[] compare = {null, null, null};
			Assert.AreEqual (compare[0], s1[0], "#C18");
			Assert.AreEqual (compare[1], s1[1], "#C19");
			Assert.AreEqual (compare[2], s1[2], "#C20");
		}
	}

	[Test]
	public void TestClone() {
		char[] c1 = {'a', 'b', 'c'};
		char[] c2 = (char[])c1.Clone();
		Assert.AreEqual (c1[0], c2[0], "#D01");
		Assert.AreEqual (c1[1], c2[1], "#D02");
		Assert.AreEqual (c1[2], c2[2], "#D03");

		char[] d10 = {'a', 'b'};
		char[] d11 = {'a', 'c'};
		char[] d12 = {'b', 'c'};
		char[][] d1 = {d10, d11, d12};
		char[][] d2 = (char[][])d1.Clone();
		Assert.AreEqual (d1[0], d2[0], "#D04");
		Assert.AreEqual (d1[1], d2[1], "#D05");
		Assert.AreEqual (d1[2], d2[2], "#D06");

		d1[0][0] = 'z';
		Assert.AreEqual (d1[0], d2[0], "#D07");
	}

	[Test]
	public void TestMemberwiseClone () {
		int[] array = new int[] { 1, 2, 3 };
		MethodBase mi = array.GetType ().GetMethod("MemberwiseClone",
												   BindingFlags.Instance | BindingFlags.NonPublic);
		int[] res = (int[])mi.Invoke (array, null);
		Assert.AreEqual (3, res.Length);
	}

	[Test] public void TestIndexer ()
	{
		int [] a = new int [10];
		IList b = a;
		try {
			object c = b [-1];
			Assert.Fail ("IList.this [-1] should throw");
		} catch (IndexOutOfRangeException) {
			// Good
		} catch (Exception){
			Assert.Fail ("Should have thrown an IndexOutOfRangeException");
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
			Assert.IsTrue (errorThrown, "#E01");
		}
		{
			bool errorThrown = false;
			try {
				Char[] c1 = {};
				Array.Copy(null, c1, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#E02");
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
			Assert.IsTrue (errorThrown, "#E03");
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
			Assert.IsTrue (errorThrown, "#E04");
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
			Assert.IsTrue (errorThrown, "#E05");
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
			Assert.IsTrue (errorThrown, "#E06");
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
			Assert.IsTrue (errorThrown, "#E07");
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
			Assert.IsTrue (errorThrown, "#E08");
		}

		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, copy, 4);
		for (int i = 0; i < orig.Length; i++) {
			Assert.AreEqual (orig[i], copy[i], "#E09(" + i + ")");
		}
		Array.Clear(copy, 0, copy.Length);
		for (int i = 0; i < orig.Length; i++) {
			Assert.AreEqual ((char)0, copy[i], "#E10(" + i + ")");
		}
		Array.Copy(orig, copy, 2);
		Assert.AreEqual (orig[0], copy[0], "#E11");
		Assert.AreEqual (orig[1], copy[1], "#E12");
		Assert.IsTrue (orig[2] != copy[2], "#E13");
		Assert.IsTrue (orig[3] != copy[3], "#E14");
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
			Assert.IsTrue (errorThrown, "#E31");
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
			Assert.IsTrue (errorThrown, "#E32");
		}
		
		char[] orig = {'a', 'b', 'd', 'a'};
		char[] copy = new Char[4];
		Array.Copy(orig, 1, copy, 1, 3);
		Assert.IsTrue (copy[0] != orig[0], "#E33");
		for (int i = 1; i < orig.Length; i++) {
			Assert.AreEqual (orig[i], copy[i], "#E34(" + i + ")");
		}
		Array.Clear(copy, 0, copy.Length);
		Array.Copy(orig, 1, copy, 0, 2);
		Assert.AreEqual (orig[1], copy[0], "#E35");
		Assert.AreEqual (orig[2], copy[1], "#E36");
		Assert.IsTrue (copy[2] != orig[2], "#E37");
		Assert.IsTrue (copy[3] != orig[3], "#E38");
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
			Assert.IsTrue (errorThrown, "#E61");
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
			Assert.IsTrue (errorThrown, "#E62");
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
			Assert.IsTrue (errorThrown, "#E63");
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
			Assert.IsTrue (errorThrown, "#E64");
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
			Assert.IsTrue (errorThrown, "#E65");
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
			Assert.IsTrue (errorThrown, "#E66");
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
			Assert.IsTrue (errorThrown, "#E67");
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
			Assert.IsTrue (errorThrown, "#E68");
		}

		Char[] orig = {'a', 'b', 'c', 'd'};
		Char[] copy = new Char[10];
		Array.Clear(copy, 0, copy.Length);
		orig.CopyTo(copy, 3);
		Assert.AreEqual ((char)0, copy[0], "#E69");
		Assert.AreEqual ((char)0, copy[1], "#E70");
		Assert.AreEqual ((char)0, copy[2], "#E71");
		Assert.AreEqual (orig[0], copy[3], "#E72");
		Assert.AreEqual (orig[1], copy[4], "#E73");
		Assert.AreEqual (orig[2], copy[5], "#E74");
		Assert.AreEqual (orig[3], copy[6], "#E75");
		Assert.AreEqual ((char)0, copy[7], "#E76");
		Assert.AreEqual ((char)0, copy[8], "#E77");
		Assert.AreEqual ((char)0, copy[9], "#E78");

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
			Assert.IsTrue (!errorThrown, "#E79");
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
			Assert.IsTrue (errorThrown, "#E80");
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
			Assert.IsTrue (errorThrown, "#F01");
		}
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), -3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#F02");
		}
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), (int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#F03a");
		}

		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), (int [])null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#F03b");
		}
#if !TARGET_JVM // Arrays lower bounds are not supported for TARGET_JVM
		{
			bool errorThrown = false;
			try {
				Array.CreateInstance(Type.GetType("System.Char"), null, null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#F04");
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
			Assert.IsTrue (errorThrown, "#F05");
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
			Assert.IsTrue (errorThrown, "#F06");
		}

		char[] c1 = (char[])Array.CreateInstance(Type.GetType("System.Char"), 12);
		Assert.AreEqual (12, c1.Length, "#F07");

		Array c2 = Array.CreateInstance(Type.GetType("System.Char"), 12, 5);
		Assert.AreEqual (2, c2.Rank, "#F08");
		Assert.AreEqual (60, c2.Length, "#F09");


		{
			int[] lengths = { 3 };
			int[] bounds = { 5 };
			int[] src = { 512, 718, 912 };
			Array array = Array.CreateInstance(typeof(int), lengths, bounds);

			Assert.AreEqual (3, array.Length, "#F10");
			Assert.AreEqual (5, array.GetLowerBound(0), "#F11");
			Assert.AreEqual (7, array.GetUpperBound(0), "#F12");

			src.CopyTo (array, 5);

			for (int i = 0; i < src.Length; i++)
				Assert.AreEqual (src[i], array.GetValue(i+5), "#F13(" + i + ")");
		}

		// Test that a 1 dimensional array with 0 lower bound is the
		// same as an szarray
		Type szarrayType = new int [10].GetType ();
		Assert.IsTrue (szarrayType == (Array.CreateInstance (typeof (int), new int[] {1}, new int[] {0})).GetType ());
		Assert.IsTrue (szarrayType != (Array.CreateInstance (typeof (int), new int[] {1}, new int[] {1})).GetType ());
#endif // TARGET_JVM
	}
	
	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestCreateInstance2 ()
	{
		Array.CreateInstance (typeof (Int32), (int[])null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestCreateInstance2b ()
	{
		Array.CreateInstance (typeof (Int32), (long[])null);
	}

	[Test]
	public void TestGetEnumerator() {
		String[] s1 = {"this", "is", "a", "test"};
		IEnumerator en = s1.GetEnumerator ();
		Assert.IsNotNull (en, "#G01");

		Assert.IsTrue (en.MoveNext (), "#G02");
		Assert.AreEqual ("this", en.Current, "#G03");
		Assert.IsTrue (en.MoveNext (), "#G04");
		Assert.AreEqual ("is", en.Current, "#G05");
		Assert.IsTrue (en.MoveNext (), "#G06");
		Assert.AreEqual ("a", en.Current, "#G07");
		Assert.IsTrue (en.MoveNext (), "#G08");
		Assert.AreEqual ("test", en.Current, "#G09");
		Assert.IsTrue (!en.MoveNext (), "#G10");

		en.Reset ();
		Assert.IsTrue (en.MoveNext (), "#G11");
		Assert.AreEqual ("this", en.Current, "#G12");

		// mutation does not invalidate array enumerator!
		s1.SetValue ("change", 1);
		Assert.IsTrue (en.MoveNext (), "#G13");
		Assert.AreEqual ("change", en.Current, "#G14");
	}

	[Test]
	public void TestGetEnumeratorMultipleDimension() {
		String[,] s1 = {{"this", "is"}, {"a", "test"}};
		IEnumerator en = s1.GetEnumerator ();
		Assert.IsNotNull (en, "#AA01");

		Assert.IsTrue (en.MoveNext (), "#AA02");
		Assert.AreEqual ("this", en.Current, "#AA03");
		Assert.IsTrue (en.MoveNext (), "#AA04");
		Assert.AreEqual ("is", en.Current, "#AA05");
		Assert.IsTrue (en.MoveNext (), "#AA06");
		Assert.AreEqual ("a", en.Current, "#AA07");
		Assert.IsTrue (en.MoveNext (), "#AA08");
		Assert.AreEqual ("test", en.Current, "#AA09");
		Assert.IsTrue (!en.MoveNext (), "#AA10");

		en.Reset ();
		Assert.IsTrue (en.MoveNext (), "#AA11");
		Assert.AreEqual ("this", en.Current, "#AA12");

		int[] idxs = {0,1};
		// mutation does not invalidate array enumerator!
		s1.SetValue ("change", idxs);
		Assert.IsTrue (en.MoveNext (), "#AA13");
		Assert.AreEqual ("change", en.Current, "#AA14");
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
		Assert.IsNotNull (en, "#AB01");

		// check the first couple of values
		Assert.IsTrue (en.MoveNext (), "#AB02");
		Assert.AreEqual ("23", en.Current, "#AB03");
		Assert.IsTrue (en.MoveNext (), "#AB04");
		Assert.AreEqual ("24", en.Current, "#AB05");

		// then check the last element's value
		string lastElement;
		do {  
			lastElement = (string)en.Current;
		} while (en.MoveNext());
		Assert.AreEqual ("47", lastElement, "#AB06");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Add () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Add ("can not");
			Assert.Fail ("IList.Add should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Assert.Fail ("IList.Add threw wrong exception type");
		}

		Assert.Fail ("IList.Add shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Insert () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Insert (0, "can not");
			Assert.Fail ("IList.Insert should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Assert.Fail ("IList.Insert threw wrong exception type");
		}

		Assert.Fail ("IList.Insert shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Remove () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).Remove ("can not");
			Assert.Fail ("IList.Remove should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Assert.Fail ("IList.Remove threw wrong exception type");
		}

		Assert.Fail ("IList.Remove shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_RemoveAt () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );
		try {
			((IList)myArray).RemoveAt (0);
			Assert.Fail ("IList.RemoveAt should throw");
		}
		catch (NotSupportedException) {
			return;
		}
		catch (Exception) {
			Assert.Fail ("IList.RemoveAt threw wrong exception type");
		}

		Assert.Fail ("IList.RemoveAt shouldn't get this far");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_Contains () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );

		try {
			bool b = ((IList)myArray).Contains ("23");
			Assert.Fail ("IList.Contains should throw with multi-dimensional arrays");
		}
		catch (RankException) {
			int[] iArr = new int[3] { 1, 2, 3};
			// check the first and last items
			Assert.IsTrue (((IList)iArr).Contains (1), "AC01");
			Assert.IsTrue (((IList)iArr).Contains (3), "AC02");

			// and one that is definately not there
			Assert.IsTrue (!((IList)iArr).Contains (42), "AC03");
			return;
		}

		Assert.Fail ("Should not get here");
	}

	[Test]
	[Category ("TargetJvmNotSupported")] // Arrays lower bounds are not supported for TARGET_JVM
	public void TestIList_IndexOf () {
		int[] myLengthsArray = new int[2] { 3, 5 };
		int[] myBoundsArray = new int[2] { 2, 3 };

		Array myArray=Array.CreateInstance ( typeof(String), myLengthsArray, myBoundsArray );

		try {
			bool b = ((IList)myArray).Contains ("23");
			Assert.Fail ("IList.Contains should throw with multi-dimensional arrays");
		}
		catch (RankException) {
			int[] iArr = new int[3] { 1, 2, 3};
			// check the first and last items
			Assert.AreEqual (0, ((IList)iArr).IndexOf (1), "AD01");
			Assert.AreEqual (2, ((IList)iArr).IndexOf (3), "AD02");

			// and one that is definately not there
			Assert.AreEqual (-1, ((IList)iArr).IndexOf (42), "AD03");
		}
		catch (Exception e) {
			Assert.Fail ("Unexpected exception: " + e.ToString());
		}

		// check that wierd case whem lowerbound is Int32.MinValue,
		// so that IndexOf() needs to return Int32.MaxValue when it cannot find the object
		int[] myLengthArray = new int[1] { 3 };
		int[] myBoundArray = new int[1] { Int32.MinValue };
		Array myExtremeArray=Array.CreateInstance ( typeof(String), myLengthArray, myBoundArray );
		Assert.AreEqual (Int32.MaxValue, ((IList)myExtremeArray).IndexOf (42), "AD04");

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
			Assert.IsTrue (errorThrown, "#H01");
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = {'a', 'b', 'c'};
				c1.GetLength(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#H02");
		}

		char[] c2 = new Char[5];
		Assert.AreEqual (5, c2.GetLength(0), "#H03");

		char[,] c3 = new Char[6,7];
		Assert.AreEqual (6, c3.GetLength(0), "#H04");
		Assert.AreEqual (7, c3.GetLength(1), "#H05");
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
			Assert.IsTrue (errorThrown, "#H31");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetLowerBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#H32");
		}

		char[] c1 = new Char[5];
		Assert.AreEqual (0, c1.GetLowerBound(0), "#H33");

		char[,] c2 = new Char[4,4];
		Assert.AreEqual (0, c2.GetLowerBound(0), "#H34");
		Assert.AreEqual (0, c2.GetLowerBound(1), "#H35");
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
			Assert.IsTrue (errorThrown, "#H61");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetUpperBound(1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#H62");
		}

		char[] c1 = new Char[5];
		Assert.AreEqual (4, c1.GetUpperBound(0), "#H63");

		char[,] c2 = new Char[4,6];
		Assert.AreEqual (3, c2.GetUpperBound(0), "#H64");
		Assert.AreEqual (5, c2.GetUpperBound(1), "#H65");
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
			Assert.IsTrue (errorThrown, "#I01");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(-1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I02");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.GetValue(4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I03");
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		for (int i = 0; i < c1.Length; i++) {
			Assert.AreEqual (c1[i], c1.GetValue(i), "#I04(" + i + ")");
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
			Assert.IsTrue (errorThrown, "#I21");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(-1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I22");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.GetValue(4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I23");
		}

		char[,] c1 = new Char[4,6];
		for (int i = 0; i < 24; i++) {
			int first = i / 6;
			int second = i % 6;
			c1[first,second] = (char)(((int)'a')+i);
		}
		for (int i = 0; i < c1.GetLength(0); i++) {
			for (int j = 0; j < c1.GetLength(1); j++) {
				Assert.AreEqual (c1[i, j], c1.GetValue(i, j), "#I24(" + i + "," + j + ")");
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
			Assert.IsTrue (errorThrown, "#I41");
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(-1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I42");
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.GetValue(4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#I43");
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
					Assert.AreEqual (c1[i, j, k], c1.GetValue(i, j, k), "#I44(" + i + "," + j + ")");
				}
			}
		}
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
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
			Assert.IsTrue (errorThrown, "#I61a");
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
			Assert.IsTrue (errorThrown, "#I62");
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
			Assert.IsTrue (errorThrown, "#I63");
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
			Assert.IsTrue (errorThrown, "#I64");
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
				Assert.AreEqual (c1[i, j], c1.GetValue(coords), "#I65(" + i + "," + j + ")");
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
			Assert.IsTrue (errorThrown, "#J01");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J02");
		}

		String[] s1 = {"this", "is", "a", "test"};
		Assert.AreEqual (-1, Array.IndexOf(s1, null), "#J03");
		Assert.AreEqual (-1, Array.IndexOf(s1, "nothing"), "#J04");
		Assert.AreEqual (0, Array.IndexOf(s1, "this"), "#J05");
		Assert.AreEqual (3, Array.IndexOf(s1, "test"), "#J06");
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
			Assert.IsTrue (errorThrown, "#J21");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J22");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J23");
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		Assert.AreEqual (-1, Array.IndexOf(s1, null, 1), "#J24");
		Assert.AreEqual (-1, Array.IndexOf(s1, "nothing", 1), "#J25");
		Assert.AreEqual (-1, Array.IndexOf(s1, "this", 1), "#J26");
		Assert.AreEqual (1, Array.IndexOf(s1, "is", 1), "#J27");
		Assert.AreEqual (4, Array.IndexOf(s1, "test", 1), "#J28");
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
			Assert.IsTrue (errorThrown, "#J41");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.IndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J42");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J43");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.IndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#J44");
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		Assert.AreEqual (-1, Array.IndexOf(s1, null, 1, 3), "#J45");
		Assert.AreEqual (-1, Array.IndexOf(s1, "nothing", 1, 3), "#J46");
		Assert.AreEqual (-1, Array.IndexOf(s1, "this", 1, 3), "#J47");
		Assert.AreEqual (1, Array.IndexOf(s1, "is", 1, 3), "#J48");
		Assert.AreEqual (-1, Array.IndexOf(s1, "test", 1, 3), "#J49");
		Assert.AreEqual (3, Array.IndexOf(s1, "a", 1, 3), "#J50");
	}
	
	[Test]
	public void TestIndexOf_CustomEqual ()
	{
		DataEqual[] test = new DataEqual [] { new DataEqual () };
		Assert.AreEqual (0, Array.IndexOf (test, "asdfas", 0));
		
		IList array = (IList)test;
		Assert.AreEqual (0, array.IndexOf ("asdfas"));
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
			Assert.IsTrue (errorThrown, "#K01");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?");
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K02");
		}

		String[] s1 = {"this", "is", "a", "a", "test"};
		Assert.AreEqual (-1, Array.LastIndexOf(s1, null), "#K03");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "nothing"), "#K04");
		Assert.AreEqual (0, Array.LastIndexOf(s1, "this"), "#K05");
		Assert.AreEqual (4, Array.LastIndexOf(s1, "test"), "#K06");
		Assert.AreEqual (3, Array.LastIndexOf(s1, "a"), "#K07");

		Assert.AreEqual (-1, Array.LastIndexOf (new String [0], "foo"));
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
			Assert.IsTrue (errorThrown, "#K21");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K22");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K23");
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		Assert.AreEqual (-1, Array.LastIndexOf(s1, null, 3), "#K24");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "nothing", 3), "#K25");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "test", 3), "#K26");
		Assert.AreEqual (3, Array.LastIndexOf(s1, "a", 3), "#K27");
		Assert.AreEqual (0, Array.LastIndexOf(s1, "this", 3), "#K28");
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
			Assert.IsTrue (errorThrown, "#K41");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.LastIndexOf(c, "huh?", 0, 1);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K42");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 3, 1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K43");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = new Char[2];
				Array.LastIndexOf(c, "huh?", 0, 5);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#K44");
		}

		String[] s1 = {"this", "is", "really", "a", "test"};
		Assert.AreEqual (-1, Array.LastIndexOf(s1, null, 3, 3), "#K45");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "nothing", 3, 3), "#K46");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "this", 3, 3), "#K47");
		Assert.AreEqual (1, Array.LastIndexOf(s1, "is", 3, 3), "#K48");
		Assert.AreEqual (-1, Array.LastIndexOf(s1, "test", 3, 3), "#K49");
		Assert.AreEqual (3, Array.LastIndexOf(s1, "a", 3, 3), "#K50");
	}

	[Test]
	public void TestLastIndexOf4 ()
	{
		short [] a = new short [] { 19, 238, 317, 6, 565, 0, -52, 60, -563, 753, 238, 238};
		try {
			Array.LastIndexOf (a, (object)16, -1);
			NUnit.Framework.Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException) { }
		
		try {
			Array.LastIndexOf<short> (a, 16, -1);
			NUnit.Framework.Assert.Fail ("#2");
		} catch (ArgumentOutOfRangeException) { }
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
		Assert.IsTrue (!error);
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
		Assert.IsTrue (idx == -1, "#01");
		idx = Array.LastIndexOf (array, (object) null, -1, 10);
		Assert.IsTrue (idx == -1, "#02");
		idx = Array.LastIndexOf (array, (object) null, -100, 10);
		Assert.IsTrue (idx == -1, "#02");

		array = Array.CreateInstance (typeof (char), 1);
		try {
			Array.LastIndexOf (array, (object) null, -1, 0);
			Assert.Fail ("#04");
		} catch (ArgumentOutOfRangeException e) {
		}
		try {
			Array.LastIndexOf (array, (object) null, -1, 10);
			Assert.Fail ("#05");
		} catch (ArgumentOutOfRangeException e) {
		}
		try {
			Array.LastIndexOf (array, (object) null, -100, 10);
			Assert.Fail ("#06");
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
			Assert.IsTrue (errorThrown, "#L01");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#L02");
		}
		
		char[] c1 = {'a', 'b', 'c', 'd'};
		Array.Reverse(c1);
		Assert.AreEqual ('d', c1[0], "#L03");
		Assert.AreEqual ('c', c1[1], "#L04");
		Assert.AreEqual ('b', c1[2], "#L05");
		Assert.AreEqual ('a', c1[3], "#L06");

		{
			bool errorThrown = false;
			try {
				Array.Reverse(null, 0, 0);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#L07");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				Array.Reverse(c, 0, 0);
			} catch (RankException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#L08");
		}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 0, 3);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert.IsTrue (errorThrown, "#L09");
		//}
		//{
		//bool errorThrown = false;
		//try {
		//	char[] c = new Char[2];
		//	Array.Reverse(c, 3, 0);
		//} catch (ArgumentOutOfRangeException) {
		//	errorThrown = true;
		//}
		//Assert.IsTrue (errorThrown, "#L10");
		//}

		char[] c2 = { 'a', 'b', 'c', 'd'};
		Array.Reverse(c2, 1, 2);
		Assert.AreEqual ('a', c2[0], "#L11");
		Assert.AreEqual ('c', c2[1], "#L12");
		Assert.AreEqual ('b', c2[2], "#L13");
		Assert.AreEqual ('d', c2[3], "#L14");
	}

	[Test]
	// #8904
	public void ReverseStruct () {
		BStruct[] c3 = new BStruct[2];
		c3 [0] = new BStruct () { i1 = 1, i2 = 2, i3 = 3 };
		c3 [1] = new BStruct () { i1 = 4, i2 = 5, i3 = 6 };
		Array.Reverse (c3);
		Assert.AreEqual (4, c3 [0].i1);
		Assert.AreEqual (5, c3 [0].i2);
		Assert.AreEqual (6, c3 [0].i3);
		Assert.AreEqual (1, c3 [1].i1);
		Assert.AreEqual (2, c3 [1].i2);
		Assert.AreEqual (3, c3 [1].i3);
	}

	struct BStruct {
		public int i1, i2, i3;
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
			Assert.IsTrue (errorThrown, "#M01");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", -1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M02");
		}
		{
			bool errorThrown = false;
			try {
				char[] c = {'a', 'b', 'c'};
				c.SetValue("buh", 4);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M03");
		}

		char[] c1 = {'a', 'b', 'c', 'd'};
		char[] c2 = new char[4];
		for (int i = 0; i < c1.Length; i++) {
			c2.SetValue(c1[i], i);
		}
		for (int i = 0; i < c1.Length; i++) {
			Assert.AreEqual (c1[i], c2[i], "#M04(" + i + ")");
		}

		int[] c3 = { 1, 2, 3 };
		long[] c4 = new long [3];

		for (int i = 0; i < c3.Length; i++)
			c4.SetValue (c3 [i], i);

		try {
			c3.CopyTo (c4, 0);
		} catch (Exception e) {
			Assert.Fail ("c3.CopyTo(): e=" + e);
		}
		for (int i = 0; i < c3.Length; i++)
			Assert.IsTrue (c3[i] == c4[i], "#M05(" + i + ")");

		Object[] c5 = new Object [3];
		long[] c6 = new long [3];

		try {
			c4.CopyTo (c5, 0);
		} catch (Exception e) {
			Assert.Fail ("c4.CopyTo(): e=" + e);
		}

		try {
			c5.CopyTo (c6, 0);
		} catch (Exception e) {
			Assert.Fail ("c5.CopyTo(): e=" + e);
		}
		// for (int i = 0; i < c5.Length; i++)
		// Assert.IsTrue (c5[i] == c6[i], "#M06(" + i + ")");
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
			Assert.IsTrue (errorThrown, "#M21");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", -1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M22");
		}
		{
			bool errorThrown = false;
			try {
				char[,] c = new Char[2,2];
				c.SetValue("buh", 4,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M23");
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
				Assert.AreEqual (c1[i, j], c2[i, j], "#M24(" + i + "," + j + ")");
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
			Assert.IsTrue (errorThrown, "#M41");
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", -1, 1, 1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M42");
		}
		{
			bool errorThrown = false;
			try {
				char[,,] c = new Char[2,2,2];
				c.SetValue("buh", 4,1,1);
			} catch (IndexOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#M43");
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
					Assert.AreEqual (c1[i, j, k], c2[i, j, k], "#M44(" + i + "," + j + " )");
				}
			}
		}
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
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
			Assert.IsTrue (errorThrown, "#M61a");
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
			Assert.IsTrue (errorThrown, "#M62");
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
			Assert.IsTrue (errorThrown, "#M63");
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
			Assert.IsTrue (errorThrown, "#M64");
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
				Assert.AreEqual (c1[i, j], c2[i, j], "#M65(" + i + "," + j + ")");
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
				Assert.IsTrue (c1[i] == c2[i], "#M81(" + i + ")");
				Assert.AreEqual (typeof (long), c2[i].GetType (), "#M82(" + i + ")");
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
			Assert.IsTrue (errorThrown, "#M83");
		}
		{
			int[] c1 = { 1, 2, 3 };
			Object[] c2 = new Object [3];

			for (int i = 0; i < c1.Length; i++)
				c2.SetValue (c1 [i], i);

			for (int i = 0; i < c1.Length; i++)
				Assert.AreEqual (c1[i], Convert.ToInt32 (c2[i]), "#M84(" + i + ")");
		}
		{
			Object[] c1 = new Object [3];
			Object[] c2 = new Object [3];
			c1[0] = new Object ();

			for (int i = 0; i < c1.Length; i++)
				c2.SetValue (c1 [i], i);

			for (int i = 0; i < c1.Length; i++)
				Assert.AreEqual (c1[i], c2[i], "#M85(" + i + ")");
		}
		{
			Object[] c1 = new Object [3];
			string[] c2 = new String [3];
			string test = "hello";
			c1[0] = test;

			c2.SetValue (c1 [0], 0);
			Assert.AreEqual (c1[0], c2[0], "#M86");
			Assert.AreEqual ("hello", c2[0], "#M87");
		}
		{
			char[] c1 = { 'a', 'b', 'c' };
			string[] c2 = new string [3];
			try {
				c2.SetValue (c1 [0], 0);
				Assert.Fail ("#M88");
			} catch (InvalidCastException) {}
		}
		{
			Single[] c1 = { 1.2F, 2.3F, 3.4F, 4.5F };
			long[] c2 = new long [3];
			try {
				c2.SetValue (c1 [0], 0);
				Assert.Fail ("#M89");
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

					Assert.AreEqual (errorThrown, arg_ex [ex_index] == 1, "#M90(" + types [i] + "," + types [j] + ")");
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

				Assert.IsTrue (errorThrown, "#M91(" + types [i] + ")");
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

				Assert.IsTrue (errorThrown, "#M92(" + types [i] + ")");
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

				Assert.IsTrue (!errorThrown, "#M93(" + types [i] + ")");
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

					Assert.AreEqual (errorThrown, arg_ex [ex_index] == 1, "#M94(" + types [i] + "," + types [j] + ")");
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

				Assert.IsTrue (errorThrown, "#M95(" + types [i] + ")");
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

				Assert.IsTrue (errorThrown, "#M96(" + types [i] + ")");
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
			Assert.IsTrue (errorThrown, "#N01");
		}
		{
			bool errorThrown = false;
			try {
				Array.Sort(null, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#N02");
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#N03");
		}
		{
			bool errorThrown = false;
			try {
				char[] c1 = new Char[2];
				Array.Sort(null, c1, 0, 1);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "#N04");
		}
		{
			int tc = 5;
			char[] arr = {'d', 'b', 'f', 'e', 'a', 'c'};
			
			try {
				Array.Sort (null, 0, 1);
				Assert.Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Assert.Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, -1, 3);
				Assert.Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Assert.Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, 1, -3);
				Assert.Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Assert.Fail ("#N" + tc.ToString ()); }
			tc++;
			
			try {
				Array.Sort (arr, arr.Length, arr.Length + 2);
				Assert.Fail ("#N" + tc.ToString ());
			}
			catch (ArgumentException) {}
			catch (Exception) { Assert.Fail ("#N" + tc.ToString ()); }
		}
		
		// note: null second array => just sort first array
		char[] starter = {'d', 'b', 'f', 'e', 'a', 'c'};
		int[] starter1 = {1,2,3,4,5,6};
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1);
			Assert.AreEqual ('a', c1[0], "#N21");
			Assert.AreEqual ('b', c1[1], "#N22");
			Assert.AreEqual ('c', c1[2], "#N23");
			Assert.AreEqual ('d', c1[3], "#N24");
			Assert.AreEqual ('e', c1[4], "#N25");
			Assert.AreEqual ('f', c1[5], "#N26");
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1);
			Assert.AreEqual ('a', c1[0], "#N41");
			Assert.AreEqual ('b', c1[1], "#N42");
			Assert.AreEqual ('c', c1[2], "#N43");
			Assert.AreEqual ('d', c1[3], "#N44");
			Assert.AreEqual ('e', c1[4], "#N45");
			Assert.AreEqual ('f', c1[5], "#N46");
			Assert.AreEqual (5, i1[0], "#N47");
			Assert.AreEqual (2, i1[1], "#N48");
			Assert.AreEqual (6, i1[2], "#N49");
			Assert.AreEqual (1, i1[3], "#N50");
			Assert.AreEqual (4, i1[4], "#N51");
			Assert.AreEqual (3, i1[5], "#N52");
		}
		{
			char[] c1 = (char[])starter.Clone();
			Array.Sort(c1, 1, 4);
			Assert.AreEqual ('d', c1[0], "#N61");
			Assert.AreEqual ('a', c1[1], "#N62");
			Assert.AreEqual ('b', c1[2], "#N63");
			Assert.AreEqual ('e', c1[3], "#N64");
			Assert.AreEqual ('f', c1[4], "#N65");
			Assert.AreEqual ('c', c1[5], "#N66");
		}
		{
			char[] c1 = (char[])starter.Clone();
			int[] i1 = (int[])starter1.Clone();
			Array.Sort(c1, i1, 1, 4);
			Assert.AreEqual ('d', c1[0], "#N81");
			Assert.AreEqual ('a', c1[1], "#N82");
			Assert.AreEqual ('b', c1[2], "#N83");
			Assert.AreEqual ('e', c1[3], "#N84");
			Assert.AreEqual ('f', c1[4], "#N85");
			Assert.AreEqual ('c', c1[5], "#N86");
			Assert.AreEqual (1, i1[0], "#N87");
			Assert.AreEqual (5, i1[1], "#N88");
			Assert.AreEqual (2, i1[2], "#N89");
			Assert.AreEqual (4, i1[3], "#N90");
			Assert.AreEqual (3, i1[4], "#N91");
			Assert.AreEqual (6, i1[5], "#N92");
		}

		{
			// #648828
			double[] a = new double[115];
			int[] b = new int[256];
			Array.Sort<double, int> (a, b, 0, 115);
		}

		/* Check that ulong[] is not sorted as long[] */
		{
			string[] names = new string[] {
				"A", "B", "C", "D", "E"
			};

			ulong[] arr = new ulong [] {
				5,
				unchecked((ulong)0xffffFFFF00000000),
					0,
						0x7FFFFFFFffffffff,
						100
						};

			Array a = arr;
			Array.Sort (a, names, null);
			Assert.AreEqual (0, a.GetValue (0));
		}
	}

	[Test] // #616416
	public void SortNonGenericDoubleItems () {
            double[] doubleValues = new double[11];

			doubleValues[0] = 0.221788066253601;
			doubleValues[1] = 0.497278285809481;
			doubleValues[2] = 0.100565033883643;
			doubleValues[3] = 0.0433309347749905;
			doubleValues[4] = 0.00476726438463812;
			doubleValues[5] = 0.1354609735456;
			doubleValues[6] = 0.57690356588135;
			doubleValues[7] = 0.466239434334826;
			doubleValues[8] = 0.409741461978934;
			doubleValues[9] = 0.0112412763949565;
			doubleValues[10] = 0.668704347674307;

            int[] indices = new int[11];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 3;
            indices[4] = 4;
            indices[5] = 5;
            indices[6] = 6;
            indices[7] = 7;
            indices[8] = 8;
            indices[9] = 9;
            indices[10] = 10;

			Array.Sort ((Array)doubleValues, (Array)indices);
			Assert.AreEqual (4, indices [0]);
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
		Assert.IsTrue (!catched, "#TI01");
	}

	[Test]
	public void TestInitializeInt()
	{
		int[] a = {1,2,0};
		a.Initialize();
		int[] b = {1,2,0};
		for(int i=a.GetLowerBound(0);i<=a.GetUpperBound(0);i++)
		{
			Assert.AreEqual (a[i], b[i], "#TI02 " + i);
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
			Assert.AreEqual (a[i], b[i], "#TI03 " + i);
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
			Assert.AreEqual (a[i], b[i], "#TI04 " + i);
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
			Assert.AreEqual (a[i], b[i], "#TI05 " + i);
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
			Assert.AreEqual (a[i], b[i], "#TI06 " + i);
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
			Assert.AreEqual (a[i], b[i], "#TI07 " + i);
		}
	}
	
	[Test]
	public void TestInitializeIntNI()
	{
		int[] a = new int[20];
		a.Initialize();
		foreach(int b in a)
		{
			Assert.AreEqual (b, 0, "#TI08");
		}
	}
	
	[Test]
	public void TestInitializeCharNI()
	{
		char[] a = new char[20];
		a.Initialize();
		foreach(char b in a)
		{
			Assert.AreEqual (b, 0, "#TI09");
		}
	}
	
	[Test]
	public void TestInitializeDoubleNI()
	{
		double[] a = new double[20];
		a.Initialize();
		foreach(double b in a)
		{
			Assert.AreEqual (b, 0.0, "#TI09");
		}
	}
	
	[Test]
	public void TestInitializeStringNI()
	{
		string[] a = new string[20];
		a.Initialize();
		foreach(string b in a)
		{
			Assert.AreEqual (b, null, "#TI10");
		}
	}
	
	[Test]
	public void TestInitializeObjectNI()
	{
		object[] a = new object[20];
		a.Initialize();
		foreach(object b in a)
		{
			Assert.AreEqual (b, null, "#TI11");
		}
	}

	[Test]
	public void TestInitializeAClassNI()
	{
		AClass[] a = new AClass[20];
		a.Initialize();
		foreach(AClass b in a)
		{
			Assert.AreEqual (b, null, "#TI12");
		}
	}


	[Test]
	public void TestInitializeAStructNI()
	{
		AStruct[] a = new AStruct[20];
		a.Initialize();
		foreach(AStruct b in a)
		{
			Assert.AreEqual (b, new AStruct(), "#TI14");
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
			Assert.AreEqual (a[i], b[i], "#TI15 " + i);
		}
	}

	[Test]
	public void TestInitializeDateTimeNI()
	{
		DateTime[] a = new DateTime[20];
		a.Initialize();
		foreach(DateTime b in a)
		{
			Assert.AreEqual (b, new DateTime(), "#TI16");
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
		Assert.AreEqual (0.3, a [0], "#1");
		Assert.AreEqual (0.9, a [1], "#2");
		Assert.AreEqual (7, b [0], "#3");
		Assert.AreEqual (4, b [1], "#4");
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
			Assert.IsNull (matrix [i], i.ToString ());
		}
	}

	[Test]
	public void ClearMultidimentionalArray () 
	{
		byte[,] matrix = new byte [2,2] { {1, 1}, {2, 2} };
		Array.Clear (matrix, 0, 2);
		Assert.AreEqual (0, matrix [0, 0], "0,0");
		Assert.AreEqual (0, matrix [0, 1], "0,1");
		Assert.AreEqual (2, matrix [1, 0], "1,0");
		Assert.AreEqual (2, matrix [1, 1], "1,1");
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
		Assert.AreEqual (1, Array.BinarySearch (x, 'b'));
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
		Assert.AreEqual (- 1, Array.BinarySearch (array, 0), "BinarySearch");
	}

	[Test]
	public void BinarySearch2_EmptyList ()
	{
		int[] array = new int[0];
		Assert.AreEqual (-1, Array.BinarySearch (array, 0, 0, 0), "BinarySearch");
	}

	[Test]
	public void BinarySearch3_EmptyList ()
	{
		Comparer comparer = new Comparer ();
		int[] array = new int[0];
		Assert.AreEqual (-1, Array.BinarySearch (array, 0, comparer), "BinarySearch");
		// bug 77030 - the comparer isn't called for an empty array/list
		Assert.IsTrue (!comparer.Called, "Called");
	}

	[Test]
	public void BinarySearch4_EmptyList ()
	{
		Comparer comparer = new Comparer ();
		int[] array = new int[0];
		Assert.AreEqual (-1, Array.BinarySearch (array, 0, 0, comparer), "BinarySearch");
		// bug 77030 - the comparer isn't called for an empty array/list
		Assert.IsTrue (!comparer.Called, "Called");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void AsReadOnly_NullArray ()
	{
		Array.AsReadOnly <int> (null);
	}

	[Test]
	public void ReadOnly_Count ()
	{
		Assert.AreEqual (10, Array.AsReadOnly (new int [10]).Count);
	}

	[Test]
	public void ReadOnly_Contains ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		Assert.IsTrue (a.Contains (3));
		Assert.IsTrue (!a.Contains (6));
	}

	[Test]
	public void ReadOnly_IndexOf ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		Assert.AreEqual (0, a.IndexOf (3));
		Assert.AreEqual (1, a.IndexOf (5));
		Assert.AreEqual (-1, a.IndexOf (6));
	}

	[Test]
	public void ReadOnly_Indexer ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		Assert.AreEqual (3, a [0]);
		Assert.AreEqual (5, a [1]);

		/* Check that modifications to the original array are visible */
		arr [0] = 6;
		Assert.AreEqual (6, a [0]);
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

		Assert.AreEqual (45, sum);
	}

	[Test]
	public void ReadOnly_CopyTo ()
	{
		int[] arr = new int [2];
		arr [0] = 3;
		arr [1] = 5;
		IList<int> a = Array.AsReadOnly (arr);

		int[] arr2 = new int [3];
		a.CopyTo (arr2, 1);

		Assert.AreEqual (0, arr2 [0]);
		Assert.AreEqual (3, arr2 [1]);
		Assert.AreEqual (5, arr2 [2]);
	}

	[Test]
	public void Resize ()
	{
		int [] arr = new int [] { 1, 3, 5 };
		Array.Resize <int> (ref arr, 3);
		Assert.AreEqual (3, arr.Length, "#A1");
		Assert.AreEqual (1, arr [0], "#A2");
		Assert.AreEqual (3, arr [1], "#A3");
		Assert.AreEqual (5, arr [2], "#A4");

		Array.Resize <int> (ref arr, 2);
		Assert.AreEqual (2, arr.Length, "#B1");
		Assert.AreEqual (1, arr [0], "#B2");
		Assert.AreEqual (3, arr [1], "#B3");

		Array.Resize <int> (ref arr, 4);
		Assert.AreEqual (4, arr.Length, "#C1");
		Assert.AreEqual (1, arr [0], "#C2");
		Assert.AreEqual (3, arr [1], "#C3");
		Assert.AreEqual (0, arr [2], "#C4");
		Assert.AreEqual (0, arr [3], "#C5");
	}

	[Test]
	public void Resize_null ()
	{
		int [] arr = null;
		Array.Resize (ref arr, 10);
		Assert.AreEqual (arr.Length, 10);
	}

	[Test]
	public void Test_ContainsAndIndexOf_EquatableItem ()
	{
		EquatableClass[] list = new EquatableClass[] {new EquatableClass (0), new EquatableClass (1), new EquatableClass (0)};

		Assert.AreEqual (0, Array.IndexOf<EquatableClass> (list, list[0]), "#0");
		Assert.AreEqual (0, Array.IndexOf<EquatableClass> (list, new EquatableClass (0)), "#1");
		Assert.AreEqual (2, Array.LastIndexOf<EquatableClass> (list, list[0]), "#2");
		Assert.AreEqual (2, Array.LastIndexOf<EquatableClass> (list, new EquatableClass (0)), "#3");
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
		Assert.AreEqual (5, arr [0]);

		IList<FooStruct> arr2 = new FooStruct [10];
		FooStruct s = new FooStruct ();
		s.i = 11;
		s.j = 22;
		arr2 [5] = s;
		s = arr2 [5];
		Assert.AreEqual (11, s.i);
		Assert.AreEqual (22, s.j);

		IList<string> arr3 = new string [10];
		arr3 [5] = "ABC";
		Assert.AreEqual ("ABC", arr3 [5]);
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
		Assert.AreEqual (test.Contains (null), false, "list<o>");

		test = new object[] {};
		Assert.AreEqual (test.Contains (null), false, "empty array");

		test = new object[] {null};
		Assert.AreEqual (test.Contains (null), true, "array with null");

		test = new object[] { 1, null};
		Assert.IsTrue (test.Contains (null), "array with last null");
		
		test = new List<object>(test);
		Assert.AreEqual (test.Contains (null), true, "List<object> with test");
		
		test = new object[] {new object()};
		Assert.AreEqual (test.Contains (null), false, "array with object");

		test = new List<object>(test);
		Assert.AreEqual (test.Contains (null), false, "array with test");
	}
	
	[Test]
	public void IListNull ()
	{
		IList<object> test;
		
		test = new List<object>();
		Assert.AreEqual (-1, test.IndexOf (null), "list<o>");

		test = new object[] {};
		Assert.AreEqual (-1, test.IndexOf (null), "empty array");

		test = new object[] {null};
		Assert.AreEqual (0, test.IndexOf (null), "array with null");

		test = new object[] { 1, null};
		Assert.AreEqual (1, test.IndexOf (null), "array with last null");
		
		test = new List<object>(test);
		Assert.AreEqual (1, test.IndexOf (null), "List<object> with test");
		
		test = new object[] {new object()};
		Assert.AreEqual (-1, test.IndexOf (null), "array with object");

		test = new List<object>(test);
		Assert.AreEqual (-1, test.IndexOf (null), "array with test");
	}
	
#endif // TARGET_JVM

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

	[Test] // bug #322248
	public void IEnumerator_Reset ()
	{
		int[] array = new int[] { 1, 2, 3};
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();
		Assert.IsTrue (e.MoveNext (), "#A1");
		Assert.AreEqual (1, e.Current, "#A2");
		Assert.IsTrue (e.MoveNext (), "#A3");
		Assert.AreEqual (2, e.Current, "#A4");

		e.Reset ();

		Assert.IsTrue (e.MoveNext (), "#C1");
		Assert.AreEqual (1, e.Current, "#C2");
	}

	[Test]
	public void IEnumerator_Current_Finished ()
	{
		int[] array = new int[] { 1, 2, 3 };
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();
		Assert.IsTrue (e.MoveNext (), "#A1");
		Assert.AreEqual (1, e.Current, "#A2");
		Assert.IsTrue (e.MoveNext (), "#A3");
		Assert.AreEqual (2, e.Current, "#A4");
		Assert.IsTrue (e.MoveNext (), "#A5");
		Assert.AreEqual (3, e.Current, "#A6");
		Assert.IsTrue (!e.MoveNext (), "#A6");

		try {
			Assert.Fail ("#B1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration already finished
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void IEnumerator_Current_NotStarted ()
	{
		int[] array = new int[] { 1, 2, 3 };
		IEnumerator<int> e = ((IEnumerable<int>)array).GetEnumerator ();

		try {
			Assert.Fail ("#A1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration has not started. Call MoveNext
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
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
			Assert.Fail ("#B1:" + e.Current);
		} catch (InvalidOperationException ex) {
			// Enumeration has not started. Call MoveNext
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	public void ICollection_IsReadOnly() {
		ICollection<string> arr = new string [10];

		Assert.IsTrue (arr.IsReadOnly);
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void ArrayCreateInstanceOfVoid ()
	{
		Array.CreateInstance (typeof (void), 42);
	}

	class Foo<T> {}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void ArrayCreateInstanceOfOpenGenericType ()
	{
		Array.CreateInstance (typeof (Foo<>), 42);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void ClearNegativeLength ()
	{
		Array.Clear (new int [] { 1, 2 }, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void MultiDimension_IList_setItem ()
	{
		IList array = new int [1, 1];
		array [0] = 2;
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void MultiDimension_IList_getItem ()
	{
		IList array = new int [1, 1];
		int a = (int) array [0];
	}

	[Test]
	public void SetValue_Nullable () {
		Array array = Array.CreateInstance (typeof (int?), 7);

		object o = 42;

		array.SetValue (o, 0);
		Assert.AreEqual (42, array.GetValue (0));

		array.SetValue (null, 0);
		Assert.AreEqual (null, array.GetValue (0));
	}

	[Test]
	public void SortNullsWithGenericVersion ()
	{
            string[] s1 = new string[6]{
	        "J",
                "M",
                 null,
                "P",
                "T",
                "A"};

            string[] s2 = new string[]{null,
                "A",
                "J",
                "M",
                "P",
                "T"};

	    Array.Sort<string> (s1);
            for (int i = 0; i < 6; i++) {
		    Assert.AreEqual (s1[i], s2[i], "At:" + i);
            }
	}
	
	//
	// This is a test case for the case that was broken by the code contributed
	// for bug  #351638.
	//
	// This tests the fix for: #622101
	//
	[Test]
	public void SortActuallyWorks ()
	{
		string[] data = new string[9]{"Foo", "Bar", "Dingus", null, "Dingu4", "123", "Iam", null, "NotNull"};
		IComparer comparer = new NullAtEndComparer ();
		Array.Sort (data, comparer);

		Assert.AreEqual (data [7], null);
		Assert.AreNotEqual (data [0], null);
	}

	class NullAtEndComparer : IComparer {
		public int Compare(object x, object y)
		{
			if (x == null) return 1;
			if (y == null) return -1;
			return ((string)x).CompareTo((string)y);
		}
	}

	[Test] //bxc #11184
	public void UnalignedArrayClear ()
	{
		byte[] input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
		byte[] expected = new byte[] { 1, 2, 3, 4, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		Array.Clear (input, 5, 11);
		
		Assert.AreEqual (input, expected);
	}

#if NET_4_0
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithJaggedArray () {
		IStructuralComparable a = new int[][] { new int [] { 1,2 }, new int [] { 3,4 }};
		IStructuralComparable b = new int[][] { new int [] { 1,2 }, new int [] { 3,4 }};
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithArrayOfTheWrongKind () {
		IStructuralComparable a = new int[] { 1, 2 };
		IStructuralComparable b = new double[] { 1, 2 };
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithNonArrayType () {
		IStructuralComparable a = new int[] { 1, 2 };
		a.CompareTo (99, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithNonArrayOfDifferentSize () {
		IStructuralComparable a = new int[] { 1, 2 };
		IStructuralComparable b = new int[] { 1, 2, 3 };
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithMultiDimArray1 () {
		IStructuralComparable a = new int [2,2] { {10, 10 }, { 10, 10 } };
		IStructuralComparable b = new int [2,2] { {10, 10 }, { 10, 10 } };
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithMultiDimArray2 () {
		IStructuralComparable a = new int [2] { 10, 10 };
		IStructuralComparable b = new int [2,2] { {10, 10 }, { 10, 10 } };
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void CompareToWithMultiDimArray3 () {
		IStructuralComparable a = new int [4] { 10, 10, 10, 10 };
		IStructuralComparable b = new int [2,2] { {10, 10 }, { 10, 10 } };
		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void CompareToWithBoundedArray1 () {
		IStructuralComparable a = new int [2] { 10, 10 };
		Array ab = Array.CreateInstance (typeof (int), new int[] { 2 }, new int [] { 5 });
		IStructuralComparable b = ab;
		ab.SetValue (10, 5);
		ab.SetValue (10, 6);

		a.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (IndexOutOfRangeException))]
	public void CompareToWithBoundedArray2 () {
		IStructuralComparable a = new int [2] { 10, 10 };
		Array ab = Array.CreateInstance (typeof (int), new int[] { 2 }, new int [] { 5 });
		IStructuralComparable b = ab;
		ab.SetValue (10, 5);
		ab.SetValue (10, 6);

		//Yes, CompareTo simply doesn't work with bounded arrays!
		b.CompareTo (b, Comparer<object>.Default);
	}

	[Test]
	[ExpectedException (typeof (NullReferenceException))]
	public void CompareToWithNullComparer () {
		IStructuralComparable a = new int[] { 1, 2 };
		IStructuralComparable b = new int[] { 1, 2 };
		a.CompareTo (b, null);
	}

	[Test]
	public void CompareToWithNullArray () {
		IStructuralComparable a = new int[] { 1, 2 };
		Assert.AreEqual (1, a.CompareTo (null, Comparer<object>.Default));
	}

	[Test]
	public void CompareToWithGoodArrays () {
		IStructuralComparable a = new int[] { 10, 20 };
		Assert.AreEqual (0, a.CompareTo (a, Comparer<object>.Default));
		Assert.AreEqual (0, a.CompareTo (new int [] { 10, 20 }, Comparer<object>.Default));
		Assert.AreEqual (-1, a.CompareTo (new int [] { 11, 20 }, Comparer<object>.Default));
		Assert.AreEqual (-1, a.CompareTo (new int [] { 10, 21 }, Comparer<object>.Default));
		Assert.AreEqual (1, a.CompareTo (new int [] { 9, 20 }, Comparer<object>.Default));
		Assert.AreEqual (1, a.CompareTo (new int [] { 10, 19 }, Comparer<object>.Default));
	}

	[Test]
	public void IStructuralEquatable_Equals ()
	{
		IStructuralEquatable array = new int[] {1, 2, 3};
		IStructuralEquatable array2 = new int[] {1, 2, 3};
		Assert.AreEqual (false, array.Equals (null, null));
		Assert.AreEqual (true, array.Equals (array, null));
		Assert.AreEqual (true, array.Equals (array2, EqualityComparer<int>.Default));
	}

	[Test]
	[ExpectedException (typeof (NullReferenceException))]
	public void IStructuralEquatable_Equals_NoComparer ()
	{
		IStructuralEquatable array = new int[] {1, 2, 3};
		IStructuralComparable array2 = new int[] {1, 2, 3};
		array.Equals (array2, null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IStructuralEquatable_Equals_ComparerThrows ()
	{
		IStructuralEquatable array = new int[] {1, 2, 3};
		IStructuralComparable array2 = new int[] {1, 2, 3};
		array.Equals (array2, EqualityComparer<long>.Default);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]	
	public void IStructuralEquatable_GetHashCode_NullComparer ()
	{
		IStructuralEquatable a = new int[] { 1, 2 };
		a.GetHashCode (null);
	}

	class TestComparer_GetHashCode : IEqualityComparer
	{
		public int Counter;

		bool IEqualityComparer.Equals (object x, object y)
		{
			throw new NotImplementedException ();
		}

		public int GetHashCode (object obj)
		{
			return Counter++;
		}
	}

	[Test]
	public void IStructuralEquatable_GetHashCode ()
	{
		IStructuralEquatable a = new int[] { 1, 2, 9 };

		var c = new TestComparer_GetHashCode ();
		a.GetHashCode (c);
		Assert.AreEqual (3, c.Counter);		
	}

#endif

}
}
