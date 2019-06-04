// CompareInfoTest.cs - NUnit Test Cases for the
// System.Globalization.CompareInfo class
//
// Dick Porter <dick@ximian.com>
// Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2003-2005 Novell, Inc.  http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System.Globalization
{

[TestFixture]
[Category ("ManagedCollator")]
public class CompareInfoTest
{
	static bool doTest = Environment.GetEnvironmentVariable ("MONO_DISABLE_MANAGED_COLLATION") != "yes";

	public CompareInfoTest() {}

	[Test]
	public void Compare()
	{
		string s1 = "foo";
		
		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", ""), "Compare two empty strings");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, ""), "Compare string with empty string");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", s1), "Compare empty string with string");

		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "", 0), "Compare two empty strings, with 0 offsets");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, "", 0), "Compare string with empty string, with 0 offsets");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, s1, 0), "Compare empty string with string, with 0 offsets");

		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "".Length, "", 0, "".Length), "Compare two empty strings, with 0 offsets and specified lengths");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1.Length, "", 0, "".Length), "Compare string with empty string, with 0 offsets and specified lengths");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "".Length, s1, 0, s1.Length), "Compare empty string with string, with 0 offsets and specified lengths");

		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s1.Length, s1, s1.Length), "Compare two strings, with offsets == string lengths");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s1.Length, s1, 0), "Compare two strings, with first offset == string length");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1, s1.Length), "Compare two strings, with second offset == string length");

		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, 0, s1, 0, 0), "Compare two strings, with zero lengths");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, 0, s1, 0, s1.Length), "Compare two strings, with first length zero");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1.Length, s1, 0, 0), "Compare strings, with second length zero");
		
		Assert.AreEqual (0, CultureInfo.InvariantCulture.CompareInfo.Compare (null, null), "Compare two null references");
		Assert.AreEqual (1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", null), "Compare a string to a null reference");
		Assert.AreEqual (-1, CultureInfo.InvariantCulture.CompareInfo.Compare (null, ""), "Compare a null reference to a string");
	}

	// Culture-sensitive collation tests

	CompareInfo invariant = CultureInfo.InvariantCulture.CompareInfo;
	CompareInfo french = new CultureInfo ("fr").CompareInfo;
	CompareInfo japanese = new CultureInfo ("ja").CompareInfo;
	CompareInfo czech = new CultureInfo ("cs").CompareInfo;
	CompareInfo hungarian = new CultureInfo ("hu").CompareInfo;

	CompareOptions ignoreCW =
		CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;
	CompareOptions ignoreCN =
		CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;

	void AssertSortKey (string message, byte [] expected, string test)
	{
		AssertSortKey (message, expected, test, CompareOptions.None);
	}

	void AssertSortKey (string message, byte [] expected, string test, CompareOptions opt)
	{
		AssertSortKey (message, expected, test, opt, invariant);
	}

	void AssertSortKey (string message, byte [] expected, string test, CompareOptions opt, CompareInfo ci)
	{
		byte [] actual = ci.GetSortKey (test, opt).KeyData;
		Assert.AreEqual (expected, actual, message);
	}

	void AssertSortKeyLevel5 (string message, byte [] expected, string test)
	{
		byte [] tmp = invariant.GetSortKey (test).KeyData;
		int idx = 0;
		for (int i = 0; i < 4; i++, idx++)
			for (; tmp [idx] != 1; idx++)
				;
		byte [] actual = new byte [tmp.Length - idx];
		Array.Copy (tmp, idx, actual, 0, actual.Length);
		Assert.AreEqual (expected, actual, message);
	}

	void AssertCompare (string message, int result, string s1, string s2)
	{
		AssertCompare (message, result, s1, s2, CompareOptions.None);
	}

	void AssertCompare (string message, int result, string s1, string s2,
		CompareOptions opt)
	{
		AssertCompare (message, result, s1, s2, opt, invariant);
	}

	void AssertCompare (string message, int result, string s1, string s2,
		CompareOptions opt, CompareInfo ci)
	{
		int ret = ci.Compare (s1, s2, opt);
		if (result == 0)
			Assert.AreEqual (0, ret, message);
		else if (result < 0)
			Assert.IsTrue (ret < 0, message + String.Format ("(neg: {0})", ret));
		else
			Assert.IsTrue (ret > 0, message + String.Format ("(pos: {0})", ret));
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2)
	{
		int ret = invariant.Compare (s1, idx1, len1, s2, idx2, len2);
		if (result == 0)
			Assert.AreEqual (0, ret, message);
		else if (result < 0)
			Assert.IsTrue (ret < 0, message);
		else
			Assert.IsTrue (ret > 0, message);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2,
		CompareOptions opt, CompareInfo ci)
	{
		int ret = ci.Compare (s1, idx1, len1, s2, idx2, len2, opt);
		if (result == 0)
			Assert.AreEqual (0, ret, message);
		else if (result < 0)
			Assert.IsTrue (ret < 0, message);
		else
			Assert.IsTrue (ret > 0, message);
	}

	void AssertIndexOf (string message, int expected,
		string source, char target)
	{
		Assert.AreEqual (expected, invariant.IndexOf (source, target), message);
	}

	void AssertIndexOf (string message, int expected, string source,
		char target, CompareOptions opt)
	{
		Assert.AreEqual (expected, invariant.IndexOf (source, target, opt), message);
	}

	void AssertIndexOf (string message, int expected, string source,
		char target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		Assert.AreEqual (expected, ci.IndexOf (source, target, idx, len, opt), message);
	}

	void AssertIndexOf (string message, int expected,
		string source, string target)
	{
		Assert.AreEqual (expected, invariant.IndexOf (source, target), message);
	}

	void AssertIndexOf (string message, int expected, string source,
		string target, CompareOptions opt)
	{
		Assert.AreEqual (expected, invariant.IndexOf (source, target, opt), message);
	}

	void AssertIndexOf (string message, int expected, string source,
		string target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		Assert.AreEqual (expected, ci.IndexOf (source, target, idx, len, opt), message);
	}

	void AssertLastIndexOf (string message, int expected,
		string source, char target)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, CompareOptions opt)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target, opt), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, int idx, int len)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target, idx, len), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		Assert.AreEqual (expected, ci.LastIndexOf (source, target, idx, len, opt), message);
	}

	void AssertLastIndexOf (string message, int expected,
		string source, string target)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, CompareOptions opt)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target, opt), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, int idx, int len)
	{
		Assert.AreEqual (expected, invariant.LastIndexOf (source, target, idx, len), message);
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		Assert.AreEqual (expected, ci.LastIndexOf (source, target, idx, len, opt), message);
	}

	void AssertIsPrefix (string message, bool expected, string source,
		string target)
	{
		Assert.IsTrue (expected == invariant.IsPrefix (source, target), message);
	}

	void AssertIsPrefix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert.IsTrue (expected == invariant.IsPrefix (source, target, opt), message);
	}

	void AssertIsSuffix (string message, bool expected, string source,
		string target)
	{
		Assert.IsTrue (expected == invariant.IsSuffix (source, target), message);
	}

	void AssertIsSuffix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert.IsTrue (expected == invariant.IsSuffix (source, target, opt), message);
	}

	[Test]
	public void GetSortKey ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// AE == \u00C6
		AssertSortKey ("#1", new byte [] {0xE, 2, 0xE, 0x21, 1, 1,
			0x12, 0x12, 1, 1, 0}, "AE");
		AssertSortKey ("#2", new byte [] {0xE, 2, 0xE, 0x21, 1, 1,
			0x12, 0x12, 1, 1, 0}, "\u00C6");
		AssertSortKey ("#3", new byte [] {1, 1, 1, 1,
			0x80, 7, 6, 0x82, 0}, "-");
		AssertSortKey ("#4", new byte [] {1, 1, 1, 1,
			0x80, 7, 6, 0x82, 0x80, 7, 6, 0x82, 0}, "--");
		AssertSortKey ("#4", new byte [] {0xE, 2, 0xE, 9,
			0xE, 0xA, 1, 1, 0x12, 0x12, 0x12, 1, 1, 0x80, 0xB,
			6, 0x82, 0x80, 0xF, 6, 0x82, 0}, "A-B-C");
		AssertSortKey ("#6", new byte [] {0xE, 2, 1,
			0x17, 1, 0x12, 1, 1, 0}, "A\u0304");
		AssertSortKey ("#7", new byte [] {0xE, 2, 1,
			0x17, 1, 0x12, 1, 1, 0}, "\u0100");

		// StringSort
		AssertSortKey ("#8", new byte [] {
			0xE, 2, 6, 0x82, 1, 1, 1, 1, 0},
			"a-", CompareOptions.StringSort);
		// FIXME: not working (table fix is required)
