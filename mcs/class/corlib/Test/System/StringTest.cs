// StringTest.cs - NUnit Test Cases for the System.String class
//
// Jeffrey Stedfast <fejj@ximian.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class StringTest : TestCase
{
	public StringTest (string name) : base (name) {}

	protected override void SetUp ()
	{
	}

	public static ITest Suite {
		get {
			return new TestSuite (typeof (StringTest));
		}
	}

	public void Testlength ()
	{
		string str = "test string";

		Assert (str.Length == 11);
	}

	public void TestCompare ()
	{
		string lesser = "abc";
		string medium = "abcd";
		string greater = "xyz";
		string caps = "ABC";

		Assert (String.Compare (lesser, greater) < 0);
		Assert (String.Compare (greater, lesser) > 0);
		Assert (String.Compare (lesser, lesser) == 0);
		Assert (String.Compare (lesser, caps, true) == 0);
		Assert (String.Compare (lesser, medium) < 0);
	}

	public void TestCompareOrdinal ()
	{

	}

	public void TestCompareTo ()
	{
		string lower = "abc";
		string greater = "xyz";
		string lesser = "abc";
		
		Assert (lower.CompareTo (greater) < 0);
		Assert (lower.CompareTo (lower) == 0);
		Assert (greater.CompareTo (lesser) > 0);
	}

	public void TestConcat ()
	{
		string string1 = "string1";
		string string2 = "string2";
		string concat = "string1string2";

		Assert (String.Concat (string1, string2) == concat);
	}

	public void TestFormat ()
	{
		Assert ("Empty format string.", String.Format ("", 0) == "");
		Assert ("Single argument.", String.Format ("{0}", 100) == "100");
		Assert ("Single argument, right justified.", String.Format ("X{0,5}X", 37) == "X   37X");
		Assert ("Single argument, left justified.", String.Format ("X{0,-5}X", 37) == "X37   X");
		Assert ("Two arguments.", String.Format ("The {0} wise {1}.", 3, "men") == "The 3 wise men.");
		Assert ("Three arguments.", String.Format ("{0} re {1} fa {2}.", "do", "me", "so") == "do re me fa so.");
		Assert ("Formatted argument.", String.Format ("###{0:x8}#", 0xc0ffee) == "###00c0ffee#");
		Assert ("Formatted argument, right justified.", String.Format ("#{0,5:x3}#", 0x33) == "#  033#");
		Assert ("Formatted argument, left justified.", String.Format ("#{0,-5:x3}#", 0x33) == "#033  #");
		Assert ("Escaped bracket", String.Format ("struct _{0} {{ ... }", "MonoObject") == "struct _MonoObject { ... }");

		// TODO test failure modes
	}

}

}
