//
// Sample application for ImageAttributes
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
		Bitmap outbmp = new Bitmap (600, 600);				
		Bitmap bmp = new Bitmap("../../Test/System.Drawing/bitmaps/almogaver32bits.bmp");
		Graphics dc = Graphics.FromImage (outbmp);        
		SolidBrush br = new SolidBrush(Color.White);
		Bitmap img = bmp.Clone (new Rectangle (0,0, 60,60) , PixelFormat.Format32bppArgb);									
		
		ImageAttributes imageAttr = new ImageAttributes();
		
		Bitmap	bmpred = new Bitmap (100,100, PixelFormat.Format32bppArgb);		
		Graphics gr = Graphics.FromImage (bmpred);		
		
		/* Sample drawing*/
		Pen cyan = new Pen(Color.Cyan, 0);
		Pen green = new Pen(Color.Green, 0);
		Pen pink = new Pen(Color.Pink, 0);			
		Pen blue = new Pen(Color.Blue, 0);			
		gr.DrawLine(cyan, 10.0F, 10.0F, 90.0F, 90.0F);
		gr.DrawLine(pink, 10.0F, 30.0F, 90.0F, 30.0F);
		gr.DrawLine(green, 10.0F, 50.0F, 90.0F, 50.0F);
		gr.DrawRectangle (blue, 10.0F, 10.0F, 80.0F, 80.0F);				
		
		/* Draw image without any imageattributes*/		
		dc.DrawImage (bmpred, 0,0);				
		dc.DrawString ("Sample drawing", new Font ("Arial", 8), br,  10, 100);				
		
		/* Remmaping colours */
		ColorMap[] clr = new ColorMap[1];	
		clr[0] = new ColorMap(); 
		clr[0].OldColor = Color.Blue;
		clr[0].NewColor = Color.Yellow;	
		
		imageAttr.SetRemapTable (clr, ColorAdjustType.Bitmap);					
		dc.DrawImage (bmpred, new Rectangle (100, 0, 100,100), 0,0, 100,100, GraphicsUnit.Pixel, imageAttr);			
		dc.DrawString ("Remapping colors", new Font ("Arial", 8), br,  110, 100);				
		
		/* Gamma correction on*/
		imageAttr = new ImageAttributes();
		imageAttr.SetGamma (2);		
		dc.DrawImage (bmpred, new Rectangle (200, 0, 100,100), 0,0, 
			100,100, GraphicsUnit.Pixel, imageAttr);
			
		dc.DrawString ("Gamma corrected", new Font ("Arial", 8), br,  210, 100);				
		
		/* WrapMode: TitleX */
		imageAttr = new ImageAttributes();	
		imageAttr.SetWrapMode (WrapMode.TileFlipX);	
		
		dc.DrawImage (bmpred, new Rectangle (0, 120, 200, 200), 0,0, 
			200, 200, GraphicsUnit.Pixel, imageAttr);					
			
		dc.DrawString ("WrapMode.TileFlipX", new Font ("Arial", 8), br,  10, 320);								
			
		/* WrapMode: TitleY */		
		imageAttr.SetWrapMode (WrapMode.TileFlipY);	
		
		dc.DrawImage (bmpred, new Rectangle (200, 120, 200, 200), 0,0, 
			200, 200, GraphicsUnit.Pixel, imageAttr);				
			
		dc.DrawString ("WrapMode.TileFlipY", new Font ("Arial", 8), br,  210, 320);											
			
		/* WrapMode: TitleXY */		
		imageAttr.SetWrapMode (WrapMode.TileFlipXY);	
		
		dc.DrawImage (bmpred, new Rectangle (400, 120, 200, 200), 0,0, 
			200, 200, GraphicsUnit.Pixel, imageAttr);				
			
		dc.DrawString ("WrapMode.TileFlipXY", new Font ("Arial", 8), br,  410, 320);														
		
		outbmp.Save("imageattributes.bmp", ImageFormat.Bmp);				
		
	}

}


