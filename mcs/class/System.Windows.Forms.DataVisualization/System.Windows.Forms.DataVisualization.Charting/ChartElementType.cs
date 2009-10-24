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

namespace System.Windows.Forms.DataVisualization.Charting
{
	public enum ChartElementType
	{
		Nothing = 0,
		Title = 1,
		PlottingArea = 2,
		Axis = 3,
		TickMarks = 4,
		Gridlines = 5,
		StripLines = 6,
		AxisLabelImage = 7,
		AxisLabels = 8,
		AxisTitle = 9,
		ScrollBarThumbTracker = 10,
		ScrollBarSmallDecrement = 11,
		ScrollBarSmallIncrement = 12,
		ScrollBarLargeDecrement = 13,
		ScrollBarLargeIncrement = 14,
		ScrollBarZoomReset = 15,
		DataPoint = 16,
		DataPointLabel = 17,
		LegendArea = 18,
		LegendTitle = 19,
		LegendHeader = 20,
		LegendItem = 21,
		Annotation = 22
	}
}
