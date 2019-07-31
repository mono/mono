//
// Tests for System.Web.UI.WebControls.WebColorConverter.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class WebColorConverterTest 
	{
		[Test]
		public void Basic () 
		{
			WebColorConverter	conv;
			Color			c;

			conv = new WebColorConverter();

			Assert.AreEqual(true, conv.CanConvertFrom(null, typeof(string)), "B1");
			Assert.AreEqual(true, conv.CanConvertTo(null, typeof(string)), "B2");
			Assert.AreEqual(false, conv.CanConvertFrom(null, typeof(int)), "B3");
			Assert.AreEqual(false, conv.CanConvertTo(null, typeof(int)), "B4");

			c = Color.Fuchsia;
			Assert.AreEqual("Fuchsia", conv.ConvertTo(null, null, c, typeof(string)), "B5");

			c = Color.FromArgb(1, 2, 3);
			Assert.AreEqual("#010203", conv.ConvertTo(null, null, c, typeof(string)), "B6");

			c = Color.FromArgb(5, 1, 2, 3);
			Assert.AreEqual("#010203", conv.ConvertTo(null, null, c, typeof(string)), "B7");

			c = Color.FromArgb(254, 253, 252);
			Assert.AreEqual("#FEFDFC", conv.ConvertTo(null, null, c, typeof(string)), "B8");

			c = Color.FromKnownColor(KnownColor.BlanchedAlmond);
			Assert.AreEqual("BlanchedAlmond", conv.ConvertTo(null, null, c, typeof(string)), "B9");

			c = Color.FromName("OingoBoingo");
			Assert.AreEqual("#000000", conv.ConvertTo(null, null, c, typeof(string)), "B10");

			Assert.AreEqual(Color.FromArgb(0, 0, 79, 80), conv.ConvertFrom(null, null, "020304"), "B11");
			Assert.AreEqual(Color.FromArgb(0, 0, 79, 80), conv.ConvertFrom(null, null, "20304"), "B12");
			Assert.AreEqual(Color.FromArgb(127, 255, 255, 255), conv.ConvertFrom(null, null, "2147483647"), "B13");
			Assert.AreEqual(Color.FromArgb(128, 0, 0, 1), conv.ConvertFrom(null, null, "-2147483647"), "B14");
			Assert.AreEqual(Color.FromArgb(255, 2, 3, 4), conv.ConvertFrom(null, null, "#FF020304"), "B15");
			Assert.AreEqual(Color.FromArgb(2, 3, 4), conv.ConvertFrom(null, null, "#020304"), "B16");
			Assert.AreEqual(Color.FromArgb(0, 2, 3, 4), conv.ConvertFrom(null, null, "#20304"), "B17");
			Assert.AreEqual(Color.Fuchsia, conv.ConvertFrom(null, null, "Fuchsia"), "B18");
			Assert.AreEqual(Color.FromArgb(0, 0, 2, 52), conv.ConvertFrom(null, null, "#234"), "B19");

			// Garbage/whitespace tests
			c = Color.FromName("\rGarbage\n");
			Assert.AreEqual("#000000", conv.ConvertTo(null, null, c, typeof(string)), "B20");
			Assert.AreEqual(Color.Fuchsia, conv.ConvertFrom(null, null, "\rFuchsia\n"), "B21");
			Assert.AreEqual(Color.FromArgb(255, 1, 2, 3), conv.ConvertFrom(null, null, "#010203"), "B22");

			Assert.AreEqual(Color.Empty, conv.ConvertFrom(null, null, ""), "B23");

			Assert.AreEqual(Color.FromArgb(0, 0, 0, 1), conv.ConvertFrom(null, null, "#1"), "B24");
			Assert.AreEqual(Color.FromArgb(0, 0, 0, 0x12), conv.ConvertFrom(null, null, "#12"), "B25");
			Assert.AreEqual(Color.FromArgb(0, 0, 1, 0x23), conv.ConvertFrom(null, null, "#123"), "B26");
			Assert.AreEqual(Color.FromArgb(0, 0, 0x12, 0x34), conv.ConvertFrom(null, null, "#1234"), "B27");
			Assert.AreEqual(Color.FromArgb(0, 1, 0x23, 0x45), conv.ConvertFrom(null, null, "#12345"), "B28");
			Assert.AreEqual(Color.FromArgb(0xff, 0x12, 0x34, 0x56), conv.ConvertFrom(null, null, "#123456"), "B29");
			Assert.AreEqual(Color.FromArgb(0x1, 0x23, 0x45, 0x67), conv.ConvertFrom(null, null, "#1234567"), "B30");
			Assert.AreEqual(Color.FromArgb(0x12, 0x34, 0x56, 0x78), conv.ConvertFrom(null, null, "#12345678"), "B31");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest1 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.Fuchsia, conv.ConvertFrom(null, null, "\rFuchsi\na\n"), "M1");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest2 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.FromArgb(255, 1, 2, 3), conv.ConvertFrom(null, null, "#010G03"), "M2");
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest3 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.FromArgb(255, 1, 2, 3), conv.ConvertFrom(null, null, "#010203Garbage"), "M3");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest4 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.FromArgb(255, 1, 2, 3), conv.ConvertFrom(null, null, "#010203 Garbage"), "M4");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest5 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			// Overflow
			Assert.AreEqual(Color.FromArgb(255, 254, 254, 254), conv.ConvertFrom(null, null, "4294901502"), "M5");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest6 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.Fuchsia, conv.ConvertFrom(null, null, "#Fuchsia"), "M6");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MalformatTest7 () 
		{
			WebColorConverter	conv;

			conv = new WebColorConverter();
			Assert.AreEqual(Color.FromArgb(255, 254, 254, 254), conv.ConvertFrom(null, null, "garbage"), "M7");
		}

		[Test (Description="Bug #546173")]
		public void NamedColorsCSS2 ()
		{
			WebColorConverter wcc = new WebColorConverter ();
			Color c;

			c = (Color)wcc.ConvertFrom (null, null, " ActiveBorder ");
			Assert.IsTrue (c.IsKnownColor, "#A1");
			Assert.AreEqual ("ActiveBorder", c.Name, "#A1-1");

			c = (Color)wcc.ConvertFrom (null, null, "ActiveCaption");
			Assert.IsTrue (c.IsKnownColor, "#A2");
			Assert.AreEqual ("ActiveCaption", c.Name, "#A2-1");

			c = (Color)wcc.ConvertFrom (null, null, "AppWorkspace");
			Assert.IsTrue (c.IsKnownColor, "#A3");
			Assert.AreEqual ("AppWorkspace", c.Name, "#A3-1");

			c = (Color)wcc.ConvertFrom (null, null, "Background");
			Assert.IsTrue (c.IsKnownColor, "#A4");
			Assert.AreEqual ("Desktop", c.Name, "#A1-4");

			c = (Color)wcc.ConvertFrom (null, null, "ButtonFace");
			Assert.IsTrue (c.IsKnownColor, "#A5");
			Assert.AreEqual ("Control", c.Name, "#A5-1");

			c = (Color)wcc.ConvertFrom (null, null, "ButtonHighlight");
			Assert.IsTrue (c.IsKnownColor, "#A6");
			Assert.AreEqual ("ControlLightLight", c.Name, "#A6-1");

			c = (Color)wcc.ConvertFrom (null, null, "ButtonShadow");
			Assert.IsTrue (c.IsKnownColor, "#A7");
			Assert.AreEqual ("ControlDark", c.Name, "#A7-1");
			
			c = (Color)wcc.ConvertFrom (null, null, "ButtonText");
			Assert.IsTrue (c.IsKnownColor, "#A8");
			Assert.AreEqual ("ControlText", c.Name, "#A8-1");

			c = (Color)wcc.ConvertFrom (null, null, "CaptionText");
			Assert.IsTrue (c.IsKnownColor, "#A9");
			Assert.AreEqual ("ActiveCaptionText", c.Name, "#A9-1");

			c = (Color)wcc.ConvertFrom (null, null, "GrayText");
			Assert.IsTrue (c.IsKnownColor, "#A10");
			Assert.AreEqual ("GrayText", c.Name, "#A10-1");

			c = (Color)wcc.ConvertFrom (null, null, "Highlight");
			Assert.IsTrue (c.IsKnownColor, "#A11");
			Assert.AreEqual ("Highlight", c.Name, "#A11-1");

			c = (Color)wcc.ConvertFrom (null, null, "HighlightText");
			Assert.IsTrue (c.IsKnownColor, "#A12");
			Assert.AreEqual ("HighlightText", c.Name, "#A12-1");

			c = (Color)wcc.ConvertFrom (null, null, "InactiveBorder");
			Assert.IsTrue (c.IsKnownColor, "#A13");
			Assert.AreEqual ("InactiveBorder", c.Name, "#A13-1");

			c = (Color)wcc.ConvertFrom (null, null, "InactiveCaption");
			Assert.IsTrue (c.IsKnownColor, "#A14");
			Assert.AreEqual ("InactiveCaption", c.Name, "#A14-1");

			c = (Color)wcc.ConvertFrom (null, null, "InactiveCaptionText");
			Assert.IsTrue (c.IsKnownColor, "#A15");
			Assert.AreEqual ("InactiveCaptionText", c.Name, "#A15-1");

			c = (Color)wcc.ConvertFrom (null, null, "InfoBackground");
			Assert.IsTrue (c.IsKnownColor, "#A16");
			Assert.AreEqual ("Info", c.Name, "#A16-1");

			c = (Color)wcc.ConvertFrom (null, null, "InfoText");
			Assert.IsTrue (c.IsKnownColor, "#A17");
			Assert.AreEqual ("InfoText", c.Name, "#A17-1");

			c = (Color)wcc.ConvertFrom (null, null, "Menu");
			Assert.IsTrue (c.IsKnownColor, "#A18");
			Assert.AreEqual ("Menu", c.Name, "#A18-1");

			c = (Color)wcc.ConvertFrom (null, null, "MenuText");
			Assert.IsTrue (c.IsKnownColor, "#A19");
			Assert.AreEqual ("MenuText", c.Name, "#A19-1");

			c = (Color)wcc.ConvertFrom (null, null, "ScrollBar");
			Assert.IsTrue (c.IsKnownColor, "#A20");
			Assert.AreEqual ("ScrollBar", c.Name, "#A20-1");

			c = (Color)wcc.ConvertFrom (null, null, "ThreeDDarkShadow");
			Assert.IsTrue (c.IsKnownColor, "#A21");
			Assert.AreEqual ("ControlDarkDark", c.Name, "#A21-1");

			c = (Color)wcc.ConvertFrom (null, null, "ThreeDFace");
			Assert.IsTrue (c.IsKnownColor, "#A22");
			Assert.AreEqual ("Control", c.Name, "#A22-1");

			c = (Color)wcc.ConvertFrom (null, null, "ThreeDHighlight");
			Assert.IsTrue (c.IsKnownColor, "#A23");
			Assert.AreEqual ("ControlLight", c.Name, "#A23-1");

			c = (Color)wcc.ConvertFrom (null, null, "ThreeDLightShadow");
			Assert.IsTrue (c.IsKnownColor, "#A24");
			Assert.AreEqual ("ControlLightLight", c.Name, "#A24-1");

			Exception ex = null;
			try {
				c = (Color)wcc.ConvertFrom (null, null, "ThreeDShadow");
			} catch (Exception e) {
				ex = e;
			}			
			Assert.IsNotNull (ex, "#A25");

			c = (Color)wcc.ConvertFrom (null, null, "Window");
			Assert.IsTrue (c.IsKnownColor, "#A26");
			Assert.AreEqual ("Window", c.Name, "#A26-1");

			c = (Color)wcc.ConvertFrom (null, null, "WindowFrame");
			Assert.IsTrue (c.IsKnownColor, "#A27");
			Assert.AreEqual ("WindowFrame", c.Name, "#A27-1");

			c = (Color)wcc.ConvertFrom (null, null, "WindowText");
			Assert.IsTrue (c.IsKnownColor, "#A28");
			Assert.AreEqual ("WindowText", c.Name, "#A28-1");
		}
		
		[Test]
		public void RoundTrip ()
		{
			WebColorConverter wcc = new WebColorConverter ();
			
			Color c = (Color) wcc.ConvertFrom(null, null, " captiontext ");
			Assert.AreEqual ("ActiveCaptionText", wcc.ConvertTo (null, null, c, typeof (string)), "#1");
		}		
	}
}
