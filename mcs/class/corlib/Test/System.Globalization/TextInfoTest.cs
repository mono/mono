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
		AssertEquals ("\ttab\tand\tPeace", ti.ToTitleCase ("\tTab\tAnd\tPeace"))
	}
}

}
