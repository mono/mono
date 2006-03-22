//
// System.Drawing.Drawing2D.LinearGradientBrush unit tests
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
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class LinearGradientBrushTest {

		private Point pt1;
		private Point pt2;
		private Color c1;
		private Color c2;
		private LinearGradientBrush default_brush;
		private Matrix empty_matrix;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			pt1 = new Point (0, 0);
			pt2 = new Point (32, 32);
			c1 = Color.Blue;
			c2 = Color.Red;
			default_brush = new LinearGradientBrush (pt1, pt2, c1, c2);
			empty_matrix = new Matrix ();
		}

		private void CheckDefaultRectangle (string msg, RectangleF rect)
		{
			Assert.AreEqual (pt1.X, rect.X, msg + ".Rectangle.X");
			Assert.AreEqual (pt1.Y, rect.Y, msg + ".Rectangle.Y");
			Assert.AreEqual (pt2.X, rect.Width, msg + ".Rectangle.Width");
			Assert.AreEqual (pt2.Y, rect.Height, msg + ".Rectangle.Height");
		}

		private void CheckEmptyMatrix (Matrix matrix)
		{
			float[] elements = matrix.Elements;
			Assert.AreEqual (1, elements[0], "matrix.0");
			Assert.AreEqual (0, elements[1], "matrix.1");
			Assert.AreEqual (0, elements[2], "matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (0, elements[4], "matrix.4");
			Assert.AreEqual (0, elements[5], "matrix.5");
		}

		private void CheckDefaultMatrix (Matrix matrix)
		{
			float[] elements = matrix.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16, elements[4], "matrix.4");
			Assert.AreEqual (-16, elements[5], "matrix.5");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void Constructor4 ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			CheckDefaultRectangle ("4", lgb.Rectangle);
			Assert.AreEqual (1, lgb.Blend.Factors.Length, "Blend.Factors");
			Assert.AreEqual (1, lgb.Blend.Factors [0], "Blend.Factors [0]");
			Assert.AreEqual (1, lgb.Blend.Positions.Length, "Blend.Positions");
			Assert.IsFalse (lgb.GammaCorrection, "GammaCorrection");
			Assert.AreEqual (2, lgb.LinearColors.Length, "LinearColors");
			Assert.IsNotNull (lgb.Transform, "Transform");
			CheckDefaultMatrix (lgb.Transform);

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
			CheckEmptyMatrix (lgb.Transform);
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
			CheckDefaultRectangle ("default", default_brush.Rectangle);
			LinearGradientBrush clone = (LinearGradientBrush) default_brush.Clone ();
			CheckDefaultRectangle ("clone", clone.Rectangle);
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
		public void ResetTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.ResetTransform ();
			CheckEmptyMatrix (lgb.Transform);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.RotateTransform (90);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (-1, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (-1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");

			lgb.RotateTransform (270);
			CheckDefaultMatrix (lgb.Transform);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Max ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.RotateTransform (Single.MaxValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (-1.69580966E+30, elements[0], 1e25, "matrix.0");
			Assert.AreEqual (1.18780928E+37, elements[1], 1e32, "matrix.1");
			Assert.AreEqual (-1.18780953E+37, elements[2], 1e32, "matrix.2");
			Assert.AreEqual (-2.79830475E+29, elements[3], 1e24, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void RotateTransform_Min ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.RotateTransform (Single.MinValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (1.06198436E+30, elements[0], 1e25, "matrix.0");
			Assert.AreEqual (-1.18780953E+37, elements[1], 1e32, "matrix.1");
			Assert.AreEqual (1.1878094E+37, elements[2], 1e32, "matrix.2");
			Assert.AreEqual (-3.53994825E+29, elements[3], 1e24, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RotateTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.RotateTransform (720, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ScaleTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.ScaleTransform (2, 4);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (2, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (2, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-4, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (4, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");

			lgb.ScaleTransform (0.5f, 0.25f);
			CheckDefaultMatrix (lgb.Transform);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ScaleTransform_MaxMin ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.ScaleTransform (Single.MaxValue, Single.MinValue);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (Single.MaxValue, elements[0], 1e33, "matrix.0");
			Assert.AreEqual (Single.MaxValue, elements[1], 1e33, "matrix.1");
			Assert.AreEqual (Single.MaxValue, elements[2], 1e33, "matrix.2");
			Assert.AreEqual (Single.MinValue, elements[3], 1e33, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-16, elements[5], 0.1, "matrix.5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ScaleTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.ScaleTransform (1, 1, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void SetBlendTriangularShape_Focus ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			// max valid
			lgb.SetBlendTriangularShape (1);
			CheckDefaultMatrix (lgb.Transform);
			// min valid
			lgb.SetBlendTriangularShape (0);
			CheckDefaultMatrix (lgb.Transform);
			// middle
			lgb.SetBlendTriangularShape (0.5f);
			CheckDefaultMatrix (lgb.Transform);
			// no impact on matrix
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void SetBlendTriangularShape_Scale ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			// max valid
			lgb.SetBlendTriangularShape (0, 1);
			CheckDefaultMatrix (lgb.Transform);
			// min valid
			lgb.SetBlendTriangularShape (1, 0);
			CheckDefaultMatrix (lgb.Transform);
			// middle
			lgb.SetBlendTriangularShape (0.5f, 0.5f);
			CheckDefaultMatrix (lgb.Transform);
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
		[NUnit.Framework.Category ("NotWorking")]
		public void SetSigmaBellShape_Focus ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			// max valid
			lgb.SetSigmaBellShape (1);
			CheckDefaultMatrix (lgb.Transform);
			// min valid
			lgb.SetSigmaBellShape (0);
			CheckDefaultMatrix (lgb.Transform);
			// middle
			lgb.SetSigmaBellShape (0.5f);
			CheckDefaultMatrix (lgb.Transform);
			// no impact on matrix
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void SetSigmaBellShape_Scale ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			// max valid
			lgb.SetSigmaBellShape (0, 1);
			CheckDefaultMatrix (lgb.Transform);
			// min valid
			lgb.SetSigmaBellShape (1, 0);
			CheckDefaultMatrix (lgb.Transform);
			// middle
			lgb.SetSigmaBellShape (0.5f, 0.5f);
			CheckDefaultMatrix (lgb.Transform);
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
		[NUnit.Framework.Category ("NotWorking")]
		public void TranslateTransform ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.TranslateTransform (1, 1);
			float[] elements = lgb.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (16, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (-14, elements[5], 0.1, "matrix.5");

			lgb.TranslateTransform (-1, -1);
			CheckDefaultMatrix (lgb.Transform);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TranslateTransform_InvalidOrder ()
		{
			LinearGradientBrush lgb = new LinearGradientBrush (pt1, pt2, c1, c2);
			lgb.TranslateTransform (1, 1, (MatrixOrder) Int32.MinValue);
		}
	}
}
