//
// Test application for pie graphics functions implementation
//
// Author:
//   Jordi Mas, jordi@ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

//
public class graphicsUI
{	
	
	public static void Main ()
	{

		Bitmap bmp = new Bitmap (400, 600);
		Graphics dc = Graphics.FromImage (bmp);

		// Clears and set the background color to red
		dc.Clear (Color.Red);

		SolidBrush blueBrush = new SolidBrush (Color.Blue);
		SolidBrush redBrush = new SolidBrush (Color.Red);
		SolidBrush yellowBrush = new SolidBrush (Color.Yellow);
		SolidBrush whiteBrush = new SolidBrush (Color.White);				
		Pen bluePen = new Pen (Color.Blue);

		// We have a column starting at x=50 for Draw operations
		// and another column starting at x=200 for Fill operations.
		// Both the columns grow downwards.

		// Column 1
		Rectangle rect11 = new Rectangle (50, 0, 75, 75);
		dc.DrawPie (bluePen, rect11, 10, 60);
		
		Rectangle rect12 = new Rectangle (50,100, 75, 75);
		dc.DrawPie (bluePen, rect12, 100, 75);

		Rectangle rect13 = new Rectangle (50, 200, 75, 75);
		dc.DrawPie (bluePen, rect13, 100, 400);

		Rectangle rect14 = new Rectangle (50, 300, 75, 75);
		dc.DrawPie (bluePen, rect14, 0, 0);

		// Column 2
		Rectangle rect21 = new Rectangle (200, 0, 75, 75);
		dc.FillPie (yellowBrush, rect21, 0, 300);
		
		Rectangle rect22 = new Rectangle (200, 100, 75, 75);
		dc.FillPie (whiteBrush, rect22, 200, 30);
                
		Rectangle rect23 = new Rectangle (200, 200, 75, 75);
		dc.FillPie (whiteBrush, rect23, 200, 400);

		Rectangle rect24 = new Rectangle (200, 300, 75, 75);
		dc.FillPie (yellowBrush, rect24, 190, 300);
		
		Rectangle rect25 = new Rectangle (200, 400, 75, 75);
		dc.FillPie (whiteBrush, rect25, 200, 20);

		Rectangle rect26 = new Rectangle (200, 500, 75, 75);
		dc.FillPie (yellowBrush, rect26, 50, 0);

        	bmp.Save("fillpie.png", ImageFormat.Png);
	}

}
