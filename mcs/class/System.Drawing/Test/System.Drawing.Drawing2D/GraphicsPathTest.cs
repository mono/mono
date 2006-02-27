//
// System.Drawing.GraphicsPath unit tests
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Point_Null_Byte ()
		{
			new GraphicsPath ((Point[]) null, new byte[1]);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Point_Byte_Null ()
		{
			new GraphicsPath (new Point[1], null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Point_Byte_LengthMismatch ()
		{
			new GraphicsPath (new Point[1], new byte [2]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_PointF_Null_Byte ()
		{
			new GraphicsPath ((PointF[])null, new byte [1]);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_PointF_Byte_Null ()
		{
			new GraphicsPath ( new PointF[1], null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_PointF_Byte_LengthMismatch ()
		{
			new GraphicsPath (new PointF[2], new byte [1]);
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
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GraphicsPath_Empty_PathPoints ()
		{
			Assert.IsNull (new GraphicsPath ().PathPoints);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GraphicsPath_Empty_PathTypes ()
		{
			Assert.IsNull (new GraphicsPath ().PathTypes);
		}

		[Test]
		[ExpectedException (typeof (SC.InvalidEnumArgumentException))]
		public void FillMode_Invalid ()
		{
			// constructor accept an invalid FillMode
			GraphicsPath gp = new GraphicsPath ((FillMode) Int32.MaxValue);
			Assert.AreEqual (Int32.MaxValue, (int) gp.FillMode, "MaxValue");
			// but you can't set the FillMode property to an invalid value ;-)
			gp.FillMode = (FillMode) Int32.MaxValue;
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
			Assert.AreEqual (2.999624f, rect.X, "Bounds.X");
			Assert.AreEqual (2.013707f, rect.Y, "Bounds.Y");
			Assert.AreEqual (0f, rect.Width, Delta, "Bounds.Width");
			Assert.AreEqual (0.01370478f, rect.Height, "Bounds.Height");

			Assert.AreEqual (2.999906f, path.PathData.Points[0].X, "Points[0].X");
			Assert.AreEqual (2.013707f, path.PathPoints[0].Y, "Points[0].Y");
			Assert.AreEqual (0, path.PathData.Types[0], "Types[0]");
			Assert.AreEqual (2.999843f, path.PathData.Points[1].X, "Points[1].X");
			Assert.AreEqual (2.018276f, path.PathPoints[1].Y, "Points[1].Y");
			Assert.AreEqual (3, path.PathTypes[1], "Types[1]");
			Assert.AreEqual (2.99974918f, path.PathData.Points[2].X, "Points[2].X");
			Assert.AreEqual (2.02284455f, path.PathPoints[2].Y, "Points[2].Y");
			Assert.AreEqual (3, path.PathData.Types[2], "Types[2]");
			Assert.AreEqual (2.999624f, path.PathData.Points[3].X, "Points[3].X");
			Assert.AreEqual (2.027412f, path.PathPoints[3].Y, "Points[3].Y");
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddBeziers_Point_Null ()
		{
			new GraphicsPath ().AddBeziers ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddBeziers_3_Points ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
		}

		[Test]
		public void AddBeziers_Point ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new Point[4] { new Point (1, 1), new Point (2, 2), new Point (3, 3), new Point (4, 4) });
			CheckBezier (gp);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddBeziers_PointF_Null ()
		{
			new GraphicsPath ().AddBeziers ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddBeziers_3_PointFs ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) });
		}

		[Test]
		public void AddBeziers_PointF ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddBeziers (new PointF[4] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f), new PointF (4f, 4f) });
			CheckBezier (gp);
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
			Assert.AreEqual (2, gp.PointCount, "PointCount");
			Assert.AreEqual (0, gp.PathTypes[0], "PathTypes[0]");
			Assert.AreEqual (1, gp.PathTypes[1], "PathTypes[1]");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddLines_Point_Null ()
		{
			new GraphicsPath ().AddLines ((Point[])null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddLines_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new Point[0]);
			CheckLine (gp);
		}

		[Test]
		[Category ("NotWorking")]
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddLines_PointF_Null ()
		{
			new GraphicsPath ().AddLines ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddLines_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddLines (new PointF[0]);
			CheckLine (gp);
		}

		[Test]
		[Category ("NotWorking")]
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPolygon_Point_Null ()
		{
			new GraphicsPath ().AddPolygon ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_Point_Empty ()
		{
			new GraphicsPath ().AddPolygon (new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new Point[1] { new Point (1, 1) });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new Point[2] { new Point (1, 1), new Point (2, 2) });
		}

		[Test]
		public void AddPolygon_Point_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			CheckPolygon (gp);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPolygon_PointF_Null ()
		{
			new GraphicsPath ().AddPolygon ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_PointF_Empty ()
		{
			new GraphicsPath ().AddPolygon (new PointF[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new PointF[1] { new PointF (1f, 1f) });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddPolygon_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) });
		}

		[Test]
		public void AddPolygon_PointF_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddPolygon (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) });
			CheckPolygon (gp);
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRectangles_Int_Null ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles ((Rectangle[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddRectangles_Int_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new Rectangle[0]);
			CheckRectangle (gp, 4);
		}

		[Test]
		public void AddRectangles_Int ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles (new Rectangle[1] { new Rectangle (1, 1, 2, 2) });
			CheckRectangle (gp, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRectangles_Float_Null ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles ((RectangleF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddRectangles_Float_Empty ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangles ( new RectangleF[0]);
			CheckRectangle (gp, 4);
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPath_Null ()
		{
			new GraphicsPath ().AddPath (null, false);
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

			// GetBounds (well GdipGetPathWorldBounds) isn't implemented
			RectangleF rect = path.GetBounds ();
			Assert.AreEqual (0.8333333f, rect.X, "Bounds.X");
			Assert.AreEqual (0.8333333f, rect.Y, "Bounds.Y");
			Assert.AreEqual (2.33333278f, rect.Width, "Bounds.Width");
			Assert.AreEqual (2.33333278f, rect.Height, "Bounds.Height");

			Assert.AreEqual (0, path.PathData.Types[0], "PathData.Types[0]");
			for (int i = 1; i < 9; i++)
				Assert.AreEqual (3, path.PathTypes[i], "PathTypes" + i.ToString ());
			Assert.AreEqual (131, path.PathData.Types[9], "PathData.Types[9]");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddClosedCurve_Point_Null ()
		{
			new GraphicsPath ().AddClosedCurve ((Point[])null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point[1] { new Point (1, 1) });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point[2] { new Point (1, 1), new Point (2, 2) });
		}

		[Test]
		public void AddClosedCurve_Point_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new Point[3] { new Point (1, 1), new Point (2, 2), new Point (3, 3) });
			CheckClosedCurve (gp);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddClosedCurve_PointF_Null ()
		{
			new GraphicsPath ().AddClosedCurve ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new PointF[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new PointF[1] { new PointF (1f, 1f) });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddClosedCurve_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) });
		}

		[Test]
		public void AddClosedCurve_PointF_3 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddClosedCurve (new PointF[3] { new PointF (1f, 1f), new PointF (2f, 2f), new PointF (3f, 3f) });
			CheckClosedCurve (gp);
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddCurve_Point_Null ()
		{
			new GraphicsPath ().AddCurve ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddCurve_Point_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddCurve_Point_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new Point[1] { new Point (1, 1) });
		}

		[Test]
		public void AddCurve_Point_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new Point[2] { new Point (1, 1), new Point (2, 2) });
			CheckCurve (gp);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddCurve_PointF_Null ()
		{
			new GraphicsPath ().AddCurve ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddCurve_PointF_0 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddCurve_PointF_1 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[1] { new PointF (1f, 1f) });
		}

		[Test]
		public void AddCurve_PointF_2 ()
		{
			GraphicsPath gp = new GraphicsPath ();
			gp.AddCurve (new PointF[2] { new PointF (1f, 1f), new PointF (2f, 2f) });
			CheckCurve (gp);
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
		[Category ("NotWorking")] // bounds+pen support is missing in libgdiplus
		public void GetBounds_WithPen ()
		{
			Rectangle rect = new Rectangle (1, 1, 2, 2);
			Pen p = new Pen (Color.Aqua, 0);
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (rect);

			RectangleF bounds = gp.GetBounds (null, p);
			Assert.AreEqual (-6.09999943f, bounds.X, "NullMatrix.Bounds.X");
			Assert.AreEqual (-6.09999943f, bounds.Y, "NullMatrix.Bounds.Y");
			Assert.AreEqual (16.1999989f, bounds.Width, "NullMatrix.Bounds.Width");
			Assert.AreEqual (16.1999989f, bounds.Height, "NullMatrix.Bounds.Height");

			Matrix m = new Matrix ();
			// an empty matrix is different than a null matrix
			bounds = gp.GetBounds (m, p);
			Assert.AreEqual (-0.419999957f, bounds.X, "EmptyMatrix.Bounds.X");
			Assert.AreEqual (-0.419999957f, bounds.Y, "EmptyMatrix.Bounds.Y");
			Assert.AreEqual (4.83999968f, bounds.Width, "EmptyMatrix.Bounds.Width");
			Assert.AreEqual (4.83999968f, bounds.Height, "EmptyMatrix.Bounds.Height");

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

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Transform_Null ()
		{
			new GraphicsPath ().Transform (null);
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

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Warp_Null ()
		{
			new GraphicsPath ().Warp (null, new RectangleF ());
		}
	}
}
