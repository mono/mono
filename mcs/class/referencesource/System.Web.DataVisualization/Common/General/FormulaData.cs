//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		DataFormula.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	DataFormula
//
//  Purpose:	DataFormula class provides properties and methods, 
//				which prepare series data for technical analyses 
//				and time series and forecasting formulas and prepare 
//				output data to be displayed as a chart.
//
//	Reviewed:	GS - August 6, 2002
//				AG - August 7, 2002
//              AG - Microsoft 15, 2007
//
//===================================================================

#region Used Namespace
using System;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
#endregion

#if Microsoft_CONTROL
using System.Windows.Forms.DataVisualization.Charting.Formulas;

namespace System.Windows.Forms.DataVisualization.Charting
#else
using System.Web.UI.DataVisualization.Charting.Formulas;
    
namespace System.Web.UI.DataVisualization.Charting
#endif
{
    #region Financial Formula Name enumeration

    /// <summary>
    /// An enumeration of financial formula names.
    /// </summary>
    public enum FinancialFormula
    {
        /// <summary>
        /// Accumulation Distribution formula. This indicator uses a relationship 
        /// between volume and prices to estimate the strength of price movements, 
        /// and if volume is increased, there is a high probability that prices will go up.
        /// </summary>
        AccumulationDistribution,

        /// <summary>
        /// Average True Range indicator.  It measures commitment and compares 
        /// the range between the High, Low and Close prices. 
        /// </summary>
        AverageTrueRange,

        /// <summary>
        /// Bollinger Bands indicators.  They are plotted at standard deviation levels 
        /// above and below a simple moving average.
        /// </summary>
        BollingerBands,

        /// <summary>
        /// Chaikin Oscillator indicator. It is the difference between a 3-day 
        /// exponential moving average and a 10-day exponential moving average 
        /// applied to the Accumulation Distribution.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chaikin")]
        ChaikinOscillator,

        /// <summary>
        /// Commodity Channel Index. It compares prices with their moving averages.
        /// </summary>
        CommodityChannelIndex,

        /// <summary>
        /// Detrended Price Oscillator.  It attempts to remove trend from prices. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Detrended")]
        DetrendedPriceOscillator,

        /// <summary>
        /// Ease of Movement deals with the relationship between volume and price change, 
        /// and uses volume to indicate how strong a trend is for prices.
        /// </summary>
        EaseOfMovement,

        /// <summary>
        /// Envelopes are plotted above and below a moving average using a specified percentage 
        /// as the shift.
        /// </summary>
        Envelopes,

        /// <summary>
        /// An Exponential Moving Average is an average of data calculated over a period of time 
        /// where the most recent days have more weight.  
        /// </summary>
        ExponentialMovingAverage,

        /// <summary>
        /// Forecasting.  It predicts future values using historical observations.
        /// </summary>
        Forecasting,

        /// <summary>
        /// Moving Average Convergence/Divergence indicator.  It compares two 
        /// moving averages of prices and is used with a 9-day Exponential 
        /// Moving average as a signal, which indicates buying and selling moments.
        /// </summary>
        MovingAverageConvergenceDivergence,

        /// <summary>
        /// The Mass Index is used to predict trend reversal by comparing the 
        /// difference and range between High and Low prices. 
        /// </summary>
        MassIndex,

        /// <summary>
        /// Median prices are mid-point values of daily prices and can be used 
        /// as a filter for trend indicators. 
        /// </summary>
        MedianPrice,

        /// <summary>
        /// The Money Flow indicator compares upward changes and downward changes
        /// of volume-weighted typical prices. 
        /// </summary>
        MoneyFlow,

        /// <summary>
        /// The Negative Volume Index should be used together with the Positive Volume index, 
        /// and the Negative Volume Index only changes if the volume decreases from the previous day.
        /// </summary>
        NegativeVolumeIndex,

        /// <summary>
        /// The On Balance Volume indicator measures positive and negative volume flow.
        /// </summary>
        OnBalanceVolume,

        /// <summary>
        /// The Performance indicator compares a current closing price (or any other price) with 
        /// the first closing value (from the first time period).
        /// </summary>
        Performance,

        /// <summary>
        /// The Positive Volume Index should be used together with the Negative Volume index. 
        /// The Positive volume index only changes if the volume decreases from the previous day. 
        /// </summary>
        PositiveVolumeIndex,

        /// <summary>
        /// The Price Volume Trend is a cumulative volume total that is calculated using 
        /// relative changes of the closing price, and should be used with other indicators.  
        /// </summary>
        PriceVolumeTrend,

        /// <summary>
        /// The Rate of Change indicator compares a specified closing price with the current price. 
        /// </summary>
        RateOfChange,

        /// <summary>
        /// The Relative Strength Index is a momentum oscillator that compares upward movements 
        /// of the closing price with downward movements, and results in values that range from 0 to 100.
        /// </summary>
        RelativeStrengthIndex,

        /// <summary>
        /// A Simple Moving Average is an average of data calculated over a period of time. 
        /// The moving average is the most popular price indicator used in technical analysis, 
        /// and can be used with any price (e.g. Hi, Low, Open and Close) 
        /// or it can be applied to other indicators. 
        /// </summary>
        MovingAverage,

        /// <summary>
        /// Standard deviation is used to indicate volatility, and measures 
        /// the difference between values (e.g. closing price) and their moving average.  
        /// </summary>
        StandardDeviation,

        /// <summary>
        /// The Stochastic Indicator helps to find trend reversal by searching in a period for
        /// when the closing prices are close to low prices in an upward trending market
        /// and for when the closing prices are close to high prices in a downward trending market.
        /// </summary>
        StochasticIndicator,

        /// <summary>
        /// A Triangular Moving Average is an average of data calculated over a period of time 
        /// where the middle portion of data has more weight.
        /// </summary>
        TriangularMovingAverage,

        /// <summary>
        /// The Triple Exponential Moving Average is based on a triple moving average of the closing Price.
        /// Its purpose is to eliminate short cycles.  This indicator keeps the closing price 
        /// in trends that are shorter than the specified period. 
        /// </summary>
        TripleExponentialMovingAverage,

        /// <summary>
        /// Typical price is the average value of daily prices, and can be used as a filter for trend indicators.
        /// </summary>
        TypicalPrice,

        /// <summary>
        /// The Volatility Chaikins indicator measures the difference between High and Low prices, 
        /// and is used to indicate tops or bottoms of the market.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chaikins")]
        VolatilityChaikins,

        /// <summary>
        /// The Volume oscillator attempts to identify trends in volume by comparing two moving averages: 
        /// one with a short period and another with a longer period.
        /// </summary>
        VolumeOscillator,

        /// <summary>
        /// The Weighted Close formula calculates the average value of daily prices. 
        /// The only difference between Typical Price and the Weighted Close is that the closing price 
        /// has extra weight, and is considered the most important price. 
        /// </summary>
        WeightedClose,

        /// <summary>
        /// A Weighted Moving Average is an average of data calculated over a period of time, 
        /// where greater weight is attached to the most recent data. 
        /// </summary>
        WeightedMovingAverage,

        /// <summary>
        /// William's %R is a momentum indicator, and is used to measure overbought and oversold levels. 
        /// </summary>
        WilliamsR
    }

    #endregion  // Financial Formula Name enumeration

    /// <summary>
    /// The DataFormula class provides properties and methods, which prepare series 
    /// data for technical analysis, apply formulas on the series data 
    /// and prepare output data to be displayed as a chart.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class DataFormula
    {
        #region Data Formulas fields

        internal const string IndexedSeriesLabelsSourceAttr = "__IndexedSeriesLabelsSource__";

        //***********************************************************
        //** Private data members, which store properties values
        //***********************************************************
        private bool _isEmptyPointIgnored = true;

        private string[] _extraParameters;

        /// <summary>
        /// All X values are zero.
        /// </summary>
        private bool _zeroXValues = false;

        /// <summary>
        /// Utility class for Statistical formulas
        /// </summary>
        private StatisticFormula _statistics;

        /// <summary>
        /// Reference to the Common elements
        /// </summary>
        internal CommonElements Common;


        #endregion

        #region Data Formulas methods

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataFormula()
        {
            _statistics = new StatisticFormula(this);

            _extraParameters = new string[1];
            _extraParameters[0] = false.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="parameters">Formula parameters</param>
        /// <param name="inputSeries">Comma separated input data series names and optional X and Y values names.</param>
        /// <param name="outputSeries">Comma separated output data series names and optional X and Y values names.</param>
        internal void Formula(string formulaName, string parameters, string inputSeries, string outputSeries)
        {            
            // Array of series
            Series[] inSeries;
            Series[] outSeries;

            // Commented out as InsertEmptyDataPoints is currently commented out.
            // This field is not used anywhere else, but we might need it if we uncomment all ---- code parts in this method. (krisztb 4/29/08)
            // True if formulas are statistical
            //bool statisticalFormulas = false;

            // Array of Y value indexes
            int[] inValueIndexes;
            int[] outValueIndexes;

            // Matrix with double values ( used in formula modules )
            double[][] inValues;
            double[][] inNoEmptyValues;
            double[][] outValues = null;
            string[][] outLabels = null;

            // Array with parameters
            string[] parameterList;

            // Split comma separated parameter list in the array of strings.
            SplitParameters(parameters, out parameterList);

            // Split comma separated series and Y values list in the array of 
            // Series and indexes to Y values.
            ConvertToArrays(inputSeries, out inSeries, out inValueIndexes, true);
            ConvertToArrays(outputSeries, out outSeries, out outValueIndexes, false);

            // Create indexes if all x values are 0
            //ConvertZeroXToIndex( ref inSeries );

            // Set X value AxisName for output series. 
            foreach (Series outSeriesItem in outSeries)
            {
                if (inSeries[0] != null)
                {
                    outSeriesItem.XValueType = inSeries[0].XValueType;
                }
            }

            // This method will convert array of Series and array of Y value 
            // indexes to matrix of double values.
            GetDoubleArray(inSeries, inValueIndexes, out inValues);

            // Remove columns with empty values from matrix
            if (!DifferentNumberOfSeries(inValues))
            {
                RemoveEmptyValues(inValues, out inNoEmptyValues);
            }
            else
            {
                inNoEmptyValues = inValues;
            }

            // Call a formula from formula modules
            string moduleName = null;
            for (int module = 0; module < Common.FormulaRegistry.Count; module++)
            {
                moduleName = Common.FormulaRegistry.GetModuleName(module);
                Common.FormulaRegistry.GetFormulaModule(moduleName).Formula(formulaName, inNoEmptyValues, out outValues, parameterList, _extraParameters, out outLabels);

                // Commented out as InsertEmptyDataPoints is currently commented out (see next block).
                // It set the statisticalFormulas field that was used to test whether to insert empty data points. (krisztb 4/29/08)
                //if( outValues != null )
                //{
                //    if (moduleName == SR.FormulaNameStatisticalAnalysis)
                //    {
                //        statisticalFormulas = true;
                //    }
                //    break;
                //}

                // Check if formula was found by detecting output 
                if (outValues != null)
                {
                    // Exit the loop
                    break;
                }
            }

            if (outValues == null)
                throw new ArgumentException(SR.ExceptionFormulaNotFound(formulaName));

            // Insert empty data points

            //
            // This has been commented out as InsertEmptyDataPoints is currently commented out.
            // In its current implementation it didn't do anything other than assign the second
            // parameter to the third, so ultimately it was a no op. --Microsoft 4/21/08
            //
            //if( !statisticalFormulas )
            //{
            //    InsertEmptyDataPoints( inValues, outValues, out outValues );
            //}

            // Fill Series with results from matrix with double values using Y value indexes.
            SetDoubleArray(outSeries, outValueIndexes, outValues, outLabels);

            if (_zeroXValues)
            {
                // we have indexed series : proceed to align output series.
                foreach (Series series in outSeries)
                {
                    if (series.Points.Count > 0)
                    {
                        // get the last xValue: the formula processing is 
                        double topXValue = series.Points[series.Points.Count - 1].XValue;
                        this.Common.Chart.DataManipulator.InsertEmptyPoints(1, IntervalType.Number, 0, IntervalType.Number, 1, topXValue, series);
                        foreach (DataPoint point in series.Points)
                        {
                            point.XValue = 0;
                        }
                    }
                }
            }
            // Copy axis labels from the original series into the calculated series            
            CopyAxisLabels(inSeries, outSeries);
        }

        /// <summary>
        /// Copy axis labels from the original series into the calculated series 
        /// </summary>
        /// <param name="inSeries">array of input series</param>
        /// <param name="outSeries">array of output series</param>
        private void CopyAxisLabels(Series[] inSeries, Series[] outSeries)
        {
            //Loop through the pairs of input and output series
            int seriesIndex = 0;
            while (seriesIndex < inSeries.Length && seriesIndex < outSeries.Length)
            {
                Series inputSeries = inSeries[seriesIndex];
                Series outputSeries = outSeries[seriesIndex];

                //Depending on whether or not the source series has X Values we need to use two different search algorithms
                if (_zeroXValues)
                {   //If we have the empty XValues then the source series should have all the AxisLabels
                    // -- set the indexed series labels source 
                    outputSeries[DataFormula.IndexedSeriesLabelsSourceAttr] = inputSeries.Name;
                }
                else
                {   //If the source series has XValues - loop through the input series points looking for the points with AxisLabels set
                    int outIndex = 0;
                    foreach (DataPoint inputPoint in inputSeries.Points)
                    {
                        if (!String.IsNullOrEmpty(inputPoint.AxisLabel))
                        {
                            //If the Axis label is set we need to find the corresponding point by the X value
                            //Most probably the points are in the same order so lets first try the corresponding point in the output series
                            if (outIndex < outputSeries.Points.Count && inputPoint.XValue == outputSeries.Points[outIndex].XValue)
                            {   // Yes, the corresponding point in the outputSeries has the same XValue as inputPoint -> copy axis label
                                outputSeries.Points[outIndex].AxisLabel = inputPoint.AxisLabel;
                            }
                            else
                            {
                                //The correspong point has a different x value -> lets go through output series and find the value with the same X
                                outIndex = 0;
                                foreach (DataPoint outputPoint in outputSeries.Points)
                                {
                                    if (inputPoint.XValue == outputPoint.XValue)
                                    {   //Found the point with the same XValue - copy axis label and break
                                        outputPoint.AxisLabel = inputPoint.AxisLabel;
                                        break;
                                    }
                                    outIndex++;
                                }
                            }
                        }
                        outIndex++;
                    }
                }
                //Sync next pair of input and output series...
                seriesIndex++;
            }
        }


        /// <summary>
        /// This method will set series X and Y values from matrix of 
        /// double values.
        /// </summary>
        /// <param name="outputSeries">Array of output series</param>
        /// <param name="valueIndex">Array of Y value indexes</param>
        /// <param name="outputValues">Array of doubles which will be used to fill series</param>
        /// <param name="outputLabels">Array of labels</param>
        internal void SetDoubleArray(Series[] outputSeries, int[] valueIndex, double[][] outputValues, string[][] outputLabels)
        {
            // Validation
            if (outputSeries.Length != valueIndex.Length)
            {
                throw new ArgumentException(SR.ExceptionFormulaDataItemsNumberMismatch);
            }

            // Number of output series is not correct
            if (outputSeries.Length < outputValues.Length - 1)
            {
                throw new ArgumentException(SR.ExceptionFormulaDataOutputSeriesNumberYValuesIncorrect);
            }

            int seriesIndex = 0;
            foreach (Series series in outputSeries)
            {
                if (seriesIndex + 1 > outputValues.Length - 1)
                {
                    break;
                }

                // If there is different number of data points.
                if (series.Points.Count != outputValues[seriesIndex].Length)
                {
                    // Delete all points
                    series.Points.Clear();
                }

                // Set the number of y values
                if (series.YValuesPerPoint < valueIndex[seriesIndex])
                {
                    series.YValuesPerPoint = valueIndex[seriesIndex];
                }

                for (int pointIndex = 0; pointIndex < outputValues[0].Length; pointIndex++)
                {
                    // Create a new series and fill data
                    if (series.Points.Count != outputValues[seriesIndex].Length)
                    {
                        // Add data points to series.
                        series.Points.AddXY(outputValues[0][pointIndex], 0);

                        // Set Labels
                        if (outputLabels != null)
                        {
                            series.Points[pointIndex].Label = outputLabels[seriesIndex][pointIndex];
                        }

                        // Set empty data points or Y values
                        if (Double.IsNaN(outputValues[seriesIndex + 1][pointIndex]))
                            series.Points[pointIndex].IsEmpty = true;
                        else
                            series.Points[pointIndex].YValues[valueIndex[seriesIndex] - 1] = outputValues[seriesIndex + 1][pointIndex];
                    }
                    // Use existing series and set Y values.
                    else
                    {
                        if (series.Points[pointIndex].XValue != outputValues[0][pointIndex] && !_zeroXValues)
                        {
                            throw new InvalidOperationException(SR.ExceptionFormulaXValuesNotAligned);
                        }

                        // Set empty data points or Y values
                        if (Double.IsNaN(outputValues[seriesIndex + 1][pointIndex]))
                            series.Points[pointIndex].IsEmpty = true;
                        else
                        {
                            series.Points[pointIndex].YValues[valueIndex[seriesIndex] - 1] = outputValues[seriesIndex + 1][pointIndex];

                            // Set Labels
                            if (outputLabels != null)
                            {
                                series.Points[pointIndex].Label = outputLabels[seriesIndex][pointIndex];
                            }
                        }
                    }
                }
                seriesIndex++;
            }
        }

        /// <summary>
        /// This method will convert a string with information about 
        /// series and y values to two arrays. The first array will 
        /// contain series and the second array will contain 
        /// corresponding indexes to y values for every series. 
        /// The arrays have to have the same number of items.
        /// </summary>
        /// <param name="inputString">A string with information about series and Y values</param>
        /// <param name="seiesArray">Array of Data Series</param>
        /// <param name="valueArray">Array of Y value indexes</param>
        /// <param name="inputSeries">Do not create new series if input series are used</param>
        private void ConvertToArrays(string inputString, out Series[] seiesArray, out int[] valueArray, bool inputSeries)
        {
            // Split string by comma
            string[] subStrings = inputString.Split(',');

            // Create array of series
            seiesArray = new Series[subStrings.Length];

            // Create array of integers - values
            valueArray = new int[subStrings.Length];

            int index = 0;

            foreach (string str in subStrings)
            {
                string[] parts = str.Split(':');


                // There must be at least one and no more than two result strings
                if (parts.Length < 1 && parts.Length > 2)
                {
                    throw (new ArgumentException(SR.ExceptionFormulaDataFormatInvalid(str)));
                }

                // Initialize value index as first Y value (default)
                int valueIndex = 1;

                // Check specified value type
                if (parts.Length == 2)
                {
                    if (parts[1].StartsWith("Y", StringComparison.Ordinal))
                    {
                        parts[1] = parts[1].TrimStart('Y');

                        if (parts[1].Length == 0)
                        {
                            valueIndex = 1;
                        }
                        else
                        {
                            // Try to convert the rest of the string to integer
                            try
                            {
                                valueIndex = Int32.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch (System.Exception)
                            {
                                throw (new ArgumentException(SR.ExceptionFormulaDataFormatInvalid(str)));
                            }
                        }
                    }
                    else
                    {
                        throw (new ArgumentException(SR.ExceptionFormulaDataSeriesNameNotFound(str)));
                    }
                }

                // Set Y value indexes
                valueArray[index] = valueIndex;

                // Set series
                try
                {
                    seiesArray[index] = Common.DataManager.Series[parts[0].Trim()];
                }
                catch (System.Exception)
                {
                    // Series doesn't exist.
                    if (!inputSeries)
                    {
                        // Create a new series if output series
                        Common.DataManager.Series.Add(new Series(parts[0]));
                        seiesArray[index] = Common.DataManager.Series[parts[0]];
                    }
                    else
                        throw (new ArgumentException(SR.ExceptionFormulaDataSeriesNameNotFoundInCollection(str)));
                }
                index++;
            }
        }


        /// <summary>
        /// Returns Jagged Arrays of doubles from array of series. 
        /// A jagged array is merely an array of arrays and
        /// it doesn't have to be square. The first item is array of 
        /// X values from the first series
        /// </summary>
        /// <param name="inputSeries">Array of Data Series</param>
        /// <param name="valueIndex">Array with indexes which represent value from data point: 0 = X, 1 = Y, 2 = Y2, 3 = Y3</param>
        /// <param name="output">Jagged Arrays of doubles</param>
        private void GetDoubleArray(Series[] inputSeries, int[] valueIndex, out double[][] output)
        {
            GetDoubleArray(inputSeries, valueIndex, out output, false);
        }

        /// <summary>
        /// Returns Jagged Arrays of doubles from array of series. 
        /// A jagged array is merely an array of arrays and
        /// it doesn't have to be square. The first item is array of 
        /// X values from the first series
        /// </summary>
        /// <param name="inputSeries">Array of Data Series</param>
        /// <param name="valueIndex">Array with indexes which represent value from data point: 0 = X, 1 = Y, 2 = Y2, 3 = Y3</param>
        /// <param name="output">Jagged Arrays of doubles</param>
        /// <param name="ignoreZeroX">Ignore Zero X values</param>
        private void GetDoubleArray(Series[] inputSeries, int[] valueIndex, out double[][] output, bool ignoreZeroX)
        {
            // Allocate a memory.
            output = new double[inputSeries.Length + 1][];

            // Check the length of the array of series and array of value indexes.
            if (inputSeries.Length != valueIndex.Length)
            {
                throw new ArgumentException(SR.ExceptionFormulaDataItemsNumberMismatch2);
            }

            // Find Maximum number of data points
            int maxNumOfPoints = int.MinValue;
            Series seriesWidthMaxPoints = null;
            foreach (Series series in inputSeries)
            {
                if (maxNumOfPoints < series.Points.Count)
                {
                    maxNumOfPoints = series.Points.Count;
                    seriesWidthMaxPoints = series;
                }
            }

            // *********************************************************
            // Set X values
            // *********************************************************

            // Check if all X values are zero
            foreach (DataPoint point in inputSeries[0].Points)
            {
                _zeroXValues = true;

                if (point.XValue != 0.0)
                {
                    _zeroXValues = false;
                    break;
                }
            }

            if (_zeroXValues && !ignoreZeroX)
            {
                // Check X values input alignment
                CheckXValuesAlignment(inputSeries);
            }


            // Data point index
            int indexPoint = 0;

            // Allocate memory for X values.
            output[0] = new double[maxNumOfPoints];

            // Data Points loop
            foreach (DataPoint point in seriesWidthMaxPoints.Points)
            {
                // Set X value
                if (_zeroXValues)
                    output[0][indexPoint] = (double)indexPoint + 1.0;
                else
                    output[0][indexPoint] = point.XValue;

                // Increase data point index.
                indexPoint++;
            }

            // *********************************************************
            // Set Y values
            // *********************************************************
            // Data Series Loop
            int indexSeries = 1;
            foreach (Series series in inputSeries)
            {
                output[indexSeries] = new double[series.Points.Count];
                indexPoint = 0;

                // Data Points loop
                foreach (DataPoint point in series.Points)
                {
                    // Set Y values
                    if (point.IsEmpty)
                        // IsEmpty data point
                        output[indexSeries][indexPoint] = double.NaN;
                    else
                    {
                        try
                        {
                            output[indexSeries][indexPoint] = point.YValues[valueIndex[indexSeries - 1] - 1];
                        }
                        catch (System.Exception)
                        {
                            throw new ArgumentException(SR.ExceptionFormulaYIndexInvalid);
                        }
                    }

                    // Increase data point index.
                    indexPoint++;
                }
                // Increase data series index.
                indexSeries++;
            }
        }

        /// <summary>
        /// Merge, split or move Y values of the series. 
        /// </summary>
        /// <param name="inputSeries">Comma separated list of input data series names and optional X and Y values names.</param>
        /// <param name="outputSeries">Comma separated list of output data series names and optional X and Y values names.</param>
        public void CopySeriesValues(string inputSeries, string outputSeries)
        {
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (outputSeries == null)
                throw new ArgumentNullException("outputSeries");

            Series[] inSeries;
            Series[] outSeries;
            int[] inValueIndexes;
            int[] outValueIndexes;
            double[][] inValues;
            double[][] outValues;

            // Convert string with information about series and Y values 
            // to array of series and indexes to Y values.
            ConvertToArrays(inputSeries, out inSeries, out inValueIndexes, true);
            ConvertToArrays(outputSeries, out outSeries, out outValueIndexes, false);

            // The number of input and output series are different.
            if (inSeries.Length != outSeries.Length)
            {
                throw new ArgumentException(SR.ExceptionFormulaInputOutputSeriesMismatch);
            }

            // Check if output series points exist. If they do not exist 
            // create data points which are copy of Input series data points
            for (int indexSeries = 0; indexSeries < inSeries.Length; indexSeries++)
            {
                Series[] series = new Series[2];
                series[0] = inSeries[indexSeries];
                series[1] = outSeries[indexSeries];
                if (series[1].Points.Count == 0)
                {
                    foreach (DataPoint point in series[0].Points)
                    {
                        DataPoint clonePoint = point.Clone();
                        clonePoint.series = series[1];
                        series[1].Points.Add(clonePoint);
                    }
                }
            }

            // Check alignment of X values.
            for (int indexSeries = 0; indexSeries < inSeries.Length; indexSeries++)
            {
                Series[] series = new Series[2];
                series[0] = inSeries[indexSeries];
                series[1] = outSeries[indexSeries];
                CheckXValuesAlignment(series);
            }

            // Covert Series X and Y values to arrays of doubles
            GetDoubleArray(inSeries, inValueIndexes, out inValues, true);

            outValues = new double[inValues.Length][];

            // Copy Series X and Y values.
            for (int seriesIndex = 0; seriesIndex < inValues.Length; seriesIndex++)
            {
                outValues[seriesIndex] = new double[inValues[seriesIndex].Length];
                for (int pointIndex = 0; pointIndex < inValues[seriesIndex].Length; pointIndex++)
                {
                    outValues[seriesIndex][pointIndex] = inValues[seriesIndex][pointIndex];
                }
            }

            // Copy Series X and Y value Types.
            for (int seriesIndx = 0; seriesIndx < inSeries.Length; seriesIndx++)
            {
                // X value type
                if (outSeries[seriesIndx].XValueType == ChartValueType.Auto)
                {
                    outSeries[seriesIndx].XValueType = inSeries[seriesIndx].XValueType;
                    outSeries[seriesIndx].autoXValueType = inSeries[seriesIndx].autoXValueType;
                }

                // Y value type.
                if (outSeries[seriesIndx].YValueType == ChartValueType.Auto)
                {
                    outSeries[seriesIndx].YValueType = inSeries[seriesIndx].YValueType;
                    outSeries[seriesIndx].autoYValueType = inSeries[seriesIndx].autoYValueType;
                }

                seriesIndx++;
            }

            SetDoubleArray(outSeries, outValueIndexes, outValues, null);
        }


        /// <summary>
        /// This method will first copy input matrix to output matrix 
        /// then will remove columns, which have 
        /// one or more empty values (NaN) from the output matrix. This 
        /// method will set all values from column of input matrix 
        /// to be empty (NaN) if one or more values of that column 
        /// are empty.
        /// </summary>
        /// <param name="input">Input matrix with empty values</param>
        /// <param name="output">Output matrix without empty values</param>
        private void RemoveEmptyValues(double[][] input, out double[][] output)
        {
            // Allocate memory
            output = new double[input.Length][];
            int seriesIndex = 0;

            int numberOfRows = 0;

            // Set Nan for all data points with same index in input array
            // Data point loop
            for (int pointIndex = 0; pointIndex < input[0].Length; pointIndex++)
            {
                bool isEmpty = false;
                // Series loop
                // Find empty data point with same point index
                for (seriesIndex = 0; seriesIndex < input.Length; seriesIndex++)
                {
                    if (seriesIndex >= input[seriesIndex].Length)
                        continue;
                    if (Double.IsNaN(input[seriesIndex][pointIndex]))
                        isEmpty = true;
                }

                if (!isEmpty)
                {
                    numberOfRows++;
                }

                // There is empty data point
                if (isEmpty)
                {
                    // Set all points with same index to be empty
                    for (seriesIndex = 1; seriesIndex < input.Length; seriesIndex++)
                    {
                        input[seriesIndex][pointIndex] = Double.NaN;
                    }
                }
            }

            // Copy input matrix to output matrix without empty columns.
            for (seriesIndex = 0; seriesIndex < input.Length; seriesIndex++)
            {
                output[seriesIndex] = new double[numberOfRows];
                int outPointIndex = 0;
                for (int pointIndex = 0; pointIndex < input[0].Length; pointIndex++)
                {
                    if (pointIndex >= input[seriesIndex].Length)
                        continue;
                    if (!double.IsNaN(input[1][pointIndex]))
                    {
                        output[seriesIndex][outPointIndex] = input[seriesIndex][pointIndex];
                        outPointIndex++;
                    }
                }
            }
        }


        /*
		/// <summary>
		/// This method will compare a input matrix with empty data 
		/// points and output matrix without empty data points and 
		/// add empty data points to output matrix according to 
		/// input matrix empty data point positions.
		/// </summary>
		/// <param name="input">Matrix With input data</param>
		/// <param name="inputWithoutEmpty">Matrix without empty data points</param>
		/// <param name="output">New Matrix with inserted data points</param>
         */
        //private void InsertEmptyDataPoints( double [][] input, double [][] inputWithoutEmpty, out double [][] output )
        //{


        // *** NOTE ***
        //
        //
        // This method is only called in one location as of this writing.
        // Therefore the entire method is being commented out for now. We wish
        // to preserve the code itself as it may be re-implemented in the future.
        // --Microsoft 4/21/08
        //
        // ************



        //output = inputWithoutEmpty;
        //return;

        //
        // NOTE: Inserting empty points in the result data after applying the formula
        // causes issues. The algorithm below do not cover most of the common spzces
        // and as a result the formula data is completly destroyed.
        // 
        // By removing this code the result data set will have "missing" points instaed 
        // of empty. 
        //   - AG
        //

        /*

        // Input matrix can have only empty rows. If one value 
        // is empty all values from a row have to be empty.

        // Find the number of empty rows
        int NumberOfEmptyRows = 0;
        foreach( double val in input[1] )
        {
            if( Double.IsNaN( val ) )
            {
                NumberOfEmptyRows++;
            }
        }

        if( NumberOfEmptyRows == 0 ||
            inputWithoutEmpty[0].Length > input[0].Length)
        {
            output = inputWithoutEmpty;
            return;
        }

        output = new double[input.Length][];
        // Series loop
        for( int seriesIndex = 0; seriesIndex < input.Length; seriesIndex++ )
        {
            int inputPointIndex = 0;
            int emptyPointIndex = 0;

            // Skip input index if points are not aligned .
            while( input[0][inputPointIndex] != inputWithoutEmpty[0][0] && inputPointIndex < input[0].Length )
            {
                inputPointIndex++;
            }

            output[seriesIndex] = new double[inputWithoutEmpty[0].Length + NumberOfEmptyRows - inputPointIndex];

            // Data Point loop
            for( int pointIndex = 0; pointIndex < output[seriesIndex].Length; pointIndex++ )
            {
                if( inputPointIndex < input[0].Length &&
                    inputPointIndex < input[1].Length )
                {
                    // If the point Y value is empty (NaN) insert empty (NaN) for all values.
                    if( double.IsNaN( input[1][inputPointIndex] ) )
                    {
                        output[seriesIndex][pointIndex] = input[seriesIndex][inputPointIndex];
                        emptyPointIndex--;
                    }
                    else if( input[0][inputPointIndex] == inputWithoutEmpty[0][emptyPointIndex] )
                    {
                        output[seriesIndex][pointIndex] = inputWithoutEmpty[seriesIndex][emptyPointIndex];
                    }
                    else
                    {
                        output[0][pointIndex] = inputWithoutEmpty[0][emptyPointIndex];
                        output[seriesIndex][pointIndex] = inputWithoutEmpty[seriesIndex][emptyPointIndex];
                    }
                }
                else
                {
                    output[seriesIndex][pointIndex] = inputWithoutEmpty[seriesIndex][emptyPointIndex];
                }

                inputPointIndex++;
                emptyPointIndex++;
            }
        }
        */
        //}


        /// <summary>
        /// This method splits a string with comma separated 
        /// parameters to the array of strings with parameters.
        /// </summary>
        /// <param name="parameters">a string with comma separated parameters</param>
        /// <param name="parameterList">the array of strings with parameters</param>
        private void SplitParameters(string parameters, out string[] parameterList)
        {
            // Split string by comma
            parameterList = parameters.Split(',');

            for (Int32 i = 0; i < parameterList.Length; i++)
            {
                parameterList[i] = parameterList[i].Trim();
            }

        }

        /// <summary>
        /// Check if series have different number of series.
        /// </summary>
        /// <param name="input">Input series.</param>
        /// <returns>true if there is different number of series.</returns>
        private static bool DifferentNumberOfSeries(double[][] input)
        {
            for (int index = 0; index < input.Length - 1; index++)
            {
                if (input[index].Length != input[index + 1].Length)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method will check if X values from different series 
        /// are aligned.
        /// </summary>
        /// <param name="series">Array of series</param>
        internal void CheckXValuesAlignment(Series[] series)
        {
            // Check aligment only if more than 1 series provided
            if (series.Length > 1)
            {
                // Series loop
                for (int seriesIndex = 0; seriesIndex < series.Length - 1; seriesIndex++)
                {
                    // Check the number of data points
                    if (series[seriesIndex].Points.Count != series[seriesIndex + 1].Points.Count)
                    {
                        throw new ArgumentException(SR.ExceptionFormulaDataSeriesAreNotAlignedDifferentDataPoints(series[seriesIndex].Name, series[seriesIndex + 1].Name));
                    }

                    // Data points loop
                    for (int pointIndex = 0; pointIndex < series[seriesIndex].Points.Count; pointIndex++)
                    {
                        // Check X values.
                        if (series[seriesIndex].Points[pointIndex].XValue != series[seriesIndex + 1].Points[pointIndex].XValue)
                            throw new ArgumentException(SR.ExceptionFormulaDataSeriesAreNotAlignedDifferentXValues(series[seriesIndex].Name, series[seriesIndex + 1].Name));

                    }
                }
            }
        }


        #endregion

        #region Data Formulas Financial methods
        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="inputSeries">Input series</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void FinancialFormula(FinancialFormula formulaName, Series inputSeries)        
        {
            FinancialFormula(formulaName, inputSeries, inputSeries);
        }

        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="inputSeries">Input series</param>
        /// <param name="outputSeries">Output series</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void FinancialFormula(FinancialFormula formulaName, Series inputSeries, Series outputSeries)
        {
            FinancialFormula(formulaName, "", inputSeries, outputSeries);
        }


        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="parameters">Formula parameters</param>
        /// <param name="inputSeries">Input series</param>
        /// <param name="outputSeries">Output series</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void FinancialFormula(FinancialFormula formulaName, string parameters, Series inputSeries, Series outputSeries)
        {
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (outputSeries == null)
                throw new ArgumentNullException("outputSeries");
            FinancialFormula(formulaName, parameters, inputSeries.Name, outputSeries.Name);
        }


        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="inputSeries">Comma separated list of input series names and optional X and Y values names.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void FinancialFormula(FinancialFormula formulaName, string inputSeries)
        {
            FinancialFormula(formulaName, inputSeries, inputSeries);
        }


        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="inputSeries">Comma separated list of input series names and optional X and Y values names.</param>
        /// <param name="outputSeries">Comma separated list of output series names and optional X and Y values names.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void FinancialFormula(FinancialFormula formulaName, string inputSeries, string outputSeries)
        {
            FinancialFormula(formulaName, "", inputSeries, outputSeries);
        }

        /// <summary>
        /// This method calls a method from a formula module with 
        /// specified name.
        /// </summary>
        /// <param name="formulaName">Formula Name</param>
        /// <param name="parameters">Formula parameters</param>
        /// <param name="inputSeries">Comma separated list of input series names and optional X and Y values names.</param>
        /// <param name="outputSeries">Comma separated list of output series names and optional X and Y values names.</param>
        public void FinancialFormula(FinancialFormula formulaName, string parameters, string inputSeries, string outputSeries)
        {
            if (inputSeries == null)
                throw new ArgumentNullException("inputSeries");
            if (outputSeries == null)
                throw new ArgumentNullException("outputSeries");

            // Get formula info
            FormulaInfo formulaInfo = FormulaHelper.GetFormulaInfo(formulaName);

            // Provide default parameters if necessary
            if (string.IsNullOrEmpty(parameters))
            {
                parameters = formulaInfo.SaveParametersToString();
            }
            else 
            {
                formulaInfo.CheckParameterString(parameters);
            }

            // Fix the InputSeries and Outputseries for cases when the series field names are not provided
            SeriesFieldList inputFields = SeriesFieldList.FromString(this.Common.Chart, inputSeries, formulaInfo.InputFields);
            SeriesFieldList outputFields = SeriesFieldList.FromString(this.Common.Chart, outputSeries, formulaInfo.OutputFields);

            if (inputFields != null) inputSeries = inputFields.ToString();
            if (outputFields != null) outputSeries = outputFields.ToString();

            Formula(formulaName.ToString(), parameters, inputSeries, outputSeries);
        }
        #endregion

        #region Data Formulas properties

        /// <summary>
        /// Gets or sets a flag which indicates whether 
        /// empty points are ignored while performing calculations; 
        /// otherwise, empty points are treated as zeros. 
        /// </summary>
        public bool IsEmptyPointIgnored
        {
            get
            {
                return _isEmptyPointIgnored;
            }
            set
            {
                _isEmptyPointIgnored = value;
            }
        }



        /// <summary>
        /// Gets or sets a flag which indicates whether 
        /// to start formulas like rolling average from zero.
        /// </summary>
        public bool IsStartFromFirst
        {
            get
            {
                return bool.Parse(_extraParameters[0]);
            }
            set
            {
                if (value)
                    _extraParameters[0] = true.ToString(System.Globalization.CultureInfo.InvariantCulture);
                else
                    _extraParameters[0] = false.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Returns a reference to the statistical utility class.
        /// </summary>
        public StatisticFormula Statistics
        {
            get
            {
                return _statistics;
            }
        }


        #endregion
    }



}
