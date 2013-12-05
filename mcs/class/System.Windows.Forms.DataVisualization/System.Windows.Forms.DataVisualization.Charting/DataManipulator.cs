// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
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

using System.Data;
using System.Collections.Generic;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataManipulator : DataFormula
	{
		public bool FilterMatchedPoints { get; set; }
		public bool FilterSetEmptyPoints { get; set; }

		[MonoTODO]
		public DataSet ExportSeriesValues ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataSet ExportSeriesValues (Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataSet ExportSeriesValues (string seriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (IDataPointFilter filterInterface, string inputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (IDataPointFilter filterInterface, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, string inputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (DateRangeType dateRange, string rangeElements, string inputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (DateRangeType dateRange, string rangeElements, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (IDataPointFilter filterInterface, string inputSeriesNames, string outputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (IDataPointFilter filterInterface, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, string inputSeriesNames, string outputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (DateRangeType dateRange, string rangeElements, string inputSeriesNames, string outputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (DateRangeType dateRange, string rangeElements, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, string inputSeriesNames, string outputSeriesNames, string usingValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Filter (CompareMethod compareMethod, double compareValue, Series inputSeries, Series outputSeries, string usingValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, string inputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, string inputSeriesNames, string outputSeriesNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, string inputSeriesNames, string outputSeriesNames, string usingValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, Series inputSeries, Series outputSeries, string usingValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, string inputSeriesNames, string outputSeriesNames, string usingValue, bool getTopValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FilterTopN (int pointCount, Series inputSeries, Series outputSeries, string usingValue, bool getTopValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, string inputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, string inputSeriesName, string outputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, string inputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, string inputSeriesName, string outputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Group (string formula, double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GroupByAxisLabel (string formula, string inputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GroupByAxisLabel (string formula, Series inputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GroupByAxisLabel (string formula, string inputSeriesName, string outputSeriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GroupByAxisLabel (string formula, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, double fromXValue, double toXValue, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertEmptyPoints (double interval, IntervalType intervalType, double intervalOffset, IntervalType intervalOffsetType, double fromXValue, double toXValue, Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (IComparer<DataPoint> comparer, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (IComparer<DataPoint> comparer, Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder, Series series)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder, string sortBy, string seriesName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Sort (PointSortOrder pointSortOrder, string sortBy, Series series)
		{
			throw new NotImplementedException ();
		}
	}
}
