//
// Sample application for region graphics functions using GraphicsPaths implementation
//
// Author:
//   Jordi Mas i Hernandez, jordi@ximian.com
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
	
	public static void Main () 
	{

		Bitmap bmp = new Bitmap (600, 300);
		Graphics dc = Graphics.FromImage (bmp);        		
		Font fnt = new Font ("Arial", 8);
		Font fnttitle = new Font ("Arial", 8, FontStyle.Underline);
		Matrix matrix = new Matrix ();		
		GraphicsPath patha = new GraphicsPath ();
		GraphicsPath pathb = new GraphicsPath ();
		Pen redPen = new Pen (Color.Red, 2);		
		Region rgn1;
		Region rgn2;		
		int x = 0;		
		
		SolidBrush whiteBrush = new SolidBrush (Color.White);				
		
		dc.DrawString ("Region samples using GraphicsPath", fnttitle, whiteBrush, 5, 5);				
		
		/* First*/		
		patha.AddLine (60, 40, 90, 90);
		patha.AddLine (90, 90, 10, 90);
		patha.AddLine (10, 90, 60, 40);			
		dc.DrawPath (redPen, patha);		
				
		pathb.AddEllipse(30, 55, 60, 60);
		dc.DrawPath(redPen, pathb);
				
		rgn1 = new Region (patha);
		rgn2 = new Region (pathb);				
		rgn1.Complement (rgn2);
		dc.FillRegion (Brushes.Blue, rgn1);			
		dc.DrawString ("Complement (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 10, 140);	
		dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
		x += 110;
		
		/* Second*/		
		patha.Reset ();
		pathb.Reset ();
		patha.AddLine (60+x, 40, 90+x, 90);
		patha.AddLine (90+x, 90, 10+x, 90);
		patha.AddLine (10+x, 90, 60+x, 40);					
		
		dc.DrawPath (redPen, patha);						
				
		pathb.AddEllipse (30+x, 55, 60, 60);						
		dc.DrawPath(redPen, pathb);
				
		rgn1 = new Region (patha);
		rgn2 = new Region (pathb);				
		rgn1.Exclude (rgn2);
		dc.FillRegion (Brushes.Blue, rgn1);				
		dc.DrawString ("Exclude ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 140, 140);	
		dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
		x += 110;
		
		/* Third*/		
		patha.Reset ();
		pathb.Reset ();
		patha.AddLine (60+x, 40, 90+x, 90);
		patha.AddLine (90+x, 90, 10+x, 90);
		patha.AddLine (10+x, 90, 60+x, 40);			
		
		dc.DrawPath (redPen, patha);		
				
		pathb.AddEllipse (30+x, 55, 60, 60);
		dc.DrawPath (redPen, pathb);
				
		rgn1 = new Region (patha);
		rgn2 = new Region (pathb);				
		rgn1.Intersect (rgn2);
		dc.FillRegion (Brushes.Blue, rgn1);		
		dc.DrawString ("Intersect ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 270, 140);		
		dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));	
		x += 110;
		
		/* Four*/		
		patha.Reset ();
		pathb.Reset ();
		patha.AddLine (60+x, 40, 90+x, 90);
		patha.AddLine (90+x, 90, 10+x, 90);
		patha.AddLine (10+x, 90, 60+x, 40);			
		
		dc.DrawPath (redPen, patha);		
				
		pathb.AddEllipse (30+x, 55, 60, 60);
		dc.DrawPath (redPen, pathb);
				
		rgn1 = new Region (patha);
		rgn2 = new Region (pathb);				
		rgn1.Xor (rgn2);
		dc.FillRegion(Brushes.Blue, rgn1);		
		dc.DrawString ("Xor ("  + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 380, 140);		
		dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));	
		x += 110;	
		
		/* Fifth */		
		patha.Reset ();
		pathb.Reset ();
		patha.AddLine (60+x, 40, 90+x, 90);
		patha.AddLine (90+x, 90, 10+x, 90);
		patha.AddLine (10+x, 90, 60+x, 40);			
		
		dc.DrawPath (redPen, patha);		
				
		pathb.AddEllipse (30+x, 55, 60, 60);
		dc.DrawPath (redPen, pathb);
				
		rgn1 = new Region (patha);
		rgn2 = new Region (pathb);				
		rgn1.Union (rgn2);
		dc.FillRegion(Brushes.Blue, rgn1);		
		dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));
		dc.DrawString ("Union (" + rgn1.GetRegionScans (matrix).Length +")", fnt, whiteBrush, 490, 140);							
		
        	bmp.Save("regionsgp.bmp", ImageFormat.Bmp);				
	}	

}


