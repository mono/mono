using System;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions
{
	[TestFixture]
	public class RegexResultTests
	{

		struct testcase
		{
			public string original, pattern, replacement, expected;
			public testcase (string o, string p, string r, string e) {
				original = o;
				pattern = p;
				replacement = r;
				expected = e;
			}
			public void Execute () {
				string result;
				try {
					Match match = Regex.Match (original, pattern);
					result = match.Result (replacement);
				}
				catch (Exception e) {
					result = "Error.";
				}
				Assert.AreEqual (expected, result, "rr#: {0} ~ s,{1},{2},",
					original, pattern, replacement);

			}
		}
		static testcase [] tests = {
			//	original 	pattern			replacement		expected
			new testcase ("F2345678910L71",	@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", 	"${S}$11$1", "Error." 	),//0
			new testcase ("F2345678910LL1",	@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", 	"${S}$11$1", "${S}LF" 	),//1
			new testcase ("texts",	"(?<foo>e)(x)", 	"${foo}$1$2$&",		"exeex" 	),//2
			new testcase ("texts",	"(?<foo>e)(x)", 	"${foo}$1$2$_",		"exetexts" 	),//3
			new testcase ("texts",	"(?<foo>e)(x)", 	"${foo}$1$2$`",		"exet" 	),//4
			new testcase ("texts",	"(?<foo>e)(x)", 	"${foo}$1$2$'",		"exets" 	),//5
			new testcase ("text",	"x",			"y",			"y"		),//6
			new testcase ("text",	"x",			"$",			"$"		),//7
			new testcase ("text",	"x",			"$1",			"$1"		),//8
			new testcase ("text",	"x",			"${1}",			"${1}"	),//9
			new testcase ("text",	"x",			"$5",			"$5"		),//10
			new testcase ("te(x)t",	"x",			"$5",			"$5"	),//11
			new testcase ("text",	"x",			"${5",			"${5"	),//12
			new testcase ("text",	"x",			"${foo",		"${foo"	),//13
			new testcase ("text",	"(x)",			"$5",			"$5"		),//14
			new testcase ("text",	"(x)",			"$1",			"x"		),//15
			new testcase ("text",	"e(x)",			"$1",			"x"		),//16
			new testcase ("text",	"e(x)",			"$5",			"$5"		),//17
			new testcase ("text",	"e(x)",			"$4",			"$4"		),//18
			new testcase ("text",	"e(x)",			"$3",			"$3"		),//19
			new testcase ("text",	"e(x)",			"${1}",			"x"		),//20
			new testcase ("text",	"e(x)",			"${3}",			"${3}"	),//21
			new testcase ("text",	"e(x)",			"${1}${3}",		"x${3}"	),//22
			new testcase ("text",	"e(x)",			"${1}${name}",		"x${name}"	),//23
			new testcase ("text",	"e(?<foo>x)",		"${1}${name}",		"x${name}"	),//24
			new testcase ("text",	"e(?<foo>x)",		"${1}${foo}",		"xx"		),//25
			new testcase ("text",	"e(?<foo>x)",		"${goll}${foo}",	"${goll}x"	),//26
			new testcase ("text",	"e(?<foo>x)",		"${goll${foo}",		"${gollx"	),//27
			new testcase ("text",	"e(?<foo>x)",		"${goll${foo}}",	"${gollx}"	),//28
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"${foo}}"	),//29
			new testcase ("text",	"e(?<foo>x)",		"${${foo}}",		"${x}"	),//30
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"${foo}}"	),//31
			new testcase ("text",	"e(?<foo>x)",		"$${bfoo}}",		"${bfoo}}"	),//32
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"${foo}}"	),//33
			new testcase ("text",	"e(?<foo>x)",		"$${foo}",		"${foo}"	),//34
			new testcase ("text",	"e(?<foo>x)",		"$$",			"$"		),//35
			new testcase ("text",	"(?<foo>e)(?<foo>x)", 	"${foo}$1$2",		"xx$2" 	),//36
			new testcase ("text",	"(e)(?<foo>x)", 	"${foo}$1$2",		"xex" 	),//37
			new testcase ("text",	"(?<foo>e)(x)", 	"${foo}$1$2",		"exe" 	),//38
			new testcase ("text",	"(e)(?<foo>x)", 	"${foo}$1$2$+",		"xexx" 	),//39
			new testcase ("text",	"(?<foo>e)(x)", 	"${foo}$1$2$+",		"exee" 	),//40
			new testcase ("314 1592 65358",		@"\d\d\d\d|\d\d\d", "a",	"a"	),//41
			new testcase ("2 314 1592 65358", 	@"\d\d\d\d|\d\d\d", "a",	"a"	),//42
			new testcase ("<i>am not</i>", 		"<(.+?)>", 	"[$0:$1]",	"[<i>:i]"),//43
			new testcase ("F2345678910L71",	@"(F)(2)(3)(4)(5)(6)(?<S>7)(8)(9)(10)(L)\11", 	"${S}$11$1", "77F" 	),//44
			new testcase ("a", "a", @"\\", @"\\"), // bug #317092 //45
		};

		[Test]
		public void ResultTest_000 () { tests [0].Execute (); }
		[Test]
		public void ResultTest_001 () { tests [1].Execute (); }
		[Test]
		public void ResultTest_002 () { tests [2].Execute (); }
		[Test]
		public void ResultTest_003 () { tests [3].Execute (); }
		[Test]
		public void ResultTest_004 () { tests [4].Execute (); }
		[Test]
		public void ResultTest_005 () { tests [5].Execute (); }
		[Test]
		public void ResultTest_006 () { tests [6].Execute (); }
		[Test]
		public void ResultTest_007 () { tests [7].Execute (); }
		[Test]
		public void ResultTest_008 () { tests [8].Execute (); }
		[Test]
		public void ResultTest_009 () { tests [9].Execute (); }
		[Test]
		public void ResultTest_010 () { tests [10].Execute (); }
		[Test]
		public void ResultTest_011 () { tests [11].Execute (); }
		[Test]
		public void ResultTest_012 () { tests [12].Execute (); }
		[Test]
		public void ResultTest_013 () { tests [13].Execute (); }
		[Test]
		public void ResultTest_014 () { tests [14].Execute (); }
		[Test]
		public void ResultTest_015 () { tests [15].Execute (); }
		[Test]
		public void ResultTest_016 () { tests [16].Execute (); }
		[Test]
		public void ResultTest_017 () { tests [17].Execute (); }
		[Test]
		public void ResultTest_018 () { tests [18].Execute (); }
		[Test]
		public void ResultTest_019 () { tests [19].Execute (); }
		[Test]
		public void ResultTest_020 () { tests [20].Execute (); }
		[Test]
		public void ResultTest_021 () { tests [21].Execute (); }
		[Test]
		public void ResultTest_022 () { tests [22].Execute (); }
		[Test]
		public void ResultTest_023 () { tests [23].Execute (); }
		[Test]
		public void ResultTest_024 () { tests [24].Execute (); }
		[Test]
		public void ResultTest_025 () { tests [25].Execute (); }
		[Test]
		public void ResultTest_026 () { tests [26].Execute (); }
		[Test]
		public void ResultTest_027 () { tests [27].Execute (); }
		[Test]
		public void ResultTest_028 () { tests [28].Execute (); }
		[Test]
		public void ResultTest_029 () { tests [29].Execute (); }
		[Test]
		public void ResultTest_030 () { tests [30].Execute (); }
		[Test]
		public void ResultTest_031 () { tests [31].Execute (); }
		[Test]
		public void ResultTest_032 () { tests [32].Execute (); }
		[Test]
		public void ResultTest_033 () { tests [33].Execute (); }
		[Test]
		public void ResultTest_034 () { tests [34].Execute (); }
		[Test]
		public void ResultTest_035 () { tests [35].Execute (); }
		[Test]
		public void ResultTest_036 () { tests [36].Execute (); }
		[Test]
		public void ResultTest_037 () { tests [37].Execute (); }
		[Test]
		public void ResultTest_038 () { tests [38].Execute (); }
		[Test]
		public void ResultTest_039 () { tests [39].Execute (); }
		[Test]
		public void ResultTest_040 () { tests [40].Execute (); }
		[Test]
		public void ResultTest_041 () { tests [41].Execute (); }
		[Test]
		public void ResultTest_042 () { tests [42].Execute (); }
		[Test]
		public void ResultTest_043 () { tests [43].Execute (); }
		[Test]
		[Category("NotWorking")]
		public void ResultTest_044 () { tests [44].Execute (); }
		[Test]
		public void ResultTest_045 () { tests [45].Execute (); }
	}
}
