//
// Sample application for drawing image implementation
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

//
public class SampleDrawingImage 
{	
		
	/*  DrawImageAbort callback method */
	static private bool DrawImageCallback(IntPtr callBackData)
	{
		Console.WriteLine("DrawImageCallback");
		return false;
	}
	
	public static void Main(string[] args)
	{	
		Graphics.DrawImageAbort imageCallback;
		Bitmap outbmp = new Bitmap (300, 300);				
		Bitmap bmp = new Bitmap("../../Test/System.Drawing/bitmaps/almogaver24bits.bmp");
		Graphics dc = Graphics.FromImage (outbmp);        
		
		ImageAttributes imageAttr = new ImageAttributes();
		
		/* Simple image drawing */		
		dc.DrawImage(bmp, 0,0);				
				
		/* Drawing using points */
		PointF ulCorner = new PointF(150.0F, 0.0F);
		PointF urCorner = new PointF(350.0F, 0.0F);
		PointF llCorner = new PointF(200.0F, 150.0F);
		RectangleF srcRect = new Rectangle (0,0,100,100);		
		PointF[] destPara = {ulCorner, urCorner, llCorner};	
		imageCallback =  new Graphics.DrawImageAbort(DrawImageCallback);		
		dc.DrawImage (bmp, destPara, srcRect, GraphicsUnit.Pixel, imageAttr, imageCallback);
	
		/* Using rectangles */	
		RectangleF destRect = new Rectangle (10,200,100,100);
		RectangleF srcRect2 = new Rectangle (50,50,100,100);		
		dc.DrawImage (bmp, destRect, srcRect2, GraphicsUnit.Pixel);		
		
		/* Simple image drawing with with scaling*/		
		dc.DrawImage(bmp, 200,200, 75, 75);				
		
		outbmp.Save("drawimage.bmp", ImageFormat.Bmp);				
		
	}

}


