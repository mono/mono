// CompareInfoTest.cs - NUnit Test Cases for the
// System.Globalization.CompareInfo class
//
// Dick Porter <dick@ximian.com>
//
// (C) 2003 Novell, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System.Globalization
{

[TestFixture]
public class CompareInfoTest : Assertion
{
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
}

}
