//
// MatchTest.cs - Unit tests for System.Text.RegularExpressions.Match
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.Text.RegularExpressions;

namespace MonoTests.System.Text.RegularExpressions
{
	[TestFixture]
	public class SpliTest {

	static void TestSplit (string pattern, string text, params string[] res) {
		var r = new Regex (pattern);
		var a = r.Split (text);
		if (a.Length != res.Length) {
			Assert.AreEqual (res.Length, a.Length, "length");
		}

		for (int i = 0; i < res.Length; ++i) {
			if (!a [i].Equals (res [i])) {
				Assert.AreEqual (res [i], a [i], "idx_" + i);
			}
		}
	}

	[Test]
	public void NoGroups () { TestSplit ("el", "hello", "h", "lo"); }

	[Test]
	public void SingleGroup () { TestSplit ( "(el)", "hello", "h", "el", "lo"); }

 	[Test]
	public void TwoGroups () { TestSplit ("(el)|(xx)", "hello", "h", "el", "lo"); }

	[Test]
	public void TwoGroupsInverted () { TestSplit ("(cc)|(el)", "hello", "h", "el", "lo"); }

	[Test]
	public void TwoValidGroups () { TestSplit ("(el)|(lo)", "hello", "h", "el", "", "lo", ""); }

 	[Test]
	public void ThreGroups () { TestSplit ("(el)|(xx)|(yy)", "hello", "h", "el", "lo"); }

	}
}

