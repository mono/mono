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
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AxisScaleBreakStyle
	{
		#region Constructors
		public AxisScaleBreakStyle ()
		{
			BreakLineStyle = BreakLineStyle.Ragged;
			CollapsibleSpaceThreshold = 25;
			Enabled = false;
			LineColor = Color.Black;
			LineDashStyle = ChartDashStyle.Solid;
			LineWidth = 1;
			MaxNumberOfBreaks = 2;
			Spacing = 1.5d;
			StartFromZero = StartFromZero.Auto;
		}
		#endregion

		#region Public Properties
		public BreakLineStyle BreakLineStyle { get; set; }
		public int CollapsibleSpaceThreshold { get; set; }
		public bool Enabled { get; set; }
		public Color LineColor { get; set; }
		public ChartDashStyle LineDashStyle { get; set; }
		public int LineWidth { get; set; }
		public int MaxNumberOfBreaks { get; set; }
		public double Spacing { get; set; }
		public StartFromZero StartFromZero { get; set; }
		#endregion
	}
}
