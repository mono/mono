// Tests for System.Drawing.SystemBrushes.cs
//
// Author: Ravindra (rkumar@novell.com)
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
using System.Security.Permissions;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
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
				Fail ("P1#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P1#3", true);
			}

			try {
				brush.Color = SystemColors.ActiveBorder;
				Fail ("P1#4: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P1#4", true);
			}

			try {
				brush.Dispose();
				Fail ("P1#5: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P1#5", true);
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
				Fail ("P2#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P2#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ActiveCaption;
				Fail ("P2#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P2#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P2#5: must throw ArgumentException");
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
				Fail ("P3#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P3#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ActiveCaptionText;
				Fail ("P3#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P3#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P3#5: must throw ArgumentException");
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
				Fail ("P4#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P4#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.AppWorkspace;
				Fail ("P4#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P4#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P4#5: must throw ArgumentException");
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
				Fail ("P5#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P5#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Control;
				Fail ("P5#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P5#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P5#5: must throw ArgumentException");
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
				Fail ("P6#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P6#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlDark;
				Fail ("P6#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P6#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P6#5: must throw ArgumentException");
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
				Fail ("P7#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P7#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlDarkDark;
				Fail ("P7#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P7#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P7#5: must throw ArgumentException");
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
				Fail ("P8#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P8#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlLight;
				Fail ("P8#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P8#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P8#5: must throw ArgumentException");
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
				Fail ("P9#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P9#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlLightLight;
				Fail ("P9#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P9#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P9#5: must throw ArgumentException");
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
				Fail ("P10#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P10#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ControlText;
				Fail ("P10#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P10#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P10#5: must throw ArgumentException");
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
				Fail ("P11#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P11#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Desktop;
				Fail ("P11#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P11#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P11#5: must throw ArgumentException");
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
				Fail ("P12#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P12#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Highlight;
				Fail ("P12#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P12#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P12#5: must throw ArgumentException");
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
				Fail ("P13#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P13#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.HighlightText;
				Fail ("P13#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P13#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P13#5: must throw ArgumentException");
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
				Fail ("P14#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P14#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.HotTrack;
				Fail ("P14#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P14#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P14#5: must throw ArgumentException");
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
				Fail ("P15#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P15#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.InactiveBorder;
				Fail ("P15#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P15#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P15#5: must throw ArgumentException");
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
				Fail ("P16#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P16#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.InactiveCaption;
				Fail ("P16#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P16#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P16#5: must throw ArgumentException");
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
				Fail ("P17#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P17#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Info;
				Fail ("P17#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P17#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P17#5: must throw ArgumentException");
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
				Fail ("P18#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P18#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Menu;
				Fail ("P18#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P18#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P18#5: must throw ArgumentException");
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
				Fail ("P19#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P19#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.ScrollBar;
				Fail ("P19#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P19#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P19#5: must throw ArgumentException");
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
				Fail ("P20#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P20#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Window;
				Fail ("P20#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P20#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P20#5: must throw ArgumentException");
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
				Fail ("P21#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P21#3", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.WindowText;
				Fail ("P21#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("P21#4", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("P21#5: must throw ArgumentException");
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
				Fail ("M1#2: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("M1#2", e is ArgumentException);
			}

			try {
				brush.Color = SystemColors.Menu;
				Fail ("M1#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("M1#3", e is ArgumentException);
			}

			try {
				brush.Dispose();
				Fail ("M1#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("M1#4", e is ArgumentException);
			}


			try {
				brush = (SolidBrush) SystemBrushes.FromSystemColor (Color.Red);
				Fail ("M2#1: must throw ArgumentException");
			} catch (Exception e) {
				Assert ("M2#1", e is ArgumentException);
			}
		}
	}
}
