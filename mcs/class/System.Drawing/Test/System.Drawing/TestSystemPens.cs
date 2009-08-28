// Tests for System.Drawing.SystemPens.cs
//
// Author: 
//     Ravindra (rkumar@novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class SystemPensTest
	{
		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp () {}

		[Test]
		public void TestActiveCaptionText ()
		{
			Pen pen;
			pen = SystemPens.ActiveCaptionText;
			CheckProperties (pen, "P1", SystemColors.ActiveCaptionText);
			CheckMethods (pen, "M1");
		}

		[Test]
		public void TestControl ()
		{
			Pen pen;
			pen = SystemPens.Control;
			CheckProperties (pen, "P2", SystemColors.Control);
			CheckMethods (pen, "M2");
		}

		[Test]
		public void TestControlDark ()
		{
			Pen pen;
			pen = SystemPens.ControlDark;
			CheckProperties (pen, "P3", SystemColors.ControlDark);
			CheckMethods (pen, "M3");
		}

		[Test]
		public void TestControlDarkDark ()
		{
			Pen pen;
			pen = SystemPens.ControlDarkDark;
			CheckProperties (pen, "P4", SystemColors.ControlDarkDark);
			CheckMethods (pen, "M4");
		}

		[Test]
		public void TestControlLight ()
		{
			Pen pen;
			pen = SystemPens.ControlLight;
			CheckProperties (pen, "P5", SystemColors.ControlLight);
			CheckMethods (pen, "M5");
		}

		[Test]
		public void TestControlLightLight ()
		{
			Pen pen;
			pen = SystemPens.ControlLightLight;
			CheckProperties (pen, "P6", SystemColors.ControlLightLight);
			CheckMethods (pen, "M6");
		}

		[Test]
		public void TestControlText ()
		{
			Pen pen;
			pen = SystemPens.ControlText;
			CheckProperties (pen, "P7", SystemColors.ControlText);
			CheckMethods (pen, "M7");
		}

		[Test]
		public void TestGrayText ()
		{
			Pen pen;
			pen = SystemPens.GrayText;
			CheckProperties (pen, "P8", SystemColors.GrayText);
			CheckMethods (pen, "M8");
		}

		[Test]
		public void TestHighlight ()
		{
			Pen pen;
			pen = SystemPens.Highlight;
			CheckProperties (pen, "P9", SystemColors.Highlight);
			CheckMethods (pen, "M9");
		}

		[Test]
		public void TestHighlightText ()
		{
			Pen pen;
			pen = SystemPens.HighlightText;
			CheckProperties (pen, "P10", SystemColors.HighlightText);
			CheckMethods (pen, "M10");
		}

		[Test]
		public void TestInactiveCaptionText ()
		{
			Pen pen;
			pen = SystemPens.InactiveCaptionText;
			CheckProperties (pen, "P11", SystemColors.InactiveCaptionText);
			CheckMethods (pen, "M11");
		}

		[Test]
		public void TestInfoText ()
		{
			Pen pen;
			pen = SystemPens.InfoText;
			CheckProperties (pen, "P12", SystemColors.InfoText);
			CheckMethods (pen, "M12");
		}

		[Test]
		public void TestMenuText ()
		{
			Pen pen;
			pen = SystemPens.MenuText;
			CheckProperties (pen, "P13", SystemColors.MenuText);
			CheckMethods (pen, "M13");
		}

		[Test]
		public void TestWindowFrame ()
		{
			Pen pen;
			pen = SystemPens.WindowFrame;
			CheckProperties (pen, "P14", SystemColors.WindowFrame);
			CheckMethods (pen, "M14");
		}

		[Test]
		public void TestWindowText ()
		{
			Pen pen;
			pen = SystemPens.WindowText;
			CheckProperties (pen, "P15", SystemColors.WindowText);
			CheckMethods (pen, "M15");
		}

		[Test]
		public void TestFromSystemColor ()
		{
			Pen pen;

			pen = SystemPens.FromSystemColor (SystemColors.MenuText);
			CheckProperties (pen, "P16", SystemColors.MenuText);
			CheckMethods (pen, "M16");

			try {
				pen = SystemPens.FromSystemColor (Color.Red);
				Assert.Fail ("M17: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, "M17");
			}
		}

		// helper test functions
		void CheckProperties (Pen pen, String tag, Color sysColor)
		{
			// Try modifying properties of a SystemPen.
			// ArgumentException must be thrown.

			Assert.IsTrue (pen.Color.IsSystemColor, tag + "#1");
			Assert.AreEqual (sysColor, pen.Color, tag + "#1");

			try {
				pen.Alignment = PenAlignment.Center;
				Assert.Fail (tag + "#2: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#2");
			}

			try {
				pen.Brush = new SolidBrush(Color.Red);
				Assert.Fail (tag + "#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#3");
			}

			try {
				pen.Color = Color.Red;
				Assert.Fail (tag + "#4: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#4");
			}

			try {
				pen.Color = sysColor;
				Assert.Fail (tag + "#5" + ": must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#5");
			}
/*
			try {
				// NotImplemented
				pen.CompoundArray = new float[2];
				Assert.Fail (tag + "#6: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#6");
			}

			try {
				// NotImplemented
				pen.CustomEndCap = null;
				Assert.Fail (tag + "#7: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#7");
			}

			try {
				// NotImplemented
				pen.CustomStartCap = null;
				Assert.Fail (tag + "#8: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#8");
			}

			try {
				// NotImplemented
				pen.DashCap = DashCap.Flat;
				Assert.Fail (tag + "#9: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#9");
			}
*/
			try {
				pen.DashOffset = 5.5F;
				Assert.Fail (tag + "#10: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#10");
			}

			try {
				pen.DashPattern = null;
				Assert.Fail (tag + "#11: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#11");
			}

			try {
				pen.DashStyle = DashStyle.Dot; // hangs!prob
				Assert.Fail (tag + "#12: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#12");
			}
/*
			try {
				// NotImplemented
				pen.EndCap = LineCap.Round;
				Assert.Fail (tag + "#13: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#13");
			}
*/
			try {
				pen.LineJoin = LineJoin.Round;
				Assert.Fail (tag + "#14: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#14");
			}

			try {
				pen.MiterLimit = 0.1f;
				Assert.Fail (tag + "#15: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#15");
			}
/*
			try {
				// NotImplemented
				pen.StartCap = LineCap.Square;
				Assert.Fail (tag + "#16: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#16");
			}
*/
			try {
				pen.Transform = new Matrix (); //Matrix hangs!problem
				Assert.Fail (tag + "#17: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#17");
			}

			try {
				pen.Width = 0.5F;
				Assert.Fail (tag + "#18: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#18");
			}
		}

		void CheckMethods (Pen pen, String tag)
		{
			// Try modifying a SystemPen by calling methods.
			// ArgumentException must be thrown in some cases.
/*
			try {
				// NotImplemented
				pen.SetLineCap (LineCap.Flat, LineCap.Round, DashCap.Triangle);
				Assert.Fail (tag + "#1: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (tag + "#1", true);
			}
*/
			pen.ResetTransform ();
			pen.RotateTransform (90);
			pen.ScaleTransform (2, 1);
			pen.TranslateTransform (10, 20);
			pen.MultiplyTransform (new Matrix ());
			pen.Clone ();

			try {
				pen.Dispose ();
				Assert.Fail (tag + "#8: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, tag + "#8");
			}
		}
	}
}
