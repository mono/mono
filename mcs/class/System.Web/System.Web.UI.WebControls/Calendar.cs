//
// System.Web.UI.WebControls.Calendar.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) Copyright 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("SelectionChanged")]
	[DefaultProperty("SelectedDate")]
	[Designer("System.Web.UI.Design.WebControls.CalendarDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.WebControls.CalendarDataBindingHandler, " + Consts.AssemblySystem_Design)]
	public class Calendar : WebControl, IPostBackEventHandler
	{
		//
		private TableItemStyle          dayHeaderStyle;
		private TableItemStyle          dayStyle;
		private TableItemStyle          nextPrevStyle;
		private TableItemStyle          otherMonthDayStyle;
		private SelectedDatesCollection selectedDates;
		private ArrayList               selectedDatesList;
		private TableItemStyle          selectedDayStyle;
		private TableItemStyle          selectorStyle;
		private TableItemStyle          titleStyle;
		private TableItemStyle          todayDayStyle;
		private TableItemStyle          weekendDayStyle;

		private static readonly object DayRenderEvent           = new object();
		private static readonly object SelectionChangedEvent    = new object();
		private static readonly object VisibleMonthChangedEvent = new object();

		private Color defaultTextColor;
		private System.Globalization.Calendar globCal;
		private DateTimeFormatInfo infoCal = DateTimeFormatInfo.CurrentInfo;

		private static readonly DateTime begin_date = new DateTime (2000,01,01,0,0,0,0);
				

		private static int MASK_WEEKEND  = (0x01 << 0);
		private static int MASK_OMONTH   = (0x01 << 1);
		private static int MASK_TODAY    = (0x01 << 2);
		private static int MASK_SELECTED = (0x01 << 3);
		private static int MASK_DAY      = (0x01 << 4);
		private static int MASK_UNIQUE = MASK_WEEKEND | MASK_OMONTH | MASK_TODAY | MASK_SELECTED;

		public Calendar(): base()
		{
		}


		[DefaultValue (2), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The border left within the calendar days.")]
		public int CellPadding
		{
			get
			{
				object o = ViewState["CellPadding"];
				if(o!=null)
					return (int)o;
				return 2;
			}
			set
			{
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "CellPadding value has to be -1 for 'not set' or > -1.");
				ViewState["CellPadding"] = value;
			}
		}

		[DefaultValue (0), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The border left between calendar days.")]
		public int CellSpacing
		{
			get
			{
				object o = ViewState["CellSpacing"];
				if(o!=null)
					return (int)o;
				return 0;
			}
			set
			{
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "CellSpacing value has to be -1 for 'not set' or > -1.");
				ViewState["CellSpacing"] = value;
			}
		}

		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the day header.")]
		public TableItemStyle DayHeaderStyle
		{
			get
			{
				if(dayHeaderStyle==null)
					dayHeaderStyle = new TableItemStyle();
				if(IsTrackingViewState)
					dayHeaderStyle.TrackViewState();
				return dayHeaderStyle;
			}
		}

		[DefaultValue (typeof (DayNameFormat), "Short"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The format for the day name display.")]
		public DayNameFormat DayNameFormat
		{
			get
			{
				object o = ViewState["DayNameFormat"];
				if(o!=null)
					return (DayNameFormat)o;
				return DayNameFormat.Short;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(DayNameFormat),value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["DayNameFormat"] = value;
			}
		}

		[DefaultValue (null)]
		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the day entry.")]
		public TableItemStyle DayStyle
		{
			get
			{
				if(dayStyle==null)
					dayStyle = new TableItemStyle();
				if(IsTrackingViewState)
					dayStyle.TrackViewState();
				return dayStyle;
			}
		}

		[DefaultValue (typeof (FirstDayOfWeek), "Default"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The day that a week begins with.")]
		public FirstDayOfWeek FirstDayOfWeek
		{
			get
			{
				object o = ViewState["FirstDayOfWeek"];
				if(o!=null)
					return (FirstDayOfWeek)o;
				return FirstDayOfWeek.Default;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(FirstDayOfWeek), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["FirstDayOfWeek"] = value;
			}
		}

		[DefaultValue (">"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text for selecting the next month.")]
		public string NextMonthText
		{
			get
			{
				object o = ViewState["NextMonthText"];
				if(o!=null)
					return (string)o;
				return "&gt;";
			}
			set
			{
				ViewState["NextMonthText"] = value;
			}
		}

		[DefaultValue (typeof (NextPrevFormat), "CustomText"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The format for the month navigation.")]
		public NextPrevFormat NextPrevFormat
		{
			get
			{
				object o = ViewState["NextPrevFormat"];
				if(o!=null)
					return (NextPrevFormat)o;
				return NextPrevFormat.CustomText;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(NextPrevFormat), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["NextPrevFormat"] = value;
			}
		}

		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the month navigation.")]
		public TableItemStyle NextPrevStyle
		{
			get
			{
				if(nextPrevStyle == null)
					nextPrevStyle = new TableItemStyle();
				if(IsTrackingViewState)
					nextPrevStyle.TrackViewState();
				return nextPrevStyle;
			}
		}

		[DefaultValue (null)]
		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to day entries belonging to another month.")]
		public TableItemStyle OtherMonthDayStyle
		{
			get
			{
				if(otherMonthDayStyle == null)
					otherMonthDayStyle = new TableItemStyle();
				if(IsTrackingViewState)
					otherMonthDayStyle.TrackViewState();
				return otherMonthDayStyle;
			}
		}

		[DefaultValue ("<"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text for selecting the previous month.")]
		public string PrevMonthText
		{
			get
			{
				object o = ViewState["PrevMonthText"];
				if(o!=null)
					return (string)o;
				return "&lt;";
			}
			set
			{
				ViewState["PrevMonthText"] = value;
			}
		}

		[DefaultValue (null), Bindable (true)]
		[WebSysDescription ("The currently selected date.")]
		public DateTime SelectedDate
		{
			get
			{
				if(SelectedDates.Count > 0)
				{
					return SelectedDates[0];
				}
				return DateTime.MinValue;
			}
			set
			{
				if(value == DateTime.MinValue)
				{
					SelectedDates.Clear();
				} else
				{
					SelectedDates.SelectRange(value, value);
				}
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("All currently selected dates.")]
		public SelectedDatesCollection SelectedDates
		{
			get
			{
				if(selectedDates==null)
				{
					if(selectedDatesList == null)
						selectedDatesList = new ArrayList();
					selectedDates = new SelectedDatesCollection(selectedDatesList);
				}
				return selectedDates;
			}
		}

		[DefaultValue (null)]
		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the selected day.")]
		public TableItemStyle SelectedDayStyle
		{
			get
			{
				if(selectedDayStyle==null)
					selectedDayStyle = new TableItemStyle();
				if(IsTrackingViewState)
					selectedDayStyle.TrackViewState();
				return selectedDayStyle;
			}
		}

		[DefaultValue (typeof (CalendarSelectionMode), "Day"), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The mode in which days or other entries are selected.")]
		public CalendarSelectionMode SelectionMode
		{
			get
			{
				object o = ViewState["SelectionMode"];
				if(o!=null)
					return (CalendarSelectionMode)o;
				return CalendarSelectionMode.Day;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(CalendarSelectionMode), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["SelectionMode"] = value;
			}
		}

		[DefaultValue (">>"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text that is used for selection of months.")]
		public string SelectMonthText
		{
			get
			{
				object o = ViewState["SelectMonthText"];
				if(o!=null)
					return (string)o;
				return "&gt;&gt;";
			}
			set
			{
				ViewState["SelectMonthText"] = value;
			}
		}

		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the selector.")]
		public TableItemStyle SelectorStyle
		{
			get
			{
				if(selectorStyle==null)
					selectorStyle = new TableItemStyle();
				return selectorStyle;
			}
		}

		[DefaultValue (">"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text that is used for selection of weeks.")]
		public string SelectWeekText
		{
			get
			{
				object o = ViewState["SelectWeekText"];
				if(o!=null)
					return (string)o;
				return "&gt;";
			}
			set
			{
				ViewState["SelectWeekText"] = value;
			}
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("Determines if the header for days is shown.")]
		public bool ShowDayHeader
		{
			get
			{
				object o = ViewState["ShowDayHeader"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowDayHeader"] = value;
			}
		}

		[DefaultValue (false), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("Determines if gridlines are shown.")]
		public bool ShowGridLines
		{
			get
			{
				object o = ViewState["ShowGridLines"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowGridLines"] = value;
			}
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("Determines if month navigation is shown.")]
		public bool ShowNextPrevMonth
		{
			get
			{
				object o = ViewState["ShowNextPrevMonth"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowNextPrevMonth"] = value;
			}
		}

		[DefaultValue (true), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("Determines if the title is shown.")]
		public bool ShowTitle
		{
			get
			{
				object o = ViewState["ShowTitle"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowTitle"] = value;
			}
		}

		[DefaultValue (typeof (TitleFormat), "MonthYear"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The format in which the title is rendered.")]
		public TitleFormat TitleFormat
		{
			get
			{
				object o = ViewState["TitleFormat"];
				if(o!=null)
					return (TitleFormat)o;
				return TitleFormat.MonthYear;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(TitleFormat), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["TitleFormat"] = value;
			}
		}

		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the title.")]
		public TableItemStyle TitleStyle
		{
			get
			{
				if(titleStyle==null)
					titleStyle = new TableItemStyle();
				if(IsTrackingViewState)
					titleStyle.TrackViewState();
				return titleStyle;
			}
		}

		[DefaultValue (null)]
		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to the today's date display.")]
		public TableItemStyle TodayDayStyle
		{
			get
			{
				if(todayDayStyle==null)
					todayDayStyle = new TableItemStyle();
				if(IsTrackingViewState)
					todayDayStyle.TrackViewState();
				return todayDayStyle;
			}
		}

		[Bindable (true), Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The current date.")]
		public DateTime TodaysDate
		{
			get
			{
				object o = ViewState["TodaysDate"];
				if(o!=null)
					return (DateTime)o;
				return DateTime.Today;
			}
			set
			{
				ViewState["TodaysDate"] = value.Date;
			}
		}

		[DefaultValue (null), Bindable (true)]
		[WebSysDescription ("The month that is displayed.")]
		public DateTime VisibleDate
		{
			get
			{
				object o = ViewState["VisibleDate"];
				if(o!=null)
					return (DateTime)o;
				return DateTime.MinValue;
			}
			set
			{
				ViewState["VisibleDate"] = value.Date;
			}
		}

		[NotifyParentProperty (true), WebCategory ("Style")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("The style applied to weekend days.")]
		public TableItemStyle WeekendDayStyle
		{
			get
			{
				if(weekendDayStyle == null)
					weekendDayStyle = new TableItemStyle();
				if(IsTrackingViewState)
				{
					weekendDayStyle.TrackViewState();
				}
				return weekendDayStyle;
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a day entry is rendered.")]
		public event DayRenderEventHandler DayRender
		{
			add
			{
				Events.AddHandler(DayRenderEvent, value);
			}
			remove
			{
				Events.RemoveHandler(DayRenderEvent, value);
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when another entry is selected.")]
		public event EventHandler SelectionChanged
		{
			add
			{
				Events.AddHandler(SelectionChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SelectionChangedEvent, value);
			}
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a the currently visible month has changed.")]
		public event MonthChangedEventHandler VisibleMonthChanged
		{
			add
			{
				Events.AddHandler(VisibleMonthChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(VisibleMonthChangedEvent, value);
			}
		}

		protected virtual void OnDayRender(TableCell cell, CalendarDay day)
		{
			if(Events!=null)
			{
				DayRenderEventHandler dreh = (DayRenderEventHandler)(Events[DayRenderEvent]);
				if(dreh!=null)
					dreh(this, new DayRenderEventArgs(cell, day));
			}
		}

		protected virtual void OnSelectionChanged()
		{
			if(Events!=null)
			{
				EventHandler eh = (EventHandler)(Events[SelectionChangedEvent]);
				if(eh!=null)
					eh(this, new EventArgs());
			}
		}

		protected virtual void OnVisibleMonthChanged(DateTime newDate, DateTime prevDate)
		{
			if(Events!=null)
			{
				MonthChangedEventHandler mceh = (MonthChangedEventHandler)(Events[VisibleMonthChangedEvent]);
				if(mceh!=null)
					mceh(this, new MonthChangedEventArgs(newDate, prevDate));
			}
		}

		/// <remarks>
		/// See test6.aspx in Tests directory for verification
		/// </remarks>
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			// initialize the calendar...TODO: find out why this isn't done in the constructor
			// if the culture is changed between rendering and postback this will be broken
			globCal = DateTimeFormatInfo.CurrentInfo.Calendar;

			// TODO: Find out what kind of exceptions to throw when we get bad data
			if (eventArgument.StartsWith ("V")) {
				TimeSpan mod = new TimeSpan (Int32.Parse (eventArgument.Substring (1)), 0, 0, 0);
				DateTime new_date = begin_date + mod;
				VisibleDate = new_date;
				OnVisibleMonthChanged (VisibleDate, new_date);
			} else if (eventArgument.StartsWith ("R")) {
				TimeSpan mod = new TimeSpan (Int32.Parse (eventArgument.Substring (1,
											  eventArgument.Length - 3)), 0, 0, 0);
				DateTime range_begin = begin_date + mod;
				int sel_days = Int32.Parse (eventArgument.Substring (eventArgument.Length - 2));
				SelectRangeInternal (range_begin, range_begin.AddDays (sel_days - 1), GetEffectiveVisibleDate ());
			} else {
				TimeSpan mod = new TimeSpan (Int32.Parse (eventArgument), 0, 0, 0);
				DateTime day = begin_date + mod;
				SelectRangeInternal (day, day, GetEffectiveVisibleDate ());
			}
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			globCal = DateTimeFormatInfo.CurrentInfo.Calendar;
			DateTime visDate   = GetEffectiveVisibleDate();
			DateTime firstDate = GetFirstCalendarDay(visDate);

			bool isEnabled;
			bool isHtmlTextWriter;
			//FIXME: when Control.Site works, reactivate this
			//if (Page == null || Site == null) {
			//	isEnabled = false;
			//	isHtmlTextWriter = false;
			//} else {
				isEnabled = Enabled;
				isHtmlTextWriter = (writer.GetType() != typeof(HtmlTextWriter));
			//}
			defaultTextColor = ForeColor;
			if(defaultTextColor == Color.Empty)
				defaultTextColor = Color.Black;

			Table calTable = new Table ();
			calTable.ID = ID;
			calTable.CopyBaseAttributes(this);
			if(ControlStyleCreated)
				calTable.ApplyStyle(ControlStyle);
			calTable.Width = Width;
			calTable.Height = Height;
			calTable.CellSpacing = CellSpacing;
			calTable.CellPadding = CellPadding;

			if (ControlStyleCreated &&
			    ControlStyle.IsSet (WebControls.Style.BORDERWIDTH) &&
			    BorderWidth != Unit.Empty)
				calTable.BorderWidth = BorderWidth;
			else
				calTable.BorderWidth = Unit.Pixel(1);

			if (ShowGridLines)
				calTable.GridLines = GridLines.Both;
			else
				calTable.GridLines = GridLines.None;

			calTable.RenderBeginTag (writer);

			if (ShowTitle)
				RenderTitle (writer, visDate, SelectionMode, isEnabled);

			if (ShowDayHeader)
				RenderHeader (writer, firstDate, SelectionMode, isEnabled, isHtmlTextWriter);

			RenderAllDays (writer, firstDate, visDate, SelectionMode, isEnabled, isHtmlTextWriter);

			calTable.RenderEndTag(writer);
		}

		protected override ControlCollection CreateControlCollection()
		{
			return new EmptyControlCollection(this);
		}

		protected override void LoadViewState(object savedState)
		{
			if (savedState!=null) {
				object[] states = (object[]) savedState;
				if(states[0] != null)
					base.LoadViewState(states[0]);
				if(states[1] != null)
					DayHeaderStyle.LoadViewState(states[1]);
				if(states[2] != null)
					DayStyle.LoadViewState(states[2]);
				if(states[3] != null)
					NextPrevStyle.LoadViewState(states[3]);
				if(states[4] != null)
					OtherMonthDayStyle.LoadViewState(states[4]);
				if(states[5] != null)
					SelectedDayStyle.LoadViewState(states[5]);
				if(states[6] != null)
					SelectorStyle.LoadViewState(states[6]);
				if(states[7] != null)
					TitleStyle.LoadViewState(states[7]);
				if(states[8] != null)
					TodayDayStyle.LoadViewState(states[8]);
				if(states[9] != null)
					WeekendDayStyle.LoadViewState(states[9]);

				ArrayList dateList = ViewState ["_CalendarSelectedDates"] as ArrayList;
				if (dateList != null)
					selectedDates = new SelectedDatesCollection (dateList);
			}
		}

		protected override object SaveViewState()
		{
			if (SelectedDates.Count > 0)
				ViewState["_CalendarSelectedDates"] = selectedDates.GetDateList ();
			
			object[] states = new object [10];
			states[0] = base.SaveViewState();
			states[1] = (dayHeaderStyle == null ? null : dayHeaderStyle.SaveViewState());
			states[2] = (dayStyle == null ? null : dayStyle.SaveViewState());
			states[3] = (nextPrevStyle == null ? null : nextPrevStyle.SaveViewState());
			states[4] = (otherMonthDayStyle == null ? null : otherMonthDayStyle.SaveViewState());
			states[5] = (selectedDayStyle == null ? null : selectedDayStyle.SaveViewState());
			states[6] = (selectorStyle == null ? null : selectorStyle.SaveViewState());
			states[7] = (titleStyle == null ? null : titleStyle.SaveViewState());
			states[8] = (todayDayStyle == null ? null : todayDayStyle.SaveViewState());
			states[9] = (weekendDayStyle == null ? null : weekendDayStyle.SaveViewState());
			for(int i=0; i < states.Length; i++)
			{
				if(states[i]!=null)
					return states;
			}
			return null;
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			if(titleStyle!=null)
			{
				titleStyle.TrackViewState();
			}
			if(nextPrevStyle!=null)
			{
				nextPrevStyle.TrackViewState();
			}
			if(dayStyle!=null)
			{
				dayStyle.TrackViewState();
			}
			if(dayHeaderStyle!=null)
			{
				dayHeaderStyle.TrackViewState();
			}
			if(todayDayStyle!=null)
			{
				todayDayStyle.TrackViewState();
			}
			if(weekendDayStyle!=null)
			{
				weekendDayStyle.TrackViewState();
			}
			if(otherMonthDayStyle!=null)
			{
				otherMonthDayStyle.TrackViewState();
			}
			if(selectedDayStyle!=null)
			{
				selectedDayStyle.TrackViewState();
			}
			if(selectorStyle!=null)
			{
				selectorStyle.TrackViewState();
			}
		}

		private void RenderAllDays (HtmlTextWriter writer,
					    DateTime firstDay,
					    DateTime activeDate,
					    CalendarSelectionMode mode,
					    bool isActive,
					    bool isDownLevel)
		{
			TableItemStyle weeksStyle = null;
			TableCell weeksCell = new TableCell ();
			TableItemStyle weekendStyle = WeekendDayStyle;
			TableItemStyle otherMonthStyle = OtherMonthDayStyle;
			Unit size;
			bool isWeekMode = (mode == CalendarSelectionMode.DayWeek ||
					   mode == CalendarSelectionMode.DayWeekMonth);

			if (isWeekMode) {
				weeksStyle = new TableItemStyle ();
				weeksStyle.Width = Unit.Percentage (12);
				weeksStyle.HorizontalAlign = HorizontalAlign.Center;
				weeksStyle.CopyFrom (SelectorStyle);
				size = Unit.Percentage (12);
			} else {
				size = Unit.Percentage (14);
			}

			TableItemStyle [] styles = new TableItemStyle [32];
			int definedStyles = MASK_SELECTED;
			if (weekendStyle != null && !weekendStyle.IsEmpty)
				definedStyles |= MASK_WEEKEND;
			if (otherMonthStyle != null && !otherMonthStyle.IsEmpty)
				definedStyles |= MASK_OMONTH;
			if (todayDayStyle != null && !todayDayStyle.IsEmpty)
				definedStyles |= MASK_TODAY;
			if (dayStyle != null && !dayStyle.IsEmpty)
				definedStyles |= MASK_DAY;

			int month = globCal.GetMonth (activeDate);
			DateTime currentDay = firstDay;
			int begin = (int) (firstDay - begin_date).TotalDays;
			for (int crr = 0; crr < 6; crr++) {
				writer.Write ("<tr>");
				if (isWeekMode) {
					int week_offset = begin + crr * 7;
					string cellText = GetCalendarLinkText (
								"R" + week_offset + "07",
								SelectWeekText, 
								weeksCell.ForeColor,
								isActive);

					weeksCell.Text = cellText;
					weeksCell.ApplyStyle (weeksStyle);
					RenderCalendarCell (writer, weeksCell, cellText);
				}

				for (int weekDay = 0; weekDay < 7; weekDay++) {
					string dayString = currentDay.Day.ToString ();
					DayOfWeek dow = currentDay.DayOfWeek;
					CalendarDay calDay =
						new CalendarDay (
								currentDay,
								dow == DayOfWeek.Sunday ||
								dow == DayOfWeek.Saturday,
								currentDay == TodaysDate, 
								SelectedDates.Contains (currentDay),
								globCal.GetMonth (currentDay) != month,
								dayString
								);


					int dayStyles = GetMask (calDay) & definedStyles;
					TableItemStyle currentDayStyle = styles [dayStyles];
					if (currentDayStyle == null) {
						currentDayStyle = new TableItemStyle ();
						if ((dayStyles & MASK_DAY) != 0)
							currentDayStyle.CopyFrom (DayStyle);

						if ((dayStyles & MASK_WEEKEND) != 0)
							currentDayStyle.CopyFrom (WeekendDayStyle);

						if ((dayStyles & MASK_TODAY) != 0)
							currentDayStyle.CopyFrom (TodayDayStyle);

						if ((dayStyles & MASK_OMONTH) != 0)
							currentDayStyle.CopyFrom (OtherMonthDayStyle);

						if ((dayStyles & MASK_SELECTED) != 0) {
							currentDayStyle.ForeColor = Color.White;
							currentDayStyle.BackColor = Color.Silver;
							currentDayStyle.CopyFrom (SelectedDayStyle);
						}

						currentDayStyle.Width = size;
						currentDayStyle.HorizontalAlign = HorizontalAlign.Center;
					}

					TableCell dayCell = new TableCell ();
					dayCell.ApplyStyle (currentDayStyle);
					dayCell.Controls.Add (new LiteralControl (dayString));
					calDay.IsSelectable = isActive;
					if (calDay.IsSelectable)
						dayCell.Text = GetCalendarLinkText (
									(begin + (crr * 7 + weekDay)).ToString (),
									dayString,
									dayCell.ForeColor,
									isActive);

					OnDayRender (dayCell, calDay);
					dayCell.RenderControl (writer);

					currentDay = globCal.AddDays (currentDay, 1);
				}
				writer.Write("</tr>");
			}
		}

		private int GetMask (CalendarDay day)
		{
			int retVal = MASK_DAY;
			if(day.IsSelected)
				retVal |= MASK_SELECTED;
			if(day.IsToday)
				retVal |= MASK_TODAY;
			if(day.IsOtherMonth)
				retVal |= MASK_OMONTH;
			if(day.IsWeekend)
				retVal |= MASK_WEEKEND;
			return retVal;
		}

		/// <remarks>
		/// Refers to the second line of the calendar, that contains a link
		/// to select whole month, and weekdays as defined by DayNameFormat
		/// </remarks>
		private void RenderHeader (HtmlTextWriter writer,
					   DateTime firstDay,
					   CalendarSelectionMode mode,
					   bool isActive,
					   bool isDownLevel)
		{
			writer.Write("<tr>");
			bool isWeekMode = (mode == CalendarSelectionMode.DayWeek ||
					   mode == CalendarSelectionMode.DayWeekMonth);

			TableCell headerCell = new TableCell ();
			headerCell.HorizontalAlign = HorizontalAlign.Center;
			string selMthText = String.Empty;
			if (isWeekMode) {
				if (mode == CalendarSelectionMode.DayWeekMonth) {
					DateTime visDate = GetEffectiveVisibleDate ();
					DateTime sel_month = new DateTime (visDate.Year, visDate.Month, 1);
					int month_offset = (int) (sel_month - begin_date).TotalDays;
					headerCell.ApplyStyle (SelectorStyle);
					selMthText = GetCalendarLinkText ("R" + month_offset +
							globCal.GetDaysInMonth (sel_month.Year,
									sel_month.Month).ToString ("d2"), // maybe there are calendars with less then 10 days in a month
									  SelectMonthText,
									  SelectorStyle.ForeColor,
									  isActive);
				} else {
					headerCell.ApplyStyle (DayHeaderStyle);
					selMthText = String.Empty;
				}
				RenderCalendarCell (writer, headerCell, selMthText);
			}

			TableCell dayHeaderCell = new TableCell ();
			dayHeaderCell.HorizontalAlign = HorizontalAlign.Center;
			dayHeaderCell.ApplyStyle (dayHeaderStyle);

			int dayOfWeek = (int) globCal.GetDayOfWeek (firstDay);
			DateTimeFormatInfo currDTInfo = DateTimeFormatInfo.CurrentInfo;
			for(int currDay = dayOfWeek; currDay < dayOfWeek + 7; currDay++) {
				DayOfWeek effDay = (DayOfWeek) Enum.ToObject (typeof (DayOfWeek), currDay % 7);
				string currDayContent;
				switch(DayNameFormat) {
				case DayNameFormat.Full:
					currDayContent = currDTInfo.GetDayName (effDay);
					break;
				case DayNameFormat.FirstLetter:
					currDayContent = currDTInfo.GetDayName (effDay).Substring (0,1);
					break;
				case DayNameFormat.FirstTwoLetters:
					currDayContent = currDTInfo.GetDayName (effDay).Substring (0,2);
					break;
				case DayNameFormat.Short:
					goto default;
				default:
					currDayContent = currDTInfo.GetAbbreviatedDayName (effDay);
					break;
				}

				RenderCalendarCell(writer, dayHeaderCell, currDayContent);
			}
			writer.Write ("</tr>");
		}

		private void RenderTitle (HtmlTextWriter writer,
					  DateTime visibleDate,
					  CalendarSelectionMode mode,
					  bool isActive)
		{
			writer.Write("<tr>");
			Table innerTable = new Table ();
			TableCell titleCell = new TableCell();
			bool isWeekMode = (mode == CalendarSelectionMode.DayWeek ||
					   mode == CalendarSelectionMode.DayWeekMonth);

			titleCell.ColumnSpan = (isWeekMode ? 8 : 7);
			titleCell.BackColor = Color.Silver;

			innerTable.GridLines = GridLines.None;
			innerTable.Width = Unit.Percentage (100);
			innerTable.CellSpacing = 0;
			ApplyTitleStyle (innerTable, titleCell, TitleStyle);

			titleCell.RenderBeginTag (writer);
			innerTable.RenderBeginTag (writer);

			writer.Write ("<tr>");
			string prevContent = String.Empty;
			if (ShowNextPrevMonth) {
				TableCell prevCell = new TableCell ();
				prevCell.Width = Unit.Percentage (15);
				prevCell.HorizontalAlign = HorizontalAlign.Left;
				if (NextPrevFormat == NextPrevFormat.CustomText) {
					prevContent = PrevMonthText;
				} else {
					int pMthInt = globCal.GetMonth(globCal.AddMonths (visibleDate, -1));
					if (NextPrevFormat == NextPrevFormat.FullMonth)
						prevContent = infoCal.GetMonthName (pMthInt);
					else
						prevContent = infoCal.GetAbbreviatedMonthName (pMthInt);
				}
				DateTime prev_month = visibleDate.AddMonths (-1);
				int prev_offset = (int) (new DateTime (prev_month.Year,
									  prev_month.Month, 1) - begin_date).TotalDays;
				prevCell.ApplyStyle (NextPrevStyle);
				RenderCalendarCell (writer,
						    prevCell,
						    GetCalendarLinkText ("V" + prev_offset,
							    		 prevContent,
									 NextPrevStyle.ForeColor,
									 isActive)
						    );
			}

			TableCell currCell = new TableCell ();
			currCell.Width = Unit.Percentage (70);
			if (TitleStyle.HorizontalAlign == HorizontalAlign.NotSet)
				currCell.HorizontalAlign = HorizontalAlign.Center;
			else
				currCell.HorizontalAlign = TitleStyle.HorizontalAlign;

			currCell.Wrap = TitleStyle.Wrap;
			string currMonthContent = String.Empty;
			if (TitleFormat == TitleFormat.Month) {
				currMonthContent = visibleDate.ToString ("MMMM");
			} else {
				string cmcFmt = infoCal.YearMonthPattern;
				if (cmcFmt.IndexOf (',') >= 0)
					cmcFmt = "MMMM yyyy";

				currMonthContent = visibleDate.ToString (cmcFmt);
			}

			RenderCalendarCell (writer, currCell, currMonthContent);
								 
			string nextContent = String.Empty;
			if (ShowNextPrevMonth) {
				TableCell nextCell = new TableCell ();
				nextCell.Width = Unit.Percentage(15);
				nextCell.HorizontalAlign = HorizontalAlign.Right;
				if (NextPrevFormat == NextPrevFormat.CustomText) {
					nextContent = NextMonthText;
				} else {
					int nMthInt = globCal.GetMonth (globCal.AddMonths (visibleDate, 1));
					if(NextPrevFormat == NextPrevFormat.FullMonth)
						nextContent = infoCal.GetMonthName(nMthInt);
					else
						nextContent = infoCal.GetAbbreviatedMonthName(nMthInt);
				}
				DateTime next_month = visibleDate.AddMonths (1);
				int next_offset = (int) (new DateTime (next_month.Year,
									  next_month.Month, 1) - begin_date).TotalDays;
				nextCell.ApplyStyle(NextPrevStyle);
				RenderCalendarCell (writer,
						    nextCell,
						    GetCalendarLinkText ("V" + next_offset,
									 nextContent,
									 NextPrevStyle.ForeColor,
									 isActive)
						    );
			}

			writer.Write("</tr>");
			innerTable.RenderEndTag(writer);
			titleCell.RenderEndTag(writer);

			writer.Write("</tr>");
		}

		private void ApplyTitleStyle(Table table, TableCell cell, TableItemStyle style)
		{
			if(style.BackColor != Color.Empty)
			{
				cell.BackColor = style.BackColor;
			}
			if(style.BorderStyle != BorderStyle.NotSet)
			{
				cell.BorderStyle = style.BorderStyle;
			}
			if(style.BorderColor != Color.Empty)
			{
				cell.BorderColor = style.BorderColor;
			}
			if(style.BorderWidth != Unit.Empty)
			{
				cell.BorderWidth = style.BorderWidth;
			}
			if(style.Height != Unit.Empty)
			{
				cell.Height = style.Height;
			}
			if(style.VerticalAlign != VerticalAlign.NotSet)
			{
				cell.VerticalAlign = style.VerticalAlign;
			}

			if(style.ForeColor != Color.Empty)
			{
				table.ForeColor = style.ForeColor;
			} else if(ForeColor != Color.Empty)
			{
				table.ForeColor = ForeColor;
			}

			if (style.CssClass != "") {
				cell.CssClass = style.CssClass;
			}

			table.Font.CopyFrom(style.Font);
			table.Font.MergeWith(Font);
		}

		private void RenderCalendarCell (HtmlTextWriter writer, TableCell cell, string text)
		{
			cell.RenderBeginTag(writer);
			writer.Write(text);
			cell.RenderEndTag(writer);
		}

		private DateTime GetFirstCalendarDay(DateTime visibleDate)
		{
			int fow = (int) FirstDayOfWeek;
			if (fow == 7)
				fow = (int) infoCal.FirstDayOfWeek;

			int days = (int) globCal.GetDayOfWeek (visibleDate) - fow;
			if (days <= 0)
				days += 7;
			return globCal.AddDays (visibleDate, -days);
		}

		private DateTime GetEffectiveVisibleDate()
		{
			DateTime dt = VisibleDate;
			if (VisibleDate == DateTime.MinValue)
				 dt = TodaysDate;

			return globCal.AddDays (dt, 1 - globCal.GetDayOfMonth (dt));
		}

		/// <summary>
		/// Creates text to be displayed, with all attributes if to be
		/// shown as a hyperlink
		/// </summary>
		private string GetCalendarLinkText (string eventArg,
						    string text,
						    Color foreground,
						    bool isLink)
		{
			if (isLink) {
				StringBuilder dispVal = new StringBuilder ();
				dispVal.Append ("<a href=\"");
				dispVal.Append (Page.GetPostBackClientHyperlink (this, eventArg));
				dispVal.Append ("\" style=\"color: ");
				if (foreground.IsEmpty) {
					dispVal.Append (ColorTranslator.ToHtml (defaultTextColor));
				} else {
					dispVal.Append (ColorTranslator.ToHtml (foreground));
				}
				dispVal.Append ("\">");
				dispVal.Append (text);
				dispVal.Append ("</a>");
				return dispVal.ToString ();
			}
			return text;
		}

		private string GetHtmlForCell (TableCell cell, bool showLinks)
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			cell.RenderBeginTag (htw);
			if(showLinks) {
				htw.Write (GetCalendarLinkText ("{0}", "{1}", cell.ForeColor, showLinks));
			} else {
				htw.Write ("{0}");
			}
			cell.RenderEndTag (htw);
			return sw.ToString();
		}

		internal void SelectRangeInternal (DateTime fromDate, DateTime toDate, DateTime visibleDate)
		{
			TimeSpan span = toDate - fromDate;
			if (SelectedDates.Count != span.Days + 1 || 
			    SelectedDates [0] != fromDate || 
			    SelectedDates [SelectedDates.Count - 1] != toDate) {
				SelectedDates.SelectRange (fromDate, toDate);
				OnSelectionChanged ();
			}
		}

		protected bool HasWeekSelectors (CalendarSelectionMode selectionMode)
		{
			return selectionMode == CalendarSelectionMode.DayWeek ||
				selectionMode == CalendarSelectionMode.DayWeekMonth;
		}
	}
}
