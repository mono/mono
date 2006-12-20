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
// Authors:
//	Rolf Bjarne Kvinge	RKvinge@novell.com


using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DateTimePickerTest {
		[Test]
		public void DefaultPropertiesTest ()
		{
			DateTimePicker dt = new DateTimePicker ();
			
			Assert.AreEqual ("Window", dt.BackColor.Name, "B1");
			Assert.AreSame (null, dt.BackgroundImage, "B2");
#if NET_2_0
			Assert.AreEqual (ImageLayout.Tile, dt.BackgroundImageLayout, "B3");
#endif

			//Assert.AreSame (null, dt.CalendarFont, "C1");
			Assert.AreEqual ("ControlText", dt.CalendarForeColor.Name, "C2");
			Assert.AreEqual ("Window", dt.CalendarMonthBackground.Name, "C3");
			Assert.AreEqual ("ActiveCaption", dt.CalendarTitleBackColor.Name, "C4");
			Assert.AreEqual ("ActiveCaptionText", dt.CalendarTitleForeColor.Name, "C5");
			Assert.AreEqual ("GrayText", dt.CalendarTrailingForeColor.Name, "C6");
			Assert.AreEqual (true, dt.Checked, "C7");
			Assert.AreEqual (null, dt.CustomFormat, "C8");

			Assert.AreEqual (LeftRightAlignment.Left, dt.DropDownAlign, "D1");
			
			Assert.AreEqual ("WindowText", dt.ForeColor.Name, "F1");
			Assert.AreEqual (DateTimePickerFormat.Long, dt.Format, "F2");
			
			Assert.AreEqual (new DateTime (9998, 12, 31, 0, 0, 0), dt.MaxDate, "M1");
			Assert.AreEqual (new DateTime (9998, 12, 31, 0, 0, 0), DateTimePicker.MaxDateTime, "M2");
			Assert.AreEqual (new DateTime (1753, 1, 1), dt.MinDate, "M3");
			Assert.AreEqual (new DateTime (1753, 1, 1), DateTimePicker.MinDateTime, "M4");
#if NET_2_0
			Assert.AreEqual (new DateTime (9998, 12, 31, 0, 0, 0), DateTimePicker.MaximumDateTime, "M5");
			Assert.AreEqual (new DateTime (1753, 1, 1), DateTimePicker.MinimumDateTime, "M6");
#endif

#if NET_2_0
			Assert.AreEqual (new Padding (0, 0, 0, 0), dt.Padding, "P1");
#endif
			// PreferredHeight is Font dependent.
			
#if NET_2_0
			Assert.AreEqual (false, dt.RightToLeftLayout, "R1");
#endif

			Assert.AreEqual (false, dt.ShowCheckBox, "S1");
			Assert.AreEqual (false, dt.ShowUpDown, "S2");
			
			Assert.AreEqual ("", dt.Text, "T1");
			
			Assert.AreEqual (DateTime.Today, dt.Value.Date, "V1");
		}
		
		[Test]
		public void TextTest ()
		{
			DateTimePicker dt = new DateTimePicker ();
			DateTime tomorrow = DateTime.Today.AddDays (1);
			dt.Value = tomorrow;
			Assert.AreEqual ("", dt.Text, "#1");

			dt.CreateControl ();
			Assert.AreEqual (tomorrow.ToLongDateString (), dt.Text, "#2");
			
		}
	}
}