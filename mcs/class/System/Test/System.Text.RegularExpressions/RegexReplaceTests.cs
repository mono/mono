using System;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions
{
[TestFixture]
public class RegexReplaceTests
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
                result = Regex.Replace (original, pattern, replacement);
            }
            catch (Exception e) {
                result = "Error.";
            }
            Assert.AreEqual (expected, result, "rr#: {0} ~ s,{1},{2},",
                             original, pattern, replacement);
        }
    }

    [Test]
    public void ReplaceTest_001 () {
        new testcase (@"(?(w)a|o)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_002 () {
        new testcase (@"(?(w)|o)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_003 () {
        new testcase (@"(?(w)a)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_004 () {
        new testcase (@"(?(w)a|)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_005 () {
        new testcase (@"(?(w)?|a|o)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_006 () {
        new testcase (@"(?(w)||o)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_007 () {
        new testcase (@"(?(w)(a)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_008 () {
        new testcase (@"(?(w))\a|)", @"\(\?\(\w+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_009 () {
        new testcase (@"(?(2)a|o)", @"\(\?\([^\)]+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_010 () {
        new testcase (@"(?(|)a|o)", @"\(\?\([^\)]+\).*\|?.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_011 () {
        new testcase (@"a\3b", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\(\d+)", @"\5", @"a\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_012 () {
        new testcase (@"\3b", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\(\d+)", @"\5", @"\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_013 () {
        new testcase (@"\\3b", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\(\d+)", @"\5", @"\\3b").Execute ();
    }
    [Test]
    public void ReplaceTest_014 () {
        new testcase (@"\\\3b", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\(\d+)", @"\5", @"\\\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_015 () {
        new testcase (@"a\\\\3b", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\(\d+)", @"\5", @"a\\\\3b").Execute ();
    }
    [Test]
    public void ReplaceTest_016 () {
        new testcase (@"\\\k<g>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k<(\w+)>", @"\5", @"\\\5").Execute ();
    }
    [Test]
    public void ReplaceTest_017 () {
        new testcase (@"a\\\k<g>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k<(\w+)>", @"\5", @"a\\\5").Execute ();
    }
    [Test]
    public void ReplaceTest_018 () {
        new testcase (@"\\\\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w+)'", @"\5", @"\\\\k'g'").Execute ();
    }
    [Test]
    public void ReplaceTest_019 () {
        new testcase (@"a\\\\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w+)'", @"\5", @"a\\\\k'g'").Execute ();
    }
    [Test]
    public void ReplaceTest_020 () {
        new testcase (@"\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w+)'", @"\5", @"\5").Execute ();
    }
    [Test]
    public void ReplaceTest_021 () {
        new testcase (@"(?<n1-n2>)", @"\(\?<[A-Za-z]\w*-[A-Za-z]\w*>.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_022 () {
        new testcase (@"(?'n1-n2'a)", @"\(\?'[A-Za-z]\w*-[A-Za-z]\w*'.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_023 () {
        new testcase (@"\p{Isa}", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\\[pP]\{)Is(?=\w+\})", "In", @"\p{Ina}").Execute ();
    }
    [Test]
    public void ReplaceTest_024 () {
        new testcase (@"\p{Is}", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\\[pP]\{)Is(?=\w+\})", "In", @"\p{Is}").Execute ();
    }
    [Test]
    public void ReplaceTest_025 () {
        new testcase (@"\p{Isa", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\\[pP]\{)Is(?=\w+\})", "In", @"\p{Isa").Execute ();
    }
    [Test]
    public void ReplaceTest_026 () {
        new testcase (@"a(?#|)", @"\(\?#[^\)]*\)", "", "a").Execute ();
    }
    [Test]
    public void ReplaceTest_027 () {
        new testcase (@"(?#|)", @"\(\?#[^\)]*\)", "", "").Execute ();
    }
    [Test]
    public void ReplaceTest_028 () {
        new testcase (@"(?#|)", @"\#[^\n\r]*", "", "(?").Execute ();
    }
    [Test]
    public void ReplaceTest_029 () {
        new testcase (@"(?inm-xs:\#)", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?[imsx]*n[-imsx]*:[^\)]+\)", "r", @"(r").Execute ();
    }
    [Test]
    public void ReplaceTest_030 () {
        new testcase (@"(?ni:())", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?[imsx]*n[-imsx]*:[^\)]+\)", "r", @"(r)").Execute ();
    }
    [Test]
    public void ReplaceTest_031 () {
        new testcase (@"(?x-i:)", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?[imsx]*n[-imsx]*:[^\)]+\)", "r", @"(?x-i:)").Execute ();
    }
    [Test]
    public void ReplaceTest_032 () {
        new testcase (@"(?n:))", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?[imsx]*n[-imsx]*:[^\)]+\)", "r", @"(?n:))").Execute ();
    }
    [Test]
    public void ReplaceTest_033 () {
        new testcase (@"(?<n1>)", @"\(\?<[A-Za-z]\w*>.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_034 () {
        new testcase (@"(?'n1'y)", @"\(\?'[A-Za-z]\w*'.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_035 () {
        new testcase (@"(?<45>y)", @"\(\?<\d+>.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_036 () {
        new testcase (@"(?'7'o)", @"\(\?'\d+'.*\)", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_037 () {
        new testcase (@"\\\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"\\r").Execute ();
    }
    [Test]
    public void ReplaceTest_038 () {
        new testcase (@"a\\\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"a\\r").Execute ();
    }
    [Test]
    public void ReplaceTest_039 () {
        new testcase (@"\\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"\\(").Execute ();
    }
    [Test]
    public void ReplaceTest_040 () {
        new testcase (@"a\\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"a\\(").Execute ();
    }
    [Test]
    public void ReplaceTest_041 () {
        new testcase (@"\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"r").Execute ();
    }
    [Test]
    public void ReplaceTest_042 () {
        new testcase (@"a\(", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\\(", "r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_043 () {
        new testcase (@"?:", @"(?:^\?[:imnsx=!>-]|^\?<[!=])", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_044 () {
        new testcase (@"?<!", @"(?:^\?[:imnsx=!>-]|^\?<[!=])", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_045 () {
        new testcase (@"?-", @"(?:^\?[:imnsx=!>-]|^\?<[!=])", "r", "r").Execute ();
    }
    [Test]
    public void ReplaceTest_046 () {
        new testcase (@"\\(?<n>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?<[A-Za-z]\w*>", "r", @"\\(r").Execute ();
    }
    [Test]
    public void ReplaceTest_047 () {
        new testcase (@"a\\(?'n'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?'[A-Za-z]\w*'", "r", @"a\\(r").Execute ();
    }
    [Test]
    public void ReplaceTest_048 () {
        new testcase (@"\\\\(?<2>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?<\d+>", "r", @"\\\\(r").Execute ();
    }
    [Test]
    public void ReplaceTest_049 () {
        new testcase (@"(?'2'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823}\()\?'\d+'", "r", "(r").Execute ();
    }
    [Test]
    public void ReplaceTest_050 () {
        new testcase (@"\\[\b]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])", @"\\u0008", @"\\[\\u0008]").Execute ();
    }
    [Test]
    public void ReplaceTest_051 () {
        new testcase (@"\\[a\bb]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])", @"\\u0008", @"\\[a\\u0008b]").Execute ();
    }
    [Test]
    public void ReplaceTest_052 () {
        new testcase (@"\[\b]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])", @"\\u0008", @"\[\b]").Execute ();
    }
    [Test]
    public void ReplaceTest_053 () {
        new testcase (@"\\[\\b]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])", @"\\u0008", @"\\[\\b]").Execute ();
    }
    [Test]
    public void ReplaceTest_054 () {
        new testcase (@"\\[\\\b]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\\b(?=[^\[\]]*\])", @"\\u0008", @"\\[\\\\u0008]").Execute ();
    }
    [Test]
    public void ReplaceTest_055 () {
        new testcase (@"[[]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\[(?=[^\[\]]*\])", @"\\[", @"[\\[]").Execute ();
    }
    [Test]
    public void ReplaceTest_056 () {
        new testcase (@"\[[]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\[(?=[^\[\]]*\])", @"\\[", @"\[[]").Execute ();
    }
    [Test]
    public void ReplaceTest_057 () {
        new testcase (@"\\[\\[]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\[(?=[^\[\]]*\])", @"\\[", @"\\[\\\\[]").Execute ();
    }
    [Test]
    public void ReplaceTest_058 () {
        new testcase (@"\\[\\\[]", @"(?<=(?:\A|[^\\])(?:[\\]{2})*(?:\[|\[[^\[\]]*[^\[\]\\])(?:[\\]{2})*)\[(?=[^\[\]]*\])", @"\\[", @"\\[\\\[]").Execute ();
    }
    [Test]
    public void ReplaceTest_059 () {
        new testcase (@"\\{", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\\\\{").Execute ();
    }
    [Test]
    public void ReplaceTest_060 () {
        new testcase (@"\\{", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\\\\{").Execute ();
    }
    [Test]
    public void ReplaceTest_061 () {
        new testcase (@"\\{1,2}", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\\{1,2}").Execute ();
    }
    [Test]
    public void ReplaceTest_062 () {
        new testcase (@"\\{1}", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\\{1}").Execute ();
    }
    [Test]
    public void ReplaceTest_063 () {
        new testcase (@"\\{1,}", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\\{1,}").Execute ();
    }
    [Test]
    public void ReplaceTest_064 () {
        new testcase (@"\{1", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\{(?!\d\d*(,(\d\d*)?)?\})", @"\\{", @"\{1").Execute ();
    }
    [Test]
    public void ReplaceTest_065 () {
        new testcase (@"\\}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"\\\\}").Execute ();
    }
    [Test]
    public void ReplaceTest_066 () {
        new testcase (@"{\\}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"{\\\\}").Execute ();
    }
    [Test]
    public void ReplaceTest_067 () {
        new testcase (@"{1,2}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"{1,2}").Execute ();
    }
    [Test]
    public void ReplaceTest_068 () {
        new testcase (@"\\{1}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"\\{1}").Execute ();
    }
    [Test]
    public void ReplaceTest_069 () {
        new testcase (@"\\{1\}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"\\{1\\\}").Execute ();
    }
    [Test]
    public void ReplaceTest_070 () {
        new testcase (@"\{1}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"\{1\\}").Execute ();
    }
    [Test]
    public void ReplaceTest_071 () {
        new testcase (@"{1,}", @"(?<!(\A|[^\\])(\\{2})*\{\d\d*(,(\d\d*)?)?)\}", @"\\}", @"{1,}").Execute ();
    }
    [Test]
    public void ReplaceTest_072 () {
        new testcase (@"\0", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\\0(?!\d)", @"\\u0000", @"\\u0000").Execute ();
    }
    [Test]
    public void ReplaceTest_073 () {
        new testcase (@"\03", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\\0(?!\d)", @"\\u0000", @"\03").Execute ();
    }
    [Test]
    public void ReplaceTest_074 () {
        new testcase (@"\\0", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\\0(?!\d)", @"\\u0000", @"\\0").Execute ();
    }
    [Test]
    public void ReplaceTest_075 () {
        new testcase (@"\\\0a", @"(?<=(?:\A|[^\\])(?:[\\]{2})*)\\0(?!\d)", @"\\u0000", @"\\\\u0000a").Execute ();
    }
    [Test]
    public void ReplaceTest_076 () {
        new testcase (@"a(?<=b*c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_077 () {
        new testcase (@"a(?<=b*c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_078 () {
        new testcase (@"a(?<=b*c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_079 () {
        new testcase (@"a(?<=b+c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_080 () {
        new testcase (@"a(?<!b+c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_081 () {
        new testcase (@"a(?<!b*c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"ar").Execute ();
    }
    [Test]
    public void ReplaceTest_082 () {
        new testcase (@"(?<!b{1}c))", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"(?<!b{1}c))").Execute ();
    }
    [Test]
    public void ReplaceTest_083 () {
        new testcase (@"(?<!b{1,}c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"r").Execute ();
    }
    [Test]
    public void ReplaceTest_084 () {
        new testcase (@"(?<!b{1,4}c)", @"\(\?\<[=!][^\)]*(?:[\*\+]|\{\d+,\}).*\)", @"r", @"(?<!b{1,4}c)").Execute ();
    }
    [Test]
    public void ReplaceTest_085 () {
        new testcase (@"\p{Isa}", @"(?<=\\[pP]\{)Is(?=\w+\})", "In", @"\p{Ina}").Execute ();
    }
    [Test]
    public void ReplaceTest_086 () {
        new testcase (@"\p{Is}", @"(?<=\\[pP]\{)Is(?=\w+\})", "In", @"\p{Is}").Execute ();
    }
    [Test]
    public void ReplaceTest_087 () {
        new testcase (@"\p{Isa", @"(?<=\\[pP]\{)Is(?=\w+\})", "In", @"\p{Isa").Execute ();
    }
    [Test]
    public void ReplaceTest_088 () {
        new testcase (@"a\3b", @"\\(\d+)", @"\5", @"a\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_089 () {
        new testcase (@"\3b", @"\\(\d+)", @"\5", @"\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_090 () {
        new testcase (@"\\3b", @"(?<=\\)\\(\d)", @"\5", @"\\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_091 () {
        new testcase (@"\\\3b", @"(?<=\\\\)\\(\d)", @"\5", @"\\\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_092 () {
        new testcase (@"a\\\\3b", @"(?<=(\\){0,5})\\(\d)", @"\5", @"a\\\\5b").Execute ();
    }
    [Test]
    public void ReplaceTest_093 () {
        new testcase (@"\\\k<g>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k<(\w)>", @"\5", @"\\\5").Execute ();
    }
    [Test]
    public void ReplaceTest_094 () {
        new testcase (@"a\\\k<g>", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k<(\w)>", @"\5", @"a\\\5").Execute ();
    }
    [Test]
    public void ReplaceTest_095 () {
        new testcase (@"\\\\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w)'", @"\5", @"\\\\k'g'").Execute ();
    }
    [Test]
    public void ReplaceTest_096 () {
        new testcase (@"a\\\\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w)'", @"\5", @"a\\\\k'g'").Execute ();
    }
    [Test]
    public void ReplaceTest_097 () {
        new testcase (@"\k'g'", @"(?<=(?:\A|[^\\])(?:[\\]{2}){0,1073741823})\\k'(\w)'", @"\5", @"\5").Execute ();
    }
    [Test]
    public void ReplaceTest_098 () {
        new testcase (@"\\(?<={1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"\5", @"\5").Execute ();
    }
    [Test]
    public void ReplaceTest_099 () {
        new testcase (@"{1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"r", @"r").Execute ();
    }
    [Test]
    public void ReplaceTest_100 () {
        new testcase (@"({1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"r", @"r").Execute ();
    }
    [Test]
    public void ReplaceTest_101 () {
        new testcase (@"(?{1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"r", @"(?{1}").Execute ();
    }
    [Test]
    public void ReplaceTest_102 () {
        new testcase (@"(?:{1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"r", @"r").Execute ();
    }
    [Test]
    public void ReplaceTest_103 () {
        new testcase (@"\({1}", @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}", @"r", @"\({1}").Execute ();
    }
}
}
