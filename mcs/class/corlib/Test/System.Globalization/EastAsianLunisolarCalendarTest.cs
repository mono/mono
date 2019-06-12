// EastAsianLunisolarCalendarTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
//

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


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	[Category ("Calendars")]
	public class EastAsianLunisolarCalendarTest
	{
		static ChineseLunisolarCalendar cn = new ChineseLunisolarCalendar ();
		static JapaneseLunisolarCalendar jp = new JapaneseLunisolarCalendar ();
		static TaiwanLunisolarCalendar tw = new TaiwanLunisolarCalendar ();
		static KoreanLunisolarCalendar kr = new KoreanLunisolarCalendar ();

		[Test]
		public void ToDateTime ()
		{
			Assert.AreEqual (new DateTime (2000, 2, 5), cn.ToDateTime (2000, 1, 1, 0, 0, 0, 0), "cn1");
			Assert.AreEqual (new DateTime (2000, 2, 5), jp.ToDateTime (12, 1, 1, 0, 0, 0, 0, 4), "jp1"); // since 1988 (current epoch)
			Assert.AreEqual (new DateTime (2000, 2, 5), tw.ToDateTime (89, 1, 1, 0, 0, 0, 0), "tw1"); // since 1912 (current epoch)
			Assert.AreEqual (new DateTime (2000, 2, 5), kr.ToDateTime (2000, 1, 1, 0, 0, 0, 0), "kr1");

			Assert.AreEqual (new DateTime (2001, 1, 24), cn.ToDateTime (2001, 1, 1, 0, 0, 0, 0), "cn2");
			Assert.AreEqual (new DateTime (2001, 1, 24), jp.ToDateTime (13, 1, 1, 0, 0, 0, 0, 4), "jp2");
			Assert.AreEqual (new DateTime (2001, 1, 24), tw.ToDateTime (90, 1, 1, 0, 0, 0, 0), "tw2");
			Assert.AreEqual (new DateTime (2001, 1, 24), kr.ToDateTime (2001, 1, 1, 0, 0, 0, 0), "kr2");

			Assert.AreEqual (new DateTime (2002, 2, 12), cn.ToDateTime (2002, 1, 1, 0, 0, 0, 0), "cn3");
			Assert.AreEqual (new DateTime (2002, 2, 12), jp.ToDateTime (14, 1, 1, 0, 0, 0, 0, 4), "jp3");
			Assert.AreEqual (new DateTime (2002, 2, 12), tw.ToDateTime (91, 1, 1, 0, 0, 0, 0), "tw3");
			Assert.AreEqual (new DateTime (2002, 2, 12), kr.ToDateTime (2002, 1, 1, 0, 0, 0, 0), "kr3");

			// actually it is 5th month which is leap, but that
			// does not afffect on resulting DateTime
			Assert.AreEqual (new DateTime (2001, 5, 23), cn.ToDateTime (2001, 5, 1, 0, 0, 0, 0), "cn4");
			Assert.AreEqual (new DateTime (2001, 5, 23), jp.ToDateTime (13, 5, 1, 0, 0, 0, 0, 4), "jp4");
			Assert.AreEqual (new DateTime (2001, 5, 23), tw.ToDateTime (90, 5, 1, 0, 0, 0, 0), "tw4");
			Assert.AreEqual (new DateTime (2001, 5, 23), kr.ToDateTime (2001, 5, 1, 0, 0, 0, 0), "kr4");

			// here the leap month works.
			Assert.AreEqual (new DateTime (2002, 2, 11), cn.ToDateTime (2001, 13, 30, 0, 0, 0, 0), "cn5");
			Assert.AreEqual (new DateTime (2002, 2, 11), jp.ToDateTime (13, 13, 30, 0, 0, 0, 0, 4), "jp5");
			Assert.AreEqual (new DateTime (2002, 2, 11), tw.ToDateTime (90, 13, 30, 0, 0, 0, 0), "tw5");
			Assert.AreEqual (new DateTime (2002, 2, 11), kr.ToDateTime (2001, 13, 30, 0, 0, 0, 0), "kr5");

			Assert.AreEqual (cn.MinSupportedDateTime, cn.ToDateTime (1901, 1, 1, 0, 0, 0, 0), "cn6");
			Assert.AreEqual (jp.MinSupportedDateTime, jp.ToDateTime (35, 1, 1, 0, 0, 0, 0, 3), "jp6"); // 1960-1-1
			Assert.AreEqual (tw.MinSupportedDateTime, tw.ToDateTime (1, 1, 1, 0, 0, 0, 0), "tw6"); // 1912
			Assert.AreEqual (kr.MinSupportedDateTime, kr.ToDateTime (918, 1, 1, 0, 0, 0, 0), "kr6");

			Assert.AreEqual (jp.MinSupportedDateTime, cn.ToDateTime (1960, 1, 1, 0, 0, 0, 0), "jp-cn1");
		}

		[Test]
		public void ToDateTimeOutOfRange ()
		{
			try {
				cn.ToDateTime (1900, 12, 29, 23, 59, 59, 0);
				Assert.Fail ("#cn1");
			} catch (ArgumentOutOfRangeException) {
			}
			try { // non-leap
				cn.ToDateTime (2000, 13, 29, 23, 59, 59, 0);
				Assert.Fail ("#cn2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		bool [] leapYears = new bool [] {
			false, true, false, false, true,
			false, true, false, false, true,
			false, false, true, false, true,
			false, false, true, false, // 19 years rotation
			};

		[Test]
		public void IsLeapYear ()
		{
			//Assert.IsFalse (cn.IsLeapYear (1901), "#cn1901");
			//Assert.AreEqual (0, cn.GetSexagenaryYear (cn.MinSupportedDateTime), "#60cn1");

			for (int i = 0; i < 60; i++)
				Assert.AreEqual (leapYears [i % 19], cn.IsLeapYear (2000 + i), "cn" + i);
			for (int i = 0; i < 48; i++) // only 1-to-61 are allowed
				Assert.AreEqual (leapYears [i % 19], jp.IsLeapYear (12 + i, 4), "jp" + i);
			for (int i = 0; i < 50; i++)
				Assert.AreEqual (leapYears [i % 19], tw.IsLeapYear (89 + i), "tw" + i);
			for (int i = 0; i < 50; i++)
				Assert.AreEqual (leapYears [i % 19], kr.IsLeapYear (2000 + i), "kr" + i);

			// 2033 Rain-Water jieqi (usui) new year day is in
			// the leap month.
			Assert.IsTrue (cn.IsLeapYear (2033), "cn2033");
			Assert.IsFalse (cn.IsLeapYear (2034), "cn2034");
		}

		[Test]
		public void IsLeapMonth ()
		{
			Dictionary<int,int> d = new Dictionary<int,int> ();
			d [2001] = 5;
			d [2004] = 3;
			d [2006] = 8;
			d [2009] = 6;
			d [2012] = 5;
			d [2014] = 10;
			d [2017] = 7;
			d [2020] = 5;
			d [2023] = 3;
			d [2025] = 7; // hmm ...
			d [2028] = 6;
			d [2031] = 4; // hmmmm ...
			d [2033] = 12; // hmmmmmm ...
			d [2036] = 7;
			for (int y = 2000; y < 2038; y++)
				for (int m = 1; m <= 12; m++)
					Assert.AreEqual (d.ContainsKey (y) && d [y] == m, cn.IsLeapMonth (y, m), "cn" + y + "/" + m);

			d = new Dictionary<int,int> ();
			d [90] = 5;
			d [93] = 3;
			d [95] = 8;
			d [98] = 6;
			d [101] = 5;
			d [103] = 10;
			d [106] = 7;
			d [109] = 5;
			d [112] = 3;
			d [114] = 7;
			d [117] = 6;
			d [120] = 4;
			d [122] = 12;
			d [125] = 7;
			for (int y = 89; y < 127; y++)
				for (int m = 1; m <= 12; m++)
					Assert.AreEqual (d.ContainsKey (y) && d [y] == m, tw.IsLeapMonth (y, m), "tw" + y + "/" + m);

			d = new Dictionary<int,int> ();
			d [13] = 5;
			d [16] = 3;
			d [18] = 8;
			d [21] = 6;
			d [24] = 4;
			d [26] = 10;
			d [29] = 6;
			for (int y = 12; y < 32; y++)
				for (int m = 1; m <= 12; m++)
					Assert.AreEqual (d.ContainsKey (y) && d [y] == m, jp.IsLeapMonth (y, m, 4), "jp" + y + "/" + m);

			d = new Dictionary<int,int> ();
			d [2001] = 5;
			d [2004] = 3;
			d [2006] = 8;
			d [2009] = 6;
			d [2012] = 4;
			d [2014] = 10;
			d [2017] = 6;
			for (int y = 2000; y < 2020; y++)
				for (int m = 1; m <= 12; m++)
					Assert.AreEqual (d.ContainsKey (y) && d [y] == m, kr.IsLeapMonth (y, m), "kr" + y + "/" + m);
		}
	}
}