//		AssertSortKey ("#9", new byte [] {
//			0xE, 2, 6, 0x82, 1, 1, 2, 0x3, 1, 1, 0},
//			"a\uFF0D", CompareOptions.StringSort);

		AssertSortKey ("#10", new byte [] {1, 1, 1, 1, 0}, "\u3007");
	}


	[Test]
	public void GetSortKeyIgnoreWidth ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0xE, 2, 1, 1, 0x13, 1, 1, 0}, "\uFF21");
		AssertSortKey ("#i2", new byte [] {
			0xE, 2, 1, 1, 0x12, 1, 1, 0}, "\uFF21", CompareOptions.IgnoreWidth);
		AssertSortKey ("#i3", new byte [] {
			0xE, 2, 1, 1, 0x3, 1, 1, 0}, "\uFF41");
		AssertSortKey ("#i4", new byte [] {
			0xE, 2, 1, 1, 1, 1, 0}, "\uFF41", CompareOptions.IgnoreWidth);
	}

	[Test]
	public void GetSortKeyDiacritical ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0xE, 0x21, 1, 0xE, 1, 1, 1, 0}, "e\u0301");
		AssertSortKey ("#i2", new byte [] {
			0xE, 0x21, 1, 0x12, 1, 1, 1, 0}, "e\u0302");
		AssertSortKey ("#i3", new byte [] {
			0xE, 0x21, 1, 0x13, 1, 1, 1, 0}, "e\u0308");
		AssertSortKey ("#i4", new byte [] {
			0xE, 0x21, 1, 0x1F, 1, 1, 1, 0}, "e\u0308\u0301");
		// FIXME: not working (table fix is required)
//		AssertSortKey ("#i5", new byte [] {
//			0xE, 0x21, 1, 0x16, 1, 1, 1, 0}, "e\u0344");
		AssertSortKey ("#i6", new byte [] {
			0x22, 2, 1, 0xE, 1, 1, 0xC4, 0xFF, 2, 0xFF, 0xFF, 1, 0}, "\u3041\u0301");
		AssertSortKey ("#i7", new byte [] {
			0xC, 0x21, 1, 0xE, 1, 1, 1, 0}, "1\u0301");
		AssertSortKey ("#i8", new byte [] {
			0x22, 0xA, 1, 3, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0}, "\u304B\u309B");
		AssertSortKey ("#i9", new byte [] {
			0x22, 0xA, 1, 3, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0}, "\u304C");

		AssertSortKey ("#i10", new byte [] {
			0xE, 0x2, 1, 0x12, 1, 0x12, 1, 1, 0},
			"A\u0302");
		AssertSortKey ("#i11", new byte [] {
			0xE, 0x2, 1, 0x65, 1, 0x12, 1, 1, 0},
			"A\u0302\u0320");
		AssertSortKey ("#i12", new byte [] {
			0xE, 0x2, 1, 0xB8, 1, 0x12, 1, 1, 0},
			"A\u0302\u0320\u0320");
		// LAMESPEC: Windows just appends diacritical weight without
//		AssertSortKey ("#i13", new byte [] {
//			0xE, 0x2, 1, 0xB, 1, 12, 1, 1, 0},
//			"A\u0302\u0320\u0320\u0320");
		// FIXME: not working (table fix is required)
