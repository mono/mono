//
// MonoTests.System.Text.RegularExpressions misc. test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2003,2004 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions
{
	[TestFixture]
	public class RegexBugs
	{
		[Test] // bug #51146
		public void SplitGroup ()
		{
			string [] splitResult = new Regex ("-").Split ("a-bcd-e-fg");
			string [] expected = new string [] {"a", "bcd", "e", "fg"};
			int length = expected.Length;
			Assert.AreEqual (length, splitResult.Length, "#1");
			for (int i = 0; i < length; i++)
				Assert.AreEqual (expected [i], splitResult [i], "#2:" + i);
			
			splitResult = new Regex ("(-)").Split ("a-bcd-e-fg");
			expected = new string [] {"a", "-", "bcd", "-", "e", "-", "fg"};
			length = expected.Length;
			Assert.AreEqual (length, splitResult.Length, "#3");
			for (int i = 0; i < length; i++)
				Assert.AreEqual (expected [i], splitResult [i], "#4:" + i);

			splitResult = new Regex ("(-)b(c)").Split ("a-bcd-e-fg");
			expected = new string [] {"a", "-", "c", "d-e-fg" };
			length = expected.Length;
			Assert.AreEqual (length, splitResult.Length, "#5");
			for (int i = 0; i < length; i++)
				Assert.AreEqual (expected [i], splitResult [i], "#6:" + i);
				
			splitResult = new Regex ("-").Split ("a-bcd-e-fg-");
			expected = new string [] {"a", "bcd", "e", "fg", ""};
			length = expected.Length;
			Assert.AreEqual (length, splitResult.Length, "#7");
			for (int i = 0; i < length; i++)
				Assert.AreEqual (expected [i], splitResult [i], "#8:" + i);
		}

		[Test] // bug #42529
		public void MathEmptyGroup ()
		{
			string str = "Match something from here.";

			Assert.IsFalse (Regex.IsMatch(str, @"(something|dog)$"), "#1");
			Assert.IsTrue (Regex.IsMatch (str, @"(|something|dog)$"), "#2");
			Assert.IsTrue (Regex.IsMatch (str, @"(something||dog)$"), "#3");
			Assert.IsTrue (Regex.IsMatch (str, @"(something|dog|)$"), "#4");

			Assert.IsTrue (Regex.IsMatch (str, @"(something|dog)*"), "#5");
			Assert.IsTrue (Regex.IsMatch (str, @"(|something|dog)*"), "#6");
			Assert.IsTrue (Regex.IsMatch (str, @"(something||dog)*"), "#7");
			Assert.IsTrue (Regex.IsMatch (str, @"(something|dog|)*"), "#8");

			Assert.IsTrue (Regex.IsMatch (str, @"(something|dog)*$"), "#9");
			Assert.IsTrue (Regex.IsMatch (str, @"(|something|dog)*$"), "#10");
			Assert.IsTrue (Regex.IsMatch (str, @"(something||dog)*$"), "#11");
			Assert.IsTrue (Regex.IsMatch (str, @"(something|dog|)*$"), "#12");
		}

		[Test] // bug #52924
		public void Braces ()
		{
			Regex regVar = new Regex(@"{\w+}");
			Match m = regVar.Match ("{   }");
			Assert.IsFalse  (m.Success);
		}

		[Test] // bug #71077
		public void WhiteSpaceGroupped ()
		{
			string s = "\n";
			string p = @"[\s\S]";	// =Category.Any
			Assert.IsTrue (Regex.IsMatch (s, p));
		}

		[Test] // bug #45976
		public void RangeIgnoreCase()
		{
			string str = "AAABBBBAAA" ;
			Assert.IsTrue (Regex.IsMatch(str, @"[A-F]+", RegexOptions.IgnoreCase), "#A1");
			Assert.IsTrue (Regex.IsMatch (str, @"[a-f]+", RegexOptions.IgnoreCase), "#A2");
			Assert.IsTrue (Regex.IsMatch (str, @"[A-Fa-f]+", RegexOptions.IgnoreCase), "#A3");
			Assert.IsTrue (Regex.IsMatch (str, @"[AB]+", RegexOptions.IgnoreCase), "#A4");
			Assert.IsTrue (Regex.IsMatch (str, @"[A-B]+", RegexOptions.IgnoreCase), "#A5");

			str = "AaaBBBaAa" ;
			Assert.IsTrue (Regex.IsMatch (str, @"[A-F]+", RegexOptions.IgnoreCase), "#B1");
			Assert.IsTrue (Regex.IsMatch (str, @"[a-f]+", RegexOptions.IgnoreCase), "#B2");
			Assert.IsTrue (Regex.IsMatch (str, @"[A-Fa-f]+", RegexOptions.IgnoreCase), "#B3");
			Assert.IsTrue (Regex.IsMatch (str, @"[AB]+", RegexOptions.IgnoreCase), "#B4");
			Assert.IsTrue (Regex.IsMatch (str, @"[A-B]+", RegexOptions.IgnoreCase), "#B5");

			str = "Aaa[";
			Assert.IsTrue (Regex.IsMatch (str, @"[A-a]+", RegexOptions.IgnoreCase), "#C");

			str = "Ae";
			Assert.IsTrue (Regex.IsMatch (str, @"[A-a]+", RegexOptions.IgnoreCase), "#D");
		}

		[Test] // bug #54797
		public void Escape0 ()
		{
			Regex r = new Regex(@"^[\s\0]*$");
			Assert.IsTrue (r.Match(" \0").Success);
		}

		[Test] // bug #432172
		public void NoBitmap ()
		{
			Regex rx =
				new Regex ("([^a-zA-Z_0-9])+", RegexOptions.Compiled);
			Assert.AreEqual ("--", rx.Match ("A--B-").Value);
		}
		
		[Test]
		public void MultipleMatches()
		{
			Regex regex = new Regex (@"^(?'path'.*(\\|/)|(/|\\))(?'file'.*)$");
			Match match = regex.Match (@"d:\Temp\SomeDir\SomeDir\bla.xml");

			Assert.AreEqual (5, match.Groups.Count, "#1");
			Assert.AreEqual ("1", regex.GroupNameFromNumber (1), "#2");
			Assert.AreEqual ("2", regex.GroupNameFromNumber (2), "#3");
			Assert.AreEqual ("path", regex.GroupNameFromNumber (3), "#4");
			Assert.AreEqual ("file", regex.GroupNameFromNumber (4), "#5");
			Assert.AreEqual ("\\", match.Groups [1].Value, "#6");
			Assert.AreEqual (string.Empty, match.Groups [2].Value, "#7");
			Assert.AreEqual (@"d:\Temp\SomeDir\SomeDir\", match.Groups [3].Value, "#8");
			Assert.AreEqual ("bla.xml", match.Groups [4].Value, "#9");
		}

		[Test] // bug #56000
		public void SameNameGroups ()
		{
			string rex = "link\\s*rel\\s*=\\s*[\"']?alternate[\"']?\\s*";
			rex += "type\\s*=\\s*[\"']?text/xml[\"']?\\s*href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|'(?<1>[^']*)'|(?<1>\\S+))";
			new Regex (rex, RegexOptions.IgnoreCase);
		}

		[Test] // bug #52890
		public void UndefinedGroup ()
		{
			Regex regex = new Regex( "[A-Za-z_0-9]" );
			Match m = regex.Match( "123456789abc" );
			Group g = m.Groups["not_defined"];
			Assert.IsNotNull (g, "#1");
			Assert.AreEqual (0, g.Index, "#2");
			Assert.AreEqual (0, g.Length, "#3");
			Assert.AreEqual (string.Empty, g.Value, "#4");
			Assert.IsFalse (g.Success, "#5");
			Assert.IsNotNull (g.Captures, "#6");
			Assert.AreEqual (0, g.Captures.Count, "#7");
		}

		[Test]
		public void Quantifiers1 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 7));
			Assert.IsFalse (m.Success);
		}

		[Test]
		public void Quantifiers2 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 8));
			Assert.IsTrue (m.Success);
		}

		[Test]
		public void Quantifiers3 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 16));
			Assert.IsTrue (m.Success);
		}

		[Test]
		public void Quantifiers4 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 32));
			Assert.IsTrue (m.Success);
		}

		[Test]
		public void Quantifiers5 ()
		{
			Regex re = new Regex ("[\\w\\W]{8,32}");
			Match m = re.Match (new string ('1', 33));
			Assert.IsTrue (m.Success);
		}

		[Test]
		public void CategoryAndNegated () // Was a regression after first attemp to fix 59150.
		{
			string text = "<?xml version=\"1.0\"?>";
			Regex re = new Regex ("<\\s*(\\/?)\\s*([\\s\\S]*?)\\s*(\\/?)\\s*>");
			text = re.Replace (text, "{blue:&lt;$1}{maroon:$2}{blue:$3&gt;}");
			Assert.AreEqual ("{blue:&lt;}{maroon:?xml version=\"1.0\"?}{blue:&gt;}", text);
		}
	
		[Test]
		public void BackSpace ()
		{
			string text = "Go, \bNo\bGo" ;
			Regex re = new Regex(@"\b[\b]");
			text = re.Replace(text, " ");
			Assert.AreEqual ("Go, \bNo Go", text);
		}

		[Test]
		public void ReplaceNegOneAndStartat ()
		{
			string text = "abcdeeee";
			Regex re = new Regex("e+");
			text = re.Replace(text, "e", -1, 4);
			Assert.AreEqual ("abcde", text);
		}

		[Test] // bug #57274
		public void SplitInfiniteLoop ()
		{
			string ss = "a b c d e";
			string [] words = Regex.Split (ss, "[ \t\n\r]*");
			Assert.AreEqual (11, words.Length, "#1");
			Assert.AreEqual (string.Empty, words [0], "#2");
			Assert.AreEqual ("a", words [1], "#3");
			Assert.AreEqual (string.Empty, words [2], "#4");
			Assert.AreEqual ("b", words [3], "#5");
			Assert.AreEqual (string.Empty, words [4], "#6");
			Assert.AreEqual ("c", words [5], "#7");
			Assert.AreEqual (string.Empty, words [6], "#8");
			Assert.AreEqual ("d", words [7], "#9");
			Assert.AreEqual (string.Empty, words [8], "#10");
			Assert.AreEqual ("e", words [9], "#11");
			Assert.AreEqual (string.Empty, words [10], "#12");
		}

		[Test] // bug #69065
		public void CaseAndSearch ()
		{
			string test1 =  @"!E   ZWEITBAD :REGLER-PARAMETER 20.10.2004  SEITE   1";
			string test2 =  @" REGLER-PARAMETER ";
			string test3 =  @"REGLER-PARAMETER ";
			Regex x = new Regex ("REGLER-PARAMETER",RegexOptions.IgnoreCase|RegexOptions.Compiled);

			Match m = x.Match (test1);
			Assert.IsTrue (m.Success, "#1");

			m = x.Match (test2);
			Assert.IsTrue (m.Success, "#2");

			m = x.Match (test3);
			Assert.IsTrue (m.Success, "#3");
		}

		[Test] // bug #69193
		public void QuantifiersParseError ()
		{
			new Regex ("{1,a}");
			new Regex ("{a,1}");
			new Regex ("{a}");
			new Regex ("{,a}");
		}

		[Test] // bug #74753
		public void NameLookupInEmptyMatch ()
		{
			Regex regTime = new Regex (
					@"(?<hour>[0-9]{1,2})([\:](?<minute>[0-9]{1,2})){0,1}([\:](?<second>[0-9]{1,2})){0,1}\s*(?<ampm>(?i:(am|pm)){0,1})");

			Match mTime = regTime.Match("");
			Assert.AreEqual ("", mTime.Groups["hour"].Value, "#A1");
			Assert.AreEqual ("", mTime.Groups ["minute"].Value, "#A2");
			Assert.AreEqual ("", mTime.Groups ["second"].Value, "#A3");
			Assert.AreEqual ("", mTime.Groups ["ampm"].Value, "#A4");

			mTime = regTime.Match("12:00 pm");
			Assert.AreEqual ("12", mTime.Groups ["hour"].Value, "#B1");
			Assert.AreEqual ("00", mTime.Groups ["minute"].Value, "#B2");
			Assert.AreEqual ("", mTime.Groups ["second"].Value, "#B3");
			Assert.AreEqual ("pm", mTime.Groups ["ampm"].Value, "#B4");
		}

		[Test] // bug #77626
		public void HangingHyphens ()
		{
			Assert.IsTrue (Regex.IsMatch ("mT1[", @"m[0-9A-Za-z_-]+\["), "#A1");
			Assert.IsTrue (Regex.IsMatch ("mT1[", @"m[-0-9A-Za-z_]+\["), "#A2");

			Assert.IsTrue (Regex.IsMatch ("-a;", @"[--a]{3}"), "#B1");
			Assert.IsTrue (Regex.IsMatch ("-&,", @"[&--]{3}"), "#B2");

			Assert.IsTrue (Regex.IsMatch ("abcz-", @"[a-c-z]{5}"), "#C1");
			Assert.IsFalse (Regex.IsMatch ("defghijklmnopqrstuvwxy", @"[a-c-z]"), "#C2");

			Assert.IsTrue (Regex.IsMatch ("abcxyz-", @"[a-c-x-z]{7}"), "#D1");
			Assert.IsFalse (Regex.IsMatch ("defghijklmnopqrstuvw", @"[a-c-x-z]"), "#D2");

			Assert.IsTrue (Regex.IsMatch (" \tz-", @"[\s-z]{4}"), "#E1");
			Assert.IsFalse (Regex.IsMatch ("abcdefghijklmnopqrstuvwxy", @"[\s-z]"), "#E2");
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void HangingHyphen1 ()
		{
			Regex.IsMatch ("foobar", @"[a-\s]");
		}

		[Test]
		public void Bug313642 ()
		{
			Regex r = new Regex ("(?<a>c)");
			Match m = r.Match ("a");
			Assert.AreEqual (1, m.Groups.Count, "#1");
			Assert.AreEqual (0, m.Groups [0].Captures.Count, "#2");
			Assert.AreEqual (0, m.Groups [0].Index, "#3");
			Assert.AreEqual (0, m.Groups [0].Length, "#4");
			Assert.IsFalse (m.Groups [0].Success, "#5");
			Assert.AreEqual (string.Empty, m.Groups [0].Value, "#6");
		}

		[Test]
		public void Bug77487 ()
		{
			Assert.IsTrue (Regex.IsMatch ("a a", "^(a[^a]*)*a$"), "#1");
			Assert.IsTrue (Regex.IsMatch ("a a", "^(a *)*a$"), "#2");
			Assert.IsTrue (Regex.IsMatch ("a a", "(a[^a]*)+a"), "#3");
			Assert.IsTrue (Regex.IsMatch ("a a", "(a *)+a"), "#4");
		}

		[Test]
		public void Bug69269 ()
		{
			string s = "CREATE aa\faa; CREATE bb\nbb; CREATE cc\rcc; CREATE dd\tdd; CREATE ee\vee;";
			Assert.AreEqual (5, Regex.Matches(s, @"CREATE[\s\S]+?;").Count, "#1");
			Assert.AreEqual (5, Regex.Matches (s, @"CREATE[ \f\n\r\t\v\S]+?;").Count, "#2");
		}

		[Test]
		public void Bug76345 ()
		{
			Match m;
			string s1 = "'asdf'";
			string s2 = "'as,'df'";

			m = new Regex("'.*?'").Match(s1);
			Assert.IsTrue (m.Success, "#A1");
			Assert.AreEqual (s1, m.Value, "#A2");

			m = new Regex("'[^,].*?'").Match(s1);
			Assert.IsTrue (m.Success, "#B1");
			Assert.AreEqual (s1, m.Value, "#B2");

			m = new Regex("'.*?[^,]'").Match(s1);
			Assert.IsTrue (m.Success, "#C1");
			Assert.AreEqual (s1, m.Value, "#C2");

			m = new Regex("'.*?[^,]'").Match(s2);
			Assert.IsTrue (m.Success, "#D1");
			Assert.AreEqual (s2, m.Value, "#D2");
		}

		[Test]
		public void Bug78007 ()
		{
			string test = "head&gt;<html>";
			string pattern = @"\Ahead&gt;\<html\>";
			Regex r = new Regex (pattern);
			Match m = r.Match (test);
			Assert.IsTrue (m.Success, "#A1");
			Assert.AreEqual (0, m.Index, "#A2");
			Assert.AreEqual (14, m.Length, "#A3");

			m = m.NextMatch ();
			Assert.IsFalse (m.Success, "#B");
		}

		[Test]
		public void Bug439947 ()
		{
			Regex r;
			r = new Regex ("(?<=^|/)[^/]*\\.cs$", RegexOptions.None);
			Assert.IsTrue (r.IsMatch ("z/text2.cs"));

			r = new Regex ("(?<=^|/)[^/]*\\.cs$", RegexOptions.Compiled);
			Assert.IsTrue (r.IsMatch ("z/text2.cs"));
		}

		[Test]
		public void bug443841 ()
		{
			string numberString = @"[0-9]+";
			string doubleString = string.Format (@" *[+-]? *{0}(\.{0})?([eE][+-]?{0})? *",
				numberString);
			string vector1To3String = string.Format (@"{0}(,{0}(,{0})?)?",
				doubleString);
			Regex r;
			MatchCollection matches;
			
			r = new Regex (string.Format ("^{0}$", vector1To3String));
			Assert.IsTrue (r.IsMatch ("1"), "#A1");
			matches = r.Matches ("1");
			Assert.AreEqual (1, matches.Count, "#A2");
			Assert.AreEqual ("1", matches [0].Value, "#A3");

			r = new Regex (string.Format ("^{0}$", vector1To3String),
				RegexOptions.Compiled);
			Assert.IsTrue (r.IsMatch ("1"), "#B1");
			matches = r.Matches ("1");
			Assert.AreEqual (1, matches.Count, "#B2");
			Assert.AreEqual ("1", matches [0].Value, "#B3");
		}

		[Test]
		public void CharClassWithIgnoreCase ()
		{
			string str = "Foobar qux";
			Regex re = new Regex (@"[a-z\s]*", RegexOptions.IgnoreCase);
			Match m = re.Match (str);
			Assert.AreEqual (str, m.Value);
		}

		[Test] // bug #78278
		public void No65535Limit ()
		{
			Kill65535_1 (65535);
			Kill65535_1 (65536);
			Kill65535_1 (131071);
			Kill65535_1 (131072);

			Kill65535_2 (65530);
			Kill65535_2 (65531);
			Kill65535_2 (131066);
			Kill65535_2 (131067);
		}

		[Test]
		public void GroupNumbers ()
		{
			GroupNumbers_1 ("a", 1);
			GroupNumbers_1 ("(a)", 2);
			GroupNumbers_1 ("(a)(b)", 3);
			GroupNumbers_1 ("(a)|(b)", 3);
			GroupNumbers_1 ("((a)(b))(c)", 5);
		}

		[Test]
		public void Trials ()
		{
			foreach (RegexTrial trial in trials)
				trial.Execute ();
		}

		[Test]
		public void Bug80554_0 ()
		{
			bug80554_trials [0].Execute ();
		}

		[Test]
		public void Bug80554_1 ()
		{
			bug80554_trials [1].Execute ();
		}

		[Test]
		public void Bug80554_2 ()
		{
			bug80554_trials [2].Execute ();
		}

		[Test]
		public void Bug80554_3 ()
		{
			bug80554_trials [3].Execute ();
		}

		[Test]
		public void Bug432172 ()
		{
			new Regex ("^(else|elif|except|finally)([^a-zA-Z_0-9]).*", RegexOptions.Compiled);
		}


		[Test]
		public void Bug610587_RepetitionOfPositionAssertion ()
		{
			Assert.AreEqual ("888", Regex.Match("888", "^*8.*").Value);
		}

		void Kill65535_1 (int length)
		{
			StringBuilder sb = new StringBuilder ("x");
			sb.Append ('a', length);
			sb.Append ('y');
			string teststring = sb.ToString ();
			Regex regex = new Regex (@"xa*y");
			Match m = regex.Match (teststring);
			Assert.IsTrue (m.Success, "#1:" + length);
			Assert.AreEqual (0, m.Index, "#2:" + length);
			Assert.AreEqual (teststring.Length, m.Length, "#3:" + length);
		}

		void Kill65535_2 (int length)
		{
			StringBuilder sb = new StringBuilder ("xaaaax");
			sb.Append ('a', length);
			sb.Append ('y');
			string teststring = sb.ToString ();
			Regex regex = new Regex (@"x.*y");
			Match m = regex.Match(teststring);
			Assert.IsTrue (m.Success, "#1:" + length);
			Assert.AreEqual (0, m.Index, "#2:" + length);
			Assert.AreEqual (teststring.Length, m.Length, "#3:" + length);
		}
		
		void GroupNumbers_1 (string s, int n)
		{
			Regex r = new Regex (s);
			int [] grps = r.GetGroupNumbers ();
			Assert.AreEqual (n, grps.Length, "#1:" + r);

			int sum = 0;
			for (int i = 0; i < grps.Length; ++i) {
				sum += grps [i];
				// group numbers are unique
				for (int j = 0; j < i; ++j)
					Assert.IsTrue (grps [i] != grps [j], "#2:" + r + " (" + i + "," + j + ")");
			}
			// no gaps in group numbering
			Assert.AreEqual ((n * (n - 1)) / 2, sum, "#3:" + r);
		}


		static string bug80554_s = @"(?(static)|(.*))(static)";
		static RegexTrial[] bug80554_trials = {
			new RegexTrial (bug80554_s, RegexOptions.None, "static", "Pass. Group[0]=(0,6) Group[1]= Group[2]=(0,6)"),
			new RegexTrial (bug80554_s, RegexOptions.None, "hydrostatic", "Pass. Group[0]=(0,11) Group[1]=(0,5) Group[2]=(5,6)"),
			new RegexTrial (bug80554_s, RegexOptions.None, "statics", "Pass. Group[0]=(0,6) Group[1]= Group[2]=(0,6)"),
			new RegexTrial (bug80554_s, RegexOptions.None, "dynamic", "Fail.")
		};

		static RegexTrial[] trials = {
			new RegexTrial (@"^[^.\d]*(\d+)(?:\D+(\d+))?", RegexOptions.None, "MD 9.18", "Pass. Group[0]=(0,7) Group[1]=(3,1) Group[2]=(5,2)"),
            new RegexTrial (@"(.*:|.*)(DirName)", RegexOptions.Compiled, "/home/homedir/DirName", "Pass. Group[0]=(0,21) Group[1]=(0,14) Group[2]=(14,7)")
	    };
	}
}
