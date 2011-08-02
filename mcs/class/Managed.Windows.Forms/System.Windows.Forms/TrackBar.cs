//
// System.Windows.Forms.TrackBar.cs
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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Rolf Bjarne Kvinge, RKvinge@novell.com
// 
// TODO:
//		- The AutoSize functionality seems quite broken for vertical controls in .Net 1.1. Not
//		sure if we are implementing it the right way.
//

// NOT COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Timers;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultBindingProperty ("Value")]
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer("System.Windows.Forms.Design.TrackBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultEvent ("Scroll")]
	[DefaultProperty("Value")]
	public class TrackBar : Control, ISupportInitialize
	{
		private int minimum;
		private int maximum;
		internal int tickFrequency;
		private bool autosize;
		private int position;
		private int smallChange;
		private int largeChange;
		private Orientation orientation;
		private TickStyle tickStyle;		
		private Rectangle thumb_pos = new Rectangle ();	 /* Current position and size of the thumb */
		private Rectangle thumb_area = new Rectangle (); /* Area where the thumb can scroll */
		internal bool thumb_pressed = false;		 
		private System.Timers.Timer holdclick_timer = new System.Timers.Timer ();
		internal int thumb_mouseclick;		
		private bool mouse_clickmove;
		private bool is_moving_right; // which way the thumb should move when mouse is down (right=up, left=down) 
		internal int mouse_down_x_offset; // how far from left side of thumb was the mouse clicked.
		internal bool mouse_moved; // has the mouse moved since it was clicked?
		private const int size_of_autosize = 45;
		private bool right_to_left_layout;
		bool thumb_entered;
	
		#region events
		[EditorBrowsable (EditorBrowsableState.Always)]
		[Browsable (true)]
		public new event EventHandler AutoSizeChanged {
			add {base.AutoSizeChanged += value;}
			remove {base.AutoSizeChanged -= value;}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
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
			add {base.MouseClick += value;}
			remove {base.MouseClick -= value;}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick
		{
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged
		{
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler RightToLeftLayoutChanged {
			add {Events.AddHandler (RightToLeftLayoutChangedEvent, value);}
			remove {Events.RemoveHandler (RightToLeftLayoutChangedEvent, value);}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		static object RightToLeftLayoutChangedEvent = new object ();
		static object ScrollEvent = new object ();
		static object ValueChangedEvent = new object ();

		public event EventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		public event EventHandler ValueChanged {
			add { Events.AddHandler (ValueChangedEvent, value); }
			remove { Events.RemoveHandler (ValueChangedEvent, value); }
		}
		
		#endregion // Events

		#region UIA FrameWork Events
		static object UIAValueParamChangedEvent = new object ();

		internal event EventHandler UIAValueParamChanged {
			add { Events.AddHandler (UIAValueParamChangedEvent, value); }
			remove { Events.RemoveHandler (UIAValueParamChangedEvent, value); }
		}

		internal void OnUIAValueParamChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAValueParamChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		#endregion

		public TrackBar ()
		{
			orientation = Orientation.Horizontal;
			minimum = 0;
			maximum = 10;
			tickFrequency = 1;
			autosize = true;
			position = 0;
			tickStyle = TickStyle.BottomRight;
			smallChange = 1;
			largeChange = 5;			
			mouse_clickmove = false;			
			MouseDown += new MouseEventHandler (OnMouseDownTB); 
			MouseUp += new MouseEventHandler (OnMouseUpTB); 
			MouseMove += new MouseEventHandler (OnMouseMoveTB);
			MouseLeave += new EventHandler (OnMouseLeave);
			KeyDown += new KeyEventHandler (OnKeyDownTB);
			LostFocus += new EventHandler (OnLostFocusTB);
			GotFocus += new EventHandler (OnGotFocusTB);
			holdclick_timer.Elapsed += new ElapsedEventHandler (OnFirstClickTimer);

			SetStyle (ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.UseTextForAccessibility, false);
		}

		#region Private & Internal Properties
		internal Rectangle ThumbPos {
			get {
				return thumb_pos;
			}

			set {
				thumb_pos = value;
			}
		}

		internal Rectangle ThumbArea {
			get {
				return thumb_area;
			}

			set {
				thumb_area = value;
			}
		}

		internal bool ThumbEntered {
			get { return thumb_entered; }
			set {
				if (thumb_entered == value)
					return;
				thumb_entered = value;
				if (ThemeEngine.Current.TrackBarHasHotThumbStyle)
					Invalidate (GetRealThumbRectangle ());
			}
		}
		#endregion	// Private & Internal Properties

		#region Public Properties

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[DefaultValue (true)]
		public override bool AutoSize {
			get { return autosize; }
			set { autosize = value;}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
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

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.TrackBarDefaultSize; }
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

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}		

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}
		
		[DefaultValue (5)]
		public int LargeChange 
		{
			get { return largeChange; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (string.Format ("Value '{0}' must be greater than or equal to 0.", value));
				
				largeChange = value;				

				OnUIAValueParamChanged ();
			}
		}

		[DefaultValue (10)]
		[RefreshProperties (RefreshProperties.All)]		
		public int Maximum {
			get { return maximum; }
			set {
				if (maximum != value)  {
					maximum = value;

					if (maximum < minimum)
						minimum = maximum;

					Refresh ();

					OnUIAValueParamChanged ();
				}
			}
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.All)]		
		public int Minimum {
			get { return minimum; }
			set {

				if (Minimum != value) {
					minimum = value;

					if (minimum > maximum)
						maximum = minimum;

					Refresh ();

					OnUIAValueParamChanged ();
				}
			}
		}

		[DefaultValue (Orientation.Horizontal)]
		[Localizable (true)]
		public Orientation Orientation {
			get { return orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for Orientation", value));

				/* Orientation can be changed once the control has been created */
				if (orientation != value) {
					orientation = value;
					
					if (this.IsHandleCreated) {
						Size = new Size (Height, Width);
						Refresh (); 
					}
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Padding Padding {
			get {
				return base.Padding;
			}
			set {
				base.Padding = value;
			}
		}
		
		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeftLayout {
			get {
				return right_to_left_layout;
			}
			set {
				if (value != right_to_left_layout) {
					right_to_left_layout = value;
					OnRightToLeftLayoutChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue (1)]
		public int SmallChange {
			get { return smallChange;}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (string.Format ("Value '{0}' must be greater than or equal to 0.", value));

				if (smallChange != value) {
					smallChange = value;					

					OnUIAValueParamChanged ();
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		[Browsable (false)]
		public override string Text {
			get {	return base.Text; }			
			set { base.Text = value; }
		}

		[DefaultValue (1)]
		public int TickFrequency {
			get { return tickFrequency; }
			set {
				if ( value > 0 ) {
					tickFrequency = value;
					Refresh ();
				}
			}
		}

		[DefaultValue (TickStyle.BottomRight)]
		public TickStyle TickStyle {
			get { return tickStyle; }
			set { 				
				if (!Enum.IsDefined (typeof (TickStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for TickStyle", value));
				
				if (tickStyle != value) {
					tickStyle = value;
					Refresh ();
				}
			}
		}
		
		[DefaultValue (0)]
		[Bindable (true)]
		public int Value {
			get { return position; }
			set {
				SetValue (value, false);
			}
		}

		void SetValue (int value, bool fire_onscroll)
		{
			if (value < Minimum || value > Maximum)
				throw new ArgumentException(
						String.Format ("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

			if (position == value)
				return;

			position = value;

			// OnScroll goes before OnValueChanged
			if (fire_onscroll)
				OnScroll (EventArgs.Empty);

			// XXX any reason we don't call OnValueChanged here?
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);

			Invalidate (thumb_area);
		}

		#endregion //Public Properties

		#region Public Methods

		public void BeginInit ()		
		{

		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void SetBoundsCore (int x, int y,int width, int height, BoundsSpecified specified)
		{
			if (AutoSize) {
				if (orientation == Orientation.Vertical) {
					width = size_of_autosize;
				} else {
					height = size_of_autosize;
				}
			}
			base.SetBoundsCore (x, y, width, height, specified);
		}

		public void EndInit ()		
		{

		}

		protected override bool IsInputKey (Keys keyData)
		{
			if ((keyData & Keys.Alt) == 0) {
				switch (keyData & Keys.KeyCode) {
				case Keys.Down:
				case Keys.Right:
				case Keys.Up:
				case Keys.Left:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
					return true;
				}
			}
			return base.IsInputKey (keyData);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{	
			base.OnHandleCreated (e);
					
			if (AutoSize)
				if (Orientation == Orientation.Horizontal)
					Size = new Size (Width, 40);
				else
					Size = new Size (50, Height);
			
			UpdatePos (Value, true);			
		}
	
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
			
			if (!Enabled) return;
    			
			if (e.Delta > 0)
				SmallDecrement ();
			else
				SmallIncrement ();    					
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [RightToLeftLayoutChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnScroll (EventArgs e) 
		{
			EventHandler eh = (EventHandler)(Events [ScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e)
		{
			base.OnSystemColorsChanged (e);
			Invalidate ();
		}

		protected virtual void OnValueChanged (EventArgs e) 
		{
			EventHandler eh = (EventHandler)(Events [ValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		public void SetRange (int minValue, int maxValue)
		{
			Minimum = minValue;
			Maximum = maxValue;			
		}

		public override string ToString()
		{
			return string.Format("System.Windows.Forms.TrackBar, Minimum: {0}, Maximum: {1}, Value: {2}",
						Minimum, Maximum, Value);
		}
							

		protected override void WndProc (ref Message m)
    		{
			base.WndProc (ref m);

			// Basically we want ControlStyles.ResizeRedraw but
			// tests say we can't set that flag
			if ((Msg)m.Msg == Msg.WM_WINDOWPOSCHANGED && Visible)
				Invalidate ();
    		}
    		
		#endregion Public Methods

		#region Private Methods
		
		private void UpdatePos (int newPos, bool update_trumbpos)
		{
			if (newPos < minimum){
				SetValue (minimum, true);
			}
			else {
				if (newPos > maximum) {
					SetValue (maximum, true);
				}
				else {
					SetValue (newPos, true);
				}
			}
		}
		
			// Used by UIA implementation, so making internal
		internal void LargeIncrement ()
    		{    			
			UpdatePos (position + LargeChange, true);
			Invalidate (thumb_area);
    		}

			// Used by UIA implementation, so making internal
    		internal void LargeDecrement ()
    		{
			UpdatePos (position - LargeChange, true);
			Invalidate (thumb_area);
    		}

		private void SmallIncrement ()
    		{    			
			UpdatePos (position + SmallChange, true);
			Invalidate (thumb_area);
    		}

    		private void SmallDecrement ()
    		{
			UpdatePos (position - SmallChange, true);
			Invalidate (thumb_area);
    		}
    		
		private void OnMouseUpTB (object sender, MouseEventArgs e)
		{	
			if (!Enabled) return;			

			if (thumb_pressed == true || mouse_clickmove == true) {	
				thumb_pressed = false;
				holdclick_timer.Enabled = false;
				this.Capture = false;
				Invalidate (thumb_area);
			}
		}

		private void OnMouseDownTB (object sender, MouseEventArgs e)
    		{
    			if (!Enabled) return;			    			

			mouse_moved = false;

			bool fire_timer = false;
    			
    			Point point = new Point (e.X, e.Y);

			if (orientation == Orientation.Horizontal) {
				
				if (thumb_pos.Contains (point)) {
					this.Capture = true;
					thumb_pressed = true;
					thumb_mouseclick = e.X;
					mouse_down_x_offset = e.X - thumb_pos.X;
					Invalidate (thumb_area);
				}
				else {
					if (thumb_area.Contains (point)) {
						is_moving_right = e.X > thumb_pos.X + thumb_pos.Width; 
						if (is_moving_right)
							LargeIncrement ();
						else
							LargeDecrement ();

						Invalidate (thumb_area);
						fire_timer = true;
						mouse_clickmove = true;
					}
				}
			}
			else {
				Rectangle vertical_thumb_pos = thumb_pos;
				vertical_thumb_pos.Width = thumb_pos.Height;
				vertical_thumb_pos.Height = thumb_pos.Width;
				if (vertical_thumb_pos.Contains (point)) {
					this.Capture = true;
					thumb_pressed = true;
					thumb_mouseclick = e.Y;
					mouse_down_x_offset = e.Y - thumb_pos.Y;
					Invalidate (thumb_area);
				}
				else {
					if (thumb_area.Contains (point)) {
						is_moving_right = e.Y > thumb_pos.Y + thumb_pos.Width;
						if (is_moving_right)
							LargeDecrement ();
						else
							LargeIncrement ();

						Invalidate (thumb_area);
						fire_timer = true;
						mouse_clickmove = true;
					}
				}
			}

			if (fire_timer) { 				
				holdclick_timer.Interval = 300;
				holdclick_timer.Enabled = true;				
			}			
    		}

    		private void OnMouseMoveTB (object sender, MouseEventArgs e)
    		{    			
    			if (!Enabled) return;
    		
    			mouse_moved = true;

    			/* Moving the thumb */
    			if (thumb_pressed)
				SetValue (ThemeEngine.Current.TrackBarValueFromMousePosition (e.X, e.Y, this), true);

			ThumbEntered = GetRealThumbRectangle ().Contains (e.Location);
    		}

		Rectangle GetRealThumbRectangle ()
		{
			Rectangle result = thumb_pos;
			if (Orientation == Orientation.Vertical) {
				result.Width = thumb_pos.Height;
				result.Height = thumb_pos.Width;
			}
			return result;
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{		
			ThemeEngine.Current.DrawTrackBar (pevent.Graphics, pevent.ClipRectangle, this);
		}

		private void OnLostFocusTB (object sender, EventArgs e)
		{
			Invalidate();
		}

		private void OnGotFocusTB (object sender, EventArgs e)
		{
			Invalidate();
		}
		private void OnKeyDownTB (object sender, KeyEventArgs e) 
		{
			bool horiz = Orientation == Orientation.Horizontal;
			switch (e.KeyCode) {
			
			case Keys.Down:
			case Keys.Right:
				if(horiz)
					SmallIncrement();
				else
					SmallDecrement ();
				break;

			case Keys.Up:
			case Keys.Left:
				if (horiz)
					SmallDecrement();
				else
					SmallIncrement();
				break;

			case Keys.PageUp:
				if (horiz)
					LargeDecrement();
				else
					LargeIncrement();
				break;

			case Keys.PageDown:
				if (horiz)
					LargeIncrement();
				else
					LargeDecrement();
				break;

			case Keys.Home:
				if (horiz)
					SetValue (Minimum, true);
				else
					SetValue (Maximum, true);
				break;

			case Keys.End:
				if (horiz)
					SetValue (Maximum, true);
				else
					SetValue (Minimum, true);
				break;

			default:
				break;
			}
		}

		private void OnFirstClickTimer (Object source, ElapsedEventArgs e)
		{						
			Point pnt;
			pnt = PointToClient (MousePosition);			
			/*
				On Win32 the thumb only moves in one direction after a click, 
				if the thumb passes the clicked point it will never go in the 
				other way unless the mouse is released and clicked again. This
				is also true if the mouse moves while beeing hold down.
			*/
		
			if (thumb_area.Contains (pnt)) 	{
				bool invalidate = false;
				if (orientation == Orientation.Horizontal) {
					if (pnt.X > thumb_pos.X + thumb_pos.Width && is_moving_right) {
						LargeIncrement ();
						invalidate = true;
					} else if (pnt.X < thumb_pos.X && !is_moving_right) {
						LargeDecrement ();			
						invalidate = true;
					}					
				} else {
					if (pnt.Y > thumb_pos.Y + thumb_pos.Width && is_moving_right) {
						LargeDecrement ();		
						invalidate = true;
					} else if (pnt.Y < thumb_pos.Y && !is_moving_right) {
						LargeIncrement ();		
						invalidate = true;
					}
 				}
 				if (invalidate)
 					// A Refresh is necessary because the mouse is down and if we just invalidate
 					// we'll only get paint events once in a while.
 					Refresh();
			}			
		}					

		void OnMouseLeave (object sender, EventArgs e)
		{
			ThumbEntered = false;
		}
    		#endregion // Private Methods
	}
}

