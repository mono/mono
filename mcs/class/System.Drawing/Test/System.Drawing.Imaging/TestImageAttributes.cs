//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
//
// Author:
//   Jordi Mas i Hernandez (jordi@ximian.com)
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	public class TestImageAttributes
	{

		[TearDown]
		public void Clean() {}

		[SetUp]
		public void GetReady()
		{

		}
		
		private static Color ProcessColorMatrix (Color color, ColorMatrix colorMatrix)
		{
			Bitmap bmp = new Bitmap (64, 64);
			Graphics gr = Graphics.FromImage (bmp);
			ImageAttributes imageAttr = new ImageAttributes ();
	
			bmp.SetPixel (0,0, color);
	
			imageAttr.SetColorMatrix (colorMatrix);
			gr.DrawImage (bmp, new Rectangle (0, 0, 64,64), 0,0, 64,64, GraphicsUnit.Pixel, imageAttr);		
			return bmp.GetPixel (0,0);
		}


		// Text Color Matrix processing
		[Test]
		public void ColorMatrix ()
		{			
			Color clr_src, clr_rslt;
			
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{2,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	1, 	0}, //A
				new float[] 	{0.2f,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, cm);

			Assert.AreEqual (clr_rslt, Color.FromArgb (255, 251, 20, 50));
			
			
			cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1.5f, 	0, 	0}, //B
				new float[] 	{0,	0,	0.5f, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 40, 25);
			clr_rslt = ProcessColorMatrix (clr_src, cm);
			Assert.AreEqual (clr_rslt, Color.FromArgb (255, 100, 40, 165));			 
		}

		
	}
}
