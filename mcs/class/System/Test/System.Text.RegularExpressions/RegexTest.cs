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

using System.Collections.Generic;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions
{

	[TestFixture]
	public class CompiledRegexTest :  RegexTest
	{
		[SetUp]
        public void SetUp ()
		{
			Compiled = true;
		}
	}

	[TestFixture]
	public class InterpretedRegexTest :  RegexTest
	{
        [SetUp]
        public void SetUp ()
        {
	        Compiled = false;
        }
	}


	public class RegexTest
	{

       RegexOptions AddOptions ( RegexOptions options ){
	       if( Compiled ){
		       options |= RegexOptions.Compiled;
	       }

	       return options;
       }

       protected bool Compiled { get; set; }

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

		[Test]
		public void Simple ()
		{
			char[] c = { (char)32, (char)8212, (char)32 };
			string s = new String(c);
			Assert.IsTrue (Regex.IsMatch(s, s), "char");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void NullPattern1 ()
		{
			new Regex (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void NullPattern2 ()
		{
			new Regex (null, AddOptions( RegexOptions.None ));
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidOptions1 ()
		{
			new Regex ("foo", (RegexOptions) Int32.MaxValue);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidOptions2 ()
		{
			new Regex ("foo", AddOptions( RegexOptions.ECMAScript | RegexOptions.RightToLeft ));
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
			Regex email = new Regex ("(?<user>[^@]+)@(?<domain>.+)",
			                         AddOptions( RegexOptions.None ));
			Match m;

			m = email.Match ("mono@example.com");

			Assert.IsTrue (m.Success, "#m01");
			Assert.AreEqual ("mono", m.Groups ["user"].Value, "#m02");
			Assert.AreEqual ("example.com", m.Groups ["domain"].Value, "#m03");

			m = email.Match ("mono.bugs@example.com");
			Assert.IsTrue (m.Success, "m04");
			Assert.AreEqual ("mono.bugs", m.Groups ["user"].Value, "#m05");
			Assert.AreEqual ("example.com", m.Groups ["domain"].Value, "#m06");
		}

		[Test]
		public void Match2 ()
		{
			Regex regex = new Regex(@"(?<tab>\t)|(?<text>[^\t]*)",
			                        AddOptions( RegexOptions.None ));
			MatchCollection col = regex.Matches("\tjust a text");
			Assert.AreEqual(3, col.Count);
			Assert.AreEqual (col [0].Value, "\t");
			Assert.AreEqual (col [1].Value, "just a text");
			Assert.AreEqual(col[2].Value, string.Empty);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Match_Null1 ()
		{
			new Regex (@"foo",AddOptions( RegexOptions.None )).Match (null);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Match_BadStart1 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", -1);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Match_BadStart2 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", -1, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Match_BadStart3 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", 7);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Match_BadStart4 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", 7, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Match_BadLength1 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", 5, -1);
		}

		[Test, ExpectedException (typeof (IndexOutOfRangeException))]
		public void Match_BadLength2 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Match ("foobar", 5, 3);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Matches_Null1 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Matches (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Matches_Null2 ()
		{
			new Regex (@"foo",
			           AddOptions( RegexOptions.None )).Matches (null, 0);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Matches_Null3 ()
		{
			new Regex (@"foo",
			           AddOptions(RegexOptions.RightToLeft)).Matches (null);
		}

		[Test]
		public void Match_SubstringAnchors ()
		{
			Regex r = new Regex ("^ooba$",
			                     AddOptions( RegexOptions.None ));
			Match m = r.Match ("foobar", 1, 4);

			Assert.IsTrue (m.Success);
			Assert.AreEqual ("ooba", m.Value);
		}

		[Test]
		public void Match_SubstringRtl ()
		{
			Regex r = new Regex(@".*", RegexOptions.RightToLeft);
			Match m = r.Match("ABCDEFGHI", 2, 6);

			Assert.IsTrue (m.Success);
			Assert.AreEqual ("CDEFGH", m.Value);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_InputNull ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			MatchEvaluator m = delegate (Match match) {return null;};
			r.Replace (null, m, 0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_InputNull2 ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			r.Replace (null, "abc", 0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_InputNull3 ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions(RegexOptions.RightToLeft));
			MatchEvaluator m = delegate (Match match) {return null;};
			r.Replace (null, m);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_InputNull4 ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions(RegexOptions.RightToLeft));
			r.Replace (null, "abc");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_ReplacementNull ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			r.Replace ("string", (string) null, 0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Replace_EvaluatorNull ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			MatchEvaluator m = null;
			r.Replace ("string", m, 0, 0);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Replace_InvalidCount ()
		{
			Regex r = new Regex ("foo|bar",
			                     AddOptions( RegexOptions.None ));
			r.Replace ("foo",  "baz", -4);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Replace_InvalidStart ()
		{
			Regex r = new Regex ("foo|bar",
			                     AddOptions( RegexOptions.None ));
			r.Replace ("foo", "baz", 1, -4);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Split_InputNull1 ()
		{
			Regex.Split (null, "^.*$");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Split_InputNull2 ()
		{
			Regex.Split (null, "^.*$", RegexOptions.RightToLeft);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Split_InvalidCount ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			r.Split ("foo", -4);
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Split_InvalidCount2 ()
		{
			Regex r = new Regex ("^.*$",
			                     AddOptions( RegexOptions.None ));
			r.Split ("foo", 1, -4);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Escape_Null ()
		{
			Regex.Escape (null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Unescape_Null ()
		{
			Regex.Unescape (null);
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

		static void runTrial (MatchCollectionTrial t, bool compiled)
		{
			runTrial (t, false, compiled);
			runTrial (t, true, compiled);
		}

		static void runTrial (MatchCollectionTrial t, bool rtl, bool compiled)
		{
			int i;
			MatchCollection mc;

			string name = t.name;
			if (rtl)
				name += "-rtl";

			int len = t.matches.Length;
			RegexOptions options = rtl ? RegexOptions.RightToLeft : RegexOptions.None;
			if( compiled )
				options |= RegexOptions.Compiled;

			Regex r = new Regex (t.regex,options);

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
			foreach (MatchCollectionTrial t in trials)
				runTrial (t,Compiled);
		}
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

		static IEnumerable<uint> Primes (uint m)
		{
			if (m < 2)
				yield break;

			yield return 2;

			Dictionary<uint, uint> w = new Dictionary<uint, uint> ();
			uint p2, n1;

			for (uint n = 3; n < m; n += 2) {
				if (w.TryGetValue (n, out p2)) {
					w.Remove (n);
					n1 = n + p2;
				} else {
					yield return n;
					n1 = n * n;
					p2 = n + n;

					// if there's an overflow, don't bother
					if (n1 / n != n || n1 >= m)
						continue;
				}

				while (w.ContainsKey (n1))
					n1 += p2;
				w [n1] = p2;
			}
		}

		[Test]
		public void PrimeRegex ()
		{
			// Perl regex oneliner by: abigail@fnx.com (Abigail)
			// from: http://www.mit.edu:8008/bloom-picayune.mit.edu/perl/10138
			// perl -wle 'print "Prime" if (1 x shift) !~ /^1?$|^(11+?)\1+$/'

			// This is a backtracking torture test

			Regex composite = new Regex (@"^1?$|^(11+?)\1+$",
			                             AddOptions( RegexOptions.None ));

			uint i = 0;
			string x = "";

			foreach (uint p in Primes (3333)) {
				while (i < p) {
					Assert.IsTrue (composite.IsMatch (x));
					++i;
					x += "1";
				}
				// i == p
				Assert.IsFalse (composite.IsMatch (x));
				++i;
				x += "1";
			}
		}
	}
}
