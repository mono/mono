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
public class StringTest : TestCase
{
	private CultureInfo orgCulture;

	protected override void SetUp ()
	{
		// save current culture
		orgCulture = CultureInfo.CurrentCulture;
	}

	protected override void TearDown ()
	{
		// restore original culture
		Thread.CurrentThread.CurrentCulture = orgCulture;
	}


#if !TARGET_JVM
	[Test] // ctor (Char [])
	public unsafe void Constructor2 ()
	{
		AssertEquals ("#1", String.Empty, new String ((char[]) null));
		AssertEquals ("#2", String.Empty, new String (new Char [0]));
		AssertEquals ("#3", "A", new String (new Char [1] {'A'}));
	}
#endif

	[Test] // ctor (Char, Int32)
	public void Constructor4 ()
	{
		AssertEquals (string.Empty, new String ('A', 0));
		AssertEquals ("AAA", new String ('A', 3));
	}

	[Test] // ctor (Char, Int32)
	public void Constructor4_Count_Negative ()
	{
		try {
			new String ('A', -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// 'count' must be non-negative
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6 ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };
		AssertEquals ("#1", "ABC", new String (arr, 0, arr.Length));
		AssertEquals ("#2", "BC", new String (arr, 1, 2));
		AssertEquals ("#3", string.Empty, new String (arr, 2, 0));
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Length_Negative ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, 0, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "length", ex.ParamName);
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Length_Overflow ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, 1, 3);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_StartIndex_Negative ()
	{
		char [] arr = new char [3] { 'A', 'B', 'C' };

		try {
			new String (arr, -1, 0);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // ctor (Char [], Int32, Int32)
	public void Constructor6_Value_Null ()
	{
		try {
			new String ((char []) null, 0, 0);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
		}
	}

#if !TARGET_JVM
	[Test]
	public unsafe void CharPtrConstructor ()
	{
		AssertEquals ("char*", String.Empty, new String ((char*) null));
		AssertEquals ("char*,int,int", String.Empty, new String ((char*) null, 0, 0));
	}

	[Test]
	public unsafe void TestSbytePtrConstructorASCII ()
	{
		Encoding encoding = Encoding.ASCII;
		String s = "ASCII*\0";
		byte[] bytes = encoding.GetBytes (s);

		fixed (byte* bytePtr = bytes)
			AssertEquals (s, new String ((sbyte*) bytePtr, 0, bytes.Length, encoding));
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
			AssertEquals (s.Substring (0, s.Length - 1), new String ((sbyte*) bytePtr));
			AssertEquals (s, new String ((sbyte*) bytePtr, 0, bytes.Length));
			AssertEquals (s, new String ((sbyte*) bytePtr, 0, bytes.Length, null));
			AssertEquals (s, new String ((sbyte*) bytePtr, 0, bytes.Length, encoding));
		}
	}

	[Test] // ctor (SByte*)
	public unsafe void Constructor3_Value_Null ()
	{
		AssertEquals (String.Empty, new String ((sbyte*) null));
	}

	[Test] // ctor (SByte*)
	public unsafe void Constructor3_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1));
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "ptr", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Length_Negative ()
	{
		try {
			new String ((sbyte*) null, 0, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "length", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_StartIndex_Negative ()
	{
		try {
			new String ((sbyte*) null, -1, 0);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public unsafe void Constructor7_StartIndex_Overflow ()
	{
		try {
			new String ((sbyte*) (-1), 1, 0);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			new String ((sbyte*) (-1), 1, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1), 0, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "ptr", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32)
	public unsafe void Constructor7_Value_Null ()
	{
#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 0);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#A", String.Empty, new String ((sbyte*) null, 0, 0));
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 1);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#B", String.Empty, new String ((sbyte*) null, 0, 1));
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 1, 0);
			Fail ("#C1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#C2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#C", String.Empty, new String ((sbyte*) null, 1, 0));
#endif
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Length_Negative ()
	{
		try {
			new String ((sbyte*) null, 0, -1, null);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "length", ex.ParamName);
		}

		try {
			new String ((sbyte*) null, 0, -1, Encoding.Default);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Non-negative number required
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "length", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_StartIndex_Negative ()
	{
		try {
			new String ((sbyte*) null, -1, 0, null);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			new String ((sbyte*) null, -1, 0, Encoding.Default);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public unsafe void Constructor8_StartIndex_Overflow ()
	{
		try {
			new String ((sbyte*) (-1), 1, 0, null);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			new String ((sbyte*) (-1), 1, 1, null);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}

		try {
			new String ((sbyte*) (-1), 1, 0, Encoding.Default);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "startIndex", ex.ParamName);
		}

		try {
			new String ((sbyte*) (-1), 1, 1, Encoding.Default);
			Fail ("#D1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#D2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#D3", ex.InnerException);
			AssertNotNull ("#D4", ex.Message);
			AssertEquals ("#D5", "startIndex", ex.ParamName);
		}
	}

	[Test] // ctor (SByte*, Int32, Int32, Encoding)
	public unsafe void Constructor8_Value_Invalid ()
	{
		try {
			new String ((sbyte*) (-1), 0, 1, null);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "ptr", ex.ParamName);
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
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#A", String.Empty, new String ((sbyte*) null, 0, 0, null));
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 0, 1, null);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#B", String.Empty, new String ((sbyte*) null, 0, 1, null));
#endif

#if NET_2_0
		try {
			new String ((sbyte*) null, 1, 0, null);
			Fail ("#C1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#C2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "value", ex.ParamName);
		}
#else
		AssertEquals ("#C", String.Empty, new String ((sbyte*) null, 1, 0, null));
#endif

		AssertEquals ("#D", String.Empty, new String ((sbyte*) null, 0, 0, Encoding.Default));

		try {
			new String ((sbyte*) null, 0, 1, Encoding.Default);
			Fail ("#E1");
#if NET_2_0
		} catch (ArgumentOutOfRangeException ex) {
			// Pointer startIndex and length do not refer to a
			// valid string
			AssertEquals ("#E2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#E3", ex.InnerException);
			AssertNotNull ("#E4", ex.Message);
			//AssertEquals ("#E5", "value", ex.ParamName);
		}
#else
		} catch (NullReferenceException ex) {
			AssertEquals ("#E2", typeof (NullReferenceException), ex.GetType ());
			AssertNull ("#E3", ex.InnerException);
			AssertNotNull ("#E4", ex.Message);
		}
#endif

		AssertEquals ("#F", String.Empty, new String ((sbyte*) null, 1, 0, Encoding.Default));
	}
#endif

	[Test]
	public void Length ()
	{
		string str = "test string";

		AssertEquals("wrong length", 11, str.Length);
	}

	[Test]
	public void Clone ()
	{
		string s1 = "oRiGiNal";
		AssertEquals ("#A1", s1, s1.Clone ());
		AssertSame ("#A2", s1, s1.Clone ());

		string s2 = new DateTime (2000, 6, 3).ToString ();
		AssertEquals ("#B1", s2, s2.Clone ());
		AssertSame ("#B2", s2, s2.Clone ());
	}

	[Test] // bug #316666
	public void CompareNotWorking ()
	{
		AssertEquals ("A03", String.Compare ("A", "a"), 1);
		AssertEquals ("A04", String.Compare ("a", "A"), -1);
	}

	[Test]
	public void CompareNotWorking2 ()
	{
		string needle = "ab";
		string haystack = "abbcbacab";
		AssertEquals("basic substring check #9", 0, 
			     String.Compare(needle, 0, haystack, 0, 2, false));
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				AssertEquals("loop substring check #8/" + i, -1, String.Compare(needle, 0, haystack, i, 2, false));
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

		AssertEquals(0, String.Compare (null, null));
		AssertEquals(1, String.Compare (lesser, null));

		Assert (String.Compare (lesser, greater) < 0);
		Assert (String.Compare (greater, lesser) > 0);
		Assert (String.Compare (lesser, lesser) == 0);
		Assert (String.Compare (lesser, medium) < 0);

		Assert (String.Compare (lesser, caps, true) == 0);
		Assert (String.Compare (lesser, caps, false) != 0);
		AssertEquals ("A01", String.Compare ("a", "b"), -1);
		AssertEquals ("A02", String.Compare ("b", "a"), 1);


		// TODO - test with CultureInfo

		string needle = "ab";
		string haystack = "abbcbacab";
		AssertEquals("basic substring check #1", 0, 
			     String.Compare(needle, 0, haystack, 0, 2));
		AssertEquals("basic substring check #2", -1,
			     String.Compare(needle, 0, haystack, 0, 3));
		AssertEquals("basic substring check #3", 0, 
			     String.Compare("ab", 0, "ab", 0, 2));
		AssertEquals("basic substring check #4", 0, 
			     String.Compare("ab", 0, "ab", 0, 3));
		AssertEquals("basic substring check #5", 0, 
			     String.Compare("abc", 0, "ab", 0, 2));
		AssertEquals("basic substring check #6", 1, 
			     String.Compare("abc", 0, "ab", 0, 5));
		AssertEquals("basic substring check #7", -1, 
			     String.Compare("ab", 0, "abc", 0, 5));

		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert("loop substring check #1/" + i, String.Compare(needle, 0, haystack, i, 2) != 0);
				Assert("loop substring check #2/" + i, String.Compare(needle, 0, haystack, i, 3) != 0);
			} else {
				AssertEquals("loop substring check #3/" + i, 0, String.Compare(needle, 0, haystack, i, 2));
				AssertEquals("loop substring check #4/" + i, 0, String.Compare(needle, 0, haystack, i, 3));
			}
		}

		needle = "AB";
		AssertEquals("basic substring check #8", 0, 
			     String.Compare(needle, 0, haystack, 0, 2, true));
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert("loop substring check #5/" + i, String.Compare(needle, 0, haystack, i, 2, true) != 0);
				Assert("loop substring check #6/" + i, String.Compare(needle, 0, haystack, i, 2, false) != 0);
			} else {
				AssertEquals("loop substring check #7/" + i, 0, String.Compare(needle, 0, haystack, i, 2, true));
			}
		}

		AssertEquals ("Compare with 0 length", 0, String.Compare (needle, 0, haystack, 0, 0));

		// TODO - extended format call with CultureInfo
	}

	[Test]
	public void CompareOrdinal ()
	{
		string lesser = "abc";
		string medium = "abcd";
		string greater = "xyz";

		AssertEquals(0, String.CompareOrdinal (null, null));
		AssertEquals(1, String.CompareOrdinal (lesser, null));

		Assert ("#1", String.CompareOrdinal (lesser, greater) < 0);
		Assert ("#2", String.CompareOrdinal (greater, lesser) > 0);
		Assert ("#3", String.CompareOrdinal (lesser, lesser) == 0);
		Assert ("#4", String.CompareOrdinal (lesser, medium) < 0);

		string needle = "ab";
		string haystack = "abbcbacab";
		AssertEquals("basic substring check", 0, 
			     String.CompareOrdinal(needle, 0, haystack, 0, 2));
		AssertEquals("basic substring miss", -1,
			     String.CompareOrdinal(needle, 0, haystack, 0, 3));
		for (int i = 1; i <= (haystack.Length - needle.Length); i++) {
			if (i != 7) {
				Assert("loop substring check " + i, String.CompareOrdinal(needle, 0, haystack, i, 2) != 0);
				Assert("loop substring check " + i, String.CompareOrdinal(needle, 0, haystack, i, 3) != 0);
			} else {
				AssertEquals("loop substring check " + i, 0, String.CompareOrdinal(needle, 0, haystack, i, 2));
				AssertEquals("loop substring check " + i, 0, String.CompareOrdinal(needle, 0, haystack, i, 3));
			}
		}
	}

	[Test]
	public void CompareTo ()
	{
		string lower = "abc";
		string greater = "xyz";
		string lesser = "abc";
		
		Assert (lower.CompareTo (greater) < 0);
		Assert (lower.CompareTo (lower) == 0);
		Assert (greater.CompareTo (lesser) > 0);
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

		Assert (String.Concat (string1, string2) == concat);
		
		AssertEquals (string1, String.Concat (string1, null));
		AssertEquals (string1, String.Concat (null, string1));
		AssertEquals (string.Empty, String.Concat (null, null));
		
		WeirdToString wts = new WeirdToString ();
		AssertEquals (string1, String.Concat (string1, wts));
		AssertEquals (string1, String.Concat (wts, string1));
		AssertEquals (string.Empty, String.Concat (wts, wts));
		string [] allstr = new string []{ string1, null, string2, concat };
		object [] allobj = new object []{ string1, null, string2, concat };
		string astr = String.Concat (allstr);
		AssertEquals ("string1string2string1string2", astr);
		string ostr = String.Concat (allobj);
		AssertEquals (astr, ostr);
	}

	[Test]
	public void Copy ()
	{
		string s1 = "original";
		string s2 = String.Copy(s1);
		AssertEquals("#1", s1, s2);
		Assert ("#2", !object.ReferenceEquals (s1, s2));
	}

	[Test]
	public void Copy_Str_Null ()
	{
		try {
			String.Copy ((string) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "str", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo ()
	{
		string s1 = "original";
		char[] c1 = new char[s1.Length];
		string s2 = new String(c1);
		Assert("#1", !s1.Equals(s2));
		for (int i = 0; i < s1.Length; i++) {
			s1.CopyTo(i, c1, i, 1);
		}
		s2 = new String(c1);
		AssertEquals("#2", s1, s2);
	}

	[Test]
	public void CopyTo_Count_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, 0, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_Count_Overflow ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, 0, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "sourceIndex", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_Destination_Null ()
	{
		string s = "original";

		try {
			s.CopyTo (0, (char []) null, 0, s.Length);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "destination", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_DestinationIndex_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, -1, 4);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "destinationIndex", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_DestinationIndex_Overflow ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (0, dest, Int32.MaxValue, 4);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "destinationIndex", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_SourceIndex_Negative ()
	{
		char [] dest = new char [4];
		try {
			"Mono".CopyTo (-1, dest, 0, 4);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "sourceIndex", ex.ParamName);
		}
	}

	[Test]
	public void CopyTo_SourceIndex_Overflow ()
	{
		char[] dest = new char [4];
		try {
			"Mono".CopyTo (Int32.MaxValue, dest, 0, 4);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "sourceIndex", ex.ParamName);
		}
	}

	[Test] // EndsWith (String)
	public void EndsWith1 ()
	{
		string s;

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");
		s = "AbC";

		Assert ("#A1", s.EndsWith ("bC"));
		Assert ("#A1", !s.EndsWith ("bc"));
		Assert ("#A2", !s.EndsWith ("dc"));
		Assert ("#A3", !s.EndsWith ("LAbC"));
		Assert ("#A4", s.EndsWith (string.Empty));
		Assert ("#A5", !s.EndsWith ("Ab"));
		Assert ("#A6", !s.EndsWith ("Abc"));
		Assert ("#A7", s.EndsWith ("AbC"));

		s = "Tai";

		Assert ("#B1", s.EndsWith ("ai"));
		Assert ("#B2", !s.EndsWith ("AI"));
		Assert ("#B3", !s.EndsWith ("LTai"));
		Assert ("#B4", s.EndsWith (string.Empty));
		Assert ("#B5", !s.EndsWith ("Ta"));
		Assert ("#B6", !s.EndsWith ("tai"));
		Assert ("#B7", s.EndsWith ("Tai"));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		Assert ("#C1", s.EndsWith ("ai"));
		Assert ("#C2", !s.EndsWith ("AI"));
		Assert ("#C3", !s.EndsWith ("LTai"));
		Assert ("#C4", s.EndsWith (string.Empty));
		Assert ("#C5", !s.EndsWith ("Ta"));
		Assert ("#C6", !s.EndsWith ("tai"));
		Assert ("#C7", s.EndsWith ("Tai"));
	}

	[Test] // EndsWith (String)
	public void EndsWith1_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
		}
	}

#if NET_2_0
	[Test] // EndsWith (String, StringComparison)
	public void EndsWith2_ComparisonType_Invalid ()
	{
		try {
			"ABC".EndsWith ("C", (StringComparison) 80);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "comparisonType", ex.ParamName);
		}
	}

	[Test] // EndsWith (String, StringComparison)
	public void EndsWith2_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null, StringComparison.CurrentCulture);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
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
		Assert ("#A1", !s.EndsWith ("bc", ignorecase, culture));
		Assert ("#A2", !s.EndsWith ("dc", ignorecase, culture));
		Assert ("#A3", !s.EndsWith ("LAbC", ignorecase, culture));
		Assert ("#A4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#A5", !s.EndsWith ("Ab", ignorecase, culture));
		Assert ("#A6", !s.EndsWith ("Abc", ignorecase, culture));
		Assert ("#A7", s.EndsWith ("AbC", ignorecase, culture));

		ignorecase = true;
		Assert ("#B1", s.EndsWith ("bc", ignorecase, culture));
		Assert ("#B2", !s.EndsWith ("dc", ignorecase, culture));
		Assert ("#B3", !s.EndsWith ("LAbC", ignorecase, culture));
		Assert ("#B4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#B5", !s.EndsWith ("Ab", ignorecase, culture));
		Assert ("#B6", s.EndsWith ("Abc", ignorecase, culture));
		Assert ("#B7", s.EndsWith ("AbC", ignorecase, culture));

		s = "Tai";
		culture = null;

		ignorecase = false;
		Assert ("#C1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#C2", !s.EndsWith ("AI", ignorecase, culture));
		Assert ("#C3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#C4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#C5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#C6", !s.EndsWith ("tai", ignorecase, culture));
		Assert ("#C7", s.EndsWith ("Tai", ignorecase, culture));

		ignorecase = true;
		Assert ("#D1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#D2", !s.EndsWith ("AI", ignorecase, culture));
		Assert ("#D3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#D4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#D5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#D6", s.EndsWith ("tai", ignorecase, culture));
		Assert ("#D7", s.EndsWith ("Tai", ignorecase, culture));

		s = "Tai";
		culture = new CultureInfo ("en-US");

		ignorecase = false;
		Assert ("#E1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#E2", !s.EndsWith ("AI", ignorecase, culture));
		Assert ("#E3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#E4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#E5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#E6", !s.EndsWith ("tai", ignorecase, culture));
		Assert ("#E7", s.EndsWith ("Tai", ignorecase, culture));

		ignorecase = true;
		Assert ("#F1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#F2", s.EndsWith ("AI", ignorecase, culture));
		Assert ("#F3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#F4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#F5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#F6", s.EndsWith ("tai", ignorecase, culture));
		Assert ("#F7", s.EndsWith ("Tai", ignorecase, culture));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		culture = null;

		ignorecase = false;
		Assert ("#G1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#G2", !s.EndsWith ("AI", ignorecase, culture));
		Assert ("#G3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#G4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#G5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#G6", !s.EndsWith ("tai", ignorecase, culture));
		Assert ("#G7", s.EndsWith ("Tai", ignorecase, culture));

		ignorecase = true;
		Assert ("#H1", s.EndsWith ("ai", ignorecase, culture));
		Assert ("#H2", s.EndsWith ("AI", ignorecase, culture));
		Assert ("#H3", !s.EndsWith ("LTai", ignorecase, culture));
		Assert ("#H4", s.EndsWith (string.Empty, ignorecase, culture));
		Assert ("#H5", !s.EndsWith ("Ta", ignorecase, culture));
		Assert ("#H6", s.EndsWith ("tai", ignorecase, culture));
		Assert ("#H7", s.EndsWith ("Tai", ignorecase, culture));
	}

	[Test] // EndsWith (String, Boolean, CultureInfo)
	public void EndsWith3_Value_Null ()
	{
		try {
			"ABC".EndsWith ((string) null, true, null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
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

		Assert("No match for null", !s1.Equals (null));
		Assert("Should match object", s1.Equals (y));
		Assert("Should match", s1.Equals (yes));
		Assert("Shouldn't match", !s1.Equals (no));

		Assert("Static nulls should match", String.Equals (null, null));
		Assert("Should match", String.Equals (s1, yes));
		Assert("Shouldn't match", !String.Equals (s1, no));

		AssertEquals ("Equals (object)", false, s1s1.Equals (y));
	}

	[Test]
	public void TestFormat ()
	{
		AssertEquals ("Empty format string.", string.Empty, String.Format (string.Empty, 0));
		AssertEquals ("Single argument.", "100", String.Format ("{0}", 100));
		AssertEquals ("Single argument, right justified.", "X   37X", String.Format ("X{0,5}X", 37));
		AssertEquals ("Single argument, left justified.", "X37   X", String.Format ("X{0,-5}X", 37));
		AssertEquals ("Whitespace in specifier", "  7d", String.Format ("{0, 4:x}", 125));
		AssertEquals ("Two arguments.", "The 3 wise men.", String.Format ("The {0} wise {1}.", 3, "men"));
		AssertEquals ("Three arguments.", "do re me fa so.", String.Format ("{0} re {1} fa {2}.", "do", "me", "so"));
		AssertEquals ("Formatted argument.", "###00c0ffee#", String.Format ("###{0:x8}#", 0xc0ffee));
		AssertEquals ("Formatted argument, right justified.", "#  033#", String.Format ("#{0,5:x3}#", 0x33));
		AssertEquals ("Formatted argument, left justified.", "#033  #", String.Format ("#{0,-5:x3}#", 0x33));
		AssertEquals ("Escaped bracket", "typedef struct _MonoObject { ... } MonoObject;", String.Format ("typedef struct _{0} {{ ... }} MonoObject;", "MonoObject"));
		AssertEquals ("With Slash", "Could not find file \"a/b\"", String.Format ("Could not find file \"{0}\"", "a/b"));
		AssertEquals ("With BackSlash", "Could not find file \"a\\b\"", String.Format ("Could not find file \"{0}\"", "a\\b"));
	}

	[Test] // Format (String, Object)
	public void Format1_Format_Null ()
	{
		try {
			String.Format (null, 1);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "format", ex.ParamName);
		}
	}

	[Test] // Format (String, Object [])
	public void Format2_Format_Null ()
	{
		try {
			String.Format (null, new object [] { 2 });
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "format", ex.ParamName);
		}
	}

	[Test] // Format (String, Object [])
	public void Format2_Args_Null ()
	{
		try {
			String.Format ("text", (object []) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "args", ex.ParamName);
		}
	}

	[Test] // Format (IFormatProvider, String, Object [])
	public void Format3_Format_Null ()
	{
		try {
			String.Format (CultureInfo.InvariantCulture, null,
				new object [] { 3 });
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "format", ex.ParamName);
		}
	}

	[Test] // Format (IFormatProvider, String, Object [])
	public void Format3_Args_Null ()
	{
		try {
			String.Format (CultureInfo.InvariantCulture, "text",
				(object []) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "args", ex.ParamName);
		}
	}

	[Test] // Format (String, Object, Object)
	public void Format4_Format_Null ()
	{
		try {
			String.Format (null, 4, 5);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "format", ex.ParamName);
		}
	}

	[Test] // Format (String, Object, Object, Object)
	public void Format5_Format_Null ()
	{
		try {
			String.Format (null, 4, 5, 6);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "format", ex.ParamName);
		}
	}

	[Test]
	public void TestGetEnumerator ()
	{
		string s1 = "original";
		char[] c1 = new char[s1.Length];
		string s2 = new String(c1);
		Assert("pre-enumerated string should not match", !s1.Equals(s2));
		CharEnumerator en = s1.GetEnumerator();
		AssertNotNull("null enumerator", en);
		
		for (int i = 0; i < s1.Length; i++) {
			en.MoveNext();
			c1[i] = en.Current;
		}
		s2 = new String(c1);
		AssertEquals("enumerated string should match", s1, s2);
	}

	[Test]
	public void TestGetHashCode ()
	{
		string s1 = "original";
		// TODO - weak test, currently.  Just verifies determinicity.
		AssertEquals("same string, same hash code", 
			     s1.GetHashCode(), s1.GetHashCode());
	}

	[Test]
	public void TestGetType ()
	{
		string s1 = "original";
		AssertEquals("String type", "System.String", s1.GetType().ToString());
	}

	[Test]
	public void TestGetTypeCode ()
	{
		string s1 = "original";
		Assert(s1.GetTypeCode().Equals(TypeCode.String));
	}

	[Test]
	public void IndexOf ()
	{
		string s1 = "original";

		try {
			s1.IndexOf ('q', s1.Length + 1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			s1.IndexOf ('q', s1.Length + 1, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}

		try {
			s1.IndexOf ("huh", s1.Length + 1);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "startIndex", ex.ParamName);
		}

		AssertEquals("basic char index", 1, s1.IndexOf('r'));
		AssertEquals("basic char index 2", 2, s1.IndexOf('i'));
		AssertEquals("basic char index - no", -1, s1.IndexOf('q'));
		
		AssertEquals("basic string index", 1, s1.IndexOf("rig"));
		AssertEquals("basic string index 2", 2, s1.IndexOf("i"));
		AssertEquals("basic string index 3", 0, string.Empty.IndexOf(string.Empty));
		AssertEquals("basic string index 4", 0, "ABC".IndexOf(string.Empty));
		AssertEquals("basic string index - no", -1, s1.IndexOf("rag"));

		AssertEquals("stepped char index", 1, s1.IndexOf('r', 1));
		AssertEquals("stepped char index 2", 2, s1.IndexOf('i', 1));
		AssertEquals("stepped char index 3", 4, s1.IndexOf('i', 3));
		AssertEquals("stepped char index 4", -1, s1.IndexOf('i', 5));
		AssertEquals("stepped char index 5", -1, s1.IndexOf('l', s1.Length));

		AssertEquals("stepped limited char index", 
			     1, s1.IndexOf('r', 1, 1));
		AssertEquals("stepped limited char index", 
			     -1, s1.IndexOf('r', 0, 1));
		AssertEquals("stepped limited char index", 
			     2, s1.IndexOf('i', 1, 3));
		AssertEquals("stepped limited char index", 
			     4, s1.IndexOf('i', 3, 3));
		AssertEquals("stepped limited char index", 
			     -1, s1.IndexOf('i', 5, 3));

		s1 = "original original";
		AssertEquals("stepped string index 1",
			     0, s1.IndexOf("original", 0));
		AssertEquals("stepped string index 2", 
			     9, s1.IndexOf("original", 1));
		AssertEquals("stepped string index 3", 
			     -1, s1.IndexOf("original", 10));
		AssertEquals("stepped string index 4", 
					 3, s1.IndexOf(string.Empty, 3));
		AssertEquals("stepped limited string index 1",
			     1, s1.IndexOf("rig", 0, 5));
		AssertEquals("stepped limited string index 2",
			     -1, s1.IndexOf("rig", 0, 3));
		AssertEquals("stepped limited string index 3",
			     10, s1.IndexOf("rig", 2, 15));
		AssertEquals("stepped limited string index 4",
			     -1, s1.IndexOf("rig", 2, 3));
		AssertEquals("stepped limited string index 5",
			     2, s1.IndexOf(string.Empty, 2, 3));
		
		string s2 = "QBitArray::bitarr_data"; 
		AssertEquals ("bug #62160", 9, s2.IndexOf ("::"));
	}

	[Test] // IndexOf (String)
	public void IndexOf2_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
#if NET_2_0
			AssertEquals ("#5", "value", ex.ParamName);
#else
			//Fixme: Does it really make sense to check for obsolete
			//       parameter names. Then case this in string.
			//AssertEquals ("#5", "string2", ex.ParamName);
#endif
		}
	}

	[Test] // IndexOf (Char, Int32)
	public void IndexOf3 ()
	{
		string s = "testing123456";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#A1", -1, s.IndexOf ('a', s.Length));
		AssertEquals ("#A2", -1, s.IndexOf ('6', s.Length));
		AssertEquals ("#A3", -1, s.IndexOf ('t', s.Length));
		AssertEquals ("#A4", -1, s.IndexOf ('T', s.Length));
		AssertEquals ("#A5", -1, s.IndexOf ('i', s.Length));
		AssertEquals ("#A6", -1, s.IndexOf ('I', s.Length));
		AssertEquals ("#A7", -1, s.IndexOf ('q', s.Length));
		AssertEquals ("#A8", -1, s.IndexOf ('3', s.Length));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#B1", -1, s.IndexOf ('a', s.Length));
		AssertEquals ("#B2", -1, s.IndexOf ('6', s.Length));
		AssertEquals ("#B3", -1, s.IndexOf ('t', s.Length));
		AssertEquals ("#B4", -1, s.IndexOf ('T', s.Length));
		AssertEquals ("#B5", -1, s.IndexOf ('i', s.Length));
		AssertEquals ("#B6", -1, s.IndexOf ('I', s.Length));
		AssertEquals ("#B7", -1, s.IndexOf ('q', s.Length));
		AssertEquals ("#B8", -1, s.IndexOf ('3', s.Length));
	}

	[Test] // IndexOf (String, Int32)
	public void IndexOf4 ()
	{
		string s = "testing123456";

		AssertEquals ("#1", -1, s.IndexOf ("IN", 3));
		AssertEquals ("#2", 4, s.IndexOf ("in", 3));
		AssertEquals ("#3", -1, s.IndexOf ("in", 5));
		AssertEquals ("#4", 7, s.IndexOf ("1", 5));
		AssertEquals ("#5", 12, s.IndexOf ("6", 12));
		AssertEquals ("#6", 0, s.IndexOf ("testing123456", 0));
		AssertEquals ("#7", -1, s.IndexOf ("testing123456", 1));
		AssertEquals ("#8", 5, s.IndexOf (string.Empty, 5));
		AssertEquals ("#9", 0, s.IndexOf (string.Empty, 0));
	}

	[Test] // IndexOf (String, Int32)
	public void IndexOf4_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, 1);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
