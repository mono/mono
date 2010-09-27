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
	public class ChartArea3DStyle
	{
		#region Constructors
		public ChartArea3DStyle (ChartArea chartArea)
		{
		}

		public ChartArea3DStyle ()
		{
		}
		#endregion

		#region Public Properties
		public bool Enable3D { get; set; }
		public int Inclination { get; set; }
		public bool IsClustered { get; set; }
		public bool IsRightAngleAxes { get; set; }
		public LightStyle LightStyle { get; set; }
		public int Perspective { get; set; }
		public int PointDepth { get; set; }
		public int PointGapDepth { get; set; }
		public int Rotation { get; set; }
		public int WallWidth { get; set; }
		#endregion
	}
}
