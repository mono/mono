//------------------------------------------------------------------------------
// <copyright file="Calendar.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Threading;
    using System.Globalization;
    using System.ComponentModel;
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Text;

    using System.IO;
    using System.Reflection;



    /// <devdoc>
    ///    <para>Displays a one-month calendar and allows the user to
    ///       view and select a specific day, week, or month.</para>
    /// </devdoc>
    [
    ControlValueProperty("SelectedDate", typeof(DateTime), "1/1/0001"),
    DataBindingHandler("System.Web.UI.Design.WebControls.CalendarDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultEvent("SelectionChanged"),
    DefaultProperty("SelectedDate"),
    Designer("System.Web.UI.Design.WebControls.CalendarDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation
    ]
    public class Calendar : WebControl, IPostBackEventHandler {

        private static readonly object EventDayRender = new object();
        private static readonly object EventSelectionChanged = new object();
        private static readonly object EventVisibleMonthChanged = new object();

        private TableItemStyle titleStyle;
        private TableItemStyle nextPrevStyle;
        private TableItemStyle dayHeaderStyle;
        private TableItemStyle selectorStyle;
        private TableItemStyle dayStyle;
        private TableItemStyle otherMonthDayStyle;
        private TableItemStyle todayDayStyle;
        private TableItemStyle selectedDayStyle;
        private TableItemStyle weekendDayStyle;
        private string defaultButtonColorText;

        private static readonly Color DefaultForeColor = Color.Black;
        private Color defaultForeColor;

        private ArrayList dateList;
        private SelectedDatesCollection selectedDates;
        private Globalization.Calendar threadCalendar;
        private DateTime minSupportedDate;
        private DateTime maxSupportedDate;
#if DEBUG
        private bool threadCalendarInitialized;
#endif

        private const string SELECT_RANGE_COMMAND = "R";
        private const string NAVIGATE_MONTH_COMMAND = "V";

        private static DateTime baseDate = new DateTime(2000, 1, 1);

        private const int STYLEMASK_DAY = 16;
        private const int STYLEMASK_UNIQUE = 15;
        private const int STYLEMASK_SELECTED = 8;
        private const int STYLEMASK_TODAY = 4;
        private const int STYLEMASK_OTHERMONTH = 2;
        private const int STYLEMASK_WEEKEND = 1;
        private const string ROWBEGINTAG = "<tr>";
        private const string ROWENDTAG = "</tr>";

        // Cache commonly used strings. This improves performance and memory usage.
        private const int cachedNumberMax = 31;
        private static readonly string[] cachedNumbers = new string [] {
                  "0",  "1",   "2",   "3",   "4",   "5",   "6",
                  "7",  "8",   "9",  "10",  "11",  "12",  "13",
                 "14", "15",  "16",  "17",  "18",  "19",  "20",
                 "21", "22",  "23",  "24",  "25",  "26",  "27",
                 "28", "29",  "30",  "31",
        };


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.Calendar'/> class.</para>
        /// </devdoc>
        public Calendar() : base(HtmlTextWriterTag.Table) {
        }


        [
        Localizable(true),
        DefaultValue(""),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Calendar_Caption)
        ]
        public virtual string Caption {
            get {
                string s = (string)ViewState["Caption"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Caption"] = value;
            }
        }


        [
        DefaultValue(TableCaptionAlign.NotSet),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_CaptionAlign)
        ]
        public virtual TableCaptionAlign CaptionAlign {
            get {
                object o = ViewState["CaptionAlign"];
                return (o != null) ? (TableCaptionAlign)o : TableCaptionAlign.NotSet;
            }
            set {
                if ((value < TableCaptionAlign.NotSet) ||
                    (value > TableCaptionAlign.Right)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CaptionAlign"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the amount of space between cells.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(2),
        WebSysDescription(SR.Calendar_CellPadding)
        ]
        public int CellPadding {
            get {
                object o = ViewState["CellPadding"];
                return((o == null) ? 2 : (int)o);
            }
            set {
                if (value < - 1 ) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CellPadding"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the amount of space between the contents of a cell
        ///       and the cell's border.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.Calendar_CellSpacing)
        ]
        public int CellSpacing {
            get {
                object o = ViewState["CellSpacing"];
                return((o == null) ?  0 : (int)o);
            }
            set {
                if (value < -1 ) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CellSpacing"] = (int)value;
            }
        }


        /// <devdoc>
        ///    <para> Gets the style property of the day-of-the-week header. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        WebSysDescription(SR.Calendar_DayHeaderStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle DayHeaderStyle {
            get {
                if (dayHeaderStyle == null) {
                    dayHeaderStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)dayHeaderStyle).TrackViewState();
                }
                return dayHeaderStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the format for the names of days.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(DayNameFormat.Short),
        WebSysDescription(SR.Calendar_DayNameFormat)
        ]
        public DayNameFormat DayNameFormat {
            get {
                object dnf = ViewState["DayNameFormat"];
                return((dnf == null) ? DayNameFormat.Short : (DayNameFormat)dnf);
            }
            set {
                if (value < DayNameFormat.Full || value > DayNameFormat.Shortest) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["DayNameFormat"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Gets the style properties for the days. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.Calendar_DayStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle DayStyle {
            get {
                if (dayStyle == null) {
                    dayStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)dayStyle).TrackViewState();
                }
                return dayStyle;
            }
        }


        /// <devdoc>
        ///    <para> Gets
        ///       or sets the day of the week to display in the calendar's first
        ///       column.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(FirstDayOfWeek.Default),
        WebSysDescription(SR.Calendar_FirstDayOfWeek)
        ]
        public FirstDayOfWeek FirstDayOfWeek {
            get {
                object o = ViewState["FirstDayOfWeek"];
                return((o == null) ? FirstDayOfWeek.Default : (FirstDayOfWeek)o);
            }
            set {
                if (value < FirstDayOfWeek.Sunday || value > FirstDayOfWeek.Default) {
                    throw new ArgumentOutOfRangeException("value");
                }

                ViewState["FirstDayOfWeek"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text shown for the next month
        ///       navigation hyperlink if the <see cref='System.Web.UI.WebControls.Calendar.ShowNextPrevMonth'/> property is set to
        ///    <see langword='true'/>.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&gt;"),
        WebSysDescription(SR.Calendar_NextMonthText)
        ]
        public string NextMonthText {
            get {
                object s = ViewState["NextMonthText"];
                return((s == null) ? "&gt;" : (String) s);
            }
            set {
                ViewState["NextMonthText"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the format of the next and previous month hyperlinks in the
        ///       title.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(NextPrevFormat.CustomText),
        WebSysDescription(SR.Calendar_NextPrevFormat)
        ]
        public NextPrevFormat NextPrevFormat {
            get {
                object npf = ViewState["NextPrevFormat"];
                return((npf == null) ? NextPrevFormat.CustomText : (NextPrevFormat)npf);
            }
            set {
                if (value < NextPrevFormat.CustomText || value > NextPrevFormat.FullMonth) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["NextPrevFormat"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Gets the style properties for the next/previous month navigators. This property is
        ///       read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        WebSysDescription(SR.Calendar_NextPrevStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle NextPrevStyle {
            get {
                if (nextPrevStyle == null) {
                    nextPrevStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)nextPrevStyle).TrackViewState();
                }
                return nextPrevStyle;
            }
        }



        /// <devdoc>
        ///    <para>Gets the style properties for the days from the months preceding and following the current month.
        ///       This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.Calendar_OtherMonthDayStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle OtherMonthDayStyle {
            get {
                if (otherMonthDayStyle == null) {
                    otherMonthDayStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)otherMonthDayStyle).TrackViewState();

                }
                return otherMonthDayStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text shown for the previous month
        ///       navigation hyperlink if the <see cref='System.Web.UI.WebControls.Calendar.ShowNextPrevMonth'/> property is set to
        ///    <see langword='true'/>
        ///    .</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&lt;"),
        WebSysDescription(SR.Calendar_PrevMonthText)
        ]
        public string PrevMonthText {
            get {
                object s = ViewState["PrevMonthText"];
                return((s == null) ? "&lt;" : (String) s);
            }
            set {
                ViewState["PrevMonthText"] = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the date that is currently selected
        ///       date.</para>
        /// </devdoc>
        [
        Bindable(true, BindingDirection.TwoWay),
        DefaultValue(typeof(DateTime), "1/1/0001"),
        WebSysDescription(SR.Calendar_SelectedDate)
        ]
        public DateTime SelectedDate {
            get {
                if (SelectedDates.Count == 0) {
                    return DateTime.MinValue;
                }
                return SelectedDates[0];
            }
            set {
                if (value == DateTime.MinValue) {
                    SelectedDates.Clear();
                }
                else {
                    SelectedDates.SelectRange(value, value);
                }
            }
        }


        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.DateTime' qualify='true'/> objects representing days selected on the <see cref='System.Web.UI.WebControls.Calendar'/>. This
        ///    property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Calendar_SelectedDates),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public SelectedDatesCollection SelectedDates {
            get {
                if (selectedDates == null) {
                    if (dateList == null) {
                        dateList = new ArrayList();
                    }
                    selectedDates = new SelectedDatesCollection(dateList);
                }
                return selectedDates;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style properties for the selected date. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.Calendar_SelectedDayStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle SelectedDayStyle {
            get {
                if (selectedDayStyle == null) {
                    selectedDayStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)selectedDayStyle).TrackViewState();
                }
                return selectedDayStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the date selection capabilities on the
        ///    <see cref='System.Web.UI.WebControls.Calendar'/>
        ///    to allow the user to select a day, week, or month.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(CalendarSelectionMode.Day),
        WebSysDescription(SR.Calendar_SelectionMode)
        ]
        public CalendarSelectionMode SelectionMode {
            get {
                object csm = ViewState["SelectionMode"];
                return((csm == null) ? CalendarSelectionMode.Day : (CalendarSelectionMode)csm);
            }
            set {
                if (value < CalendarSelectionMode.None || value > CalendarSelectionMode.DayWeekMonth) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["SelectionMode"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text shown for the month selection in
        ///       the selector column if <see cref='System.Web.UI.WebControls.Calendar.SelectionMode'/> is
        ///    <see langword='CalendarSelectionMode.DayWeekMonth'/>.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&gt;&gt;"),
        WebSysDescription(SR.Calendar_SelectMonthText)
        ]
        public string SelectMonthText {
            get {
                object s = ViewState["SelectMonthText"];
                return((s == null) ? "&gt;&gt;" : (String) s);
            }
            set {
                ViewState["SelectMonthText"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Gets the style properties for the week and month selectors. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        WebSysDescription(SR.Calendar_SelectorStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle SelectorStyle {
            get {
                if (selectorStyle == null) {
                    selectorStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)selectorStyle).TrackViewState();
                }
                return selectorStyle;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the text shown for the week selection in
        ///       the selector column if <see cref='System.Web.UI.WebControls.Calendar.SelectionMode'/> is
        ///    <see langword='CalendarSelectionMode.DayWeek '/>or
        ///    <see langword='CalendarSelectionMode.DayWeekMonth'/>.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue("&gt;"),
        WebSysDescription(SR.Calendar_SelectWeekText)
        ]
        public string SelectWeekText {
            get {
                object s = ViewState["SelectWeekText"];
                return((s == null) ? "&gt;" : (String) s);
            }
            set {
                ViewState["SelectWeekText"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       a value indicating whether the days of the week are displayed.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        WebSysDescription(SR.Calendar_ShowDayHeader)
        ]
        public bool ShowDayHeader {
            get {
                object b = ViewState["ShowDayHeader"];
                return((b == null) ? true : (bool)b);
            }
            set {
                ViewState["ShowDayHeader"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or set
        ///       a value indicating whether days on the calendar are displayed with a border.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(false),
        WebSysDescription(SR.Calendar_ShowGridLines)
        ]
        public bool ShowGridLines {
            get {
                object b= ViewState["ShowGridLines"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["ShowGridLines"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value indicating whether the <see cref='System.Web.UI.WebControls.Calendar'/>
        /// displays the next and pervious month
        /// hyperlinks in the title.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        WebSysDescription(SR.Calendar_ShowNextPrevMonth)
        ]
        public bool ShowNextPrevMonth {
            get {
                object b = ViewState["ShowNextPrevMonth"];
                return((b == null) ? true : (bool)b);
            }
            set {
                ViewState["ShowNextPrevMonth"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Gets or
        ///       sets a value indicating whether the title is displayed.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        WebSysDescription(SR.Calendar_ShowTitle)
        ]
        public bool ShowTitle {
            get {
                object b = ViewState["ShowTitle"];
                return((b == null) ? true : (bool)b);
            }
            set {
                ViewState["ShowTitle"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets how the month name is formatted in the title
        ///       bar.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(TitleFormat.MonthYear),
        WebSysDescription(SR.Calendar_TitleFormat)
        ]
        public TitleFormat TitleFormat {
            get {
                object tf = ViewState["TitleFormat"];
                return((tf == null) ? TitleFormat.MonthYear : (TitleFormat)tf);
            }
            set {
                if (value < TitleFormat.Month || value > TitleFormat.MonthYear) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TitleFormat"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties of the <see cref='System.Web.UI.WebControls.Calendar'/> title. This property is
        ///    read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        WebSysDescription(SR.Calendar_TitleStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public TableItemStyle TitleStyle {
            get {
                if (titleStyle == null) {
                    titleStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)titleStyle).TrackViewState();
                }
                return titleStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style properties for today's date on the
        ///    <see cref='System.Web.UI.WebControls.Calendar'/>. This
        ///       property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        WebSysDescription(SR.Calendar_TodayDayStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle TodayDayStyle {
            get {
                if (todayDayStyle == null) {
                    todayDayStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)todayDayStyle).TrackViewState();
                }
                return todayDayStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the value to use as today's date.</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Calendar_TodaysDate),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public DateTime TodaysDate {
            get {
                object o = ViewState["TodaysDate"];
                return((o == null) ? DateTime.Today : (DateTime)o);
            }
            set {
                ViewState["TodaysDate"] = value.Date;
            }
        }


        [
        DefaultValue(true),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Table_UseAccessibleHeader)
        ]
        public virtual bool UseAccessibleHeader {
            get {
                object o = ViewState["UseAccessibleHeader"];
                return (o != null) ? (bool)o : true;
            }
            set {
                ViewState["UseAccessibleHeader"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the date that specifies what month to display. The date can be
        ///       be any date within the month.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(typeof(DateTime), "1/1/0001"),
        WebSysDescription(SR.Calendar_VisibleDate)
        ]
        public DateTime VisibleDate {
            get {
                object o = ViewState["VisibleDate"];
                return((o == null) ? DateTime.MinValue : (DateTime)o);
            }
            set {
                ViewState["VisibleDate"] = value.Date;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style properties for the displaying weekend dates. This property is
        ///       read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        WebSysDescription(SR.Calendar_WeekendDayStyle),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public TableItemStyle WeekendDayStyle {
            get {
                if (weekendDayStyle == null) {
                    weekendDayStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)weekendDayStyle).TrackViewState();
                }
                return weekendDayStyle;
            }
        }



        /// <devdoc>
        /// <para>Occurs when each day is created in teh control hierarchy for the <see cref='System.Web.UI.WebControls.Calendar'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Calendar_OnDayRender)
        ]
        public event DayRenderEventHandler DayRender {
            add {
                Events.AddHandler(EventDayRender, value);
            }
            remove {
                Events.RemoveHandler(EventDayRender, value);
            }
        }




        /// <devdoc>
        ///    <para>Occurs when the user clicks on a day, week, or month
        ///       selector and changes the <see cref='System.Web.UI.WebControls.Calendar.SelectedDate'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Calendar_OnSelectionChanged)
        ]
        public event EventHandler SelectionChanged {
            add {
                Events.AddHandler(EventSelectionChanged, value);
            }
            remove {
                Events.RemoveHandler(EventSelectionChanged, value);
            }
        }



        /// <devdoc>
        ///    <para>Occurs when the
        ///       user clicks on the next or previous month <see cref='System.Web.UI.WebControls.Button'/> controls on the title.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Calendar_OnVisibleMonthChanged)
        ]
        public event MonthChangedEventHandler VisibleMonthChanged {
            add {
                Events.AddHandler(EventVisibleMonthChanged, value);
            }
            remove {
                Events.RemoveHandler(EventVisibleMonthChanged, value);
            }
        }

        // Methods


        /// <devdoc>
        /// </devdoc>
        private void ApplyTitleStyle(TableCell titleCell, Table titleTable, TableItemStyle titleStyle) {
            // apply affects that affect the whole background to the cell
            if (titleStyle.BackColor != Color.Empty) {
                titleCell.BackColor = titleStyle.BackColor;
            }
            if (titleStyle.BorderColor != Color.Empty) {
                titleCell.BorderColor = titleStyle.BorderColor;
            }
            if (titleStyle.BorderWidth != Unit.Empty) {
                titleCell.BorderWidth= titleStyle.BorderWidth;
            }
            if (titleStyle.BorderStyle != BorderStyle.NotSet) {
                titleCell.BorderStyle = titleStyle.BorderStyle;
            }
            if (titleStyle.Height != Unit.Empty) {
                titleCell.Height = titleStyle.Height;
            }
            if (titleStyle.VerticalAlign != VerticalAlign.NotSet) {
                titleCell.VerticalAlign = titleStyle.VerticalAlign;
            }

            // apply affects that affect everything else to the table
            if (titleStyle.CssClass.Length > 0) {
                titleTable.CssClass = titleStyle.CssClass;
            }
            else if (CssClass.Length > 0) {
                titleTable.CssClass = CssClass;
            }

            if (titleStyle.ForeColor != Color.Empty) {
                titleTable.ForeColor = titleStyle.ForeColor;
            }
            else if (ForeColor != Color.Empty) {
                titleTable.ForeColor = ForeColor;
            }
            titleTable.Font.CopyFrom(titleStyle.Font);
            titleTable.Font.MergeWith(this.Font);

        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new InternalControlCollection(this);
        }



        /// <devdoc>
        /// </devdoc>
        private DateTime EffectiveVisibleDate() {
            DateTime visDate = VisibleDate;
            if (visDate.Equals(DateTime.MinValue)) {
                visDate = TodaysDate;
            }

            // VSWhidbey 366243
            if (IsMinSupportedYearMonth(visDate)) {
                return minSupportedDate;
            }
            else {
                return threadCalendar.AddDays(visDate, -(threadCalendar.GetDayOfMonth(visDate) - 1));
            }
        }


        /// <devdoc>
        /// </devdoc>
        private DateTime FirstCalendarDay(DateTime visibleDate) {
            DateTime firstDayOfMonth = visibleDate;

            // VSWhidbey 366243
            if (IsMinSupportedYearMonth(firstDayOfMonth)) {
                return firstDayOfMonth;
            }

            int daysFromLastMonth = ((int)threadCalendar.GetDayOfWeek(firstDayOfMonth)) - NumericFirstDayOfWeek();
            // Always display at least one day from the previous month
            if (daysFromLastMonth <= 0) {
                daysFromLastMonth += 7;
            }
            return threadCalendar.AddDays(firstDayOfMonth, -daysFromLastMonth);
        }


        /// <devdoc>
        /// </devdoc>
        private string GetCalendarButtonText(string eventArgument, string buttonText, string title, bool showLink, Color foreColor) {
            if (showLink) {
                StringBuilder sb = new StringBuilder();
                sb.Append("<a href=\"");
                sb.Append(Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true));

                // ForeColor needs to go on the actual link. This breaks the uplevel/downlevel rules a little bit,
                // but it is worth doing so the day links do not change color when they go in the history on
                // downlevel browsers. Otherwise, people get it confused with the selection mechanism.
                sb.Append("\" style=\"color:");
                sb.Append(foreColor.IsEmpty ? defaultButtonColorText : ColorTranslator.ToHtml(foreColor));

                if (!String.IsNullOrEmpty(title)) {
                    sb.Append("\" title=\"");
                    sb.Append(title);
                }

                sb.Append("\">");
                sb.Append(buttonText);
                sb.Append("</a>");
                return sb.ToString();
            }
            else {
                return buttonText;
            }
        }


        /// <devdoc>
        /// </devdoc>
        private int GetDefinedStyleMask() {

            // Selected is always defined because it has default effects
            int styleMask = STYLEMASK_SELECTED;

            if (dayStyle != null && !dayStyle.IsEmpty)
                styleMask |= STYLEMASK_DAY;
            if (todayDayStyle != null && !todayDayStyle.IsEmpty)
                styleMask |= STYLEMASK_TODAY;
            if (otherMonthDayStyle != null && !otherMonthDayStyle.IsEmpty)
                styleMask |= STYLEMASK_OTHERMONTH;
            if (weekendDayStyle != null && !weekendDayStyle.IsEmpty)
                styleMask |= STYLEMASK_WEEKEND;
            return styleMask;
        }


        /// <devdoc>
        /// </devdoc>
        private string GetMonthName(int m, bool bFull) {
            if (bFull) {
                return DateTimeFormatInfo.CurrentInfo.GetMonthName(m);
            }
            else {
                return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(m);
            }
        }


        /// <devdoc>
        /// <para>Determines if a <see cref='System.Web.UI.WebControls.CalendarSelectionMode'/>
        /// contains week selectors.</para>
        /// </devdoc>
        protected bool HasWeekSelectors(CalendarSelectionMode selectionMode) {
            return(selectionMode == CalendarSelectionMode.DayWeek
                   || selectionMode == CalendarSelectionMode.DayWeekMonth);
        }

        private bool IsTheSameYearMonth(DateTime date1, DateTime date2) {
#if DEBUG
            Debug.Assert(threadCalendarInitialized);
#endif
            return (threadCalendar.GetEra(date1) == threadCalendar.GetEra(date2) &&
                    threadCalendar.GetYear(date1) == threadCalendar.GetYear(date2) &&
                    threadCalendar.GetMonth(date1) == threadCalendar.GetMonth(date2));
        }

        private bool IsMinSupportedYearMonth(DateTime date) {
#if DEBUG
            Debug.Assert(threadCalendarInitialized);
#endif
            return IsTheSameYearMonth(minSupportedDate, date);
        }

        private bool IsMaxSupportedYearMonth(DateTime date) {
#if DEBUG
            Debug.Assert(threadCalendarInitialized);
#endif
            return IsTheSameYearMonth(maxSupportedDate, date);
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Loads a saved state of the <see cref='System.Web.UI.WebControls.Calendar'/>. </para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                if (myState[0] != null)
                    base.LoadViewState(myState[0]);
                if (myState[1] != null)
                    ((IStateManager)TitleStyle).LoadViewState(myState[1]);
                if (myState[2] != null)
                    ((IStateManager)NextPrevStyle).LoadViewState(myState[2]);
                if (myState[3] != null)
                    ((IStateManager)DayStyle).LoadViewState(myState[3]);
                if (myState[4] != null)
                    ((IStateManager)DayHeaderStyle).LoadViewState(myState[4]);
                if (myState[5] != null)
                    ((IStateManager)TodayDayStyle).LoadViewState(myState[5]);
                if (myState[6] != null)
                    ((IStateManager)WeekendDayStyle).LoadViewState(myState[6]);
                if (myState[7] != null)
                    ((IStateManager)OtherMonthDayStyle).LoadViewState(myState[7]);
                if (myState[8] != null)
                    ((IStateManager)SelectedDayStyle).LoadViewState(myState[8]);
                if (myState[9] != null)
                    ((IStateManager)SelectorStyle).LoadViewState(myState[9]);

                ArrayList selDates = (ArrayList)ViewState["SD"];
                if (selDates != null) {
                    dateList = selDates;
                    selectedDates = null;   // reset wrapper collection
                }

            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Marks the starting point to begin tracking and saving changes to the
        ///       control as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (titleStyle != null)
                ((IStateManager)titleStyle).TrackViewState();
            if (nextPrevStyle != null)
                ((IStateManager)nextPrevStyle).TrackViewState();
            if (dayStyle != null)
                ((IStateManager)dayStyle).TrackViewState();
            if (dayHeaderStyle != null)
                ((IStateManager)dayHeaderStyle).TrackViewState();
            if (todayDayStyle != null)
                ((IStateManager)todayDayStyle).TrackViewState();
            if (weekendDayStyle != null)
                ((IStateManager)weekendDayStyle).TrackViewState();
            if (otherMonthDayStyle != null)
                ((IStateManager)otherMonthDayStyle).TrackViewState();
            if (selectedDayStyle != null)
                ((IStateManager)selectedDayStyle).TrackViewState();
            if (selectorStyle != null)
                ((IStateManager)selectorStyle).TrackViewState();
        }


        /// <devdoc>
        /// </devdoc>
        private int NumericFirstDayOfWeek() {
            // Used globalized value by default
            return(FirstDayOfWeek == FirstDayOfWeek.Default)
            ? (int) DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek
            : (int) FirstDayOfWeek;
        }


        /// <devdoc>
        /// <para>Raises the <see langword='DayRender '/>event for a <see cref='System.Web.UI.WebControls.Calendar'/>.</para>
        /// </devdoc>
        protected virtual void OnDayRender(TableCell cell, CalendarDay day) {
            DayRenderEventHandler handler = (DayRenderEventHandler)Events[EventDayRender];
            if (handler != null) {
                int absoluteDay = day.Date.Subtract(baseDate).Days;

                // VSWhidbey 215383: We return null for selectUrl if a control is not in
                // the page control tree.
                string selectUrl = null;
                Page page = Page;
                if (page != null) {
                    string eventArgument = absoluteDay.ToString(CultureInfo.InvariantCulture);
                    selectUrl = Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true);
                }
                handler(this, new DayRenderEventArgs(cell, day, selectUrl));
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='SelectionChanged '/>event for a <see cref='System.Web.UI.WebControls.Calendar'/>.</para>
        /// </devdoc>
        protected virtual void OnSelectionChanged() {
            EventHandler handler = (EventHandler)Events[EventSelectionChanged];
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='VisibleMonthChanged '/>event for a <see cref='System.Web.UI.WebControls.Calendar'/>.</para>
        /// </devdoc>
        protected virtual void OnVisibleMonthChanged(DateTime newDate, DateTime previousDate) {
            MonthChangedEventHandler handler = (MonthChangedEventHandler)Events[EventVisibleMonthChanged];
            if (handler != null) {
                handler(this, new MonthChangedEventArgs(newDate, previousDate));
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises events on post back for the <see cref='System.Web.UI.WebControls.Calendar'/> control.</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {

            ValidateEvent(UniqueID, eventArgument);

            if (AdapterInternal != null) {
                IPostBackEventHandler pbeh = AdapterInternal as IPostBackEventHandler;
                if (pbeh != null) {
                    pbeh.RaisePostBackEvent(eventArgument);
                }
            } else {

                if (String.Compare(eventArgument, 0, NAVIGATE_MONTH_COMMAND, 0, NAVIGATE_MONTH_COMMAND.Length, StringComparison.Ordinal) == 0) {
                    // Month navigation. The command starts with a "V" and the remainder is day difference from the
                    // base date.
                    DateTime oldDate = VisibleDate;
                    if (oldDate.Equals(DateTime.MinValue)) {
                        oldDate = TodaysDate;
                    }
                    int newDateDiff = Int32.Parse(eventArgument.Substring(NAVIGATE_MONTH_COMMAND.Length), CultureInfo.InvariantCulture);
                    VisibleDate = baseDate.AddDays(newDateDiff);
                    if (VisibleDate == DateTime.MinValue) {
                        // MinValue would make the calendar shows today's month instead because it
                        // is the default value of VisibleDate property, so we add a day to keep
                        // showing the first supported month.
                        // We assume the first supported month has more than one day.
                        VisibleDate = DateTimeFormatInfo.CurrentInfo.Calendar.AddDays(VisibleDate, 1);
                    }
                    OnVisibleMonthChanged(VisibleDate, oldDate);
                }
                else if (String.Compare(eventArgument, 0, SELECT_RANGE_COMMAND, 0, SELECT_RANGE_COMMAND.Length, StringComparison.Ordinal) == 0) {
                    // Range selection. The command starts with an "R". The remainder is an integer. When divided by 100
                    // the result is the day difference from the base date of the first day, and the remainder is the
                    // number of days to select.
                    int rangeValue = Int32.Parse(eventArgument.Substring(SELECT_RANGE_COMMAND.Length), CultureInfo.InvariantCulture);
                    int dayDiff = rangeValue / 100;
                    int dayRange = rangeValue % 100;
                    if (dayRange < 1) {
                        dayRange = 100 + dayRange;
                        dayDiff -= 1;
                    }
                    DateTime dt = baseDate.AddDays(dayDiff);
                    SelectRange(dt, dt.AddDays(dayRange - 1));
                }
                else {
                    // Single day selection. This is just a number which is the day difference from the base date.
                    int dayDiff = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);
                    DateTime dt = baseDate.AddDays(dayDiff);
                    SelectRange(dt, dt);
                }
            }
        }


        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null) {
                Page.RegisterPostBackScript();
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Displays the <see cref='System.Web.UI.WebControls.Calendar'/> control on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            threadCalendar = DateTimeFormatInfo.CurrentInfo.Calendar;
            minSupportedDate = threadCalendar.MinSupportedDateTime;
            maxSupportedDate = threadCalendar.MaxSupportedDateTime;
#if DEBUG
            threadCalendarInitialized = true;
#endif
            DateTime visibleDate = EffectiveVisibleDate();
            DateTime firstDay = FirstCalendarDay(visibleDate);
            CalendarSelectionMode selectionMode = SelectionMode;

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // We only want to display the link if we have a page, or if we are on the design surface
            // If we can stops links being active on the Autoformat dialog, then we can remove this these checks.
            Page page = Page;
            bool buttonsActive;
            if (page == null || DesignMode) {
                buttonsActive = false;
            }
            else {
                buttonsActive = IsEnabled;
            }

            defaultForeColor = ForeColor;
            if (defaultForeColor == Color.Empty) {
                defaultForeColor = DefaultForeColor;
            }
            defaultButtonColorText = ColorTranslator.ToHtml(defaultForeColor);

            Table table = new Table();

            if (ID != null) {
                table.ID = ClientID;
            }
            table.CopyBaseAttributes(this);
            if (ControlStyleCreated) {
                table.ApplyStyle(ControlStyle);
            }
            table.Width = Width;
            table.Height = Height;
            table.CellPadding = CellPadding;
            table.CellSpacing = CellSpacing;

            // default look
            if ((ControlStyleCreated == false) ||
                (ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_BORDERWIDTH) == false) ||
                BorderWidth.Equals(Unit.Empty)) {
                table.BorderWidth = Unit.Pixel(1);
            }

            if (ShowGridLines) {
                table.GridLines = GridLines.Both;
            }
            else {
                table.GridLines = GridLines.None;
            }

            bool useAccessibleHeader = UseAccessibleHeader;
            if (useAccessibleHeader) {
                if (table.Attributes["title"] == null) {
                    table.Attributes["title"] = SR.GetString(SR.Calendar_TitleText);
                }
            }

            string caption = Caption;
            if (caption.Length > 0) {
                table.Caption = caption;
                table.CaptionAlign = CaptionAlign;
            }

            table.RenderBeginTag(writer);

            if (ShowTitle) {
                RenderTitle(writer, visibleDate, selectionMode, buttonsActive, useAccessibleHeader);
            }

            if (ShowDayHeader) {
                RenderDayHeader(writer, visibleDate, selectionMode, buttonsActive, useAccessibleHeader);
            }

            RenderDays(writer, firstDay, visibleDate, selectionMode, buttonsActive, useAccessibleHeader);

            table.RenderEndTag(writer);
        }

        private void RenderCalendarCell(HtmlTextWriter writer, TableItemStyle style, string text, string title, bool hasButton, string eventArgument) {
            style.AddAttributesToRender(writer, this);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (hasButton) {

                // render the button
                Color foreColor = style.ForeColor;
                writer.Write("<a href=\"");
                writer.Write(Page.ClientScript.GetPostBackClientHyperlink(this, eventArgument, true));

                // ForeColor needs to go on the actual link. This breaks the uplevel/downlevel rules a little bit,
                // but it is worth doing so the day links do not change color when they go in the history on
                // downlevel browsers. Otherwise, people get it confused with the selection mechanism.
                writer.Write("\" style=\"color:");
                writer.Write(foreColor.IsEmpty ? defaultButtonColorText : ColorTranslator.ToHtml(foreColor));

                if (!String.IsNullOrEmpty(title)) {
                    writer.Write("\" title=\"");
                    writer.Write(title);
                }

                writer.Write("\">");
                writer.Write(text);
                writer.Write("</a>");
            }
            else {
                writer.Write(text);
            }

            writer.RenderEndTag();
        }

        private void RenderCalendarHeaderCell(HtmlTextWriter writer, TableItemStyle style, string text, string abbrText) {
            style.AddAttributesToRender(writer, this);
            writer.AddAttribute("abbr", abbrText);
            writer.AddAttribute("scope", "col");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.Write(text);
            writer.RenderEndTag();
        }


        /// <devdoc>
        /// </devdoc>
        private void RenderDayHeader(HtmlTextWriter writer, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader) {

            writer.Write(ROWBEGINTAG);

            DateTimeFormatInfo dtf = DateTimeFormatInfo.CurrentInfo;

            if (HasWeekSelectors(selectionMode)) {
                TableItemStyle monthSelectorStyle = new TableItemStyle();
                monthSelectorStyle.HorizontalAlign = HorizontalAlign.Center;
                // add the month selector button if required;
                if (selectionMode == CalendarSelectionMode.DayWeekMonth) {

                    // Range selection. The command starts with an "R". The remainder is an integer. When divided by 100
                    // the result is the day difference from the base date of the first day, and the remainder is the
                    // number of days to select.
                    int startOffset = visibleDate.Subtract(baseDate).Days;
                    int monthLength = threadCalendar.GetDaysInMonth(threadCalendar.GetYear(visibleDate), threadCalendar.GetMonth(visibleDate), threadCalendar.GetEra(visibleDate));
                    if (IsMinSupportedYearMonth(visibleDate)) {
                        // The first supported month might not start with day 1
                        // (e.g. Sept 8 is the first supported date of JapaneseCalendar)
                        monthLength = monthLength - threadCalendar.GetDayOfMonth(visibleDate) + 1;
                    }
                    else if (IsMaxSupportedYearMonth(visibleDate)) {
                        // The last supported month might not have all days supported in that calendar month
                        // (e.g. April 3 is the last supported date of HijriCalendar)
                        monthLength = threadCalendar.GetDayOfMonth(maxSupportedDate);
                    }

                    string monthSelectKey = SELECT_RANGE_COMMAND + ((startOffset * 100) + monthLength).ToString(CultureInfo.InvariantCulture);
                    monthSelectorStyle.CopyFrom(SelectorStyle);

                    string selectMonthTitle = null;
                    if (useAccessibleHeader) {
                        selectMonthTitle = SR.GetString(SR.Calendar_SelectMonthTitle);
                    }
                    RenderCalendarCell(writer, monthSelectorStyle, SelectMonthText, selectMonthTitle, buttonsActive, monthSelectKey);
                }
                else {
                    // otherwise make it look like the header row
                    monthSelectorStyle.CopyFrom(DayHeaderStyle);
                    RenderCalendarCell(writer, monthSelectorStyle, string.Empty, null, false, null);
                }
            }

            TableItemStyle dayNameStyle = new TableItemStyle();
            dayNameStyle.HorizontalAlign = HorizontalAlign.Center;
            dayNameStyle.CopyFrom(DayHeaderStyle);
            DayNameFormat dayNameFormat = DayNameFormat;

            int numericFirstDay = NumericFirstDayOfWeek();
            for (int i = numericFirstDay; i < numericFirstDay + 7; i++) {
                string dayName;
                int dayOfWeek = i % 7;
                switch (dayNameFormat) {
                    case DayNameFormat.FirstLetter:
                        dayName = dtf.GetDayName((DayOfWeek)dayOfWeek).Substring(0, 1);
                        break;
                    case DayNameFormat.FirstTwoLetters:
                        dayName = dtf.GetDayName((DayOfWeek)dayOfWeek).Substring(0, 2);
                        break;
                    case DayNameFormat.Full:
                        dayName = dtf.GetDayName((DayOfWeek)dayOfWeek);
                        break;
                    case DayNameFormat.Short:
                        dayName = dtf.GetAbbreviatedDayName((DayOfWeek)dayOfWeek);
                        break;
                    case DayNameFormat.Shortest:
                        dayName = dtf.GetShortestDayName((DayOfWeek)dayOfWeek);
                        break;
                    default:
                        Debug.Assert(false, "Unknown DayNameFormat value!");
                        goto case DayNameFormat.Short;
                }

                if (useAccessibleHeader) {
                    string fullDayName = dtf.GetDayName((DayOfWeek)dayOfWeek);
                    RenderCalendarHeaderCell(writer, dayNameStyle, dayName, fullDayName);
                }
                else {
                    RenderCalendarCell(writer, dayNameStyle, dayName, null, false, null);
                }
            }
            writer.Write(ROWENDTAG);
        }


        /// <devdoc>
        /// </devdoc>
        private void RenderDays(HtmlTextWriter writer, DateTime firstDay, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader) {
            // Now add the rows for the actual days

            DateTime d = firstDay;
            TableItemStyle weekSelectorStyle = null;
            Unit defaultWidth;
            bool hasWeekSelectors = HasWeekSelectors(selectionMode);
            if (hasWeekSelectors) {
                weekSelectorStyle = new TableItemStyle();
                weekSelectorStyle.Width = Unit.Percentage(12);
                weekSelectorStyle.HorizontalAlign = HorizontalAlign.Center;
                weekSelectorStyle.CopyFrom(SelectorStyle);
                defaultWidth = Unit.Percentage(12);
            }
            else {
                defaultWidth = Unit.Percentage(14);
            }

            // This determines whether we need to call DateTime.ToString for each day. The only culture/calendar
            // that requires this for now is the HebrewCalendar.
            bool usesStandardDayDigits = !(threadCalendar is HebrewCalendar);

            // This determines whether we can write out cells directly, or whether we have to create whole
            // TableCell objects for each day.
            bool hasRenderEvent = (this.GetType() != typeof(Calendar)
                                   || Events[EventDayRender] != null);

            TableItemStyle [] cellStyles = new TableItemStyle[16];
            int definedStyleMask = GetDefinedStyleMask();
            DateTime todaysDate = TodaysDate;
            string selectWeekText = SelectWeekText;
            bool daysSelectable = buttonsActive && (selectionMode != CalendarSelectionMode.None);
            int visibleDateMonth = threadCalendar.GetMonth(visibleDate);
            int absoluteDay = firstDay.Subtract(baseDate).Days;

            // VSWhidbey 480155: flag to indicate if forecolor needs to be set
            // explicitly in design mode to mimic runtime rendering with the
            // limitation of not supporting CSS class color setting.
            bool inDesignSelectionMode = (DesignMode && SelectionMode != CalendarSelectionMode.None);

            //------------------------------------------------------------------
            // VSWhidbey 366243: The following variables are for boundary cases
            // such as the current visible month is the first or the last
            // supported month.  They are used in the 'for' loops below.

            // For the first supported month, calculate how many days to
            // skip at the beginning of the first month.  E.g. JapaneseCalendar
            // starts at Sept 8.
            int numOfFirstDaysToSkip = 0;
            if (IsMinSupportedYearMonth(visibleDate)) {
                numOfFirstDaysToSkip = (int)threadCalendar.GetDayOfWeek(firstDay) - NumericFirstDayOfWeek();
                // If negative, it simply means the the index of the starting
                // day name is greater than the day name of the first supported
                // date.  We add back 7 to get the number of days to skip.
                if (numOfFirstDaysToSkip < 0) {
                    numOfFirstDaysToSkip += 7;
                }
            }
            Debug.Assert(numOfFirstDaysToSkip < 7);

            // For the last or second last supported month, initialize variables
            // to identify the last supported date of the current calendar.
            // e.g. The last supported date of HijriCalendar is April 3.  When
            // the second last monthh is shown, it can be the case that not all
            // cells will be filled up.
            bool passedLastSupportedDate = false;
            DateTime secondLastMonth = threadCalendar.AddMonths(maxSupportedDate, -1);
            bool lastOrSecondLastMonth = (IsMaxSupportedYearMonth(visibleDate) ||
                                IsTheSameYearMonth(secondLastMonth, visibleDate));
            //------------------------------------------------------------------

            for (int iRow = 0; iRow < 6; iRow++) {
                if (passedLastSupportedDate) {
                    break;
                }

                writer.Write(ROWBEGINTAG);

                // add week selector column and button if required
                if (hasWeekSelectors) {
                    // Range selection. The command starts with an "R". The remainder is an integer. When divided by 100
                    // the result is the day difference from the base date of the first day, and the remainder is the
                    // number of days to select.
                    int dayDiffParameter = (absoluteDay * 100) + 7;

                    // Adjust the dayDiff for the first or the last supported month
                    if (numOfFirstDaysToSkip > 0) {
                        dayDiffParameter -= numOfFirstDaysToSkip;
                    }
                    else if (lastOrSecondLastMonth) {
                        int daysFromLastDate = maxSupportedDate.Subtract(d).Days;
                        if (daysFromLastDate < 6) {
                            dayDiffParameter -= (6 - daysFromLastDate);
                        }
                    }
                    string weekSelectKey = SELECT_RANGE_COMMAND + dayDiffParameter.ToString(CultureInfo.InvariantCulture);

                    string selectWeekTitle = null;
                    if (useAccessibleHeader) {
                        int weekOfMonth = iRow + 1;
                        selectWeekTitle = SR.GetString(SR.Calendar_SelectWeekTitle, weekOfMonth.ToString(CultureInfo.InvariantCulture));
                    }
                    RenderCalendarCell(writer, weekSelectorStyle, selectWeekText, selectWeekTitle, buttonsActive, weekSelectKey);
                }

                for (int iDay = 0; iDay < 7; iDay++) {

                    // Render empty cells for special cases to handle the first
                    // or last supported month.
                    if (numOfFirstDaysToSkip > 0) {
                        iDay += numOfFirstDaysToSkip;
                        for ( ; numOfFirstDaysToSkip > 0; numOfFirstDaysToSkip--) {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.RenderEndTag();
                        }
                    }
                    else if (passedLastSupportedDate) {
                        for ( ; iDay < 7; iDay++) {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.RenderEndTag();
                        }
                        break;
                    }

                    int dayOfWeek = (int)threadCalendar.GetDayOfWeek(d);
                    int dayOfMonth = threadCalendar.GetDayOfMonth(d);
                    string dayNumberText;
                    if ((dayOfMonth <= cachedNumberMax) && usesStandardDayDigits) {
                        dayNumberText = cachedNumbers[dayOfMonth];
                    }
                    else {
                        dayNumberText = d.ToString("dd", CultureInfo.CurrentCulture);
                    }

                    CalendarDay day = new CalendarDay(d,
                                                      (dayOfWeek == 0 || dayOfWeek == 6), // IsWeekend
                                                      d.Equals(todaysDate), // IsToday
                                                      (selectedDates != null) && selectedDates.Contains(d), // IsSelected
                                                      threadCalendar.GetMonth(d) != visibleDateMonth, // IsOtherMonth
                                                      dayNumberText // Number Text
                                                      );

                    int styleMask = STYLEMASK_DAY;
                    if (day.IsSelected)
                        styleMask |= STYLEMASK_SELECTED;
                    if (day.IsOtherMonth)
                        styleMask |= STYLEMASK_OTHERMONTH;
                    if (day.IsToday)
                        styleMask |= STYLEMASK_TODAY;
                    if (day.IsWeekend)
                        styleMask |= STYLEMASK_WEEKEND;
                    int dayStyleMask = definedStyleMask  & styleMask;
                    // determine the unique portion of the mask for the current calendar,
                    // which will strip out the day style bit
                    int dayStyleID = dayStyleMask & STYLEMASK_UNIQUE;

                    TableItemStyle cellStyle = cellStyles[dayStyleID];
                    if (cellStyle == null) {
                        cellStyle = new TableItemStyle();
                        SetDayStyles(cellStyle, dayStyleMask, defaultWidth);
                        cellStyles[dayStyleID] = cellStyle;
                    }


                    string dayTitle = null;
                    if (useAccessibleHeader) {
                        dayTitle = d.ToString("m", CultureInfo.CurrentCulture);
                    }

                    if (hasRenderEvent) {

                        TableCell cdc = new TableCell();
                        cdc.ApplyStyle(cellStyle);

                        LiteralControl dayContent = new LiteralControl(dayNumberText);
                        cdc.Controls.Add(dayContent);

                        day.IsSelectable = daysSelectable;

                        OnDayRender(cdc, day);

                        // refresh the day content
                        dayContent.Text = GetCalendarButtonText(absoluteDay.ToString(CultureInfo.InvariantCulture),
                                                                dayNumberText,
                                                                dayTitle,
                                                                buttonsActive && day.IsSelectable,
                                                                cdc.ForeColor);
                        cdc.RenderControl(writer);

                    }
                    else {
                        // VSWhidbey 480155: In design mode we render days as
                        // texts instead of links so CSS class color setting is
                        // supported.  But this differs in runtime rendering
                        // where CSS class color setting is not supported.  To
                        // correctly mimic the forecolor of runtime rendering in
                        // design time, the default color, which is used in
                        // runtime rendering, is explicitly set in this case.
                        if (inDesignSelectionMode && cellStyle.ForeColor.IsEmpty) {
                            cellStyle.ForeColor = defaultForeColor;
                        }

                        RenderCalendarCell(writer, cellStyle, dayNumberText, dayTitle, daysSelectable, absoluteDay.ToString(CultureInfo.InvariantCulture));
                    }

                    Debug.Assert(!passedLastSupportedDate);
                    if (lastOrSecondLastMonth && d.Month == maxSupportedDate.Month && d.Day == maxSupportedDate.Day) {
                        passedLastSupportedDate = true;
                    }
                    else {
                        d = threadCalendar.AddDays(d, 1);
                        absoluteDay++;
                    }
                }
                writer.Write(ROWENDTAG);
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void RenderTitle(HtmlTextWriter writer, DateTime visibleDate, CalendarSelectionMode selectionMode, bool buttonsActive, bool useAccessibleHeader) {
            writer.Write(ROWBEGINTAG);

            TableCell titleCell = new TableCell();
            Table titleTable = new Table();

            // default title table/cell styles
            titleCell.ColumnSpan = HasWeekSelectors(selectionMode) ? 8 : 7;
            titleCell.BackColor = Color.Silver;
            titleTable.GridLines = GridLines.None;
            titleTable.Width = Unit.Percentage(100);
            titleTable.CellSpacing = 0;

            TableItemStyle titleStyle = TitleStyle;
            ApplyTitleStyle(titleCell, titleTable, titleStyle);

            titleCell.RenderBeginTag(writer);
            titleTable.RenderBeginTag(writer);
            writer.Write(ROWBEGINTAG);

            NextPrevFormat nextPrevFormat = NextPrevFormat;

            TableItemStyle nextPrevStyle = new TableItemStyle();
            nextPrevStyle.Width = Unit.Percentage(15);
            nextPrevStyle.CopyFrom(NextPrevStyle);
            if (ShowNextPrevMonth) {
                if (IsMinSupportedYearMonth(visibleDate)) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                }
                else {
                    string prevMonthText;
                    if (nextPrevFormat == NextPrevFormat.ShortMonth || nextPrevFormat == NextPrevFormat.FullMonth) {
                        int monthNo = threadCalendar.GetMonth(threadCalendar.AddMonths(visibleDate, - 1));
                        prevMonthText = GetMonthName(monthNo, (nextPrevFormat == NextPrevFormat.FullMonth));
                    }
                    else {
                        prevMonthText = PrevMonthText;
                    }
                    // Month navigation. The command starts with a "V" and the remainder is day difference from the
                    // base date.
                    DateTime prevMonthDate;

                    // VSWhidbey 366243: Some calendar's min supported date is
                    // not the first day of the month (e.g. JapaneseCalendar.
                    // So if we are setting the second supported month, the prev
                    // month link should always point to the first supported
                    // date instead of the first day of the previous month.
                    DateTime secondSupportedMonth = threadCalendar.AddMonths(minSupportedDate, 1);
                    if (IsTheSameYearMonth(secondSupportedMonth, visibleDate)) {
                        prevMonthDate = minSupportedDate;
                    }
                    else {
                        prevMonthDate = threadCalendar.AddMonths(visibleDate, -1);
                    }

                    string prevMonthKey = NAVIGATE_MONTH_COMMAND + (prevMonthDate.Subtract(baseDate)).Days.ToString(CultureInfo.InvariantCulture);

                    string previousMonthTitle = null;
                    if (useAccessibleHeader) {
                        previousMonthTitle = SR.GetString(SR.Calendar_PreviousMonthTitle);
                    }
                    RenderCalendarCell(writer, nextPrevStyle, prevMonthText, previousMonthTitle, buttonsActive, prevMonthKey);
                }
            }


            TableItemStyle cellMainStyle = new TableItemStyle();

            if (titleStyle.HorizontalAlign != HorizontalAlign.NotSet) {
                cellMainStyle.HorizontalAlign = titleStyle.HorizontalAlign;
            }
            else {
                cellMainStyle.HorizontalAlign = HorizontalAlign.Center;
            }
            cellMainStyle.Wrap = titleStyle.Wrap;
            cellMainStyle.Width = Unit.Percentage(70);

            string titleText;

            switch (TitleFormat) {
                case TitleFormat.Month:
                    titleText = visibleDate.ToString("MMMM", CultureInfo.CurrentCulture);
                    break;
                case TitleFormat.MonthYear:
                    string titlePattern = DateTimeFormatInfo.CurrentInfo.YearMonthPattern;
                    // Some cultures have a comma in their YearMonthPattern, which does not look
                    // right in a calendar. Use a fixed pattern for those.
                    if (titlePattern.IndexOf(',') >= 0) {
                        titlePattern = "MMMM yyyy";
                    }
                    titleText = visibleDate.ToString(titlePattern, CultureInfo.CurrentCulture);
                    break;
                default:
                    Debug.Assert(false, "Unknown TitleFormat value!");
                    goto case TitleFormat.MonthYear;
            }
            RenderCalendarCell(writer, cellMainStyle, titleText, null, false, null);

            if (ShowNextPrevMonth) {
                if (IsMaxSupportedYearMonth(visibleDate)) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                }
                else {
                    // Style for this one is identical bar
                    nextPrevStyle.HorizontalAlign = HorizontalAlign.Right;
                    string nextMonthText;
                    if (nextPrevFormat == NextPrevFormat.ShortMonth || nextPrevFormat == NextPrevFormat.FullMonth) {
                        int monthNo = threadCalendar.GetMonth(threadCalendar.AddMonths(visibleDate, 1));
                        nextMonthText = GetMonthName(monthNo, (nextPrevFormat == NextPrevFormat.FullMonth));
                    }
                    else {
                        nextMonthText = NextMonthText;
                    }
                    // Month navigation. The command starts with a "V" and the remainder is day difference from the
                    // base date.
                    DateTime nextMonthDate = threadCalendar.AddMonths(visibleDate, 1);
                    string nextMonthKey = NAVIGATE_MONTH_COMMAND + (nextMonthDate.Subtract(baseDate)).Days.ToString(CultureInfo.InvariantCulture);

                    string nextMonthTitle = null;
                    if (useAccessibleHeader) {
                        nextMonthTitle = SR.GetString(SR.Calendar_NextMonthTitle);
                    }
                    RenderCalendarCell(writer, nextPrevStyle, nextMonthText, nextMonthTitle, buttonsActive, nextMonthKey);
                }
            }
            writer.Write(ROWENDTAG);
            titleTable.RenderEndTag(writer);
            titleCell.RenderEndTag(writer);
            writer.Write(ROWENDTAG);

        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Stores the state of the System.Web.UI.WebControls.Calender.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            if (SelectedDates.Count > 0)
                ViewState["SD"] = dateList;

            object[] myState = new object[10];

            myState[0] = base.SaveViewState();
            myState[1] = (titleStyle != null) ? ((IStateManager)titleStyle).SaveViewState() : null;
            myState[2] = (nextPrevStyle != null) ? ((IStateManager)nextPrevStyle).SaveViewState() : null;
            myState[3] = (dayStyle != null) ? ((IStateManager)dayStyle).SaveViewState() : null;
            myState[4] = (dayHeaderStyle != null) ? ((IStateManager)dayHeaderStyle).SaveViewState() : null;
            myState[5] = (todayDayStyle != null) ? ((IStateManager)todayDayStyle).SaveViewState() : null;
            myState[6] = (weekendDayStyle != null) ? ((IStateManager)weekendDayStyle).SaveViewState() : null;
            myState[7] = (otherMonthDayStyle != null) ? ((IStateManager)otherMonthDayStyle).SaveViewState() : null;
            myState[8] = (selectedDayStyle != null) ? ((IStateManager)selectedDayStyle).SaveViewState() : null;
            myState[9] = (selectorStyle != null) ? ((IStateManager)selectorStyle).SaveViewState() : null;

            for (int i = 0; i<myState.Length; i++) {
                if (myState[i] != null)
                    return myState;
            }

            return null;
        }

        private void SelectRange(DateTime dateFrom, DateTime dateTo) {

            Debug.Assert(dateFrom <= dateTo, "Bad Date Range");

            // see if this range differs in any way from the current range
            // these checks will determine this because the colleciton is sorted
            TimeSpan ts = dateTo - dateFrom;
            if (SelectedDates.Count != ts.Days + 1
                || SelectedDates[0] != dateFrom
                || SelectedDates[SelectedDates.Count - 1] != dateTo) {
                SelectedDates.SelectRange(dateFrom, dateTo);
                OnSelectionChanged();
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void SetDayStyles(TableItemStyle style, int styleMask, Unit defaultWidth) {

            // default day styles
            style.Width = defaultWidth;
            style.HorizontalAlign = HorizontalAlign.Center;

            if ((styleMask & STYLEMASK_DAY) != 0) {
                style.CopyFrom(DayStyle);
            }
            if ((styleMask & STYLEMASK_WEEKEND) != 0) {
                style.CopyFrom(WeekendDayStyle);
            }
            if ((styleMask & STYLEMASK_OTHERMONTH) != 0) {
                style.CopyFrom(OtherMonthDayStyle);
            }
            if ((styleMask & STYLEMASK_TODAY) != 0) {
                style.CopyFrom(TodayDayStyle);
            }

            if ((styleMask & STYLEMASK_SELECTED) != 0) {
                // default selected day style
                style.ForeColor = Color.White;
                style.BackColor = Color.Silver;

                style.CopyFrom(SelectedDayStyle);
            }
        }
    }
}


