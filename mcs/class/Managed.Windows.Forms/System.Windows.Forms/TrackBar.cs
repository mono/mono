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
// Autors:
//		Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//		- The AutoSize functionality seems quite broken for vertical controls in .Net 1.1. Not
//		sure if we are implementing it the right way.
//
// Copyright (C) Novell Inc., 2004
//
//
// $Revision: 1.11 $
// $Modtime: $
// $Log: TrackBar.cs,v $
// Revision 1.11  2004/08/20 19:45:50  jordi
// fixes timer, new properties and methods
//
// Revision 1.10  2004/08/13 20:55:20  jordi
// change from wndproc to events
//
// Revision 1.9  2004/08/13 18:46:26  jordi
// adds timer and grap window
//
// Revision 1.8  2004/08/12 20:29:01  jordi
// Trackbar enhancement, fix mouse problems, highli thumb, etc
//
// Revision 1.7  2004/08/10 23:27:12  jordi
// add missing methods, properties, and restructure to hide extra ones
//
// Revision 1.6  2004/08/10 15:47:11  jackson
// Allow control to handle buffering
//
// Revision 1.5  2004/08/07 23:32:26  jordi
// throw exceptions of invalid enums values
//
// Revision 1.4  2004/08/06 23:18:06  pbartok
// - Fixed some rounding issues with float/int
//
// Revision 1.3  2004/07/27 15:53:02  jordi
// fixes trackbar events, def classname, methods signature
//
// Revision 1.2  2004/07/26 17:42:03  jordi
// Theme support
//
// Revision 1.1  2004/07/15 09:38:02  jordi
// Horizontal and Vertical TrackBar control implementation
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Timers;

namespace System.Windows.Forms
{	
	public class TrackBar : Control, ISupportInitialize
	{
		private int minimum;
		private int maximum;
		private int tickFrequency;
		private bool autosize;
		private int position;
		private int smallChange;
		private int largeChange;
		private Orientation orientation;
		private TickStyle tickStyle;
		private Rectangle paint_area = new Rectangle ();
		private Rectangle thumb_pos = new Rectangle ();	 /* Current position and size of the thumb */
		private Rectangle thumb_area = new Rectangle (); /* Area where the thumb can scroll */
		private bool thumb_pressed = false;		 
		private System.Timers.Timer holdclick_timer = new System.Timers.Timer ();
		private int thumb_mouseclick;		
		private bool mouse_clickmove;

