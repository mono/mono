//
// System.Web.UI.WebControls.Calendar.cs
//
// Authors:
//    Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
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
//

using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Threading;
using System.Text;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DataBindingHandler("System.Web.UI.Design.WebControls.CalendarDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent("SelectionChanged")]
	[DefaultProperty("SelectedDate")]
	[Designer("System.Web.UI.Design.WebControls.CalendarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ControlValueProperty ("SelectedDate", "1/1/0001 12:00:00 AM")]
	[SupportsEventValidation]
	public class Calendar : WebControl, IPostBackEventHandler
	{
		TableItemStyle dayHeaderStyle;
		TableItemStyle dayStyle;
		TableItemStyle nextPrevStyle;
		TableItemStyle otherMonthDayStyle;
		TableItemStyle selectedDayStyle;
		TableItemStyle titleStyle;
		TableItemStyle todayDayStyle;
		TableItemStyle selectorStyle;
		TableItemStyle weekendDayStyle;
		DateTimeFormatInfo dateInfo;
		SelectedDatesCollection selectedDatesCollection;
		ArrayList dateList;
		DateTime today = DateTime.Today;
		static DateTime dateZenith  = new DateTime (2000, 1,1);
		const int daysInAWeek = 7;
		static readonly object DayRenderEvent = new object ();
		static readonly object SelectionChangedEvent = new object ();
		static readonly object VisibleMonthChangedEvent = new object ();

		public Calendar ()
		{
		}

		[Localizable (true)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Appearance")]
		public virtual string Caption {
			get { return ViewState.GetString ("Caption", String.Empty); }
			set { ViewState["Caption"] = value; }
		}

		[DefaultValue (TableCaptionAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Accessibility")]
		public virtual TableCaptionAlign CaptionAlign {
			get { return (TableCaptionAlign)ViewState.GetInt ("CaptionAlign", (int)TableCaptionAlign.NotSet); }
			set { ViewState ["CaptionAlign"] = value; }
		}

		[DefaultValue(2)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public int CellPadding {
			get { return ViewState.GetInt ("CellPadding", 2); }

			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("The specified cell padding is less than -1.");

				ViewState ["CellPadding"] = value;
			}
		}

		[DefaultValue(0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public int CellSpacing {
			get { return ViewState.GetInt ("CellSpacing", 0); }

			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("The specified cell spacing is less than -1");

				ViewState ["CellSpacing"] = value;
			}
		}

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[NotifyParentProperty(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle DayHeaderStyle {
			get {
				if (dayHeaderStyle == null) {
					dayHeaderStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						dayHeaderStyle.TrackViewState ();
				}

				return dayHeaderStyle;
			}
		}

		[DefaultValue(DayNameFormat.Short)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public DayNameFormat DayNameFormat {
			get { return (DayNameFormat) ViewState.GetInt ("DayNameFormat", (int) DayNameFormat.Short); }

			set {
				if (value != DayNameFormat.FirstLetter && value != DayNameFormat.FirstTwoLetters &&
					value != DayNameFormat.Full && value != DayNameFormat.Short && value != DayNameFormat.Shortest) {
					throw new ArgumentOutOfRangeException ("The specified day name format is not one of the DayNameFormat values.");
				}

				ViewState ["DayNameFormat"] = value;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle DayStyle {
			get {
				if (dayStyle == null) {
					dayStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						dayStyle.TrackViewState ();
				}

				return dayStyle;
			}
		}

		[DefaultValue(FirstDayOfWeek.Default)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public FirstDayOfWeek FirstDayOfWeek {
			get { return (FirstDayOfWeek) ViewState.GetInt ("FirstDayOfWeek", (int) FirstDayOfWeek.Default); }

			set {
				if (value < FirstDayOfWeek.Sunday || value > FirstDayOfWeek.Default) {
					throw new ArgumentOutOfRangeException ("The specified day name format is not one of the DayNameFormat values.");
				}

				ViewState ["FirstDayOfWeek"] = value;
			}
		}

		[DefaultValue("&gt;")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string NextMonthText {
			get { return ViewState.GetString ("NextMonthText", "&gt;"); }
			set { ViewState ["NextMonthText"] = value; }
		}

		[DefaultValue(NextPrevFormat.CustomText)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public NextPrevFormat NextPrevFormat {
			get { return (NextPrevFormat) ViewState.GetInt ("NextPrevFormat", (int) NextPrevFormat.CustomText); }

			set {
				if (value != NextPrevFormat.CustomText && value != NextPrevFormat.ShortMonth && value != NextPrevFormat.FullMonth)
					throw new ArgumentOutOfRangeException ("The specified day name format is not one of the DayNameFormat values.");

				ViewState ["NextPrevFormat"] = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle NextPrevStyle {
			get {
				if (nextPrevStyle == null) {
					nextPrevStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						nextPrevStyle.TrackViewState ();
				}

				return nextPrevStyle;
			}
		}

		[DefaultValue(null)]
		[NotifyParentProperty(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle OtherMonthDayStyle {
			get {
				if (otherMonthDayStyle == null) {
					otherMonthDayStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						otherMonthDayStyle.TrackViewState ();
				}

				return otherMonthDayStyle;
			}
		}

		[DefaultValue("&lt;")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string PrevMonthText {
			get { return ViewState.GetString ("PrevMonthText", "&lt;"); }
			set { ViewState ["PrevMonthText"] = value; }
		}

		[Bindable(true, BindingDirection.TwoWay)]
		[DefaultValue("1/1/0001 12:00:00 AM")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public DateTime SelectedDate {
			get {
				if (SelectedDates.Count > 0)
					return SelectedDates [0];

				return DateTime.MinValue;
			}

			set { SelectedDates.SelectRange (value, value); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public SelectedDatesCollection SelectedDates {
			get {
				if (dateList == null)
					dateList = new ArrayList ();

				if (selectedDatesCollection == null)
					selectedDatesCollection = new SelectedDatesCollection (dateList);

				return selectedDatesCollection;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle SelectedDayStyle {
			get {
				if (selectedDayStyle == null) {
					selectedDayStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						selectedDayStyle.TrackViewState ();
				}

				return selectedDayStyle;
			}
		}

		[DefaultValue(CalendarSelectionMode.Day)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public CalendarSelectionMode SelectionMode {
			get { return (CalendarSelectionMode) ViewState.GetInt ("SelectionMode", (int) CalendarSelectionMode.Day); }

			set {
				if (value != CalendarSelectionMode.Day  && value != CalendarSelectionMode.DayWeek &&
					value != CalendarSelectionMode.DayWeekMonth  && value != CalendarSelectionMode.None) {
					throw new ArgumentOutOfRangeException ("The specified selection mode is not one of the CalendarSelectionMode values.");
				}
				ViewState ["SelectionMode"] = value;
			}
		}

		[DefaultValue("&gt;&gt;")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string SelectMonthText {
			get { return ViewState.GetString ("SelectMonthText", "&gt;&gt;"); }
			set { ViewState ["SelectMonthText"] = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle SelectorStyle {
			get {
				if (selectorStyle == null) {
					 selectorStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						selectorStyle.TrackViewState ();
				}

				return selectorStyle;
			}
		}

		[DefaultValue("&gt;")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string SelectWeekText {
			get { return ViewState.GetString ("SelectWeekText", "&gt;"); }
			set { ViewState ["SelectWeekText"] = value; }
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public bool ShowDayHeader {
			get { return ViewState.GetBool ("ShowDayHeader", true); }
			set { ViewState ["ShowDayHeader"] = value; }
		}

		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public bool ShowGridLines {
			get { return ViewState.GetBool ("ShowGridLines", false); }
			set { ViewState ["ShowGridLines"] = value; }
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public bool ShowNextPrevMonth {
			get { return ViewState.GetBool ("ShowNextPrevMonth", true); }
			set { ViewState ["ShowNextPrevMonth"] = value; }
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public bool ShowTitle {
			get { return ViewState.GetBool ("ShowTitle", true); }
			set { ViewState ["ShowTitle"] = value; }
		}

		[DefaultValue(TitleFormat.MonthYear)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public TitleFormat TitleFormat {
			get { return (TitleFormat) ViewState.GetInt ("TitleFormat", (int) TitleFormat.MonthYear); }

			set {
				if (value != TitleFormat.Month && value != TitleFormat.MonthYear) {
					throw new ArgumentOutOfRangeException ("The specified title format is not one of the TitleFormat values.");
				}

				ViewState ["TitleFormat"] = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle TitleStyle {
			get {
				if (titleStyle == null) {
					titleStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						titleStyle.TrackViewState ();
				}

				return titleStyle;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle TodayDayStyle {
			get {
				if (todayDayStyle == null) {
					todayDayStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						todayDayStyle.TrackViewState ();
				}

				return todayDayStyle;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public DateTime TodaysDate {
			get {
				object obj = ViewState ["TodaysDate"];

				if (obj != null)
					return (DateTime) obj;

				return today;
			}

			set { ViewState ["TodaysDate"] = value.Date; }
		}

		[DefaultValue (true)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Accessibility")]
		public virtual bool UseAccessibleHeader  {
			get { return ViewState.GetBool ("UseAccessibleHeader", true); }
			set { ViewState ["UseAccessibleHeader"] = value; }
		}

		[Bindable(true)]
		[DefaultValue("1/1/0001 12:00:00 AM")]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public DateTime VisibleDate {
			get {
				object obj = ViewState ["VisibleDate"];

				if (obj != null)
					return (DateTime) obj;

				return DateTime.MinValue;
			}

			set { ViewState ["VisibleDate"] = value.Date; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public TableItemStyle WeekendDayStyle {
			get {
				if (weekendDayStyle == null) {
					weekendDayStyle = new TableItemStyle ();
					if (IsTrackingViewState)
						weekendDayStyle.TrackViewState ();
				}

				return weekendDayStyle;
			}
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		// Private properties
		DateTimeFormatInfo DateInfo {
			get {
				if (dateInfo == null)
					dateInfo = Thread.CurrentThread.CurrentCulture.DateTimeFormat;

				return dateInfo;
			}
		}
		
		DateTime DisplayDate {
			get {
				DateTime dateTime = VisibleDate;
				if (dateTime == DateTime.MinValue) // If visibledate is still the default value
					dateTime = TodaysDate;

				return dateTime;
			}
		}

		DayOfWeek DisplayFirstDayOfWeek {
			get {
				if (FirstDayOfWeek != FirstDayOfWeek.Default)
					return (DayOfWeek)  FirstDayOfWeek;

				return (DayOfWeek) DateInfo.FirstDayOfWeek;
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}

		protected bool HasWeekSelectors (CalendarSelectionMode selectionMode)
		{
			if (selectionMode == CalendarSelectionMode.DayWeek || selectionMode == CalendarSelectionMode.DayWeekMonth)
				return true;

			return false;
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected virtual void RaisePostBackEvent (string arg)
		{
			ValidateEvent (UniqueID, arg);
			if (arg.Length < 1)
				return;

			if (arg[0] == 'V') { // Goes to Next or Previous month
				DateTime prev = VisibleDate;
				int days = Int32.Parse (arg.Substring (1));
				DateTime dt = GetGlobalCalendar().AddDays (dateZenith, days);
				VisibleDate = dt;
				OnVisibleMonthChanged (VisibleDate, prev);
				return;
			}

			if (arg[0] == 'R') { // Selects a range of dates
				string num, date, days;
				num = arg.Substring (1);
				days = num.Substring (num.Length - 2, 2);
				date = num.Substring (0, num.Length - 2);
				DateTime d = GetGlobalCalendar().AddDays (dateZenith, Int32.Parse (date));
				SelectedDates.SelectRange (d, d.AddDays (Int32.Parse (days)));
				OnSelectionChanged ();
				return;
			}

			// Selects a single day
			int daysFromZenith = Int32.Parse (arg);
			DateTime day = GetGlobalCalendar().AddDays (dateZenith, daysFromZenith);
			SelectedDates.SelectRange (day, day);
			OnSelectionChanged ();
		}

		protected override void LoadViewState (object savedState)
		{
			object [] states = (object []) savedState;

			if (states [0] != null)
				 base.LoadViewState (states [0]);

			if (states [1] != null)
				DayHeaderStyle.LoadViewState (states [1]);

			if (states [2] != null)
				DayStyle.LoadViewState (states [2]);

			if (states [3] != null)
			 	NextPrevStyle.LoadViewState (states [3]);

			if (states [4] != null)
				OtherMonthDayStyle.LoadViewState (states [4]);

			if (states [5] != null)
				SelectedDayStyle.LoadViewState (states [5]);

			if (states [6] != null)
			 	TitleStyle.LoadViewState (states [6]);

			if (states [7] != null)
				TodayDayStyle.LoadViewState (states [7]);

			if (states [8] != null)
				SelectorStyle.LoadViewState (states [8]);

			if (states [9] != null)
				WeekendDayStyle.LoadViewState (states [9]);

			ArrayList array = (ArrayList) ViewState ["SelectedDates"];
			if (array != null) {
				dateList = array;
				selectedDatesCollection = new SelectedDatesCollection (dateList);
			}
		}

		protected virtual void OnDayRender (TableCell cell, CalendarDay day)
		{
			DayRenderEventHandler eh = (DayRenderEventHandler) (Events [DayRenderEvent]);
			if (eh != null) {
				Page page = Page;
				if (page != null)
					eh (this, new DayRenderEventArgs (cell, day, page.ClientScript.GetPostBackClientHyperlink (this, GetDaysFromZenith (day.Date).ToString (), true)));
				else
					eh (this, new DayRenderEventArgs (cell, day));
			}
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnSelectionChanged ()
		{
			EventHandler eh = (EventHandler) (Events [SelectionChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		protected virtual void OnVisibleMonthChanged (DateTime newDate,  DateTime previousDate)
		{
			MonthChangedEventHandler eh = (MonthChangedEventHandler) (Events [VisibleMonthChangedEvent]);
			if (eh != null)
				eh (this, new MonthChangedEventArgs (newDate, previousDate));
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			TableStyle ts = new TableStyle ();
			ts.CellSpacing = CellSpacing;
			ts.CellPadding = CellPadding;
			ts.BorderWidth = 1;
			if (ControlStyleCreated)
				ts.CopyFrom (ControlStyle);
			if (ShowGridLines)
				ts.GridLines = GridLines.Both;
			ts.AddAttributesToRender (writer);
			writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);

			if (!String.IsNullOrEmpty (Caption))
				WriteCaption (writer);

			bool enabled = IsEnabled;
			
			if (ShowTitle)
				WriteTitle (writer, enabled);
			
			if (ShowDayHeader)
				WriteDayHeader (writer, enabled);

			WriteDays (writer, enabled);

			writer.RenderEndTag ();
		}

		protected override object SaveViewState ()
		{
			object [] states = new object [10];

			if (dayHeaderStyle != null)
				states [1] = dayHeaderStyle.SaveViewState ();

			if (dayStyle != null)
				states [2] = dayStyle.SaveViewState ();

			if (nextPrevStyle != null)
				states [3] = nextPrevStyle.SaveViewState ();

			if (otherMonthDayStyle != null)
				states [4] = otherMonthDayStyle.SaveViewState ();

			if (selectedDayStyle != null)
				states [5] = selectedDayStyle.SaveViewState ();

			if (titleStyle != null)
				states [6] = titleStyle.SaveViewState ();

			if (todayDayStyle != null)
				states [7] =todayDayStyle.SaveViewState ();

			if (selectorStyle != null)
				states [8] = selectorStyle.SaveViewState ();

			if (weekendDayStyle != null)
				states [9] = weekendDayStyle.SaveViewState ();

			if (SelectedDates.Count > 0) {
				ViewState ["SelectedDates"] = dateList;
			}

			states [0] = base.SaveViewState ();

			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}

			return null;
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();

			if (dayHeaderStyle != null)
				dayHeaderStyle.TrackViewState ();

			if (dayStyle != null)
				dayStyle.TrackViewState ();

			if (nextPrevStyle != null)
				nextPrevStyle.TrackViewState ();

			if (otherMonthDayStyle != null)
				otherMonthDayStyle.TrackViewState ();

			if (selectedDayStyle != null)
				selectedDayStyle.TrackViewState ();

			if (titleStyle != null)
				titleStyle.TrackViewState ();

			if (todayDayStyle  != null)
				todayDayStyle.TrackViewState ();

			if (selectorStyle != null)
				selectorStyle.TrackViewState ();

			if (weekendDayStyle != null)
				weekendDayStyle.TrackViewState ();
		}

		//
		// Private methods
		//
		void WriteDayHeader (HtmlTextWriter writer, bool enabled)
		{
			int i, first;
			string dayName;
			i = first = (int) (DisplayFirstDayOfWeek);
			TableCell cell;


			writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			if (SelectionMode == CalendarSelectionMode.DayWeek) {
				cell = new TableCell();
				cell.HorizontalAlign = HorizontalAlign.Center;
				cell.ApplyStyle (DayHeaderStyle);

				// Empty Cell
				cell.RenderBeginTag (writer);
				cell.RenderEndTag (writer);
			} else {
				if (SelectionMode == CalendarSelectionMode.DayWeekMonth) {
					TableCell selector = new TableCell ();
					selector.ApplyStyle (SelectorStyle);
					selector.HorizontalAlign = HorizontalAlign.Center;

					DateTime date = new DateTime (DisplayDate.Year, DisplayDate.Month, 1); // first date
					int days =  DateTime.DaysInMonth (DisplayDate.Year, DisplayDate.Month);

					selector.RenderBeginTag (writer);
					writer.Write (BuildLink ("R" + GetDaysFromZenith (date) + days, SelectMonthText, DayHeaderStyle.ForeColor, enabled));
					selector.RenderEndTag (writer);
				}
			}

			DateTimeFormatInfo dti = DateInfo;
			while (true) {
				DayOfWeek dayOfWeek = (DayOfWeek) i;
				dayName = dti.GetDayName (dayOfWeek);

				if (UseAccessibleHeader) {
					writer.AddAttribute (HtmlTextWriterAttribute.Abbr, dayName);
					writer.AddAttribute (HtmlTextWriterAttribute.Scope, "col", false);
					cell = new TableHeaderCell();
				} else
					cell = new TableCell();

				cell.HorizontalAlign = HorizontalAlign.Center;
				cell.ApplyStyle (DayHeaderStyle);

				cell.RenderBeginTag (writer);

				switch (DayNameFormat) {
					case DayNameFormat.FirstLetter:
						dayName = dayName.Substring (0, 1);
						break;
					case DayNameFormat.FirstTwoLetters:
						dayName = dayName.Substring (0, 2);
						break;
					case DayNameFormat.Shortest:
						dayName = dti.GetShortestDayName (dayOfWeek);
						break;
					case DayNameFormat.Full:
						break;
					case DayNameFormat.Short:
					default:
						dayName = dti.GetAbbreviatedDayName (dayOfWeek);
						break;
				}

				writer.Write (dayName);
				cell.RenderEndTag (writer);

				if (i >= daysInAWeek - 1)
					i = 0;
				else
					i++;
				
				if (i == first)
					break;
			}

			writer.RenderEndTag ();
		}

		void WriteDay (DateTime date, HtmlTextWriter writer, bool enabled)
		{			
			TableItemStyle style = new TableItemStyle ();
			TableCell cell = new TableCell ();

			CalendarDay day = new CalendarDay (date,
				IsWeekEnd (date.DayOfWeek),
				date == TodaysDate, SelectedDates.Contains (date),
				GetGlobalCalendar ().GetMonth (DisplayDate) != GetGlobalCalendar ().GetMonth (date),
				date.Day.ToString ());

			day.IsSelectable = SelectionMode != CalendarSelectionMode.None;
			cell.HorizontalAlign = HorizontalAlign.Center;
			cell.Width = Unit.Percentage (GetCellWidth ());

			LiteralControl lit = new LiteralControl (day.DayNumberText);
			cell.Controls.Add (lit);

			OnDayRender (cell, day);
					
			if (dayStyle != null && !dayStyle.IsEmpty)
				style.CopyFrom (dayStyle);

			if (day.IsWeekend && weekendDayStyle != null && !weekendDayStyle.IsEmpty)
				style.CopyFrom (weekendDayStyle);

			if (day.IsToday && todayDayStyle != null && !todayDayStyle.IsEmpty)
				style.CopyFrom (todayDayStyle);

			if (day.IsOtherMonth && otherMonthDayStyle != null && !otherMonthDayStyle.IsEmpty)
				style.CopyFrom (otherMonthDayStyle);

			if (enabled && day.IsSelected) {
				style.BackColor = Color.Silver;
				style.ForeColor = Color.White;
				if (selectedDayStyle != null && !selectedDayStyle.IsEmpty)
					style.CopyFrom (selectedDayStyle);
			}

			cell.ApplyStyle (style);

			lit.Text = BuildLink (GetDaysFromZenith (date).ToString (), day.DayNumberText,
					      cell.ForeColor, enabled && day.IsSelectable);

			cell.RenderControl (writer);
		}

		void WriteDays (HtmlTextWriter writer, bool enabled)
		{
			DateTime date = new DateTime (DisplayDate.Year, DisplayDate.Month, 1); // first date
			DateTime lastDate;
			TableCell selectorCell = null;
			int n;

			// Goes backwards until we find the date of that is begining of the week
			for (n = 0; n < daysInAWeek; n++) {
				if (date.DayOfWeek == DisplayFirstDayOfWeek)
					break;

				date = GetGlobalCalendar().AddDays (date, -1);
			}
			/* if the start date is the first day of the week, we need to shift backward one more week */
			if (n == 0)
				date = GetGlobalCalendar().AddDays (date, -1 * daysInAWeek);

			lastDate = GetGlobalCalendar().AddDays (date, 6 * daysInAWeek); // Always six weeks per months

			while (true) {
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);

				if (HasWeekSelectors (SelectionMode)) {	// Week selector
					if (selectorCell == null) {
						selectorCell = new TableCell ();
						selectorCell.ApplyStyle (SelectorStyle);
						selectorCell.HorizontalAlign = HorizontalAlign.Center;
						selectorCell.Width = Unit.Percentage (GetCellWidth ());
					}

					selectorCell.RenderBeginTag (writer);
					writer.Write (BuildLink ("R" + GetDaysFromZenith (date) + "07", SelectWeekText, selectorCell.ForeColor, enabled));
					selectorCell.RenderEndTag (writer);
				}

				for (int i = 0; i < daysInAWeek; i++) {
					WriteDay (date, writer, enabled);
					date = GetGlobalCalendar().AddDays (date, 1);
				}

				writer.RenderEndTag ();
				if (date >= lastDate)
					break;
			}
		}

		string BuildLink (string arg, string text, Color foreColor, bool hasLink)
		{
			StringBuilder str = new StringBuilder ();
			Color clr;
			Page page = Page;
			hasLink = (page != null && hasLink == true) ? true : false;

			if (hasLink) {
				str.Append ("<a href=\"");
				str.Append (page.ClientScript.GetPostBackClientHyperlink (this, arg, true));
				str.Append ('\"');
			

				if (!foreColor.IsEmpty)
					clr = foreColor;
				else {
					if (ForeColor.IsEmpty)
						clr = Color.Black;
					else
						clr = ForeColor;
				}

				str.Append (" style=\"color:" + ColorTranslator.ToHtml (clr));
				str.Append ("\">");
				str.Append (text);
				str.Append ("</a>");
			} else 
				str.Append (text);

			return str.ToString ();
		}

		int GetDaysFromZenith (DateTime date)
		{
			TimeSpan span =  date.Subtract (dateZenith);
			return span.Days;
		}

		void WriteCaption (HtmlTextWriter writer)
		{
			if (CaptionAlign != TableCaptionAlign.NotSet)
				writer.AddAttribute (HtmlTextWriterAttribute.Align, CaptionAlign.ToString (Helpers.InvariantCulture));

			writer.RenderBeginTag (HtmlTextWriterTag.Caption);
			writer.Write (Caption);
			writer.RenderEndTag ();
		}

		void WriteTitle (HtmlTextWriter writer, bool enabled)
		{
			TableCell cellNextPrev = null;
			TableCell titleCell = new TableCell ();
			Table tableTitle = new Table ();

			writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			titleCell.ColumnSpan = HasWeekSelectors (SelectionMode) ? 8 : 7;

			if (titleStyle != null && !titleStyle.IsEmpty && !titleStyle.BackColor.IsEmpty)
				titleCell.BackColor = titleStyle.BackColor;
			else
				titleCell.BackColor = Color.Silver;

			titleCell.RenderBeginTag (writer);

			// Table
			tableTitle.Width =  Unit.Percentage (100);
			if (titleStyle != null && !titleStyle.IsEmpty)
				tableTitle.ApplyStyle (titleStyle);

			tableTitle.RenderBeginTag (writer);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			if (ShowNextPrevMonth) { // Previous Table Data
				cellNextPrev = new TableCell ();
				cellNextPrev.ApplyStyle (nextPrevStyle);
				cellNextPrev.Width = Unit.Percentage (15);

				DateTime date = GetGlobalCalendar().AddMonths (DisplayDate, - 1);
				date = GetGlobalCalendar ().AddDays (date, -date.Day + 1);
				cellNextPrev.RenderBeginTag (writer);
				writer.Write (BuildLink ("V" + GetDaysFromZenith (date), GetNextPrevFormatText (date, false), cellNextPrev.ForeColor, enabled));
				cellNextPrev.RenderEndTag (writer);
			}

			// Current Month Table Data
			{
				DateTimeFormatInfo dti = DateInfo;
				string str;
				TableCell cellMonth = new TableCell ();
				cellMonth.Width = Unit.Percentage (70);
				cellMonth.HorizontalAlign = HorizontalAlign.Center;

				cellMonth.RenderBeginTag (writer);

				if (TitleFormat == TitleFormat.MonthYear)
					str = DisplayDate.ToString (dti.YearMonthPattern, dti);
				else
					str = dti.GetMonthName (GetGlobalCalendar ().GetMonth (DisplayDate));

				writer.Write (str);
				cellMonth.RenderEndTag (writer);
			}

			if (ShowNextPrevMonth) { // Next Table Data
				DateTime date = GetGlobalCalendar().AddMonths (DisplayDate, + 1);
				date = GetGlobalCalendar ().AddDays (date, -date.Day + 1);

				cellNextPrev.HorizontalAlign = HorizontalAlign.Right;
				cellNextPrev.RenderBeginTag (writer);
				writer.Write (BuildLink ("V" + GetDaysFromZenith (date), GetNextPrevFormatText (date, true), cellNextPrev.ForeColor, enabled));
				cellNextPrev.RenderEndTag (writer);
			}

			writer.RenderEndTag ();
			tableTitle.RenderEndTag (writer);
			titleCell.RenderEndTag (writer);
			writer.RenderEndTag (); //tr
		}

		string GetNextPrevFormatText (DateTime date, bool next)
		{
			string text;
			DateTimeFormatInfo dti = DateInfo;
			switch (NextPrevFormat) {
				case NextPrevFormat.FullMonth:
					text = dti.GetMonthName (GetGlobalCalendar ().GetMonth (date));
					break;
				case NextPrevFormat.ShortMonth:
					text = dti.GetAbbreviatedMonthName (GetGlobalCalendar ().GetMonth (date));
					break;
				case NextPrevFormat.CustomText:
				default:
					text = ((next) ? NextMonthText : PrevMonthText);
					break;
			}

			return text;
		}

		bool IsWeekEnd (DayOfWeek day)
		{
			return (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday);
		}

		double GetCellWidth ()
		{
			return HasWeekSelectors (SelectionMode) ? 100/8 : 100/7;
		}

		System.Globalization.Calendar GetGlobalCalendar ()
		{
			return DateTimeFormatInfo.CurrentInfo.Calendar;
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DayRenderEventHandler DayRender {
			add { Events.AddHandler (DayRenderEvent, value); }
			remove { Events.RemoveHandler (DayRenderEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler SelectionChanged {
			add { Events.AddHandler (SelectionChangedEvent, value); }
			remove { Events.RemoveHandler (SelectionChangedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event MonthChangedEventHandler VisibleMonthChanged {
			add { Events.AddHandler (VisibleMonthChangedEvent, value); }
			remove { Events.RemoveHandler (VisibleMonthChangedEvent, value); }
		}
	}
}
