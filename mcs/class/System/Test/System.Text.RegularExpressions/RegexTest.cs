//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	RegexTest.cs
//
// Authors:	
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (c) 2003 Juraj Skripsky

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	
	[TestFixture]
	public class RegexTest {

		[Test]
		public void Simple () {
			char[] c = { (char)32, (char)8212, (char)32 };
			string s = new String(c);			
			Assertion.AssertEquals ("char", true, Regex.IsMatch(s, s));
		}
	}
}
