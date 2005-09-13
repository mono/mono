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
using System.Drawing.Drawing2D;

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


	}
}

