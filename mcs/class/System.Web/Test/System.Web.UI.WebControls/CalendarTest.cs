//
// Tests for System.Web.UI.WebControls.Calendar.cs
//
// Author:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

class PokerCalendar : System.Web.UI.WebControls.Calendar
{
	public PokerCalendar ()
	{
		TrackViewState ();
	}

	public object SaveState ()
	{
		return SaveViewState ();
	}

	public void LoadState (object o)
	{
		LoadViewState (o);
	}

	public string Render ()
	{
		StringWriter sw = new StringWriter ();
		sw.NewLine = "\n";
		HtmlTextWriter writer = new HtmlTextWriter (sw);
		base.Render (writer);
		return writer.InnerWriter.ToString ();
	}

	bool cs_called;
	public bool CS_Called {
		get { return cs_called; }
		set { cs_called = value; }
	}

	protected override Style CreateControlStyle ()
	{
		cs_called = true;
		return base.CreateControlStyle ();
	}
}

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class CalendarTest
	{
		[Test]
		public void Calendar_DefaultValues ()
		{
			PokerCalendar c = new PokerCalendar ();

			Assert.AreEqual (2, c.CellPadding, "CellPadding");
			Assert.AreEqual (0, c.CellSpacing, "CellSpacing");
			Assert.AreEqual (DayNameFormat.Short, c.DayNameFormat, "DayNameFormat");
			Assert.AreEqual (FirstDayOfWeek.Default, c.FirstDayOfWeek, "FirstDayOfWeek");
			Assert.AreEqual ("&gt;",c.NextMonthText, "NextMonthText");
			Assert.AreEqual (NextPrevFormat.CustomText, c.NextPrevFormat, "NextPrevFormat");
			Assert.AreEqual ("&lt;", c.PrevMonthText, "PrevMonthText");
			Assert.AreEqual (CalendarSelectionMode.Day, c.SelectionMode, "SelectionMode");
			Assert.AreEqual ("&gt;&gt;", c.SelectMonthText, "SelectMonthText");
			Assert.AreEqual ("&gt;", c.SelectWeekText, "SelectWeekText");
			Assert.AreEqual (true, c.ShowDayHeader, "ShowDayHeader");
			Assert.AreEqual (false, c.ShowGridLines, "ShowGridLines");
			Assert.AreEqual (true, c.ShowNextPrevMonth , "ShowNextPrevMonth");
			Assert.AreEqual (true, c.ShowTitle, "ShowTitle");
			Assert.AreEqual (TitleFormat.MonthYear, c.TitleFormat, "TitleFormat");
			Assert.AreEqual (DateTime.Today, c.TodaysDate , "TodaysDate");
			Assert.AreEqual (DateTime.MinValue, c.VisibleDate, "VisibleDate");
		}

		//
		// Properties
		//
		[Test]
		public void NextMonthTextProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.NextMonthText = "NextMonthText";
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) != -1, "NextMonthText");
		}

		[Test]
		public void NextPrevFormatProperty ()
		{
			DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			DateTime prevMonth = dateInfo.Calendar.AddMonths (DateTime.Today, -1);
			DateTime nextMonth = dateInfo.Calendar.AddMonths (DateTime.Today, 1);

			c.NextMonthText = "NextMonthText";	// CustomText
			c.PrevMonthText = "PrevMonthText";
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) != -1, "NextPrevFormat1");
			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) != -1, "NextPrevFormat2");

			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			c.NextPrevFormat = NextPrevFormat.FullMonth;	// FullMonth
			c.RenderControl (tw);

			Assert.AreEqual (true, sw.ToString ().IndexOf (dateInfo.GetMonthName (dateInfo.Calendar.GetMonth (prevMonth))) != -1, "NextPrevFormat3:" + sw.ToString () + "|||" + dateInfo.GetMonthName (DateTimeFormatInfo.CurrentInfo.Calendar.GetMonth (prevMonth)));
			Assert.AreEqual (true, sw.ToString ().IndexOf (dateInfo.GetMonthName (dateInfo.Calendar.GetMonth (nextMonth))) != -1, "NextPrevFormat4");

			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			c.NextPrevFormat = NextPrevFormat.ShortMonth;	// ShortMonth
			c.RenderControl (tw);

			Assert.AreEqual (true, sw.ToString ().IndexOf (dateInfo.GetAbbreviatedMonthName (dateInfo.Calendar.GetMonth (prevMonth))) != -1, "NextPrevFormat5");
			Assert.AreEqual (true, sw.ToString ().IndexOf (dateInfo.GetAbbreviatedMonthName (dateInfo.Calendar.GetMonth (nextMonth))) != -1, "NextPrevFormat6");
		}

		[Test]
		public void DayHeaderStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.DayHeaderStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "DayHeaderStyleProperty");
		}

		[Test]
		public void NextPrevStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.NextPrevStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "NextPrevStyleProperty");
		}

		[Test]
		public void SelectorStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.SelectorStyle.BackColor = Color.Green;
			c.SelectionMode = CalendarSelectionMode.DayWeek;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "SelectorStyleProperty");
		}

		//[Test]
		public void TitleStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.SelectorStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "TitleStyleProperty");
		}

		[Test]
		public void OtherMonthDayStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.OtherMonthDayStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "OtherMonthDayStyle");
		}

		[Test]
		public void SelectedDayStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.SelectedDayStyle.BackColor = Color.Green;
			c.TodaysDate = new DateTime (2000, 1,1);
			((IPostBackEventHandler)c).RaisePostBackEvent ("0001");
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "SelectedDayStyle");
		}

		[Test]
		public void TodayDayStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.TodayDayStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "TodayDayStyle");
		}

		[Test]
		public void WeekendDayStyleProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.WeekendDayStyle.BackColor = Color.Green;
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "WeekendDayStyle");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectDateProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			DateTime now = DateTime.Now;
			DateTime today = now.Date;
			
			c.SelectedDate = today;
			Assert.AreEqual (today, c.SelectedDate, "SelectDateProperty #1");

			c.SelectedDate = now;
			Assert.AreEqual (now, c.SelectedDate, "SelectDateProperty #2");
		}

		[Test]
		public void PrevMonthTextProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.PrevMonthText = "PrevMonthText";
			c.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) != -1, "PrevMonthText");
		}

		[Test]
		public void ShowNextPrevMonthProperty ()
		{
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.NextMonthText = "NextMonthText";
			c.PrevMonthText = "PrevMonthText";
			c.RenderControl (tw);

			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) != -1, "ShowNextPrevMonth1");
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) != -1, "ShowNextPrevMonth2");

			c.ShowNextPrevMonth = false;
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			c.RenderControl (tw);

			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) == -1, "ShowNextPrevMonth3");
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) == -1, "ShowNextPrevMonth4");
		}

		[Test]
		public void ShowTitleProperty ()
		{
			String monthName;
			DateTimeFormatInfo dateInfo = DateTimeFormatInfo.CurrentInfo;
			PokerCalendar c = new PokerCalendar ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.NextMonthText = "NextMonthText";
			c.PrevMonthText = "PrevMonthText";
			c.RenderControl (tw);
			monthName = dateInfo.GetMonthName (dateInfo.Calendar.GetMonth (DateTime.Today));

			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) != -1, "ShowTitle1");
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) != -1, "ShowTitle2");
			Assert.AreEqual (true, sw.ToString().IndexOf (monthName) != -1, "ShowTitle3");

			c.ShowTitle = false;
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			c.RenderControl (tw);

			Assert.AreEqual (true, sw.ToString().IndexOf (c.PrevMonthText) == -1, "ShowTitle4");
			Assert.AreEqual (true, sw.ToString().IndexOf (c.NextMonthText) == -1, "ShowTitle5");
			Assert.AreEqual (true, sw.ToString().IndexOf (monthName) == -1, "ShowTitle6");
		}

		//
		// Properties exceptions
		//

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellPaddingException ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.CellPadding = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellSpacingException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.CellSpacing = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DayNameFormatException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.DayNameFormat = (DayNameFormat) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FirstDayOfWeekException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.FirstDayOfWeek = (FirstDayOfWeek) 15;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NextPrevFormatException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.NextPrevFormat = (NextPrevFormat) 15;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectionModeException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.SelectionMode = (CalendarSelectionMode) 15;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TitleFormatException ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.TitleFormat = (TitleFormat) 15;
		}

		//
		// Events
		//
		private bool eventFired;
		private void SelectionChangedHandler (object sender, EventArgs e)
		{
			eventFired = true;
		}

		private void VisibleMonthChangedHandler (object sender, MonthChangedEventArgs e)
		{
			eventFired = true;
		}

		int days;
		private void DayRenderEventHandler (object sender, DayRenderEventArgs e)
		{
			days++;
			e.Cell.BackColor = Color.Yellow;
		}

		[Test]
		public void SelectionChanged ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.SelectionChanged += new EventHandler (SelectionChangedHandler);
			eventFired = false;
			((IPostBackEventHandler)c).RaisePostBackEvent ("0001");
			Assert.AreEqual (true, eventFired, "SelectionChanged event");
		}

		[Test]
		public void VisibleMonthChanged ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.VisibleMonthChanged += new MonthChangedEventHandler (VisibleMonthChangedHandler);
			eventFired = false;
			((IPostBackEventHandler)c).RaisePostBackEvent ("V0001");
			Assert.AreEqual (true, eventFired, "VisibleMonthChanged event");
		}

		[Test]
		public void DayRender ()
		{
			PokerCalendar c = new PokerCalendar ();
			c.DayRender += new DayRenderEventHandler (DayRenderEventHandler);
			days = 0;

			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			c.RenderControl (tw);
			Assert.AreEqual (6 * 7, days, "DayRender event");
			Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("yellow") != -1, "DayRender event change");
		}

		// ViewState
		[Test]
		public void Calendar_ViewState ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.CellPadding = 10;
			p.CellSpacing = 20;
  			p.DayNameFormat = DayNameFormat.Short;
			p.FirstDayOfWeek = FirstDayOfWeek.Friday;
			p.NextMonthText = "NextMonth";
			p.NextPrevFormat = NextPrevFormat.ShortMonth;
			p.PrevMonthText = "PrevMonth";
			p.SelectionMode = CalendarSelectionMode.DayWeek;
			p.SelectMonthText = "SelectMonth";
			p.SelectWeekText = "SelectWeek";
			p.ShowDayHeader = false;
			p.ShowGridLines = true;
			p.ShowNextPrevMonth = false;
			p.ShowTitle = false;
			p.TitleFormat = TitleFormat.Month;
			p.TodaysDate = new DateTime (1999,1,1);
			p.VisibleDate = new DateTime (1998,1,1);
