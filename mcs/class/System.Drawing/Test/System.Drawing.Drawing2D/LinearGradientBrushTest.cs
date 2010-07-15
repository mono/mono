//
// System.Drawing.Drawing2D.LinearGradientBrush unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006, 2008 Novell, Inc (http://www.novell.com)
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
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class LinearGradientBrushTest {

		private Point pt1;
		private Point pt2;
		private Color c1;
		private Color c2;
		private LinearGradientBrush default_brush;
		private Matrix empty_matrix;
		private RectangleF rect;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			pt1 = new Point (0, 0);
			pt2 = new Point (32, 32);
			c1 = Color.Blue;
			c2 = Color.Red;
			default_brush = new LinearGradientBrush (pt1, pt2, c1, c2);
			empty_matrix = new Matrix ();
			rect = new RectangleF (0, 0, 32, 32);
		}

		private void CheckDefaultRectangle (string msg, RectangleF rect)
		{
			Assert.AreEqual (pt1.X, rect.X, msg + ".Rectangle.X");
			Assert.AreEqual (pt1.Y, rect.Y, msg + ".Rectangle.Y");
			Assert.AreEqual (pt2.X, rect.Width, msg + ".Rectangle.Width");
			Assert.AreEqual (pt2.Y, rect.Height, msg + ".Rectangle.Height");
		}

		private void CheckDefaultMatrix (Matrix matrix)
		{
			float[] elements = matrix.Elements;
			Assert.AreEqual (1.0f, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1.0f, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1.0f, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (1.0f, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16.0f, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16.0f, elements[5], 0.1, "matrix.5");
		}

		private void CheckBrushAt45 (LinearGradientBrush lgb)
		{
			CheckDefaultRectangle ("4", lgb.Rectangle);
			Assert.AreEqual (1, lgb.Blend.Factors.Length, "Blend.Factors");
			Assert.AreEqual (1, lgb.Blend.Factors[0], "Blend.Factors [0]");
			Assert.AreEqual (1, lgb.Blend.Positions.Length, "Blend.Positions");
			// lgb.Blend.Positions [0] is always small (e-39) but never quite the same
			Assert.IsFalse (lgb.GammaCorrection, "GammaCorrection");
			Assert.AreEqual (2, lgb.LinearColors.Length, "LinearColors");
			Assert.IsNotNull (lgb.Transform, "Transform");
			CheckDefaultMatrix (lgb.Transform);
		}

		private void CheckMatrixAndRect (PointF pt1, PointF pt2, float[] testVals)
		{
			Matrix m;
			RectangleF rect;

			using (LinearGradientBrush b = new LinearGradientBrush (pt1, pt2, Color.Black, Color.White)) {
				m = b.Transform;
				rect = b.Rectangle;
			}

			Assert.AreEqual (testVals[0], m.Elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (testVals[1], m.Elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (testVals[2], m.Elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (testVals[3], m.Elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (testVals[4], m.Elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (testVals[5], m.Elements[5], 0.0001, "matrix.5");

			Assert.AreEqual (testVals[6], rect.X, 0.0001, "rect.X");
			Assert.AreEqual (testVals[7], rect.Y, 0.0001, "rect.Y");
			Assert.AreEqual (testVals[8], rect.Width, 0.0001, "rect.Width");
			Assert.AreEqual (testVals[9], rect.Height, 0.0001, "rect.Height");
		}

		private void CheckMatrixForScalableAngle (RectangleF rect, float angle, float[] testVals)
		{
			Matrix m;

			using (LinearGradientBrush b = new LinearGradientBrush (rect, Color.Firebrick, Color.Lavender, angle, true)) {
				m = b.Transform;
			}

			Assert.AreEqual (testVals[0], m.Elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (testVals[1], m.Elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (testVals[2], m.Elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (testVals[3], m.Elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (testVals[4], m.Elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (testVals[5], m.Elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_Point_Point_Color_Color ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			CheckBrushAt45 (lgb);

			Assert.AreEqual (WrapMode.Tile, lgb.WrapMode, "WrapMode.Tile");
			lgb.WrapMode = WrapMode.TileFlipX;
			Assert.AreEqual (WrapMode.TileFlipX, lgb.WrapMode, "WrapMode.TileFlipX");
			lgb.WrapMode = WrapMode.TileFlipY;
			Assert.AreEqual (WrapMode.TileFlipY, lgb.WrapMode, "WrapMode.TileFlipY");
			lgb.WrapMode = WrapMode.TileFlipXY;
			Assert.AreEqual (WrapMode.TileFlipXY, lgb.WrapMode, "WrapMode.TileFlipXY");
			// can't set WrapMode.Clamp
		}

		[Test]
		public void Constructor_Point_Point_Color_Color_1 ()
		{
			PointF pt1 = new Point (100, 200);
			PointF pt2 = new Point (200, 200);
			CheckMatrixAndRect (pt1, pt2, new float[] { 1, 0, 0, 1, 0, 0, 100, 150, 100, 100 });

			pt1 = new Point (100, 200);
			pt2 = new Point (0, 200);
			CheckMatrixAndRect (pt1, pt2, new float[] { -1, 0, 0, -1, 100, 400, 0, 150, 100, 100 });

			pt1 = new Point (100, 200);
			pt2 = new Point (100, 300);
			CheckMatrixAndRect (pt1, pt2, new float[] { 0, 1, -1, 0, 350, 150, 50, 200, 100, 100  });

			pt1 = new Point (100, 200);
			pt2 = new Point (100, 100);
			CheckMatrixAndRect (pt1, pt2, new float[] { 0, -1, 1, 0, -50, 250, 50, 100, 100, 100 });

			pt1 = new Point (100, 100);
			pt2 = new Point (150, 225);
			CheckMatrixAndRect (pt1, pt2, new float[] { 1, 2.5f, -0.6896552f, 0.2758622f, 112.069f, -194.8276f, 100, 100, 50, 125 });

			pt1 = new Point (100, 100);
			pt2 = new Point (55, 200);
			CheckMatrixAndRect (pt1, pt2, new float[] { -1, 2.222222f, -0.7484408f, -0.3367983f, 267.2661f, 28.29753f, 55, 100, 45, 100 });

			pt1 = new Point (100, 100);
			pt2 = new Point (150, 60);
			CheckMatrixAndRect (pt1, pt2, new float[] { 1, -0.8000001f, 0.9756095f, 1.219512f, -78.04876f, 82.43903f, 100, 60, 50, 40 });

			pt1 = new Point (100, 100);
			pt2 = new Point (27, 59);
			CheckMatrixAndRect (pt1, pt2, new float[] { -1, -0.5616435f, 0.8539224f, -1.520399f, 59.11317f, 236.0361f, 27, 59, 73, 41 });
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_0 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			Assert.AreEqual (1, lgb.Blend.Factors.Length, "Blend.Factors");
			Assert.AreEqual (1, lgb.Blend.Factors[0], "Blend.Factors[0]");
			Assert.AreEqual (1, lgb.Blend.Positions.Length, "Blend.Positions");
			// lgb.Blend.Positions [0] is always small (e-39) but never quite the same
			Assert.IsFalse (lgb.GammaCorrection, "GammaCorrection");
			Assert.AreEqual (c1.ToArgb (), lgb.LinearColors[0].ToArgb (), "LinearColors[0]");
			Assert.AreEqual (c2.ToArgb (), lgb.LinearColors[1].ToArgb (), "LinearColors[1]");
			Assert.AreEqual (rect, lgb.Rectangle, "Rectangle");
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity");
			Assert.AreEqual (WrapMode.Tile, lgb.WrapMode, "WrapMode");

			Matrix matrix = new Matrix (2, -1, 1, 2, 10, 10);
			lgb.Transform = matrix;
			Assert.AreEqual (matrix, lgb.Transform, "Transform");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_22_5 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 22.5f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (1.207107, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (0.5, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (-0.5, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (1.207107, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (4.686291, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (-11.313709, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_45 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 45f);
			CheckBrushAt45 (lgb);
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_90 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 90f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (0, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (0, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (32, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_135 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 135f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (-1, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (-1, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (48, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (16, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_180 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 180f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (-1, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (-1, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (32, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (32, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_270 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 270f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (0, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (-1, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (1, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (0, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (32, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_315 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 315f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (-1, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (1, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (-16, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (16, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_360()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 360f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			// just like 0'
			Assert.AreEqual (1, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		public void Constructor_RectangleF_Color_Color_Single_540 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 540f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			float[] elements = lgb.Transform.Elements;
			// just like 180'
			Assert.AreEqual (-1, elements[0], 0.0001, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.0001, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.0001, "matrix.2");
			Assert.AreEqual (-1, elements[3], 0.0001, "matrix.3");
			Assert.AreEqual (32, elements[4], 0.0001, "matrix.4");
			Assert.AreEqual (32, elements[5], 0.0001, "matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InterpolationColors_Colors_InvalidBlend ()
		{
			// default Blend doesn't allow getting this property
			Assert.IsNotNull (default_brush.InterpolationColors.Colors);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InterpolationColors_Positions_InvalidBlend ()
		{
			// default Blend doesn't allow getting this property
			Assert.IsNotNull (default_brush.InterpolationColors.Positions);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void LinearColors_Empty ()
		{
			default_brush.LinearColors = new Color[0];
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void LinearColors_One ()
		{
			default_brush.LinearColors = new Color[1];
		}

		[Test]
		public void LinearColors_Two ()
		{
			Assert.AreEqual (Color.FromArgb (255, 0, 0, 255), default_brush.LinearColors[0], "0");
			Assert.AreEqual (Color.FromArgb (255, 255, 0, 0), default_brush.LinearColors[1], "1");

			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.LinearColors = new Color[2] { Color.Black, Color.White };
			// not the same, the alpha is changed to 255 so they can't compare
			Assert.AreEqual (Color.FromArgb (255, 0, 0, 0), lgb.LinearColors[0], "0");
			Assert.AreEqual (Color.FromArgb (255, 255, 255, 255), lgb.LinearColors[1], "1");
		}

		[Test]
		public void LinearColors_Three ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.LinearColors = new Color[3] { Color.Red, Color.Green, Color.Blue };
			// not the same, the alpha is changed to 255 so they can't compare
			Assert.AreEqual (Color.FromArgb (255, 255, 0, 0), lgb.LinearColors[0], "0");
			Assert.AreEqual (Color.FromArgb (255, 0, 128, 0), lgb.LinearColors[1], "1");
		}

		[Test]
		public void Rectangle ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			CheckDefaultRectangle ("Original", lgb.Rectangle);
			lgb.MultiplyTransform (new Matrix (2, 0, 0, 2, 2, 2));
			CheckDefaultRectangle ("Multiply", lgb.Rectangle);
			lgb.ResetTransform ();
			CheckDefaultRectangle ("Reset", lgb.Rectangle);
			lgb.RotateTransform (90);
			CheckDefaultRectangle ("Rotate", lgb.Rectangle);
			lgb.ScaleTransform (4, 0.25f);
			CheckDefaultRectangle ("Scale", lgb.Rectangle);
			lgb.TranslateTransform (-10, -20);
			CheckDefaultRectangle ("Translate", lgb.Rectangle);

			lgb.SetBlendTriangularShape (0.5f);
			CheckDefaultRectangle ("SetBlendTriangularShape", lgb.Rectangle);
			lgb.SetSigmaBellShape (0.5f);
			CheckDefaultRectangle ("SetSigmaBellShape", lgb.Rectangle);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Transform_Null ()
		{
			default_brush.Transform = null;
		}

		[Test]
		public void Transform_Empty ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.Transform = new Matrix ();
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Transform_NonInvertible ()
		{
			default_brush.Transform = new Matrix (123, 24, 82, 16, 47, 30);
		}

		[Test]
		public void WrapMode_AllValid ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.WrapMode = WrapMode.Tile;
			Assert.AreEqual (WrapMode.Tile, lgb.WrapMode, "WrapMode.Tile");
			lgb.WrapMode = WrapMode.TileFlipX;
			Assert.AreEqual (WrapMode.TileFlipX, lgb.WrapMode, "WrapMode.TileFlipX");
			lgb.WrapMode = WrapMode.TileFlipY;
			Assert.AreEqual (WrapMode.TileFlipY, lgb.WrapMode, "WrapMode.TileFlipY");
			lgb.WrapMode = WrapMode.TileFlipXY;
			Assert.AreEqual (WrapMode.TileFlipXY, lgb.WrapMode, "WrapMode.TileFlipXY");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrapMode_Clamp ()
		{
			default_brush.WrapMode = WrapMode.Clamp;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void WrapMode_Invalid ()
		{
			default_brush.WrapMode = (WrapMode) Int32.MinValue;
		}


		[Test]
		public void Clone ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			LinearGradientBrush clone = (LinearGradientBrush) lgb.Clone ();
			Assert.AreEqual (lgb.Blend.Factors.Length, clone.Blend.Factors.Length, "Blend.Factors.Length");
			Assert.AreEqual (lgb.Blend.Positions.Length, clone.Blend.Positions.Length, "Blend.Positions.Length");
			Assert.AreEqual (lgb.GammaCorrection, clone.GammaCorrection, "GammaCorrection");
			Assert.AreEqual (lgb.LinearColors.Length, clone.LinearColors.Length, "LinearColors.Length");
			Assert.AreEqual (lgb.LinearColors.Length, clone.LinearColors.Length, "LinearColors.Length");
			Assert.AreEqual (lgb.Rectangle, clone.Rectangle, "Rectangle");
			Assert.AreEqual (lgb.Transform, clone.Transform, "Transform");
			Assert.AreEqual (lgb.WrapMode, clone.WrapMode, "WrapMode");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MultiplyTransform1_Null ()
		{
			default_brush.MultiplyTransform (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MultiplyTransform2_Null ()
		{
			default_brush.MultiplyTransform (null, MatrixOrder.Append);
		}

		[Test]
		public void MultiplyTransform2_Invalid ()
		{
			default_brush.MultiplyTransform (empty_matrix, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MultiplyTransform_NonInvertible ()
		{
			Matrix noninvertible = new Matrix (123, 24, 82, 16, 47, 30);
			default_brush.MultiplyTransform (noninvertible);
		}

		[Test]
		public void ResetTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			Assert.IsFalse (lgb.Transform.IsIdentity, "Transform.IsIdentity");
			lgb.ResetTransform ();
			Assert.IsTrue (lgb.Transform.IsIdentity, "Reset.IsIdentity");
		}

		[Test]
		public void RotateTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.RotateTransform (90);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (0, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (0, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

			lgb.RotateTransform (270);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Max ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.RotateTransform (Single.MaxValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (5.93904E+36, elements[0], 1e32, "matrix.0");
			Assert.AreEqual (5.93904E+36, elements[1], 1e32, "matrix.1");
			Assert.AreEqual (-5.93904E+36, elements[2], 1e32, "matrix.2");
			Assert.AreEqual (5.93904E+36, elements[3], 1e32, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Min ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.RotateTransform (Single.MinValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (-5.93904E+36, elements[0], 1e32, "matrix.0");
			Assert.AreEqual (-5.93904E+36, elements[1], 1e32, "matrix.1");
			Assert.AreEqual (5.93904E+36, elements[2], 1e32, "matrix.2");
			Assert.AreEqual (-5.93904E+36, elements[3], 1e32, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RotateTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.RotateTransform (720, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		public void ScaleTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.ScaleTransform (2, 4);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (2, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (4, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

			lgb.ScaleTransform (0.5f, 0.25f);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity");
		}

		[Test]
		public void ScaleTransform_45 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 45f);
			lgb.ScaleTransform (3, 3);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (3, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (3, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-3, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (3, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");
		}

		[Test]
		public void ScaleTransform_MaxMin ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.ScaleTransform (Single.MaxValue, Single.MinValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (Single.MaxValue, elements[0], 1e33, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (Single.MinValue, elements[3], 1e33, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.ScaleTransform (1, 1, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		public void SetBlendTriangularShape_Focus ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			// max valid
			lgb.SetBlendTriangularShape (1);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// min valid
			lgb.SetBlendTriangularShape (0);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// middle
			lgb.SetBlendTriangularShape (0.5f);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// no impact on matrix
		}

		[Test]
		public void SetBlendTriangularShape_Scale ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			// max valid
			lgb.SetBlendTriangularShape (0, 1);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// min valid
			lgb.SetBlendTriangularShape (1, 0);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// middle
			lgb.SetBlendTriangularShape (0.5f, 0.5f);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// no impact on matrix
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBlendTriangularShape_FocusTooSmall ()
		{
			default_brush.SetBlendTriangularShape (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBlendTriangularShape_FocusTooBig ()
		{
			default_brush.SetBlendTriangularShape (1.01f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBlendTriangularShape_ScaleTooSmall ()
		{
			default_brush.SetBlendTriangularShape (1, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBlendTriangularShape_ScaleTooBig ()
		{
			default_brush.SetBlendTriangularShape (1, 1.01f);
		}

		[Test]
		public void SetSigmaBellShape_Focus ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			// max valid
			lgb.SetSigmaBellShape (1);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// min valid
			lgb.SetSigmaBellShape (0);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// middle
			lgb.SetSigmaBellShape (0.5f);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// no impact on matrix
		}

		[Test]
		public void SetSigmaBellShape_Scale ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			// max valid
			lgb.SetSigmaBellShape (0, 1);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-1");
			// min valid
			lgb.SetSigmaBellShape (1, 0);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-2");
			// middle
			lgb.SetSigmaBellShape (0.5f, 0.5f);
			Assert.IsTrue (lgb.Transform.IsIdentity, "Transform.IsIdentity-3");
			// no impact on matrix
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetSigmaBellShape_FocusTooSmall ()
		{
			default_brush.SetSigmaBellShape (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetSigmaBellShape_FocusTooBig ()
		{
			default_brush.SetSigmaBellShape (1.01f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetSigmaBellShape_ScaleTooSmall ()
		{
			default_brush.SetSigmaBellShape (1, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetSigmaBellShape_ScaleTooBig ()
		{
			default_brush.SetSigmaBellShape (1, 1.01f);
		}

		[Test]
		public void TranslateTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 0f);
			lgb.TranslateTransform (1, 1);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (1, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (1, elements[5], 0.1, "matrix.5");

			lgb.TranslateTransform (-1, -1);
			// strangely lgb.Transform.IsIdentity is false
			elements = lgb.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "revert.matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "revert.matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "revert.matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "revert.matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "revert.matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "revert.matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TranslateTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.TranslateTransform (1, 1, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		public void Transform_Operations ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 45f);
			Matrix clone = lgb.Transform.Clone ();
			Matrix mul = clone.Clone ();

			clone.Multiply (mul, MatrixOrder.Append);
			lgb.MultiplyTransform (mul, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Multiply/Append");

			clone.Multiply (mul, MatrixOrder.Prepend);
			lgb.MultiplyTransform (mul, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Multiply/Prepend");

			clone.Rotate (45, MatrixOrder.Append);
			lgb.RotateTransform (45, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Rotate/Append");

			clone.Rotate (45, MatrixOrder.Prepend);
			lgb.RotateTransform (45, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Rotate/Prepend");

			clone.Scale (0.25f, 2, MatrixOrder.Append);
			lgb.ScaleTransform (0.25f, 2, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Scale/Append");

			clone.Scale (0.25f, 2, MatrixOrder.Prepend);
			lgb.ScaleTransform (0.25f, 2, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Scale/Prepend");

			clone.Translate (10, 20, MatrixOrder.Append);
			lgb.TranslateTransform (10, 20, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Translate/Append");

			clone.Translate (30, 40, MatrixOrder.Prepend);
			lgb.TranslateTransform (30, 40, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Translate/Prepend");

			clone.Reset ();
			lgb.ResetTransform ();
			Assert.AreEqual (lgb.Transform, clone, "Reset");
		}

		[Test]
		public void Transform_Operations_OnScalableAngle ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (rect, c1, c2, 360f, true);
			Matrix clone = lgb.Transform.Clone ();
			Matrix mul = clone.Clone ();
			Matrix m = new Matrix ();
			m.Scale (2, 1);
			m.Translate (rect.Width, rect.Height);
			m.Rotate (30f);

			clone.Multiply (mul, MatrixOrder.Append);
			lgb.MultiplyTransform (mul, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Multiply/Append");

			clone.Multiply (mul, MatrixOrder.Prepend);
			lgb.MultiplyTransform (mul, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Multiply/Prepend");

			clone.Rotate (45, MatrixOrder.Append);
			lgb.RotateTransform (45, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Rotate/Append");

			clone.Rotate (45, MatrixOrder.Prepend);
			lgb.RotateTransform (45, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Rotate/Prepend");

			clone.Scale (0.25f, 2, MatrixOrder.Append);
			lgb.ScaleTransform (0.25f, 2, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Scale/Append");

			clone.Scale (0.25f, 2, MatrixOrder.Prepend);
			lgb.ScaleTransform (0.25f, 2, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Scale/Prepend");

			clone.Translate (10, 20, MatrixOrder.Append);
			lgb.TranslateTransform (10, 20, MatrixOrder.Append);
			Assert.AreEqual (lgb.Transform, clone, "Translate/Append");

			clone.Translate (30, 40, MatrixOrder.Prepend);
			lgb.TranslateTransform (30, 40, MatrixOrder.Prepend);
			Assert.AreEqual (lgb.Transform, clone, "Translate/Prepend");

			clone.Reset ();
			lgb.ResetTransform ();
			Assert.AreEqual (lgb.Transform, clone, "Reset");
		}

		[Test]
		public void Constructor_Rectangle_Angle_Scalable ()
		{
			CheckMatrixForScalableAngle (new RectangleF (0, 0, 10, 10), 15, new float[] { 1.183013f, 0.3169873f, -0.3169873f, 1.183012f, 0.6698728f, -2.5f });

			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 15, new float[] { 1.183012f, 0.176104f, -0.5705772f, 1.183012f, 34.77311f, -28.76387f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 75, new float[] { 0.3169872f, 0.6572293f, -2.129423f, 0.3169873f, 232.2269f, 8.763878f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 95, new float[] { -0.09442029f, 0.599571f, -1.942611f, -0.09442017f, 247.2034f, 48.05788f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 150, new float[] { -1.183013f, 0.3794515f, -1.229423f, -1.183013f, 268.2269f, 157.0972f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 215, new float[] { -1.140856f, -0.4437979f, 1.437905f, -1.140856f, 38.34229f, 215.2576f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 50), 300, new float[] { 0.6830127f, -0.6572294f, 2.129422f, 0.6830124f, -157.2269f, 76.23613f });

			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 15, new float[] { 1.183012f, 0.5283121f, -0.1901924f, 1.183012f, 11.95002f, -64.33012f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 75, new float[] { 0.3169872f, 1.971688f, -0.7098077f, 0.3169872f, 147.05f, -55.66987f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 95, new float[] { -0.09442029f, 1.798713f, -0.6475369f, -0.09442022f, 169.499f, 12.84323f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 150, new float[] { -1.183013f, 1.138354f, -0.4098077f, -1.183013f, 219.05f, 209.3301f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 215, new float[] { -1.140856f, -1.331394f, 0.4793016f, -1.140856f, 95.85849f, 388.8701f });
			CheckMatrixForScalableAngle (new RectangleF (30, 60, 90, 150), 300, new float[] { 0.6830127f, -1.971688f, 0.7098075f, 0.6830125f, -72.04998f, 190.6699f });
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void LinearColors_Null ()
		{
			default_brush.LinearColors = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InterpolationColors_Null ()
		{
			default_brush.InterpolationColors = null;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Blend_Null ()
		{
			default_brush.Blend = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ZeroWidthRectangle ()
		{
			Rectangle r = new Rectangle (10, 10, 0, 10);
			Assert.AreEqual (0, r.Width, "Width");
			new LinearGradientBrush (r, Color.Red, Color.Blue, LinearGradientMode.Vertical);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ZeroHeightRectangleF ()
		{
			RectangleF r = new RectangleF (10.0f, 10.0f, 10.0f, 0.0f);
			Assert.AreEqual (0.0f, r.Height, "Height");
			new LinearGradientBrush (r, Color.Red, Color.Blue, LinearGradientMode.Vertical);
		}
	}
}
