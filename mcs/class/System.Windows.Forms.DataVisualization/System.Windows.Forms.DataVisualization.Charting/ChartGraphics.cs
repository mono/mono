﻿//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
// Francis Fisher (frankie@terrorise.me.uk)
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
using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ChartGraphics : ChartElement, IDisposable
	{
		#region Constructors
		internal ChartGraphics (Graphics graphics)
		{
			Graphics = graphics;
		}
		#endregion

		#region Public Properties
		public Graphics Graphics { get; set; }
		#endregion

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PointF GetAbsolutePoint (PointF point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RectangleF GetAbsoluteRectangle (RectangleF rectangle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF GetAbsoluteSize (SizeF size)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double GetPositionFromAxis (string chartAreaName, AxisName axis, double axisValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PointF GetRelativePoint (PointF point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RectangleF GetRelativeRectangle (RectangleF rectangle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF GetRelativeSize (SizeF size)
		{
			throw new NotImplementedException ();
		}
	}
}
