// StringTest.cs - NUnit Test Cases for the System.String class
//
// Jeffrey Stedfast <fejj@ximian.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Globalization;

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

		Assert (Compare (lesser, greater) < 0);
		Assert (Compare (greater, lesser) > 0);
		Assert (Compare (lesser, lesser) == 0);
		Assert (Compare (lesser, caps, true) == 0);
		Assert (Compare (lesser, medium) < 0);
	}

	public void TestCompareOrdinal ()
	{

	}

	public void TestCompareTo ()
	{
		string lower = "abc";
		string greater = "xyz";

		Assert (lower.CompareTo (greater) < 0);
		Assert (lower.CompareTo (lower) == 0);
		Assert (greater.CompareTo (lesser) > 0);
	}

	public void TestConcat ()
	{
		string string1 = "string1";
		string string2 = "string2";
		string concat = "string1string2";

		Assert (Concat (string1, string2) == concat);
	}

}