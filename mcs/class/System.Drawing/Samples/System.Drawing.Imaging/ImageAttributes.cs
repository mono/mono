//
// Sample application for ImageAttributes
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
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
		Bitmap outbmp = new Bitmap (500, 500);				
		Bitmap bmp = new Bitmap("../../Test/System.Drawing/bitmaps/almogaver24bits.bmp");
		Graphics dc = Graphics.FromImage (outbmp);        
		SolidBrush br = new SolidBrush(Color.White);
		
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
		dc.DrawString ("Sample drawing", new Font ("Arial", 8), br,  10, 110);				
		
		/* Remmaping colours */
		ColorMap[] clr = new ColorMap[1];	
		clr[0] = new ColorMap(); 
		clr[0].OldColor = Color.Blue;
		clr[0].NewColor = Color.Yellow;	
		
		imageAttr.SetRemapTable (clr, ColorAdjustType.Bitmap);					
		dc.DrawImage (bmpred, new Rectangle (100, 0, 100,100), 0,0, 100,100, GraphicsUnit.Pixel, imageAttr);			
		dc.DrawString ("Remapping colors", new Font ("Arial", 8), br,  110, 110);				
		
		/* Gamma correction on*/
		imageAttr = new ImageAttributes();
		imageAttr.SetGamma (2);		
		dc.DrawImage (bmpred, new Rectangle (200, 0, 100,100), 0,0, 
			100,100, GraphicsUnit.Pixel, imageAttr);
			
		dc.DrawString ("Gamma corrected", new Font ("Arial", 8), br,  210, 110);				
				
		
		outbmp.Save("drawimage.bmp", ImageFormat.Bmp);				
		
	}

}


