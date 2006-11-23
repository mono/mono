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
// Author:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace MonoTests.System.Drawing.Printing
{
	[TestFixture]
	public class PageSettingsTest
	{
		[Test]
		public void CloneTest ()
		{
			// Check for installed printers, because we need
			// to have at least one to test
			if (PrinterSettings.InstalledPrinters.Count == 0)
				return;

			PageSettings ps = new PageSettings ();
			ps.Color = false;
			ps.Landscape = true;
			ps.Margins = new Margins (120, 130, 140, 150);
			ps.PaperSize = new PaperSize ("My Custom Size", 222, 333);
			PageSettings clone = (PageSettings) ps.Clone ();

			Assert.AreEqual (ps.Color, clone.Color, "#1");
			Assert.AreEqual (ps.Landscape, clone.Landscape, "#2");
			Assert.AreEqual (ps.Margins, clone.Margins, "#3");
			Assert.AreSame (ps.PrinterSettings, clone.PrinterSettings, "#4");

			// PaperSize
			Assert.AreEqual (ps.PaperSize.PaperName, clone.PaperSize.PaperName, "#5");
			Assert.AreEqual (ps.PaperSize.Width, clone.PaperSize.Width, "#6");
			Assert.AreEqual (ps.PaperSize.Height, clone.PaperSize.Height, "#7");
			Assert.AreEqual (ps.PaperSize.Kind, clone.PaperSize.Kind, "#8");

			// PrinterResolution
			Assert.AreEqual (ps.PrinterResolution.X, clone.PrinterResolution.X, "#9");
			Assert.AreEqual (ps.PrinterResolution.Y, clone.PrinterResolution.Y, "#10");
			Assert.AreEqual (ps.PrinterResolution.Kind, clone.PrinterResolution.Kind, "#11");

			// PaperSource
			Assert.AreEqual (ps.PaperSource.Kind, clone.PaperSource.Kind, "#12");
			Assert.AreEqual (ps.PaperSource.SourceName, clone.PaperSource.SourceName, "#13");
		}
	}
}

