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
//		- OnMouseWheel event
//
// Copyright (C) Novell Inc., 2004
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: TrackBar.cs,v $
// Revision 1.1  2004/07/15 09:38:02  jordi
// Horizontal and Vertical TrackBar control implementation
//
//

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	#region ThemePainter support

	/* TrackBar Theme painter class*/
	internal class ThemePainter_TrackBar
	{
		static private Color light = Color.FromArgb (255, 255, 255, 255);
		static private Color disabled = Color.FromArgb (255, 172, 168, 153);
		static private Color dark = Color.FromArgb (255, 113, 111, 100);
		static private Color main = Color.FromArgb (255, 236, 233, 216);
		static private Color focus = Color.Black;
		static private SolidBrush br_light = new SolidBrush (light);
		static private SolidBrush br_main = new SolidBrush (main);
		static private SolidBrush br_dark = new SolidBrush (dark);
		static private Pen pen_light = new Pen (light);
		static private Pen pen_dark = new Pen (dark);
		static private Pen pen_ticks = new Pen (Color.Black);
		static private Pen pen_disabled = new Pen (disabled);
		static private SolidBrush br_arrow = new SolidBrush (Color.Black);
		static private SolidBrush br_disabled = new SolidBrush (disabled);
		static private HatchBrush br_focus = new HatchBrush (HatchStyle.Percent50, main, focus);

		static public void DrawTrackBar (Graphics dc, Rectangle area, ref Rectangle thumb_pos,
			 ref Rectangle thumb_area, TickStyle style, int ticks, Orientation orientation, bool focused)
		{
			if (orientation == Orientation.Vertical)
				DrawTrackBar_Vertical (dc, area, ref thumb_pos, ref thumb_area,
					style, ticks, focused);
			else
				DrawTrackBar_Horitonzal (dc, area, ref thumb_pos, ref thumb_area,
					style, ticks, focused);
		}

		/* Vertical trackbar */
		static internal void DrawTrackBar_Vertical (Graphics dc, Rectangle area, ref Rectangle thumb_pos,
			 ref Rectangle thumb_area, TickStyle style, int ticks, bool focused)
		{

			/* Background */
			dc.FillRectangle (br_main, area);

			if (focused)  {
				dc.FillRectangle (br_focus, area.X, area.Y, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y + area.Height - 1, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y, 1, area.Height - 1);
				dc.FillRectangle (br_focus, area.X + area.Width - 1, area.Y, 1, area.Height - 1);
			}
			
			int space_from_top = 8;
			int space_from_bottom = 7;
			int y = area.Y + space_from_top;
			y += space_from_bottom;
			int x = area.X;

			switch (style) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				x += 10;
				break;
			case TickStyle.TopLeft:
				x += (area.Width / 2);
				break;
			case TickStyle.Both:
				x += 18;
				break;
			default:
				break;
			}

			
			thumb_area.X = area.X + 2;
			thumb_area.Y = area.Y + space_from_top;
			thumb_area.Height = area.Height - space_from_top - space_from_bottom;
			thumb_area.Width = 4;

			/* Draw channel */
			
			dc.FillRectangle (br_disabled, x, y,
				1, area.Height - (y - area.Y) - space_from_bottom);

			dc.FillRectangle (br_dark, x + 1, y,
				1, area.Height - (y - area.Y) - space_from_bottom);

			dc.FillRectangle (br_light, x + 3, y,
				1, area.Height - (y - area.Y) - space_from_bottom);

			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (style) {
			case TickStyle.BottomRight:
			case TickStyle.None:
			{
				//Console.WriteLine ("TickStyle.BottomRight");
				thumb_pos.X = area.X + 2;
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X, thumb_pos.Y);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X +16, thumb_pos.Y);
				dc.DrawLine (pen_light, thumb_pos.X  + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);

				dc.DrawLine (pen_disabled, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X +16, thumb_pos.Y  + 9);
				dc.DrawLine (pen_disabled, thumb_pos.X + 16, thumb_pos.Y  + 9, thumb_pos.X +16 + 4, thumb_pos.Y +9 - 4);

				dc.DrawLine (pen_dark, thumb_pos.X, thumb_pos.Y  + 10 , thumb_pos.X  +16, thumb_pos.Y+10);
				dc.DrawLine (pen_dark, thumb_pos.X + 16, thumb_pos.Y + 10, thumb_pos.X +16 + 5, thumb_pos.Y +10 - 5);

				dc.FillRectangle (br_main, thumb_pos.X + 1, thumb_pos.Y + 1, 14, 8);

				break;
			}
			case TickStyle.TopLeft:
			{
				thumb_pos.X = x - 10;

				dc.DrawLine (pen_light, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X  + 4 + 16, thumb_pos.Y);
				dc.DrawLine (pen_light, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

				dc.DrawLine (pen_disabled, thumb_pos.X + 4, thumb_pos.Y  + 9, thumb_pos.X + 4 + 16, thumb_pos.Y + 9);
				dc.DrawLine (pen_disabled, thumb_pos.X + 4, thumb_pos.Y + 9, thumb_pos.X, thumb_pos.Y + 5);
				dc.DrawLine (pen_disabled, thumb_pos.X + 19, thumb_pos.Y + 9, thumb_pos.X + 19 , thumb_pos.Y + 9);

				dc.DrawLine (pen_dark, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X + 4 + 16, thumb_pos.Y + 10);
				dc.DrawLine (pen_dark, thumb_pos.X + 4, thumb_pos.Y + 10, thumb_pos.X -1, thumb_pos.Y  + 5);
				dc.DrawLine (pen_dark, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X  + 20, thumb_pos.Y + 10);

				dc.FillRectangle (br_main, thumb_pos.X + 5, thumb_pos.Y + 1, 14, 8);
				break;
			}

			case TickStyle.Both:
			{
				thumb_pos.X = area.X + 10;
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X , thumb_pos.Y + 9);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 20, thumb_pos.Y);

				dc.DrawLine (pen_disabled, thumb_pos.X + 1, thumb_pos.Y  + 9, thumb_pos.X  + 20, thumb_pos.Y + 9);
				dc.DrawLine (pen_disabled, thumb_pos.X  + 20, thumb_pos.Y + 1, thumb_pos.X  + 20, thumb_pos.Y + 8);

				dc.DrawLine (pen_dark, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X  + 21, thumb_pos.Y + 10);
				dc.DrawLine (pen_dark, thumb_pos.X + 21, thumb_pos.Y, thumb_pos.X + 21, thumb_pos.Y + 9);

				dc.FillRectangle (br_main, thumb_pos.X + 1, thumb_pos.Y + 1, 19, 8);

				break;
			}
			default:
				break;
			}
			
			/* Draw ticks*/
			if ((style & TickStyle.BottomRight) == TickStyle.BottomRight ||
				((style & TickStyle.Both) == TickStyle.Both)) {

				float pixel_len = thumb_area.Height - 10;
				float ticks_to_draw = pixel_len / ticks;

				if (ticks_to_draw < 0)
					ticks_to_draw = 1;

				for (int inc = 0; inc < pixel_len; inc += (int) ticks_to_draw){					
					if (inc == 0 || (inc + (int) ticks_to_draw) >= pixel_len)
						dc.DrawLine (pen_ticks,  thumb_pos.X + 23, thumb_area.Y + space_from_bottom - 2 + inc, 
							thumb_pos.X + 26, thumb_area.Y + space_from_bottom - 2 + inc);
					else
						dc.DrawLine (pen_ticks,  thumb_pos.X + 23, thumb_area.Y + space_from_bottom - 2 + inc, 
							thumb_pos.X + 25, thumb_area.Y + space_from_bottom - 2 + inc);
					
				}
			}
			
			if ((style & TickStyle.TopLeft) == TickStyle.TopLeft ||
				((style & TickStyle.Both) == TickStyle.Both)) {

				float pixel_len = thumb_area.Height - 10;
				float ticks_to_draw = pixel_len / ticks;

				if (ticks_to_draw < 0)
					ticks_to_draw = 1;

				for (int inc = 0; inc < pixel_len; inc += (int) ticks_to_draw)
					if (inc == 0 || (inc + (int) ticks_to_draw) >= pixel_len)
						dc.DrawLine (pen_ticks, area.X + 4, thumb_area.Y + 5 + inc,  area.X + 8,  thumb_area.Y + 5 + inc);
					else
						dc.DrawLine (pen_ticks, area.X + 5, thumb_area.Y + 5 + inc,  area.X + 8,  thumb_area.Y + 5 + inc);
			}

		}

		/* Horizontal trackbar */
		static internal void DrawTrackBar_Horitonzal (Graphics dc, Rectangle area, ref Rectangle thumb_pos,
			 ref Rectangle thumb_area, TickStyle style, int ticks, bool focused)
		{

			/* Background */
			dc.FillRectangle (br_main, area);

			if (focused)  {
				dc.FillRectangle (br_focus, area.X, area.Y, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y + area.Height - 1, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y, 1, area.Height - 1);
				dc.FillRectangle (br_focus, area.X + area.Width - 1, area.Y, 1, area.Height - 1);
			}
			
			int space_from_right = 8;
			int space_from_left = 7;
			int x = space_from_left;
			int y = area.Y;

			switch (style) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				y += (area.Height / 4);
				break;
			case TickStyle.TopLeft:
				y += (area.Height / 2);
				break;
			case TickStyle.Both:
				y += 18;
				break;
			default:
				break;
			}

			thumb_area.X = area.X + space_from_left;
			thumb_area.Y = area.Y + 2;
			thumb_area.Width = area.Width - space_from_right - space_from_left;
			thumb_area.Height = 4;

			/* Draw channel */
			dc.FillRectangle (br_disabled, x, y,
				area.Width - (x - area.X) - space_from_right, 1);

			dc.FillRectangle (br_dark, x, y + 1,
				area.Width - (x - area.X) - space_from_right, 1);

			dc.FillRectangle (br_light, x, y + 3,
				area.Width - (x - area.X) - space_from_right, 1);


			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (style) {
			case TickStyle.BottomRight:
			case TickStyle.None:
			{
				thumb_pos.Y = area.Y + 2;
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y, thumb_pos.X +9, thumb_pos.Y +16);
				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X +9 - 4, thumb_pos.Y +16 + 4);

				dc.DrawLine (pen_dark, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y +16);
				dc.DrawLine (pen_dark, thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X +10 - 5, thumb_pos.Y +16 + 5);

				dc.FillRectangle (br_main, thumb_pos.X + 1, thumb_pos.Y + 1, 9, 16);

				break;
			}
			case TickStyle.TopLeft:
			{
				thumb_pos.Y = y - 10;

				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1 , thumb_pos.Y +19);

				dc.DrawLine (pen_dark, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_dark, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y -1);
				dc.DrawLine (pen_dark, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

				dc.FillRectangle (br_main, thumb_pos.X + 1, thumb_pos.Y + 5, 8, 14);

				break;
			}

			case TickStyle.Both:
			{
				thumb_pos.Y = area.Y + 10;
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
				dc.DrawLine (pen_light, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 20);

				dc.DrawLine (pen_disabled, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 20);
				dc.DrawLine (pen_disabled, thumb_pos.X + 1, thumb_pos.Y + 20, thumb_pos.X + 8, thumb_pos.Y + 20);

				dc.DrawLine (pen_dark, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y + 21);
				dc.DrawLine (pen_dark, thumb_pos.X, thumb_pos.Y + 21, thumb_pos.X + 9, thumb_pos.Y + 21);

				dc.FillRectangle (br_main, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 19);

				break;
			}

			default:
				break;
			}

			/* Draw ticks*/
			if ((style & TickStyle.BottomRight) == TickStyle.BottomRight ||
				((style & TickStyle.Both) == TickStyle.Both)) {

				float pixel_len = thumb_area.Width - 10;
				float ticks_to_draw = pixel_len / ticks;

				if (ticks_to_draw < 0)
					ticks_to_draw = 1;

				for (int inc = 0; inc < pixel_len; inc += (int) ticks_to_draw)
					if (inc == 0 || (inc + (int) ticks_to_draw) >= pixel_len)
						dc.DrawLine (pen_ticks, thumb_area.X + space_from_left - 2 + inc, thumb_pos.Y + 25, thumb_area.X + space_from_left - 2 + inc, thumb_pos.Y + 28);
					else
						dc.DrawLine (pen_ticks, thumb_area.X + space_from_left - 2 + inc, thumb_pos.Y + 25, thumb_area.X + space_from_left - 2 + inc, thumb_pos.Y + 27);

			}

			if ((style & TickStyle.TopLeft) == TickStyle.TopLeft ||
				((style & TickStyle.Both) == TickStyle.Both)) {

				float pixel_len = thumb_area.Width - 10;
				float ticks_to_draw = pixel_len / ticks;

				if (ticks_to_draw < 0)
					ticks_to_draw = 1;

				for (int inc = 0; inc < pixel_len; inc += (int) ticks_to_draw)
					if (inc == 0 || (inc + (int) ticks_to_draw) >= pixel_len)
						dc.DrawLine (pen_ticks, thumb_area.X + 5 + inc, area.Y + 5, thumb_area.X + 5 + inc, area.Y + 9);
					else
						dc.DrawLine (pen_ticks, thumb_area.X + 5 + inc, area.Y + 6, thumb_area.X + 5 + inc, area.Y + 8);
			}
		}
	}

	#endregion // ThemePainter support

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
		private Bitmap bmp_mem = null;
		private Graphics dc_mem = null;
		private Rectangle paint_area = new Rectangle ();
		private Rectangle thumb_pos = new Rectangle ();	 /* Current position and size of the thumb */
		private Rectangle thumb_area = new Rectangle (); /* Area where the thumb can scroll */
		private bool thumb_pressed = false;
		private int thumb_pixel_click_move;
		private float pixel_per_pos = 0;		

		#region Events
		public event EventHandler scroll_event;
		public event EventHandler valuechanged_event;		

		public void add_Scroll (System.EventHandler value)
		{
			scroll_event = value;
		}
		
		
		public void add_ValueChanged (System.EventHandler value)
		{			
			valuechanged_event = value;
		}

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
			scroll_event = null;
			valuechanged_event  = null;		

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
				Invalidate ();
			}
		}

		public int Maximum {
			get { return maximum; }
			set {
				maximum = value;

				if (maximum < minimum)
					minimum = maximum;

				Invalidate ();
			}
		}

		public int Minimum {
			get { return minimum; }
			set {
				minimum = value;

				if (minimum > maximum)
					maximum = minimum;

				Invalidate ();
			}
		}

		public Orientation Orientation {
			get { return orientation; }
			set {
				/* Orientation can be changed once the control has been created*/
				orientation = value;
				
				int old_witdh = Width;
				Width = Height;
				Height = old_witdh;
				Invalidate(); 
			}
		}

		public int SmallChange {
			get { return smallChange;}
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				smallChange = value;
				Invalidate ();
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
					Invalidate ();
				}
			}
		}

		public TickStyle TickStyle {
			get { return tickStyle; }
			set { tickStyle = value ;}
		}


		public int Value {
			get { return position; }
			set {
				if (value < Minimum || value > Maximum)
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
				
				if (position != value) {													
					position = value;					
					
					if (valuechanged_event != null)				
						valuechanged_event (this, new EventArgs ());
						
					Invalidate ();
				}				
			}
		}

		#endregion //Public Properties

		#region Public Methods

		public void SetRange (int minValue, int maxValue)
		{
			Minimum = minValue;
			Maximum = maxValue;

			Invalidate ();
		}

		public override string ToString()
		{
			return string.Format("System.Windows.Forms.Trackbar, Minimum: {0}, Maximum: {1}, Value: {2}",
						Minimum, Maximum, Value);
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = "BUTTON";

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

		void ISupportInitialize.BeginInit()
		{

		}

		void ISupportInitialize.EndInit()
		{

		}

		#endregion Public Methods

		#region Private Methods

		private void fire_scroll_event ()
		{
			if (scroll_event == null)
				return;

			scroll_event (this, new EventArgs ());
		}

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

		private void LargeIncrement()
    		{
    			//Console.WriteLine ("large inc: {0} {1}", position, LargeChange);
			UpdatePos (position + LargeChange, true);

			fire_scroll_event ();
    		}

    		private void LargeDecrement()
    		{
    			//Console.WriteLine ("large dec: {0} {1}", position, LargeChange);
			UpdatePos (position - LargeChange, true);

			fire_scroll_event ();
    		}

    		private void UpdatePixelPerPos ()
    		{
			pixel_per_pos = ((float)(thumb_area.Width)
				/ (float) (1 + Maximum - Minimum));
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

    			//Console.WriteLine ("UpdatePos: " + newPos + " ; " + Value);

    			//Console.WriteLine ("event : {0} {1} {2}", scroll_event != null, position, old);

    			if (orientation == Orientation.Horizontal) {
				if (update_trumbpos)
					UpdateThumbPos (thumb_area.X + (int)(((float)(Value - Minimum)) * pixel_per_pos), false);
			}
			else {
				if (update_trumbpos)
					UpdateThumbPos (thumb_area.Y + (int)(((float)(Value - Minimum)) * pixel_per_pos), false);
			}
    		}

    		#endregion // Private Methods

    		#region Override Event Methods

		protected override void OnResize (EventArgs e)
    		{
    			//Console.WriteLine ("OnResize");
    			base.OnResize (e);

    			if (Width <= 0 || Height <= 0)
    				return;

			UpdateArea ();

			/* Area for double buffering */
			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
    		}


		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated(e);
			//Console.WriteLine ("OnHandleCreated");
			UpdateArea ();

			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);

			UpdatePos (Value, true);
			UpdatePixelPerPos ();

			draw();
			UpdatePos (Value, true);

			if (AutoSize)
				if (Orientation == Orientation.Horizontal)
					Size = new Size (Width, 45);
				else
					Size = new Size (50, Height);
		}


		/* Disable background painting to avoid flickering, since we do our painting*/
		protected override void OnPaintBackground (PaintEventArgs pevent)
    		{
    			// None
    		}

		private void draw ()
		{
			int ticks = (Maximum - Minimum)	/ tickFrequency;

			ThemePainter_TrackBar.DrawTrackBar (dc_mem, paint_area, ref thumb_pos, ref thumb_area,
				tickStyle, ticks, Orientation, Focused);
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{		
			//Console.WriteLine ("OnDraw");

			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			/* Copies memory drawing buffer to screen*/
			UpdateArea ();
			draw();
			pevent.Graphics.DrawImage (bmp_mem, 0, 0);
		}

		protected override void OnMouseDown (MouseEventArgs e)
    		{
    			if (!Enabled) return;
    			
    			//System.Console.WriteLine ("OnMouseDown" + thumb_pos);			
    			
    			Point point = new Point (e.X, e.Y);

			if (orientation == Orientation.Horizontal) {

				if (thumb_pos.Contains (point)) {
					thumb_pressed = true;
					Invalidate ();
					thumb_pixel_click_move = e.X;
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement ();
						else
							LargeDecrement ();

						Invalidate ();
					}
				}
			}
			else {
				if (thumb_pos.Contains (point)) {
					thumb_pressed = true;
					Invalidate ();
					thumb_pixel_click_move = e.Y;
				}
				else {
					if (paint_area.Contains (point)) {
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeIncrement ();
						else
							LargeDecrement ();

						Invalidate ();
					}
				}

			}
    		}

    		protected override void OnMouseMove (MouseEventArgs e)
    		{    			
    			if (!Enabled) return;
    			
    			//System.Console.WriteLine ("OnMouseMove " + thumb_pressed);
    			Point pnt = new Point (e.X, e.Y);

    			/* Moving the thumb */
    			if (thumb_pos.Contains (pnt) && thumb_pressed) {

    				System.Console.WriteLine ("OnMouseMove " + thumb_pressed);

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


    				fire_scroll_event ();

				//System.Console.WriteLine ("OnMouseMove thumb "+ e.Y
				//	+ " clickpos " + thumb_pixel_click_move   + " pos:" + thumb_pos.Y);

				Invalidate ();
			}

			if (!thumb_pos.Contains (pnt) && thumb_pressed) {
    				thumb_pressed = false;
				Invalidate ();
			}

    		}
    		
    		protected override void OnMouseWheel (MouseEventArgs e) 
    		{
			if (!Enabled) return;
    			
    			//System.Console.WriteLine ("OnMouseWheel delta: " + e.Delta + " clicks:" + e.Clicks);
    					
    		}
    		
    		#endregion //Override Event Methods
	}
}

