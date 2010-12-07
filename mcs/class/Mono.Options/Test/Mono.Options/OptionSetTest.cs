//
// OptionSetTest.cs
//
// Authors:
//  Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

#if NDESK_OPTIONS
using NDesk.Options;
#else
using Mono.Options;
#endif

using Cadenza.Collections.Tests;

using NUnit.Framework;

#if NDESK_OPTIONS
namespace Tests.NDesk.Options
#else
namespace Tests.Mono.Options
#endif
{
	class FooConverter : TypeConverter {
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
				CultureInfo culture, object value)
		{
			string v = value as string;
			if (v != null) {
				switch (v) {
					case "A": return Foo.A;
					case "B": return Foo.B;
				}
			}

			return base.ConvertFrom (context, culture, value);
		}
	}

	[TypeConverter (typeof(FooConverter))]
	class Foo {
		public static readonly Foo A = new Foo ("A");
		public static readonly Foo B = new Foo ("B");
		string s;
		Foo (string s) { this.s = s; }
		public override string ToString () {return s;}
	}

	class TestArgumentSource : ArgumentSource, IEnumerable {
		string[] names;
		string desc;

		public TestArgumentSource (string[] names, string desc)
		{
			this.names  = names;
			this.desc   = desc;
		}

		Dictionary<string, string[]> args = new Dictionary<string, string[]>();

		public void Add (string key, params string[] values)
		{
			args.Add (key, values);
		}

		public override string[] GetNames ()
		{
			return names;
		}

		public override string Description {
			get {return desc;}
		}

		public override bool GetArguments (string value, out IEnumerable<string> replacement)
		{
			replacement = null;

			string[] values;
			if (args.TryGetValue (value, out values)) {
				replacement = values;
				return true;
			}

			return false;
		}


		IEnumerator IEnumerable.GetEnumerator ()
		{
			return args.GetEnumerator ();
		}
	}

	[TestFixture]
	public class OptionSetTest : ListContract<Option> {

		protected override ICollection<Option> CreateCollection (IEnumerable<Option> values)
		{
			OptionSet set = new OptionSet();
			foreach (Option value in values)
				set.Add (value);
			return set;
		}

		protected override Option CreateValueA ()
		{
			return new CustomOption ("A", null, 0, null);
		}

		protected override Option CreateValueB ()
		{
			return new CustomOption ("B", null, 0, null);
		}

		protected override Option CreateValueC ()
		{
			return new CustomOption ("C", null, 0, null);
		}

		static IEnumerable<string> _ (params string[] a)
		{
			return a;
		}

		[Test]
		public void BundledValues ()
		{
			BundledValues (_("-DNAME", "-D", "NAME2", "-Debug", "-L/foo", "-L", "/bar", "-EDNAME3"));
			BundledValues (_("@s1", "-D", "@s2", "-L/foo", "@s4"));
		}

		public void BundledValues (IEnumerable<string> args)
		{
			var defines = new List<string> ();
			var libs    = new List<string> ();
			bool debug  = false;
			var p = new OptionSet () {
				{ "D|define=",  v => defines.Add (v) },
				{ "L|library:", v => libs.Add (v) },
				{ "Debug",      v => debug = v != null },
				{ "E",          v => { /* ignore */ } },
				new TestArgumentSource (null, null) {
					{ "@s1", "-DNAME" },
					{ "@s2", "NAME2", "@s3" },
					{ "@s3", "-Debug" },
					{ "@s4", "-L", "/bar", "-EDNAME3" },
				},
			};
			p.Parse (args);
			Assert.AreEqual (defines.Count, 3);
			Assert.AreEqual (defines [0], "NAME");
			Assert.AreEqual (defines [1], "NAME2");
			Assert.AreEqual (defines [2], "NAME3");
			Assert.AreEqual (debug, true);
			Assert.AreEqual (libs.Count, 2);
			Assert.AreEqual (libs [0], "/foo");
			Assert.AreEqual (libs [1], null);

			Utils.AssertException (typeof(OptionException), 
					"Cannot bundle unregistered option '-V'.",
					p, v => { v.Parse (_("-EVALUENOTSUP")); });
		}

		[Test]
		public void RequiredValues ()
		{
			RequiredValues (_("a", "-a", "s", "-n=42", "n"));
			RequiredValues (_("@s1", "s", "@s2", "n"));
		}

		void RequiredValues (IEnumerable<string> args)
		{
			string a = null;
			int n = 0;
			OptionSet p = new OptionSet () {
				{ "a=", v => a = v },
				{ "n=", (int v) => n = v },
				new TestArgumentSource (null, null) {
					{ "@s1", "a", "-a" },
					{ "@s2", "-n=42" },
				},
			};
			List<string> extra = p.Parse (args);
			Assert.AreEqual (extra.Count, 2);
			Assert.AreEqual (extra [0], "a");
			Assert.AreEqual (extra [1], "n");
			Assert.AreEqual (a, "s");
			Assert.AreEqual (n, 42);

			extra = p.Parse (_("-a="));
			Assert.AreEqual (extra.Count, 0);
			Assert.AreEqual (a, "");
		}

		[Test]
		public void OptionalValues ()
		{
			string a = null;
			int? n = -1;
			Foo f = null;
			OptionSet p = new OptionSet () {
				{ "a:", v => a = v },
				{ "n:", (int? v) => n = v },
				{ "f:", (Foo v) => f = v },
			};
			p.Parse (_("-a=s"));
			Assert.AreEqual (a, "s");
			p.Parse (_("-a"));
			Assert.AreEqual (a, null);
			p.Parse (_("-a="));
			Assert.AreEqual (a, "");

			p.Parse (_("-f", "A"));
			Assert.AreEqual (f, null);
			p.Parse (_("-f"));
			Assert.AreEqual (f, null);
			p.Parse (_("-f=A"));
			Assert.AreEqual (f, Foo.A);
			f = null;
			p.Parse (_("-fA"));
			Assert.AreEqual (f, Foo.A);

			p.Parse (_("-n42"));
			Assert.AreEqual (n.Value, 42);
			p.Parse (_("-n", "42"));
			Assert.AreEqual (n.HasValue, false);
			p.Parse (_("-n=42"));
			Assert.AreEqual (n.Value, 42);
			p.Parse (_("-n"));
			Assert.AreEqual (n.HasValue, false);
			Utils.AssertException (typeof(OptionException),
					"Could not convert string `' to type Int32 for option `-n'.",
					p, v => { v.Parse (_("-n=")); });
		}

		[Test]
		public void BooleanValues ()
		{
			bool a = false;
			OptionSet p = new OptionSet () {
				{ "a", v => a = v != null },
			};
			p.Parse (_("-a"));
			Assert.AreEqual (a, true);
			p.Parse (_("-a+"));
			Assert.AreEqual (a, true);
			p.Parse (_("-a-"));
			Assert.AreEqual (a, false);
		}

		[Test]
		public void CombinationPlatter ()
		{
			CombinationPlatter (new string[]{"foo", "-v", "-a=42", "/b-",
				"-a", "64", "bar", "--f", "B", "/h", "-?", "--help", "-v"});
			CombinationPlatter (_("@s1", "-a=42", "@s3", "-a", "64", "bar", "@s4"));
		}

		void CombinationPlatter (IEnumerable<string> args)
		{
			int a = -1, b = -1;
			string av = null, bv = null;
			Foo f = null;
			int help = 0;
			int verbose = 0;
			OptionSet p = new OptionSet () {
				{ "a=", v => { a = 1; av = v; } },
				{ "b", "desc", v => {b = 2; bv = v;} },
				{ "f=", (Foo v) => f = v },
				{ "v", v => { ++verbose; } },
				{ "h|?|help", (v) => { switch (v) {
					case "h": help |= 0x1; break; 
					case "?": help |= 0x2; break;
					case "help": help |= 0x4; break;
				} } },
				new TestArgumentSource (null, null) {
					{ "@s1", "foo", "-v", "@s2" },
					{ "@s2" },
					{ "@s3", "/b-" },
					{ "@s4", "--f", "B", "/h", "-?", "--help", "-v" },
				},
			};
			List<string> e = p.Parse (args);

			Assert.AreEqual (e.Count, 2);
			Assert.AreEqual (e[0], "foo");
			Assert.AreEqual (e[1], "bar");
			Assert.AreEqual (a, 1);
			Assert.AreEqual (av, "64");
			Assert.AreEqual (b, 2);
			Assert.AreEqual (bv, null);
			Assert.AreEqual (verbose, 2);
			Assert.AreEqual (help, 0x7);
			Assert.AreEqual (f, Foo.B);
		}

		[Test]
		public void Exceptions ()
		{
			string a = null;
			var p = new OptionSet () {
				{ "a=", v => a = v },
				{ "b",  v => { } },
				{ "c",  v => { } },
				{ "n=", (int v) => { } },
				{ "f=", (Foo v) => { } },
			};
			// missing argument
			Utils.AssertException (typeof(OptionException), 
					"Missing required value for option '-a'.", 
					p, v => { v.Parse (_("-a")); });
			// another named option while expecting one -- follow Getopt::Long
			Utils.AssertException (null, null,
					p, v => { v.Parse (_("-a", "-a")); });
			Assert.AreEqual (a, "-a");
			// no exception when an unregistered named option follows.
			Utils.AssertException (null, null, 
					p, v => { v.Parse (_("-a", "-b")); });
			Assert.AreEqual (a, "-b");
			Utils.AssertException (typeof(ArgumentNullException),
					"Argument cannot be null.\nParameter name: option",
					p, v => { v.Add ((Option) null); });

			// bad type
			Utils.AssertException (typeof(OptionException),
					"Could not convert string `value' to type Int32 for option `-n'.",
					p, v => { v.Parse (_("-n", "value")); });
			Utils.AssertException (typeof(OptionException),
					"Could not convert string `invalid' to type Foo for option `--f'.",
					p, v => { v.Parse (_("--f", "invalid")); });

			// try to bundle with an option requiring a value
			Utils.AssertException (typeof(OptionException), 
					"Cannot bundle unregistered option '-z'.", 
					p, v => { v.Parse (_("-cz", "extra")); });

			Utils.AssertException (typeof(ArgumentNullException), 
					"Argument cannot be null.\nParameter name: action",
					p, v => { v.Add ("foo", (Action<string>) null); });
			Utils.AssertException (typeof(ArgumentException), 
					"Cannot provide maxValueCount of 2 for OptionValueType.None.\nParameter name: maxValueCount",
					p, v => { v.Add ("foo", (k, val) => {/* ignore */}); });
		}

		[Test]
		public void WriteOptionDescriptions ()
		{
			var p = new OptionSet () {
				{ "p|indicator-style=", "append / indicator to directories",    v => {} },
				{ "color:",             "controls color info",                  v => {} },
				{ "color2:",            "set {color}",                          v => {} },
				{ "rk=",                "required key/value option",            (k, v) => {} },
				{ "rk2=",               "required {{foo}} {0:key}/{1:value} option",    (k, v) => {} },
				{ "ok:",                "optional key/value option",            (k, v) => {} },
				{ "long-desc",
					"This has a really\nlong, multi-line description that also\ntests\n" +
						"the-builtin-supercalifragilisticexpialidicious-break-on-hyphen.  " + 
						"Also, a list:\n" +
						"  item 1\n" +
						"  item 2",
					v => {} },
				{ "long-desc2",
					"IWantThisDescriptionToBreakInsideAWordGeneratingAutoWordHyphenation.",
					v => {} },
				{ "long-desc3",
					"OnlyOnePeriod.AndNoWhitespaceShouldBeSupportedEvenWithLongDescriptions",
					v => {} },
				{ "long-desc4",
					"Lots of spaces in the middle 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 and more until the end.",
					v => {} },
				{ "long-desc5",
					"Lots of spaces in the middle - . - . - . - . - . - . - . - and more until the end.",
					v => {} },
				{ "o|out=",
					"The {DIRECTORY} to place the generated files and directories.\n\n" +
					"If not specified, defaults to\n`dirname FILE`/cache/`basename FILE .tree`.",
					v => {} },
				{ "h|?|help",           "show help text",                       v => {} },
				{ "version",            "output version information and exit",  v => {} },
				{ "<>", v => {} },
				new TestArgumentSource (new[]{"@s1", "@s2"}, "Read Response File for More Options"),
			};

			StringWriter expected = new StringWriter ();
			expected.WriteLine ("  -p, --indicator-style=VALUE");
			expected.WriteLine ("                             append / indicator to directories");
			expected.WriteLine ("      --color[=VALUE]        controls color info");
			expected.WriteLine ("      --color2[=color]       set color");
			expected.WriteLine ("      --rk=VALUE1:VALUE2     required key/value option");
			expected.WriteLine ("      --rk2=key:value        required {foo} key/value option");
			expected.WriteLine ("      --ok[=VALUE1:VALUE2]   optional key/value option");
			expected.WriteLine ("      --long-desc            This has a really");
			expected.WriteLine ("                               long, multi-line description that also");
			expected.WriteLine ("                               tests");
			expected.WriteLine ("                               the-builtin-supercalifragilisticexpialidicious-");
			expected.WriteLine ("                               break-on-hyphen.  Also, a list:");
			expected.WriteLine ("                                 item 1");
			expected.WriteLine ("                                 item 2");
			expected.WriteLine ("      --long-desc2           IWantThisDescriptionToBreakInsideAWordGeneratingAu-");
			expected.WriteLine ("                               toWordHyphenation.");
			expected.WriteLine ("      --long-desc3           OnlyOnePeriod.");
			expected.WriteLine ("                               AndNoWhitespaceShouldBeSupportedEvenWithLongDesc-");
			expected.WriteLine ("                               riptions");
			expected.WriteLine ("      --long-desc4           Lots of spaces in the middle 1 2 3 4 5 6 7 8 9 0 1");
			expected.WriteLine ("                               2 3 4 5 and more until the end.");
			expected.WriteLine ("      --long-desc5           Lots of spaces in the middle - . - . - . - . - . -");
			expected.WriteLine ("                               . - . - and more until the end.");
 			expected.WriteLine ("  -o, --out=DIRECTORY        The DIRECTORY to place the generated files and");
			expected.WriteLine ("                               directories.");
 			expected.WriteLine ("                               ");
			expected.WriteLine ("                               If not specified, defaults to");
 			expected.WriteLine ("                               `dirname FILE`/cache/`basename FILE .tree`.");
			expected.WriteLine ("  -h, -?, --help             show help text");
			expected.WriteLine ("      --version              output version information and exit");
			expected.WriteLine ("  @s1, @s2                   Read Response File for More Options");

			StringWriter actual = new StringWriter ();
			p.WriteOptionDescriptions (actual);

			Assert.AreEqual (expected.ToString (), actual.ToString ());
		}

		[Test]
		public void OptionBundling ()
		{
			OptionBundling (_ ("-abcf", "foo", "bar"));
			OptionBundling (_ ("@s1", "foo", "bar"));
		}

		void OptionBundling (IEnumerable<string> args)
		{
			string a, b, c, f;
			a = b = c = f = null;
			var p = new OptionSet () {
				{ "a", v => a = "a" },
				{ "b", v => b = "b" },
				{ "c", v => c = "c" },
				{ "f=", v => f = v },
				new TestArgumentSource (null, null) {
					{ "@s1", "-abcf" },
				},
			};
			List<string> extra = p.Parse (args);
			Assert.AreEqual (extra.Count, 1);
			Assert.AreEqual (extra [0], "bar");
			Assert.AreEqual (a, "a");
			Assert.AreEqual (b, "b");
			Assert.AreEqual (c, "c");
			Assert.AreEqual (f, "foo");
		}

		[Test]
		public void HaltProcessing ()
		{
			var p = new OptionSet () {
				{ "a", v => {} },
				{ "b", v => {} },
				new TestArgumentSource (null, null) {
					{ "@s1", "-a", "-b" },
				},
			};
			List<string> e = p.Parse (_ ("-a", "-b", "--", "-a", "-b"));
			Assert.AreEqual (e.Count, 2);
			Assert.AreEqual (e [0], "-a");
			Assert.AreEqual (e [1], "-b");

			e = p.Parse (_ ("@s1", "--", "@s1"));
			Assert.AreEqual (e.Count, 1);
			Assert.AreEqual (e [0], "@s1");
		}

		[Test]
		public void KeyValueOptions ()
		{
			var a = new Dictionary<string, string> ();
			var b = new Dictionary<int, char> ();
			var p = new OptionSet () {
				{ "a=", (k,v) => a.Add (k, v) },
				{ "b=", (int k, char v) => b.Add (k, v) },
				{ "c:", (k, v) => {if (k != null) a.Add (k, v);} },
				{ "d={=>}{-->}", (k, v) => a.Add (k, v) },
				{ "e={}", (k, v) => a.Add (k, v) },
				{ "f=+/", (k, v) => a.Add (k, v) },
				new TestArgumentSource (null, null) {
					{ "@s1", "-a", "A" },
					{ "@s2", @"C:\tmp", "-a" },
					{ "@s3", "C=D", @"-a=E=F:\tmp" },
					{ "@s4", "-a:G:H", "-aI=J" },
					{ "@s5", "-b", "1" },
					{ "@s6", "a", "-b" },
					{ "@s7", "2", "b" },
					{ "@s8", "-dA=>B", "-d" },
					{ "@s9", "C-->D", "-d:E" },
					{ "@s10", "F", "-d" },
					{ "@s11", "G", "H" },
					{ "@s12", "-dJ-->K" }
				},
			};
			p.Parse (_("-a", "A", @"C:\tmp", "-a", "C=D", @"-a=E=F:\tmp", "-a:G:H", "-aI=J", "-b", "1", "a", "-b", "2", "b"));
			Action assert = () => {
				AssertDictionary (a, 
						"A", @"C:\tmp", 
						"C", "D", 
						"E", @"F:\tmp", 
						"G", "H", 
						"I", "J");
				AssertDictionary (b,
						"1", "a",
						"2", "b");
			};
			assert ();
			a.Clear ();
			b.Clear ();

			p.Parse (_("@s1", "@s2", "@s3", "@s4", "@s5", "@s6", "@s7"));
			assert ();
			a.Clear ();
			b.Clear ();

			p.Parse (_("-c"));
			Assert.AreEqual (a.Count, 0);
			p.Parse (_("-c", "a"));
			Assert.AreEqual (a.Count, 0);
			p.Parse (_("-ca"));
			AssertDictionary (a, "a", null);
			a.Clear ();
			p.Parse (_("-ca=b"));
			AssertDictionary (a, "a", "b");

			a.Clear ();
			p.Parse (_("-dA=>B", "-d", "C-->D", "-d:E", "F", "-d", "G", "H", "-dJ-->K"));
			assert = () => {
				AssertDictionary (a,
						"A", "B",
						"C", "D", 
						"E", "F",
						"G", "H",
						"J", "K");
			};
			assert ();
			a.Clear ();

			p.Parse (_("@s8", "@s9", "@s10", "@s11", "@s12"));
			assert ();
			a.Clear ();

			p.Parse (_("-eA=B", "-eC=D", "-eE", "F", "-e:G", "H"));
			AssertDictionary (a,
					"A=B", "-eC=D",
					"E", "F", 
					"G", "H");

			a.Clear ();
			p.Parse (_("-f1/2", "-f=3/4", "-f:5+6", "-f7", "8", "-f9=10", "-f11=12"));
			AssertDictionary (a,
					"1", "2",
					"3", "4",
					"5", "6", 
					"7", "8", 
					"9=10", "-f11=12");
		}

		static void AssertDictionary<TKey, TValue> (Dictionary<TKey, TValue> dict, params string[] set)
		{
			TypeConverter k = TypeDescriptor.GetConverter (typeof (TKey));
			TypeConverter v = TypeDescriptor.GetConverter (typeof (TValue));

			Assert.AreEqual (dict.Count, set.Length / 2);
			for (int i = 0; i < set.Length; i += 2) {
				TKey key = (TKey) k.ConvertFromString (set [i]);
				Assert.AreEqual (dict.ContainsKey (key), true);
				if (set [i+1] == null)
					Assert.AreEqual (dict [key], default (TValue));
				else
					Assert.AreEqual (dict [key], (TValue) v.ConvertFromString (set [i+1]));
			}
		}

		class CustomOption : Option {
			Action<OptionValueCollection> action;

			public CustomOption (string p, string d, int c, Action<OptionValueCollection> a)
				: base (p, d, c)
			{
				this.action = a;
			}

			protected override void OnParseComplete (OptionContext c)
			{
				action (c.OptionValues);
			}
		}

		[Test]
		public void CustomKeyValue ()
		{
			var a = new Dictionary<string, string> ();
			var b = new Dictionary<string, string[]> ();
			var p = new OptionSet () {
				new CustomOption ("a==:", null, 2, v => a.Add (v [0], v [1])),
				new CustomOption ("b==:", null, 3, v => b.Add (v [0], new string[]{v [1], v [2]})),
			};
			p.Parse (_(@"-a=b=C:\tmp", "-a=d", @"C:\e", @"-a:f=C:\g", @"-a:h:C:\i", "-a", @"j=C:\k", "-a", @"l:C:\m"));
			Assert.AreEqual (a.Count, 6);
			Assert.AreEqual (a ["b"], @"C:\tmp");
			Assert.AreEqual (a ["d"], @"C:\e");
			Assert.AreEqual (a ["f"], @"C:\g");
			Assert.AreEqual (a ["h"], @"C:\i");
			Assert.AreEqual (a ["j"], @"C:\k");
			Assert.AreEqual (a ["l"], @"C:\m");

			Utils.AssertException (typeof(OptionException),
					"Missing required value for option '-a'.",
					p, v => {v.Parse (_("-a=b"));});

			p.Parse (_("-b", "a", "b", @"C:\tmp", @"-b:d:e:F:\tmp", @"-b=g=h:i:\tmp", @"-b:j=k:l:\tmp"));
			Assert.AreEqual (b.Count, 4);
			Assert.AreEqual (b ["a"][0], "b");
			Assert.AreEqual (b ["a"][1], @"C:\tmp");
			Assert.AreEqual (b ["d"][0], "e");
			Assert.AreEqual (b ["d"][1], @"F:\tmp");
			Assert.AreEqual (b ["g"][0], "h");
			Assert.AreEqual (b ["g"][1], @"i:\tmp");
			Assert.AreEqual (b ["j"][0], "k");
			Assert.AreEqual (b ["j"][1], @"l:\tmp");
		}

		[Test]
		public void Localization ()
		{
			var p = new OptionSet (f => "hello!") {
				{ "n=", (int v) => { } },
			};
			Utils.AssertException (typeof(OptionException), "hello!",
					p, v => { v.Parse (_("-n=value")); });

			StringWriter expected = new StringWriter ();
			expected.WriteLine ("  -nhello!                   hello!");

			StringWriter actual = new StringWriter ();
			p.WriteOptionDescriptions (actual);

			Assert.AreEqual (actual.ToString (), expected.ToString ());
		}

		class CiOptionSet : OptionSet {
			protected override void InsertItem (int index, Option item)
			{
				if (item.Prototype.ToLower () != item.Prototype)
					throw new ArgumentException ("prototypes must be null!");
				base.InsertItem (index, item);
			}

			protected override bool Parse (string option, OptionContext c)
			{
				if (c.Option != null)
					return base.Parse (option, c);
				string f, n, s, v;
				if (!GetOptionParts (option, out f, out n, out s, out v)) {
					return base.Parse (option, c);
				}
				return base.Parse (f + n.ToLower () + (v != null && s != null ? s + v : ""), c);
			}

			public new Option GetOptionForName (string n)
			{
				return base.GetOptionForName (n);
			}

			public void CheckOptionParts (string option, bool er, string ef, string en, string es, string ev)
			{
				string f, n, s, v;
				bool r = GetOptionParts (option, out f, out n, out s, out v);
				Assert.AreEqual (r, er);
				Assert.AreEqual (f, ef);
				Assert.AreEqual (n, en);
				Assert.AreEqual (s, es);
				Assert.AreEqual (v, ev);
			}
		}

		[Test]
		public void DerivedType ()
		{
			bool help = false;
			var p = new CiOptionSet () {
				{ "h|help", v => help = v != null },
			};
			p.Parse (_("-H"));
			Assert.AreEqual (help, true);
			help = false;
			p.Parse (_("-HELP"));
			Assert.AreEqual (help, true);

			Assert.AreEqual (p.GetOptionForName ("h"), p [0]);
			Assert.AreEqual (p.GetOptionForName ("help"), p [0]);
			Assert.AreEqual (p.GetOptionForName ("invalid"), null);

			Utils.AssertException (typeof(ArgumentException), "prototypes must be null!",
					p, v => { v.Add ("N|NUM=", (int n) => {}); });
			Utils.AssertException (typeof(ArgumentNullException),
					"Argument cannot be null.\nParameter name: option",
					p, v => { v.GetOptionForName (null); });
		}

		[Test]
		public void OptionParts ()
		{
			var p = new CiOptionSet ();
			p.CheckOptionParts ("A",        false,  null, null, null, null);
			p.CheckOptionParts ("A=B",      false,  null, null, null, null);
			p.CheckOptionParts ("-A=B",     true,   "-",  "A",  "=",  "B");
			p.CheckOptionParts ("-A:B",     true,   "-",  "A",  ":",  "B");
			p.CheckOptionParts ("--A=B",    true,   "--", "A",  "=",  "B");
			p.CheckOptionParts ("--A:B",    true,   "--", "A",  ":",  "B");
			p.CheckOptionParts ("/A=B",     true,   "/",  "A",  "=",  "B");
			p.CheckOptionParts ("/A:B",     true,   "/",  "A",  ":",  "B");
			p.CheckOptionParts ("-A=B=C",   true,   "-",  "A",  "=",  "B=C");
			p.CheckOptionParts ("-A:B=C",   true,   "-",  "A",  ":",  "B=C");
			p.CheckOptionParts ("-A:B:C",   true,   "-",  "A",  ":",  "B:C");
			p.CheckOptionParts ("--A=B=C",  true,   "--", "A",  "=",  "B=C");
			p.CheckOptionParts ("--A:B=C",  true,   "--", "A",  ":",  "B=C");
			p.CheckOptionParts ("--A:B:C",  true,   "--", "A",  ":",  "B:C");
			p.CheckOptionParts ("/A=B=C",   true,   "/",  "A",  "=",  "B=C");
			p.CheckOptionParts ("/A:B=C",   true,   "/",  "A",  ":",  "B=C");
			p.CheckOptionParts ("/A:B:C",   true,   "/",  "A",  ":",  "B:C");
			p.CheckOptionParts ("-AB=C",    true,   "-",  "AB", "=",  "C");
			p.CheckOptionParts ("-AB:C",    true,   "-",  "AB", ":",  "C");
		}

		class ContextCheckerOption : Option {
			string eName, eValue;
			int index;

			public ContextCheckerOption (string p, string d, string eName, string eValue, int index)
				: base (p, d)
			{
				this.eName  = eName;
				this.eValue = eValue;
				this.index  = index;
			}

			protected override void OnParseComplete (OptionContext c)
			{
				Assert.AreEqual (c.OptionValues.Count, 1);
				Assert.AreEqual (c.OptionValues [0], eValue);
				Assert.AreEqual (c.OptionName, eName);
				Assert.AreEqual (c.OptionIndex, index);
				Assert.AreEqual (c.Option, this);
				Assert.AreEqual (c.Option.Description, base.Description);
			}
		}

		[Test]
		public void OptionContext ()
		{
			var p = new OptionSet () {
				new ContextCheckerOption ("a=", "a desc", "/a",   "a-val", 1),
				new ContextCheckerOption ("b",  "b desc", "--b+", "--b+",  2),
				new ContextCheckerOption ("c=", "c desc", "--c",  "C",     3),
				new ContextCheckerOption ("d",  "d desc", "/d-",  null,    4),
			};
			Assert.AreEqual (p.Count, 4);
			p.Parse (_("/a", "a-val", "--b+", "--c=C", "/d-"));
		}

		[Test]
		public void DefaultHandler ()
		{
			var extra = new List<string> ();
			var p = new OptionSet () {
				{ "<>", v => extra.Add (v) },
			};
			var e = p.Parse (_("-a", "b", "--c=D", "E"));
			Assert.AreEqual (e.Count, 0);
			Assert.AreEqual (extra.Count, 4);
			Assert.AreEqual (extra [0], "-a");
			Assert.AreEqual (extra [1], "b");
			Assert.AreEqual (extra [2], "--c=D");
			Assert.AreEqual (extra [3], "E");
		}

		[Test]
		public void MixedDefaultHandler ()
		{
			var tests = new List<string> ();
			var p = new OptionSet () {
				{ "t|<>=", v => tests.Add (v) },
			};
			var e = p.Parse (_("-tA", "-t:B", "-t=C", "D", "--E=F"));
			Assert.AreEqual (e.Count, 0);
			Assert.AreEqual (tests.Count, 5);
			Assert.AreEqual (tests [0], "A");
			Assert.AreEqual (tests [1], "B");
			Assert.AreEqual (tests [2], "C");
			Assert.AreEqual (tests [3], "D");
			Assert.AreEqual (tests [4], "--E=F");
		}

		[Test]
		public void DefaultHandlerRuns ()
		{
			var formats = new Dictionary<string, List<string>> ();
			string format = "foo";
			var p = new OptionSet () {
				{ "f|format=", v => format = v },
				{ "<>", 
					v => {
						List<string> f;
						if (!formats.TryGetValue (format, out f)) {
							f = new List<string> ();
							formats.Add (format, f);
						}
						f.Add (v);
				} },
			};
			var e = p.Parse (_("a", "b", "-fbar", "c", "d", "--format=baz", "e", "f"));
			Assert.AreEqual (e.Count, 0);
			Assert.AreEqual (formats.Count, 3);
			Assert.AreEqual (formats ["foo"].Count, 2);
			Assert.AreEqual (formats ["foo"][0], "a");
			Assert.AreEqual (formats ["foo"][1], "b");
			Assert.AreEqual (formats ["bar"].Count, 2);
			Assert.AreEqual (formats ["bar"][0], "c");
			Assert.AreEqual (formats ["bar"][1], "d");
			Assert.AreEqual (formats ["baz"].Count, 2);
			Assert.AreEqual (formats ["baz"][0], "e");
			Assert.AreEqual (formats ["baz"][1], "f");
		}
	}
}

