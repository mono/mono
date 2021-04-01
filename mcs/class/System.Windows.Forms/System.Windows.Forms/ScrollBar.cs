//
// System.Windows.Forms.ScrollBar.cs
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
// Copyright (C) 2004-2005, Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	jordi@ximian.com
//
//

// COMPLETE

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent ("Scroll")]
	[DefaultProperty ("Value")]
	public abstract class ScrollBar : Control
	{
		#region Local Variables
		private int position;
		private int minimum;
		private int maximum;
		private int large_change;
		private int small_change;
		internal int scrollbutton_height;
		internal int scrollbutton_width;
		private Rectangle first_arrow_area = new Rectangle ();		// up or left
		private Rectangle second_arrow_area = new Rectangle ();		// down or right
		private Rectangle thumb_pos = new Rectangle ();
		private Rectangle thumb_area = new Rectangle ();
		internal ButtonState firstbutton_state = ButtonState.Normal;
		internal ButtonState secondbutton_state = ButtonState.Normal;
		private bool firstbutton_pressed = false;
		private bool secondbutton_pressed = false;
		private bool thumb_pressed = false;
		private float pixel_per_pos = 0;
		private Timer timer = new Timer ();
		private TimerType timer_type;
		private int thumb_size = 40;
		private const int thumb_min_size = 8;
		private const int thumb_notshown_size = 40;
		internal bool use_manual_thumb_size;
		internal int manual_thumb_size;
		internal bool vert;
		internal bool implicit_control;
		private int lastclick_pos;		// Position of the last button-down event
		private int thumbclick_offset;		// Position of the last button-down event relative to the thumb edge
		private Rectangle dirty;

		internal ThumbMoving thumb_moving = ThumbMoving.None;
		bool first_button_entered;
		bool second_button_entered;
		bool thumb_entered;
		#endregion	// Local Variables

		private enum TimerType
		{
			HoldButton,
			RepeatButton,
			HoldThumbArea,
			RepeatThumbArea
		}

		internal enum ThumbMoving
		{
			None,
			Forward,
			Backwards,
		}

		#region events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDown {
			add { base.MouseDown += value; }
			remove { base.MouseDown -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseUp {
			add { base.MouseUp += value; }
			remove { base.MouseUp -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		static object ScrollEvent = new object ();
		static object ValueChangedEvent = new object ();

		public event ScrollEventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}
		#endregion Events

		public ScrollBar ()
		{
			position = 0;
			minimum = 0;
			maximum = 100;
			large_change = 10;
			small_change = 1;

			timer.Tick += new EventHandler (OnTimer);
			MouseEnter += new EventHandler (OnMouseEnter);
			MouseLeave += new EventHandler (OnMouseLeave);
			base.KeyDown += new KeyEventHandler (OnKeyDownSB);
			base.MouseDown += new MouseEventHandler (OnMouseDownSB);
			base.MouseUp += new MouseEventHandler (OnMouseUpSB);
			base.MouseMove += new MouseEventHandler (OnMouseMoveSB);
			base.Resize += new EventHandler (OnResizeSB);
			base.TabStop = false;
			base.Cursor = Cursors.Default;

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);
		}

		#region Internal & Private Properties
		internal Rectangle FirstArrowArea {
			get {
				return this.first_arrow_area;
			}

			set {
				this.first_arrow_area = value;
			}
		}

		internal Rectangle SecondArrowArea {
			get {
				return this.second_arrow_area;
			}

			set {
				this.second_arrow_area = value;
			}
		}

		int MaximumAllowed {
			get {
				return use_manual_thumb_size ? maximum - manual_thumb_size + 1 :
					maximum - LargeChange + 1;
			}
		}

		internal Rectangle ThumbPos {
			get {
				return thumb_pos;
			}

			set {
				thumb_pos = value;
			}
		}

		internal bool FirstButtonEntered {
			get { return first_button_entered; }
			private set {
				if (first_button_entered == value)
					return;
				first_button_entered = value;
				if (ThemeEngine.Current.ScrollBarHasHotElementStyles)
					Invalidate (first_arrow_area);
			}
		}

		internal bool SecondButtonEntered {
			get { return second_button_entered; }
			private set {
				if (second_button_entered == value)
					return;
				second_button_entered = value;
				if (ThemeEngine.Current.ScrollBarHasHotElementStyles)
					Invalidate (second_arrow_area);
			}
		}

		internal bool ThumbEntered {
			get { return thumb_entered; }
			private set {
				if (thumb_entered == value)
					return;
				thumb_entered = value;
				if (ThemeEngine.Current.ScrollBarHasHotElementStyles)
					Invalidate (thumb_pos);
			}
		}

		internal bool ThumbPressed {
			get { return thumb_pressed; }
			private set {
				if (thumb_pressed == value)
					return;
				thumb_pressed = value;
				if (ThemeEngine.Current.ScrollBarHasPressedThumbStyle)
					Invalidate (thumb_pos);
			}
		}

		#endregion	// Internal & Private Properties

		#region Public Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set {
				if (base.BackColor == value)
					return;
				base.BackColor = value;
				Refresh ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Image BackgroundImage
		{
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

				base.BackgroundImage = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		protected override CreateParams CreateParams
		{
			get {	return base.CreateParams; }
		}

		protected override Padding DefaultMargin {
			get { return Padding.Empty; }
		}

		protected override ImeMode DefaultImeMode
		{
			get { return ImeMode.Disable; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Font Font
		{
			get { return base.Font; }
			set {
				if (base.Font.Equals (value))
					return;

				base.Font = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Color ForeColor
		{
			get { return base.ForeColor; }
			set {
				if (base.ForeColor == value)
					return;

				base.ForeColor = value;
				Refresh ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new ImeMode ImeMode
		{
			get { return base.ImeMode; }
			set {
				if (base.ImeMode == value)
					return;

				base.ImeMode = value;
			}
		}

		[DefaultValue (10)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Scroll amount when clicking in the scroll area"), MWFCategory("Behaviour")]
		public int LargeChange {
			get { return Math.Min (large_change, maximum - minimum + 1); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("LargeChange", string.Format ("Value '{0}' must be greater than or equal to 0.", value));

				if (large_change != value) {
					large_change = value;

					// thumb area depends on large change value,
					// so we need to recalculate it.
					CalcThumbArea ();
					UpdatePos (Value, true);
					InvalidateDirty ();

					// UIA Framework: Generate UIA Event to indicate LargeChange change
					OnUIAValueChanged (new ScrollEventArgs (ScrollEventType.LargeIncrement, value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
				}
			}
		}

		[DefaultValue (100)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Highest value for scrollbar"), MWFCategory("Behaviour")]
		public int Maximum {
			get { return maximum; }
			set {
				if (maximum == value)
					return;

				maximum = value;

				// UIA Framework: Generate UIA Event to indicate Maximum change
				OnUIAValueChanged (new ScrollEventArgs (ScrollEventType.Last, value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));

				if (maximum < minimum)
					minimum = maximum;
				if (Value > maximum)
					Value = maximum;
					
				// thumb area depends on maximum value,
				// so we need to recalculate it.
				CalcThumbArea ();
				UpdatePos (Value, true);
				InvalidateDirty ();
			}
		}

		internal void SetValues (int maximum, int large_change)
		{
			SetValues (-1, maximum, -1, large_change);
		}

		internal void SetValues (int minimum, int maximum, int small_change, int large_change)
		{
			bool update = false;

			if (-1 != minimum && this.minimum != minimum) {
				this.minimum = minimum;

				if (minimum > this.maximum)
					this.maximum = minimum;
				update = true;

				// change the position if it is out of range now
				position = Math.Max (position, minimum);
			}

			if (-1 != maximum && this.maximum != maximum) {
				this.maximum = maximum;

				if (maximum < this.minimum)
					this.minimum = maximum;
				update = true;

				// change the position if it is out of range now
				position = Math.Min (position, maximum);
			}

			if (-1 != small_change && this.small_change != small_change) {
				this.small_change = small_change;
			}

			if (this.large_change != large_change) {
				this.large_change = large_change;
				update = true;
			}

			if (update) {
				CalcThumbArea ();
				UpdatePos (Value, true);
				InvalidateDirty ();
			}
		}

		[DefaultValue (0)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFDescription("Smallest value for scrollbar"), MWFCategory("Behaviour")]
		public int Minimum {
			get { return minimum; }
			set {
				if (minimum == value)
					return;

				minimum = value;

				// UIA Framework: Generate UIA Event to indicate Minimum change
				OnUIAValueChanged (new ScrollEventArgs (ScrollEventType.First, value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));

				if (minimum > maximum)
					maximum = minimum;

				// thumb area depends on minimum value,
				// so we need to recalculate it.
				CalcThumbArea ();
				UpdatePos (Value, true);
				InvalidateDirty ();
			}
		}

		[DefaultValue (1)]
		[MWFDescription("Scroll amount when clicking scroll arrows"), MWFCategory("Behaviour")]
		public int SmallChange {
			get { return small_change > LargeChange ? LargeChange : small_change; }
			set {
				if ( value < 0 )
					throw new ArgumentOutOfRangeException ("SmallChange", string.Format ("Value '{0}' must be greater than or equal to 0.", value));

				if (small_change != value) {
					small_change = value;
					UpdatePos (Value, true);
					InvalidateDirty ();

					// UIA Framework: Generate UIA Event to indicate SmallChange change
					OnUIAValueChanged (new ScrollEventArgs (ScrollEventType.SmallIncrement, value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
				}
			}
		}

		[DefaultValue (false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			 get { return base.Text;  }
			 set { base.Text = value; }
		}

		[Bindable(true)]
		[DefaultValue (0)]
		[MWFDescription("Current value for scrollbar"), MWFCategory("Behaviour")]
		public int Value {
			get { return position; }
			set {
				if ( value < minimum || value > maximum )
					throw new ArgumentOutOfRangeException ("Value", string.Format ("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				if (position != value){
					position = value;

					OnValueChanged (EventArgs.Empty);

					if (IsHandleCreated) {
						Rectangle thumb_rect = thumb_pos;

						UpdateThumbPos ((vert ? thumb_area.Y : thumb_area.X) + (int)(((float)(position - minimum)) * pixel_per_pos), false, false);

						MoveThumb (thumb_rect, vert ? thumb_pos.Y : thumb_pos.X);
					}
				}
			}
		}

		#endregion //Public Properties

		#region Public Methods
		protected override Rectangle GetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
		{
			// Basically, we want to keep our small edge and scale the long edge
			// ie: if we are vertical, don't scale our width
			if (vert)
				return base.GetScaledBounds (bounds, factor, (specified & BoundsSpecified.Height) | (specified & BoundsSpecified.Location));
			else
				return base.GetScaledBounds (bounds, factor, (specified & BoundsSpecified.Width) | (specified & BoundsSpecified.Location));
		}
		
		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);

			if (Enabled)
				firstbutton_state = secondbutton_state = ButtonState.Normal;
			else
				firstbutton_state = secondbutton_state = ButtonState.Inactive;

			Refresh ();
		}

		protected override void OnHandleCreated (System.EventArgs e)
		{
			base.OnHandleCreated (e);

			CalcButtonSizes ();
			CalcThumbArea ();
			UpdateThumbPos (thumb_area.Y + (int)(((float)(position - minimum)) * pixel_per_pos), true, false);
		}

		protected virtual void OnScroll (ScrollEventArgs se)
		{
			ScrollEventHandler eh = (ScrollEventHandler)(Events [ScrollEvent]);
			if (eh == null)
				return;

			if (se.NewValue < Minimum) {
				se.NewValue = Minimum;
			}

			if (se.NewValue > Maximum) {
				se.NewValue = Maximum;
			}

			eh (this, se);
		}

		private void SendWMScroll(ScrollBarCommands cmd) {
			if ((Parent != null) && Parent.IsHandleCreated) {
				if (vert) {
					XplatUI.SendMessage(Parent.Handle, Msg.WM_VSCROLL, (IntPtr)cmd, implicit_control ? IntPtr.Zero : Handle);
				} else {
					XplatUI.SendMessage(Parent.Handle, Msg.WM_HSCROLL, (IntPtr)cmd, implicit_control ? IntPtr.Zero : Handle);
				}
			}
		}

		protected virtual void OnValueChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		public override string ToString()
		{
			return string.Format("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
						GetType( ).FullName, minimum, maximum, position);
		}

		protected void UpdateScrollInfo ()
		{
			Refresh ();
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion //Public Methods

		#region Private Methods

		private void CalcButtonSizes ()
    		{
    			if (vert) {
				if (Height < ThemeEngine.Current.ScrollBarButtonSize * 2)
					scrollbutton_height = Height /2;
				else
					scrollbutton_height = ThemeEngine.Current.ScrollBarButtonSize;

			} else {
				if (Width < ThemeEngine.Current.ScrollBarButtonSize * 2)
					scrollbutton_width = Width /2;
				else
					scrollbutton_width = ThemeEngine.Current.ScrollBarButtonSize;
			}
		}

		private void CalcThumbArea ()
		{
			int lchange = use_manual_thumb_size ? manual_thumb_size : LargeChange;

			// Thumb area
			if (vert) {

				thumb_area.Height = Height - scrollbutton_height -  scrollbutton_height;
				thumb_area.X = 0;
				thumb_area.Y = scrollbutton_height;
				thumb_area.Width = Width;

				if (Height < thumb_notshown_size)
					thumb_size = 0;
				else {
					double per =  ((double) lchange / (double)((1 + maximum - minimum)));
					thumb_size = 1 + (int) (thumb_area.Height * per);

					if (thumb_size < thumb_min_size)
						thumb_size = thumb_min_size;
						
					// Give the user something to drag if LargeChange is zero
					if (LargeChange == 0)
						thumb_size = 17;
				}

				pixel_per_pos = ((float)(thumb_area.Height - thumb_size) / (float) ((maximum - minimum - lchange) + 1));

			} else	{

				thumb_area.Y = 0;
				thumb_area.X = scrollbutton_width;
				thumb_area.Height = Height;
				thumb_area.Width = Width - scrollbutton_width -  scrollbutton_width;

				if (Width < thumb_notshown_size)
					thumb_size = 0;
				else {
					double per =  ((double) lchange / (double)((1 + maximum - minimum)));
					thumb_size = 1 + (int) (thumb_area.Width * per);

					if (thumb_size < thumb_min_size)
						thumb_size = thumb_min_size;
						
					// Give the user something to drag if LargeChange is zero
					if (LargeChange == 0)
						thumb_size = 17;
				}

				pixel_per_pos = ((float)(thumb_area.Width - thumb_size) / (float) ((maximum - minimum - lchange) + 1));
			}
		}

		private void LargeIncrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Min (MaximumAllowed, position + large_change);

			event_args = new ScrollEventArgs (ScrollEventType.LargeIncrement, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
    			Value = event_args.NewValue;
		
			// UIA Framework event invoked when the "LargeIncrement 
			// Button" is "clicked" either by using the Invoke Pattern
			// or the space between the thumb and the bottom/right button
			OnUIAScroll (new ScrollEventArgs (ScrollEventType.LargeIncrement, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
    		}

    		private void LargeDecrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Max (Minimum, position - large_change);

			event_args = new ScrollEventArgs (ScrollEventType.LargeDecrement, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
    			Value = event_args.NewValue;
    			
			// UIA Framework event invoked when the "LargeDecrement 
			// Button" is "clicked" either by using the Invoke Pattern
			// or the space between the thumb and the top/left button
			OnUIAScroll (new ScrollEventArgs (ScrollEventType.LargeDecrement, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
    		}

    		private void OnResizeSB (Object o, EventArgs e)
    		{
    			if (Width <= 0 || Height <= 0)
    				return;

			CalcButtonSizes ();
			CalcThumbArea ();
			UpdatePos (position, true);

			Refresh ();
    		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawScrollBar (pevent.Graphics, pevent.ClipRectangle, this);
		}

		private void OnTimer (Object source, EventArgs e)
		{
			ClearDirty ();

			switch (timer_type) {

			case TimerType.HoldButton:
				SetRepeatButtonTimer ();
				break;

			case TimerType.RepeatButton:
			{
				if ((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed && position != Minimum) {
					SmallDecrement();
					SendWMScroll(ScrollBarCommands.SB_LINEUP);
				}

				if ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed && position != Maximum) {
					SmallIncrement();
					SendWMScroll(ScrollBarCommands.SB_LINEDOWN);
				}

				break;
			}

			case TimerType.HoldThumbArea:
				SetRepeatThumbAreaTimer ();
				break;

			case TimerType.RepeatThumbArea:
			{
				Point pnt, pnt_screen;
				Rectangle thumb_area_screen = thumb_area;

				pnt_screen = PointToScreen (new Point (thumb_area.X, thumb_area.Y));
				thumb_area_screen.X = pnt_screen.X;
				thumb_area_screen.Y = pnt_screen.Y;

				if (thumb_area_screen.Contains (MousePosition) == false) {
					timer.Enabled = false;
					thumb_moving = ThumbMoving.None;
					DirtyThumbArea ();
					InvalidateDirty ();
				}

				pnt = PointToClient (MousePosition);

				if (vert)
					lastclick_pos = pnt.Y;
				else
					lastclick_pos = pnt.X;

				if (thumb_moving == ThumbMoving.Forward) {
					if ((vert && (thumb_pos.Y + thumb_size > lastclick_pos)) ||
					   (!vert && (thumb_pos.X + thumb_size > lastclick_pos)) ||
					   (thumb_area.Contains (pnt) == false)) {
						timer.Enabled = false;
    						thumb_moving = ThumbMoving.None;
    						Refresh ();
						return;
					} else {
						LargeIncrement ();
						SendWMScroll(ScrollBarCommands.SB_PAGEDOWN);
					}
				} else {
					if ((vert && (thumb_pos.Y < lastclick_pos)) ||
					   (!vert && (thumb_pos.X  < lastclick_pos))){
						timer.Enabled = false;
						thumb_moving = ThumbMoving.None;
						SendWMScroll(ScrollBarCommands.SB_PAGEUP);
						Refresh ();
					} else {
						LargeDecrement ();
						SendWMScroll(ScrollBarCommands.SB_PAGEUP);
					}
				}

				break;
			}
			default:
				break;
			}

			InvalidateDirty ();
		}

		private void MoveThumb (Rectangle original_thumbpos, int value)
		{
			/* so, the reason this works can best be
			 * described by the following 1 dimensional
			 * pictures
			 *
			 * say you have a scrollbar thumb positioned
			 * thusly:
			 *
			 * <---------------------|          |------------------------------>
			 *
			 * and you want it to end up looking like this:
			 *
			 * <-----------------------------|          |---------------------->
			 *
			 * that can be done with the scrolling api by
			 * extending the rectangle to encompass both
			 * positions:
			 *
			 *               start of range          end of range
			 *                       \	    	    /
			 * <---------------------|	    |-------|---------------------->
			 *
			 * so, we end up scrolling just this little region:
			 *
			 *                       |          |-------|
			 *
			 * and end up with       ********|          |
			 *
			 * where ****** is space that is automatically
			 * redrawn.
			 *
			 * It's clear that in both cases (left to
			 * right, right to left) we need to extend the
			 * size of the scroll rectangle to encompass
			 * both.  In the right to left case, we also
			 * need to decrement the X coordinate.
			 *
			 * We call Update after scrolling to make sure
			 * there's no garbage left in the window to be
			 * copied again if we're called before the
			 * paint events have been handled.
			 *
			 */
			int delta;

			if (vert) {
				delta = value - original_thumbpos.Y;

				if (delta < 0) {
					original_thumbpos.Y += delta;
					original_thumbpos.Height -= delta;
				}
				else {
					original_thumbpos.Height += delta;
				}

				XplatUI.ScrollWindow (Handle, original_thumbpos, 0, delta, false);
			}
			else {
				delta = value - original_thumbpos.X;

				if (delta < 0) {
					original_thumbpos.X += delta;
					original_thumbpos.Width -= delta;
				}
				else {
					original_thumbpos.Width += delta;
				}

				XplatUI.ScrollWindow (Handle, original_thumbpos, delta, 0, false);
			}

			Update ();
		}

    		private void OnMouseMoveSB (object sender, MouseEventArgs e)
    		{
			if (Enabled == false)
				return;

			FirstButtonEntered = first_arrow_area.Contains (e.Location);
			SecondButtonEntered = second_arrow_area.Contains (e.Location);

			if (thumb_size == 0)
				return;
			
			ThumbEntered = thumb_pos.Contains (e.Location);

			if (firstbutton_pressed) {
    				if (!first_arrow_area.Contains (e.X, e.Y) && ((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {
					firstbutton_state = ButtonState.Normal;
					Invalidate (first_arrow_area);
					Update();
					return;
				} else if (first_arrow_area.Contains (e.X, e.Y) && ((firstbutton_state & ButtonState.Normal) == ButtonState.Normal)) {
					firstbutton_state = ButtonState.Pushed;
					Invalidate (first_arrow_area);
					Update();
					return;
				}
			} else if (secondbutton_pressed) {
				if (!second_arrow_area.Contains (e.X, e.Y) && ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {
					secondbutton_state = ButtonState.Normal;
					Invalidate (second_arrow_area);
					Update();
					return;
				} else if (second_arrow_area.Contains (e.X, e.Y) && ((secondbutton_state & ButtonState.Normal) == ButtonState.Normal)) {
					secondbutton_state = ButtonState.Pushed;
					Invalidate (second_arrow_area);
					Update();
					return;
				}
			} else if (thumb_pressed == true) {
				if (vert) {
					int thumb_edge = e.Y - thumbclick_offset;

					if (thumb_edge < thumb_area.Y)
						thumb_edge = thumb_area.Y;
					else if (thumb_edge > thumb_area.Bottom - thumb_size)
						thumb_edge = thumb_area.Bottom - thumb_size;

					if (thumb_edge != thumb_pos.Y) {
						Rectangle thumb_rect = thumb_pos;

						UpdateThumbPos (thumb_edge, false, true);

						MoveThumb (thumb_rect, thumb_pos.Y);

						OnScroll (new ScrollEventArgs (ScrollEventType.ThumbTrack, position, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
					}
					SendWMScroll(ScrollBarCommands.SB_THUMBTRACK);
				} else {
					int thumb_edge = e.X - thumbclick_offset;

					if (thumb_edge < thumb_area.X)
						thumb_edge = thumb_area.X;
					else if (thumb_edge > thumb_area.Right - thumb_size)
						thumb_edge = thumb_area.Right - thumb_size;

					if (thumb_edge != thumb_pos.X) {
						Rectangle thumb_rect = thumb_pos;

						UpdateThumbPos (thumb_edge, false, true);

						MoveThumb (thumb_rect, thumb_pos.X);

						OnScroll (new ScrollEventArgs (ScrollEventType.ThumbTrack, position, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
					}
					SendWMScroll(ScrollBarCommands.SB_THUMBTRACK);
				}

			}

    		}

    		private void OnMouseDownSB (object sender, MouseEventArgs e)
    		{
			ClearDirty ();

			if (Enabled == false || (e.Button & MouseButtons.Left) == 0)
				return;

    			if (firstbutton_state != ButtonState.Inactive && first_arrow_area.Contains (e.X, e.Y)) {
				SendWMScroll(ScrollBarCommands.SB_LINEUP);
				firstbutton_state = ButtonState.Pushed;
				firstbutton_pressed = true;
				Invalidate (first_arrow_area);
				Update();
				if (!timer.Enabled) {
					SetHoldButtonClickTimer ();
					timer.Enabled = true;
				}
			}

			if (secondbutton_state != ButtonState.Inactive && second_arrow_area.Contains (e.X, e.Y)) {
				SendWMScroll(ScrollBarCommands.SB_LINEDOWN);
				secondbutton_state = ButtonState.Pushed;
				secondbutton_pressed = true;
				Invalidate (second_arrow_area);
				Update();
				if (!timer.Enabled) {
					SetHoldButtonClickTimer ();
					timer.Enabled = true;
				}
			}

			if (thumb_size > 0 && thumb_pos.Contains (e.X, e.Y)) {
				ThumbPressed = true;
				SendWMScroll(ScrollBarCommands.SB_THUMBTRACK);
				if (vert) {
					thumbclick_offset = e.Y - thumb_pos.Y;
					lastclick_pos = e.Y;
				}
				else {
					thumbclick_offset = e.X - thumb_pos.X;
					lastclick_pos = e.X;
				}
			} else {
				if (thumb_size > 0 && thumb_area.Contains (e.X, e.Y)) {

					if (vert) {
						lastclick_pos = e.Y;

						if (e.Y > thumb_pos.Y + thumb_pos.Height) {
							SendWMScroll(ScrollBarCommands.SB_PAGEDOWN);
							LargeIncrement ();
							thumb_moving = ThumbMoving.Forward;
							Dirty (new Rectangle (0, thumb_pos.Y + thumb_pos.Height,
										      ClientRectangle.Width,
										      ClientRectangle.Height -	(thumb_pos.Y + thumb_pos.Height) -
										      scrollbutton_height));
						} else {
							SendWMScroll(ScrollBarCommands.SB_PAGEUP);
							LargeDecrement ();
							thumb_moving = ThumbMoving.Backwards;
							Dirty (new Rectangle (0,  scrollbutton_height,
										      ClientRectangle.Width,
										      thumb_pos.Y - scrollbutton_height));
						}
					} else {

						lastclick_pos = e.X;

						if (e.X > thumb_pos.X + thumb_pos.Width) {
							SendWMScroll(ScrollBarCommands.SB_PAGEDOWN);
							thumb_moving = ThumbMoving.Forward;
							LargeIncrement ();
							Dirty (new Rectangle (thumb_pos.X + thumb_pos.Width, 0,
										      ClientRectangle.Width -  (thumb_pos.X + thumb_pos.Width) -
										      scrollbutton_width,
										      ClientRectangle.Height));
						} else {
							SendWMScroll(ScrollBarCommands.SB_PAGEUP);
							thumb_moving = ThumbMoving.Backwards;
							LargeDecrement ();
							Dirty (new Rectangle (scrollbutton_width,  0,
										      thumb_pos.X - scrollbutton_width,
										      ClientRectangle.Height));
						}
					}

					SetHoldThumbAreaTimer ();
					timer.Enabled = true;
					InvalidateDirty ();
				}
			}
    		}

    		private void OnMouseUpSB (object sender, MouseEventArgs e)
    		{
			ClearDirty ();

			if (Enabled == false)
				return;

    			timer.Enabled = false;
    			if (thumb_moving != ThumbMoving.None) {
				DirtyThumbArea ();
    				thumb_moving = ThumbMoving.None;
    			}

			if (firstbutton_pressed) {
				firstbutton_state = ButtonState.Normal;
				if (first_arrow_area.Contains (e.X, e.Y)) {
					SmallDecrement ();
				}
				SendWMScroll(ScrollBarCommands.SB_LINEUP);
				firstbutton_pressed = false;
				Dirty (first_arrow_area);
			} else if (secondbutton_pressed) {
				secondbutton_state = ButtonState.Normal;
				if (second_arrow_area.Contains (e.X, e.Y)) {
					SmallIncrement ();
				}
				SendWMScroll(ScrollBarCommands.SB_LINEDOWN);
				Dirty (second_arrow_area);
				secondbutton_pressed = false;
			} else if (thumb_pressed == true) {
				OnScroll (new ScrollEventArgs (ScrollEventType.ThumbPosition, position, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
				OnScroll (new ScrollEventArgs (ScrollEventType.EndScroll, position, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
				SendWMScroll(ScrollBarCommands.SB_THUMBPOSITION);
				ThumbPressed = false;
				return;
			}

			InvalidateDirty ();
    		}

    		private void OnKeyDownSB (Object o, KeyEventArgs key)
		{
			if (Enabled == false)
				return;

			ClearDirty ();

			switch (key.KeyCode){
			case Keys.Up:
			{
				SmallDecrement ();
				break;
			}
			case Keys.Down:
			{
				SmallIncrement ();
				break;
			}
			case Keys.PageUp:
			{
				LargeDecrement ();
				break;
			}
			case Keys.PageDown:
			{
				LargeIncrement ();
				break;
			}
			case Keys.Home:
			{
				SetHomePosition ();
				break;
			}
			case Keys.End:
			{
				SetEndPosition ();
				break;
			}
			default:
				break;
			}

			InvalidateDirty ();
		}

		// I hate to do this, but we don't have the resources to track
		// down everything internal that is setting a value outside the
		// correct range, so we'll clamp it to the acceptable values.
		internal void SafeValueSet (int value)
		{
			value = Math.Min (value, maximum);
			value = Math.Max (value, minimum);
			
			Value = value;
		}
		
		private void SetEndPosition ()
		{
			ScrollEventArgs event_args;
    			int pos = MaximumAllowed;

			event_args = new ScrollEventArgs (ScrollEventType.Last, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
    			pos = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
    			pos = event_args.NewValue;

			SetValue (pos);
		}

		private void SetHomePosition ()
		{
			ScrollEventArgs event_args;
    			int pos = Minimum;

			event_args = new ScrollEventArgs (ScrollEventType.First, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
    			pos = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
			pos = event_args.NewValue;

			SetValue (pos);
		}

    		private void SmallIncrement ()
    		{
    			ScrollEventArgs event_args;
    			int pos = Math.Min (MaximumAllowed, position + SmallChange);

			event_args = new ScrollEventArgs (ScrollEventType.SmallIncrement, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
			Value = event_args.NewValue;
			
			// UIA Framework event invoked when the "SmallIncrement 
			// Button" (a.k.a bottom/right button) is "clicked" either
			// by using the Invoke Pattern or the button itself
			OnUIAScroll (new ScrollEventArgs (ScrollEventType.SmallIncrement, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
    		}

    		private void SmallDecrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Max (Minimum, position - SmallChange);

			event_args = new ScrollEventArgs (ScrollEventType.SmallDecrement, pos, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll));
			OnScroll (event_args);
			Value = event_args.NewValue;
			
			// UIA Framework event invoked when the "SmallDecrement 
			// Button" (a.k.a top/left button) is "clicked" either
			// by using the Invoke Pattern or the button itself
			OnUIAScroll (new ScrollEventArgs (ScrollEventType.SmallDecrement, Value, (vert ? ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll)));
    		}

    		private void SetHoldButtonClickTimer ()
		{
			timer.Enabled = false;
			timer.Interval = 200;
			timer_type = TimerType.HoldButton;
			timer.Enabled = true;
		}

		private void SetRepeatButtonTimer ()
		{
			timer.Enabled = false;
			timer.Interval = 50;
			timer_type = TimerType.RepeatButton;
			timer.Enabled = true;
		}

		private void SetHoldThumbAreaTimer ()
		{
			timer.Enabled = false;
			timer.Interval = 200;
			timer_type = TimerType.HoldThumbArea;
			timer.Enabled = true;
		}

		private void SetRepeatThumbAreaTimer ()
		{
			timer.Enabled = false;
			timer.Interval = 50;
			timer_type = TimerType.RepeatThumbArea;
			timer.Enabled = true;
		}

    		private void UpdatePos (int newPos, bool update_thumbpos)
    		{
			int pos;

    			if (newPos < minimum)
    				pos = minimum;
    			else
    				if (newPos > MaximumAllowed)
    					pos = MaximumAllowed;
				else
					pos = newPos;

			// pos can't be less than minimum or greater than maximum
			if (pos < minimum)
				pos = minimum;
			if (pos > maximum)
				pos = maximum;

			if (update_thumbpos) {
				if (vert)
					UpdateThumbPos (thumb_area.Y + (int)(((float)(pos - minimum)) * pixel_per_pos), true, false);
				else
					UpdateThumbPos (thumb_area.X + (int)(((float)(pos - minimum)) * pixel_per_pos), true, false);
				SetValue (pos);
			}
			else {
				position = pos; // Updates directly the value to avoid thumb pos update


				// XXX some reason we don't call OnValueChanged?
				EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
				if (eh != null)
					eh (this, EventArgs.Empty);
			}
    		}

    		private void UpdateThumbPos (int pixel, bool dirty, bool update_value)
    		{
    			float new_pos = 0;

    			if (vert) {
				if (dirty)
					Dirty (thumb_pos);
	    			if (pixel < thumb_area.Y)
	    				thumb_pos.Y = thumb_area.Y;
	    			else if (pixel > thumb_area.Bottom - thumb_size)
	    				thumb_pos.Y = thumb_area.Bottom - thumb_size;
	    			else
	    				thumb_pos.Y = pixel;

				thumb_pos.X = 0;
				thumb_pos.Width = ThemeEngine.Current.ScrollBarButtonSize;
				thumb_pos.Height = thumb_size;
				new_pos = (float) (thumb_pos.Y - thumb_area.Y);
				new_pos = new_pos / pixel_per_pos;
				if (dirty)
					Dirty (thumb_pos);
			} else	{
				if (dirty)
					Dirty (thumb_pos);
				if (pixel < thumb_area.X)
	    				thumb_pos.X = thumb_area.X;
	    			else if (pixel > thumb_area.Right - thumb_size)
	    				thumb_pos.X = thumb_area.Right - thumb_size;
	    			else
	    				thumb_pos.X = pixel;

				thumb_pos.Y = 0;
				thumb_pos.Width =  thumb_size;
				thumb_pos.Height = ThemeEngine.Current.ScrollBarButtonSize;
				new_pos = (float) (thumb_pos.X - thumb_area.X);
				new_pos = new_pos / pixel_per_pos;

				if (dirty)
					Dirty (thumb_pos);
			}

			if (update_value)
				UpdatePos ((int) new_pos + minimum, false);
    		}

		private void SetValue (int value)
		{
			if ( value < minimum || value > maximum )
				throw new ArgumentException(
					String.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

			if (position != value){
				position = value;

				OnValueChanged (EventArgs.Empty);
				UpdatePos (value, true);
			}
		}

		private void ClearDirty ()
		{
			dirty = Rectangle.Empty;
		}

		private void Dirty (Rectangle r)
		{
			if (dirty == Rectangle.Empty) {
				dirty = r;
				return;
			}
			dirty = Rectangle.Union (dirty, r);
		}

		private void DirtyThumbArea ()
		{
			if (thumb_moving == ThumbMoving.Forward) {
				if (vert) {
					Dirty (new Rectangle (0, thumb_pos.Y + thumb_pos.Height,
								      ClientRectangle.Width,
								      ClientRectangle.Height -	(thumb_pos.Y + thumb_pos.Height) -
								      scrollbutton_height));
				} else {
					Dirty (new Rectangle (thumb_pos.X + thumb_pos.Width, 0,
								      ClientRectangle.Width -  (thumb_pos.X + thumb_pos.Width) -
								      scrollbutton_width,
								      ClientRectangle.Height));
				}
			} else if (thumb_moving == ThumbMoving.Backwards) {
				if (vert) {
					Dirty(new Rectangle (0,	 scrollbutton_height,
								      ClientRectangle.Width,
								      thumb_pos.Y - scrollbutton_height));
				} else {
					Dirty (new Rectangle (scrollbutton_width,  0,
								      thumb_pos.X - scrollbutton_width,
								      ClientRectangle.Height));
				}
			}
		}

		private void InvalidateDirty ()
		{
			Invalidate (dirty);
			Update();
			dirty = Rectangle.Empty;
		}

		void OnMouseEnter (object sender, EventArgs e)
		{
			if (ThemeEngine.Current.ScrollBarHasHoverArrowButtonStyle) {
				Region region_to_invalidate = new Region (first_arrow_area);
				region_to_invalidate.Union (second_arrow_area);
				Invalidate (region_to_invalidate);
			}
		}

		void OnMouseLeave (object sender, EventArgs e)
		{
			Region region_to_invalidate = new Region ();
			region_to_invalidate.MakeEmpty ();
			bool dirty = false;
			if (ThemeEngine.Current.ScrollBarHasHoverArrowButtonStyle) {
				region_to_invalidate.Union (first_arrow_area);
				region_to_invalidate.Union (second_arrow_area);
				dirty = true;
			} else
				if (ThemeEngine.Current.ScrollBarHasHotElementStyles)
					if (first_button_entered) {
						region_to_invalidate.Union (first_arrow_area);
						dirty = true;
					} else if (second_button_entered) {
						region_to_invalidate.Union (second_arrow_area);
						dirty = true;
					}
			if (ThemeEngine.Current.ScrollBarHasHotElementStyles)
				if (thumb_entered) {
					region_to_invalidate.Union (thumb_pos);
					dirty = true;
				}
			first_button_entered = false;
			second_button_entered = false;
			thumb_entered = false;
			if (dirty)
				Invalidate (region_to_invalidate);
			region_to_invalidate.Dispose ();
		}
		#endregion //Private Methods
		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
		}

		#region UIA Framework Section: Events, Methods and Properties.

		//NOTE:
		//	We are using Reflection to add/remove internal events.
		//	Class ScrollBarButtonInvokePatternInvokeEvent uses the events.
		//
    		// Types used to generate UIA InvokedEvent
		// * args.Type = ScrollEventType.LargeIncrement. Space between Thumb and bottom/right Button
		// * args.Type = ScrollEventType.LargeDecrement. Space between Thumb and top/left Button
		// * args.Type = ScrollEventType.SmallIncrement. Small increment UIA Button (bottom/right Button)
    		// * args.Type = ScrollEventType.SmallDecrement. Small decrement UIA Button (top/left Button)
		// Types used to generate RangeValue-related events
		// * args.Type = ScrollEventType.LargeIncrement. LargeChange event
		// * args.Type = ScrollEventType.Last. Maximum event
		// * args.Type = ScrollEventType.First. Minimum event
		// * args.Type = ScrollEventType.SmallIncrement. SmallChange event
		static object UIAScrollEvent = new object ();
		static object UIAValueChangeEvent = new object ();

		internal event ScrollEventHandler UIAScroll {
			add { Events.AddHandler (UIAScrollEvent, value); }
			remove { Events.RemoveHandler (UIAScrollEvent, value); }
		}

		internal event ScrollEventHandler UIAValueChanged {
			add { Events.AddHandler (UIAValueChangeEvent, value); }
			remove { Events.RemoveHandler (UIAValueChangeEvent, value); }
		}

		internal void OnUIAScroll (ScrollEventArgs args)
		{
			ScrollEventHandler eh = (ScrollEventHandler) Events [UIAScrollEvent];
			if (eh != null)
				eh (this, args);
		}

		internal void OnUIAValueChanged (ScrollEventArgs args)
		{
			ScrollEventHandler eh = (ScrollEventHandler) Events [UIAValueChangeEvent];
			if (eh != null)
				eh (this, args);
		}

		//NOTE:
		//	Wrapper methods used by the Reflection.
		//	Class ScrollBarButtonInvokeProviderBehavior uses the events.
		//
		internal void UIALargeIncrement ()
		{
			LargeIncrement ();
		}

		internal void UIALargeDecrement ()
		{
			LargeDecrement ();
		}

		internal void UIASmallIncrement ()
		{
			SmallIncrement ();
		}

		internal void UIASmallDecrement ()
		{
			SmallDecrement ();
		}

		internal Rectangle UIAThumbArea {
			get { return thumb_area; }
		}

		internal Rectangle UIAThumbPosition {
			get { return thumb_pos; }
		}

		#endregion UIA Framework Section: Events, Methods and Properties.

	 }
}



