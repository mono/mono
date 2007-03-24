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
		internal bool vert;
		internal bool implicit_control;
		private int lastclick_pos;		// Position of the last button-down event
		private int thumbclick_offset;		// Position of the last button-down event relative to the thumb edge
		private Rectangle dirty;

		internal ThumbMoving thumb_moving = ThumbMoving.None;
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
			base.KeyDown += new KeyEventHandler (OnKeyDownSB);
			base.MouseDown += new MouseEventHandler (OnMouseDownSB);
			base.MouseUp += new MouseEventHandler (OnMouseUpSB);
			base.MouseMove += new MouseEventHandler (OnMouseMoveSB);
			base.Resize += new EventHandler (OnResizeSB);
			base.TabStop = false;
			base.Cursor = Cursors.Default;

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);
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

		internal Rectangle ThumbPos {
			get {
				return thumb_pos;
			}

			set {
				thumb_pos = value;
			}
		}
		#endregion	// Internal & Private Properties

		#region Public Properties

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

		protected override CreateParams CreateParams
		{
			get {	return base.CreateParams; }
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
			get {
				if (large_change > maximum)
					return (maximum + 1);
				else
					return large_change;
			}
			set {
				if (value < 0)
#if NET_2_0
					throw new ArgumentOutOfRangeException ("LargeChange", string.Format ("Value '{0}' must be greater than or equal to 0.", value));
#else
					throw new ArgumentException( string.Format("Value '{0}' must be greater than or equal to 0.", value));
#endif

				if (large_change != value) {
					large_change = value;

					// thumb area depends on large change value,
					// so we need to recalculate it.
					CalcThumbArea ();
					UpdatePos (Value, true);
					InvalidateDirty ();
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

				if (maximum < minimum)
					minimum = maximum;

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
			}

			if (-1 != maximum && this.maximum != maximum) {
				this.maximum = maximum;

				if (maximum < this.minimum)
					this.minimum = maximum;
				update = true;
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
			get { return small_change; }
			set {
				if ( value < 0 )
#if NET_2_0
					throw new ArgumentOutOfRangeException ("SmallChange", string.Format ("Value '{0}' must be greater than or equal to 0.", value));
#else
					throw new ArgumentException( string.Format("Value '{0}' must be greater than or equal to 0.", value));
#endif

				if (small_change != value) {
					small_change = value;
					UpdatePos (Value, true);
					InvalidateDirty ();
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
#if NET_2_0
					throw new ArgumentOutOfRangeException ("Value", string.Format ("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
#else
					throw new ArgumentException (string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
#endif					

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

		protected virtual void OnScroll (ScrollEventArgs event_args)
		{
			ScrollEventHandler eh = (ScrollEventHandler)(Events [ScrollEvent]);
			if (eh == null)
				return;

			if (event_args.NewValue < Minimum) {
				event_args.NewValue = Minimum;
			}

			if (event_args.NewValue > Maximum) {
				event_args.NewValue = Maximum;
			}

			eh (this, event_args);
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
						GetType( ).FullName.ToString( ), minimum, maximum, position);
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
			// Thumb area
			if (vert) {

				thumb_area.Height = Height - scrollbutton_height -  scrollbutton_height;
				thumb_area.X = 0;
				thumb_area.Y = scrollbutton_height;
				thumb_area.Width = Width;

				if (Height < thumb_notshown_size)
					thumb_size = 0;
				else {
					double per =  ((double) this.LargeChange / (double)((1 + maximum - minimum)));
					thumb_size = 1 + (int) (thumb_area.Height * per);

					if (thumb_size < thumb_min_size)
						thumb_size = thumb_min_size;
				}

				pixel_per_pos = ((float)(thumb_area.Height - thumb_size) / (float) ((maximum - minimum - this.LargeChange) + 1));

			} else	{

				thumb_area.Y = 0;
				thumb_area.X = scrollbutton_width;
				thumb_area.Height = Height;
				thumb_area.Width = Width - scrollbutton_width -  scrollbutton_width;

				if (Width < thumb_notshown_size)
					thumb_size = 0;
				else {
					double per =  ((double) this.LargeChange / (double)((1 + maximum - minimum)));
					thumb_size = 1 + (int) (thumb_area.Width * per);

					if (thumb_size < thumb_min_size)
						thumb_size = thumb_min_size;
				}

				pixel_per_pos = ((float)(thumb_area.Width - thumb_size) / (float) ((maximum - minimum - this.LargeChange) + 1));
			}
		}

		private void LargeIncrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Min (Maximum - large_change + 1, position + large_change);

    			event_args = new ScrollEventArgs (ScrollEventType.LargeIncrement, pos);
    			OnScroll (event_args);
			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value);
			OnScroll (event_args);
    			Value = event_args.NewValue;
    		}

    		private void LargeDecrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Max (Minimum, position - large_change);

    			event_args = new ScrollEventArgs (ScrollEventType.LargeDecrement, pos);
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value);
			OnScroll (event_args);
    			Value = event_args.NewValue;
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
			if (Enabled == false || thumb_size == 0)
				return;

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

						OnScroll (new ScrollEventArgs (ScrollEventType.ThumbTrack, position));
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

						OnScroll (new ScrollEventArgs (ScrollEventType.ThumbTrack, position));
					}
					SendWMScroll(ScrollBarCommands.SB_THUMBTRACK);
				}

			}

    		}

    		private void OnMouseDownSB (object sender, MouseEventArgs e)
    		{
			ClearDirty ();

			if (Enabled == false)
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
				thumb_pressed = true;
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
				OnScroll (new ScrollEventArgs (ScrollEventType.ThumbPosition, position));
				OnScroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
				SendWMScroll(ScrollBarCommands.SB_THUMBPOSITION);
				thumb_pressed = false;
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

		private void SetEndPosition ()
		{
			ScrollEventArgs event_args;
    			int pos = Maximum - large_change + 1;

    			event_args = new ScrollEventArgs (ScrollEventType.Last, pos);
    			OnScroll (event_args);
    			pos = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, pos);
			OnScroll (event_args);
    			pos = event_args.NewValue;

			SetValue (pos);
		}

		private void SetHomePosition ()
		{
			ScrollEventArgs event_args;
    			int pos = Minimum;

    			event_args = new ScrollEventArgs (ScrollEventType.First, pos);
    			OnScroll (event_args);
    			pos = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, pos);
			OnScroll (event_args);
			pos = event_args.NewValue;

			SetValue (pos);
		}

    		private void SmallIncrement ()
    		{
    			ScrollEventArgs event_args;
    			int pos = Math.Min (Maximum - large_change + 1, position + small_change);

    			event_args = new ScrollEventArgs (ScrollEventType.SmallIncrement, pos);
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value);
			OnScroll (event_args);
			Value = event_args.NewValue;
    		}

    		private void SmallDecrement ()
    		{
			ScrollEventArgs event_args;
    			int pos = Math.Max (Minimum, position - small_change);

    			event_args = new ScrollEventArgs (ScrollEventType.SmallDecrement, pos);
    			OnScroll (event_args);
    			Value = event_args.NewValue;

			event_args = new ScrollEventArgs (ScrollEventType.EndScroll, Value);
			OnScroll (event_args);
			Value = event_args.NewValue;
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
    				if (newPos > maximum + 1 - large_change)
    					pos = maximum + 1 - large_change;
				else
					pos = newPos;

			// pos can't be less than minimum
			if (pos < minimum)
				pos = minimum;

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

		#endregion //Private Methods
#if NET_2_0
		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
		}
#endif
	 }
}


