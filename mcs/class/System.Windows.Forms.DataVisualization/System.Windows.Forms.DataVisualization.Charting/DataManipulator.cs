// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Data;
using System.Collections.Generic;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataManipulator : DataFormula
	{
		public bool FilterMatchedPoints { get; set; }
		public bool FilterSetEmptyPoints { get; set; }

		public DataSet ExportSeriesValues(){
			throw new NotImplementedException ();
		}

		public DataSet ExportSeriesValues(Series series){
			throw new NotImplementedException ();
		}

		public DataSet ExportSeriesValues(string seriesNames){
			throw new NotImplementedException ();
		}

		public void Filter(
			IDataPointFilter filterInterface,
			string inputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			IDataPointFilter filterInterface,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			DateRangeType dateRange,
			string rangeElements,
			string inputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			DateRangeType dateRange,
			string rangeElements,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			IDataPointFilter filterInterface,
			string inputSeriesNames,
			string outputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			IDataPointFilter filterInterface,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames,
			string outputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			DateRangeType dateRange,
			string rangeElements,
			string inputSeriesNames,
			string outputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			DateRangeType dateRange,
			string rangeElements,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue
			){
			throw new NotImplementedException ();
		}

		public void Filter(
			CompareMethod compareMethod,
			double compareValue,
			Series inputSeries,
			Series outputSeries,
			string usingValue
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			string inputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			string inputSeriesNames,
			string outputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			Series inputSeries,
			Series outputSeries,
			string usingValue
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue,
			bool getTopValues
			){
			throw new NotImplementedException ();
		}

		public void FilterTopN(
			int pointCount,
			Series inputSeries,
			Series outputSeries,
			string usingValue,
			bool getTopValues
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			string inputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			string inputSeriesName,
			string outputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			string inputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			string inputSeriesName,
			string outputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void Group(
			string formula,
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}

		public void GroupByAxisLabel(
			string formula,
			string inputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void GroupByAxisLabel(
			string formula,
			Series inputSeries
			){
			throw new NotImplementedException ();
		}

		public void GroupByAxisLabel(
			string formula,
			string inputSeriesName,
			string outputSeriesName
			){
			throw new NotImplementedException ();
		}

		public void GroupByAxisLabel(
			string formula,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException ();
		}



		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			Series series
			){
			throw new NotImplementedException ();
		}

		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			Series series
			){
			throw new NotImplementedException ();
		}

		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			double fromXValue,
			double toXValue,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			double fromXValue,
			double toXValue,
			Series series
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			IComparer<DataPoint> comparer,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			IComparer<DataPoint> comparer,
			Series series
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			PointSortOrder pointSortOrder,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			PointSortOrder pointSortOrder,
			Series series
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			PointSortOrder pointSortOrder,
			string sortBy,
			string seriesName
			){
			throw new NotImplementedException ();
		}

		public void Sort(
			PointSortOrder pointSortOrder,
			string sortBy,
			Series series
			){
			throw new NotImplementedException ();
		}


	}
}