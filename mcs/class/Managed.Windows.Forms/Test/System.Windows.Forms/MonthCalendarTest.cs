//
// MonthCalendarTest.cs: Test case for MonthCalendar
// 
// Authors:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MonthCalendarPropertiesTest : MonthCalendar
	{
		private bool clickRaised;
		private bool doubleClickRaised;

		[SetUp]
		protected void SetUp () {
			clickRaised = false;
			doubleClickRaised = false;
		}

		[Test]
		public void ClickEventTest ()
		{
			Click += new EventHandler (OnClickHandler);
			DoubleClick += new EventHandler (OnDoubleClickHandler);
			OnClick (EventArgs.Empty);
			OnDoubleClick (EventArgs.Empty);
			
			Assert.IsTrue (clickRaised, "Click event not raised");
			Assert.IsTrue (doubleClickRaised, "DoubleClick event not raised");
		}

		void OnDoubleClickHandler (object sender, EventArgs e)
		{
			doubleClickRaised = true;
		}

		void OnClickHandler (object sender, EventArgs e)
		{
			clickRaised = true;
		}
		
#if NET_2_0
		[Test]
		public void DefaultMarginTest ()
		{
			Assert.AreEqual (DefaultMargin.All, 9, "#01");
		}
#endif
	}

	[TestFixture]
	public class MonthCalendarTest : TestHelper
	{
		[Test]
		public void MonthCalendarPropertyTest ()
		{
			Form myfrm = new Form ();
			myfrm.ShowInTaskbar = false;
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			//MonthCalendar myMonthCal2 = new MonthCalendar ();
			myMonthCal1.Name = "MonthCendar";
			myMonthCal1.TabIndex = 1;
			//DateTime myDateTime = new DateTime ();
			
			// A
			Assert.IsNotNull (myMonthCal1.AnnuallyBoldedDates, "#A-1");
			myMonthCal1.AddAnnuallyBoldedDate (new DateTime (2005, 09, 01));
			Assert.IsNotNull (myMonthCal1.AnnuallyBoldedDates, "#A0");
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
			// Font dependent. // Assert.AreEqual (153, myMonthCal1.SingleMonthSize.Height, "#S8a");
			// Font dependent. // Assert.AreEqual (176, myMonthCal1.SingleMonthSize.Width, "#S8b");
			Assert.AreEqual (null, myMonthCal1.Site, "#S9");
			// T
			Assert.AreEqual ("ActiveCaption", myMonthCal1.TitleBackColor.Name, "#T1");
			Assert.AreEqual ("ActiveCaptionText", myMonthCal1.TitleForeColor.Name, "#T2");
			Assert.AreEqual (DateTime.Today, myMonthCal1.TodayDate, "#T3");
			Assert.AreEqual (false, myMonthCal1.TodayDateSet, "#T4");
			Assert.AreEqual ("GrayText", myMonthCal1.TrailingForeColor.Name, "#T5");

			myfrm.Dispose ();
		}
		
		[Test]
		public void InitialSizeTest ()
		{
			MonthCalendar cal = new MonthCalendar ();
			Assert.IsTrue (cal.Size != Size.Empty, "#01");
		}
	
		[Test]
		public void MonthCalMaxSelectionCountException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();

			try {
				myMonthCal1.MaxSelectionCount = 0; // value is less than 1
				Assert.Fail ("#A1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("MaxSelectionCount", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#endif
		}

		[Test]
		public void MonthCalMaxDateException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();

			try {
				myMonthCal1.MaxDate = new DateTime (1752, 1, 1, 0, 0, 0, 0); // value is less than min date (01/01/1753)
				Assert.Fail ("#A1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("MaxDate", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#endif
		}

		[Test]
		public void MonthCalMinDateException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();

			try {
				myMonthCal1.MinDate = new DateTime (1752, 1, 1, 0, 0, 0, 0); // Date earlier than 01/01/1753
				Assert.Fail ("#A1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("MinDate", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#endif

			try {
				myMonthCal1.MinDate = new DateTime (9999, 12, 31, 0, 0, 0, 0); // Date greater than max date
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("MinDate", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalSelectRangeException ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.SelectionRange = new SelectionRange (new DateTime (1752, 01, 01), new DateTime (1752, 01, 02));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MonthCalSelectRangeException2 ()
		{
			MonthCalendar myMonthCal1 = new MonthCalendar ();
			myMonthCal1.SelectionRange = new SelectionRange (new DateTime (9999, 12, 30), new DateTime (9999, 12, 31));
		}

		[Test]
		public void AddAnnuallyBoldedDateTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddAnnuallyBoldedDate (new DateTime (2005, 09, 01));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 01), myMonthCal.AnnuallyBoldedDates.GetValue (0), "#add1");

			DateTime dt = new DateTime (2006, 02, 03, 04, 05, 06, 07);
			DateTime dt2 = new DateTime (2006, 02, 03);

			myMonthCal.RemoveAllAnnuallyBoldedDates ();
			myMonthCal.AddAnnuallyBoldedDate (dt);
			Assert.AreEqual (dt, myMonthCal.AnnuallyBoldedDates [0], "#add2");

			myMonthCal.AddAnnuallyBoldedDate (dt2);
			Assert.AreEqual (dt2, myMonthCal.AnnuallyBoldedDates [1], "#add3");

			myForm.Dispose ();
		}
	
		[Test]
		public void RemoveAnnuallyBoldedDateTest ()
		{
			MonthCalendar myMonthCal = new MonthCalendar ();

			DateTime[] dts = new DateTime [10];
			dts[0] = new DateTime (2001, 02, 03, 04, 05, 06, 07); // base datetime
 			dts[1] = new DateTime (2001, 02, 03); // only date 
			dts[2] = new DateTime (2002, 03, 04); // only date, different
			dts[3] = new DateTime (2002, 02, 03, 04, 05, 06, 07); // different year
			dts[4] = new DateTime (2001, 03, 03, 04, 05, 06, 07); // different month
			dts[5] = new DateTime (2001, 02, 04, 04, 05, 06, 07); // etc...
			dts[6] = new DateTime (2001, 02, 03, 05, 05, 06, 07);
			dts[7] = new DateTime (2001, 02, 03, 04, 06, 06, 07);
			dts[8] = new DateTime (2001, 02, 03, 04, 05, 07, 07);
			dts[9] = new DateTime (2001, 02, 03, 04, 05, 06, 08);

			for (int i = 0; i < dts.Length; i++) {
				for (int j = 0; j < dts.Length; j++) {
					myMonthCal.RemoveAllAnnuallyBoldedDates ();
					myMonthCal.AddAnnuallyBoldedDate (dts [j]);
					myMonthCal.RemoveAnnuallyBoldedDate (dts [i]);
					if (dts [j].Month == dts [i].Month && dts [j].Day == dts [i].Day)
						Assert.AreEqual (0, myMonthCal.AnnuallyBoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
					else
						Assert.AreEqual (1, myMonthCal.AnnuallyBoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
				}
			}
			
			for (int i = 0; i < dts.Length; i++) 
			{
				myMonthCal.AnnuallyBoldedDates = dts;
				myMonthCal.RemoveAnnuallyBoldedDate (dts [i]);
				Assert.AreEqual (9, myMonthCal.AnnuallyBoldedDates.Length, "#remove1" + i.ToString ());
			}
		}

		[Test]
		public void RemoveMonthlyBoldedDateTest ()
		{
			MonthCalendar myMonthCal = new MonthCalendar ();

			DateTime[] dts = new DateTime [10];
			dts[0] = new DateTime (2001, 02, 03, 04, 05, 06, 07); // base datetime
			dts[1] = new DateTime (2001, 02, 03); // only date 
			dts[2] = new DateTime (2002, 03, 04); // only date, different
			dts[3] = new DateTime (2002, 02, 03, 04, 05, 06, 07); // different year
			dts[4] = new DateTime (2001, 03, 03, 04, 05, 06, 07); // different month
			dts[5] = new DateTime (2001, 02, 04, 04, 05, 06, 07); // etc...
			dts[6] = new DateTime (2001, 02, 03, 05, 05, 06, 07);
			dts[7] = new DateTime (2001, 02, 03, 04, 06, 06, 07);
			dts[8] = new DateTime (2001, 02, 03, 04, 05, 07, 07);
			dts[9] = new DateTime (2001, 02, 03, 04, 05, 06, 08);

			for (int i = 0; i < dts.Length; i++) 
			{
				for (int j = 0; j < dts.Length; j++) 
				{
					myMonthCal.RemoveAllMonthlyBoldedDates ();
					myMonthCal.AddMonthlyBoldedDate (dts [j]);
					myMonthCal.RemoveMonthlyBoldedDate (dts [i]);
					if (dts [j].Month == dts [i].Month && dts [j].Day == dts [i].Day)
						Assert.AreEqual (0, myMonthCal.MonthlyBoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
					else
						Assert.AreEqual (1, myMonthCal.MonthlyBoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
				}
			}
			
			for (int i = 0; i < dts.Length; i++) 
			{
				myMonthCal.MonthlyBoldedDates = dts;
				myMonthCal.RemoveMonthlyBoldedDate (dts [i]);
				Assert.AreEqual (9, myMonthCal.MonthlyBoldedDates.Length, "#remove1" + i.ToString ());
			}
		}

		[Test]
		public void RemoveBoldedDateTest ()
		{
			MonthCalendar myMonthCal = new MonthCalendar ();

			DateTime[] dts = new DateTime [10];
			dts[0] = new DateTime (2001, 02, 03, 04, 05, 06, 07); // base datetime
			dts[1] = new DateTime (2001, 02, 03); // only date 
			dts[2] = new DateTime (2002, 03, 04); // only date, different
			dts[3] = new DateTime (2002, 02, 03, 04, 05, 06, 07); // different year
			dts[4] = new DateTime (2001, 03, 03, 04, 05, 06, 07); // different month
			dts[5] = new DateTime (2001, 02, 04, 04, 05, 06, 07); // etc...
			dts[6] = new DateTime (2001, 02, 03, 05, 05, 06, 07);
			dts[7] = new DateTime (2001, 02, 03, 04, 06, 06, 07);
			dts[8] = new DateTime (2001, 02, 03, 04, 05, 07, 07);
			dts[9] = new DateTime (2001, 02, 03, 04, 05, 06, 08);

			for (int i = 0; i < dts.Length; i++) 
			{
				for (int j = 0; j < dts.Length; j++) 
				{
					myMonthCal.RemoveAllBoldedDates ();
					myMonthCal.AddBoldedDate (dts [j]);
					myMonthCal.RemoveBoldedDate (dts [i]);
					if (dts [j].Month == dts [i].Month && dts [j].Day == dts [i].Day && dts[j].Year == dts[i].Year)
						Assert.AreEqual (0, myMonthCal.BoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
					else
						Assert.AreEqual (1, myMonthCal.BoldedDates.Length, "#remove0" + i.ToString () + ", " + j.ToString ());
				}
			}
			
			for (int i = 0; i < dts.Length; i++) 
			{
				myMonthCal.BoldedDates = dts;
				myMonthCal.RemoveBoldedDate (dts [i]);
				Assert.AreEqual (9, myMonthCal.BoldedDates.Length, "#remove1" + i.ToString ());
			}
		}

		[Test]
		public void AddBoldedDateTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddBoldedDate (new DateTime (2005, 09, 02));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 02), myMonthCal.BoldedDates.GetValue (0), "#add2");
			myForm.Dispose ();
		}

		[Test]
		public void AddMonthlyBoldedDateTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			MonthCalendar myMonthCal = new MonthCalendar ();
			myMonthCal.AddMonthlyBoldedDate (new DateTime (2005, 09, 03));
			myForm.Controls.Add (myMonthCal);
			Assert.AreEqual (new DateTime (2005, 09, 03), myMonthCal.MonthlyBoldedDates.GetValue (0), "#add2");
			myForm.Dispose ();
		}
		
		[Test]
		public void GetDisplayRangeTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			MonthCalendar myMonthCal = new MonthCalendar ();
			myForm.Controls.Add (myMonthCal);
			SelectionRange mySelRange = new SelectionRange ();
			mySelRange.Start = new DateTime (DateTime.Now.Year, DateTime.Now.Month, 1);
			mySelRange.End = mySelRange.Start.AddMonths (1).AddDays (-1);
			Assert.AreEqual (mySelRange.Start, myMonthCal.GetDisplayRange (true).Start, "#Get1");
			Assert.AreEqual (mySelRange.End, myMonthCal.GetDisplayRange (true).End, "#Get22");
			myForm.Dispose ();
		}
		
		[Test]
		public void HitTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			MonthCalendar myMonthCal = new MonthCalendar ();
			myForm.Controls.Add (myMonthCal);
			MonthCalendar.HitTestInfo hitTest = myMonthCal.HitTest (10, 10);
			Assert.AreEqual (MonthCalendar.HitArea.PrevMonthButton, hitTest.HitArea, "#Hit1");
			Assert.AreEqual (new DateTime (01, 01, 01), hitTest.Time, "#Hit2");
		}

		[Test]
		public void DateChangedEventTest ()
		{
			MonthCalendar myCalendar = new MonthCalendar ();
			
			myCalendar.Tag = false;
			myCalendar.DateChanged += new DateRangeEventHandler (DateChangedEventHandler);
			myCalendar.SetDate (DateTime.Today.AddDays (72));
			Assert.AreEqual (true, (bool) myCalendar.Tag, "#01");
		}

		void DateChangedEventHandler (object sender, DateRangeEventArgs e)
		{
			((Control) sender).Tag = true;
		}
	}
}
