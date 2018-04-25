//
// System.Drawing.Drawing2D.PathGradientBrush unit tests
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing.Drawing2D {

	[TestFixture]
	public class PathGradientBrushTest {

		private Point[] pts_2i;
		private PointF[] pts_2f;
		private Matrix empty_matrix;

		private void CheckDefaultRectangle (string message, RectangleF rect)
		{
			Assert.AreEqual (1f, rect.X, message + ".Rectangle.X");
			Assert.AreEqual (2f, rect.Y, message + ".Rectangle.Y");
			Assert.AreEqual (19f, rect.Width, message + ".Rectangle.Width");
			Assert.AreEqual (28f, rect.Height, message + ".Rectangle.Height");
		}

		private void CheckDefaults (PathGradientBrush pgb)
		{
			Assert.AreEqual (1, pgb.Blend.Factors.Length, "Blend.Factors.Length");
			Assert.AreEqual (1f, pgb.Blend.Factors[0], "Blend.Factors[0]");
			Assert.AreEqual (1, pgb.Blend.Positions.Length, "Blend.Positions.Length");
			Assert.AreEqual (0f, pgb.Blend.Positions[0], 1e-30, "Blend.Positions[0]");
			Assert.AreEqual (10.5f, pgb.CenterPoint.X, "CenterPoint.X");
			Assert.AreEqual (16f, pgb.CenterPoint.Y, "CenterPoint.Y");
			Assert.IsTrue (pgb.FocusScales.IsEmpty, "FocusScales");
			Assert.AreEqual (1, pgb.InterpolationColors.Colors.Length, "InterpolationColors.Colors.Length");
			Assert.AreEqual (0, pgb.InterpolationColors.Colors[0].ToArgb (), "InterpolationColors.Colors[0]");
			Assert.AreEqual (1, pgb.InterpolationColors.Positions.Length, "InterpolationColors.Positions.Length");
			Assert.AreEqual (0f, pgb.InterpolationColors.Positions[0], 1e-38, "InterpolationColors.Positions[0]");
			CheckDefaultRectangle (String.Empty, pgb.Rectangle);
			Assert.AreEqual (1, pgb.SurroundColors.Length, "SurroundColors.Length");
			Assert.AreEqual (-1, pgb.SurroundColors[0].ToArgb (), "SurroundColors[0]");
			Assert.IsTrue (pgb.Transform.IsIdentity, "Transform");
		}

		private void CheckPointsDefaults (PathGradientBrush pgb)
		{
			CheckDefaults (pgb);
			Assert.AreEqual (-16777216, pgb.CenterColor.ToArgb (), "CenterColor");
		}

		private void CheckPathDefaults (PathGradientBrush pgb)
		{
			CheckDefaults (pgb);
			Assert.AreEqual (-1, pgb.CenterColor.ToArgb (), "CenterColor");
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			pts_2i = new Point[2] { new Point (1, 2), new Point (20, 30) };
			pts_2f = new PointF[2] { new PointF (1, 2), new PointF (20, 30) };
			empty_matrix = new Matrix ();
		}

		[Test]
		public void Constructor_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new PathGradientBrush (gp));
		}

		[Test]
		public void Constructor_GraphicsPath_Empty ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (gp));
			}
		}

		[Test]
		public void Constructor_GraphicsPath_SinglePoint ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (new Point[1] { new Point (1, 1) });
				// Special case - a line with a single point is valid
				Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (gp));
			}
		}

		[Test]
		public void Constructor_GraphicsPath_Line ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (PathGradientBrush pgb = new PathGradientBrush (gp)) {
					CheckPathDefaults (pgb);
					Assert.AreEqual (WrapMode.Clamp, pgb.WrapMode, "WrapMode");
				}
			}
		}

		[Test]
		public void Constructor_Point_Null ()
		{
			Point[] pts = null;
			Assert.Throws<ArgumentNullException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_Point_Empty ()
		{
			Point[] pts = new Point [0];
			Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_Point_One ()
		{
			Point[] pts = new Point[1] { new Point (1, 1) };
			Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_Point_Two ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Clamp, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_Point_WrapMode_Clamp ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i, WrapMode.Clamp)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Clamp, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_Point_WrapMode_Tile ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i, WrapMode.Tile)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Tile, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_Point_WrapMode_TileFlipX ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i, WrapMode.TileFlipX)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipX, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_Point_WrapMode_TileFlipY ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i, WrapMode.TileFlipY)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipY, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_Point_WrapMode_TileFlipXY ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2i, WrapMode.TileFlipXY)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipXY, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_Null ()
		{
			PointF[] pts = null;
			Assert.Throws<ArgumentNullException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_PointF_Empty ()
		{
			PointF[] pts = new PointF[0];
			Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_PointF_One ()
		{
			PointF[] pts = new PointF[1] { new PointF (1, 1) };
			Assert.Throws<OutOfMemoryException> (() => new PathGradientBrush (pts));
		}

		[Test]
		public void Constructor_PointF_Two ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Clamp, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_WrapMode_Invalid ()
		{
			Assert.Throws<InvalidEnumArgumentException> (() => new PathGradientBrush (pts_2f, (WrapMode)Int32.MinValue));
		}

		[Test]
		public void Constructor_PointF_WrapMode_Clamp ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Clamp, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_WrapMode_Tile ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Tile)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.Tile, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_WrapMode_TileFlipX ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipX)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipX, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_WrapMode_TileFlipY ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipY)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipY, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Constructor_PointF_WrapMode_TileFlipXY ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				CheckPointsDefaults (pgb);
				Assert.AreEqual (WrapMode.TileFlipXY, pgb.WrapMode, "WrapMode");
			}
		}

		[Test]
		public void Blend ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				// change not accepted - but no exception is thrown
				pgb.Blend.Factors = new float[0];
				Assert.AreEqual (1, pgb.Blend.Factors.Length, "Factors-0");
				pgb.Blend.Factors = new float[2];
				Assert.AreEqual (1, pgb.Blend.Factors.Length, "Factors-1");

				// change not accepted - but no exception is thrown
				pgb.Blend.Positions = new float[0];
				Assert.AreEqual (1, pgb.Blend.Positions.Length, "Positions-0");
				pgb.Blend.Positions = new float[2];
				Assert.AreEqual (1, pgb.Blend.Positions.Length, "Positions-1");
			}
		}

		[Test]
		public void FocusScales ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				PointF fs = new PointF (Single.MaxValue, Single.MinValue);
				pgb.FocusScales = fs;
				Assert.AreEqual (Single.MaxValue, pgb.FocusScales.X, "MaxValue");
				Assert.AreEqual (Single.MinValue, pgb.FocusScales.Y, "MinValue");

				fs.X = Single.NaN;
				fs.Y = Single.NegativeInfinity;
				pgb.FocusScales = fs;
				Assert.AreEqual (Single.NaN, pgb.FocusScales.X, "NaN");
				Assert.AreEqual (Single.NegativeInfinity, pgb.FocusScales.Y, "NegativeInfinity");
			}
		}

		[Test]
		public void CenterColor ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				pgb.CenterColor = Color.Black;
				Assert.AreEqual (Color.Black.ToArgb (), pgb.CenterColor.ToArgb (), "Black");
				pgb.CenterColor = Color.Transparent;
				Assert.AreEqual (Color.Transparent.ToArgb (), pgb.CenterColor.ToArgb (), "Transparent");
			}
		}

		[Test]
		public void CenterPoint ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				PointF cp = new PointF (Single.MaxValue, Single.MinValue);
				pgb.CenterPoint = cp;
				Assert.AreEqual (Single.MaxValue, pgb.CenterPoint.X, "MaxValue");
				Assert.AreEqual (Single.MinValue, pgb.CenterPoint.Y, "MinValue");

				cp.X = Single.NaN;
				cp.Y = Single.NegativeInfinity;
				pgb.CenterPoint = cp;
				Assert.AreEqual (Single.NaN, pgb.CenterPoint.X, "NaN");
				Assert.AreEqual (Single.NegativeInfinity, pgb.CenterPoint.Y, "NegativeInfinity");
			}
		}

		[Test]
		public void InterpolationColors ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				// change not accepted - but no exception is thrown
				pgb.InterpolationColors.Colors = new Color[0];
				Assert.AreEqual (1, pgb.InterpolationColors.Colors.Length, "Colors-0");
				pgb.InterpolationColors.Colors = new Color[2];
				Assert.AreEqual (1, pgb.InterpolationColors.Colors.Length, "Colors-1");

				// change not accepted - but no exception is thrown
				pgb.InterpolationColors.Positions = new float[0];
				Assert.AreEqual (1, pgb.InterpolationColors.Positions.Length, "Positions-0");
				pgb.InterpolationColors.Positions = new float[2];
				Assert.AreEqual (1, pgb.InterpolationColors.Positions.Length, "Positions-1");
			}
		}

		[Test]
		public void Rectangle ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				CheckDefaultRectangle ("Original", pgb.Rectangle);
				pgb.MultiplyTransform (new Matrix (2, 0, 0, 2, 2, 2));
				CheckDefaultRectangle ("Multiply", pgb.Rectangle);
				pgb.ResetTransform ();
				CheckDefaultRectangle ("Reset", pgb.Rectangle);
				pgb.RotateTransform (90);
				CheckDefaultRectangle ("Rotate", pgb.Rectangle);
				pgb.ScaleTransform (4, 0.25f);
				CheckDefaultRectangle ("Scale", pgb.Rectangle);
				pgb.TranslateTransform (-10, -20);
				CheckDefaultRectangle ("Translate", pgb.Rectangle);

				pgb.SetBlendTriangularShape (0.5f);
				CheckDefaultRectangle ("SetBlendTriangularShape", pgb.Rectangle);
				pgb.SetSigmaBellShape (0.5f);
				CheckDefaultRectangle ("SetSigmaBellShape", pgb.Rectangle);
			}
		}

		[Test]
		public void SurroundColors_Empty ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				Assert.Throws<ArgumentException> (() => pgb.SurroundColors = new Color[0]);
			}
		}

		[Test]
		public void SurroundColors_2PointF ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.TileFlipXY)) {
				// default values
				Assert.AreEqual (1, pgb.SurroundColors.Length, "Length-0");
				Assert.AreEqual (-1, pgb.SurroundColors[0].ToArgb (), "SurroundColors-0");

				// default can't be changed
				pgb.SurroundColors[0] = Color.Gold;
				Assert.AreEqual (-1, pgb.SurroundColors[0].ToArgb (), "SurroundColors-1");

				// 2 empty color isn't valid, change isn't accepted
				pgb.SurroundColors = new Color[2];
				Assert.AreEqual (1, pgb.SurroundColors.Length, "Length-1");

				pgb.SurroundColors = new Color[2] { Color.Black, Color.White };
				Assert.AreEqual (2, pgb.SurroundColors.Length, "Length-2");
				Assert.AreEqual (-16777216, pgb.SurroundColors[0].ToArgb (), "SurroundColors-2");
				Assert.AreEqual (-1, pgb.SurroundColors[1].ToArgb (), "SurroundColors-3");
			}
		}

		[Test]
		public void SurroundColors_3PointsF ()
		{
			PointF[] points = new PointF[3] { new PointF (5, 50), new PointF (10, 100), new PointF (20, 75) };
			using (PathGradientBrush pgb = new PathGradientBrush (points)) {
				// 3 empty color isn't valid, change isn't accepted
				pgb.SurroundColors = new Color[3] { Color.Empty, Color.Empty, Color.Empty };
				Assert.AreEqual (1, pgb.SurroundColors.Length, "Length-1");

				pgb.SurroundColors = new Color[3] { Color.Red, Color.Green, Color.Blue };
				// change not accepted - but no exception is thrown
				Assert.AreEqual (3, pgb.SurroundColors.Length, "Length-1");
				Assert.AreEqual (-65536, pgb.SurroundColors[0].ToArgb (), "SurroundColors-1");
				Assert.AreEqual (-16744448, pgb.SurroundColors[1].ToArgb (), "SurroundColors-2");
				Assert.AreEqual (-16776961, pgb.SurroundColors[2].ToArgb (), "SurroundColors-3");
			}
		}

		[Test]
		public void Transform_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new PathGradientBrush (pts_2f, WrapMode.Clamp).Transform = null);
		}

		[Test]
		public void Transform_Empty ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.Transform = new Matrix ();
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity");
			}
		}

		[Test]
		public void Transform_NonInvertible ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.Transform = new Matrix (123, 24, 82, 16, 47, 30));
			}
		}

		[Test]
		public void WrapMode_All ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				foreach (WrapMode wm in Enum.GetValues (typeof (WrapMode))) {
					pgb.WrapMode = wm;
					Assert.AreEqual (wm, pgb.WrapMode, wm.ToString ());
				}
			}
		}

		[Test]
		public void WrapMode_Invalid ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<InvalidEnumArgumentException> (() => pgb.WrapMode = (WrapMode) Int32.MinValue);
			}
		}

		[Test]
		public void Clone ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (PathGradientBrush pgb = new PathGradientBrush (gp)) {
					using (PathGradientBrush clone = (PathGradientBrush) pgb.Clone ()) {
						CheckPathDefaults (clone);
						Assert.AreEqual (WrapMode.Clamp, clone.WrapMode, "WrapMode");
					}
				}
			}
		}

		[Test]
		public void MultiplyTransform1_Null ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentNullException> (() => pgb.MultiplyTransform (null));
			}
		}

		[Test]
		public void MultiplyTransform2_Null ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentNullException> (() => pgb.MultiplyTransform (null, MatrixOrder.Append));
			}
		}

		[Test]
		public void MultiplyTransform2_Invalid ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.MultiplyTransform (empty_matrix, (MatrixOrder) Int32.MinValue);
			}
		}

		[Test]
		public void MultiplyTransform_NonInvertible ()
		{
			using (Matrix noninvertible = new Matrix (123, 24, 82, 16, 47, 30)) {
				using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
					Assert.Throws<ArgumentException> (() => pgb.MultiplyTransform (noninvertible));
				}
			}
		}

		[Test]
		public void ResetTransform ()
		{
			using (Matrix m = new Matrix (2, 0, 0, 2, 10, -10)) {
				using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
					pgb.Transform = m;
					Assert.IsFalse (pgb.Transform.IsIdentity, "Transform.IsIdentity");
					pgb.ResetTransform ();
					Assert.IsTrue (pgb.Transform.IsIdentity, "Reset.IsIdentity");
				}
			}
		}

		[Test]
		public void RotateTransform ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.RotateTransform (90);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (0, elements[0], 0.1, "matrix.0");
				Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
				Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
				Assert.AreEqual (0, elements[3], 0.1, "matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

				pgb.RotateTransform (270);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity");
			}
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Max ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.RotateTransform (Single.MaxValue);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (5.93904E+36, elements[0], 1e32, "matrix.0");
				Assert.AreEqual (5.93904E+36, elements[1], 1e32, "matrix.1");
				Assert.AreEqual (-5.93904E+36, elements[2], 1e32, "matrix.2");
				Assert.AreEqual (5.93904E+36, elements[3], 1e32, "matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
			}
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Min ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.RotateTransform (Single.MinValue);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (-5.93904E+36, elements[0], 1e32, "matrix.0");
				Assert.AreEqual (-5.93904E+36, elements[1], 1e32, "matrix.1");
				Assert.AreEqual (5.93904E+36, elements[2], 1e32, "matrix.2");
				Assert.AreEqual (-5.93904E+36, elements[3], 1e32, "matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
			}
		}

		[Test]
		public void RotateTransform_InvalidOrder ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.RotateTransform (720, (MatrixOrder) Int32.MinValue));
			}
		}

		[Test]
		public void ScaleTransform ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.ScaleTransform (2, 4);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (2, elements[0], 0.1, "matrix.0");
				Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
				Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
				Assert.AreEqual (4, elements[3], 0.1, "matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

				pgb.ScaleTransform (0.5f, 0.25f);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity");
			}
		}

		[Test]
		public void ScaleTransform_MaxMin ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.ScaleTransform (Single.MaxValue, Single.MinValue);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (Single.MaxValue, elements[0], 1e33, "matrix.0");
				Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
				Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
				Assert.AreEqual (Single.MinValue, elements[3], 1e33, "matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
			}
		}

		[Test]
		public void ScaleTransform_InvalidOrder ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.ScaleTransform (1, 1, (MatrixOrder) Int32.MinValue));
			}
		}

		[Test]
		public void SetBlendTriangularShape_Focus ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				// max valid
				pgb.SetBlendTriangularShape (1);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// min valid
				pgb.SetBlendTriangularShape (0);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// middle
				pgb.SetBlendTriangularShape (0.5f);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// no impact on matrix
			}
		}

		[Test]
		public void SetBlendTriangularShape_Scale ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				// max valid
				pgb.SetBlendTriangularShape (0, 1);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// min valid
				pgb.SetBlendTriangularShape (1, 0);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// middle
				pgb.SetBlendTriangularShape (0.5f, 0.5f);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// no impact on matrix
			}
		}

		[Test]
		public void SetBlendTriangularShape_FocusTooSmall ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetBlendTriangularShape (-1));
			}
		}

		[Test]
		public void SetBlendTriangularShape_FocusTooBig ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetBlendTriangularShape (1.01f));
			}
		}

		[Test]
		public void SetBlendTriangularShape_ScaleTooSmall ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetBlendTriangularShape (1, -1));
			}
		}

		[Test]
		public void SetBlendTriangularShape_ScaleTooBig ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetBlendTriangularShape (1, 1.01f));
			}
		}

		[Test]
		public void SetSigmaBellShape_Focus ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				// max valid
				pgb.SetSigmaBellShape (1);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// min valid
				pgb.SetSigmaBellShape (0);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// middle
				pgb.SetSigmaBellShape (0.5f);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// no impact on matrix
			}
		}

		[Test]
		public void SetSigmaBellShape_Scale ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				// max valid
				pgb.SetSigmaBellShape (0, 1);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-1");
				// min valid
				pgb.SetSigmaBellShape (1, 0);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-2");
				// middle
				pgb.SetSigmaBellShape (0.5f, 0.5f);
				Assert.IsTrue (pgb.Transform.IsIdentity, "Transform.IsIdentity-3");
				// no impact on matrix
			}
		}

		[Test]
		public void SetSigmaBellShape_FocusTooSmall ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetSigmaBellShape (-1));
			}
		}

		[Test]
		public void SetSigmaBellShape_FocusTooBig ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetSigmaBellShape (1.01f));
			}
		}

		[Test]
		public void SetSigmaBellShape_ScaleTooSmall ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetSigmaBellShape (1, -1));
			}
		}

		[Test]
		public void SetSigmaBellShape_ScaleTooBig ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.SetSigmaBellShape (1, 1.01f));
			}
		}

		[Test]
		public void TranslateTransform ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				pgb.TranslateTransform (1, 1);
				float[] elements = pgb.Transform.Elements;
				Assert.AreEqual (1, elements[0], 0.1, "matrix.0");
				Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
				Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
				Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
				Assert.AreEqual (1, elements[4], 0.1, "matrix.4");
				Assert.AreEqual (1, elements[5], 0.1, "matrix.5");

				pgb.TranslateTransform (-1, -1);
				// strangely lgb.Transform.IsIdentity is false
				elements = pgb.Transform.Elements;
				Assert.AreEqual (1, elements[0], 0.1, "revert.matrix.0");
				Assert.AreEqual (0, elements[1], 0.1, "revert.matrix.1");
				Assert.AreEqual (0, elements[2], 0.1, "revert.matrix.2");
				Assert.AreEqual (1, elements[3], 0.1, "revert.matrix.3");
				Assert.AreEqual (0, elements[4], 0.1, "revert.matrix.4");
				Assert.AreEqual (0, elements[5], 0.1, "revert.matrix.5");
			}
		}

		[Test]
		public void TranslateTransform_InvalidOrder ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Assert.Throws<ArgumentException> (() => pgb.TranslateTransform (1, 1, (MatrixOrder) Int32.MinValue));
			}
		}

		[Test]
		public void Transform_Operations ()
		{
			using (PathGradientBrush pgb = new PathGradientBrush (pts_2f, WrapMode.Clamp)) {
				Matrix clone = pgb.Transform.Clone ();
				Matrix mul = clone.Clone ();

				clone.Multiply (mul, MatrixOrder.Append);
				pgb.MultiplyTransform (mul, MatrixOrder.Append);
				Assert.AreEqual (pgb.Transform, clone, "Multiply/Append");

				clone.Multiply (mul, MatrixOrder.Prepend);
				pgb.MultiplyTransform (mul, MatrixOrder.Prepend);
				Assert.AreEqual (pgb.Transform, clone, "Multiply/Prepend");

				clone.Rotate (45, MatrixOrder.Append);
				pgb.RotateTransform (45, MatrixOrder.Append);
				Assert.AreEqual (pgb.Transform, clone, "Rotate/Append");

				clone.Rotate (45, MatrixOrder.Prepend);
				pgb.RotateTransform (45, MatrixOrder.Prepend);
				Assert.AreEqual (pgb.Transform, clone, "Rotate/Prepend");

				clone.Scale (0.25f, 2, MatrixOrder.Append);
				pgb.ScaleTransform (0.25f, 2, MatrixOrder.Append);
				Assert.AreEqual (pgb.Transform, clone, "Scale/Append");

				clone.Scale (0.25f, 2, MatrixOrder.Prepend);
				pgb.ScaleTransform (0.25f, 2, MatrixOrder.Prepend);
				Assert.AreEqual (pgb.Transform, clone, "Scale/Prepend");

				clone.Translate (10, 20, MatrixOrder.Append);
				pgb.TranslateTransform (10, 20, MatrixOrder.Append);
				Assert.AreEqual (pgb.Transform, clone, "Translate/Append");

				clone.Translate (30, 40, MatrixOrder.Prepend);
				pgb.TranslateTransform (30, 40, MatrixOrder.Prepend);
				Assert.AreEqual (pgb.Transform, clone, "Translate/Prepend");

				clone.Reset ();
				pgb.ResetTransform ();
				Assert.AreEqual (pgb.Transform, clone, "Reset");
			}
		}

		[Test]
		public void Blend_Null ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (PathGradientBrush pgb = new PathGradientBrush (gp)) {
					Assert.Throws<NullReferenceException> (() => pgb.Blend = null);
				}
			}
		}

		[Test]
		public void InterpolationColors_Null ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (PathGradientBrush pgb = new PathGradientBrush (gp)) {
					Assert.Throws<NullReferenceException> (() => pgb.InterpolationColors = null);
				}
			}
		}

		[Test]
		public void SurroundColors_Null ()
		{
			using (GraphicsPath gp = new GraphicsPath ()) {
				gp.AddLines (pts_2f);
				using (PathGradientBrush pgb = new PathGradientBrush (gp)) {
					Assert.Throws<NullReferenceException> (() => pgb.SurroundColors = null);
				}
			}
		}
	}
}
