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
using System.Runtime.InteropServices;

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

		//[Test]
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
		
		public void LockBmp (PixelFormat fmt, PixelFormat fmtlock, string output, 
			int lwidth , int lheight, ref string hash1, ref string hash2)
		{			
			int width = 100, height = 100, bbps, cur, pos;
			Bitmap	bmp = new Bitmap (width, height, fmt);										
			Graphics gr = Graphics.FromImage (bmp);			
			byte[] hash;
			Color clr;
			byte[] btv = new byte[1];   						
			int y, x, len = width * height * 4, index = 0;			
			byte[] pixels = new byte [len];
			hash1 = hash2 ="";
			
			bbps = Image.GetPixelFormatSize (fmt);			
				 
			Pen p = new Pen (Color.FromArgb (255, 100, 200, 250), 2);				
			gr.DrawRectangle(p, 1.0F, 1.0F, 80.0F, 80.0F);				
			
			BitmapData bd = bmp.LockBits (new Rectangle (0, 0, lwidth, lheight), ImageLockMode.ReadOnly,  fmtlock);
			
			pos = bd.Scan0.ToInt32();			
			for (y = 0; y < bd.Height; y++) {			
				for (x = 0; x < bd.Width; x++) {
					
					/* Read the pixels*/
					for (int bt =0; bt < bbps/8; bt++, index++) {
						cur = pos;
						cur+= y * bd.Stride;
						cur+= x * bbps/8;					
						cur+= bt;					
						Marshal.Copy ((IntPtr)cur, btv, 0, 1);
						pixels[index] = btv[0];
						
						/* Make change of all the colours = 250 to 10*/						
						if (btv[0] == 250) {
							btv[0] = 10;
							Marshal.Copy (btv, 0, (IntPtr)cur, 1);
						}
					}
				}
			}					
			
			for (int i = index; i < len; i++)
				pixels[index] = 0;
		
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);			
			bmp.UnlockBits (bd);							
						
			hash1 = ByteArrayToString (hash);
			
			/* MD5 of the changed bitmap*/
			for (y = 0, index = 0; y < height; y++) {
				for (x = 0; x < width; x++) {				
					clr = bmp.GetPixel (x,y);
					pixels[index++] = clr.R; pixels[index++] = clr.G; pixels[index++]  = clr.B;	
				}				
			}
			
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);						
			hash2 = ByteArrayToString (hash);
			
			/*bmp.Save (output, ImageFormat.Bmp);*/
		}
		/*
			Tests the LockBitmap functions. Makes a hash of the block of pixels that it returns
			firsts, changes them, and then using GetPixel does another check of the changes.
			The results match the .Net framework
		*/
		[Test]
		public void LockBitmap ()
		{	
			string hash = "";		
			string hashchg = "";				
							
			/* Locks the whole bitmap*/			
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 100, 100, ref hash, ref hashchg);				
			AssertEquals ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			AssertEquals ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 100, 100, ref hash, ref hashchg);			
			AssertEquals ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			AssertEquals ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			AssertEquals ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			AssertEquals ("BBEE27DC85563CB58EE11E8951230F", hashchg);		
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			AssertEquals ("A8A071D0B3A3743905B4E193A62769", hash);			
			AssertEquals ("EEE846FA8F892339C64082DFF775CF", hashchg);					
			
			/* Locks a portion of the bitmap*/		
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 50, 50, ref hash, ref hashchg);				
			AssertEquals ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			AssertEquals ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 50, 50, ref hash, ref hashchg);			
			AssertEquals ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			AssertEquals ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
		
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			AssertEquals ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			AssertEquals ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			AssertEquals ("FFE86628478591D1A1EB30E894C34F", hash);			
			AssertEquals ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);				
						
		}
		
	}
}
