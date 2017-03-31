//
// Autogen RepeatInfoTest.auto.cs
//
// Authors:
//	Ben Maurer <bmaurer@novell.com>
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;
using System.IO;
using System.Text;

using MonoTests.Helpers;

class X {
	static string GetLayoutName (RepeatLayout layout)
	{
		switch (layout) {
			case RepeatLayout.Flow:
				return "flow";

			case RepeatLayout.Table:
				return "tbl";
			case RepeatLayout.OrderedList:
				return "ol";

			case RepeatLayout.UnorderedList:
				return "ul";
			default:
				throw new InvalidOperationException ("Unsupported layout value: " + layout);
		}
	}

	static void Main ()
	{
		bool isMono = Type.GetType ("Mono.Runtime", false) != null;

		Console.WriteLine (@"
// THIS IS AUTOGENERATED DO NOT EDIT
//
// Generated on {0} runtime v{1}
//
// Authors:
//    Ben Maurer (bmaurer@novell.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// ""Software""), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
using MonoTests.Helpers;

namespace MonoTests.System.Web.UI.WebControls {{
[TestFixture]
[SetCulture (""en-us"")]
public class RepeatInfo_Autogen {{
", isMono ? "Mono" : "Microsoft .NET", Environment.Version);		
		int num = 0;
		int [][] combos = {
			new int [] {0, 0},
			new int [] {0, 1},
			new int [] {0, 2},
			new int [] {0, 5},
			new int [] {1, 0},
			new int [] {1, 5},
			new int [] {2, 4},
			new int [] {2, 7},
			new int [] {3, 9},
			new int [] {3, 7}
		};
		int ntests = 1 << 7;
		int lcount = -1;
		RepeatDirection d;
		RepeatLayout l;
		bool oti, hdr, ftr, sep;
		StringBuilder sb = new StringBuilder ();

		for (int i = 0; i < ntests; i ++) {
			d = (RepeatDirection) (i & (1 << 0));
			if ((i % 2) == 0)
				lcount++;
			l = (RepeatLayout) (lcount % 4);
			oti = (i & (1 << 3)) == 0;
			hdr = (i & (1 << 4)) == 0;
			ftr = (i & (1 << 5)) == 0;
			sep = (i & (1 << 6)) == 0;

			foreach (int [] col_cnt in combos) {
				string nm = String.Format ("RepeatInfo_{0}cols_{1}itms_{2}_{3}{4}{5}{6}{7}",
						col_cnt [0],
						col_cnt [1],
						d == RepeatDirection.Vertical ? "vert" : "horiz",
						GetLayoutName (l),
						oti ? "_otrtblimp" : String.Empty,
						hdr ? "_hdr" : String.Empty,
						ftr ? "_ftr" : String.Empty,
						sep ? "_sep" : String.Empty);
				sb.Length = 0;
				sb.AppendFormat (@"
	public void {0} ()
	{{
		// cols              : {1}
		// cnt               : {2}
		// RepeatDirection   : {3}
		// RepeatLayout      : {4}
		// OuterTableImplied : {5}
		// Header            : {6}
		// Footer            : {7}
		// Separator         : {8}
",
						nm,
						col_cnt [0],
						col_cnt [1],
						d,
						l,
						oti,
						hdr,
						ftr,
						sep
					);
				try {
					string exp = RepeatInfoUser.DoTest (col_cnt [0], col_cnt [1], d, l, oti, hdr, ftr, sep).Replace (@"""", @"""""");
					BuildTestCode (sb, null, col_cnt [0], col_cnt [1], d, l, oti, hdr, ftr, sep, exp, num++);
				} catch (Exception ex) {
					BuildTestCode (sb, ex, col_cnt [0], col_cnt [1], d, l, oti, hdr, ftr, sep, null, num++);
				}
				Console.WriteLine (sb.ToString ());
			}
		}
		Console.WriteLine (@"
}
}");
	}

	static void BuildTestCode (StringBuilder sb, Exception ex, int cols, int cnt, RepeatDirection d, RepeatLayout l, bool oti, bool hdr, bool ftr, bool sep, string exp, int num)
	{
		if (ex == null) {
			sb.Insert (0, "\t[Test]");
		} else {
			sb.Insert (0, String.Format ("\t[ExpectedException (typeof (global::{0}))]", ex.GetType ().FullName));
			sb.Insert (0, "\t[Test]\n");
		}

		sb.AppendFormat (@"
		string v = global::MonoTests.Helpers.RepeatInfoUser.DoTest ({0}, {1}, RepeatDirection.{2}, RepeatLayout.{3}, {4}, {5}, {6}, {7});
",
			cols,
			cnt,
			d,
			l,
			oti ? "true" : "false",
			hdr ? "true" : "false",
			ftr ? "true" : "false",
			sep ? "true" : "false");
		if (ex == null) {
			sb.AppendFormat (@"		string exp = @""{0}"";
		Assert.AreEqual (exp.Replace (""\r\n"", ""\n""), v, ""#{1}"");
	}}
", exp, num);
		} else {
			sb.AppendFormat (@"
		// Exception: {0} (""{1}"")
	}}
", ex.GetType ().FullName, ex.Message);
		}
	}
}
