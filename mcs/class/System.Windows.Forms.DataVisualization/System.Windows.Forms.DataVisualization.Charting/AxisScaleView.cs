//
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
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AxisScaleView
	{
		public bool IsZoomed { get; private set;}
		public double MinSize { get; set; }
		public DateTimeIntervalType MinSizeType { get; set; }
		public double Position { get; set; }
		public double Size { get; set; }
		public DateTimeIntervalType SizeType { get; set; }
		public double SmallScrollMinSize { get; set; }
		public DateTimeIntervalType SmallScrollMinSizeType { get; set; }
		public double SmallScrollSize { get; set; }
		public DateTimeIntervalType SmallScrollSizeType { get; set; }
		public double ViewMaximum { get; private set; }
		public double ViewMinimum { get; private set;}
		public bool Zoomable { get; set; }


		[MonoTODO]
		public void Scroll (DateTime newPosition)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Scroll (double newPosition)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Scroll (ScrollType scrollType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Zoom (double viewStart,double viewEnd)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Zoom (double viewPosition, double viewSize, DateTimeIntervalType viewSizeType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Zoom (double viewPosition, double viewSize, DateTimeIntervalType viewSizeType, bool saveState)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ZoomReset ()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ZoomReset (int numberOfViews)
		{
			throw new NotImplementedException();
		}
	}
}