#if NET_2_0
			p.Caption = "This is a Caption";
			p.CaptionAlign = TableCaptionAlign.Right;
#endif

			p.DayHeaderStyle.BackColor = Color.Blue;
			p.DayStyle.BackColor = Color.Yellow;
			p.NextPrevStyle.BackColor = Color.Red;
			p.OtherMonthDayStyle.BackColor = Color.Green;
			p.SelectedDayStyle.BackColor = Color.Silver;
			p.SelectorStyle.BackColor = Color.Pink;
			p.TodayDayStyle.BackColor = Color.White;
			p.WeekendDayStyle.BackColor = Color.Brown;

			object state = p.SaveState ();

			PokerCalendar copy = new PokerCalendar ();
			copy.LoadState (state);

			Assert.AreEqual (10, copy.CellPadding, "CellPadding");
			Assert.AreEqual (20, copy.CellSpacing, "CellSpacing");
			Assert.AreEqual (DayNameFormat.Short, copy.DayNameFormat, "DayNameFormat");
			Assert.AreEqual (FirstDayOfWeek.Friday, copy.FirstDayOfWeek, "FirstDayOfWeek");
			Assert.AreEqual ("NextMonth", copy.NextMonthText, "NextMonthText");
			Assert.AreEqual (NextPrevFormat.ShortMonth, copy.NextPrevFormat, "NextPrevFormat");
			Assert.AreEqual ("PrevMonth", copy.PrevMonthText, "PrevMonthText");
			Assert.AreEqual (CalendarSelectionMode.DayWeek, copy.SelectionMode, "SelectionMode");
			Assert.AreEqual ("SelectMonth", copy.SelectMonthText, "SelectMonthText");
			Assert.AreEqual ("SelectWeek", copy.SelectWeekText, "SelectWeekText");
			Assert.AreEqual (false, copy.ShowDayHeader, "ShowDayHeader");
			Assert.AreEqual (true, copy.ShowGridLines, "ShowGridLines");
			Assert.AreEqual (false, copy.ShowNextPrevMonth, "ShowNextPrevMonth");
			Assert.AreEqual (false, copy.ShowTitle, "ShowTitle");
			Assert.AreEqual (TitleFormat.Month, copy.TitleFormat, "TitleFormat");
			Assert.AreEqual (new DateTime (1999,1,1), copy.TodaysDate, "TodaysDate");
			Assert.AreEqual (new DateTime (1998,1,1), copy.VisibleDate, "VisibleDate");

