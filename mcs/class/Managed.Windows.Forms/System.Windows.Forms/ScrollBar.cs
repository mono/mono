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
// Copyright (C) 2004, Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	jordi@ximian.com
//
//
// $Revision: 1.10 $
// $Modtime: $
// $Log: ScrollBar.cs,v $
// Revision 1.10  2004/08/21 20:22:21  pbartok
// - Replaced direct XplatUI calls with their Control counterpart
//
// Revision 1.9  2004/08/20 19:35:33  jackson
// Use the SWF timer so callbacks are run in the correct thread
//
// Revision 1.8  2004/08/20 19:34:26  jackson
// Use the SWF timer so callbacks are run in the correct thread
//
// Revision 1.7  2004/08/19 22:25:31  jordi
// theme enhancaments
//
// Revision 1.6  2004/08/18 15:56:12  jordi
// fixes to scrollbar: steps and multiple timers
//
// Revision 1.5  2004/08/10 19:21:27  jordi
// scrollbar enhancements and standarize on win colors defaults
//
// Revision 1.4  2004/08/10 15:41:50  jackson
// Allow control to handle buffering
//
// Revision 1.3  2004/07/27 15:29:40  jordi
// fixes scrollbar events
//
// Revision 1.2  2004/07/26 17:42:03  jordi
// Theme support
//

