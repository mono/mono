//
// System.Drawing.GraphicsPath unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using SC = System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Drawing2D {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class GraphicsPathTest {

		private const float Pi4 = (float) (Math.PI / 4);
		// let's tolerate a few differences
		private const float Delta = 0.0003f;

		private void CheckEmpty (string prefix, GraphicsPath gp)
		{
			Assert.AreEqual (0, gp.PathData.Points.Length, "PathData.Points");
			Assert.AreEqual (0, gp.PathData.Types.Length, "PathData.Types");
			Assert.AreEqual (0, gp.PointCount, prefix + "PointCount");
		}

		[Test]
		public void Constructor_InvalidFillMode ()
		{
			GraphicsPath gp = new GraphicsPath ((FillMode) Int32.MinValue);
			Assert.AreEqual (Int32.MinValue, (int) gp.FillMode, "FillMode");
			CheckEmpty ("InvalidFillMode.", gp);
		}

		[Test]
		public void Constructor_Point_Null_Byte ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ((Point[]) null, new byte[1]));
		}

		[Test]
		public void Constructor_Point_Byte_Null ()
		{
			Assert.Throws<NullReferenceException> (() => new GraphicsPath (new Point[1], null));
		}

		[Test]
		public void Constructor_Point_Byte_LengthMismatch ()
		{
			Assert.Throws<ArgumentException> (() => new GraphicsPath (new Point[1], new byte [2]));
		}

		[Test]
		public void Constructor_PointF_Null_Byte ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ((PointF[])null, new byte [1]));
		}

		[Test]
		public void Constructor_PointF_Byte_Null ()
		{
			Assert.Throws<NullReferenceException> (() => new GraphicsPath ( new PointF[1], null));
		}

		[Test]
		public void Constructor_PointF_Byte_LengthMismatch ()
		{
			Assert.Throws<ArgumentException> (() => new GraphicsPath (new PointF[2], new byte [1]));
		}

		[Test]
		public void GraphicsPath_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.AreEqual (FillMode.Alternate, gp.FillMode, "Empty.FillMode");
			CheckEmpty ("Empty.", gp);

			GraphicsPath clone = (GraphicsPath) gp.Clone ();
			Assert.AreEqual (FillMode.Alternate, gp.FillMode, "Clone.FillMode");
			CheckEmpty ("Clone.", gp);

			gp.Reverse ();
			CheckEmpty ("Reverse.", gp);
		}

		[Test]
		public void GraphicsPath_Empty_PathPoints ()
		{
			Assert.Throws<ArgumentException> (() => Assert.IsNull (new GraphicsPath ().PathPoints));
		}

		[Test]
		public void GraphicsPath_Empty_PathTypes ()
		{
			Assert.Throws<ArgumentException> (() => Assert.IsNull (new GraphicsPath ().PathTypes));
		}

		[Test]
		public void GraphicsPath_SamePoint ()
		{
			Point[] points = new Point [] {
				new Point (1, 1),
				new Point (1, 1),
				new Point (1, 1),
				new Point (1, 1),
				new Point (1, 1),
				new Point (1, 1),
			};
			byte [] types = new byte [6] { 0, 1, 1, 1, 1, 1 };
			using (GraphicsPath gp = new GraphicsPath (points, types)) {
				Assert.AreEqual (6, gp.PointCount, "0-PointCount");
			}
			types [0] = 1;
			using (GraphicsPath gp = new GraphicsPath (points, types)) {
				Assert.AreEqual (6, gp.PointCount, "1-PointCount");
			}
		}

		[Test]
		public void GraphicsPath_SamePointF ()
		{
			PointF [] points = new PointF [] {
				new PointF (1f, 1f),
				new PointF (1f, 1f),
				new PointF (1f, 1f),
				new PointF (1f, 1f),
				new PointF (1f, 1f),
				new PointF (1f, 1f),
			};
			byte [] types = new byte [6] { 0, 1, 1, 1, 1, 1 };
			using (GraphicsPath gp = new GraphicsPath (points, types)) {
				Assert.AreEqual (6, gp.PointCount, "0-PointCount");
			}
			types [0] = 1;
			using (GraphicsPath gp = new GraphicsPath (points, types)) {
				Assert.AreEqual (6, gp.PointCount, "1-PointCount");
			}
		}

		[Test]
		public void FillMode_Invalid ()
		{
			// constructor accept an invalid FillMode
			GraphicsPath gp = new GraphicsPath ((FillMode) Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, (int) gp.FillMode, "MaxValue");
			// but you can't set the FillMode property to an invalid value ;-)
			Assert.Throws<SC.InvalidEnumArgumentException> (() => gp.FillMode = (FillMode) Int32.MaxValue);
		}

		[Test]
		public void PathData_CannotChange ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));

			Assert.AreEqual (1f, gp.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, gp.PathData.Points[0].Y, "Points[0].Y");

			// now try to change the first point
			gp.PathData.Points[0] = new Point (0, 0);
			// the changes isn't reflected in the property
			Assert.AreEqual (1f, gp.PathData.Points[0].X, "Points[0].X-1");
			Assert.AreEqual (1f, gp.PathData.Points[0].Y, "Points[0].Y-1");
		}

		[Test]
		public void PathPoints_CannotChange ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));

			Assert.AreEqual (1f, gp.PathPoints[0].X, "PathPoints[0].X");
			Assert.AreEqual (1f, gp.PathPoints[0].Y, "PathPoints[0].Y");

			// now try to change the first point
			gp.PathPoints[0] = new Point (0, 0);
			// the changes isn't reflected in the property
			Assert.AreEqual (1f, gp.PathPoints[0].X, "PathPoints[0].X-1");
			Assert.AreEqual (1f, gp.PathPoints[0].Y, "PathPoints[0].Y-1");
		}

		[Test]
		public void PathTypes_CannotChange ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));

			Assert.AreEqual (0, gp.PathTypes[0], "PathTypes[0]");

			// now try to change the first type
			gp.PathTypes[0] = 1;
			// the changes isn't reflected in the property
			Assert.AreEqual (0, gp.PathTypes[0], "PathTypes[0]-1");
		}

		private void CheckArc (GraphicsPath path)
		{
			Assert.AreEqual (4, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (4, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (4, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (2.99962401f, rect.X, Delta, "Bounds.X");
			Assert.AreEqual (2.01370716f, rect.Y, Delta, "Bounds.Y");
			Assert.AreEqual (0f, rect.Width, Delta, "Bounds.Width");
			Assert.AreEqual (0.0137047768f, rect.Height, "Bounds.Height");

			Assert.AreEqual (2.99990582f, path.PathData.Points[0].X, Delta, "Points[0].X");
			Assert.AreEqual (2.01370716f, path.PathPoints[0].Y, Delta, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2.99984312f, path.PathData.Points[1].X, Delta, "Points[1].X");
			Assert.AreEqual (2.018276f, path.PathPoints[1].Y, Delta, "Points[1].Y");
			Assert.AreEqual (3, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (2.99974918f, path.PathData.Points[2].X, Delta, "Points[2].X");
			Assert.AreEqual (2.02284455f, path.PathPoints[2].Y, Delta, "Points[2].Y");
			Assert.AreEqual (3, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (2.999624f, path.PathData.Points[3].X, Delta, "Points[3].X");
			Assert.AreEqual (2.027412f, path.PathPoints[3].Y, Delta, "Points[3].Y");
			Assert.AreEqual (3, path.PathTypes[3], "Types[3]");
		}

		[Test]
		public void AddArc_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddArc (new Rectangle (1, 1, 2, 2), Pi4, Pi4);
			CheckArc (gp);
		}

		[Test]
		public void AddArc_RectangleF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddArc (new RectangleF (1f, 1f, 2f, 2f), Pi4, Pi4);
			CheckArc (gp);
		}

		[Test]
		public void AddArc_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddArc (1, 1, 2, 2, Pi4, Pi4);
			CheckArc (gp);
		}

		[Test]
		public void AddArc_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddArc (1f, 1f, 2f, 2f, Pi4, Pi4);
			CheckArc (gp);
		}

		private void CheckBezier (GraphicsPath path)
		{
			Assert.AreEqual (4, path.PointCount, "PointCount");
			Assert.AreEqual (4, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (4, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (4, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (3f, rect.Width, "Bounds.Width");
			Assert.AreEqual (3f, rect.Height, "Bounds.Height");

			Assert.AreEqual (1f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (2f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (3, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (3f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (3f, path.PathPoints[2].Y, "Points[2].Y");
			Assert.AreEqual (3, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (4f, path.PathData.Points[3].X, "Points[3].X");
			Assert.AreEqual (4f, path.PathPoints[3].Y, "Points[3].Y");
			Assert.AreEqual (3, path.PathTypes[3], "Types[3]");
		}

		[Test]
		public void AddBezier_Point ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (new Point (1, 1), new Point (2, 2), new Point (3, 3), new Point (4, 4));
			CheckBezier (gp);
		}

		[Test]
		public void AddBezier_PointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f), new PointF (4f, 4f));
			CheckBezier (gp);
		}

		[Test]
		public void AddBezier_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (1, 1, 2, 2, 3, 3, 4, 4);
			CheckBezier (gp);
		}

		[Test]
		public void AddBezier_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (1f, 1f, 2f, 2f, 3f, 3f, 4f, 4f);
			CheckBezier (gp);
		}

		[Test]
		public void AddBezier_SamePoint ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (1, 1, 1, 1, 1, 1, 1, 1);
			// all points are present
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (3, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (3, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (3, gp.PathTypes [3], "1-PathTypes[3]");

			gp.AddBezier (new Point (1, 1), new Point (1, 1), new Point (1, 1), new Point (1, 1));
			// the first point (move to) can be compressed (i.e. removed)
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
			Assert.AreEqual (3, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (3, gp.PathTypes [5], "2-PathTypes[5]");
			Assert.AreEqual (3, gp.PathTypes [6], "2-PathTypes[6]");
		}

		[Test]
		public void AddBezier_SamePointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBezier (new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f));
			// all points are present
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (3, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (3, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (3, gp.PathTypes [3], "1-PathTypes[3]");

			gp.AddBezier (new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f));
			// the first point (move to) can be compressed (i.e. removed)
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
			Assert.AreEqual (3, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (3, gp.PathTypes [5], "2-PathTypes[5]");
			Assert.AreEqual (3, gp.PathTypes [6], "2-PathTypes[6]");
		}

		[Test]
		public void AddBeziers_Point_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddBeziers ((Point[]) null));
		}

		[Test]
		public void AddBeziers_3_Points ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddBeziers (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) }));
		}

		[Test]
		public void AddBeziers_Point ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new Point[4] { new Point (1, 1), new Point (2, 2), new Point (3, 3), new Point (4, 4) });
			CheckBezier (gp);
		}

		[Test]
		public void AddBeziers_PointF_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddBeziers ((PointF[]) null));
		}

		[Test]
		public void AddBeziers_3_PointFs ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddBeziers (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) }));
		}

		[Test]
		public void AddBeziers_PointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new PointF[4] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f), new PointF (4f, 4f) });
			CheckBezier (gp);
		}

		[Test]
		public void AddBeziers_SamePoint ()
		{
			Point [] points = new Point [4] { new Point (1, 1), new Point (1, 1), new Point (1, 1), new Point (1, 1) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (points);
			// all points are present
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (3, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (3, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (3, gp.PathTypes [3], "1-PathTypes[3]");

			gp.AddBeziers (points);
			// the first point (move to) can be compressed (i.e. removed)
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
			Assert.AreEqual (3, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (3, gp.PathTypes [5], "2-PathTypes[5]");
			Assert.AreEqual (3, gp.PathTypes [6], "2-PathTypes[6]");
		}

		[Test]
		public void AddBeziers_SamePointF ()
		{
			PointF[] points = new PointF [4] { new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (points);
			// all points are present
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (3, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (3, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (3, gp.PathTypes [3], "1-PathTypes[3]");

			gp.AddBeziers (points);
			// the first point (move to) can be compressed (i.e. removed)
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
			Assert.AreEqual (3, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (3, gp.PathTypes [5], "2-PathTypes[5]");
			Assert.AreEqual (3, gp.PathTypes [6], "2-PathTypes[6]");
		}

		private void CheckEllipse (GraphicsPath path)
		{
			Assert.AreEqual (13, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (13, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (13, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2f, rect.Height, "Bounds.Height");

			Assert.AreEqual (0, path.PathData.Types[0], "PathData.Types[0]");
			for (int i = 1; i < 12; i++)
				Assert.AreEqual (3, path.PathTypes[i], "PathTypes" + i.ToString ());
			Assert.AreEqual (131, path.PathData.Types[12], "PathData.Types[12]");
		}

		[Test]
		public void AddEllipse_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddEllipse (new Rectangle (1, 1, 2, 2));
			CheckEllipse (gp);
		}

		[Test]
		public void AddEllipse_RectangleF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddEllipse (new RectangleF (1f, 1f, 2f, 2f));
			CheckEllipse (gp);
		}

		[Test]
		public void AddEllipse_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddEllipse (1, 1, 2, 2);
			CheckEllipse (gp);
		}

		[Test]
		public void AddEllipse_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddEllipse (1f, 1f, 2f, 2f);
			CheckEllipse (gp);
		}

		private void CheckLine (GraphicsPath path)
		{
			Assert.AreEqual (2, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (2, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (2, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (1f, rect.Width, "Bounds.Width");
			Assert.AreEqual (1f, rect.Height, "Bounds.Height");

			Assert.AreEqual (1f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (2f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (1, path.PathTypes[1], "Types[1]");
		}

		[Test]
		public void AddLine_Point ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (new Point (1, 1), new Point (2, 2));
			CheckLine (gp);
		}

		[Test]
		public void AddLine_PointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (new PointF (1f, 1f), new PointF (2f, 2f));
			CheckLine (gp);
		}

		[Test]
		public void AddLine_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (1, 1, 2, 2);
			CheckLine (gp);
		}

		[Test]
		public void AddLine_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (1f, 1f, 2f, 2f);
			CheckLine (gp);
		}

		[Test]
		public void AddLine_SamePoint ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (new Point (1, 1), new Point (1, 1));
			Assert.AreEqual (2, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes[0], "1-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes[1], "1-PathTypes[1]");

			gp.AddLine (new Point (1, 1), new Point (1, 1));
			// 3 not 4 points, the first point (only) is compressed
			Assert.AreEqual (3, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");

			gp.AddLine (new Point (1, 1), new Point (1, 1));
			// 4 not 5 (or 6) points, the first point (only) is compressed
			Assert.AreEqual (4, gp.PointCount, "3-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "3-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "3-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "3-PathTypes[2]");
			Assert.AreEqual (1, gp.PathTypes [3], "3-PathTypes[3]");
		}

		[Test]
		public void AddLine_SamePointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (new PointF (49.2f, 157f), new PointF (49.2f, 157f));
			Assert.AreEqual (2, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "PathTypes[1]");

			gp.AddLine (new PointF (49.2f, 157f), new PointF (49.2f, 157f));
			// 3 not 4 points, the first point (only) is compressed
			Assert.AreEqual (3, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");
		}

		[Test]
		public void AddLine_SamePointsF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLine (new PointF (49.2f, 157f), new PointF (75.6f, 196f));
			gp.AddLine (new PointF (75.6f, 196f), new PointF (102f, 209f));
			Assert.AreEqual (3, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "1-PathTypes[2]");

			gp.AddLine (new PointF (102f, 209f), new PointF (75.6f, 196f));
			Assert.AreEqual (4, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");
			Assert.AreEqual (1, gp.PathTypes [3], "2-PathTypes[3]");
		}

		[Test]
		public void AddLines_Point_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddLines ((Point[])null));
		}

		[Test]
		public void AddLines_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddLines (new Point[0]));
		}

		[Test]
		public void AddLines_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new Point[1] { new Point (1, 1) });
			// Special case - a line with a single point is valid
			Assert.AreEqual (1, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes[0], "PathTypes[0]");
		}

		[Test]
		public void AddLines_Point ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new Point[2] { new Point (1, 1), new Point (2, 2) });
			CheckLine (gp);
		}

		[Test]
		public void AddLines_PointF_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddLines ((PointF[]) null));
		}

		[Test]
		public void AddLines_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddLines (new PointF[0]));
		}

		[Test]
		public void AddLines_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new PointF[1] { new PointF (1f, 1f) });
			// Special case - a line with a single point is valid
			Assert.AreEqual (1, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes[0], "PathTypes[0]");
		}

		[Test]
		public void AddLines_PointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) });
			CheckLine (gp);
		}

		[Test]
		public void AddLines_SamePoint ()
		{
			Point [] points = new Point [] { new Point (1, 1), new Point (1, 1) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (points);
			Assert.AreEqual (2, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "1-PathTypes[1]");

			gp.AddLines (points);
			// 3 not 4 points, the first point (only) is compressed
			Assert.AreEqual (3, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");

			gp.AddLines (points);
			// 4 not 5 (or 6) points, the first point (only) is compressed
			Assert.AreEqual (4, gp.PointCount, "3-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "3-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "3-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "3-PathTypes[2]");
			Assert.AreEqual (1, gp.PathTypes [3], "3-PathTypes[3]");
		}

		[Test]
		public void AddLines_SamePointF ()
		{
			PointF [] points = new PointF [] { new PointF (49.2f, 157f), new PointF (49.2f, 157f), new PointF (49.2f, 157f), new PointF (49.2f, 157f) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (points);
			// all identical points are added
			Assert.AreEqual (4, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "PathTypes[2]");
			Assert.AreEqual (1, gp.PathTypes [3], "PathTypes[3]");

			gp.AddLines (points);
			// only the first new point is compressed
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");
			Assert.AreEqual (1, gp.PathTypes [3], "2-PathTypes[3]");
			Assert.AreEqual (1, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (1, gp.PathTypes [5], "2-PathTypes[5]");
			Assert.AreEqual (1, gp.PathTypes [6], "2-PathTypes[6]");
		}

		private void CheckPie (GraphicsPath path)
		{
			// the number of points generated for a Pie isn't the same between Mono and MS
#if false
			Assert.AreEqual (5, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (5, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (5, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (2f, rect.X, "Bounds.X");
			Assert.AreEqual (2f, rect.Y, "Bounds.Y");
			Assert.AreEqual (0.9999058f, rect.Width, "Bounds.Width");
			Assert.AreEqual (0.0274119377f, rect.Height, "Bounds.Height");

			Assert.AreEqual (2f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (2f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2.99990582f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (2.01370716f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (1, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (2.99984312f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (2.018276f, path.PathPoints[2].Y, "Points[2].Y");
			Assert.AreEqual (3, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (2.99974918f, path.PathData.Points[3].X, "Points[2].X");
			Assert.AreEqual (2.02284455f, path.PathPoints[3].Y, "Points[2].Y");
			Assert.AreEqual (3, path.PathData.Types[3], "Types[2]");
			Assert.AreEqual (2.999624f, path.PathData.Points[4].X, "Points[3].X");
			Assert.AreEqual (2.027412f, path.PathPoints[4].Y, "Points[3].Y");
			Assert.AreEqual (131, path.PathTypes[4], "Types[3]");
#endif
		}

		[Test]
		public void AddPie_Rect ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (new Rectangle (1, 1, 2, 2), Pi4, Pi4);
			CheckPie (gp);
		}

		[Test]
		public void AddPie_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (1, 1, 2, 2, Pi4, Pi4);
			CheckPie (gp);
		}

		[Test]
		public void AddPie_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (1f, 1f, 2f, 2f, Pi4, Pi4);
			CheckPie (gp);
		}

		private void CheckPolygon (GraphicsPath path)
		{
			// an extra point is generated by Mono (libgdiplus)
#if false
			Assert.AreEqual (3, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (3, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (3, path.PathData.Points.Length, "PathData");
#endif
			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2f, rect.Height, "Bounds.Height");

			Assert.AreEqual (1f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (2f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (1, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (3f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (3f, path.PathPoints[2].Y, "Points[2].Y");
			// the extra point change the type of the last point
#if false
			Assert.AreEqual (129, path.PathData.Types[2], "Types[2]");
#endif
		}

		[Test]
		public void AddPolygon_Point_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddPolygon ((Point[]) null));
		}

		[Test]
		public void AddPolygon_Point_Empty ()
		{
			Assert.Throws<ArgumentException> (() => new GraphicsPath ().AddPolygon (new Point[0]));
		}

		[Test]
		public void AddPolygon_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddPolygon (new Point[1] { new Point (1, 1) }));
		}

		[Test]
		public void AddPolygon_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddPolygon (new Point[2] { new Point (1, 1), new Point (2, 2) }));
		}

		[Test]
		public void AddPolygon_Point_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			CheckPolygon (gp);
		}

		[Test]
		public void AddPolygon_PointF_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddPolygon ((PointF[]) null));
		}

		[Test]
		public void AddPolygon_PointF_Empty ()
		{
			Assert.Throws<ArgumentException> (() => new GraphicsPath ().AddPolygon (new PointF[0]));
		}

		[Test]
		public void AddPolygon_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddPolygon (new PointF[1] { new PointF (1f, 1f) }));
		}

		[Test]
		public void AddPolygon_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddPolygon (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }));
		}

		[Test]
		public void AddPolygon_PointF_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) });
			CheckPolygon (gp);
		}

		[Test]
		public void AddPolygon_SamePoint ()
		{
			Point [] points = new Point [3] { new Point (1, 1), new Point (1, 1), new Point (1, 1) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (points);
			// all identical points are added
			Assert.AreEqual (3, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "PathTypes[1]");
			Assert.AreEqual (129, gp.PathTypes [2], "PathTypes[2]");

			gp.AddPolygon (points);
			// all identical points are added (again)
			Assert.AreEqual (6, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [3], "2-PathTypes[3]");
			Assert.AreEqual (1, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (129, gp.PathTypes [5], "2-PathTypes[5]");

			gp.AddLines (points);
			// all identical points are added as a line (because previous point is closed)
			Assert.AreEqual (9, gp.PointCount, "3-PointCount");
			Assert.AreEqual (0, gp.PathTypes [6], "3-PathTypes[6]");
			Assert.AreEqual (1, gp.PathTypes [7], "3-PathTypes[7]");
			Assert.AreEqual (1, gp.PathTypes [8], "3-PathTypes[8]");

			gp.AddPolygon (points);
			// all identical points are added (again)
			Assert.AreEqual (12, gp.PointCount, "4-PointCount");
			Assert.AreEqual (0, gp.PathTypes [9], "4-PathTypes[9]");
			Assert.AreEqual (1, gp.PathTypes [10], "4-PathTypes[10]");
			Assert.AreEqual (129, gp.PathTypes [11], "4-PathTypes[11]");
		}

		[Test]
		public void AddPolygon_SamePointF ()
		{
			PointF [] points = new PointF [3] { new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (points);
			// all identical points are added
			Assert.AreEqual (3, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "PathTypes[1]");
			Assert.AreEqual (129, gp.PathTypes [2], "PathTypes[2]");

			gp.AddPolygon (points);
			// all identical points are added (again)
			Assert.AreEqual (6, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [3], "2-PathTypes[3]");
			Assert.AreEqual (1, gp.PathTypes [4], "2-PathTypes[4]");
			Assert.AreEqual (129, gp.PathTypes [5], "2-PathTypes[5]");

			gp.AddLines (points);
			// all identical points are added as a line (because previous point is closed)
			Assert.AreEqual (9, gp.PointCount, "3-PointCount");
			Assert.AreEqual (0, gp.PathTypes [6], "3-PathTypes[6]");
			Assert.AreEqual (1, gp.PathTypes [7], "3-PathTypes[7]");
			Assert.AreEqual (1, gp.PathTypes [8], "3-PathTypes[8]");

			gp.AddPolygon (points);
			// all identical points are added (again)
			Assert.AreEqual (12, gp.PointCount, "4-PointCount");
			Assert.AreEqual (0, gp.PathTypes [9], "4-PathTypes[9]");
			Assert.AreEqual (1, gp.PathTypes [10], "4-PathTypes[10]");
			Assert.AreEqual (129, gp.PathTypes [11], "4-PathTypes[11]");
		}

		private void CheckRectangle (GraphicsPath path, int count)
		{
			Assert.AreEqual (count, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (count, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (count, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2f, rect.Height, "Bounds.Height");

			// check first four points (first rectangle)
			Assert.AreEqual (1f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (3f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (1f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (1, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (3f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (3f, path.PathPoints[2].Y, "Points[2].Y");
			Assert.AreEqual (1, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (1f, path.PathData.Points[3].X, "Points[3].X");
			Assert.AreEqual (3f, path.PathPoints[3].Y, "Points[3].Y");
			Assert.AreEqual (129, path.PathTypes[3], "Types[3]");
		}

		[Test]
		public void AddRectangle_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			CheckRectangle (gp, 4);
		}

		[Test]
		public void AddRectangle_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new RectangleF (1f, 1f, 2f, 2f));
			CheckRectangle (gp, 4);
		}

		[Test]
		public void AddRectangle_SamePoint ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 0, 0));
			Assert.AreEqual (0, gp.PointCount, "0-PointCount");

			gp.AddRectangle (new Rectangle (1, 1, 1, 1));
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (129, gp.PathTypes [3], "1-PathTypes[3]");
			PointF end = gp.PathPoints [3];

			// add rectangle at the last path point
			gp.AddRectangle (new Rectangle ((int)end.X, (int)end.Y, 1, 1));
			// no compression (different type)
			Assert.AreEqual (8, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");
			Assert.AreEqual (129, gp.PathTypes [3], "2-PathTypes[3]");
		}

		[Test]
		public void AddRectangle_SamePointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new RectangleF (1f, 1f, 0f, 0f));
			Assert.AreEqual (0, gp.PointCount, "0-PointCount");

			gp.AddRectangle (new RectangleF (1f, 1f, 1f, 1f));
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "1-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "1-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "1-PathTypes[2]");
			Assert.AreEqual (129, gp.PathTypes [3], "1-PathTypes[3]");
			PointF end = gp.PathPoints [3];

			// add rectangle at the last path point
			gp.AddRectangle (new RectangleF (end.X, end.Y, 1f, 1f));
			// no compression (different type)
			Assert.AreEqual (8, gp.PointCount, "2-PointCount");
			Assert.AreEqual (0, gp.PathTypes [0], "2-PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes [1], "2-PathTypes[1]");
			Assert.AreEqual (1, gp.PathTypes [2], "2-PathTypes[2]");
			Assert.AreEqual (129, gp.PathTypes [3], "2-PathTypes[3]");
		}

		[Test]
		public void AddRectangles_Int_Null ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentNullException> (() => gp.AddRectangles ((Rectangle[]) null));
		}

		[Test]
		public void AddRectangles_Int_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddRectangles (new Rectangle[0]));
		}

		[Test]
		public void AddRectangles_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new Rectangle[1] { new Rectangle (1, 1, 2, 2) });
			CheckRectangle (gp, 4);
		}

		[Test]
		public void AddRectangles_Float_Null ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentNullException> (() => gp.AddRectangles ((RectangleF[]) null));
		}

		[Test]
		public void AddRectangles_Float_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddRectangles ( new RectangleF[0]));
		}

		[Test]
		public void AddRectangles_Float ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new RectangleF [1] { new RectangleF (1f, 1f, 2f, 2f) });
			CheckRectangle (gp, 4);
		}

		[Test]
		public void AddRectangles_Two ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new RectangleF[2] {
				new RectangleF (1f, 1f, 2f, 2f),
				new RectangleF (2f, 2f, 1f, 1f) } );
			RectangleF rect = gp.GetBounds ();
			Assert.AreEqual (1f, rect.X, "Bounds.X");
			Assert.AreEqual (1f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2f, rect.Height, "Bounds.Height");
			// second rectangle is completely within the first one
			CheckRectangle (gp, 8);
		}

		[Test]
		public void AddRectangles_SamePoint ()
		{
			Rectangle r1 = new Rectangle (1, 1, 0, 0);
			Rectangle r2 = new Rectangle (1, 1, 1, 1);
			Rectangle r3 = new Rectangle (1, 2, 1, 1);

			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new Rectangle[] { r1, r2, r3 });
			Assert.AreEqual (8, gp.PointCount, "1-PointCount");
			// first rect is ignore, then all other 2x4 (8) points are present, no compression
		}

		[Test]
		public void AddPath_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddPath (null, false));
		}

		[Test]
		public void AddPath ()
		{
			GraphicsPath gpr = new GraphicsPath ();
			gpr.AddRectangle (new Rectangle (1, 1, 2, 2));
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPath (gpr, true);
			CheckRectangle (gp, 4);
		}

		private void CheckClosedCurve (GraphicsPath path)
		{
			Assert.AreEqual (10, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (10, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (10, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't very precise with curves
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (0.8333333f, rect.X, 0.2f, "Bounds.X");
			Assert.AreEqual (0.8333333f, rect.Y, 0.2f, "Bounds.Y");
			Assert.AreEqual (2.33333278f, rect.Width, 0.4f, "Bounds.Width");
			Assert.AreEqual (2.33333278f, rect.Height, 0.4f, "Bounds.Height");

			Assert.AreEqual (0, path.PathData.Types[0], "PathData.Types[0]");
			for (int i = 1; i < 9; i++)
				Assert.AreEqual (3, path.PathTypes[i], "PathTypes" + i.ToString ());
			Assert.AreEqual (131, path.PathData.Types[9], "PathData.Types[9]");
		}

		[Test]
		public void AddClosedCurve_Point_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddClosedCurve ((Point[])null));
		}

		[Test]
		public void AddClosedCurve_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new Point [0]));
		}

		[Test]
		public void AddClosedCurve_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new Point[1] { new Point (1, 1) }));
		}

		[Test]
		public void AddClosedCurve_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new Point[2] { new Point (1, 1), new Point (2, 2) }));
		}

		[Test]
		public void AddClosedCurve_Point_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			CheckClosedCurve (gp);
		}

		[Test]
		public void AddClosedCurve_PointF_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddClosedCurve ((PointF[]) null));
		}

		[Test]
		public void AddClosedCurve_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new PointF[0]));
		}

		[Test]
		public void AddClosedCurve_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new PointF[1] { new PointF (1f, 1f) }));
		}

		[Test]
		public void AddClosedCurve_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddClosedCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }));
		}

		[Test]
		public void AddClosedCurve_PointF_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) });
			CheckClosedCurve (gp);
		}

		[Test]
		public void AddClosedCurve_SamePoint ()
		{
			Point [] points = new Point [3] { new Point (1, 1), new Point (1, 1), new Point (1, 1) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (points);
			Assert.AreEqual (10, gp.PointCount, "1-PointCount");
			gp.AddClosedCurve (points);
			Assert.AreEqual (20, gp.PointCount, "2-PointCount");
		}

		[Test]
		public void AddClosedCurve_SamePointF ()
		{
			PointF [] points = new PointF [3] { new PointF (1f, 1f), new PointF (1f, 1f), new PointF (1f, 1f) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (points);
			Assert.AreEqual (10, gp.PointCount, "1-PointCount");
			gp.AddClosedCurve (points);
			Assert.AreEqual (20, gp.PointCount, "2-PointCount");
		}

		private void CheckCurve (GraphicsPath path)
		{
			Assert.AreEqual (4, path.PathPoints.Length, "PathPoints");
			Assert.AreEqual (4, path.PathTypes.Length, "PathPoints");
			Assert.AreEqual (4, path.PathData.Points.Length, "PathData");

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (1.0f, rect.X, "Bounds.X");
			Assert.AreEqual (1.0f, rect.Y, "Bounds.Y");
			Assert.AreEqual (1.0f, rect.Width, "Bounds.Width");
			Assert.AreEqual (1.0f, rect.Height, "Bounds.Height");

			Assert.AreEqual (1f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (1f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			// Mono has wrong? results
#if false
			Assert.AreEqual (1.16666663f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (1.16666663f, path.PathPoints[1].Y, "Points[1].Y");
#endif
			Assert.AreEqual (3, path.PathTypes[1], "Types[1]");
			// Mono has wrong? results
#if false
			Assert.AreEqual (1.83333325f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (1.83333325f, path.PathPoints[2].Y, "Points[2].Y");
#endif
			Assert.AreEqual (3, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (2f, path.PathData.Points[3].X, "Points[3].X");
			Assert.AreEqual (2f, path.PathPoints[3].Y, "Points[3].Y");
			Assert.AreEqual (3, path.PathTypes[3], "Types[3]");
		}

		[Test]
		public void AddCurve_Point_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddCurve ((Point[]) null));
		}

		[Test]
		public void AddCurve_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new Point[0]));
		}

		[Test]
		public void AddCurve_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new Point[1] { new Point (1, 1) }));
		}

		[Test]
		public void AddCurve_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new Point[2] { new Point (1, 1), new Point (2, 2) });
			CheckCurve (gp);
			// note: GdipAddPathCurveI allows adding a "curve" with only 2 points (a.k.a. a line ;-)
			gp.Dispose ();
		}

		[Test]
		public void AddCurve_Point_2_Tension ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new Point[2] { new Point (1, 1), new Point (2, 2) }, 1.0f);
			CheckCurve (gp);
			// note: GdipAddPathCurve2I allows adding a "curve" with only 2 points (a.k.a. a line ;-)
			gp.Dispose ();
		}

		[Test]
		public void AddCurve3_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new Point[2] { new Point (1, 1), new Point (2, 2) }, 0, 2, 0.5f));
			// adding only two points isn't supported by GdipAddCurve3I
		}

		[Test]
		public void AddCurve_PointF_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().AddCurve ((PointF[]) null));
		}

		[Test]
		public void AddCurve_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[0]));
		}

		[Test]
		public void AddCurve_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[1] { new PointF (1f, 1f) }));
		}

		[Test]
		public void AddCurve_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) });
			CheckCurve (gp);
			// note: GdipAddPathCurve allows adding a "curve" with only 2 points (a.k.a. a line ;-)
			gp.Dispose ();
		}

		[Test]
		public void AddCurve_PoinFt_2_Tension ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }, 1.0f);
			CheckCurve (gp);
			// note: GdipAddPathCurve2 allows adding a "curve" with only 2 points (a.k.a. a line ;-)
			gp.Dispose ();
		}

		[Test]
		public void AddCurve3_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }, 0, 2, 0.5f));
			// adding only two points isn't supported by GdipAddCurve3
		}

		[Test]
		public void AddCurve_LargeTension ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[3] { new PointF (1f, 1f), new PointF (0f, 20f), new PointF (20f, 0f) }, 0, 2, Single.MaxValue);
			Assert.AreEqual (7, gp.PointCount, "PointCount");
			gp.Dispose ();
		}

		[Test]
		public void AddCurve_ZeroSegments ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }, 0, 0, 0.5f));
		}

		[Test]
		public void AddCurve_NegativeSegments ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) }, 0, -1, 0.5f));
		}

		[Test]
		public void AddCurve_OffsetTooLarge ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddCurve (new PointF[3] { new PointF (1f, 1f), new PointF (0f, 20f), new PointF (20f, 0f) }, 1, 2, 0.5f));
		}

		[Test]
		public void AddCurve_Offset ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[4] { new PointF (1f, 1f), new PointF (0f, 20f), new PointF (20f, 0f), new PointF (0f, 10f) }, 1, 2, 0.5f);
			Assert.AreEqual (7, gp.PointCount, "PointCount");
			gp.Dispose ();
		}

		[Test]
		public void AddCurve_SamePoint ()
		{
			Point [] points = new Point [2] { new Point (1, 1), new Point (1, 1) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (points);
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			gp.AddCurve (points);
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
		}

		[Test]
		public void AddCurve_SamePointF ()
		{
			PointF [] points = new PointF [2] { new PointF (1f, 1f), new PointF (1f, 1f) };
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (points);
			Assert.AreEqual (4, gp.PointCount, "1-PointCount");
			gp.AddCurve (points);
			Assert.AreEqual (7, gp.PointCount, "2-PointCount");
		}

		[Test]
		public void AddCurve ()
		{
			PointF [] points = new PointF [] {
				new PointF (37f, 185f),
				new PointF (99f, 185f),
				new PointF (161f, 159f),
				new PointF (223f, 185f),
				new PointF (285f, 54f),
			};
			int[] count = { 4, 7, 10, 13 };

			using (GraphicsPath gp = new GraphicsPath ()) {
				for (int i = 0; i < points.Length - 1; i++) {
					gp.AddCurve (points, i, 1, 0.5f);
					// all non-curves points are compressed expect the first one (positioning)
					Assert.AreEqual (count [i], gp.PointCount, i.ToString ());
				}

				Assert.AreEqual (0, gp.PathData.Types [0], "Types[0]");
				Assert.AreEqual (37f, gp.PathData.Points [0].X, 0.001, "Points[0].X");
				Assert.AreEqual (185f, gp.PathData.Points [1].Y, 0.001, "Points[0].Y");
				Assert.AreEqual (3, gp.PathData.Types [1], "Types[1]");
				Assert.AreEqual (47.3334f, gp.PathData.Points [1].X, 0.001, "Points[1].X");
				Assert.AreEqual (185f, gp.PathData.Points [1].Y, 0.001, "Points[1].Y");
				Assert.AreEqual (3, gp.PathData.Types [2], "Types[2]");
				Assert.AreEqual (78.33333f, gp.PathData.Points [2].X, 0.001, "Points[2].X");
				Assert.AreEqual (189.3333f, gp.PathData.Points [2].Y, 0.001, "Points[2].Y");
				Assert.AreEqual (3, gp.PathData.Types [3], "Types[3]");
				Assert.AreEqual (99f, gp.PathData.Points [3].X, 0.001, "Points[3].X");
				Assert.AreEqual (185f, gp.PathData.Points [3].Y, 0.001, "Points[3].Y");
				Assert.AreEqual (3, gp.PathData.Types [4], "Types[4]");
				Assert.AreEqual (119.6667f, gp.PathData.Points [4].X, 0.001, "Points[4].X");
				Assert.AreEqual (180.6667f, gp.PathData.Points [4].Y, 0.001, "Points[4].Y");
				Assert.AreEqual (3, gp.PathData.Types [5], "Types[5]");
				Assert.AreEqual (140.3333f, gp.PathData.Points [5].X, 0.001, "Points[5].X");
				Assert.AreEqual (159f, gp.PathData.Points [5].Y, 0.001, "Points[5].Y");
				Assert.AreEqual (3, gp.PathData.Types [6], "Types[6]");
				Assert.AreEqual (161f, gp.PathData.Points [6].X, 0.001, "Points[6].X");
				Assert.AreEqual (159f, gp.PathData.Points [6].Y, 0.001, "Points[6].Y");
				Assert.AreEqual (3, gp.PathData.Types [7], "Types[7]");
				Assert.AreEqual (181.6667f, gp.PathData.Points [7].X, 0.001, "Points[7].X");
				Assert.AreEqual (159f, gp.PathData.Points [7].Y, 0.001, "Points[7].Y");
				Assert.AreEqual (3, gp.PathData.Types [8], "Types[8]");
				Assert.AreEqual (202.3333f, gp.PathData.Points [8].X, 0.001, "Points[8].X");
				Assert.AreEqual (202.5f, gp.PathData.Points [8].Y, 0.001, "Points[8].Y");
				Assert.AreEqual (3, gp.PathData.Types [9], "Types[9]");
				Assert.AreEqual (223f, gp.PathData.Points [9].X, 0.001, "Points[9].X");
				Assert.AreEqual (185f, gp.PathData.Points [9].Y, 0.001, "Points[9].Y");
				Assert.AreEqual (3, gp.PathData.Types [10], "Types[10]");
				Assert.AreEqual (243.6667f, gp.PathData.Points [10].X, 0.001, "Points[10].X");
				Assert.AreEqual (167.5f, gp.PathData.Points [10].Y, 0.001, "Points[10].Y");
				Assert.AreEqual (3, gp.PathData.Types [11], "Types[11]");
				Assert.AreEqual (274.6667f, gp.PathData.Points [11].X, 0.001, "Points[11].X");
				Assert.AreEqual (75.83334f, gp.PathData.Points [11].Y, 0.001, "Points[11].Y");
				Assert.AreEqual (3, gp.PathData.Types [12], "Types[12]");
				Assert.AreEqual (285f, gp.PathData.Points [12].X, 0.001, "Points[12].X");
				Assert.AreEqual (54f, gp.PathData.Points [12].Y, 0.001, "Points[12].Y");
			}
		}

		private FontFamily GetFontFamily ()
		{
			try {
				return FontFamily.GenericMonospace;
			}
			catch (ArgumentException) {
				Assert.Ignore ("GenericMonospace FontFamily couldn't be found");
				return null;
			}
		}

		[Test]
		public void AddString_NullString ()
		{
			GraphicsPath gp = new GraphicsPath ();
			FontFamily ff = GetFontFamily ();
			Assert.Throws<NullReferenceException> (() => gp.AddString (null, ff, 0, 10, new Point (10, 10), StringFormat.GenericDefault));
		}

		[Test]
		public void AddString_EmptyString ()
		{
			GraphicsPath gp = new GraphicsPath ();
			FontFamily ff = GetFontFamily ();
			gp.AddString (String.Empty, ff, 0, 10, new Point (10, 10), StringFormat.GenericDefault);
			Assert.AreEqual (0, gp.PointCount, "PointCount");
		}

		[Test]
		public void AddString_NullFontFamily ()
		{
			GraphicsPath gp = new GraphicsPath ();
			Assert.Throws<ArgumentException> (() => gp.AddString ("mono", null, 0, 10, new Point (10, 10), StringFormat.GenericDefault));
		}

		[Test]
		public void AddString_NegativeSize ()
		{
			GraphicsPath gp = new GraphicsPath ();
			FontFamily ff = GetFontFamily ();
			gp.AddString ("mono", ff, 0, -10, new Point (10, 10), StringFormat.GenericDefault);
			Assert.IsTrue (gp.PointCount > 0, "PointCount");
		}

		[Test]
		[Category ("NotWorking")] // StringFormat not yet supported in libgdiplus
		public void AddString_StringFormat ()
		{
			FontFamily ff = GetFontFamily ();
			// null maps to ?
			GraphicsPath gp1 = new GraphicsPath ();
			gp1.AddString ("mono", ff, 0, 10, new RectangleF (10, 10, 10, 10), null);

			// StringFormat.GenericDefault
			GraphicsPath gp2 = new GraphicsPath ();
			gp2.AddString ("mono", ff, 0, 10, new RectangleF (10, 10, 10, 10), StringFormat.GenericDefault);
			Assert.AreEqual (gp1.PointCount, gp2.PointCount, "GenericDefault");

			// StringFormat.GenericTypographic
			GraphicsPath gp3 = new GraphicsPath ();
			gp3.AddString ("mono", ff, 0, 10, new RectangleF (10, 10, 10, 10), StringFormat.GenericTypographic);
			Assert.IsFalse (gp1.PointCount == gp3.PointCount, "GenericTypographic");
		}

		[Test]
		public void GetBounds_Empty_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			RectangleF rect = gp.GetBounds ();
			Assert.AreEqual (0.0f, rect.X, "Bounds.X");
			Assert.AreEqual (0.0f, rect.Y, "Bounds.Y");
			Assert.AreEqual (0.0f, rect.Width, "Bounds.Width");
			Assert.AreEqual (0.0f, rect.Height, "Bounds.Height");
		}

		private void CheckRectangleBounds (RectangleF rect)
		{
			Assert.AreEqual (1.0f, rect.X, "Bounds.X");
			Assert.AreEqual (1.0f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2.0f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2.0f, rect.Height, "Bounds.Height");
		}

		[Test]
		public void GetBounds_Empty_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			CheckRectangleBounds (gp.GetBounds ());
		}

		[Test]
		public void GetBounds_Null_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			CheckRectangleBounds (gp.GetBounds (null));
		}

		[Test]
		public void GetBounds_MatrixEmpty_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			CheckRectangleBounds (gp.GetBounds (new Matrix ()));
		}

		[Test]
		public void GetBounds_NullNull_Rectangle ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			CheckRectangleBounds (gp.GetBounds (null, null));
		}

		[Test]
		[Category ("NotWorking")] // can't/wont duplicate the lack of precision
		public void GetBounds_WithPen ()
		{
			Rectangle rect = new Rectangle (1, 1, 2, 2);
			Pen p = new Pen (Color.Aqua, 0);
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (rect);

			RectangleF bounds = gp.GetBounds (null, p);
			// those bounds doesn't make any sense (even visually)
			// probably null gets mis-interpreted ???
			Assert.AreEqual (-6.09999943f, bounds.X, "NullMatrix.Bounds.X");
			Assert.AreEqual (-6.09999943f, bounds.Y, "NullMatrix.Bounds.Y");
			Assert.AreEqual (16.1999989f, bounds.Width, "NullMatrix.Bounds.Width");
			Assert.AreEqual (16.1999989f, bounds.Height, "NullMatrix.Bounds.Height");

			Matrix m = new Matrix ();
			bounds = gp.GetBounds (m, p);
			Assert.AreEqual (-0.419999957f, bounds.X, "EmptyMatrix.Bounds.X");
			Assert.AreEqual (-0.419999957f, bounds.Y, "EmptyMatrix.Bounds.Y");
			Assert.AreEqual (4.83999968f, bounds.Width, "EmptyMatrix.Bounds.Width");
			Assert.AreEqual (4.83999968f, bounds.Height, "EmptyMatrix.Bounds.Height");
			// visually we can see the bounds just a pixel bigger than the rectangle

			gp = new GraphicsPath ();
			gp.AddRectangle (rect);
			gp.Widen (p);
			bounds = gp.GetBounds (null);
			Assert.AreEqual (0.499999523f, bounds.X, "WidenNullMatrix.Bounds.X");
			Assert.AreEqual (0.499999523f, bounds.Y, "WidenNullMatrix.Bounds.Y");
			Assert.AreEqual (3.000001f, bounds.Width, "WidenNullMatrix.Bounds.Width");
			Assert.AreEqual (3.000001f, bounds.Height, "WidenNullMatrix.Bounds.Height");

			bounds = gp.GetBounds (m);
			Assert.AreEqual (0.499999523f, bounds.X, "WidenEmptyMatrix.Bounds.X");
			Assert.AreEqual (0.499999523f, bounds.Y, "WidenEmptyMatrix.Bounds.Y");
			Assert.AreEqual (3.000001f, bounds.Width, "WidenEmptyMatrix.Bounds.Width");
			Assert.AreEqual (3.000001f, bounds.Height, "WidenEmptyMatrix.Bounds.Height");
		}

		private void CheckPieBounds (RectangleF rect)
		{
			Assert.AreEqual (60.0f, rect.X, 1, "Bounds.X");
			Assert.AreEqual (60.0f, rect.Y, 1, "Bounds.Y");
			Assert.AreEqual (43.3f, rect.Width, 1, "Bounds.Width");
			Assert.AreEqual (48.3f, rect.Height, 1, "Bounds.Height");
		}

		[Test]
		public void GetBounds_Empty_Pie ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (10, 10, 100, 100, 30, 45);
			CheckPieBounds (gp.GetBounds ());
			gp.Dispose ();
		}

		[Test]
		public void GetBounds_Null_Pie ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (10, 10, 100, 100, 30, 45);
			CheckPieBounds (gp.GetBounds (null));
			gp.Dispose ();
		}

		[Test]
		public void GetBounds_MatrixEmpty_Pie ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (10, 10, 100, 100, 30, 45);
			CheckPieBounds (gp.GetBounds (new Matrix ()));
			gp.Dispose ();
		}

		[Test]
		public void GetBounds_NullNull_Pie ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPie (10, 10, 100, 100, 30, 45);
			CheckPieBounds (gp.GetBounds (null, null));
			gp.Dispose ();
		}

		[Test]
		public void GetBounds_Empty_ClosedCurve ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point[4] { new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100) });
