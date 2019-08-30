//
// Graphics class testing unit
//
// Authors:
//   Jordi Mas, jordi@ximian.com
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2008 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

using MonoTests.Helpers;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class GraphicsTest {

		private RectangleF[] rects;
		private Font font;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				font = new Font ("Arial", 12);
			}
			catch {
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			if (font != null)
				font.Dispose ();
		}


		private bool IsEmptyBitmap (Bitmap bitmap, out int x, out int y)
		{
			bool result = true;
			int empty = Color.Empty.ToArgb ();
#if false
			for (y = 0; y < bitmap.Height; y++) {
				for (x = 0; x < bitmap.Width; x++) {
					if (bitmap.GetPixel (x, y).ToArgb () != empty) {
						Console.Write ("X");
						result = false;
					} else
						Console.Write (" ");
				}
				Console.WriteLine ();
			}
#else
			for (y = 0; y < bitmap.Height; y++) {
				for (x = 0; x < bitmap.Width; x++) {
					if (bitmap.GetPixel (x, y).ToArgb () != empty)
						return false;
				}
			}
#endif
			x = -1;
			y = -1;
			return result;
		}

		private void CheckForEmptyBitmap (Bitmap bitmap)
		{
			int x, y;
			if (!IsEmptyBitmap (bitmap, out x, out y))
				Assert.Fail (String.Format ("Position {0},{1}", x, y));
		}

		private void CheckForNonEmptyBitmap (Bitmap bitmap)
		{
			int x, y;
			if (IsEmptyBitmap (bitmap, out x, out y))
				Assert.Fail ("Bitmap was empty");
		}

		private void AssertEquals (string msg, object expected, object actual)
		{
			Assert.AreEqual (expected, actual, msg);
		}

		private void AssertEquals (string msg, double expected, double actual, double delta)
		{
			Assert.AreEqual (expected, actual, delta, msg);
		}

		[Test]
		public void DefaultProperties ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Region r = new Region ();

			Assert.AreEqual (r.GetBounds (g), g.ClipBounds, "DefaultProperties1");
			Assert.AreEqual (CompositingMode.SourceOver, g.CompositingMode, "DefaultProperties2");
			Assert.AreEqual (CompositingQuality.Default, g.CompositingQuality, "DefaultProperties3");
			Assert.AreEqual (InterpolationMode.Bilinear, g.InterpolationMode, "DefaultProperties4");
			Assert.AreEqual (1, g.PageScale, "DefaultProperties5");
			Assert.AreEqual (GraphicsUnit.Display, g.PageUnit, "DefaultProperties6");
			Assert.AreEqual (PixelOffsetMode.Default, g.PixelOffsetMode, "DefaultProperties7");
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "DefaultProperties8");
			Assert.AreEqual (SmoothingMode.None, g.SmoothingMode, "DefaultProperties9");
			Assert.AreEqual (TextRenderingHint.SystemDefault, g.TextRenderingHint, "DefaultProperties10");

			r.Dispose ();
		}

		[Test]
		public void SetGetProperties ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.GammaCorrected;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.PageScale = 2;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (10, 20);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.SystemDefault;

			//Clipping set/get tested in clipping functions			
			Assert.AreEqual (CompositingMode.SourceCopy, g.CompositingMode, "SetGetProperties2");
			Assert.AreEqual (CompositingQuality.GammaCorrected, g.CompositingQuality, "SetGetProperties3");
			Assert.AreEqual (InterpolationMode.HighQualityBilinear, g.InterpolationMode, "SetGetProperties4");
			Assert.AreEqual (2, g.PageScale, "SetGetProperties5");
			Assert.AreEqual (GraphicsUnit.Inch, g.PageUnit, "SetGetProperties6");
			Assert.AreEqual (PixelOffsetMode.Half, g.PixelOffsetMode, "SetGetProperties7");
			Assert.AreEqual (new Point (10, 20), g.RenderingOrigin, "SetGetProperties8");
			Assert.AreEqual (SmoothingMode.AntiAlias, g.SmoothingMode, "SetGetProperties9");
			Assert.AreEqual (TextRenderingHint.SystemDefault, g.TextRenderingHint, "SetGetProperties10");
		}

		// Properties
		[Test]
		public void Clip ()
		{
			RectangleF[] rects;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			g.Clip = new Region (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());

			Assert.AreEqual (1, rects.Length, "Clip1");
			Assert.AreEqual (50, rects[0].X, "Clip2");
			Assert.AreEqual (40, rects[0].Y, "Clip3");
			Assert.AreEqual (210, rects[0].Width, "Clip4");
			Assert.AreEqual (220, rects[0].Height, "Clip5");
		}

		[Test]
		public void Clip_NotAReference ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite");
			g.Clip.IsEmpty (g);
			Assert.IsFalse (g.Clip.IsEmpty (g), "!IsEmpty");
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite-2");
		}

		[Test]
		public void ExcludeClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (10, 10, 100, 100));
			g.ExcludeClip (new Rectangle (40, 60, 100, 20));
			rects = g.Clip.GetRegionScans (new Matrix ());

			Assert.AreEqual (3, rects.Length, "ExcludeClip1");

			Assert.AreEqual (10, rects[0].X, "ExcludeClip2");
			Assert.AreEqual (10, rects[0].Y, "ExcludeClip3");
			Assert.AreEqual (100, rects[0].Width, "ExcludeClip4");
			Assert.AreEqual (50, rects[0].Height, "ExcludeClip5");

			Assert.AreEqual (10, rects[1].X, "ExcludeClip6");
			Assert.AreEqual (60, rects[1].Y, "ExcludeClip7");
			Assert.AreEqual (30, rects[1].Width, "ExcludeClip8");
			Assert.AreEqual (20, rects[1].Height, "ExcludeClip9");

			Assert.AreEqual (10, rects[2].X, "ExcludeClip10");
			Assert.AreEqual (80, rects[2].Y, "ExcludeClip11");
			Assert.AreEqual (100, rects[2].Width, "ExcludeClip12");
			Assert.AreEqual (30, rects[2].Height, "ExcludeClip13");
		}

		[Test]
		public void IntersectClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (260, 30, 60, 80));
			g.IntersectClip (new Rectangle (290, 40, 60, 80));
			rects = g.Clip.GetRegionScans (new Matrix ());

			Assert.AreEqual (1, rects.Length, "IntersectClip");

			Assert.AreEqual (290, rects[0].X, "IntersectClip");
			Assert.AreEqual (40, rects[0].Y, "IntersectClip");
			Assert.AreEqual (30, rects[0].Width, "IntersectClip");
			Assert.AreEqual (70, rects[0].Height, "IntersectClip");
		}

		[Test]
		public void ResetClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (260, 30, 60, 80));
			g.IntersectClip (new Rectangle (290, 40, 60, 80));
			g.ResetClip ();
			rects = g.Clip.GetRegionScans (new Matrix ());

			Assert.AreEqual (1, rects.Length, "ResetClip");

			Assert.AreEqual (-4194304, rects[0].X, "ResetClip");
			Assert.AreEqual (-4194304, rects[0].Y, "ResetClip");
			Assert.AreEqual (8388608, rects[0].Width, "ResetClip");
			Assert.AreEqual (8388608, rects[0].Height, "ResetClip");
		}

		[Test]
		public void SetClip ()
		{
			RectangleF[] rects;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			// Region
			g.SetClip (new Region (new Rectangle (50, 40, 210, 220)), CombineMode.Replace);
			rects = g.Clip.GetRegionScans (new Matrix ());
			Assert.AreEqual (1, rects.Length, "SetClip1");
			Assert.AreEqual (50, rects[0].X, "SetClip2");
			Assert.AreEqual (40, rects[0].Y, "SetClip3");
			Assert.AreEqual (210, rects[0].Width, "SetClip4");
			Assert.AreEqual (220, rects[0].Height, "SetClip5");

			// RectangleF
			g = Graphics.FromImage (bmp);
			g.SetClip (new RectangleF (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			Assert.AreEqual (1, rects.Length, "SetClip6");
			Assert.AreEqual (50, rects[0].X, "SetClip7");
			Assert.AreEqual (40, rects[0].Y, "SetClip8");
			Assert.AreEqual (210, rects[0].Width, "SetClip9");
			Assert.AreEqual (220, rects[0].Height, "SetClip10");

			// Rectangle
			g = Graphics.FromImage (bmp);
			g.SetClip (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			Assert.AreEqual (1, rects.Length, "SetClip10");
			Assert.AreEqual (50, rects[0].X, "SetClip11");
			Assert.AreEqual (40, rects[0].Y, "SetClip12");
			Assert.AreEqual (210, rects[0].Width, "SetClip13");
			Assert.AreEqual (220, rects[0].Height, "SetClip14");
		}

		[Test]
		public void SetSaveReset ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			GraphicsState state_default, state_modified;

			state_default = g.Save (); // Default

			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.GammaCorrected;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.PageScale = 2;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.Clip = new Region (new Rectangle (0, 0, 100, 100));
			g.RenderingOrigin = new Point (10, 20);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;


			state_modified = g.Save (); // Modified

			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.Default;
			g.InterpolationMode = InterpolationMode.Bilinear;
			g.PageScale = 5;
			g.PageUnit = GraphicsUnit.Display;
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.Clip = new Region (new Rectangle (1, 2, 20, 25));
			g.RenderingOrigin = new Point (5, 6);
			g.SmoothingMode = SmoothingMode.None;
			g.TextRenderingHint = TextRenderingHint.SystemDefault;

			g.Restore (state_modified);

			Assert.AreEqual (CompositingMode.SourceCopy, g.CompositingMode, "SetSaveReset1");
			Assert.AreEqual (CompositingQuality.GammaCorrected, g.CompositingQuality, "SetSaveReset2");
			Assert.AreEqual (InterpolationMode.HighQualityBilinear, g.InterpolationMode, "SetSaveReset3");
			Assert.AreEqual (2, g.PageScale, "SetSaveReset4");
			Assert.AreEqual (GraphicsUnit.Inch, g.PageUnit, "SetSaveReset5");
			Assert.AreEqual (PixelOffsetMode.Half, g.PixelOffsetMode, "SetSaveReset6");
			Assert.AreEqual (new Point (10, 20), g.RenderingOrigin, "SetSaveReset7");
			Assert.AreEqual (SmoothingMode.AntiAlias, g.SmoothingMode, "SetSaveReset8");
			Assert.AreEqual (TextRenderingHint.ClearTypeGridFit, g.TextRenderingHint, "SetSaveReset9");
			Assert.AreEqual (0, (int) g.ClipBounds.X, "SetSaveReset10");
			Assert.AreEqual (0, (int) g.ClipBounds.Y, "SetSaveReset10");

			g.Restore (state_default);

			Assert.AreEqual (CompositingMode.SourceOver, g.CompositingMode, "SetSaveReset11");
			Assert.AreEqual (CompositingQuality.Default, g.CompositingQuality, "SetSaveReset12");
			Assert.AreEqual (InterpolationMode.Bilinear, g.InterpolationMode, "SetSaveReset13");
			Assert.AreEqual (1, g.PageScale, "SetSaveReset14");
			Assert.AreEqual (GraphicsUnit.Display, g.PageUnit, "SetSaveReset15");
			Assert.AreEqual (PixelOffsetMode.Default, g.PixelOffsetMode, "SetSaveReset16");
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "SetSaveReset17");
			Assert.AreEqual (SmoothingMode.None, g.SmoothingMode, "SetSaveReset18");
			Assert.AreEqual (TextRenderingHint.SystemDefault, g.TextRenderingHint, "SetSaveReset19");

			Region r = new Region ();
			Assert.AreEqual (r.GetBounds (g), g.ClipBounds, "SetSaveReset20");

			g.Dispose ();
		}

		[Test]
		[Category ("NotWorking")] // looks like MS PNG codec promote indexed format to 32bpp ARGB
		public void LoadIndexed_PngStream ()
		{
			// Tests that we can load an indexed file
			using (Stream s = TestResourceHelper.GetStreamOfResource ("Test/resources/indexed.png")) {
				using (Image img = Image.FromStream (s)) {
					// however it's no more indexed once loaded
					Assert.AreEqual (PixelFormat.Format32bppArgb, img.PixelFormat, "PixelFormat");
					using (Graphics g = Graphics.FromImage (img)) {
						Assert.AreEqual (img.Height, g.VisibleClipBounds.Height, "Height");
						Assert.AreEqual (img.Width, g.VisibleClipBounds.Width, "Width");
					}
				}
			}
		}

		[Test]
		public void LoadIndexed_BmpFile ()
		{
			// Tests that we can load an indexed file, but...
			string sInFile = TestResourceHelper.GetFullPathOfResource ("Test/System.Drawing/bitmaps/almogaver1bit.bmp");
			// note: file is misnamed (it's a 4bpp bitmap)
			using (Image img = Image.FromFile (sInFile)) {
				Assert.AreEqual (PixelFormat.Format4bppIndexed, img.PixelFormat, "PixelFormat");
				Assert.Throws<Exception> (() => Graphics.FromImage (img));
			}
		}

		[Test]
		public void FromImage ()
		{
			Assert.Throws<ArgumentNullException> (() => Graphics.FromImage (null));
		}

		private Graphics Get (int w, int h)
		{
			Bitmap bitmap = new Bitmap (w, h);
			Graphics g = Graphics.FromImage (bitmap);
			g.Clip = new Region (new Rectangle (0, 0, w, h));
			return g;
		}

		private void Compare (string msg, RectangleF b1, RectangleF b2)
		{
			AssertEquals (msg + ".compare.X", b1.X, b2.X);
			AssertEquals (msg + ".compare.Y", b1.Y, b2.Y);
			AssertEquals (msg + ".compare.Width", b1.Width, b2.Width);
			AssertEquals (msg + ".compare.Height", b1.Height, b2.Height);
		}

		[Test]
		public void Clip_GetBounds ()
		{
			Graphics g = Get (16, 16);
			RectangleF bounds = g.Clip.GetBounds (g);
			Assert.AreEqual (0, bounds.X, "X");
			Assert.AreEqual (0, bounds.Y, "Y");
			Assert.AreEqual (16, bounds.Width, "Width");
			Assert.AreEqual (16, bounds.Height, "Height");
			Assert.IsTrue (g.Transform.IsIdentity, "Identity");
			g.Dispose ();
		}

		[Test]
		public void Clip_TranslateTransform ()
		{
			Graphics g = Get (16, 16);
			g.TranslateTransform (12.22f, 10.10f);
			RectangleF bounds = g.Clip.GetBounds (g);
			Compare ("translate", bounds, g.ClipBounds);
			Assert.AreEqual (-12.2200003f, bounds.X, "translate.X");
			Assert.AreEqual (-10.1000004f, bounds.Y, "translate.Y");
			Assert.AreEqual (16, bounds.Width, "translate.Width");
			Assert.AreEqual (16, bounds.Height, "translate.Height");
			float[] elements = g.Transform.Elements;
			Assert.AreEqual (1, elements[0], "translate.0");
			Assert.AreEqual (0, elements[1], "translate.1");
			Assert.AreEqual (0, elements[2], "translate.2");
			Assert.AreEqual (1, elements[3], "translate.3");
			Assert.AreEqual (12.2200003f, elements[4], "translate.4");
			Assert.AreEqual (10.1000004f, elements[5], "translate.5");

			g.ResetTransform ();
			bounds = g.Clip.GetBounds (g);
			Compare ("reset", bounds, g.ClipBounds);
			Assert.AreEqual (0, bounds.X, "reset.X");
			Assert.AreEqual (0, bounds.Y, "reset.Y");
			Assert.AreEqual (16, bounds.Width, "reset.Width");
			Assert.AreEqual (16, bounds.Height, "reset.Height");
			Assert.IsTrue (g.Transform.IsIdentity, "Identity");
			g.Dispose ();
		}

		[Test]
		public void Transform_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "IsInvertible");
			Graphics g = Get (16, 16);
			Assert.Throws<ArgumentException> (() => g.Transform = matrix);
		}


		[Test]
		public void Multiply_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "IsInvertible");
			Graphics g = Get (16, 16);
			Assert.Throws<ArgumentException> (() => g.MultiplyTransform (matrix));
		}

		[Test]
		public void Multiply_Null ()
		{
			Graphics g = Get (16, 16);
			Assert.Throws<ArgumentNullException> (() => g.MultiplyTransform (null));
		}

		private void CheckBounds (string msg, RectangleF bounds, float x, float y, float w, float h)
		{
			AssertEquals (msg + ".X", x, bounds.X, 0.1);
			AssertEquals (msg + ".Y", y, bounds.Y, 0.1);
			AssertEquals (msg + ".Width", w, bounds.Width, 0.1);
			AssertEquals (msg + ".Height", h, bounds.Height, 0.1);
		}

		[Test]
		public void ClipBounds ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			CheckBounds ("clip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("clip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Rotate ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.RotateTransform (90);
			CheckBounds ("rotate.ClipBounds", g.ClipBounds, 0, -8, 8, 8);
			CheckBounds ("rotate.Clip.GetBounds", g.Clip.GetBounds (g), 0, -8, 8, 8);

			g.Transform = new Matrix ();
			CheckBounds ("identity.ClipBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
			CheckBounds ("identity.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Scale ()
		{
			RectangleF clip = new Rectangle (0, 0, 8, 8);
			Graphics g = Get (16, 16);
			g.Clip = new Region (clip);
			g.ScaleTransform (0.25f, 0.5f);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 32, 16);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 32, 16);

			g.SetClip (clip);
			CheckBounds ("setclip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("setclip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Translate ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			Region clone = g.Clip.Clone ();
			g.TranslateTransform (8, 8);
			CheckBounds ("translate.ClipBounds", g.ClipBounds, -8, -8, 8, 8);
			CheckBounds ("translate.Clip.GetBounds", g.Clip.GetBounds (g), -8, -8, 8, 8);

			g.SetClip (clone, CombineMode.Replace);
			CheckBounds ("setclip.ClipBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
			CheckBounds ("setclip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Transform_Translation ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (1, 0, 0, 1, 8, 8);
			CheckBounds ("transform.ClipBounds", g.ClipBounds, -8, -8, 8, 8);
			CheckBounds ("transform.Clip.GetBounds", g.Clip.GetBounds (g), -8, -8, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Transform_Scale ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (0.5f, 0, 0, 0.25f, 0, 0);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 16, 32);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 32);

			g.ResetClip ();
			// see next test for ClipBounds
			CheckBounds ("resetclip.Clip.GetBounds", g.Clip.GetBounds (g), -4194304, -4194304, 8388608, 8388608);
			Assert.IsTrue (g.Clip.IsInfinite (g), "IsInfinite");
		}

		[Test]
		[Category ("NotWorking")]
		public void ClipBounds_Transform_Scale_Strange ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (0.5f, 0, 0, 0.25f, 0, 0);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, 0, 16, 32);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 32);

			g.ResetClip ();
			// note: strange case where g.ClipBounds and g.Clip.GetBounds are different
			CheckBounds ("resetclip.ClipBounds", g.ClipBounds, -8388608, -16777216, 16777216, 33554432);
		}

		[Test]
		public void ClipBounds_Multiply ()
		{
			Graphics g = Get (16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			g.Transform = new Matrix (1, 0, 0, 1, 8, 8);
			g.MultiplyTransform (g.Transform);
			CheckBounds ("multiply.ClipBounds", g.ClipBounds, -16, -16, 8, 8);
			CheckBounds ("multiply.Clip.GetBounds", g.Clip.GetBounds (g), -16, -16, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void ClipBounds_Cumulative_Effects ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			CheckBounds ("clip.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("clip.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.RotateTransform (90);
			CheckBounds ("rotate.ClipBounds", g.ClipBounds, 0, -8, 8, 8);
			CheckBounds ("rotate.Clip.GetBounds", g.Clip.GetBounds (g), 0, -8, 8, 8);

			g.ScaleTransform (0.25f, 0.5f);
			CheckBounds ("scale.ClipBounds", g.ClipBounds, 0, -16, 32, 16);
			CheckBounds ("scale.Clip.GetBounds", g.Clip.GetBounds (g), 0, -16, 32, 16);

			g.TranslateTransform (8, 8);
			CheckBounds ("translate.ClipBounds", g.ClipBounds, -8, -24, 32, 16);
			CheckBounds ("translate.Clip.GetBounds", g.Clip.GetBounds (g), -8, -24, 32, 16);

			g.MultiplyTransform (g.Transform);
			CheckBounds ("multiply.ClipBounds", g.ClipBounds, -104, -56, 64, 64);
			CheckBounds ("multiply.Clip.GetBounds", g.Clip.GetBounds (g), -104, -56, 64, 64);

			g.ResetTransform ();
			CheckBounds ("reset.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			CheckBounds ("reset.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);
		}

		[Test]
		public void Clip_TranslateTransform_BoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.TranslateTransform (-16, -16);
			CheckBounds ("translated.ClipBounds", g.ClipBounds, 16, 16, 16, 16);
			CheckBounds ("translated.Clip.GetBounds", g.Clip.GetBounds (g), 16, 16, 16, 16);

			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous translation
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous translation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -16, -16, 8, 8);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -16, -16, 8, 8);
		}

		[Test]
		public void Clip_RotateTransform_BoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			// we select a "simple" angle because the region will be converted into
			// a bitmap (well for libgdiplus) and we would lose precision after that
			g.RotateTransform (90);
			CheckBounds ("rotated.ClipBounds", g.ClipBounds, 0, -16, 16, 16);
			CheckBounds ("rotated.Clip.GetBounds", g.Clip.GetBounds (g), 0, -16, 16, 16);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous rotation (90)
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous rotation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -8, 0, 8, 8);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -8, 0, 8, 8);
		}

		private void CheckBoundsInt (string msg, RectangleF bounds, int x, int y, int w, int h)
		{
			// currently bounds are rounded at 8 pixels (FIXME - we can go down to 1 pixel)
			AssertEquals (msg + ".X", x, bounds.X, 4f);
			AssertEquals (msg + ".Y", y, bounds.Y, 4f);
			AssertEquals (msg + ".Width", w, bounds.Width, 4f);
			AssertEquals (msg + ".Height", h, bounds.Height, 4f);
		}

		[Test]
		[Category ("NotWorking")]
		public void Clip_RotateTransform_BoundsChange_45 ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.RotateTransform (45);
			// we can't use the "normal" CheckBound here because of libgdiplus crude rounding
			CheckBoundsInt ("rotated.ClipBounds", g.ClipBounds, 0, -11, 24, 24);
			CheckBoundsInt ("rotated.Clip.GetBounds", g.Clip.GetBounds (g), 0, -11, 24, 24);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds IS affected by a previous rotation (45)
			CheckBoundsInt ("rectangle.ClipBounds", g.ClipBounds, -3, -4, 16, 16);
			// Clip.GetBounds isn't affected by a previous rotation
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -5, 1, 11, 11);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -5.6f, 0, 11.3f, 11.3f);
		}

		[Test]
		public void Clip_ScaleTransform_NoBoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.ScaleTransform (2, 0.5f);
			CheckBounds ("scaled.ClipBounds", g.ClipBounds, 0, 0, 8, 32);
			CheckBounds ("scaled.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 32);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds isn't affected by a previous scaling
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, 0, 0, 8, 8);
			// Clip.GetBounds isn't affected by a previous scaling
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, 0, 0, 16, 4);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 4);
		}

		[Test]
		[Category ("NotWorking")]
		public void Clip_MultiplyTransform_NoBoundsChange ()
		{
			Graphics g = Get (16, 16);
			CheckBounds ("graphics.ClipBounds", g.ClipBounds, 0, 0, 16, 16);
			CheckBounds ("graphics.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 16, 16);
			g.MultiplyTransform (new Matrix (2.5f, 0.5f, -2.5f, 0.5f, 4, -4));
			CheckBounds ("multiplied.ClipBounds", g.ClipBounds, 3.2f, 1.6f, 19.2f, 19.2f);
			CheckBounds ("multiplied.Clip.GetBounds", g.Clip.GetBounds (g), 3.2f, 1.6f, 19.2f, 19.2f);
			g.Clip = new Region (new Rectangle (0, 0, 8, 8));
			// ClipBounds IS affected by the previous multiplication
			CheckBounds ("rectangle.ClipBounds", g.ClipBounds, -3, -3, 15, 15);
			// Clip.GetBounds isn't affected by the previous multiplication
			CheckBounds ("rectangle.Clip.GetBounds", g.Clip.GetBounds (g), 0, 0, 8, 8);

			g.ResetTransform ();
			CheckBounds ("reseted.ClipBounds", g.ClipBounds, -16, -3, 40, 7);
			CheckBounds ("reseted.Clip.GetBounds", g.Clip.GetBounds (g), -16, -4, 40, 8);
		}

		[Test]
		public void ScaleTransform_X0 ()
		{
			Graphics g = Get (16, 16);
			Assert.Throws<ArgumentException> (() => g.ScaleTransform (0, 1));
		}

		[Test]
		public void ScaleTransform_Y0 ()
		{
			Graphics g = Get (16, 16);
			Assert.Throws<ArgumentException> (() => g.ScaleTransform (1, 0));
		}

		[Test]
		public void TranslateTransform_Order ()
		{
			Graphics g = Get (16, 16);
			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3);
			float[] elements = g.Transform.Elements;
			Assert.AreEqual (1, elements[0], "default.0");
			Assert.AreEqual (2, elements[1], "default.1");
			Assert.AreEqual (3, elements[2], "default.2");
			Assert.AreEqual (4, elements[3], "default.3");
			Assert.AreEqual (-1, elements[4], "default.4");
			Assert.AreEqual (0, elements[5], "default.5");

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Prepend);
			elements = g.Transform.Elements;
			Assert.AreEqual (1, elements[0], "prepend.0");
			Assert.AreEqual (2, elements[1], "prepend.1");
			Assert.AreEqual (3, elements[2], "prepend.2");
			Assert.AreEqual (4, elements[3], "prepend.3");
			Assert.AreEqual (-1, elements[4], "prepend.4");
			Assert.AreEqual (0, elements[5], "prepend.5");

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Append);
			elements = g.Transform.Elements;
			Assert.AreEqual (1, elements[0], "append.0");
			Assert.AreEqual (2, elements[1], "append.1");
			Assert.AreEqual (3, elements[2], "append.2");
			Assert.AreEqual (4, elements[3], "append.3");
			Assert.AreEqual (8, elements[4], "append.4");
			Assert.AreEqual (3, elements[5], "append.5");
		}

		static Point[] SmallCurve = new Point[3] { new Point (0, 0), new Point (15, 5), new Point (5, 15) };
		static PointF[] SmallCurveF = new PointF[3] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15) };

		static Point[] TooSmallCurve = new Point[2] { new Point (0, 0), new Point (15, 5) };
		static PointF[] LargeCurveF = new PointF[4] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15), new PointF (0, 20) };

		[Test]
		public void DrawCurve_PenNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentNullException> (() => g.DrawCurve (null, SmallCurveF));
		}

		[Test]
		public void DrawCurve_PointFNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentNullException> (() => g.DrawCurve (Pens.Black, (PointF[]) null));
		}

		[Test]
		public void DrawCurve_PointNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentNullException> (() => g.DrawCurve (Pens.Black, (Point[]) null));
		}

		[Test]
		public void DrawCurve_NotEnoughPoints ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			CheckForEmptyBitmap (bitmap);
			g.DrawCurve (Pens.Black, TooSmallCurve, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			// so a "curve" can be drawn with less than 3 points!
			// actually I used to call that a line... (and it's not related to tension)
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_SinglePoint ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentException> (() => g.DrawCurve (Pens.Black, new Point[1] { new Point (10, 10) }, 0.5f));
			// a single point isn't enough
		}

		[Test]
		public void DrawCurve3_NotEnoughPoints ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentException> (() => g.DrawCurve (Pens.Black, TooSmallCurve, 0, 2, 0.5f));
			// aha, this is API dependent
		}

		[Test]
		public void DrawCurve_NegativeTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// documented as bigger (or equals) to 0
			g.DrawCurve (Pens.Black, SmallCurveF, -0.9f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_PositiveTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0.9f);
			// this is not the same as -1
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus is drawing something
		public void DrawCurve_LargeTension ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurve, Single.MaxValue);
			CheckForEmptyBitmap (bitmap);
			// too much tension ;)
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_ZeroSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentException> (() => g.DrawCurve (Pens.Black, SmallCurveF, 0, 0));
		}

		[Test]
		public void DrawCurve_NegativeSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentException> (() => g.DrawCurve (Pens.Black, SmallCurveF, 0, -1));
		}

		[Test]
		public void DrawCurve_OffsetTooLarge ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// starting offset 1 doesn't give 3 points to make a curve
			Assert.Throws<ArgumentException> (() => g.DrawCurve (Pens.Black, SmallCurveF, 1, 2));
			// and in this case 2 points aren't enough to draw something
		}

		[Test]
		public void DrawCurve_Offset_0 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, LargeCurveF, 0, 2, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_Offset_1 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, LargeCurveF, 1, 2, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawCurve_Offset_2 ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// it works even with two points because we know the previous ones
			g.DrawCurve (Pens.Black, LargeCurveF, 2, 1, 0.5f);
			CheckForNonEmptyBitmap (bitmap);
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawRectangle_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Pen pen = new Pen (Color.Red);
			g.DrawRectangle (pen, 5, 5, -10, -10);
			g.DrawRectangle (pen, 0.0f, 0.0f, 5.0f, -10.0f);
			g.DrawRectangle (pen, new Rectangle (15, 0, -10, 5));
			CheckForEmptyBitmap (bitmap);
			pen.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void DrawRectangles_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Pen pen = new Pen (Color.Red);
			Rectangle[] rects = new Rectangle[2] {
				new Rectangle (5, 5, -10, -10), new Rectangle (0, 0, 5, -10)
			};
			RectangleF[] rectf = new RectangleF[2] {
				new RectangleF (0.0f, 5.0f, -10.0f, -10.0f), new RectangleF (15.0f, 0.0f, -10.0f, 5.0f)
			};
			g.DrawRectangles (pen, rects);
			g.DrawRectangles (pen, rectf);
			CheckForEmptyBitmap (bitmap);
			pen.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void FillRectangle_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			SolidBrush brush = new SolidBrush (Color.Red);
			g.FillRectangle (brush, 5, 5, -10, -10);
			g.FillRectangle (brush, 0.0f, 0.0f, 5.0f, -10.0f);
			g.FillRectangle (brush, new Rectangle (15, 0, -10, 5));
			CheckForEmptyBitmap (bitmap);
			brush.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test]
		public void FillRectangles_Negative ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			SolidBrush brush = new SolidBrush (Color.Red);
			Rectangle[] rects = new Rectangle[2] {
				new Rectangle (5, 5, -10, -10), new Rectangle (0, 0, 5, -10)
			};
			RectangleF[] rectf = new RectangleF[2] {
				new RectangleF (0.0f, 5.0f, -10.0f, -10.0f), new RectangleF (15.0f, 0.0f, -10.0f, 5.0f)
			};
			g.FillRectangles (brush, rects);
			g.FillRectangles (brush, rectf);
			CheckForEmptyBitmap (bitmap);
			brush.Dispose ();
			g.Dispose ();
			bitmap.Dispose ();
		}

		[Test] // bug #355141
		[Category ("CAS")]
		public void FromHwnd_Zero ()
		{
			Graphics g = Graphics.FromHwnd (IntPtr.Zero);
			Assert.IsNotNull (g);
		}

		private void CheckDefaultProperties (string message, Graphics g)
		{
			Assert.IsTrue (g.Clip.IsInfinite (g), message + ".Clip.IsInfinite");
			AssertEquals (message + ".CompositingMode", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals (message + ".CompositingQuality", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals (message + ".InterpolationMode", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals (message + ".PageScale", 1.0f, g.PageScale);
			AssertEquals (message + ".PageUnit", GraphicsUnit.Display, g.PageUnit);
			AssertEquals (message + ".PixelOffsetMode", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals (message + ".SmoothingMode", SmoothingMode.None, g.SmoothingMode);
			AssertEquals (message + ".TextContrast", 4, g.TextContrast);
			AssertEquals (message + ".TextRenderingHint", TextRenderingHint.SystemDefault, g.TextRenderingHint);
			Assert.IsTrue (g.Transform.IsIdentity, message + ".Transform.IsIdentity");
		}

		private void CheckCustomProperties (string message, Graphics g)
		{
			Assert.IsFalse (g.Clip.IsInfinite (g), message + ".Clip.IsInfinite");
			AssertEquals (message + ".CompositingMode", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals (message + ".CompositingQuality", CompositingQuality.HighQuality, g.CompositingQuality);
			AssertEquals (message + ".InterpolationMode", InterpolationMode.HighQualityBicubic, g.InterpolationMode);
			AssertEquals (message + ".PageScale", 0.5f, g.PageScale);
			AssertEquals (message + ".PageUnit", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals (message + ".PixelOffsetMode", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals (message + ".RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);
			AssertEquals (message + ".SmoothingMode", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals (message + ".TextContrast", 0, g.TextContrast);
			AssertEquals (message + ".TextRenderingHint", TextRenderingHint.AntiAlias, g.TextRenderingHint);
			Assert.IsFalse (g.Transform.IsIdentity, message + ".Transform.IsIdentity");
		}

		private void CheckMatrix (string message, Matrix m, float xx, float yx, float xy, float yy, float x0, float y0)
		{
			float[] elements = m.Elements;
			AssertEquals (message + ".Matrix.xx", xx, elements[0], 0.01);
			AssertEquals (message + ".Matrix.yx", yx, elements[1], 0.01);
			AssertEquals (message + ".Matrix.xy", xy, elements[2], 0.01);
			AssertEquals (message + ".Matrix.yy", yy, elements[3], 0.01);
			AssertEquals (message + ".Matrix.x0", x0, elements[4], 0.01);
			AssertEquals (message + ".Matrix.y0", y0, elements[5], 0.01);
		}

		[Test]
		public void BeginContainer ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "default.RenderingOrigin");

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer ();
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			Assert.AreEqual (new Point (-1, -1), g.RenderingOrigin, "BeginContainer.RenderingOrigin");

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
		}

		[Test]
		public void BeginContainer_Rect ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "default.RenderingOrigin");

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer (new Rectangle (10, 20, 30, 40), new Rectangle (10, 20, 300, 400), GraphicsUnit.Millimeter);
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			Assert.AreEqual (new Point (-1, -1), g.RenderingOrigin, "BeginContainer.RenderingOrigin");

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
			CheckMatrix ("EndContainer.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);
		}

		[Test]
		public void BeginContainer_RectF ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "default.RenderingOrigin");

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsContainer gc = g.BeginContainer (new RectangleF (40, 30, 20, 10), new RectangleF (10, 20, 30, 40), GraphicsUnit.Inch);
			// things gets reseted after calling BeginContainer
			CheckDefaultProperties ("BeginContainer", g);
			// but not everything 
			Assert.AreEqual (new Point (-1, -1), g.RenderingOrigin, "BeginContainer.RenderingOrigin");

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
		}

		private void BeginContainer_GraphicsUnit (GraphicsUnit unit)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.BeginContainer (new RectangleF (40, 30, 20, 10), new RectangleF (10, 20, 30, 40), unit);
		}

		[Test]
		public void BeginContainer_GraphicsUnit_Display ()
		{
			Assert.Throws<ArgumentException> (() => BeginContainer_GraphicsUnit(GraphicsUnit.Display));
		}

		[Test]
		public void BeginContainer_GraphicsUnit_Valid ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.Document);
			BeginContainer_GraphicsUnit (GraphicsUnit.Inch);
			BeginContainer_GraphicsUnit (GraphicsUnit.Millimeter);
			BeginContainer_GraphicsUnit (GraphicsUnit.Pixel);
			BeginContainer_GraphicsUnit (GraphicsUnit.Point);
		}

		[Test]
		public void BeginContainer_GraphicsUnit_World ()
		{
			Assert.Throws<ArgumentException> (() => BeginContainer_GraphicsUnit(GraphicsUnit.World));
		}

		[Test]
		public void BeginContainer_GraphicsUnit_Bad ()
		{
			Assert.Throws<ArgumentException> (() => BeginContainer_GraphicsUnit((GraphicsUnit) Int32.MinValue));
		}

		[Test]
		public void EndContainer_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<ArgumentNullException> (() => g.EndContainer (null));
		}

		[Test]
		public void Save ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "default.RenderingOrigin");

			GraphicsState gs1 = g.Save ();
			// nothing is changed after a save
			CheckDefaultProperties ("save1", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "save1.RenderingOrigin");

			g.Clip = new Region (new Rectangle (10, 10, 10, 10));
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PageScale = 0.5f;
			g.PageUnit = GraphicsUnit.Inch;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.RenderingOrigin = new Point (-1, -1);
			g.RotateTransform (45);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextContrast = 0;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;
			CheckCustomProperties ("modified", g);
			CheckMatrix ("modified.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			GraphicsState gs2 = g.Save ();
			CheckCustomProperties ("save2", g);

			g.Restore (gs2);
			CheckCustomProperties ("restored1", g);
			CheckMatrix ("restored1.Transform", g.Transform, 0.707f, 0.707f, -0.707f, 0.707f, 0, 0);

			g.Restore (gs1);
			CheckDefaultProperties ("restored2", g);
			Assert.AreEqual (new Point (0, 0), g.RenderingOrigin, "restored2.RenderingOrigin");
		}

		[Test]
		public void Restore_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			Assert.Throws<NullReferenceException> (() => g.Restore (null));
		}

		[Test]
		public void FillRectangles_BrushNull_Rectangle ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.FillRectangles (null, new Rectangle[1]));
				}
			}
		}

		[Test]
		public void FillRectangles_Rectangle_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.FillRectangles (Brushes.Red, (Rectangle[]) null));
				}
			}
		}

		[Test] // see bug #78408
		public void FillRectanglesZeroRectangle ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentException> (() => g.FillRectangles (Brushes.Red, new Rectangle[0]));
				}
			}
		}

		[Test]
		public void FillRectangles_BrushNull_RectangleF ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.FillRectangles (null, new RectangleF[1]));
				}
			}
		}

		[Test]
		public void FillRectangles_RectangleF_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.FillRectangles (Brushes.Red, (RectangleF[]) null));
				}
			}
		}

		[Test] // see bug #78408
		public void FillRectanglesZeroRectangleF ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentException> (() => g.FillRectangles (Brushes.Red, new RectangleF[0]));
				}
			}
		}

		[Test]
		public void FillRectangles_NormalBehavior ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.Clear (Color.Fuchsia);
					Rectangle rect = new Rectangle (5, 5, 10, 10);
					g.Clip = new Region (rect);
					g.FillRectangle (Brushes.Red, rect);
				}
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (5, 14).ToArgb (), "5,14");
				Assert.AreEqual (Color.Red.ToArgb (), bitmap.GetPixel (14, 14).ToArgb (), "14,14");

				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (Color.Fuchsia.ToArgb (), bitmap.GetPixel (15, 15).ToArgb (), "15,15");
			}
		}

		// see bug #81737 for details
		private Bitmap FillDrawRectangle (float width)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				g.Clear (Color.Red);
				Rectangle rect = new Rectangle (5, 5, 10, 10);
				g.FillRectangle (Brushes.Green, rect);
				if (width >= 0) {
					using (Pen pen = new Pen (Color.Blue, width)) {
						g.DrawRectangle (pen, rect);
					}
				} else {
					g.DrawRectangle (Pens.Blue, rect);
				}
			}
			return bitmap;
		}

		[Test]
		public void FillDrawRectangle_Width_Default ()
		{
			// default pen size
			using (Bitmap bitmap = FillDrawRectangle (Single.MinValue)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		[Category ("NotOnMac")]
		public void FillDrawRectangle_Width_2 ()
		{
			// even pen size
			using (Bitmap bitmap = FillDrawRectangle (2.0f)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 6).ToArgb (), "13,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 9).ToArgb (), "13,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 13).ToArgb (), "13,13");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 13).ToArgb (), "9,13");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 16).ToArgb (), "3,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 15).ToArgb (), "4,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 14).ToArgb (), "5,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 13).ToArgb (), "6,13");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		public void FillDrawRectangle_Width_3 ()
		{
			// odd pen size
			using (Bitmap bitmap = FillDrawRectangle (3.0f)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 7).ToArgb (), "7,7");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 7).ToArgb (), "9,7");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 3).ToArgb (), "17,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 7).ToArgb (), "13,7");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 9).ToArgb (), "17,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 9).ToArgb (), "13,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 17).ToArgb (), "17,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (13, 13).ToArgb (), "13,13");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 17).ToArgb (), "9,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 13).ToArgb (), "9,13");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 17).ToArgb (), "3,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 13).ToArgb (), "7,13");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (7, 9).ToArgb (), "7,9");
			}
		}

		// reverse, draw the fill over
		private Bitmap DrawFillRectangle (float width)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				g.Clear (Color.Red);
				Rectangle rect = new Rectangle (5, 5, 10, 10);
				if (width >= 0) {
					using (Pen pen = new Pen (Color.Blue, width)) {
						g.DrawRectangle (pen, rect);
					}
				} else {
					g.DrawRectangle (Pens.Blue, rect);
				}
				g.FillRectangle (Brushes.Green, rect);
			}
			return bitmap;
		}

		[Test]
		public void DrawFillRectangle_Width_Default ()
		{
			// default pen size
			using (Bitmap bitmap = DrawFillRectangle (Single.MinValue)) {
				// NW - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 6).ToArgb (), "6,6");
				// N - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 6).ToArgb (), "9,6");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W - no blue border
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 9).ToArgb (), "6,9");
			}
		}

		[Test]
		[Category ("NotOnMac")]
		public void DrawFillRectangle_Width_2 ()
		{
			// even pen size
			using (Bitmap bitmap = DrawFillRectangle (2.0f)) {
				// looks like a one pixel border - but enlarged
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
			}
		}

		[Test]
		public void DrawFillRectangle_Width_3 ()
		{
			// odd pen size
			using (Bitmap bitmap = DrawFillRectangle (3.0f)) {
				// NW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 3).ToArgb (), "3,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				// N
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 3).ToArgb (), "9,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 4).ToArgb (), "9,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 5).ToArgb (), "9,5");
				// NE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 3).ToArgb (), "17,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				// E
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 9).ToArgb (), "17,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 9).ToArgb (), "16,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 9).ToArgb (), "15,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 9).ToArgb (), "14,9");
				// SE
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 17).ToArgb (), "17,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				// S
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (9, 17).ToArgb (), "9,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 16).ToArgb (), "9,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (9, 15).ToArgb (), "9,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (9, 14).ToArgb (), "9,14");
				// SW
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 17).ToArgb (), "3,17");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 16).ToArgb (), "4,16");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 15).ToArgb (), "5,15");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (6, 14).ToArgb (), "6,14");
				// W
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (3, 9).ToArgb (), "3,9");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (4, 9).ToArgb (), "4,9");
				Assert.AreEqual (0xFF008000, (uint) bitmap.GetPixel (5, 9).ToArgb (), "5,9");
			}
		}

		private Bitmap DrawLines (float width)
		{
			Bitmap bitmap = new Bitmap (20, 20);
			using (Graphics g = Graphics.FromImage (bitmap)) {
				g.Clear (Color.Red);
				Point[] pts = new Point[3] { new Point (5, 5), new Point (15, 5), new Point (15, 15) };
				if (width >= 0) {
					using (Pen pen = new Pen (Color.Blue, width)) {
						g.DrawLines (pen, pts);
					}
				} else {
					g.DrawLines (Pens.Blue, pts);
				}
			}
			return bitmap;
		}

		[Test]
		public void DrawLines_Width_Default ()
		{
			// default pen size
			using (Bitmap bitmap = DrawLines (Single.MinValue)) {
				// start
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 5).ToArgb (), "4,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 6).ToArgb (), "4,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 4).ToArgb (), "5,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 6).ToArgb (), "5,6");
				// middle
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 4).ToArgb (), "14,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 6).ToArgb (), "15,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 5).ToArgb (), "16,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 6).ToArgb (), "16,6");
				//end
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 15).ToArgb (), "14,15");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 15).ToArgb (), "16,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 16).ToArgb (), "14,16");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 16).ToArgb (), "15,16");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 16).ToArgb (), "16,16");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void DrawLines_Width_2 ()
		{
			// default pen size
			using (Bitmap bitmap = DrawLines (2.0f)) {
				// start
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 3).ToArgb (), "4,3");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 5).ToArgb (), "4,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 6).ToArgb (), "4,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 3).ToArgb (), "5,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 4).ToArgb (), "5,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 6).ToArgb (), "5,6");
				// middle
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 3).ToArgb (), "13,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (13, 4).ToArgb (), "13,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (13, 5).ToArgb (), "13,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 6).ToArgb (), "13,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 3).ToArgb (), "14,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 4).ToArgb (), "14,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 3).ToArgb (), "15,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 6).ToArgb (), "15,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 5).ToArgb (), "16,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 6).ToArgb (), "16,6");
				//end
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 14).ToArgb (), "13,14");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 14).ToArgb (), "15,14");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 14).ToArgb (), "16,14");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 15).ToArgb (), "13,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 15).ToArgb (), "14,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 15).ToArgb (), "16,15");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void DrawLines_Width_3 ()
		{
			// default pen size
			using (Bitmap bitmap = DrawLines (3.0f)) {
				// start
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 3).ToArgb (), "4,3");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 4).ToArgb (), "4,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 5).ToArgb (), "4,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 6).ToArgb (), "4,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (4, 7).ToArgb (), "4,7");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 3).ToArgb (), "5,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 4).ToArgb (), "5,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 5).ToArgb (), "5,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (5, 6).ToArgb (), "5,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (5, 7).ToArgb (), "5,7");
				// middle
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 3).ToArgb (), "13,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (13, 4).ToArgb (), "13,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (13, 5).ToArgb (), "13,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (13, 6).ToArgb (), "13,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 7).ToArgb (), "13,7");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 3).ToArgb (), "14,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 4).ToArgb (), "14,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 5).ToArgb (), "14,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 6).ToArgb (), "14,6");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 7).ToArgb (), "14,7");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 3).ToArgb (), "15,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 4).ToArgb (), "15,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 5).ToArgb (), "15,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 6).ToArgb (), "15,6");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 7).ToArgb (), "15,7");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 3).ToArgb (), "16,3");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 4).ToArgb (), "16,4");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 5).ToArgb (), "16,5");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 6).ToArgb (), "16,6");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 7).ToArgb (), "16,7");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 3).ToArgb (), "17,3");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 4).ToArgb (), "17,4");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 5).ToArgb (), "17,5");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 6).ToArgb (), "17,6");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 7).ToArgb (), "17,7");
				//end
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 14).ToArgb (), "13,14");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (14, 14).ToArgb (), "14,14");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (15, 14).ToArgb (), "15,14");
				Assert.AreEqual (0xFF0000FF, (uint) bitmap.GetPixel (16, 14).ToArgb (), "16,14");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 14).ToArgb (), "17,14");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (13, 15).ToArgb (), "13,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (14, 15).ToArgb (), "14,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (15, 15).ToArgb (), "15,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (16, 15).ToArgb (), "16,15");
				Assert.AreEqual (0xFFFF0000, (uint) bitmap.GetPixel (17, 15).ToArgb (), "17,15");
			}
		}

		[Test]
		public void MeasureString_StringFont ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (null, font);
					Assert.IsTrue (size.IsEmpty, "MeasureString(null,font)");
					size = g.MeasureString (String.Empty, font);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,font)");
					// null font
					size = g.MeasureString (null, null);
					Assert.IsTrue (size.IsEmpty, "MeasureString(null,null)");
					size = g.MeasureString (String.Empty, null);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,null)");
				}
			}
		}

		[Test]
		public void MeasureString_StringFont_Null ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.MeasureString ("a", null));
				}
			}
		}

		[Test]
		public void MeasureString_StringFontSizeF ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString ("a", font, SizeF.Empty);
					Assert.IsFalse (size.IsEmpty, "MeasureString(a,font,empty)");

					size = g.MeasureString (String.Empty, font, SizeF.Empty);
					Assert.IsTrue (size.IsEmpty, "MeasureString(empty,font,empty)");
				}
			}
		}

		private void MeasureString_StringFontInt (string s)
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size0 = g.MeasureString (s, font, 0);
					SizeF sizeN = g.MeasureString (s, font, Int32.MinValue);
					SizeF sizeP = g.MeasureString (s, font, Int32.MaxValue);
					Assert.AreEqual (size0, sizeN, "0-Min");
					Assert.AreEqual (size0, sizeP, "0-Max");
				}
			}
		}

		[Test]
		public void MeasureString_StringFontInt_ShortString ()
		{
			MeasureString_StringFontInt ("a");
		}

		[Test]
		public void MeasureString_StringFontInt_LongString ()
		{
			HostIgnoreList.CheckTest ("MonoTests.System.Drawing.GraphicsTest.MeasureString_StringFontInt_LongString");
			MeasureString_StringFontInt ("A very long string..."); // see bug #79643
		}

		[Test]
		public void MeasureString_StringFormat_Alignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_Alignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.Alignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_LineAlignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_StringFormat_LineAlignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					SizeF near = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Center;
					SizeF center = g.MeasureString (text, font, Int32.MaxValue, string_format);

					string_format.LineAlignment = StringAlignment.Far;
					SizeF far = g.MeasureString (text, font, Int32.MaxValue, string_format);

					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		public void MeasureString_MultlineString_Width ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					StringFormat string_format = new StringFormat ();

					string text1 = "Test\nTest123\nTest 456\nTest 1,2,3,4,5...";
					string text2 = "Test 1,2,3,4,5...";

					SizeF size1 = g.MeasureString (text1, font, SizeF.Empty, string_format);
					SizeF size2 = g.MeasureString (text2, font, SizeF.Empty, string_format);

					Assert.AreEqual ((int) size1.Width, (int) size2.Width, "Multiline Text Width");
				}
			}
		}

		[Test]
		public void MeasureString_Bug76664 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string s = "aaa aa aaaa a aaa";
					SizeF size = g.MeasureString (s, font);

					int chars, lines;
					SizeF size2 = g.MeasureString (s, font, new SizeF (80, size.Height), null, out chars, out lines);

					// in pixels
					Assert.IsTrue (size2.Width < size.Width, "Width/pixel");
					Assert.AreEqual (size2.Height, size.Height, "Height/pixel");

					Assert.AreEqual (1, lines, "lines fitted");
					// LAMESPEC: documentation seems to suggest chars is total length
					Assert.IsTrue (chars < s.Length, "characters fitted");
				}
			}
		}

		[Test]
		public void MeasureString_Bug80680 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string s = String.Empty;
					SizeF size = g.MeasureString (s, font);
					Assert.AreEqual (0, size.Height, "Empty.Height");
					Assert.AreEqual (0, size.Width, "Empty.Width");

					s += " ";
					SizeF expected = g.MeasureString (s, font);
					for (int i = 1; i < 10; i++) {
						s += " ";
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width, size.Width, 0.1, ">" + s + "< Width");
					}

					s = "a";
					expected = g.MeasureString (s, font);
					s = " " + s;
					size = g.MeasureString (s, font);
					float space_width = size.Width - expected.Width;
					for (int i = 1; i < 10; i++) {
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width + i * space_width, size.Width, 0.1, ">" + s + "< Width");
						s = " " + s;
					}

					s = "a";
					expected = g.MeasureString (s, font);
					for (int i = 1; i < 10; i++) {
						s = s + " ";
						size = g.MeasureString (s, font);
						Assert.AreEqual (expected.Height, size.Height, 0.1, ">" + s + "< Height");
						Assert.AreEqual (expected.Width, size.Width, 0.1, ">" + s + "< Width");
					}
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_NullOrEmptyText ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Region[] regions = g.MeasureCharacterRanges (null, font, new RectangleF (), null);
					Assert.AreEqual (0, regions.Length, "text null");
					regions = g.MeasureCharacterRanges (String.Empty, font, new RectangleF (), null);
					Assert.AreEqual (0, regions.Length, "text empty");
					// null font is ok with null or empty string
					regions = g.MeasureCharacterRanges (null, null, new RectangleF (), null);
					Assert.AreEqual (0, regions.Length, "text null/null font");
					regions = g.MeasureCharacterRanges (String.Empty, null, new RectangleF (), null);
					Assert.AreEqual (0, regions.Length, "text empty/null font");
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_EmptyStringFormat ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					// string format without character ranges
					Region[] regions = g.MeasureCharacterRanges ("Mono", font, new RectangleF (), new StringFormat ());
					Assert.AreEqual (0, regions.Length, "empty stringformat");
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_FontNull ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentNullException> (() => g.MeasureCharacterRanges ("a", null, new RectangleF (), null));
				}
			}
		}

		[Test] // adapted from bug #78777
		public void MeasureCharacterRanges_TwoLines ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "this\nis a test";
			CharacterRange[] ranges = new CharacterRange[2];
			ranges[0] = new CharacterRange (0, 5);
			ranges[1] = new CharacterRange (5, 9);

			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.NoClip;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0, 0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);
					Region[] regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);

					Assert.AreEqual (2, regions.Length, "Length");
					Assert.AreEqual (regions[0].GetBounds (g).Height, regions[1].GetBounds (g).Height, "Height");
				}
			}
		}

		private void MeasureCharacterRanges (string text, int first, int length)
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (first, length);

			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.NoClip;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0, 0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);
					g.MeasureCharacterRanges (text, font, layout_rect, string_format);
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_FirstTooFar ()
		{
			string text = "this\nis a test";
			Assert.Throws<ArgumentException> (() => MeasureCharacterRanges(text, text.Length, 1));
		}

		[Test]
		public void MeasureCharacterRanges_LengthTooLong ()
		{
			string text = "this\nis a test";
			Assert.Throws<ArgumentException> (() => MeasureCharacterRanges(text, 0, text.Length + 1));
		}

		[Test]
		public void MeasureCharacterRanges_Prefix ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello &Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);

			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					SizeF size = g.MeasureString (text, font, new Point (0, 0), string_format);
					RectangleF layout_rect = new RectangleF (0.0f, 0.0f, size.Width, size.Height);

					// here & is part of the measure and visible
					string_format.HotkeyPrefix = HotkeyPrefix.None;
					Region[] regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_none = regions[0].GetBounds (g);

					// here & is part of the measure (range) but visible as an underline
					string_format.HotkeyPrefix = HotkeyPrefix.Show;
					regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_show = regions[0].GetBounds (g);
					Assert.IsTrue (bounds_show.Width < bounds_none.Width, "Show<None");

					// here & is part of the measure (range) but invisible
					string_format.HotkeyPrefix = HotkeyPrefix.Hide;
					regions = g.MeasureCharacterRanges (text, font, layout_rect, string_format);
					RectangleF bounds_hide = regions[0].GetBounds (g);
					Assert.AreEqual (bounds_hide.Width, bounds_show.Width, "Hide==None");
				}
			}
		}

		[Test]
		public void MeasureCharacterRanges_NullStringFormat ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Assert.Throws<ArgumentException> (() => g.MeasureCharacterRanges ("Mono", font, new RectangleF (), null));
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_Alignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X < center.X, "near-center/X");
					Assert.AreEqual (near.Y, center.Y, 0.1, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.IsTrue (center.X < far.X, "center-far/X");
					Assert.AreEqual (center.Y, far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_LineAlignment ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.AreEqual (near.X, center.X, 0.1, "near-center/X");
					Assert.IsTrue (near.Y < center.Y, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.X, far.X, 0.1, "center-far/X");
					Assert.IsTrue (center.Y < far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_Alignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.Alignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.Alignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X == center.X, "near-center/X"); // ???
					Assert.IsTrue (near.Y < center.Y, "near-center/Y");
					Assert.IsTrue (near.Width == center.Width, "near-center/Width"); // ???
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.AreEqual (center.X, far.X, 0.1, "center-far/X");
					Assert.IsTrue (center.Y < far.Y, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void MeasureCharacterRanges_StringFormat_LineAlignment_DirectionVertical ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "Hello Mono::";
			CharacterRange[] ranges = new CharacterRange[1];
			ranges[0] = new CharacterRange (5, 4);
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.DirectionVertical;
			string_format.SetMeasurableCharacterRanges (ranges);

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					string_format.LineAlignment = StringAlignment.Near;
					Region[] regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Near.Region");
					RectangleF near = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Center;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Center.Region");
					RectangleF center = regions[0].GetBounds (g);

					string_format.LineAlignment = StringAlignment.Far;
					regions = g.MeasureCharacterRanges (text, font, new RectangleF (0, 0, 320, 32), string_format);
					Assert.AreEqual (1, regions.Length, "Far.Region");
					RectangleF far = regions[0].GetBounds (g);

					Assert.IsTrue (near.X < center.X, "near-center/X");
					Assert.AreEqual (near.Y, center.Y, 0.1, "near-center/Y");
					Assert.AreEqual (near.Width, center.Width, 0.1, "near-center/Width");
					Assert.AreEqual (near.Height, center.Height, 0.1, "near-center/Height");

					Assert.IsTrue (center.X < far.X, "center-far/X");
					Assert.AreEqual (center.Y, far.Y, 0.1, "center-far/Y");
					Assert.AreEqual (center.Width, far.Width, 0.1, "center-far/Width");
					Assert.AreEqual (center.Height, far.Height, 0.1, "center-far/Height");
				}
			}
		}

		static CharacterRange [] ranges = new CharacterRange [] {
					new CharacterRange (0, 1),
					new CharacterRange (1, 1),
					new CharacterRange (2, 1)
				};

		Region [] Measure (Graphics gfx, RectangleF rect)
		{
			using (StringFormat format = StringFormat.GenericTypographic) {
				format.SetMeasurableCharacterRanges (ranges);

				using (Font font = new Font (FontFamily.GenericSerif, 11.0f)) {
					return gfx.MeasureCharacterRanges ("abc", font, rect, format);
				}
			}
		}

		[Test]
		public void Measure ()
		{
			using (Graphics gfx = Graphics.FromImage (new Bitmap (1, 1))) {
				Region [] zero = Measure (gfx, new RectangleF (0, 0, 0, 0));
				Assert.AreEqual (3, zero.Length, "zero.Length");

				Region [] small = Measure (gfx, new RectangleF (0, 0, 100, 100));
				Assert.AreEqual (3, small.Length, "small.Length");
				for (int i = 0; i < 3; i++ ) {
					RectangleF zb = zero [i].GetBounds (gfx);
					RectangleF sb = small [i].GetBounds (gfx);
					Assert.AreEqual (sb.X, zb.X, "sx" + i.ToString ());
					Assert.AreEqual (sb.Y, zb.Y, "sy" + i.ToString ());
					Assert.AreEqual (sb.Width, zb.Width, "sw" + i.ToString ());
					Assert.AreEqual (sb.Height, zb.Height, "sh" + i.ToString ());
				}

				Region [] max = Measure (gfx, new RectangleF (0, 0, Single.MaxValue, Single.MaxValue));
				Assert.AreEqual (3, max.Length, "empty.Length");
				for (int i = 0; i < 3; i++) {
					RectangleF zb = zero [i].GetBounds (gfx);
					RectangleF mb = max [i].GetBounds (gfx);
					Assert.AreEqual (mb.X, zb.X, "mx" + i.ToString ());
					Assert.AreEqual (mb.Y, zb.Y, "my" + i.ToString ());
					Assert.AreEqual (mb.Width, zb.Width, "mw" + i.ToString ());
					Assert.AreEqual (mb.Height, zb.Height, "mh" + i.ToString ());
				}
			}
		}

		[Test]
		public void MeasureLimits ()
		{
			using (Graphics gfx = Graphics.FromImage (new Bitmap (1, 1))) {
				Region [] min = Measure (gfx, new RectangleF (0, 0, Single.MinValue, Single.MinValue));
				Assert.AreEqual (3, min.Length, "origin.Length");
				for (int i = 0; i < 3; i++) {
					RectangleF mb = min [i].GetBounds (gfx);
					Assert.AreEqual (-4194304.0f, mb.X, "minx" + i.ToString ());
					Assert.AreEqual (-4194304.0f, mb.Y, "miny" + i.ToString ());
					Assert.AreEqual (8388608.0f, mb.Width, "minw" + i.ToString ());
					Assert.AreEqual (8388608.0f, mb.Height, "minh" + i.ToString ());
				}

				Region [] neg = Measure (gfx, new RectangleF (0, 0, -20, -20));
				Assert.AreEqual (3, neg.Length, "neg.Length");
				for (int i = 0; i < 3; i++) {
					RectangleF mb = neg [i].GetBounds (gfx);
					Assert.AreEqual (-4194304.0f, mb.X, "minx" + i.ToString ());
					Assert.AreEqual (-4194304.0f, mb.Y, "miny" + i.ToString ());
					Assert.AreEqual (8388608.0f, mb.Width, "minw" + i.ToString ());
					Assert.AreEqual (8388608.0f, mb.Height, "minh" + i.ToString ());
				}
			}
		}

		[Test]
		public void DrawString_EndlessLoop_Bug77699 ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Rectangle rect = Rectangle.Empty;
					rect.Location = new Point (10, 10);
					rect.Size = new Size (1, 20);
					StringFormat fmt = new StringFormat ();
					fmt.Alignment = StringAlignment.Center;
					fmt.LineAlignment = StringAlignment.Center;
					fmt.FormatFlags = StringFormatFlags.NoWrap;
					fmt.Trimming = StringTrimming.EllipsisWord;
					g.DrawString ("Test String", font, Brushes.Black, rect, fmt);
				}
			}
		}

		[Test]
		public void DrawString_EndlessLoop_Wrapping ()
		{
			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					Rectangle rect = Rectangle.Empty;
					rect.Location = new Point (10, 10);
					rect.Size = new Size (1, 20);
					StringFormat fmt = new StringFormat ();
					fmt.Alignment = StringAlignment.Center;
					fmt.LineAlignment = StringAlignment.Center;
					fmt.Trimming = StringTrimming.EllipsisWord;
					g.DrawString ("Test String", font, Brushes.Black, rect, fmt);
				}
			}
		}

		[Test]
		public void MeasureString_Wrapping_Dots ()
		{
			HostIgnoreList.CheckTest ("MonoTests.System.Drawing.GraphicsTest.MeasureString_Wrapping_Dots");

			if (font == null)
				Assert.Ignore ("Couldn't create required font");

			string text = "this is really long text........................................... with a lot o periods.";
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					using (StringFormat format = new StringFormat ()) {
						format.Alignment = StringAlignment.Center;
						SizeF sz = g.MeasureString (text, font, 80, format);
						Assert.IsTrue (sz.Width <= 80, "Width");
						Assert.IsTrue (sz.Height >= font.Height * 2, "Height");
					}
				}
			}
		}

		[Test]
		public void GetReleaseHdcInternal ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					IntPtr hdc1 = g.GetHdc ();
					g.ReleaseHdcInternal (hdc1);
					IntPtr hdc2 = g.GetHdc ();
					g.ReleaseHdcInternal (hdc2);
					Assert.AreEqual (hdc1, hdc2, "hdc");
				}
			}
		}

		[Test]
		public void ReleaseHdcInternal_IntPtrZero ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					Assert.Throws<ArgumentException> (() => g.ReleaseHdcInternal (IntPtr.Zero));
				}
			}
		}

		[Test]
		public void ReleaseHdcInternal_TwoTimes ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					IntPtr hdc = g.GetHdc ();
					g.ReleaseHdcInternal (hdc);
					Assert.Throws<ArgumentException> (() => g.ReleaseHdcInternal (hdc));
				}
			}
		}
		[Test]
		public void TestReleaseHdc ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					IntPtr hdc1 = g.GetHdc ();
					g.ReleaseHdc ();
					IntPtr hdc2 = g.GetHdc ();
					g.ReleaseHdc ();
					Assert.AreEqual (hdc1, hdc2, "hdc");
				}
			}
		}

		[Test]
		public void TestReleaseHdcException ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					Assert.Throws<ArgumentException> (() => g.ReleaseHdc ());
				}
			}
		}

		[Test]
		public void TestReleaseHdcException2 ()
		{
			using (Bitmap b = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (b)) {
					g.GetHdc ();
					g.ReleaseHdc ();
					Assert.Throws<ArgumentException> (() => g.ReleaseHdc ());
				}
			}
		}
		[Test]
		public void VisibleClipBound ()
		{
			// see #78958
			using (Bitmap bmp = new Bitmap (100, 100)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF noclip = g.VisibleClipBounds;
					Assert.AreEqual (0, noclip.X, "noclip.X");
					Assert.AreEqual (0, noclip.Y, "noclip.Y");
					Assert.AreEqual (100, noclip.Width, "noclip.Width");
					Assert.AreEqual (100, noclip.Height, "noclip.Height");

					// note: libgdiplus regions are precise to multiple of multiple of 8
					g.Clip = new Region (new RectangleF (0, 0, 32, 32));
					RectangleF clip = g.VisibleClipBounds;
					Assert.AreEqual (0, clip.X, "clip.X");
					Assert.AreEqual (0, clip.Y, "clip.Y");
					Assert.AreEqual (32, clip.Width, "clip.Width");
					Assert.AreEqual (32, clip.Height, "clip.Height");

					g.RotateTransform (90);
					RectangleF rotclip = g.VisibleClipBounds;
					Assert.AreEqual (0, rotclip.X, "rotclip.X");
					Assert.AreEqual (-32, rotclip.Y, "rotclip.Y");
					Assert.AreEqual (32, rotclip.Width, "rotclip.Width");
					Assert.AreEqual (32, rotclip.Height, "rotclip.Height");
				}
			}
		}

		[Test]
		public void VisibleClipBound_BigClip ()
		{
			using (Bitmap bmp = new Bitmap (100, 100)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF noclip = g.VisibleClipBounds;
					Assert.AreEqual (0, noclip.X, "noclip.X");
					Assert.AreEqual (0, noclip.Y, "noclip.Y");
					Assert.AreEqual (100, noclip.Width, "noclip.Width");
					Assert.AreEqual (100, noclip.Height, "noclip.Height");

					// clip is larger than bitmap
					g.Clip = new Region (new RectangleF (0, 0, 200, 200));
					RectangleF clipbound = g.ClipBounds;
					Assert.AreEqual (0, clipbound.X, "clipbound.X");
					Assert.AreEqual (0, clipbound.Y, "clipbound.Y");
					Assert.AreEqual (200, clipbound.Width, "clipbound.Width");
					Assert.AreEqual (200, clipbound.Height, "clipbound.Height");

					RectangleF clip = g.VisibleClipBounds;
					Assert.AreEqual (0, clip.X, "clip.X");
					Assert.AreEqual (0, clip.Y, "clip.Y");
					Assert.AreEqual (100, clip.Width, "clip.Width");
					Assert.AreEqual (100, clip.Height, "clip.Height");

					g.RotateTransform (90);
					RectangleF rotclipbound = g.ClipBounds;
					Assert.AreEqual (0, rotclipbound.X, "rotclipbound.X");
					Assert.AreEqual (-200, rotclipbound.Y, "rotclipbound.Y");
					Assert.AreEqual (200, rotclipbound.Width, "rotclipbound.Width");
					Assert.AreEqual (200, rotclipbound.Height, "rotclipbound.Height");

					RectangleF rotclip = g.VisibleClipBounds;
					Assert.AreEqual (0, rotclip.X, "rotclip.X");
					Assert.AreEqual (-100, rotclip.Y, "rotclip.Y");
					Assert.AreEqual (100, rotclip.Width, "rotclip.Width");
					Assert.AreEqual (100, rotclip.Height, "rotclip.Height");
				}
			}
		}

		[Test]
		public void Rotate ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.RotateTransform (90);
					RectangleF rvcb = g.VisibleClipBounds;
					Assert.AreEqual (0, rvcb.X, "rvcb.X");
					Assert.AreEqual (-100, rvcb.Y, "rvcb.Y");
					Assert.AreEqual (50.0f, rvcb.Width, 0.0001, "rvcb.Width");
					Assert.AreEqual (100, rvcb.Height, "rvcb.Height");
				}
			}
		}

		[Test]
		public void Scale ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.ScaleTransform (2, 0.5f);
					RectangleF svcb = g.VisibleClipBounds;
					Assert.AreEqual (0, svcb.X, "svcb.X");
					Assert.AreEqual (0, svcb.Y, "svcb.Y");
					Assert.AreEqual (50, svcb.Width, "svcb.Width");
					Assert.AreEqual (100, svcb.Height, "svcb.Height");
				}
			}
		}

		[Test]
		public void Translate ()
		{
			using (Bitmap bmp = new Bitmap (100, 50)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					RectangleF vcb = g.VisibleClipBounds;
					Assert.AreEqual (0, vcb.X, "vcb.X");
					Assert.AreEqual (0, vcb.Y, "vcb.Y");
					Assert.AreEqual (100, vcb.Width, "vcb.Width");
					Assert.AreEqual (50, vcb.Height, "vcb.Height");

					g.TranslateTransform (-25, 25);
					RectangleF tvcb = g.VisibleClipBounds;
					Assert.AreEqual (25, tvcb.X, "tvcb.X");
					Assert.AreEqual (-25, tvcb.Y, "tvcb.Y");
					Assert.AreEqual (100, tvcb.Width, "tvcb.Width");
					Assert.AreEqual (50, tvcb.Height, "tvcb.Height");
				}
			}
		}

		[Test]
		public void DrawIcon_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawIcon (null, new Rectangle (0, 0, 32, 32)));
				}
			}
		}

		[Test]
		public void DrawIcon_IconRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (SystemIcons.Application, new Rectangle (0, 0, 40, 20));
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawIcon (SystemIcons.Asterisk, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawIcon (SystemIcons.Error, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (10, 20, -1, 0));
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		public void DrawIcon_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawIcon (null, 4, 2));
				}
			}
		}

		[Test]
		public void DrawIcon_IconIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIcon (SystemIcons.Exclamation, 4, 2);
					g.DrawIcon (SystemIcons.Hand, 0, 0);
				}
			}
		}

		[Test]
		public void DrawIconUnstretched_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawIconUnstretched (null, new Rectangle (0, 0, 40, 20)));
				}
			}
		}

		[Test]
		public void DrawIconUnstretched_IconRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawIconUnstretched (SystemIcons.Information, new Rectangle (0, 0, 40, 20));
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawIconUnstretched (SystemIcons.Question, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawIconUnstretched (SystemIcons.Warning, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (10, 20, -1, 0));
					g.DrawIconUnstretched (SystemIcons.WinLogo, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		public void DrawImage_NullRectangleF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new RectangleF (0, 0, 0, 0)));
				}
			}
		}

		[Test]
		public void DrawImage_ImageRectangleF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new RectangleF (0, 0, 0, 0));
					g.DrawImage (bmp, new RectangleF (20, 40, 0, 0));
					g.DrawImage (bmp, new RectangleF (10, 20, -1, 0));
					g.DrawImage (bmp, new RectangleF (20, 10, 0, -1));
				}
			}
		}

		[Test]
		public void DrawImage_NullPointF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new PointF (0, 0)));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointF ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new PointF (0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_NullPointFArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new PointF[0]));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointFArrayNull ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (bmp, (PointF[]) null));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointFArrayEmpty ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentException> (() => g.DrawImage (bmp, new PointF[0]));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointFArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new PointF[] { 
						new PointF (0, 0), new PointF (1, 1), new PointF (2, 2) });
				}
			}
		}

		[Test]
		public void DrawImage_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new Rectangle (0, 0, 0, 0)));
				}
			}
		}

		[Test]
		public void DrawImage_ImageRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawImage (bmp, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawImage (bmp, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawImage (bmp, new Rectangle (10, 20, -1, 0));
					g.DrawImage (bmp, new Rectangle (20, 10, 0, -1));
				}
			}
		}

		[Test]
		public void DrawImage_NullPoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new Point (0, 0)));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new Point (0, 0));
				}
			}
		}

		[Test]
		public void DrawImage_NullPointArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new Point[0]));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointArrayNull ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (bmp, (Point[]) null));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointArrayEmpty ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentException> (() => g.DrawImage (bmp, new Point[0]));
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointArray ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, new Point[] { 
						new Point (0, 0), new Point (1, 1), new Point (2, 2) });
				}
			}
		}

		[Test]
		public void DrawImage_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, Int32.MaxValue, Int32.MinValue));
				}
			}
		}

		[Test]
		public void DrawImage_ImageIntInt_Overflow ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<OverflowException> (() => g.DrawImage (bmp, Int32.MaxValue, Int32.MinValue));
				}
			}
		}

		[Test]
		public void DrawImage_ImageIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, -40, -40);
				}
			}
		}

		[Test]
		public void DrawImage_NullFloat ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, Single.MaxValue, Single.MinValue));
				}
			}
		}

		[Test]
		public void DrawImage_ImageFloatFloat_Overflow ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<OverflowException> (() => g.DrawImage (bmp, Single.MaxValue, Single.MinValue));
				}
			}
		}

		[Test]
		public void DrawImage_ImageFloatFloat ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, -40.0f, -40.0f);
				}
			}
		}

		[Test]
		public void DrawImage_NullRectangleRectangleGraphicsUnit ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, new Rectangle (), new Rectangle (), GraphicsUnit.Display));
				}
			}
		}

		private void DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit unit)
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Rectangle r = new Rectangle (0, 0, 40, 40);
					g.DrawImage (bmp, r, r, unit);
				}
			}
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Display ()
		{
			Assert.Throws<ArgumentException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Display));
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Document ()
		{
			Assert.Throws<NotImplementedException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Document));
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Inch ()
		{
			Assert.Throws<NotImplementedException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit(GraphicsUnit.Inch));
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Millimeter ()
		{
			Assert.Throws<NotImplementedException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Millimeter));
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Pixel ()
		{
			// this unit works
			DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Pixel);
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_Point ()
		{
			Assert.Throws<NotImplementedException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.Point));
		}

		[Test]
		public void DrawImage_ImageRectangleRectangleGraphicsUnit_World ()
		{
			Assert.Throws<ArgumentException> (() => DrawImage_ImageRectangleRectangleGraphicsUnit (GraphicsUnit.World));
		}

		[Test]
		public void DrawImage_NullPointRectangleGraphicsUnit ()
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			Point[] pts = new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) };
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, pts, r, GraphicsUnit.Pixel));
				}
			}
		}

		private void DrawImage_ImagePointRectangleGraphicsUnit (Point[] pts)
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		[Test]
		public void DrawImage_ImageNullRectangleGraphicsUnit ()
		{
			Assert.Throws<ArgumentNullException> (() => DrawImage_ImagePointRectangleGraphicsUnit (null));
		}

		[Test]
		public void DrawImage_ImagePoint0RectangleGraphicsUnit ()
		{
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointRectangleGraphicsUnit (new Point[0]));
		}

		[Test]
		public void DrawImage_ImagePoint1RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointRectangleGraphicsUnit (new Point[1] { p }));
		}

		[Test]
		public void DrawImage_ImagePoint2RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointRectangleGraphicsUnit (new Point[2] { p, p }));
		}

		[Test]
		public void DrawImage_ImagePoint3RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			DrawImage_ImagePointRectangleGraphicsUnit (new Point[3] { p, p, p });
		}

		[Test]
		public void DrawImage_ImagePoint4RectangleGraphicsUnit ()
		{
			Point p = new Point (1, 1);
			Assert.Throws<NotImplementedException> (() => DrawImage_ImagePointRectangleGraphicsUnit (new Point[4] { p, p, p, p }));
		}

		[Test]
		public void DrawImage_NullPointFRectangleGraphicsUnit ()
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			PointF[] pts = new PointF[3] { new PointF (1, 1), new PointF (2, 2), new PointF (3, 3) };
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImage (null, pts, r, GraphicsUnit.Pixel));
				}
			}
		}

		private void DrawImage_ImagePointFRectangleGraphicsUnit (PointF[] pts)
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel);
				}
			}
		}

		[Test]
		public void DrawImage_ImageNullFRectangleGraphicsUnit ()
		{
			Assert.Throws<ArgumentNullException> (() => DrawImage_ImagePointFRectangleGraphicsUnit (null));
		}

		[Test]
		public void DrawImage_ImagePointF0RectangleGraphicsUnit ()
		{
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[0]));
		}

		[Test]
		public void DrawImage_ImagePointF1RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[1] { p }));
		}

		[Test]
		public void DrawImage_ImagePointF2RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			Assert.Throws<ArgumentException> (() => DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[2] { p, p }));
		}

		[Test]
		public void DrawImage_ImagePointF3RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[3] { p, p, p });
		}

		[Test]
		public void DrawImage_ImagePointF4RectangleGraphicsUnit ()
		{
			PointF p = new PointF (1, 1);
			Assert.Throws<NotImplementedException> (() => DrawImage_ImagePointFRectangleGraphicsUnit (new PointF[4] { p, p, p, p }));
		}

		[Test]
		public void DrawImage_ImagePointRectangleGraphicsUnitNull ()
		{
			Point p = new Point (1, 1);
			Point[] pts = new Point[3] { p, p, p };
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel, null);
				}
			}
		}

		[Test]
		public void DrawImage_ImagePointRectangleGraphicsUnitAttributes ()
		{
			Point p = new Point (1, 1);
			Point[] pts = new Point[3] { p, p, p };
			Rectangle r = new Rectangle (1, 2, 3, 4);
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					ImageAttributes ia = new ImageAttributes ();
					g.DrawImage (bmp, pts, r, GraphicsUnit.Pixel, ia);
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_NullPoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImageUnscaled (null, new Point (0, 0)));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImagePoint ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, new Point (0, 0));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_NullRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImageUnscaled (null, new Rectangle (0, 0, -1, -1)));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageRectangle ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, new Rectangle (0, 0, -1, -1));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_NullIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImageUnscaled (null, 0, 0));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, 0, 0);
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_NullIntIntIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImageUnscaled (null, 0, 0, -1, -1));
				}
			}
		}

		[Test]
		public void DrawImageUnscaled_ImageIntIntIntInt ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					g.DrawImageUnscaled (bmp, 0, 0, -1, -1);
				}
			}
		}
		[Test]
		public void DrawImageUnscaledAndClipped_Null ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawImageUnscaledAndClipped (null, new Rectangle (0, 0, 0, 0)));
				}
			}
		}

		[Test]
		public void DrawImageUnscaledAndClipped ()
		{
			using (Bitmap bmp = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					// Rectangle is empty when X, Y, Width and Height == 0 
					// (yep X and Y too, RectangleF only checks for Width and Height)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 0, 0));
					// so this one is half-empty ;-)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (20, 40, 0, 0));
					// negative width or height isn't empty (for Rectangle)
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (10, 20, -1, 0));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (20, 10, 0, -1));
					// smaller
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 10, 20));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 40, 10));
					g.DrawImageUnscaledAndClipped (bmp, new Rectangle (0, 0, 80, 20));
				}
			}
		}

		[Test]
		public void DrawPath_Pen_Null ()
		{
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					using (GraphicsPath path = new GraphicsPath ()) {
						Assert.Throws<ArgumentNullException> (() => g.DrawPath (null, path));
					}
				}
			}
		}

		[Test]
		public void DrawPath_Path_Null ()
		{
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.DrawPath (Pens.Black, null));
				}
			}
		}

		[Test]
		public void DrawPath_82202 ()
		{
			// based on test case from bug #82202
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					using (GraphicsPath path = new GraphicsPath ()) {
						int d = 5;
						Rectangle baserect = new Rectangle (0, 0, 19, 19);
						Rectangle arcrect = new Rectangle (baserect.Location, new Size (d, d));

						path.AddArc (arcrect, 180, 90);
						arcrect.X = baserect.Right - d;
						path.AddArc (arcrect, 270, 90);
						arcrect.Y = baserect.Bottom - d;
						path.AddArc (arcrect, 0, 90);
						arcrect.X = baserect.Left;
						path.AddArc (arcrect, 90, 90);
						path.CloseFigure ();
						g.Clear (Color.White);
						g.DrawPath (Pens.SteelBlue, path);

						Assert.AreEqual (-12156236, bmp.GetPixel (0, 9).ToArgb (), "0,9");
						Assert.AreEqual (-1, bmp.GetPixel (1, 9).ToArgb (), "1,9");
					}
				}
			}
		}

		[Test]
		public void FillPath_Brush_Null ()
		{
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					using (GraphicsPath path = new GraphicsPath ()) {
						Assert.Throws<ArgumentNullException> (() => g.FillPath (null, path));
					}
				}
			}
		}

		[Test]
		public void FillPath_Path_Null ()
		{
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.Throws<ArgumentNullException> (() => g.FillPath (Brushes.Black, null));
				}
			}
		}

		[Test]
		public void FillPath_82202 ()
		{
			// based on test case from bug #82202
			using (Bitmap bmp = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					using (GraphicsPath path = new GraphicsPath ()) {
						int d = 5;
						Rectangle baserect = new Rectangle (0, 0, 19, 19);
						Rectangle arcrect = new Rectangle (baserect.Location, new Size (d, d));

						path.AddArc (arcrect, 180, 90);
						arcrect.X = baserect.Right - d;
						path.AddArc (arcrect, 270, 90);
						arcrect.Y = baserect.Bottom - d;
						path.AddArc (arcrect, 0, 90);
						arcrect.X = baserect.Left;
						path.AddArc (arcrect, 90, 90);
						path.CloseFigure ();
						g.Clear (Color.White);
						g.FillPath (Brushes.SteelBlue, path);

						Assert.AreEqual (-12156236, bmp.GetPixel (0, 9).ToArgb (), "0,9");
						Assert.AreEqual (-12156236, bmp.GetPixel (1, 9).ToArgb (), "1,9");
					}
				}
			}
		}

		[Test]
		public void TransformPoints_349800 ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					Point [] pts = new Point [5];
					PointF [] ptf = new PointF [5];
					for (int i = 0; i < 5; i++) {
						pts [i] = new Point (i, i);
						ptf [i] = new PointF (i, i);
					}

					g.TransformPoints (CoordinateSpace.Page, CoordinateSpace.Device, pts);
					g.TransformPoints (CoordinateSpace.Page, CoordinateSpace.Device, ptf);

					for (int i = 0; i < 5; i++) {
						Assert.AreEqual (i, pts [i].X, "Point.X " + i.ToString ());
						Assert.AreEqual (i, pts [i].Y, "Point.Y " + i.ToString ());
						Assert.AreEqual (i, ptf [i].X, "PointF.X " + i.ToString ());
						Assert.AreEqual (i, ptf [i].Y, "PointF.Y " + i.ToString ());
					}
				}
			}
		}

		[Test]
		public void Dpi_556181 ()
		{
			float x, y;
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					x = g.DpiX - 10;
					y = g.DpiY + 10;
				}
				bmp.SetResolution (x, y);
				using (Graphics g = Graphics.FromImage (bmp)) {
					Assert.AreEqual (x, g.DpiX, "DpiX");
					Assert.AreEqual (y, g.DpiY, "DpiY");
				}
			}
		}
	}

	[TestFixture]
	public class GraphicsFullTrustTest {

		// note: this test would fail, on ReleaseHdc, without fulltrust
		// i.e. it's a demand and not a linkdemand
		[Test]
		public void GetReleaseHdc ()
		{
			using (Bitmap b = new Bitmap (100, 100)) {
				using (Graphics g = Graphics.FromImage (b)) {
					IntPtr hdc1 = g.GetHdc ();
					g.ReleaseHdc (hdc1);
					IntPtr hdc2 = g.GetHdc ();
					g.ReleaseHdc (hdc2);
					Assert.AreEqual (hdc1, hdc2, "hdc");
				}
			}
		}

	}
}