// NOT COMPLETE

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms 
{	
	
	public class ScrollBar : Control 
	{		
		#region Local Variables
		private int position;
		private int minimum;
		private int maximum;		
		private int largeChange;
		private int smallChange;
		private int scrollbutton_height;
		private int scrollbutton_width;	          	    
		private Rectangle paint_area = new Rectangle ();
		private ScrollBars type;
		private Rectangle first_arrow_area = new Rectangle ();		// up or left
		private Rectangle second_arrow_area = new Rectangle ();		// down or right		
		private Rectangle thumb_pos = new Rectangle ();		
		private Rectangle thumb_area = new Rectangle ();		
		private ButtonState firstbutton_state = ButtonState.Normal;
		private ButtonState secondbutton_state = ButtonState.Normal;
		private bool thumb_pressed = false;
		private float pixel_per_pos = 0;
		private Timer firstclick_timer;
		private Timer holdclick_timer;
		private int thumb_pixel_click_move;			
		private int thumb_size = 0;		
		private const int thumb_min_size = 8;
		protected bool vert;
				
		public event ScrollEventHandler Scroll;
		public event EventHandler ValueChanged;				
		#endregion	// Local Variables
				
		
		public ScrollBar() : base()
		{				
			position = 0;
			minimum = 0;
			maximum = 100;
			largeChange = 10;
			smallChange = 1;			
//			base.TabStop = false;
//			RightToLeft = RightToLeft.No;
			Scroll = null;
			ValueChanged = null;			


			holdclick_timer = new Timer ();
			firstclick_timer = new Timer ();
			holdclick_timer.Tick += new EventHandler (OnHoldClickTimer);
			firstclick_timer.Tick += new EventHandler (OnFirstClickTimer);

			if (ThemeEngine.Current.WriteToWindow == true)
				double_buffering = false;
			else
				double_buffering = true;
						
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color BackColor 
		{
			get { return base.BackColor; }
			set { base.BackColor = value;}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		[MonoTODO]
		public override Image BackgroundImage 
		{
			get {
				throw new NotImplementedException();	
			}
			set { 
				throw new NotImplementedException();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Color ForeColor 
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		[MonoTODO]
		public new ImeMode ImeMode 
		{
			get { return ImeMode.Disable;}
			set { throw new NotImplementedException(); }
		}

		
		public int LargeChange {
			get { return largeChange; }
			set {
				if (value < 0)
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (largeChange != value) {
					largeChange = value;	
					Refresh ();
				}
			}
		}
		
		public int Maximum {
			get { return maximum; }
			set {
				maximum = value;

				if (maximum < minimum)
					minimum = maximum;

				Refresh ();
			}
		}
		
		public int Minimum {
			get { return minimum; }
			set {
				minimum = value;

				if (minimum > maximum)
					maximum = minimum;		

				Refresh ();
			}
		}
		
		public int SmallChange {
			get { return smallChange; }
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
			 get { return base.Text;  }
			 set { base.Text = value; }
		 }

		
		public int Value {
			get { return position; }
			set {
				if ( value < Minimum || value > Maximum )
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				if (position != value){
					position = value;
					
					if (ValueChanged != null)
						ValueChanged (this, EventArgs.Empty);

					Refresh ();
				}
			}
		}

		public override string ToString()
		{	
			return string.Format("{0}, Minimum: {1}, Maximum: {2}, Value: {3}",
						GetType( ).FullName.ToString( ), Minimum, Maximum, position);
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
			get { return ImeMode.Disable; }
		}
		
		
		private void fire_Scroll (ScrollEventArgs event_args)
		{
			if (Scroll == null)
				return;
				
			Scroll (this, event_args);					
			
			//if (event_args.NewValue != position)
			//	UpdatePos (event_args.NewValue, true);
		}
		
		protected virtual void OnValueChanged (EventArgs e)
		{					
			if (ValueChanged != null) 
				ValueChanged (this, e);			
		}

		private void Draw ()
		{					
			ThemeEngine.Current.DrawScrollBar (DeviceContext, paint_area, this, thumb_pos,
				ref first_arrow_area, ref second_arrow_area,
				firstbutton_state, secondbutton_state, 
				ref scrollbutton_width, ref scrollbutton_height, vert);
			
		}
				
		private void CalcThumbArea ()
		{	
			// Thumb area			
			if (vert) {
				
				thumb_area.Height = Height - scrollbutton_height -  scrollbutton_height;
				thumb_area.X = 0;
				thumb_area.Y = scrollbutton_height;
				thumb_area.Width = Width;	

				if (Height < scrollbutton_height * 2)
					thumb_size = 0;
				else {
					double per =  ((double)LargeChange / (double)((1 + Maximum - Minimum)));
					thumb_size = 1 + (int) (thumb_area.Height * per);
	
					if (thumb_size < thumb_min_size)
						thumb_size = thumb_min_size; 					
				}
					
							
				pixel_per_pos = ((float)(thumb_area.Height - thumb_size) / (float) ((Maximum - Minimum - LargeChange) + 1));									
				
			} else	{
				
				if (Width < scrollbutton_width * 2)
					thumb_size = 0;
				else
					if (Width < 70)
						thumb_size = 8;
					else
						thumb_size = Width /10;
				
				thumb_area.Y = 0;
				thumb_area.X = scrollbutton_width;
				thumb_area.Height = Height;
				thumb_area.Width = Width - scrollbutton_width -  scrollbutton_width;			
				pixel_per_pos = ((float)(thumb_area.Width - thumb_size) / (float) ((Maximum - Minimum - LargeChange) + 1));			
			}			
		}
		
    		protected override void OnResize (EventArgs e) 
    		{
    			base.OnResize (e);    
    			
    			if (Width <= 0 || Height <= 0)
    				return;
			
			paint_area.X = paint_area. Y = 0;
			paint_area.Width = Width; 
			paint_area.Height = Height;						

			CreateBuffers (Width, Height);
			
			CalcThumbArea ();
			UpdatePos (position, true);
    		}
    						     

		/*
			Called when the control is created
		*/		
		protected override void CreateHandle()
		{				
			base.CreateHandle();	// Let control.cs create the underlying Window							
			
			scrollbutton_height = ThemeEngine.Current.ScrollBarButtonSize;
			scrollbutton_width = ThemeEngine.Current.ScrollBarButtonSize;

			CreateBuffers (Width, Height);			
			
			CalcThumbArea ();
			UpdatePos (Value, true);					
		}
		
		protected override void OnPaint (PaintEventArgs pevent)
		{				
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;
										
			/* Copies memory drawing buffer to screen*/		
			Draw ();

			if (double_buffering)
				pevent.Graphics.DrawImage (ImageBuffer, 0, 0);			

		}	
		
		/* Disable background painting to avoid flickering, since we do our painting*/


		protected override void OnPaintBackground (PaintEventArgs pevent) 
    		{
    			if (!double_buffering)
				base.OnPaintBackground (pevent);
    		}		
		
    		protected override void OnClick (EventArgs e)
    		{
    			//Console.WriteLine ("On click");    			
    		}
    		
    		private void UpdatePos (int newPos, bool update_trumbpos)
    		{    	
    			int old = position;
    					
    			if (newPos < minimum)
    				position = minimum;
    			else
    				if (newPos > maximum)
    					position = maximum;    			
    					else
    						position = newPos;
    			    			
			if (update_trumbpos) 
				if (vert)
					UpdateThumbPos (thumb_area.Y + (int)(((float)(position - Minimum)) * pixel_per_pos), false);
				else
					UpdateThumbPos (thumb_area.X + (int)(((float)(position - Minimum)) * pixel_per_pos), false);

			if (position != old) // Fire event
				fire_Scroll (new ScrollEventArgs (ScrollEventType.ThumbTrack, position));
			
    		}	
    		
    		private void UpdateThumbPos (int pixel, bool update_value)
    		{    			    			    			
    			float new_pos = 0;
    			
    			if (vert) {
	    			if (pixel < thumb_area.Y)
	    				thumb_pos.Y = thumb_area.Y;
	    			else
	    				if (pixel > thumb_area.Y + thumb_area.Height - thumb_size)
	    					thumb_pos.Y = thumb_area.Y +  thumb_area.Height - thumb_size;    			
	    				else
	    					thumb_pos.Y = pixel;		 
				
				thumb_pos = new Rectangle (0, thumb_pos.Y, ThemeEngine.Current.ScrollBarButtonSize, thumb_size);
				new_pos = (float) (thumb_pos.Y - thumb_area.Y);
				new_pos = new_pos / pixel_per_pos;
			} else	{
				
				if (pixel < thumb_area.X)
	    				thumb_pos.X = thumb_area.X;
	    			else
	    				if (pixel > thumb_area.X + thumb_area.Width - thumb_size)
	    					thumb_pos.X = thumb_area.X +  thumb_area.Width - thumb_size;    			
	    				else
	    					thumb_pos.X = pixel;		 
				
				thumb_pos = new Rectangle (thumb_pos.X, 0, thumb_size, ThemeEngine.Current.ScrollBarButtonSize);								
				new_pos = (float) (thumb_pos.X - thumb_area.X);
				new_pos = new_pos / pixel_per_pos;
			}
			
				  // Console.WriteLine ("UpdateThumbPos: thumb_pos.Y {0} thumb_area.Y {1} pixel_per_pos {2}, new pos {3}, pixel {4}",
			//	thumb_pos.Y, thumb_area.Y, pixel_per_pos, new_pos, pixel);
			
			if (update_value) 				
				UpdatePos ((int) new_pos, false);						
    		}
    		
		private void OnHoldClickTimer (Object source, EventArgs e)
		{			
			if ((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)								
				SmallDecrement();
				
			if ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)				
				SmallIncrement();				
			
		}
    		
		private void OnFirstClickTimer (Object source, EventArgs e)
		{
			firstclick_timer.Enabled = false;			
		        holdclick_timer.Interval = 50;
		        holdclick_timer.Enabled = true;		        
		}			
		
    		protected override void OnMouseMove (MouseEventArgs e) 
    		{    			
    			if (!first_arrow_area.Contains (new Point (e.X, e.Y)) && 
    				((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {				    				
				firstbutton_state = ButtonState.Normal;				
				Refresh ();
			}			
			
			if (!second_arrow_area.Contains (new Point (e.X, e.Y)) && 
    				((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {				    				
				secondbutton_state = ButtonState.Normal;				
				Refresh ();
			}			

			if (thumb_pressed == true) {

    				int pixel_pos;
    				
    				if (vert)
    				 	pixel_pos = e.Y - (thumb_pixel_click_move - thumb_pos.Y);
    				else
    				 	pixel_pos = e.X - (thumb_pixel_click_move - thumb_pos.X);
    				
    				UpdateThumbPos (pixel_pos, true);
    				
    				if (vert)
    					thumb_pixel_click_move = e.Y;    				
    				else
    					thumb_pixel_click_move = e.X;    				

				//System.Console.WriteLine ("OnMouseMove thumb "+ e.Y 
				//	+ " clickpos " + thumb_pixel_click_move   + " pos:" + thumb_pos.Y);    								
					
				Refresh ();
			}						
			
    		}	    		
    		
    		protected override void OnMouseDown (MouseEventArgs e) 
    		{
    			//System.Console.WriteLine ("OnMouseDown");    			
    			
    			Point point = new Point (e.X, e.Y);
    			
    			if (first_arrow_area.Contains (point)) {				
				firstbutton_state = ButtonState.Pushed;
				Refresh ();
			}
			
			if (second_arrow_area.Contains (point)) {
				secondbutton_state = ButtonState.Pushed;				
				Refresh ();
			}
			
			if (thumb_pos.Contains (point)) {								
				thumb_pressed = true;
				this.Capture = true;
				Refresh ();
				if (vert)
					thumb_pixel_click_move = e.Y;
				else
					thumb_pixel_click_move = e.X;
			}			
			else
				if (thumb_area.Contains (point)) {					
					if (vert) {					
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeIncrement ();
						else
							LargeDecrement ();
					} else 	{
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement ();
						else
							LargeDecrement ();
					}							
					
				}			
			
			
			/* If arrows are pressed, lunch timer for auto-repeat */
			if ((((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)
			|| ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) && 
				firstclick_timer.Enabled == false) {			
				//Console.WriteLine ("Activate Timer");				
		        	firstclick_timer.Interval = 200;
		        	firstclick_timer.Enabled = true;
			}  			
    		}
    		
    		private void SmallIncrement ()
    		{			
			UpdatePos (Value + SmallChange, true);

			Refresh ();				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.SmallIncrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void SmallDecrement ()
    		{
			UpdatePos (Value - SmallChange, true);
			
			Refresh ();				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.SmallDecrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void LargeIncrement ()
    		{
			UpdatePos (Value + LargeChange, true);

			Refresh ();
			fire_Scroll (new ScrollEventArgs (ScrollEventType.LargeIncrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void LargeDecrement ()
    		{
			UpdatePos (Value - LargeChange, true);
				
			Refresh ();
			fire_Scroll (new ScrollEventArgs (ScrollEventType.LargeDecrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		protected override void OnMouseUp (MouseEventArgs e) 
    		{
    			//System.Console.WriteLine ("OnMouseUp");
    			
    			if (first_arrow_area.Contains (new Point (e.X, e.Y))) {				
    				
				firstbutton_state = ButtonState.Normal;												
				SmallDecrement ();								
				holdclick_timer.Enabled = false;
			}
			
			if (second_arrow_area.Contains (new Point (e.X, e.Y))) {				
				
				secondbutton_state = ButtonState.Normal;						
				SmallIncrement ();					
				holdclick_timer.Enabled = false;
			}
			
			if (thumb_pressed == true) {				
				fire_Scroll (new ScrollEventArgs (ScrollEventType.ThumbPosition, position));
				fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
				this.Capture = false;
				thumb_pressed = false;				
				Refresh ();
			}


    		}    		
    		
    		protected override void OnKeyDown (KeyEventArgs key)
		{
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
			default:
				break;
			}

		}    				
		
		protected void UpdateScrollInfo ()
		{
			Refresh ();
		}
	 }
}


