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
//		- Draging the thumb does not behave exactly like in Win32
//		- The AutoSize functionality seems quite broken for vertical controls in .Net 1.1. Not
//		sure if we are implementing it the right way.
//		- Vertical orientation still needs some work
//
// Copyright (C) Novell Inc., 2004
//
//
// $Revision: 1.7 $
// $Modtime: $
// $Log: TrackBar.cs,v $
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
		private int thumb_pixel_click_move;
		private float pixel_per_pos = 0;		

		#region Events
		public event EventHandler Scroll;
		public event EventHandler ValueChanged;		
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

			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);			
		}

		#region Public Properties

		public bool AutoSize {
			get { return autosize; }
			set { autosize = value;}
		}

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

		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
		

		public int LargeChange {
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

				/* Orientation can be changed once the control has been created*/
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


		public override string Text {
			get {	return base.Text; }
			set {	base.Text = value; }
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
					Size = new Size (Width, 45);
				else
					Size = new Size (50, Height);

			UpdateArea ();
			CreateBuffers (Width, Height);

			UpdatePos (Value, true);
			UpdatePixelPerPos ();

			Draw ();
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
				
			case Msg.WM_LBUTTONDOWN:
				OnMouseDownTB (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
					
				break;
				
			case Msg.WM_MOUSEMOVE: 
				OnMouseMoveTB  (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
						clicks, 
						LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 
						0));
				break;

			case Msg.WM_SIZE:
				OnResize_TB ();
				break;

			case Msg.WM_PAINT: {
				Rectangle	rect;
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				OnPaint_TB (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}
				
			case Msg.WM_ERASEBKGND:
				m.Result = (IntPtr)1; /* Disable background painting to avoid flickering */
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

			UpdatePixelPerPos ();
			//Console.WriteLine ("UpdateArea: {0} {1} {2}", thumb_area.Width, thumb_pos.Width, pixel_per_pos);

		}

		private void UpdateThumbPos (int pixel, bool update_value)
    		{
    			float new_pos = 0;
    			//Console.WriteLine ("UpdateThumbPos: " + pixel  + " per " + pixel_per_pos);

    			if (orientation == Orientation.Horizontal) {

				if (pixel < thumb_area.X)
	    				thumb_pos.X = thumb_area.X;
	    			else
	    				if (pixel > thumb_area.X + thumb_area.Width - thumb_pos.Width)
	    					thumb_pos.X = thumb_area.X +  thumb_area.Width - thumb_pos.Width;
	    				else
	    					thumb_pos.X = pixel;

				new_pos = (float) (thumb_pos.X - thumb_area.X);
				new_pos = new_pos / pixel_per_pos;

			} else {

				if (pixel < thumb_area.Y)
	    				thumb_pos.Y = thumb_area.Y;
	    			else
	    				if (pixel > thumb_area.Y + thumb_area.Height - thumb_pos.Height)
	    					thumb_pos.Y = thumb_area.Y +  thumb_area.Height - thumb_pos.Height;
	    				else
	    					thumb_pos.Y = pixel;

				new_pos = (float) (thumb_pos.Y - thumb_area.Y);
				new_pos = new_pos / pixel_per_pos;

			}


			//Console.WriteLine ("UpdateThumbPos: thumb_pos.Y {0} thumb_area.Y {1} pixel_per_pos {2}, new pos {3}, pixel {4}",
			//	thumb_pos.Y, thumb_area.Y, pixel_per_pos, new_pos, pixel);

			if (update_value)
				UpdatePos ((int) new_pos, false);
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

    		private void UpdatePixelPerPos ()
    		{
			pixel_per_pos = ((float)(thumb_area.Width)
				/ (float) (1 + Maximum - Minimum));
    		}

		private void Draw ()
		{
			int ticks = (Maximum - Minimum)	/ tickFrequency;
			
			ThemeEngine.Current.DrawTrackBar (DeviceContext, paint_area, ref thumb_pos, ref thumb_area,
				tickStyle, ticks, Orientation, Focused);
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

    			if (orientation == Orientation.Horizontal) {
				if (update_trumbpos)
					UpdateThumbPos (thumb_area.X + (int)(((float)(Value - Minimum)) * pixel_per_pos), false);
			}
			else {
				if (update_trumbpos)
					UpdateThumbPos (thumb_area.Y + (int)(((float)(Value - Minimum)) * pixel_per_pos), false);
			}
    		}

		private void OnMouseDownTB (MouseEventArgs e)
    		{
    			if (!Enabled) return;
    			
    			//System.Console.WriteLine ("OnMouseDown" + thumb_pos);			
    			
    			Point point = new Point (e.X, e.Y);

			if (orientation == Orientation.Horizontal) {

				if (thumb_pos.Contains (point)) {
					//XplatUI.GrabWindow (Handle);
					thumb_pressed = true;
					Refresh ();
					thumb_pixel_click_move = e.X;
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement ();
						else
							LargeDecrement ();

						Refresh ();
					}
				}
			}
			else {
				if (thumb_pos.Contains (point)) {
					//XplatUI.GrabWindow (Handle);
					thumb_pressed = true;
					Refresh ();
					thumb_pixel_click_move = e.Y;
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeIncrement ();
						else
							LargeDecrement ();

						Refresh ();
					}
				}

			}
    		}

    		private void OnMouseMoveTB (MouseEventArgs e)
    		{    			
    			if (!Enabled) return;
    		
    			Point pnt = new Point (e.X, e.Y);

    			/* Moving the thumb */
    			if (thumb_pos.Contains (pnt) && thumb_pressed) {

    				//System.Console.WriteLine ("OnMouseMove " + thumb_pressed);
				//XplatUI.GrabWindow (Handle);
    				int pixel_pos;

    				if (orientation == Orientation.Horizontal)
    					pixel_pos = e.X - (thumb_pixel_click_move - thumb_pos.X);
    				else
    					pixel_pos = e.Y - (thumb_pixel_click_move - thumb_pos.Y);

    				UpdateThumbPos (pixel_pos, true);

    				if (orientation == Orientation.Horizontal)
    					thumb_pixel_click_move = e.X;
    				else
    					thumb_pixel_click_move = e.Y;

    				OnScroll (new EventArgs ());

				//System.Console.WriteLine ("OnMouseMove thumb "+ e.Y
				//	+ " clickpos " + thumb_pixel_click_move   + " pos:" + thumb_pos.Y);

				Refresh ();
			}

			if (!thumb_pos.Contains (pnt) && thumb_pressed) {
				//XplatUI.ReleaseWindow (Handle);
    				thumb_pressed = false;
				Invalidate ();
			}

    		}

		private void OnResize_TB ()
    		{
    			//Console.WriteLine ("OnResize");

    			if (Width <= 0 || Height <= 0)
    				return;

			UpdateArea ();
			CreateBuffers (Width, Height);
		}		

		private void OnPaint_TB (PaintEventArgs pevent)
		{		
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			/* Copies memory drawing buffer to screen*/
			UpdateArea ();
			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}  

    		#endregion // Private Methods
	}
}

