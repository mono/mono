// TextInfoTest.cs - NUnit Test Cases for the
// System.Globalization.TextInfo class
//
// Miguel de Icaza <miguel@ximian.com>
//
// (C) 2004 Novell, Inc.  http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System.Globalization
{

[TestFixture]
public class TextInfoTest : Assertion
{
	public TextInfoTest () {}

	[Test]
	public void TitleCase ()
	{
		TextInfo ti = new CultureInfo ("en-US", false).TextInfo;

		AssertEquals (" The Dog", ti.ToTitleCase (" the dog"));
		AssertEquals (" The Dude", ti.ToTitleCase (" The Dude"));
		AssertEquals ("La Guerra Yla Paz", ti.ToTitleCase ("la Guerra yLa pAz"));
		AssertEquals ("\tTab\tAnd\tPeace", ti.ToTitleCase ("\ttab\taNd\tpeaCE"));
	}

	[Test]
	public void TitleCase2 ()
	{
		foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
			Check (ci, "AB", "AB");
			Check (ci, "ab", "Ab");
			Check (ci, "aB", "Ab");
			Check (ci, "1Ab", "1Ab");
			Check (ci, "abc AB ab aB Ab ABc 1AB 1Ab 1ab 1aB",
				"Abc AB Ab Ab Ab Abc 1AB 1Ab 1Ab 1Ab");
			Check (ci, "LJ", "LJ");
			Check (ci, "lj", "Lj");
			Check (ci, "lJ", "Lj");
			Check (ci, "lj abc ljabc", "Lj Abc Ljabc");
			Check (ci, "ab", "Ab");

			// Some greek titlecase characters
			Check (ci, "\u01c4", "\u01c5");
			Check (ci, "\u01c5", "\u01c5");
			Check (ci, "\u01c6", "\u01c5");
			Check (ci, "\u01ca", "\u01cb");
			Check (ci, "\u01cb", "\u01cb");
			Check (ci, "\u01cc", "\u01cb");
			// Roman numbers are not converted unlike ToUpper().
			Check (ci, "\u2170", "\u2170");
			Check (ci, "\u24e9", "\u24e9");
		}
	}

	private void Check (CultureInfo ci, string src, string expected)
	{
		AssertEquals (src + " at culture " + ci.LCID,
			expected,
			ci.TextInfo.ToTitleCase (src));
	}
}

}
