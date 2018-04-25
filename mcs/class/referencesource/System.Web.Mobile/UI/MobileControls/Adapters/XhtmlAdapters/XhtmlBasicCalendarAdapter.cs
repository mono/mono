//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicCalendarAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

using WebControls=System.Web.UI.WebControls;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlCalendarAdapter : XhtmlControlAdapter {
        private SelectionList _selectList;
        private TextBox _textBox;
        private Command _command;
        private List _optionList;
        private List _monthList;
        private List _weekList;
        private List _dayList;
        private int _chooseOption = FirstPrompt;
        private int _monthsToDisplay;
        private int _eraCount = 0;

        /////////////////////////////////////////////////////////////////////
        // Globalization of Calendar Information:
        // Similar to the globalization support of the ASP.NET Calendar control,
        // this support is done via COM+ thread culture info/object.
        // Specific example can be found from ASP.NET Calendar spec.
        /////////////////////////////////////////////////////////////////////

        // This member variable is set each time when calendar info needs to
        // be accessed and be shared for other helper functions.
        private System.Globalization.Calendar _threadCalendar;

        private String _textBoxErrorMessage;

        // Since SecondaryUIMode is an int type, we use constant integers here
        // instead of enum so the mode can be compared without casting.
        private const int FirstPrompt = NotSecondaryUIInit;
        private const int OptionPrompt = NotSecondaryUIInit + 1;
        private const int TypeDate = NotSecondaryUIInit + 2;
        private const int DateOption = NotSecondaryUIInit + 3;
        private const int WeekOption = NotSecondaryUIInit + 4;
        private const int MonthOption = NotSecondaryUIInit + 5;
        private const int ChooseMonth = NotSecondaryUIInit + 6;
        private const int ChooseWeek = NotSecondaryUIInit + 7;
        private const int ChooseDay = NotSecondaryUIInit + 8;
        private const int DefaultDateDone = NotSecondaryUIInit + 9;
        private const int TypeDateDone = NotSecondaryUIInit + 10;
        private const int Done = NotSecondaryUIInit + 11;

        private const String DaySeparator = " - ";
        private const String Space = " ";

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.Control"]/*' />
        protected new System.Web.UI.MobileControls.Calendar Control {
            get {
                return base.Control as System.Web.UI.MobileControls.Calendar;
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e) {
            ListCommandEventHandler listCommandEventHandler;

            // Create secondary child controls for rendering secondary UI.
            // Note that their ViewState is disabled because they are used
            // for rendering only.
            //---------------------------------------------------------------

            _selectList = new SelectionList();
            _selectList.Visible = false;
            _selectList.EnableViewState = false;
            Control.Controls.Add(_selectList);

            _textBox = new TextBox();
            _textBox.Visible = false;
            _textBox.EnableViewState = false;
            EventHandler eventHandler = new EventHandler(this.TextBoxEventHandler);
            _textBox.TextChanged += eventHandler;
            Control.Controls.Add(_textBox);

            _command = new Command();
            _command.Visible = false;
            _command.EnableViewState = false;
            Control.Controls.Add(_command);

            // Below are initialization of several list controls.  A note is
            // that here the usage of DataMember is solely for remembering
            // how many items a particular list control is bounded to.  The
            // property is not used as originally designed.
            //---------------------------------------------------------------

            _optionList = new List();
            _optionList.DataMember = "5";
            listCommandEventHandler = new ListCommandEventHandler(this.OptionListEventHandler);
            InitList(_optionList, listCommandEventHandler);

            // Use MobileCapabilities to check screen size and determine how
            // many months should be displayed for different devices.
            _monthsToDisplay = MonthsToDisplay(Device.ScreenCharactersHeight);

            // Create the list of months, including [Next] and [Prev] links
            _monthList = new List();
            _monthList.DataMember = Convert.ToString(_monthsToDisplay + 2, CultureInfo.InvariantCulture);
            listCommandEventHandler = new ListCommandEventHandler(this.MonthListEventHandler);
            InitList(_monthList, listCommandEventHandler);

            _weekList = new List();
            _weekList.DataMember = "6";
            listCommandEventHandler = new ListCommandEventHandler(this.WeekListEventHandler);
            InitList(_weekList, listCommandEventHandler);

            _dayList = new List();
            _dayList.DataMember = "7";
            listCommandEventHandler = new ListCommandEventHandler(this.DayListEventHandler);
            InitList(_dayList, listCommandEventHandler);

            // Initialize the VisibleDate which will be used to keep track
            // the ongoing selection of year, month and day from multiple
            // secondary UI screens.  If the page is loaded for the first
            // time, it doesn't need to be initialized (since it is not used
            // yet) so no unnecessary viewstate value will be generated.
            if (Page.IsPostBack && Control.VisibleDate == DateTime.MinValue) {
                Control.VisibleDate = DateTime.Today;
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.OnLoad"]/*' />
        public override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            // Here we check to see which list control should be initialized
            // with items so postback event can be handled properly.
            if (Page.IsPostBack) {
                String controlId = Page.Request[Constants.EventSourceID];
                if (controlId != null && controlId.Length != 0) {
                    List list = Page.FindControl(controlId) as List;
                    if (list != null &&
                        Control.Controls.Contains(list)) {

                        DataBindListWithEmptyValues(
                            list, Convert.ToInt32(list.DataMember, CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.LoadAdapterState"]/*' />
        public override void LoadAdapterState(Object state)
        {
            if (state != null) {
                if (state is Pair) {
                    Pair pair = (Pair)state;
                    base.LoadAdapterState(pair.First);
                    _chooseOption = (int)pair.Second;
                }
                else if (state is Triplet) {
                    Triplet triplet = (Triplet)state;
                    base.LoadAdapterState(triplet.First);
                    _chooseOption = (int)triplet.Second;
                    Control.VisibleDate = new DateTime(Int64.Parse((String)triplet.Third, CultureInfo.InvariantCulture));
                }
                else if (state is Object[]) {
                    Object[] viewState = (Object[])state;
                    base.LoadAdapterState(viewState[0]);
                    _chooseOption = (int)viewState[1];
                    Control.VisibleDate = new DateTime(Int64.Parse((String)viewState[2], CultureInfo.InvariantCulture));
                    _eraCount = (int)viewState[3];

                    if (SecondaryUIMode == TypeDate) {
                        // Create a placeholder list for capturing the selected era
                        // in postback data.
                        for (int i = 0; i < _eraCount; i++) {
                            _selectList.Items.Add(String.Empty);
                        }
                    }
                }
                else {
                    _chooseOption = (int)state;
                }
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.SaveAdapterState"]/*' />
        public override Object SaveAdapterState()
        {
            DateTime visibleDate = Control.VisibleDate;

            bool saveVisibleDate = visibleDate != DateTime.MinValue &&
                                        DateTime.Compare(visibleDate, DateTime.Today) != 0 && 
                                        !IsViewStateEnabled();
            Object baseState = base.SaveAdapterState();

            if (baseState == null && !saveVisibleDate && _eraCount == 0) {
                if (_chooseOption != FirstPrompt) {
                    return _chooseOption;
                }
                else {
                    return null;
                }
            }
            else if (!saveVisibleDate && _eraCount == 0) {
                return new Pair(baseState, _chooseOption);
            }
            else if (_eraCount == 0) {
                return new Triplet(baseState, _chooseOption, visibleDate.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            else {
                return new Object[] { baseState,
                                      _chooseOption,
                                      visibleDate.Ticks.ToString(CultureInfo.InvariantCulture),
                                      _eraCount };
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.OnPreRender"]/*' />
        public override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // We specially binding eras of the current calendar object here
            // when the UI of typing date is display.  We do it only if the
            // calendar supports more than one era.
            if (SecondaryUIMode == TypeDate) {
                DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;

                int [] ints = currentInfo.Calendar.Eras;

                if (ints.Length > 1) {
                    // Save the value in private view state
                    _eraCount = ints.Length;

                    int currentEra;
                    if (_selectList.SelectedIndex != -1) {
                        currentEra = ints[_selectList.SelectedIndex];
                    }
                    else {
                        currentEra =
                            currentInfo.Calendar.GetEra(Control.VisibleDate);
                    }

                    // Clear the placeholder item list if created in LoadAdapterState
                    _selectList.Items.Clear();

                    for (int i = 0; i < ints.Length; i++) {
                        int era = ints[i];

                        _selectList.Items.Add(currentInfo.GetEraName(era));

                        // There is no association between the era value and
                        // its index in the era array, so we need to check it
                        // explicitly for the default selected index.
                        if (currentEra == era) {
                            _selectList.SelectedIndex = i;
                        }
                    }
                    _selectList.Visible = true;
                }
                else {
                    // disable viewstate since no need to save any data for
                    // this control
                    _selectList.EnableViewState = false;
                }
            }
            else {
                _selectList.EnableViewState = false;
            }
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            ArrayList arr;
            DateTime tempDate;
            DateTimeFormatInfo currentDateTimeInfo = DateTimeFormatInfo.CurrentInfo;
            String abbreviatedMonthDayPattern = AbbreviateMonthPattern(currentDateTimeInfo.MonthDayPattern);
            _threadCalendar = currentDateTimeInfo.Calendar;

            ConditionalClearPendingBreak(writer);
            ConditionalEnterStyle(writer, Style);
            // Use div (rather than span) with all secondaryUIModes for consistency across secondary UI.
            // If span is used in secondary ui, no alignment is possible.  If span is used in FirstPrompt
            // and div for secondary ui, alignment will not be consistent.
            ConditionalRenderOpeningDivElement(writer);

            Debug.Assert(NotSecondaryUI == NotSecondaryUIInit);
            switch (SecondaryUIMode) {
                case FirstPrompt:
                    String promptText = Control.CalendarEntryText;
                    if (promptText == null || promptText.Length == 0) {
                        promptText = SR.GetString(SR.CalendarAdapterFirstPrompt);
                    }

                    // Link to input option selection screen
                    RenderPostBackEventAsAnchor(writer,
                                                OptionPrompt.ToString(CultureInfo.InvariantCulture),
                                                promptText);

                    // We should honor BreakAfter property here as the first
                    // UI is shown with other controls on the same form.
                    // For other secondary UI, it is not necessary.
                    ConditionalSetPendingBreakAfterInline(writer);
                    break;

                // Render the first secondary page that provides differnt
                // options to select a date.
                case OptionPrompt:
                    writer.Write(SR.GetString(SR.CalendarAdapterOptionPrompt));
                    writer.WriteBreak();

                    arr = new ArrayList();

                    // Option to select the default date
                    arr.Add(Control.VisibleDate.ToString(
                        currentDateTimeInfo.ShortDatePattern, CultureInfo.CurrentCulture));

                    // Option to another page that can enter a date by typing
                    arr.Add(SR.GetString(SR.CalendarAdapterOptionType));

                    // Options to a set of pages for selecting a date, a week
                    // or a month by picking month/year, week and day
                    // accordingly.  Available options are determined by
                    // SelectionMode.
                    arr.Add(SR.GetString(SR.CalendarAdapterOptionChooseDate));

                    if (Control.SelectionMode == WebControls.CalendarSelectionMode.DayWeek ||
                        Control.SelectionMode == WebControls.CalendarSelectionMode.DayWeekMonth) {

                        arr.Add(SR.GetString(SR.CalendarAdapterOptionChooseWeek));

                        if (Control.SelectionMode == WebControls.CalendarSelectionMode.DayWeekMonth) {
                            arr.Add(SR.GetString(SR.CalendarAdapterOptionChooseMonth));
                        }
                    }
                    DataBindAndRender(writer, _optionList, arr);
                    break;

                // Render a title and textbox to capture a date entered by user
                case TypeDate:
                    if (_textBoxErrorMessage != null) {
                        writer.Write(_textBoxErrorMessage);
                        writer.WriteBreak();
                    }

                    if (_selectList.Visible) {
                        writer.Write(SR.GetString(SR.CalendarAdapterOptionEra));
                        writer.WriteBreak();
                        _selectList.RenderControl(writer);
                    }

                    String numericDateFormat = GetNumericDateFormat();

                    writer.Write(SR.GetString(SR.CalendarAdapterOptionType));
                    writer.Write(":");
                    writer.WriteBreak();
                    writer.Write("(");
                    writer.Write(numericDateFormat.ToUpper(CultureInfo.InvariantCulture));
                    writer.Write(")");

                    if (!_selectList.Visible) {
                        writer.Write(GetEra(Control.VisibleDate));
                    }
                    writer.WriteBreak();

                    _textBox.Numeric = true;
                    _textBox.Size = numericDateFormat.Length;
                    _textBox.MaxLength = numericDateFormat.Length;
                    _textBox.Text = Control.VisibleDate.ToString(numericDateFormat, CultureInfo.CurrentCulture);
                    _textBox.Visible = true;
                    _textBox.RenderControl(writer);

                    // Command button for sending the textbox value back to the server
                    _command.Text = GetDefaultLabel(OKLabel);
                    _command.Visible = true;
                    _command.RenderControl(writer);
                    
                    break;

                // Render a paged list for choosing a month
                case ChooseMonth:
                    writer.Write(SR.GetString(SR.CalendarAdapterOptionChooseMonth));
                    writer.Write(":");
                    writer.WriteBreak();

                    tempDate = Control.VisibleDate;

                    String abbreviatedYearMonthPattern = AbbreviateMonthPattern(currentDateTimeInfo.YearMonthPattern);

                    // This is to be consistent with ASP.NET Calendar control
                    // on handling YearMonthPattern:
                    // Some cultures have a comma in their YearMonthPattern,
                    // which does not look right in a calendar.  Here we
                    // strip the comma off.
                    int indexComma = abbreviatedYearMonthPattern.IndexOf(',');
                    if (indexComma >= 0) {
                        abbreviatedYearMonthPattern =
                            abbreviatedYearMonthPattern.Remove(indexComma, 1);
                    }

                    arr = new ArrayList();
                    for (int i = 0; i < _monthsToDisplay; i++) {
                        arr.Add(tempDate.ToString(abbreviatedYearMonthPattern, CultureInfo.CurrentCulture));
                        tempDate = _threadCalendar.AddMonths(tempDate, 1);
                    }
                    arr.Add(GetDefaultLabel(NextLabel));
                    arr.Add(GetDefaultLabel(PreviousLabel));

                    DataBindAndRender(writer, _monthList, arr);
                    break;

                // Based on the month selected in case ChooseMonth above, render a list of
                // availabe weeks of the month.
                case ChooseWeek:
                    String monthFormat = (GetNumericDateFormat()[0] == 'y') ? "yyyy/M" : "M/yyyy";
                    writer.Write(SR.GetString(SR.CalendarAdapterOptionChooseWeek));
                    writer.Write(" (");
                    writer.Write(Control.VisibleDate.ToString(monthFormat, CultureInfo.CurrentCulture));
                    writer.Write("):");
                    writer.WriteBreak();

                    // List weeks of days of the selected month.  May include
                    // days from the previous and the next month to fill out
                    // all six week choices.  This is consistent with the
                    // ASP.NET Calendar control.

                    // Note that the event handling code of this list control
                    // should be implemented according to the index content
                    // generated here.

                    tempDate = FirstCalendarDay(Control.VisibleDate);

                    arr = new ArrayList();
                    String weekDisplay;
                    for (int i = 0; i < 6; i++) {
                        weekDisplay = tempDate.ToString(abbreviatedMonthDayPattern, CultureInfo.CurrentCulture);
                        weekDisplay += DaySeparator;
                        tempDate = _threadCalendar.AddDays(tempDate, 6);
                        weekDisplay += tempDate.ToString(abbreviatedMonthDayPattern, CultureInfo.CurrentCulture);
                        arr.Add(weekDisplay);
                        tempDate = _threadCalendar.AddDays(tempDate, 1);
                    }

                    DataBindAndRender(writer, _weekList, arr);
                    break;

                // Based on the month and week selected in case ChooseMonth and ChooseWeek above,
                // render a list of the dates in the week.
                case ChooseDay:
                    writer.Write(SR.GetString(SR.CalendarAdapterOptionChooseDate));
                    writer.Write(":");
                    writer.WriteBreak();

                    tempDate = Control.VisibleDate;

                    arr = new ArrayList();
                    String date;
                    String dayName;
                    StringBuilder dayDisplay = new StringBuilder();
                    bool dayNameFirst = (GetNumericDateFormat()[0] != 'y');

                    for (int i = 0; i < 7; i++) {
                        date = tempDate.ToString(abbreviatedMonthDayPattern, CultureInfo.CurrentCulture);

                        if (Control.ShowDayHeader) {
                            // Use the short format for displaying day name
                            dayName = GetAbbreviatedDayName(tempDate);
                            dayDisplay.Length = 0;

                            if (dayNameFirst) {
                                dayDisplay.Append(dayName);
                                dayDisplay.Append(Space);
                                dayDisplay.Append(date);
                            }
                            else {
                                dayDisplay.Append(date);
                                dayDisplay.Append(Space);
                                dayDisplay.Append(dayName);
                            }
                            arr.Add(dayDisplay.ToString());
                        }
                        else {
                            arr.Add(date);
                        }
                        tempDate = _threadCalendar.AddDays(tempDate, 1);
                    }

                    DataBindAndRender(writer, _dayList, arr);
                    break;

                default:
                    Debug.Assert(false, "Unexpected Secondary UI Mode");
                    break;
            }
            ConditionalRenderClosingDivElement(writer);
            ConditionalExitStyle(writer, Style);
        }

        /// <include file='doc\XhtmlBasicCalendarAdapter.uex' path='docs/doc[@for="XhtmlCalendarAdapter.HandlePostBackEvent"]/*' />
        public override bool HandlePostBackEvent(String eventArgument) {
            // This is mainly to capture the option picked by the user on
            // secondary pages and manipulate SecondaryUIMode accordingly so
            // Render() can generate the appropriate UI.
            // It also capture the state "Done" which can be set when a date,
            // a week or a month is selected or entered in some secondary
            // page.

            SecondaryUIMode = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);

            Debug.Assert(NotSecondaryUI == NotSecondaryUIInit);
            switch (SecondaryUIMode) {
            case DefaultDateDone:
                SelectRange(Control.VisibleDate, Control.VisibleDate);
                goto case Done;

            case TypeDate:
                break;

            case TypeDateDone:
                try {
                    String dateText = _textBox.Text;
                    String dateFormat = GetNumericDateFormat();
                    DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
                    int eraIndex = _selectList.SelectedIndex;

                    if (eraIndex >= 0 &&
                        eraIndex < currentInfo.Calendar.Eras.Length) {

                        dateText += currentInfo.GetEraName(currentInfo.Calendar.Eras[eraIndex]);
                        dateFormat += "gg";
                    }

                    DateTime dateTime = DateTime.ParseExact(dateText, dateFormat, null);
                    SelectRange(dateTime, dateTime);
                    Control.VisibleDate = dateTime;
                }
                catch {
                    _textBoxErrorMessage = SR.GetString(SR.CalendarAdapterTextBoxErrorMessage);
                    SecondaryUIMode = TypeDate;
                    goto case TypeDate;
                }
                goto case Done;

            case Done:
                // Set the secondary exit code and raise the selection event for
                // web page developer to manipulate the selected date.
                ExitSecondaryUIMode();
                _chooseOption = FirstPrompt;
                break;

            case DateOption:
            case WeekOption:
            case MonthOption:
                _chooseOption = SecondaryUIMode;  // save in the ViewState

                // In all 3 cases, continue to the UI that chooses a month
                SecondaryUIMode = ChooseMonth;
                break;
            }

            return true;
        }

        /////////////////////////////////////////////////////////////////////
        // Misc. helper and wrapper functions
        /////////////////////////////////////////////////////////////////////

        private int MonthsToDisplay(int screenCharactersHeight) {
            const int MinMonthsToDisplay = 4;
            const int MaxMonthsToDisplay = 12;

            if (screenCharactersHeight < MinMonthsToDisplay) {
                return MinMonthsToDisplay;
            }
            else if (screenCharactersHeight > MaxMonthsToDisplay) {
                return MaxMonthsToDisplay;
            }
            return screenCharactersHeight;
        }

        // A helper function to initialize and add a child list control
        private void InitList(List list,
                              ListCommandEventHandler eventHandler) {
            list.Visible = false;
            list.ItemCommand += eventHandler;
            list.EnableViewState = false;
            Control.Controls.Add(list);
        }

        private void DataBindListWithEmptyValues(List list, int arraySize) {
            ArrayList arr = new ArrayList();
            for (int i = 0; i < arraySize; i++) {
                arr.Add("");
            }
            list.DataSource = arr;
            list.DataBind();
        }

        // A helper function to do the common code for DataBind and
        // RenderChildren.
        private void DataBindAndRender(XhtmlMobileTextWriter writer,
                                       List list,
                                       ArrayList arr) {
            list.DataSource = arr;
            list.DataBind();
            list.Visible = true;
            list.RenderControl(writer);
        }

        // Abbreviate the Month format from "MMMM" (full
        // month name) to "MMM" (three-character month abbreviation)
        private String AbbreviateMonthPattern(String pattern) {
            const String FullMonthFormat = "MMMM";

            int i = pattern.IndexOf(FullMonthFormat, StringComparison.Ordinal);
            if (i != -1) {
                pattern = pattern.Remove(i, 1);
            }
            return pattern;
        }

        private String GetAbbreviatedDayName(DateTime dateTime) {
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(
                        _threadCalendar.GetDayOfWeek(dateTime));
        }

        private String GetEra(DateTime dateTime) {
            // We shouldn't need to display the era for the common Gregorian
            // Calendar
            if (DateTimeFormatInfo.CurrentInfo.Calendar.GetType() ==
                typeof(GregorianCalendar)) {
                return String.Empty;
            }
            else {
                return dateTime.ToString("gg", CultureInfo.CurrentCulture);
            }
        }

        private static readonly char[] formatChars =
                                            new char[] { 'M', 'd', 'y' };

        private String GetNumericDateFormat() {
            String shortDatePattern =
                DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

            // Guess on what short date pattern should be used
            int i = shortDatePattern.IndexOfAny(formatChars);

            char firstFormatChar;
            if (i == -1) {
                firstFormatChar = 'M';
            }
            else {
                firstFormatChar = shortDatePattern[i];
            }

            // We either use two or four digits for the year
            String yearPattern;
            if (shortDatePattern.IndexOf("yyyy", StringComparison.Ordinal) == -1) {
                yearPattern = "yy";
            }
            else {
                yearPattern = "yyyy";
            }

            switch (firstFormatChar) {
                case 'M':
                default:
                    return "MMdd" + yearPattern;
                case 'd':
                    return "ddMM" + yearPattern;
                case 'y':
                    return yearPattern + "MMdd";
            }
        }

        /////////////////////////////////////////////////////////////////////
        // Helper functions
        /////////////////////////////////////////////////////////////////////

        // Return the first date of the input year and month
        private DateTime EffectiveVisibleDate(DateTime visibleDate) {
            return _threadCalendar.AddDays(
                        visibleDate,
                        -(_threadCalendar.GetDayOfMonth(visibleDate) - 1));
        }

        // Return the beginning date of a calendar that includes the
        // targeting month.  The date can actually be in the previous month.
        private DateTime FirstCalendarDay(DateTime visibleDate) {
            DateTime firstDayOfMonth = EffectiveVisibleDate(visibleDate);
            int daysFromLastMonth =
                ((int)_threadCalendar.GetDayOfWeek(firstDayOfMonth)) -
                NumericFirstDayOfWeek();

            // Always display at least one day from the previous month
            if (daysFromLastMonth <= 0) {
                daysFromLastMonth += 7;
            }
            return _threadCalendar.AddDays(firstDayOfMonth, -daysFromLastMonth);
        }

        private int NumericFirstDayOfWeek() {
            // Used globalized value by default
            return(Control.FirstDayOfWeek == WebControls.FirstDayOfWeek.Default)
            ? (int) DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek
            : (int) Control.FirstDayOfWeek;
        }

        /////////////////////////////////////////////////////////////////////
        // The followings are event handlers to capture the selection from
        // the corresponding list control in an secondary page.  The index of
        // the selection is used to determine which and how the next
        // secondary page is rendered.  Some event handlers below update
        // Calendar.VisibleDate and set SecondaryUIMode with appropriate
        // values.
        ////////////////////////////////////////////////////////////////////////

        private void TextBoxEventHandler(Object source, EventArgs e) {
            HandlePostBackEvent(TypeDateDone.ToString(CultureInfo.InvariantCulture));
        }

        private static readonly int[] Options =
            {DefaultDateDone, TypeDate, DateOption, WeekOption, MonthOption};

        private void OptionListEventHandler(Object source, ListCommandEventArgs e) {
            SecondaryUIMode = Options[e.ListItem.Index];
            HandlePostBackEvent(SecondaryUIMode.ToString(CultureInfo.InvariantCulture));
        }

        private void MonthListEventHandler(Object source, ListCommandEventArgs e) {
            _threadCalendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            if (e.ListItem.Index == _monthsToDisplay) {
                // Next was selected
                Control.VisibleDate = _threadCalendar.AddMonths(
                                        Control.VisibleDate, _monthsToDisplay);
                SecondaryUIMode = ChooseMonth;
            }
            else if (e.ListItem.Index == _monthsToDisplay + 1) {
                // Prev was selected
                Control.VisibleDate = _threadCalendar.AddMonths(
                                        Control.VisibleDate, -_monthsToDisplay);
                SecondaryUIMode = ChooseMonth;
            }
            else {
                // A month was selected
                Control.VisibleDate = _threadCalendar.AddMonths(
                                        Control.VisibleDate,
                                        e.ListItem.Index);

                if (_chooseOption == MonthOption) {
                    // Add the whole month to the date list
                    DateTime beginDate = EffectiveVisibleDate(Control.VisibleDate);
                    Control.VisibleDate = beginDate;

                    DateTime endDate = _threadCalendar.AddMonths(beginDate, 1);
                    endDate = _threadCalendar.AddDays(endDate, -1);

                    SelectRange(beginDate, endDate);
                    HandlePostBackEvent(Done.ToString(CultureInfo.InvariantCulture));
                }
                else {
                    SecondaryUIMode = ChooseWeek;
                }
            }
        }

        private void WeekListEventHandler(Object source, ListCommandEventArgs e) {
            // Get the first calendar day and adjust it to the week the user
            // selected (to be consistent with the index setting in Render())
            _threadCalendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            DateTime tempDate = FirstCalendarDay(Control.VisibleDate);

            Control.VisibleDate = _threadCalendar.AddDays(tempDate, e.ListItem.Index * 7);

            if (_chooseOption == WeekOption) {
                // Add the whole week to the date list
                DateTime endDate = _threadCalendar.AddDays(Control.VisibleDate, 6);

                SelectRange(Control.VisibleDate, endDate);
                HandlePostBackEvent(Done.ToString(CultureInfo.InvariantCulture));
            }
            else {
                SecondaryUIMode = ChooseDay;
            }
        }

        private void DayListEventHandler(Object source, ListCommandEventArgs e) {
            _threadCalendar = DateTimeFormatInfo.CurrentInfo.Calendar;

            // VisibleDate should have been set with the first day of the week
            // so the selected index can be used to adjust to the selected day.
            Control.VisibleDate = _threadCalendar.AddDays(Control.VisibleDate, e.ListItem.Index);

            SelectRange(Control.VisibleDate, Control.VisibleDate);
            HandlePostBackEvent(Done.ToString(CultureInfo.InvariantCulture));
        }

        private void SelectRange(DateTime dateFrom, DateTime dateTo) {
            Debug.Assert(dateFrom <= dateTo, "Bad Date Range");

            // see if this range differs in any way from the current range
            // these checks will determine this because the colleciton is sorted
            TimeSpan ts = dateTo - dateFrom;
            WebControls.SelectedDatesCollection selectedDates = Control.SelectedDates;
            if (selectedDates.Count != ts.Days + 1 
                || selectedDates[0] != dateFrom
                || selectedDates[selectedDates.Count - 1] != dateTo) {

                selectedDates.SelectRange(dateFrom, dateTo);
                Control.RaiseSelectionChangedEvent();
            }
        }

        private bool IsViewStateEnabled() {
            Control ctl = Control;
            while (ctl != null) {
                if (!ctl.EnableViewState) {
                    return false;
                }
                ctl = ctl.Parent;
            }
            return true;
        }
    }
}
