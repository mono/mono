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

using System;
using Cairo;
using System.Windows.Forms;
	
public class SwfCairo : Form
{
	static void Main ()
	{		
		SwfCairo f = new SwfCairo ();
		f.ShowDialog ();
	}

	protected override void OnPaint (PaintEventArgs a)
	{
		IntPtr hdc = a.Graphics.GetHdc ();
		Win32Surface s = new Win32Surface (hdc);
		Context cr = new Context (s);
		draw (cr, this.Width, this.Height);
		
		a.Graphics.ReleaseHdc (hdc);
	}
	
	static void draw (Context cr, int width, int height)
	{
		double xc = 0.5;
		double yc = 0.5;
		double radius = 0.4;
		double angle1 = 45.0  * (Math.PI / 180.0);  // angles are specified
		double angle2 = 180.0 * (Math.PI / 180.0);  // in radians
		
		cr.Scale (width, height);
		cr.LineWidth = 0.04;

		cr.Arc (xc, yc, radius, angle1, angle2);
		cr.Stroke ();
		
		// draw helping lines
		cr.Color = new Color(1, 0.2, 0.2, 0.6);
		cr.Arc (xc, yc, 0.05, 0, 2 * Math.PI);
		cr.Fill ();
		cr.LineWidth = 0.03;
		cr.Arc (xc, yc, radius, angle1, angle1);
		cr.LineTo (new PointD (xc, yc));
		cr.Arc (xc, yc, radius, angle2, angle2);
		cr.LineTo (new PointD (xc, yc));
		cr.Stroke ();
	}
}

