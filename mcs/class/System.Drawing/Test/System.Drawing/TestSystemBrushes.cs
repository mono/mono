// Tests for System.Drawing.SystemBrushes.cs
//
// Author: Ravindra (rkumar@novell.com)
//
// Copyright (c) 2004 Novell, Inc. http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Drawing;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class SystemBrushesTest : Assertion
	{
		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp () {}

		[Test]
		public void TestActiveBorder ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ActiveBorder;
			Assert ("P1#1", brush.Color.IsSystemColor);
			AssertEquals ("P1#2", SystemColors.ActiveBorder, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P1#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ActiveBorder;
			} catch (Exception e) {
				Assert ("P1#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P1#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestActiveCaption ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ActiveCaption;
			Assert ("P2#1", brush.Color.IsSystemColor);
			AssertEquals ("P2#2", SystemColors.ActiveCaption, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P2#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ActiveCaption;
			} catch (Exception e) {
				Assert ("P2#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P2#5", e is ArgumentException);
			}

		}

		[Test]
		public void TestActiveCaptionText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ActiveCaptionText;
			Assert ("P3#1", brush.Color.IsSystemColor);
			AssertEquals ("P3#2", SystemColors.ActiveCaptionText, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P3#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ActiveCaptionText;
			} catch (Exception e) {
				Assert ("P3#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P3#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestAppWorkspace ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.AppWorkspace;
			Assert ("P4#1", brush.Color.IsSystemColor);
			AssertEquals ("P4#2", SystemColors.AppWorkspace, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P4#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.AppWorkspace;
			} catch (Exception e) {
				Assert ("P4#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P4#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestControl ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Control;
			Assert ("P5#1", brush.Color.IsSystemColor);
			AssertEquals ("P5#2", SystemColors.Control, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P5#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Control;
			} catch (Exception e) {
				Assert ("P5#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P5#5", e is ArgumentException);
			}

		}

		[Test]
		public void TestControlDark ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlDark;
			Assert ("P6#1", brush.Color.IsSystemColor);
			AssertEquals ("P6#2", SystemColors.ControlDark, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P6#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlDark;
			} catch (Exception e) {
				Assert ("P6#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P6#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestControlDarkDark ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlDarkDark;
			Assert ("P7#1", brush.Color.IsSystemColor);
			AssertEquals ("P7#2", SystemColors.ControlDarkDark, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P7#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlDarkDark;
			} catch (Exception e) {
				Assert ("P7#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P7#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestControlLight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlLight;
			Assert ("P8#1", brush.Color.IsSystemColor);
			AssertEquals ("P8#2", SystemColors.ControlLight, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P8#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlLight;
			} catch (Exception e) {
				Assert ("P8#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P8#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestControlLightLight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlLightLight;
			Assert ("P9#1", brush.Color.IsSystemColor);
			AssertEquals ("P9#2", SystemColors.ControlLightLight, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P9#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlLightLight;
			} catch (Exception e) {
				Assert ("P9#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P9#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestControlText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlText;
			Assert ("P10#1", brush.Color.IsSystemColor);
			AssertEquals ("P10#2", SystemColors.ControlText, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P10#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlText;
			} catch (Exception e) {
				Assert ("P10#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P10#5", e is ArgumentException);
			}
		}


		[Test]
		public void TestDesktop ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Desktop;
			Assert ("P11#1", brush.Color.IsSystemColor);
			AssertEquals ("P11#2", SystemColors.Desktop, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P11#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Desktop;
			} catch (Exception e) {
				Assert ("P11#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P11#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestHighlight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Highlight;
			Assert ("P12#1", brush.Color.IsSystemColor);
			AssertEquals ("P12#2", SystemColors.Highlight, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P12#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Highlight;
			} catch (Exception e) {
				Assert ("P12#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P12#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestHighlightText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.HighlightText;
			Assert ("P13#1", brush.Color.IsSystemColor);
			AssertEquals ("P13#2", SystemColors.HighlightText, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P13#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.HighlightText;
			} catch (Exception e) {
				Assert ("P13#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P13#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestHotTrack ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.HotTrack;
			Assert ("P14#1", brush.Color.IsSystemColor);
			AssertEquals ("P14#2", SystemColors.HotTrack, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P14#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.HotTrack;
			} catch (Exception e) {
				Assert ("P14#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P14#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestInactiveBorder ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.InactiveBorder;
			Assert ("P15#1", brush.Color.IsSystemColor);
			AssertEquals ("P15#2", SystemColors.InactiveBorder, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P15#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.InactiveBorder;
			} catch (Exception e) {
				Assert ("P15#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P15#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestInactiveCaption ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.InactiveCaption;
			Assert ("P16#1", brush.Color.IsSystemColor);
			AssertEquals ("P16#2", SystemColors.InactiveCaption, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P16#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.InactiveCaption;
			} catch (Exception e) {
				Assert ("P16#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P16#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestInfo ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Info;
			Assert ("P17#1", brush.Color.IsSystemColor);
			AssertEquals ("P17#2", SystemColors.Info, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P17#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Info;
			} catch (Exception e) {
				Assert ("P17#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P17#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestMenu ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Menu;
			Assert ("P18#1", brush.Color.IsSystemColor);
			AssertEquals ("P18#2", SystemColors.Menu, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P18#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Menu;
			} catch (Exception e) {
				Assert ("P18#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P18#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestScrollBar ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ScrollBar;
			Assert ("P19#1", brush.Color.IsSystemColor);
			AssertEquals ("P19#2", SystemColors.ScrollBar, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P19#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ScrollBar;
			} catch (Exception e) {
				Assert ("P19#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P19#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestWindow ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Window;
			Assert ("P20#1", brush.Color.IsSystemColor);
			AssertEquals ("P20#2", SystemColors.Window, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P20#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Window;
			} catch (Exception e) {
				Assert ("P20#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P20#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestWindowText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.WindowText;
			Assert ("P21#1", brush.Color.IsSystemColor);
			AssertEquals ("P21#2", SystemColors.WindowText, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("P21#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.WindowText;
			} catch (Exception e) {
				Assert ("P21#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("P21#5", e is ArgumentException);
			}
		}

		[Test]
		public void TestFromSystemColor ()
		{
			SolidBrush brush;

			brush = (SolidBrush) SystemBrushes.FromSystemColor (SystemColors.Menu);
			AssertEquals ("M1#1", SystemColors.Menu, brush.Color);

			try {
				brush.Color = Color.Red;
			} catch (Exception e) {
				Assert ("M1#2", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Menu;
			} catch (Exception e) {
				Assert ("M1#3", e is ArgumentException);
			}

			try {
				brush.Dispose();
			} catch (Exception e) {
				Assert ("M1#4", e is ArgumentException);
			}


			try {
				brush = (SolidBrush) SystemBrushes.FromSystemColor (Color.Red);
			} catch (Exception e) {
				Assert ("M2#1", e is ArgumentException);
			}
		}
	}
}
