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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//
// REMAINING TODO:
//	- get the date_cell_size and title_size to be pixel perfect match of SWF
//	- show the month context menu
//	- show the year spin control
//	- at some res, single selection of a date is not filling properly
//	- appears to be having issues filling the title background properly (using title_size.Width)

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace System.Windows.Forms {
	public class MonthCalendar : Control {

		#region Local variables

		ArrayList 		annually_bolded_dates;
		Color 			back_color;
		ArrayList 		bolded_dates;
		Size 			calendar_dimensions;
		Day 			first_day_of_week;
		Color 			fore_color;
		DateTime 		max_date;
		int 			max_selection_count;
		DateTime 		min_date;
		ArrayList 		monthly_bolded_dates;
		int 			scroll_change;
		SelectionRange 		selection_range;
		bool 			show_today;
		bool 			show_today_circle;
		bool 			show_week_numbers;
		Color 			title_back_color;
		Color 			title_fore_color;
		DateTime 		today_date;
		bool 			today_date_set;
		Color 			trailing_fore_color;

		// internal variables used
		internal DateTime 		current_month;			// the month that is being displayed in top left corner of the grid		
		internal ArrayList 		all_bolded_dates;		// all the bolded dates for the current month
		internal int 			button_x_offset;
		internal Size 			button_size;
		internal Size			title_size;
		internal Size			date_cell_size;
		internal Size			calendar_spacing;
		internal int			divider_line_offset;
		internal DateTime		clicked_date;
		internal bool			is_date_clicked;
		internal bool			is_previous_clicked;
		internal bool			is_next_clicked;
		internal bool 			is_shift_pressed;
		internal DateTime		shift_select_start_date;			
		
		#endregion	// Local variables

		#region Public Constructors

		public MonthCalendar() {
			// initialise default values 
			DateTime now = DateTime.Now.Date;
			selection_range = new SelectionRange (now, now);
			today_date = now;
			current_month = now;

			// iniatialise local members
			annually_bolded_dates = null;
			back_color = SystemColors.Window;
			bolded_dates = null;
			calendar_dimensions = new Size (1,1);
			first_day_of_week = Day.Default;
			fore_color = SystemColors.ControlText;
			max_date = new DateTime (9998, 12, 31);
			max_selection_count = 7;
			min_date = new DateTime (1953, 1, 1);
			monthly_bolded_dates = null;
			scroll_change = 1;
			show_today = true;
			show_today_circle = true;
			show_week_numbers = false;
			title_back_color = SystemColors.ActiveCaption;
			title_fore_color = SystemColors.ActiveCaptionText;			
			today_date_set = false;
			trailing_fore_color = Color.Gray;

			// intiailise internal variables used
			all_bolded_dates = null;
			button_x_offset = 5;
			button_size = new Size (22, 17);
			// default settings based on 8.25 pt San Serif Font
			// Not sure of algroithm used to establish this
			Size title_size = new Size(24*7, 46);		// 7 cells, not including WeekNumber column
			date_cell_size = new Size (24, 16);		// default size at san-serif 8.25
			divider_line_offset = 4;
			calendar_spacing = new Size (4, 5);		// horiz and vert spacing between months in a calendar grid

			// set some state info
			clicked_date = now;
			is_date_clicked = false;
			is_previous_clicked = false;
			is_next_clicked = false;
			is_shift_pressed = false;
			shift_select_start_date = now;

			// event handlers
			MouseDown += new MouseEventHandler (MouseDownHandler);
			KeyDown += new KeyEventHandler (KeyDownHandler);
			MouseUp += new MouseEventHandler (MouseUpHandler);
			KeyUp += new KeyEventHandler (KeyUpHandler);
			Paint += new PaintEventHandler (PaintHandler);
			
		}

		#endregion	// Public Constructors

		#region Public Instance Properties

		// dates to make bold on calendar annually (recurring)
		public DateTime[] AnnuallyBoldedDates {
			set {
				if (annually_bolded_dates == null || (DateTime[]) annually_bolded_dates.ToArray (typeof (DateTime)) != value) {
					annually_bolded_dates = new ArrayList (value);
					UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
				if (annually_bolded_dates != null) {
					return (DateTime[]) annually_bolded_dates.ToArray (typeof (DateTime));
				} else {
					return null;
				}
			}
		}

		// the back color for the main part of the calendar
		public Color BackColor {
			set {
				if (back_color != value) {
					back_color = value;
					this.OnBackColorChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
			get {
				return back_color;
			}
		}

		// specific dates to make bold on calendar (non-recurring)
		public DateTime[] BoldedDates {
			set {
				if (bolded_dates == null || (DateTime[]) bolded_dates.ToArray (typeof (DateTime)) != value) {
					bolded_dates = new ArrayList (value);
					UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
				if (bolded_dates != null) {
					return (DateTime[]) bolded_dates.ToArray (typeof (DateTime));
				} else {
					return null;
				}

			}
		}

		// the configuration of the monthly grid display - only allowed to display at most,
		// 1 calendar year at a time, will be trimmed to fit it properly
		public Size CalendarDimensions {
			set {
				if (value.Width < 0 || value.Height < 0) {
					throw new ArgumentException ();
				}
				if (calendar_dimensions != value) {
					// squeeze the grid into 1 calendar year
					if (value.Width * value.Height > 12) {
						// iteratively reduce the largest dimension till our
						// product is less than 12
						if (value.Width > 12 && value.Height > 12) {
							calendar_dimensions = new Size (4, 3);
						} else if (value.Width > 12) {
							for (int i = 12; i > 0; i--) {
								if (i * value.Height <= 12) {
									calendar_dimensions = new Size (i, value.Height);
									break;
								}
							}
						} else if (value.Height > 12) {
							for (int i = 12; i > 0; i--) {
								if (i * value.Width <= 12) {
									calendar_dimensions = new Size (value.Width, i);
									break;
								}
							}
						}
					} else {
						calendar_dimensions = value;
					}
					this.Invalidate ();
				}
			}
			get {
				return calendar_dimensions;
			}
		}

		// the first day of the week to display
		public Day FirstDayOfWeek {
			set {
				if (first_day_of_week != value) {
					first_day_of_week = value;
					this.Invalidate ();
				}
			}
			get {
				return first_day_of_week;
			}
		}

		// the fore color for the main part of the calendar
		public Color ForeColor {
			set {
				if (fore_color != value) {
					fore_color = value;
					this.OnForeColorChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
			get {
				return fore_color;
			}
		}

		// the maximum date allowed to be selected on this month calendar
		public DateTime MaxDate {
			set {
				if (value < MinDate) {
					throw new ArgumentException();
				}

				if (max_date != value) {
					max_date = value;
				}
			}
			get {
				return max_date;
			}
		}

		// the maximum number of selectable days
		public int MaxSelectionCount {
			set {
				if (value < 0) {
					throw new ArgumentException();
				}
		
				// can't set selectioncount less than already selected dates
				if ((SelectionEnd - SelectionStart).Days > value) {
					throw new ArgumentException();
				}
			
				if (max_selection_count != value) {
					max_selection_count = value;
					this.Invalidate ();
				}
			}
			get {
				return max_selection_count;
			}
		}

		// the minimum date allowed to be selected on this month calendar
		public DateTime MinDate {
			set {
				if (value < new DateTime (1953, 1, 1)) {
					throw new ArgumentException();
				}

				if (value > MaxDate) {
					throw new ArgumentException();
				}

				if (max_date != value) {
					min_date = value;
				}
			}
			get {
				return min_date;
			}
		}

		// dates to make bold on calendar monthly (recurring)
		public DateTime[] MonthlyBoldedDates {
			set {
				if (monthly_bolded_dates == null || (DateTime[]) monthly_bolded_dates.ToArray (typeof (DateTime)) != value) {
					monthly_bolded_dates = new ArrayList (value);
					UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
				if (monthly_bolded_dates != null) {
					return (DateTime[]) monthly_bolded_dates.ToArray (typeof (DateTime));
				} else {
					return null;
				}
			}
		}

		// the maximum date allowed to be selected on this month calendar
		public int ScrollChange {
			set {
				if (value < 0 || value > 20000) {
					throw new ArgumentException();
				}

				// if zero it to the default -> the total number of months currently visible
				if (value == 0) {
					scroll_change = CalendarDimensions.Width * CalendarDimensions.Height;
				}

				if (scroll_change != value) {
					scroll_change = value;
				}
			}
			get {
				return scroll_change;
			}
		}


		// the last selected date
		public DateTime SelectionEnd {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (SelectionRange.End != value) {
					SelectionRange.End = value;
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.Invalidate ();
				}
			}
			get {
				return SelectionRange.End;
			}
		}

		// the range of selected dates
		public SelectionRange SelectionRange {
			set {
				if (selection_range != value) {
					selection_range = value;
					SelectionRange visible_range = this.GetDisplayRange(true);
					if(visible_range.Start > selection_range.End) {
						current_month = new DateTime (selection_range.Start.Year, selection_range.Start.Month, 1);
					} else if (visible_range.End < selection_range.Start) {
						int year_diff = selection_range.End.Year - visible_range.End.Year;
						int month_diff = selection_range.End.Month - visible_range.End.Month;
						current_month = current_month.AddMonths(year_diff * 12 + month_diff);
					}
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.Invalidate ();
				}
			}
			get {
				return selection_range;
			}
		}

		// the first selected date
		public DateTime SelectionStart {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (selection_range.Start != value) {
					selection_range.Start = value;
					CurrentMonth = value;
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.Invalidate ();
				}
			}
			get {
				return selection_range.Start;
			}
		}

		// whether or not to show todays date
		public bool ShowToday {
			set {
				if (show_today != value) {
					show_today = value;
					this.Invalidate ();
				}
			}
			get {
				return show_today;
			}
		}

		// whether or not to show a circle around todays date
		public bool ShowTodayCircle {
			set {
				if (show_today_circle != value) {
					show_today_circle = value;
					this.Invalidate ();
				}
			}
			get {
				return show_today_circle;
			}
		}

		// whether or not to show numbers beside each row of weeks
		public bool ShowWeekNumbers {
			set {
				if (show_week_numbers != value) {
					show_week_numbers = value;
					this.Invalidate ();
				}
			}
			get {
				return show_week_numbers;
			}
		}

		// the rectangle size required to render one month based on current font
		public Size SingleMonthSize {
			get {
				if (this.Font == null) {
					throw new InvalidOperationException();
				}

				// multiplier is sucked out from the font size
				int multiplier = this.Font.Height;

				// establis how many columns and rows we have					
				int column_count = (ShowWeekNumbers) ? 8 : 7;
				int row_count = 7;		// not including the today date

				// set the date_cell_size and the title_size
				date_cell_size = new Size ((int) Math.Ceiling (2.5 * multiplier), (int) Math.Ceiling (1.5 * multiplier));
				title_size = new Size ((date_cell_size.Width * column_count), 3 * multiplier);

				return new Size (column_count * date_cell_size.Width, row_count * date_cell_size.Height + title_size.Height);
			}
		}

		// the back color for the title of the calendar and the
		// forecolor for the day of the week text
		public Color TitleBackColor {
			set {
				if (title_back_color != value) {
					title_back_color = value;
					this.Invalidate ();
				}
			}
			get {
				return title_back_color;
			}
		}

		// the fore color for the title of the calendar
		public Color TitleForeColor {
			set {
				if (title_fore_color != value) {
					title_fore_color = value;
					this.Invalidate ();
				}
			}
			get {
				return title_fore_color;
			}
		}

		// the date this calendar is using to refer to today's date
		public DateTime TodayDate {
			set {
				today_date_set = true;
				if (today_date != value) {
					today_date = value;
					this.Invalidate ();
				}
			}
			get {
				return today_date;
			}
		}

		// tells if user specifically set today_date for this control		
		public bool TodayDateSet {
			get {
				return today_date_set;
			}
		}

		// the color used for trailing dates in the calendar
		public Color TrailingForeColor {
			set {
				if (trailing_fore_color != value) {
					trailing_fore_color = value;
					this.Invalidate ();
				}
			}
			get {
				return trailing_fore_color;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties

		// not sure what to put in here - just doing a base() call - jba
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}


		// not sure what to put in here - just doing a base() call - jba
		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}

		protected override Size DefaultSize {
			get {
				Size single_month = SingleMonthSize;
				// get the width
				int width = calendar_dimensions.Width * single_month.Width;
				if (calendar_dimensions.Width > 1) {
					width += (calendar_dimensions.Width - 1) * calendar_spacing.Width;
				}

				// get the height
				int height = calendar_dimensions.Height * single_month.Height;
				if (this.ShowToday) {
					height += date_cell_size.Height + 2;		// add the height of the "Today: " ...
				}
				if (calendar_dimensions.Height > 1) {
					height += (calendar_dimensions.Height - 1) * calendar_spacing.Height;
				}

				// add the 1 pixel boundary
				if (width > 0) {
					width += 2;
				}
				if (height > 0) {
					height +=2;
				}

				return new Size (width, height);
			}
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods

		// add a date to the anually bolded date arraylist
		public void AddAnnuallyBoldedDate (DateTime date) {
			annually_bolded_dates.Add (date);
		}

		// add a date to the normal bolded date arraylist
		public void AddBoldedDate (DateTime date) {
			bolded_dates.Add (date);
		}

		// add a date to the anually monthly date arraylist
		public void AddMonthlyBoldedDate (DateTime date) {
			monthly_bolded_dates.Add (date);
		}

		// if visible = true, return only the dates of full months, else return all dates visible
		public SelectionRange GetDisplayRange (bool visible) {
			DateTime start;
			DateTime end;
			start = new DateTime (current_month.Year, current_month.Month, 1);
			end = start.AddMonths (calendar_dimensions.Width * calendar_dimensions.Height);
			end = end.AddDays(-1);

			// process all visible dates if needed (including the grayed out dates
			if (!visible) {
				start = GetFirstDateInMonthGrid (start);
				end = GetLastDateInMonthGrid (end);
			}

			return new SelectionRange (start, end);
		}

		// HitTest overload that recieve's x and y co-ordinates as separate ints
		public HitTestInfo HitTest (int x, int y) {
			return HitTest (new Point (x, y));
		}

		// returns a HitTestInfo for MonthCalendar element's under the specified point
		public HitTestInfo HitTest (Point point) {
			// before doing all the hard work, see if the today's date wasn't clicked
			Rectangle today_rect = new Rectangle (
				ClientRectangle.X, 
				ClientRectangle.Bottom - date_cell_size.Height,
				7 * date_cell_size.Width,
				date_cell_size.Height);
			if (today_rect.Contains (point) && this.ShowToday) {
				return new HitTestInfo(HitArea.TodayLink, point, DateTime.Now);
			}

			Size month_size = SingleMonthSize;
			// define calendar rect's that this thing can land in
			Rectangle[] calendars = new Rectangle [CalendarDimensions.Width * CalendarDimensions.Height];
			for (int i=0; i < CalendarDimensions.Width * CalendarDimensions.Height; i ++) {
				if (i == 0) {
					calendars[i] = new Rectangle (
						new Point (ClientRectangle.X + 1, ClientRectangle.Y + 1),
						month_size);
				} else {
					// calendar on the next row
					if (i % CalendarDimensions.Width == 0) {
						calendars[i] = new Rectangle (
							new Point (calendars[i-CalendarDimensions.Width].X, calendars[i-CalendarDimensions.Width].Bottom + calendar_spacing.Height),
							month_size);
					} else {
						// calendar on the next column
						calendars[i] = new Rectangle (
							new Point (calendars[i-1].Right + calendar_spacing.Width, calendars[i-1].Y),
							month_size);
					}
				}
			}
			
			// through each trying to find a match
			for (int i = 0; i < calendars.Length ; i++) {
				if (calendars[i].Contains (point)) {					
					// check the title section
					Rectangle title_rect = new Rectangle (
						calendars[i].Location,
						title_size);
					if (title_rect.Contains (point) ) {
						// make sure it's not a previous button
						if (i == 0) {
							Rectangle button_rect = new Rectangle(
								new Point (calendars[i].X + button_x_offset, (title_size.Height - button_size.Height)/2),
								button_size);
							if (button_rect.Contains (point)) 
							{
								return new HitTestInfo(HitArea.PrevMonthButton, point, DateTime.Now);
							}
						}
						// make sure it's not the next button
						if (i % CalendarDimensions.Height == 0 && i % CalendarDimensions.Width == calendar_dimensions.Width - 1) {
							Rectangle button_rect = new Rectangle(
								new Point (calendars[i].Right - button_x_offset - button_size.Width, (title_size.Height - button_size.Height)/2),
								button_size);
							if (button_rect.Contains (point)) 
							{
								return new HitTestInfo(HitArea.NextMonthButton, point, DateTime.Now);
							}
						}

						// make sure it's not the month or the year of the calendar
						if (GetMonthNameRectangle (title_rect, i).Contains (point)) {
							return new HitTestInfo(HitArea.TitleMonth, point, DateTime.Now);
						}
						if (GetYearNameRectangle (title_rect, i).Contains (point)) {
							return new HitTestInfo(HitArea.TitleYear, point, DateTime.Now);
						}

						// return the hit test in the title background
						return new HitTestInfo(HitArea.TitleBackground, point, DateTime.Now);
					}

					Point date_grid_location = new Point (calendars[i].X, title_rect.Bottom);

					// see if it's in the Week numbers
					if (ShowWeekNumbers) {
						Rectangle weeks_rect = new Rectangle (
							date_grid_location,
							new Size (date_cell_size.Width,Math.Max (calendars[i].Height - title_rect.Height, 0)));
						if (weeks_rect.Contains (point)) {
							return new HitTestInfo(HitArea.WeekNumbers, point, DateTime.Now);
						}

						// move the location of the grid over
						date_grid_location.X += date_cell_size.Width;
					}

					// see if it's in the week names
					Rectangle day_rect = new Rectangle (
						date_grid_location,
						new Size (Math.Max (calendars[i].Right - date_grid_location.X, 0), date_cell_size.Height));
					if (day_rect.Contains (point)) {						
						return new HitTestInfo(HitArea.DayOfWeek, point, DateTime.Now);
					}
						
					// finally see if it was a date that was clicked
					Rectangle date_grid = new Rectangle (
						new Point (day_rect.X, day_rect.Bottom),
						new Size (day_rect.Width, Math.Max(calendars[i].Bottom - day_rect.Bottom, 0)));
					if (date_grid.Contains (point)) {
						// okay so it's inside the grid, get the offset
						Point offset = new Point (point.X - date_grid.X, point.Y - date_grid.Y);
						int row = offset.Y / date_cell_size.Height;
						int col = offset.X / date_cell_size.Width;
						// establish our first day of the month
						DateTime calendar_month = this.CurrentMonth.AddMonths(i);
						DateTime first_day = GetFirstDateInMonthGrid (calendar_month);
						DateTime time = first_day.AddDays ((row * 7) + col);
						// establish which date was clicked
						if (time.Year != calendar_month.Year || time.Month != calendar_month.Month) {
							if (time < calendar_month && i == 0) {
								return new HitTestInfo(HitArea.PrevMonthDate, point, time);
							} else if (time > calendar_month && i == CalendarDimensions.Width*CalendarDimensions.Height - 1) {
								return new HitTestInfo(HitArea.NextMonthDate, point, time);
							}
							return new HitTestInfo(HitArea.NoWhere, point, time);
						}
						return new HitTestInfo(HitArea.Date, point, time);
					}
				}				
			}

			return new HitTestInfo ();
		}

		// clears all the annually bolded dates
		public void RemoveAllAnnuallyBoldedDates () {
			if (annually_bolded_dates != null) {
				annually_bolded_dates.Clear ();
			}
		}

		// clears all the normal bolded dates
		public void RemoveAllBoldedDates () {
			if (bolded_dates != null) {
				bolded_dates.Clear ();
			}
		}

		// clears all the monthly bolded dates
		public void RemoveAllMonthlyBoldedDates () {
			if (monthly_bolded_dates != null) {
				monthly_bolded_dates.Clear ();
			}
		}

		// clears the specified annually bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveAnnuallyBoldedDate (DateTime date) {
			if (annually_bolded_dates != null) {
				foreach (DateTime bolded_date in annually_bolded_dates) {
					if (bolded_date.Day == date.Day && bolded_date.Month == date.Month) {
						annually_bolded_dates.Remove (bolded_date);
						break;
					}
				}
			}
		}

		// clears all the normal bolded date
		// only removes the first instance of the match
		public void RemoveBoldedDate (DateTime date) {
			if (bolded_dates != null) {
				int match = bolded_dates.IndexOf (date);
				bolded_dates.Remove (match);
			}
		}

		// clears the specified monthly bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveMonthlyBoldedDate (DateTime date) {
			if (monthly_bolded_dates != null) {
				foreach (DateTime bolded_date in monthly_bolded_dates) {
					if (bolded_date.Day == date.Day && bolded_date.Month == date.Month) {
						monthly_bolded_dates.Remove (bolded_date);
						break;
					}
				}

			}
		}

		// sets the calendar_dimensions. If product is > 12, the larger dimension is reduced to make product < 12
		public void SetCalendarDimensions(int x, int y) {
			this.CalendarDimensions = new Size(x, y);
		}

		// sets the currently selected date as date
		public void SetDate (DateTime date) {
			this.SetSelectionRange (date, date);
		}

		// utility method set the SelectionRange property using individual dates
		public void SetSelectionRange (DateTime date1, DateTime date2) {
			this.SelectionRange = new SelectionRange(date1, date2);
		}

		public override string ToString () {
			return this.GetType().Name + ", " + this.SelectionRange.ToString ();
		}

		// usually called after an AddBoldedDate method is called
		// formats monthly and daily bolded dates according to the current calendar year
		public void UpdateBoldedDates () {
			// clear array list
			if (all_bolded_dates == null) {
				all_bolded_dates = new ArrayList ();
			} else {
				all_bolded_dates.Clear ();
			}

			// set up all the dates
			if (bolded_dates != null) {
				all_bolded_dates.AddRange (bolded_dates);				
			}
			if (annually_bolded_dates != null) {
				// adjust the year to be the currently visible one
				foreach (DateTime date in annually_bolded_dates) {
					DateTime new_date = new DateTime (CurrentMonth.Year, date.Month, date.Day);
					if (!all_bolded_dates.Contains (new_date)) {
						all_bolded_dates.Add (new_date);
					}
				}
			}
			if (monthly_bolded_dates != null) {
				// add one instance of the monthly bolded date for each month
				foreach (DateTime date in monthly_bolded_dates) {
					for (int i=1; i <= 12; i++) {
						DateTime new_date = new DateTime (CurrentMonth.Year, i, date.Day);
						if (!all_bolded_dates.Contains (new_date)) {
							all_bolded_dates.Add (new_date);
						}
					}
				}
			}

			// sort the array list
			all_bolded_dates.Sort();
		}

		#endregion	// Public Instance Methods

		#region	Protected Instance Methods

		// not sure why this needs to be overriden
		protected override void CreateHandle () {
			base.CreateHandle ();
		}

		// not sure why this needs to be overriden
		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);
		}

		// not sure why this needs to be overriden
		protected override bool IsInputKey (Keys keyData) {
			return base.IsInputKey (keyData);
		}

		// not sure why this needs to be overriden
		protected override void OnBackColorChanged (EventArgs e) {
			base.OnBackColorChanged (e);
			this.Invalidate ();
		}

		// raises the date changed event
		protected virtual void OnDateChanged (DateRangeEventArgs drevent) {
			if (this.DateChanged != null) {
				this.DateChanged (this, drevent);
			}
		}

		// raises the DateSelected event
		protected virtual void OnDateSelected (DateRangeEventArgs drevent) {
			if (this.DateSelected != null) {
				this.DateSelected (this, drevent);
			}
		}

		protected override void OnFontChanged (EventArgs e) {
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e) {
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
		}

		// i think this is overriden to not allow the control to be changed to an arbitrary size
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) {
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height ||
				(specified & BoundsSpecified.Width) == BoundsSpecified.Width ||
				(specified & BoundsSpecified.Size) == BoundsSpecified.Size) {
				// only allow sizes = default size to be set
				base.SetBoundsCore (x, y, DefaultSize.Width, DefaultSize.Height, specified);
			} else {
				base.SetBoundsCore (x, y, width, height, specified);
			}
		}

		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}

		#endregion	// Protected Instance Methods

		#region public events

		// fired when the date is changed (either explicitely or implicitely)
		// when navigating the month selector
		public event DateRangeEventHandler DateChanged;

		// fired when the user explicitely clicks on date to select it
		public event DateRangeEventHandler DateSelected;

		#endregion	// public events

		#region internal properties

		internal DateTime CurrentMonth {
			set {
				// only interested in if the month (not actual date) has changed
				if (value.Month != current_month.Month ||
					value.Year != current_month.Year) {
					this.SelectionRange = new SelectionRange(
						this.SelectionStart.Add(value.Subtract(current_month)),
						this.SelectionEnd.Add(value.Subtract(current_month)));
					current_month = value;
					UpdateBoldedDates();
					this.Invalidate();
				}
			}
			get {
				return current_month;
			}
		}

		#endregion	// internal properties

		#region internal/private methods

		// returns the date of the first cell of the specified month grid
		internal DateTime GetFirstDateInMonthGrid (DateTime month) {
			// convert the first_day_of_week into a DayOfWeekEnum
			DayOfWeek first_day = GetDayOfWeek (first_day_of_week);
			// find the first day of the month
			DateTime first_date_of_month = new DateTime (month.Year, month.Month, 1);
			DayOfWeek first_day_of_month = first_date_of_month.DayOfWeek;			
			// adjust for the starting day of the week
			int offset = first_day_of_month - first_day;
			if (offset < 0) {
				offset += 7;
			}
			return first_date_of_month.AddDays (-1*offset);
		}

		// returns the date of the last cell of the specified month grid
		internal DateTime GetLastDateInMonthGrid (DateTime month) 
		{
			DateTime start = GetFirstDateInMonthGrid(month);
			return start.AddDays ((7 * 6)-1);
		}

		// returns the first date of the month
		private DateTime GetFirstDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1);
		}

		// returns the last date of the month
		private DateTime GetLastDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
		}

		// attempts to add the date to the selection without throwing exception
		private void SelectDate (DateTime date, bool add_to_selection) {
			// try and add the new date to the selction range
			if (add_to_selection) {
				SelectionRange new_range;
				if (date < SelectionStart) {
					new_range = new SelectionRange (date, SelectionEnd);
				} else if (clicked_date > SelectionEnd) {
					new_range = new SelectionRange (date, SelectionStart);
				} else {
					// it's inside the selected dates, just ignore
					new_range = SelectionRange;							
				}
				// only allow the selection if the range isn't too large
				if (((TimeSpan)new_range.End.Subtract (new_range.Start)).Days <= MaxSelectionCount) {
					SelectionRange = new_range;
				}
			} else {
				SelectionRange = new SelectionRange (date, date);
			}
		}

		// gets the week of the year
		internal int GetWeekOfYear (DateTime date) {
			// convert the first_day_of_week into a DayOfWeekEnum
			DayOfWeek first_day = GetDayOfWeek (first_day_of_week);
			// find the first day of the year
			DayOfWeek first_day_of_year = new DateTime (date.Year, 1, 1).DayOfWeek;
			DayOfWeek day_of_week = date.DayOfWeek;
			// adjust for the starting day of the week
			int offset = first_day_of_year - first_day;
			int week = ((date.DayOfYear + offset) / 7) + 1;
			return week;
		}

		// convert a Day enum into a DayOfWeek enum
		internal DayOfWeek GetDayOfWeek (Day day) {
			if (day == Day.Default) {
				return DayOfWeek.Sunday;
			} else {
				return (DayOfWeek) DayOfWeek.Parse (typeof (DayOfWeek), day.ToString ());
			}
		}

		// returns the rectangle for themonth name
		internal Rectangle GetMonthNameRectangle (Rectangle title_rect, int calendar_index) {
			Graphics g = this.DeviceContext;
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			Size title_text_size = g.MeasureString (this_month.ToString ("MMMM yyyy"), this.Font).ToSize ();
			Size month_size = g.MeasureString (this_month.ToString ("MMMM"), this.Font).ToSize ();
			// return only the month name part of that
			return new Rectangle (
				new Point (
					title_rect.X + ((title_rect.Width - title_text_size.Width)/2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height)/2)),
				month_size);
		}

		// returns the rectangle for the year in the title
		internal Rectangle GetYearNameRectangle (Rectangle title_rect, int calendar_index) {
			
			Graphics g = this.DeviceContext;
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			Size title_text_size = g.MeasureString (this_month.ToString ("MMMM yyyy"), this.Font).ToSize ();
			Size year_size = g.MeasureString (this_month.ToString ("yyyy"), this.Font).ToSize ();
			// find out how much space the title took
			Rectangle text_rect =  new Rectangle (
				new Point (
					title_rect.X + ((title_rect.Width - title_text_size.Width)/2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height)/2)),
				title_text_size);
			// return only the rect of the year
			return new Rectangle (
				new Point (
					text_rect.Right - year_size.Width,
					text_rect.Y),
				year_size);
		}

		// determine if date is allowed to be drawn in month
		internal bool IsValidWeekToDraw (DateTime month, DateTime date, int row, int col) {
			DateTime tocheck = month.AddMonths (-1);
			if ((month.Year == date.Year && month.Month == date.Month) ||
				(tocheck.Year == date.Year && tocheck.Month == date.Month)) {
				return true;
			}

			// check the railing dates (days in the month after the last month in grid)
			if (row == CalendarDimensions.Height - 1 && col == CalendarDimensions.Width - 1) {
				tocheck = month.AddMonths (1);
				return (tocheck.Year == date.Year && tocheck.Month == date.Month) ;
			}

			return false;			
		}

		// set one item clicked and all others off
		private void SetItemClick(HitTestInfo hti) 
		{
			switch(hti.HitArea) {
				case HitArea.NextMonthButton:
					this.is_previous_clicked = false;
					this.is_next_clicked = true;
					this.is_date_clicked = false;
					break;
				case HitArea.PrevMonthButton:
					this.is_previous_clicked = true;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					break;
				case HitArea.PrevMonthDate:
				case HitArea.NextMonthDate:
				case HitArea.Date:
					this.clicked_date = hti.Time;
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = true;
					break;
				default :
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					break;
			}
		}

		// to check if the mouse has come down on this control
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			// reset first			
			bool invalidate = true;
			//establish where was hit
			HitTestInfo hti = this.HitTest(e.X, e.Y);
			switch(hti.HitArea) {
				case HitArea.NextMonthButton:
					// show the click then move on
					SetItemClick(hti);
					this.Invalidate ();
					this.CurrentMonth = this.CurrentMonth.AddMonths (CalendarDimensions.Width * CalendarDimensions.Height);
					break;
				case HitArea.PrevMonthButton:
					// show the click then move on
					SetItemClick(hti);
					this.Invalidate ();
					this.CurrentMonth = this.CurrentMonth.AddMonths ((CalendarDimensions.Width * CalendarDimensions.Height)*-1);
					break;
				case HitArea.PrevMonthDate:
					SetItemClick(hti);
					this.SelectionRange = new SelectionRange (clicked_date, clicked_date);
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case HitArea.NextMonthDate:
					SetItemClick(hti);
					this.SelectionRange = new SelectionRange (clicked_date, clicked_date);					
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case HitArea.TitleMonth:
					//TODO: show the month context menu
					System.Console.WriteLine ("//TODO: show the month context menu");
					break;
				case HitArea.TitleYear:
					//TODO: show the year spin control
					System.Console.WriteLine ("//TODO: show the year spin control");
					break;
				case HitArea.TodayLink:
					this.SetSelectionRange (DateTime.Now.Date, DateTime.Now.Date);
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case HitArea.Date:
					// see if user is selecting multiple dates
					bool add_to_selection = is_date_clicked || is_shift_pressed;
					SetItemClick(hti);
					// see if it was a selection
					this.SelectDate (clicked_date, add_to_selection);
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				default:
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					invalidate = false;
					break;
			}
			
			// invalidate if something worthwhile happened
			if (invalidate) {
				this.Invalidate ();
			}
		}

		// raised by any key down events
		private void KeyDownHandler (object sender, KeyEventArgs e) {
System.Console.WriteLine ("Key press on calendar with " + e.KeyCode);

			if (!is_shift_pressed && e.Shift) {
				shift_select_start_date = SelectionStart;
				is_shift_pressed = e.Shift;
			}
			bool changed = false;		
			switch (e.KeyCode) {
				case Keys.Home:
					// set the date to the start of the month
					if (is_shift_pressed) {
						DateTime date = GetFirstDateInMonth (this.SelectionStart);
						if (date < this.SelectionStart.AddDays (MaxSelectionCount * -1)) {
							date = this.SelectionStart.AddDays (MaxSelectionCount * -1);
						}
						this.SetSelectionRange (date, this.SelectionStart);
					} else {
						DateTime date = GetFirstDateInMonth (this.SelectionStart);
						this.SetSelectionRange (date, date);
					}
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case Keys.End:
					// set the date to the last of the month
					if (is_shift_pressed) {
						DateTime date = GetLastDateInMonth (this.SelectionStart);
						if (date > this.SelectionStart.AddDays (MaxSelectionCount)) {
							date = this.SelectionStart.AddDays (MaxSelectionCount);
						}
						this.SetSelectionRange (date, this.SelectionStart);
					} else {
						DateTime date = GetLastDateInMonth (this.SelectionStart);
						this.SetSelectionRange (date, date);
					}
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case Keys.PageUp:
					// set the date to the last of the month
					if (is_shift_pressed) {
						DateTime date = this.SelectionStart.AddMonths (-1);
						if (date < this.SelectionStart.AddDays (MaxSelectionCount*-1)) {
							date = this.SelectionStart.AddDays (MaxSelectionCount*-1);
						}
						this.SetSelectionRange (date, this.SelectionStart);
					} else {
						DateTime date = this.SelectionStart.AddMonths (-1);
						this.SetSelectionRange (date, date);
					}
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case Keys.PageDown:
					// set the date to the last of the month
					if (is_shift_pressed) {
						DateTime date = this.SelectionStart.AddMonths (1);
						if (date > this.SelectionStart.AddDays (MaxSelectionCount)) {
							date = this.SelectionStart.AddDays (MaxSelectionCount);
						}
						this.SetSelectionRange (date, this.SelectionStart);
					} else {
						DateTime date = this.SelectionStart.AddMonths (1);
						this.SetSelectionRange (date, date);
					}
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				case Keys.Up:
					// set the back 1 week
					if (is_shift_pressed) {
						DateTime date;
						// find out if we need to move forward or backward
						if (SelectionEnd > shift_select_start_date) {
							date = SelectionEnd.AddDays (-7);
							if (date >= SelectionStart.AddDays (MaxSelectionCount*-1)) {
								this.SetSelectionRange (date, SelectionStart);
								changed = true;
							} 
						} else {
							date = SelectionStart.AddDays (-7);
							if (date >= SelectionEnd.AddDays (MaxSelectionCount*-1)) {
								this.SetSelectionRange (date, SelectionEnd);
								changed = true;
							}
						}
					} else {
						DateTime date = this.SelectionStart.AddDays (-7);
						this.SetSelectionRange (date, date);
						changed = true;
					}
					if (changed) {
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					}
					break;
				case Keys.Down:
					// set the date forward 1 week
					if (is_shift_pressed) {
						DateTime date;
						// find out if we need to move forward or backward
						if (SelectionEnd > shift_select_start_date) {
							date = SelectionEnd.AddDays (7);
							if (date <= SelectionStart.AddDays (MaxSelectionCount)) {
								this.SetSelectionRange (date, SelectionStart);
								changed = true;
							} 
						} else {
							date = SelectionStart.AddDays (7);
							this.SetSelectionRange (date, SelectionEnd);
							changed = true;
						}
					} else {
						DateTime date = this.SelectionStart.AddDays (7);
						this.SetSelectionRange (date, date);
						changed = true;
					}
					if (changed) {
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					}
					break;
				case Keys.Left:
					// move one left
					if (is_shift_pressed) {
						DateTime date;
						// find out if we need to move forward or backward
						if (SelectionEnd > shift_select_start_date) {
							date = SelectionEnd.AddDays (-1);
							this.SetSelectionRange (date, SelectionStart);
							changed = true;
						} else {
							date = SelectionStart.AddDays (-1);
							if (date >= SelectionEnd.AddDays (MaxSelectionCount*-1)) {
								this.SetSelectionRange (date, SelectionEnd);
								changed = true;
							}
						}
					} else {
						DateTime date = this.SelectionStart.AddDays (-1);
						this.SetSelectionRange (date, date);
						changed = true;
					}
					if (changed) {
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					}
					break;
				case Keys.Right:
					// move one left
					if (is_shift_pressed) {
						DateTime date;
						// find out if we need to move forward or backward
						if (SelectionEnd > shift_select_start_date) {
							date = SelectionEnd.AddDays (1);
							if (date <= SelectionStart.AddDays (MaxSelectionCount)) {
								this.SetSelectionRange (date, SelectionStart);
								changed = true;
							} 
						} else {
							date = SelectionStart.AddDays (1);
							this.SetSelectionRange (date, SelectionEnd);
							changed = true;
						}
					} else {
						DateTime date = this.SelectionStart.AddDays (1);
						this.SetSelectionRange (date, date);
						changed = true;
					}
					if (changed) {
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					}
					break;
				default:
					// do nothing
					break;
			}
			e.Handled = true;
		}

		// to check if the mouse has come up on this control
		private void MouseUpHandler (object sender, MouseEventArgs e)
		{
			this.is_previous_clicked = false;
			this.is_next_clicked = false;
			this.is_date_clicked = false;
			this.Invalidate();
		}

		// raised by any key up events
		private void KeyUpHandler (object sender, KeyEventArgs e) {
			if (e.Shift) {
				is_shift_pressed = false;
			}
			e.Handled = true;
		}

		// paint this control now
		private void PaintHandler (object sender, PaintEventArgs pe) {
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw ();
			pe.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		internal void Draw ()
		{			
			ThemeEngine.Current.DrawMonthCalendar(DeviceContext, ClientRectangle, this);
		}

		#endregion 	//internal methods

		#region internal drawing methods


		#endregion	// internal drawing methods

		#region inner classes and enumerations

		// enumeration about what type of area on the calendar was hit 
		public enum HitArea {
			NoWhere,
			TitleBackground,
			TitleMonth,
			TitleYear,
			NextMonthButton,
			PrevMonthButton,
			CalendarBackground,
			Date,
			NextMonthDate,
			PrevMonthDate,
			DayOfWeek,
			WeekNumbers,
			TodayLink
		}
		
		// info regarding to a hit test on this calendar
		public sealed class HitTestInfo {

			private HitArea hit_area;
			private Point point;
			private DateTime time;

			// default constructor
			internal HitTestInfo () {
				hit_area = HitArea.NoWhere;
				point = new Point (0, 0);
				time = DateTime.Now;
			}

			// overload receives all properties
			internal HitTestInfo (HitArea hit_area, Point point, DateTime time) {
				this.hit_area = hit_area;
				this.point = point;
				this.time = time;
			}

			// the type of area that was hit
			public HitArea HitArea {
				get {
					return hit_area;
				}
			}

			// the point that is being test
			public Point Point {
				get {
					return point;
				}
			}
			
			// the date under the hit test point, only valid if HitArea is Date
			public DateTime Time {
				get {
					return time;
				}
			}
		}

		#endregion 	// inner classes


	}
}
