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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	class ControlPaintTest : TestHelper
	{
		[Test]
		public void DarkTest ()
		{
			Color color;

			// Non control colours
			color = Color.FromArgb (255, 100, 0, 50);
			color = ControlPaint.Dark (color);
			Assert.AreEqual (255, color.A, "testdark#1A");
			Assert.AreEqual (34, color.R, "testdark#1R");
			Assert.AreEqual (0, color.G, "testdark#1G");
			Assert.AreEqual (17, color.B, "testdark#1B");

			color = Color.FromArgb (255, 40, 50, 60);
			color = ControlPaint.Dark (color);
			Assert.AreEqual (255, color.A, "testdark#2A");
			Assert.AreEqual (14, color.R, "testdark#2R");
			Assert.AreEqual (17, color.G, "testdark#2G");
			Assert.AreEqual (20, color.B, "testdark#2B");

			// Non-control colours using a specific percentage
			color = Color.FromArgb (255, 20, 50, 40);
			color = ControlPaint.Dark (color,  0.8f);
			Assert.AreEqual (255, color.A, "testdark#3A");
			Assert.AreEqual (3, color.R, "testdark#3R");
			Assert.AreEqual (7, color.G, "testdark#3G");
			Assert.AreEqual (6, color.B, "testdark#3B");

			color = Color.FromArgb (255, 100, 0, 50);
			color = ControlPaint.Dark (color,  0.6f);
			Assert.AreEqual (255, color.A, "testdark#4A");
			Assert.AreEqual (28, color.R, "testdark#4R");
			Assert.AreEqual (0, color.G, "testdark#4G");
			Assert.AreEqual (14, color.B, "testdark#4B");

			// Fixed Control colours
			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Dark (color, 1f);
			Assert.AreEqual (Color.FromKnownColor (KnownColor.ControlDarkDark), color, "testdark#5");

			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Dark (color, 0f);
			Assert.AreEqual (Color.FromKnownColor (KnownColor.ControlDark), color, "testdark#6");

			// Calculated non control fixed colour
			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Dark (color, 0.5f);

			int r_sub, g_sub, b_sub;
			Color new_color;
			float per = 0.5f;

			r_sub = Color.FromKnownColor (KnownColor.ControlDarkDark).R -
				Color.FromKnownColor (KnownColor.ControlDark).R;
			g_sub = Color.FromKnownColor (KnownColor.ControlDarkDark).G -
				Color.FromKnownColor (KnownColor.ControlDark).G;
			b_sub = Color.FromKnownColor (KnownColor.ControlDarkDark).B -
				Color.FromKnownColor (KnownColor.ControlDark).B;

			new_color = Color.FromArgb (Color.FromKnownColor (KnownColor.ControlDark).A,
				(int) (Color.FromKnownColor (KnownColor.ControlDark).R + (r_sub * per)),
				(int) (Color.FromKnownColor (KnownColor.ControlDark).G + (g_sub * per)),
				(int) (Color.FromKnownColor (KnownColor.ControlDark).B + (b_sub * per)));

			Assert.AreEqual (new_color, color, "testdark#7");
		}

		[Test]
		public void LightTest ()
		{
			Color color;

			// Non control colours

			// Non-control colours using a specific percentage

			// Fixed Control colours
			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Light (color, 1f);
			Assert.AreEqual (Color.FromKnownColor (KnownColor.ControlLightLight), color, "testlight#5");

			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Light (color, 0f);
			Assert.AreEqual (Color.FromKnownColor (KnownColor.ControlLight), color, "testlight#6");

			// Calculated non control fixed colour
			color = Color.FromKnownColor (KnownColor.Control);
			color = ControlPaint.Light (color, 0.5f);

			int r_sub, g_sub, b_sub;
			Color new_color;
			float per = 0.5f;

			r_sub = Color.FromKnownColor (KnownColor.ControlLightLight).R -
				Color.FromKnownColor (KnownColor.ControlLight).R;
			g_sub = Color.FromKnownColor (KnownColor.ControlLightLight).G -
				Color.FromKnownColor (KnownColor.ControlLight).G;
			b_sub = Color.FromKnownColor (KnownColor.ControlLightLight).B -
				Color.FromKnownColor (KnownColor.ControlLight).B;

			new_color = Color.FromArgb (Color.FromKnownColor (KnownColor.ControlLight).A,
				(int) (Color.FromKnownColor (KnownColor.ControlLight).R + (r_sub * per)),
				(int) (Color.FromKnownColor (KnownColor.ControlLight).G + (g_sub * per)),
				(int) (Color.FromKnownColor (KnownColor.ControlLight).B + (b_sub * per)));

			Assert.AreEqual (new_color, color, "testlight#7");

		}

	}
}
