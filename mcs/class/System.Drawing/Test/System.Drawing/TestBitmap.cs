//
// Bitmap class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jmas@softcatala.org>
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class BitMapTest : Assertion {
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
			
		[Test]
		public void TestPixels() 
		{		
			// Tests GetSetPixel/SetPixel			
			Bitmap bmp= new Bitmap(100,100, PixelFormat.Format32bppRgb);											
			bmp.SetPixel(0,0,Color.FromArgb(255,128,128,128));					
			Color color = bmp.GetPixel(0,0);				
						
			AssertEquals (Color.FromArgb(255,128,128,128), color);
			
			bmp.SetPixel(99,99,Color.FromArgb(255,255,0,155));					
			Color color2 = bmp.GetPixel(99,99);										
			AssertEquals (Color.FromArgb(255,255,0,155), color2);			
		}
		
		[Test]
		public void BitmapLoadAndSave() 
		{							
			string sOutFile = "output/linerect.bmp";
						
			// Save		
			Bitmap	bmp = new Bitmap(100,100, PixelFormat.Format32bppRgb);						
			Graphics gr = Graphics.FromImage(bmp);		
			
			Pen p = new Pen(Color.Red, 2);
			gr.DrawLine(p, 10.0F, 10.0F, 90.0F, 90.0F);
			gr.DrawRectangle(p, 10.0F, 10.0F, 80.0F, 80.0F);
			p.Dispose();					
			bmp.Save(sOutFile, ImageFormat.Bmp);
			gr.Dispose();
			bmp.Dispose();							
			
			// Load			
			Bitmap	bmpLoad = new Bitmap(sOutFile);
			if( bmpLoad == null) 
				Console.WriteLine("Unable to load "+ sOutFile);						
			
			Color color = bmpLoad.GetPixel(10,10);		
			
			Console.WriteLine("Color "+ color);			
			AssertEquals (Color.FromArgb(255,255,0,0), color);											
		}

		[Test]
		public void MakeTransparent() 
		{
			string sInFile = "bitmaps/maketransparent.bmp";
			string sOutFile = "output/transparent.bmp";
						
			Bitmap	bmp = new Bitmap(sInFile);
			Console.WriteLine("Bitmap loaded OK", bmp != null);
					
			bmp.MakeTransparent();
			bmp.Save(sOutFile);							
			
			Color color = bmp.GetPixel(1,1);							
			AssertEquals (Color.Black.R, color.R);											
			AssertEquals (Color.Black.G, color.G);											
			AssertEquals (Color.Black.B, color.B);										
		}
		
		[Test]
		public void Clone ()
		{
			string sInFile = "bitmaps/almogaver24bits.bmp";
			string sOutFile = "output/clone24.bmp";			
			
			Rectangle rect = new Rectangle(0,0,50,50);						
			Bitmap	bmp = new Bitmap(sInFile);
			Console.WriteLine("Bitmap loaded OK", bmp != null);
			
			Bitmap bmpNew = bmp.Clone (rect, PixelFormat.Format32bppArgb);			
			bmpNew.Save(sOutFile);							
			
			Color colororg0 = bmp.GetPixel(0,0);		
			Color colororg50 = bmp.GetPixel(49,49);					
			Color colornew0 = bmpNew.GetPixel(0,0);		
			Color colornew50 = bmpNew.GetPixel(49,49);				
			
			AssertEquals (colororg0, colornew0);											
			AssertEquals (colororg50, colornew50);				
		}
		
		[Test]
		public void Handles()
		{
			// This is a simple test. We should test really if there are valid
			// handles for Win32/Wine			
			
			string sInFile = "bitmaps/almogaver24bits.bmp";
							
			Bitmap	bmp = new Bitmap(sInFile);
			IntPtr HandleBmp = bmp.GetHbitmap();
			Console.WriteLine("Handles.GetHbitmap()-> " + HandleBmp);
			Assert(HandleBmp!=IntPtr.Zero);				
			
			IntPtr HandleIcon = bmp.GetHicon();
			Console.WriteLine("Handles.GetHicon()-> " + HandleIcon);
			Assert(HandleIcon!=IntPtr.Zero);							
		}
		
			
	}
}
