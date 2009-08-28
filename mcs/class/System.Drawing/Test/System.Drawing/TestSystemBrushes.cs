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
	public class SystemBrushesTest
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
			Assert.IsTrue (brush.Color.IsSystemColor, "P1#1");
			Assert.AreEqual (SystemColors.ActiveBorder, brush.Color, "P1#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P1#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, "P1#3");
			}

			try {
				brush.Color = SystemColors.ActiveBorder;
				Assert.Fail ("P1#4: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, "P1#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P1#5: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert.IsTrue (true, "P1#5");
			}
		}

		[Test]
		public void TestActiveCaption ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ActiveCaption;
			Assert.IsTrue (brush.Color.IsSystemColor, "P2#1");
			Assert.AreEqual (SystemColors.ActiveCaption, brush.Color, "P2#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P2#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P2#3");
			}

			try {
				brush.Color = SystemColors.ActiveCaption;
				Assert.Fail ("P2#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P2#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P2#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P2#5");
			}

		}

		[Test]
		public void TestActiveCaptionText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ActiveCaptionText;
			Assert.IsTrue (brush.Color.IsSystemColor, "P3#1");
			Assert.AreEqual (SystemColors.ActiveCaptionText, brush.Color, "P3#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P3#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P3#3");
			}

			try {
				brush.Color = SystemColors.ActiveCaptionText;
				Assert.Fail ("P3#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P3#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P3#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P3#5");
			}
		}

		[Test]
		public void TestAppWorkspace ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.AppWorkspace;
			Assert.IsTrue (brush.Color.IsSystemColor, "P4#1");
			Assert.AreEqual (SystemColors.AppWorkspace, brush.Color, "P4#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P4#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P4#3");
			}

			try {
				brush.Color = SystemColors.AppWorkspace;
				Assert.Fail ("P4#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P4#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P4#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P4#5");
			}
		}

		[Test]
		public void TestControl ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Control;
			Assert.IsTrue (brush.Color.IsSystemColor, "P5#1");
			Assert.AreEqual (SystemColors.Control, brush.Color, "P5#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P5#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P5#3");
			}

			try {
				brush.Color = SystemColors.Control;
				Assert.Fail ("P5#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P5#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P5#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P5#5");
			}

		}

		[Test]
		public void TestControlDark ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlDark;
			Assert.IsTrue (brush.Color.IsSystemColor, "P6#1");
			Assert.AreEqual (SystemColors.ControlDark, brush.Color, "P6#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P6#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P6#3");
			}

			try {
				brush.Color = SystemColors.ControlDark;
				Assert.Fail ("P6#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P6#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P6#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P6#5");
			}
		}

		[Test]
		public void TestControlDarkDark ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlDarkDark;
			Assert.IsTrue (brush.Color.IsSystemColor, "P7#1");
			Assert.AreEqual (SystemColors.ControlDarkDark, brush.Color, "P7#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P7#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P7#3");
			}

			try {
				brush.Color = SystemColors.ControlDarkDark;
				Assert.Fail ("P7#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P7#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P7#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P7#5");
			}
		}

		[Test]
		public void TestControlLight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlLight;
			Assert.IsTrue (brush.Color.IsSystemColor, "P8#1");
			Assert.AreEqual (SystemColors.ControlLight, brush.Color, "P8#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P8#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P8#3");
			}

			try {
				brush.Color = SystemColors.ControlLight;
				Assert.Fail ("P8#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P8#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P8#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P8#5");
			}
		}

		[Test]
		public void TestControlLightLight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlLightLight;
			Assert.IsTrue (brush.Color.IsSystemColor, "P9#1");
			Assert.AreEqual (SystemColors.ControlLightLight, brush.Color, "P9#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P9#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P9#3");
			}

			try {
				brush.Color = SystemColors.ControlLightLight;
				Assert.Fail ("P9#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P9#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P9#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P9#5");
			}
		}

		[Test]
		public void TestControlText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ControlText;
			Assert.IsTrue (brush.Color.IsSystemColor, "P10#1");
			Assert.AreEqual (SystemColors.ControlText, brush.Color, "P10#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P10#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P10#3");
			}

			try {
				brush.Color = SystemColors.ControlText;
				Assert.Fail ("P10#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P10#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P10#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P10#5");
			}
		}


		[Test]
		public void TestDesktop ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Desktop;
			Assert.IsTrue (brush.Color.IsSystemColor, "P11#1");
			Assert.AreEqual (SystemColors.Desktop, brush.Color, "P11#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P11#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P11#3");
			}

			try {
				brush.Color = SystemColors.Desktop;
				Assert.Fail ("P11#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P11#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P11#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P11#5");
			}
		}

		[Test]
		public void TestHighlight ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Highlight;
			Assert.IsTrue (brush.Color.IsSystemColor, "P12#1");
			Assert.AreEqual (SystemColors.Highlight, brush.Color, "P12#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P12#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P12#3");
			}

			try {
				brush.Color = SystemColors.Highlight;
				Assert.Fail ("P12#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P12#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P12#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P12#5");
			}
		}

		[Test]
		public void TestHighlightText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.HighlightText;
			Assert.IsTrue (brush.Color.IsSystemColor, "P13#1");
			Assert.AreEqual (SystemColors.HighlightText, brush.Color, "P13#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P13#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P13#3");
			}

			try {
				brush.Color = SystemColors.HighlightText;
				Assert.Fail ("P13#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P13#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P13#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P13#5");
			}
		}

		[Test]
		public void TestHotTrack ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.HotTrack;
			Assert.IsTrue (brush.Color.IsSystemColor, "P14#1");
			Assert.AreEqual (SystemColors.HotTrack, brush.Color, "P14#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P14#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P14#3");
			}

			try {
				brush.Color = SystemColors.HotTrack;
				Assert.Fail ("P14#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P14#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P14#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P14#5");
			}
		}

		[Test]
		public void TestInactiveBorder ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.InactiveBorder;
			Assert.IsTrue (brush.Color.IsSystemColor, "P15#1");
			Assert.AreEqual (SystemColors.InactiveBorder, brush.Color, "P15#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P15#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P15#3");
			}

			try {
				brush.Color = SystemColors.InactiveBorder;
				Assert.Fail ("P15#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P15#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P15#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P15#5");
			}
		}

		[Test]
		public void TestInactiveCaption ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.InactiveCaption;
			Assert.IsTrue (brush.Color.IsSystemColor, "P16#1");
			Assert.AreEqual (SystemColors.InactiveCaption, brush.Color, "P16#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P16#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P16#3");
			}

			try {
				brush.Color = SystemColors.InactiveCaption;
				Assert.Fail ("P16#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P16#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P16#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P16#5");
			}
		}

		[Test]
		public void TestInfo ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Info;
			Assert.IsTrue (brush.Color.IsSystemColor, "P17#1");
			Assert.AreEqual (SystemColors.Info, brush.Color, "P17#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P17#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P17#3");
			}

			try {
				brush.Color = SystemColors.Info;
				Assert.Fail ("P17#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P17#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P17#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P17#5");
			}
		}

		[Test]
		public void TestMenu ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Menu;
			Assert.IsTrue (brush.Color.IsSystemColor, "P18#1");
			Assert.AreEqual (SystemColors.Menu, brush.Color, "P18#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P18#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P18#3");
			}

			try {
				brush.Color = SystemColors.Menu;
				Assert.Fail ("P18#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P18#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P18#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P18#5");
			}
		}

		[Test]
		public void TestScrollBar ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.ScrollBar;
			Assert.IsTrue (brush.Color.IsSystemColor, "P19#1");
			Assert.AreEqual (SystemColors.ScrollBar, brush.Color, "P19#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P19#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P19#3");
			}

			try {
				brush.Color = SystemColors.ScrollBar;
				Assert.Fail ("P19#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P19#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P19#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P19#5");
			}
		}

		[Test]
		public void TestWindow ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.Window;
			Assert.IsTrue (brush.Color.IsSystemColor, "P20#1");
			Assert.AreEqual (SystemColors.Window, brush.Color, "P20#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P20#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P20#3");
			}

			try {
				brush.Color = SystemColors.Window;
				Assert.Fail ("P20#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P20#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P20#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P20#5");
			}
		}

		[Test]
		public void TestWindowText ()
		{
			SolidBrush brush;
			brush = (SolidBrush) SystemBrushes.WindowText;
			Assert.IsTrue (brush.Color.IsSystemColor, "P21#1");
			Assert.AreEqual (SystemColors.WindowText, brush.Color, "P21#2");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("P21#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P21#3");
			}

			try {
				brush.Color = SystemColors.WindowText;
				Assert.Fail ("P21#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P21#4");
			}

			try {
				brush.Dispose();
				Assert.Fail ("P21#5: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "P21#5");
			}
		}

		[Test]
		public void TestFromSystemColor ()
		{
			SolidBrush brush;

			brush = (SolidBrush) SystemBrushes.FromSystemColor (SystemColors.Menu);
			Assert.AreEqual (SystemColors.Menu, brush.Color, "M1#1");

			try {
				brush.Color = Color.Red;
				Assert.Fail ("M1#2: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "M1#2");
			}

			try {
				brush.Color = SystemColors.Menu;
				Assert.Fail ("M1#3: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "M1#3");
			}

			try {
				brush.Dispose();
				Assert.Fail ("M1#4: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "M1#4");
			}


			try {
				brush = (SolidBrush) SystemBrushes.FromSystemColor (Color.Red);
				Assert.Fail ("M2#1: must throw ArgumentException");
			} catch (Exception e) {
				Assert.IsTrue (e is ArgumentException, "M2#1");
			}
		}
	}
}
