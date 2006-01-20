//
// Bitmap class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jmas@softcatala.org>
//	 Jonathan Gilbert <logic@deltaq.org>
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
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
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace MonoTests.System.Drawing{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class TestBitmap {
		
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
						
			Assert.AreEqual (Color.FromArgb(255,128,128,128), color);
			
			bmp.SetPixel(99,99,Color.FromArgb(255,255,0,155));					
			Color color2 = bmp.GetPixel(99,99);										
			Assert.AreEqual (Color.FromArgb(255,255,0,155), color2);			
		}
		
		/* Get the output directory depending on the runtime and location*/
		public static string getOutSubDir()
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
		public static string getInFile(string file)
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
					
			bmp.MakeTransparent();
			bmp.Save(sOutFile);							
			
			Color color = bmp.GetPixel(1,1);							
			Assert.AreEqual (Color.Black.R, color.R);											
			Assert.AreEqual (Color.Black.G, color.G);											
			Assert.AreEqual (Color.Black.B, color.B);										
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
			
			Assert.AreEqual (colororg0, colornew0);											
			Assert.AreEqual (colororg50, colornew50);				
		}	
		
		[Test]
		public void CloneImage()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);			
			
			Bitmap bmpNew = (Bitmap) bmp.Clone ();			
			
			Assert.AreEqual (bmp.Width, bmpNew.Width);
			Assert.AreEqual (bmp.Height, bmpNew.Height);		
			Assert.AreEqual (bmp.PixelFormat, bmpNew.PixelFormat);
			
		}	

		[Test]
		public void Frames()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);						
			int cnt = bmp.GetFrameCount(FrameDimension.Page);			
			int active = bmp.SelectActiveFrame (FrameDimension.Page, 0);
			
			Assert.AreEqual (1, cnt);								
			Assert.AreEqual (0, active);											
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FileDoesNotExists ()
		{			
			Bitmap	bmp = new Bitmap ("FileDoesNotExists.jpg");			
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

		public string RotateIndexedBmp (Bitmap src, RotateFlipType type)
		{
			int pixels_per_byte;

			switch (src.PixelFormat)
			{
				case PixelFormat.Format1bppIndexed: pixels_per_byte = 8; break;
				case PixelFormat.Format4bppIndexed: pixels_per_byte = 2; break;
				case PixelFormat.Format8bppIndexed: pixels_per_byte = 1; break;

				default: throw new Exception("Cannot pass a bitmap of format " + src.PixelFormat + " to RotateIndexedBmp");
			}

			Bitmap test = src.Clone () as Bitmap;

			test.RotateFlip (type);

			BitmapData data = null;
			byte[] pixel_data;

			try
			{
				data = test.LockBits (new Rectangle (0, 0, test.Width, test.Height), ImageLockMode.ReadOnly, test.PixelFormat);

				int scan_size = (data.Width + pixels_per_byte - 1) / pixels_per_byte;
				pixel_data = new byte[data.Height * scan_size];

				for (int y=0; y < data.Height; y++) {
					IntPtr src_ptr = (IntPtr)(y * data.Stride + data.Scan0.ToInt64 ());
					int dest_offset = y * scan_size;
					for (int x=0; x < scan_size; x++)
						pixel_data[dest_offset + x] = Marshal.ReadByte (src_ptr, x);
				}
			}
			finally
			{
				if (test != null) {
					if (data != null)
						try { test.UnlockBits(data); } catch {}

					try { test.Dispose(); } catch {}
				}
			}

			if (pixel_data == null)
				return "--ERROR--";

			byte[] hash = new MD5CryptoServiceProvider().ComputeHash (pixel_data);
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
			
			Assert.AreEqual ("312958A3C67402E1299413794988A3", RotateBmp (bmp, RotateFlipType.Rotate90FlipNone));	
			Assert.AreEqual ("BF70D8DA4F1545AEDD77D0296B47AE", RotateBmp (bmp, RotateFlipType.Rotate180FlipNone));
			Assert.AreEqual ("15AD2ADBDC7090C0EC744D0F7ACE2F", RotateBmp (bmp, RotateFlipType.Rotate270FlipNone));
			Assert.AreEqual ("2E10FEC1F4FD64ECC51D7CE68AEB18", RotateBmp (bmp, RotateFlipType.RotateNoneFlipX));
			Assert.AreEqual ("E63204779B566ED01162B90B49BD9E", RotateBmp (bmp, RotateFlipType.Rotate90FlipX));
			Assert.AreEqual ("B1ECB17B5093E13D04FF55CFCF7763", RotateBmp (bmp, RotateFlipType.Rotate180FlipX));
			Assert.AreEqual ("71A173882C16755D86F4BC26532374", RotateBmp (bmp, RotateFlipType.Rotate270FlipX));

		}

		/*
			Rotate 1- and 4-bit bitmaps in different ways and check the
			resulting pixels using MD5
		*/
		[Test]
		public void Rotate1bit4bit()
		{
			string[] files = {
			                   getInFile ("bitmaps/1bit.png"),
			                   getInFile ("bitmaps/4bit.png")
			                 };

			StringBuilder md5s = new StringBuilder();

			foreach (string file in files)
				using (Bitmap bmp = new Bitmap(file))
					foreach (RotateFlipType type in Enum.GetValues (typeof(RotateFlipType)))
						md5s.Append (RotateIndexedBmp (bmp, type));

			using (StreamWriter writer = new StreamWriter("/tmp/md5s.txt"))
				writer.WriteLine(md5s);

			Assert.AreEqual (
				"A4DAF507C92BDE10626BC7B34FEFE5" + // 1-bit RotateNoneFlipNone
				"A4DAF507C92BDE10626BC7B34FEFE5" + // 1-bit Rotate180FlipXY
				"C0975EAFD2FC1CC9CC7AF20B92FC9F" + // 1-bit Rotate90FlipNone
				"C0975EAFD2FC1CC9CC7AF20B92FC9F" + // 1-bit Rotate270FlipXY
				"64AE60858A02228F7B1B18C7812FB6" + // 1-bit Rotate180FlipNone
				"64AE60858A02228F7B1B18C7812FB6" + // 1-bit RotateNoneFlipXY
				"E96D3390938350F9DE2608C4364424" + // 1-bit Rotate270FlipNone
				"E96D3390938350F9DE2608C4364424" + // 1-bit Rotate90FlipXY
				"23947CE822C1DDE6BEA69C01F8D0D9" + // 1-bit RotateNoneFlipX
				"23947CE822C1DDE6BEA69C01F8D0D9" + // 1-bit Rotate180FlipY
				"BE45F685BDEBD7079AA1B2CBA46723" + // 1-bit Rotate90FlipX
				"BE45F685BDEBD7079AA1B2CBA46723" + // 1-bit Rotate270FlipY
				"353E937CFF31B1BF6C3DD0A031ACB5" + // 1-bit Rotate180FlipX
				"353E937CFF31B1BF6C3DD0A031ACB5" + // 1-bit RotateNoneFlipY
				"AEA18A770A845E25B6A8CE28DD6DCB" + // 1-bit Rotate270FlipX
				"AEA18A770A845E25B6A8CE28DD6DCB" + // 1-bit Rotate90FlipY
				"3CC874B571902366AACED5D619E87D" + // 4-bit RotateNoneFlipNone
				"3CC874B571902366AACED5D619E87D" + // 4-bit Rotate180FlipXY
				"8DE25C7E1BE4A3B535DB5D83198D83" + // 4-bit Rotate90FlipNone
				"8DE25C7E1BE4A3B535DB5D83198D83" + // 4-bit Rotate270FlipXY
				"27CF5E9CE70BE9EBC47FB996721B95" + // 4-bit Rotate180FlipNone
				"27CF5E9CE70BE9EBC47FB996721B95" + // 4-bit RotateNoneFlipXY
				"A919CCB8F97CAD7DC1F01026D11A5D" + // 4-bit Rotate270FlipNone
				"A919CCB8F97CAD7DC1F01026D11A5D" + // 4-bit Rotate90FlipXY
				"545876C99ACF833E69FBFFBF436034" + // 4-bit RotateNoneFlipX
				"545876C99ACF833E69FBFFBF436034" + // 4-bit Rotate180FlipY
				"5DB56687757CDEFC52D89C77CA9223" + // 4-bit Rotate90FlipX
				"5DB56687757CDEFC52D89C77CA9223" + // 4-bit Rotate270FlipY
				"05A77EDDCDF20D5B0AC0169E95D7D7" + // 4-bit Rotate180FlipX
				"05A77EDDCDF20D5B0AC0169E95D7D7" + // 4-bit RotateNoneFlipY
				"B6B6245796C836923ABAABDF368B29" + // 4-bit Rotate270FlipX
				"B6B6245796C836923ABAABDF368B29",  // 4-bit Rotate90FlipY
				md5s.ToString ());
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
		//[Test]
		public void LockBitmap ()
		{	
			string hash = "";		
			string hashchg = "";				
							
			/* Locks the whole bitmap*/			
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 100, 100, ref hash, ref hashchg);				
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 100, 100, ref hash, ref hashchg);			
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);		
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			Assert.AreEqual ("A8A071D0B3A3743905B4E193A62769", hash);			
			Assert.AreEqual ("EEE846FA8F892339C64082DFF775CF", hashchg);					
			
			/* Locks a portion of the bitmap*/		
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 50, 50, ref hash, ref hashchg);				
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 50, 50, ref hash, ref hashchg);			
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
		
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			Assert.AreEqual ("FFE86628478591D1A1EB30E894C34F", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);				
						
		}

		/*
			Tests the LockBitmap and UnlockBitmap functions, specifically the copying
			of bitmap data in the directions indicated by the ImageLockMode.
		*/
		[Test]
		public void LockUnlockBitmap()
		{
			BitmapData data;
			int pixel_value;
			Color pixel_colour;

			Color red  = Color.FromArgb (Color.Red.A,  Color.Red.R,  Color.Red.G,  Color.Red.B);
			Color blue = Color.FromArgb (Color.Blue.A, Color.Blue.R, Color.Blue.G, Color.Blue.B);

			using (Bitmap bmp = new Bitmap (1, 1, PixelFormat.Format24bppRgb))
			{
				bmp.SetPixel (0, 0, red);

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				pixel_value = Marshal.ReadInt32 (data.Scan0);
				pixel_colour = Color.FromArgb (pixel_value);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, pixel_colour);

				Marshal.WriteInt32 (data.Scan0, blue.ToArgb ());

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, pixel_colour);

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

				Marshal.WriteInt32 (data.Scan0, blue.ToArgb ());

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(blue.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (blue, pixel_colour);
			}

			using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			{
				bmp.SetPixel (0, 0, red);

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

				int r, g, b;

				b = Marshal.ReadByte (data.Scan0, 0);
				g = Marshal.ReadByte (data.Scan0, 1);
				r = Marshal.ReadByte (data.Scan0, 2);
				pixel_colour = Color.FromArgb (red.A, r, g, b);

				Assert.AreEqual (red, pixel_colour);

				Marshal.WriteByte (data.Scan0, 0, blue.B);
				Marshal.WriteByte (data.Scan0, 1, blue.G);
				Marshal.WriteByte (data.Scan0, 2, blue.R);

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, bmp.GetPixel (0, 0));

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

				Marshal.WriteByte (data.Scan0, 0, blue.B);
				Marshal.WriteByte (data.Scan0, 1, blue.G);
				Marshal.WriteByte (data.Scan0, 2, blue.R);

				bmp.UnlockBits(data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(blue.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (blue, bmp.GetPixel (0, 0));
			}
		}
	}
}
