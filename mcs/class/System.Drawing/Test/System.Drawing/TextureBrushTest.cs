//
// System.Drawing.TextureBrush unit tests
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class TextureBrushTest {

		private Image image;
		private Rectangle rect;
		private RectangleF rectf;
		private ImageAttributes attr;
		private Bitmap bmp;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			image = new Bitmap (10, 10);
			rect = new Rectangle (0, 0, 10, 10);
			rectf = new RectangleF (0, 0, 10, 10);
			attr = new ImageAttributes ();
			bmp = new Bitmap (50, 50);
		}

		private void Common (TextureBrush t, WrapMode wm)
		{
			using (Image img = t.Image) {
				Assert.IsNotNull (img, "Image");
			}
			Assert.IsFalse (Object.ReferenceEquals (image, t.Image), "Image-Equals");
			Assert.IsTrue (t.Transform.IsIdentity, "Transform.IsIdentity");
			Assert.AreEqual (wm, t.WrapMode, "WrapMode");
		}

		[Test]
		public void CtorImage_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null));
		}

		[Test]
		public void CtorImage ()
		{
			TextureBrush t = new TextureBrush (image);
			Common (t, WrapMode.Tile);
		}

		[Test]
		public void CtorImage_Null_WrapMode ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null, WrapMode.Clamp));
		}

		[Test]
		public void CtorImageWrapMode ()
		{
			foreach (WrapMode wm in Enum.GetValues (typeof (WrapMode))) {
				TextureBrush t = new TextureBrush (image, wm);
				Common (t, wm);
			}
		}

		[Test]
		public void CtorImageWrapMode_Invalid ()
		{
			Assert.Throws<InvalidEnumArgumentException> (() => new TextureBrush (image, (WrapMode) Int32.MinValue));
		}

		[Test]
		public void CtorImage_Null_Rectangle ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null, rect));
		}

		[Test]
		public void CtorImageRectangle_Empty ()
		{
			Assert.Throws<OutOfMemoryException> (() => new TextureBrush (image, new Rectangle ()));
		}

		[Test]
		public void CtorImageRectangle ()
		{
			TextureBrush t = new TextureBrush (image, rect);
			Common (t, WrapMode.Tile);
		}

		[Test]
		public void CtorImage_Null_RectangleF ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null, rectf));
		}

		[Test]
		public void CtorImageRectangleF_Empty ()
		{
			Assert.Throws<OutOfMemoryException> (() => new TextureBrush (image, new RectangleF ()));
		}

		[Test]
		public void CtorImageRectangleF ()
		{
			TextureBrush t = new TextureBrush (image, rectf);
			Common (t, WrapMode.Tile);
		}

		[Test]
		public void CtorImage_Null_RectangleAttributes ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null, rect, attr));
		}

		[Test]
		public void CtorImageRectangle_Empty_Attributes ()
		{
			Assert.Throws<OutOfMemoryException> (() => new TextureBrush (image, new Rectangle (), attr));
		}

		[Test]
		public void CtorImageRectangleAttributes_Null ()
		{
			TextureBrush t = new TextureBrush (image, rect, null);
			Common (t, WrapMode.Tile);
		}

		[Test]
		public void CtorImageRectangleAttributes ()
		{
			TextureBrush t = new TextureBrush (image, rect, attr);
			Common (t, WrapMode.Clamp);
		}

		[Test]
		public void CtorImage_Null_RectangleFAttributes ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (null, rectf, attr));
		}

		[Test]
		public void CtorImageRectangleF_Empty_Attributes ()
		{
			Assert.Throws<OutOfMemoryException> (() => new TextureBrush (image, new RectangleF ()));
		}

		[Test]
		public void CtorImageRectangleFAttributes_Null ()
		{
			TextureBrush t = new TextureBrush (image, rectf, null);
			Common (t, WrapMode.Tile);
		}

		[Test]
		public void CtorImageRectangleFAttributes ()
		{
			TextureBrush t = new TextureBrush (image, rectf, attr);
			Common (t, WrapMode.Clamp);
		}

		[Test]
		public void CtorImageWrapModeRectangle ()
		{
			foreach (WrapMode wm in Enum.GetValues (typeof (WrapMode))) {
				TextureBrush t = new TextureBrush (image, wm, rect);
				Common (t, wm);
			}
		}

		[Test]
		public void CtorImageWrapMode_Invalid_Rectangle ()
		{
			Assert.Throws<InvalidEnumArgumentException> (() => new TextureBrush (image, (WrapMode) Int32.MinValue, rect));
		}

		[Test]
		public void CtorImageWrapModeRectangleF ()
		{
			foreach (WrapMode wm in Enum.GetValues (typeof (WrapMode))) {
				TextureBrush t = new TextureBrush (image, wm, rectf);
				Common (t, wm);
			}
		}

		[Test]
		public void CtorImageWrapMode_Invalid_RectangleF ()
		{
			Assert.Throws<InvalidEnumArgumentException> (() => new TextureBrush (image, (WrapMode) Int32.MinValue, rectf));
		}

		[Test]
		public void TextureBush_RectangleInsideBitmap ()
		{
			Rectangle r = new Rectangle (10, 10, 40, 40);
			Assert.IsTrue (r.Y + r.Height <= bmp.Height, "Height");
			Assert.IsTrue (r.X + r.Width <= bmp.Width, "Width");
			TextureBrush b = new TextureBrush (bmp, r);
			using (Image img = b.Image) {
				Assert.AreEqual (r.Height, img.Height, "Image.Height");
				Assert.AreEqual (r.Width, img.Width, "Image.Width");
			}
			Assert.IsTrue (b.Transform.IsIdentity, "Transform.IsIdentity");
			Assert.AreEqual (WrapMode.Tile, b.WrapMode, "WrapMode");
		}

		[Test]
		public void TextureBush_RectangleOutsideBitmap ()
		{
			Rectangle r = new Rectangle (50, 50, 50, 50);
			Assert.IsFalse (r.Y + r.Height <= bmp.Height, "Height");
			Assert.IsFalse (r.X + r.Width <= bmp.Width, "Width");
			Assert.Throws<OutOfMemoryException> (() => new TextureBrush (bmp, r));
		}

		[Test]
		public void Transform_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (image).Transform = null);
		}

		[Test]
		public void Transform ()
		{
			Matrix m = new Matrix ();
			TextureBrush t = new TextureBrush (image);
			t.Transform = m;
			Assert.IsFalse (Object.ReferenceEquals (m, t.Transform));
		}

		[Test]
		public void WrapMode_Valid ()
		{
			foreach (WrapMode wm in Enum.GetValues (typeof (WrapMode))) {
				TextureBrush t = new TextureBrush (image);
				t.WrapMode = wm;
				Assert.AreEqual (wm, t.WrapMode, wm.ToString ());
			}
		}

		[Test]
		public void WrapMode_Invalid ()
		{
			Assert.Throws<InvalidEnumArgumentException> (() => new TextureBrush (image).WrapMode = (WrapMode)Int32.MinValue);
		}

		[Test]
		public void Clone ()
		{
			TextureBrush t = new TextureBrush (image);
			TextureBrush clone = (TextureBrush) t.Clone ();
			Common (clone, t.WrapMode);
		}

		[Test]
		public void Dispose_Clone ()
		{
			TextureBrush t = new TextureBrush (image);
			t.Dispose ();
			Assert.Throws<ArgumentException> (() => t.Clone ());
		}

		[Test]
		public void Dispose_Dispose ()
		{
			TextureBrush t = new TextureBrush (image);
			t.Dispose ();
			t.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("NotDotNet")] // AccessViolationException under 2.0
		public void Dispose_Image ()
		{
			TextureBrush t = new TextureBrush (image);
			t.Dispose ();
			Assert.Throws<ArgumentException> (() => Assert.IsNotNull (t.Image, "Image"));
		}

		[Test]
		public void MultiplyTransform_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (image).MultiplyTransform (null));
		}

		[Test]
		public void MultiplyTransform_Null_Order ()
		{
			Assert.Throws<ArgumentNullException> (() => new TextureBrush (image).MultiplyTransform (null, MatrixOrder.Append));
		}

		[Test]
		public void MultiplyTransformOrder_Invalid ()
		{
			TextureBrush t = new TextureBrush (image);
			t.MultiplyTransform (new Matrix (), (MatrixOrder) Int32.MinValue);
		}

		[Test]
		public void MultiplyTransform_NonInvertible ()
		{
			TextureBrush t = new TextureBrush (image);
			Matrix noninvertible = new Matrix (123, 24, 82, 16, 47, 30);
			Assert.Throws<ArgumentException> (() => t.MultiplyTransform (noninvertible));
		}

		[Test]
		public void ResetTransform ()
		{
			TextureBrush t = new TextureBrush (image);
			t.RotateTransform (90);
			Assert.IsFalse (t.Transform.IsIdentity, "Transform.IsIdentity");
			t.ResetTransform ();
			Assert.IsTrue (t.Transform.IsIdentity, "Reset.IsIdentity");
		}

		[Test]
		public void RotateTransform ()
		{
			TextureBrush t = new TextureBrush (image);
			t.RotateTransform (90);
			float[] elements = t.Transform.Elements;
			Assert.AreEqual (0, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (1, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (-1, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (0, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

			t.RotateTransform (270);
			Assert.IsTrue (t.Transform.IsIdentity, "Transform.IsIdentity");
		}

		[Test]
		public void RotateTransform_InvalidOrder ()
		{
			TextureBrush t = new TextureBrush (image);
			Assert.Throws<ArgumentException> (() => t.RotateTransform (720, (MatrixOrder) Int32.MinValue));
		}

		[Test]
		public void ScaleTransform ()
		{
			TextureBrush t = new TextureBrush (image);
			t.ScaleTransform (2, 4);
			float[] elements = t.Transform.Elements;
			Assert.AreEqual (2, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (4, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");

			t.ScaleTransform (0.5f, 0.25f);
			Assert.IsTrue (t.Transform.IsIdentity, "Transform.IsIdentity");
		}

		[Test]
		public void ScaleTransform_MaxMin ()
		{
			TextureBrush t = new TextureBrush (image);
			t.ScaleTransform (Single.MaxValue, Single.MinValue);
			float[] elements = t.Transform.Elements;
			Assert.AreEqual (Single.MaxValue, elements[0], 1e33, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (Single.MinValue, elements[3], 1e33, "matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "matrix.5");
		}

		[Test]
		public void ScaleTransform_InvalidOrder ()
		{
			TextureBrush t = new TextureBrush (image);
			Assert.Throws<ArgumentException> (() => t.ScaleTransform (1, 1, (MatrixOrder) Int32.MinValue));
		}

		[Test]
		public void TranslateTransform ()
		{
			TextureBrush t = new TextureBrush (image);
			t.TranslateTransform (1, 1);
			float[] elements = t.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "matrix.3");
			Assert.AreEqual (1, elements[4], 0.1, "matrix.4");
			Assert.AreEqual (1, elements[5], 0.1, "matrix.5");

			t.TranslateTransform (-1, -1);
			elements = t.Transform.Elements;
			Assert.AreEqual (1, elements[0], 0.1, "revert.matrix.0");
			Assert.AreEqual (0, elements[1], 0.1, "revert.matrix.1");
			Assert.AreEqual (0, elements[2], 0.1, "revert.matrix.2");
			Assert.AreEqual (1, elements[3], 0.1, "revert.matrix.3");
			Assert.AreEqual (0, elements[4], 0.1, "revert.matrix.4");
			Assert.AreEqual (0, elements[5], 0.1, "revert.matrix.5");
		}

		[Test]
		public void TranslateTransform_InvalidOrder ()
		{
			TextureBrush t = new TextureBrush (image);
			Assert.Throws<ArgumentException> (() => t.TranslateTransform (1, 1, (MatrixOrder) Int32.MinValue));
		}

		private void Alpha_81828 (WrapMode mode, bool equals)
		{
			using (Bitmap bm = new Bitmap (2, 1)) {
				using (Graphics g = Graphics.FromImage (bm)) {
					Color t = Color.FromArgb (128, Color.White);
					g.Clear (Color.Green);
					g.FillRectangle (new SolidBrush (t), 0, 0, 1, 1);
					using (Bitmap bm_for_brush = new Bitmap (1, 1)) {
						bm_for_brush.SetPixel (0, 0, t);
						using (TextureBrush tb = new TextureBrush (bm_for_brush, mode)) {
							g.FillRectangle (tb, 1, 0, 1, 1);
						}
					}
					Color c1 = bm.GetPixel (0, 0);
					Color c2 = bm.GetPixel (1, 0);
					if (equals)
						Assert.AreEqual (c1, c2);
					else
						Assert.AreEqual (-16744448, c2.ToArgb (), "Green");
				}
			}
		}

		[Test]
		public void Alpha_81828_Clamp ()
		{
			Alpha_81828 (WrapMode.Clamp, false);
		}

		[Test]
		public void Alpha_81828_Tile ()
		{
			Alpha_81828 (WrapMode.Tile, true);
		}

		[Test]
		public void Alpha_81828_TileFlipX ()
		{
			Alpha_81828 (WrapMode.TileFlipX, true);
		}

		[Test]
		public void Alpha_81828_TileFlipY ()
		{
			Alpha_81828 (WrapMode.TileFlipY, true);
		}

		[Test]
		public void Alpha_81828_TileFlipXY ()
		{
			Alpha_81828 (WrapMode.TileFlipXY, true);
		}
	}
}
