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
// Copyright (c) 2006 Novell, Inc.
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.Layout {

	[TestFixture]
	public class TableLayoutSettingsTypeConverterTest {
		
		[Test]
		public void CanConvertFrom ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();

			Assert.IsTrue (c.CanConvertFrom (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (object)), "4");
		}

		[Test]
		public void CanConvertTo ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();

			Assert.IsTrue (c.CanConvertTo (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertTo (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertTo (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertTo (null, typeof (object)), "4");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void Roundtrip ()
		{
			TableLayoutSettingsTypeConverter c = new TableLayoutSettingsTypeConverter ();
			object result;

			string sv = @"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls><Control Name=""userNameLabel"" Row=""0"" RowSpan=""1"" Column=""0"" ColumnSpan=""1"" /><Control Name=""userName"" Row=""0"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" /><Control Name=""passwordLabel"" Row=""1"" RowSpan=""1"" Column=""0"" ColumnSpan=""1"" /><Control Name=""password"" Row=""1"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" /><Control Name=""savePassword"" Row=""2"" RowSpan=""1"" Column=""1"" ColumnSpan=""1"" /></Controls><Columns Styles=""AutoSize,0,Percent,100"" /><Rows Styles=""AutoSize,0,AutoSize,0,AutoSize,0"" /></TableLayoutSettings>";

			result = c.ConvertFrom (null, null, sv);

			Assert.AreEqual (typeof (TableLayoutSettings), result.GetType(), "1");

			TableLayoutSettings ts = (TableLayoutSettings)result;

			Assert.AreEqual (2, ts.ColumnStyles.Count, "2");
			Assert.AreEqual (SizeType.AutoSize, ts.ColumnStyles[0].SizeType, "3");
			Assert.AreEqual (0.0f, ts.ColumnStyles[0].Width, "4");
			Assert.AreEqual (SizeType.Percent, ts.ColumnStyles[1].SizeType, "5");
			Assert.AreEqual (100.0f, ts.ColumnStyles[1].Width, "6");

			Assert.AreEqual (3, ts.RowStyles.Count, "7");

			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[0].SizeType, "8");
			Assert.AreEqual (0.0f, ts.RowStyles[0].Height, "9");
			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[1].SizeType, "10");
			Assert.AreEqual (0.0f, ts.RowStyles[1].Height, "11");
			Assert.AreEqual (SizeType.AutoSize, ts.RowStyles[2].SizeType, "12");
			Assert.AreEqual (0.0f, ts.RowStyles[2].Height, "13");

			string rv = (string)c.ConvertTo (null, null, ts, typeof (string));

			Assert.AreEqual (sv, rv, "roundtrip");
		}
	}
}

#endif
