// StringTest.cs - NUnit Test Cases for the System.String class
//
// Authors:
//   Jeffrey Stedfast <fejj@ximian.com>
//   David Brandt <bucky@keystreams.com>
//   Kornel Pal <http://www.kornelpal.hu/>
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2006 Kornel Pal
// Copyright (C) 2006 Novell (http://www.novell.com)
//

using System;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System
{

[TestFixture]
public class StringTest
{
	private CultureInfo orgCulture;

	[SetUp]
	public void SetUp ()
	{
		// save current culture
		orgCulture = CultureInfo.CurrentCulture;
	}

	[TearDown]
	public void TearDown ()
	{
		// restore original culture
		Thread.CurrentThread.CurrentCulture = orgCulture;
	}


#if !TARGET_JVM
	[Test] // ctor (Char [])
	public unsafe void Constructor2 ()
	{
		Assert.AreEqual (String.Empty, new String ((char[]) null), "#1");
		Assert.AreEqual (String.Empty, new String (new Char [0]), "#2");
		Assert.AreEqual ("A", new String (new Char [1] {'A'}), "#3");
	}
#endif

	[Test] // ctor (Char, Int32)
	public void Constructor4 ()
	{
		Assert.AreEqual (string.Empty, new String ('A', 0));
		Assert.AreEqual (new String ('A', 3), "AAA");
	}

	[Test] // ctor (Char, Int32)
	public void Constructor4_Count_Negative ()
	{
		try {
			new String ('A', -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// 'count' must be non-negative
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6 ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };
		Assert.AreEqual ("ABC", new String (arr, 0, arr.Length), "#1");
		Assert.AreEqual ("BC", new String (arr, 1, 2), "#2");
		Assert.AreEqual (string.Empty, new String (arr, 2, 0), "#3");
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Length_Negative ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, 0, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("length", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Length_Overflow ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, 1, 3);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_StartIndex_Negative ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, -1, 0);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Value_Null ()
	{
		try {
			new String ((char []) null, 0, 0);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

#if !TARGET_JVM
	[Test]
	public unsafe void CharPtrConstructor ()
	{
		Assert.AreEqual (String.Empty, new String ((char*) null), "char*");
		Assert.AreEqual (String.Empty, new String ((char*) null, 0, 0), "char*,int,int");
	}

	[Test]
	public unsafe void TestSbytePtrConstructorASCII ()
	{
		Encoding encoding = Encoding.ASCII;
		String s = "ASCII*\0";
		byte[] bytes = encoding.GetBytes (s);

		fixed (byte* bytePtr = bytes)
			Assert.AreEqual (s, new String ((sbyte*) bytePtr, 0, bytes.Length, encoding));
	}

	[Test]
	public unsafe void TestSbytePtrConstructorDefault ()
	{
		Encoding encoding = Encoding.Default;
		byte [] bytes = new byte [256];
		
		for (int i = 0; i < 255; i++)
			bytes [i] = (byte) (i + 1);
		bytes [255] = (byte) 0;

		// Ensure that bytes are valid for Encoding.Default
		bytes = encoding.GetBytes (encoding.GetChars (bytes));
		String s = encoding.GetString(bytes);

		// Ensure null terminated array
		bytes [bytes.Length - 1] = (byte) 0;

		fixed (byte* bytePtr = bytes) 
		{
			Assert.AreEqual (s.Substring (0, s.Length - 1), new String ((sbyte*) bytePtr));
			Assert.AreEqual (s, new String ((sbyte*) bytePtr, 0, bytes.Length));
			Assert.AreEqual (s, new String ((sbyte*) bytePtr, 0, bytes.Length, null));
			Assert.AreEqual (s, new String ((sbyte*) bytePtr, 0, bytes.Length, encoding));
		}
	}

	[Test] // ctor (SByte*)
	public unsafe void Constructor3_Value_Null ()
	{
		Assert.AreEqual (String.Empty, new String ((sbyte*) null));
	}

	[Test] // ctor (SByte*)
	[Category ("NotDotNet")] // this crashes nunit 2.4 and 2.6
	public unsafe void Constructor3_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1));
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("ptr", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Length_Negative ()
	{
		try {
			new String ((sbyte*) null, 0, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("length", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_StartIndex_Negative ()
	{
		try {
			new String ((sbyte*) null, -1, 0);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public unsafe void Constructor7_StartIndex_Overflow ()
	{
		try {
			new String ((sbyte*) (-1), 1, 0);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			new String ((sbyte*) (-1), 1, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1), 0, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("ptr", ex.ParamName, "#5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Value_Null ()
	{
#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 0);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("value", ex.ParamName, "#A5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 0), "#A");
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("value", ex.ParamName, "#B5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 1), "#B");
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 1, 0);
			Assert.Fail ("#C1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("value", ex.ParamName, "#C5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 1, 0), "#C");
#endif
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Length_Negative ()
	{
		try {
			new String ((sbyte*) null, 0, -1, null);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("length", ex.ParamName, "#A5");
		}

		try {
			new String ((sbyte*) null, 0, -1, Encoding.Default);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Non-negative number required
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("length", ex.ParamName, "#B5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_StartIndex_Negative ()
	{
		try {
			new String ((sbyte*) null, -1, 0, null);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			new String ((sbyte*) null, -1, 0, Encoding.Default);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}
	}

	[Test]
	public unsafe void Constructor8_StartIndex_Overflow ()
	{
		try {
			new String ((sbyte*) (-1), 1, 0, null);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			new String ((sbyte*) (-1), 1, 1, null);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}

		try {
			new String ((sbyte*) (-1), 1, 0, Encoding.Default);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#C5");
		}

		try {
			new String ((sbyte*) (-1), 1, 1, Encoding.Default);
			Assert.Fail ("#D1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#D5");
		}
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1), 0, 1, null);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("ptr", ex.ParamName, "#5");
		}
	}

	[Test]
#if NET_2_0
	[Ignore ("Runtime throws NullReferenceException instead of AccessViolationException")]
	[ExpectedException (typeof (AccessViolationException))]
#else
	[ExpectedException (typeof (NullReferenceException))]
#endif
	public unsafe void Constructor8_Value_Invalid2 ()
	{
		new String ((sbyte*) (-1), 0, 1, Encoding.Default);
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Value_Null ()
	{
#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 0, null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("value", ex.ParamName, "#A5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 0, null), "#A");
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 1, null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("value", ex.ParamName, "#B5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 1, null), "#B");
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 1, 0, null);
			Assert.Fail ("#C1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("value", ex.ParamName, "#C5");
		}
#else
		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 1, 0, null), "#C");
#endif

		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 0, 0, Encoding.Default), "#D");

		try {
			new String ((sbyte*) null, 0, 1, Encoding.Default);
			Assert.Fail ("#E1");
#if NET_2_0
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#E2");
			Assert.IsNull (ex.InnerException, "#E3");
			Assert.IsNotNull (ex.Message, "#E4");
			//Assert.AreEqual ("value", ex.ParamName, "#E5");
		}
#else
		} catch (NullReferenceException ex) {
			Assert.AreEqual (typeof (NullReferenceException), ex.GetType (), "#E2");
			Assert.IsNull (ex.InnerException, "#E3");
			Assert.IsNotNull (ex.Message, "#E4");
		}
#endif

		Assert.AreEqual (String.Empty, new String ((sbyte*) null, 1, 0, Encoding.Default), "#F");
	}
#endif

	[Test]
	public void Length ()
	{
		string str = "test string";

		Assert.AreEqual (11, str.Length, "wrong length");
	}

	[Test]
	public void Clone ()
	{
		string s1 = "oRiGiNal";
		Assert.AreEqual (s1, s1.Clone (), "#A1");
		Assert.AreSame (s1, s1.Clone (), "#A2");

		string s2 = new DateTime (2000, 6, 3).ToString ();
		Assert.AreEqual (s2, s2.Clone (), "#B1");
		Assert.AreSame (s2, s2.Clone (), "#B2");
	}

	[Test] // bug #316666
	public void CompareNotWorking ()
	{
		Assert.AreEqual (String.Compare ("A", "a"), 1, "A03");
		Assert.AreEqual (String.Compare ("a", "A"), -1, "A04");
	}

	[Test]
	public void CompareNotWorking2 ()
	{
		string needle = "ab";
		string haystack = "abbcbacab";
		Assert.AreEqual (0, String.Compare(needle, 0, haystack, 0, 2, false), "basic substring check #9");
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert.AreEqual (-1, String.Compare(needle, 0, haystack, i, 2, false), "loop substring check #8/" + i);
			}
		}
	}

	[Test]
	public void Compare ()
	{
		string lesser = "abc";
		string medium = "abcd";
		string greater = "xyz";
		string caps = "ABC";

		Assert.AreEqual (0, String.Compare (null, null));
		Assert.AreEqual (1, String.Compare (lesser, null));

		Assert.IsTrue (String.Compare (lesser, greater) < 0);
		Assert.IsTrue (String.Compare (greater, lesser) > 0);
		Assert.IsTrue (String.Compare (lesser, lesser) == 0);
		Assert.IsTrue (String.Compare (lesser, medium) < 0);

		Assert.IsTrue (String.Compare (lesser, caps, true) == 0);
		Assert.IsTrue (String.Compare (lesser, caps, false) != 0);
		Assert.AreEqual (String.Compare ("a", "b"), -1, "A01");
		Assert.AreEqual (String.Compare ("b", "a"), 1, "A02");


		// TODO - test with CultureInfo

		string needle = "ab";
		string haystack = "abbcbacab";
		Assert.AreEqual (0, String.Compare(needle, 0, haystack, 0, 2), "basic substring check #1");
		Assert.AreEqual (-1, String.Compare(needle, 0, haystack, 0, 3), "basic substring check #2");
		Assert.AreEqual (0, String.Compare("ab", 0, "ab", 0, 2), "basic substring check #3");
		Assert.AreEqual (0, String.Compare("ab", 0, "ab", 0, 3), "basic substring check #4");
		Assert.AreEqual (0, String.Compare("abc", 0, "ab", 0, 2), "basic substring check #5");
		Assert.AreEqual (1, String.Compare("abc", 0, "ab", 0, 5), "basic substring check #6");
		Assert.AreEqual (-1, String.Compare("ab", 0, "abc", 0, 5), "basic substring check #7");

		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert.IsTrue (String.Compare(needle, 0, haystack, i, 2) != 0, "loop substring check #1/" + i);
				Assert.IsTrue (String.Compare(needle, 0, haystack, i, 3) != 0, "loop substring check #2/" + i);
			} else {
				Assert.AreEqual (0, String.Compare(needle, 0, haystack, i, 2), "loop substring check #3/" + i);
				Assert.AreEqual (0, String.Compare(needle, 0, haystack, i, 3), "loop substring check #4/" + i);
			}
		}

		needle = "AB";
		Assert.AreEqual (0, String.Compare(needle, 0, haystack, 0, 2, true), "basic substring check #8");
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert.IsTrue (String.Compare(needle, 0, haystack, i, 2, true) != 0, "loop substring check #5/" + i);
				Assert.IsTrue (String.Compare(needle, 0, haystack, i, 2, false) != 0, "loop substring check #6/" + i);
			} else {
				Assert.AreEqual (0, String.Compare(needle, 0, haystack, i, 2, true), "loop substring check #7/" + i);
			}
		}

		Assert.AreEqual (0, String.Compare (needle, 0, haystack, 0, 0), "Compare with 0 length");

		// TODO - extended format call with CultureInfo
	}

	[Test]
	public void CompareOrdinal ()
	{
		string lesser = "abc";
		string medium = "abcd";
		string greater = "xyz";

		Assert.AreEqual (0, String.CompareOrdinal (null, null));
		Assert.AreEqual (1, String.CompareOrdinal (lesser, null));

		Assert.IsTrue (String.CompareOrdinal (lesser, greater) < 0, "#1");
		Assert.IsTrue (String.CompareOrdinal (greater, lesser) > 0, "#2");
		Assert.IsTrue (String.CompareOrdinal (lesser, lesser) == 0, "#3");
		Assert.IsTrue (String.CompareOrdinal (lesser, medium) < 0, "#4");

		string needle = "ab";
		string haystack = "abbcbacab";
		Assert.AreEqual (0, String.CompareOrdinal(needle, 0, haystack, 0, 2), "basic substring check");
		Assert.AreEqual (-1, String.CompareOrdinal(needle, 0, haystack, 0, 3), "basic substring miss");
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert.IsTrue (String.CompareOrdinal(needle, 0, haystack, i, 2) != 0, "loop substring check " + i);
				Assert.IsTrue (String.CompareOrdinal(needle, 0, haystack, i, 3) != 0, "loop substring check " + i);
			} else {
				Assert.AreEqual (0, String.CompareOrdinal(needle, 0, haystack, i, 2), "loop substring check " + i);
				Assert.AreEqual (0, String.CompareOrdinal(needle, 0, haystack, i, 3), "loop substring check " + i);
			}
		}
	}

	[Test]
	public void CompareTo ()
	{
		string lower = "abc";
		string greater = "xyz";
		string lesser = "abc";
		
		Assert.IsTrue (lower.CompareTo (greater) < 0);
		Assert.IsTrue (lower.CompareTo (lower) == 0);
		Assert.IsTrue (greater.CompareTo (lesser) > 0);
	}
	
	class WeirdToString
	{
		public override string ToString ()
		{
			return null;
		}
	}

	[Test]
	public void Concat ()
	{
		string string1 = "string1";
		string string2 = "string2";
		string concat = "string1string2";

		Assert.IsTrue (String.Concat (string1, string2) == concat);
		
		Assert.AreEqual (string1, String.Concat (string1, null));
		Assert.AreEqual (string1, String.Concat (null, string1));
		Assert.AreEqual (string.Empty, String.Concat (null, null));
		
		WeirdToString wts = new WeirdToString ();
		Assert.AreEqual (string1, String.Concat (string1, wts));
		Assert.AreEqual (string1, String.Concat (wts, string1));
		Assert.AreEqual (string.Empty, String.Concat (wts, wts));
		string [] allstr = new string []{ string1, null, string2, concat };
		object [] allobj = new object []{ string1, null, string2, concat };
		string astr = String.Concat (allstr);
		Assert.AreEqual ("string1string2string1string2", astr);
		string ostr = String.Concat (allobj);
		Assert.AreEqual (astr, ostr);
	}

	[Test]
	public void Copy ()
	{
		string s1 = "original";
		string s2 = String.Copy(s1);
		Assert.AreEqual (s1, s2, "#1");
		Assert.IsTrue (!object.ReferenceEquals (s1, s2), "#2");
	}

	[Test]
	public void Copy_Str_Null ()
	{
		try {
			String.Copy ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("str", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo ()
	{
		string s1 = "original";
		char[] c1 = new char[s1.Length];
		string s2 = new String(c1);
		Assert.IsTrue (!s1.Equals(s2), "#1");
		for (int i = 0; i < s1.Length; i++) {
			s1.CopyTo(i, c1, i, 1);
		}
		s2 = new String(c1);
		Assert.AreEqual (s1, s2, "#2");
	}

	[Test]
	public void CopyTo_Count_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, 0, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_Count_Overflow ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, 0, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("sourceIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_Destination_Null ()
	{
		string s = "original";

		try {
			s.CopyTo (0, (char []) null, 0, s.Length);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("destination", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_DestinationIndex_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, -1, 4);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("destinationIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_DestinationIndex_Overflow ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, Int32.MaxValue, 4);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("destinationIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_SourceIndex_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (-1, dest, 0, 4);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("sourceIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CopyTo_SourceIndex_Overflow ()
	{
		char[] dest = new char [4];
		try {
			"Mono".CopyTo (Int32.MaxValue, dest, 0, 4);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("sourceIndex", ex.ParamName, "#5");
		}
	}

	[Test] // EndsWith (String)
	public void EndsWith1 ()
	{
		string s;

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");
		s = "AbC";

		Assert.IsTrue (s.EndsWith ("bC"), "#A1");
		Assert.IsTrue (!s.EndsWith ("bc"), "#A1");
		Assert.IsTrue (!s.EndsWith ("dc"), "#A2");
		Assert.IsTrue (!s.EndsWith ("LAbC"), "#A3");
		Assert.IsTrue (s.EndsWith (string.Empty), "#A4");
		Assert.IsTrue (!s.EndsWith ("Ab"), "#A5");
		Assert.IsTrue (!s.EndsWith ("Abc"), "#A6");
		Assert.IsTrue (s.EndsWith ("AbC"), "#A7");

		s = "Tai";

		Assert.IsTrue (s.EndsWith ("ai"), "#B1");
		Assert.IsTrue (!s.EndsWith ("AI"), "#B2");
		Assert.IsTrue (!s.EndsWith ("LTai"), "#B3");
		Assert.IsTrue (s.EndsWith (string.Empty), "#B4");
		Assert.IsTrue (!s.EndsWith ("Ta"), "#B5");
		Assert.IsTrue (!s.EndsWith ("tai"), "#B6");
		Assert.IsTrue (s.EndsWith ("Tai"), "#B7");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.IsTrue (s.EndsWith ("ai"), "#C1");
		Assert.IsTrue (!s.EndsWith ("AI"), "#C2");
		Assert.IsTrue (!s.EndsWith ("LTai"), "#C3");
		Assert.IsTrue (s.EndsWith (string.Empty), "#C4");
		Assert.IsTrue (!s.EndsWith ("Ta"), "#C5");
		Assert.IsTrue (!s.EndsWith ("tai"), "#C6");
		Assert.IsTrue (s.EndsWith ("Tai"), "#C7");
	}

	[Test] // EndsWith (String)
	public void EndsWith1_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

#if NET_2_0
	[Test] // EndsWith (String, StringComparison)
	public void EndsWith2_ComparisonType_Invalid ()
	{
		try {
			"ABC".EndsWith ("C", (StringComparison) 80);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("comparisonType", ex.ParamName, "#5");
		}
	}

	[Test] // EndsWith (String, StringComparison)
	public void EndsWith2_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null, StringComparison.CurrentCulture);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

	[Test] // EndsWith (String, Boolean, CultureInfo)
	public void EndsWith3 ()
	{
		string s;
		bool ignorecase;
		CultureInfo culture;

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");
		s = "AbC";
		culture = null;

		ignorecase = false;
		Assert.IsTrue (!s.EndsWith ("bc", ignorecase, culture), "#A1");
		Assert.IsTrue (!s.EndsWith ("dc", ignorecase, culture), "#A2");
		Assert.IsTrue (!s.EndsWith ("LAbC", ignorecase, culture), "#A3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#A4");
		Assert.IsTrue (!s.EndsWith ("Ab", ignorecase, culture), "#A5");
		Assert.IsTrue (!s.EndsWith ("Abc", ignorecase, culture), "#A6");
		Assert.IsTrue (s.EndsWith ("AbC", ignorecase, culture), "#A7");

		ignorecase = true;
		Assert.IsTrue (s.EndsWith ("bc", ignorecase, culture), "#B1");
		Assert.IsTrue (!s.EndsWith ("dc", ignorecase, culture), "#B2");
		Assert.IsTrue (!s.EndsWith ("LAbC", ignorecase, culture), "#B3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#B4");
		Assert.IsTrue (!s.EndsWith ("Ab", ignorecase, culture), "#B5");
		Assert.IsTrue (s.EndsWith ("Abc", ignorecase, culture), "#B6");
		Assert.IsTrue (s.EndsWith ("AbC", ignorecase, culture), "#B7");

		s = "Tai";
		culture = null;

		ignorecase = false;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#C1");
		Assert.IsTrue (!s.EndsWith ("AI", ignorecase, culture), "#C2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#C3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#C4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#C5");
		Assert.IsTrue (!s.EndsWith ("tai", ignorecase, culture), "#C6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#C7");

		ignorecase = true;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#D1");
		Assert.IsTrue (!s.EndsWith ("AI", ignorecase, culture), "#D2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#D3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#D4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#D5");
		Assert.IsTrue (s.EndsWith ("tai", ignorecase, culture), "#D6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#D7");

		s = "Tai";
		culture = new CultureInfo ("en-US");

		ignorecase = false;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#E1");
		Assert.IsTrue (!s.EndsWith ("AI", ignorecase, culture), "#E2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#E3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#E4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#E5");
		Assert.IsTrue (!s.EndsWith ("tai", ignorecase, culture), "#E6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#E7");

		ignorecase = true;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#F1");
		Assert.IsTrue (s.EndsWith ("AI", ignorecase, culture), "#F2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#F3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#F4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#F5");
		Assert.IsTrue (s.EndsWith ("tai", ignorecase, culture), "#F6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#F7");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		culture = null;

		ignorecase = false;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#G1");
		Assert.IsTrue (!s.EndsWith ("AI", ignorecase, culture), "#G2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#G3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#G4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#G5");
		Assert.IsTrue (!s.EndsWith ("tai", ignorecase, culture), "#G6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#G7");

		ignorecase = true;
		Assert.IsTrue (s.EndsWith ("ai", ignorecase, culture), "#H1");
		Assert.IsTrue (s.EndsWith ("AI", ignorecase, culture), "#H2");
		Assert.IsTrue (!s.EndsWith ("LTai", ignorecase, culture), "#H3");
		Assert.IsTrue (s.EndsWith (string.Empty, ignorecase, culture), "#H4");
		Assert.IsTrue (!s.EndsWith ("Ta", ignorecase, culture), "#H5");
		Assert.IsTrue (s.EndsWith ("tai", ignorecase, culture), "#H6");
		Assert.IsTrue (s.EndsWith ("Tai", ignorecase, culture), "#H7");
	}

	[Test] // EndsWith (String, Boolean, CultureInfo)
	public void EndsWith3_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null, true, null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}
#endif

	[Test]
	public void TestEquals ()
	{
		string s1 = "original";
		string yes = "original";
		object y = yes;
		string no = "copy";
		string s1s1 = s1 + s1;

		Assert.IsTrue (!s1.Equals (null), "No match for null");
		Assert.IsTrue (s1.Equals (y), "Should match object");
		Assert.IsTrue (s1.Equals (yes), "Should match");
		Assert.IsTrue (!s1.Equals (no), "Shouldn't match");

		Assert.IsTrue (String.Equals (null, null), "Static nulls should match");
		Assert.IsTrue (String.Equals (s1, yes), "Should match");
		Assert.IsTrue (!String.Equals (s1, no), "Shouldn't match");

		Assert.AreEqual (false, s1s1.Equals (y), "Equals (object)");
	}

	[Test]
	public void TestFormat ()
	{
		Assert.AreEqual (string.Empty, String.Format (string.Empty, 0), "Empty format string.");
		Assert.AreEqual ("100", String.Format ("{0}", 100), "Single argument.");
		Assert.AreEqual ("X   37X", String.Format ("X{0,5}X", 37), "Single argument, right justified.");
		Assert.AreEqual ("X37   X", String.Format ("X{0,-5}X", 37), "Single argument, left justified.");
		Assert.AreEqual ("  7d", String.Format ("{0, 4:x}", 125), "Whitespace in specifier");
		Assert.AreEqual ("The 3 wise men.", String.Format ("The {0} wise {1}.", 3, "men"), "Two arguments.");
		Assert.AreEqual ("do re me fa so.", String.Format ("{0} re {1} fa {2}.", "do", "me", "so"), "Three arguments.");
		Assert.AreEqual ("###00c0ffee#", String.Format ("###{0:x8}#", 0xc0ffee), "Formatted argument.");
		Assert.AreEqual ("#  033#", String.Format ("#{0,5:x3}#", 0x33), "Formatted argument, right justified.");
		Assert.AreEqual ("#033  #", String.Format ("#{0,-5:x3}#", 0x33), "Formatted argument, left justified.");
		Assert.AreEqual ("typedef struct _MonoObject { ... } MonoObject;", String.Format ("typedef struct _{0} {{ ... }} MonoObject;", "MonoObject"), "Escaped bracket");
		Assert.AreEqual ("Could not find file \"a/b\"", String.Format ("Could not find file \"{0}\"", "a/b"), "With Slash");
		Assert.AreEqual ("Could not find file \"a\\b\"", String.Format ("Could not find file \"{0}\"", "a\\b"), "With BackSlash");
	}

	[Test] // Format (String, Object)
	public void Format1_Format_Null ()
	{
		try {
			String.Format (null, 1);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("format", ex.ParamName, "#5");
		}
	}

	[Test] // Format (String, Object [])
	public void Format2_Format_Null ()
	{
		try {
			String.Format (null, new object [] { 2 });
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("format", ex.ParamName, "#5");
		}
	}

	[Test] // Format (String, Object [])
	public void Format2_Args_Null ()
	{
		try {
			String.Format ("text", (object []) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("args", ex.ParamName, "#5");
		}
	}

	[Test] // Format (IFormatProvider, String, Object [])
	public void Format3_Format_Null ()
	{
		try {
			String.Format (CultureInfo.InvariantCulture, null,
				new object [] { 3 });
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("format", ex.ParamName, "#5");
		}
	}

	[Test] // Format (IFormatProvider, String, Object [])
	public void Format3_Args_Null ()
	{
		try {
			String.Format (CultureInfo.InvariantCulture, "text",
				(object []) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("args", ex.ParamName, "#5");
		}
	}

	[Test] // Format (String, Object, Object)
	public void Format4_Format_Null ()
	{
		try {
			String.Format (null, 4, 5);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("format", ex.ParamName, "#5");
		}
	}

	[Test] // Format (String, Object, Object, Object)
	public void Format5_Format_Null ()
	{
		try {
			String.Format (null, 4, 5, 6);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("format", ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestGetEnumerator ()
	{
		string s1 = "original";
		char[] c1 = new char[s1.Length];
		string s2 = new String(c1);
		Assert.IsTrue (!s1.Equals(s2), "pre-enumerated string should not match");
		CharEnumerator en = s1.GetEnumerator();
		Assert.IsNotNull (en, "null enumerator");
		
		for (int i = 0; i < s1.Length; i++) {
			en.MoveNext();
			c1[i] = en.Current;
		}
		s2 = new String(c1);
		Assert.AreEqual (s1, s2, "enumerated string should match");
	}

	[Test]
	public void TestGetHashCode ()
	{
		string s1 = "original";
		// TODO - weak test, currently.  Just verifies determinicity.
		Assert.AreEqual (s1.GetHashCode(), s1.GetHashCode(), "same string, same hash code");
	}

	[Test]
	public void TestGetType ()
	{
		string s1 = "original";
		Assert.AreEqual ("System.String", s1.GetType().ToString(), "String type");
	}

	[Test]
	public void TestGetTypeCode ()
	{
		string s1 = "original";
		Assert.IsTrue (s1.GetTypeCode().Equals(TypeCode.String));
	}

	[Test]
	public void IndexOf ()
	{
		string s1 = "original";

		try {
			s1.IndexOf ('q', s1.Length + 1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			s1.IndexOf ('q', s1.Length + 1, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}

		try {
			s1.IndexOf ("huh", s1.Length + 1);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#C5");
		}

		Assert.AreEqual (1, s1.IndexOf('r'), "basic char index");
		Assert.AreEqual (2, s1.IndexOf('i'), "basic char index 2");
		Assert.AreEqual (-1, s1.IndexOf('q'), "basic char index - no");
		
		Assert.AreEqual (1, s1.IndexOf("rig"), "basic string index");
		Assert.AreEqual (2, s1.IndexOf("i"), "basic string index 2");
		Assert.AreEqual (0, string.Empty.IndexOf(string.Empty), "basic string index 3");
		Assert.AreEqual (0, "ABC".IndexOf(string.Empty), "basic string index 4");
		Assert.AreEqual (-1, s1.IndexOf("rag"), "basic string index - no");

		Assert.AreEqual (1, s1.IndexOf('r', 1), "stepped char index");
		Assert.AreEqual (2, s1.IndexOf('i', 1), "stepped char index 2");
		Assert.AreEqual (4, s1.IndexOf('i', 3), "stepped char index 3");
		Assert.AreEqual (-1, s1.IndexOf('i', 5), "stepped char index 4");
		Assert.AreEqual (-1, s1.IndexOf('l', s1.Length), "stepped char index 5");

		Assert.AreEqual (1, s1.IndexOf('r', 1, 1), "stepped limited char index");
		Assert.AreEqual (-1, s1.IndexOf('r', 0, 1), "stepped limited char index");
		Assert.AreEqual (2, s1.IndexOf('i', 1, 3), "stepped limited char index");
		Assert.AreEqual (4, s1.IndexOf('i', 3, 3), "stepped limited char index");
		Assert.AreEqual (-1, s1.IndexOf('i', 5, 3), "stepped limited char index");

		s1 = "original original";
		Assert.AreEqual (0, s1.IndexOf("original", 0), "stepped string index 1");
		Assert.AreEqual (9, s1.IndexOf("original", 1), "stepped string index 2");
		Assert.AreEqual (-1, s1.IndexOf("original", 10), "stepped string index 3");
		Assert.AreEqual (3, s1.IndexOf(string.Empty, 3), "stepped string index 4");
		Assert.AreEqual (1, s1.IndexOf("rig", 0, 5), "stepped limited string index 1");
		Assert.AreEqual (-1, s1.IndexOf("rig", 0, 3), "stepped limited string index 2");
		Assert.AreEqual (10, s1.IndexOf("rig", 2, 15), "stepped limited string index 3");
		Assert.AreEqual (-1, s1.IndexOf("rig", 2, 3), "stepped limited string index 4");
		Assert.AreEqual (2, s1.IndexOf(string.Empty, 2, 3), "stepped limited string index 5");
		
		string s2 = "QBitArray::bitarr_data"; 
		Assert.AreEqual (9, s2.IndexOf ("::"), "bug #62160");
	}

	[Test] // IndexOf (String)
	public void IndexOf2_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#5");
#else
			//Fixme: Does it really make sense to check for obsolete
			//       parameter names. Then case this in string.
			//Assert.AreEqual ("string2", ex.ParamName, "#5");
#endif
		}
	}

	[Test] // IndexOf (Char, Int32)
	public void IndexOf3 ()
	{
		string s = "testing123456";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual (-1, s.IndexOf ('a', s.Length), "#A1");
		Assert.AreEqual (-1, s.IndexOf ('6', s.Length), "#A2");
		Assert.AreEqual (-1, s.IndexOf ('t', s.Length), "#A3");
		Assert.AreEqual (-1, s.IndexOf ('T', s.Length), "#A4");
		Assert.AreEqual (-1, s.IndexOf ('i', s.Length), "#A5");
		Assert.AreEqual (-1, s.IndexOf ('I', s.Length), "#A6");
		Assert.AreEqual (-1, s.IndexOf ('q', s.Length), "#A7");
		Assert.AreEqual (-1, s.IndexOf ('3', s.Length), "#A8");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual (-1, s.IndexOf ('a', s.Length), "#B1");
		Assert.AreEqual (-1, s.IndexOf ('6', s.Length), "#B2");
		Assert.AreEqual (-1, s.IndexOf ('t', s.Length), "#B3");
		Assert.AreEqual (-1, s.IndexOf ('T', s.Length), "#B4");
		Assert.AreEqual (-1, s.IndexOf ('i', s.Length), "#B5");
		Assert.AreEqual (-1, s.IndexOf ('I', s.Length), "#B6");
		Assert.AreEqual (-1, s.IndexOf ('q', s.Length), "#B7");
		Assert.AreEqual (-1, s.IndexOf ('3', s.Length), "#B8");
	}

	[Test] // IndexOf (String, Int32)
	public void IndexOf4 ()
	{
		string s = "testing123456";

		Assert.AreEqual (-1, s.IndexOf ("IN", 3), "#1");
		Assert.AreEqual (4, s.IndexOf ("in", 3), "#2");
		Assert.AreEqual (-1, s.IndexOf ("in", 5), "#3");
		Assert.AreEqual (7, s.IndexOf ("1", 5), "#4");
		Assert.AreEqual (12, s.IndexOf ("6", 12), "#5");
		Assert.AreEqual (0, s.IndexOf ("testing123456", 0), "#6");
		Assert.AreEqual (-1, s.IndexOf ("testing123456", 1), "#7");
		Assert.AreEqual (5, s.IndexOf (string.Empty, 5), "#8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, 0), "#9");
	}

	[Test] // IndexOf (String, Int32)
	public void IndexOf4_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, 1);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#5");
#else
			Assert.AreEqual ("string2", ex.ParamName, "#5");
#endif
		}
	}

#if NET_2_0
	[Test] // IndexOf (String, StringComparison)
	public void IndexOf5 ()
	{
		string s = "testing123456";
		StringComparison comparison_type;

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		comparison_type = StringComparison.CurrentCulture;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#A1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#A2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#A3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#A4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#A5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#A6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#A7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#A8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#A9");

		comparison_type = StringComparison.CurrentCultureIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#B1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#B2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#B3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#B4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#B5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#B6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#B7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#B8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#B9");

		comparison_type = StringComparison.InvariantCulture;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#C1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#C2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#C3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#C4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#C5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#C6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#C7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#C8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#C9");

		comparison_type = StringComparison.InvariantCultureIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#D1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#D2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#D3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#D4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#D5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#D6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#D7");
		Assert.AreEqual (3, s.IndexOf ("TIN", comparison_type), "#D8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#D9");

		comparison_type = StringComparison.Ordinal;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#E1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#E2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#E3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#E4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#E5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#E6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#E7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#E8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#E9");

		comparison_type = StringComparison.OrdinalIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#F1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#F2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#F3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#F4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#F5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#F6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#F7");
		Assert.AreEqual (3, s.IndexOf ("TIN", comparison_type), "#F8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#F9");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		comparison_type = StringComparison.CurrentCulture;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#G1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#G2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#G3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#G4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#G5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#G6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#G7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#G8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#G9");

		comparison_type = StringComparison.CurrentCultureIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#H1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#H2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#H3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#H4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#H5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#H6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#H7");
		Assert.AreEqual (3, s.IndexOf ("TIN", comparison_type), "#H8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#H9");

		comparison_type = StringComparison.InvariantCulture;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#I1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#I2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#I3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#I4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#I5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#I6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#I7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#I8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#I9");

		comparison_type = StringComparison.InvariantCultureIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#J1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#J2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#J3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#J4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#J5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#J6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#J7");
		Assert.AreEqual (3, s.IndexOf ("TIN", comparison_type), "#J8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#J9");

		comparison_type = StringComparison.Ordinal;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#K1");
		Assert.AreEqual (-1, s.IndexOf ("NG", comparison_type), "#K2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#K3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#K4");
		Assert.AreEqual (-1, s.IndexOf ("T", comparison_type), "#K5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#K6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#K7");
		Assert.AreEqual (-1, s.IndexOf ("TIN", comparison_type), "#K8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#K9");

		comparison_type = StringComparison.OrdinalIgnoreCase;
		Assert.AreEqual (7, s.IndexOf ("123", comparison_type), "#L1");
		Assert.AreEqual (5, s.IndexOf ("NG", comparison_type), "#L2");
		Assert.AreEqual (-1, s.IndexOf ("nga", comparison_type), "#L3");
		Assert.AreEqual (0, s.IndexOf ("t", comparison_type), "#L4");
		Assert.AreEqual (0, s.IndexOf ("T", comparison_type), "#L5");
		Assert.AreEqual (12, s.IndexOf ("6", comparison_type), "#L6");
		Assert.AreEqual (3, s.IndexOf ("tin", comparison_type), "#L7");
		Assert.AreEqual (3, s.IndexOf ("TIN", comparison_type), "#L8");
		Assert.AreEqual (0, s.IndexOf (string.Empty, comparison_type), "#L9");

		Assert.AreEqual (0, string.Empty.IndexOf (string.Empty, comparison_type), "#M");
	}

	[Test] // IndexOf (String, StringComparison)
	public void IndexOf5_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, (StringComparison) Int32.MinValue);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("comparisonType", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, StringComparison)
	public void IndexOf5_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, StringComparison.Ordinal);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void IndexOfStringComparisonOrdinalRangeException1 ()
	{
		"Mono".IndexOf ("no", 5, StringComparison.Ordinal);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void IndexOfStringComparisonOrdinalRangeException2 ()
	{
		"Mono".IndexOf ("no", 1, 5, StringComparison.Ordinal);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void IndexOfStringComparisonOrdinalIgnoreCaseRangeException1 ()
	{
		"Mono".IndexOf ("no", 5, StringComparison.OrdinalIgnoreCase);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void IndexOfStringComparisonOrdinalIgnoreCaseRangeException2 ()
	{
		"Mono".IndexOf ("no", 1, 5, StringComparison.OrdinalIgnoreCase);
	}

	[Test]
	public void IndexOfStringComparisonCurrentCulture_Empty ()
	{
		Assert.AreEqual (1, "Mono".IndexOf ("", 1, StringComparison.CurrentCultureIgnoreCase));
	}

	[Test]
	public void IndexOfStringComparison ()
	{
		string text = "testing123456";
		string text2 = "123";
		string text3 = "NG";
		string text4 = "t";
		Assert.AreEqual (7, text.IndexOf (text2, StringComparison.Ordinal), "#1-1");
		Assert.AreEqual (5, text.IndexOf (text3, StringComparison.OrdinalIgnoreCase), "#2-1");

		Assert.AreEqual (7, text.IndexOf (text2, 0, StringComparison.Ordinal), "#1-2");
		Assert.AreEqual (5, text.IndexOf (text3, 0, StringComparison.OrdinalIgnoreCase), "#2-2");

		Assert.AreEqual (7, text.IndexOf (text2, 1, StringComparison.Ordinal), "#1-3");
		Assert.AreEqual (5, text.IndexOf (text3, 1, StringComparison.OrdinalIgnoreCase), "#2-3");

		Assert.AreEqual (7, text.IndexOf (text2, 6, StringComparison.Ordinal), "#1-4");
		Assert.AreEqual (-1, text.IndexOf (text3, 6, StringComparison.OrdinalIgnoreCase), "#2-4");

		Assert.AreEqual (7, text.IndexOf (text2, 7, 3, StringComparison.Ordinal), "#1-5");
		Assert.AreEqual (-1, text.IndexOf (text3, 7, 3, StringComparison.OrdinalIgnoreCase), "#2-5");

		Assert.AreEqual (-1, text.IndexOf (text2, 6, 0, StringComparison.Ordinal), "#1-6");
		Assert.AreEqual (-1, text.IndexOf (text3, 5, 0, StringComparison.OrdinalIgnoreCase), "#2-6");

		Assert.AreEqual (-1, text.IndexOf (text2, 7, 1, StringComparison.Ordinal), "#1-7");
		Assert.AreEqual (-1, text.IndexOf (text3, 5, 1, StringComparison.OrdinalIgnoreCase), "#2-7");

		Assert.AreEqual (0, text.IndexOf (text4, 0, StringComparison.Ordinal), "#3-1");
		Assert.AreEqual (0, text.IndexOf (text4, 0, StringComparison.OrdinalIgnoreCase), "#3-2");

		Assert.AreEqual (-1, text.IndexOf (text4, 13, StringComparison.Ordinal), "#4-1");
		Assert.AreEqual (-1, text.IndexOf (text4, 13, StringComparison.OrdinalIgnoreCase), "#4-2");

		Assert.AreEqual (-1, text.IndexOf (text4, 13, 0, StringComparison.Ordinal), "#4-1");
		Assert.AreEqual (-1, text.IndexOf (text4, 13, 0, StringComparison.OrdinalIgnoreCase), "#4-2");

		Assert.AreEqual (12, text.IndexOf ("6", 12, 1, StringComparison.Ordinal), "#5-1");
		Assert.AreEqual (12, text.IndexOf ("6", 12, 1, StringComparison.OrdinalIgnoreCase), "#5-2");
	}

	[Test]
	public void IndexOfStringComparisonOrdinal ()
	{
		string text = "testing123456";
		Assert.AreEqual (10, text.IndexOf ("456", StringComparison.Ordinal), "#1");
		Assert.AreEqual (-1, text.IndexOf ("4567", StringComparison.Ordinal), "#2");
		Assert.AreEqual (0, text.IndexOf ("te", StringComparison.Ordinal), "#3");
		Assert.AreEqual (2, text.IndexOf ("s", StringComparison.Ordinal), "#4");
		Assert.AreEqual (-1, text.IndexOf ("ates", StringComparison.Ordinal), "#5");
		Assert.AreEqual (-1, text.IndexOf ("S", StringComparison.Ordinal), "#6");
	}

	[Test]
	public void IndexOfStringComparisonOrdinalIgnoreCase ()
	{
		string text = "testing123456";
		Assert.AreEqual (10, text.IndexOf ("456", StringComparison.OrdinalIgnoreCase), "#1");
		Assert.AreEqual (-1, text.IndexOf ("4567", StringComparison.OrdinalIgnoreCase), "#2");
		Assert.AreEqual (0, text.IndexOf ("te", StringComparison.OrdinalIgnoreCase), "#3");
		Assert.AreEqual (2, text.IndexOf ("s", StringComparison.OrdinalIgnoreCase), "#4");
		Assert.AreEqual (-1, text.IndexOf ("ates", StringComparison.OrdinalIgnoreCase), "#5");
		Assert.AreEqual (2, text.IndexOf ("S", StringComparison.OrdinalIgnoreCase), "#6");
	}

	[Test]
	public void IndexOfOrdinalCountSmallerThanValueString ()
	{
		Assert.AreEqual (-1, "Test".IndexOf ("ST", 2, 1, StringComparison.Ordinal), "#1");
		Assert.AreEqual (-1, "Test".IndexOf ("ST", 2, 1, StringComparison.OrdinalIgnoreCase), "#2");
		Assert.AreEqual (-1, "Test".LastIndexOf ("ST", 2, 1, StringComparison.Ordinal), "#3");
		Assert.AreEqual (-1, "Test".LastIndexOf ("ST", 2, 1, StringComparison.OrdinalIgnoreCase), "#4");
	}
#endif

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_Count_Negative ()
	{
		try {
			"Mono".IndexOf ('o', 1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_Count_Overflow ()
	{
		try {
			"Mono".IndexOf ('o', 1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ('o', -1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_StartIndex_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ('o', s.Length + 1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7 ()
	{
		string s = "testing123456test";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual (-1, s.IndexOf ("123", 4, 5), "#A1");
		Assert.AreEqual (7, s.IndexOf ("123", 4, 6), "#A2");
		Assert.AreEqual (-1, s.IndexOf ("123", 5, 4), "#A3");
		Assert.AreEqual (7, s.IndexOf ("123", 5, 5), "#A4");
		Assert.AreEqual (7, s.IndexOf ("123", 0, s.Length), "#A5");
		Assert.AreEqual (-1, s.IndexOf ("123", s.Length, 0), "#A6");

		Assert.AreEqual (-1, s.IndexOf ("tin", 2, 3), "#B1");
		Assert.AreEqual (3, s.IndexOf ("tin", 3, 3), "#B2");
		Assert.AreEqual (-1, s.IndexOf ("tin", 2, 2), "#B3");
		Assert.AreEqual (-1, s.IndexOf ("tin", 1, 4), "#B4");
		Assert.AreEqual (3, s.IndexOf ("tin", 0, s.Length), "#B5");
		Assert.AreEqual (-1, s.IndexOf ("tin", s.Length, 0), "#B6");

		Assert.AreEqual (6, s.IndexOf ("g12", 4, 5), "#C1");
		Assert.AreEqual (-1, s.IndexOf ("g12", 5, 2), "#C2");
		Assert.AreEqual (-1, s.IndexOf ("g12", 5, 3), "#C3");
		Assert.AreEqual (6, s.IndexOf ("g12", 6, 4), "#C4");
		Assert.AreEqual (6, s.IndexOf ("g12", 0, s.Length), "#C5");
		Assert.AreEqual (-1, s.IndexOf ("g12", s.Length, 0), "#C6");

		Assert.AreEqual (1, s.IndexOf ("est", 0, 5), "#D1");
		Assert.AreEqual (-1, s.IndexOf ("est", 1, 2), "#D2");
		Assert.AreEqual (-1, s.IndexOf ("est", 2, 10), "#D3");
		Assert.AreEqual (14, s.IndexOf ("est", 7, 10), "#D4");
		Assert.AreEqual (1, s.IndexOf ("est", 0, s.Length), "#D5");
		Assert.AreEqual (-1, s.IndexOf ("est", s.Length, 0), "#D6");

		Assert.AreEqual (-1, s.IndexOf ("T", 0, s.Length), "#E1");
		Assert.AreEqual (4, s.IndexOf ("i", 0, s.Length), "#E2");
		Assert.AreEqual (-1, s.IndexOf ("I", 0, s.Length), "#E3");
		Assert.AreEqual (12, s.IndexOf ("6", 0, s.Length), "#E4");
		Assert.AreEqual (0, s.IndexOf ("testing123456", 0, s.Length), "#E5");
		Assert.AreEqual (-1, s.IndexOf ("testing1234567", 0, s.Length), "#E6");
		Assert.AreEqual (0, s.IndexOf (string.Empty, 0, 0), "#E7");
		Assert.AreEqual (4, s.IndexOf (string.Empty, 4, 3), "#E8");
		Assert.AreEqual (0, string.Empty.IndexOf (string.Empty, 0, 0), "#E9");
		Assert.AreEqual (-1, string.Empty.IndexOf ("abc", 0, 0), "#E10");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual (-1, s.IndexOf ("123", 4, 5), "#F1");
		Assert.AreEqual (7, s.IndexOf ("123", 4, 6), "#F2");
		Assert.AreEqual (-1, s.IndexOf ("123", 5, 4), "#F3");
		Assert.AreEqual (7, s.IndexOf ("123", 5, 5), "#F4");
		Assert.AreEqual (7, s.IndexOf ("123", 0, s.Length), "#F5");
		Assert.AreEqual (-1, s.IndexOf ("123", s.Length, 0), "#F6");

		Assert.AreEqual (-1, s.IndexOf ("tin", 2, 3), "#G1");
		Assert.AreEqual (3, s.IndexOf ("tin", 3, 3), "#G2");
		Assert.AreEqual (-1, s.IndexOf ("tin", 2, 2), "#G3");
		Assert.AreEqual (-1, s.IndexOf ("tin", 1, 4), "#G4");
		Assert.AreEqual (3, s.IndexOf ("tin", 0, s.Length), "#G5");
		Assert.AreEqual (-1, s.IndexOf ("tin", s.Length, 0), "#G6");

		Assert.AreEqual (6, s.IndexOf ("g12", 4, 5), "#H1");
		Assert.AreEqual (-1, s.IndexOf ("g12", 5, 2), "#H2");
		Assert.AreEqual (-1, s.IndexOf ("g12", 5, 3), "#H3");
		Assert.AreEqual (6, s.IndexOf ("g12", 6, 4), "#H4");
		Assert.AreEqual (6, s.IndexOf ("g12", 0, s.Length), "#H5");
		Assert.AreEqual (-1, s.IndexOf ("g12", s.Length, 0), "#H6");

		Assert.AreEqual (1, s.IndexOf ("est", 0, 5), "#I1");
		Assert.AreEqual (-1, s.IndexOf ("est", 1, 2), "#I2");
		Assert.AreEqual (-1, s.IndexOf ("est", 2, 10), "#I3");
		Assert.AreEqual (14, s.IndexOf ("est", 7, 10), "#I4");
		Assert.AreEqual (1, s.IndexOf ("est", 0, s.Length), "#I5");
		Assert.AreEqual (-1, s.IndexOf ("est", s.Length, 0), "#I6");

		Assert.AreEqual (-1, s.IndexOf ("T", 0, s.Length), "#J1");
		Assert.AreEqual (4, s.IndexOf ("i", 0, s.Length), "#J2");
		Assert.AreEqual (-1, s.IndexOf ("I", 0, s.Length), "#J3");
		Assert.AreEqual (12, s.IndexOf ("6", 0, s.Length), "#J4");
		Assert.AreEqual (0, s.IndexOf ("testing123456", 0, s.Length), "#J5");
		Assert.AreEqual (-1, s.IndexOf ("testing1234567", 0, s.Length), "#J6");
		Assert.AreEqual (0, s.IndexOf (string.Empty, 0, 0), "#J7");
		Assert.AreEqual (4, s.IndexOf (string.Empty, 4, 3), "#J8");
		Assert.AreEqual (0, string.Empty.IndexOf (string.Empty, 0, 0), "#J9");
		Assert.AreEqual (-1, string.Empty.IndexOf ("abc", 0, 0), "#J10");
	}

#if NET_2_0
       [Test]
       public void IndexOf7_Empty () {
		Assert.AreEqual (1, "FOO".IndexOf ("", 1, 2, StringComparison.Ordinal));
		Assert.AreEqual (1, "FOO".IndexOf ("", 1, 2, StringComparison.OrdinalIgnoreCase));
       }
#endif

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Count_Negative ()
	{
		try {
			"Mono".IndexOf ("no", 1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Count_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ("no", 1, s.Length);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("count", ex.ParamName, "#A5");
		}

		try {
			s.IndexOf ("no", 1, s.Length + 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("count", ex.ParamName, "#B5");
		}

		try {
			s.IndexOf ("no", 1, int.MaxValue);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
#if NET_2_0
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("count", ex.ParamName, "#C5");
#else
			// Index was out of range.  Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNotNull (ex.ParamName, "#C5");
			//Assert.AreEqual ("startIndex", ex.ParamName, "#C5");
#endif
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("no", -1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_StartIndex_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ("no", s.Length + 1, 1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
#if NET_2_0
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
#else
			Assert.IsNotNull (ex.ParamName, "#A5");
			//Assert.AreEqual ("count", ex.ParamName, "#A5");
#endif
		}

		try {
			s.IndexOf ("no", int.MaxValue, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, 0, 1);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#5");
#else
			Assert.AreEqual ("string2", ex.ParamName, "#5");
#endif
		}
	}

#if NET_2_0
	[Test] // IndexOf (String, Int32, StringComparison)
	public void IndexOf8_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, 1, (StringComparison) Int32.MinValue);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("comparisonType", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, StringComparison)
	public void IndexOf8_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("o", -1, StringComparison.Ordinal);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, 0, 1, (StringComparison) Int32.MinValue);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("comparisonType", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_Count_Negative ()
	{
		try {
			"Mono".IndexOf ("o", 1, -1, StringComparison.Ordinal);
			Assert.Fail ("#1");
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("o", -1, 0, StringComparison.Ordinal);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}
#endif

	[Test]
	public void IndexOfAny1 ()
	{
		string s = "abcdefghijklmd";
		char[] c;

		c = new char [] {'a', 'e', 'i', 'o', 'u'};
		Assert.AreEqual (0, s.IndexOfAny (c), "#1");
		c = new char [] { 'd', 'z' };
		Assert.AreEqual (3, s.IndexOfAny (c), "#1");
		c = new char [] { 'q', 'm', 'z' };
		Assert.AreEqual (12, s.IndexOfAny (c), "#2");
		c = new char [0];
		Assert.AreEqual (-1, s.IndexOfAny (c), "#3");

	}

	[Test] // IndexOfAny (Char [])
	public void IndexOfAny1_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2 ()
	{
		string s = "abcdefghijklmd";
		char [] c;

		c = new char [] { 'a', 'e', 'i', 'o', 'u' };
		Assert.AreEqual (0, s.IndexOfAny (c, 0), "#A1");
		Assert.AreEqual (4, s.IndexOfAny (c, 1), "#A1");
		Assert.AreEqual (-1, s.IndexOfAny (c, 9), "#A2");
		Assert.AreEqual (-1, s.IndexOfAny (c, s.Length), "#A3");

		c = new char [] { 'd', 'z' };
		Assert.AreEqual (3, s.IndexOfAny (c, 0), "#B1");
		Assert.AreEqual (3, s.IndexOfAny (c, 3), "#B2");
		Assert.AreEqual (13, s.IndexOfAny (c, 4), "#B3");
		Assert.AreEqual (13, s.IndexOfAny (c, 9), "#B4");
		Assert.AreEqual (-1, s.IndexOfAny (c, s.Length), "#B5");
		Assert.AreEqual (13, s.IndexOfAny (c, s.Length - 1), "#B6");

		c = new char [] { 'q', 'm', 'z' };
		Assert.AreEqual (12, s.IndexOfAny (c, 0), "#C1");
		Assert.AreEqual (12, s.IndexOfAny (c, 4), "#C2");
		Assert.AreEqual (12, s.IndexOfAny (c, 12), "#C3");
		Assert.AreEqual (-1, s.IndexOfAny (c, s.Length), "#C4");

		c = new char [0];
		Assert.AreEqual (-1, s.IndexOfAny (c, 0), "#D1");
		Assert.AreEqual (-1, s.IndexOfAny (c, 4), "#D2");
		Assert.AreEqual (-1, s.IndexOfAny (c, 9), "#D3");
		Assert.AreEqual (-1, s.IndexOfAny (c, s.Length), "#D4");
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null, 0);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2_StartIndex_Negative ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, -1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny2_StartIndex_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, s.Length + 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3 ()
	{
		string s = "abcdefghijklmd";
		char [] c;

		c = new char [] { 'a', 'e', 'i', 'o', 'u' };
		Assert.AreEqual (0, s.IndexOfAny (c, 0, 2), "#A1");
		Assert.AreEqual (-1, s.IndexOfAny (c, 1, 2), "#A2");
		Assert.AreEqual (-1, s.IndexOfAny (c, 1, 3), "#A3");
		Assert.AreEqual (4, s.IndexOfAny (c, 1, 4), "#A3");
		Assert.AreEqual (4, s.IndexOfAny (c, 1, s.Length - 1), "#A4");

		c = new char [] { 'd', 'z' };
		Assert.AreEqual (-1, s.IndexOfAny (c, 0, 2), "#B1");
		Assert.AreEqual (-1, s.IndexOfAny (c, 1, 2), "#B2");
		Assert.AreEqual (3, s.IndexOfAny (c, 1, 3), "#B3");
		Assert.AreEqual (3, s.IndexOfAny (c, 0, s.Length), "#B4");
		Assert.AreEqual (3, s.IndexOfAny (c, 1, s.Length - 1), "#B5");
		Assert.AreEqual (-1, s.IndexOfAny (c, s.Length, 0), "#B6");

		c = new char [] { 'q', 'm', 'z' };
		Assert.AreEqual (-1, s.IndexOfAny (c, 0, 10), "#C1");
		Assert.AreEqual (12, s.IndexOfAny (c, 10, 4), "#C2");
		Assert.AreEqual (-1, s.IndexOfAny (c, 1, 3), "#C3");
		Assert.AreEqual (12, s.IndexOfAny (c, 0, s.Length), "#C4");
		Assert.AreEqual (12, s.IndexOfAny (c, 1, s.Length - 1), "#C5");

		c = new char [0];
		Assert.AreEqual (-1, s.IndexOfAny (c, 0, 3), "#D1");
		Assert.AreEqual (-1, s.IndexOfAny (c, 4, 9), "#D2");
		Assert.AreEqual (-1, s.IndexOfAny (c, 9, 5), "#D3");
		Assert.AreEqual (-1, s.IndexOfAny (c, 13, 1), "#D4");
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null, 0, 0);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_Count_Negative ()
	{
		try {
			"Mono".IndexOfAny (new char [1] { 'o' }, 1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_Length_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, 1, s.Length);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOfAny (new char [1] { 'o' }, -1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_StartIndex_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'o' }, s.Length + 1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

#if NET_2_0
	[Test]
	public void Contains ()
	{
		Assert.IsTrue ("ABC".Contains (string.Empty));
		Assert.IsTrue ("ABC".Contains ("ABC"));
		Assert.IsTrue ("ABC".Contains ("AB"));
		Assert.IsTrue (!"ABC".Contains ("AD"));
		Assert.IsTrue (!"encyclop�dia".Contains("encyclopaedia"));
	}

	[Test]
	public void Contains_Value_Null ()
	{
		try {
			"ABC".Contains (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

	[Test]
	public void IsNullOrEmpty ()
	{
		Assert.IsTrue (String.IsNullOrEmpty (null));
		Assert.IsTrue (String.IsNullOrEmpty (String.Empty));
		Assert.IsTrue (String.IsNullOrEmpty (""));
		Assert.IsTrue (!String.IsNullOrEmpty ("A"));
		Assert.IsTrue (!String.IsNullOrEmpty (" "));
		Assert.IsTrue (!String.IsNullOrEmpty ("\t"));
		Assert.IsTrue (!String.IsNullOrEmpty ("\n"));
	}
#endif

	[Test]
	public void TestInsert ()
	{
		string s1 = "original";
		
		try {
			s1.Insert (0, null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("value", ex.ParamName, "#A5");
		}

		try {
			s1.Insert (s1.Length + 1, "Hi!");
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}

		Assert.AreEqual ("Hi!original", s1.Insert (0, "Hi!"), "#C1");
		Assert.AreEqual ("originalHi!", s1.Insert (s1.Length, "Hi!"), "#C2");
		Assert.AreEqual ("origHi!inal", s1.Insert (4, "Hi!"), "#C3");
	}

	[Test]
	public void Intern ()
	{
		string s1 = "original";
		Assert.AreSame (s1, String.Intern (s1), "#A1");
		Assert.AreSame (String.Intern(s1), String.Intern(s1), "#A2");

		string s2 = "originally";
		Assert.AreSame (s2, String.Intern (s2), "#B1");
		Assert.IsTrue (String.Intern(s1) != String.Intern(s2), "#B2");

		string s3 = new DateTime (2000, 3, 7).ToString ();
		Assert.IsNull (String.IsInterned (s3), "#C1");
		Assert.AreSame (s3, String.Intern (s3), "#C2");
		Assert.AreSame (s3, String.IsInterned (s3), "#C3");
		Assert.AreSame (s3, String.IsInterned (new DateTime (2000, 3, 7).ToString ()), "#C4");
		Assert.AreSame (s3, String.Intern (new DateTime (2000, 3, 7).ToString ()), "#C5");
	}

	[Test]
	public void Intern_Str_Null ()
	{
		try {
			String.Intern (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("str", ex.ParamName, "#5");
		}
	}

	[Test]
	public void IsInterned ()
	{
		Assert.IsNull (String.IsInterned (new DateTime (2000, 3, 6).ToString ()), "#1");
		string s1 = "original";
		Assert.AreSame (s1, String.IsInterned (s1), "#2");
	}

	[Test]
	public void IsInterned_Str_Null ()
	{
		try {
			String.IsInterned (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("str", ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestJoin ()
	{
		try {
			string s = String.Join(" ", null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("value", ex.ParamName, "#A5");
		}

		string[] chunks = {"this", "is", "a", "test"};
		Assert.AreEqual ("this is a test", String.Join(" ", chunks), "Basic join");
		Assert.AreEqual ("this.is.a.test", String.Join(".", chunks), "Basic join");

		Assert.AreEqual ("is a", String.Join(" ", chunks, 1, 2), "Subset join");
		Assert.AreEqual ("is.a", String.Join(".", chunks, 1, 2), "Subset join");
		Assert.AreEqual ("is a test", String.Join(" ", chunks, 1, 3), "Subset join");

		try {
			string s = String.Join(" ", chunks, 2, 3);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#C5");
		}
	}

	[Test]
	public void Join_SeparatorNull ()
	{
		string[] chunks = {"this", "is", "a", "test"};
		Assert.AreEqual ("thisisatest", String.Join (null, chunks), "SeparatorNull");
	}

	[Test]
	public void Join_ValuesNull ()
	{
		string[] chunks1 = {null, "is", "a", null};
		Assert.AreEqual (" is a ", String.Join (" ", chunks1), "SomeNull");

		string[] chunks2 = {null, "is", "a", null};
		Assert.AreEqual ("isa", String.Join (null, chunks2), "Some+Sep=Null");

		string[] chunks3 = {null, null, null, null};
		Assert.AreEqual ("   ", String.Join (" ", chunks3), "AllValuesNull");
	}

	[Test]
	public void Join_AllNull ()
	{
		string[] chunks = {null, null, null};
		Assert.AreEqual (string.Empty, String.Join (null, chunks), "AllNull");
	}

	[Test]
	public void Join_StartIndexNegative ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, -1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void Join_StartIndexOverflow ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, Int32.MaxValue, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void Join_LengthNegative ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, 1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test]
	public void Join_LengthOverflow ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, 1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOf ()
	{
		string s1 = "original";

		try {
			s1.LastIndexOf ('q', -1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			s1.LastIndexOf ('q', -1, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}

		try {
			s1.LastIndexOf ("huh", s1.Length + 1);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#C5");
		}

		try {
			int i = s1.LastIndexOf ("huh", s1.Length + 1, 3);
			Assert.Fail ("#D1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#D5");
		}

		try {
			s1.LastIndexOf (null);
			Assert.Fail ("#E1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#E2");
			Assert.IsNull (ex.InnerException, "#E3");
			Assert.IsNotNull (ex.Message, "#E4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#E5");
#else
			Assert.AreEqual ("string2", ex.ParamName, "#E5");
#endif
		}

		try {
			s1.LastIndexOf (null, 0);
			Assert.Fail ("#F1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#F2");
			Assert.IsNull (ex.InnerException, "#F3");
			Assert.IsNotNull (ex.Message, "#F4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#F5");
#else
			Assert.AreEqual ("string2", ex.ParamName, "#F5");
#endif
		}

		try {
			s1.LastIndexOf (null, 0, 1);
			Assert.Fail ("#G1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#G2");
			Assert.IsNull (ex.InnerException, "#G3");
			Assert.IsNotNull (ex.Message, "#G4");
#if NET_2_0
			Assert.AreEqual ("value", ex.ParamName, "#G5");
#else
			Assert.AreEqual ("string2", ex.ParamName, "#G5");
#endif
		}

		Assert.AreEqual (1, s1.LastIndexOf('r'), "basic char index");
		Assert.AreEqual (4, s1.LastIndexOf('i'), "basic char index");
		Assert.AreEqual (-1, s1.LastIndexOf('q'), "basic char index - no");

		Assert.AreEqual (7, s1.LastIndexOf(string.Empty), "basic string index");
		Assert.AreEqual (1, s1.LastIndexOf("rig"), "basic string index");
		Assert.AreEqual (4, s1.LastIndexOf("i"), "basic string index");
		Assert.AreEqual (-1, s1.LastIndexOf("rag"), "basic string index - no");

		Assert.AreEqual (1, s1.LastIndexOf('r', s1.Length-1), "stepped char index");
		Assert.AreEqual (4, s1.LastIndexOf('i', s1.Length-1), "stepped char index");
		Assert.AreEqual (2, s1.LastIndexOf('i', 3), "stepped char index");
		Assert.AreEqual (-1, s1.LastIndexOf('i', 1), "stepped char index");

		Assert.AreEqual (1, s1.LastIndexOf('r', 1, 1), "stepped limited char index");
		Assert.AreEqual (-1, s1.LastIndexOf('r', 0, 1), "stepped limited char index");
		Assert.AreEqual (4, s1.LastIndexOf('i', 6, 3), "stepped limited char index");
		Assert.AreEqual (2, s1.LastIndexOf('i', 3, 3), "stepped limited char index");
		Assert.AreEqual (-1, s1.LastIndexOf('i', 1, 2), "stepped limited char index");

		s1 = "original original";
		Assert.AreEqual (9, s1.LastIndexOf("original", s1.Length), "stepped string index #1");
		Assert.AreEqual (0, s1.LastIndexOf("original", s1.Length-2), "stepped string index #2");
		Assert.AreEqual (-1, s1.LastIndexOf("original", s1.Length-11), "stepped string index #3");
		Assert.AreEqual (-1, s1.LastIndexOf("translator", 2), "stepped string index #4");
		Assert.AreEqual (0, string.Empty.LastIndexOf(string.Empty, 0), "stepped string index #5");
#if !TARGET_JVM
		Assert.AreEqual (-1, string.Empty.LastIndexOf("A", -1), "stepped string index #6");
#endif
		Assert.AreEqual (10, s1.LastIndexOf("rig", s1.Length-1, 10), "stepped limited string index #1");
		Assert.AreEqual (-1, s1.LastIndexOf("rig", s1.Length, 3), "stepped limited string index #2");
		Assert.AreEqual (10, s1.LastIndexOf("rig", s1.Length-2, 15), "stepped limited string index #3");
		Assert.AreEqual (-1, s1.LastIndexOf("rig", s1.Length-2, 3), "stepped limited string index #4");
			     
		string s2 = "QBitArray::bitarr_data"; 
		Assert.AreEqual (9, s2.LastIndexOf ("::"), "bug #62160");

		string s3 = "test123";
		Assert.AreEqual (0, s3.LastIndexOf ("test123"), "bug #77412");

		Assert.AreEqual (1, "\u267B RT \u30FC".LastIndexOf ("\u267B RT "), "bug #605094");
	}

#if NET_2_0
	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void LastIndexOf_StringComparison ()
	{
		" ".LastIndexOf (string.Empty, 0, 1, (StringComparison)Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOfStringComparisonOrdinalRangeException1 ()
	{
		"Mono".LastIndexOf ("no", 5, StringComparison.Ordinal);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOfStringComparisonOrdinalRangeException2 () 
	{
		"Mono".LastIndexOf ("no", 1, 3, StringComparison.Ordinal);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOfStringComparisonOrdinalIgnoreCaseRangeException1 ()
	{
		"Mono".LastIndexOf ("no", 5, StringComparison.OrdinalIgnoreCase);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void LastIndexOfStringComparisonOrdinalIgnoreCaseRangeException2 ()
	{
		"Mono".LastIndexOf ("no", 1, 3, StringComparison.OrdinalIgnoreCase);
	}

	[Test]
	public void LastIndexOfStringComparison ()
	{
		string text = "testing123456";
		string text2 = "123";
		string text3 = "NG";
		string text4 = "t";
		Assert.AreEqual (7, text.LastIndexOf (text2, StringComparison.Ordinal), "#1-1");
		Assert.AreEqual (5, text.LastIndexOf (text3, StringComparison.OrdinalIgnoreCase), "#2-1");

		Assert.AreEqual (7, text.LastIndexOf (text2, 12, StringComparison.Ordinal), "#1-2");
		Assert.AreEqual (5, text.LastIndexOf (text3, 12, StringComparison.OrdinalIgnoreCase), "#2-2");

		Assert.AreEqual (-1, text.LastIndexOf (text2, 0, StringComparison.Ordinal), "#1-3");
		Assert.AreEqual (-1, text.LastIndexOf (text3, 0, StringComparison.OrdinalIgnoreCase), "#2-3");

		Assert.AreEqual (-1, text.LastIndexOf (text2, 6, StringComparison.Ordinal), "#1-4");
		Assert.AreEqual (5, text.LastIndexOf (text3, 6, StringComparison.OrdinalIgnoreCase), "#2-4");

		Assert.AreEqual (-1, text.LastIndexOf (text2, 7, 3, StringComparison.Ordinal), "#1-5");
		Assert.AreEqual (5, text.LastIndexOf (text3, 7, 3, StringComparison.OrdinalIgnoreCase), "#2-5");

		Assert.AreEqual (-1, text.LastIndexOf (text2, 6, 0, StringComparison.Ordinal), "#1-6");
		Assert.AreEqual (-1, text.LastIndexOf (text3, 5, 0, StringComparison.OrdinalIgnoreCase), "#2-6");

		Assert.AreEqual (-1, text.LastIndexOf (text2, 7, 1, StringComparison.Ordinal), "#1-7");
		Assert.AreEqual (-1, text.LastIndexOf (text3, 5, 1, StringComparison.OrdinalIgnoreCase), "#2-7");

		Assert.AreEqual (0, text.LastIndexOf (text4, 0, StringComparison.Ordinal), "#3-1");
		Assert.AreEqual (0, text.LastIndexOf (text4, 0, StringComparison.OrdinalIgnoreCase), "#3-2");

		Assert.AreEqual (3, text.LastIndexOf (text4, 13, StringComparison.Ordinal), "#4-1");
		Assert.AreEqual (3, text.LastIndexOf (text4, 13, StringComparison.OrdinalIgnoreCase), "#4-2");

		Assert.AreEqual (3, text.LastIndexOf (text4, 13, 14, StringComparison.Ordinal), "#4-1");
		Assert.AreEqual (3, text.LastIndexOf (text4, 13, 14, StringComparison.OrdinalIgnoreCase), "#4-2");

		Assert.AreEqual (0, text.LastIndexOf (text4, 1, 2, StringComparison.Ordinal), "#5-1");
		Assert.AreEqual (0, text.LastIndexOf (text4, 1, 2, StringComparison.OrdinalIgnoreCase), "#5-2");

		Assert.AreEqual (-1, "".LastIndexOf ("FOO", StringComparison.Ordinal));
		Assert.AreEqual (0, "".LastIndexOf ("", StringComparison.Ordinal));
	}

	[Test]
	public void LastIndexOfStringComparisonOrdinal ()
	{
		string text = "testing123456";
		Assert.AreEqual (10, text.LastIndexOf ("456", StringComparison.Ordinal), "#1");
		Assert.AreEqual (-1, text.LastIndexOf ("4567", StringComparison.Ordinal), "#2");
		Assert.AreEqual (0, text.LastIndexOf ("te", StringComparison.Ordinal), "#3");
		Assert.AreEqual (2, text.LastIndexOf ("s", StringComparison.Ordinal), "#4");
		Assert.AreEqual (-1, text.LastIndexOf ("ates", StringComparison.Ordinal), "#5");
		Assert.AreEqual (-1, text.LastIndexOf ("S", StringComparison.Ordinal), "#6");
	}

	[Test]
	public void LastIndexOfStringComparisonOrdinalIgnoreCase ()
	{
		string text = "testing123456";
		Assert.AreEqual (10, text.LastIndexOf ("456", StringComparison.OrdinalIgnoreCase), "#1");
		Assert.AreEqual (-1, text.LastIndexOf ("4567", StringComparison.OrdinalIgnoreCase), "#2");
		Assert.AreEqual (0, text.LastIndexOf ("te", StringComparison.OrdinalIgnoreCase), "#3");
		Assert.AreEqual (2, text.LastIndexOf ("s", StringComparison.OrdinalIgnoreCase), "#4");
		Assert.AreEqual (-1, text.LastIndexOf ("ates", StringComparison.OrdinalIgnoreCase), "#5");
		Assert.AreEqual (2, text.LastIndexOf ("S", StringComparison.OrdinalIgnoreCase), "#6");
	}
#endif

	[Test]
	public void LastIndexOf_Char_StartIndexStringLength ()
	{
		string s = "Mono";
		try {
			s.LastIndexOf ('n', s.Length, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
		// this works for string but not for a char
	}

	[Test]
	public void LastIndexOf_Char_StartIndexOverflow ()
	{
		try {
			"Mono".LastIndexOf ('o', Int32.MaxValue, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOf_Char_LengthOverflow ()
	{
		try {
			"Mono".LastIndexOf ('o', 1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOf_String_StartIndexStringLength ()
	{
		string s = "Mono";
		Assert.AreEqual (-1, s.LastIndexOf ("n", s.Length, 1));
		// this works for string but not for a char
	}

	[Test]
	public void LastIndexOf_String_StartIndexStringLength_Plus1 ()
	{
		string s = "Mono";
		try {
			s.LastIndexOf ("n", s.Length + 1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOf_String_StartIndexOverflow ()
	{
		try {
			"Mono".LastIndexOf ("no", Int32.MaxValue, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOf_String_LengthOverflow ()
	{
		try {
			"Mono".LastIndexOf ("no", 1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOfAny ()
	{
		string s1 = ".bcdefghijklm";

		try {
			s1.LastIndexOfAny (null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			s1.LastIndexOfAny (null, s1.Length);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}

		try {
			s1.LastIndexOfAny (null, s1.Length, 1);
			Assert.Fail ("#C1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNull (ex.ParamName, "#C5");
		}

		char[] c1 = {'a', 'e', 'i', 'o', 'u'};
		Assert.AreEqual (8, s1.LastIndexOfAny (c1), "#D1");
		Assert.AreEqual (4, s1.LastIndexOfAny (c1, 7), "#D2");
		Assert.AreEqual (-1, s1.LastIndexOfAny (c1, 3), "#D3");
		Assert.AreEqual (4, s1.LastIndexOfAny (c1, s1.Length - 6, 4), "#D4");
		Assert.AreEqual (-1, s1.LastIndexOfAny (c1, s1.Length - 6, 3), "#D5");

		try {
			s1.LastIndexOfAny (c1, -1);
			Assert.Fail ("#E1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#E2");
			Assert.IsNull (ex.InnerException, "#E3");
			Assert.IsNotNull (ex.Message, "#E4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#E5");
		}

		try {
			s1.LastIndexOfAny (c1, -1, 1);
			Assert.Fail ("#F1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#F2");
			Assert.IsNull (ex.InnerException, "#F3");
			Assert.IsNotNull (ex.Message, "#F4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#F5");
		}
	}

	[Test]
	public void LastIndexOfAny_Length_Overflow ()
	{
		try {
			"Mono".LastIndexOfAny (new char [1] { 'o' }, 1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test]
	public void LastIndexOfAny_StartIndex_Overflow ()
	{
		try {
			"Mono".LastIndexOfAny (new char [1] { 'o' }, Int32.MaxValue, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // PadLeft (Int32)
	public void PadLeft1 ()
	{
		string s1 = "Hi!";
		string result;

		result = s1.PadLeft (0);
		Assert.AreSame (s1, result, "#A");

		result = s1.PadLeft (s1.Length - 1);
		Assert.AreSame (s1, result, "#B");

		result = s1.PadLeft (s1.Length);
		Assert.AreEqual (s1, result, "#C1");
		Assert.IsTrue (!object.ReferenceEquals (s1, result), "#C2");

		result = s1.PadLeft (s1.Length + 1);
		Assert.AreEqual (" Hi!", result, "#D");
	}

	[Test] // PadLeft (Int32)
	public void PadLeft1_TotalWidth_Negative ()
	{
		try {
			"Mono".PadLeft (-1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("totalWidth", ex.ParamName, "#5");
		}
	}

	[Test] // PadRight (Int32)
	public void PadRight1 ()
	{
		string s1 = "Hi!";
		string result;

		result = s1.PadRight (0);
		Assert.AreSame (s1, result, "#A");

		result = s1.PadRight (s1.Length - 1);
		Assert.AreSame (s1, result, "#B");

		result = s1.PadRight (s1.Length);
		Assert.AreEqual (s1, result, "#C1");
		Assert.IsTrue (!object.ReferenceEquals (s1, result), "#C2");

		result = s1.PadRight (s1.Length + 1);
		Assert.AreEqual ("Hi! ", result, "#D");
	}

	[Test] // PadRight1 (Int32)
	public void PadRight1_TotalWidth_Negative ()
	{
		try {
			"Mono".PadRight (-1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("totalWidth", ex.ParamName, "#5");
		}
	}

	[Test]
	public void PadRight2 ()
	{
		Assert.AreEqual ("100000000000", "1".PadRight (12, '0'), "#1");
		Assert.AreEqual ("000000000000", "".PadRight (12, '0'), "#2");
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2 ()
	{
		string s1 = "original";

		try {
			s1.Remove (-1, 1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			s1.Remove (1,-1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("count", ex.ParamName, "#B5");
		}

		try {
			s1.Remove (s1.Length, s1.Length);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("count", ex.ParamName, "#C5");
		}

		Assert.AreEqual ("oinal", s1.Remove(1, 3), "#D1");
		Assert.AreEqual (s1, s1.Remove (0, 0), "#D2");
		Assert.IsTrue (!object.ReferenceEquals (s1, s1.Remove (0, 0)), "#D3");
		Assert.AreEqual ("riginal", s1.Remove (0, 1), "#D4");
		Assert.AreEqual ("origina", s1.Remove (7, 1), "#D5");
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2_Length_Overflow ()
	{
		try {
			"Mono".Remove (1, Int32.MaxValue);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2_StartIndex_Overflow ()
	{
		try {
			"Mono".Remove (Int32.MaxValue, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

#if NET_2_0
	[Test] // Remove (Int32)
	public void Remove1_StartIndex_Negative ()
	{
		try {
			"ABC".Remove (-1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // Remove (Int32)
	public void Remove1_StartIndex_Overflow ()
	{
		try {
			"ABC".Remove (3);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex must be less than length of string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // Remove (Int32)
	public void Remove1 ()
	{
		string s = "ABC";

		Assert.AreEqual ("AB", s.Remove (2), "#1");
		Assert.AreEqual (string.Empty, s.Remove (0), "#2");
		Assert.AreEqual ("A", s.Remove (1), "#3");
	}
#endif

	[Test]
	public void Replace()
	{
		string s1 = "original";

		Assert.AreEqual (s1, s1.Replace('q', 's'), "non-hit char");
		Assert.AreEqual ("oxiginal", s1.Replace('r', 'x'), "single char");
		Assert.AreEqual ("orxgxnal", s1.Replace('i', 'x'), "double char");

		bool errorThrown = false;
		try {
			string s = s1.Replace(null, "feh");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert.IsTrue (errorThrown, "should get null arg exception");

		Assert.AreEqual ("ornal", s1.Replace("igi", null), "replace as remove");
		Assert.AreEqual (s1, s1.Replace("spam", "eggs"), "non-hit string");
		Assert.AreEqual ("orirumal", s1.Replace("gin", "rum"), "single string");
		Assert.AreEqual ("oreigeinal", s1.Replace("i", "ei"), "double string");

		Assert.AreEqual ("ooriginal", s1.Replace("o", "oo"), "start");
		Assert.AreEqual ("originall", s1.Replace("l", "ll"), "end");

		Assert.AreEqual ("riginal", s1.Replace("o", string.Empty), "start empty");
		Assert.AreEqual ("origina", s1.Replace("l", string.Empty), "end empty");

		Assert.AreEqual ("original", s1.Replace("original2", "original3"), "replace bigger that original");

		Assert.AreEqual (":!:", "::".Replace ("::", ":!:"), "result longer");

		// Test overlapping matches (bug #54988)
		string s2 = "...aaaaaaa.bbbbbbbbb,............ccccccc.u...";
		Assert.AreEqual (s2.Replace("..", "."), "..aaaaaaa.bbbbbbbbb,......ccccccc.u..");

		// Test replacing null characters (bug #67395)
#if !TARGET_JVM //bug #7276
		Assert.AreEqual ("is this ok ?", "is \0 ok ?".Replace ("\0", "this"), "should not strip content after nullchar");
#endif
	}

	[Test]
	public void ReplaceStringBeginEndTest ()
	{
		string s1 = "original";

		Assert.AreEqual ("riginal", s1.Replace ("o", ""), "#1");
		Assert.AreEqual ("origina", s1.Replace ("l", ""), "#2");
		Assert.AreEqual ("ariginal", s1.Replace ("o", "a"), "#3");
		Assert.AreEqual ("originaa", s1.Replace ("l", "a"), "#4");
		Assert.AreEqual ("aariginal", s1.Replace ("o", "aa"), "#5");
		Assert.AreEqual ("originaaa", s1.Replace ("l", "aa"), "#6");
		Assert.AreEqual ("original", s1.Replace ("o", "o"), "#7");
		Assert.AreEqual ("original", s1.Replace ("l", "l"), "#8");
		Assert.AreEqual ("original", s1.Replace ("original", "original"), "#9");
		Assert.AreEqual ("", s1.Replace ("original", ""), "#10");
	}

	[Test]
	public void ReplaceStringBeginEndTestFallback ()
	{
		string prev = new String ('o', 300);
		string s1 = prev + "riginal";

		Assert.AreEqual ("riginal", s1.Replace ("o", ""), "#1");
		Assert.AreEqual (prev + "rigina", s1.Replace ("l", ""), "#2");
		Assert.AreEqual (new String ('a', 300) + "riginal", s1.Replace ("o", "a"), "#3");
		Assert.AreEqual (prev + "riginaa", s1.Replace ("l", "a"), "#4");
		Assert.AreEqual (new String ('a', 600) + "riginal", s1.Replace ("o", "aa"), "#5");
		Assert.AreEqual (prev + "riginaaa", s1.Replace ("l", "aa"), "#6");
		Assert.AreEqual (s1, s1.Replace ("o", "o"), "#7");
		Assert.AreEqual (s1, s1.Replace ("l", "l"), "#8");
		Assert.AreEqual (s1, s1.Replace (s1, s1), "#9");
		Assert.AreEqual ("", s1.Replace (prev + "riginal", ""), "#10");
	}

	[Test]
	public void ReplaceStringOffByOne ()
	{
		Assert.AreEqual ("", new String ('o', 199).Replace ("o", ""), "#-1");
		Assert.AreEqual ("", new String ('o', 200).Replace ("o", ""), "#0");
		Assert.AreEqual ("", new String ('o', 201).Replace ("o", ""), "#+1");
	}

	[Test]
	public void ReplaceStringCultureTests ()
	{
		// LAMESPEC: According to MSDN Replace with String parameter is culture-senstive.
		// However this does not currently seem to be the case. Otherwise following code should
		// produce "check" instead of "AE"

		CultureInfo old = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		Assert.AreEqual ("AE", "AE".Replace ("\u00C6", "check"), "#1");
		Thread.CurrentThread.CurrentCulture = old;
	}

	[Test] // StartsWith (String)
	public void StartsWith1 ()
	{
		string s1 = "original";
		
		Assert.IsTrue (s1.StartsWith ("o"), "#1");
		Assert.IsTrue (s1.StartsWith ("orig"), "#2");
		Assert.IsTrue (!s1.StartsWith ("rig"), "#3");
		Assert.IsTrue (s1.StartsWith (String.Empty), "#4");
		Assert.IsTrue (String.Empty.StartsWith (String.Empty), "#5");
		Assert.IsTrue (!String.Empty.StartsWith ("rig"), "#6");
	}

	[Test] // StartsWith (String)
	public void StartsWith1_Value_Null ()
	{
		try {
			"A".StartsWith (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

#if NET_2_0
	[Test] // StartsWith (String, StringComparison)
	public void StartsWith2_ComparisonType_Invalid ()
	{
		try {
			"ABC".StartsWith ("A", (StringComparison) 80);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("comparisonType", ex.ParamName, "#5");
		}
	}

	[Test] // StartsWith (String, StringComparison)
	public void StartsWith2_Value_Null ()
	{
		try {
			"A".StartsWith (null, StringComparison.CurrentCulture);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("value", ex.ParamName, "#5");
		}
	}

	[Test] // StartsWith (String, Boolean, CultureInfo)
	public void StartsWith3_Culture_Null ()
	{
		// This should not crash
		string s = "boo";

		s.StartsWith ("this", true, null);
	}
#endif

	[Test] // SubString (Int32)
	public void Substring1 ()
	{
		string s = "original";

		Assert.AreEqual ("inal", s.Substring (4), "#1");
		Assert.AreEqual (string.Empty, s.Substring (s.Length), "#2");
#if NET_2_0
		Assert.AreSame (s, s.Substring (0), "#3");
#else
		Assert.AreEqual (s, s.Substring (0), "#3a");
		Assert.IsTrue (!object.ReferenceEquals (s, s.Substring (0)), "#3b");
#endif
	}

	[Test] // SubString (Int32)
	public void SubString1_StartIndex_Negative ()
	{
		string s = "original";

		try {
			s.Substring (-1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // SubString (Int32)
	public void SubString1_StartIndex_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length + 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
#else
			Assert.AreEqual ("length", ex.ParamName, "#5");
#endif
		}
	}

	[Test] // SubString (Int32, Int32)
	public void Substring2 ()
	{
		string s = "original";

		Assert.AreEqual ("igin", s.Substring (2, 4), "#1");
		Assert.AreEqual (string.Empty, s.Substring (s.Length, 0), "#2");
		Assert.AreEqual ("origina", s.Substring (0, s.Length - 1), "#3");
		Assert.AreEqual (s, s.Substring (0, s.Length), "#4");
#if NET_2_0
		Assert.AreSame (s, s.Substring (0, s.Length), "#5");
#else
		Assert.IsTrue (!object.ReferenceEquals (s, s.Substring (0, s.Length)), "#5");
#endif
	}

	[Test] // SubString (Int32, Int32)
	public void SubString2_Length_Negative ()
	{
		string s = "original";

		try {
			s.Substring (1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("length", ex.ParamName, "#5");
		}
	}
	
	[Test] // SubString (Int32, Int32)
	public void Substring2_Length_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length, 1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("length", ex.ParamName, "#A5");
		}

		try {
			s.Substring (1, s.Length);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("length", ex.ParamName, "#B5");
		}

		try {
			s.Substring (1, Int32.MaxValue);
			Assert.Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("length", ex.ParamName, "#C5");
		}
	}

	[Test] // SubString (Int32, Int32)
	public void SubString2_StartIndex_Negative ()
	{
		string s = "original";

		try {
			s.Substring (-1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test] // SubString (Int32, Int32)
	public void Substring2_StartIndex_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length + 1, 0);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
#if NET_2_0
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
#else
			Assert.AreEqual ("length", ex.ParamName, "#A5");
#endif
		}

		try {
			"Mono".Substring (Int32.MaxValue, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
#if NET_2_0
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
#else
			Assert.AreEqual ("length", ex.ParamName, "#B5");
#endif
		}
	}

	[Test]
	public void ToCharArray ()
	{
		const string s = "original";
		char [] c;

		c = s.ToCharArray ();
		Assert.AreEqual (s.Length, c.Length, "#A1");
		Assert.AreEqual (s, new String (c), "#A2");

		c = s.ToCharArray (0, s.Length);
		Assert.AreEqual (s.Length, c.Length, "#B1");
		Assert.AreEqual (s, new String (c), "#B2");

		c = s.ToCharArray (1, s.Length - 1);
		Assert.AreEqual (7, c.Length, "#C1");
		Assert.AreEqual ("riginal", new String (c), "#C2");

		c = s.ToCharArray (0, 3);
		Assert.AreEqual (3, c.Length, "#D1");
		Assert.AreEqual ("ori", new String (c), "#D2");

		c = s.ToCharArray (2, 0);
		Assert.AreEqual (0, c.Length, "#E1");
		Assert.AreEqual (string.Empty, new String (c), "#E2");

		c = s.ToCharArray (3, 2);
		Assert.AreEqual (2, c.Length, "#F1");
		Assert.AreEqual ("gi", new String (c), "#F2");

		c = s.ToCharArray (s.Length, 0);
		Assert.AreEqual (0, c.Length, "#G1");
		Assert.AreEqual (string.Empty, new String (c), "#G2");
	}

	[Test]
	public void ToCharArray_Length_Negative ()
	{
		const string s = "original";

		try {
			s.ToCharArray (1, -1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("length", ex.ParamName, "#5");
		}
	}

	[Test]
	public void ToCharArray_Length_Overflow ()
	{
		const string s = "original";

		try {
			s.ToCharArray (1, s.Length);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			s.ToCharArray (1, Int32.MaxValue);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}
	}

	[Test]
	public void ToCharArray_StartIndex_Negative ()
	{
		const string s = "original";

		try {
			s.ToCharArray (-1, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void ToCharArray_StartIndex_Overflow ()
	{
		const string s = "original";

		try {
			s.ToCharArray (s.Length, 1);
			Assert.Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#A5");
		}

		try {
			s.ToCharArray (Int32.MaxValue, 1);
			Assert.Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#B5");
		}
	}

	[Test] // ToLower ()
	public void ToLower1 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual ("\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069", s.ToLower(), "#1");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual ("originali", s.ToLower (), "#2");
	}

	[Test] // ToLower (CultureInfo)
	public void ToLower2 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual ("originali", s.ToLower (new CultureInfo ("en-US")), "#A1");
		Assert.AreEqual ("\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069", s.ToLower (new CultureInfo ("tr-TR")), "#A2");
		Assert.AreEqual (string.Empty, string.Empty.ToLower (new CultureInfo ("en-US")), "#A3");
		Assert.AreEqual (string.Empty, string.Empty.ToLower (new CultureInfo ("tr-TR")), "#A4");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual ("originali", s.ToLower (new CultureInfo ("en-US")), "#B1");
		Assert.AreEqual ("\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069", s.ToLower (new CultureInfo ("tr-TR")), "#B2");
		Assert.AreEqual (string.Empty, string.Empty.ToLower (new CultureInfo ("en-US")), "#B3");
		Assert.AreEqual (string.Empty, string.Empty.ToLower (new CultureInfo ("tr-TR")), "#B4");
	}

	[Test] // ToLower (CultureInfo)
	public void ToLower2_Culture_Null ()
	{
		string s = "OrIgInAl";

		try {
			s.ToLower ((CultureInfo) null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("culture", ex.ParamName, "#A5");
		}

		try {
			string.Empty.ToLower ((CultureInfo) null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("culture", ex.ParamName, "#B5");
		}
	}

	[Test]
	public void TestToString ()
	{
		string s1 = "OrIgInAli";
		Assert.AreEqual (s1, s1.ToString(), "ToString failed!");
	}

	[Test] // ToUpper ()
	public void ToUpper1 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual ("ORIGINAL\u0130", s.ToUpper (), "#1");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual ("ORIGINALI", s.ToUpper (), "#2");
	}

	[Test] // ToUpper (CultureInfo)
	public void ToUpper2 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		Assert.AreEqual ("ORIGINALI", s.ToUpper (new CultureInfo ("en-US")), "#A1");
		Assert.AreEqual ("ORIGINAL\u0130", s.ToUpper (new CultureInfo ("tr-TR")), "#A2");
		Assert.AreEqual (string.Empty, string.Empty.ToUpper (new CultureInfo ("en-US")), "#A3");
		Assert.AreEqual (string.Empty, string.Empty.ToUpper (new CultureInfo ("tr-TR")), "#A4");

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert.AreEqual ("ORIGINALI", s.ToUpper (new CultureInfo ("en-US")), "#B1");
		Assert.AreEqual ("ORIGINAL\u0130", s.ToUpper (new CultureInfo ("tr-TR")), "#B2");
		Assert.AreEqual (string.Empty, string.Empty.ToUpper (new CultureInfo ("en-US")), "#B3");
		Assert.AreEqual (string.Empty, string.Empty.ToUpper (new CultureInfo ("tr-TR")), "#B4");
	}

	[Test] // ToUpper (CultureInfo)
	public void ToUpper2_Culture_Null ()
	{
		string s = "OrIgInAl";

		try {
			s.ToUpper ((CultureInfo) null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("culture", ex.ParamName, "#A5");
		}

		try {
			string.Empty.ToUpper ((CultureInfo) null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("culture", ex.ParamName, "#B5");
		}
	}

	[Test]
	public void TestTrim ()
	{
		string s1 = "  original\t\n";
		Assert.AreEqual ("original", s1.Trim(), "basic trim failed");
		Assert.AreEqual ("original", s1.Trim(null), "basic trim failed");

		s1 = "original";
		Assert.AreEqual ("original", s1.Trim(), "basic trim failed");
		Assert.AreEqual ("original", s1.Trim(null), "basic trim failed");

		s1 = "   \t \n  ";
		Assert.AreEqual (string.Empty, s1.Trim(), "empty trim failed");
		Assert.AreEqual (string.Empty, s1.Trim(null), "empty trim failed");

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		Assert.AreEqual ("original", s1.Trim(delims), "custom trim failed");

#if NET_2_0
		Assert.AreEqual ("original", "\u2028original\u2029".Trim (), "net_2_0 additional char#1");
		Assert.AreEqual ("original", "\u0085original\u1680".Trim (), "net_2_0 additional char#2");
#endif
	}

	[Test]
	public void TestTrimEnd ()
	{
		string s1 = "  original\t\n";
		Assert.AreEqual ("  original", s1.TrimEnd(null), "basic TrimEnd failed");

		s1 = "  original";
		Assert.AreEqual ("  original", s1.TrimEnd(null), "basic TrimEnd failed");

		s1 = "  \t  \n  \n    ";
		Assert.AreEqual (string.Empty, s1.TrimEnd(null), "empty TrimEnd failed");

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		Assert.AreEqual ("aaaoriginal", s1.TrimEnd(delims), "custom TrimEnd failed");
	}

	[Test]
	public void TestTrimStart ()
	{
		string s1 = "  original\t\n";
		Assert.AreEqual ("original\t\n", s1.TrimStart(null), "basic TrimStart failed");

		s1 = "original\t\n";
		Assert.AreEqual ("original\t\n", s1.TrimStart(null), "basic TrimStart failed");

		s1 = "    \t \n \n  ";
		Assert.AreEqual (string.Empty, s1.TrimStart(null), "empty TrimStart failed");

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		Assert.AreEqual ("originalbbb", s1.TrimStart(delims), "custom TrimStart failed");
	}

	[Test]
	public void TestChars ()
	{
		string s;

		s = string.Empty;
		try {
			char c = s [0];
			Assert.Fail ("#A1:" + c);
		} catch (IndexOutOfRangeException ex) {
			Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		s = "A";
		try {
			char c = s [-1];
			Assert.Fail ("#B1:" + c);
		} catch (IndexOutOfRangeException ex) {
			Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void TestComparePeriod ()
	{
		// according to bug 63981, this behavior is for all cultures
		Assert.AreEqual (-1, String.Compare ("foo.obj", "foobar.obj", false), "#1");
	}

	[Test]
	public void LastIndexOfAnyBounds1 ()
	{
		string mono = "Mono";
		char [] k = { 'M' };
		try {
			mono.LastIndexOfAny (k, mono.Length, 1);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("startIndex", ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestSplit ()
	{
		string s1 = "abcdefghijklm";
		char[] c1 = {'q', 'r'};
		Assert.AreEqual (s1, (s1.Split(c1))[0], "No splitters");

		char[] c2 = {'a', 'e', 'i', 'o', 'u'};
		string[] chunks = s1.Split(c2);
		Assert.AreEqual (string.Empty, chunks[0], "First chunk");
		Assert.AreEqual ("bcd", chunks[1], "Second chunk");
		Assert.AreEqual ("fgh", chunks[2], "Third chunk");
		Assert.AreEqual ("jklm", chunks[3], "Fourth chunk");

		{
			bool errorThrown = false;
			try {
				chunks = s1.Split(c2, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "Split out of range");
		}

		chunks = s1.Split(c2, 2);
		Assert.AreEqual (2, chunks.Length, "Limited chunk");
		Assert.AreEqual (string.Empty, chunks[0], "First limited chunk");
		Assert.AreEqual ("bcdefghijklm", chunks[1], "Second limited chunk");

		string s3 = "1.0";
		char[] c3 = {'.'};
		chunks = s3.Split(c3,2);
		Assert.AreEqual (2, chunks.Length, "1.0 split length");
		Assert.AreEqual ("1", chunks[0], "1.0 split first chunk");
		Assert.AreEqual ("0", chunks[1], "1.0 split second chunk");

		string s4 = "1.0.0";
		char[] c4 = {'.'};
		chunks = s4.Split(c4,2);
		Assert.AreEqual (2, chunks.Length, "1.0.0 split length");
		Assert.AreEqual ("1", chunks[0], "1.0.0 split first chunk");
		Assert.AreEqual ("0.0", chunks[1], "1.0.0 split second chunk");

		string s5 = ".0.0";
		char[] c5 = {'.'};
		chunks = s5.Split (c5, 2);
		Assert.AreEqual (2, chunks.Length, ".0.0 split length");
		Assert.AreEqual (string.Empty, chunks[0], ".0.0 split first chunk");
		Assert.AreEqual ("0.0", chunks[1], ".0.0 split second chunk");

		string s6 = ".0";
		char[] c6 = {'.'};
		chunks = s6.Split (c6, 2);
		Assert.AreEqual (2, chunks.Length, ".0 split length");
		Assert.AreEqual (string.Empty, chunks[0], ".0 split first chunk");
		Assert.AreEqual ("0", chunks[1], ".0 split second chunk");

		string s7 = "0.";
		char[] c7 = {'.'};
		chunks = s7.Split (c7, 2);
		Assert.AreEqual (2, chunks.Length, "0. split length");
		Assert.AreEqual ("0", chunks[0], "0. split first chunk");
		Assert.AreEqual (string.Empty, chunks[1], "0. split second chunk");

		string s8 = "0.0000";
		char[] c8 = {'.'};
		chunks = s8.Split (c8, 2);
		Assert.AreEqual (2, chunks.Length, "0.0000/2 split length");
		Assert.AreEqual ("0", chunks[0], "0.0000/2 split first chunk");
		Assert.AreEqual ("0000", chunks[1], "0.0000/2 split second chunk");

		chunks = s8.Split (c8, 3);
		Assert.AreEqual (2, chunks.Length, "0.0000/3 split length");
		Assert.AreEqual ("0", chunks[0], "0.0000/3 split first chunk");
		Assert.AreEqual ("0000", chunks[1], "0.0000/3 split second chunk");

		chunks = s8.Split (c8, 1);
		Assert.AreEqual (1, chunks.Length, "0.0000/1 split length");
		Assert.AreEqual ("0.0000", chunks[0], "0.0000/1 split first chunk");

		chunks = s1.Split(c2, 1);
		Assert.AreEqual (1, chunks.Length, "Single split");
		Assert.AreEqual (s1, chunks[0], "Single chunk");

		chunks = s1.Split(c2, 0);
		Assert.AreEqual (0, chunks.Length, "Zero split");
	}

	[Test]
	public void MoreSplit ()
	{
		string test = "123 456 789";
		string [] st = test.Split ();
		Assert.AreEqual ("123", st [0], "#01");
		st = test.Split (null);
		Assert.AreEqual ("123", st [0], "#02");
	}

#if NET_2_0
	[Test] // Split (Char [], StringSplitOptions)
	public void Split3_Options_Invalid ()
	{
		try {
			"A B".Split (new Char [] { 'A' }, (StringSplitOptions) 4096);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("4096") != 1, "#5");
			Assert.IsNull (ex.ParamName, "#6");
		}
	}

	[Test] // Split (Char [], StringSplitOptions)
	public void Split4_Options_Invalid ()
	{
		try {
			"A B".Split (new String [] { "A" }, (StringSplitOptions) 4096);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("4096") != 1, "#5");
			Assert.IsNull (ex.ParamName, "#6");
		}
	}

	[Test] // Split (Char [], StringSplitOptions)
	public void Split5_Options_Invalid ()
	{
		try {
			"A B".Split (new Char [] { 'A' }, 0, (StringSplitOptions) 4096);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("4096") != 1, "#5");
			Assert.IsNull (ex.ParamName, "#6");
		}
	}

	[Test] // Split (String [], Int32, StringSplitOptions)
	public void Split6_Count_Negative ()
	{
		try {
			"A B".Split (new String [] { "A" }, -1, StringSplitOptions.None);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("count", ex.ParamName, "#5");
		}
	}

	[Test] // Split (String [], Int32, StringSplitOptions)
	public void Split6_Options_Invalid ()
	{
		try {
			"A B".Split (new String [] { "A" }, 0, (StringSplitOptions) 4096);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("4096") != 1, "#5");
			Assert.IsNull (ex.ParamName, "#6");
		}
	}

	[Test]
	public void SplitString ()
	{
		String[] res;
		
		// count == 0
		res = "A B C".Split (new String [] { "A" }, 0, StringSplitOptions.None);
		Assert.AreEqual (0, res.Length);

		// empty and RemoveEmpty
		res = string.Empty.Split (new String [] { "A" }, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (0, res.Length);

		// Not found
		res = "A B C".Split (new String [] { "D" }, StringSplitOptions.None);
		Assert.AreEqual (1, res.Length);
		Assert.AreEqual ("A B C", res [0]);

		// A normal test
		res = "A B C DD E".Split (new String[] { "B", "D" }, StringSplitOptions.None);
		Assert.AreEqual (4, res.Length);
		Assert.AreEqual ("A ", res [0]);
		Assert.AreEqual (" C ", res [1]);
		Assert.AreEqual (string.Empty, res [2]);
		Assert.AreEqual (" E", res [3]);

		// Same with RemoveEmptyEntries
		res = "A B C DD E".Split (new String[] { "B", "D" }, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (3, res.Length);
		Assert.AreEqual ("A ", res [0]);
		Assert.AreEqual (" C ", res [1]);
		Assert.AreEqual (" E", res [2]);

		// Delimiter matches once at the beginning of the string
		res = "A B".Split (new String [] { "A" }, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length);
		Assert.AreEqual (" B", res [0]);

		// Delimiter at the beginning and at the end
		res = "B C DD B".Split (new String[] { "B" }, StringSplitOptions.None);
		Assert.AreEqual (3, res.Length);
		Assert.AreEqual (string.Empty, res [0]);
		Assert.AreEqual (" C DD ", res [1]);
		Assert.AreEqual (string.Empty, res [2]);

		res = "B C DD B".Split (new String[] { "B" }, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length);
		Assert.AreEqual (" C DD ", res [0]);

		// count
		res = "A B C DD E".Split (new String[] { "B", "D" }, 2, StringSplitOptions.None);
		Assert.AreEqual (2, res.Length);
		Assert.AreEqual ("A ", res [0]);
		Assert.AreEqual (" C DD E", res [1]);

		// Ordering
		res = "ABCDEF".Split (new String[] { "EF", "BCDE" }, StringSplitOptions.None);
		Assert.AreEqual (2, res.Length);
		Assert.AreEqual ("A", res [0]);
		Assert.AreEqual ("F", res [1]);

		res = "ABCDEF".Split (new String[] { "BCD", "BC" }, StringSplitOptions.None);
		Assert.AreEqual (2, res.Length);
		Assert.AreEqual ("A", res [0]);
		Assert.AreEqual ("EF", res [1]);

		// Whitespace
		res = "A B\nC".Split ((String[])null, StringSplitOptions.None);
		Assert.AreEqual (3, res.Length);
		Assert.AreEqual ("A", res [0]);
		Assert.AreEqual ("B", res [1]);
		Assert.AreEqual ("C", res [2]);

		res = "A B\nC".Split (new String [0], StringSplitOptions.None);
		Assert.AreEqual (3, res.Length);
		Assert.AreEqual ("A", res [0]);
		Assert.AreEqual ("B", res [1]);
		Assert.AreEqual ("C", res [2]);
	}
	
	[Test]
	public void SplitStringChars ()
	{
		String[] res;

		// count == 0
		res = "..A..B..".Split (new Char[] { '.' }, 0, StringSplitOptions.None);
		Assert.AreEqual (0, res.Length, "#01-01");

		// count == 1
		res = "..A..B..".Split (new Char[] { '.' }, 1, StringSplitOptions.None);
		Assert.AreEqual (1, res.Length, "#02-01");
		Assert.AreEqual ("..A..B..", res [0], "#02-02");

		// count == 1 + RemoveEmpty
		res = "..A..B..".Split (new Char[] { '.' }, 1, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#03-01");
		Assert.AreEqual ("..A..B..", res [0], "#03-02");
		
		// Strange Case A+B A
		res = "...".Split (new Char[] { '.' }, 1, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#ABA-01");
		Assert.AreEqual ("...", res [0], "#ABA-02");

		// Strange Case A+B B
		res = "...".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (0, res.Length, "#ABB-01");

		// Keeping Empties and multipe split chars
		res = "..A;.B.;".Split (new Char[] { '.', ';' }, StringSplitOptions.None);
		Assert.AreEqual (7, res.Length, "#04-01");
		Assert.AreEqual (string.Empty, res [0], "#04-02");
		Assert.AreEqual (string.Empty, res [1], "#04-03");
		Assert.AreEqual ("A", res [2], "#04-04");
		Assert.AreEqual (string.Empty, res [3], "#04-05");
		Assert.AreEqual ("B", res [4], "#04-06");
		Assert.AreEqual (string.Empty, res [5], "#04-07");
		Assert.AreEqual (string.Empty, res [6], "#04-08");

		// Trimming (3 tests)
		res = "..A".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#05-01");
		Assert.AreEqual ("A", res [0], "#05-02");
		
		res = "A..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#06-01");
		Assert.AreEqual ("A", res [0], "#06-02");
		
		res = "..A..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#07-01");
		Assert.AreEqual ("A", res [0], "#07-02");

		// Lingering Tail
		res = "..A..B..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (2, res.Length, "#08-01");
		Assert.AreEqual ("A", res [0], "#08-02");
		Assert.AreEqual ("B..", res [1], "#08-03");

		// Whitespace and Long split chain (removing empty chars)
		res = "  A\tBC\n\rDEF    GHI  ".Split ((Char[])null, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (4, res.Length, "#09-01");
		Assert.AreEqual ("A", res [0], "#09-02");
		Assert.AreEqual ("BC", res [1], "#09-03");
		Assert.AreEqual ("DEF", res [2], "#09-04");
		Assert.AreEqual ("GHI", res [3], "#09-05");

		// Nothing but separators
		res = "..,.;.,".Split (new Char[]{'.',',',';'},2,StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (0, res.Length, "#10-01");

		// Complete testseries
		char[] dash = new Char[] { '/' };
		StringSplitOptions o = StringSplitOptions.RemoveEmptyEntries;
		Assert.AreEqual ("hi", "hi".Split (dash, o)[0], "#11-01");
		Assert.AreEqual ("hi", "hi/".Split (dash, o)[0], "#11-02");
		Assert.AreEqual ("hi", "/hi".Split (dash, o)[0], "#11-03");

		Assert.AreEqual ("hi..", "hi../".Split (dash, o)[0], "#11-04-1");
		Assert.AreEqual ("hi..", "/hi..".Split (dash, o)[0], "#11-04-2");

		res = "/hi/..".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-05-1");
		Assert.AreEqual ("..", res[1], "#11-05-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");

		res = "hi/..".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-06-1");
		Assert.AreEqual ("..", res[1], "#11-06-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");

		res = "hi/../".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-07-1");
		Assert.AreEqual ("..", res[1], "#11-07-2");
		Assert.AreEqual (2, res.Length, "#11-07-3");

		res = "/hi../".Split (dash, o);
		Assert.AreEqual ("hi..", res[0], "#11-08-1");
		Assert.AreEqual (1, res.Length, "#11-08-2");

		res = "/hi/../".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-09-1");
		Assert.AreEqual ("..", res[1], "#11-09-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");
	}
	
	[Test]
	public void SplitStringStrings ()
	{
		String[] res;

		// count == 0
		res = "..A..B..".Split (new String[] { "." }, 0, StringSplitOptions.None);
		Assert.AreEqual (0, res.Length, "#01-01");

		// count == 1
		res = "..A..B..".Split (new String[] { "." }, 1, StringSplitOptions.None);
		Assert.AreEqual (1, res.Length, "#02-01");
		Assert.AreEqual ("..A..B..", res [0], "#02-02");

		// count == 1 + RemoveEmpty
		res = "..A..B..".Split (new String[] { "." }, 1, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#03-01");
		Assert.AreEqual ("..A..B..", res [0], "#03-02");
		
		// Strange Case A+B A
		res = "...".Split (new String[] { "." }, 1, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#ABA-01");
		Assert.AreEqual ("...", res [0], "#ABA-02");

		// Strange Case A+B B
		res = "...".Split (new String[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (0, res.Length, "#ABB-01");

		// Keeping Empties and multipe split chars
		res = "..A;.B.;".Split (new String[] { ".", ";" }, StringSplitOptions.None);
		Assert.AreEqual (7, res.Length, "#04-01");
		Assert.AreEqual (string.Empty, res [0], "#04-02");
		Assert.AreEqual (string.Empty, res [1], "#04-03");
		Assert.AreEqual ("A", res [2], "#04-04");
		Assert.AreEqual (string.Empty, res [3], "#04-05");
		Assert.AreEqual ("B", res [4], "#04-06");
		Assert.AreEqual (string.Empty, res [5], "#04-07");
		Assert.AreEqual (string.Empty, res [6], "#04-08");

		// Trimming (3 tests)
		res = "..A".Split (new String[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#05-01");
		Assert.AreEqual ("A", res [0], "#05-02");
		
		res = "A..".Split (new String[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#06-01");
		Assert.AreEqual ("A", res [0], "#06-02");
		
		res = "..A..".Split (new String[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (1, res.Length, "#07-01");
		Assert.AreEqual ("A", res [0], "#07-02");

		// Lingering Tail
		res = "..A..B..".Split (new String[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (2, res.Length, "#08-01");
		Assert.AreEqual ("A", res [0], "#08-02");
		Assert.AreEqual ("B..", res [1], "#08-03");

		// Whitespace and Long split chain (removing empty chars)
		res = "  A\tBC\n\rDEF    GHI  ".Split ((String[])null, StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (4, res.Length, "#09-01");
		Assert.AreEqual ("A", res [0], "#09-02");
		Assert.AreEqual ("BC", res [1], "#09-03");
		Assert.AreEqual ("DEF", res [2], "#09-04");
		Assert.AreEqual ("GHI", res [3], "#09-05");

		// Nothing but separators
		res = "..,.;.,".Split (new String[]{".",",",";"},2,StringSplitOptions.RemoveEmptyEntries);
		Assert.AreEqual (0, res.Length, "#10-01");

		// Complete testseries
		String[] dash = new String[] { "/" };
		StringSplitOptions o = StringSplitOptions.RemoveEmptyEntries;
		Assert.AreEqual ("hi", "hi".Split (dash, o)[0], "#11-01");
		Assert.AreEqual ("hi", "hi/".Split (dash, o)[0], "#11-02");
		Assert.AreEqual ("hi", "/hi".Split (dash, o)[0], "#11-03");

		Assert.AreEqual ("hi..", "hi../".Split (dash, o)[0], "#11-04-1");
		Assert.AreEqual ("hi..", "/hi..".Split (dash, o)[0], "#11-04-2");

		res = "/hi/..".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-05-1");
		Assert.AreEqual ("..", res[1], "#11-05-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");

		res = "hi/..".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-06-1");
		Assert.AreEqual ("..", res[1], "#11-06-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");

		res = "hi/../".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-07-1");
		Assert.AreEqual ("..", res[1], "#11-07-2");
		Assert.AreEqual (2, res.Length, "#11-07-3");

		res = "/hi../".Split (dash, o);
		Assert.AreEqual ("hi..", res[0], "#11-08-1");
		Assert.AreEqual (1, res.Length, "#11-08-2");

		res = "/hi/../".Split (dash, o);
		Assert.AreEqual ("hi", res[0], "#11-09-1");
		Assert.AreEqual ("..", res[1], "#11-09-2");
		Assert.AreEqual (2, res.Length, "#11-09-3");
	}

	[Test]
	[Category ("NotDotNet")]
	public void Normalize1 ()
	{
		// .NET does not combine them into U+1F80
		// seealso: http://demo.icu-project.org/icu-bin/nbrowser?t=\u03B1\u0313\u0345
		string s = "\u03B1\u0313\u0345";
		Assert.IsTrue (!s.IsNormalized (NormalizationForm.FormC), "#1");
		Assert.IsTrue (!s.IsNormalized (NormalizationForm.FormKC), "#2");
		Assert.AreEqual ("\u1F80", s.Normalize (NormalizationForm.FormC), "#3");
		Assert.AreEqual ("\u1F80", s.Normalize (NormalizationForm.FormKC), "#4");
	}

	[Test]
	[Category ("NotDotNet")]
	public void Normalize2 ()
	{
		string s1 = "\u0061\u0301bc";
		string s2 = "\u00e1bc";
		// .NET does not combine \u0061\0301 into \u00E1
		// seealso: http://demo.icu-project.org/icu-bin/nbrowser?t=\u0061\u0301bc
		Assert.AreEqual (s2, s1.Normalize (NormalizationForm.FormC), "#1");
		Assert.AreEqual (s2, s1.Normalize (NormalizationForm.FormKC), "#2");
	}

	[Test]
	public void Normalize3 ()
	{
		var s = new string (new char [] { '\u064A', '\u064F', '\u0648', '\u0654', '\u0652', '\u064A', '\u064F', '\u0648', '\u0654' });

		var formC = new string (new char [] { '\u064A', '\u064F', '\u0624', '\u0652', '\u064a', '\u064f', '\u0624' });
		var formD = new string (new char [] { '\u064A', '\u064F', '\u0648', '\u0652', '\u0654', '\u064a', '\u064f', '\u0648', '\u0654' });
		var formKC = new string (new char [] { '\u064A', '\u064F', '\u0624', '\u0652', '\u064a', '\u064f', '\u0624' });
		var formKD = new string (new char [] { '\u064A', '\u064F', '\u0648', '\u0652', '\u0654', '\u064a', '\u064f', '\u0648', '\u0654' });

		Assert.AreEqual (formD, s.Normalize (NormalizationForm.FormD), "#1");
		Assert.AreEqual (formC, s.Normalize (NormalizationForm.FormC), "#2");
		Assert.AreEqual (formKD, s.Normalize (NormalizationForm.FormKD), "#3");
		Assert.AreEqual (formKC, s.Normalize (NormalizationForm.FormKC), "#4");
	}

	[Test] // bug #480152, test cases by David Mitchell
	public void NormalizeFormD ()
	{
		Assert.AreEqual ("\u212B".Normalize (NormalizationForm.FormD), "\u0041\u030A", "#1");
		Assert.AreEqual ("\u1E69".Normalize (NormalizationForm.FormD), "\u0073\u0323\u0307", "#2");
		Assert.AreEqual ("\u1e4e".Normalize (NormalizationForm.FormD), "\u004f\u0303\u0308", "#3");
		Assert.AreEqual ("\u1e2f".Normalize (NormalizationForm.FormD), "\u0069\u0308\u0301", "#4");
	}

	[Test] // bug #480152, test cases by David Mitchell
	public void NormalizeFormC ()
	{
		Assert.AreEqual ("\u0041\u030a\u0061\u030a".Normalize (NormalizationForm.FormC), "\u00c5\u00e5", "#1");
		Assert.AreEqual ("\u006E\u0303".Normalize (NormalizationForm.FormC), "\u00F1", "#2");
		Assert.AreEqual ("\u03B7\u0313\u0300\u0345".Normalize (NormalizationForm.FormC), "\u1F92", "#3");
	}

        [Test] // bug #480152, test cases by Tom Philpot
        public void NormalizeFormCCrashers ()
        {
		string[][] entries = new string[][] {
			new string[] { "\u05d0\u0307\u05dc", "#1" },
			new string[] { "\u05d0\u0307\u05dc\u05d9\u05d9\u05df", "#2" },
			new string[] { "\u05d4\u05d0\u0307\u05dc\u0307\u05d9\u0307\u05df\u0307", "#3" },
			new string[] { "\u05d9\u05e9\u05de\u05e2\u0307\u05d0\u0307\u05dc\u0307", "#4" },
			new string[] { "\u05d9\u05e9\u05e8\u05d0\u0307\u05dc\u0307", "#5" },
		};

		foreach (string[] entry in entries)
			entry [0].Normalize (NormalizationForm.FormC);
	}

	[Test]
	public void NormalizeFormCHangul ()
	{
		Assert.AreEqual ("\u1100\u116C".Normalize (NormalizationForm.FormC), "\uAD34", "#1");
		Assert.AreEqual ("\u1100\u116B\u11C2".Normalize (NormalizationForm.FormC), "\uAD33", "#2");
		Assert.AreEqual ("\u1100!".Normalize (NormalizationForm.FormC), "\u1100!", "#3");
		Assert.AreEqual ("\u1100\u116B!".Normalize (NormalizationForm.FormC), "\uAD18\u0021", "#4");
		Assert.AreEqual ("!\u116C".Normalize (NormalizationForm.FormC), "!\u116C", "#5");
		Assert.AreEqual ("!\u116B\u11C2".Normalize (NormalizationForm.FormC), "!\u116B\u11C2", "#6");
	}

	[Test]
	public void MoreNormalizeFormC ()
	{
		Assert.AreEqual ("\u1E0A\u0323".Normalize (NormalizationForm.FormC), "\u1E0C\u0307", "#1");
		Assert.AreEqual ("\u0044\u0323\u0307".Normalize (NormalizationForm.FormC), "\u1E0C\u0307", "#2");
	}
#endif
	[Test]
	public void Emptiness ()
	{
		// note: entries using AreEqual are in reality AreNotSame on MS FX
		// but I prefer Mono implementation ;-) and it minimize the changes
		Assert.AreSame (String.Empty, "", "Empty");

		Assert.AreSame (String.Empty, String.Concat ((object) null), "Concat(null)");
		Assert.AreSame (String.Empty, String.Concat ((object) String.Empty), "Concat(empty)");
		Assert.AreSame (String.Empty, String.Concat ((object) String.Empty, (object) String.Empty), "Concat(object,object)");
		Assert.AreSame (String.Empty, String.Concat (String.Empty, String.Empty), "Concat(string,string)");
		Assert.AreEqual (String.Empty, String.Concat (String.Empty, String.Empty, String.Empty), "Concat(string,string,string)");
		Assert.AreEqual (String.Empty, String.Concat ((object) null, (object) (object) null, (object) null, (object) null), "Concat(null,null,null,null)-object");
		Assert.AreSame (String.Empty, String.Concat ((string) null, (string) (string) null, (string) null, (string) null), "Concat(null,null,null,null)-string");
		Assert.AreNotSame (String.Empty, String.Concat (String.Empty, String.Empty, String.Empty, String.Empty), "Concat(string,string,string,string)");
		Assert.AreEqual (String.Empty, String.Concat (new object [] { String.Empty, String.Empty }), "Concat(object[])");
		Assert.AreEqual (String.Empty, String.Concat (new string [] { String.Empty, String.Empty }), "Concat(string[])");

		Assert.AreNotSame (String.Empty, String.Copy (String.Empty), "Copy");

		Assert.AreEqual (String.Empty, "".Insert (0, String.Empty), "Insert(Empty)");
		Assert.AreEqual (String.Empty, String.Empty.Insert (0, ""), "Empty.Insert");

		Assert.AreNotSame (String.Empty, String.Empty.PadLeft (0), "PadLeft(int)");
		Assert.AreNotSame (String.Empty, String.Empty.PadLeft (0, '.'), "PadLeft(int.char)");
		Assert.AreSame (String.Empty, String.Empty.PadRight (0), "PadRight(int)");
		Assert.AreSame (String.Empty, String.Empty.PadRight (0, '.'), "PadRight(int.char)");

		Assert.AreSame (String.Empty, "".Substring (0), "Substring(int)");
		Assert.AreSame (String.Empty, "ab".Substring (1, 0), "Substring(int,int)");

		Assert.AreSame (String.Empty, "".ToLower (), "ToLower");
		Assert.AreSame (String.Empty, "".ToUpper (), "ToUpper");
		Assert.AreSame (String.Empty, "".ToLower (CultureInfo.CurrentCulture), "ToLower(CultureInfo)");
		Assert.AreSame (String.Empty, "".ToUpper (CultureInfo.CurrentCulture), "ToUpper(CultureInfo)");
		Assert.AreSame (String.Empty, "".ToLowerInvariant (), "ToLowerInvariant");
		Assert.AreSame (String.Empty, "".ToUpperInvariant (), "ToUpperInvariant");

		Assert.AreSame (String.Empty, "".Trim (), "Trim()");
		Assert.AreSame (String.Empty, "a".Trim ('a'), "Trim(char)");
		Assert.AreSame (String.Empty, "a".TrimEnd ('a'), "TrimEnd(char)");
		Assert.AreSame (String.Empty, "a".TrimStart ('a'), "TrimStart(char)");
	}
	
	[Test]
	public void LastIndexOfAndEmptiness () {
		Assert.AreEqual (-1, "".LastIndexOf('.'), "#1");
		Assert.AreEqual (-1, "".LastIndexOf('.', -1), "#2");
		Assert.AreEqual (-1, "".LastIndexOf('.', -1, -1), "#3");
		Assert.AreEqual (0, "x".LastIndexOf('x', 0), "#4");
		Assert.AreEqual (0 , "x".LastIndexOf('x', 0, 1), "#5");
		Assert.AreEqual (-1 , "x".LastIndexOf('z', 0, 1), "#6");

		try {
			"".LastIndexOf(null);
			Assert.Fail ("#7");
		} catch (ArgumentNullException) {}

		Assert.AreEqual (0, "".LastIndexOf(""), "#8");
		Assert.AreEqual (0, "".LastIndexOf("", -1), "#9");
		Assert.AreEqual (0, "".LastIndexOf("", -1, 1), "#10");
		Assert.AreEqual (0, "".LastIndexOf("", StringComparison.Ordinal), "#11");
		Assert.AreEqual (0, "".LastIndexOf("", -1, StringComparison.Ordinal), "#12");
		Assert.AreEqual (0, "".LastIndexOf("", -1, -1, StringComparison.Ordinal), "#13");
		Assert.AreEqual (0, "x".LastIndexOf(""), "#14");

		Assert.AreEqual (0, "x".LastIndexOf("x", 0), "#15");
		Assert.AreEqual (0, "x".LastIndexOf("", 0), "#16");
		Assert.AreEqual (0, "xxxx".LastIndexOf("", 0), "#17");
		Assert.AreEqual (1, "xxxx".LastIndexOf("", 1), "#18");

		Assert.AreEqual (1, "xy".LastIndexOf(""), "#19");
		Assert.AreEqual (2, "xyz".LastIndexOf(""), "#20");
		Assert.AreEqual (1, "xy".LastIndexOf(""), "#21");
		Assert.AreEqual (1, "xy".LastIndexOf("", 2), "#22");
		Assert.AreEqual (2, "xyz".LastIndexOf("", 2), "#23");
		Assert.AreEqual (2, "xyz".LastIndexOf("", 2, 2), "#24");
		Assert.AreEqual (2, "xyz".LastIndexOf("", 3, 3), "#25");

		try {
			"xy".LastIndexOf("", 29);
			Assert.Fail ("#26");
		}catch (ArgumentOutOfRangeException){}

		Assert.AreEqual (-1, "".LastIndexOf("x"), "#27");
		Assert.AreEqual (-1, "".LastIndexOf("x", -1), "#28");
		Assert.AreEqual (-1, "".LastIndexOf("x", -1), "#29");
		Assert.AreEqual (-1, "".LastIndexOf("x", StringComparison.Ordinal), "#30");
		Assert.AreEqual (-1, "".LastIndexOf("x", -1, StringComparison.Ordinal), "#31");
		Assert.AreEqual (-1, "".LastIndexOf("x", -1, -1, StringComparison.Ordinal), "#32");

		Assert.AreEqual (1, "xx".LastIndexOf("", StringComparison.Ordinal), "#33");
		Assert.AreEqual (1, "xx".LastIndexOf("", 2, StringComparison.Ordinal), "#34");
		Assert.AreEqual (1, "xx".LastIndexOf("", 2, 2, StringComparison.Ordinal), "#35");

		Assert.AreEqual (3, "xxxx".LastIndexOf("", StringComparison.Ordinal), "#36");
		Assert.AreEqual (2, "xxxx".LastIndexOf("", 2, StringComparison.Ordinal), "#37");
		Assert.AreEqual (2, "xxxx".LastIndexOf("", 2, 2, StringComparison.Ordinal), "#38");

		Assert.AreEqual (3, "xxxx".LastIndexOf("", 3, StringComparison.Ordinal), "#39");
		Assert.AreEqual (3, "xxxx".LastIndexOf("", 3, 3, StringComparison.Ordinal), "#40");
	}
	
	
	[Test]
	public void LastIndexOfAnyAndEmptiness () {
		Assert.AreEqual (-1, "".LastIndexOfAny(new char[] {'.', 'x'}), "#1");
		Assert.AreEqual (-1, "".LastIndexOfAny(new char[] {'.', 'x'}, -1), "#2");
		Assert.AreEqual (-1, "".LastIndexOfAny(new char[] {'.', 'x'}, -1, -1), "#3");
	}
}

}
