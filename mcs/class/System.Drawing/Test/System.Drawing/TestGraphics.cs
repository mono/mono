//
// Graphics class testing unit
//
// Author:
//   Jordi Mas, jordi@ximian.com
//
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

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()
		{

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
	}
}

