//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		DataManipulator.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	DataManipulator, IDataPointFilter
//
//  Purpose:	DataManipulator class exposes to the user methods
//				to perform data filtering, grouping, inserting 
//				empty points, sorting and exporting data.
//
//				It also expose financial and statistical formulas 
//              through the DataFormula base class.
//
//	Reviewed:	AG - Jul 31, 2002; 
//              GS - Aug 7, 2002
//              AG - Microsoft 15, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Design;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
	#region Data manipulation enumerations

	/// <summary>
	/// Grouping functions types
	/// </summary>
	internal enum GroupingFunction
	{
		/// <summary>
		/// Not defined
		/// </summary>
		None,
		/// <summary>
		/// Minimum value of the group
		/// </summary>
		Min,
		/// <summary>
		/// Maximum value of the group
		/// </summary>
		Max,
		/// <summary>
		/// Average value of the group
		/// </summary>
		Ave,
		/// <summary>
		/// Total of all values of the group
		/// </summary>
		Sum,
		/// <summary>
		/// Value of the first point in the group
		/// </summary>
		First,
		/// <summary>
		/// Value of the last point in the group
		/// </summary>
		Last,
		/// <summary>
		/// Value of the center point in the group
		/// </summary>
		Center,
		/// <summary>
		/// High, Low, Open, Close values in the group
		/// </summary>
		HiLoOpCl,
		/// <summary>
		/// High, Low values in the group
		/// </summary>
		HiLo,
		/// <summary>
		/// Number of points in the group
		/// </summary>
		Count,
		/// <summary>
		/// Number of unique points in the group
		/// </summary>
		DistinctCount,
		/// <summary>
		/// Variance of points in the group
		/// </summary>
		Variance,
		/// <summary>
		/// Deviation of points in the group
		/// </summary>
		Deviation
	}

	/// <summary>
	/// An enumeration of units of measurement for intervals.
	/// </summary>
	public enum IntervalType
	{
		/// <summary>
		/// Interval in numbers.
		/// </summary>
		Number, 
		/// <summary>
		/// Interval in years.
		/// </summary>
		Years, 
		/// <summary>
		/// Interval in months.
		/// </summary>
		Months, 
		/// <summary>
		/// Interval in weeks.
		/// </summary>
		Weeks, 
		/// <summary>
		/// Interval in days.
		/// </summary>
		Days, 
		/// <summary>
		/// Interval in hours.
		/// </summary>
		Hours, 
		/// <summary>
		/// Interval in minutes.
		/// </summary>
		Minutes,
		/// <summary>
		/// Interval in seconds.
		/// </summary>
		Seconds,
		/// <summary>
		/// Interval in milliseconds.
		/// </summary>
		Milliseconds
	}

	/// <summary>
    /// An enumeration of units of measurement for date ranges.
	/// </summary>
	public enum DateRangeType
	{
		/// <summary>
		/// Range defined in years.
		/// </summary>
		Year, 
		/// <summary>
		/// Range defined in months.
		/// </summary>
		Month, 
		/// <summary>
		/// Range defined in days of week.
		/// </summary>
		DayOfWeek,
		/// <summary>
		/// Range defined in days of month.
		/// </summary>
		DayOfMonth, 
		/// <summary>
		/// Range defined in hours.
		/// </summary>
		Hour, 
		/// <summary>
		/// Range defined in minutes.
		/// </summary>
		Minute
	}

	/// <summary>
	/// An enumeration of methods of comparison.
	/// </summary>
	public enum CompareMethod
	{
		/// <summary>
		/// One value is more than the other value.
		/// </summary>
		MoreThan, 
		/// <summary>
        /// One value is less than the other value.
		/// </summary>
		LessThan,
		/// <summary>
        /// One value is equal the other value.
		/// </summary>
		EqualTo,
		/// <summary>
        /// One value is more or equal to the other value.
		/// </summary>
		MoreThanOrEqualTo,
		/// <summary>
        /// One value is less or equal to the other value.
		/// </summary>
		LessThanOrEqualTo,
		/// <summary>
        /// One value is not equal to the other value.
		/// </summary>
		NotEqualTo
	}

	#endregion

	#region Data points filtering inteface

	/// <summary>
	/// The IDataPointFilter interface is used for filtering series data points.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public interface IDataPointFilter
	{
		/// <summary>
		/// Checks if the specified data point must be filtered.
		/// </summary>
		/// <param name="point">Data point object.</param>
		/// <param name="series">Series of the point.</param>
		/// <param name="pointIndex">Index of the point in the series.</param>
		/// <returns>True if point must be removed</returns>
		bool FilterDataPoint(DataPoint point, Series series, int pointIndex);
	}

	#endregion

	/// <summary>
    /// The DataManipulator class is used at runtime to perform data manipulation 
    /// operations, and is exposed via the DataManipulator property of the 
    /// root Chart object.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class DataManipulator : DataFormula
	{
        #region Fields

        // Indicates that filtering do not remove points, just mark them as empty
		private bool		_filterSetEmptyPoints = false;

		// Indicates that points that match the criteria must be filtered out
		private bool		_filterMatchedPoints = true;

        #endregion // Fields

        #region Data manipulator helper functions

		/// <summary>
		/// Helper function that converts one series or a comma separated
		/// list of series names into the Series array.
		/// </summary>
		/// <param name="obj">Series or string of series names.</param>
		/// <param name="createNew">If series with this name do not exist - create new.</param>
		/// <returns>Array of series.</returns>
		internal Series[] ConvertToSeriesArray(object obj, bool createNew)
		{
			Series[] array = null;

			if(obj == null)
			{
				return null;
			}

			// Parameter is one series
			if(obj.GetType() == typeof(Series))
			{
				array = new Series[1];
				array[0] = (Series)obj;
			}
			
			// Parameter is a string (comma separated series names)
			else if(obj.GetType() == typeof(string))
			{
				string	series = (string)obj;
				int		index = 0;

				// "*" means process all series from the collection
				if(series == "*")
				{
					// Create array of series
					array = new Series[Common.DataManager.Series.Count];

					// Add all series from the collection
					foreach(Series s in Common.DataManager.Series)
					{
						array[index] = s;
						++index;
					}
				}

				// Comma separated list
				else if(series.Length > 0)
				{
					// Replace commas in value string
					series = series.Replace("\\,", "\\x45");
					series = series.Replace("\\=", "\\x46");

					// Split string by comma
					string[] seriesNames = series.Split(',');

					// Create array of series
					array = new Series[seriesNames.Length];

					// Find series by name
					foreach(string s in seriesNames)
					{
						// Put pack a comma character
						string seriesName = s.Replace("\\x45", ",");
						seriesName = seriesName.Replace("\\x46", "=");

						try
						{
							array[index] = Common.DataManager.Series[seriesName.Trim()];
						}
						catch(System.Exception)
						{
							if(createNew)
							{
                                Series newSeries = new Series(seriesName.Trim());
                                Common.DataManager.Series.Add(newSeries);
                                array[index] = newSeries;
							}
							else
							{
								throw;
							}
						}

						++index;
					}
				}
			}

			return array;
		}

        /// <summary>
        /// Public constructor
        /// </summary>
        public DataManipulator()
        {
        }

		#endregion

		#region Series points sorting methods

		/// <summary>
		/// Sort series data points in specified order.
		/// </summary>
		/// <param name="pointSortOrder">Sorting order.</param>
		/// <param name="sortBy">Value to sort by.</param>
		/// <param name="series">Series array to sort.</param>
        private void Sort(PointSortOrder pointSortOrder, string sortBy, Series[] series)
		{
            // Check arguments
            if (sortBy == null)
                throw new ArgumentNullException("sortBy");
            if (series == null)
                throw new ArgumentNullException("series");
            
            // Check array of series
			if(series.Length == 0)
			{
				return;
			}

			// Sort series 
            DataPointComparer comparer = new DataPointComparer(series[0], pointSortOrder, sortBy);
			this.Sort(comparer, series);
		}

		/// <summary>
		/// Sort series data points in specified order.
		/// </summary>
		/// <param name="comparer">Comparing interface.</param>
		/// <param name="series">Series array to sort.</param>
		private void Sort(IComparer<DataPoint> comparer, Series[] series)
		{
            // Check arguments
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            if (series == null)
                throw new ArgumentNullException("series");

			//**************************************************
			//** Check array of series
			//**************************************************
			if(series.Length == 0)
			{
				return;
			}

			//**************************************************
			//** If we sorting more than one series
			//**************************************************
			if(series.Length > 1)
			{
				// Check if series X values are aligned
				this.CheckXValuesAlignment(series);

				// Apply points indexes to the first series
				int pointIndex = 0;
				foreach(DataPoint point in series[0].Points)
				{
					point["_Index"] = pointIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
					++pointIndex;
				}
			}

			//**************************************************
			//** Sort first series
			//**************************************************
			series[0].Sort(comparer);

			//**************************************************
			//** If we sorting more than one series
			//**************************************************
			if(series.Length > 1)
			{
				// Sort other series (depending on the first)
				int toIndex = 0;
				int fromIndex = 0;
				foreach(DataPoint point in series[0].Points)
				{
					// Move point from index is stored in point attribute (as index before sorting)
					fromIndex = int.Parse(point["_Index"], System.Globalization.CultureInfo.InvariantCulture);

					// Move points in series
					for(int seriesIndex = 1; seriesIndex < series.Length; seriesIndex++)
					{
						series[seriesIndex].Points.Insert(toIndex, series[seriesIndex].Points[toIndex + fromIndex]);
					}

					// Increase move point to index
					++toIndex;
				}

				// Remove extra points from series
				for(int seriesIndex = 1; seriesIndex < series.Length; seriesIndex++)
				{
					while(series[seriesIndex].Points.Count > series[0].Points.Count)
					{
						series[seriesIndex].Points.RemoveAt(series[seriesIndex].Points.Count - 1);
					}
				}
				
				//**************************************************
				//** Remove points index attribute
				//**************************************************
				foreach(DataPoint point in series[0].Points)
				{
					point.DeleteCustomProperty("_Index");
				}
			}
		}

		#endregion

		#region Series points sorting overloaded methods

        /// <summary>
        /// Sort the series' data points in specified order.
        /// </summary>
        /// <param name="pointSortOrder">Sorting order.</param>
        /// <param name="sortBy">Value to sort by.</param>
        /// <param name="seriesName">Comma separated series names to sort.</param>
        public void Sort(PointSortOrder pointSortOrder, string sortBy, string seriesName)
		{
            // Check arguments
            if (seriesName == null)
                throw new ArgumentNullException("seriesName");

            Sort(pointSortOrder, sortBy, ConvertToSeriesArray(seriesName, false));
		}

		/// <summary>
        /// Sort the series' data points in specified order.
		/// </summary>
		/// <param name="pointSortOrder">Sorting order.</param>
		/// <param name="series">Series to sort.</param>
        public void Sort(PointSortOrder pointSortOrder, Series series)
		{
            // Check arguments
            if (series == null)
                throw new ArgumentNullException("series");

            Sort(pointSortOrder, "Y", ConvertToSeriesArray(series, false));
		}

		/// <summary>
		/// Sort the series' data points in specified order.
		/// </summary>
		/// <param name="pointSortOrder">Sorting order.</param>
		/// <param name="seriesName">Comma separated series names to sort.</param>
        public void Sort(PointSortOrder pointSortOrder, string seriesName)
		{
            // Check arguments
            if (seriesName == null)
                throw new ArgumentNullException("seriesName");

            Sort(pointSortOrder, "Y", ConvertToSeriesArray(seriesName, false));
		}

		/// <summary>
		/// Sort the series' data points in specified order.
		/// </summary>
		/// <param name="pointSortOrder">Sorting order.</param>
		/// <param name="sortBy">Value to sort by.</param>
		/// <param name="series">Series to sort.</param>
        public void Sort(PointSortOrder pointSortOrder, string sortBy, Series series)
		{
            // Check arguments
            if (series == null)
                throw new ArgumentNullException("series");

            Sort(pointSortOrder, sortBy, ConvertToSeriesArray(series, false));
		}

		/// <summary>
		/// Sort the series' data points in specified order.
		/// </summary>
		/// <param name="comparer">IComparer interface.</param>
		/// <param name="series">Series to sort.</param>
        public void Sort(IComparer<DataPoint> comparer, Series series)
		{
            // Check arguments - comparer is checked in the private override of Sort
            if (series == null)
                throw new ArgumentNullException("series");
            
            Sort(comparer, ConvertToSeriesArray(series, false));
		}

		/// <summary>
		/// Sort the series' data points in specified order.
		/// </summary>
		/// <param name="comparer">Comparing interface.</param>
		/// <param name="seriesName">Comma separated series names to sort.</param>
        public void Sort(IComparer<DataPoint> comparer, string seriesName)
		{
            // Check arguments - comparer is checked in the private override of Sort
            if (seriesName == null)
                throw new ArgumentNullException("seriesName");
            
            Sort(comparer, ConvertToSeriesArray(seriesName, false));
		}

		#endregion

		#region Insert empty data points method

		/// <summary>
		/// Insert empty data points using specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="fromXValue">Check intervals from this X value.</param>
		/// <param name="toXValue">Check intervals until this X value.</param>
		/// <param name="series">Series array.</param>
		private void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			double fromXValue,
			double toXValue,
			Series[] series)
		{
            // Check the arguments
            if (interval <= 0)
                throw new ArgumentOutOfRangeException("interval");

			//**************************************************
			//** Automaticly detect minimum and maximum values
			//**************************************************
			double	fromX = Math.Min(fromXValue, toXValue);
			double	toX = Math.Max(fromXValue, toXValue);
			bool	fromIsNaN = double.IsNaN(fromX);
			bool	toIsNaN = double.IsNaN(toX);
			foreach(Series ser in series)
			{
				if(ser.Points.Count >= 1)
				{
					if(toIsNaN)
					{
						if(double.IsNaN(toX))
						{
							toX = ser.Points[ser.Points.Count - 1].XValue;
						}
						else
						{
							toX = Math.Max(toX, ser.Points[ser.Points.Count - 1].XValue);
						}
					}
					if(fromIsNaN)
					{
						if(double.IsNaN(fromX))
						{
							fromX = ser.Points[0].XValue;
						}
						else
						{
							fromX = Math.Min(fromX, ser.Points[0].XValue);
						}
					}
					if(fromX > toX)
					{
						double tempValue = fromX;
						fromX = toX;
						toX = tempValue;
					}
				}
			}

			//**************************************************
			//** Automaticly adjust the beginning interval and
			//** offset
			//**************************************************
			double	nonAdjustedFromX = fromX;
            fromX = ChartHelper.AlignIntervalStart(fromX, interval, ConvertIntervalType(intervalType));

			// Add offset to the start position
			if( intervalOffset != 0 )
			{
                fromX = fromX + ChartHelper.GetIntervalSize(fromX, intervalOffset, ConvertIntervalType(intervalOffsetType), null, 0, DateTimeIntervalType.Number, true, false);
			}


			//**************************************************
			//** Loop through all series
			//**************************************************
			foreach(Series ser in series)
			{
				//**************************************************
				//** Loop through all data points
				//**************************************************
				int	numberOfPoints = 0;
				int lastInsertPoint = 0;
				double currentPointValue = fromX;
				while(currentPointValue <= toX)
				{
					//**************************************************
					//** Check that X value is in range 
					//**************************************************
					bool	outOfRange = false;
					if(double.IsNaN(fromXValue) && currentPointValue < nonAdjustedFromX ||
						!double.IsNaN(fromXValue) && currentPointValue < fromXValue)
					{
						outOfRange = true;
					}
					else if(currentPointValue > toXValue)
					{
						outOfRange = true;
					}


					// Current X value is in range of points values
					if(!outOfRange)
					{
						//**************************************************
						//** Find required X value
						//**************************************************
						int	insertPosition = lastInsertPoint;
						for(int pointIndex = lastInsertPoint; pointIndex < ser.Points.Count; pointIndex++)
						{
							// Value was found
							if(ser.Points[pointIndex].XValue == currentPointValue)
							{
								insertPosition = -1;
								break;
							}

							// Save point index where we should insert new empty point
							if(ser.Points[pointIndex].XValue > currentPointValue)
							{
								insertPosition = pointIndex;
								break;
							}

							// Insert as last point
							if(pointIndex == (ser.Points.Count - 1))
							{
								insertPosition = ser.Points.Count;
							}
						}

						//**************************************************
						//** Required value was not found - insert empty data point
						//**************************************************
						if(insertPosition != -1)
						{
							lastInsertPoint = insertPosition;
							++numberOfPoints;
							DataPoint	dataPoint = new DataPoint(ser);
							dataPoint.XValue = currentPointValue;
							dataPoint.IsEmpty = true;
							ser.Points.Insert(insertPosition, dataPoint);
						}
					}

					//**************************************************
					//** Determine next required data point
					//**************************************************
                    currentPointValue += ChartHelper.GetIntervalSize(currentPointValue, 
						interval, 
						ConvertIntervalType(intervalType));


					//**************************************************
					//** Check if we exceed number of empty points
					//** we can add.
					//**************************************************
					if(numberOfPoints > 1000)
					{
						currentPointValue = toX + 1;
						continue;
					}
				}
			}
		}

		/// <summary>
		/// Helper function which converts IntervalType enumeration
		/// into DateTimeIntervalType enumeration.
		/// </summary>
		/// <param name="type">Interval type value.</param>
		/// <returns>Date time interval type value.</returns>
		private DateTimeIntervalType ConvertIntervalType(IntervalType type)
		{
			switch(type)
			{
				case(IntervalType.Milliseconds):
					return DateTimeIntervalType.Milliseconds;
				case(IntervalType.Seconds):
					return DateTimeIntervalType.Seconds;
				case(IntervalType.Days):
					return DateTimeIntervalType.Days;
				case(IntervalType.Hours):
					return DateTimeIntervalType.Hours;
				case(IntervalType.Minutes):
					return DateTimeIntervalType.Minutes;
				case(IntervalType.Months):
					return DateTimeIntervalType.Months;
				case(IntervalType.Number):
					return DateTimeIntervalType.Number;
				case(IntervalType.Weeks):
					return DateTimeIntervalType.Weeks;
				case(IntervalType.Years):
					return DateTimeIntervalType.Years;
			}
	
			return DateTimeIntervalType.Auto;
		}

		#endregion

		#region Insert empty data points overloaded methods

		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="series">Series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			Series series)
		{
			InsertEmptyPoints(interval, intervalType, 0, IntervalType.Number, series);
		}
		
		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="seriesName">Name of series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			string seriesName)
		{
			InsertEmptyPoints(interval, intervalType, 0, IntervalType.Number, seriesName);
		}
		
		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="seriesName">Name of series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			string seriesName)
		{
			InsertEmptyPoints(interval, intervalType, intervalOffset, intervalOffsetType, double.NaN, double.NaN, seriesName);
		}
		
		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="series">Series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			Series series)
		{
            InsertEmptyPoints(interval, intervalType, intervalOffset, intervalOffsetType, double.NaN, double.NaN, series);
		}
		
		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="fromXValue">Check intervals from this X value.</param>
		/// <param name="toXValue">Check intervals until this X value.</param>
		/// <param name="seriesName">Name of series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			double fromXValue,
			double toXValue,
			string seriesName)
		{
            // Check arguments
            if (seriesName == null)
                throw new ArgumentNullException("seriesName"); 
            
            InsertEmptyPoints(
				interval, 
				intervalType, 
				intervalOffset, 
				intervalOffsetType, 
				fromXValue, 
				toXValue, 
				ConvertToSeriesArray(seriesName, false));
		}
		

		/// <summary>
		/// Insert empty data points using the specified interval.
		/// </summary>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="fromXValue">Check intervals from this X value.</param>
		/// <param name="toXValue">Check intervals until this X value.</param>
		/// <param name="series">Series to insert the empty points.</param>
		public void InsertEmptyPoints(
			double interval,
			IntervalType intervalType,
			double intervalOffset,
			IntervalType intervalOffsetType,
			double fromXValue,
			double toXValue,
			Series series)
		{
            // Check arguments
            if (series == null)
                throw new ArgumentNullException("series");
            
            InsertEmptyPoints(
				interval, 
				intervalType, 
				intervalOffset, 
				intervalOffsetType, 
				fromXValue, 
				toXValue, 
				ConvertToSeriesArray(series, false));
		}
		

		#endregion

		#region Series data exporting methods

		/// <summary>
		/// Export series data into the DataSet object.
		/// </summary>
		/// <param name="series">Array of series which should be exported.</param>
		/// <returns>Data set object with series data.</returns>
		internal DataSet ExportSeriesValues(Series[] series)
		{
			//*****************************************************
			//** Create DataSet object
			//*****************************************************
			DataSet	dataSet = new DataSet();
            dataSet.Locale = System.Globalization.CultureInfo.CurrentCulture;
			// If input series are specified
			if(series != null)
			{
				// Export each series in the loop
				foreach(Series ser in series)
				{

					//*****************************************************
					//** Check if all X values are zeros
					//*****************************************************
					bool	zeroXValues = true;
					foreach( DataPoint point in ser.Points )
					{
						if( point.XValue != 0.0 )
						{
							zeroXValues = false;
							break;
						}
					}
                    
                    // Added 10 May 2005, DT - dataset after databinding 
                    // to string x value returns X as indexes 
                    if (zeroXValues && ser.XValueType == ChartValueType.String)
                    {
                        zeroXValues = false;
                    }

					//*****************************************************
					//** Create new table for the series
					//*****************************************************
					DataTable seriesTable = new DataTable(ser.Name);
                    seriesTable.Locale = System.Globalization.CultureInfo.CurrentCulture;

					//*****************************************************
					//** Add X column into data table schema
					//*****************************************************
					Type	columnType = typeof(double);
					if(ser.IsXValueDateTime())
					{
						columnType = typeof(DateTime);
					}
					else if(ser.XValueType == ChartValueType.String)
					{
						columnType = typeof(string);
					}
					seriesTable.Columns.Add("X", columnType);


					//*****************************************************
					//** Add Y column(s) into data table schema
					//*****************************************************
					columnType = typeof(double);
					if(ser.IsYValueDateTime())
					{
						columnType = typeof(DateTime);
					}
					else if(ser.YValueType == ChartValueType.String)
					{
						columnType = typeof(string);
					}
					for(int yIndex = 0; yIndex < ser.YValuesPerPoint; yIndex++)
					{
						if(yIndex == 0)
						{
							seriesTable.Columns.Add("Y", columnType);
						}
						else
						{
							seriesTable.Columns.Add("Y" + (yIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), columnType);
						}
					}


					//*****************************************************
					//** Fill data table's rows
					//*****************************************************
					double pointIndex = 1.0;
					foreach(DataPoint point in ser.Points)
					{
						if(!point.IsEmpty || !this.IsEmptyPointIgnored)
						{
							DataRow dataRow = seriesTable.NewRow();
					
							// Set row X value
							object	xValue = point.XValue;
							if(ser.IsXValueDateTime())
							{
                                if (Double.IsNaN(point.XValue))
                                    xValue = DBNull.Value;
                                else
                                    xValue = DateTime.FromOADate(point.XValue);
							}
							else if(ser.XValueType == ChartValueType.String)
							{
								xValue = point.AxisLabel;
							}
							dataRow["X"] = (zeroXValues) ? pointIndex : xValue;

							// Set row Y value(s)
							for(int yIndex = 0; yIndex < ser.YValuesPerPoint; yIndex++)
							{
								object	yValue = point.YValues[yIndex];
								if(!point.IsEmpty)
								{
									if(ser.IsYValueDateTime())
									{
                                        if (Double.IsNaN(point.YValues[yIndex]))
                                            xValue = DBNull.Value;
                                        else
                                            yValue = DateTime.FromOADate(point.YValues[yIndex]);
									}
									else if(ser.YValueType == ChartValueType.String)
									{
										yValue = point.AxisLabel;
									}
								}
								else if(!this.IsEmptyPointIgnored)
								{
									// Special handling of empty points
									yValue = DBNull.Value;
								}

								if(yIndex == 0)
								{
									dataRow["Y"] = yValue;
								}
								else
								{
									dataRow["Y" + (yIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)] = yValue;
								}
							}

							// Add row to the table
							seriesTable.Rows.Add(dataRow);

							++pointIndex;
						}
					}

					// Accept changes
					seriesTable.AcceptChanges();

					//*****************************************************
					//** Add data table into the data set
					//*****************************************************
					dataSet.Tables.Add(seriesTable);
				}
			}

			return dataSet;
		}

		#endregion

        #region Series data exporting overloaded methods

        /// <summary>
		/// Export all series from the collection into the DataSet object.
		/// </summary>
		/// <returns>Dataset object with series data.</returns>
		public DataSet ExportSeriesValues()
		{
			return ExportSeriesValues("*");
		}

		/// <summary>
		/// Export series data into the DataSet object.
		/// </summary>
		/// <param name="seriesNames">Comma separated list of series names to be exported.</param>
		/// <returns>Dataset object with series data.</returns>
		public DataSet ExportSeriesValues(string seriesNames)
		{
            // Check arguments
            if (seriesNames == null)
                throw new ArgumentNullException(seriesNames);

			return ExportSeriesValues(ConvertToSeriesArray(seriesNames, false));
		}

		/// <summary>
		/// Export series data into the DataSet object.
		/// </summary>
		/// <param name="series">Series to be exported.</param>
		/// <returns>Dataset object with series data.</returns>
		public DataSet ExportSeriesValues(Series series)
		{
            // Check arguments
            if (series == null)
                throw new ArgumentNullException("series");
            
            return ExportSeriesValues(ConvertToSeriesArray(series, false));
		}

		#endregion

		#region Filtering properties

		/// <summary>
        /// Gets or sets a flag which indicates whether points filtered by 
        /// the Filter or FilterTopN methods are removed or marked as empty.
        /// If set to true, filtered points are marked as empty; otherwise they are removed. 
        /// This property defaults to be false.
		/// </summary>
		public bool FilterSetEmptyPoints
		{
			get
			{
				return _filterSetEmptyPoints;
			}
			set
			{
				_filterSetEmptyPoints = value;
			}
		}

		/// <summary>
        /// Gets or sets a value that determines if points are filtered 
        /// if they match criteria that is specified in Filter method calls. 
        /// If set to true, points that match specified criteria are filtered. 
        /// If set to false, points that do not match the criteria are filtered. 
        /// This property defaults to be true.
		/// </summary>
		public bool FilterMatchedPoints
		{
			get
			{
				return _filterMatchedPoints;
			}
			set
			{
				_filterMatchedPoints = value;
			}
		}

		#endregion

		#region Filtering methods

		/// <summary>
		/// Keeps only N top/bottom points of the series
		/// </summary>
		/// <param name="pointCount">Number of top/bottom points to return.</param>
		/// <param name="inputSeries">Input series array.</param>
		/// <param name="outputSeries">Output series array.</param>
		/// <param name="usingValue">Defines which value of the point use in comparison (X, Y, Y2, ...).</param>
		/// <param name="getTopValues">Indicate that N top values must be retrieved, otherwise N bottom values.</param>
		private void FilterTopN(int pointCount,
			Series[] inputSeries,
			Series[] outputSeries,
			string usingValue,
			bool getTopValues)
		{
			// Check input/output series arrays
			CheckSeriesArrays(inputSeries, outputSeries);

			// Check input series alignment
			CheckXValuesAlignment(inputSeries);

			if(pointCount <= 0)
			{
                throw (new ArgumentOutOfRangeException("pointCount", SR.ExceptionDataManipulatorPointCountIsZero));
			}

			//**************************************************
			//** Filter points in the first series and remove
			//** in all
			//**************************************************

			// Define an output series array
			Series[] output = new Series[inputSeries.Length];
			for(int seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
			{
				output[seriesIndex] = inputSeries[seriesIndex];
				if(outputSeries != null && outputSeries.Length > seriesIndex)
				{
					output[seriesIndex] = outputSeries[seriesIndex];
				}

				// Remove all points from the output series
				if(output[seriesIndex] != inputSeries[seriesIndex])
				{
					output[seriesIndex].Points.Clear();

					// Make sure there is enough Y values per point
					output[seriesIndex].YValuesPerPoint = inputSeries[seriesIndex].YValuesPerPoint;

					// Copy X values type
					if(output[seriesIndex].XValueType == ChartValueType.Auto || output[seriesIndex].autoXValueType)
					{
						output[seriesIndex].XValueType = inputSeries[seriesIndex].XValueType;
						output[seriesIndex].autoXValueType = true;
					}
					// Copy Y values type
					if(output[seriesIndex].YValueType == ChartValueType.Auto || output[seriesIndex].autoYValueType)
					{
						output[seriesIndex].YValueType = inputSeries[seriesIndex].YValueType;
						output[seriesIndex].autoYValueType = true;
					}

					// Copy input points into output
					foreach(DataPoint point in inputSeries[seriesIndex].Points)
					{
						output[seriesIndex].Points.Add(point.Clone());
					}
				}

			}

			// No points to filter
			if(inputSeries[0].Points.Count == 0)
			{
				return;
			}

			//**************************************************
			//** Sort input data 
			//**************************************************
			this.Sort((getTopValues) ? PointSortOrder.Descending : PointSortOrder.Ascending,
				usingValue,
				output);

			//**************************************************
			//** Get top/bottom points
			//**************************************************
			// Process all series
			for(int	seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
			{
				// Only keep N first points
				while(output[seriesIndex].Points.Count > pointCount)
				{
					if(this.FilterSetEmptyPoints)
					{
						output[seriesIndex].Points[pointCount].IsEmpty = true;
						++pointCount;
					}
					else
					{
						output[seriesIndex].Points.RemoveAt(pointCount);
					}
				}
			}
		}

		/// <summary>
		/// Filter data points using IDataPointFilter interface
		/// </summary>
		/// <param name="filterInterface">Data points filtering interface.</param>
		/// <param name="inputSeries">Input series array.</param>
		/// <param name="outputSeries">Output series array.</param>
		private void Filter(IDataPointFilter filterInterface,
			Series[] inputSeries,
			Series[] outputSeries)
		{
			//**************************************************
			//** Check input/output series arrays
			//**************************************************
			CheckSeriesArrays(inputSeries, outputSeries);

			CheckXValuesAlignment(inputSeries);

			if(filterInterface == null)
			{
				throw(new ArgumentNullException("filterInterface"));
			}

			//**************************************************
			//** Filter points in the first series and remove
			//** in all
			//**************************************************

			// Define an output series array
			Series[] output = new Series[inputSeries.Length];
			for(int seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
			{
				output[seriesIndex] = inputSeries[seriesIndex];
				if(outputSeries != null && outputSeries.Length > seriesIndex)
				{
					output[seriesIndex] = outputSeries[seriesIndex];
				}

				// Remove all points from the output series
				if(output[seriesIndex] != inputSeries[seriesIndex])
				{
					output[seriesIndex].Points.Clear();

					// Make sure there is enough Y values per point
					output[seriesIndex].YValuesPerPoint = inputSeries[seriesIndex].YValuesPerPoint;

					// Copy X values type
					if(output[seriesIndex].XValueType == ChartValueType.Auto || output[seriesIndex].autoXValueType)
					{
						output[seriesIndex].XValueType = inputSeries[seriesIndex].XValueType;
						output[seriesIndex].autoXValueType = true;
					}
					// Copy Y values type
					if(output[seriesIndex].YValueType == ChartValueType.Auto || output[seriesIndex].autoYValueType)
					{
						output[seriesIndex].YValueType = inputSeries[seriesIndex].YValueType;
						output[seriesIndex].autoYValueType = true;
					}

				}

			}

			// No points to filter
			if(inputSeries[0].Points.Count == 0)
			{
				return;
			}

			//**************************************************
			//** Loop through all points of the first input series
			//**************************************************
			int originalPointIndex = 0;
			for(int pointIndex = 0; pointIndex < inputSeries[0].Points.Count; pointIndex++, originalPointIndex++)
			{
				bool pointRemoved = false;

				// Check if point match the criteria
				bool matchCriteria = filterInterface.FilterDataPoint(
					inputSeries[0].Points[pointIndex],
					inputSeries[0],
					originalPointIndex) == this.FilterMatchedPoints;


				// Process all series
				for(int	seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
				{
					bool seriesMatchCriteria = matchCriteria;
					if(output[seriesIndex] != inputSeries[seriesIndex])
					{
						if(seriesMatchCriteria && !this.FilterSetEmptyPoints)
						{
							// Don't do anything...
							seriesMatchCriteria = false;
						}
						else
						{
							// Copy point into the output series for all series
							output[seriesIndex].Points.Add(inputSeries[seriesIndex].Points[pointIndex].Clone());
						}
					}
					
				
					// If point match the criteria
					if(seriesMatchCriteria)
					{
						// Set point's empty flag
						if(this.FilterSetEmptyPoints)
						{
							output[seriesIndex].Points[pointIndex].IsEmpty = true;
							for(int valueIndex = 0; valueIndex <  output[seriesIndex].Points[pointIndex].YValues.Length; valueIndex++)
							{
								output[seriesIndex].Points[pointIndex].YValues[valueIndex] = 0.0;
							}
						}

						// Remove point
						else
						{
							output[seriesIndex].Points.RemoveAt(pointIndex);
							pointRemoved = true;
						}
					}
				}

				// Adjust index because of the removed point
				if(pointRemoved)
				{
					--pointIndex;
				}
			}
		}

		/// <summary>
		/// Data point filter. 
		/// Filters points using element type and index
		/// </summary>
		private class PointElementFilter : IDataPointFilter
		{
			// Private fields
			private DataManipulator	_dataManipulator = null;
			private DateRangeType	_dateRange;
			private int[]			_rangeElements = null;

			// Default constructor is not accesiable
			private PointElementFilter()
			{
			}

			/// <summary>
			/// Public constructor.
			/// </summary>
			/// <param name="dataManipulator">Data manipulator object.</param>
			/// <param name="dateRange">Range type.</param>
			/// <param name="rangeElements">Range elements to filter.</param>
			public PointElementFilter(DataManipulator dataManipulator, DateRangeType dateRange, string rangeElements)
			{
				this._dataManipulator = dataManipulator;
				this._dateRange = dateRange;
				this._rangeElements = dataManipulator.ConvertElementIndexesToArray(rangeElements);
			}
			
			/// <summary>
			/// Data points filtering method.
			/// </summary>
			/// <param name="point">Data point.</param>
			/// <param name="series">Data point series.</param>
			/// <param name="pointIndex">Data point index.</param>
			/// <returns>Indicates that point should be filtered.</returns>
			public bool FilterDataPoint(DataPoint point, Series series, int pointIndex)
			{
				return _dataManipulator.CheckFilterElementCriteria(
					this._dateRange,
					this._rangeElements,
					point);
			}
		}

		/// <summary>
		/// Data point filter. 
		/// Filters points using point values
		/// </summary>
		private class PointValueFilter : IDataPointFilter
		{
			// Private fields
			private CompareMethod	_compareMethod;
			private string			_usingValue;
			private double			_compareValue;

            /// <summary>
            /// Default constructor is not accessible
            /// </summary>
			private PointValueFilter()
			{
			}

            /// <summary>
            /// Public constructor.
            /// </summary>
            /// <param name="compareMethod">Comparing method.</param>
            /// <param name="compareValue">Comparing constant.</param>
            /// <param name="usingValue">Value used in comparison.</param>
			public PointValueFilter(CompareMethod compareMethod,
				double compareValue,
				string usingValue)
			{
				this._compareMethod = compareMethod;
				this._usingValue = usingValue;
				this._compareValue = compareValue;
			}
			
			/// <summary>
			/// IDataPointFilter interface method implementation
			/// </summary>
			/// <param name="point">Data point.</param>
			/// <param name="series">Data point series.</param>
			/// <param name="pointIndex">Data point index.</param>
			/// <returns>Indicates that point should be filtered.</returns>
			public bool FilterDataPoint(DataPoint point, Series series, int pointIndex)
			{
				// Check if point match the criteria
				bool matchCriteria = false;
				switch(_compareMethod)
				{
					case(CompareMethod.EqualTo):
						matchCriteria = point.GetValueByName(_usingValue) 
							== _compareValue;
						break;
					case(CompareMethod.LessThan):
						matchCriteria = point.GetValueByName(_usingValue) 
							< _compareValue;
						break;
					case(CompareMethod.LessThanOrEqualTo):
						matchCriteria = point.GetValueByName(_usingValue) 
							<= _compareValue;
						break;
					case(CompareMethod.MoreThan):
						matchCriteria = point.GetValueByName(_usingValue) 
							> _compareValue;
						break;
					case(CompareMethod.MoreThanOrEqualTo):
						matchCriteria = point.GetValueByName(_usingValue) 
							>= _compareValue;
						break;
					case(CompareMethod.NotEqualTo):
						matchCriteria = point.GetValueByName(_usingValue) 
							!= _compareValue;
						break;
				}

				return matchCriteria;
			}
		}

		/// <summary>
		/// Helper function to convert elements indexes from a string
		/// into an array of integers
		/// </summary>
		/// <param name="rangeElements">Element indexes string. Ex:"3,5,6-9,15"</param>
		/// <returns>Array of integer indexes.</returns>
		private int[] ConvertElementIndexesToArray(string rangeElements)
		{
			// Split input string by comma
			string[] indexes = rangeElements.Split(',');

			// Check if there are items in the array
			if(indexes.Length == 0)
			{
                throw (new ArgumentException(SR.ExceptionDataManipulatorIndexUndefined, "rangeElements"));
			}

			// Allocate memory for the result array
			int[]	result = new int[indexes.Length * 2];

			// Process each element index
			int		index = 0;
			foreach(string str in indexes)
			{
				// Check if it's a simple index or a range
				if(str.IndexOf('-') != -1)
				{
					string[]	rangeIndex = str.Split('-');
					if(rangeIndex.Length == 2)
					{
						// Convert to integer
						try
						{
							result[index] = Int32.Parse(rangeIndex[0], System.Globalization.CultureInfo.InvariantCulture);
							result[index + 1] = Int32.Parse(rangeIndex[1], System.Globalization.CultureInfo.InvariantCulture);

							if(result[index + 1] < result[index])
							{
								int temp = result[index];
								result[index] = result[index + 1];
								result[index + 1] = temp;
							}
						}
						catch(System.Exception)
						{
                            throw (new ArgumentException(SR.ExceptionDataManipulatorIndexFormatInvalid, "rangeElements"));
						}
					}
					else
					{
                        throw (new ArgumentException(SR.ExceptionDataManipulatorIndexFormatInvalid, "rangeElements"));
					}
				}
				else
				{
					// Convert to integer
					try
					{
						result[index] = Int32.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
						result[index + 1] = result[index];
					}
					catch(System.Exception)
					{
                        throw (new ArgumentException(SR.ExceptionDataManipulatorIndexFormatInvalid, "rangeElements"));
					}
				}

				index += 2;
			}

			return result;
		}

		/// <summary>
		/// Helper function, which checks if specified point matches the criteria
		/// </summary>
		/// <param name="dateRange">Element type.</param>
		/// <param name="rangeElements">Array of element indexes ranges (pairs).</param>
		/// <param name="point">Data point to check.</param>
		/// <returns>True if point matches the criteria.</returns>
		private bool CheckFilterElementCriteria(
			DateRangeType dateRange,
			int[] rangeElements,
			DataPoint point)
		{
			// Conver X value to DateTime
			DateTime dateTimeValue = DateTime.FromOADate(point.XValue);

			for(int index = 0; index < rangeElements.Length; index += 2)
			{
				switch(dateRange)
				{
					case(DateRangeType.Year):
						if(dateTimeValue.Year >= rangeElements[index] && 
							dateTimeValue.Year <= rangeElements[index+1])
							return true;
						break;
					case(DateRangeType.Month):
						if(dateTimeValue.Month >= rangeElements[index] && 
							dateTimeValue.Month <= rangeElements[index+1])
							return true;
						break;
					case(DateRangeType.DayOfWeek):
						if((int)dateTimeValue.DayOfWeek >= rangeElements[index] && 
							(int)dateTimeValue.DayOfWeek <= rangeElements[index+1])
							return true;
						break;
					case(DateRangeType.DayOfMonth):
						if(dateTimeValue.Day >= rangeElements[index] && 
							dateTimeValue.Day <= rangeElements[index+1])
							return true;
						break;
					case(DateRangeType.Hour):
						if(dateTimeValue.Hour >= rangeElements[index] && 
							dateTimeValue.Hour <= rangeElements[index+1])
							return true;
						break;
					case(DateRangeType.Minute):
						if(dateTimeValue.Minute >= rangeElements[index] && 
							dateTimeValue.Minute <= rangeElements[index+1])
							return true;
						break;
				}
			}

			return false;
		}

		#endregion

		#region Filtering overloaded methods

		/// <summary>
        /// Filters a series' data points, either removing the specified points 
        /// or marking them as empty for the given date/time ranges.
		/// </summary>
		/// <param name="dateRange">Element type.</param>
        /// <param name="rangeElements">Specifies the elements within the date/time range 
        /// (specified by the dateRange parameter) that will be filtered. Can be a single value (e.g. "7"), 
        /// comma-separated values (e.g. "5,6"), a range of values (e.g. 9-11), 
        /// or any variation thereof (e.g. "5,6,9-11").</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names, to store the output.</param>
		public void Filter(DateRangeType dateRange,
			string rangeElements,
			string inputSeriesNames,
			string outputSeriesNames)
		{
            // Check arguments
            if (rangeElements == null)
                throw new ArgumentNullException("rangeElements");
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			// Filter points using filtering interface
			Filter(new PointElementFilter(this, dateRange, rangeElements), 
				ConvertToSeriesArray(inputSeriesNames, false), 
				ConvertToSeriesArray(outputSeriesNames, true));
		}

		/// <summary>
        /// Filters a series' data points, either removing the specified points 
        /// or marking them as empty for the given date/time ranges. 
        /// The Series object that is filtered is used to store the modified data.
		/// </summary>
		/// <param name="dateRange">Element type.</param>
        /// <param name="rangeElements">Specifies the elements within the date/time range 
        /// (specified by the dateRange parameter) that will be filtered. Can be a single value (e.g. "7"), 
        /// comma-separated values (e.g. "5,6"), a range of values (e.g. 9-11), 
        /// or any variation thereof (e.g. "5,6,9-11").</param>
		/// <param name="inputSeries">Input series.</param>
		public void Filter(DateRangeType dateRange,
			string rangeElements,
			Series inputSeries)
		{
            // Check arguments
            if (rangeElements == null)
                throw new ArgumentNullException("rangeElements");
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

            Filter(dateRange, rangeElements, inputSeries, null);
		}

		/// <summary>
        /// Filters a series' data points, either removing the specified points 
        /// or marking them as empty for the given date/time ranges.
		/// </summary>
		/// <param name="dateRange">Element type.</param>
        /// <param name="rangeElements">Specifies the elements within the date/time range 
        /// (specified by the dateRange parameter) that will be filtered. Can be a single value (e.g. "7"), 
        /// comma-separated values (e.g. "5,6"), a range of values (e.g. 9-11), 
        /// or any variation thereof (e.g. "5,6,9-11").</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
		public void Filter(DateRangeType dateRange,
			string rangeElements,
			Series inputSeries,
			Series outputSeries)
		{
            // Check arguments
            if (rangeElements == null)
                throw new ArgumentNullException("rangeElements");
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			// Filter points using filtering interface
			Filter(new PointElementFilter(this, dateRange, rangeElements), 
				ConvertToSeriesArray(inputSeries, false), 
				ConvertToSeriesArray(outputSeries, false));
		}

		/// <summary>
        /// Filters a series' data points, either removing the specified points 
        /// or marking them as empty for the given date/time ranges.
        /// The filtered Series objects are used to store the modified data. 
		/// </summary>
		/// <param name="dateRange">Element type.</param>
        /// <param name="rangeElements">Specifies the elements within the date/time range 
        /// (specified by the dateRange parameter) that will be filtered. Can be a single value (e.g. "7"), 
        /// comma-separated values (e.g. "5,6"), a range of values (e.g. 9-11), 
        /// or any variation thereof (e.g. "5,6,9-11").</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		public void Filter(DateRangeType dateRange,
			string rangeElements,
			string inputSeriesNames)
		{
            // Check arguments
            if (rangeElements == null)
                throw new ArgumentNullException("rangeElements");
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

            Filter(dateRange, 
				rangeElements, 
				inputSeriesNames,
				"");
		}

		/// <summary>
        /// Filters a series' data points by applying a filtering rule to the first Y-value of data points. 
        /// The Series object that is filtered is used to store the modified data.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeries">Input series.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			Series inputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Filter(compareMethod,
				compareValue,
				inputSeries,
				null,
				"Y");
		}

		/// <summary>
        /// Filters a series' data points by applying a filtering rule to the first Y-value of data points.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			Series inputSeries,
			Series outputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			// Filter points using filtering interface
			Filter(new PointValueFilter(compareMethod, compareValue, "Y"), 
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false));
		}

		/// <summary>
        /// Filters a series' data points by applying a filtering rule to the specified value for comparison.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			Series inputSeries,
			Series outputSeries,
			string usingValue)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			// Filter points using filtering interface
			Filter(new PointValueFilter(compareMethod, compareValue, usingValue), 
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false));
		}

		/// <summary>
        /// Filters one or more series by applying a filtering rule to the first Y-value of the first series' data points. 
        /// The filtered Series objects are used to store the modified data.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			Filter(compareMethod,
				compareValue,
				inputSeriesNames,
				"",
				"Y");
		}
		
		/// <summary>
        /// Filters one or more series by applying a filtering rule to the first Y-value of the first series' data points.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames,
			string outputSeriesNames)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			// Filter points using filtering interface
			Filter(new PointValueFilter(compareMethod, compareValue, "Y"), 
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true));
		}
		
		/// <summary>
        /// Filters one or more series by applying a filtering rule to the specified value of the first series' data points.
		/// </summary>
		/// <param name="compareMethod">Value comparing method.</param>
		/// <param name="compareValue">Value to compare with.</param>
		/// <param name="inputSeriesNames">Comma separated input series names.</param>
		/// <param name="outputSeriesNames">Comma separated output series names.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
		public void Filter(CompareMethod compareMethod,
			double compareValue,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			// Filter points using filtering interface
			Filter(new PointValueFilter(compareMethod, compareValue, usingValue), 
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true));
		}

		/// <summary>
        /// Filters all data points in one or more series except for a specified number of points. 
        /// The points that are not filtered correspond to points in the first input series that have the largest or smallest values.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
        /// <param name="getTopValues">The largest values are kept if set to true; otherwise the smallest values are kept.</param>
		public void FilterTopN(int pointCount,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue,
			bool getTopValues)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true),
				usingValue,
				getTopValues);
		}

		/// <summary>
        /// Filters out all data points in a series except for a specified number of points with the largest (first) Y-values. 
        /// The Series object that is filtered is used to store the modified data.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeries">Input series.</param>
		public void FilterTopN(int pointCount,
			Series inputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            
            FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeries, false),
				null,
				"Y",
				true);
		}

		/// <summary>
        /// Filters all data points in a series except for a specified number of points with the largest first Y-values.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
		public void FilterTopN(int pointCount,
			Series inputSeries,
			Series outputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            
            FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false),
				"Y",
				true);
		}

		/// <summary>
        /// Filters all data points in a series except for a specified number of points with the largest values.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
		public void FilterTopN(int pointCount,
			Series inputSeries,
			Series outputSeries,
			string usingValue)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false),
				usingValue,
				true);
		}

		/// <summary>
        /// Filters all data points in a series except for a specified number of points with the smallest or largest values.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
        /// <param name="getTopValues">The largest values are kept if set to true; otherwise the smallest values are kept.</param>
		public void FilterTopN(int pointCount,
			Series inputSeries,
			Series outputSeries,
			string usingValue,
			bool getTopValues)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false),
				usingValue,
				getTopValues);
		}
		
		/// <summary>
        /// Filters all data points in one or more series except for a specified number of points.
        /// The points that are not filtered correspond to points in the first series that have the largest first Y-values.  
        /// The Series objects that are filtered are used to store the modified data.
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		public void FilterTopN(int pointCount,
			string inputSeriesNames)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeriesNames, false),
				null,
				"Y",
				true);
		}

		/// <summary>
        /// Filters out data points in one or more series except for a specified number of points. 
        /// The points that aren't filtered correspond to points in the first series that have the largest first Y-values. 
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names.</param>
		public void FilterTopN(int pointCount,
			string inputSeriesNames,
			string outputSeriesNames)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true),
				"Y",
				true);
		}
		
		/// <summary>
        /// Filters all data points in one or more series except for a specified number of points. 
        /// The points that are not filtered correspond to points in the first series that have the largest values.  
		/// </summary>
        /// <param name="pointCount">The number of data points that the filtering operation will not remove.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names.</param>
        /// <param name="usingValue">The data point value that the filtering rule is applied to. Can be X, Y, Y2, Y3, etc.</param>
		public void FilterTopN(int pointCount,
			string inputSeriesNames,
			string outputSeriesNames,
			string usingValue)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");
            if (usingValue == null)
                throw new ArgumentNullException("usingValue");

			FilterTopN(pointCount,
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true),
				usingValue,
				true);
		}
		
	
		/// <summary>
        /// Performs custom filtering on a series' data points. 
        /// The Series object that is filtered is used to store the modified data. 
		/// </summary>
		/// <param name="filterInterface">Filtering interface.</param>
		/// <param name="inputSeries">Input series.</param>
        public void Filter(IDataPointFilter filterInterface,
			Series inputSeries)
		{
            // Check arguments
            if (filterInterface == null)
                throw new ArgumentNullException("filterInterface");
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Filter(filterInterface,
				ConvertToSeriesArray(inputSeries, false),
				null);
		}
		
		/// <summary>
        /// Performs custom filtering on a series' data points.
		/// </summary>
		/// <param name="filterInterface">Filtering interface.</param>
		/// <param name="inputSeries">Input series.</param>
		/// <param name="outputSeries">Output series.</param>
        public void Filter(IDataPointFilter filterInterface,
			Series inputSeries,
			Series outputSeries)
		{
            // Check arguments
            if (filterInterface == null)
                throw new ArgumentNullException("filterInterface");
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Filter(filterInterface,
				ConvertToSeriesArray(inputSeries, false),
				ConvertToSeriesArray(outputSeries, false));
		}

		/// <summary>
        /// Performs custom filtering on one or more series' data points, based on the first series' points. 
        /// The filtered series are also used to store the modified data.  
		/// </summary>
		/// <param name="filterInterface">Filtering interface.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
        public void Filter(IDataPointFilter filterInterface,
			string inputSeriesNames)
		{
            // Check arguments
            if (filterInterface == null)
                throw new ArgumentNullException("filterInterface");
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			Filter(filterInterface,
				ConvertToSeriesArray(inputSeriesNames, false),
				null);
		}
		
		/// <summary>
        /// Performs custom filtering on one or more series' data points, based on the first series' points. 
		/// </summary>
		/// <param name="filterInterface">Filtering interface.</param>
		/// <param name="inputSeriesNames">Comma separated list of input series names.</param>
		/// <param name="outputSeriesNames">Comma separated list of output series names.</param>
        public void Filter(IDataPointFilter filterInterface,
			string inputSeriesNames,
			string outputSeriesNames)
		{
            // Check arguments
            if (filterInterface == null)
                throw new ArgumentNullException("filterInterface");
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");
            
            Filter(filterInterface,
				ConvertToSeriesArray(inputSeriesNames, false),
				ConvertToSeriesArray(outputSeriesNames, true));
		}

		#endregion

		#region Grouping methods

		/// <summary>
		/// Class stores information about the grouping function type and
		/// index of output value.
		/// </summary>
		private class GroupingFunctionInfo
		{
			// AxisName of the grouping function
			internal	GroupingFunction	function = GroupingFunction.None;

			// Index of the Y value for storing results
			internal	int					outputIndex	= 0;

            /// <summary>
            /// Constructor.
            /// </summary>
            internal GroupingFunctionInfo()
            {
            }
		}

        /// <summary>
        /// Grouping by X value, when it’s a string (stored in AxisLabel property).
        /// </summary>
        /// <param name="formula">Grouping formula.</param>
        /// <param name="inputSeries">Array of input series.</param>
        /// <param name="outputSeries">Array of output series.</param>
		private void GroupByAxisLabel(string formula, Series[] inputSeries, Series[] outputSeries)
		{
            // Check arguments
            if (formula == null)
                throw new ArgumentNullException("formula");

			//**************************************************
			//** Check input/output series arrays
			//**************************************************
			CheckSeriesArrays(inputSeries, outputSeries);

			//**************************************************
			//** Check and parse formula
			//**************************************************
			int	outputValuesNumber = 1;
			GroupingFunctionInfo[] functions = GetGroupingFunctions(inputSeries, formula, out outputValuesNumber);

			//**************************************************
			//** Loop through all input series
			//**************************************************
			for(int	seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
			{
				// Define an input and output series
				Series input = inputSeries[seriesIndex];
				Series output = input;
				if(outputSeries != null && seriesIndex < outputSeries.Length)
				{
					output = outputSeries[seriesIndex];

					// Remove all points from the output series
					if(output.Name != input.Name)
					{
						output.Points.Clear();

						// Copy X values type
						if(output.XValueType == ChartValueType.Auto || output.autoXValueType)
						{
							output.XValueType = input.XValueType;
							output.autoXValueType = true;
						}
						// Copy Y values type
						if(output.YValueType == ChartValueType.Auto || output.autoYValueType)
						{
							output.YValueType = input.YValueType;
							output.autoYValueType = true;
						}

					}
				}

				// Copy input data into temp storage
				if(input != output)
				{
					Series inputTemp = new Series("Temp", input.YValuesPerPoint);
					foreach(DataPoint point in input.Points)
					{
						DataPoint dp = new DataPoint(inputTemp);
						dp.AxisLabel = point.AxisLabel;
						dp.XValue = point.XValue;
						point.YValues.CopyTo(dp.YValues, 0);
						dp.IsEmpty = point.IsEmpty;
						inputTemp.Points.Add(dp);
					}
					input = inputTemp;
				}

				// No points to group
				if(input.Points.Count == 0)
				{
					continue;
				}

				// Make sure there is enough Y values per point
				output.YValuesPerPoint = outputValuesNumber - 1;

				//**************************************************
				//** Sort input data by axis label
				//**************************************************
				input.Sort(PointSortOrder.Ascending, "AxisLabel");

				//**************************************************
				//** Initialize interval & value tracking variables
				//**************************************************
				int		intervalFirstIndex = 0;
				int		intervalLastIndex = 0;

				//**************************************************
				//** Allocate array for storing temp. 
				//** values of the point
				//**************************************************
				double[]	pointTempValues = new double[outputValuesNumber];

				//**************************************************
				//** Loop through the series points 
				//**************************************************
				string	currentLabel = null;
				bool	lastPoint = false;
				int		emptyPointsSkipped = 0;
				for(int	pointIndex = 0; pointIndex <= input.Points.Count && !lastPoint; pointIndex++)
				{	
					bool	endOfInterval = false;
					
					//**************************************************
					//** Check if it's the last point
					//**************************************************
					if(pointIndex == input.Points.Count)
					{
						// End of the group interval detected
						lastPoint = true;
						intervalLastIndex = pointIndex - 1;
						pointIndex = intervalLastIndex;
						endOfInterval = true;
					}

					// Set current axis label
					if(!endOfInterval && currentLabel == null)
					{
						currentLabel = input.Points[pointIndex].AxisLabel;
					}

					//**************************************************
					//** Check if current point X value is inside current group
					//**************************************************
					if(!endOfInterval && input.Points[pointIndex].AxisLabel != currentLabel)
					{
						// End of the group interval detected
						intervalLastIndex = pointIndex - 1;
						endOfInterval = true;
					}

					//**************************************************
					//** Process data at end of the interval
					//**************************************************
					if(endOfInterval)
					{
						// Finalize the calculation
						ProcessPointValues(
							functions, 
							pointTempValues,
							inputSeries[seriesIndex],
							input.Points[pointIndex],
							pointIndex, 
							intervalFirstIndex, 
							intervalLastIndex,
							true,
							ref emptyPointsSkipped);

						//**************************************************
						//** Calculate the X values
						//**************************************************
						if(functions[0].function == GroupingFunction.Center)
						{
							pointTempValues[0] = 
								(inputSeries[seriesIndex].Points[intervalFirstIndex].XValue + 
								inputSeries[seriesIndex].Points[intervalLastIndex].XValue) / 2.0;
						}
						else if(functions[0].function == GroupingFunction.First)
						{
							pointTempValues[0] = 
								inputSeries[seriesIndex].Points[intervalFirstIndex].XValue;
						}
						if(functions[0].function == GroupingFunction.Last)
						{
							pointTempValues[0] = 
								inputSeries[seriesIndex].Points[intervalLastIndex].XValue;
						}

						//**************************************************
						//** Create new point object
						//**************************************************
						DataPoint	newPoint = new DataPoint();
						newPoint.ResizeYValueArray(outputValuesNumber - 1);
						newPoint.XValue = pointTempValues[0];
						newPoint.AxisLabel = currentLabel;
						for(int i = 1; i < pointTempValues.Length; i++)
						{
							newPoint.YValues[i - 1] = pointTempValues[i];
						}
						
						//**************************************************
						//** Remove grouped points if output and input 
						//** series are the same
						//**************************************************
						int	newPointIndex = output.Points.Count;
						if(output == input)
						{
							newPointIndex = intervalFirstIndex;
							pointIndex = newPointIndex + 1;

							// Remove grouped points
							for(int removedPoint = intervalFirstIndex; removedPoint <= intervalLastIndex; removedPoint++)
							{
								output.Points.RemoveAt(intervalFirstIndex);
							}
						}

						//**************************************************
						//** Add point to the output series
						//**************************************************
						output.Points.Insert(newPointIndex, newPoint);


						// Set new group interval indexes
						intervalFirstIndex = pointIndex;
						intervalLastIndex = pointIndex;
						
						// Reset number of skipped points
						emptyPointsSkipped = 0;
						currentLabel = null;

						// Process point once again
						--pointIndex;

						continue;
					}

					//**************************************************
					//** Use current point values in the formula
					//**************************************************
					ProcessPointValues(
						functions, 
						pointTempValues,
						inputSeries[seriesIndex],
						input.Points[pointIndex],
						pointIndex, 
						intervalFirstIndex, 
						intervalLastIndex,
						false,
						ref emptyPointsSkipped);
				}
			}
		}

        /// <summary>
        /// Groups series points in the interval with offset
        /// </summary>
        /// <param name="formula">Grouping formula.</param>
        /// <param name="interval">Interval size.</param>
        /// <param name="intervalType">Interval type.</param>
        /// <param name="intervalOffset">Interval offset size.</param>
        /// <param name="intervalOffsetType">Interval offset type.</param>
        /// <param name="inputSeries">Array of input series.</param>
        /// <param name="outputSeries">Array of output series.</param>
		private void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			double intervalOffset,
			IntervalType intervalOffsetType, 
			Series[] inputSeries, 
			Series[] outputSeries)
		{
            // Check arguments
            if (formula == null)
                throw new ArgumentNullException("formula");

			//**************************************************
			//** Check input/output series arrays
			//**************************************************
			CheckSeriesArrays(inputSeries, outputSeries);

			//**************************************************
			//** Check and parse formula
			//**************************************************
			int	outputValuesNumber = 1;
			GroupingFunctionInfo[] functions = GetGroupingFunctions(inputSeries, formula, out outputValuesNumber);

			//**************************************************
			//** Loop through all input series
			//**************************************************
			for(int	seriesIndex = 0; seriesIndex < inputSeries.Length; seriesIndex++)
			{
				// Define an input and output series
				Series input = inputSeries[seriesIndex];
				Series output = input;
				if(outputSeries != null && seriesIndex < outputSeries.Length)
				{
					output = outputSeries[seriesIndex];

					// Remove all points from the output series
					if(output.Name != input.Name)
					{
						output.Points.Clear();

						// Copy X values type
						if(output.XValueType == ChartValueType.Auto || output.autoXValueType)
						{
							output.XValueType = input.XValueType;
							output.autoXValueType = true;
						}
						// Copy Y values type
						if(output.YValueType == ChartValueType.Auto || output.autoYValueType)
						{
							output.YValueType = input.YValueType;
							output.autoYValueType = true;
						}
						
					}
				}

				// No points to group
				if(input.Points.Count == 0)
				{
					continue;
				}

				// Make sure there is enough Y values per point
				output.YValuesPerPoint = outputValuesNumber - 1;

				//**************************************************
				//** Initialize interval & value tracking variables
				//**************************************************
				int		intervalFirstIndex = 0;
				int		intervalLastIndex = 0;
				double	intervalFrom = 0;
				double	intervalTo = 0;

				// Set interval start point
				intervalFrom = input.Points[0].XValue;

				// Adjust start point depending on the interval type
                intervalFrom = ChartHelper.AlignIntervalStart(intervalFrom, interval, ConvertIntervalType(intervalType));

				// Add offset to the start position
				double offsetFrom = 0;
				if( intervalOffset != 0 )
				{
                    offsetFrom = intervalFrom + ChartHelper.GetIntervalSize(intervalFrom, 
						intervalOffset, 
						ConvertIntervalType(intervalOffsetType));

					// Check if there are points left outside first group
					if(input.Points[0].XValue < offsetFrom)
					{
						if(intervalType == IntervalType.Number)
						{
                            intervalFrom = offsetFrom + ChartHelper.GetIntervalSize(offsetFrom, 
								-interval, 
								ConvertIntervalType(intervalType));
						}
						else
						{
                            intervalFrom = offsetFrom - ChartHelper.GetIntervalSize(offsetFrom, 
								interval, 
								ConvertIntervalType(intervalType));
						}
						intervalTo = offsetFrom;

					}
					else
					{
						intervalFrom = offsetFrom;
                        intervalTo = intervalFrom + ChartHelper.GetIntervalSize(intervalFrom, interval, ConvertIntervalType(intervalType));
					}
				}
				else
				{
                    intervalTo = intervalFrom + ChartHelper.GetIntervalSize(intervalFrom, interval, ConvertIntervalType(intervalType));
				}

				//**************************************************
				//** Allocate array for storing temp. 
				//** values of the point
				//**************************************************
				double[]	pointTempValues = new double[outputValuesNumber];


				//**************************************************
				//** Loop through the series points 
				//**************************************************
				bool	lastPoint = false;
				int		emptyPointsSkipped = 0;
				int		pointsNumberInInterval = 0;
				for(int	pointIndex = 0; pointIndex <= input.Points.Count && !lastPoint; pointIndex++)
				{	
					bool	endOfInterval = false;

					//**************************************************
					//** Check if series is sorted by X value
					//**************************************************
					if(pointIndex > 0 && pointIndex < input.Points.Count)
					{
						if(input.Points[pointIndex].XValue < input.Points[pointIndex - 1].XValue)
						{
                            throw (new InvalidOperationException(SR.ExceptionDataManipulatorGroupedSeriesNotSorted));
						}
					}

					//**************************************************
					//** Check if it's the last point
					//**************************************************
					if(pointIndex == input.Points.Count)
					{
						// End of the group interval detected
						lastPoint = true;
						intervalLastIndex = pointIndex - 1;
						pointIndex = intervalLastIndex;
						endOfInterval = true;
					}

					//**************************************************
					//** Check if current point X value is inside current group
					//**************************************************
					if(!endOfInterval && input.Points[pointIndex].XValue >= intervalTo)
					{
						// End of the group interval detected
						if(pointIndex == 0)
						{
							continue;
						}
						intervalLastIndex = pointIndex - 1;
						endOfInterval = true;
					}

					//**************************************************
					//** Process data at end of the interval
					//**************************************************
					if(endOfInterval)
					{
						// Add grouped point only if there are non empty points in the interval
						if(pointsNumberInInterval > emptyPointsSkipped)
						{
							// Finalize the calculation
							ProcessPointValues(
								functions, 
								pointTempValues,
								inputSeries[seriesIndex],
								input.Points[pointIndex],
								pointIndex, 
								intervalFirstIndex, 
								intervalLastIndex,
								true,
								ref emptyPointsSkipped);

							//**************************************************
							//** Calculate the X values
							//**************************************************
							if(functions[0].function == GroupingFunction.Center)
							{
								pointTempValues[0] = (intervalFrom + intervalTo) / 2.0;
							}
							else if(functions[0].function == GroupingFunction.First)
							{
								pointTempValues[0] = intervalFrom;
							}
							if(functions[0].function == GroupingFunction.Last)
							{
								pointTempValues[0] = intervalTo;
							}

							//**************************************************
							//** Create new point object
							//**************************************************
							DataPoint	newPoint = new DataPoint();
							newPoint.ResizeYValueArray(outputValuesNumber - 1);
							newPoint.XValue = pointTempValues[0];
							for(int i = 1; i < pointTempValues.Length; i++)
							{
								newPoint.YValues[i - 1] = pointTempValues[i];
							}
						
							//**************************************************
							//** Remove grouped points if output and input 
							//** series are the same
							//**************************************************
							int	newPointIndex = output.Points.Count;
							if(output == input)
							{
								newPointIndex = intervalFirstIndex;
								pointIndex = newPointIndex + 1;

								// Remove grouped points
								for(int removedPoint = intervalFirstIndex; removedPoint <= intervalLastIndex; removedPoint++)
								{
									output.Points.RemoveAt(intervalFirstIndex);
								}
							}

							//**************************************************
							//** Add point to the output series
							//**************************************************
							output.Points.Insert(newPointIndex, newPoint);
						}

						// Set new From To values of the group interval
						intervalFrom = intervalTo;
                        intervalTo = intervalFrom + ChartHelper.GetIntervalSize(intervalFrom, interval, ConvertIntervalType(intervalType));

						// Set new group interval indexes
						intervalFirstIndex = pointIndex;
						intervalLastIndex = pointIndex;
						
						// Reset number of points in the interval
						pointsNumberInInterval = 0;

						// Reset number of skipped points
						emptyPointsSkipped = 0;

						// Process point once again
						--pointIndex;

						continue;
					}

					//**************************************************
					//** Use current point values in the formula
					//**************************************************
					ProcessPointValues(
						functions, 
						pointTempValues,
						inputSeries[seriesIndex],
						input.Points[pointIndex],
						pointIndex, 
						intervalFirstIndex, 
						intervalLastIndex,
						false,
						ref emptyPointsSkipped);

					// Increase number of points in the group
					++pointsNumberInInterval;
				}
			}
		}

        /// <summary>
        /// Adds current point values to the temp. formula results.
        /// </summary>
        /// <param name="functions">Array of functions type.</param>
        /// <param name="pointTempValues">Temp. point values.</param>
        /// <param name="series">Point series.</param>
        /// <param name="point">Current point.</param>
        /// <param name="pointIndex">Current point index.</param>
        /// <param name="intervalFirstIndex">Index of the first point in the interval.</param>
        /// <param name="intervalLastIndex">Index of the last point in the interval.</param>
        /// <param name="finalPass">Indicates that interval processing is finished.</param>
        /// <param name="numberOfEmptyPoints">Number of skipped points in the interval.</param>
		private void ProcessPointValues(
			GroupingFunctionInfo[]	functions, 
			double[]	pointTempValues,
			Series		series,
			DataPoint	point,
			int	pointIndex, 
			int	intervalFirstIndex, 
			int intervalLastIndex,
			bool finalPass,
			ref int	numberOfEmptyPoints)
		{
			//*******************************************************************
			//** Initialize temp data if it's the first point in the interval
			//*******************************************************************
			if(pointIndex == intervalFirstIndex && !finalPass)
			{
				// Initialize values depending on the function type
				int	funcIndex = 0;
				foreach(GroupingFunctionInfo functionInfo in functions)
				{
					// Check that we do not exced number of input values
					if(funcIndex > point.YValues.Length)
					{
						break;
					}

					// Initialize with zero
					pointTempValues[functionInfo.outputIndex] = 0;

					// Initialize with custom value depending on the formula
					if(functionInfo.function == GroupingFunction.Min)
					{
						pointTempValues[functionInfo.outputIndex] = double.MaxValue;
					}

					else if(functionInfo.function == GroupingFunction.Max)
					{
						pointTempValues[functionInfo.outputIndex] = double.MinValue;
					}

					else if(functionInfo.function == GroupingFunction.First)
					{
						if(funcIndex == 0)
						{
							pointTempValues[0] = point.XValue;
						}
						else
						{
							pointTempValues[functionInfo.outputIndex] = point.YValues[funcIndex-1];
						}
					}

					else if(functionInfo.function == GroupingFunction.HiLo ||
						functionInfo.function == GroupingFunction.HiLoOpCl)
					{
						// Hi
						pointTempValues[functionInfo.outputIndex] = double.MinValue;
						//Lo
						pointTempValues[functionInfo.outputIndex + 1] = double.MaxValue;
						if(functionInfo.function == GroupingFunction.HiLoOpCl)
						{
							//Open
							pointTempValues[functionInfo.outputIndex + 2] = point.YValues[funcIndex-1];
							//Close
							pointTempValues[functionInfo.outputIndex + 3] = 0;
						}
					}

					// Increase current function index
					++funcIndex;
				}
			}

			//*******************************************************************
			//** Add points values using formula
			//*******************************************************************
			if(!finalPass)
			{
				//*******************************************************************
				//** Ignore empty points
				//*******************************************************************
				if(point.IsEmpty && this.IsEmptyPointIgnored)
				{
					++numberOfEmptyPoints;
					return;
				}

				//*******************************************************************
				//** Loop through each grouping function
				//*******************************************************************
				int	funcIndex = 0;
				foreach(GroupingFunctionInfo functionInfo in functions)
				{
					// Check that we do not exced number of input values
					if(funcIndex > point.YValues.Length)
					{
						break;
					}

					// Process point values depending on the formula
					if(functionInfo.function == GroupingFunction.Min &&
						(!point.IsEmpty && this.IsEmptyPointIgnored))
					{
						pointTempValues[functionInfo.outputIndex] = 
							Math.Min(pointTempValues[functionInfo.outputIndex], point.YValues[funcIndex-1]);
					}

					else if(functionInfo.function == GroupingFunction.Max)
					{
						pointTempValues[functionInfo.outputIndex] = 
							Math.Max(pointTempValues[functionInfo.outputIndex], point.YValues[funcIndex-1]);
					}

					else if(functionInfo.function == GroupingFunction.Ave || 
						functionInfo.function == GroupingFunction.Sum)
					{
						if(funcIndex == 0)
						{
							pointTempValues[0] += point.XValue;
						}
						else
						{
							pointTempValues[functionInfo.outputIndex] += point.YValues[funcIndex-1];
						}
					}

					else if(functionInfo.function == GroupingFunction.Variance ||
						functionInfo.function == GroupingFunction.Deviation)
					{
						pointTempValues[functionInfo.outputIndex] += point.YValues[funcIndex-1];
					}

					else if(functionInfo.function == GroupingFunction.Last)
					{
						if(funcIndex == 0)
						{
							pointTempValues[0] = point.XValue;
						}
						else
						{
							pointTempValues[functionInfo.outputIndex] = point.YValues[funcIndex-1];
						}
					}

					else if(functionInfo.function == GroupingFunction.Count)
					{
						pointTempValues[functionInfo.outputIndex] += 1;
					}

					else if(functionInfo.function == GroupingFunction.HiLo ||
						functionInfo.function == GroupingFunction.HiLoOpCl)
					{
						// Hi
						pointTempValues[functionInfo.outputIndex] = 
							Math.Max(pointTempValues[functionInfo.outputIndex], point.YValues[funcIndex-1]);
						// Lo
						pointTempValues[functionInfo.outputIndex + 1] = 
							Math.Min(pointTempValues[functionInfo.outputIndex + 1], point.YValues[funcIndex-1]);
						if(functionInfo.function == GroupingFunction.HiLoOpCl)
						{
							// Close
							pointTempValues[functionInfo.outputIndex + 3] = point.YValues[funcIndex-1];
						}
					}

					// Increase current function index
					++funcIndex;
				}
			}


			//*******************************************************************
			//** Adjust formula results at final pass
			//*******************************************************************
			if(finalPass)
			{
				int	funcIndex = 0;
				foreach(GroupingFunctionInfo functionInfo in functions)
				{
					// Check that we do not exceed number of input values
					if(funcIndex > point.YValues.Length)
					{
						break;
					}

					if(functionInfo.function == GroupingFunction.Ave)
					{
						pointTempValues[functionInfo.outputIndex] /= intervalLastIndex - intervalFirstIndex - numberOfEmptyPoints + 1;
					}

					if(functionInfo.function == GroupingFunction.DistinctCount)
					{
						// Initialize value with zero
						pointTempValues[functionInfo.outputIndex] = 0;

						// Create a list of uniques values
						ArrayList uniqueValues = new ArrayList(intervalLastIndex - intervalFirstIndex + 1);

						// Second pass through inteval points required for calculations
						for(int secondPassIndex = intervalFirstIndex; secondPassIndex <= intervalLastIndex; secondPassIndex++)
						{
							// Ignore empty points
							if(series.Points[secondPassIndex].IsEmpty && this.IsEmptyPointIgnored)
							{
								continue;
							}

							// Check if current value is in the unique list
							if(!uniqueValues.Contains(series.Points[secondPassIndex].YValues[funcIndex-1]))
							{
								uniqueValues.Add(series.Points[secondPassIndex].YValues[funcIndex-1]);
							}
						}

						// Get count of unique values
						pointTempValues[functionInfo.outputIndex] = uniqueValues.Count;
					}

					else if(functionInfo.function == GroupingFunction.Variance ||
						functionInfo.function == GroupingFunction.Deviation)
					{
						// Calculate average first
						double average = pointTempValues[functionInfo.outputIndex] / (intervalLastIndex - intervalFirstIndex - numberOfEmptyPoints + 1);

						// Second pass through inteval points required for calculations
						pointTempValues[functionInfo.outputIndex] = 0;
						for(int secondPassIndex = intervalFirstIndex; secondPassIndex <= intervalLastIndex; secondPassIndex++)
						{
							// Ignore empty points
							if(series.Points[secondPassIndex].IsEmpty && this.IsEmptyPointIgnored)
							{
								continue;
							}

							pointTempValues[functionInfo.outputIndex] += 
								Math.Pow(series.Points[secondPassIndex].YValues[funcIndex-1] - average, 2);
						}

						// Divide by points number
						pointTempValues[functionInfo.outputIndex] /= 
							intervalLastIndex - intervalFirstIndex - numberOfEmptyPoints + 1;

						// If calculating the deviation - take a square root of variance
						if(functionInfo.function == GroupingFunction.Deviation)
						{
							pointTempValues[functionInfo.outputIndex] = 
								Math.Sqrt(pointTempValues[functionInfo.outputIndex]);
						}
					}

					// Increase current function index
					++funcIndex;
				}
			}

		}

		/// <summary>
		/// Checks the formula format and returns an array of formula types
		/// for each X and each Y value of the input series.
		/// </summary>
		/// <param name="inputSeries">Array of input series.</param>
		/// <param name="formula">Formula string.</param>
		/// <param name="outputValuesNumber">Number of values in output series.</param>
		/// <returns>Array of functions for each Y value.</returns>
		private GroupingFunctionInfo[] GetGroupingFunctions(Series[] inputSeries, string formula, out int outputValuesNumber)
		{
			// Get maximum number of Y values in all series
			int	numberOfYValues = 0;
			foreach(Series series in inputSeries)
			{
				numberOfYValues = (int)Math.Max(numberOfYValues, series.YValuesPerPoint);
			}

			// Allocate memory for the result array for X and each Y values
			GroupingFunctionInfo[]	result = new GroupingFunctionInfo[numberOfYValues + 1];
			for(int index = 0 ; index < result.Length; index++)
			{
				result[index] = new GroupingFunctionInfo();
			}

			// Split formula by comma
			string[]	valueFormulas = formula.Split(',');

			// At least one formula must be specified
			if(valueFormulas.Length == 0)
			{
                throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaUndefined));
			}

			// Check each formula in the array
			GroupingFunctionInfo	defaultFormula = new GroupingFunctionInfo();
			foreach(string s in valueFormulas)
			{
				// Trim white space and make upper case
				string formulaString = s.Trim();
				formulaString = formulaString.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

				// Get value index and formula type from the string
				int	valueIndex = 1;
				GroupingFunction	formulaType = ParseFormulaAndValueType(formulaString, out valueIndex);

				// Save the default (first) formula
				if(defaultFormula.function == GroupingFunction.None)
				{
					defaultFormula.function = formulaType;
				}

				// Check that value index do not exceed the max values number
				if(valueIndex >= result.Length)
				{
					throw(new ArgumentException(SR.ExceptionDataManipulatorYValuesIndexExceeded( formulaString )));
				}

				// Check if formula for this value type was already set
				if(result[valueIndex].function != GroupingFunction.None)
				{
                    throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaAlreadyDefined(formulaString)));
				}
                
				// Set formula type
				result[valueIndex].function = formulaType;
			}
				
			// Apply default formula for non set X value
			if(result[0].function == GroupingFunction.None)
			{
				result[0].function = GroupingFunction.First;
			}

			// Apply default formula for all non set Y values
			for(int funcIndex = 1; funcIndex < result.Length; funcIndex++)
			{
				if(result[funcIndex].function == GroupingFunction.None)
				{
					result[funcIndex].function = defaultFormula.function;
				}
			}

			// Specify output value index
			outputValuesNumber = 0;
			for(int funcIndex = 0; funcIndex < result.Length; funcIndex++)
			{
				result[funcIndex].outputIndex = outputValuesNumber;

				if(result[funcIndex].function == GroupingFunction.HiLoOpCl)
				{
					outputValuesNumber += 3;
				}
				else if(result[funcIndex].function == GroupingFunction.HiLo)
				{
					outputValuesNumber += 1;
				}

				++outputValuesNumber;
			}

			// X value formula can be FIRST, LAST and AVE
			if(result[0].function != GroupingFunction.First && 
				result[0].function != GroupingFunction.Last && 
				result[0].function != GroupingFunction.Center)
			{
                throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaUnsupported));
			}

			return result;
		}

		/// <summary>
		/// Parse one formula with optional value prefix.
		/// Example: "Y2:MAX"
		/// </summary>
		/// <param name="formulaString">One formula name with optional value prefix.</param>
		/// <param name="valueIndex">Return value index.</param>
		/// <returns>Formula type.</returns>
		private GroupingFunction ParseFormulaAndValueType(string formulaString, out int valueIndex)
		{
			// Initialize value index as first Y value (default)
			valueIndex = 1;

			// Split formula by optional ':' character
			string[] formulaParts = formulaString.Split(':');

			// There must be at least one and no more than two result strings
			if(formulaParts.Length < 1 && formulaParts.Length > 2)
			{
				throw(new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaFormatInvalid( formulaString )));
			}

			// Check specified value type
			if(formulaParts.Length == 2)
			{
				if(formulaParts[0] == "X")
				{
					valueIndex = 0;
				}
				else if(formulaParts[0].StartsWith("Y", StringComparison.Ordinal))
				{
					formulaParts[0] = formulaParts[0].TrimStart('Y');

					if(formulaParts[0].Length == 0)
					{
						valueIndex = 1;
					}
					else
					{
						// Try to convert the rest of the string to integer
						try
						{
							valueIndex = Int32.Parse(formulaParts[0], System.Globalization.CultureInfo.InvariantCulture);
						}
						catch(System.Exception)
						{
                            throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaFormatInvalid( formulaString )));
						}
					}
				}
				else
				{
                    throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaFormatInvalid( formulaString )));
				}
			}

			// Check formula name
			if(formulaParts[formulaParts.Length - 1] == "MIN")
				return GroupingFunction.Min;
			else if(formulaParts[formulaParts.Length - 1] == "MAX")
				return GroupingFunction.Max;
			else if(formulaParts[formulaParts.Length - 1] == "AVE")
				return GroupingFunction.Ave;
			else if(formulaParts[formulaParts.Length - 1] == "SUM")
				return GroupingFunction.Sum;
			else if(formulaParts[formulaParts.Length - 1] == "FIRST")
				return GroupingFunction.First;
			else if(formulaParts[formulaParts.Length - 1] == "LAST")
				return GroupingFunction.Last;
			else if(formulaParts[formulaParts.Length - 1] == "HILOOPCL")
				return GroupingFunction.HiLoOpCl;
			else if(formulaParts[formulaParts.Length - 1] == "HILO")
				return GroupingFunction.HiLo;
			else if(formulaParts[formulaParts.Length - 1] == "COUNT")
				return GroupingFunction.Count;
			else if(formulaParts[formulaParts.Length - 1] == "DISTINCTCOUNT")
				return GroupingFunction.DistinctCount;
			else if(formulaParts[formulaParts.Length - 1] == "VARIANCE")
				return GroupingFunction.Variance;
			else if(formulaParts[formulaParts.Length - 1] == "DEVIATION")
				return GroupingFunction.Deviation;
			else if(formulaParts[formulaParts.Length - 1] == "CENTER")
				return GroupingFunction.Center;
			
			// Invalid formula name
            throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingFormulaNameInvalid(formulaString)));
		}

        /// <summary>
        /// Checks if input/output series parameters are correct.
        /// If not - fires an exception
        /// </summary>
        /// <param name="inputSeries">Input series array.</param>
        /// <param name="outputSeries">Output series array.</param>
		private void CheckSeriesArrays(Series[] inputSeries, Series[] outputSeries)
		{
			// At least one series must be in the input series
			if(inputSeries == null || inputSeries.Length == 0)
			{
                throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingInputSeriesUndefined));
			}

			// Output series must be empty or have the same number of items
			if(outputSeries != null && outputSeries.Length != inputSeries.Length)
			{
                throw (new ArgumentException(SR.ExceptionDataManipulatorGroupingInputOutputSeriesNumberMismatch));
			}
		}

		#endregion

		#region Grouping overloaded methods

		/// <summary>
        /// Groups data using one or more formulas. 
        /// The series that is grouped is cleared of its original data, and used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="inputSeries">Input series.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			Series inputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Group(formula, interval, intervalType, inputSeries, null);
		}

		/// <summary>
        /// Groups data using one or more formulas. 
        /// Series are cleared of their original data and used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			string inputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");

			Group(formula, interval, intervalType, inputSeriesName, "");
		}

		/// <summary>
        /// Groups data using one or more formulas. 
        /// The series that is grouped is cleared of its original data, and used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="inputSeries">Input series.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			double intervalOffset,
			IntervalType intervalOffsetType, 
			Series inputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            
            Group(formula, interval, intervalType, intervalOffset, intervalOffsetType, inputSeries, null);
		}

		/// <summary>
        /// Groups data using one or more formulas. 
        /// Series are cleared of their original data and used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			double intervalOffset,
			IntervalType intervalOffsetType, 
			string inputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");
            
            Group(formula, interval, intervalType, intervalOffset, intervalOffsetType, inputSeriesName, "");
		}

		/// <summary>
        /// Groups series data by axis labels using one or more formulas. 
        /// Output series are used to store the grouped data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		/// <param name="outputSeriesName">Comma separated list of output series names.</param>
		public void GroupByAxisLabel(string formula, string inputSeriesName, string outputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");

			GroupByAxisLabel(formula, 
				ConvertToSeriesArray(inputSeriesName, false), 
				ConvertToSeriesArray(outputSeriesName, true));
		}

		/// <summary>
        /// Groups a series' data by axis labels using one or more formulas. 
        /// The series is cleared of its original data, and then used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="inputSeries">Input data series.</param>
		public void GroupByAxisLabel(string formula, Series inputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            
            GroupByAxisLabel(formula, inputSeries, null);
		}

		/// <summary>
        /// Groups series data by axis labels using one or more formulas. 
        /// Each series that is grouped is cleared of its original data, and used to store the new data points. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		public void GroupByAxisLabel(string formula, string inputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName"); 
            
            GroupByAxisLabel(formula, inputSeriesName, null);
		}


		/// <summary>
        /// Groups series using one or more formulas. 
        /// Output series are used to store the grouped data points, and an offset can be used for intervals.  
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		/// <param name="outputSeriesName">Comma separated list of output series names.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			double intervalOffset,
			IntervalType intervalOffsetType, 
			string inputSeriesName, 
			string outputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");

			Group(formula,
				interval, 
				intervalType, 
				intervalOffset,
				intervalOffsetType, 
				ConvertToSeriesArray(inputSeriesName, false), 
				ConvertToSeriesArray(outputSeriesName, true));
		}
		
		/// <summary>
        /// Groups a series' data using one or more formulas. 
        /// An output series is used to store the grouped data points.  
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="inputSeries">Input data series.</param>
		/// <param name="outputSeries">Output data series.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			Series inputSeries, 
			Series outputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Group(formula, interval, intervalType, 0, IntervalType.Number, inputSeries, outputSeries);
		}

		/// <summary>
        /// Groups data for series using one or more formulas. 
        /// Output series are used to store the grouped data points.  
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="inputSeriesName">Comma separated list of input series names.</param>
		/// <param name="outputSeriesName">Comma separated list of output series names.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			string inputSeriesName, 
			string outputSeriesName)
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");

			Group(formula, interval, intervalType, 0, IntervalType.Number, inputSeriesName, outputSeriesName);
		}

		/// <summary>
        /// Groups a series using one or more formulas. 
        /// An output series is used to store the grouped data points, and an offset can be used for intervals. 
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="intervalType">Interval type.</param>
		/// <param name="intervalOffset">Interval offset size.</param>
		/// <param name="intervalOffsetType">Interval offset type.</param>
		/// <param name="inputSeries">Input data series.</param>
		/// <param name="outputSeries">Output data series.</param>
		public void Group(string formula,
			double interval, 
			IntervalType intervalType, 
			double intervalOffset,
			IntervalType intervalOffsetType, 
			Series inputSeries, 
			Series outputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");

			Group(formula,
				interval, 
				intervalType, 
				intervalOffset,
				intervalOffsetType, 
				ConvertToSeriesArray(inputSeries, false), 
				ConvertToSeriesArray(outputSeries, false));
		}

		/// <summary>
        /// Groups a series' data by axis labels using one or more formulas. 
        /// An output series is used to store the grouped data points.  
		/// </summary>
		/// <param name="formula">Grouping formula.</param>
		/// <param name="inputSeries">Input data series.</param>
		/// <param name="outputSeries">Output data series.</param>
		public void GroupByAxisLabel(string formula, Series inputSeries, Series outputSeries)
		{
            // Check arguments
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            
            GroupByAxisLabel(formula, 
				ConvertToSeriesArray(inputSeries, false), 
				ConvertToSeriesArray(outputSeries, false));
		}

		#endregion
	}
}

