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
		
		[Test]
		public void Unescape () {
			string inString = @"\a\b\t\r\v\f\n\e\02400\x231\cC\ufffff\*";
			char [] c = { (char)7, (char)8, (char)9, (char)13, 
				      (char)11, (char)12, (char)10, (char)27, 
				      (char)160, (char)48, (char)35, (char)49, 
				      (char)3, (char)65535, (char)102, (char)42
			};
			string expectedString = new String(c);
			string outString = Regex.Unescape(inString);

			Assertion.AssertEquals("unescape", outString, expectedString);
		}
	}
}
