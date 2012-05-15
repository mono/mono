//
// DateTimeFormatInfo.cs
//
// Authors:
//     Ben Maurer <bmaurer@andrew.cmu.edu>
//
// Copyright (C) 2005 Novell (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Globalization;
using System.IO;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class DateTimeFormatInfoTest
	{
		[Test]
		public void GetAllDateTimePatterns_ret_diff_obj ()
		{
			// We need to return different objects for security reasons
			DateTimeFormatInfo dtfi = CultureInfo.InvariantCulture.DateTimeFormat;

			string [] one = dtfi.GetAllDateTimePatterns ();
			string [] two = dtfi.GetAllDateTimePatterns ();
			Assert.IsTrue (one != two);
		}

		[Test]
		public void EraName ()
		{
			CultureInfo en_US = new CultureInfo ("en-US");
			DateTimeFormatInfo dtfi = en_US.DateTimeFormat;
			Assert.AreEqual ("AD", dtfi.GetAbbreviatedEraName (0), "#1");
			Assert.AreEqual ("A.D.", dtfi.GetEraName (1), "#2");
			Assert.AreEqual (1, dtfi.GetEra ("A.D."), "#3");
			Assert.AreEqual (1, dtfi.GetEra ("AD"), "#4");
			Assert.AreEqual (-1, dtfi.GetEra ("C.E"), "#5");
			Assert.AreEqual (-1, dtfi.GetEra ("Common Era"), "#6");
		}

		[Test] // bug #332553
		public void MonthNames ()
		{
			CultureInfo c = CultureInfo.CreateSpecificCulture ("en");
			string [] monthNamesA = c.DateTimeFormat.MonthNames;
			Assert.AreEqual (13, monthNamesA.Length, "#A1");
			Assert.AreEqual ("January", monthNamesA [0], "#A2");
			Assert.AreEqual ("February", monthNamesA [1], "#A3");
			Assert.AreEqual ("March", monthNamesA [2], "#A4");
			Assert.AreEqual ("April", monthNamesA [3], "#A5");
			Assert.AreEqual ("May", monthNamesA [4], "#A6");
			Assert.AreEqual ("June", monthNamesA [5], "#A7");
			Assert.AreEqual ("July", monthNamesA [6], "#A8");
			Assert.AreEqual ("August", monthNamesA [7], "#A9");
			Assert.AreEqual ("September", monthNamesA [8], "#A10");
			Assert.AreEqual ("October", monthNamesA [9], "#A11");
			Assert.AreEqual ("November", monthNamesA [10], "#A12");
			Assert.AreEqual ("December", monthNamesA [11], "#A13");
			Assert.AreEqual (string.Empty, monthNamesA [12], "#A14");

			c.DateTimeFormat.MonthNames = c.DateTimeFormat.MonthNames;

			string [] monthNamesB = c.DateTimeFormat.MonthNames;
			Assert.AreEqual (monthNamesA, monthNamesB, "#B1");
			Assert.IsFalse (object.ReferenceEquals (monthNamesA, monthNamesB), "#B2");
		}

		[Test]
		public void TestSpecificCultureFormats_es_ES ()
		{
			CultureInfo ci = new CultureInfo ("es-ES");
			DateTimeFormatInfo di = ci.DateTimeFormat;
			Assert.AreEqual ("dddd, dd' de 'MMMM' de 'yyyy", di.LongDatePattern, "#1");
			Assert.AreEqual ("H:mm:ss", di.LongTimePattern, "#2");
			Assert.AreEqual ("dddd, dd' de 'MMMM' de 'yyyy H:mm:ss", di.FullDateTimePattern, "#3");
			Assert.AreEqual ("MMMM' de 'yyyy", di.YearMonthPattern, "#4");
			Assert.AreEqual ("dd MMMM", di.MonthDayPattern, "#5");
		}

		[Test]
		public void Clone ()
		{
			DateTimeFormatInfo dfi = (DateTimeFormatInfo) DateTimeFormatInfo.InvariantInfo.Clone ();
			dfi.MonthNames[0] = "foo";
			dfi.AbbreviatedDayNames[0] = "b1";
			dfi.AbbreviatedMonthGenitiveNames[0] = "b2";


			Assert.IsFalse (dfi.IsReadOnly, "#0");
			Assert.AreEqual ("January", DateTimeFormatInfo.InvariantInfo.MonthNames [0], "#1");
			Assert.AreEqual ("Sun", DateTimeFormatInfo.InvariantInfo.AbbreviatedDayNames[0], "#2");
			Assert.AreEqual ("Jan", DateTimeFormatInfo.InvariantInfo.AbbreviatedMonthGenitiveNames[0], "#3");
		}

		[Test]
		public void MonthGenitiveNames ()
		{
			var dfi = new CultureInfo ("cs-CZ").DateTimeFormat;
			Assert.AreEqual ("ledna", dfi.MonthGenitiveNames[0], "#1");
		}

#if !TARGET_JVM
		[Test]
		public void Bug78569 ()
		{
			DateTime dt = DateTime.Now;
			CultureInfo ci = new CultureInfo ("en-GB");
			string s = dt.ToString (ci);
			DateTime dt2 = DateTime.Parse (s, ci);
			Assert.AreEqual (dt.Month, dt2.Month);
		}
#endif
	}
}