#if false
			// so far from reality that it's totally useless
			Assert.AreEqual (1.666666f, rect.X, 0.00001, "Bounds.X");
			Assert.AreEqual (-6.66666f, rect.Y, 1, "Bounds.Y");
			Assert.AreEqual (196.6666f, rect.Width, 1, "Bounds.Width");
			Assert.AreEqual (221.6666f, rect.Height, 1, "Bounds.Height");
#endif
			gp.Dispose ();
		}

		[Test]
		public void Transform_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().Transform (null));
		}
		[Test]
		public void Transform_Empty ()
		{
			// no points in path and no exception
			new GraphicsPath ().Transform (new Matrix ());
		}

		private void ComparePaths (GraphicsPath expected, GraphicsPath actual)
		{
			Assert.AreEqual (expected.PointCount, actual.PointCount, "PointCount");
			for (int i = 0; i < expected.PointCount; i++) {
				Assert.AreEqual (expected.PathPoints[i], actual.PathPoints[i], "PathPoints-" + i.ToString ());
				Assert.AreEqual (expected.PathTypes[i], actual.PathTypes[i], "PathTypes-" + i.ToString ());
			}
		}

		private void CompareFlats (GraphicsPath flat, GraphicsPath original)
		{
			Assert.IsTrue (flat.PointCount >= original.PointCount, "PointCount");
			for (int i = 0; i < flat.PointCount; i++) {
				Assert.IsTrue (flat.PathTypes[i] != 3, "PathTypes-" + i.ToString ());
			}
		}

		[Test]
		public void Flatten_Empty ()
		{
			GraphicsPath path = new GraphicsPath ();
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			// this is a no-op as there's nothing in the path
			path.Flatten ();
			ComparePaths (path, clone);
		}

		[Test]
		public void Flatten_Null ()
		{
			GraphicsPath path = new GraphicsPath ();
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			// this is a no-op as there's nothing in the path
			// an no matrix to apply
			path.Flatten (null);
			ComparePaths (path, clone);
		}

		[Test]
		public void Flatten_NullFloat ()
		{
			GraphicsPath path = new GraphicsPath ();
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			// this is a no-op as there's nothing in the path
			// an no matrix to apply
			path.Flatten (null, 1f);
			ComparePaths (path, clone);
		}

		[Test]
		public void Flatten_Arc ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddArc (0f, 0f, 100f, 100f, 30, 30);
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_Bezier ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddBezier (0, 0, 100, 100, 30, 30, 60, 60);
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_ClosedCurve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddClosedCurve (new Point[4] { 
				new Point (0, 0), new Point (40, 20),
				new Point (20, 40), new Point (40, 40)
				});
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_Curve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddCurve (new Point[4] { 
				new Point (0, 0), new Point (40, 20),
				new Point (20, 40), new Point (40, 40)
				});
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_Ellipse ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddEllipse (10f, 10f, 100f, 100f);
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_Line ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (10f, 10f, 100f, 100f);
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			ComparePaths (path, clone);
		}

		[Test]
		public void Flatten_Pie ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddPie (0, 0, 100, 100, 30, 30);
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			CompareFlats (path, clone);
		}

		[Test]
		public void Flatten_Polygon ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[4] { 
				new Point (0, 0), new Point (10, 10),
				new Point (20, 20), new Point (40, 40)
				});
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			ComparePaths (path, clone);
		}

		[Test]
		public void Flatten_Rectangle ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddRectangle (new Rectangle (0, 0, 100, 100));
			GraphicsPath clone = (GraphicsPath) path.Clone ();
			path.Flatten ();
			ComparePaths (path, clone);
		}

		private void CheckWrap (GraphicsPath path)
		{
			Assert.AreEqual (3, path.PointCount, "Count");

			PointF[] pts = path.PathPoints;
			Assert.AreEqual (0, pts[0].X, 1e-30, "0.X");
			Assert.AreEqual (0, pts[0].Y, 1e-30, "0.Y");
			Assert.AreEqual (0, pts[1].X, 1e-30, "1.X");
			Assert.AreEqual (0, pts[1].Y, 1e-30, "1.Y");
			Assert.AreEqual (0, pts[2].X, 1e-30, "2.X");
			Assert.AreEqual (0, pts[2].Y, 1e-30, "2.Y");

			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (129, types[2], "2");
		}

		private void CheckWrapNaN (GraphicsPath path, bool closed)
		{
			Assert.AreEqual (3, path.PointCount, "Count");

			PointF[] pts = path.PathPoints;
			Assert.AreEqual (Single.NaN, pts[0].X, "0.X");
			Assert.AreEqual (Single.NaN, pts[0].Y, "0.Y");
			Assert.AreEqual (Single.NaN, pts[1].X, "1.X");
			Assert.AreEqual (Single.NaN, pts[1].Y, "1.Y");
			Assert.AreEqual (Single.NaN, pts[2].X, "2.X");
			Assert.AreEqual (Single.NaN, pts[2].Y, "2.Y");

			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (closed ? 129 : 1, types[2], "2");
		}

		[Test]
		public void Warp_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().Warp (null, new RectangleF ()));
		}

		[Test]
		public void Warp_NoPoints ()
		{
			Assert.Throws<ArgumentException> (() => new GraphicsPath ().Warp (new PointF[0], new RectangleF ()));
		}

		[Test]
		public void Wrap_NoPoint ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Assert.AreEqual (0, gp.PointCount, "PointCount-1");

				PointF[] pts = new PointF[1] { new PointF (0, 0) };
				RectangleF r = new RectangleF (10, 20, 30, 40);
				gp.Warp (pts, r, new Matrix ());
				Assert.AreEqual (0, gp.PointCount, "PointCount-2");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Wrap_SinglePoint ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (new Point[1] { new Point (1, 1) });
				// Special case - a line with a single point is valid
				Assert.AreEqual (1, gp.PointCount, "PointCount-1");

				PointF[] pts = new PointF[1] { new PointF (0, 0) };
				RectangleF r = new RectangleF (10, 20, 30, 40);
				gp.Warp (pts, r, new Matrix ());
				Assert.AreEqual (0, gp.PointCount, "PointCount-2");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void Wrap_Line ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (new Point (1, 1), new Point (20, 20));
				Assert.AreEqual (2, gp.PointCount, "PointCount-1");

				PointF[] pts = new PointF[1] { new PointF (0, 0) };
				RectangleF r = new RectangleF (10, 20, 30, 40);
				gp.Warp (pts, r, new Matrix ());
				Assert.AreEqual (2, gp.PointCount, "PointCount-2");
			}
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_NullMatrix ()
		{
			PointF[] pts = new PointF[1] { new PointF (0,0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, null);
			CheckWrap (path);
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_EmptyMatrix ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix ());
			CheckWrap (path);
		}

		[Test]
		[Category ("NotWorking")]
		public void Warp_Rectangle_Empty ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			path.Warp (pts, new RectangleF (), null);
			CheckWrapNaN (path, true);
		}

		[Test]
		[Category ("NotWorking")]
		public void Warp_Rectangle_NegativeWidthHeight ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, -30, -40);
			path.Warp (pts, r, null);
			Assert.AreEqual (3, path.PointCount, "Count");

			pts = path.PathPoints;
			Assert.AreEqual (1.131355e-39, pts[0].X, 1e40, "0.X");
			Assert.AreEqual (-2.0240637E-33, pts[0].Y, 1e40, "0.Y");
			Assert.AreEqual (1.070131E-39, pts[1].X, 1e40, "1.X");
			Assert.AreEqual (-2.02406389E-33, pts[1].Y, 1e40, "1.Y");
			Assert.AreEqual (3.669146E-40, pts[2].X, 1e40, "2.X");
			Assert.AreEqual (-6.746879E-34, pts[2].Y, 1e40, "2.Y");
			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (129, types[2], "2");
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_Matrix_NonInvertible ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "!IsInvertible");
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, matrix);

			Assert.AreEqual (3, path.PointCount, "Count");
			pts = path.PathPoints;
			Assert.AreEqual (47, pts[0].X, "0.X");
			Assert.AreEqual (30, pts[0].Y, "0.Y");
			Assert.AreEqual (47, pts[1].X, "1.X");
			Assert.AreEqual (30, pts[1].Y, "1.Y");
			Assert.AreEqual (47, pts[2].X, "2.X");
			Assert.AreEqual (30, pts[2].Y, "2.Y");
			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (129, types[2], "2");
		}

		[Test]
		[Category ("NotWorking")]
		public void Warp_Bilinear ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix (), WarpMode.Bilinear);
			// note that the last point is no more closed
			CheckWrapNaN (path, false);
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_Perspective ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix (), WarpMode.Perspective);
			CheckWrap (path);
		}

		[Test]
		public void Warp_Invalid ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix (), (WarpMode) Int32.MinValue);
			Assert.AreEqual (0, path.PointCount, "Count");
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_Flatness_Negative ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix (), WarpMode.Perspective, -1f);
			CheckWrap (path);
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Warp_Flatness_OverOne ()
		{
			PointF[] pts = new PointF[1] { new PointF (0, 0) };
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			RectangleF r = new RectangleF (10, 20, 30, 40);
			path.Warp (pts, r, new Matrix (), WarpMode.Perspective, 2.0f);
			CheckWrap (path);
		}

		[Test]
		public void SetMarkers_EmptyPath ()
		{
			new GraphicsPath ().SetMarkers ();
		}

		[Test]
		public void ClearMarkers_EmptyPath ()
		{
			new GraphicsPath ().ClearMarkers ();
		}

		[Test]
		public void CloseFigure_EmptyPath ()
		{
			new GraphicsPath ().CloseFigure ();
		}

		[Test]
		public void CloseAllFigures_EmptyPath ()
		{
			new GraphicsPath ().CloseAllFigures ();
		}

		[Test]
		public void StartClose_AddArc ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddArc (10, 10, 100, 100, 90, 180);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Arc");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Arc");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddBezier ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddBezier (10, 10, 100, 100, 20, 20, 200, 200);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Bezier");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Bezier");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddBeziers ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddBeziers (new Point[7] { new Point (10, 10), 
				new Point (20, 10), new Point (20, 20), new Point (30, 20),
				new Point (40, 40), new Point (50, 40), new Point (50, 50)
			});
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Bezier");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Bezier");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddClosedCurve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddClosedCurve (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/ClosedCurve");
			// check last types
			Assert.AreEqual (131, types[path.PointCount - 3], "end/ClosedCurve");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line3");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line3");
		}

		[Test]
		public void StartClose_AddCurve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddCurve (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Curve");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Curve");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddEllipse ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddEllipse (10, 10, 100, 100);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Ellipse");
			// check last types
			Assert.AreEqual (131, types[path.PointCount - 3], "end/Ellipse");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line3");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line3");
		}

		[Test]
		public void StartClose_AddLine ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddLine (5, 5, 10, 10);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Line2");
			// check last types
			Assert.AreEqual (1, types[path.PointCount - 3], "end/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddLines ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddLines (new Point[4] { new Point (10, 10), new Point (20, 10), new Point (20, 20), new Point (30, 20) });
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Lines");
			// check last types
			Assert.AreEqual (1, types[path.PointCount - 3], "end/Lines");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddPath_Connect ()
		{
			GraphicsPath inner = new GraphicsPath ();
			inner.AddArc (10, 10, 100, 100, 90, 180);
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddPath (inner, true);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (1, types[2], "start/Path");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Path");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddPath_NoConnect ()
		{
			GraphicsPath inner = new GraphicsPath ();
			inner.AddArc (10, 10, 100, 100, 90, 180);
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddPath (inner, false);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Path");
			// check last types
			Assert.AreEqual (3, types[path.PointCount - 3], "end/Path");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line");
		}

		[Test]
		public void StartClose_AddPie ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddPie (10, 10, 10, 10, 90, 180);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Pie");
			// check last types
			// libgdiplus draws pie by ending with a line (not a curve) section
			Assert.IsTrue ((types[path.PointCount - 3] & 128) == 128, "end/Pie");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line2");
		}

		[Test]
		public void StartClose_AddPolygon ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddPolygon (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Polygon");
			// check last types
			Assert.AreEqual (129, types[path.PointCount - 3], "end/Polygon");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line2");
		}

		[Test]
		public void StartClose_AddRectangle ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddRectangle (new RectangleF (10, 10, 20, 20));
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Rectangle");
			// check last types
			Assert.AreEqual (129, types[path.PointCount - 3], "end/Rectangle");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line2");
		}

		[Test]
		public void StartClose_AddRectangles ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddRectangles (new RectangleF[2] {
				new RectangleF (10, 10, 20, 20),
				new RectangleF (20, 20, 10, 10) });
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/Rectangles");
			// check last types
			Assert.AreEqual (129, types[path.PointCount - 3], "end/Rectangles");
			Assert.AreEqual (0, types[path.PointCount - 2], "start/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line2");
		}

		[Test]
		[Category ("NotWorking")]
		public void StartClose_AddString ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (1, 1, 2, 2);
			path.AddString ("mono", FontFamily.GenericMonospace, 0, 10, new Point (20,20), StringFormat.GenericDefault);
			path.AddLine (10, 10, 20, 20);
			byte[] types = path.PathTypes;
			// check first types
			Assert.AreEqual (0, types[0], "start/Line");
			Assert.AreEqual (0, types[2], "start/String");
			// check last types
			Assert.AreEqual (163, types[path.PointCount - 3], "end/String");
			Assert.AreEqual (1, types[path.PointCount - 2], "start/Line2");
			Assert.AreEqual (1, types[path.PointCount - 1], "end/Line2");
		}

		[Test]
		public void Widen_Pen_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().Widen (null));
		}

		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen ()
		{
			Pen pen = new Pen (Color.Blue);
			GraphicsPath path = new GraphicsPath ();
			path.AddRectangle (new Rectangle (1, 1, 2, 2));
			Assert.AreEqual (4, path.PointCount, "Count-1");
			path.Widen (pen);
			Assert.AreEqual (12, path.PointCount, "Count-2");

			PointF[] pts = path.PathPoints;
			Assert.AreEqual (0.5, pts[0].X, 0.25, "0.X");
			Assert.AreEqual (0.5, pts[0].Y, 0.25, "0.Y");
			Assert.AreEqual (3.5, pts[1].X, 0.25, "1.X");
			Assert.AreEqual (0.5, pts[1].Y, 0.25, "1.Y");
			Assert.AreEqual (3.5, pts[2].X, 0.25, "2.X");
			Assert.AreEqual (3.5, pts[2].Y, 0.25, "2.Y");
			Assert.AreEqual (0.5, pts[3].X, 0.25, "3.X");
			Assert.AreEqual (3.5, pts[3].Y, 0.25, "3.Y");
			Assert.AreEqual (1.5, pts[4].X, 0.25, "4.X");
			Assert.AreEqual (3.0, pts[4].Y, 0.25, "4.Y");
			Assert.AreEqual (1.0, pts[5].X, 0.25, "5.X");
			Assert.AreEqual (2.5, pts[5].Y, 0.25, "5.Y");
			Assert.AreEqual (3.0, pts[6].X, 0.25, "6.X");
			Assert.AreEqual (2.5, pts[6].Y, 0.25, "6.Y");
			Assert.AreEqual (2.5, pts[7].X, 0.25, "7.X");
			Assert.AreEqual (3.0, pts[7].Y, 0.25, "7.Y");
			Assert.AreEqual (2.5, pts[8].X, 0.25, "8.X");
			Assert.AreEqual (1.0, pts[8].Y, 0.25, "8.Y");
			Assert.AreEqual (3.0, pts[9].X, 0.25, "9.X");
			Assert.AreEqual (1.5, pts[9].Y, 0.25, "9.Y");
			Assert.AreEqual (1.0, pts[10].X, 0.25, "10.X");
			Assert.AreEqual (1.5, pts[10].Y, 0.25, "10.Y");
			Assert.AreEqual (1.5, pts[11].X, 0.25, "11.X");
			Assert.AreEqual (1.0, pts[11].Y, 0.25, "11.Y");

			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (1, types[2], "2");
			Assert.AreEqual (129, types[3], "3");
			Assert.AreEqual (0, types[4], "4");
			Assert.AreEqual (1, types[5], "5");
			Assert.AreEqual (1, types[6], "6");
			Assert.AreEqual (1, types[7], "7");
			Assert.AreEqual (1, types[8], "8");
			Assert.AreEqual (1, types[9], "9");
			Assert.AreEqual (1, types[10], "10");
			Assert.AreEqual (129, types[11], "11");
		}

		[Test]
		public void Widen_Pen_Null_Matrix ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().Widen (null, new Matrix ()));
		}

		[Test]
		public void Widen_NoPoint ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Assert.AreEqual (0, gp.PointCount, "PointCount-1");
				Pen pen = new Pen (Color.Blue);
				gp.Widen (pen);
				Assert.AreEqual (0, gp.PointCount, "PointCount-2");
			}
		}

		[Test]
		public void Widen_SinglePoint ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (new Point[1] { new Point (1, 1) });
				// Special case - a line with a single point is valid
				Assert.AreEqual (1, gp.PointCount, "PointCount");
				Assert.Throws<OutOfMemoryException> (() => gp.Widen (Pens.Red));
				// oops ;-)
			}
		}

		private void CheckWiden3 (GraphicsPath path)
		{
			PointF[] pts = path.PathPoints;
			Assert.AreEqual (4.2, pts[0].X, 0.25, "0.X");
			Assert.AreEqual (4.5, pts[0].Y, 0.25, "0.Y");
			Assert.AreEqual (15.8, pts[1].X, 0.25, "1.X");
			Assert.AreEqual (4.5, pts[1].Y, 0.25, "1.Y");
			Assert.AreEqual (10.0, pts[2].X, 0.25, "2.X");
			Assert.AreEqual (16.1, pts[2].Y, 0.25, "2.Y");
			Assert.AreEqual (10.4, pts[3].X, 0.25, "3.X");
			Assert.AreEqual (14.8, pts[3].Y, 0.25, "3.Y");
			Assert.AreEqual (9.6, pts[4].X, 0.25, "4.X");
			Assert.AreEqual (14.8, pts[4].Y, 0.25, "4.Y");
			Assert.AreEqual (14.6, pts[5].X, 0.25, "7.X");
			Assert.AreEqual (4.8, pts[5].Y, 0.25, "7.Y");
			Assert.AreEqual (15.0, pts[6].X, 0.25, "5.X");
			Assert.AreEqual (5.5, pts[6].Y, 0.25, "5.Y");
			Assert.AreEqual (5.0, pts[7].X, 0.25, "6.X");
			Assert.AreEqual (5.5, pts[7].Y, 0.25, "6.Y");
			Assert.AreEqual (5.4, pts[8].X, 0.25, "8.X");
			Assert.AreEqual (4.8, pts[8].Y, 0.25, "8.Y");

			byte[] types = path.PathTypes;
			Assert.AreEqual (0, types[0], "0");
			Assert.AreEqual (1, types[1], "1");
			Assert.AreEqual (129, types[2], "2");
			Assert.AreEqual (0, types[3], "3");
			Assert.AreEqual (1, types[4], "4");
			Assert.AreEqual (1, types[5], "5");
			Assert.AreEqual (1, types[6], "6");
			Assert.AreEqual (1, types[7], "7");
			Assert.AreEqual (129, types[8], "8");
		}

		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen_Matrix_Null ()
		{
			Pen pen = new Pen (Color.Blue);
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			path.Widen (pen, null);
			Assert.AreEqual (9, path.PointCount, "Count");
			CheckWiden3 (path);
		}

		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen_Matrix_Empty ()
		{
			Pen pen = new Pen (Color.Blue);
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[3] { new Point (5, 5), new Point (15, 5), new Point (10, 15) });
			path.Widen (pen, new Matrix ());
			Assert.AreEqual (9, path.PointCount, "Count");
			CheckWiden3 (path);
		}

		[Test]
		[Ignore ("results aren't always constant and differs from 1.x and 2.0")]
		public void Widen_Pen_Matrix_NonInvertible ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.IsFalse (matrix.IsInvertible, "!IsInvertible");
			GraphicsPath path = new GraphicsPath ();
			path.Widen (new Pen (Color.Blue), matrix);
			Assert.AreEqual (0, path.PointCount, "Points");
		}

		private void CheckWidenedBounds (string message, GraphicsPath gp, Matrix m)
		{
			RectangleF bounds = gp.GetBounds (m);
			Assert.AreEqual (0.5f, bounds.X, 0.00001f, message + ".Bounds.X");
			Assert.AreEqual (0.5f, bounds.Y, 0.00001f, message + ".Bounds.Y");
			Assert.AreEqual (3.0f, bounds.Width, 0.00001f, message + ".Bounds.Width");
			Assert.AreEqual (3.0f, bounds.Height, 0.00001f, message + ".Bounds.Height");
		}

		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen_SmallWidth ()
		{
			Matrix m = new Matrix ();
			Rectangle rect = new Rectangle (1, 1, 2, 2);

			// pen's smaller than 1.0 (width) are "promoted" to 1
			Pen p = new Pen (Color.Aqua, 0);
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (rect);
			gp.Widen (p);
			CheckWidenedBounds ("Width == 0, Null matrix", gp, null);
			CheckWidenedBounds ("Width == 0, Empty matrix", gp, m);

			p.Width = 0.5f;
			gp = new GraphicsPath ();
			gp.AddRectangle (rect);
			gp.Widen (p);
			CheckWidenedBounds ("Width == 0.5, Null matrix", gp, null);
			CheckWidenedBounds ("Width == 0.5, Empty matrix", gp, m);

			p.Width = 1.0f;
			gp = new GraphicsPath ();
			gp.AddRectangle (rect);
			gp.Widen (p);
			CheckWidenedBounds ("Width == 1.0, Null matrix", gp, null);
			CheckWidenedBounds ("Width == 1.0, Empty matrix", gp, m);

			p.Width = 1.1f;
			gp = new GraphicsPath ();
			gp.AddRectangle (rect);
			gp.Widen (p);
			RectangleF bounds = gp.GetBounds (m);
			Assert.AreEqual (0.45f, bounds.X, 0.00001f, "1.1.Bounds.X");
			Assert.AreEqual (0.45f, bounds.Y, 0.00001f, "1.1.Bounds.Y");
			Assert.AreEqual (3.10f, bounds.Width, 0.00001f, "1.1.Bounds.Width");
			Assert.AreEqual (3.10f, bounds.Height, 0.00001f, "1.1.Bounds.Height");
		}

		[Test]
		public void IsOutlineVisible_IntNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().IsOutlineVisible (1, 1, null));
		}

		[Test]
		public void IsOutlineVisible_FloatNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().IsOutlineVisible (1.0f, 1.0f, null));
		}

		[Test]
		public void IsOutlineVisible_PointNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().IsOutlineVisible (new Point (), null));
		}

		[Test]
		public void IsOutlineVisible_PointFNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new GraphicsPath ().IsOutlineVisible (new PointF (), null));
		}

		private void IsOutlineVisible_Line (Graphics graphics)
		{
			Pen p2 = new Pen (Color.Red, 3.0f);
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (10, 1, 14, 1);
				Assert.IsTrue (gp.IsOutlineVisible (10, 1, Pens.Red, graphics), "Int1");
				Assert.IsTrue (gp.IsOutlineVisible (10, 2, p2, graphics), "Int2");
				Assert.IsFalse (gp.IsOutlineVisible (10, 2, Pens.Red, graphics), "Int3");

				Assert.IsTrue (gp.IsOutlineVisible (11.0f, 1.0f, Pens.Red, graphics), "Float1");
				Assert.IsTrue (gp.IsOutlineVisible (11.0f, 1.0f, p2, graphics), "Float2");
				Assert.IsFalse (gp.IsOutlineVisible (11.0f, 2.0f, Pens.Red, graphics), "Float3");

				Point pt = new Point (12, 2);
				Assert.IsFalse (gp.IsOutlineVisible (pt, Pens.Red, graphics), "Point1");
				Assert.IsTrue (gp.IsOutlineVisible (pt, p2, graphics), "Point2");
				pt.Y = 1;
				Assert.IsTrue (gp.IsOutlineVisible (pt, Pens.Red, graphics), "Point3");

				PointF pf = new PointF (13.0f, 2.0f);
				Assert.IsFalse (gp.IsOutlineVisible (pf, Pens.Red, graphics), "PointF1");
				Assert.IsTrue (gp.IsOutlineVisible (pf, p2, graphics), "PointF2");
				pf.Y = 1;
				Assert.IsTrue (gp.IsOutlineVisible (pf, Pens.Red, graphics), "PointF3");
			}
			p2.Dispose ();
		}

		[Test]
		public void IsOutlineVisible_Line_WithoutGraphics ()
		{
			IsOutlineVisible_Line (null);
		}

		[Test]
		public void IsOutlineVisible_Line_WithGraphics_Inside ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					IsOutlineVisible_Line (g);
				}
			}
		}

		[Test]
		public void IsOutlineVisible_Line_WithGraphics_Outside ()
		{
			using (Bitmap bitmap = new Bitmap (5, 5)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					IsOutlineVisible_Line (g);
				}
				// graphics "seems" ignored as the line is outside the bitmap!
			}
		}

		// docs ways the point is in world coordinates and that the graphics transform 
		// should be applied

		[Test]
		public void IsOutlineVisible_Line_WithGraphics_Transform ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.Transform = new Matrix (2, 0, 0, 2, 50, -50);
					IsOutlineVisible_Line (g);
				}
				// graphics still "seems" ignored (Transform)
			}
		}

		[Test]
		public void IsOutlineVisible_Line_WithGraphics_PageUnit ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.PageUnit = GraphicsUnit.Millimeter;
					IsOutlineVisible_Line (g);
				}
				// graphics still "seems" ignored (PageUnit)
			}
		}

		[Test]
		public void IsOutlineVisible_Line_WithGraphics_PageScale ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.PageScale = 2.0f;
					IsOutlineVisible_Line (g);
				}
				// graphics still "seems" ignored (PageScale)
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void IsOutlineVisible_Line_WithGraphics ()
		{
			using (Bitmap bitmap = new Bitmap (20, 20)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					g.Transform = new Matrix (2, 0, 0, 2, 50, -50);
					g.PageUnit = GraphicsUnit.Millimeter;
					g.PageScale = 2.0f;
					using (GraphicsPath gp = new GraphicsPath ()) {
						gp.AddLine (10, 1, 14, 1);
						Assert.IsFalse (gp.IsOutlineVisible (10, 1, Pens.Red, g), "Int1");
					}
				}
				// graphics ISN'T ignored (Transform+PageUnit+PageScale)
			}
		}

		[Test]
		[Category ("NotWorking")] // looks buggy - reported to MS as FDBK50868
		public void IsOutlineVisible_Line_End ()
		{
			// horizontal line
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (10, 1, 14, 1);
				Assert.IsFalse (gp.IsOutlineVisible (14, 1, Pens.Red, null), "Int1h");
				Assert.IsFalse (gp.IsOutlineVisible (13.5f, 1.0f, Pens.Red, null), "Float1h");
				Assert.IsTrue (gp.IsOutlineVisible (13.4f, 1.0f, Pens.Red, null), "Float2h");
				Assert.IsFalse (gp.IsOutlineVisible (new Point (14, 1), Pens.Red, null), "Point1h");
				Assert.IsFalse (gp.IsOutlineVisible (new PointF (13.5f, 1.0f), Pens.Red, null), "PointF1h");
				Assert.IsTrue (gp.IsOutlineVisible (new PointF (13.49f, 1.0f), Pens.Red, null), "PointF2h");
			}
			// vertical line
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (1, 10, 1, 14);
				Assert.IsFalse (gp.IsOutlineVisible (1, 14, Pens.Red, null), "Int1v");
				Assert.IsFalse (gp.IsOutlineVisible (1.0f, 13.5f, Pens.Red, null), "Float1v");
				Assert.IsTrue (gp.IsOutlineVisible (1.0f, 13.4f, Pens.Red, null), "Float2v");
				Assert.IsFalse (gp.IsOutlineVisible (new Point (1, 14), Pens.Red, null), "Point1v");
				Assert.IsFalse (gp.IsOutlineVisible (new PointF (1.0f, 13.5f), Pens.Red, null), "PointF1v");
				Assert.IsTrue (gp.IsOutlineVisible (new PointF (1.0f, 13.49f), Pens.Red, null), "PointF2v");
			}
		}

		private void IsOutlineVisible_Rectangle (Graphics graphics)
		{
			Pen p2 = new Pen (Color.Red, 3.0f);
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddRectangle (new Rectangle (10, 10, 20, 20));
				Assert.IsTrue (gp.IsOutlineVisible (10, 10, Pens.Red, graphics), "Int1");
				Assert.IsTrue (gp.IsOutlineVisible (10, 11, p2, graphics), "Int2");
				Assert.IsFalse (gp.IsOutlineVisible (11, 11, Pens.Red, graphics), "Int3");

				Assert.IsTrue (gp.IsOutlineVisible (11.0f, 10.0f, Pens.Red, graphics), "Float1");
				Assert.IsTrue (gp.IsOutlineVisible (11.0f, 11.0f, p2, graphics), "Float2");
				Assert.IsFalse (gp.IsOutlineVisible (11.0f, 11.0f, Pens.Red, graphics), "Float3");

				Point pt = new Point (15, 10);
				Assert.IsTrue (gp.IsOutlineVisible (pt, Pens.Red, graphics), "Point1");
				Assert.IsTrue (gp.IsOutlineVisible (pt, p2, graphics), "Point2");
				pt.Y = 15;
				Assert.IsFalse (gp.IsOutlineVisible (pt, Pens.Red, graphics), "Point3");

				PointF pf = new PointF (29.0f, 29.0f);
				Assert.IsFalse (gp.IsOutlineVisible (pf, Pens.Red, graphics), "PointF1");
				Assert.IsTrue (gp.IsOutlineVisible (pf, p2, graphics), "PointF2");
				pf.Y = 31.0f;
				Assert.IsTrue (gp.IsOutlineVisible (pf, p2, graphics), "PointF3");
			}
			p2.Dispose ();
		}

		[Test]
		public void IsOutlineVisible_Rectangle_WithoutGraphics ()
		{
			IsOutlineVisible_Rectangle (null);
		}

		private void IsVisible_Rectangle (Graphics graphics)
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddRectangle (new Rectangle (10, 10, 20, 20));
				Assert.IsFalse (gp.IsVisible (9, 9, graphics), "Int0");
				Assert.IsTrue (gp.IsVisible (10, 10, graphics), "Int1");
				Assert.IsTrue (gp.IsVisible (20, 20, graphics), "Int2");
				Assert.IsTrue (gp.IsVisible (29, 29, graphics), "Int3");
				Assert.IsFalse (gp.IsVisible (30, 29, graphics), "Int4");
				Assert.IsFalse (gp.IsVisible (29, 30, graphics), "Int5");
				Assert.IsFalse (gp.IsVisible (30, 30, graphics), "Int6");

				Assert.IsFalse (gp.IsVisible (9.4f, 9.4f, graphics), "Float0");
				Assert.IsTrue (gp.IsVisible (9.5f, 9.5f, graphics), "Float1");
				Assert.IsTrue (gp.IsVisible (10f, 10f, graphics), "Float2");
				Assert.IsTrue (gp.IsVisible (20f, 20f, graphics), "Float3");
				// the next diff is too close, so this fails with libgdiplus/cairo
				//Assert.IsTrue (gp.IsVisible (29.4f, 29.4f, graphics), "Float4");
				Assert.IsFalse (gp.IsVisible (29.5f, 29.5f, graphics), "Float5");
				Assert.IsFalse (gp.IsVisible (29.5f, 29.4f, graphics), "Float6");
				Assert.IsFalse (gp.IsVisible (29.4f, 29.5f, graphics), "Float7");
			}
		}

		[Test]
		public void IsVisible_Rectangle_WithoutGraphics ()
		{
			IsVisible_Rectangle (null);
		}

		[Test]
		public void IsVisible_Rectangle_WithGraphics ()
		{
			using (Bitmap bitmap = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					IsVisible_Rectangle (g);
				}
			}
		}

		// bug #325502 has shown that ellipse didn't work with earlier code
		private void IsVisible_Ellipse (Graphics graphics)
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddEllipse (new Rectangle (10, 10, 20, 20));
				Assert.IsFalse (gp.IsVisible (10, 10, graphics), "Int1");
				Assert.IsTrue (gp.IsVisible (20, 20, graphics), "Int2");
				Assert.IsFalse (gp.IsVisible (29, 29, graphics), "Int3");

				Assert.IsFalse (gp.IsVisible (10f, 10f, graphics), "Float2");
				Assert.IsTrue (gp.IsVisible (20f, 20f, graphics), "Float3");
				Assert.IsFalse (gp.IsVisible (29.4f, 29.4f, graphics), "Float4");
			}
		}

		[Test]
		public void IsVisible_Ellipse_WithoutGraphics ()
		{
			IsVisible_Ellipse (null);
		}

		[Test]
		public void IsVisible_Ellipse_WithGraphics ()
		{
			using (Bitmap bitmap = new Bitmap (40, 40)) {
				using (Graphics g = Graphics.FromImage (bitmap)) {
					IsVisible_Ellipse (g);
				}
			}
		}

		// Reverse simple test cases

		private void Reverse (GraphicsPath gp)
		{
			PointF[] bp = gp.PathPoints;
			byte[] bt = gp.PathTypes;

			gp.Reverse ();
			PointF[] ap = gp.PathPoints;
			byte[] at = gp.PathTypes;

			int count = gp.PointCount;
			Assert.AreEqual (bp.Length, count, "PointCount");
			for (int i = 0; i < count; i++) {
				Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
				Assert.AreEqual (bt[i], at[i], "Type" + i.ToString ());
			}
		}

		[Test]
		public void Reverse_Arc ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddArc (1f, 1f, 2f, 2f, Pi4, Pi4);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Bezier ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddBezier (1, 2, 3, 4, 5, 6, 7, 8);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Beziers ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Point[] beziers = new Point[] { new Point (1,2), new Point (3,4), new Point (5,6),
					new Point (7,8), new Point (9,10), new Point (11,12), new Point (13,14) };
				gp.AddBeziers (beziers);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_ClosedCurve ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Point[] beziers = new Point[] { new Point (1,2), new Point (3,4), new Point (5,6),
					new Point (7,8), new Point (9,10), new Point (11,12), new Point (13,14) };
				gp.AddClosedCurve (beziers);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Curve ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Point[] beziers = new Point[] { new Point (1,2), new Point (3,4), new Point (5,6),
					new Point (7,8), new Point (9,10), new Point (11,12), new Point (13,14) };
				gp.AddCurve (beziers);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Ellipse ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddEllipse (1, 2, 3, 4);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Line ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (1, 2, 3, 4);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Line_Closed ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (1, 2, 3, 4);
				gp.CloseFigure ();
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Lines ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Point[] points = new Point[] { new Point (1,2), new Point (3,4), new Point (5,6),
					new Point (7,8), new Point (9,10), new Point (11,12), new Point (13,14) };
				gp.AddLines (points);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Polygon ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Point[] points = new Point[] { new Point (1,2), new Point (3,4), new Point (5,6),
					new Point (7,8), new Point (9,10), new Point (11,12), new Point (13,14) };
				gp.AddPolygon (points);
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Rectangle ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddRectangle (new Rectangle (1,2,3,4));
				Reverse (gp);
			}
		}

		[Test]
		public void Reverse_Rectangles ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Rectangle[] rects = new Rectangle[] { new Rectangle (1, 2, 3, 4), new Rectangle (5, 6, 7, 8) }; 
				gp.AddRectangles (rects);
				Reverse (gp);
			}
		}

		// Reverse complex test cases

		[Test]
		[Category ("NotWorking")] // the output differs from GDI+ and libgdiplus
		public void Reverse_Pie ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddPie (1, 2, 3, 4, 10, 20);
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 3, 3, 3, 129 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;
				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void Reverse_Path ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				GraphicsPath path = new GraphicsPath ();
				path.AddArc (1f, 1f, 2f, 2f, Pi4, Pi4);
				path.AddLine (1, 2, 3, 4);
				gp.AddPath (path, true);
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 1, 1, 3, 3, 3 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void Reverse_Path_2 ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddEllipse (50, 51, 50, 100);
				gp.AddRectangle (new Rectangle (200, 201, 60, 61));
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 1, 1, 129, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 131 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		[Category ("NotWorking")] // the output differs from GDI+ and libgdiplus
		public void Reverse_String ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				FontFamily ff = GetFontFamily ();
				gp.AddString ("Mono::", ff, 0, 10, new Point (10, 10), StringFormat.GenericDefault);
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,129,0,3,3,3,
					3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,161,0,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,
					3,3,3,3,3,3,3,129,0,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,161,0,3,3,3,3,3,
					3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,131,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,
					163,0,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,
					1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,1,1,
					3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,161,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,131,
					0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,163,0,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,
					3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,
					1,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,3,1,1,1,1,129 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void Reverse_Marker ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddRectangle (new Rectangle (200, 201, 60, 61));
				gp.SetMarkers ();
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 1, 1, 129 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void Reverse_Subpath_Marker ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (0, 1, 2, 3);
				gp.SetMarkers ();
				gp.CloseFigure ();
				gp.AddBezier (5, 6, 7, 8, 9, 10, 11, 12);
				gp.CloseFigure ();
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 3, 3, 163, 0, 129 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void Reverse_Subpath_Marker_2 ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLine (0, 1, 2, 3);
				gp.SetMarkers ();
				gp.StartFigure ();
				gp.AddLine (20, 21, 22, 23);
				gp.AddBezier (5, 6, 7, 8, 9, 10, 11, 12);
				PointF[] bp = gp.PathPoints;
				byte[] expected = new byte[] { 0, 3, 3, 3, 1, 33, 0, 1 };

				gp.Reverse ();
				PointF[] ap = gp.PathPoints;
				byte[] at = gp.PathTypes;

				int count = gp.PointCount;
				Assert.AreEqual (bp.Length, count, "PointCount");
				for (int i = 0; i < count; i++) {
					Assert.AreEqual (bp[i], ap[count - i - 1], "Point" + i.ToString ());
					Assert.AreEqual (expected[i], at[i], "Type" + i.ToString ());
				}
			}
		}

		[Test]
		public void bug413461 ()
		{
			int dX = 520;
			int dY = 320;
			Point[] expected_points = new Point [] {
				new Point(dX-64, dY-24),//start
				new Point(dX-59, dY-34),//focal point 1
				new Point(dX-52, dY-54),//focal point 2
				new Point(dX-18, dY-66),//top
				new Point(dX-34, dY-47),//focal point 1
				new Point(dX-43, dY-27),//focal point 2
				new Point(dX-44, dY-8),//end
				};
			byte[] expected_types = new byte [] {
				(byte)PathPointType.Start,
				(byte)PathPointType.Bezier,
				(byte)PathPointType.Bezier,
				(byte)PathPointType.Bezier,
				(byte)PathPointType.Bezier,
				(byte)PathPointType.Bezier,
				(byte)PathPointType.Bezier };
			using (GraphicsPath path = new GraphicsPath (expected_points, expected_types)) {
				Assert.AreEqual (7, path.PointCount, "PathCount");
				byte [] actual_types = path.PathTypes;
				Assert.AreEqual (expected_types [0], actual_types [0], "types-0");
				Assert.AreEqual (expected_types [1], actual_types [1], "types-1");
				Assert.AreEqual (expected_types [2], actual_types [2], "types-2");
				Assert.AreEqual (expected_types [3], actual_types [3], "types-3");
				Assert.AreEqual (expected_types [4], actual_types [4], "types-4");
				Assert.AreEqual (expected_types [5], actual_types [5], "types-5");
				// path is filled like closed but this does not show on the type
				Assert.AreEqual (expected_types [6], actual_types [6], "types-6");
			}
		}
	}
}
