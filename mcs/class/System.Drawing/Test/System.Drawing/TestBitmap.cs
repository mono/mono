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
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class TestBitmap : Assertion {
		
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
				sRslt = "Test/System.Drawing/" + sSub;							
			
			if (sRslt.Length > 0)
				if (sRslt[sRslt.Length-1] != '\\' && sRslt[sRslt.Length-1] != '/')
					sRslt += "/";					
			
			return sRslt;
		}
		
		/* Get the input directory depending on the runtime*/
		internal string getInFile(string file)
		{				
			string sRslt;						
			
			sRslt = Path.GetFullPath (file);
				
			if (File.Exists(file)==false) 
				sRslt = "Test/System.Drawing/" + file;							
			
			return sRslt;
		}
		
		[Test]
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
		
		[Test]
		public void Clone()
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
		
		[Test]
		public void CloneImage()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);			
			
			Bitmap bmpNew = (Bitmap) bmp.Clone ();			
			
			AssertEquals (bmp.Width, bmpNew.Width);
			AssertEquals (bmp.Height, bmpNew.Height);		
			AssertEquals (bmp.PixelFormat, bmpNew.PixelFormat);			
			
		}	
		
		/* Check bitmap features on a know 24-bits bitmap*/
		[Test]
		public void BitmapFeatures()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");
			Bitmap	bmp = new Bitmap(sInFile);						
			RectangleF rect;
			GraphicsUnit unit = GraphicsUnit.World;
			
			rect = bmp.GetBounds(ref unit);
			
			AssertEquals (PixelFormat.Format24bppRgb, bmp.PixelFormat);
			AssertEquals (173, bmp.Width);
			AssertEquals (183, bmp.Height);		
			
			AssertEquals (0, rect.X);
			AssertEquals (0, rect.Y);		
			AssertEquals (173, rect.Width);
			AssertEquals (183, rect.Height);					
			
			AssertEquals (173, bmp.Size.Width);
			AssertEquals (183, bmp.Size.Height);					
		}
		
		[Test]
		public void Frames()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);						
			int cnt = bmp.GetFrameCount(FrameDimension.Page);			
			int active = bmp.SelectActiveFrame (FrameDimension.Page, 0);
			
			AssertEquals (1, cnt);								
			AssertEquals (0, active);											
		}

		static string ByteArrayToString(byte[] arrInput)
		{
			int i;
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (i=0;i < arrInput.Length -1; i++) 
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}
			return sOutput.ToString();
		}


		public string RotateBmp (Bitmap src, RotateFlipType rotate)
		{			
			int witdh = 150, height = 150, index = 0;			
			byte[] pixels = new byte [witdh * height * 3];
			Bitmap bmp_rotate;
			byte[] hash;
			Color clr;


			bmp_rotate = src.Clone (new RectangleF (0,0, witdh, height), PixelFormat.Format32bppArgb);	
			bmp_rotate.RotateFlip (rotate);			

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < witdh; x++) {				
					clr = bmp_rotate.GetPixel (x,y);
					pixels[index++] = clr.R; pixels[index++] = clr.G; pixels[index++]  = clr.B;	
				}				
			}
		
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);
			return ByteArrayToString (hash);
		}
		
		
		/*
			Rotate bitmap in diffent ways, and check the result
			pixels using MD5
		*/
		[Test]
		public void Rotate()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");	
			Bitmap	bmp = new Bitmap(sInFile);		
			
			AssertEquals ("312958A3C67402E1299413794988A3", RotateBmp (bmp, RotateFlipType.Rotate90FlipNone));	
			AssertEquals ("BF70D8DA4F1545AEDD77D0296B47AE", RotateBmp (bmp, RotateFlipType.Rotate180FlipNone));
			AssertEquals ("15AD2ADBDC7090C0EC744D0F7ACE2F", RotateBmp (bmp, RotateFlipType.Rotate270FlipNone));
			AssertEquals ("2E10FEC1F4FD64ECC51D7CE68AEB18", RotateBmp (bmp, RotateFlipType.RotateNoneFlipX));
			AssertEquals ("E63204779B566ED01162B90B49BD9E", RotateBmp (bmp, RotateFlipType.Rotate90FlipX));
			AssertEquals ("B1ECB17B5093E13D04FF55CFCF7763", RotateBmp (bmp, RotateFlipType.Rotate180FlipX));
			AssertEquals ("71A173882C16755D86F4BC26532374", RotateBmp (bmp, RotateFlipType.Rotate270FlipX));

		}
		
	}
}
