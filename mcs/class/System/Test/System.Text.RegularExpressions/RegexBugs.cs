//
// MonoTests.System.Text.RegularExpressions misc. test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace MonoTests.System.Text.RegularExpressions
{
	[TestFixture]
	public class RegexBugs : Assertion
	{
		[Test]
		public void SplitGroup () // bug51146
		{
		        string [] splitResult = new Regex ("-").Split ("a-bcd-e-fg");
			string [] expected = new string [] {"a", "bcd", "e", "fg"};
			int length = expected.Length;
			int i;
			AssertEquals ("#01", length, splitResult.Length);
			for (i = 0; i < length; i++)
				AssertEquals ("#02 " + i, expected [i], splitResult [i]);
			
			splitResult = new Regex ("(-)").Split ("a-bcd-e-fg");
			expected = new string [] {"a", "-", "bcd", "-", "e", "-", "fg"};
			length = expected.Length;
			AssertEquals ("#03", length, splitResult.Length);
			for (i = 0; i < length; i++)
				AssertEquals ("#04 " + i, expected [i], splitResult [i]);

			splitResult = new Regex ("(-)b(c)").Split ("a-bcd-e-fg");
			expected = new string [] {"a", "-", "c", "d-e-fg" };
			length = expected.Length;
			AssertEquals ("#04", length, splitResult.Length);
			for (i = 0; i < length; i++)
				AssertEquals ("#05 " + i, expected [i], splitResult [i]);
				
			splitResult = new Regex ("-").Split ("a-bcd-e-fg-");
			expected = new string [] {"a", "bcd", "e", "fg", ""};
			length = expected.Length;
			AssertEquals ("#06", length, splitResult.Length);
			for (i = 0; i < length; i++)
				AssertEquals ("#07 " + i, expected [i], splitResult [i]);
		}

		[Test]
		public void MathEmptyGroup () // bug 42529
		{
			string str = "Match something from here.";

			AssertEquals ("MEG #01", false, Regex.IsMatch(str, @"(something|dog)$"));
			AssertEquals ("MEG #02", true, Regex.IsMatch(str, @"(|something|dog)$"));
			AssertEquals ("MEG #03", true, Regex.IsMatch(str, @"(something||dog)$"));
			AssertEquals ("MEG #04", true, Regex.IsMatch(str, @"(something|dog|)$"));

			AssertEquals ("MEG #05", true, Regex.IsMatch(str, @"(something|dog)*"));
			AssertEquals ("MEG #06", true, Regex.IsMatch(str, @"(|something|dog)*"));
			AssertEquals ("MEG #07", true, Regex.IsMatch(str, @"(something||dog)*"));
			AssertEquals ("MEG #08", true, Regex.IsMatch(str, @"(something|dog|)*"));

			AssertEquals ("MEG #09", true, Regex.IsMatch(str, @"(something|dog)*$"));
			AssertEquals ("MEG #10", true, Regex.IsMatch(str, @"(|something|dog)*$"));
			AssertEquals ("MEG #11", true, Regex.IsMatch(str, @"(something||dog)*$"));
			AssertEquals ("MEG #12", true, Regex.IsMatch(str, @"(something|dog|)*$"));

		}

		[Test]
		public void Braces () // bug 52924
		{
			// Before the fix, the next line throws an exception
			Regex regVar = new Regex(@"{\w+}");
			Match m = regVar.Match ("{   }");
			AssertEquals ("BR #01", false, m.Success);
		}

                [Test]
                public void RangeIgnoreCase() // bug 45976
                {
                        string str = "AAABBBBAAA" ;
                        AssertEquals("RIC #01", true, Regex.IsMatch(str, @"[A-F]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #02", true, Regex.IsMatch(str, @"[a-f]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #03", true, Regex.IsMatch(str, @"[A-Fa-f]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #04", true, Regex.IsMatch(str, @"[AB]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #05", true, Regex.IsMatch(str, @"[A-B]+", RegexOptions.IgnoreCase));

                        str = "AaaBBBaAa" ;
                        AssertEquals("RIC #06", true, Regex.IsMatch(str, @"[A-F]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #07", true, Regex.IsMatch(str, @"[a-f]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #08", true, Regex.IsMatch(str, @"[A-Fa-f]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #09", true, Regex.IsMatch(str, @"[AB]+", RegexOptions.IgnoreCase));
                        AssertEquals("RIC #10", true, Regex.IsMatch(str, @"[A-B]+", RegexOptions.IgnoreCase));

			str = "Aaa[";
			AssertEquals("RIC #11", true, Regex.IsMatch(str, @"[A-a]+", RegexOptions.IgnoreCase));
			
			str = "Ae";
			Assert("RIC #12", Regex.IsMatch(str, @"[A-a]+", RegexOptions.IgnoreCase));

                }

		[Test]
		public void Escape0 () // bug54797
		{
            		Regex r = new Regex(@"^[\s\0]*$");
			AssertEquals ("E0-1", true, r.Match(" \0").Success);
		}
	}
}

