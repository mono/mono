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
// Copyright (C) 2004 Novell, Inc.
//
// Autors:
//		Jordi Mas i Hernandez	jordi@ximian.com
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ProgressBar.cs,v $
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms 
{
	/* Scroll bar Theme painter class*/
	#region ThemePainter support
	internal class ThemePainter_ProgressBar 
	{
		static private Color shadow = Color.FromArgb (255, 172, 168, 153);
		static private Color light = Color.FromArgb (255, 255, 255, 255);		
		static private SolidBrush br_shadow = new SolidBrush (shadow);
		static private SolidBrush br_light = new SolidBrush (light);
		private static SolidBrush br_main = new SolidBrush (Color.FromArgb (255, 236, 233, 216));
		private static SolidBrush br_bar = new SolidBrush (Color.FromArgb (255, 49, 106, 197));			
		private static int space_betweenblocs = 2;
		
		/* Draw a progress bar */
		static public void drawProgressBar (Graphics dc, Rectangle area, 
			Rectangle client_area, int barpos_pixels, int bloc_width)
		{	
			
			Rectangle rect = new Rectangle (client_area.X, client_area.Y,
				bloc_width, client_area.Height);	
				
			/* Background*/
			dc.FillRectangle (br_main, area);				
			
			/* Draw background*/
			while ((rect.X - client_area.X) < barpos_pixels) {            		        
            		        
                		dc.FillRectangle (br_bar, rect);
                		rect.X  += rect.Width + space_betweenblocs;
            		}			
            		
            		/* Draw border */
            		dc.FillRectangle (br_shadow, area.X, area.Y, area.Width, 1);
            		dc.FillRectangle (br_shadow, area.X, area.Y, 1, area.Height);
            		dc.FillRectangle (br_light, area.X, area.Y + area.Height - 1, area.Width, 1);
            		dc.FillRectangle (br_light, area.X + area.Width - 1, area.Y, 1, area.Height);
		}

	}
	#endregion	// ThemePainter support

	public sealed class ProgressBar : Control 
	{	
		#region Local Variables
		private int maximum;
		private int minimum;
		private int step;
		private int val;	
		private Bitmap bmp_mem = null;
		private Graphics dc_mem = null;				
		private Rectangle paint_area = new Rectangle ();		
		private Rectangle client_area = new Rectangle ();
		#endregion	// Local Variables

		#region Public Constructors
		public ProgressBar() 
		{			
			maximum = 100;
			minimum = 0;
			step = 10;
			val = 0;
			
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int Maximum {
			get {
				return maximum;
			}
			set {
				if (value < 0)
					throw new ArgumentException( 
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));
						
				maximum = value;
				Invalidate ();		
			}
		}
		
		public int Minimum {
			get {
				return minimum;
			}
			set {
				if (value < 0)
					throw new ArgumentException( 
						string.Format("Value '{0}' must be greater than or equal to 0.", value ));
						
				minimum = value;
				Invalidate ();
			}
		}

		public int Step {
			get { 	return step; }
			set {
				step = value;
				Invalidate ();
			}
		}		
		
		public int Value {
			get {
				return val;
			}
			set {
				if (value < Minimum || value > Maximum)
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));

				val = value; 
				Invalidate ();
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
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
			get {	return ImeMode.Disable;	}
		}

		protected override Size DefaultSize {
			get {	return new Size(100, 23); }
		}
		#endregion	// Protected Instance Properties
		
		#region Public Instance Methods
		public void Increment (int value) 
		{
			int newValue = Value + value;
			
			if (newValue < Minimum)
				newValue = Minimum;
				
			if (newValue > Maximum)
				newValue = Maximum;
				
			Value = newValue;
			Invalidate ();
		}

		public void PerformStep () {
			if (Value >= Maximum)
				return;
			
			Value = Value + Step;			
			Invalidate ();
		}
		#endregion	// Public Instance Methods

		private void UpdateAreas ()
		{
			paint_area.X = paint_area.Y = 0;
			paint_area.Width = Width; 
			paint_area.Height = Height;						
			
			client_area.X = client_area.Y = 2;
			client_area.Width = Width - 4; 
			client_area.Height = Height - 4;												
		}
		
		protected override void OnResize (EventArgs e) 
    		{
    			//Console.WriteLine ("Onresize");
    			base.OnResize (e);    
    			
    			if (Width <= 0 || Height <= 0)
    				return;
			
			UpdateAreas ();
			
			/* Area for double buffering */			
			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);	
			dc_mem = Graphics.FromImage (bmp_mem);								
    		}
		
		protected override void OnHandleCreated (EventArgs e) 
		{			
			base.OnHandleCreated(e);
			
			//Console.WriteLine ("OnHandleCreated");
			
			UpdateAreas ();
			
			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);	
			dc_mem = Graphics.FromImage (bmp_mem);							
			draw ();
		}
		
		public override string ToString() 
		{
			return string.Format ("{0}, Minimum: {1}, Maximum: {2}, Value: {3}", 
						GetType().FullName.ToString (),
						Maximum.ToString (),
						Minimum.ToString (),
						Value.ToString () );
		}
		
		/* Disable background painting to avoid flickering, since we do our painting*/
		protected override void OnPaintBackground (PaintEventArgs pevent) 
    		{
    			// None
    		}		
		
		private void draw ()
		{	
			int bloc_width, barpos_pixels;			
			int steps = (Maximum - Minimum) / step;			
			
			bloc_width = ((client_area.Height) * 2 ) / 3;			
			barpos_pixels = ((Value - Minimum) * client_area.Width) / (Maximum - Minimum);									
			
			//Console.WriteLine ("draw bloc witdh:{0} barpos: {1}", bloc_width, barpos_pixels);
			//Console.WriteLine ("draw Max {0} Min {1} Value {2}", 
			//	Maximum, Minimum, Value);
					
			ThemePainter_ProgressBar.drawProgressBar (dc_mem, paint_area, client_area, barpos_pixels,
				bloc_width);
		}
		
				
		protected override void OnPaint (PaintEventArgs pevent)
		{	
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;
										
			/* Copies memory drawing buffer to screen*/		
			draw();
			pevent.Graphics.DrawImage (bmp_mem, 0, 0);			
		}	
	}
}
