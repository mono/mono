//
// System.Drawing.Region non-rectangular unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	/* NOTE: General tests and rectangular region tests are located in TestRegion.cs */
	/*       Here we exclusively tests non-rectangular (GraphicsPath based) regions. */

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class RegionNonRectTest {

		private Bitmap bitmap;
		private Graphics graphic;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (10, 10);
			graphic = Graphics.FromImage (bitmap);
		}

		// a region with an "empty ctor" graphic path is "empty" (i.e. not infinite)
		private void CheckEmpty (string prefix, Region region)
		{
			Assert.IsTrue (region.IsEmpty (graphic), prefix + "IsEmpty");
			Assert.IsFalse (region.IsInfinite (graphic), prefix + "graphic");

			RectangleF rect = region.GetBounds (graphic);
			Assert.AreEqual (0f, rect.X, prefix + "GetBounds.X");
			Assert.AreEqual (0f, rect.Y, prefix + "GetBounds.Y");
			Assert.AreEqual (0f, rect.Width, prefix + "GetBounds.Width");
			Assert.AreEqual (0f, rect.Height, prefix + "GetBounds.Height");
		}

		[Test]
		public void Region_Ctor_GraphicsPath_Empty ()
		{
			Region region = new Region (new GraphicsPath ());
			CheckEmpty ("GraphicsPath.", region);

			Region clone = region.Clone ();
			CheckEmpty ("Clone.", region);
		}

		[Test]
		[Category ("NotDotNet")] // MS.NET (atleast 2.0) throws an ExternalException in this case
		public void Region_Ctor_RegionData ()
		{
			Region region = new Region (new GraphicsPath ());
			RegionData data = region.GetRegionData ();
			Region r2 = new Region (data);
			CheckEmpty ("RegionData.", region);
		}

		[Test]
		public void Region_Ctor_GraphicsPath ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Region region = new Region (gp);
			CheckEmpty ("GraphicsPath.", region);

			Region clone = region.Clone ();
			CheckEmpty ("Clone.", region);
		}

		private void CheckInfiniteBounds (GraphicsPath path)
		{
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (-4194304f, rect.X, "Bounds.X");
			Assert.AreEqual (-4194304f, rect.Y, "Bounds.Y");
			Assert.AreEqual (8388608f, rect.Width, "Bounds.Width");
			Assert.AreEqual (8388608f, rect.Height, "Bounds.Height");
		}

		[Test]
		public void Region_Curve_IsInfinite ()
		{
			Point[] points = new Point[2] { new Point (-4194304, -4194304), new Point (4194304, 4194304) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (points);
			CheckInfiniteBounds (gp);

			Region region = new Region (gp);
			Assert.IsFalse (region.IsInfinite (graphic), "IsInfinite");
			// note: infinity isn't based on the bounds
		}

		[Test]
		[Category ("NotWorking")]
		public void Region_Polygon4_IsInfinite ()
		{
			Point[] points = new Point[4] { new Point (-4194304, -4194304), new Point (-4194304, 4194304), new Point (4194304, 4194304), new Point (4194304, -4194304) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (points);
			CheckInfiniteBounds (gp);

			Region region = new Region (gp);
			Assert.IsTrue (region.IsInfinite (graphic), "IsInfinite");
		}

		[Test]
		[Category ("NotWorking")]
		public void Region_Polygon5_IsInfinite ()
		{
			// overlap the first/last point
			Point[] points = new Point[5] { new Point (-4194304, -4194304), new Point (-4194304, 4194304), new Point (4194304, 4194304), new Point (4194304, -4194304), new Point (-4194304, -4194304) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (points);
			CheckInfiniteBounds (gp);

			Region region = new Region (gp);
			Assert.IsTrue (region.IsInfinite (graphic), "IsInfinite");
		}

		[Test]
		[Category ("NotWorking")]
		public void Region_Rectangle_IsInfinite ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (-4194304, -4194304, 8388608, 8388608));
			CheckInfiniteBounds (gp);

			Region region = new Region (gp);
			Assert.IsTrue (region.IsInfinite (graphic), "IsInfinite");
		}
	}
}
