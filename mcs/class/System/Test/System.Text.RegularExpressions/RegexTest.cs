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

#if NET_2_0
		private int cache_initial_value;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cache_initial_value = Regex.CacheSize;
		}

		[TearDown]
		public void TearDown ()
		{
			Regex.CacheSize = cache_initial_value;
		}
#endif
		[Test]
		public void Simple ()
		{
			char[] c = { (char)32, (char)8212, (char)32 };
			string s = new String(c);			
			Assert.IsTrue (Regex.IsMatch(s, s), "char");
		}
		
		[Test]
		public void Unescape ()
		{
			string inString = @"\a\b\t\r\v\f\n\e\02400\x231\cC\ufffff\*";
			char [] c = { (char)7, (char)8, (char)9, (char)13, 
				      (char)11, (char)12, (char)10, (char)27, (char) 20,
				      (char)48, (char)48, (char)35, (char)49, 
				      (char)3, (char)65535, (char)102, (char)42
			};
			string expectedString = new String(c);
			string outString = Regex.Unescape(inString);

			Assert.AreEqual (outString, expectedString, "unescape");
		}

		[Test]
		public void Match1 ()
		{
			Regex email = new Regex ("(?<user>[^@]+)@(?<domain>.+)");
			Match m;

			m = email.Match ("mono@go-mono.com");

			Assert.IsTrue (m.Success, "#m01");
			Assert.AreEqual ("mono", m.Groups ["user"].Value, "#m02");
			Assert.AreEqual ("go-mono.com", m.Groups ["domain"].Value, "#m03");

			m = email.Match ("mono.bugs@go-mono.com");
			Assert.IsTrue (m.Success, "m04");
			Assert.AreEqual ("mono.bugs", m.Groups ["user"].Value, "#m05");
			Assert.AreEqual ("go-mono.com", m.Groups ["domain"].Value, "#m06");
		}

		static string story =	
			"Two little dragons lived in the forest\n" +
			"They spent their days collecting honey suckle,\n" +
			"And eating curds and whey\n" +
			"Until an evil sorcer came along\n" +
			"And chased my dragon friends away";

		struct MatchCollectionTrial {
			public readonly string name;
			public readonly string text;
			public readonly string regex;
			public readonly string [] matches;
			public MatchCollectionTrial (string name, string text, string regex, string [] matches)
			{
				this.name = name;
				this.text = text;
				this.regex = regex;
				this.matches = matches;
			}
		}

		static readonly MatchCollectionTrial [] trials = {
			new MatchCollectionTrial ("word", "the fat cat ate the rat", "(?<word>\\w+)", 
				new string [] { "the", "fat", "cat", "ate", "the", "rat" }),
			new MatchCollectionTrial ("digit", "0 1 2 3 4 5 6a7b8c9d10", "(?<digit>\\d+)", 
				new string [] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }),
			new MatchCollectionTrial ("line", story, "(?<line>.+)", 
				new string [] { "Two little dragons lived in the forest",
						"They spent their days collecting honey suckle,",
						"And eating curds and whey",
						"Until an evil sorcer came along",
						"And chased my dragon friends away" }),
			new MatchCollectionTrial ("nonwhite", "ab 12 cde 456 fghi .,\niou", "(?<nonwhite>\\S+)",
				new string [] { "ab", "12", "cde", "456", "fghi", ".,", "iou" }),
			new MatchCollectionTrial ("nondigit", "ab0cd1ef2", "(?<nondigit>\\D+)",
				new string [] { "ab", "cd", "ef" })
		};

		static void runTrial (MatchCollectionTrial t)
		{
			runTrial (t, false);
			runTrial (t, true);
		}

		static void runTrial (MatchCollectionTrial t, bool rtl)
		{
			int i;
			MatchCollection mc;

			string name = t.name;
			if (rtl)
				name += "-rtl";

			int len = t.matches.Length;
			Regex r = new Regex (t.regex, rtl ? RegexOptions.RightToLeft : RegexOptions.None);

			// Incremental mode -- this access
			mc = r.Matches (t.text);
			for (i = 0; i < len; ++i)
				Assert.AreEqual (mc [i].Value, t.matches [rtl ? len - i - 1 : i], "{0}:this:{1}", name, i);
			Assert.AreEqual (i, mc.Count, "{0}:this:count", name);

			// Incremental mode -- enumerator
			mc = r.Matches (t.text);
			i = 0;
			foreach (Match m in mc) {
				Assert.AreEqual (m.Value, t.matches [rtl ? len - i - 1 : i], "{0}:enum:{1}", name, i);
				++i;
			}
			Assert.AreEqual (i, len, "{0}:enum:count", name);

			// random mode
			Random rng = new Random ();
			for (int j = 0; j < len * 5; ++j) {
				i = rng.Next (len);
				Assert.AreEqual (mc [i].Value, t.matches [rtl ? len - i - 1 : i], "{0}:random{1}:{2}", name, j, i);
			}

			// Non-incremental mode
			mc = r.Matches (t.text);
			Assert.AreEqual (mc.Count, len);
			i = 0;
			foreach (Match m in mc) {
				Assert.AreEqual (m.Value, t.matches [rtl ? len - i - 1 : i], "{0}:nienum:{1}", name, i);
				++i;
			}
			for (i = 0; i < len; ++i)
				Assert.AreEqual (mc [i].Value, t.matches [rtl ? len - i - 1 : i], "{0}:nithis:{1}", name, i);
		}

		[Test]
		public void Matches ()
		{
			int i;
			MatchCollection mc;
			Regex r;
			foreach (MatchCollectionTrial t in trials)
				runTrial (t);
		}
#if NET_2_0
		[Test]
		public void CacheSize ()
		{
			Assert.AreEqual (15, Regex.CacheSize, "CacheSize");
			Regex.CacheSize = 0;
			Regex.CacheSize = Int32.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CacheSize_Negative ()
		{
			Regex.CacheSize = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CacheSize_Min ()
		{
			Regex.CacheSize = Int32.MinValue;
		}
#endif
	}
}
