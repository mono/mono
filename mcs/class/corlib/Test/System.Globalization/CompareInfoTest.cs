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
		int ret = ci.Compare (s1, s2, opt);
		if (result == 0)
			AssertEquals (message, 0, ret);
		else if (result < 0)
			Assert (message, ret < 0);
		else
			Assert (message, ret > 0);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2)
	{
		int ret = invariant.Compare (s1, idx1, len1, s2, idx2, len2);
		if (result == 0)
			AssertEquals (message, 0, ret);
		else if (result < 0)
			Assert (message, ret < 0);
		else
			Assert (message, ret > 0);
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

	void AssertIndexOf (string message, int expected,
		string source, char target)
	{
		AssertEquals (message, expected,
			invariant.IndexOf (source, target));
	}

	void AssertIndexOf (string message, int expected, string source,
		char target, CompareOptions opt)
	{
		AssertEquals (message, expected,
			invariant.IndexOf (source, target, opt));
	}

	void AssertIndexOf (string message, int expected, string source,
		char target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		AssertEquals (message, expected,
			ci.IndexOf (source, target, idx, len, opt));
	}

	void AssertIndexOf (string message, int expected,
		string source, string target)
	{
		AssertEquals (message, expected,
			invariant.IndexOf (source, target));
	}

	void AssertIndexOf (string message, int expected, string source,
		string target, CompareOptions opt)
	{
		AssertEquals (message, expected,
			invariant.IndexOf (source, target, opt));
	}

	void AssertIndexOf (string message, int expected, string source,
		string target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		AssertEquals (message, expected,
			ci.IndexOf (source, target, idx, len, opt));
	}

	void AssertLastIndexOf (string message, int expected,
		string source, char target)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, CompareOptions opt)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target, opt));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, int idx, int len)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target, idx, len));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		char target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		AssertEquals (message, expected,
			ci.LastIndexOf (source, target, idx, len, opt));
	}

	void AssertLastIndexOf (string message, int expected,
		string source, string target)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, CompareOptions opt)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target, opt));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, int idx, int len)
	{
		AssertEquals (message, expected,
			invariant.LastIndexOf (source, target, idx, len));
	}

	void AssertLastIndexOf (string message, int expected, string source,
		string target, int idx, int len, CompareOptions opt, CompareInfo ci)
	{
		AssertEquals (message, expected,
			ci.LastIndexOf (source, target, idx, len, opt));
	}

	void AssertIsPrefix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert (message, expected == invariant.IsPrefix (
			source, target, opt));
	}

	void AssertIsSuffix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert (message, expected == invariant.IsSuffix (
			source, target, opt));
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

		AssertCompare ("#12", 0, "ae", "\u00E6");
		AssertCompare ("#13", 0, "\u00E6", "ae");
		AssertCompare ("#14", 0, "\u00E6s", 0, 1, "ae", 0, 2);
	}

	[Test]
	public void IndexOfChar ()
	{
		if (!doTest)
			return;

		AssertIndexOf ("#1", -1, "ABC", '1');
		AssertIndexOf ("#2", 2, "ABCABC", 'c', CompareOptions.IgnoreCase);
		AssertIndexOf ("#3", 1, "ABCABC", '\uFF22', ignoreCW);
		AssertIndexOf ("#4", 4, "ABCDE", '\u0117', ignoreCN);
		AssertIndexOf ("#5", 1, "ABCABC", 'B', 1, 5, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#6", 4, "ABCABC", 'B', 2, 4, CompareOptions.IgnoreCase, invariant);
	}

	[Test]
	[Category ("NotDotNet")]
	public void IndexOfCharMSBug ()
	{
		if (!doTest)
			return;

		AssertIndexOf ("#1", 0, "\u00E6", 'a');
	}

	[Test]
	public void LastIndexOfChar ()
	{
		if (!doTest)
			return;

		AssertLastIndexOf ("#1", -1, "ABC", '1');
		AssertLastIndexOf ("#2", 5, "ABCABC", 'c', CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#3", 4, "ABCABC", '\uFF22', ignoreCW);
		AssertLastIndexOf ("#4", 4, "ABCDE", '\u0117', ignoreCN);
		AssertLastIndexOf ("#5", 1, "ABCABC", 'B', 3, 3);
		AssertLastIndexOf ("#6", 4, "ABCABC", 'B', 4, 4);
		AssertLastIndexOf ("#7", -1, "ABCABC", 'B', 5, 1);
	}

	[Test]
	[Category ("NotDotNet")]
	public void LastIndexOfCharMSBug ()
	{
		if (!doTest)
			return;

		AssertIndexOf ("#1", 0, "\u00E6", 'a');
	}

	[Test]
	public void IsPrefix ()
	{
		if (!doTest)
			return;

		AssertIsPrefix ("#1", false, "ABC", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#2", false, "BC", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#3", true, "C", "c", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#4", true, "EDCBA", "\u0117", ignoreCN);
		AssertIsPrefix ("#5", true, "ABC", "AB", CompareOptions.IgnoreCase);
		AssertIsPrefix ("#6", true, "ae", "\u00E6", CompareOptions.None);
		AssertIsPrefix ("#7", true, "\u00E6", "ae", CompareOptions.None);

		AssertIsPrefix ("#8", true, "\u00E6", "a", CompareOptions.None);
		AssertIsPrefix ("#9", true, "\u00E6s", "ae", CompareOptions.None);
		AssertIsPrefix ("#10", false, "\u00E6", "aes", CompareOptions.None);
		AssertIsPrefix ("#11", true, "--start", "--", CompareOptions.None);
		AssertIsPrefix ("#12", true, "-d:NET_1_1", "-", CompareOptions.None);
		AssertIsPrefix ("#13", false, "-d:NET_1_1", "@", CompareOptions.None);
	}

	[Test]
	public void IsSuffix ()
	{
		if (!doTest)
			return;

		AssertIsSuffix ("#1", true, "ABC", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#2", true, "BC", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#3", false, "CBA", "c", CompareOptions.IgnoreCase);
		AssertIsSuffix ("#4", true, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertIsSuffix ("#5", false, "\u00E6", "a", CompareOptions.None);
		AssertIsSuffix ("#6", true, "\u00E6", "ae", CompareOptions.None);
		AssertIsSuffix ("#7", true, "ae", "\u00E6", CompareOptions.None);
		AssertIsSuffix ("#8", false, "e", "\u00E6", CompareOptions.None);

	}

	[Test]
	[Category ("NotDotNet")]
	public void IsSuffixMSBug ()
	{
		if (!doTest)
			return;

		AssertIsSuffix ("#1", true, "\u00E6", "e", CompareOptions.None);
	}

	[Test]
	public void IndexOfString ()
	{
		if (!doTest)
			return;

		AssertIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertIndexOf ("#2", 2, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertIndexOf ("#3", 1, "ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
		AssertIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertIndexOf ("#5", 1, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#6", 1, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#7", -1, "ABCDEF", "BCD", 0, 3, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#8", 0, "-ABC", "-", CompareOptions.None);
		AssertIndexOf ("#9", 0, "--ABC", "--", CompareOptions.None);
		AssertIndexOf ("#9", -1, "--ABC", "--", 1, 2, CompareOptions.None, invariant);
	}


	[Test]
	public void LastIndexOfString ()
	{
		if (!doTest)
			return;

		AssertLastIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertLastIndexOf ("#2", 5, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#3", 4, "ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
		AssertLastIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#5", 4, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#6", 4, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#7", 1, "original", "rig", CompareOptions.None);
		AssertLastIndexOf ("#8", 0, "\u00E6", "ae", CompareOptions.None);
		AssertLastIndexOf ("#9", 0, "-ABC", "-", CompareOptions.None);
		AssertLastIndexOf ("#10", 0, "--ABC", "--", CompareOptions.None);
		AssertLastIndexOf ("#11", -1, "--ABC", "--", 2, 2, CompareOptions.None, invariant);
	}
}

}
