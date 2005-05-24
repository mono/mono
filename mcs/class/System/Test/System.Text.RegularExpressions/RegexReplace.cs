//
// RegexReplace.cs
//
// Author:
//	Raja R Harinath <rharinath@novell.com>
//
// (C) 2005, Novell Inc.

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {

	[TestFixture]
	public class RegexReplaceTest {
		struct testcase { 
			public string original, pattern, replacement, expected;
			public testcase (string o, string p, string r, string e)
			{
				original = o;
				pattern = p;
				replacement = r;
				expected = e;
			}
		}
		
		static testcase [] tests = {
			//	original 	pattern			replacement		expected
			new testcase ("text",	"x",			"y",			"teyt"		),
			new testcase ("text",	"x",			"$",			"te$t"		),
			new testcase ("text",	"x",			"$1",			"te$1t"		),
			new testcase ("text",	"x",			"${1}",			"te${1}t"	),
			new testcase ("text",	"x",			"$5",			"te$5t"		),
			new testcase ("te(x)t",	"x",			"$5",			"te($5)t"	),
			new testcase ("text",	"x",			"${5",			"te${5t"	),
			new testcase ("text",	"x",			"${foo",		"te${foot"	),
			new testcase ("text",	"(x)",			"$5",			"te$5t"		),
			new testcase ("text",	"(x)",			"$1",			"text"		),
			new testcase ("text",	"e(x)",			"$1",			"txt"		),
			new testcase ("text",	"e(x)",			"$5",			"t$5t"		),
			new testcase ("text",	"e(x)",			"$4",			"t$4t"		),
			new testcase ("text",	"e(x)",			"$3",			"t$3t"		),
			new testcase ("text",	"e(x)",			"${1}",			"txt"		),
			new testcase ("text",	"e(x)",			"${3}",			"t${3}t"	),
			new testcase ("text",	"e(x)",			"${1}${3}",		"tx${3}t"	),
			new testcase ("text",	"e(x)",			"${1}${name}",		"tx${name}t"	),
			new testcase ("text",	"e(?<foo>x)",		"${1}${name}",		"tx${name}t"	),
			new testcase ("text",	"e(?<foo>x)",		"${1}${foo}",		"txxt"		),
			new testcase ("text",	"e(?<foo>x)",		"${goll}${foo}",	"t${goll}xt"	),
			new testcase ("text",	"e(?<foo>x)",		"${goll${foo}",		"t${gollxt"	),
			new testcase ("text",	"e(?<foo>x)",		"${goll${foo}}",	"t${gollx}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"t${foo}}t"	),
			new testcase ("text",	"e(?<foo>x)",		"${${foo}}",		"t${x}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"t${foo}}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$${bfoo}}",		"t${bfoo}}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$${foo}}",		"t${foo}}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$${foo}",		"t${foo}t"	),
			new testcase ("text",	"e(?<foo>x)",		"$$",			"t$t"		),
			new testcase ("text",	"(?<foo>e)(?<foo>x)", 	"${foo}$1$2",		"txx$2t" 	),
			new testcase ("text",	"(e)(?<foo>x)", 	"${foo}$1$2",		"txext" 	),
			new testcase ("text",	"(?<foo>e)(x)", 	"${foo}$1$2",		"texet" 	),
			new testcase ("text",	"(e)(?<foo>x)", 	"${foo}$1$2$+",		"txexxt" 	),
			new testcase ("text",	"(?<foo>e)(x)", 	"${foo}$1$2$+",		"texeet" 	),
		};

		[Test]
		public void ReplaceTests ()
		{
			string result;
			int i = 0;
			foreach (testcase test in tests) {
				try {
					result = Regex.Replace (test.original, test.pattern, test.replacement);
					Assert.AreEqual (result, test.expected, "rr#{0}: {1} ~ s,{2},{3},", i,
							 test.original, test.pattern, test.replacement);
				} catch (Exception e) {
					Assert.Fail ("rr#{0}: Exception thrown", i);
				}
				++i;
			}
		}
	}
}