#if NET_2_0
			Assert.AreEqual ("This is a Caption", copy.Caption, "Caption");
			Assert.AreEqual (TableCaptionAlign.Right, copy.CaptionAlign, "CaptionAlign");
#endif

			copy.ShowDayHeader = true;
			copy.ShowNextPrevMonth = true;
			copy.ShowTitle = true;
			copy.TodaysDate = copy.VisibleDate;

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("blue") != -1, "DayHeaderStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("yellow") != -1, "BackColor");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("red") != -1, "NextPrevStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "OtherMonthDayStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("silver") != -1, "SelectedDayStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("green") != -1, "OtherMonthDayStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("pink") != -1, "SelectorStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("white") != -1, "TodayDayStyle");
			}

			{
				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				copy.RenderControl (tw);
				Assert.AreEqual (true, sw.ToString().ToLower().IndexOf ("brown") != -1, "WeekendDayStyle");
			}
		}

		string tofind = "";
		public void Event_TestDayRenderCellAdd_DayRender(object sender, DayRenderEventArgs e) {
			if (e.Day.Date.Day == 1)
				e.Cell.Controls.Add (new LiteralControl (tofind));	
		}
		[Test]
		public void TestDayRenderCellAdd ()
		{
			PokerCalendar p = new PokerCalendar ();
			tofind = Guid.NewGuid ().ToString ();

			p.DayRender += new DayRenderEventHandler(Event_TestDayRenderCellAdd_DayRender);

			Assert.IsTrue (p.Render ().IndexOf (tofind) != -1, "control added");
		}

		//
		// Here we test rendering May 2005
		//      April 2005             May 2005             June 2005
		// Su Mo Tu We Th Fr Sa  Su Mo Tu We Th Fr Sa  Su Mo Tu We Th Fr Sa
		//                1  2   1  2  3  4  5  6  7            1  2  3  4
		// 3  4  5  6  7  8  9   8  9  10 11 12 13 14   5  6  7  8  9 10 11
		// 10 11 12 13 14 15 16  15 16 17 18 19 20 21  12 13 14 15 16 17 18
		// 17 18 19 20 21 22 23  22 23 24 25 26 27 28  19 20 21 22 23 24 25
		// 24 25 26 27 28 29 30  29 30 31              26 27 28 29 30
		//
		// Microsoft renders months like this (where Blah 1st falls on Sunday) by rendering
		// the last week of the other month.
		//
		bool first = true;
		public void Event_TestRenderMonthStartsOnSunday_DayRender(object sender, DayRenderEventArgs e) {
			if (first) {
				Assert.IsTrue (e.Day.IsOtherMonth);
				Assert.AreEqual (new DateTime (2005, 4, 24), e.Day.Date);
				first = false;
			}
		}
		[Test]
		public void TestRenderMonthStartsOnSunday ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
			PokerCalendar p = new PokerCalendar ();
			p.TodaysDate = new DateTime (2005, 5, 14);
			
			first = true;
			p.DayRender += new DayRenderEventHandler(Event_TestRenderMonthStartsOnSunday_DayRender);
			p.Render ();
		}

		[Test]
		public void TestSelectedColorDefault ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.TodaysDate = new DateTime (2005, 8, 4);
			p.SelectedDate = p.TodaysDate;
			string s = p.Render();
			Assert.IsTrue (s.IndexOf ("color:White") != 1 && s.IndexOf ("background-color:Silver") != -1, "A1");
		}

		[Test]
		public void HaveID ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.ID = "hola";
			p.TodaysDate = new DateTime (2005, 8, 4);
			p.SelectedDate = p.TodaysDate;
			string s = p.Render();
			Assert.IsTrue (s.IndexOf ("id=\"hola\"") != -1, "#01");
		}

		/*
		* Not meant to be run. Just to get a stack trace.
		[Test]
		public void NoCreateStyleCollection ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.ID = "hola";
			p.TodaysDate = new DateTime (2005, 8, 4);
			p.SelectedDate = p.TodaysDate;
			string s = p.Render();
			Assert.IsTrue (p.CS_Called == false, "#01");
		}
		*/

		[Test]
		[Category ("NotWorking")] 
		public void HaveBaseAttributes ()
		{
			PokerCalendar p = new PokerCalendar ();
			p.ID = "hola";
			p.ToolTip = "adios";
			p.TodaysDate = new DateTime (2005, 8, 4);
			p.SelectedDate = p.TodaysDate;
			string s = p.Render();
			Assert.IsTrue (s.IndexOf ("adios") != -1, "#01");
			Assert.IsTrue (p.CS_Called == true, "#02");
		}
	}
}