		#region Events
		public event EventHandler Scroll;
		public event EventHandler ValueChanged;		
		public new event EventHandler ImeModeChanged;
		public new event EventHandler ForeColorChanged;
		public new event EventHandler TextChanged;
		#endregion // Events

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
			Scroll = null;
			ValueChanged  = null;		
			mouse_clickmove = false;
			SizeChanged += new System.EventHandler (OnResizeTB);
			MouseDown += new MouseEventHandler (OnMouseDownTB); 
			MouseUp += new MouseEventHandler (OnMouseUpTB); 
			holdclick_timer.Elapsed += new ElapsedEventHandler (OnFirstClickTimer);

			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);			
		}

		#region Public Properties

		public bool AutoSize {
			get { return autosize; }
			set { autosize = value;}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = XplatUI.DefaultClassName;

				createParams.Style = (int) (
					WindowStyles.WS_CHILD |
					WindowStyles.WS_VISIBLE);

				return createParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return new System.Drawing.Size (104, 42); }
		}	

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		public override Color ForeColor {
			get { return base.ForeColor; }
			set {
				if (value == base.ForeColor)
					return;

				if (ForeColorChanged != null)
					ForeColorChanged (this, EventArgs.Empty);

				Refresh ();
			}
		}		

		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set {
				if (value == base.ImeMode)
					return;

				base.ImeMode = value;
				if (ImeModeChanged != null)
					ImeModeChanged (this, EventArgs.Empty);
			}
		}

		public int LargeChange 
		{
			get { return largeChange; }
			set {
				if (value < 0)
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				largeChange = value;
				Refresh ();
			}
		}

		public int Maximum {
			get { return maximum; }
			set {
				if (maximum != value)  {
					maximum = value;

					if (maximum < minimum)
						minimum = maximum;

					Refresh ();
				}
			}
		}

		public int Minimum {
			get { return minimum; }
			set {

				if (Minimum != value) {
					minimum = value;

					if (minimum > maximum)
						maximum = minimum;

					Refresh ();
				}
			}
		}

		public Orientation Orientation {
			get { return orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for Orientation", value));

				/* Orientation can be changed once the control has been created */
				if (orientation != value) {
					orientation = value;
				
					int old_witdh = Width;
					Width = Height;
					Height = old_witdh;
					Refresh (); 
				}
			}
		}

		public int SmallChange {
			get { return smallChange;}
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (smallChange != value) {
					smallChange = value;
					Refresh ();
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get {	return base.Text; }			
			set {
				if (value == base.Text)
					return;

				if (TextChanged != null)
					TextChanged (this, EventArgs.Empty);

				Refresh ();
			}
		}


		public int TickFrequency {
			get { return tickFrequency; }
			set {
				if ( value > 0 ) {
					tickFrequency = value;
					Refresh ();
				}
			}
		}

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

		public int Value {
			get { return position; }
			set {
				if (value < Minimum || value > Maximum)
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
				
				if (position != value) {													
					position = value;					
					
					if (ValueChanged != null)				
						ValueChanged (this, new EventArgs ());
						
					Refresh ();
				}				
			}
		}

		#endregion //Public Properties

		#region Public Methods

		public virtual void BeginInit ()		
		{

		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}


		public virtual void EndInit ()		
		{

		}

		protected override bool IsInputKey (Keys keyData)
		{
			return false;
		}

		protected override void OnBackColorChanged (EventArgs e)
		{

		}

		protected override void OnHandleCreated (EventArgs e)
		{			
			if (AutoSize)
				if (Orientation == Orientation.Horizontal)
					Size = new Size (Width, 40);
				else
					Size = new Size (50, Height);

			UpdateArea ();
			CreateBuffers (Width, Height);
			UpdatePos (Value, true);			
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			if (!Enabled) return;
    			
			if (e.Delta > 0)
				SmallDecrement ();
			else
				SmallIncrement ();
    					
		}

		protected virtual void OnScroll (EventArgs e) 
		{
			if (Scroll != null) 
				Scroll (this, e);
		}

		protected virtual void OnValueChanged (EventArgs e) 
		{
			if (ValueChanged != null) 
				ValueChanged (this, e);
		}

		public void SetRange (int minValue, int maxValue)
		{
			Minimum = minValue;
			Maximum = maxValue;

			Refresh ();
		}

		public override string ToString()
		{
			return string.Format("System.Windows.Forms.Trackbar, Minimum: {0}, Maximum: {1}, Value: {2}",
						Minimum, Maximum, Value);
		}
				
			

		protected override void WndProc (ref Message m)
    		{
			int clicks = 1;

			switch ((Msg) m.Msg) {
				
				
			case Msg.WM_MOUSEMOVE: 
				OnMouseMoveTB  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, 
						LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
				break;
			
			case Msg.WM_PAINT: {				
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				OnPaintTB (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}		

			case Msg.WM_KEYDOWN: 
				OnKeyDownTB (new KeyEventArgs ((Keys)m.WParam.ToInt32 ()));
				return;			
				
			case Msg.WM_ERASEBKGND:
				m.Result = (IntPtr) 1; /* Disable background painting to avoid flickering */
				return;
				
			default:
				break;
			}

			base.WndProc (ref m);
    		}
    		
		#endregion Public Methods

		#region Private Methods

		private void UpdateArea ()
		{
			paint_area.X = paint_area.Y = 0;
			paint_area.Width = Width;
			paint_area.Height = Height;			
		}

		private void UpdatePos (int newPos, bool update_trumbpos)
		{
			int old = position;

			if (newPos < minimum)
				Value = minimum;
			else
				if (newPos > maximum)
				Value = maximum;
			else
				Value = newPos;    			
		}
		
		private void LargeIncrement ()
    		{    			
			UpdatePos (position + LargeChange, true);
			Refresh ();
			OnScroll (new EventArgs ());
    		}

    		private void LargeDecrement ()
    		{
			UpdatePos (position - LargeChange, true);
			Refresh ();
			OnScroll (new EventArgs ());
    		}

		private void SmallIncrement ()
    		{    			
			UpdatePos (position + SmallChange, true);
			Refresh ();
			OnScroll (new EventArgs ());
    		}

    		private void SmallDecrement ()
    		{
			UpdatePos (position - SmallChange, true);
			Refresh ();
			OnScroll (new EventArgs ());	
    		}
    		
		private void Draw ()
		{					
			float ticks = (Maximum - Minimum) / tickFrequency; /* N of ticks draw*/                        
	
			if (thumb_pressed)
				ThemeEngine.Current.DrawTrackBar (DeviceContext, paint_area, this, 
					ref thumb_pos, ref thumb_area, thumb_pressed, ticks, thumb_mouseclick, true);
			else
				ThemeEngine.Current.DrawTrackBar (DeviceContext, paint_area, this,
					ref thumb_pos, ref thumb_area, thumb_pressed, ticks,  Value - Minimum, false);

		}		

		private void OnMouseUpTB (object sender, MouseEventArgs e)
		{	
			if (!Enabled) return;			

			if (thumb_pressed == true || mouse_clickmove == true) {	
				thumb_pressed = false;
				holdclick_timer.Enabled = false;
				XplatUI.ReleaseWindow (Handle);
				Refresh ();
			}
		}

		private void OnMouseDownTB (object sender, MouseEventArgs e)
    		{
    			if (!Enabled) return;			    			

			bool fire_timer = false;
    			
    			Point point = new Point (e.X, e.Y);

			if (orientation == Orientation.Horizontal) {
				
				if (thumb_pos.Contains (point)) {
					XplatUI.GrabWindow (Handle);
					thumb_pressed = true;
					thumb_mouseclick = e.X;
					Refresh ();					
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement ();
						else
							LargeDecrement ();

						Refresh ();
						fire_timer = true;
						mouse_clickmove = true;
					}
				}
			}
			else {
				if (thumb_pos.Contains (point)) {
					XplatUI.GrabWindow (Handle);
					thumb_pressed = true;
					thumb_mouseclick = e.Y;
					Refresh ();
					
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeIncrement ();
						else
							LargeDecrement ();

						Refresh ();
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

    		private void OnMouseMoveTB (MouseEventArgs e)
    		{    			
    			if (!Enabled) return;
    		
    			Point pnt = new Point (e.X, e.Y);

    			/* Moving the thumb */
    			if (thumb_pressed) {
								 				
    				if (orientation == Orientation.Horizontal){
					if (paint_area.Contains (e.X, thumb_pos.Y))
						thumb_mouseclick = e.X;	
				}
    				else {
					if (paint_area.Contains (thumb_pos.X, e.Y))
						thumb_mouseclick = e.Y;
				}

				Refresh ();
    				OnScroll (new EventArgs ());
			}
    		}

		private void OnResizeTB (object sender, System.EventArgs e)
    		{			
    			if (Width <= 0 || Height <= 0)
    				return;

			UpdateArea ();
			CreateBuffers (Width, Height);
		}		

		private void OnPaintTB (PaintEventArgs pevent)
		{		
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;		

			/* Copies memory drawing buffer to screen*/
			UpdateArea ();
			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}  

		private void OnKeyDownTB (KeyEventArgs e) 
		{			
			switch (e.KeyCode) {			
			case Keys.Up:
			case Keys.Right:
				SmallIncrement ();
				break;

			case Keys.Down:
			case Keys.Left:
				SmallDecrement ();
				break;
			
			default:
				break;
			}
		}

		private void OnFirstClickTimer (Object source, ElapsedEventArgs e)
		{						
			Point pnt;
			pnt = PointToClient (MousePosition);			

			if (thumb_area.Contains (pnt)) 	{
				if (orientation == Orientation.Horizontal) {
					if (pnt.X > thumb_pos.X + thumb_pos.Width)
						LargeIncrement ();

					if (pnt.X < thumb_pos.X)
						LargeDecrement ();						
				}
				else 				{
					if (pnt.Y > thumb_pos.Y + thumb_pos.Height)
						LargeIncrement ();

					if (pnt.Y < thumb_pos.Y)
						LargeDecrement ();
				}

				Refresh ();

			}			
		}					

		protected override void SetBoundsCore (int x, int y,int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y,width,	height, specified);
		}

		
    		#endregion // Private Methods
	}
}

