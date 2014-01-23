//
// System.Drawing.SolidBrush unit tests
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
using System.Drawing;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class SolidBrushTest {

		[Test]
		public void Transparent ()
		{
			SolidBrush sb = new SolidBrush (Color.Transparent);
			Assert.AreEqual (Color.Transparent, sb.Color, "Color");
			sb.Color = Color.Empty;
			SolidBrush clone = (SolidBrush) sb.Clone ();
			sb.Dispose ();
			Assert.AreEqual (Color.Empty.ToArgb (), clone.Color.ToArgb (), "Clone.Color");
		}

		[Test]
		public void Dispose_Color ()
		{
			SolidBrush sb = new SolidBrush (Color.Transparent);
			sb.Dispose ();
			Assert.AreEqual (Color.Transparent, sb.Color, "Color");
			// no exception - the call probably doesn't get to gdi+
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Dispose_Clone ()
		{
			SolidBrush sb = new SolidBrush (Color.Transparent);
			sb.Dispose ();
			sb.Clone ();
		}

		[Test]
		public void Dispose_Dispose ()
		{
			SolidBrush sb = new SolidBrush (Color.Transparent);
			sb.Dispose ();
			sb.Dispose ();
		}

		[Test]
		public void FillRectangle ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					SolidBrush sb = new SolidBrush (Color.Red);
					g.FillRectangle (sb, 0, 0, 9, 9);
					sb.Color = Color.Blue;
					g.FillRectangle (sb, 4, 4, 5, 5);
				}
				Assert.AreEqual (Color.Red.ToArgb (), bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (Color.Blue.ToArgb (), bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (0, bmp.GetPixel (9, 9).ToArgb (), "9,9");
			}
		}

		[Test]
		public void DrawLine ()
		{
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					SolidBrush sb = new SolidBrush (Color.Red);
					Pen p = new Pen (sb);
					g.DrawLine (p, 0, 0, 9, 9);
					sb.Color = Color.Blue;
					g.DrawLine (p, 8, 8, 4, 4); // pen is still red
				}
				Assert.AreEqual (Color.Red.ToArgb (), bmp.GetPixel (0, 0).ToArgb (), "0,0");
				Assert.AreEqual (Color.Red.ToArgb (), bmp.GetPixel (8, 8).ToArgb (), "8,8");
				Assert.AreEqual (Color.Red.ToArgb (), bmp.GetPixel (9, 9).ToArgb (), "9,9"); // include end point
			}
			using (Bitmap bmp = new Bitmap (10, 10)) {
				using (Graphics g = Graphics.FromImage (bmp)) {
					SolidBrush sb = new SolidBrush (Color.Red);
					Pen p = new Pen (sb);
					g.DrawLine (p, float.NaN, float.NaN, 9, 9);
				}
				Assert.AreNotEqual (Color.Red.ToArgb (), bmp.GetPixel (0, 0).ToArgb (), "#DrawLine: Drew Line with NaN value when shouldn't have");
			}
		}

		[Test]
		public void Clone ()
		{
			using (SolidBrush sb = new SolidBrush (Color.Transparent)) {
				// we still get a "named" color
				Assert.AreEqual (Color.Transparent, sb.Color, "Color");
				using (SolidBrush clone = (SolidBrush) sb.Clone ()) {
					// but not after cloning the brush
					Assert.IsFalse (Color.Transparent.Equals (clone.Color), "Color-Clone-Unnamed");
					Assert.AreEqual (Color.Transparent.ToArgb (), clone.Color.ToArgb (), "Color-Clone-Argb");
				}
			}
		}
	}
}
