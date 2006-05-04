//
// Graphics class testing unit
//
// Authors:
//   Jordi Mas, jordi@ximian.com
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class GraphicsTest : Assertion
	{
		private RectangleF[] rects;

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
				Fail (String.Format ("Position {0},{1}", x, y));
		}

		private void CheckForNonEmptyBitmap (Bitmap bitmap)
		{
			int x, y;
			if (IsEmptyBitmap (bitmap, out x, out y))
				Fail ("Bitmap was empty");
		}

		[Test]
		public void DefaultProperties ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Region r = new Region ();

			AssertEquals ("DefaultProperties1", r.GetBounds (g) , g.ClipBounds);
			AssertEquals ("DefaultProperties2", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals ("DefaultProperties3", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals ("DefaultProperties4", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals ("DefaultProperties5", 1, g.PageScale);
			AssertEquals ("DefaultProperties6", GraphicsUnit.Display, g.PageUnit);
			AssertEquals ("DefaultProperties7", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals ("DefaultProperties8", new Point (0, 0) , g.RenderingOrigin);
			AssertEquals ("DefaultProperties9", SmoothingMode.None, g.SmoothingMode);
			AssertEquals ("DefaultProperties10", TextRenderingHint.SystemDefault, g.TextRenderingHint);

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
			AssertEquals ("SetGetProperties2", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals ("SetGetProperties3", CompositingQuality.GammaCorrected, g.CompositingQuality);
			AssertEquals ("SetGetProperties4", InterpolationMode.HighQualityBilinear, g.InterpolationMode);
			AssertEquals ("SetGetProperties5", 2, g.PageScale);
			AssertEquals ("SetGetProperties6", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals ("SetGetProperties7", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals ("SetGetProperties8", new Point (10, 20), g.RenderingOrigin);
			AssertEquals ("SetGetProperties9", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals ("SetGetProperties10", TextRenderingHint.SystemDefault, g.TextRenderingHint);			
		}

		// Properties
		[Test]
		public void Clip ()
		{
			RectangleF[] rects ;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			g.Clip = new Region (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("Clip1", 1, rects.Length);
			AssertEquals ("Clip2", 50, rects[0].X);
			AssertEquals ("Clip3", 40, rects[0].Y);
			AssertEquals ("Clip4", 210, rects[0].Width);
			AssertEquals ("Clip5", 220, rects[0].Height);
		}

		[Test]
		public void Clip_NotAReference ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);
			Assert ("IsInfinite", g.Clip.IsInfinite (g));
			g.Clip.IsEmpty (g);
			Assert ("!IsEmpty", !g.Clip.IsEmpty (g));
			Assert ("IsInfinite-2", g.Clip.IsInfinite (g));
		}

		[Test]
		public void ExcludeClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (10, 10, 100, 100));
			g.ExcludeClip (new Rectangle (40, 60, 100, 20));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("ExcludeClip1", 3, rects.Length);

			AssertEquals ("ExcludeClip2", 10, rects[0].X);
			AssertEquals ("ExcludeClip3", 10, rects[0].Y);
			AssertEquals ("ExcludeClip4", 100, rects[0].Width);
			AssertEquals ("ExcludeClip5", 50, rects[0].Height);

			AssertEquals ("ExcludeClip6", 10, rects[1].X);
			AssertEquals ("ExcludeClip7", 60, rects[1].Y);
			AssertEquals ("ExcludeClip8", 30, rects[1].Width);
			AssertEquals ("ExcludeClip9", 20, rects[1].Height);

			AssertEquals ("ExcludeClip10", 10, rects[2].X);
			AssertEquals ("ExcludeClip11", 80, rects[2].Y);
			AssertEquals ("ExcludeClip12", 100, rects[2].Width);
			AssertEquals ("ExcludeClip13", 30, rects[2].Height);
		}

		[Test]
		public void IntersectClip ()
		{
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			g.Clip = new Region (new RectangleF (260, 30, 60, 80));
			g.IntersectClip (new Rectangle (290, 40, 60, 80));
			rects = g.Clip.GetRegionScans (new Matrix ());

			AssertEquals ("IntersectClip", 1, rects.Length);

			AssertEquals ("IntersectClip", 290, rects[0].X);
			AssertEquals ("IntersectClip", 40, rects[0].Y);
			AssertEquals ("IntersectClip", 30, rects[0].Width);
			AssertEquals ("IntersectClip", 70, rects[0].Height);
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

			AssertEquals ("ResetClip", 1, rects.Length);

			AssertEquals ("ResetClip", -4194304, rects[0].X);
			AssertEquals ("ResetClip", -4194304, rects[0].Y);
			AssertEquals ("ResetClip", 8388608, rects[0].Width);
			AssertEquals ("ResetClip", 8388608, rects[0].Height);
		}

		[Test]
		public void SetClip ()
		{
			RectangleF[] rects ;
			Bitmap bmp = new Bitmap (200, 200);
			Graphics g = Graphics.FromImage (bmp);

			// Region
			g.SetClip (new Region (new Rectangle (50, 40, 210, 220)), CombineMode.Replace);
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip1", 1, rects.Length);
			AssertEquals ("SetClip2", 50, rects[0].X);
			AssertEquals ("SetClip3", 40, rects[0].Y);
			AssertEquals ("SetClip4", 210, rects[0].Width);
			AssertEquals ("SetClip5", 220, rects[0].Height);

			// RectangleF
			g = Graphics.FromImage (bmp);
			g.SetClip (new RectangleF (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip6", 1, rects.Length);
			AssertEquals ("SetClip7", 50, rects[0].X);
			AssertEquals ("SetClip8", 40, rects[0].Y);
			AssertEquals ("SetClip9", 210, rects[0].Width);
			AssertEquals ("SetClip10", 220, rects[0].Height);

			// Rectangle
			g = Graphics.FromImage (bmp);
			g.SetClip (new Rectangle (50, 40, 210, 220));
			rects = g.Clip.GetRegionScans (new Matrix ());
			AssertEquals ("SetClip10", 1, rects.Length);
			AssertEquals ("SetClip11", 50, rects[0].X);
			AssertEquals ("SetClip12", 40, rects[0].Y);
			AssertEquals ("SetClip13", 210, rects[0].Width);
			AssertEquals ("SetClip14", 220, rects[0].Height);
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
			
			AssertEquals ("SetSaveReset1", CompositingMode.SourceCopy, g.CompositingMode);
			AssertEquals ("SetSaveReset2", CompositingQuality.GammaCorrected, g.CompositingQuality);
			AssertEquals ("SetSaveReset3", InterpolationMode.HighQualityBilinear, g.InterpolationMode);
			AssertEquals ("SetSaveReset4", 2, g.PageScale);
			AssertEquals ("SetSaveReset5", GraphicsUnit.Inch, g.PageUnit);
			AssertEquals ("SetSaveReset6", PixelOffsetMode.Half, g.PixelOffsetMode);
			AssertEquals ("SetSaveReset7", new Point (10, 20), g.RenderingOrigin);
			AssertEquals ("SetSaveReset8", SmoothingMode.AntiAlias, g.SmoothingMode);
			AssertEquals ("SetSaveReset9", TextRenderingHint.ClearTypeGridFit, g.TextRenderingHint);			
			AssertEquals ("SetSaveReset10", 0, (int) g.ClipBounds.X);
			AssertEquals ("SetSaveReset10", 0, (int) g.ClipBounds.Y);
			
			g.Restore (state_default);			
			
			AssertEquals ("SetSaveReset11", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals ("SetSaveReset12", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals ("SetSaveReset13", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals ("SetSaveReset14", 1, g.PageScale);
			AssertEquals ("SetSaveReset15", GraphicsUnit.Display, g.PageUnit);
			AssertEquals ("SetSaveReset16", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals ("SetSaveReset17", new Point (0, 0) , g.RenderingOrigin);
			AssertEquals ("SetSaveReset18", SmoothingMode.None, g.SmoothingMode);
			AssertEquals ("SetSaveReset19", TextRenderingHint.SystemDefault, g.TextRenderingHint);		

			Region r = new Region ();
			AssertEquals ("SetSaveReset20", r.GetBounds (g) , g.ClipBounds);
			
			g.Dispose ();			
		}

		[Test]
		public void LoadIndexed ()
		{
			//
			// Tests that we can load an indexed file
			//

			Stream str = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("indexed.png");
			Image x = Image.FromStream (str);
			Graphics g = Graphics.FromImage (x);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromImage ()
		{
			Graphics g = Graphics.FromImage (null);
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
			AssertEquals ("X", 0, bounds.X);
			AssertEquals ("Y", 0, bounds.Y);
			AssertEquals ("Width", 16, bounds.Width);
			AssertEquals ("Height", 16, bounds.Height);
			Assert ("Identity", g.Transform.IsIdentity);
			g.Dispose ();
		}

		[Test]
		public void Clip_TranslateTransform ()
		{
			Graphics g = Get (16, 16);
			g.TranslateTransform (12.22f, 10.10f);
			RectangleF bounds = g.Clip.GetBounds (g);
			Compare ("translate", bounds, g.ClipBounds);
			AssertEquals ("translate.X", -12.22, bounds.X);
			AssertEquals ("translate.Y", -10.10, bounds.Y);
			AssertEquals ("translate.Width", 16, bounds.Width);
			AssertEquals ("translate.Height", 16, bounds.Height);
			float[] elements = g.Transform.Elements;
			AssertEquals ("translate.0", 1, elements[0]);
			AssertEquals ("translate.1", 0, elements[1]);
			AssertEquals ("translate.2", 0, elements[2]);
			AssertEquals ("translate.3", 1, elements[3]);
			AssertEquals ("translate.4", 12.22, elements[4]);
			AssertEquals ("translate.5", 10.10, elements[5]);

			g.ResetTransform ();
			bounds = g.Clip.GetBounds (g);
			Compare ("reset", bounds, g.ClipBounds);
			AssertEquals ("reset.X", 0, bounds.X);
			AssertEquals ("reset.Y", 0, bounds.Y);
			AssertEquals ("reset.Width", 16, bounds.Width);
			AssertEquals ("reset.Height", 16, bounds.Height);
			Assert ("Identity", g.Transform.IsIdentity);
			g.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transform_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert ("IsInvertible", !matrix.IsInvertible);
			Graphics g = Get (16, 16);
			g.Transform = matrix;
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Multiply_NonInvertibleMatrix ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert ("IsInvertible", !matrix.IsInvertible);
			Graphics g = Get (16, 16);
			g.MultiplyTransform (matrix);
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
			Assert ("IsInfinite", g.Clip.IsInfinite (g));
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
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_X0 ()
		{
			Graphics g = Get (16, 16);
			g.ScaleTransform (0, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_Y0 ()
		{
			Graphics g = Get (16, 16);
			g.ScaleTransform (1, 0);
		}

		[Test]
		public void TranslateTransform_Order ()
		{
			Graphics g = Get (16, 16);
			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3);
			float[] elements = g.Transform.Elements;
			AssertEquals ("default.0", 1, elements[0]);
			AssertEquals ("default.1", 2, elements[1]);
			AssertEquals ("default.2", 3, elements[2]);
			AssertEquals ("default.3", 4, elements[3]);
			AssertEquals ("default.4", -1, elements[4]);
			AssertEquals ("default.5", 0, elements[5]);

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Prepend);
			elements = g.Transform.Elements;
			AssertEquals ("prepend.0", 1, elements[0]);
			AssertEquals ("prepend.1", 2, elements[1]);
			AssertEquals ("prepend.2", 3, elements[2]);
			AssertEquals ("prepend.3", 4, elements[3]);
			AssertEquals ("prepend.4", -1, elements[4]);
			AssertEquals ("prepend.5", 0, elements[5]);

			g.Transform = new Matrix (1, 2, 3, 4, 5, 6);
			g.TranslateTransform (3, -3, MatrixOrder.Append);
			elements = g.Transform.Elements;
			AssertEquals ("append.0", 1, elements[0]);
			AssertEquals ("append.1", 2, elements[1]);
			AssertEquals ("append.2", 3, elements[2]);
			AssertEquals ("append.3", 4, elements[3]);
			AssertEquals ("append.4", 8, elements[4]);
			AssertEquals ("append.5", 3, elements[5]);
		}

		static Point[] SmallCurve = new Point[3] { new Point (0, 0), new Point (15, 5), new Point (5, 15) };
		static PointF[] SmallCurveF = new PointF[3] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15) };

		static Point[] TooSmallCurve = new Point[2] { new Point (0, 0), new Point (15, 5) };
		static PointF[] LargeCurveF = new PointF[4] { new PointF (0, 0), new PointF (15, 5), new PointF (5, 15), new PointF (0, 20) };

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PenNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (null, SmallCurveF);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PointFNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, (PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DrawCurve_PointNull ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, (Point[]) null);
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
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_SinglePoint ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, new Point[1] { new Point (10, 10) }, 0.5f);
			// a single point isn't enough
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve3_NotEnoughPoints ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, TooSmallCurve, 0, 2, 0.5f);
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
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_ZeroSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_NegativeSegments ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.DrawCurve (Pens.Black, SmallCurveF, 0, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawCurve_OffsetTooLarge ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			// starting offset 1 doesn't give 3 points to make a curve
			g.DrawCurve (Pens.Black, SmallCurveF, 1, 2);
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

		private void CheckDefaultProperties (string message, Graphics g)
		{
			Assert (message + ".Clip.IsInfinite", g.Clip.IsInfinite (g));
			AssertEquals (message + ".CompositingMode", CompositingMode.SourceOver, g.CompositingMode);
			AssertEquals (message + ".CompositingQuality", CompositingQuality.Default, g.CompositingQuality);
			AssertEquals (message + ".InterpolationMode", InterpolationMode.Bilinear, g.InterpolationMode);
			AssertEquals (message + ".PageScale", 1.0f, g.PageScale);
			AssertEquals (message + ".PageUnit", GraphicsUnit.Display, g.PageUnit);
			AssertEquals (message + ".PixelOffsetMode", PixelOffsetMode.Default, g.PixelOffsetMode);
			AssertEquals (message + ".SmoothingMode", SmoothingMode.None, g.SmoothingMode);
			AssertEquals (message + ".TextContrast", 4, g.TextContrast);
			AssertEquals (message + ".TextRenderingHint", TextRenderingHint.SystemDefault, g.TextRenderingHint);
			Assert (message + ".Transform.IsIdentity", g.Transform.IsIdentity);
		}

		private void CheckCustomProperties (string message, Graphics g)
		{
			Assert (message + ".Clip.IsInfinite", !g.Clip.IsInfinite (g));
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
			Assert (message + ".Transform.IsIdentity", !g.Transform.IsIdentity);
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
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

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
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

			g.EndContainer (gc);
			CheckCustomProperties ("EndContainer", g);
		}

		[Test]
		public void BeginContainer_Rect ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

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
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

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
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

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
			AssertEquals ("BeginContainer.RenderingOrigin", new Point (-1, -1), g.RenderingOrigin);

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
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_Display ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.Display);
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
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_World ()
		{
			BeginContainer_GraphicsUnit (GraphicsUnit.World);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void BeginContainer_GraphicsUnit_Bad ()
		{
			BeginContainer_GraphicsUnit ((GraphicsUnit)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EndContainer_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.EndContainer (null);
		}

		[Test]
		public void Save ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);

			CheckDefaultProperties ("default", g);
			AssertEquals ("default.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

			GraphicsState gs1 = g.Save ();
			// nothing is changed after a save
			CheckDefaultProperties ("save1", g);
			AssertEquals ("save1.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);

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
			AssertEquals ("restored2.RenderingOrigin", new Point (0, 0), g.RenderingOrigin);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Restore_Null ()
		{
			Bitmap bitmap = new Bitmap (20, 20);
			Graphics g = Graphics.FromImage (bitmap);
			g.Restore (null);
		}
	}
}
