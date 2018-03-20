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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//
// REMAINING TODO:
//	- get the date_cell_size and title_size to be pixel perfect match of SWF

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultBindingProperty("SelectionRange")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[DefaultProperty("SelectionRange")]
	[DefaultEvent("DateChanged")]
	[Designer ("System.Windows.Forms.Design.MonthCalendarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class MonthCalendar : Control {
		#region Local variables
		ArrayList		annually_bolded_dates;
		ArrayList		monthly_bolded_dates;
		ArrayList		bolded_dates;
		Size 			calendar_dimensions;
		Day 			first_day_of_week;
		DateTime 		max_date;
		int 			max_selection_count;
		DateTime 		min_date;
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
		ContextMenu		today_menu;
		ContextMenu		month_menu;
		Timer			timer;
		Timer			updown_timer;
		const int		initial_delay = 500;
		const int		subsequent_delay = 100;
		private bool		is_year_going_up;
		private bool		is_year_going_down;
		private bool		is_mouse_moving_year;
		private int		year_moving_count;
		private bool 		date_selected_event_pending;
		bool			right_to_left_layout;

		// internal variables used
		internal bool			show_year_updown;
		internal DateTime 		current_month;			// the month that is being displayed in top left corner of the grid		
		internal DateTimePicker		owner;				// used if this control is popped up
		internal int 			button_x_offset;
		internal Size 			button_size;
		internal Size			title_size;
		internal Size			date_cell_size;
		internal Size			calendar_spacing;
		internal int			divider_line_offset;
		internal DateTime		clicked_date;
		internal Rectangle 		clicked_rect;
		internal bool			is_date_clicked;
		internal bool			is_previous_clicked;
		internal bool			is_next_clicked;
		internal bool 			is_shift_pressed;
		internal DateTime		first_select_start_date;
		internal int			last_clicked_calendar_index;
		internal Rectangle		last_clicked_calendar_rect;
		internal Font 			bold_font;			// Cache the font in FontStyle.Bold
		internal StringFormat		centered_format;		// Cache centered string format
		private Point			month_title_click_location;
		// this is used to see which item was actually clicked on in the beginning
		// so that we know which item to fire on timer
		//	0: date clicked
		//	1: previous clicked
		//	2: next clicked
		private bool[]			click_state;
		
		
		
		#endregion	// Local variables

		#region Public Constructors

		public MonthCalendar () {
			// set up the control painting
			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);
			
			// mouse down timer
			timer = new Timer ();
			timer.Interval = 500;
			timer.Enabled = false;
			
			// initialise default values 
			DateTime now = DateTime.Now.Date;
			selection_range = new SelectionRange (now, now);
			today_date = now;
			current_month = new DateTime (now.Year , now.Month, 1);

			// iniatialise local members
			annually_bolded_dates = null;
			bolded_dates = null;
			calendar_dimensions = new Size (1,1);
			first_day_of_week = Day.Default;
			max_date = new DateTime (9998, 12, 31);
			max_selection_count = 7;
			min_date = new DateTime (1753, 1, 1);
			monthly_bolded_dates = null;
			scroll_change = 0;
			show_today = true;
			show_today_circle = true;
			show_week_numbers = false;
			title_back_color = ThemeEngine.Current.ColorActiveCaption;
			title_fore_color = ThemeEngine.Current.ColorActiveCaptionText;
			today_date_set = false;
			trailing_fore_color = SystemColors.GrayText;
			bold_font = new Font (Font, Font.Style | FontStyle.Bold);
			centered_format = new StringFormat (StringFormat.GenericTypographic);
			centered_format.FormatFlags = centered_format.FormatFlags | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
			centered_format.FormatFlags &= ~StringFormatFlags.NoClip;
			centered_format.LineAlignment = StringAlignment.Center;
			centered_format.Alignment = StringAlignment.Center;
			
			// Set default values
			ForeColor = SystemColors.WindowText;
			BackColor = ThemeEngine.Current.ColorWindow;
		
			// intiailise internal variables used
			button_x_offset = 5;
			button_size = new Size (22, 17);
			// default settings based on 8.25 pt San Serif Font
			// Not sure of algorithm used to establish this
			date_cell_size = new Size (24, 16);		// default size at san-serif 8.25
			divider_line_offset = 4;
			calendar_spacing = new Size (4, 5);		// horiz and vert spacing between months in a calendar grid

			// set some state info
			clicked_date = now;
			is_date_clicked = false;
			is_previous_clicked = false;
			is_next_clicked = false;
			is_shift_pressed = false;
			click_state = new bool [] {false, false, false};
			first_select_start_date = now;
			month_title_click_location = Point.Empty;

			// set up context menus
			SetUpTodayMenu ();
			SetUpMonthMenu ();
			
			// event handlers
			timer.Tick += new EventHandler (TimerHandler);
			MouseMove += new MouseEventHandler (MouseMoveHandler);
			MouseDown += new MouseEventHandler (MouseDownHandler);
			KeyDown += new KeyEventHandler (KeyDownHandler);
			MouseUp += new MouseEventHandler (MouseUpHandler);
			KeyUp += new KeyEventHandler (KeyUpHandler);
			
			// this replaces paint so call the control version
			base.Paint += new PaintEventHandler (PaintHandler);
			
			Size = DefaultSize;
		}
		
		// called when this control is added to date time picker
		internal MonthCalendar (DateTimePicker owner) : this () {
			this.owner = owner;
			this.is_visible = false;
			this.Size = this.DefaultSize;
			if (owner != null)
				SetTopLevel (true);
		}

		#endregion	// Public Constructors

		#region Public Instance Properties

		// dates to make bold on calendar annually (recurring)
		[Localizable (true)]
		public DateTime [] AnnuallyBoldedDates {
			set {
				if (annually_bolded_dates == null)
					annually_bolded_dates = new ArrayList (value);
				else {
					annually_bolded_dates.Clear ();
					annually_bolded_dates.AddRange (value);
				}

				UpdateBoldedDates ();
			}
			get {
				if (annually_bolded_dates == null || annually_bolded_dates.Count == 0) {
					return new DateTime [0];
				}
				DateTime [] result = new DateTime [annually_bolded_dates.Count];
				annually_bolded_dates.CopyTo (result);
				return result;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
			}
		}

		// the back color for the main part of the calendar
		public override Color BackColor {
			set {
				base.BackColor = value;
			}
			get {
				return base.BackColor;
			}
		}

		// specific dates to make bold on calendar (non-recurring)
		[Localizable (true)]
		public DateTime[] BoldedDates {
			set {
				if (bolded_dates == null) {
					bolded_dates = new ArrayList (value);
				} else {
					bolded_dates.Clear ();
					bolded_dates.AddRange (value);
				}

				UpdateBoldedDates ();
			}
			get {
				if (bolded_dates == null || bolded_dates.Count == 0) 
					return new DateTime [0];
				
				DateTime [] result = new DateTime [bolded_dates.Count];
				bolded_dates.CopyTo (result);
				return result;
			}
		}

		// the configuration of the monthly grid display - only allowed to display at most,
		// 1 calendar year at a time, will be trimmed to fit it properly
		[Localizable (true)]
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
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get {
				return base.DoubleBuffered;
			}
			set {
				base.DoubleBuffered = value;
			}
		}

		// the first day of the week to display
		[Localizable (true)]
		[DefaultValue (Day.Default)]
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
		public override Color ForeColor {
			set {
				base.ForeColor = value;
			}
			get {
				return base.ForeColor;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		// the maximum date allowed to be selected on this month calendar
		public DateTime MaxDate {
			set {
				if (value < MinDate) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"Value of '{0}' is not valid for 'MaxDate'. 'MaxDate' " +
						"must be greater than or equal to MinDate.",
						value.ToString ("d", CultureInfo.CurrentCulture));
					throw new ArgumentOutOfRangeException ("MaxDate",
						msg);
				}

				if (max_date == value)
					return;
					
				max_date = value;

				if (max_date < selection_range.Start || max_date < selection_range.End) {
					DateTime start = max_date < selection_range.Start ? max_date : selection_range.Start;
					DateTime end = max_date < selection_range.End ? max_date : selection_range.End;
					SelectionRange = new SelectionRange (start, end);
				}
			}
			get {
				return max_date;
			}
		}

		// the maximum number of selectable days
		[DefaultValue (7)]
		public int MaxSelectionCount {
			set {
				if (value < 1) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"Value of '{0}' is not valid for 'MaxSelectionCount'. " +
						"'MaxSelectionCount' must be greater than or equal to {1}.",
						value, 1);
					throw new ArgumentOutOfRangeException ("MaxSelectionCount",
						msg);
				}
		
				// can't set selectioncount less than already selected dates
				if ((SelectionEnd - SelectionStart).Days > value) {
					throw new ArgumentException();
				}
			
				if (max_selection_count != value) {
					max_selection_count = value;
					this.OnUIAMaxSelectionCountChanged ();
				}
			}
			get {
				return max_selection_count;
			}
		}

		// the minimum date allowed to be selected on this month calendar
		public DateTime MinDate {
			set {
				DateTime absoluteMinDate = new DateTime (1753, 1, 1);

				if (value < absoluteMinDate) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"Value of '{0}' is not valid for 'MinDate'. 'MinDate' " +
						"must be greater than or equal to {1}.",
						value.ToString ("d", CultureInfo.CurrentCulture),
						absoluteMinDate.ToString ("d", CultureInfo.CurrentCulture));
					throw new ArgumentOutOfRangeException ("MinDate",
						msg);
				}

				if (value > MaxDate) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"Value of '{0}' is not valid for 'MinDate'. 'MinDate' " +
						"must be less than MaxDate.",
						value.ToString ("d", CultureInfo.CurrentCulture));
					throw new ArgumentOutOfRangeException ("MinDate",
						msg);
				}

				if (min_date == value)
					return;

				min_date = value;

				if (min_date > selection_range.Start || min_date > selection_range.End) {
					DateTime start = min_date > selection_range.Start ? min_date : selection_range.Start;
					DateTime end = min_date > selection_range.End ? min_date : selection_range.End;
					SelectionRange = new SelectionRange (start, end);
				}
			}
			get {
				return min_date;
			}
		}

		// dates to make bold on calendar monthly (recurring)
		[Localizable (true)]
		public DateTime[] MonthlyBoldedDates {
			set {
				if (monthly_bolded_dates == null) {
					monthly_bolded_dates = new ArrayList (value);
				} else {
					monthly_bolded_dates.Clear ();
					monthly_bolded_dates.AddRange (value);
				}

				UpdateBoldedDates ();
			}
			get {
				if (monthly_bolded_dates == null || monthly_bolded_dates.Count == 0) 
					return new DateTime [0];
				
				DateTime [] result = new DateTime [monthly_bolded_dates.Count];
				monthly_bolded_dates.CopyTo (result);
				return result;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		// Padding should not have any effect on the appearance of MonthCalendar.
		public new Padding Padding {
			get {
				return base.Padding;
			}
			set {
				base.Padding = value;
			}
		}
		
		[DefaultValue (false)]
		[Localizable (true)]
		public virtual bool RightToLeftLayout {
			get {
				return right_to_left_layout;
			}
			set {
				right_to_left_layout = value;
			}
		}

		// the ammount by which to scroll this calendar by
		[DefaultValue (0)]
		public int ScrollChange {
			set {
				if (value < 0 || value > 20000) {
					throw new ArgumentException();
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
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionEnd {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (SelectionRange.End != value) {
					DateTime old_end = SelectionRange.End; 
					// make sure the end obeys the max selection range count
					if (value < SelectionRange.Start) {
						SelectionRange.Start = value;
					}
					if (value.AddDays((MaxSelectionCount-1)*-1) > SelectionRange.Start) {
						SelectionRange.Start = value.AddDays((MaxSelectionCount-1)*-1);
					}
					SelectionRange.End = value;
					this.InvalidateDateRange (new SelectionRange (old_end, SelectionRange.End));
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.OnUIASelectionChanged ();
				}
			}
			get {
				return SelectionRange.End;
			}
		}

		[Bindable(true)]
		// the range of selected dates
		public SelectionRange SelectionRange {
			set {
				if (selection_range != value) {
					if (value.Start < MinDate)
						throw new ArgumentException ("SelectionStart cannot be less than MinDate");
					else if (value.End > MaxDate)
						throw new ArgumentException ("SelectionEnd cannot be greated than MaxDate");
					
					SelectionRange old_range = selection_range;

					// make sure the end obeys the max selection range count
					if (value.End.AddDays((MaxSelectionCount-1)*-1) > value.Start) {
						selection_range = new SelectionRange (value.End.AddDays((MaxSelectionCount-1)*-1), value.End);
					} else {
						selection_range = value;
					}
					SelectionRange visible_range = this.GetDisplayRange(true);
					if(visible_range.Start > selection_range.End) {
						this.current_month = new DateTime (selection_range.Start.Year, selection_range.Start.Month, 1);
						this.Invalidate ();
					} else if (visible_range.End < selection_range.Start) {
						int year_diff = selection_range.End.Year - visible_range.End.Year;
						int month_diff = selection_range.End.Month - visible_range.End.Month;
						this.current_month = current_month.AddMonths(year_diff * 12 + month_diff);
						this.Invalidate ();
					}
					// invalidate the selected range changes
					DateTime diff_start = old_range.Start;
					DateTime diff_end = old_range.End;
					// now decide which region is greated
					if (old_range.Start > SelectionRange.Start) {
						diff_start = SelectionRange.Start;
					} else if (old_range.Start == SelectionRange.Start) {
						if (old_range.End < SelectionRange.End) {
							diff_start = old_range.End;
						} else {
							diff_start = SelectionRange.End;
						}
					}
					if (old_range.End < SelectionRange.End) {
						diff_end = SelectionRange.End;
					} else if (old_range.End == SelectionRange.End) {
						if (old_range.Start < SelectionRange.Start) {
							diff_end = SelectionRange.Start;
						} else {
							diff_end = old_range.Start;
						}
					}


					// invalidate the region required	
					SelectionRange new_range = new SelectionRange (diff_start, diff_end);
					if (new_range.End != old_range.End || new_range.Start != old_range.Start)
						this.InvalidateDateRange (new_range);
					// raise date changed event
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.OnUIASelectionChanged ();
				}
			}
			get {
				return selection_range;
			}
		}

		// the first selected date
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionStart {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (SelectionRange.Start != value) {
					// make sure the end obeys the max selection range count
					if (value > SelectionRange.End) {
						SelectionRange.End = value;
					} else if (value.AddDays(MaxSelectionCount-1) < SelectionRange.End) {
						SelectionRange.End = value.AddDays(MaxSelectionCount-1);
					}
					SelectionRange.Start = value;
					DateTime new_month = new DateTime(value.Year, value.Month, 1);
					if (current_month != new_month)
						current_month = new_month;
					
					this.Invalidate ();
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					this.OnUIASelectionChanged ();
				}
			}
			get {
				return selection_range.Start;
			}
		}

		// whether or not to show todays date
		[DefaultValue (true)]
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
		[DefaultValue (true)]
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
		[Localizable (true)]
		[DefaultValue (false)]
		public bool ShowWeekNumbers {
			set {
				if (show_week_numbers != value) {
					show_week_numbers = value;
					// The values here don't matter, SetBoundsCore will calculate its own
					SetBoundsCore (Left, Top, Width, Height, BoundsSpecified.Width);
					this.Invalidate ();
				}
			}
			get {
				return show_week_numbers;
			}
		}

		// the rectangle size required to render one month based on current font
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
				date_cell_size = new Size ((int) Math.Ceiling (1.8 * multiplier), multiplier);
				title_size = new Size ((date_cell_size.Width * column_count), 2 * multiplier);

				return new Size (column_count * date_cell_size.Width, row_count * date_cell_size.Height + title_size.Height);
			}
		}

		[Localizable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size Size {
			get {
				return base.Size;
			}
			set {
				base.Size = value;
			}
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
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
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
					SelectionRange bounds = this.GetDisplayRange (false);
					SelectionRange visible_bounds = this.GetDisplayRange (true);
					this.InvalidateDateRange (new SelectionRange (bounds.Start, visible_bounds.Start));
					this.InvalidateDateRange (new SelectionRange (bounds.End, visible_bounds.End));
				}
			}
			get {
				return trailing_fore_color;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties

		// overloaded to allow controll to be windowed for drop down
		protected override CreateParams CreateParams {
			get {
				if (this.owner == null) {
					return base.CreateParams;
				} else {
					CreateParams cp = base.CreateParams;
					cp.Style ^= (int) WindowStyles.WS_CHILD;
					cp.Style |= (int) WindowStyles.WS_POPUP;
					cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

					return cp;
				}
			}
		}
	
		// not sure what to put in here - just doing a base() call - jba
		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}
		
		protected override Padding DefaultMargin {
			get {
				return new Padding (9);
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
			if (annually_bolded_dates == null)
				annually_bolded_dates = new ArrayList ();
			if (!annually_bolded_dates.Contains (date))
				annually_bolded_dates.Add (date);
		}

		// add a date to the normal bolded date arraylist
		public void AddBoldedDate (DateTime date) {
			if (bolded_dates == null)
				bolded_dates = new ArrayList ();
			if (!bolded_dates.Contains (date))
				bolded_dates.Add (date);
		}

		// add a date to the anually monthly date arraylist
		public void AddMonthlyBoldedDate (DateTime date) {
			if (monthly_bolded_dates == null)
				monthly_bolded_dates = new ArrayList ();
			if (!monthly_bolded_dates.Contains (date))
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
			return HitTest (point, out last_clicked_calendar_index, out last_clicked_calendar_rect);
		}

		// clears all the annually bolded dates
		public void RemoveAllAnnuallyBoldedDates () {
			if (annually_bolded_dates != null)
				annually_bolded_dates.Clear ();
		}

		// clears all the normal bolded dates
		public void RemoveAllBoldedDates () {
			if (bolded_dates != null)
				bolded_dates.Clear ();
		}

		// clears all the monthly bolded dates
		public void RemoveAllMonthlyBoldedDates () {
			if (monthly_bolded_dates != null)
				monthly_bolded_dates.Clear ();
		}

		// clears the specified annually bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveAnnuallyBoldedDate (DateTime date) {
			if (annually_bolded_dates == null)
				return;
				
			for (int i = 0; i < annually_bolded_dates.Count; i++) {
				DateTime dt = (DateTime) annually_bolded_dates [i];
				if (dt.Day == date.Day && dt.Month == date.Month) {
					annually_bolded_dates.RemoveAt (i);
					return;
				}
			}
		}

		// clears all the normal bolded date
		// only removes the first instance of the match
		public void RemoveBoldedDate (DateTime date) {
			if (bolded_dates == null)
				return;

			for (int i = 0; i < bolded_dates.Count; i++) {
				DateTime dt = (DateTime) bolded_dates [i];
				if (dt.Year == date.Year && dt.Month == date.Month && dt.Day == date.Day) {
					bolded_dates.RemoveAt (i);
					return;
				}
			}
		}

		// clears the specified monthly bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveMonthlyBoldedDate (DateTime date) {
			if (monthly_bolded_dates == null)
				return;

			for (int i = 0; i < monthly_bolded_dates.Count; i++) {
				DateTime dt = (DateTime) monthly_bolded_dates [i];
				if (dt.Day == date.Day && dt.Month == date.Month) {
					monthly_bolded_dates.RemoveAt (i);
					return;
				}
			}
		}

		// sets the calendar_dimensions. If product is > 12, the larger dimension is reduced to make product < 12
		public void SetCalendarDimensions(int x, int y) {
			this.CalendarDimensions = new Size(x, y);
		}

		// sets the currently selected date as date
		public void SetDate (DateTime date) {
			this.SetSelectionRange (date.Date, date.Date);
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
			Invalidate ();
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

		// Handle arrow keys
		protected override bool IsInputKey (Keys keyData) {
			switch (keyData & ~Keys.Shift) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Left:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
					return true;
				default:
					break;
			}

			return base.IsInputKey (keyData);
		}

		// not sure why this needs to be overriden
		protected override void OnBackColorChanged (EventArgs e) {
			base.OnBackColorChanged (e);
			this.Invalidate ();
		}

		// raises the date changed event
		protected virtual void OnDateChanged (DateRangeEventArgs drevent) {
			DateRangeEventHandler eh = (DateRangeEventHandler) (Events [DateChangedEvent]);
			if (eh != null)
				eh (this, drevent);
		}

		// raises the DateSelected event
		protected virtual void OnDateSelected (DateRangeEventArgs drevent) {
			DateRangeEventHandler eh = (DateRangeEventHandler) (Events [DateSelectedEvent]);
			if (eh != null)
				eh (this, drevent);
		}

		protected override void OnFontChanged (EventArgs e) {
			// Update size based on new font's space requirements
			Size = new Size (CalendarDimensions.Width * SingleMonthSize.Width,
					CalendarDimensions.Height * SingleMonthSize.Height);
			bold_font = new Font (Font, Font.Style | FontStyle.Bold);
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e) {
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e) {
			EventHandler eh = (EventHandler) (Events [RightToLeftLayoutChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		// i think this is overriden to not allow the control to be changed to an arbitrary size
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) 
		{
			// only allow sizes = default size to be set
			Size default_size = DefaultSize;
			Size min_size = default_size;
			Size max_size = new Size (default_size.Width + SingleMonthSize.Width + calendar_spacing.Width,
					default_size.Height + SingleMonthSize.Height + calendar_spacing.Height);
			int x_mid_point = (max_size.Width + min_size.Width)/2;
			int y_mid_point = (max_size.Height + min_size.Height)/2;

			if (width < x_mid_point) {
				width = min_size.Width;
			} else {
				width = max_size.Width;
			}
			if (height < y_mid_point) {
				height = min_size.Height;
			} else {
				height = max_size.Height;
			}
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}

		#endregion	// Protected Instance Methods

		#region public events
		static object DateChangedEvent = new object ();
		static object DateSelectedEvent = new object ();
		static object RightToLeftLayoutChangedEvent = new object ();

		// fired when the date is changed (either explicitely or implicitely)
		// when navigating the month selector
		public event DateRangeEventHandler DateChanged {
			add { Events.AddHandler (DateChangedEvent, value); }
			remove { Events.RemoveHandler (DateChangedEvent, value); }
		}

		// fired when the user explicitely clicks on date to select it
		public event DateRangeEventHandler DateSelected {
			add { Events.AddHandler (DateSelectedEvent, value); }
			remove { Events.RemoveHandler (DateSelectedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value;}
			remove { base.BackgroundImageLayoutChanged += value;}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add {base.Click += value; }
			remove {base.Click -= value;}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add {base.DoubleClick += value; }
			remove {base.DoubleClick -= value; }
		}
		
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value;}
			remove { base.MouseClick -= value;}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add {base.PaddingChanged += value;}
			remove {base.PaddingChanged -= value;}
		}

		// XXX check this out
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;

		public event EventHandler RightToLeftLayoutChanged {
			add {Events.AddHandler (RightToLeftLayoutChangedEvent, value);}
			remove {Events.RemoveHandler (RightToLeftLayoutChangedEvent, value);}
		}

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion	// public events

		#region internal properties

		private void AddYears (int years, bool fast)
		{
			DateTime newDate;
			if (fast) {
				if (!(CurrentMonth.Year + years * 5 > MaxDate.Year)) {
					newDate = CurrentMonth.AddYears (years * 5);
					if (MaxDate >= newDate && MinDate <= newDate) {
						CurrentMonth = newDate;
						return;
					}
				}
			}
			if (!(CurrentMonth.Year + years > MaxDate.Year)) {
				newDate = CurrentMonth.AddYears (years);
				if (MaxDate >= newDate && MinDate <= newDate) {
					CurrentMonth = newDate;
				}
			}
		}
		
		internal bool IsYearGoingUp {
			get {
				return is_year_going_up;
			}
			set {
				if (value) {
					is_year_going_down = false;
					year_moving_count = (is_year_going_up ? year_moving_count + 1 : 1);
					if (is_year_going_up)
						year_moving_count++;
					else {
						year_moving_count = 1;
					}
					AddYears (1, year_moving_count > 10);
					if (is_mouse_moving_year)
						StartHideTimer ();
				} else {
					year_moving_count = 0;
				}
				is_year_going_up = value;
				Invalidate ();
			}
		}
		
		internal bool IsYearGoingDown {
			get {
				return is_year_going_down;
			}
			set
			{
				if (value) {
					is_year_going_up = false;
					year_moving_count = (is_year_going_down ? year_moving_count + 1 : 1);
					if (is_year_going_down)
						year_moving_count++;
					else {
						year_moving_count = 1;
					}
					AddYears (-1, year_moving_count > 10);
					if (is_mouse_moving_year)
						StartHideTimer ();
				} else {
					year_moving_count = 0;
				}
				is_year_going_down = value;
				Invalidate ();
			}
		}

		internal bool ShowYearUpDown {
			get {
				return show_year_updown;
			} 
			set {
				if (show_year_updown != value) {
					show_year_updown = value;
					Invalidate ();
				}
			}
		}

		internal DateTime CurrentMonth {
			set {
				// only interested in if the month (not actual date) has change
				if (value < new DateTime(MinDate.Year, MinDate.Month, 1) || value > MaxDate) {
					return;
				}
				
				if (value.Month != current_month.Month ||
					value.Year != current_month.Year) {
					DateTime start = this.SelectionStart.Add(value.Subtract(current_month));
					if (start < MinDate)
						start = MinDate;
					else if (start > MaxDate)
						start = MaxDate;
					DateTime end = this.SelectionEnd.Add (value.Subtract (current_month));
					if (end < MinDate)
						end = MinDate;
					else if (end > MaxDate)
						end = MaxDate;
					this.SelectionRange = new SelectionRange (start, end);
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
		internal HitTestInfo HitTest (
			Point point,
			out int calendar_index,
			out Rectangle calendar_rect) {
			// start by initialising the ref parameters
			calendar_index = -1;
			calendar_rect = Rectangle.Empty;

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
							if (button_rect.Contains (point)) {
								return new HitTestInfo (HitArea.PrevMonthButton, point, new DateTime (1, 1, 1));
							}
						}
						// make sure it's not the next button
						if (i % CalendarDimensions.Height == 0 && i % CalendarDimensions.Width == calendar_dimensions.Width - 1) {
							Rectangle button_rect = new Rectangle(
								new Point (calendars[i].Right - button_x_offset - button_size.Width, (title_size.Height - button_size.Height)/2),
								button_size);
							if (button_rect.Contains (point)) {
								return new HitTestInfo (HitArea.NextMonthButton, point, new DateTime (1, 1, 1));
							}
						}

						// indicate which calendar and month it was
						calendar_index = i;
						calendar_rect = calendars[i];

						// make sure it's not the month or the year of the calendar
						if (GetMonthNameRectangle (title_rect, i).Contains (point)) {
							return new HitTestInfo (HitArea.TitleMonth, point, new DateTime (1, 1, 1));
						}
						Rectangle year, up, down;
						GetYearNameRectangles (title_rect, i, out year, out up, out down);
						if (year.Contains (point)) {
							return new HitTestInfo (HitArea.TitleYear, point, new DateTime (1, 1, 1), HitAreaExtra.YearRectangle);
						} else if (up.Contains (point)) {
							return new HitTestInfo (HitArea.TitleYear, point, new DateTime (1, 1, 1), HitAreaExtra.UpButton);
						} else if (down.Contains (point)) {
							return new HitTestInfo (HitArea.TitleYear, point, new DateTime (1, 1, 1), HitAreaExtra.DownButton);
						}

						// return the hit test in the title background
						return new HitTestInfo (HitArea.TitleBackground, point, new DateTime (1, 1, 1));
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
						return new HitTestInfo (HitArea.DayOfWeek, point, new DateTime (1, 1, 1));
					}
						
					// finally see if it was a date that was clicked
					Rectangle date_grid = new Rectangle (
						new Point (day_rect.X, day_rect.Bottom),
						new Size (day_rect.Width, Math.Max(calendars[i].Bottom - day_rect.Bottom, 0)));
					if (date_grid.Contains (point)) {
						clicked_rect = date_grid;
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
								return new HitTestInfo (HitArea.PrevMonthDate, point, new DateTime (1, 1, 1), time);
							} else if (time > calendar_month && i == CalendarDimensions.Width*CalendarDimensions.Height - 1) {
								return new HitTestInfo (HitArea.NextMonthDate, point, new DateTime (1, 1, 1), time);
							}
							return new HitTestInfo (HitArea.Nowhere, point, new DateTime (1, 1, 1));
						}
						return new HitTestInfo(HitArea.Date, point, time);
					}
				}
			}

			return new HitTestInfo ();
		}

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
		
		internal bool IsBoldedDate (DateTime date) {
			// check bolded dates
			if (bolded_dates != null && bolded_dates.Count > 0) {
				foreach (DateTime bolded_date in bolded_dates) {
					if (bolded_date.Date == date.Date) {
						return true;
					}
				}
			}
			// check monthly dates
			if (monthly_bolded_dates != null && monthly_bolded_dates.Count > 0) {
				foreach (DateTime bolded_date in monthly_bolded_dates) {
					if (bolded_date.Day == date.Day) {
						return true;
					}
				}
			}
			// check yearly dates
			if (annually_bolded_dates != null && annually_bolded_dates.Count > 0) {
				foreach (DateTime bolded_date in annually_bolded_dates) {
					if (bolded_date.Month == date.Month && bolded_date.Day == date.Day) {
						return true;
					}
				}
			}
			
			return false;  // no match
		}
		
		// initialise the 'go to today' context menu
		private void SetUpTodayMenu () {
			today_menu = new ContextMenu ();
			MenuItem menu_item = new MenuItem ("Go to today");
			menu_item.Click += new EventHandler (TodayMenuItemClickHandler);
			today_menu.MenuItems.Add (menu_item);
		}

		// initialise the month context menu
		private void SetUpMonthMenu () {
			month_menu = new ContextMenu ();
			for (int i=0; i < 12; i++) {
				MenuItem menu_item = new MenuItem ( new DateTime (2000, i+1, 1).ToString ("MMMM"));
				menu_item.Click += new EventHandler (MonthMenuItemClickHandler);
				month_menu.MenuItems.Add (menu_item);
			}
		}

		// returns the first date of the month
		private DateTime GetFirstDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1);
		}

		// returns the last date of the month
		private DateTime GetLastDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
		}

		// called in response to users seletion with shift key
		private void AddTimeToSelection (int delta, bool isDays)
		{
			DateTime cursor_point;
			DateTime end_point;
			// okay we add the period to the date that is not the same as the 
			// start date when shift was first clicked.
			if (SelectionStart != first_select_start_date) {
				cursor_point = SelectionStart;
			} else {
				cursor_point = SelectionEnd;
			}
			// add the days
			if (isDays) {
				end_point = cursor_point.AddDays (delta);
			} else {
				// delta must be months
				end_point = cursor_point.AddMonths (delta);
			}
			// set the new selection range
			SelectionRange range = new SelectionRange (first_select_start_date, end_point);
			if (range.Start.AddDays (MaxSelectionCount-1) < range.End) {
				// okay the date is beyond what is allowed, lets set the maximum we can
				if (range.Start != first_select_start_date) {
					range.Start = range.End.AddDays ((MaxSelectionCount-1)*-1);
				} else {
					range.End = range.Start.AddDays (MaxSelectionCount-1);
				}
			}

			// validate range
			if (range.Start < MinDate)
				range.Start = MinDate;
			if (range.End > MaxDate)
				range.End = MaxDate;

			// Avoid re-setting SelectionRange to the same value and fire an extra DateChanged event
			if (range.Start != selection_range.Start || range.End != selection_range.End)
				SelectionRange = range;
		}

		// attempts to add the date to the selection without throwing exception
		private void SelectDate (DateTime date) {
			if (date < MinDate || date > MaxDate)
				return;
			// try and add the new date to the selction range
			SelectionRange range = null;
			if (is_shift_pressed || (click_state [0])) {
				range = new SelectionRange (first_select_start_date, date);
				if (range.Start.AddDays (MaxSelectionCount-1) < range.End) {
					// okay the date is beyond what is allowed, lets set the maximum we can
					if (range.Start != first_select_start_date) {
						range.Start = range.End.AddDays ((MaxSelectionCount-1)*-1);
					} else {
						range.End = range.Start.AddDays (MaxSelectionCount-1);
					}
				}
			} else {
				range = new SelectionRange (date, date);
				first_select_start_date = date;
			}
				
			// Only set if we re actually getting a different range (avoid an extra DateChanged event)
			if (range.Start != selection_range.Start || range.End != selection_range.End)
				SelectionRange = range;
		}

		// gets the week of the year
		internal int GetWeekOfYear (DateTime date) {
			// convert the first_day_of_week into a DayOfWeekEnum
			DayOfWeek first_day = GetDayOfWeek (first_day_of_week);
			// find the first day of the year
			DayOfWeek first_day_of_year = new DateTime (date.Year, 1, 1).DayOfWeek;
			// adjust for the starting day of the week
			int offset = first_day_of_year - first_day;
			int week = ((date.DayOfYear + offset) / 7) + 1;
			return week;
		}

		// convert a Day enum into a DayOfWeek enum
		internal DayOfWeek GetDayOfWeek (Day day) {
			if (day == Day.Default) {
				return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
			} else {
				return (DayOfWeek) DayOfWeek.Parse (typeof (DayOfWeek), day.ToString ());
			}
		}

		// returns the rectangle for themonth name
		internal Rectangle GetMonthNameRectangle (Rectangle title_rect, int calendar_index) {
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			Size title_text_size = TextRenderer.MeasureString (this_month.ToString ("MMMM yyyy"), this.Font).ToSize ();
			Size month_size = TextRenderer.MeasureString (this_month.ToString ("MMMM"), this.Font).ToSize ();
			// return only the month name part of that
			return new Rectangle (
				new Point (
					title_rect.X + ((title_rect.Width - title_text_size.Width)/2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height)/2)),
				month_size);
		}

		internal void GetYearNameRectangles (Rectangle title_rect, int calendar_index, out Rectangle year_rect, out Rectangle up_rect, out Rectangle down_rect)
		{
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			SizeF title_text_size = TextRenderer.MeasureString (this_month.ToString ("MMMM yyyy"), this.bold_font, int.MaxValue, centered_format);
			SizeF year_size = TextRenderer.MeasureString (this_month.ToString ("yyyy"), this.bold_font, int.MaxValue, centered_format);
			// find out how much space the title took
			RectangleF text_rect = new RectangleF (
				new PointF (
					title_rect.X + ((title_rect.Width - title_text_size.Width) / 2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height) / 2)),
				title_text_size);
			// return only the rect of the year
			year_rect = new Rectangle (
				new Point (
					((int)(text_rect.Right - year_size.Width + 1)),
					(int)text_rect.Y),
				new Size ((int)(year_size.Width + 1), (int)(year_size.Height + 1)));
			
			year_rect.Inflate (0, 1);
			up_rect = new Rectangle ();
			up_rect.Location = new Point (year_rect.X + year_rect.Width + 2, year_rect.Y);
			up_rect.Size = new Size (16, year_rect.Height / 2);
			down_rect = new Rectangle ();
			down_rect.Location = new Point (up_rect.X, up_rect.Y + up_rect.Height + 1);
			down_rect.Size = up_rect.Size;
		}

		// returns the rectangle for the year in the title
		internal Rectangle GetYearNameRectangle (Rectangle title_rect, int calendar_index) {
			Rectangle result, discard;
			GetYearNameRectangles (title_rect, calendar_index, out result, out discard, out discard);
			return result;		
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
					this.clicked_date = hti.hit_time;
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

		// called when today context menu is clicked
		private void TodayMenuItemClickHandler (object sender, EventArgs e)
		{
			DateTime date = DateTime.Now.Date;
			if (date < MinDate || date > MaxDate)
				return;
			this.SetSelectionRange (date, date);
			this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
		}

		// called when month context menu is clicked
		private void MonthMenuItemClickHandler (object sender, EventArgs e) {
			MenuItem item = sender as MenuItem;
			if (item != null && month_title_click_location != Point.Empty) {
				// establish which month we want to move to
				if (item.Parent == null) {
					return;
				}
				int new_month = item.Parent.MenuItems.IndexOf (item) + 1;
				if (new_month == 0) {
					return;
				}
				// okay let's establish which calendar was hit
				Size month_size = this.SingleMonthSize;
				for (int i=0; i < CalendarDimensions.Height; i++) {
					for (int j=0; j < CalendarDimensions.Width; j++) {
						int month_index = (i * CalendarDimensions.Width) + j;
						Rectangle month_rect = new Rectangle ( new Point (0, 0), month_size);
						if (j == 0) {
							month_rect.X = this.ClientRectangle.X + 1;
						} else {
							month_rect.X = this.ClientRectangle.X + 1 + ((j)*(month_size.Width+calendar_spacing.Width));
						}
						if (i == 0) {
							month_rect.Y = this.ClientRectangle.Y + 1;
						} else {
							month_rect.Y = this.ClientRectangle.Y + 1 + ((i)*(month_size.Height+calendar_spacing.Height));
						}
						// see if the point is inside
						if (month_rect.Contains (month_title_click_location)) {
							DateTime clicked_month = CurrentMonth.AddMonths (month_index);
							// get the month that we want to move to
							int month_offset = new_month - clicked_month.Month;
							
							// move forward however more months we need to
							this.CurrentMonth = this.CurrentMonth.AddMonths (month_offset);
							break;
						}
					}
				}

				// clear the point
				month_title_click_location = Point.Empty;
			}
		}
		
		// raised on the timer, for mouse hold clicks
		private void TimerHandler (object sender, EventArgs e) {
			// now find out which area was click
			if (this.Capture) {
				HitTestInfo hti = this.HitTest (this.PointToClient (MousePosition));
				// see if it was clicked on the prev or next mouse 
				if (click_state [1] || click_state [2]) {
					// invalidate the area where the mouse was last held
					DoMouseUp ();
					// register the click
					if (hti.HitArea == HitArea.PrevMonthButton ||
						hti.HitArea == HitArea.NextMonthButton) {
						DoButtonMouseDown (hti);
						click_state [1] = (hti.HitArea == HitArea.PrevMonthButton);
						click_state [2] = !click_state [1];
					}
					if (timer.Interval != 300) {
						timer.Interval = 300;
					}
				}
			} else  {
				timer.Enabled = false;
			}
		}
		
		// selects one of the buttons
		private void DoButtonMouseDown (HitTestInfo hti) {
			// show the click then move on
			SetItemClick(hti);
			if (hti.HitArea == HitArea.PrevMonthButton) {
				// invalidate the prev monthbutton
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.X + 1 + button_x_offset,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
				int scroll = (scroll_change == 0 ? CalendarDimensions.Width * CalendarDimensions.Height : scroll_change);
				this.CurrentMonth = this.CurrentMonth.AddMonths (-scroll);
			} else {
				// invalidate the next monthbutton
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.Right - 1 - button_x_offset - button_size.Width,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
				int scroll = (scroll_change == 0 ? CalendarDimensions.Width * CalendarDimensions.Height : scroll_change);
				this.CurrentMonth = this.CurrentMonth.AddMonths (scroll);
			}
		}
		
		// selects the clicked date
		private void DoDateMouseDown (HitTestInfo hti) {
			SetItemClick(hti);
		}
		
		// event run on the mouse up event
		private void DoMouseUp () {

			IsYearGoingDown = false;
			IsYearGoingUp = false;
			is_mouse_moving_year = false;
			
			// invalidate the next monthbutton
			if (this.is_next_clicked) {
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.Right - 1 - button_x_offset - button_size.Width,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
			}					
			// invalidate the prev monthbutton
			if (this.is_previous_clicked) {
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.X + 1 + button_x_offset,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
			}
			if (this.is_date_clicked) {
				// invalidate the area under the cursor, to remove focus rect
				this.InvalidateDateRange (new SelectionRange (clicked_date, clicked_date));
			}
			this.is_previous_clicked = false;
			this.is_next_clicked = false;
			this.is_date_clicked = false;
		}

		// needed when in windowed mode to close the calendar if no 
		// part of it has focus.
		private void UpDownTimerTick(object sender, EventArgs e)
		{
			if (IsYearGoingUp) {
				IsYearGoingUp = true;
			} 
			if (IsYearGoingDown) {
				IsYearGoingDown = true;
			}
			
			if (!IsYearGoingDown && !IsYearGoingUp) {
				updown_timer.Enabled = false;
			} else if (IsYearGoingDown || IsYearGoingUp) {
				updown_timer.Interval = subsequent_delay;
			}
		}

		// Needed when in windowed mode.
		private void StartHideTimer ()
		{
			if (updown_timer == null) {
				updown_timer = new Timer ();
				updown_timer.Tick += new EventHandler (UpDownTimerTick);
			}
			updown_timer.Interval = initial_delay;
			updown_timer.Enabled = true;
		}

		// occurs when mouse moves around control, used for selection
		private void MouseMoveHandler (object sender, MouseEventArgs e) {
			HitTestInfo hti = this.HitTest (e.X, e.Y);
			// clear the last clicked item 
			if (click_state [0]) {
				// register the click
				if (hti.HitArea == HitArea.PrevMonthDate ||
					hti.HitArea == HitArea.NextMonthDate ||
					hti.HitArea == HitArea.Date)
				{
					Rectangle prev_rect = clicked_rect;
					DateTime prev_clicked = clicked_date;
					DoDateMouseDown (hti);
					if (owner == null) {
						click_state [0] = true;
					} else {
						click_state [0] = false;
						click_state [1] = false;
						click_state [2] = false;
					}

					if (prev_clicked != clicked_date) {
						// select date after updating click_state and clicked_date
						SelectDate (clicked_date);
						date_selected_event_pending = true;

						Rectangle invalid = Rectangle.Union (prev_rect, clicked_rect);
						Invalidate (invalid);
					}
				}
				
			}
		}
		
		// to check if the mouse has come down on this control
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == 0)
				return;

			// clear the click_state variables
			click_state [0] = false;
			click_state [1] = false;
			click_state [2] = false;

			// disable the timer if it was enabled 
			if (timer.Enabled) {
				timer.Stop ();
				timer.Enabled = false;
			}
			
			Point point = new Point (e.X, e.Y);
			// figure out if we are in drop down mode and a click happened outside us
			if (this.owner != null) {
				if (!this.ClientRectangle.Contains (point)) {
					this.owner.HideMonthCalendar ();
					return;
				}
			}

			//establish where was hit
			HitTestInfo hti = this.HitTest(point);
			// hide the year numeric up down if it was clicked
			if (ShowYearUpDown && hti.HitArea != HitArea.TitleYear) {
				ShowYearUpDown = false;
			}
			switch (hti.HitArea) {
				case HitArea.PrevMonthButton:
				case HitArea.NextMonthButton:
					DoButtonMouseDown (hti);
					click_state [1] = (hti.HitArea == HitArea.PrevMonthDate);
					click_state [2] = !click_state [1];
					timer.Interval = 750;
					timer.Start ();
					break;
				case HitArea.Date:
				case HitArea.PrevMonthDate:
				case HitArea.NextMonthDate:
					DoDateMouseDown (hti);

					// select date before updating click_state
					SelectDate (clicked_date);
					date_selected_event_pending = true;

					// leave clicked state blank if drop down window
					if (owner == null) {
						click_state [0] = true;
					} else {
						click_state [0] = false;
						click_state [1] = false;
						click_state [2] = false;
					}

					break;
				case HitArea.TitleMonth:
					month_title_click_location = hti.Point;
					month_menu.Show (this, hti.Point);
					if (this.Capture && owner != null) {
						Capture = false;
						Capture = true;
					}
					break;
				case HitArea.TitleYear:
					// place the numeric up down
					if (ShowYearUpDown) {
						if (hti.hit_area_extra == HitAreaExtra.UpButton) {
							is_mouse_moving_year = true;
							IsYearGoingUp = true;
						} else if (hti.hit_area_extra == HitAreaExtra.DownButton) {
							is_mouse_moving_year = true;
							IsYearGoingDown = true; 
						}
						return;
					} else {
						ShowYearUpDown = true;
					}
					break;
				case HitArea.TodayLink: {
					DateTime date = DateTime.Now.Date;
					if (date >= MinDate && date <= MaxDate) {
						this.SetSelectionRange (date, date);
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					}
					break;
				}
				default:
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					break;
			}
		}

		// raised by any key down events
		private void KeyDownHandler (object sender, KeyEventArgs e) {
			// send keys to the year_updown control, let it handle it
			if(ShowYearUpDown) {
				switch (e.KeyCode) {
					case Keys.Enter:
						ShowYearUpDown = false;
						IsYearGoingDown = false;
						IsYearGoingUp = false;
						break;
					case Keys.Up: {
						IsYearGoingUp = true;
						break;
					}
					case Keys.Down: {
						IsYearGoingDown = true;
						break;
					}
				}
			} else {
				if (!is_shift_pressed && e.Shift) {
					first_select_start_date = SelectionStart;
					is_shift_pressed = e.Shift;
					e.Handled = true;
				}
				switch (e.KeyCode) {
					case Keys.Home:
						// set the date to the start of the month
						if (is_shift_pressed) {
							DateTime date = GetFirstDateInMonth (first_select_start_date);
							if (date < first_select_start_date.AddDays ((MaxSelectionCount-1)*-1)) {
								date = first_select_start_date.AddDays ((MaxSelectionCount-1)*-1);
							}
							if (date < MinDate)
								date = MinDate;
							this.SetSelectionRange (date, first_select_start_date);
						} else {
							DateTime date = GetFirstDateInMonth (this.SelectionStart);
							if (date < MinDate)
								date = MinDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.End:
						// set the date to the last of the month
						if (is_shift_pressed) {
							DateTime date = GetLastDateInMonth (first_select_start_date);
							if (date > first_select_start_date.AddDays (MaxSelectionCount-1)) {
								date = first_select_start_date.AddDays (MaxSelectionCount-1);
							}
							if (date > MaxDate)
								date = MaxDate;
							this.SetSelectionRange (date, first_select_start_date);
						} else {
							DateTime date = GetLastDateInMonth (this.SelectionStart);
							if (date > MaxDate)
								date = MaxDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.PageUp:
						// set the date to the last of the month
						if (is_shift_pressed) {
							this.AddTimeToSelection (-1, false);
						} else {
							DateTime date = this.SelectionStart.AddMonths (-1);
							if (date < MinDate)
								date = MinDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.PageDown:
						// set the date to the last of the month
						if (is_shift_pressed) {
							this.AddTimeToSelection (1, false);
						} else {
							DateTime date = this.SelectionStart.AddMonths (1);
							if (date > MaxDate)
								date = MaxDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.Up:
						// set the back 1 week
						if (is_shift_pressed) {
							this.AddTimeToSelection (-7, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (-7);
							if (date < MinDate)
								date = MinDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.Down:
						// set the date forward 1 week
						if (is_shift_pressed) {
							this.AddTimeToSelection (7, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (7);
							if (date > MaxDate)
								date = MaxDate;
							this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.Left:
						// move one left
						if (is_shift_pressed) {
							this.AddTimeToSelection (-1, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (-1);
							if (date >= MinDate)
								this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.Right:
						// move one left
						if (is_shift_pressed) {
							this.AddTimeToSelection (1, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (1);
							if (date <= MaxDate)
								this.SetSelectionRange (date, date);
						}
						e.Handled = true;
						break;
					case Keys.F4:
						// Close ourselves on Alt-F4 if we are a popup
						if (e.Alt && owner != null) {
							this.Hide ();
							e.Handled = true;
						}
						break;
					default:
						break;
				}
			}
		}

		// to check if the mouse has come up on this control
		private void MouseUpHandler (object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == 0) {
				if (show_today && (this.ContextMenu == null))
					today_menu.Show (this, new Point (e.X, e.Y));
				return;
			}

			if (timer.Enabled) {
				timer.Stop ();
			}
			// clear the click state array
			click_state [0] = false;
			click_state [1] = false;
			click_state [2] = false;
			// do the regulare mouseup stuff
			this.DoMouseUp ();

			if (date_selected_event_pending) {
				OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
				date_selected_event_pending = false;
			}
		}

		// raised by any key up events
		private void KeyUpHandler (object sender, KeyEventArgs e) {
			is_shift_pressed = e.Shift ;
			e.Handled = true;
			IsYearGoingUp = false;
			IsYearGoingDown = false;
		}

		// paint this control now
		private void PaintHandler (object sender, PaintEventArgs pe) {
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pe.ClipRectangle, pe.Graphics);

			// fire the new paint handler
			if (this.Paint != null) 
			{
				this.Paint (sender, pe);
			}
		}

		// returns the region of the control that needs to be redrawn 
		private void InvalidateDateRange (SelectionRange range) {
			SelectionRange bounds = this.GetDisplayRange (false);

			if (range.End < bounds.Start || range.Start > bounds.End) {
				// don't invalidate anything, as the modified date range
				// is outside the visible bounds of this control
				return;
			}
			// adjust the start and end to be inside the visible range
			if (range.Start < bounds.Start) {
				range = new SelectionRange (bounds.Start, range.End);
			}
			if (range.End > bounds.End) {
				range = new SelectionRange (range.Start, bounds.End);
			}
			// now invalidate the date rectangles as series of rows
			DateTime last_month = this.current_month.AddMonths ((CalendarDimensions.Width * CalendarDimensions.Height)).AddDays (-1);
			DateTime current = range.Start;
			while (current <= range.End) {
				DateTime month_end = new DateTime (current.Year, current.Month, 1).AddMonths (1).AddDays (-1);;
				Rectangle start_rect;
				Rectangle end_rect;
				// see if entire selection is in this current month
				if (range.End <= month_end && current < last_month)	{
					// the end is the last date
					if (current < this.current_month) {
						start_rect = GetDateRowRect (current_month, current_month);
					} else {
						start_rect = GetDateRowRect (current, current);
					}
					end_rect = GetDateRowRect (current, range.End);
				} else if (current < last_month) {
					// otherwise it simply means we have a selection spaning
					// multiple months simply set rectangle inside the current month
					start_rect = GetDateRowRect (current, current);
					end_rect = GetDateRowRect (month_end, month_end);
				} else {
					// it's outside the visible range
					start_rect = GetDateRowRect (last_month, last_month.AddDays (1));
					end_rect = GetDateRowRect (last_month, range.End);
				}
				// push to the next month
				current = month_end.AddDays (1);
				// invalidate from the start row to the end row for this month				
				this.Invalidate (
					new Rectangle (
						start_rect.X,
						start_rect.Y,
						start_rect.Width,
						Math.Max (end_rect.Bottom - start_rect.Y, 0)));
				}
		} 
		
		// gets the rect of the row where the specified date appears on the specified month
		private Rectangle GetDateRowRect (DateTime month, DateTime date) {
			// first get the general rect of the supplied month
			Size month_size = SingleMonthSize;
			Rectangle month_rect = Rectangle.Empty;
			for (int i=0; i < CalendarDimensions.Width*CalendarDimensions.Height; i++) {
				DateTime this_month = this.current_month.AddMonths (i);
				if (month.Year == this_month.Year && month.Month == this_month.Month) {
					month_rect = new Rectangle (
						this.ClientRectangle.X + 1 + (month_size.Width * (i%CalendarDimensions.Width)) + (this.calendar_spacing.Width * (i%CalendarDimensions.Width)),
						this.ClientRectangle.Y + 1 + (month_size.Height * (i/CalendarDimensions.Width)) + (this.calendar_spacing.Height * (i/CalendarDimensions.Width)),
						month_size.Width,
						month_size.Height);
						break;
				}
			}
			// now find out where in the month the supplied date is
			if (month_rect == Rectangle.Empty) {
				return Rectangle.Empty;
			}
			// find out which row this date is in
			int row = -1;
			DateTime first_date = GetFirstDateInMonthGrid (month);
			DateTime end_date = first_date.AddDays (7); 
			for (int i=0; i < 6; i++) {
				if (date >= first_date && date < end_date) {
					row = i;
					break;
				}
				first_date = end_date;
				end_date = end_date.AddDays (7);
			}
			// ensure it's a valid row
			if (row < 0) {
				return Rectangle.Empty;
			}
			int x_offset = (this.ShowWeekNumbers) ? date_cell_size.Width : 0;
			int y_offset = title_size.Height + (date_cell_size.Height * (row + 1));
			return new Rectangle (
				month_rect.X + x_offset,
				month_rect.Y + y_offset,
				date_cell_size.Width * 7,
				date_cell_size.Height);
		}

		internal void Draw (Rectangle clip_rect, Graphics dc)
		{
			ThemeEngine.Current.DrawMonthCalendar (dc, clip_rect, this);
		}

		internal override bool InternalCapture {
			get {
				return base.InternalCapture;
			}
			set {
				// Don't allow internal capture when DateTimePicker is using us
				// Control sets this on MouseDown 
				if (owner == null)
					base.InternalCapture = value;
			}
		}

		#endregion 	//internal methods

		#region internal drawing methods


		#endregion	// internal drawing methods

		#region inner classes and enumerations

		// enumeration about what type of area on the calendar was hit 
		public enum HitArea {
			Nowhere,
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
		
		internal enum HitAreaExtra {
			YearRectangle,
			UpButton,
			DownButton
		}
		
		// info regarding to a hit test on this calendar
		public sealed class HitTestInfo {

			private HitArea hit_area;
			private Point point;
			private DateTime time;

			internal HitAreaExtra hit_area_extra;
			internal DateTime hit_time;
			
			// default constructor
			internal HitTestInfo () {
				hit_area = HitArea.Nowhere;
				point = new Point (0, 0);
				time = DateTime.Now;
			}

			// overload receives all properties
			internal HitTestInfo (HitArea hit_area, Point point, DateTime time) {
				this.hit_area = hit_area;
				this.point = point;
				this.time = time;
				this.hit_time = time;
			}
			
			// overload receives all properties
			internal HitTestInfo (HitArea hit_area, Point point, DateTime time, DateTime hit_time)
			{
				this.hit_area = hit_area;
				this.point = point;
				this.time = time;
				this.hit_time = hit_time;
			}
			
			internal HitTestInfo (HitArea hit_area, Point point, DateTime time, HitAreaExtra hit_area_extra)
			{
				this.hit_area = hit_area;
				this.hit_area_extra = hit_area_extra;
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

		#region UIA Framework: Methods, Properties and Events

		static object UIAMaxSelectionCountChangedEvent = new object ();
		static object UIASelectionChangedEvent = new object ();

		internal event EventHandler UIAMaxSelectionCountChanged {
			add { Events.AddHandler (UIAMaxSelectionCountChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMaxSelectionCountChangedEvent, value); }
		}

		internal event EventHandler UIASelectionChanged {
			add { Events.AddHandler (UIASelectionChangedEvent, value); }
			remove { Events.RemoveHandler (UIASelectionChangedEvent, value); }
		}

		private void OnUIAMaxSelectionCountChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAMaxSelectionCountChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void OnUIASelectionChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIASelectionChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		#endregion
	}
}
