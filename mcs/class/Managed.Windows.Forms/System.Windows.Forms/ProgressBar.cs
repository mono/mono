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
// $Revision: 1.6 $
// $Modtime: $
// $Log: ProgressBar.cs,v $
// Revision 1.6  2004/08/10 15:41:50  jackson
// Allow control to handle buffering
//
// Revision 1.5  2004/07/26 17:42:03  jordi
// Theme support
//
// Revision 1.4  2004/07/09 20:13:05  miguel
// Spelling
//
// Revision 1.3  2004/07/09 17:25:23  pbartok
// - Removed usage of Rectangle for drawing. Miguel pointed out it's faster
//
// Revision 1.2  2004/07/09 17:17:46  miguel
// 2004-07-09  Miguel de Icaza  <miguel@ximian.com>
//
// 	* ProgressBar.cs: Fixed spelling for `block'
//
// 	drawProgressBar: renamed to `DrawProgressBar' to follow the coding
// 	style guidelines.
//
// 	Avoid using the += on rect.X, that exposed a bug in the compiler.
//
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
	public sealed class ProgressBar : Control 
	{	
		#region Local Variables
		private int maximum;
		private int minimum;
		private int step;
		private int val;	
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
			
			CreateBuffers (Width, Height);							
    		}
		
		protected override void OnHandleCreated (EventArgs e) 
		{			
			base.OnHandleCreated(e);
			
			//Console.WriteLine ("OnHandleCreated");
			
			UpdateAreas ();
			
			CreateBuffers (Width, Height);						
			Draw ();
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
		
		private void Draw ()
		{	
			int block_width, barpos_pixels;			
			int steps = (Maximum - Minimum) / step;			
			
			block_width = ((client_area.Height) * 2 ) / 3;			
			barpos_pixels = ((Value - Minimum) * client_area.Width) / (Maximum - Minimum);									
			
			//Console.WriteLine ("draw block witdh:{0} barpos: {1}", block_width, barpos_pixels);
			//Console.WriteLine ("draw Max {0} Min {1} Value {2}", 
			//	Maximum, Minimum, Value);
					
			ThemeEngine.Current.DrawProgressBar (DeviceContext, paint_area, client_area, barpos_pixels,
				block_width);
		}
		
				
		protected override void OnPaint (PaintEventArgs pevent)
		{	
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;
										
			/* Copies memory drawing buffer to screen*/		
			Draw();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);			
		}	
	}
}
