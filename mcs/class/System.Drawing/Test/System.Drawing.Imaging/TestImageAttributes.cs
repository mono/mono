//
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
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
// Authors:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	public class ImageAttributesTest {

		static ColorMatrix global_color_matrix = new ColorMatrix (new float[][] {
			new float[] 	{2,	0,	0, 	0, 	0}, //R
			new float[] 	{0,	1,	0, 	0, 	0}, //G
			new float[] 	{0,	0,	1, 	0, 	0}, //B
			new float[] 	{0,	0,	0, 	1, 	0}, //A
			new float[] 	{0.2f,	0,	0, 	0, 	0}, //Translation
		});

		static ColorMatrix global_gray_matrix = new ColorMatrix (new float[][] {
			new float[] 	{1,	0,	0, 	0, 	0}, //R
			new float[] 	{0,	2,	0, 	0, 	0}, //G
			new float[] 	{0,	0,	3, 	0, 	0}, //B
			new float[] 	{0,	0,	0, 	1, 	0}, //A
			new float[] 	{0.5f,	0,	0, 	0, 	0}, //Translation
		});

		private static Color ProcessColorMatrix (Color color, ColorMatrix colorMatrix)
		{
			using (Bitmap bmp = new Bitmap (64, 64)) {
				using (Graphics gr = Graphics.FromImage (bmp)) {
					ImageAttributes imageAttr = new ImageAttributes ();
					bmp.SetPixel (0,0, color);
					imageAttr.SetColorMatrix (colorMatrix);
					gr.DrawImage (bmp, new Rectangle (0, 0, 64, 64), 0, 0, 64, 64, GraphicsUnit.Pixel, imageAttr);		
					return bmp.GetPixel (0,0);
				}
			}
		}


		// Text Color Matrix processing
		[Test]
		public void ColorMatrix1 ()
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

			Assert.AreEqual (Color.FromArgb (255, 251, 20, 50), clr_rslt, "Color");
		}

		[Test]
		public void ColorMatrix2 ()
		{
			Color clr_src, clr_rslt;

			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1.5f, 	0, 	0}, //B
				new float[] 	{0,	0,	0.5f, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 40, 25);
			clr_rslt = ProcessColorMatrix (clr_src, cm);
			Assert.AreEqual (Color.FromArgb (255, 100, 40, 165), clr_rslt, "Color");
		}

		private void Bug80323 (Color c)
		{
			string fileName = String.Format ("80323-{0}.png", c.ToArgb ().ToString ("X"));

			// test case from bug #80323
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	0.5f, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	1}, //Translation
			  });

			using (SolidBrush sb = new SolidBrush (c)) {
				using (Bitmap bmp = new Bitmap (100, 100)) {
					using (Graphics gr = Graphics.FromImage (bmp)) {
						gr.FillRectangle (Brushes.White, 0, 0, 100, 100);
						gr.FillEllipse (sb, 0, 0, 100, 100);
					}
					using (Bitmap b = new Bitmap (200, 100)) {
						using (Graphics g = Graphics.FromImage (b)) {
							g.FillRectangle (Brushes.White, 0, 0, 200, 100);

							ImageAttributes ia = new ImageAttributes ();
							ia.SetColorMatrix (cm);
							g.DrawImage (bmp, new Rectangle (0, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, null);
							g.DrawImage (bmp, new Rectangle (100, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, ia);
						}
						b.Save (fileName);
						Assert.AreEqual (Color.FromArgb (255, 255, 155, 155), b.GetPixel (50, 50), "50,50");
						Assert.AreEqual (Color.FromArgb (255, 255, 205, 205), b.GetPixel (150, 50), "150,50");
					}
				}
			}

			File.Delete (fileName);
		}
	
		[Test]
		public void ColorMatrix_80323_UsingAlpha ()
		{
			Bug80323 (Color.FromArgb (100, 255, 0, 0));
		}

		[Test]
		public void ColorMatrix_80323_WithoutAlpha ()
		{
			// this color is identical, once drawn over the bitmap, to Color.FromArgb (100, 255, 0, 0)
			Bug80323 (Color.FromArgb (255, 255, 155, 155));
		}



		private static Color ProcessColorMatrices (Color color, ColorMatrix colorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag flags, ColorAdjustType type)
		{
			using (Bitmap bmp = new Bitmap (64, 64)) {
				using (Graphics gr = Graphics.FromImage (bmp)) {
					ImageAttributes imageAttr = new ImageAttributes ();
					bmp.SetPixel (0, 0, color);
					imageAttr.SetColorMatrices (colorMatrix, grayMatrix, flags, type);
					gr.DrawImage (bmp, new Rectangle (0, 0, 64, 64), 0, 0, 64, 64, GraphicsUnit.Pixel, imageAttr);
					return bmp.GetPixel (0, 0);
				}
			}
		}

		[Test]
		public void SetColorMatrix_Null ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (null));
			}
		}

		[Test]
		public void SetColorMatrix_Default ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Brush);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Pen);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Text);
			}
		}

		[Test]
		public void SetColorMatrix_Default_Any ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Any));
			}
		}

		[Test]
		public void SetColorMatrix_Default_Count ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, ColorAdjustType.Count));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Any ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Any));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Bitmap ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Bitmap));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Brush ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Brush));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Count ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Count));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Default ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Default));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Pen ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Pen));
			}
		}

		[Test]
		public void SetColorMatrix_AltGrays_Text ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Text));
			}
		}

		[Test]
		public void SetColorMatrix_SkipGrays ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Bitmap);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Brush);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Default);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Pen);
				ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Text);
			}
		}

		[Test]
		public void SetColorMatrix_SkipGrays_Any ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Any));
			}
		}

		[Test]
		public void SetColorMatrix_SkipGrays_Count ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Count));
			}
		}

		[Test]
		public void SetColorMatrix_InvalidFlag ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, (ColorMatrixFlag) Int32.MinValue));
			}
		}

		[Test]
		public void SetColorMatrix_InvalidType()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrix (global_color_matrix, ColorMatrixFlag.Default, (ColorAdjustType)Int32.MinValue));
			}
		}

		[Test]
		public void SetColorMatrices_Null_ColorMatrix ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrices (null, global_color_matrix));
			}
		}

		[Test]
		public void SetColorMatrices_ColorMatrix_Null ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				ia.SetColorMatrices (global_color_matrix, null);
				ia.SetColorMatrices (global_color_matrix, null, ColorMatrixFlag.Default);
				ia.SetColorMatrices (global_color_matrix, null, ColorMatrixFlag.SkipGrays);
			}
		}

		[Test]
		public void SetColorMatrices_ColorMatrix_Null_AltGrays ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrices (global_color_matrix, null, ColorMatrixFlag.AltGrays));
			}
		}

		[Test]
		public void SetColorMatrices_ColorMatrix_ColorMatrix ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				ia.SetColorMatrices (global_color_matrix, global_color_matrix);
				ia.SetColorMatrices (global_color_matrix, global_color_matrix, ColorMatrixFlag.Default);
				ia.SetColorMatrices (global_color_matrix, global_color_matrix, ColorMatrixFlag.SkipGrays);
				ia.SetColorMatrices (global_color_matrix, global_color_matrix, ColorMatrixFlag.AltGrays);
			}
		}

		[Test]
		public void SetColorMatrices_Gray ()
		{
			Color c = ProcessColorMatrices (Color.Gray, global_color_matrix, global_gray_matrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
			Assert.AreEqual (0xFFFF8080, (uint)c.ToArgb (), "Gray|Default|Default");

			c = ProcessColorMatrices (Color.Gray, global_color_matrix, global_gray_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Default);
			Assert.AreEqual (0xFF808080, (uint) c.ToArgb (), "Gray|SkipGrays|Default");

			c = ProcessColorMatrices (Color.Gray, global_color_matrix, global_gray_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Default);
			Assert.AreEqual (0xFFFFFFFF, (uint) c.ToArgb (), "Gray|AltGrays|Default");
		}

		[Test]
		public void SetColorMatrices_Color ()
		{
			Color c = ProcessColorMatrices (Color.MidnightBlue, global_color_matrix, global_gray_matrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
			Assert.AreEqual (0xFF651970, (uint) c.ToArgb (), "Color|Default|Default");

			c = ProcessColorMatrices (Color.MidnightBlue, global_color_matrix, global_gray_matrix, ColorMatrixFlag.SkipGrays, ColorAdjustType.Default);
			Assert.AreEqual (0xFF651970, (uint) c.ToArgb (), "Color|SkipGrays|Default");

			c = ProcessColorMatrices (Color.MidnightBlue, global_color_matrix, global_gray_matrix, ColorMatrixFlag.AltGrays, ColorAdjustType.Default);
			Assert.AreEqual (0xFF651970, (uint) c.ToArgb (), "Color|AltGrays|Default");
		}

		[Test]
		public void SetColorMatrices_InvalidFlags ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrices (global_color_matrix, global_color_matrix, (ColorMatrixFlag) Int32.MinValue));
			}
		}

		[Test]
		public void SetColorMatrices_InvalidType ()
		{
			using (ImageAttributes ia = new ImageAttributes ()) {
				Assert.Throws<ArgumentException> (() => ia.SetColorMatrices (global_color_matrix, global_color_matrix, ColorMatrixFlag.Default, (ColorAdjustType) Int32.MinValue));
			}
		}

		private void Alpha (string prefix, int n, float a)
		{
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	a, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	1}, //Translation
			  });

			using (Bitmap bmp = new Bitmap (1, 4)) {
				bmp.SetPixel (0, 0, Color.White);
				bmp.SetPixel (0, 1, Color.Red);
				bmp.SetPixel (0, 2, Color.Lime);
				bmp.SetPixel (0, 3, Color.Blue);
				using (Bitmap b = new Bitmap (1, 4)) {
					using (Graphics g = Graphics.FromImage (b)) {
						ImageAttributes ia = new ImageAttributes ();
						ia.SetColorMatrix (cm);
						g.FillRectangle (Brushes.White, new Rectangle (0, 0, 1, 4));
						g.DrawImage (bmp, new Rectangle (0, 0, 1, 4), 0, 0, 1, 4, GraphicsUnit.Pixel, ia);
						Assert.AreEqual (Color.FromArgb (255, 255, 255, 255), b.GetPixel (0,0), prefix + "-0,0");
						int val = 255 - n;
						Assert.AreEqual (Color.FromArgb (255, 255, val, val), b.GetPixel (0, 1), prefix + "-0,1");
						Assert.AreEqual (Color.FromArgb (255, val, 255, val), b.GetPixel (0, 2), prefix + "-0,2");
						Assert.AreEqual (Color.FromArgb (255, val, val, 255), b.GetPixel (0, 3), prefix + "-0,3");
					}
				}
			}
		}

		[Test]
		public void ColorMatrixAlpha ()
		{
			for (int i = 0; i < 256; i++) {
				Alpha (i.ToString (), i, (float) i / 255);
				// generally color matrix are specified with values between [0..1]
				Alpha ("small-" + i.ToString (), i, (float) i / 255);
				// but GDI+ also accept value > 1
				Alpha ("big-" + i.ToString (), i, 256 - i);
			}
		}
	}
}
