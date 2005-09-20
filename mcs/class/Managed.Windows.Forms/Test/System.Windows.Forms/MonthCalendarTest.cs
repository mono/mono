//
// MonthCalendarTest.cs: Test case for MonthCalendar
// 
// Authors:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.Syetem.Windows.Forms
{
	[TestFixture]
	[Ignore ("This test has to be completly reviewed")]	
	public class MonthCalendarTest
	{
		[Test]
		public void MonthCalendarPropertyTest ()
		{
			Form myfrm = new Form ();
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			MonthCalendar myMonthCal2 = new MonthCalendar ();
			myMonthCal1.Name = "MonthCendar";
			myMonthCal1.TabIndex = 1;
			DateTime myDateTime = new DateTime ();
			
			// A
			myMonthCal1.AddAnnuallyBoldedDate (new DateTime (2005, 09, 01));
			Assert.AreEqual (new DateTime (2005, 09, 01), myMonthCal1.AnnuallyBoldedDates.GetValue (0), "#A1");
                     
			// B 
			Assert.AreEqual ("Window", myMonthCal1.BackColor.Name, "#B1");
			myMonthCal1.AddBoldedDate (new DateTime (2005, 09, 01));
			Assert.AreEqual (new DateTime (2005, 09, 01), myMonthCal1.BoldedDates.GetValue (0), "#B2");
				
			// C
			Assert.AreEqual (1, myMonthCal1.CalendarDimensions.Height, "#C1");
			Assert.AreEqual (1, myMonthCal1.CalendarDimensions.Width, "#C2");
			Assert.AreEqual (false, myMonthCal1.CalendarDimensions.IsEmpty, "#C3");

			// F
			Assert.AreEqual (Day.Default, myMonthCal1.FirstDayOfWeek, "#F1");
			myMonthCal1.FirstDayOfWeek = Day.Sunday;
			Assert.AreEqual (Day.Sunday, myMonthCal1.FirstDayOfWeek, "#F2");
			Assert.AreEqual ("WindowText", myMonthCal1.ForeColor.Name, "#F3");

			// M 
			Assert.AreEqual (new DateTime (9998,12,31), myMonthCal1.MaxDate, "#M1");
			Assert.AreEqual (7, myMonthCal1.MaxSelectionCount, "#M2");
			Assert.AreEqual (new DateTime (1753,1,1), myMonthCal1.MinDate, "#M3");
			myMonthCal1.AddMonthlyBoldedDate (new DateTime (2005, 09, 01));
			Assert.AreEqual (new DateTime(2005, 09, 01), myMonthCal1.MonthlyBoldedDates.GetValue (0), "#M4");
			
			// S 
			Assert.AreEqual (0, myMonthCal1.ScrollChange, "#S1");
			myMonthCal1.SelectionStart = new DateTime (2005,09,02);
			myMonthCal1.SelectionEnd = new DateTime (2005,09,03);
			Assert.AreEqual (new DateTime (2005,09,03), myMonthCal1.SelectionEnd, "#S2");
			//Assert.AreEqual (new SelectionRange (new DateTime(2005,09,02), new DateTime(2005,09,03)), myMonthCal1.SelectionRange, "#S3");
			Assert.AreEqual (new DateTime (2005,09,02), myMonthCal1.SelectionStart, "#S4");
			Assert.AreEqual (true, myMonthCal1.ShowToday, "#S5");
			Assert.AreEqual (true, myMonthCal1.ShowTodayCircle, "#S6");
			Assert.AreEqual (false, myMonthCal1.ShowWeekNumbers, "#S7");
			Assert.AreEqual (153, myMonthCal1.SingleMonthSize.Height, "#S8a");
			Assert.AreEqual (176, myMonthCal1.SingleMonthSize.Width, "#S8b");
			Assert.AreEqual (null, myMonthCal1.Site, "#S9");
			// T
			Assert.AreEqual ("ActiveCaption", myMonthCal1.TitleBackColor.Name, "#T1");
			Assert.AreEqual ("ActiveCaptionText", myMonthCal1.TitleForeColor.Name, "#T2");
			Assert.AreEqual (DateTime.Today, myMonthCal1.TodayDate, "#T3");
			Assert.AreEqual (false, myMonthCal1.TodayDateSet, "#T4");
			Assert.AreEqual ("GrayText", myMonthCal1.TrailingForeColor.Name, "#T5");
		}
	
		[Test]		
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalMaxSelectionCountException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.MaxSelectionCount = 0 ; // value is less than 1
		}

		[Test]		
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalMaxDateException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.MaxDate = new DateTime (1752, 1, 1, 0, 0, 0, 0); // value is less than min date (01/01/1753)
		}

		[Test]		
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalMinDateException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.MinDate = new DateTime(1752, 1, 1, 0, 0, 0, 0); // Date earlier than 01/01/1753
			myMonthCal1.MinDate = new DateTime(9999, 12, 31, 0, 0, 0, 0); // Date greater than max date (01/01/1753)
		}

		[Test]		
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalSelectRangeException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.SelectionRange = new SelectionRange (new DateTime (1752, 01, 01), new DateTime (1752, 01, 02));
			myMonthCal1.SelectionRange = new SelectionRange (new DateTime (9999, 12, 30), new DateTime (9999, 12, 31));
		}
		
		[Test]
		public void AddAnnuallyBoldedDateTest ()
		{
			Form myForm = new Form ();
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddAnnuallyBoldedDate (new DateTime (2005, 09, 01));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 01), myMonthCal.AnnuallyBoldedDates.GetValue (0), "#add1");
		}

		[Test]
		public void AddBoldedDateTest ()
		{
			Form myForm = new Form ();
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddBoldedDate (new DateTime (2005, 09, 02));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 02), myMonthCal.BoldedDates.GetValue (0), "#add2");
		}

		[Test]
		public void AddMonthlyBoldedDateTest ()
		{
			Form myForm = new Form ();
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddMonthlyBoldedDate (new DateTime (2005, 09, 03));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 03), myMonthCal.MonthlyBoldedDates.GetValue (0), "#add2");
		}
		
		[Test]
		public void GetDispalyRangeTest ()
		{
			Form myForm = new Form ();
			MonthCalendar myMonthCal = new MonthCalendar ();
			myForm.Controls.Add (myMonthCal);
			SelectionRange mySelRange = new SelectionRange ();
			mySelRange.Start = new DateTime (2005, 09, 01); 
			mySelRange.End = new DateTime (2005, 09, 30);
			Assert.AreEqual (mySelRange.Start, myMonthCal.GetDisplayRange (true).Start, "#Get1");
			Assert.AreEqual (mySelRange.End, myMonthCal.GetDisplayRange (true).End, "#Get22");
		}
		
		[Test]
		public void HitTest ()
		{
			Form myForm = new Form ();
			MonthCalendar myMonthCal = new MonthCalendar ();
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (01, 01, 01), myMonthCal.HitTest(10, 10).Time, "#Hit1");
		}
	}
}
