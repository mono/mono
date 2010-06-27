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
    [Flags]
    enum direction {
        LTR = 1,
        RTL = 2,
        Both = 3
    }
    struct testcase {
        public string original, pattern, replacement, expected;
        public direction direction;
        public testcase (string o, string p, string r, string e, direction d)
        {
            original = o;
            pattern = p;
            replacement = r;
            expected = e;
            direction = d;
        }
        public testcase (string o, string p, string r, string e) : this (o, p, r, e, direction.Both) {}
    }

    static testcase [] tests = {
        //	original	pattern			replacement		expected
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
        new testcase ("text",	"(?<foo>e)(?<foo>x)",	"${foo}$1$2",		"txx$2t", direction.LTR),
        new testcase ("text",	"(?<foo>e)(?<foo>x)",	"${foo}$1$2",		"tee$2t", direction.RTL),
        new testcase ("text",	"(e)(?<foo>x)",	"${foo}$1$2",		"txext"	),
        new testcase ("text",	"(?<foo>e)(x)",	"${foo}$1$2",		"texet"	),
        new testcase ("text",	"(e)(?<foo>x)",	"${foo}$1$2$+",		"txexxt"	),
        new testcase ("text",	"(?<foo>e)(x)",	"${foo}$1$2$+",		"texeet"	),
        new testcase ("314 1592 65358",		@"\d\d\d\d|\d\d\d", "a",	"a a a8", direction.LTR),
        new testcase ("314 1592 65358",		@"\d\d\d\d|\d\d\d", "a",	"a a 6a", direction.RTL),
        new testcase ("2 314 1592 65358",	@"\d\d\d\d|\d\d\d", "a",	"2 a a a8", direction.LTR),
        new testcase ("2 314 1592 65358",	@"\d\d\d\d|\d\d\d", "a",	"2 a a 6a", direction.RTL),
        new testcase ("<i>am not</i>",		"<(.+?)>",	"[$0:$1]",	"[<i>:i]am not[</i>:/i]"),
        new testcase ("texts",	"(?<foo>e)(x)",	"${foo}$1$2$_",		"texetextsts"	),
        new testcase ("texts",	"(?<foo>e)(x)",	"${foo}$1$2$`",		"texetts"	),
        new testcase ("texts",	"(?<foo>e)(x)",	"${foo}$1$2$'",		"texetsts"	),
        new testcase ("texts",	"(?<foo>e)(x)",	"${foo}$1$2$&",		"texeexts"	),
        //new testcase ("F2345678910L71",	@"(F)(2)(3)(4)(5)(6)(?<S>7)(8)(9)(10)(L)\11",	"${S}$11$1", "77F1"	),
        new testcase ("F2345678910L71",	@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11",	"${S}$11$1", "F2345678910L71"	),
        new testcase ("F2345678910LL1",	@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11",	"${S}$11$1", "${S}LF1", /*FIXME: this should work in RTL direction too */ direction.LTR),
        new testcase ("a", "a", @"\\", @"\\"), // bug #317092
        new testcase ("a", "^", "x", "xa"), // bug #324390
    };

    [Test]
    public void ReplaceTests ()
    {
        string result = "";
        int i = 0;
        foreach (testcase test in tests) {
            if ((test.direction & direction.LTR) == direction.LTR) {
                try {
                    result = Regex.Replace (test.original, test.pattern, test.replacement);
                } catch (Exception e) {
                    Assert.Fail ("rr#{0}ltr Exception thrown: " + e.ToString (), i);
                }
                Assert.AreEqual (test.expected, result, "rr#{0}ltr: {1} ~ s,{2},{3},", i,
                                 test.original, test.pattern, test.replacement);
            }

            if ((test.direction & direction.RTL) == direction.RTL) {
                try {
                    result = Regex.Replace (test.original, test.pattern, test.replacement, RegexOptions.RightToLeft);
                } catch (Exception e) {
                    Assert.Fail ("rr#{0}rtl Exception thrown: " + e.ToString (), i);
                }
                Assert.AreEqual (test.expected, result, "rr#{0}rtl: {1} ~ s,{2},{3},", i,
                                 test.original, test.pattern, test.replacement);
            }
            ++i;
        }
    }

    static string substitute (Match m)
    {
        switch (m.Value.Substring(2, m.Length - 3)) {
        case "foo":
            return "bar";
        case "hello":
            return "world";
        default:
            return "ERROR";
        }
    }

    static string resolve (string val, Regex r)
    {
        MatchEvaluator ev = new MatchEvaluator (substitute);
        while (r.IsMatch (val))
            val = r.Replace (val, ev);
        return val;
    }

    [Test] // #321036
    public void EvaluatorTests ()
    {
        string test = "@(foo) @(hello)";
        string regex = "\\@\\([^\\)]*?\\)";
        Assert.AreEqual ("bar world", resolve (test, new Regex (regex)), "ltr");
        Assert.AreEqual ("bar world", resolve (test, new Regex (regex, RegexOptions.RightToLeft)), "rtl");
    }
}
}
