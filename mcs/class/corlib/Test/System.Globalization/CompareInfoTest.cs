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
public class CompareInfoTest : Assertion
{
	static bool doTest = Environment.GetEnvironmentVariable ("MONO_USE_MANAGED_COLLATION") == "yes";

	public CompareInfoTest() {}

	[Test]
	public void Compare()
	{
		string s1 = "foo";
		
		AssertEquals ("Compare two empty strings", 0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", ""));
		AssertEquals ("Compare string with empty string", 1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, ""));
		AssertEquals ("Compare empty string with string", -1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", s1));

		AssertEquals ("Compare two empty strings, with 0 offsets", 0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "", 0));
		AssertEquals ("Compare string with empty string, with 0 offsets", 1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, "", 0));
		AssertEquals ("Compare empty string with string, with 0 offsets", -1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, s1, 0));

		AssertEquals ("Compare two empty strings, with 0 offsets and specified lengths", 0, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "".Length, "", 0, "".Length));
		AssertEquals ("Compare string with empty string, with 0 offsets and specified lengths", 1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1.Length, "", 0, "".Length));
		AssertEquals ("Compare empty string with string, with 0 offsets and specified lengths", -1, CultureInfo.InvariantCulture.CompareInfo.Compare ("", 0, "".Length, s1, 0, s1.Length));

		AssertEquals ("Compare two strings, with offsets == string lengths", 0, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s1.Length, s1, s1.Length));
		AssertEquals ("Compare two strings, with first offset == string length", -1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s1.Length, s1, 0));
		AssertEquals ("Compare two strings, with second offset == string length", 1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1, s1.Length));

		AssertEquals ("Compare two strings, with zero lengths", 0, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, 0, s1, 0, 0));
		AssertEquals ("Compare two strings, with first length zero", -1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, 0, s1, 0, s1.Length));
		AssertEquals ("Compare strings, with second length zero", 1, CultureInfo.InvariantCulture.CompareInfo.Compare (s1, 0, s1.Length, s1, 0, 0));
		
	}

	// Culture-sensitive collation tests

	CompareInfo invariant = CultureInfo.InvariantCulture.CompareInfo;
	CompareInfo french = new CultureInfo ("fr").CompareInfo;

	CompareOptions ignoreCW =
		CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;

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
		/*
		int min = expected.Length < actual.Length ?
			expected.Length : actual.Length;
		for (int i = 0; i < min; i++)
			if (expected
		*/
		AssertEquals (message, expected, actual);
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
		AssertCompare (message, result, s1, 0, s1.Length,
			s2, 0, s2.Length, opt, ci);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2)
	{
		AssertCompare (message, result, s1, idx1, len1, s2, idx2, len2,
			CompareOptions.None, invariant);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2,
		CompareOptions opt, CompareInfo ci)
	{
		int ret = ci.Compare (s1, idx1, len1, s2, idx2, len2, opt);
		if (result == 0)
			AssertEquals (message, 0, ret);
		else if (result < 0)
			Assert (message, ret < 0);
		else
			Assert (message, ret > 0);
	}

	[Test]
	public void GetSortKey ()
	{
		if (!doTest)
			return;

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
	}

	[Test]
	public void FrenchSort ()
	{
		if (!doTest)
			return;

		// invariant
		AssertSortKey ("#inv-1", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 0x12, 1, 1, 1, 0}, "c\u00F4te");
		AssertSortKey ("#inv-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 2, 2, 0xE, 1, 1, 1, 0}, "cot\u00E9");
		AssertCompare ("#inv-3", 1, "c\u00F4te", "cot\u00E9");

		// french
		AssertSortKey ("#fr-1", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 2, 2, 0x12, 1, 1, 1, 0}, "c\u00F4te", CompareOptions.None, french);
		AssertSortKey ("#fr-2", new byte [] {0xE, 0xA, 0xE, 0x7C, 0xE, 0x99, 0xE, 0x21, 1, 0xE, 1, 1, 1, 0}, "cot\u00E9", CompareOptions.None, french);
		AssertCompare ("#fr-3", -1, "c\u00F4te", "cot\u00E9", CompareOptions.None, french);
	}

	[Test]
	public void CultureSensitiveCompare ()
	{
		if (!doTest)
			return;

		AssertCompare ("#1", -1, "1", "2");
		AssertCompare ("#2", 1, "A", "a");
		AssertCompare ("#3", 0, "A", "a", CompareOptions.IgnoreCase);
		AssertCompare ("#4", 0, "\uFF10", "0", CompareOptions.IgnoreWidth);
		AssertCompare ("#5", 0, "\uFF21", "a", ignoreCW);
		AssertCompare ("#6", 1, "12", "1");
		AssertCompare ("#7", 0, "AE", "\u00C6");
		AssertCompare ("#8", 0, "AB\u01c0C", "A\u01c0B\u01c0C", CompareOptions.IgnoreSymbols);
		AssertCompare ("#9", 0, "A\u0304", "\u0100");
		AssertCompare ("#10", 1, "ABCABC", 5, 1, "1", 0, 1, CompareOptions.IgnoreCase, invariant);
		AssertCompare ("#11", 0, "-d:NET_2_0", 0, 1, "-", 0, 1);
	}
}

}
