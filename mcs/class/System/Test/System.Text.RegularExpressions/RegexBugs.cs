//
// MonoTests.System.Text.RegularExpressions misc. test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2003,2004 Novell, Inc. (http://www.novell.com)
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

        	[Test()]
        	public void MultipleMatches()
        	{
            		Regex regex = new Regex (@"^(?'path'.*(\\|/)|(/|\\))(?'file'.*)$");
            		Match match = regex.Match (@"d:\Temp\SomeDir\SomeDir\bla.xml");
                                                                                           
            		AssertEquals ("MM #01", 5, match.Groups.Count);
                                                                                           
            		AssertEquals ("MM #02", "1", regex.GroupNameFromNumber(1));
            		AssertEquals ("MM #03", "2", regex.GroupNameFromNumber(2));
            		AssertEquals ("MM #04", "path", regex.GroupNameFromNumber(3));
            		AssertEquals ("MM #05", "file", regex.GroupNameFromNumber(4));
                                                                                           
            		AssertEquals ("MM #06", "\\", match.Groups[1].Value);
            		AssertEquals ("MM #07", "", match.Groups[2].Value);
            		AssertEquals ("MM #08", @"d:\Temp\SomeDir\SomeDir\", match.Groups[3].Value);
            		AssertEquals ("MM #09", "bla.xml", match.Groups[4].Value);
        	}

		[Test] 
		public void SameNameGroups () // First problem in fixing bug #56000
		{
			string rex = "link\\s*rel\\s*=\\s*[\"']?alternate[\"']?\\s*";
			rex += "type\\s*=\\s*[\"']?text/xml[\"']?\\s*href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|'(?<1>[^']*)'|(?<1>\\S+))";
			Regex rob = new Regex (rex, RegexOptions.IgnoreCase);
		}
		
		[Test]
		public void UndefinedGroup () // bug 52890
		{
			Regex regex = new Regex( "[A-Za-z_0-9]" );
			Match m = regex.Match( "123456789abc" );
			Group g = m.Groups["not_defined"];
			AssertNotNull ("#0", g);
			AssertEquals ("#1", 0, g.Index);
			AssertEquals ("#2", 0, g.Length);
			AssertEquals ("#3", "", g.Value);
			Assert ("#4", !g.Success);
			AssertNotNull ("#5", g.Captures);
			AssertEquals ("#6", 0, g.Captures.Count);
		}

		[Test]
		public void Quantifiers1 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 7));
			AssertEquals ("#01", false, m.Success);
		}

		[Test]
		public void Quantifiers2 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 8));
			AssertEquals ("#01", true, m.Success);
		}

		[Test]
		public void Quantifiers3 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 16));
			AssertEquals ("#01", true, m.Success);
		}

		[Test]
		public void Quantifiers4 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 32));
			AssertEquals ("#01", true, m.Success);
		}

		[Test]
		public void Quantifiers5 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 33));
			AssertEquals ("#01", true, m.Success);
		}

		[Test]
		public void CategoryAndNegated () // Was a regression after first attemp to fix 59150.
		{
			string text = "<?xml version=\"1.0\"?>";
			Regex re = new Regex ("<\\s*(\\/?)\\s*([\\s\\S]*?)\\s*(\\/?)\\s*>");
			text = re.Replace (text, "{blue:&lt;$1}{maroon:$2}{blue:$3&gt;}");
			AssertEquals ("#01", "{blue:&lt;}{maroon:?xml version=\"1.0\"?}{blue:&gt;}", text);
		}
	
		[Test]
		public void BackSpace ()
		{
			string text = "Go, \bNo\bGo" ;
			Regex re = new Regex(@"\b[\b]");
			text = re.Replace(text, " ");
			AssertEquals("#01", "Go, \bNo Go", text);
		}

		[Test]
		public void ReplaceNegOneAndStartat ()
		{
			string text = "abcdeeee";
			Regex re = new Regex("e+");
			text = re.Replace(text, "e", -1, 4);
			AssertEquals("#01", "abcde", text);
		}

		[Test]
		//[Ignore] You may want to ignore this if the bugs gets back
		public void SplitInfiniteLoop () // bug 57274
		{
			string ss = "a b c d e";
			string [] words = Regex.Split (ss, "[ \t\n\r]*");
			AssertEquals ("#01Length", 11, words.Length);
			AssertEquals ("#00", "", words [0]);
			AssertEquals ("#01", "a", words [1]);
			AssertEquals ("#02", "", words [2]);
			AssertEquals ("#03", "b", words [3]);
			AssertEquals ("#04", "", words [4]);
			AssertEquals ("#05", "c", words [5]);
			AssertEquals ("#06", "", words [6]);
			AssertEquals ("#07", "d", words [7]);
			AssertEquals ("#08", "", words [8]);
			AssertEquals ("#09", "e", words [9]);
			AssertEquals ("#10", "", words [10]);

		}
	}
}

