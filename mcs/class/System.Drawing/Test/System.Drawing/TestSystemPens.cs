// Tests for System.Drawing.SystemPens.cs
//
// Author: Ravindra (rkumar@novell.com)
//
// Copyright (c) 2004 Novell, Inc. http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class SystemPensTest : Assertion
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
				Fail ("M17: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("M17", e is ArgumentException);
			}
		}

		// helper test functions
		void CheckProperties (Pen pen, String tag, Color sysColor)
		{
			// Try modifying properties of a SystemPen.
			// ArgumentException must be thrown.

			Assert (tag + "#1", pen.Color.IsSystemColor);
			AssertEquals (tag + "#1", sysColor, pen.Color);

			try {
				pen.Alignment = PenAlignment.Center;
				Fail (tag + "#2: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#2", e is ArgumentException);
			}

			try {
				pen.Brush = new SolidBrush(Color.Red);
				Fail (tag + "#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#3", e is ArgumentException);
			}

			try {
				pen.Color = Color.Red;
				Fail (tag + "#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#4", e is ArgumentException);
			}

			try {
				pen.Color = sysColor;
				Fail (tag + "#5" + ": must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#5", e is ArgumentException);
			}
/*
			try {
				// NotImplemented
				pen.CompoundArray = new float[2];
				Fail (tag + "#6: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#6", e is ArgumentException);
			}

			try {
				// NotImplemented
				pen.CustomEndCap = null;
				Fail (tag + "#7: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#7", e is ArgumentException);
			}

			try {
				// NotImplemented
				pen.CustomStartCap = null;
				Fail (tag + "#8: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#8", e is ArgumentException);
			}

			try {
				// NotImplemented
				pen.DashCap = DashCap.Flat;
				Fail (tag + "#9: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#9", e is ArgumentException);
			}
*/
			try {
				pen.DashOffset = 5.5F;
				Fail (tag + "#10: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#10", e is ArgumentException);
			}

			try {
				pen.DashPattern = null;
				Fail (tag + "#11: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#11", e is ArgumentException);
			}

			try {
				pen.DashStyle = DashStyle.Dot; // hangs!prob
				Fail (tag + "#12: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#12", e is ArgumentException);
			}
/*
			try {
				// NotImplemented
				pen.EndCap = LineCap.Round;
				Fail (tag + "#13: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#13", e is ArgumentException);
			}
*/
			try {
				pen.LineJoin = LineJoin.Round;
				Fail (tag + "#14: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#14", e is ArgumentException);
			}

			try {
				pen.MiterLimit = 0.1f;
				Fail (tag + "#15: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#15", e is ArgumentException);
			}
/*
			try {
				// NotImplemented
				pen.StartCap = LineCap.Square;
				Fail (tag + "#16: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#16", e is ArgumentException);
			}
*/
			try {
				pen.Transform = new Matrix (); //Matrix hangs!problem
				Fail (tag + "#17: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#17", e is ArgumentException);
			}

			try {
				pen.Width = 0.5F;
				Fail (tag + "#18: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#18", e is ArgumentException);
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
				Fail (tag + "#1: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#1", e is ArgumentException);
			}
*/
			try {
				pen.ResetTransform ();
			} catch (Exception e) {
				Fail (tag + "#2: unexpected Exception");
			}

			try {
				pen.RotateTransform (90);
			} catch (Exception e) {
				Fail (tag + "#3: unexpected Exception");
			}

			try {
				pen.ScaleTransform (2, 1);
			} catch (Exception e) {
				Fail (tag + "#4: unexpected Exception");
			}

			try {
				pen.TranslateTransform (10, 20);
			} catch (Exception e) {
				Fail (tag + "#5: unexpected Exception");
			}

			try {
				pen.MultiplyTransform (new Matrix ());
			} catch (Exception e) {
				Fail (tag + "#6: unexpected Exception");
			}

			try {
				pen.Clone ();
			} catch (Exception e) {
				Fail (tag + "#7: unexpected Exception");
			}

			try {
				pen.Dispose ();
				Fail (tag + "#8: must throw ArgumentException");
			} catch (Exception e) {
				Assert (tag + "#8", e is ArgumentException);
			}
		}
	}
}
