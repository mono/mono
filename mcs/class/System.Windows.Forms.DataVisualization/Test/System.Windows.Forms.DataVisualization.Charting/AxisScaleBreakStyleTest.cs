//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com) 
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

using System;
using System.Windows.Forms.DataVisualization.Charting;
using NUnit.Framework;
using System.Drawing;

namespace ChartingTests
{
	[TestFixture]
	public class AxisScaleBreakStyleTest
	{
		[Test]
		public void Constructor ()
		{
			AxisScaleBreakStyle asbs = new AxisScaleBreakStyle ();

			Assert.AreEqual (BreakLineStyle.Ragged, asbs.BreakLineStyle, "A1");
			Assert.AreEqual (25, asbs.CollapsibleSpaceThreshold, "A2");
			Assert.AreEqual (false, asbs.Enabled, "A3");
			Assert.AreEqual (Color.Black, asbs.LineColor, "A4");
			Assert.AreEqual (ChartDashStyle.Solid, asbs.LineDashStyle, "A5");
			Assert.AreEqual (1, asbs.LineWidth, "A6");
			Assert.AreEqual (2, asbs.MaxNumberOfBreaks, "A7");
			Assert.AreEqual (1.5d, asbs.Spacing, "A8");
			Assert.AreEqual (StartFromZero.Auto, asbs.StartFromZero, "A9");
		}
	}
}
