//
// Sample application for region graphics functions using Rects implementation
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
public class Regions
{	
	public static void DumpRegion (Region rgn)
	{
		Matrix matrix = new Matrix ();		
		RectangleF [] rects = rgn.GetRegionScans (matrix);

		for (int i = 0; i < rects.Length; i++)
			Console.WriteLine ( rects[i]);
	}
	
	public static void Main () 
	{
		Bitmap bmp = new Bitmap (600, 800);
		Graphics dc = Graphics.FromImage (bmp);        		
		Font fnt = new Font ("Arial", 8);
		Font fnttitle = new Font("Arial", 8, FontStyle.Underline);
		Matrix matrix = new Matrix ();		
		int x = 0;
		Rectangle rect1, rect2, rect3, rect4;		
		Region rgn1, rgn2, rgn3, rgn4;
				
		bool complement = true, exclude = true, union = true, xor = true, intersect = true;
		
		SolidBrush whiteBrush = new SolidBrush (Color.White);				
		
		dc.DrawString ("Region samples using two Rectangle classes", fnttitle, whiteBrush, 5, 5);
				
		/* First */				
		if (complement) {	
			rect1 = new Rectangle (20, 30, 60, 80);		
			rect2 = new Rectangle (50, 40, 60, 80);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);						
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			rgn1.Complement (rgn2);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Complement ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 10, 130);						
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));		
			DumpRegion (rgn1);
		}
		
		/* Second */		
		if (exclude) {
			rect3 = new Rectangle (130, 30, 60, 80);		
			rect4 = new Rectangle (170, 40, 60, 80);		
			rgn3 = new Region (rect3);
			rgn4 = new Region (rect4);			
			dc.DrawRectangle (Pens.Green, rect3);		
			dc.DrawRectangle (Pens.Red, rect4);
			rgn3.Exclude (rgn4);
			dc.FillRegion (Brushes.Blue, rgn3);		
			dc.DrawString ("Exclude ("  + rgn3.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 130, 130);
			dc.DrawRectangles (Pens.Yellow, rgn3.GetRegionScans (matrix));
			DumpRegion (rgn3);
		}
		
		/* Third */
		if (intersect) {		
			
			Rectangle rect5 = new Rectangle (260, 30, 60, 80);		
			Rectangle rect6 = new Rectangle (290, 40, 60, 80);		
			Region rgn5 = new Region (rect5);
			Region rgn6 = new Region (rect6);			
			dc.DrawRectangle (Pens.Green, rect5);		
			dc.DrawRectangle (Pens.Red, rect6);
			rgn5.Intersect (rgn6);
			dc.FillRegion (Brushes.Blue, rgn5);		
			dc.DrawString ("Intersect ("  + rgn5.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 270, 130);
			dc.DrawRectangles (Pens.Yellow, rgn5.GetRegionScans (matrix));
			DumpRegion (rgn5);
		}
		
		/* Four */		
		if (xor) {
			Rectangle rect7 = new Rectangle (380, 30, 60, 80);		
			Rectangle rect8 = new Rectangle (410, 40, 60, 80);		
			Region rgn7 = new Region (rect7);
			Region rgn8 = new Region (rect8);			
			dc.DrawRectangle (Pens.Green, rect7);		
			dc.DrawRectangle (Pens.Red, rect8);
			rgn7.Xor (rgn8);
			dc.FillRegion (Brushes.Blue, rgn7);		
			dc.DrawString ("Xor ("  + rgn7.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 400, 130);
			dc.DrawRectangles (Pens.Yellow, rgn7.GetRegionScans (matrix));
			DumpRegion (rgn7);
		}
		
		/* Fifht */		
		if (union) {
			Rectangle rect9 = new Rectangle (500, 30, 60, 80);		
			Rectangle rect10 = new Rectangle (520, 40, 60, 80);		
			Region rgn9 = new Region(rect9);
			Region rgn10 = new Region(rect10);			
			dc.DrawRectangle (Pens.Green, rect9);		
			dc.DrawRectangle (Pens.Red, rect10);
			rgn9.Union(rgn10);
			dc.FillRegion (Brushes.Blue, rgn9);		
			dc.DrawString ("Union (" + rgn9.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 530, 130);		
			dc.DrawRectangles (Pens.Yellow, rgn9.GetRegionScans (matrix));		
			DumpRegion (rgn9);
		}
		
		dc.DrawString ("Region samples using three Rectangle class", fnttitle, whiteBrush, 5, 155);
		
		/* First */		
		x = 0;		
		
		if (complement) {	
			rect1 = new Rectangle (20+x, 180, 40, 50);		
			rect2 = new Rectangle (50+x, 190, 40, 50);		
			rect3 = new Rectangle (70+x, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			
			rgn1.Complement (rgn2);
			rgn1.Complement (rgn3);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Complement (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 10, 275);	
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));	
			DumpRegion (rgn1);
		}
		x += 110;
		
		/* Second */				
		if (exclude) {	
			rect1 = new Rectangle (20+x, 180, 40, 50);		
			rect2 = new Rectangle (50+x, 190, 40, 50);		
			rect3 = new Rectangle (70+x, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			
			rgn1.Exclude (rgn2);
			rgn1.Exclude (rgn3);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Exclude (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 130, 275);		
			DumpRegion (rgn1);
		}
		x += 110;
		
		/* Third */	
		if (intersect) {				
			
			rect1 = new Rectangle (20+x, 180, 40, 50);		
			rect2 = new Rectangle (50+x, 190, 40, 50);		
			rect3 = new Rectangle (70+x, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			
			rgn1.Union (rgn2);
			rgn1.Intersect (rgn3);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Intersect (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 270, 275);		
			DumpRegion (rgn1);
			
		}
		x += 110;
		
		/* Fourth */				
		if (xor) {	
			rect1 = new Rectangle (20+x, 180, 40, 50);		
			rect2 = new Rectangle (50+x, 190, 40, 50);		
			rect3 = new Rectangle (70+x, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			
			rgn1.Union (rgn2);
			rgn1.Xor (rgn3);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Xor ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 380, 275);		
			DumpRegion (rgn1);
		}
		x += 110;
		
		/* Fifth */				
		if (union) {	
			rect1 = new Rectangle (20+x, 180, 40, 50);		
			rect2 = new Rectangle (50+x, 190, 40, 50);		
			rect3 = new Rectangle (70+x, 210, 30, 50);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Union (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 500, 275);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			DumpRegion (rgn1);
		}
		x += 110;		
		
		dc.DrawString ("Region samples using four Rectangle class", fnttitle, whiteBrush, 5, 300);
		
		/* First */		
		x = 0;
		
		if (complement) {	
			rect1 = new Rectangle (20+x, 330, 40, 50);		
			rect2 = new Rectangle (50+x, 340, 40, 50);		
			rect3 = new Rectangle (70+x, 360, 30, 50);		
			rect4 = new Rectangle (80+x, 400, 30, 10);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			dc.DrawRectangle (Pens.Red, rect4);
			
			rgn1.Complement (rgn2);
			rgn1.Complement (rgn3);
			rgn1.Complement (rgn4);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Complement (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 10, 430);	
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));	
			DumpRegion (rgn1);
		}
		x += 110;
		
		/* Second */				
		if (exclude) {	
			rect1 = new Rectangle (20+x, 330, 40, 50);		
			rect2 = new Rectangle (50+x, 340, 40, 50);		
			rect3 = new Rectangle (70+x, 360, 30, 50);		
			rect4 = new Rectangle (80+x, 400, 30, 10);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			dc.DrawRectangle (Pens.Red, rect4);
			
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rgn1.Exclude (rgn4);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Exclude (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 130, 430);		
		}
		x += 110;
		
		/* Third */				
		if (intersect) {	

			rect1 = new Rectangle (20+x, 330, 40, 50);		
			rect2 = new Rectangle (50+x, 340, 40, 50);		
			rect3 = new Rectangle (70+x, 360, 30, 50);		
			rect4 = new Rectangle (80+x, 400, 30, 10);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			dc.DrawRectangle (Pens.Red, rect4);
			
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rgn1.Intersect (rgn4);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Intersect (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 250, 430);		
			DumpRegion (rgn1);		
		}
		x += 110;
		
		/* Fourth */				
		if (xor) {	
			rect1 = new Rectangle (20+x, 330, 40, 50);		
			rect2 = new Rectangle (50+x, 340, 40, 50);		
			rect3 = new Rectangle (70+x, 360, 30, 50);		
			rect4 = new Rectangle (80+x, 400, 30, 10);		
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			dc.DrawRectangle (Pens.Red, rect4);		
			
			rgn1.Union (rgn2);			
			rgn3.Union (rgn4);
			rgn1.Xor (rgn3);
			
			dc.FillRegion(Brushes.Blue, rgn1);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			dc.DrawString ("Xor ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 370, 430);		
			DumpRegion (rgn1);
		}
		x += 110;
		
		/* Fifth */				
		if (union) {	
			rect1 = new Rectangle (20+x, 330, 40, 50);		
			rect2 = new Rectangle (50+x, 340, 40, 50);		
			rect3 = new Rectangle (70+x, 360, 30, 50);		
			rect4 = new Rectangle (80+x, 400, 30, 10);				
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);		
			rgn3 = new Region (rect3);	
			rgn4 = new Region (rect4);	
			
			dc.DrawRectangle (Pens.Green, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawEllipse (Pens.Red, rect3);
			dc.DrawRectangle (Pens.Red, rect4);		
			
			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rgn1.Union (rgn4);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Union (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 490, 430);		
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			DumpRegion (rgn1);
		}
		x += 110;
		
		dc.DrawString ("Region samples using Regions with two Rectangles", fnttitle, whiteBrush, 5, 455);
		
		x = 0;
		
		if (complement) {	
			rect1 = new Rectangle (20+x, 330+150, 40, 50);		
			rect2 = new Rectangle (50+x, 340+150, 40, 50);		
			rect3 = new Rectangle (70+x, 360+150, 30, 50);		
			rect4 = new Rectangle (80+x, 400+150, 30, 10);		
			rgn1 = new Region (rect1);
			rgn1.Union (rect2);
			rgn2 = new Region (rect3);	
			rgn2.Union (rect4);
			
			dc.DrawRectangle (Pens.Red, rect1);				
			dc.DrawRectangle (Pens.Red, rect2);
			dc.DrawRectangle (Pens.Green, rect3);
			dc.DrawRectangle (Pens.Green, rect4);
			
			rgn1.Complement (rgn2);
			dc.FillRegion (Brushes.Blue, rgn1);		
			dc.DrawString ("Complement (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 10, 430+150);	
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));	
			DumpRegion (rgn1);
		}
		x += 110;

		dc.DrawString ("Special cases (old bugs)", fnttitle, whiteBrush, 5, 610);
		
		x = 0;

		if (xor) {
			rect1 = new Rectangle (20+x, 330+300, 40, 50);
			rect2 = new Rectangle (40+x, 360+300, 20, 20);
			dc.DrawRectangle (Pens.Red, rect1);
			dc.DrawRectangle (Pens.Green, rect2);
			rgn1 = new Region (rect1);
			rgn1.Xor (rect2);
			dc.FillRegion (Brushes.Blue, rgn1);
			dc.DrawString ("Xor (" + rgn1.GetRegionScans (matrix).Length +") #77408", fnt, whiteBrush, 10, 430+300);
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
			DumpRegion (rgn1);
		}
		
		rect1 = new Rectangle (1, 1, 4, 1);		
		dc.DrawRectangle (Pens.Pink, rect1);				
		
		
        	bmp.Save("regionsrc.bmp", ImageFormat.Bmp);				
	}	

}


