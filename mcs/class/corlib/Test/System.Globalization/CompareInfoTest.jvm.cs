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
		
	}

	// Culture-sensitive collation tests

	CompareInfo invariant = CultureInfo.InvariantCulture.CompareInfo;
	CompareInfo french = new CultureInfo ("fr").CompareInfo;
	CompareInfo japanese = new CultureInfo ("ja").CompareInfo;
	CompareInfo czech = new CultureInfo ("cs").CompareInfo;
	CompareInfo hungarian = new CultureInfo ("hu").CompareInfo;

	CompareOptions ignoreCN =
		CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;

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
			Assert.IsTrue (message + String.Format ("(neg: {0})", ret), ret < 0);
		else
			Assert.IsTrue (message + String.Format ("(pos: {0})", ret), ret > 0);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2)
	{
		int ret = invariant.Compare (s1, idx1, len1, s2, idx2, len2);
		if (result == 0)
			AssertEquals (message, 0, ret);
		else if (result < 0)
			Assert.IsTrue (message, ret < 0);
		else
			Assert.IsTrue (message, ret > 0);
	}

	void AssertCompare (string message, int result,
		string s1, int idx1, int len1, string s2, int idx2, int len2,
		CompareOptions opt, CompareInfo ci)
	{
		int ret = ci.Compare (s1, idx1, len1, s2, idx2, len2, opt);
		if (result == 0)
			AssertEquals (message, 0, ret);
		else if (result < 0)
			Assert.IsTrue (message, ret < 0);
		else
			Assert.IsTrue (message, ret > 0);
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
		string target)
	{
		Assert.IsTrue (message, expected == invariant.IsPrefix (
			source, target));
	}

	void AssertIsPrefix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert.IsTrue (message, expected == invariant.IsPrefix (
			source, target, opt));
	}

	void AssertIsSuffix (string message, bool expected, string source,
		string target)
	{
		Assert.IsTrue (message, expected == invariant.IsSuffix (
			source, target));
	}

	void AssertIsSuffix (string message, bool expected, string source,
		string target, CompareOptions opt)
	{
		Assert.IsTrue (message, expected == invariant.IsSuffix (
			source, target, opt));
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void CultureSensitiveCompare ()
	{
		if (!doTest)
			return;

		AssertCompare ("#1", -1, "1", "2");
		AssertCompare ("#2", 1, "A", "a");
		AssertCompare ("#3", 0, "A", "a", CompareOptions.IgnoreCase);
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
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void CompareSpecialWeight ()
	{
		if (!doTest)
			return;

		// Japanese (in invariant)
// BUG in .NET 2.0 : half-width kana should be bigger.
		AssertCompare ("#1", 1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertCompare ("#4", 1, "\u3042\u309D", "\u3042\u3042");
		AssertCompare ("#5", 0, "\u3042\u309D", "\u3042\u3042", CompareOptions.IgnoreNonSpace);

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
			return;

		AssertIndexOf ("#1", -1, "ABC", '1');
		AssertIndexOf ("#2", 2, "ABCABC", 'c', CompareOptions.IgnoreCase);
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
		AssertLastIndexOf ("#4", 4, "ABCDE", '\u0117', ignoreCN);
		AssertLastIndexOf ("#5", 1, "ABCABC", 'B', 3, 3);
		AssertLastIndexOf ("#6", 4, "ABCABC", 'B', 4, 4);
		AssertLastIndexOf ("#7", -1, "ABCABC", 'B', 5, 1);
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
			return;

		AssertIndexOf ("#1", 0, "\u00E6", 'a');
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void IsPrefix ()
	{
		if (!doTest)
			return;

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
			return;

		// Japanese (in invariant)
		AssertIsPrefix ("#1", false, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIsPrefix ("#2-2", false, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIsPrefix ("#3-2", false, "\uFF80\uFF9E\uFF72\uFF8C\uFF9E", 
			"\u30C0\u30A4\u30D6");
		AssertIsPrefix ("#4", false, "\u3042\u309D", "\u3042\u3042");

		// extender in target
		AssertIsPrefix ("#7", false, "\u30D1\u30A2", "\u30D1\u30FC");
		// extender in source
		AssertIsPrefix ("#9", false, "\u30D1\u30FC", "\u30D1\u30A2");

		// empty suffix always matches the source.
		AssertIsPrefix ("#11", true, "", "");
		AssertIsPrefix ("#12", true, "/test.css", "");

		// bug #76243
		AssertIsPrefix ("#13", false, "\u00e4_", "a");
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void IsSuffix ()
	{
		if (!doTest)
			return;

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
			return;

		AssertIsSuffix ("#1", true, "\u00E6", "e", CompareOptions.None);
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void IndexOfString ()
	{
		if (!doTest)
			return;

		AssertIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertIndexOf ("#2", 2, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertIndexOf ("#5", 1, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#6", 1, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertIndexOf ("#7", -1, "ABCDEF", "BCD", 0, 3, CompareOptions.IgnoreCase, invariant);
		AssertIndexOf ("#8", 0, "-ABC", "-", CompareOptions.None);
		AssertIndexOf ("#9", 0, "--ABC", "--", CompareOptions.None);
		AssertIndexOf ("#10", -1, "--ABC", "--", 1, 2, CompareOptions.None, invariant);
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertIndexOf ("#11", 0, "AE", "\u00C6", CompareOptions.None);
		// U+3007 is completely ignored character.
		AssertIndexOf ("#12", 0, "\uff21\uff21", "\uff21", CompareOptions.None);
// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIndexOf ("#13", 0, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);
		AssertIndexOf ("#14", 0, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		AssertIndexOf ("#15", 0, "\uff21\uff21", "\u3007", CompareOptions.None);
		AssertIndexOf ("#15-2", 1, "\u3007\uff21", "\uff21", CompareOptions.None);
		// target is "empty" (in culture-sensitive context).
		AssertIndexOf ("#16", -1, String.Empty, "\u3007");
// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertIndexOf ("#17", 0, "A", "\u3007");
		AssertIndexOf ("#18", 0, "ABC", "\u3007");

		AssertIndexOf ("#19", 0, "\\b\\a a", "\\b\\a a");
		Assert.AreEqual (0, new CultureInfo ("en").CompareInfo.IndexOf ("\\b\\a a", "\\b\\a a"), "#19en");
		Assert.AreEqual (0, new CultureInfo ("ja").CompareInfo.IndexOf ("\\b\\a a", "\\b\\a a"), "#19ja");
	}

	[Test]
	public void IndexOfSpecialWeight ()
	{
		if (!doTest)
			return;

		// Japanese (in invariant)
		AssertIndexOf ("#1", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		// extender in target
		AssertIndexOf ("#1-2", -1, "\u30D1\u30A2", "\u30D1\u30FC");
		// extender in source
		AssertIndexOf ("#2-2", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		AssertIndexOf ("#4", -1, "\u3042\u309D", "\u3042\u3042");
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void LastIndexOfString ()
	{
		if (!doTest)
			return;

		AssertLastIndexOf ("#1", -1, "ABC", "1", CompareOptions.None);
		AssertLastIndexOf ("#2", 5, "ABCABC", "c", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#4", 4, "ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#5", 4, "ABCABC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#6", 4, "BBCBBC", "BC", CompareOptions.IgnoreCase);
		AssertLastIndexOf ("#7", 1, "original", "rig", CompareOptions.None);
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertLastIndexOf ("#8", 0, "\u00E6", "ae", CompareOptions.None);
		AssertLastIndexOf ("#9", 0, "-ABC", "-", CompareOptions.None);
		AssertLastIndexOf ("#10", 0, "--ABC", "--", CompareOptions.None);
		AssertLastIndexOf ("#11", -1, "--ABC", "--", 2, 2, CompareOptions.None, invariant);
		AssertLastIndexOf ("#12", -1, "--ABC", "--", 4, 2, CompareOptions.None, invariant);
// BUG in .NET 2.0 : see GetSortKey() test (mentioned above).
		AssertLastIndexOf ("#13", 0, "AE", "\u00C6", CompareOptions.None);
		// U+3007 is completely ignored character.
		AssertLastIndexOf ("#14", 1, "\uff21\uff21", "\uff21", CompareOptions.None);
// BUG in .NET 2.0 : see \u3007 issue (mentioned above).
		AssertLastIndexOf ("#15", 1, "\uff21\uff21", "\u3007\uff21", CompareOptions.None);
		AssertLastIndexOf ("#16", 1, "\uff21\uff21", "\uff21\u3007", CompareOptions.None);
		AssertLastIndexOf ("#17", 1, "\uff21\uff21", "\u3007", CompareOptions.None);
		AssertLastIndexOf ("#18", 1, "\u3007\uff21", "\uff21", CompareOptions.None);
		AssertLastIndexOf ("#19", 0, "\\b\\a a", "\\b\\a a");
		Assert.AreEqual (0, new CultureInfo ("en").CompareInfo.LastIndexOf ("\\b\\a a", "\\b\\a a"), "#19en");
		Assert.AreEqual (0, new CultureInfo ("ja").CompareInfo.LastIndexOf ("\\b\\a a", "\\b\\a a"), "#19ja");
		// bug #80612
		AssertLastIndexOf ("#20", 8, "/system/web", "w");
		Assert.AreEqual (8, new CultureInfo ("sv").CompareInfo.LastIndexOf ("/system/web", "w"), "#20sv");
	}

	[Test]
	public void LastIndexOfSpecialWeight ()
	{
		if (!doTest)
			return;

		// Japanese (in invariant)
		AssertLastIndexOf ("#1", -1, "\u30D1\u30FC\u30B9", "\uFF8A\uFF9F\uFF70\uFF7D");
		// extender in target
		AssertLastIndexOf ("#1-2", -1, "\u30D1\u30A2", "\u30D1\u30FC");
		// extender in source
		AssertLastIndexOf ("#4", -1, "\u3042\u309D", "\u3042\u3042");
	}

	[Test]
	public void LastIndexOfOrdinalString ()
	{
		if (!doTest)
			return;

		AssertLastIndexOf ("#1", -1, "ABC", "1", CompareOptions.Ordinal);
		AssertLastIndexOf ("#2", 5, "ABCABC", "C", CompareOptions.Ordinal);
		AssertLastIndexOf ("#3", -1, "ABCABC", "\uFF22", CompareOptions.Ordinal);
		AssertLastIndexOf ("#4", 4, "ABCABC", "BC", CompareOptions.Ordinal);
		AssertLastIndexOf ("#5", 4, "BBCBBC", "BC", CompareOptions.Ordinal);
		AssertLastIndexOf ("#6", 1, "original", "rig", CompareOptions.Ordinal);
		AssertLastIndexOf ("#7", 0, "\\b\\a a", "\\b\\a a", CompareOptions.Ordinal);
	}

	[Test]
	[Category ("TargetJvmNotWorking")]
	// for bug #76702
	public void NullCharacter ()
	{
		Assert.AreEqual (-1, "MONO".IndexOf ("\0\0\0"), "#1");
		Assert.AreEqual (-1, "MONO".LastIndexOf ("\0\0\0"), "#2");
		Assert.AreEqual (1, "MONO".CompareTo ("\0\0\0"), "#3");
	}

	[Test]
	[Category ("NotDotNet")]
	// MS.NET treats it as equivalent, while in IndexOf() it does not match.
	public void NullCharacterWeird ()
	{
		Assert.AreEqual (-1, "MONO".CompareTo ("MONO\0\0\0"), "#4");
	}

#if NET_2_0
	[Test]
	[Category ("NotDotNet")]
	public void OrdinalIgnoreCaseCompare ()
	{
		if (!doTest)
			return;

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
		Assert ("aaaa".StartsWith ("A", StringComparison.OrdinalIgnoreCase));
	}
#endif
}

}
