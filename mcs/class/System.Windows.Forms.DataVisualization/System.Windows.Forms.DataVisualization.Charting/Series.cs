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
using System.Collections.Generic;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Series : DataPointCustomProperties
	{
		public Series ()
		{
			Points = new DataPointCollection ();
		}
		public Series (string name)
		{
			Name = name;
		}
		public Series (string name,int yValues)
		{
			Name = name;
			YValuesPerPoint = yValues;
		}

		public override string AxisLabel { get; set; }
		public string ChartArea { get; set; }
		public SeriesChartType ChartType { get; set; }
		public string ChartTypeName { get; set; }
		public DataPointCustomProperties EmptyPointStyle { get; set; }
		public bool Enabled { get; set; }
		public bool IsXValueIndexed { get; set; }
		public string Legend { get; set; }
		public int MarkerStep { get; set; }
		public override string Name { get; set; }
		public ChartColorPalette Palette { get; set; }
		public DataPointCollection Points { get; private set; }
		public Color ShadowColor { get; set; }
		public int ShadowOffset { get; set; }
		public SmartLabelStyle SmartLabelStyle { get; set; }
		public AxisType XAxisType { get; set; }
		public string XValueMember { get; set; }
		public ChartValueType XValueType { get; set; }
		public AxisType YAxisType { get; set; }
		public string YValueMembers { get; set; }
		public ChartValueType YValueType { get; set; }

		public int YValuesPerPoint { get; set; }


		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Sort (IComparer<DataPoint> comparer)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder,string sortBy)
		{
			throw new NotImplementedException();
		}
	}
}
