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
using System.IO;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class TestBitmap : Assertion {
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
			
		//[Test]
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
		
		/* Get the output directory depending on the runtime and location*/
		internal string getOutSubDir()
		{				
			string sSub, sRslt;			
			
			if (Environment.GetEnvironmentVariable("MSNet")==null)
				sSub = "mono/";
			else
				sSub = "MSNet/";			
			
			sRslt = Path.GetFullPath (sSub);
				
			if (Directory.Exists(sRslt) == 	false) 
				sRslt = Path.GetFullPath ("Test/System.Drawing/") + sSub;				
			
			Console.WriteLine(sRslt);	
			return sRslt;
		}
		
		/* Get the input directory depending on the runtime*/
		internal string getInFile(string file)
		{				
			string sRslt;						
			
			sRslt = Path.GetFullPath (file);
				
			if (File.Exists(file)==false) 
				sRslt = "Test/System.Drawing/" + file;				
			
			Console.WriteLine (sRslt);	
			return sRslt;
		}
		
		//[Test]
		public void BitmapLoadAndSave() 
		{				
			string sOutFile =  getOutSubDir() + "linerect.bmp";
						
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

		//[Test]
		public void MakeTransparent() 
		{
			string sInFile =   getInFile("bitmaps/maketransparent.bmp");
			string sOutFile =  getOutSubDir() + "transparent.bmp";
						
			Bitmap	bmp = new Bitmap(sInFile);
			Console.WriteLine("Bitmap loaded OK", bmp != null);
					
			bmp.MakeTransparent();
			bmp.Save(sOutFile);							
			
			Color color = bmp.GetPixel(1,1);							
			AssertEquals (Color.Black.R, color.R);											
			AssertEquals (Color.Black.G, color.G);											
			AssertEquals (Color.Black.B, color.B);										
		}
		
		//[Test]
		public void Clone ()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");
			string sOutFile =  getOutSubDir() + "clone24.bmp";			
			
			Rectangle rect = new Rectangle(0,0,50,50);						
			Bitmap	bmp = new Bitmap(sInFile);			
			
			Bitmap bmpNew = bmp.Clone (rect, PixelFormat.Format32bppArgb);			
			bmpNew.Save(sOutFile);							
			
			Color colororg0 = bmp.GetPixel(0,0);		
			Color colororg50 = bmp.GetPixel(49,49);					
			Color colornew0 = bmpNew.GetPixel(0,0);		
			Color colornew50 = bmpNew.GetPixel(49,49);				
			
			AssertEquals (colororg0, colornew0);											
			AssertEquals (colororg50, colornew50);				
		}	
	
	}
}
