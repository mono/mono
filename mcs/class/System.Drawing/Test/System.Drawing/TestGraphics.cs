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
	}
}