#if NET_2_0
			AssertEquals ("#5", "value", ex.ParamName);
#else
			AssertEquals ("#5", "string2", ex.ParamName);
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
		AssertEquals ("#A1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#A2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#A3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#A4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#A5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#A6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#A7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#A8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#A9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.CurrentCultureIgnoreCase;
		AssertEquals ("#B1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#B2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#B3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#B4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#B5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#B6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#B7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#B8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#B9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.InvariantCulture;
		AssertEquals ("#C1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#C2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#C3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#C4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#C5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#C6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#C7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#C8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#C9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.InvariantCultureIgnoreCase;
		AssertEquals ("#D1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#D2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#D3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#D4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#D5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#D6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#D7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#D8", 3, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#D9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.Ordinal;
		AssertEquals ("#E1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#E2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#E3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#E4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#E5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#E6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#E7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#E8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#E9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.OrdinalIgnoreCase;
		AssertEquals ("#F1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#F2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#F3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#F4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#F5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#F6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#F7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#F8", 3, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#F9", 0, s.IndexOf (string.Empty, comparison_type));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		comparison_type = StringComparison.CurrentCulture;
		AssertEquals ("#G1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#G2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#G3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#G4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#G5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#G6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#G7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#G8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#G9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.CurrentCultureIgnoreCase;
		AssertEquals ("#H1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#H2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#H3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#H4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#H5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#H6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#H7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#H8", 3, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#H9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.InvariantCulture;
		AssertEquals ("#I1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#I2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#I3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#I4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#I5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#I6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#I7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#I8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#I9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.InvariantCultureIgnoreCase;
		AssertEquals ("#J1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#J2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#J3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#J4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#J5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#J6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#J7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#J8", 3, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#J9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.Ordinal;
		AssertEquals ("#K1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#K2", -1, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#K3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#K4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#K5", -1, s.IndexOf ("T", comparison_type));
		AssertEquals ("#K6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#K7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#K8", -1, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#K9", 0, s.IndexOf (string.Empty, comparison_type));

		comparison_type = StringComparison.OrdinalIgnoreCase;
		AssertEquals ("#L1", 7, s.IndexOf ("123", comparison_type));
		AssertEquals ("#L2", 5, s.IndexOf ("NG", comparison_type));
		AssertEquals ("#L3", -1, s.IndexOf ("nga", comparison_type));
		AssertEquals ("#L4", 0, s.IndexOf ("t", comparison_type));
		AssertEquals ("#L5", 0, s.IndexOf ("T", comparison_type));
		AssertEquals ("#L6", 12, s.IndexOf ("6", comparison_type));
		AssertEquals ("#L7", 3, s.IndexOf ("tin", comparison_type));
		AssertEquals ("#L8", 3, s.IndexOf ("TIN", comparison_type));
		AssertEquals ("#L9", 0, s.IndexOf (string.Empty, comparison_type));

		AssertEquals ("#M", 0, string.Empty.IndexOf (string.Empty, comparison_type));
	}

	[Test] // IndexOf (String, StringComparison)
	public void IndexOf5_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, (StringComparison) Int32.MinValue);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "comparisonType", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, StringComparison)
	public void IndexOf5_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, StringComparison.Ordinal);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
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
	public void IndexOfStringComparison ()
	{
		string text = "testing123456";
		string text2 = "123";
		string text3 = "NG";
		string text4 = "t";
		AssertEquals ("#1-1", 7, text.IndexOf (text2, StringComparison.Ordinal));
		AssertEquals ("#2-1", 5, text.IndexOf (text3, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-2", 7, text.IndexOf (text2, 0, StringComparison.Ordinal));
		AssertEquals ("#2-2", 5, text.IndexOf (text3, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-3", 7, text.IndexOf (text2, 1, StringComparison.Ordinal));
		AssertEquals ("#2-3", 5, text.IndexOf (text3, 1, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-4", 7, text.IndexOf (text2, 6, StringComparison.Ordinal));
		AssertEquals ("#2-4", -1, text.IndexOf (text3, 6, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-5", 7, text.IndexOf (text2, 7, 3, StringComparison.Ordinal));
		AssertEquals ("#2-5", -1, text.IndexOf (text3, 7, 3, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-6", -1, text.IndexOf (text2, 6, 0, StringComparison.Ordinal));
		AssertEquals ("#2-6", -1, text.IndexOf (text3, 5, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-7", -1, text.IndexOf (text2, 7, 1, StringComparison.Ordinal));
		AssertEquals ("#2-7", -1, text.IndexOf (text3, 5, 1, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#3-1", 0, text.IndexOf (text4, 0, StringComparison.Ordinal));
		AssertEquals ("#3-2", 0, text.IndexOf (text4, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#4-1", -1, text.IndexOf (text4, 13, StringComparison.Ordinal));
		AssertEquals ("#4-2", -1, text.IndexOf (text4, 13, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#4-1", -1, text.IndexOf (text4, 13, 0, StringComparison.Ordinal));
		AssertEquals ("#4-2", -1, text.IndexOf (text4, 13, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#5-1", 12, text.IndexOf ("6", 12, 1, StringComparison.Ordinal));
		AssertEquals ("#5-2", 12, text.IndexOf ("6", 12, 1, StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void IndexOfStringComparisonOrdinal ()
	{
		string text = "testing123456";
		AssertEquals ("#1", 10, text.IndexOf ("456", StringComparison.Ordinal));
		AssertEquals ("#2", -1, text.IndexOf ("4567", StringComparison.Ordinal));
		AssertEquals ("#3", 0, text.IndexOf ("te", StringComparison.Ordinal));
		AssertEquals ("#4", 2, text.IndexOf ("s", StringComparison.Ordinal));
		AssertEquals ("#5", -1, text.IndexOf ("ates", StringComparison.Ordinal));
		AssertEquals ("#6", -1, text.IndexOf ("S", StringComparison.Ordinal));
	}

	[Test]
	public void IndexOfStringComparisonOrdinalIgnoreCase ()
	{
		string text = "testing123456";
		AssertEquals ("#1", 10, text.IndexOf ("456", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#2", -1, text.IndexOf ("4567", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#3", 0, text.IndexOf ("te", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#4", 2, text.IndexOf ("s", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#5", -1, text.IndexOf ("ates", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#6", 2, text.IndexOf ("S", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void IndexOfOrdinalCountSmallerThanValueString ()
	{
		AssertEquals ("#1", -1, "Test".IndexOf ("ST", 2, 1, StringComparison.Ordinal));
		AssertEquals ("#2", -1, "Test".IndexOf ("ST", 2, 1, StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#3", -1, "Test".LastIndexOf ("ST", 2, 1, StringComparison.Ordinal));
		AssertEquals ("#4", -1, "Test".LastIndexOf ("ST", 2, 1, StringComparison.OrdinalIgnoreCase));
	}
#endif

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_Count_Negative ()
	{
		try {
			"Mono".IndexOf ('o', 1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_Count_Overflow ()
	{
		try {
			"Mono".IndexOf ('o', 1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ('o', -1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // IndexOf (Char, Int32, Int32)
	public void IndexOf6_StartIndex_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ('o', s.Length + 1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7 ()
	{
		string s = "testing123456test";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#A1", -1, s.IndexOf ("123", 4, 5));
		AssertEquals ("#A2", 7, s.IndexOf ("123", 4, 6));
		AssertEquals ("#A3", -1, s.IndexOf ("123", 5, 4));
		AssertEquals ("#A4", 7, s.IndexOf ("123", 5, 5));
		AssertEquals ("#A5", 7, s.IndexOf ("123", 0, s.Length));
		AssertEquals ("#A6", -1, s.IndexOf ("123", s.Length, 0));

		AssertEquals ("#B1", -1, s.IndexOf ("tin", 2, 3));
		AssertEquals ("#B2", 3, s.IndexOf ("tin", 3, 3));
		AssertEquals ("#B3", -1, s.IndexOf ("tin", 2, 2));
		AssertEquals ("#B4", -1, s.IndexOf ("tin", 1, 4));
		AssertEquals ("#B5", 3, s.IndexOf ("tin", 0, s.Length));
		AssertEquals ("#B6", -1, s.IndexOf ("tin", s.Length, 0));

		AssertEquals ("#C1", 6, s.IndexOf ("g12", 4, 5));
		AssertEquals ("#C2", -1, s.IndexOf ("g12", 5, 2));
		AssertEquals ("#C3", -1, s.IndexOf ("g12", 5, 3));
		AssertEquals ("#C4", 6, s.IndexOf ("g12", 6, 4));
		AssertEquals ("#C5", 6, s.IndexOf ("g12", 0, s.Length));
		AssertEquals ("#C6", -1, s.IndexOf ("g12", s.Length, 0));

		AssertEquals ("#D1", 1, s.IndexOf ("est", 0, 5));
		AssertEquals ("#D2", -1, s.IndexOf ("est", 1, 2));
		AssertEquals ("#D3", -1, s.IndexOf ("est", 2, 10));
		AssertEquals ("#D4", 14, s.IndexOf ("est", 7, 10));
		AssertEquals ("#D5", 1, s.IndexOf ("est", 0, s.Length));
		AssertEquals ("#D6", -1, s.IndexOf ("est", s.Length, 0));

		AssertEquals ("#E1", -1, s.IndexOf ("T", 0, s.Length));
		AssertEquals ("#E2", 4, s.IndexOf ("i", 0, s.Length));
		AssertEquals ("#E3", -1, s.IndexOf ("I", 0, s.Length));
		AssertEquals ("#E4", 12, s.IndexOf ("6", 0, s.Length));
		AssertEquals ("#E5", 0, s.IndexOf ("testing123456", 0, s.Length));
		AssertEquals ("#E6", -1, s.IndexOf ("testing1234567", 0, s.Length));
		AssertEquals ("#E7", 0, s.IndexOf (string.Empty, 0, 0));
		AssertEquals ("#E8", 4, s.IndexOf (string.Empty, 4, 3));
		AssertEquals ("#E9", 0, string.Empty.IndexOf (string.Empty, 0, 0));
		AssertEquals ("#E10", -1, string.Empty.IndexOf ("abc", 0, 0));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#F1", -1, s.IndexOf ("123", 4, 5));
		AssertEquals ("#F2", 7, s.IndexOf ("123", 4, 6));
		AssertEquals ("#F3", -1, s.IndexOf ("123", 5, 4));
		AssertEquals ("#F4", 7, s.IndexOf ("123", 5, 5));
		AssertEquals ("#F5", 7, s.IndexOf ("123", 0, s.Length));
		AssertEquals ("#F6", -1, s.IndexOf ("123", s.Length, 0));

		AssertEquals ("#G1", -1, s.IndexOf ("tin", 2, 3));
		AssertEquals ("#G2", 3, s.IndexOf ("tin", 3, 3));
		AssertEquals ("#G3", -1, s.IndexOf ("tin", 2, 2));
		AssertEquals ("#G4", -1, s.IndexOf ("tin", 1, 4));
		AssertEquals ("#G5", 3, s.IndexOf ("tin", 0, s.Length));
		AssertEquals ("#G6", -1, s.IndexOf ("tin", s.Length, 0));

		AssertEquals ("#H1", 6, s.IndexOf ("g12", 4, 5));
		AssertEquals ("#H2", -1, s.IndexOf ("g12", 5, 2));
		AssertEquals ("#H3", -1, s.IndexOf ("g12", 5, 3));
		AssertEquals ("#H4", 6, s.IndexOf ("g12", 6, 4));
		AssertEquals ("#H5", 6, s.IndexOf ("g12", 0, s.Length));
		AssertEquals ("#H6", -1, s.IndexOf ("g12", s.Length, 0));

		AssertEquals ("#I1", 1, s.IndexOf ("est", 0, 5));
		AssertEquals ("#I2", -1, s.IndexOf ("est", 1, 2));
		AssertEquals ("#I3", -1, s.IndexOf ("est", 2, 10));
		AssertEquals ("#I4", 14, s.IndexOf ("est", 7, 10));
		AssertEquals ("#I5", 1, s.IndexOf ("est", 0, s.Length));
		AssertEquals ("#I6", -1, s.IndexOf ("est", s.Length, 0));

		AssertEquals ("#J1", -1, s.IndexOf ("T", 0, s.Length));
		AssertEquals ("#J2", 4, s.IndexOf ("i", 0, s.Length));
		AssertEquals ("#J3", -1, s.IndexOf ("I", 0, s.Length));
		AssertEquals ("#J4", 12, s.IndexOf ("6", 0, s.Length));
		AssertEquals ("#J5", 0, s.IndexOf ("testing123456", 0, s.Length));
		AssertEquals ("#J6", -1, s.IndexOf ("testing1234567", 0, s.Length));
		AssertEquals ("#J7", 0, s.IndexOf (string.Empty, 0, 0));
		AssertEquals ("#J8", 4, s.IndexOf (string.Empty, 4, 3));
		AssertEquals ("#J9", 0, string.Empty.IndexOf (string.Empty, 0, 0));
		AssertEquals ("#J10", -1, string.Empty.IndexOf ("abc", 0, 0));
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Count_Negative ()
	{
		try {
			"Mono".IndexOf ("no", 1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Count_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ("no", 1, s.Length);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "count", ex.ParamName);
		}

		try {
			s.IndexOf ("no", 1, s.Length + 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "count", ex.ParamName);
		}

		try {
			s.IndexOf ("no", 1, int.MaxValue);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
#if NET_2_0
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "count", ex.ParamName);
#else
			// Index was out of range.  Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertNotNull ("#C5", ex.ParamName);
			//AssertEquals ("#C5", "startIndex", ex.ParamName);
#endif
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("no", -1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_StartIndex_Overflow ()
	{
		string s = "testing123456";

		try {
			s.IndexOf ("no", s.Length + 1, 1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
#if NET_2_0
			AssertEquals ("#A5", "startIndex", ex.ParamName);
#else
			AssertNotNull ("#A5", ex.ParamName);
			//AssertEquals ("#A5", "count", ex.ParamName);
#endif
		}

		try {
			s.IndexOf ("no", int.MaxValue, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32)
	public void IndexOf7_Value_Null ()
	{
		try {
			"Mono".IndexOf ((string) null, 0, 1);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
#if NET_2_0
			AssertEquals ("#5", "value", ex.ParamName);
#else
			AssertEquals ("#5", "string2", ex.ParamName);
#endif
		}
	}

#if NET_2_0
	[Test] // IndexOf (String, Int32, StringComparison)
	public void IndexOf8_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, 1, (StringComparison) Int32.MinValue);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "comparisonType", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, StringComparison)
	public void IndexOf8_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("o", -1, StringComparison.Ordinal);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_ComparisonType_Invalid ()
	{
		try {
			"Mono".IndexOf (string.Empty, 0, 1, (StringComparison) Int32.MinValue);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "comparisonType", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_Count_Negative ()
	{
		try {
			"Mono".IndexOf ("o", 1, -1, StringComparison.Ordinal);
			Fail ("#1");
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOf (String, Int32, Int32, StringComparison)
	public void IndexOf9_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOf ("o", -1, 0, StringComparison.Ordinal);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}
#endif

	[Test]
	public void IndexOfAny1 ()
	{
		string s = "abcdefghijklmd";
		char[] c;

		c = new char [] {'a', 'e', 'i', 'o', 'u'};
		AssertEquals ("#1", 0, s.IndexOfAny (c));
		c = new char [] { 'd', 'z' };
		AssertEquals ("#1", 3, s.IndexOfAny (c));
		c = new char [] { 'q', 'm', 'z' };
		AssertEquals ("#2", 12, s.IndexOfAny (c));
		c = new char [0];
		AssertEquals ("#3", -1, s.IndexOfAny (c));

	}

	[Test] // IndexOfAny (Char [])
	public void IndexOfAny1_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2 ()
	{
		string s = "abcdefghijklmd";
		char [] c;

		c = new char [] { 'a', 'e', 'i', 'o', 'u' };
		AssertEquals ("#A1", 0, s.IndexOfAny (c, 0));
		AssertEquals ("#A1", 4, s.IndexOfAny (c, 1));
		AssertEquals ("#A2", -1, s.IndexOfAny (c, 9));
		AssertEquals ("#A3", -1, s.IndexOfAny (c, s.Length));

		c = new char [] { 'd', 'z' };
		AssertEquals ("#B1", 3, s.IndexOfAny (c, 0));
		AssertEquals ("#B2", 3, s.IndexOfAny (c, 3));
		AssertEquals ("#B3", 13, s.IndexOfAny (c, 4));
		AssertEquals ("#B4", 13, s.IndexOfAny (c, 9));
		AssertEquals ("#B5", -1, s.IndexOfAny (c, s.Length));
		AssertEquals ("#B6", 13, s.IndexOfAny (c, s.Length - 1));

		c = new char [] { 'q', 'm', 'z' };
		AssertEquals ("#C1", 12, s.IndexOfAny (c, 0));
		AssertEquals ("#C2", 12, s.IndexOfAny (c, 4));
		AssertEquals ("#C3", 12, s.IndexOfAny (c, 12));
		AssertEquals ("#C4", -1, s.IndexOfAny (c, s.Length));

		c = new char [0];
		AssertEquals ("#D1", -1, s.IndexOfAny (c, 0));
		AssertEquals ("#D2", -1, s.IndexOfAny (c, 4));
		AssertEquals ("#D3", -1, s.IndexOfAny (c, 9));
		AssertEquals ("#D4", -1, s.IndexOfAny (c, s.Length));
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null, 0);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32)
	public void IndexOfAny2_StartIndex_Negative ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, -1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny2_StartIndex_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, s.Length + 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3 ()
	{
		string s = "abcdefghijklmd";
		char [] c;

		c = new char [] { 'a', 'e', 'i', 'o', 'u' };
		AssertEquals ("#A1", 0, s.IndexOfAny (c, 0, 2));
		AssertEquals ("#A2", -1, s.IndexOfAny (c, 1, 2));
		AssertEquals ("#A3", -1, s.IndexOfAny (c, 1, 3));
		AssertEquals ("#A3", 4, s.IndexOfAny (c, 1, 4));
		AssertEquals ("#A4", 4, s.IndexOfAny (c, 1, s.Length - 1));

		c = new char [] { 'd', 'z' };
		AssertEquals ("#B1", -1, s.IndexOfAny (c, 0, 2));
		AssertEquals ("#B2", -1, s.IndexOfAny (c, 1, 2));
		AssertEquals ("#B3", 3, s.IndexOfAny (c, 1, 3));
		AssertEquals ("#B4", 3, s.IndexOfAny (c, 0, s.Length));
		AssertEquals ("#B5", 3, s.IndexOfAny (c, 1, s.Length - 1));
		AssertEquals ("#B6", -1, s.IndexOfAny (c, s.Length, 0));

		c = new char [] { 'q', 'm', 'z' };
		AssertEquals ("#C1", -1, s.IndexOfAny (c, 0, 10));
		AssertEquals ("#C2", 12, s.IndexOfAny (c, 10, 4));
		AssertEquals ("#C3", -1, s.IndexOfAny (c, 1, 3));
		AssertEquals ("#C4", 12, s.IndexOfAny (c, 0, s.Length));
		AssertEquals ("#C5", 12, s.IndexOfAny (c, 1, s.Length - 1));

		c = new char [0];
		AssertEquals ("#D1", -1, s.IndexOfAny (c, 0, 3));
		AssertEquals ("#D2", -1, s.IndexOfAny (c, 4, 9));
		AssertEquals ("#D3", -1, s.IndexOfAny (c, 9, 5));
		AssertEquals ("#D4", -1, s.IndexOfAny (c, 13, 1));
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_AnyOf_Null ()
	{
		try {
			"mono".IndexOfAny ((char []) null, 0, 0);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_Count_Negative ()
	{
		try {
			"Mono".IndexOfAny (new char [1] { 'o' }, 1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_Length_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'd' }, 1, s.Length);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_StartIndex_Negative ()
	{
		try {
			"Mono".IndexOfAny (new char [1] { 'o' }, -1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

	[Test] // IndexOfAny (Char [], Int32, Int32)
	public void IndexOfAny3_StartIndex_Overflow ()
	{
		string s = "abcdefghijklm";

		try {
			s.IndexOfAny (new char [1] { 'o' }, s.Length + 1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid
			// values
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertNull ("#5", ex.ParamName);
		}
	}

#if NET_2_0
	[Test]
	public void Contains ()
	{
		Assert ("ABC".Contains (string.Empty));
		Assert ("ABC".Contains ("ABC"));
		Assert ("ABC".Contains ("AB"));
		Assert (!"ABC".Contains ("AD"));
	}

	[Test]
	public void Contains_Value_Null ()
	{
		try {
			"ABC".Contains (null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
		}
	}

	[Test]
	public void IsNullOrEmpty ()
	{
		Assert (String.IsNullOrEmpty (null));
		Assert (String.IsNullOrEmpty (String.Empty));
		Assert (String.IsNullOrEmpty (""));
		Assert (!String.IsNullOrEmpty ("A"));
		Assert (!String.IsNullOrEmpty (" "));
		Assert (!String.IsNullOrEmpty ("\t"));
		Assert (!String.IsNullOrEmpty ("\n"));
	}
#endif

	[Test]
	public void TestInsert ()
	{
		string s1 = "original";
		
		try {
			s1.Insert (0, null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "value", ex.ParamName);
		}

		try {
			s1.Insert (s1.Length + 1, "Hi!");
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}

		AssertEquals("#C1", "Hi!original", s1.Insert (0, "Hi!"));
		AssertEquals("#C2", "originalHi!", s1.Insert (s1.Length, "Hi!"));
		AssertEquals("#C3", "origHi!inal", s1.Insert (4, "Hi!"));
	}

	[Test]
	public void Intern ()
	{
		string s1 = "original";
		AssertSame ("#A1", s1, String.Intern (s1));
		AssertSame ("#A2", String.Intern(s1), String.Intern(s1));

		string s2 = "originally";
		AssertSame ("#B1", s2, String.Intern (s2));
		Assert ("#B2", String.Intern(s1) != String.Intern(s2));

		string s3 = new DateTime (2000, 3, 7).ToString ();
		AssertNull ("#C1", String.IsInterned (s3));
		AssertSame ("#C2", s3, String.Intern (s3));
		AssertSame ("#C3", s3, String.IsInterned (s3));
		AssertSame ("#C4", s3, String.IsInterned (new DateTime (2000, 3, 7).ToString ()));
		AssertSame ("#C5", s3, String.Intern (new DateTime (2000, 3, 7).ToString ()));
	}

	[Test]
	public void Intern_Str_Null ()
	{
		try {
			String.Intern (null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "str", ex.ParamName);
		}
	}

	[Test]
	public void IsInterned ()
	{
		AssertNull ("#1", String.IsInterned (new DateTime (2000, 3, 6).ToString ()));
		string s1 = "original";
		AssertSame("#2", s1, String.IsInterned (s1));
	}

	[Test]
	public void IsInterned_Str_Null ()
	{
		try {
			String.IsInterned (null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "str", ex.ParamName);
		}
	}

	[Test]
	public void TestJoin ()
	{
		try {
			string s = String.Join(" ", null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "value", ex.ParamName);
		}

		string[] chunks = {"this", "is", "a", "test"};
		AssertEquals("Basic join", "this is a test",
			     String.Join(" ", chunks));
		AssertEquals("Basic join", "this.is.a.test",
			     String.Join(".", chunks));

		AssertEquals("Subset join", "is a",
			     String.Join(" ", chunks, 1, 2));
		AssertEquals("Subset join", "is.a",
			     String.Join(".", chunks, 1, 2));
		AssertEquals("Subset join", "is a test",
			     String.Join(" ", chunks, 1, 3));

		try {
			string s = String.Join(" ", chunks, 2, 3);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void Join_SeparatorNull ()
	{
		string[] chunks = {"this", "is", "a", "test"};
		AssertEquals ("SeparatorNull", "thisisatest", String.Join (null, chunks));
	}

	[Test]
	public void Join_ValuesNull ()
	{
		string[] chunks1 = {null, "is", "a", null};
		AssertEquals ("SomeNull", " is a ", String.Join (" ", chunks1));

		string[] chunks2 = {null, "is", "a", null};
		AssertEquals ("Some+Sep=Null", "isa", String.Join (null, chunks2));

		string[] chunks3 = {null, null, null, null};
		AssertEquals ("AllValuesNull", "   ", String.Join (" ", chunks3));
	}

	[Test]
	public void Join_AllNull ()
	{
		string[] chunks = {null, null, null};
		AssertEquals ("AllNull", string.Empty, String.Join (null, chunks));
	}

	[Test]
	public void Join_StartIndexNegative ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, -1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void Join_StartIndexOverflow ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, Int32.MaxValue, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void Join_LengthNegative ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, 1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test]
	public void Join_LengthOverflow ()
	{
		string[] values = { "Mo", "no" };
		try {
			String.Join ("o", values, 1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOf ()
	{
		string s1 = "original";

		try {
			s1.LastIndexOf ('q', -1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			s1.LastIndexOf ('q', -1, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}

		try {
			s1.LastIndexOf ("huh", s1.Length + 1);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "startIndex", ex.ParamName);
		}

		try {
			int i = s1.LastIndexOf ("huh", s1.Length + 1, 3);
			Fail ("#D1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#D2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#D3", ex.InnerException);
			AssertNotNull ("#D4", ex.Message);
			AssertEquals ("#D5", "startIndex", ex.ParamName);
		}

		try {
			s1.LastIndexOf (null);
			Fail ("#E1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#E2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#E3", ex.InnerException);
			AssertNotNull ("#E4", ex.Message);
#if NET_2_0
			AssertEquals ("#E5", "value", ex.ParamName);
#else
			AssertEquals ("#E5", "string2", ex.ParamName);
#endif
		}

		try {
			s1.LastIndexOf (null, 0);
			Fail ("#F1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#F2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#F3", ex.InnerException);
			AssertNotNull ("#F4", ex.Message);
#if NET_2_0
			AssertEquals ("#F5", "value", ex.ParamName);
#else
			AssertEquals ("#F5", "string2", ex.ParamName);
#endif
		}

		try {
			s1.LastIndexOf (null, 0, 1);
			Fail ("#G1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#G2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#G3", ex.InnerException);
			AssertNotNull ("#G4", ex.Message);
#if NET_2_0
			AssertEquals ("#G5", "value", ex.ParamName);
#else
			AssertEquals ("#G5", "string2", ex.ParamName);
#endif
		}

		AssertEquals("basic char index", 1, s1.LastIndexOf('r'));
		AssertEquals("basic char index", 4, s1.LastIndexOf('i'));
		AssertEquals("basic char index - no", -1, s1.LastIndexOf('q'));

		AssertEquals("basic string index", 7, s1.LastIndexOf(string.Empty));
		AssertEquals("basic string index", 1, s1.LastIndexOf("rig"));
		AssertEquals("basic string index", 4, s1.LastIndexOf("i"));
		AssertEquals("basic string index - no", -1, 
			     s1.LastIndexOf("rag"));

		AssertEquals("stepped char index", 1, 
			     s1.LastIndexOf('r', s1.Length-1));
		AssertEquals("stepped char index", 4, 
			     s1.LastIndexOf('i', s1.Length-1));
		AssertEquals("stepped char index", 2, 
			     s1.LastIndexOf('i', 3));
		AssertEquals("stepped char index", -1, 
			     s1.LastIndexOf('i', 1));

		AssertEquals("stepped limited char index", 
			     1, s1.LastIndexOf('r', 1, 1));
		AssertEquals("stepped limited char index", 
			     -1, s1.LastIndexOf('r', 0, 1));
		AssertEquals("stepped limited char index", 
			     4, s1.LastIndexOf('i', 6, 3));
		AssertEquals("stepped limited char index", 
			     2, s1.LastIndexOf('i', 3, 3));
		AssertEquals("stepped limited char index", 
			     -1, s1.LastIndexOf('i', 1, 2));

		s1 = "original original";
		AssertEquals("stepped string index #1",
			     9, s1.LastIndexOf("original", s1.Length));
		AssertEquals("stepped string index #2", 
			     0, s1.LastIndexOf("original", s1.Length-2));
		AssertEquals("stepped string index #3", 
			     -1, s1.LastIndexOf("original", s1.Length-11));
		AssertEquals("stepped string index #4",
			     -1, s1.LastIndexOf("translator", 2));
		AssertEquals("stepped string index #5",
			     0, string.Empty.LastIndexOf(string.Empty, 0));
#if !TARGET_JVM
		AssertEquals("stepped string index #6",
			     -1, string.Empty.LastIndexOf("A", -1));
#endif
		AssertEquals("stepped limited string index #1",
			     10, s1.LastIndexOf("rig", s1.Length-1, 10));
		AssertEquals("stepped limited string index #2",
			     -1, s1.LastIndexOf("rig", s1.Length, 3));
		AssertEquals("stepped limited string index #3",
			     10, s1.LastIndexOf("rig", s1.Length-2, 15));
		AssertEquals("stepped limited string index #4",
			     -1, s1.LastIndexOf("rig", s1.Length-2, 3));
			     
		string s2 = "QBitArray::bitarr_data"; 
		AssertEquals ("bug #62160", 9, s2.LastIndexOf ("::"));

		string s3 = "test123";
		AssertEquals ("bug #77412", 0, s3.LastIndexOf ("test123"));
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
		AssertEquals ("#1-1", 7, text.LastIndexOf (text2, StringComparison.Ordinal));
		AssertEquals ("#2-1", 5, text.LastIndexOf (text3, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-2", 7, text.LastIndexOf (text2, 12, StringComparison.Ordinal));
		AssertEquals ("#2-2", 5, text.LastIndexOf (text3, 12, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-3", -1, text.LastIndexOf (text2, 0, StringComparison.Ordinal));
		AssertEquals ("#2-3", -1, text.LastIndexOf (text3, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-4", -1, text.LastIndexOf (text2, 6, StringComparison.Ordinal));
		AssertEquals ("#2-4", 5, text.LastIndexOf (text3, 6, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-5", -1, text.LastIndexOf (text2, 7, 3, StringComparison.Ordinal));
		AssertEquals ("#2-5", 5, text.LastIndexOf (text3, 7, 3, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-6", -1, text.LastIndexOf (text2, 6, 0, StringComparison.Ordinal));
		AssertEquals ("#2-6", -1, text.LastIndexOf (text3, 5, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#1-7", -1, text.LastIndexOf (text2, 7, 1, StringComparison.Ordinal));
		AssertEquals ("#2-7", -1, text.LastIndexOf (text3, 5, 1, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#3-1", 0, text.LastIndexOf (text4, 0, StringComparison.Ordinal));
		AssertEquals ("#3-2", 0, text.LastIndexOf (text4, 0, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#4-1", 3, text.LastIndexOf (text4, 13, StringComparison.Ordinal));
		AssertEquals ("#4-2", 3, text.LastIndexOf (text4, 13, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#4-1", 3, text.LastIndexOf (text4, 13, 14, StringComparison.Ordinal));
		AssertEquals ("#4-2", 3, text.LastIndexOf (text4, 13, 14, StringComparison.OrdinalIgnoreCase));

		AssertEquals ("#5-1", 0, text.LastIndexOf (text4, 1, 2, StringComparison.Ordinal));
		AssertEquals ("#5-2", 0, text.LastIndexOf (text4, 1, 2, StringComparison.OrdinalIgnoreCase));

		AssertEquals (-1, "".LastIndexOf ("FOO", StringComparison.Ordinal));
		AssertEquals (0, "".LastIndexOf ("", StringComparison.Ordinal));
	}

	[Test]
	public void LastIndexOfStringComparisonOrdinal ()
	{
		string text = "testing123456";
		AssertEquals ("#1", 10, text.LastIndexOf ("456", StringComparison.Ordinal));
		AssertEquals ("#2", -1, text.LastIndexOf ("4567", StringComparison.Ordinal));
		AssertEquals ("#3", 0, text.LastIndexOf ("te", StringComparison.Ordinal));
		AssertEquals ("#4", 2, text.LastIndexOf ("s", StringComparison.Ordinal));
		AssertEquals ("#5", -1, text.LastIndexOf ("ates", StringComparison.Ordinal));
		AssertEquals ("#6", -1, text.LastIndexOf ("S", StringComparison.Ordinal));
	}

	[Test]
	public void LastIndexOfStringComparisonOrdinalIgnoreCase ()
	{
		string text = "testing123456";
		AssertEquals ("#1", 10, text.LastIndexOf ("456", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#2", -1, text.LastIndexOf ("4567", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#3", 0, text.LastIndexOf ("te", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#4", 2, text.LastIndexOf ("s", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#5", -1, text.LastIndexOf ("ates", StringComparison.OrdinalIgnoreCase));
		AssertEquals ("#6", 2, text.LastIndexOf ("S", StringComparison.OrdinalIgnoreCase));
	}
#endif

	[Test]
	public void LastIndexOf_Char_StartIndexStringLength ()
	{
		string s = "Mono";
		try {
			s.LastIndexOf ('n', s.Length, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
		// this works for string but not for a char
	}

	[Test]
	public void LastIndexOf_Char_StartIndexOverflow ()
	{
		try {
			"Mono".LastIndexOf ('o', Int32.MaxValue, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOf_Char_LengthOverflow ()
	{
		try {
			"Mono".LastIndexOf ('o', 1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOf_String_StartIndexStringLength ()
	{
		string s = "Mono";
		AssertEquals (-1, s.LastIndexOf ("n", s.Length, 1));
		// this works for string but not for a char
	}

	[Test]
	public void LastIndexOf_String_StartIndexStringLength_Plus1 ()
	{
		string s = "Mono";
		try {
			s.LastIndexOf ("n", s.Length + 1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOf_String_StartIndexOverflow ()
	{
		try {
			"Mono".LastIndexOf ("no", Int32.MaxValue, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOf_String_LengthOverflow ()
	{
		try {
			"Mono".LastIndexOf ("no", 1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOfAny ()
	{
		string s1 = ".bcdefghijklm";

		try {
			s1.LastIndexOfAny (null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertNull ("#A5", ex.ParamName);
		}

		try {
			s1.LastIndexOfAny (null, s1.Length);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertNull ("#B5", ex.ParamName);
		}

		try {
			s1.LastIndexOfAny (null, s1.Length, 1);
			Fail ("#C1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#C2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertNull ("#C5", ex.ParamName);
		}

		char[] c1 = {'a', 'e', 'i', 'o', 'u'};
		AssertEquals("#D1", 8, s1.LastIndexOfAny (c1));
		AssertEquals("#D2", 4, s1.LastIndexOfAny (c1, 7));
		AssertEquals("#D3", -1, s1.LastIndexOfAny (c1, 3));
		AssertEquals("#D4", 4, s1.LastIndexOfAny (c1, s1.Length - 6, 4));
		AssertEquals("#D5", -1, s1.LastIndexOfAny (c1, s1.Length - 6, 3));

		try {
			s1.LastIndexOfAny (c1, -1);
			Fail ("#E1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#E2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#E3", ex.InnerException);
			AssertNotNull ("#E4", ex.Message);
			AssertEquals ("#E5", "startIndex", ex.ParamName);
		}

		try {
			s1.LastIndexOfAny (c1, -1, 1);
			Fail ("#F1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#F2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#F3", ex.InnerException);
			AssertNotNull ("#F4", ex.Message);
			AssertEquals ("#F5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOfAny_Length_Overflow ()
	{
		try {
			"Mono".LastIndexOfAny (new char [1] { 'o' }, 1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count must be positive and count must refer to a
			// location within the string/array/collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test]
	public void LastIndexOfAny_StartIndex_Overflow ()
	{
		try {
			"Mono".LastIndexOfAny (new char [1] { 'o' }, Int32.MaxValue, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // PadLeft (Int32)
	public void PadLeft1 ()
	{
		string s1 = "Hi!";
		string result;

		result = s1.PadLeft (0);
		AssertSame ("#A", s1, result);

		result = s1.PadLeft (s1.Length - 1);
		AssertSame ("#B", s1, result);

		result = s1.PadLeft (s1.Length);
		AssertEquals ("#C1", s1, result);
		Assert ("#C2", !object.ReferenceEquals (s1, result));

		result = s1.PadLeft (s1.Length + 1);
		AssertEquals("#D", " Hi!", result);
	}

	[Test] // PadLeft (Int32)
	public void PadLeft1_TotalWidth_Negative ()
	{
		try {
			"Mono".PadLeft (-1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "totalWidth", ex.ParamName);
		}
	}

	[Test] // PadRight (Int32)
	public void PadRight1 ()
	{
		string s1 = "Hi!";
		string result;

		result = s1.PadRight (0);
		AssertSame ("#A", s1, result);

		result = s1.PadRight (s1.Length - 1);
		AssertSame ("#B", s1, result);

		result = s1.PadRight (s1.Length);
		AssertEquals ("#C1", s1, result);
		Assert ("#C2", !object.ReferenceEquals (s1, result));

		result = s1.PadRight (s1.Length + 1);
		AssertEquals("#D", "Hi! ", result);
	}

	[Test] // PadRight1 (Int32)
	public void PadRight1_TotalWidth_Negative ()
	{
		try {
			"Mono".PadRight (-1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "totalWidth", ex.ParamName);
		}
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2 ()
	{
		string s1 = "original";

		try {
			s1.Remove (-1, 1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			s1.Remove (1,-1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Count cannot be less than zero
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "count", ex.ParamName);
		}

		try {
			s1.Remove (s1.Length, s1.Length);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "count", ex.ParamName);
		}

		AssertEquals ("#D1", "oinal", s1.Remove(1, 3));
		AssertEquals ("#D2", s1, s1.Remove (0, 0));
		Assert ("#D3", !object.ReferenceEquals (s1, s1.Remove (0, 0)));
		AssertEquals ("#D4", "riginal", s1.Remove (0, 1));
		AssertEquals ("#D5", "origina", s1.Remove (7, 1));
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2_Length_Overflow ()
	{
		try {
			"Mono".Remove (1, Int32.MaxValue);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // Remove (Int32, Int32)
	public void Remove2_StartIndex_Overflow ()
	{
		try {
			"Mono".Remove (Int32.MaxValue, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and count must refer to a location within the
			// string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

#if NET_2_0
	[Test] // Remove (Int32)
	public void Remove1_StartIndex_Negative ()
	{
		try {
			"ABC".Remove (-1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // Remove (Int32)
	public void Remove1_StartIndex_Overflow ()
	{
		try {
			"ABC".Remove (3);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex must be less than length of string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // Remove (Int32)
	public void Remove1 ()
	{
		string s = "ABC";

		AssertEquals ("#1", "AB", s.Remove (2));
		AssertEquals ("#2", string.Empty, s.Remove (0));
		AssertEquals ("#3", "A", s.Remove (1));
	}
#endif

	[Test]
	public void Replace()
	{
		string s1 = "original";

		AssertEquals("non-hit char", s1, s1.Replace('q','s'));
		AssertEquals("single char", "oxiginal", s1.Replace('r', 'x'));
		AssertEquals("double char", "orxgxnal", s1.Replace('i', 'x'));

		bool errorThrown = false;
		try {
			string s = s1.Replace(null, "feh");
		} catch (ArgumentNullException) {
			errorThrown = true;
		}
		Assert("should get null arg exception", errorThrown);

		AssertEquals("replace as remove", "ornal", 
			     s1.Replace("igi", null));
		AssertEquals("non-hit string", s1, s1.Replace("spam", "eggs"));
		AssertEquals("single string", "orirumal", 
			     s1.Replace("gin", "rum"));
		AssertEquals("double string", "oreigeinal", 
			     s1.Replace("i", "ei"));

		AssertEquals("start", "ooriginal", s1.Replace("o", "oo"));
		AssertEquals("end", "originall", s1.Replace("l", "ll"));

		AssertEquals("start empty", "riginal", s1.Replace("o", string.Empty));
		AssertEquals("end empty", "origina", s1.Replace("l", string.Empty));

		AssertEquals("replace bigger that original", "original", s1.Replace("original2", "original3"));

		AssertEquals ("result longer", ":!:", "::".Replace ("::", ":!:"));

		// Test overlapping matches (bug #54988)
		string s2 = "...aaaaaaa.bbbbbbbbb,............ccccccc.u...";
		AssertEquals ("..aaaaaaa.bbbbbbbbb,......ccccccc.u..", s2.Replace("..", "."));

		// Test replacing null characters (bug #67395)
#if !TARGET_JVM //bug #7276
		AssertEquals ("should not strip content after nullchar",
			"is this ok ?", "is \0 ok ?".Replace ("\0", "this"));
#endif
	}

	[Test]
	public void ReplaceStringBeginEndTest ()
	{
		string s1 = "original";

		AssertEquals ("#1", "riginal", s1.Replace ("o", ""));
		AssertEquals ("#2", "origina", s1.Replace ("l", ""));
		AssertEquals ("#3", "ariginal", s1.Replace ("o", "a"));
		AssertEquals ("#4", "originaa", s1.Replace ("l", "a"));
		AssertEquals ("#5", "aariginal", s1.Replace ("o", "aa"));
		AssertEquals ("#6", "originaaa", s1.Replace ("l", "aa"));
		AssertEquals ("#7", "original", s1.Replace ("o", "o"));
		AssertEquals ("#8", "original", s1.Replace ("l", "l"));
		AssertEquals ("#9", "original", s1.Replace ("original", "original"));
		AssertEquals ("#10", "", s1.Replace ("original", ""));
	}

	[Test]
	public void ReplaceStringBeginEndTestFallback ()
	{
		string prev = new String ('o', 300);
		string s1 = prev + "riginal";

		AssertEquals ("#1", "riginal", s1.Replace ("o", ""));
		AssertEquals ("#2", prev + "rigina", s1.Replace ("l", ""));
		AssertEquals ("#3", new String ('a', 300) + "riginal", s1.Replace ("o", "a"));
		AssertEquals ("#4", prev + "riginaa", s1.Replace ("l", "a"));
		AssertEquals ("#5", new String ('a', 600) + "riginal", s1.Replace ("o", "aa"));
		AssertEquals ("#6", prev + "riginaaa", s1.Replace ("l", "aa"));
		AssertEquals ("#7", s1, s1.Replace ("o", "o"));
		AssertEquals ("#8", s1, s1.Replace ("l", "l"));
		AssertEquals ("#9", s1, s1.Replace (s1, s1));
		AssertEquals ("#10", "", s1.Replace (prev + "riginal", ""));
	}

	[Test]
	public void ReplaceStringOffByOne ()
	{
		AssertEquals ("#-1", "", new String ('o', 199).Replace ("o", ""));
		AssertEquals ("#0", "", new String ('o', 200).Replace ("o", ""));
		AssertEquals ("#+1", "", new String ('o', 201).Replace ("o", ""));
	}

	[Test]
	public void ReplaceStringCultureTests ()
	{
		// LAMESPEC: According to MSDN Replace with String parameter is culture-senstive.
		// However this does not currently seem to be the case. Otherwise following code should
		// produce "check" instead of "AE"

		CultureInfo old = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		AssertEquals ("#1", "AE", "AE".Replace ("\u00C6", "check"));
		Thread.CurrentThread.CurrentCulture = old;
	}

	[Test] // StartsWith (String)
	public void StartsWith1 ()
	{
		string s1 = "original";
		
		Assert ("#1", s1.StartsWith ("o"));
		Assert ("#2", s1.StartsWith ("orig"));
		Assert ("#3", !s1.StartsWith ("rig"));
		Assert ("#4", s1.StartsWith (String.Empty));
		Assert ("#5", String.Empty.StartsWith (String.Empty));
		Assert ("#6", !String.Empty.StartsWith ("rig"));
	}

	[Test] // StartsWith (String)
	public void StartsWith1_Value_Null ()
	{
		try {
			"A".StartsWith (null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
		}
	}

#if NET_2_0
	[Test] // StartsWith (String, StringComparison)
	public void StartsWith2_ComparisonType_Invalid ()
	{
		try {
			"ABC".StartsWith ("A", (StringComparison) 80);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// The string comparison type passed in is currently
			// not supported
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "comparisonType", ex.ParamName);
		}
	}

	[Test] // StartsWith (String, StringComparison)
	public void StartsWith2_Value_Null ()
	{
		try {
			"A".StartsWith (null, StringComparison.CurrentCulture);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "value", ex.ParamName);
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

		AssertEquals("#1", "inal", s.Substring (4));
		AssertEquals ("#2", string.Empty, s.Substring (s.Length));
#if NET_2_0
		AssertSame ("#3", s, s.Substring (0));
#else
		AssertEquals ("#3a", s, s.Substring (0));
		Assert ("#3b", !object.ReferenceEquals (s, s.Substring (0)));
#endif
	}

	[Test] // SubString (Int32)
	public void SubString1_StartIndex_Negative ()
	{
		string s = "original";

		try {
			s.Substring (-1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // SubString (Int32)
	public void SubString1_StartIndex_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length + 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
#if NET_2_0
			AssertEquals ("#5", "startIndex", ex.ParamName);
#else
			AssertEquals ("#5", "length", ex.ParamName);
#endif
		}
	}

	[Test] // SubString (Int32, Int32)
	public void Substring2 ()
	{
		string s = "original";

		AssertEquals ("#1", "igin", s.Substring (2, 4));
		AssertEquals ("#2", string.Empty, s.Substring (s.Length, 0));
		AssertEquals ("#3", "origina", s.Substring (0, s.Length - 1));
		AssertEquals ("#4", s, s.Substring (0, s.Length));
#if NET_2_0
		AssertSame ("#5", s, s.Substring (0, s.Length));
#else
		Assert ("#5", !object.ReferenceEquals (s, s.Substring (0, s.Length)));
#endif
	}

	[Test] // SubString (Int32, Int32)
	public void SubString2_Length_Negative ()
	{
		string s = "original";

		try {
			s.Substring (1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Length cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "length", ex.ParamName);
		}
	}
	
	[Test] // SubString (Int32, Int32)
	public void Substring2_Length_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length, 1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "length", ex.ParamName);
		}

		try {
			s.Substring (1, s.Length);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "length", ex.ParamName);
		}

		try {
			s.Substring (1, Int32.MaxValue);
			Fail ("#C1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index and length must refer to a location within
			// the string
			AssertEquals ("#C2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "length", ex.ParamName);
		}
	}

	[Test] // SubString (Int32, Int32)
	public void SubString2_StartIndex_Negative ()
	{
		string s = "original";

		try {
			s.Substring (-1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// StartIndex cannot be less than zero
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test] // SubString (Int32, Int32)
	public void Substring2_StartIndex_Overflow ()
	{
		string s = "original";

		try {
			s.Substring (s.Length + 1, 0);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
#if NET_2_0
			AssertEquals ("#A5", "startIndex", ex.ParamName);
#else
			AssertEquals ("#A5", "length", ex.ParamName);
#endif
		}

		try {
			"Mono".Substring (Int32.MaxValue, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// startIndex cannot be larger than length of string
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
#if NET_2_0
			AssertEquals ("#B5", "startIndex", ex.ParamName);
#else
			AssertEquals ("#B5", "length", ex.ParamName);
#endif
		}
	}

	[Test]
	public void ToCharArray ()
	{
		const string s = "original";
		char [] c;

		c = s.ToCharArray ();
		AssertEquals ("#A1", s.Length, c.Length);
		AssertEquals ("#A2", s, new String (c));

		c = s.ToCharArray (0, s.Length);
		AssertEquals ("#B1", s.Length, c.Length);
		AssertEquals ("#B2", s, new String (c));

		c = s.ToCharArray (1, s.Length - 1);
		AssertEquals ("#C1", 7, c.Length);
		AssertEquals ("#C2", "riginal", new String (c));

		c = s.ToCharArray (0, 3);
		AssertEquals ("#D1", 3, c.Length);
		AssertEquals ("#D2", "ori", new String (c));

		c = s.ToCharArray (2, 0);
		AssertEquals ("#E1", 0, c.Length);
		AssertEquals ("#E2", string.Empty, new String (c));

		c = s.ToCharArray (3, 2);
		AssertEquals ("#F1", 2, c.Length);
		AssertEquals ("#F2", "gi", new String (c));

		c = s.ToCharArray (s.Length, 0);
		AssertEquals ("#G1", 0, c.Length);
		AssertEquals ("#G2", string.Empty, new String (c));
	}

	[Test]
	public void ToCharArray_Length_Negative ()
	{
		const string s = "original";

		try {
			s.ToCharArray (1, -1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "length", ex.ParamName);
		}
	}

	[Test]
	public void ToCharArray_Length_Overflow ()
	{
		const string s = "original";

		try {
			s.ToCharArray (1, s.Length);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			s.ToCharArray (1, Int32.MaxValue);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void ToCharArray_StartIndex_Negative ()
	{
		const string s = "original";

		try {
			s.ToCharArray (-1, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void ToCharArray_StartIndex_Overflow ()
	{
		const string s = "original";

		try {
			s.ToCharArray (s.Length, 1);
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "startIndex", ex.ParamName);
		}

		try {
			s.ToCharArray (Int32.MaxValue, 1);
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Index was out of range. Must be non-negative and
			// less than the size of the collection
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "startIndex", ex.ParamName);
		}
	}

	[Test] // ToLower ()
	public void ToLower1 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#1", "\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069", s.ToLower());

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#2", "originali", s.ToLower ());
	}

	[Test] // ToLower (CultureInfo)
	public void ToLower2 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#A1", "originali", s.ToLower (new CultureInfo ("en-US")));
		AssertEquals ("#A2", "\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069",
			s.ToLower (new CultureInfo ("tr-TR")));
		AssertEquals ("#A3", string.Empty, string.Empty.ToLower (new CultureInfo ("en-US")));
		AssertEquals ("#A4", string.Empty, string.Empty.ToLower (new CultureInfo ("tr-TR")));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#B1", "originali", s.ToLower (new CultureInfo ("en-US")));
		AssertEquals ("#B2", "\u006f\u0072\u0131\u0067\u0131\u006e\u0061\u006c\u0069",
			s.ToLower (new CultureInfo ("tr-TR")));
		AssertEquals ("#B3", string.Empty, string.Empty.ToLower (new CultureInfo ("en-US")));
		AssertEquals ("#B4", string.Empty, string.Empty.ToLower (new CultureInfo ("tr-TR")));
	}

	[Test] // ToLower (CultureInfo)
	public void ToLower2_Culture_Null ()
	{
		string s = "OrIgInAl";

		try {
			s.ToLower ((CultureInfo) null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "culture", ex.ParamName);
		}

		try {
			string.Empty.ToLower ((CultureInfo) null);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "culture", ex.ParamName);
		}
	}

	[Test]
	public void TestToString ()
	{
		string s1 = "OrIgInAli";
		AssertEquals("ToString failed!", s1, s1.ToString());
	}

	[Test] // ToUpper ()
	public void ToUpper1 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#1", "ORIGINAL\u0130", s.ToUpper ());

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#2", "ORIGINALI", s.ToUpper ());
	}

	[Test] // ToUpper (CultureInfo)
	public void ToUpper2 ()
	{
		string s = "OrIgInAli";

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

		AssertEquals ("#A1", "ORIGINALI", s.ToUpper (new CultureInfo ("en-US")));
		AssertEquals ("#A2", "ORIGINAL\u0130", s.ToUpper (new CultureInfo ("tr-TR")));
		AssertEquals ("#A3", string.Empty, string.Empty.ToUpper (new CultureInfo ("en-US")));
		AssertEquals ("#A4", string.Empty, string.Empty.ToUpper (new CultureInfo ("tr-TR")));

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");

		AssertEquals ("#B1", "ORIGINALI", s.ToUpper (new CultureInfo ("en-US")));
		AssertEquals ("#B2", "ORIGINAL\u0130", s.ToUpper (new CultureInfo ("tr-TR")));
		AssertEquals ("#B3", string.Empty, string.Empty.ToUpper (new CultureInfo ("en-US")));
		AssertEquals ("#B4", string.Empty, string.Empty.ToUpper (new CultureInfo ("tr-TR")));
	}

	[Test] // ToUpper (CultureInfo)
	public void ToUpper2_Culture_Null ()
	{
		string s = "OrIgInAl";

		try {
			s.ToUpper ((CultureInfo) null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "culture", ex.ParamName);
		}

		try {
			string.Empty.ToUpper ((CultureInfo) null);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "culture", ex.ParamName);
		}
	}

	[Test]
	public void TestTrim ()
	{
		string s1 = "  original\t\n";
		AssertEquals("basic trim failed", "original", s1.Trim());
		AssertEquals("basic trim failed", "original", s1.Trim(null));

		s1 = "original";
		AssertEquals("basic trim failed", "original", s1.Trim());
		AssertEquals("basic trim failed", "original", s1.Trim(null));

		s1 = "   \t \n  ";
		AssertEquals("empty trim failed", string.Empty, s1.Trim());
		AssertEquals("empty trim failed", string.Empty, s1.Trim(null));

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		AssertEquals("custom trim failed", 
			     "original", s1.Trim(delims));

#if NET_2_0
		AssertEquals ("net_2_0 additional char#1", "original", "\u2028original\u2029".Trim ());
		AssertEquals ("net_2_0 additional char#2", "original", "\u0085original\u1680".Trim ());
#endif
	}

	[Test]
	public void TestTrimEnd ()
	{
		string s1 = "  original\t\n";
		AssertEquals("basic TrimEnd failed", 
			     "  original", s1.TrimEnd(null));

		s1 = "  original";
		AssertEquals("basic TrimEnd failed", 
			     "  original", s1.TrimEnd(null));

		s1 = "  \t  \n  \n    ";
		AssertEquals("empty TrimEnd failed", 
			     string.Empty, s1.TrimEnd(null));

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		AssertEquals("custom TrimEnd failed", 
			     "aaaoriginal", s1.TrimEnd(delims));
	}

	[Test]
	public void TestTrimStart ()
	{
		string s1 = "  original\t\n";
		AssertEquals("basic TrimStart failed", 
			     "original\t\n", s1.TrimStart(null));

		s1 = "original\t\n";
		AssertEquals("basic TrimStart failed", 
			     "original\t\n", s1.TrimStart(null));

		s1 = "    \t \n \n  ";
		AssertEquals("empty TrimStart failed", 
			     string.Empty, s1.TrimStart(null));

		s1 = "aaaoriginalbbb";
		char[] delims = {'a', 'b'};
		AssertEquals("custom TrimStart failed", 
			     "originalbbb", s1.TrimStart(delims));
	}

	[Test]
	public void TestChars ()
	{
		string s;

		s = string.Empty;
		try {
			char c = s [0];
			Fail ("#A1:" + c);
		} catch (IndexOutOfRangeException ex) {
			AssertEquals ("#A2", typeof (IndexOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
		}

		s = "A";
		try {
			char c = s [-1];
			Fail ("#B1:" + c);
		} catch (IndexOutOfRangeException ex) {
			AssertEquals ("#B2", typeof (IndexOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
		}
	}

	[Test]
	public void TestComparePeriod ()
	{
		// according to bug 63981, this behavior is for all cultures
		AssertEquals ("#1", -1, String.Compare ("foo.obj", "foobar.obj", false));
	}

	[Test]
	public void LastIndexOfAnyBounds1 ()
	{
		string mono = "Mono";
		char [] k = { 'M' };
		try {
			mono.LastIndexOfAny (k, mono.Length, 1);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "startIndex", ex.ParamName);
		}
	}

	[Test]
	public void TestSplit ()
	{
		string s1 = "abcdefghijklm";
		char[] c1 = {'q', 'r'};
		AssertEquals("No splitters", s1, (s1.Split(c1))[0]);

		char[] c2 = {'a', 'e', 'i', 'o', 'u'};
		string[] chunks = s1.Split(c2);
		AssertEquals("First chunk", string.Empty, chunks[0]);
		AssertEquals("Second chunk", "bcd", chunks[1]);
		AssertEquals("Third chunk", "fgh", chunks[2]);
		AssertEquals("Fourth chunk", "jklm", chunks[3]);

		{
			bool errorThrown = false;
			try {
				chunks = s1.Split(c2, -1);
			} catch (ArgumentOutOfRangeException) {
				errorThrown = true;
			}
			Assert("Split out of range", errorThrown);
		}

		chunks = s1.Split(c2, 2);
		AssertEquals("Limited chunk", 2, chunks.Length);
		AssertEquals("First limited chunk", string.Empty, chunks[0]);
		AssertEquals("Second limited chunk", "bcdefghijklm", chunks[1]);

		string s3 = "1.0";
		char[] c3 = {'.'};
		chunks = s3.Split(c3,2);
		AssertEquals("1.0 split length", 2, chunks.Length);
		AssertEquals("1.0 split first chunk", "1", chunks[0]);
		AssertEquals("1.0 split second chunk", "0", chunks[1]);

		string s4 = "1.0.0";
		char[] c4 = {'.'};
		chunks = s4.Split(c4,2);
		AssertEquals("1.0.0 split length", 2, chunks.Length);
		AssertEquals("1.0.0 split first chunk", "1", chunks[0]);
		AssertEquals("1.0.0 split second chunk", "0.0", chunks[1]);

		string s5 = ".0.0";
		char[] c5 = {'.'};
		chunks = s5.Split (c5, 2);
		AssertEquals(".0.0 split length", 2, chunks.Length);
		AssertEquals(".0.0 split first chunk", string.Empty, chunks[0]);
		AssertEquals(".0.0 split second chunk", "0.0", chunks[1]);

		string s6 = ".0";
		char[] c6 = {'.'};
		chunks = s6.Split (c6, 2);
		AssertEquals(".0 split length", 2, chunks.Length);
		AssertEquals(".0 split first chunk", string.Empty, chunks[0]);
		AssertEquals(".0 split second chunk", "0", chunks[1]);

		string s7 = "0.";
		char[] c7 = {'.'};
		chunks = s7.Split (c7, 2);
		AssertEquals("0. split length", 2, chunks.Length);
		AssertEquals("0. split first chunk", "0", chunks[0]);
		AssertEquals("0. split second chunk", string.Empty, chunks[1]);

		string s8 = "0.0000";
		char[] c8 = {'.'};
		chunks = s8.Split (c8, 2);
		AssertEquals("0.0000/2 split length", 2, chunks.Length);
		AssertEquals("0.0000/2 split first chunk", "0", chunks[0]);
		AssertEquals("0.0000/2 split second chunk", "0000", chunks[1]);

		chunks = s8.Split (c8, 3);
		AssertEquals("0.0000/3 split length", 2, chunks.Length);
		AssertEquals("0.0000/3 split first chunk", "0", chunks[0]);
		AssertEquals("0.0000/3 split second chunk", "0000", chunks[1]);

		chunks = s8.Split (c8, 1);
		AssertEquals("0.0000/1 split length", 1, chunks.Length);
		AssertEquals("0.0000/1 split first chunk", "0.0000", chunks[0]);

		chunks = s1.Split(c2, 1);
		AssertEquals("Single split", 1, chunks.Length);
		AssertEquals("Single chunk", s1, chunks[0]);

		chunks = s1.Split(c2, 0);
		AssertEquals("Zero split", 0, chunks.Length);
	}

	[Test]
	public void MoreSplit ()
	{
		string test = "123 456 789";
		string [] st = test.Split ();
		AssertEquals ("#01", "123", st [0]);
		st = test.Split (null);
		AssertEquals ("#02", "123", st [0]);
	}

#if NET_2_0
	[Test] // Split (Char [], StringSplitOptions)
	public void Split3_Options_Invalid ()
	{
		try {
			"A B".Split (new Char [] { 'A' }, (StringSplitOptions) 4096);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			Assert ("#5", ex.Message.IndexOf ("4096") != 1);
			AssertNull ("#6", ex.ParamName);
		}
	}

	[Test] // Split (Char [], StringSplitOptions)
	public void Split4_Options_Invalid ()
	{
		try {
			"A B".Split (new String [] { "A" }, (StringSplitOptions) 4096);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			Assert ("#5", ex.Message.IndexOf ("4096") != 1);
			AssertNull ("#6", ex.ParamName);
		}
	}

	[Test] // Split (Char [], StringSplitOptions)
	public void Split5_Options_Invalid ()
	{
		try {
			"A B".Split (new Char [] { 'A' }, 0, (StringSplitOptions) 4096);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			Assert ("#5", ex.Message.IndexOf ("4096") != 1);
			AssertNull ("#6", ex.ParamName);
		}
	}

	[Test] // Split (String [], Int32, StringSplitOptions)
	public void Split6_Count_Negative ()
	{
		try {
			"A B".Split (new String [] { "A" }, -1, StringSplitOptions.None);
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "count", ex.ParamName);
		}
	}

	[Test] // Split (String [], Int32, StringSplitOptions)
	public void Split6_Options_Invalid ()
	{
		try {
			"A B".Split (new String [] { "A" }, 0, (StringSplitOptions) 4096);
			Fail ("#1");
		} catch (ArgumentException ex) {
			// Illegal enum value: 4096
			AssertEquals ("#2", typeof (ArgumentException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			Assert ("#5", ex.Message.IndexOf ("4096") != 1);
			AssertNull ("#6", ex.ParamName);
		}
	}

	[Test]
	public void SplitString ()
	{
		String[] res;
		
		// count == 0
		res = "A B C".Split (new String [] { "A" }, 0, StringSplitOptions.None);
		AssertEquals (0, res.Length);

		// empty and RemoveEmpty
		res = string.Empty.Split (new String [] { "A" }, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals (0, res.Length);

		// Not found
		res = "A B C".Split (new String [] { "D" }, StringSplitOptions.None);
		AssertEquals (1, res.Length);
		AssertEquals ("A B C", res [0]);

		// A normal test
		res = "A B C DD E".Split (new String[] { "B", "D" }, StringSplitOptions.None);
		AssertEquals (4, res.Length);
		AssertEquals ("A ", res [0]);
		AssertEquals (" C ", res [1]);
		AssertEquals (string.Empty, res [2]);
		AssertEquals (" E", res [3]);

		// Same with RemoveEmptyEntries
		res = "A B C DD E".Split (new String[] { "B", "D" }, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals (3, res.Length);
		AssertEquals ("A ", res [0]);
		AssertEquals (" C ", res [1]);
		AssertEquals (" E", res [2]);

		// Delimiter matches once at the beginning of the string
		res = "A B".Split (new String [] { "A" }, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals (1, res.Length);
		AssertEquals (" B", res [0]);

		// Delimiter at the beginning and at the end
		res = "B C DD B".Split (new String[] { "B" }, StringSplitOptions.None);
		AssertEquals (3, res.Length);
		AssertEquals (string.Empty, res [0]);
		AssertEquals (" C DD ", res [1]);
		AssertEquals (string.Empty, res [2]);

		res = "B C DD B".Split (new String[] { "B" }, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals (1, res.Length);
		AssertEquals (" C DD ", res [0]);

		// count
		res = "A B C DD E".Split (new String[] { "B", "D" }, 2, StringSplitOptions.None);
		AssertEquals (2, res.Length);
		AssertEquals ("A ", res [0]);
		AssertEquals (" C DD E", res [1]);

		// Ordering
		res = "ABCDEF".Split (new String[] { "EF", "BCDE" }, StringSplitOptions.None);
		AssertEquals (2, res.Length);
		AssertEquals ("A", res [0]);
		AssertEquals ("F", res [1]);

		res = "ABCDEF".Split (new String[] { "BCD", "BC" }, StringSplitOptions.None);
		AssertEquals (2, res.Length);
		AssertEquals ("A", res [0]);
		AssertEquals ("EF", res [1]);

		// Whitespace
		res = "A B\nC".Split ((String[])null, StringSplitOptions.None);
		AssertEquals (3, res.Length);
		AssertEquals ("A", res [0]);
		AssertEquals ("B", res [1]);
		AssertEquals ("C", res [2]);

		res = "A B\nC".Split (new String [0], StringSplitOptions.None);
		AssertEquals (3, res.Length);
		AssertEquals ("A", res [0]);
		AssertEquals ("B", res [1]);
		AssertEquals ("C", res [2]);
	}
	
	[Test]
	public void SplitStringChars ()
	{
		String[] res;

		// count == 0
		res = "..A..B..".Split (new Char[] { '.' }, 0, StringSplitOptions.None);
		AssertEquals ("#01-01", 0, res.Length);

		// count == 1
		res = "..A..B..".Split (new Char[] { '.' }, 1, StringSplitOptions.None);
		AssertEquals ("#02-01", 1, res.Length);
		AssertEquals ("#02-02", "..A..B..", res [0]);

		// count == 1 + RemoveEmpty
		res = "..A..B..".Split (new Char[] { '.' }, 1, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#03-01", 1, res.Length);
		AssertEquals ("#03-02", "..A..B..", res [0]);
		
		// Strange Case A+B A
		res = "...".Split (new Char[] { '.' }, 1, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#ABA-01", 1, res.Length);
		AssertEquals ("#ABA-02", "...", res [0]);

		// Strange Case A+B B
		res = "...".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#ABB-01", 0, res.Length);

		// Keeping Empties and multipe split chars
		res = "..A;.B.;".Split (new Char[] { '.', ';' }, StringSplitOptions.None);
		AssertEquals ("#04-01", 7, res.Length);
		AssertEquals ("#04-02", string.Empty, res [0]);
		AssertEquals ("#04-03", string.Empty, res [1]);
		AssertEquals ("#04-04", "A", res [2]);
		AssertEquals ("#04-05", string.Empty, res [3]);
		AssertEquals ("#04-06", "B", res [4]);
		AssertEquals ("#04-07", string.Empty, res [5]);
		AssertEquals ("#04-08", string.Empty, res [6]);

		// Trimming (3 tests)
		res = "..A".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#05-01", 1, res.Length);
		AssertEquals ("#05-02", "A", res [0]);
		
		res = "A..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#06-01", 1, res.Length);
		AssertEquals ("#06-02", "A", res [0]);
		
		res = "..A..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#07-01", 1, res.Length);
		AssertEquals ("#07-02", "A", res [0]);

		// Lingering Tail
		res = "..A..B..".Split (new Char[] { '.' }, 2, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#08-01", 2, res.Length);
		AssertEquals ("#08-02", "A", res [0]);
		AssertEquals ("#08-03", "B..", res [1]);

		// Whitespace and Long split chain (removing empty chars)
		res = "  A\tBC\n\rDEF    GHI  ".Split ((Char[])null, StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#09-01", 4, res.Length);
		AssertEquals ("#09-02", "A", res [0]);
		AssertEquals ("#09-03", "BC", res [1]);
		AssertEquals ("#09-04", "DEF", res [2]);
		AssertEquals ("#09-05", "GHI", res [3]);

		// Nothing but separators
		res = "..,.;.,".Split (new Char[]{'.',',',';'},2,StringSplitOptions.RemoveEmptyEntries);
		AssertEquals ("#10-01", 0, res.Length);

		// Complete testseries
		char[] dash = new Char[] { '/' };
		StringSplitOptions o = StringSplitOptions.RemoveEmptyEntries;
		AssertEquals ("#11-01", "hi", "hi".Split (dash, o)[0]);
		AssertEquals ("#11-02", "hi", "hi/".Split (dash, o)[0]);
		AssertEquals ("#11-03", "hi", "/hi".Split (dash, o)[0]);

		AssertEquals ("#11-04-1", "hi..", "hi../".Split (dash, o)[0]);
		AssertEquals ("#11-04-2", "hi..", "/hi..".Split (dash, o)[0]);

		res = "/hi/..".Split (dash, o);
		AssertEquals ("#11-05-1", "hi", res[0]);
		AssertEquals ("#11-05-2", "..", res[1]);
		AssertEquals ("#11-09-3", 2, res.Length);

		res = "hi/..".Split (dash, o);
		AssertEquals ("#11-06-1", "hi", res[0]);
		AssertEquals ("#11-06-2", "..", res[1]);
		AssertEquals ("#11-09-3", 2, res.Length);

		res = "hi/../".Split (dash, o);
		AssertEquals ("#11-07-1", "hi", res[0]);
		AssertEquals ("#11-07-2", "..", res[1]);
		AssertEquals ("#11-07-3", 2, res.Length);

		res = "/hi../".Split (dash, o);
		AssertEquals ("#11-08-1", "hi..", res[0]);
		AssertEquals ("#11-08-2", 1, res.Length);

		res = "/hi/../".Split (dash, o);
		AssertEquals ("#11-09-1", "hi", res[0]);
		AssertEquals ("#11-09-2", "..", res[1]);
		AssertEquals ("#11-09-3", 2, res.Length);
	}

	[Test]
	public void Normalize1 ()
	{
		string s = "\u03B1\u0313\u0345";
		Assert ("#1", s.IsNormalized (s, NormalizationForm.FormC));
		AssertEquals ("#2", s, s.Normalize (NormalizationForm.FormC));
	}
#endif
}

}
