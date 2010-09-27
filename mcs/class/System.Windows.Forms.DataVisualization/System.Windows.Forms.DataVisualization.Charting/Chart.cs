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
using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Chart : Control
	{
		public Chart ()
		{
			BackColor = Color.White;
			ChartAreas = new ChartAreaCollection ();
			Series = new SeriesCollection ();
		}

		public ChartAreaCollection ChartAreas { get; private set; }
		public SeriesCollection Series { get; private set; }

		#region Protected Properties
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			ChartGraphics g = new ChartGraphics (e.Graphics);

			PaintElement (g, this, new ElementPosition (0, 0, 100, 100));

			foreach (var area in ChartAreas)
				PaintElement (g, area, new ElementPosition (9.299009f, 6.15f, 86.12599f, 81.1875f));

			foreach (var series in Series)
				PaintElement (g, series, new ElementPosition (9.299009f, 6.15f, 86.12599f, 81.1875f));
		}

		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);
		}

		protected virtual void OnPostPaint (ChartPaintEventArgs e)
		{
		}

		protected virtual void OnPrePaint (ChartPaintEventArgs e)
		{
		}
		#endregion

		#region Private Methods
		private void PaintElement (ChartGraphics g, object element, ElementPosition position)
		{
			ChartPaintEventArgs e = new ChartPaintEventArgs (this, element, g, position);

			OnPrePaint (e);
			OnPostPaint (e);
		}
		#endregion
	}
}