//		AssertSortKey ("#i14", new byte [] {
//			0xE, 0x2, 1, 0xF2, 1, 0x12, 1, 1, 0},
//			"A\u20E1");
		// LAMESPEC: It should not be equivalent to \u1EA6
		AssertSortKey ("#i15", new byte [] {
			0xE, 0x2, 1, 0x1F, 1, 0x12, 1, 1, 0},
			"A\u0308\u0301");
		AssertSortKey ("#i16", new byte [] {
			0xE, 0x2, 1, 0x1F, 1, 0x12, 1, 1, 0},
			"\u1EA6");

	}

	[Test]
	public void GetSortKeyIgnoreNonSpaceKana ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0x22, 0x1A, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u305F");
		AssertSortKey ("#i2", new byte [] {
			0x22, 0x1A, 1, 3, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u3060");
		AssertSortKey ("#i3", new byte [] {
			0x22, 0x1A, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u305F", CompareOptions.IgnoreNonSpace);
		AssertSortKey ("#i4", new byte [] {
			0x22, 0x1A, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u3060", CompareOptions.IgnoreNonSpace);
	}

	[Test]
	public void GetSortKeySpecialWeight ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0x22, 0xA, 0x22, 2, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u3042");
		AssertSortKey ("#i2", new byte [] {
			0x22, 0xA, 0x22, 2, 1, 1, 1, 0xFF, 3, 5, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u30FC");
		AssertSortKey ("#i3", new byte [] {
			0x22, 0xA, 0x22, 2, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u30FC", CompareOptions.IgnoreNonSpace);

		AssertSortKey ("#i4", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 2, 3, 1, 1, 0xFF, 2, 0xC4, 0xC4, 0xFF, 0xFF, 1, 0},
			"\u30AB\u30AC");
		AssertSortKey ("#i5", new byte [] {
			0x22, 0xA, 0x22, 0xA, 0x22, 2, 1, 1, 1, 0xFF, 3, 3, 5, 2, 0xC4, 0xC4, 0xC4, 0xFF, 0xFF, 1, 0},
			"\u30AB\u30AB\u30FC");
		AssertSortKey ("#i6", new byte [] {
			0x22, 0xA, 0x22, 2, 0x22, 2, 1, 1, 1, 0xFF, 3, 3, 5, 2, 0xC4, 0xC4, 0xC4, 0xFF, 0xFF, 1, 0},
			"\u30AB\u30A2\u30FC");
		AssertSortKey ("#i7", new byte [] {
			0x22, 0xA, 0x22, 2, 0x22, 2, 0x22, 0xA, 1, 1, 1, 0xFF, 3, 3, 5, 2, 0xC4, 0xC4, 0xC4, 0xC4, 0xFF, 0xFF, 1, 0},
			"\u30AB\u30A2\u30FC\u30AB");
		AssertSortKey ("#i8", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u309D");
		AssertSortKey ("#i9", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 2, 3, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u309E");
		AssertSortKey ("#i10", new byte [] {
			0x22, 0x2, 0x22, 0x2, 1, 2, 3, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u3042\u309E");//not possible
		AssertSortKey ("#i11", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u30FD");//not possible
		AssertSortKey ("#i12", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 2, 3, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u30FE");//not possible
		AssertSortKey ("#i13", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 1, 1, 0xFF, 3, 4, 2, 0xC4, 0xC4, 0xFF, 0xFF, 1, 0},
			"\u30AB\u30FD");
		AssertSortKey ("#i14", new byte [] {
			0x22, 0xA, 0x22, 2, 1, 1, 1, 0xFF, 3, 5, 2, 0xC4, 0xC4, 0xFF, 0xC4, 0xC4, 0xFF, 1, 0},
			"\uFF76\uFF70");
		AssertSortKey ("#i15", new byte [] {
			0x22, 0xA, 0x22, 0xA, 1, 2, 5, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
			"\u304B\u3005");
		AssertSortKey ("#i16", new byte [] {
			0xAF, 9, 0xAF, 9, 1, 2, 5, 1, 1, 1, 0},
			"\u5EA6\u3005");
		AssertSortKey ("#i17", new byte [] {
			0xE, 2, 0xE, 2, 1, 2, 5, 1, 1, 1, 0},
			"a\u3005"); // huh
		// Not working, but I wonder if it is really FIXME.
//		AssertSortKey ("#i18", new byte [] {
//			0xFF, 0xFF, 1, 1, 1, 1, 0},
//			"\u3005");
		// LAMESPEC. No one can handle \u3031 correctly.
//		AssertSortKey ("#i19", new byte [] {
//			0x22, 0x22, 0x22, 0xC, 0x22, 0xC, 1, 1, 1, 0xFF, 3, 4, 2, 0xFF, 0xFF, 1, 0},
//			"\u306A\u304F\u3031");

		// IgnoreWidth -> all Kana becomes half-width
		AssertSortKey ("#i20", new byte [] {
			34, 26, 34, 3, 34, 44, 1, 3, 2, 3, 1, 1, 255, 2, 196, 196, 196, 255, 196, 196, 196, 255, 1, 0},
			"\uFF80\uFF9E\uFF72\uFF8C\uFF9E", CompareOptions.IgnoreWidth);
		AssertSortKey ("#i21", new byte [] {
			34, 26, 34, 3, 34, 44, 1, 3, 2, 3, 1, 1, 255, 2, 196, 196, 196, 255, 196, 196, 196, 255, 1, 0},
			"\u30C0\u30A4\u30D6", CompareOptions.IgnoreWidth);

		AssertSortKey ("#i22", new byte [] {
			0x22, 0x2A, 0x22, 2, 0x22, 0x44, 1, 3, 1, 1, 0xFF,
			3, 5, 2, 0xC4, 0xC4, 0xC4, 0xFF, 0xC4, 0xC4, 0xC4,
			0xFF, 1, 0},
			"\u30D0\u30FC\u30EB", CompareOptions.IgnoreWidth);
		AssertSortKey ("#i23", new byte [] {
			0x22, 0x2A, 0x22, 2, 0x22, 0x44, 1, 3, 1, 1, 0xFF,
			3, 5, 2, 0xC4, 0xC4, 0xC4, 0xFF, 0xC4, 0xC4, 0xC4,
			0xFF, 1, 0},
			"\uFF8A\uFF9E\uFF70\uFF99", CompareOptions.IgnoreWidth);
		// extender + IgnoreNonSpace
		AssertSortKey ("#i24", new byte [] {
			0x22, 2, 0x22, 2, 1, 1, 1, 0xFF, 2, 0xFF, 0xFF, 1, 0},
			"\u3042\u309D", CompareOptions.IgnoreNonSpace);
	}

	[Test]
	public void GetSortKeyLevel5 ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// shift weight
		AssertSortKeyLevel5 ("#8", new byte [] {
			0x80, 7, 6, 0x82, 0x80, 0x2F, 6, 0x82, 0},
			'-' + new string ('A', 10) + '-');
		AssertSortKeyLevel5 ("#9", new byte [] {
			0x80, 7, 6, 0x82, 0x80, 0xFF, 6, 0x82, 0},
			'-' + new string ('A', 62) + '-');
		AssertSortKeyLevel5 ("#10", new byte [] {
			0x80, 7, 6, 0x82, 0x81, 3, 6, 0x82, 0},
			'-' + new string ('A', 63) + '-');
		AssertSortKeyLevel5 ("#11", new byte [] {
			0x80, 7, 6, 0x82, 0x81, 0x97, 6, 0x82, 0},
			'-' + new string ('A', 100) + '-');
		AssertSortKeyLevel5 ("#12", new byte [] {
			0x80, 7, 6, 0x82, 0x8F, 0xA7, 6, 0x82, 0},
			'-' + new string ('A', 1000) + '-');
		AssertSortKeyLevel5 ("#13", new byte [] {
			0x80, 7, 6, 0x82, 0x9A, 0x87, 6, 0x82, 0},
			'-' + new string ('A', 100000) + '-');
		// This shows how Windows is broken.
//		AssertSortKeyLevel5 ("#14",
//			0x80, 7, 6, 0x82, 0x89, 0x07, 6, 0x82, 0},
//			'-' + new string ('A', 1000000) + '-');

	}

	[Test]
	public void GetSortKey_Options ()
	{
		Array values = Enum.GetValues (typeof (CompareOptions));
		foreach (int i in values) {
			CompareOptions option = (CompareOptions) i;
			if (option == CompareOptions.OrdinalIgnoreCase || option == CompareOptions.Ordinal) {
				try {
					french.GetSortKey ("foo", option);
					Assert.Fail ("#1: " + option.ToString ());
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2: " + option.ToString ());
					Assert.IsNotNull (ex.Message, "#2: " + option.ToString ());
					Assert.IsNotNull (ex.ParamName, "#3: " + option.ToString ());
					Assert.AreEqual ("options", ex.ParamName, "#4: " + option.ToString ());
					Assert.IsNull (ex.InnerException, "#5: " + option.ToString ());
				}
			} else {
				french.GetSortKey ("foo", option);
			}
		}
	}

	[Test]
	[Category ("NotDotNet")]
	public void FrenchSort ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// invariant
		AssertSortKey ("#inv-1", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 0x12, 1, 1, 1, 0}, "c\u00F4te");
		AssertSortKey ("#inv-1-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 0x12, 1, 1, 1, 0}, "co\u0302te");
		AssertSortKey ("#inv-1-3", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 2, 2, 0x15, 1, 1, 1, 0}, "cote\u0306");
		AssertSortKey ("#inv-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 2, 2, 0xE, 1, 1, 1, 0}, "cot\u00E9");
		AssertSortKey ("#inv-2-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 0x14, 1, 1, 1, 0}, "co\u030Cte");
// They are all bugs in 2.0:
// #inv-3: should not be 0 since those sortkey values differ.
// #inv-4: should not be -1 since co\u0302te sortkey is bigger than cote\u0306.
		AssertCompare ("#inv-3", 1, "c\u00F4te", "cot\u00E9");
		AssertCompare ("#inv-4", 1, "co\u0302te", "cote\u0306");
		AssertCompare ("#inv-5", 1, "co\u030Cte", "cote\u0306");

		// french
		AssertSortKey ("#fr-1", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 2, 0x12, 1, 1, 1, 0}, "c\u00F4te", CompareOptions.None, french);
		AssertSortKey ("#fr-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 0xE, 1, 1, 1, 0}, "cot\u00E9", CompareOptions.None, french);
		AssertCompare ("#fr-3", -1, "c\u00F4te", "cot\u00E9", CompareOptions.None, french);
		// FIXME: why does .NET return 1 ?
//		AssertCompare ("#fr-4", -1, "co\u0302te", "cote\u0306", CompareOptions.None, french);
//		AssertCompare ("#fr-4", -1, "co\u030Cte", "cote\u0306", CompareOptions.None, french);
	}

	[Test]
	public void GetSortKeyThai ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0x1E, 7, 0x1F, 0x28, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E3A");
		AssertSortKey ("#i2", new byte [] {
			0x1E, 7, 1, 3, 1, 1, 1, 0},
			"\u0E01\u0E3B");
// FIXME: not working (table fix required)
//		AssertSortKey ("#i6", new byte [] {
//			0x1E, 7, 0xA, 0xF9, 1, 3, 1, 1, 1, 0},
//			"\u0E01\u0E3F");
		AssertSortKey ("#i7", new byte [] {
			0x1E, 7, 0x1E, 2, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E40");
		AssertSortKey ("#i8", new byte [] {
			0x1E, 7, 0x1E, 3, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E41");
		AssertSortKey ("#i9", new byte [] {
			0x1E, 7, 0x1E, 4, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E42");
		AssertSortKey ("#i10", new byte [] {
			0x1E, 7, 0x1E, 5, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E43");
		AssertSortKey ("#i11", new byte [] {
			0x1E, 7, 0x1E, 6, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E44");
		AssertSortKey ("#i12", new byte [] {
			0x1E, 7, 0x1F, 0x29, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E45");
		AssertSortKey ("#i13", new byte [] {
			0x1E, 7, 0x1F, 0x2A, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E46");
// FIXME: not working (U+E47 table fix required)
//		AssertSortKey ("#i14", new byte [] {
//			0x1E, 7, 1, 5, 1, 1, 1, 0},
//			"\u0E01\u0E47");
		AssertSortKey ("#i15", new byte [] {
			0x1E, 7, 1, 6, 1, 1, 1, 0},
			"\u0E01\u0E48");
		AssertSortKey ("#i16", new byte [] {
			0x1E, 7, 1, 7, 1, 1, 1, 0},
			"\u0E01\u0E49");
		AssertSortKey ("#i17", new byte [] {
			0x1E, 7, 1, 8, 1, 1, 1, 0},
			"\u0E01\u0E4A");
		AssertSortKey ("#i18", new byte [] {
			0x1E, 7, 1, 9, 1, 1, 1, 0},
			"\u0E01\u0E4B");
// FIXME: not working (U+E47 table fix required)
//		AssertSortKey ("#i19", new byte [] {
//			0x1E, 7, 1, 8, 1, 1, 1, 0},
//			"\u0E01\u0E48\u0E47");
		AssertSortKey ("#i20", new byte [] {
			0x1E, 7, 0x1E, 4, 0x1E, 0xD, 1, 3, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E42\u0E02");
		AssertSortKey ("#i21", new byte [] {
			0x1E, 7, 0x1E, 0xD, 1, 3, 3, 1, 1, 1, 0},
			"\u0E01\u0E02");
	}

	[Test]
	public void GetSortKeyCzechTailoring ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#i1", new byte [] {
			0xE, 0xA, 0xE, 0x2C, 1, 1, 1, 1, 0},
			"ch");
		AssertSortKey ("#cs1", new byte [] {
			0xE, 0x2E, 1, 1, 1, 1, 0},
			"ch", CompareOptions.None, czech);
		AssertSortKey ("#i2", new byte [] {
			0xE, 0x8A, 1, 0x14, 1, 1, 1, 0},
			"r\u030C");
		AssertSortKey ("#cs2", new byte [] {
			0xE, 0x8A, 1, 0x14, 1, 1, 1, 0},
			"r\u030C", CompareOptions.None, czech);
	}

	[Test]
	public void GetSortKeyHungarianTailoring ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#1", new byte [] {
			0xE, 0xE, 1, 1, 0x1A, 1, 1, 0},
			"CS", CompareOptions.None, hungarian);
		AssertSortKey ("#2", new byte [] {
			0xE, 0xE, 1, 1, 0x12, 1, 1, 0},
			"Cs", CompareOptions.None, hungarian);
		AssertSortKey ("#3", new byte [] {
			0xE, 0xE, 1, 1, 1, 1, 0},
			"cs", CompareOptions.None, hungarian);
		AssertSortKey ("#4", new byte [] {
			0xE, 0x1C, 1, 1, 0x1A, 1, 1, 0},
			"DZ", CompareOptions.None, hungarian);
		AssertSortKey ("#5", new byte [] {
			0xE, 0x1C, 1, 1, 0x12, 1, 1, 0},
			"Dz", CompareOptions.None, hungarian);
		AssertSortKey ("#6", new byte [] {
			0xE, 0x1C, 1, 1, 1, 1, 0},
			"dz", CompareOptions.None, hungarian);
		AssertSortKey ("#7", new byte [] {
			0xE, 0x75, 1, 1, 0x1A, 1, 1, 0},
			"NY", CompareOptions.None, hungarian);
		AssertSortKey ("#8", new byte [] {
			0xE, 0x75, 1, 1, 0x12, 1, 1, 0},
			"Ny", CompareOptions.None, hungarian);
		AssertSortKey ("#9", new byte [] {
			0xE, 0x75, 1, 1, 1, 1, 0},
			"ny", CompareOptions.None, hungarian);
		AssertSortKey ("#10", new byte [] {
			0xE, 0xB1, 1, 1, 0x1A, 1, 1, 0},
			"ZS", CompareOptions.None, hungarian);
		AssertSortKey ("#11", new byte [] {
			0xE, 0xB1, 1, 1, 0x12, 1, 1, 0},
			"Zs", CompareOptions.None, hungarian);
		AssertSortKey ("#12", new byte [] {
			0xE, 0xB1, 1, 1, 1, 1, 0},
			"zs", CompareOptions.None, hungarian);

		// Windows seems to have bugs around repetitive characters
		// that is tailored.
//		AssertSortKey ("#x", new byte [] {
//			0xE, 0x2E, 1, 1, 1, 1, 0},
//			"CCS", CompareOptions.None, hungarian);

		// FIXME: we need to handle case insensitivity
	}

	[Test]
	public void CustomCJKTable ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertSortKey ("#1", new byte [] {
			0x9E, 9, 0x9E, 0x11, 1, 1, 1, 1, 0},
			"\u4E03\u4E09");
		AssertSortKey ("#2", new byte [] {
			0x84, 0xD3, 0x84, 0x61, 1, 1, 1, 1, 0},
			"\u4E03\u4E09", CompareOptions.None, japanese);
	}

	[Test]
	[Category ("NotDotNet")]
	public void CultureSensitiveCompare ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertCompare ("#1", -1, "1", "2");
		AssertCompare ("#2", 1, "A", "a");
		AssertCompare ("#3", 0, "A", "a", CompareOptions.IgnoreCase);
		AssertCompare ("#4", 0, "\uFF10", "0", CompareOptions.IgnoreWidth);
		AssertCompare ("#5", 0, "\uFF21", "a", ignoreCW);
		AssertCompare ("#6", 1, "12", "1");
// BUG in .NET 2.0: See GetSortKey() test that assures sortkeys for "AE" and
// "\u00C6" are equivalent.
		AssertCompare ("#7", 0, "AE", "\u00C6");
		AssertCompare ("#8", 0, "AB\u01c0C", "A\u01c0B\u01c0C", CompareOptions.IgnoreSymbols);
// BUG in .NET 2.0: ditto.
		AssertCompare ("#9", 0, "A\u0304", "\u0100");
		AssertCompare ("#10", 1, "ABCABC", 5, 1, "1", 0, 1, CompareOptions.IgnoreCase, invariant);
		AssertCompare ("#11", 0, "-d:NET_2_0", 0, 1, "-", 0, 1);

// BUG in .NET 2.0: ditto.
		AssertCompare ("#12", 0, "ae", "\u00E6");
		AssertCompare ("#13", 0, "\u00E6", "ae");
		AssertCompare ("#14", 0, "\u00E6s", 0, 1, "ae", 0, 2);

		// target is "empty" (in culture-sensitive context).
// BUG in .NET 2.0: \u3007 is totally-ignored character as a GetSortKey()
// result, while it is not in Compare().
		AssertCompare ("#17", 0, String.Empty, "\u3007");
		AssertCompare ("#18", 1, "A", "\u3007");
		AssertCompare ("#19", 1, "ABC", "\u3007");

		// shift weight comparison 
		AssertCompare ("#20", 1, "--start", "--");
		// expansion
// BUG in .NET 2.0: the same 00C6/00E6 issue.
		AssertCompare ("#21", -1, "\u00E6", "aes");

// bug #78748
		AssertCompare ("#22", -1, "++)", "+-+)");
		AssertCompare ("#23", -1, "+-+)", "+-+-)");
		AssertCompare ("#24", 1, "+-+-)", "++)");
		// BUG in .NET: it returns 1
		AssertCompare ("#25", -1, "+-+-)", "-+-+)");
		AssertCompare ("#26", -1, "+-+)", "-+-+)");
		AssertCompare ("#27", -1, "++)", "-+-+)");
		// bug #79714
		AssertCompare ("#28", 1, "aa ", "A");
	}

	[Test]
	[Category ("NotDotNet")]
	public void CompareSpecialWeight ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// Japanese (in invariant)
// BUG in .NET 2.0 : half-width kana should be bigger.
		AssertCompare ("#1", 1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertCompare ("#2", 0, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D", CompareOptions.IgnoreWidth);
		AssertCompare ("#3", 0, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6", CompareOptions.IgnoreWidth);
		AssertCompare ("#4", 1, "\u3042\u309D", "\u3042\u3042");
		AssertCompare ("#5", 0, "\u3042\u309D", "\u3042\u3042", CompareOptions.IgnoreNonSpace);
		AssertCompare ("#6", 0, "\uFF8A\uFF9E\uFF70\uFF99",
			"\u30D0\u30FC\u30EB", CompareOptions.IgnoreWidth);

		// extender in target
// BUG in .NET 2.0 : an extender should result in bigger sortkey
		AssertCompare ("#7", -1, "\u30D1\u30A2", "\u30D1\u30FC");
		AssertCompare ("#8", 0, "\u30D1\u30A2", "\u30D1\u30FC", CompareOptions.IgnoreNonSpace);
		// extender in source
// BUG in .NET 2.0 : vice versa
		AssertCompare ("#9", 1, "\u30D1\u30FC", "\u30D1\u30A2");
		AssertCompare ("#10", 0, "\u30D1\u30FC", "\u30D1\u30A2", CompareOptions.IgnoreNonSpace);
	}

	[Test]
	public void IndexOfChar ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIndexOf ("#1", -1, "ABC", '1');
		AssertIndexOf ("#2", 2, "ABCABC", 'c', CompareOptions.IgnoreCase);
		AssertIndexOf ("#3", 1, "ABCABC", '\uFF22', ignoreCW);
		AssertIndexOf ("#4", 4, "ABCDE", '\u0117', ignoreCN);
		AssertIndexOf ("#5", 1, "ABCABC", 'B', 1, 5, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#6", 4, "ABCABC", 'B', 2, 4, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#7", 1, "\u30D1\u30FC", '\u30A2', CompareOptions.IgnoreNonSpace);
		AssertIndexOf ("#8", 1, "UAE", '\u00C6');
		AssertIndexOf ("#8-2", 1, "AAE", '\u00C6');
		AssertIndexOf ("#9", -1, "UA", '\u00C6');
		AssertIndexOf ("#10", -1, "UE", '\u00C6');
	}

	[Test]
	[Category ("NotDotNet")]
	public void IndexOfCharMSBug ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIndexOf ("#1", 0, "\u00E6", 'a');
	}

	[Test]
	public void LastIndexOfChar ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertLastIndexOf ("#1", -1, "ABC", '1');
		AssertLastIndexOf ("#2", 5, "ABCABC", 'c', CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#3", 4, "ABCABC", '\uFF22', ignoreCW);
		AssertLastIndexOf ("#4", 4, "ABCDE", '\u0117', ignoreCN);
		AssertLastIndexOf ("#5", 1, "ABCABC", 'B', 3, 3);
		AssertLastIndexOf ("#6", 4, "ABCABC", 'B', 4, 4);
		AssertLastIndexOf ("#7", -1, "ABCABC", 'B', 5, 1);
		AssertLastIndexOf ("#8", 1, "UAE", '\u00C6');
		AssertLastIndexOf ("#8-2", 1, "UAEE", '\u00C6');
		AssertLastIndexOf ("#9", -1, "UA", '\u00C6');
		AssertLastIndexOf ("#10", -1, "UE", '\u00C6');
		AssertLastIndexOf ("#11", 0, "\\", '\\');
		Assert.AreEqual (0, new CultureInfo ("en").CompareInfo.LastIndexOf ("\\", '\\'), "#11en");
		Assert.AreEqual (0, new CultureInfo ("ja").CompareInfo.LastIndexOf ("\\", '\\'), "#11ja");
		AssertLastIndexOf ("#12", 8, "/system/web", 'w');
		Assert.AreEqual (8, new CultureInfo ("sv").CompareInfo.LastIndexOf ("/system/web", 'w'), "#12sv");
	}

	[Test]
	[Category ("NotDotNet")]
	public void LastIndexOfCharMSBug ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIndexOf ("#1", 0, "\u00E6", 'a');
	}

	[Test]
	[Category ("NotDotNet")]
	public void IsPrefix ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIsPrefix ("#1", false, "ABC", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#2", false, "BC", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#3", true, "C", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#4", true, "EDCBA", "\u0117", ignoreCN);
		AssertIsPrefix ("#5", true, "ABC", "AB", CompareOptions.IgnoreCase);
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertIsPrefix ("#6", true, "ae", "\u00E6", CompareOptions.None);
		AssertIsPrefix ("#7", true, "\u00E6", "ae", CompareOptions.None);

// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertIsPrefix ("#8", true, "\u00E6", "a", CompareOptions.None);
		AssertIsPrefix ("#9", true, "\u00E6s", "ae", CompareOptions.None);
		AssertIsPrefix ("#10", false, "\u00E6", "aes", CompareOptions.None);
		AssertIsPrefix ("#11", true, "--start", "--", CompareOptions.None);
		AssertIsPrefix ("#12", true, "-d:NET_1_1", "-", CompareOptions.None);
		AssertIsPrefix ("#13", false, "-d:NET_1_1", "@", CompareOptions.None);
		// U+3007 is completely ignored character.
		AssertIsPrefix ("#14", true, "\uff21\uff21", "\uff21", CompareOptions.None);
// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIsPrefix ("#15", true, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);
		AssertIsPrefix ("#16", true, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		AssertIsPrefix ("#17", true, "\\b\\a a", "\\b\\a a");
		Assert.IsTrue (new CultureInfo ("en").CompareInfo.IsPrefix ("\\b\\a a", "\\b\\a a"), "#17en");
		Assert.IsTrue (new CultureInfo ("ja").CompareInfo.IsPrefix ("\\b\\a a", "\\b\\a a"), "#17ja");
	}

	[Test]
	public void IsPrefixSpecialWeight ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// Japanese (in invariant)
		AssertIsPrefix ("#1", false, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIsPrefix ("#2", true, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D", CompareOptions.IgnoreWidth);
		AssertIsPrefix ("#2-2", false, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIsPrefix ("#3", true, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6", CompareOptions.IgnoreWidth);
		AssertIsPrefix ("#3-2", false, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6");
		AssertIsPrefix ("#4", false, "\u3042\u309D", "\u3042\u3042");
		AssertIsPrefix ("#5", true, "\u3042\u309D", "\u3042\u3042", CompareOptions.IgnoreNonSpace);
		AssertIsPrefix ("#6", true, "\uFF8A\uFF9E\uFF70\uFF99",
			"\u30D0\u30FC\u30EB", CompareOptions.IgnoreWidth);

		// extender in target
		AssertIsPrefix ("#7", false, "\u30D1\u30A2", "\u30D1\u30FC");
		AssertIsPrefix ("#8", true, "\u30D1\u30A2", "\u30D1\u30FC", CompareOptions.IgnoreNonSpace);
		// extender in source
		AssertIsPrefix ("#9", false, "\u30D1\u30FC", "\u30D1\u30A2");
		AssertIsPrefix ("#10", true, "\u30D1\u30FC", "\u30D1\u30A2", CompareOptions.IgnoreNonSpace);

		// empty suffix always matches the source.
		AssertIsPrefix ("#11", true, "", "");
		AssertIsPrefix ("#12", true, "/test.css", "");

		// bug #76243
		AssertIsPrefix ("#13", false, "\u00e4_", "a");
	}

	[Test]
	[Category ("NotDotNet")]
	public void IsSuffix ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIsSuffix ("#1", true, "ABC", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#2", true, "BC", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#3", false, "CBA", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#4", true, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertIsSuffix ("#5", false, "\u00E6", "a", CompareOptions.None);
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertIsSuffix ("#6", true, "\u00E6", "ae", CompareOptions.None);
		AssertIsSuffix ("#7", true, "ae", "\u00E6", CompareOptions.None);
		AssertIsSuffix ("#8", false, "e", "\u00E6", CompareOptions.None);
		// U+3007 is completely ignored character.
		AssertIsSuffix ("#9", true, "\uff21\uff21", "\uff21", CompareOptions.None);
// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIsSuffix ("#10", true, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);
		AssertIsSuffix ("#11", true, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		// extender in target
		AssertIsSuffix ("#12", false, "\u30D1\u30A2", "\u30D1\u30FC");
		AssertIsSuffix ("#13", true, "\u30D1\u30A2", "\u30D1\u30FC", CompareOptions.IgnoreNonSpace);
		// extender in source
		AssertIsSuffix ("#14", false, "\u30D1\u30FC", "\u30D1\u30A2");
		AssertIsSuffix ("#15", true, "\u30D1\u30FC", "\u30D1\u30A2", CompareOptions.IgnoreNonSpace);
		// optimization sanity check
		AssertIsSuffix ("#16", true,
			"/configuration/system.runtime.remoting",
			"system.runtime.remoting");

		// empty suffix always matches the source.
		AssertIsSuffix ("#17", true, "", "");
		AssertIsSuffix ("#18", true, "/test.css", "");
		AssertIsSuffix ("#19", true, "/test.css", "/test.css");
		AssertIsSuffix ("#20", true, "\\b\\a a", "\\b\\a a");
		Assert.IsTrue (new CultureInfo ("en").CompareInfo.IsSuffix ("\\b\\a a", "\\b\\a a"), "#20en");
		Assert.IsTrue (new CultureInfo ("ja").CompareInfo.IsSuffix ("\\b\\a a", "\\b\\a a"), "#20ja");
	}

	[Test]
	[Category ("NotDotNet")]
	[Category ("NotWorking")]
	public void IsSuffixMSBug ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIsSuffix ("#1", true, "\u00E6", "e", CompareOptions.None);
	}

	[Test]
	public void IndexOfString ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertIndexOf ("#0", 0, "", "", CompareOptions.None);
		AssertIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertIndexOf ("#2", 2, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertIndexOf ("#3", 1, "ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
		AssertIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertIndexOf ("#5", 1, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#6", 1, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#7", -1, "ABCDEF", "BCD", 0, 3, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#8", 0, "-ABC", "-", CompareOptions.None);
		AssertIndexOf ("#9", 0, "--ABC", "--", CompareOptions.None);
		AssertIndexOf ("#10", -1, "--ABC", "--", 1, 2, CompareOptions.None, invariant);

		// U+3007 is completely ignored character.
		AssertIndexOf ("#12", 0, "\uff21\uff21", "\uff21", CompareOptions.None);

		AssertIndexOf ("#14", 0, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		AssertIndexOf ("#15", 0, "\uff21\uff21", "\u3007", CompareOptions.None);
		AssertIndexOf ("#15-2", 1, "\u3007\uff21", "\uff21", CompareOptions.None);
		// target is "empty" (in culture-sensitive context).
		AssertIndexOf ("#16", -1, String.Empty, "\u3007");

		AssertIndexOf ("#18", 0, "ABC", "\u3007");

		AssertIndexOf ("#19", 0, "\\b\\a a", "\\b\\a a");
		Assert.AreEqual (0, new CultureInfo ("en").CompareInfo.IndexOf ("\\b\\a a", "\\b\\a a"), "#19en");
		Assert.AreEqual (0, new CultureInfo ("ja").CompareInfo.IndexOf ("\\b\\a a", "\\b\\a a"), "#19ja");
	}

	[Test]
	[Category ("NotDotNet")]
	public void IndexOfStringWeird ()
	{
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertIndexOf ("#11", 0, "AE", "\u00C6", CompareOptions.None);

// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIndexOf ("#13", 0, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);

// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIndexOf ("#17", 0, "A", "\u3007");
	}
	
	[Test]
	public void IndexOfSpecialWeight ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// Japanese (in invariant)
		AssertIndexOf ("#1", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		// extender in target
		AssertIndexOf ("#1-2", -1, "\u30D1\u30A2", "\u30D1\u30FC");
		AssertIndexOf ("#1-3", 0, "\u30D1\u30A2", "\u30D1\u30FC", CompareOptions.IgnoreNonSpace);
		// extender in source
		AssertIndexOf ("#1-4", 0, "\u30D1\u30FC", "\u30D1\u30A2", CompareOptions.IgnoreNonSpace);
		AssertIndexOf ("#1-5", 1, "\u30D1\u30FC", "\u30A2", CompareOptions.IgnoreNonSpace);
		AssertIndexOf ("#2", 0, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D", CompareOptions.IgnoreWidth);
		AssertIndexOf ("#2-2", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIndexOf ("#3", 0, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6", CompareOptions.IgnoreWidth);
		AssertIndexOf ("#4", -1, "\u3042\u309D", "\u3042\u3042");
		AssertIndexOf ("#5", 0, "\u3042\u309D", "\u3042\u3042", CompareOptions.IgnoreNonSpace);
		AssertIndexOf ("#6", 0, "\uFF8A\uFF9E\uFF70\uFF99",
			"\u30D0\u30FC\u30EB", CompareOptions.IgnoreWidth);

	}

	[Test]
	public void LastIndexOfString ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertLastIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertLastIndexOf ("#2", 5, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#3", 4, "ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
		AssertLastIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#5", 4, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#6", 4, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#7", 1, "original", "rig", CompareOptions.None);

		AssertLastIndexOf ("#9", 0, "-ABC", "-", CompareOptions.None);
		AssertLastIndexOf ("#10", 0, "--ABC", "--", CompareOptions.None);
		AssertLastIndexOf ("#11", -1, "--ABC", "--", 2, 2, CompareOptions.None, invariant);
		AssertLastIndexOf ("#12", -1, "--ABC", "--", 4, 2, CompareOptions.None, invariant);

		// U+3007 is completely ignored character.
		AssertLastIndexOf ("#14", 1, "\uff21\uff21", "\uff21", CompareOptions.None);

		AssertLastIndexOf ("#16", 1, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		AssertLastIndexOf ("#17", 1, "\uff21\uff21", "\u3007", CompareOptions.None);
		AssertLastIndexOf ("#18", 1, "\u3007\uff21", "\uff21", CompareOptions.None);
		AssertLastIndexOf ("#19", 0, "\\b\\a a", "\\b\\a a");
		Assert.AreEqual (0, new CultureInfo ("en").CompareInfo.LastIndexOf ("\\b\\a a", "\\b\\a a"), "#19en");
		Assert.AreEqual (0, new CultureInfo ("ja").CompareInfo.LastIndexOf ("\\b\\a a", "\\b\\a a"), "#19ja");
		// bug #80612
		AssertLastIndexOf ("#20", 8, "/system/web", "w");
		Assert.AreEqual (8, new CultureInfo ("sv").CompareInfo.LastIndexOf ("/system/web", "w"), "#20sv");

		AssertLastIndexOf ("#21", 2, "foo", String.Empty);
	}

	[Test]
	[Category ("NotDotNet")]
	public void LastIndexOfStringDotnetWeird ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertLastIndexOf ("#8", 0, "\u00E6", "ae", CompareOptions.None);

// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertLastIndexOf ("#13", 0, "AE", "\u00C6", CompareOptions.None);

// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertLastIndexOf ("#15", 1, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);
	}

	[Test]
	public void LastIndexOfSpecialWeight ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// Japanese (in invariant)
		AssertLastIndexOf ("#1", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		// extender in target
		AssertLastIndexOf ("#1-2", -1, "\u30D1\u30A2", "\u30D1\u30FC");
		AssertLastIndexOf ("#1-3", 0, "\u30D1\u30A2", "\u30D1\u30FC", CompareOptions.IgnoreNonSpace);
		// extender in source
		AssertLastIndexOf ("#1-4", 0, "\u30D1\u30FC", "\u30D1\u30A2", CompareOptions.IgnoreNonSpace);
		// FIXME: not working (extender support is not complete. 
		// Currently private IsPrefix() cannot handle heading
		// extenders to consume previous primary char.)
//		AssertLastIndexOf ("#1-5", 1, "\u30D1\u30FC", "\u30A2", CompareOptions.IgnoreNonSpace);
		// this shows that Windows accesses beyond the length and
		// acquires the corresponding character to expand.
//		AssertLastIndexOf ("#1-6", 1, "\u30D1\u30FC", "\u30A2", 1, 1, CompareOptions.IgnoreNonSpace, invariant);
		AssertLastIndexOf ("#2", 0, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D", CompareOptions.IgnoreWidth);
		AssertLastIndexOf ("#3", 0, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6", CompareOptions.IgnoreWidth);
		AssertLastIndexOf ("#4", -1, "\u3042\u309D", "\u3042\u3042");
		AssertLastIndexOf ("#5", 0, "\u3042\u309D", "\u3042\u3042", CompareOptions.IgnoreNonSpace);
		AssertLastIndexOf ("#6", 0, "\uFF8A\uFF9E\uFF70\uFF99",
			"\u30D0\u30FC\u30EB", CompareOptions.IgnoreWidth);
	}

	[Test]
	public void LastIndexOfOrdinalString ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		AssertLastIndexOf ("#1", -1, "ABC", "1", CompareOptions.Ordinal);
		AssertLastIndexOf ("#2", 5, "ABCABC", "C", CompareOptions.Ordinal);
		AssertLastIndexOf ("#3", -1, "ABCABC", "\uFF22", CompareOptions.Ordinal);
		AssertLastIndexOf ("#4", 4, "ABCABC", "BC", CompareOptions.Ordinal);
		AssertLastIndexOf ("#5", 4, "BBCBBC", "BC", CompareOptions.Ordinal);
		AssertLastIndexOf ("#6", 1, "original", "rig", CompareOptions.Ordinal);
		AssertLastIndexOf ("#7", 0, "\\b\\a a", "\\b\\a a", CompareOptions.Ordinal);
	}

	[Test]
	public void NullCharacter ()
	{
		// for bug #76702
		Assert.AreEqual (-1, "MONO".IndexOf ("\0\0\0"), "#1");
		Assert.AreEqual (-1, "MONO".LastIndexOf ("\0\0\0"), "#2");
		Assert.AreEqual (1, "MONO".CompareTo ("\0\0\0"), "#3");

		// I don't really understand why they are so...
		AssertIndexOf ("#4", 0, "\0\0", "\0");
		AssertIndexOf ("#5", -1, "\0", "\0\0");
		AssertIndexOf ("#6", -1, "foo", "\0");
		AssertLastIndexOf ("#7", 1, "\0\0", "\0");
		AssertLastIndexOf ("#8", -1, "\0", "\0\0");
		AssertLastIndexOf ("#9", -1, "foo", "\0");
	}

	[Test]
	// LAMESPEC: MS.NET treats it as equivalent, while in IndexOf() it does not match.
	public void NullCharacterWeird ()
	{
		Assert.AreEqual (0, "MONO".CompareTo ("MONO\0\0\0"), "#4");
	}

	[Test]
	[Category ("NotDotNet")]
	public void OrdinalIgnoreCaseCompare ()
	{
		if (!doTest)
			Assert.Ignore ("Test is disabled.");

		// matches
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertCompare ("#1", 0, "AE", "\u00C6", CompareOptions.None);
// BUG in .NET 2.0 : It raises inappropriate ArgumentException.
		// should not match since it is Ordinal
		AssertCompare ("#2", -133, "AE", "\u00C6", CompareOptions.OrdinalIgnoreCase);

		AssertCompare ("#3", 1, "AE", "\u00E6", CompareOptions.None);
		// matches
		AssertCompare ("#4", 0, "AE", "\u00E6", CompareOptions.IgnoreCase);
		// should not match since it is Ordinal
		AssertCompare ("#5", -133, "AE", "\u00E6", CompareOptions.OrdinalIgnoreCase);

		AssertCompare ("#6", 0, "AE", "ae", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#7", 0, "aE", "ae", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#8", 0, "aE", "ae", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#9", 0, "ae", "ae", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#10", 0, "AE", "AE", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#11", 0, "aE", "AE", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#12", 0, "aE", "AE", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#13", 0, "ae", "AE", CompareOptions.OrdinalIgnoreCase);
		AssertCompare ("#14", 0, "ola", "OLA", CompareOptions.OrdinalIgnoreCase);
		// check ignorable characters
		AssertCompare ("#15", 0, "AE\uFFFC", "AE", CompareOptions.None);
		AssertCompare ("#16", 1, "AE\uFFFC", "AE", CompareOptions.OrdinalIgnoreCase);
	}

	[Test]
	public void OrdinalIgnoreCaseIndexOf ()
	{
		AssertIndexOf ("#1-1", 0, "ABC", "abc", CompareOptions.OrdinalIgnoreCase);
		AssertIndexOf ("#1-2", -1, "AEBECE", "\u00E6", CompareOptions.OrdinalIgnoreCase);
		AssertIndexOf ("#1-3", -1, "@_@", "`_`", CompareOptions.OrdinalIgnoreCase);
	}

	[Test]
	public void OrdinalIgnoreCaseIndexOfChar ()
	{
		AssertIndexOf ("#2-1", 0, "ABC", 'a', CompareOptions.OrdinalIgnoreCase);
		AssertIndexOf ("#2-2", -1, "AEBECE", '\u00C0', CompareOptions.OrdinalIgnoreCase);
		AssertIndexOf ("#2-3", -1, "@_@", '`', CompareOptions.OrdinalIgnoreCase);
	}

	[Test]
	public void OrdinalIgnoreCaseLastIndexOf ()
	{
		AssertLastIndexOf ("#1-1", 0, "ABC", "abc", CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#1-2", -1, "AEBECE", "\u00E6", CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#1-3", -1, "@_@", "`_`", CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#1-4", 1, "ABCDE", "bc", CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#1-5", -1, "BBBBB", "ab", CompareOptions.OrdinalIgnoreCase);
	}

	[Test]
	public void OrdinalIgnoreCaseLastIndexOfChar ()
	{
		AssertLastIndexOf ("#2-1", 0, "ABC", 'a', CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#2-2", -1, "AEBECE", '\u00C0', CompareOptions.OrdinalIgnoreCase);
		AssertLastIndexOf ("#2-3", -1, "@_@", '`', CompareOptions.OrdinalIgnoreCase);
	}

	[Test] // bug #80865
	public void IsPrefixOrdinalIgnoreCase ()
	{
		Assert.IsTrue ("aaaa".StartsWith ("A", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void IsPrefix_SourceNull ()
	{
		invariant.IsPrefix (null, "b");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void IsPrefix_PrefixNull ()
	{
		invariant.IsPrefix ("a", null, CompareOptions.None);
	}

	[Test]
	public void IsPrefix_PrefixEmpty ()
	{
		Assert.IsTrue (invariant.IsPrefix ("a", String.Empty));
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IsPrefix_CompareOptions_Invalid ()
	{
		invariant.IsPrefix ("a", "b", (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IsPrefix_CompareOptions_StringSort ()
	{
		invariant.IsPrefix ("a", "b", CompareOptions.StringSort);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void IsSuffix_SourceNull ()
	{
		invariant.IsSuffix (null, "b");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void IsSuffix_SuffixNull ()
	{
		invariant.IsSuffix ("a", null, CompareOptions.None);
	}

	[Test]
	public void IsSuffix_PrefixEmpty ()
	{
		Assert.IsTrue (invariant.IsSuffix ("a", String.Empty));
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IsSuffix_CompareOptions_Invalid ()
	{
		invariant.IsSuffix ("a", "b", (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IsSuffix_CompareOptions_StringSort ()
	{
		invariant.IsSuffix ("a", "b", CompareOptions.StringSort);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Compare_String_String_CompareOptions_Invalid ()
	{
		// validation of CompareOptions is made before null checks
		invariant.Compare (null, null, (CompareOptions) Int32.MinValue);
	}

	[Test]
	public void Compare_String_String_CompareOptions_StringSort ()
	{
		// StringSort is valid for Compare only
		Assert.AreEqual (-1, invariant.Compare ("a", "b", CompareOptions.StringSort));
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Compare_String_Int_String_Int_CompareOptions_Invalid ()
	{
		invariant.Compare (null, 0, null, 0, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Compare_String_Int_Int_String_Int_Int_CompareOptions_Invalid ()
	{
		invariant.Compare (null, 0, 0, null, 0, 0, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IndexOf_String_Char_Int_Int_CompareOptions_Invalid ()
	{
		invariant.IndexOf ("a", 'a', 0, 1, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IndexOf_String_Char_Int_Int_CompareOptions_StringSort ()
	{
		invariant.IndexOf ("a", 'a', 0, 1, CompareOptions.StringSort);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IndexOf_String_String_Int_Int_CompareOptions_Invalid ()
	{
		invariant.IndexOf ("a", "a", 0, 1, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void IndexOf_String_String_Int_Int_CompareOptions_StringSort ()
	{
		invariant.IndexOf ("a", "a", 0, 1, CompareOptions.StringSort);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void LastIndexOf_String_Char_Int_Int_CompareOptions_Invalid ()
	{
		invariant.LastIndexOf ("a", 'a', 0, 1, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void LastIndexOf_String_Char_Int_Int_CompareOptions_StringSort ()
	{
		invariant.LastIndexOf ("a", 'a', 0, 1, CompareOptions.StringSort);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void LastIndexOf_String_String_Int_Int_CompareOptions_Invalid ()
	{
		invariant.LastIndexOf ("a", "a", 0, 1, (CompareOptions) Int32.MinValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void LastIndexOf_String_String_Int_Int_CompareOptions_StringSort ()
	{
		invariant.LastIndexOf ("a", "a", 0, 1, CompareOptions.StringSort);
	}
}

}

