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
	public class RegexTest : Assertion {

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

		[Test]
		public void Match1 ()
		{
			Regex email = new Regex ("(?<user>[^@]+)@(?<domain>.+)");
			Match m;

			m = email.Match ("mono@go-mono.com");

			Assert ("#m01", m.Success);
			AssertEquals ("#m02", "mono", m.Groups ["user"].Value);
			AssertEquals ("#m03", "go-mono.com", m.Groups ["domain"].Value);

			m = email.Match ("mono.bugs@go-mono.com");
			Assert ("#m04", m.Success);
			AssertEquals ("#m05", "mono.bugs", m.Groups ["user"].Value);
			AssertEquals ("#m06", "go-mono.com", m.Groups ["domain"].Value);
		}

		[Test]
		public void Matches1 ()
		{
			Regex words = new Regex ("(?<word>\\w+)");

			MatchCollection mc = words.Matches ("the fat cat ate the rat");

			AssertEquals ("#m01", 6, mc.Count);
			AssertEquals ("#m02", "the", mc [0].Value);
			AssertEquals ("#m03", "fat", mc [1].Value);
			AssertEquals ("#m04", "cat", mc [2].Value);
			AssertEquals ("#m05", "ate", mc [3].Value);
			AssertEquals ("#m06", "the", mc [4].Value);
			AssertEquals ("#m07", "rat", mc [5].Value);
		}

		[Test]
		public void Matches2 ()
		{
			Regex words = new Regex ("(?<word>\\w+)", RegexOptions.RightToLeft);

			MatchCollection mc = words.Matches ("the fat cat ate the rat");

			AssertEquals ("#m01", 6, mc.Count);
			AssertEquals ("#m02", "the", mc [5].Value);
			AssertEquals ("#m03", "fat", mc [4].Value);
			AssertEquals ("#m04", "cat", mc [3].Value);
			AssertEquals ("#m05", "ate", mc [2].Value);
			AssertEquals ("#m06", "the", mc [1].Value);
			AssertEquals ("#m07", "rat", mc [0].Value);
		}

		[Test]
		public void Matches3 ()
		{
			Regex digits = new Regex ("(?<digit>\\d+)");

			MatchCollection mc = digits.Matches ("0 1 2 3 4 5 6a7b8c9d10");

			AssertEquals ("#m01", 11, mc.Count);
			AssertEquals ("#m02", "0", mc [0].Value);
			AssertEquals ("#m03", "1", mc [1].Value);
			AssertEquals ("#m04", "2", mc [2].Value);
			AssertEquals ("#m05", "3", mc [3].Value);
			AssertEquals ("#m06", "4", mc [4].Value);
			AssertEquals ("#m07", "5", mc [5].Value);
			AssertEquals ("#m08", "6", mc [6].Value);
			AssertEquals ("#m09", "7", mc [7].Value);
			AssertEquals ("#m10", "8", mc [8].Value);
			AssertEquals ("#m11", "9", mc [9].Value);
			AssertEquals ("#m12", "10", mc [10].Value);
		}

		[Test]
		public void Matches4 ()
		{
			Regex digits = new Regex ("(?<digit>\\d+)", RegexOptions.RightToLeft);

			MatchCollection mc = digits.Matches ("0 1 2 3 4 5 6a7b8c9d10");

			AssertEquals ("#m01", 11, mc.Count);
			AssertEquals ("#m02", "0", mc [10].Value);
			AssertEquals ("#m03", "1", mc [9].Value);
			AssertEquals ("#m04", "2", mc [8].Value);
			AssertEquals ("#m05", "3", mc [7].Value);
			AssertEquals ("#m06", "4", mc [6].Value);
			AssertEquals ("#m07", "5", mc [5].Value);
			AssertEquals ("#m08", "6", mc [4].Value);
			AssertEquals ("#m09", "7", mc [3].Value);
			AssertEquals ("#m10", "8", mc [2].Value);
			AssertEquals ("#m11", "9", mc [1].Value);
			AssertEquals ("#m12", "10", mc [0].Value);
		}

		[Test]
		public void Matches5 ()
		{
			Regex lines = new Regex ("(?<line>.+)");

			MatchCollection mc = lines.Matches (story);

			AssertEquals ("#m01", 5, mc.Count);
			AssertEquals ("#m02", "Two little dragons lived in the forest", mc [0].Value);
			AssertEquals ("#m03", "They spent their days collecting honey suckle,",
					mc [1].Value);
			AssertEquals ("#m04", "And eating curds and whey", mc [2].Value);
			AssertEquals ("#m05", "Until an evil sorcer came along", mc [3].Value);
			AssertEquals ("#m06", "And chased my dragon friends away", mc [4].Value);
		}

		[Test]
		public void Matches6 ()
		{
			Regex lines = new Regex ("(?<line>.+)", RegexOptions.RightToLeft);

			MatchCollection mc = lines.Matches (story);

			AssertEquals ("#m01", 5, mc.Count);
			AssertEquals ("#m02", "Two little dragons lived in the forest", mc [4].Value);
			AssertEquals ("#m03", "They spent their days collecting honey suckle,",
					mc [3].Value);
			AssertEquals ("#m04", "And eating curds and whey", mc [2].Value);
			AssertEquals ("#m05", "Until an evil sorcer came along", mc [1].Value);
			AssertEquals ("#m06", "And chased my dragon friends away", mc [0].Value);
		}

		string story =	"Two little dragons lived in the forest\n" +
				"They spent their days collecting honey suckle,\n" +
				"And eating curds and whey\n" +
				"Until an evil sorcer came along\n" +
				"And chased my dragon friends away";

		[Test]
		public void Matches7 ()
		{
			Regex nonwhite = new Regex ("(?<nonwhite>\\S+)");

			MatchCollection mc = nonwhite.Matches ("ab 12 cde 456 fghi .,\niou");

			AssertEquals ("#m01", 7, mc.Count);
			AssertEquals ("#m02", "ab", mc [0].Value);
			AssertEquals ("#m03", "12", mc [1].Value);
			AssertEquals ("#m04", "cde", mc [2].Value);
			AssertEquals ("#m05", "456", mc [3].Value);
			AssertEquals ("#m06", "fghi", mc [4].Value);
			AssertEquals ("#m07", ".,", mc [5].Value);
			AssertEquals ("#m08", "iou", mc [6].Value);
		}

		[Test]
		public void Matches8 ()
		{
			Regex nonwhite = new Regex ("(?<nonwhite>\\S+)", RegexOptions.RightToLeft);

			MatchCollection mc = nonwhite.Matches ("ab 12 cde 456 fghi .,\niou");

			AssertEquals ("#m01", 7, mc.Count);
			AssertEquals ("#m02", "ab", mc [6].Value);
			AssertEquals ("#m03", "12", mc [5].Value);
			AssertEquals ("#m04", "cde", mc [4].Value);
			AssertEquals ("#m05", "456", mc [3].Value);
			AssertEquals ("#m06", "fghi", mc [2].Value);
			AssertEquals ("#m07", ".,", mc [1].Value);
			AssertEquals ("#m08", "iou", mc [0].Value);
		}

		[Test]
		public void Matches9 ()
		{
			Regex nondigit = new Regex ("(?<nondigit>\\D+)");

			MatchCollection mc = nondigit.Matches ("ab0cd1ef2");

			AssertEquals ("#m01", 3, mc.Count);
			AssertEquals ("#m02", "ab", mc [0].Value);
			AssertEquals ("#m02", "cd", mc [1].Value);
			AssertEquals ("#m02", "ef", mc [2].Value);
			
		}

		[Test]
		public void Matches10 ()
		{
			Regex nondigit = new Regex ("(?<nondigit>\\D+)", RegexOptions.RightToLeft);

			MatchCollection mc = nondigit.Matches ("ab0cd1ef2");

			AssertEquals ("#m01", 3, mc.Count);
			AssertEquals ("#m02", "ab", mc [2].Value);
			AssertEquals ("#m02", "cd", mc [1].Value);
			AssertEquals ("#m02", "ef", mc [0].Value);
			
		}
	}
}

