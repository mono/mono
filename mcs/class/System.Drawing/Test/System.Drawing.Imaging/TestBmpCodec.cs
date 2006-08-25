//
// BMPCodec class testing unit
//
// Author:
//
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
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
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Drawing.Imaging
{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class TestBmpCodec
	{
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		/* Get suffix to add to the filename */
		internal string getOutSufix()
		{			
			if (Environment.GetEnvironmentVariable("MSNet")==null)
				return "-mono";
					
			return "";
		}

		/* Get the input directory depending on the runtime*/
		internal string getInFile(string file)
		{				
			string sRslt, local;						

			local = "../System.Drawing/" + file;
			
			sRslt = Path.GetFullPath (local);
				
			if (File.Exists(sRslt)==false) 
				sRslt = "Test/System.Drawing/" + file;							
			
			return sRslt;
		}
		
		/* Checks bitmap features on a know 24-bits bitmap */
#if TARGET_JVM
		[NUnit.Framework.Category ("NotWorking")]
#endif
		[Test]
		public void Bitmap24bitFeatures()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);						
			RectangleF rect;
			GraphicsUnit unit = GraphicsUnit.World;
			
			rect = bmp.GetBounds(ref unit);
			
			Assert.AreEqual (PixelFormat.Format24bppRgb, bmp.PixelFormat);
			Assert.AreEqual (173, bmp.Width);
			Assert.AreEqual (183, bmp.Height);		
			
			Assert.AreEqual (0, rect.X);
			Assert.AreEqual (0, rect.Y);		
			Assert.AreEqual (173, rect.Width);
			Assert.AreEqual (183, rect.Height);					
			
			Assert.AreEqual (173, bmp.Size.Width);
			Assert.AreEqual (183, bmp.Size.Height);	

			// sampling values from a well known bitmap
			Assert.AreEqual (-1645353, bmp.GetPixel (0, 32).ToArgb (), "0,32");
			Assert.AreEqual (-461332, bmp.GetPixel (0, 64).ToArgb (), "0,64");
			Assert.AreEqual (-330005, bmp.GetPixel (0, 96).ToArgb (), "0,96");
			Assert.AreEqual (-2237489, bmp.GetPixel (0, 128).ToArgb (), "0,128");
			Assert.AreEqual (-1251105, bmp.GetPixel (0, 160).ToArgb (), "0,160");
			Assert.AreEqual (-3024947, bmp.GetPixel (32, 0).ToArgb (), "32,0");
			Assert.AreEqual (-2699070, bmp.GetPixel (32, 32).ToArgb (), "32,32");
			Assert.AreEqual (-2366734, bmp.GetPixel (32, 64).ToArgb (), "32,64");
			Assert.AreEqual (-4538413, bmp.GetPixel (32, 96).ToArgb (), "32,96");
			Assert.AreEqual (-6116681, bmp.GetPixel (32, 128).ToArgb (), "32,128");
			Assert.AreEqual (-7369076, bmp.GetPixel (32, 160).ToArgb (), "32,160");
			Assert.AreEqual (-13024729, bmp.GetPixel (64, 0).ToArgb (), "64,0");
			Assert.AreEqual (-7174020, bmp.GetPixel (64, 32).ToArgb (), "64,32");
			Assert.AreEqual (-51, bmp.GetPixel (64, 64).ToArgb (), "64,64");
			Assert.AreEqual (-16053503, bmp.GetPixel (64, 96).ToArgb (), "64,96");
			Assert.AreEqual (-8224431, bmp.GetPixel (64, 128).ToArgb (), "64,128");
			Assert.AreEqual (-16579326, bmp.GetPixel (64, 160).ToArgb (), "64,160");
			Assert.AreEqual (-2502457, bmp.GetPixel (96, 0).ToArgb (), "96,0");
			Assert.AreEqual (-9078395, bmp.GetPixel (96, 32).ToArgb (), "96,32");
			Assert.AreEqual (-12696508, bmp.GetPixel (96, 64).ToArgb (), "96,64");
			Assert.AreEqual (-70772, bmp.GetPixel (96, 96).ToArgb (), "96,96");
			Assert.AreEqual (-4346279, bmp.GetPixel (96, 128).ToArgb (), "96,128");
			Assert.AreEqual (-11583193, bmp.GetPixel (96, 160).ToArgb (), "96,160");
			Assert.AreEqual (-724763, bmp.GetPixel (128, 0).ToArgb (), "128,0");
			Assert.AreEqual (-7238268, bmp.GetPixel (128, 32).ToArgb (), "128,32");
			Assert.AreEqual (-2169612, bmp.GetPixel (128, 64).ToArgb (), "128,64");
			Assert.AreEqual (-3683883, bmp.GetPixel (128, 96).ToArgb (), "128,96");
			Assert.AreEqual (-12892867, bmp.GetPixel (128, 128).ToArgb (), "128,128");
			Assert.AreEqual (-3750464, bmp.GetPixel (128, 160).ToArgb (), "128,160");
			Assert.AreEqual (-3222844, bmp.GetPixel (160, 0).ToArgb (), "160,0");
			Assert.AreEqual (-65806, bmp.GetPixel (160, 32).ToArgb (), "160,32");
			Assert.AreEqual (-2961726, bmp.GetPixel (160, 64).ToArgb (), "160,64");
			Assert.AreEqual (-2435382, bmp.GetPixel (160, 96).ToArgb (), "160,96");
			Assert.AreEqual (-2501944, bmp.GetPixel (160, 128).ToArgb (), "160,128");
			Assert.AreEqual (-9211799, bmp.GetPixel (160, 160).ToArgb (), "160,160");
/*			for (int x = 0; x <bmp.Width; x += 32) {
				for (int y = 0; y < bmp.Height; y += 32)
					Console.WriteLine ("\t\t\tAssert.AreEqual ({0}, bmp.GetPixel ({1}, {2}).ToArgb (), \"{1},{2}\");", bmp.GetPixel (x, y).ToArgb (), x, y);
			}*/
		}

		

		/* Checks bitmap features on a know 32-bits bitmap (codec)*/
		[Test]
		public void Bitmap32bitFeatures()
		{
			string sInFile = getInFile ("bitmaps/almogaver32bits.bmp");
			Bitmap	bmp = new Bitmap(sInFile);						
			RectangleF rect;
			GraphicsUnit unit = GraphicsUnit.World;
			
			rect = bmp.GetBounds(ref unit);	

			Assert.AreEqual (173, bmp.Width);
			Assert.AreEqual (183, bmp.Height);		
			
			Assert.AreEqual (0, rect.X);
			Assert.AreEqual (0, rect.Y);		
			Assert.AreEqual (173, rect.Width);
			Assert.AreEqual (183, rect.Height);					
			
			Assert.AreEqual (173, bmp.Size.Width);
			Assert.AreEqual (183, bmp.Size.Height);					
		}

		[Test]
		public void Save() 
		{				
			string sOutFile =  "linerect" + getOutSufix() + ".bmp";
						
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
			
			Color color = bmpLoad.GetPixel(10,10);					
			
			//Assert.AreEqual (Color.FromArgb(255,255,0,0), color);											
		}

		
	}
}
