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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//	Rolf Bjarne Kvinge	rolfkvinge@ya.com
//
// TODO:
//		- wire in all events from monthcalendar


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms {
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultBindingProperty ("Value")]
	[ComVisible (true)]
	[DefaultEvent("ValueChanged")]
	[DefaultProperty("Value")]
	[Designer("System.Windows.Forms.Design.DateTimePickerDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DateTimePicker : Control {

		#region Public variables
		
		// this class has to have the specified hour, minute and second, as it says in msdn
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public static readonly DateTime MaxDateTime = new DateTime (9998, 12, 31, 0, 0, 0);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public static readonly DateTime MinDateTime = new DateTime (1753, 1, 1);

		internal const int check_box_size = 13;
		internal const int check_box_space = 4;
		
		#endregion 	// Public variables
		
		#region Local variables
		
		protected static readonly Color DefaultMonthBackColor = ThemeEngine.Current.ColorWindow;
		protected static readonly Color DefaultTitleBackColor = ThemeEngine.Current.ColorActiveCaption;
		protected static readonly Color DefaultTitleForeColor = ThemeEngine.Current.ColorActiveCaptionText;
		protected static readonly Color DefaultTrailingForeColor = SystemColors.GrayText;
		
		internal MonthCalendar			month_calendar;
		bool							is_checked;
		string							custom_format;
		LeftRightAlignment				drop_down_align;
		DateTimePickerFormat			format;
		DateTime						max_date;
		DateTime						min_date;
		bool							show_check_box;
		bool							show_up_down;
		DateTime						date_value;
		bool							right_to_left_layout;

		// variables used for drawing and such
		internal const int					up_down_width = check_box_size;
		internal bool 					is_drop_down_visible;
		internal bool						is_up_pressed;
		internal bool						is_down_pressed;
		internal Timer						updown_timer;
		internal const int					initial_timer_delay = 500;
		internal const int					subsequent_timer_delay = 100;
		internal bool						is_checkbox_selected;

		// variables for determining how to format the string
		internal PartData[]					part_data;
		internal int						editing_part_index = -1;
		internal int 						editing_number = -1;
		internal string						editing_text;

		bool drop_down_button_entered;
		#endregion	// Local variables
		
		#region DateTimePickerAccessibleObject Subclass
		[ComVisible(true)]
		public class DateTimePickerAccessibleObject : ControlAccessibleObject {
			#region DateTimePickerAccessibleObject Local Variables
			private new DateTimePicker	owner;
			#endregion	// DateTimePickerAccessibleObject Local Variables

			#region DateTimePickerAccessibleObject Constructors
			public DateTimePickerAccessibleObject(DateTimePicker owner) : base(owner) {
				this.owner = owner;
			}
			#endregion	// DateTimePickerAccessibleObject Constructors

			#region DateTimePickerAccessibleObject Properties
			public override string KeyboardShortcut {
				get {
					return base.KeyboardShortcut;
				}
			}

			public override AccessibleRole Role {
				get {
					return base.Role;
				}
			}

			public override AccessibleStates State {
				get {
					AccessibleStates	retval;

					retval = AccessibleStates.Default;

					if (owner.Checked) {
						retval |= AccessibleStates.Checked;
					}

					return retval;
				}
			}

			public override string Value {
				get {
					return owner.Text;
				}
			}
			#endregion	// DateTimePickerAccessibleObject Properties
		}
		#endregion	// DateTimePickerAccessibleObject Sub-class

		#region public constructors
		
		// only public constructor
		public DateTimePicker () {
		
			// initialise the month calendar
			month_calendar = new MonthCalendar (this);
			month_calendar.CalendarDimensions = new Size (1, 1);
			month_calendar.MaxSelectionCount = 1;
			month_calendar.ForeColor = Control.DefaultForeColor;
			month_calendar.BackColor = DefaultMonthBackColor;
			month_calendar.TitleBackColor = DefaultTitleBackColor;
			month_calendar.TitleForeColor = DefaultTitleForeColor;
			month_calendar.TrailingForeColor = DefaultTrailingForeColor;
			month_calendar.Visible = false;
			// initialize the timer
			updown_timer = new Timer();
			updown_timer.Interval = initial_timer_delay;

			
			// initialise other variables
			is_checked = true;
			custom_format = null;
			drop_down_align = LeftRightAlignment.Left;
			format = DateTimePickerFormat.Long;
			max_date = MaxDateTime;
			min_date = MinDateTime;
			show_check_box = false;
			show_up_down = false;
			date_value = DateTime.Now;
						
			is_drop_down_visible = false;
			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
			
			month_calendar.DateChanged += new DateRangeEventHandler (MonthCalendarDateChangedHandler);
			month_calendar.DateSelected += new DateRangeEventHandler (MonthCalendarDateSelectedHandler);
			month_calendar.LostFocus += new EventHandler (MonthCalendarLostFocusHandler);
			updown_timer.Tick += new EventHandler (UpDownTimerTick);
			KeyPress += new KeyPressEventHandler (KeyPressHandler);
			KeyDown += new KeyEventHandler (KeyDownHandler);
			GotFocus += new EventHandler (GotFocusHandler);
			LostFocus += new EventHandler (LostFocusHandler);
			MouseDown += new MouseEventHandler (MouseDownHandler);			
			MouseUp += new MouseEventHandler (MouseUpHandler);
			MouseEnter += new EventHandler (OnMouseEnter);
			MouseLeave += new EventHandler (OnMouseLeave);
			MouseMove += new MouseEventHandler (OnMouseMove);
			Paint += new PaintEventHandler (PaintHandler);
			Resize += new EventHandler (ResizeHandler);
			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);
			SetStyle (ControlStyles.FixedHeight, true);
			SetStyle (ControlStyles.Selectable, true);

			CalculateFormats ();
		}
		
		#endregion
		
		#region public properties
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			set {
				base.BackColor = value;
			}
			get {
				return base.BackColor;
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			set {
				base.BackgroundImage = value;
			}
			get {
				return base.BackgroundImage;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get{
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
			}
		}

		[AmbientValue(null)]
		[Localizable(true)]
		public Font CalendarFont {
			set {
				month_calendar.Font = value;
			}
			get {
				return month_calendar.Font;
			}
		}

		public Color CalendarForeColor {
			set {
				month_calendar.ForeColor = value;
			}
			get {
				return month_calendar.ForeColor;
			}
		}

		public Color CalendarMonthBackground {
			set {
				month_calendar.BackColor = value;
			}
			get {
				return month_calendar.BackColor;
			}
		}

		public Color CalendarTitleBackColor {
			set {
				month_calendar.TitleBackColor = value;
			}
			get {
				return month_calendar.TitleBackColor;
			}
		}

		public Color CalendarTitleForeColor {
			set {
				month_calendar.TitleForeColor = value;
			}
			get {
				return month_calendar.TitleForeColor;
			}
		}

		public Color CalendarTrailingForeColor {
			set {
				month_calendar.TrailingForeColor = value;
			}
			get {
				return month_calendar.TrailingForeColor;
			}
		}
		
		// when checked the value is grayed out
		[Bindable(true)]
		[DefaultValue(true)]
		public bool Checked {
			set {
				if (is_checked != value) {
					is_checked = value;
					// invalidate the value inside this control
					if (ShowCheckBox) {
						for (int i = 0; i < part_data.Length; i++)
							part_data [i].Selected = false;
						Invalidate (date_area_rect);
						OnUIAChecked ();
						OnUIASelectionChanged ();
					}
				}
			}
			get {
				return is_checked;
			}
		}
		
		// the custom format string to format this control with
		[Localizable (true)]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public string CustomFormat {
			set {
				if (custom_format != value) {
					custom_format = value;
					if (this.Format == DateTimePickerFormat.Custom) {
						CalculateFormats ();
					}
				}
			}
			get {
				return custom_format;
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

		// which side the drop down is to be aligned on
		[DefaultValue(LeftRightAlignment.Left)]
		[Localizable(true)]
		public LeftRightAlignment DropDownAlign {
			set {
				if (drop_down_align != value) {
					drop_down_align = value;
				}
			}
			get {
				return drop_down_align;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			set {
				base.ForeColor = value;
			}
			get {
				return base.ForeColor;
			}
		}
		
		// the format of the date time picker text, default is long
		[RefreshProperties(RefreshProperties.Repaint)]
		public DateTimePickerFormat Format {
			set {
				if (format != value) {
					format = value;
					RecreateHandle (); // MS recreates the handle on every format change.
					CalculateFormats ();
					this.OnFormatChanged (EventArgs.Empty);
					// invalidate the value inside this control
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return format;
			}
		}
		
		public DateTime MaxDate {
			set {
				if (value < min_date) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"'{0}' is not a valid value for 'MaxDate'. 'MaxDate' "
						+ "must be greater than or equal to MinDate.",
						value.ToString ("G"));
					throw new ArgumentOutOfRangeException ("MaxDate", msg);
				}
				if (value > MaxDateTime) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"DateTimePicker does not support dates after {0}.",
						MaxDateTime.ToString ("G", CultureInfo.CurrentCulture));
					throw new ArgumentOutOfRangeException ("MaxDate", msg);
				}
				if (max_date != value) {
					max_date = value;
					if (Value > max_date) {
						Value = max_date;
						// invalidate the value inside this control
						this.Invalidate (date_area_rect);
					}
					OnUIAMaximumChanged ();
				}
			}
			get {
				return max_date;
			}
		}
		
		public static DateTime MaximumDateTime {
			get {
				return MaxDateTime;
			}
		}
		
		public DateTime MinDate {
			set {
				// If the user tries to set DateTime.MinValue, fix it to
				// DateTimePicker's minimum.
				if (value == DateTime.MinValue)
					value = MinDateTime;
					
				if (value > MaxDate) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"'{0}' is not a valid value for 'MinDate'. 'MinDate' "
						+ "must be less than MaxDate.",
						value.ToString ("G"));
					throw new ArgumentOutOfRangeException ("MinDate", msg);
				}
				if (value < MinDateTime) {
					string msg = string.Format (CultureInfo.CurrentCulture,
						"DateTimePicker does not support dates before {0}.",
						MinDateTime.ToString ("G", CultureInfo.CurrentCulture));
					throw new ArgumentOutOfRangeException ("MinDate", msg);
				}
				if (min_date != value) {
					min_date = value;
					if (Value < min_date) {
						Value = min_date;
						// invalidate the value inside this control
						this.Invalidate (date_area_rect);
					}
					OnUIAMinimumChanged ();
				}
			}
			get {
				return min_date;
			}
		}

		public static DateTime MinimumDateTime {
			get {
				return MinDateTime;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		// the prefered height to draw this control using current font
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PreferredHeight {
			get {
				// Make it proportional
				return (int) Math.Ceiling (Font.Height * 1.5);
			}
		}

		[DefaultValue (false)]
		[Localizable (true)]
		public virtual bool RightToLeftLayout {
			get {
				return right_to_left_layout;
			}
			set {
				if (right_to_left_layout != value) {
					right_to_left_layout = value;
					OnRightToLeftLayoutChanged (EventArgs.Empty);
				}
			}
		}

		// whether or not the check box is shown
		[DefaultValue(false)]
		public bool ShowCheckBox {
			set {
				if (show_check_box != value) {
					show_check_box = value;
					// invalidate the value inside this control
					this.Invalidate (date_area_rect);
					OnUIAShowCheckBoxChanged ();
				}
			}
			get {
				return show_check_box;
			}
		}
		
		// if true show the updown control, else popup the monthcalendar
		[DefaultValue(false)]
		public bool ShowUpDown {
			set {
				if (show_up_down != value) {
					show_up_down = value;
					// need to invalidate the whole control
					this.Invalidate ();
					OnUIAShowUpDownChanged ();
				}
			}
			get {
				return show_up_down;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text {
			set {
				DateTime parsed_value;
				
				if (value == null || value == string.Empty) {
					date_value = DateTime.Now;
					OnValueChanged (EventArgs.Empty);
					OnTextChanged (EventArgs.Empty);
					return;
				}
				
				if (format == DateTimePickerFormat.Custom) {
					// TODO: if the format is a custom format we need to do a custom parse here
					// This implementation will fail if the custom format is set to something that can
					// be a standard datetime format string
					// http://msdn2.microsoft.com/en-us/library/az4se3k1.aspx
					parsed_value = DateTime.ParseExact (value, GetExactFormat (), null);
				} else {
					parsed_value = DateTime.ParseExact (value, GetExactFormat (), null);
				}
				
				if (date_value != parsed_value) {
					Value = parsed_value;
				}
			}
			get {
				if (!IsHandleCreated)
					return "";
				
				if (format == DateTimePickerFormat.Custom) {
					System.Text.StringBuilder result = new System.Text.StringBuilder ();
					for (int i = 0; i < part_data.Length; i++) { 
						result.Append(part_data[i].GetText(date_value));
					}
					return result.ToString ();
				} else {
					return Value.ToString (GetExactFormat ());
				}
			}
		}	

		[Bindable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public DateTime Value {
			set {
				if (date_value != value) {
					if (value < MinDate || value > MaxDate)
						throw new ArgumentOutOfRangeException ("value", "value must be between MinDate and MaxDate");

					date_value = value;
					this.OnValueChanged (EventArgs.Empty);
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return date_value;
			}			
		}

		#endregion 	// public properties
		
		#region public methods
		
		// just return the text value
		public override string ToString () {
			return this.Text;
		} 
				
		#endregion 	// public methods
		
		#region public events
		static object CloseUpEvent = new object ();
		static object DropDownEvent = new object ();
		static object FormatChangedEvent = new object ();
		static object ValueChangedEvent = new object ();
		static object RightToLeftLayoutChangedEvent = new object ();

		// raised when the monthcalendar is closed
		public event EventHandler CloseUp {
			add { Events.AddHandler (CloseUpEvent, value); }
			remove { Events.RemoveHandler (CloseUpEvent, value); }
		}
		
		// raised when the monthcalendar is opened
		public event EventHandler DropDown {
			add { Events.AddHandler (DropDownEvent, value); }
			remove { Events.RemoveHandler (DropDownEvent, value); }
		}
		
		// raised when the format of the value is changed
		public event EventHandler FormatChanged {
			add { Events.AddHandler (FormatChangedEvent, value); }
			remove { Events.RemoveHandler (FormatChangedEvent, value); }
		}
		
		// raised when the date Value is changed
		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add {
				base.BackColorChanged += value;
			}

			remove {
				base.BackColorChanged -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add {
				base.BackgroundImageChanged += value;
			}

			remove {
				base.BackgroundImageChanged -= value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add
			{
				base.BackgroundImageLayoutChanged += value;
			}

			remove
			{
				base.BackgroundImageLayoutChanged -= value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add {
				base.Click += value;
			}
			remove {
				base.Click -= value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add {
				base.DoubleClick += value;
			}
			remove {
				base.DoubleClick -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add {
				base.ForeColorChanged += value;
			}

			remove {
				base.ForeColorChanged -= value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick {
			add {
				base.MouseClick += value;
			}
			remove {
				base.MouseClick -= value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add {
				base.MouseDoubleClick += value;
			}
			remove {
				base.MouseDoubleClick -= value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add
			{
				base.PaddingChanged += value;
			}
			remove
			{
				base.PaddingChanged -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add {
				base.Paint += value;
			}

			remove {
				base.Paint -= value;
			}
		}

		public event EventHandler RightToLeftLayoutChanged {
			add {
				Events.AddHandler (RightToLeftLayoutChangedEvent, value);
			}
			remove {
				Events.RemoveHandler (RightToLeftLayoutChangedEvent, value);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler TextChanged {
			add {
				base.TextChanged += value;
			}

			remove {
				base.TextChanged -= value;
			}
		}
		#endregion	// public events
		
		#region protected properties

		// not sure why we're overriding this one		
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		
		// specify the default size for this control
		protected override Size DefaultSize {
			get {
				// todo actually measure this properly
				return new Size (200, PreferredHeight);
			}
		}
		
		#endregion	// protected properties
		
		#region protected methods
		
		// not sure why we're overriding this one
		protected override AccessibleObject CreateAccessibilityInstance () {
			return base.CreateAccessibilityInstance ();
		}
		
		// not sure why we're overriding this one
		protected override void CreateHandle () {
			base.CreateHandle ();
		}
		
		// not sure why we're overriding this one
		protected override void DestroyHandle () {
			base.DestroyHandle ();
		}

		// find out if this key is an input key for us, depends on which date part is focused
		protected override bool IsInputKey (Keys keyData) {
			switch (keyData)
			{
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
					return true;
			}
			return false;
		}
		
		// raises the CloseUp event
		protected virtual void OnCloseUp (EventArgs eventargs) {
			EventHandler eh = (EventHandler)(Events [CloseUpEvent]);
			if (eh != null)
				eh (this, eventargs);
		}
		
		// raise the drop down event
		protected virtual void OnDropDown (EventArgs eventargs) {
			EventHandler eh = (EventHandler)(Events [DropDownEvent]);
			if (eh != null)
				eh (this, eventargs);
		}

		protected override void OnFontChanged(EventArgs e) {
			// FIXME - do we need to update/invalidate/recalc our stuff?
			month_calendar.Font = Font;
			Size = new Size (Size.Width, PreferredHeight);

			base.OnFontChanged (e);
		}
		
		// raises the format changed event
		protected virtual void OnFormatChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [FormatChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void  OnHandleCreated (EventArgs e) {
			 base.OnHandleCreated(e);
		}
		protected override void  OnHandleDestroyed (EventArgs e) {
 			 base.OnHandleDestroyed(e);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e) {
			EventHandler eh = (EventHandler) Events [RightToLeftLayoutChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		// not sure why we're overriding this one 
		protected override void OnSystemColorsChanged (EventArgs e) {
			base.OnSystemColorsChanged (e);
		}
		
		// raise the ValueChanged event
		protected virtual void OnValueChanged (EventArgs eventargs) {
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, eventargs);
		}

		// SetBoundsCore was removed from the 2.0 public API, so
		// I had to do this hack instead.  :/
		internal override int OverrideHeight (int height)
		{
			return DefaultSize.Height;
		}

		// not sure why we're overriding this
		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}
		
		#endregion	// protected methods
		
		#region internal / private properties
		
		// this is the region that the date and the check box is drawn on
		internal Rectangle date_area_rect {
			get {
				return ThemeEngine.Current.DateTimePickerGetDateArea (this);
			}
		}

		internal Rectangle CheckBoxRect {
			get {
				Rectangle retval = new Rectangle (check_box_space, ClientSize.Height / 2 - check_box_size / 2, 
						check_box_size, check_box_size);
				return retval;
			}
		}
		
		// the rectangle for the drop down arrow
		internal Rectangle drop_down_arrow_rect {
			get {
				return ThemeEngine.Current.DateTimePickerGetDropDownButtonArea (this);
			}
		}
		
		// the part of the date that is currently hilighted
		internal Rectangle hilight_date_area {
			get {
				// TODO: put hilighted part calculation in here
				return Rectangle.Empty;
			}
		}	

		internal bool DropDownButtonEntered {
			get { return drop_down_button_entered; }
		}
			
		#endregion
		
		#region internal / private methods

		private void ResizeHandler (object sender, EventArgs e)
		{
			Invalidate ();
		}

		private void UpDownTimerTick (object sender, EventArgs e)
		{
			if (updown_timer.Interval == initial_timer_delay)
				updown_timer.Interval = subsequent_timer_delay;

			if (is_down_pressed)
				IncrementSelectedPart (-1);
			else if (is_up_pressed)
				IncrementSelectedPart (1);
			else
				updown_timer.Enabled = false;
		}
		
		// calculates the maximum width 
		internal Single CalculateMaxWidth(string format, Graphics gr, StringFormat string_format)
		{
			SizeF size;
			float result = 0;
			string text;
			Font font = this.Font;

			switch (format)
			{
				case "M":
				case "MM":
				case "MMM":
				case "MMMM":
					for (int i = 1; i <= 12; i++) {
						text = PartData.GetText (Value.AddMonths (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "d":
				case "dd":
				case "ddd":
				case "dddd":
					for (int i = 1; i <= 12; i++) {
						text = PartData.GetText (Value.AddDays (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "h":
				case "hh":
					for (int i = 1; i <= 12; i++) {
						text = PartData.GetText (Value.AddHours (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "H":
				case "HH":
					for (int i = 1; i <= 24; i++) {
						text = PartData.GetText (Value.AddDays (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "m":
				case "mm":
					for (int i = 1; i <= 60; i++) {
						text = PartData.GetText (Value.AddMinutes (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "s":
				case "ss":
					for (int i = 1; i <= 60; i++) {
						text = PartData.GetText (Value.AddSeconds (i), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "t":
				case "tt":
					for (int i = 1; i <= 2; i++) {
						text = PartData.GetText (Value.AddHours (i * 12), format);
						size = gr.MeasureString (text, font, int.MaxValue, string_format);
						result = Math.Max (result, size.Width);
					}
					return result;
				case "y":
				case "yy":
				case "yyyy":
					// Actually all the allowed year values are between MinDateTime and MaxDateTime,
					// which are 4 digits always
					text = PartData.GetText (Value, format);
					size = gr.MeasureString (text, font, int.MaxValue, string_format);
					result = Math.Max (result, size.Width);
					return result;
				default:
					return gr.MeasureString (format, font, int.MaxValue, string_format).Width;
			}
		}

		// returns the format of the date as a string 
		// (i.e. resolves the Format enum values to it's corresponding string format)
		// Why CurrentCulture and not CurrentUICulture is explained here:
		// http://blogs.msdn.com/michkap/archive/2007/01/11/1449754.aspx
		private string GetExactFormat()
		{
			switch (this.format) {
			case DateTimePickerFormat.Long:
				return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern;
			case DateTimePickerFormat.Short:
				return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
			case DateTimePickerFormat.Time:
				return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern;
			case DateTimePickerFormat.Custom:
				return this.custom_format == null ? String.Empty : this.custom_format;
			default:
				return Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern;
			}
		}

		private void CalculateFormats()
		{
			string real_format;
			System.Text.StringBuilder literal = new System.Text.StringBuilder ();
			System.Collections.ArrayList formats = new ArrayList ();
			bool is_literal = false;
			char lastch = (char) 0;
			char ch;

			real_format = GetExactFormat ();

			// parse the format string
			for (int i = 0; i < real_format.Length; i++)
			{
				ch = real_format [i];

				if (is_literal && ch != '\'')
				{
					literal.Append (ch);
					continue;
				}

				switch (ch)
				{
					case 't':
					case 'd':
					case 'h':
					case 'H':
					case 'm':
					case 'M':
					case 's':
					case 'y':
					case 'g': // Spec says nothing about g, but it seems to be treated like spaces.
						if (!(lastch == ch || lastch == 0) && literal.Length != 0)
						{
							formats.Add (new PartData(literal.ToString (), false, this));
							literal.Length = 0;
						}
						literal.Append (ch);
						break;
					case '\'':
						if (is_literal && i < real_format.Length - 1 && real_format [i + 1] == '\'') {
							literal.Append (ch);
							i++;
							break;
						}
						if (literal.Length == 0) {
							is_literal = !is_literal;
							break;
						}
						formats.Add (new PartData (literal.ToString (), is_literal, this));
						literal.Length = 0;
						is_literal = !is_literal;
						break;
					default:
						if (literal.Length != 0)
						{
							formats.Add (new PartData(literal.ToString (), false, this));
							literal.Length = 0;
						}
						formats.Add (new PartData (ch.ToString(), true, this));
						break;

				}
				lastch = ch;
			}
			if (literal.Length >= 0)
				formats.Add (new PartData (literal.ToString (), is_literal, this));

			part_data = new PartData [formats.Count];
			formats.CopyTo (part_data);
		}

		private Point CalculateDropDownLocation (Rectangle parent_control_rect, Size child_size, bool align_left)
		{
			// default bottom left
			Point location = new Point(parent_control_rect.Left + 5, parent_control_rect.Bottom);
			// now adjust the alignment
			if (!align_left) {
				location.X = parent_control_rect.Right - child_size.Width;				
			}
			
			Point screen_location = PointToScreen (location);			
			Rectangle working_area = Screen.FromControl(this).WorkingArea;
			// now adjust if off the right side of the screen			
			if (screen_location.X < working_area.X) {
				screen_location.X = working_area.X;
			}  
			// now adjust if it should be displayed above control
			if (screen_location.Y + child_size.Height > working_area.Bottom) {
				screen_location.Y -= (parent_control_rect.Height + child_size.Height);
			}

			// since the parent of the month calendar is the form, adjust accordingly.
			if (month_calendar.Parent != null) {
				screen_location = month_calendar.Parent.PointToClient(screen_location);
			}

			return screen_location;
		}
		
		// actually draw this control
		internal void Draw (Rectangle clip_rect, Graphics dc)
		{			
			ThemeEngine.Current.DrawDateTimePicker (dc, clip_rect, this);
		}			
		
		// drop the calendar down
		internal void DropDownMonthCalendar ()
		{
			EndDateEdit (true);
			// ensure the right date is set for the month_calendar
			month_calendar.SetDate (this.date_value);
			// get a rectangle that has the dimensions of the text area,
			// but the height of the dtp control.
			Rectangle align_area = this.date_area_rect;
			align_area.Y = this.ClientRectangle.Y;
			align_area.Height = this.ClientRectangle.Height;

			// establish the month calendar's location
			month_calendar.Location = CalculateDropDownLocation (
				align_area,
				month_calendar.Size,
				(this.DropDownAlign == LeftRightAlignment.Left));
			month_calendar.Show ();
			month_calendar.Focus ();
			month_calendar.Capture = true;

			// fire any registered events
			// XXX should this just call OnDropDown?
			EventHandler eh = (EventHandler)(Events [DropDownEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		// hide the month calendar
		internal void HideMonthCalendar () 
		{
			this.is_drop_down_visible = false;
			Invalidate (drop_down_arrow_rect);
			month_calendar.Capture = false;
			if (month_calendar.Visible) {
				month_calendar.Hide ();
			}
			Focus ();
		}

		private int GetSelectedPartIndex()
		{
			for (int i = 0; i < part_data.Length; i++)
			{
				if (part_data[i].Selected && !part_data[i].is_literal)
					return i;
			}
			return -1;
		}

		internal void IncrementSelectedPart(int delta)
		{
			int selected_index = GetSelectedPartIndex();

			if (selected_index == -1) {
				return;
			}

			EndDateEdit (false);

			DateTimePart dt_part = part_data [selected_index].date_time_part;
			switch (dt_part)
			{
				case DateTimePart.Day:
					if (delta < 0) {
						if (Value.Day == 1)
							SetPart(DateTime.DaysInMonth(Value.Year, Value.Month), dt_part);
						else
							SetPart(Value.Day + delta, dt_part);
					} else {
						if (Value.Day == DateTime.DaysInMonth(Value.Year, Value.Month))
							SetPart(1, dt_part);
						else
							SetPart(Value.Day + delta, dt_part) ;
					}
					break;
				case DateTimePart.DayName:
					Value = Value.AddDays(delta);
					break;
				case DateTimePart.AMPMHour:
				case DateTimePart.Hour:
					SetPart(Value.Hour + delta, dt_part);
					break;
				case DateTimePart.Minutes:
					SetPart(Value.Minute + delta, dt_part);
					break;
				case DateTimePart.Month:
					SetPart (Value.Month + delta, dt_part, true);
					break;
				case DateTimePart.Seconds:
					SetPart(Value.Second + delta, dt_part);
					break;
				case DateTimePart.AMPMSpecifier:
					int hour = Value.Hour;
					hour = hour >= 0 && hour <= 11 ? hour + 12 : hour - 12;
					SetPart (hour, DateTimePart.Hour);
					break;
				case DateTimePart.Year:
					SetPart(Value.Year + delta, dt_part);
					break;
			}
		}
		
		internal void SelectPart (int index)
		{
			is_checkbox_selected = false;
			for (int i = 0; i < part_data.Length; i++)
			{
				part_data[i].Selected = (i == index);
			}

			Invalidate ();
			OnUIASelectionChanged ();
		}

		internal void SelectNextPart()
		{
			int selected_index;
			if (is_checkbox_selected) {
				for (int i = 0; i < part_data.Length; i++)
				{
					if (!part_data[i].is_literal)
					{
						is_checkbox_selected = false;
						part_data[i].Selected = true;
						Invalidate();
						break;
					}
				}
			} else {
				selected_index = GetSelectedPartIndex();
				if (selected_index >= 0)
					part_data [selected_index].Selected = false;

				for (int i = selected_index + 1; i < part_data.Length; i++)
				{
					if (!part_data[i].is_literal)
					{
						part_data [i].Selected = true;
						Invalidate();
						break;
					}
				}
				if (GetSelectedPartIndex() == -1)
				{ // if no part was found before the end, look from the beginning
					if (ShowCheckBox)
					{
						is_checkbox_selected = true;
						Invalidate();
					}
					else
					{
						for (int i = 0; i <= selected_index; i++)
						{
							if (!part_data[i].is_literal)
							{
								part_data[i].Selected = true;
								Invalidate();
								break;
							}
						}
					}
				}
			}

			OnUIASelectionChanged ();
		}

		internal void SelectPreviousPart()
		{
			if (is_checkbox_selected)
			{
				for (int i = part_data.Length - 1; i >= 0; i--)
				{
					if (!part_data[i].is_literal)
					{
						is_checkbox_selected = false;
						part_data[i].Selected = true;
						Invalidate();
						break;
					}
				}
			}
			else
			{
				int selected_index = GetSelectedPartIndex();

				if (selected_index >= 0)
					part_data[selected_index].Selected = false;

				for (int i = selected_index - 1; i >= 0; i--)
				{
					if (!part_data[i].is_literal)
					{
						part_data[i].Selected = true;
						Invalidate();
						break;
					}
				}
				if (GetSelectedPartIndex() == -1)
				{	// if no part was found before the beginning, look from the end
					if (ShowCheckBox)
					{
						is_checkbox_selected = true;
						Invalidate();
					}
					else
					{
						for (int i = part_data.Length - 1; i >= selected_index; i--)
						{
							if (!part_data[i].is_literal)
							{
								part_data[i].Selected = true;
								Invalidate();
								break;
							}
						}
					}
				}
			}

			OnUIASelectionChanged ();
		}

		// raised by key down events.
		private void KeyDownHandler(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Add:
				case Keys.Up:
					{
						if (ShowCheckBox && Checked == false)
							break;
						IncrementSelectedPart(1);
						e.Handled = true;
						break;
					}
				case Keys.Subtract:
				case Keys.Down:
					{
						if (ShowCheckBox && Checked == false)
							break;
						IncrementSelectedPart(-1);
						e.Handled = true;
						break;
					}
				case Keys.Left:
					{// select the next part to the left
						if (ShowCheckBox && Checked == false)
							break;
						SelectPreviousPart();
						e.Handled = true;
						break;
					}
				case Keys.Right:
					{// select the next part to the right
						if (ShowCheckBox && Checked == false)
							break;
						SelectNextPart();
						e.Handled = true;
						break;
					}
				case Keys.F4:
					if (!e.Alt && !is_drop_down_visible) {
						DropDownMonthCalendar ();
						e.Handled = true;
					}

					break;
			}
		}

		// raised by any key down events
		private void KeyPressHandler (object sender, KeyPressEventArgs e)
		{
			switch (e.KeyChar) {
				case ' ':
					if (show_check_box && is_checkbox_selected)
						Checked = !Checked;
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					int number = e.KeyChar - (int) '0';
					int selected_index = GetSelectedPartIndex();
					if (selected_index == -1)
						break;
					if (!part_data[selected_index].is_numeric_format)
						break;

					DateTimePart dt_part = part_data [selected_index].date_time_part;
					if (editing_part_index < 0) {
						editing_part_index = selected_index;
						editing_number = 0;
						editing_text = String.Empty;
					}

					editing_text += number.ToString ();
					int date_part_max_length = 0;

					switch (dt_part) {
						case DateTimePart.Day:
						case DateTimePart.Month:
						case DateTimePart.Seconds:
						case DateTimePart.Minutes:
						case DateTimePart.AMPMHour:
						case DateTimePart.Hour:
							date_part_max_length = 2;
							break;
						case DateTimePart.Year:
							date_part_max_length = 4;
							break;
					}

					editing_number = editing_number * 10 + number;
					if (editing_text.Length >= date_part_max_length)
						EndDateEdit (false);

					Invalidate (date_area_rect);
					break;
				default:
					break;
			}
			e.Handled = true;
		}

		private void EndDateEdit (bool invalidate)
		{
			if (editing_part_index == -1)
				return;

			PartData part = part_data [editing_part_index];
			if (part.date_time_part == DateTimePart.Year) { // Special case
				// Infer, like .Net does
				if (editing_number > 0 && editing_number < 30)
					editing_number += 2000;
				else if (editing_number >= 30 && editing_number < 100)
					editing_number += 1900;
			}

			SetPart (editing_number, part.date_time_part);
			editing_part_index = editing_number = -1;
			editing_text = null;

			if (invalidate)
				Invalidate (date_area_rect);
		}

		internal void SetPart (int value, DateTimePart dt_part)
		{
			SetPart (value, dt_part, false);
		}

		// set the specified part of the date to the specified value
		internal void SetPart (int value, DateTimePart dt_part, bool adjust)
		{
			switch (dt_part)
			{
				case DateTimePart.Seconds:
					if (value == -1)
						value = 59;
					if (value >= 0 && value <= 59)
						Value = new DateTime(Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, value, Value.Millisecond);
					break;
				case DateTimePart.Minutes:
					if (value == -1)
						value = 59;
					if (value >= 0 && value <= 59)
						Value = new DateTime(Value.Year, Value.Month, Value.Day, Value.Hour, value, Value.Second, Value.Millisecond);
					break;
				case DateTimePart.AMPMHour:
					if (value == -1)
						value = 23;
					if (value >= 0 && value <= 23) {
						int prev_hour = Value.Hour;
						if ((prev_hour >= 12 && prev_hour <= 23) && value < 12) // Adjust to p.m.
							value += 12;
						Value = new DateTime (Value.Year, Value.Month, Value.Day, value, Value.Minute, Value.Second, Value.Millisecond);
					}
					break;
				case DateTimePart.Hour:
					if (value == -1)
						value = 23;
					if (value >= 0 && value <= 23)
						Value = new DateTime(Value.Year, Value.Month, Value.Day, value, Value.Minute, Value.Second, Value.Millisecond);
					break;
				case DateTimePart.Day:
					int max_days = DateTime.DaysInMonth(Value.Year, Value.Month);
					if (value >= 1 && value <= 31 && value <= max_days)
						Value = new DateTime(Value.Year, Value.Month, value, Value.Hour, Value.Minute, Value.Second, Value.Millisecond);
					break;
				case DateTimePart.Month:
					DateTime date = Value;

					if (adjust) {
						if (value == 0) {
							date = date.AddYears (-1);
							value = 12;
						} else if (value == 13) {
							date = date.AddYears (1);
							value = 1;
						}
					}

					if (value >= 1 && value <= 12) {
						// if we move from say december to november with days on 31, we must
						// remap to the maximum number of days
						int days_in_new_month = DateTime.DaysInMonth (date.Year, value);
						
						if (date.Day > days_in_new_month)
							Value = new DateTime (date.Year, value, days_in_new_month, date.Hour, date.Minute, date.Second, date.Millisecond);
						else
							Value = new DateTime (date.Year, value, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
					}
					break;
				case DateTimePart.Year:
					if (value >= min_date.Year && value <= max_date.Year) {
						// if we move to a leap year, the days in month could throw an exception
						int days_in_new_month = DateTime.DaysInMonth (value, Value.Month);
						
						if (Value.Day > days_in_new_month)
							Value = new DateTime (value, Value.Month, days_in_new_month, Value.Hour, Value.Minute, Value.Second, Value.Millisecond);
						else
							Value = new DateTime (value, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second, Value.Millisecond);
					}
					break;
			}
		}

		private void GotFocusHandler (object sender, EventArgs e)
		{
			if (ShowCheckBox) {
				is_checkbox_selected = true;
				Invalidate (CheckBoxRect);
				OnUIASelectionChanged ();
			}
		}

		// if we loose focus deselect any selected parts.
		private void LostFocusHandler (object sender, EventArgs e) 
		{
			int selected_index = GetSelectedPartIndex ();
			if (selected_index != -1)
			{
				part_data [selected_index].Selected = false;
				Rectangle invalidate_rect = Rectangle.Ceiling (part_data [selected_index].drawing_rectangle);
				invalidate_rect.Inflate (2, 2);
				Invalidate (invalidate_rect);
				OnUIASelectionChanged ();
			}
			else if (is_checkbox_selected)
			{
				is_checkbox_selected = false;
				Invalidate (CheckBoxRect);
				OnUIASelectionChanged ();
			}
		}

		// if month calendar looses focus and the drop down is up, then close it
		private void MonthCalendarLostFocusHandler(object sender, EventArgs e)
		{
			if (is_drop_down_visible && !month_calendar.Focused)
			{
				//this.HideMonthCalendar(); 
				//This is handled from the monthcalender itself, 
				//it may loose focus, but still has to be visible,
				//for instance when the context menu is displayed.
			}

		}

		private void MonthCalendarDateChangedHandler (object sender, DateRangeEventArgs e)
		{
			if (month_calendar.Visible)
				this.Value = e.Start.Date.Add (this.Value.TimeOfDay);
		}

		// fired when a user clicks on the month calendar to select a date
		private void MonthCalendarDateSelectedHandler (object sender, DateRangeEventArgs e)
		{
			this.HideMonthCalendar ();	
		} 

		private void MouseUpHandler(object sender, MouseEventArgs e)
		{
			if (ShowUpDown)
			{
				if (is_up_pressed || is_down_pressed)
				{
					updown_timer.Enabled = false;
					is_up_pressed = false;
					is_down_pressed = false;
					Invalidate (drop_down_arrow_rect);
				}
			}
		}

		// to check if the mouse has come down on this control
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			// Only left clicks are handled.
			if (e.Button != MouseButtons.Left)
				return;

			if (ShowCheckBox && CheckBoxRect.Contains(e.X, e.Y))
			{
				is_checkbox_selected = true;
				Checked = !Checked;
				OnUIASelectionChanged ();
				return;
			}

			// Deselect the checkbox only if the pointer is not on it
			// *and* the other parts are enabled (Checked as true)
			if (Checked) {
				is_checkbox_selected = false;
				OnUIASelectionChanged ();
			}

			if (ShowUpDown && drop_down_arrow_rect.Contains (e.X, e.Y))
			{
				if (!(ShowCheckBox && Checked == false))
				{
					if (e.Y < this.Height / 2) {
						is_up_pressed = true;
						is_down_pressed = false;
						IncrementSelectedPart (1);
					} else {
						is_up_pressed = false;
						is_down_pressed = true;
						IncrementSelectedPart (-1);
					}
					Invalidate (drop_down_arrow_rect);
					updown_timer.Interval = initial_timer_delay;
					updown_timer.Enabled = true;
				}
			} else if (is_drop_down_visible == false && drop_down_arrow_rect.Contains (e.X, e.Y)) {
				DropDownButtonClicked ();
    			} else {
    				// mouse down on this control anywhere else collapses it
    				if (is_drop_down_visible) {    				
    					HideMonthCalendar ();
    				}
				if (!(ShowCheckBox && Checked == false))
				{
					// go through the parts to see if the click is in any of them
					bool invalidate_afterwards = false;
					for (int i = 0; i < part_data.Length; i++) {
						bool old = part_data [i].Selected;

						if (part_data [i].is_literal)
							continue;

						if (part_data [i].drawing_rectangle.Contains (e.X, e.Y)) {
							part_data [i].Selected = true;
						} else
							part_data [i].Selected = false;

						if (old != part_data [i].Selected) 
							invalidate_afterwards = true;
					}
					if (invalidate_afterwards) {
						Invalidate ();
						OnUIASelectionChanged ();
					}
				}
				
			}
		}
		
		internal void DropDownButtonClicked ()
		{
			if (!is_drop_down_visible) {
				is_drop_down_visible = true;
				if (!Checked)
					Checked = true;
				Invalidate (drop_down_arrow_rect);
				DropDownMonthCalendar ();
			} else {
				HideMonthCalendar ();
			}
		}
		
		// paint this control now
		private void PaintHandler (object sender, PaintEventArgs pe) {
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pe.ClipRectangle, pe.Graphics);
		}
		
		void OnMouseEnter (object sender, EventArgs e)
		{
			if (ThemeEngine.Current.DateTimePickerBorderHasHotElementStyle)
				Invalidate ();
		}

		void OnMouseLeave (object sender, EventArgs e)
		{
			drop_down_button_entered = false;
			if (ThemeEngine.Current.DateTimePickerBorderHasHotElementStyle)
				Invalidate ();
		}

		void OnMouseMove (object sender, MouseEventArgs e)
		{
			if (!is_drop_down_visible &&
				ThemeEngine.Current.DateTimePickerDropDownButtonHasHotElementStyle &&
				drop_down_button_entered != drop_down_arrow_rect.Contains (e.Location)) {
				drop_down_button_entered = !drop_down_button_entered;
				Invalidate (drop_down_arrow_rect);
			}
		}
		#endregion		

		#region internal classes
		internal enum DateTimePart {
			Seconds,
			Minutes,
			AMPMHour,
			Hour,
			Day,
			DayName,
			Month,
			Year,
			AMPMSpecifier,
			Literal
		}

		internal class PartData
		{
			internal string value;
			internal bool is_literal;
			bool is_selected;
			internal RectangleF drawing_rectangle;
			internal DateTimePart date_time_part;
			DateTimePicker owner;

			internal bool is_numeric_format
			{
				get
				{
					if (is_literal)
						return false;
					switch (value) {
					case "m":
					case "mm":
					case "d":
					case "dd":
					case "h":
					case "hh":
					case "H":
					case "HH":
					case "M":
					case "MM":
					case "s":
					case "ss":
					case "y":
					case "yy":
					case "yyyy":
						return true;
					case "ddd":
					case "dddd":
						return false;
					default:
						return false;
					}
				}
			}

			internal PartData(string value, bool is_literal, DateTimePicker owner)
			{
				this.value = value;
				this.is_literal = is_literal;
				this.owner = owner;
				date_time_part = GetDateTimePart (value);
			}

			internal bool Selected {
				get {
					return is_selected;
				}
				set {
					if (value == is_selected)
						return;

					owner.EndDateEdit (false);
					is_selected = value;
				}
			}

			// calculate the string to show for this data
			internal string GetText(DateTime date)
			{
				if (is_literal) {
					return value;
				} else {
					return GetText (date, value);
				}
			}

			static DateTimePart GetDateTimePart (string value)
			{
				switch (value) {
					case "s":
					case "ss":
						return DateTimePart.Seconds;
					case "m":
					case "mm":
						return DateTimePart.Minutes;
					case "h":
					case "hh":
						return DateTimePart.AMPMHour;
					case "H":
					case "HH":
						return DateTimePart.Hour;
					case "d":
					case "dd":
						return DateTimePart.Day;
					case "ddd":
					case "dddd":
						return DateTimePart.DayName;
					case "M":
					case "MM":
					case "MMMM":
						return DateTimePart.Month;
					case "y":
					case "yy":
					case "yyy":
					case "yyyy":
						return DateTimePart.Year;
					case "t":
					case "tt":
						return DateTimePart.AMPMSpecifier;
				}
					
				return DateTimePart.Literal;
			}

			static internal string GetText(DateTime date, string format)
			{
				if (format.StartsWith ("g")) 
					return " ";
				else if (format.Length == 1)
					return date.ToString ("%" + format);
				else if (format == "yyyyy" || format == "yyyyyy" || format == "yyyyyyy" || format == "yyyyyyyy")
					return date.ToString ("yyyy");
				else if (format.Length > 1)
					return date.ToString (format);
				else
					return string.Empty;
			}
		}
		
		#endregion		

		#region UIA Framework: Methods, Properties and Events

		internal bool UIAIsCheckBoxSelected {
			get { return is_checkbox_selected; }
		}

		static object UIAMinimumChangedEvent = new object ();
		static object UIAMaximumChangedEvent = new object ();
		static object UIASelectionChangedEvent = new object ();
		static object UIACheckedEvent = new object ();
		static object UIAShowCheckBoxChangedEvent = new object ();
		static object UIAShowUpDownChangedEvent = new object ();

		internal event EventHandler UIAMinimumChanged {
			add { Events.AddHandler (UIAMinimumChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMinimumChangedEvent, value); }
		}

		internal event EventHandler UIAMaximumChanged {
			add { Events.AddHandler (UIAMinimumChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMinimumChangedEvent, value); }
		}

		internal event EventHandler UIASelectionChanged {
			add { Events.AddHandler (UIASelectionChangedEvent, value); }
			remove { Events.RemoveHandler (UIASelectionChangedEvent, value); }
		}

		internal event EventHandler UIAChecked {
			add { Events.AddHandler (UIACheckedEvent, value); }
			remove { Events.RemoveHandler (UIACheckedEvent, value); }
		}

		internal event EventHandler UIAShowCheckBoxChanged {
			add { Events.AddHandler (UIAShowCheckBoxChangedEvent, value); }
			remove { Events.RemoveHandler (UIAShowCheckBoxChangedEvent, value); }
		}

		internal event EventHandler UIAShowUpDownChanged {
			add { Events.AddHandler (UIAShowUpDownChangedEvent, value); }
			remove { Events.RemoveHandler (UIAShowUpDownChangedEvent, value); }
		}

		internal void OnUIAMinimumChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIAMinimumChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAMaximumChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIAMaximumChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIASelectionChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIASelectionChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAChecked ()
		{
			EventHandler eh = (EventHandler)(Events [UIACheckedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAShowCheckBoxChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIAShowCheckBoxChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAShowUpDownChanged ()
		{
			EventHandler eh = (EventHandler)(Events [UIAShowUpDownChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		#endregion
	}
}
