using System;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions
{
	[TestFixture]
	public class RegexMatchTests
	{
		static RegexTrial [] trials = new RegexTrial [] {
			new RegexTrial (@"(a)(b)(c)", RegexOptions.ExplicitCapture, "abc", "Pass. Group[0]=(0,3)"),//0
			new RegexTrial (@"(a)(?<1>b)(c)", RegexOptions.ExplicitCapture, "abc", "Pass. Group[0]=(0,3) Group[1]=(1,1)"),//1
			new RegexTrial (@"(a)(?<2>b)(c)", RegexOptions.None, "abc", "Pass. Group[0]=(0,3) Group[1]=(0,1) Group[1]=(1,1)(2,1)"),//2
			new RegexTrial (@"(a)(?<foo>b)(c)", RegexOptions.ExplicitCapture, "abc", "Pass. Group[0]=(0,3) Group[1]=(1,1)"),//3
			new RegexTrial (@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", RegexOptions.None, "F2345678910LL", "Pass. Group[0]=(0,13)"//4
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(3,1)"
			+ " Group[5]=(4,1)"
			+ " Group[6]=(5,1)"
			+ " Group[7]=(6,1)"
			+ " Group[8]=(7,1)"
			+ " Group[9]=(8,1)"
			+ " Group[10]=(9,2)"
			+ " Group[11]=(11,1)"
			),
			new RegexTrial (@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", RegexOptions.ExplicitCapture, "F2345678910LL", "Fail."),//5
			new RegexTrial (@"(F)(2)(3)(4)(5)(6)(?<S>7)(8)(9)(10)(L)\1", RegexOptions.None, "F2345678910L71", "Fail."),//6
			new RegexTrial (@"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", RegexOptions.None, "F2345678910LF1", "Fail."),//7
			new RegexTrial (@"(F)(2)(3)(4)(5)(6)(?<S>7)(8)(9)(10)(L)\11", RegexOptions.None, "F2345678910L71", "Pass."//8
			+ "	Group[0]=(0,13)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(3,1)"
			+ " Group[5]=(4,1)"
			+ " Group[6]=(5,1)"
			+ " Group[7]=(7,1)"
			+ " Group[8]=(8,1)"
			+ " Group[9]=(9,2)"
			+ " Group[10]=(11,1)"
			+ " Group[11]=(6,1)"
			),
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)\10", RegexOptions.None, "F2345678910L71", "Pass."//9
			+ "	Group[0]=(0,13)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(4,1)"
			+ " Group[5]=(5,1)"
			+ " Group[6]=(7,1)"
			+ " Group[7]=(8,1)"
			+ " Group[8]=(9,2)"
			+ " Group[9]=(11,1)"
			+ " Group[10]=(3,1)(6,1)"
			),
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)\10", RegexOptions.ExplicitCapture, "F2345678910L70", "Fail."),//10
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)\1", RegexOptions.ExplicitCapture, "F2345678910L70", "Pass. Group[0]=(0,13) Group[1]=(3,1)(6,1)"),//11
			new RegexTrial (@"(?n:(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)\1)", RegexOptions.None, "F2345678910L70", "Pass. Group[0]=(0,13) Group[1]=(3,1)(6,1)"),//12
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)(?(10)\10)", RegexOptions.None, "F2345678910L70","Pass."//13
			+ "	Group[0]=(0,13)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(4,1)"
			+ " Group[5]=(5,1)"
			+ " Group[6]=(7,1)"
			+ " Group[7]=(8,1)"
			+ " Group[8]=(9,2)"
			+ " Group[9]=(11,1)"
			+ " Group[10]=(3,1)(6,1)"
			),
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)(?(S)|\10)", RegexOptions.None, "F2345678910L70","Pass."//14
			+ "	Group[0]=(0,12)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(4,1)"
			+ " Group[5]=(5,1)"
			+ " Group[6]=(7,1)"
			+ " Group[7]=(8,1)"
			+ " Group[8]=(9,2)"
			+ " Group[9]=(11,1)"
			+ " Group[10]=(3,1)(6,1)"
			),
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)(?(7)|\10)", RegexOptions.None, "F2345678910L70","Pass."//15
			+ "	Group[0]=(0,12)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(4,1)"
			+ " Group[5]=(5,1)"
			+ " Group[6]=(7,1)"
			+ " Group[7]=(8,1)"
			+ " Group[8]=(9,2)"
			+ " Group[9]=(11,1)"
			+ " Group[10]=(3,1)(6,1)"
			),
			new RegexTrial (@"(F)(2)(3)(?<S>4)(5)(6)(?'S'7)(8)(9)(10)(L)(?(K)|\10)", RegexOptions.None, "F2345678910L70","Pass."//16
			+ "	Group[0]=(0,13)"
			+ " Group[1]=(0,1)"
			+ " Group[2]=(1,1)"
			+ " Group[3]=(2,1)"
			+ " Group[4]=(4,1)"
			+ " Group[5]=(5,1)"
			+ " Group[6]=(7,1)"
			+ " Group[7]=(8,1)"
			+ " Group[8]=(9,2)"
			+ " Group[9]=(11,1)"
			+ " Group[10]=(3,1)(6,1)"
			),
			new RegexTrial (@"\P{IsHebrew}", RegexOptions.None, "Fì", "Pass. Group[0]=(0,1)"),//17
			new RegexTrial (@"\p{IsHebrew}", RegexOptions.None, "Fì", "Pass. Group[0]=(1,1)"),//18
			new RegexTrial (@"(?<=a+)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//19
			new RegexTrial (@"(?<=a*)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(0,4)"),//20
			new RegexTrial (@"(?<=a{1,5})(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//21
			new RegexTrial (@"(?<=a{1})(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//22
			new RegexTrial (@"(?<=a{1,})(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//23
			new RegexTrial (@"(?<=a+?)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//24
			new RegexTrial (@"(?<=a*?)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(0,4)"),//25
			new RegexTrial (@"(?<=a{1,5}?)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//26
			new RegexTrial (@"(?<=a{1}?)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//27
			new RegexTrial (@"(?<=a{1}?)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(1,3)"),//28
			new RegexTrial (@"(?<!a+)(?:a)*bc", RegexOptions.None, "aabc", "Pass. Group[0]=(0,4)"),//29
			new RegexTrial (@"(?<!a*)(?:a)*bc", RegexOptions.None, "aabc", "Fail."),//30
			new RegexTrial (@"abc*(?=c*)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//31
			new RegexTrial (@"abc*(?=c+)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//32
			new RegexTrial (@"abc*(?=c{1})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//33
			new RegexTrial (@"abc*(?=c{1,5})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//34
			new RegexTrial (@"abc*(?=c{1,})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//35
			new RegexTrial (@"abc*(?=c*?)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//36
			new RegexTrial (@"abc*(?=c+?)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//37
			new RegexTrial (@"abc*(?=c{1}?)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//38
			new RegexTrial (@"abc*(?=c{1,5}?)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//39
			new RegexTrial (@"abc*(?=c{1,}?)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,3)"),//40
			new RegexTrial (@"abc*?(?=c*)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,2)"),//41
			new RegexTrial (@"abc*?(?=c+)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,2)"),//42
			new RegexTrial (@"abc*?(?=c{1})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,2)"),//43
			new RegexTrial (@"abc*?(?=c{1,5})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,2)"),//44
			new RegexTrial (@"abc*?(?=c{1,})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,2)"),//45
			new RegexTrial (@"abc*(?!c*)", RegexOptions.None, "abcc", "Fail."),//46
			new RegexTrial (@"abc*(?!c+)", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//47
			new RegexTrial (@"abc*(?!c{1})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//48
			new RegexTrial (@"abc*(?!c{1,5})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//49
			new RegexTrial (@"abc*(?!c{1,})", RegexOptions.None, "abcc", "Pass. Group[0]=(0,4)"),//50
			new RegexTrial (@"(a)(?<1>b)(?'1'c)", RegexOptions.ExplicitCapture, "abc", "Pass. Group[0]=(0,3) Group[1]=(1,1)(2,1)"),//51
			new RegexTrial (@"(?>a*).", RegexOptions.ExplicitCapture, "aaaa", "Fail."),//52

			new RegexTrial (@"(?<ab>ab)c\1", RegexOptions.None, "abcabc", "Pass. Group[0]=(0,5) Group[1]=(0,2)"),//53
			new RegexTrial (@"\1", RegexOptions.ECMAScript, "-", "Fail."),//54
			new RegexTrial (@"\2", RegexOptions.ECMAScript, "-", "Fail."),//55
			new RegexTrial (@"(a)|\2", RegexOptions.ECMAScript, "-", "Fail."),//56
			new RegexTrial (@"\4400", RegexOptions.None, "asdf 012", "Pass. Group[0]=(4,2)"),//57
			new RegexTrial (@"\4400", RegexOptions.ECMAScript, "asdf 012", "Fail."),//58
			new RegexTrial (@"\4400", RegexOptions.None, "asdf$0012", "Fail."),//59
			new RegexTrial (@"\4400", RegexOptions.ECMAScript, "asdf$0012", "Pass. Group[0]=(4,3)"),//60
			new RegexTrial (@"(?<2>ab)(?<c>c)(?<d>d)", RegexOptions.None, "abcd", "Pass. Group[0]=(0,4) Group[1]=(2,1) Group[2]=(0,2) Group[3]=(3,1)"),// 61
			new RegexTrial (@"(?<1>ab)(c)", RegexOptions.None, "abc", "Pass. Group[0]=(0,3) Group[1]=(0,2)(2,1)"),//62
			new RegexTrial (@"(?<44>a)", RegexOptions.None, "a", "Pass. Group[0]=(0,1) Group[44]=(0,1)"),//63
			new RegexTrial (@"(?<44>a)(?<8>b)", RegexOptions.None, "ab", "Pass. Group[0]=(0,2) Group[8]=(1,1) Group[44]=(0,1)"),//64
			new RegexTrial (@"(?<44>a)(?<8>b)(?<1>c)(d)", RegexOptions.None, "abcd", "Pass. Group[0]=(0,4) Group[1]=(2,1)(3,1) Group[8]=(1,1) Group[44]=(0,1)"),//65
			new RegexTrial (@"(?<44>a)(?<44>b)", RegexOptions.None, "ab", "Pass. Group[0]=(0,2) Group[44]=(0,1)(1,1)"),//66
			new RegexTrial (@"(?<44>a)\440", RegexOptions.None, "a ", "Pass. Group[0]=(0,2) Group[44]=(0,1)"),//67
			new RegexTrial (@"(?<44>a)\440", RegexOptions.ECMAScript, "a ", "Fail."),//68
			new RegexTrial (@"(?<44>a)\440", RegexOptions.None, "aa0", "Fail."),//69
			new RegexTrial (@"(?<44>a)\440", RegexOptions.ECMAScript, "aa0", "Pass. Group[0]=(0,3) Group[44]=(0,1)"),//70
		};

		[Test]
		public void RegexJvmTrial0000 ()
		{
			trials [0].Execute ();
		}

		[Test]
		public void RegexJvmTrial0001 ()
		{
			trials [1].Execute ();
		}

		[Test]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void RegexJvmTrial0002 ()
		{
			trials [2].Execute ();
		}

		[Test]
		public void RegexJvmTrial0003 ()
		{
			trials [3].Execute ();
		}

		[Test]
		public void RegexJvmTrial0004 ()
		{
			trials [4].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		public void RegexJvmTrial0005 ()
		{
			trials [5].Execute ();
		}

		[Test]
		public void RegexJvmTrial0006 ()
		{
			trials [6].Execute ();
		}

		[Test]
		public void RegexJvmTrial0007 ()
		{
			trials [7].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0008 ()
		{
			trials [8].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0009 ()
		{
			trials [9].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		public void RegexJvmTrial0010 ()
		{
			trials [10].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		public void RegexJvmTrial0011 ()
		{
			trials [11].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		public void RegexJvmTrial0012 ()
		{
			trials [12].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0013 ()
		{
			trials [13].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0014 ()
		{
			trials [14].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0015 ()
		{
			trials [15].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0016 ()
		{
			trials [16].Execute ();
		}

		[Test]
		public void RegexJvmTrial0017 ()
		{
			trials [17].Execute ();
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]
		public void RegexJvmTrial0018 ()
		{
			trials [18].Execute ();
		}
		
		[Test]	public void RegexJvmTrial0019 () { trials [19].Execute (); }
		[Test]	public void RegexJvmTrial0020 () { trials [20].Execute (); }
		[Test]	public void RegexJvmTrial0021 () { trials [21].Execute (); }
		[Test]	public void RegexJvmTrial0022 () { trials [22].Execute (); }
		[Test]	public void RegexJvmTrial0023 () { trials [23].Execute (); }
		[Test]	public void RegexJvmTrial0024 () { trials [24].Execute (); }
		[Test]	public void RegexJvmTrial0025 () { trials [25].Execute (); }
		[Test]	public void RegexJvmTrial0026 () { trials [26].Execute (); }
		[Test]	public void RegexJvmTrial0027 () { trials [27].Execute (); }
		[Test]	public void RegexJvmTrial0028 () { trials [28].Execute (); }
		[Test]	public void RegexJvmTrial0029 () { trials [29].Execute (); }
		[Test]	public void RegexJvmTrial0030 () { trials [30].Execute (); }
		[Test]	public void RegexJvmTrial0031 () { trials [31].Execute (); }
		[Test]	public void RegexJvmTrial0032 () { trials [32].Execute (); }
		[Test]	public void RegexJvmTrial0033 () { trials [33].Execute (); }
		[Test]	public void RegexJvmTrial0034 () { trials [34].Execute (); }
		[Test]	public void RegexJvmTrial0035 () { trials [35].Execute (); }
		[Test]	public void RegexJvmTrial0036 () { trials [36].Execute (); }
		[Test]	public void RegexJvmTrial0037 () { trials [37].Execute (); }
		[Test]	public void RegexJvmTrial0038 () { trials [38].Execute (); }
		[Test]	public void RegexJvmTrial0039 () { trials [39].Execute (); }
		[Test]	public void RegexJvmTrial0040 () { trials [40].Execute (); }
		[Test]	public void RegexJvmTrial0041 () { trials [41].Execute (); }
		[Test]	public void RegexJvmTrial0042 () { trials [42].Execute (); }
		[Test]	public void RegexJvmTrial0043 () { trials [43].Execute (); }
		[Test]	public void RegexJvmTrial0044 () { trials [44].Execute (); }
		[Test]	public void RegexJvmTrial0045 () { trials [45].Execute (); }
		[Test]	public void RegexJvmTrial0046 () { trials [46].Execute (); }
		[Test]	public void RegexJvmTrial0047 () { trials [47].Execute (); }
		[Test]	public void RegexJvmTrial0048 () { trials [48].Execute (); }
		[Test]	public void RegexJvmTrial0049 () { trials [49].Execute (); }
		[Test]	public void RegexJvmTrial0050 () { trials [50].Execute (); }
		[Test]	public void RegexJvmTrial0051 () { trials [51].Execute (); }
		[Test]	public void RegexJvmTrial0052 () { trials [52].Execute (); }

		[Test]	public void RegexTrial0053 () { trials [53].Execute (); }
		[Test]	public void RegexTrial0054 () { trials [54].Execute (); }
		[Test]	public void RegexTrial0055 () { trials [55].Execute (); }
		[Test]	public void RegexTrial0056 () { trials [56].Execute (); }
		[Test]	public void RegexTrial0057 () { trials [57].Execute (); }
		[Test]	public void RegexTrial0058 () { trials [58].Execute (); }
		[Test]	public void RegexTrial0059 () { trials [59].Execute (); }
		[Test]	public void RegexTrial0060 () { trials [60].Execute (); }
		[Test]	public void RegexTrial0061 () { trials [61].Execute (); }
		[Test]	public void RegexTrial0062 () { trials [62].Execute (); }
		[Test]	public void RegexTrial0063 () { trials [63].Execute (); }
		[Test]	public void RegexTrial0064 () { trials [64].Execute (); }
		[Test]	public void RegexTrial0065 () { trials [65].Execute (); }
		[Test]	public void RegexTrial0066 () { trials [66].Execute (); }
		[Test]	public void RegexTrial0067 () { trials [67].Execute (); }
		[Test]	public void RegexTrial0068 () { trials [68].Execute (); }
		[Test]	public void RegexTrial0069 () { trials [69].Execute (); }
		[Test]	public void RegexTrial0070 () { trials [70].Execute (); }
	}
}
