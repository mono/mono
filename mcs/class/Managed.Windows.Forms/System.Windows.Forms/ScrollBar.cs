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
// $Revision: 1.4 $
// $Modtime: $
// $Log: ScrollBar.cs,v $
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
using System.Timers;

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
		private System.Timers.Timer firstclick_timer = new System.Timers.Timer ();
		private System.Timers.Timer holdclick_timer = new System.Timers.Timer ();
		private int thumb_pixel_click_move;			
		private int thumb_size = 0;		
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
			get { return ImeMode.Disable;
throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		
		public int LargeChange {
			get { return largeChange; }
			set {
				if (value < 0)
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (largeChange != value) {
					largeChange = value;	
					Invalidate ();
				}
			}
		}
		
		public int Maximum {
			get { return maximum; }
			set {
				maximum = value;

				if (maximum < minimum)
					minimum = maximum;
			}
		}
		
		public int Minimum {
			get { return minimum; }
			set {
				minimum = value;

				if (minimum > maximum)
					maximum = minimum;		
			}
		}
		
		public int SmallChange {
			get { return smallChange; }
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));
				 
				if (smallChange != value) {
					smallChange = value;	
					Invalidate ();
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
			if (ValueChanged != null) {
				Console.Write ("OnValueChanged");
				ValueChanged (this, e);
			}
		}

		
		private void draw()
		{					
			ThemeEngine.Current.DrawScrollBar (DeviceContext, paint_area, thumb_pos,
				ref first_arrow_area, ref second_arrow_area,
				firstbutton_state, secondbutton_state, 
				ref scrollbutton_width, ref scrollbutton_height,
				Enabled, vert);
			
		}
				
		private void CalcThumbArea ()
		{	
			// Thumb area
			
			if (vert) {
				
				if (Height < scrollbutton_height * 2)
					thumb_size = 0;
				else
					if (Height < 70)
						thumb_size = 8;
					else
						thumb_size = Height /10;
					
				thumb_area.X = 0;
				thumb_area.Y = scrollbutton_height;
				thumb_area.Width = Width;
				thumb_area.Height = Height - scrollbutton_height -  scrollbutton_height;			
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
			
			//Console.WriteLine ("thumb_area:" + thumb_area);
			//Console.WriteLine ("Maximum {0} Minimum {1} " , Maximum, Minimum);						
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
			//Console.WriteLine ("CreateHandle()");			
			
			base.CreateHandle();	// Let control.cs create the underlying Window							
			
			scrollbutton_height = 17;
			scrollbutton_width = 17;

			CreateBuffers (Width, Height);

			//Console.WriteLine ("OnCreate: Width " + Width + " Height " +  Height);
			CalcThumbArea ();
			UpdatePos (Value, true);
			draw();					
			
		}
		
		protected override void OnPaint (PaintEventArgs pevent)
		{	
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;
										
			/* Copies memory drawing buffer to screen*/		
			draw();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);			
		}	
		
		/* Disable background painting to avoid flickering, since we do our painting*/
		protected override void OnPaintBackground (PaintEventArgs pevent) 
    		{
    			// None
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
    						
    			//Console.WriteLine ("event : {0} {1} {2}", Scroll != null, position, old);    			
    			
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
				
				thumb_pos = new Rectangle (0, thumb_pos.Y, Width, thumb_size);								
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
				
				thumb_pos = new Rectangle (thumb_pos.X, 0, thumb_size, Height);								
				new_pos = (float) (thumb_pos.X - thumb_area.X);
				new_pos = new_pos / pixel_per_pos;
			}
			
			//Console.WriteLine ("UpdateThumbPos: thumb_pos.Y {0} thumb_area.Y {1} pixel_per_pos {2}, new pos {3}, pixel {4}",
			//	thumb_pos.Y, thumb_area.Y, pixel_per_pos, new_pos, pixel);
			
			if (update_value) 				
				UpdatePos ((int) new_pos, false);						
    		}
    		
    		private void OnHoldClickTimer (Object source, ElapsedEventArgs e)
		{			
			if ((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)								
				SmallDecrement();
				
			if ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)				
				SmallIncrement();
				
			Invalidate ();
		}
    		
    		private void OnFirstClickTimer (Object source, ElapsedEventArgs e)
		{
			//Console.WriteLine ("OnFirstClickTimer");
			firstclick_timer.Enabled = false;			
			holdclick_timer.Elapsed += new ElapsedEventHandler (OnHoldClickTimer);
		        holdclick_timer.Interval = 50;
		        holdclick_timer.Enabled = true;		        
		}			
		
    		protected override void OnMouseMove (MouseEventArgs e) 
    		{    			
    			if (!first_arrow_area.Contains (new Point (e.X, e.Y)) && 
    				((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {				    				
				firstbutton_state = ButtonState.Normal;				
				Invalidate ();
			}			
			
			if (!second_arrow_area.Contains (new Point (e.X, e.Y)) && 
    				((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) {				    				
				secondbutton_state = ButtonState.Normal;				
				Invalidate ();
			}			
			
			if (thumb_pos.Contains (new Point (e.X, e.Y)) && thumb_pressed) {				    				
    				
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
					
				Invalidate ();
			}						
			
			if (!thumb_pos.Contains (new Point (e.X, e.Y)) && thumb_pressed) {				
    				thumb_pressed = false;				
				Invalidate ();
			}
			
    		}	    		
    		
    		protected override void OnMouseDown (MouseEventArgs e) 
    		{
    			//System.Console.WriteLine ("OnMouseDown");    			
    			
    			Point point = new Point (e.X, e.Y);
    			
    			if (first_arrow_area.Contains (point)) {				
				firstbutton_state = ButtonState.Pushed;
				Invalidate ();
			}
			
			if (second_arrow_area.Contains (point)) {
				secondbutton_state = ButtonState.Pushed;				
				Invalidate ();
			}
			
			if (thumb_pos.Contains (point)) {								
				thumb_pressed = true;				
				Invalidate ();
				if (vert)
					thumb_pixel_click_move = e.Y;
				else
					thumb_pixel_click_move = e.X;
			}			
			else
				if (thumb_area.Contains (point)) {								
					
					if (vert) {					
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeIncrement();
						else
							LargeDecrement();
					} else 	{
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement();
						else
							LargeDecrement();
					}				
						
					Invalidate ();
				}			
			
			
			/* If arrows are pressed, lunch timer for auto-repeat */
			if ((((firstbutton_state & ButtonState.Pushed) == ButtonState.Pushed)
			|| ((secondbutton_state & ButtonState.Pushed) == ButtonState.Pushed)) && 
				firstclick_timer.Enabled == false) {			
				//Console.WriteLine ("Activate Timer");
				firstclick_timer.Elapsed += new ElapsedEventHandler (OnFirstClickTimer);
		        	firstclick_timer.Interval = 400;
		        	firstclick_timer.Enabled = true;
		        	firstclick_timer.AutoReset = false;		        	
			}  			
    		}
    		
    		private void SmallIncrement()
    		{
			if (vert)
				UpdateThumbPos (thumb_pos.Y + SmallChange, true);
			else
				UpdateThumbPos (thumb_pos.X + SmallChange, true);
				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.SmallIncrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void SmallDecrement()
    		{
			if (vert)
				UpdateThumbPos (thumb_pos.Y - SmallChange, true);
			else
				UpdateThumbPos (thumb_pos.X - SmallChange, true);		
				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.SmallDecrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void LargeIncrement()
    		{
			if (vert)
				UpdateThumbPos (thumb_pos.Y + LargeChange, true);
			else
				UpdateThumbPos (thumb_pos.X + LargeChange, true);
				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.LargeIncrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		
    		private void LargeDecrement()
    		{
			if (vert)
				UpdateThumbPos (thumb_pos.Y - LargeChange, true);
			else
				UpdateThumbPos (thumb_pos.X - LargeChange, true);		
				
			fire_Scroll (new ScrollEventArgs (ScrollEventType.LargeDecrement, position));
			fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
    		}
    		protected override void OnMouseUp (MouseEventArgs e) 
    		{
    			//System.Console.WriteLine ("OnMouseUp");
    			
    			if (first_arrow_area.Contains (new Point (e.X, e.Y))) {				
    				
				firstbutton_state = ButtonState.Normal;												
				SmallDecrement ();				
				Invalidate ();
				holdclick_timer.Enabled = false;
			}
			
			if (second_arrow_area.Contains (new Point (e.X, e.Y))) {				
				
				secondbutton_state = ButtonState.Normal;						
				SmallIncrement ();	
				Invalidate ();
				holdclick_timer.Enabled = false;
			}
			
			if (thumb_pos.Contains (new Point (e.X, e.Y))) {																
				
				fire_Scroll (new ScrollEventArgs (ScrollEventType.ThumbPosition, position));
				fire_Scroll (new ScrollEventArgs (ScrollEventType.EndScroll, position));
				
				thumb_pressed = false;				
				Invalidate ();
			}
    		}
    		
    		//TODO: Untested
    		protected override void OnKeyDown (KeyEventArgs key)
		{
			switch (key.KeyCode){
			case Keys.Up:
			{
				SmallDecrement();					
				break;	
			}
			case Keys.Down:
			{
				SmallIncrement();					
				break;	
			}
			case Keys.PageUp:
			{
				LargeDecrement();					
				break;	
			}
			case Keys.PageDown:
			{
				LargeIncrement();					
				break;	
			}
			default:
				break;
			}

		}    				
		
		protected void UpdateScrollInfo ()
		{
			Invalidate ();
		}
	 }
}


