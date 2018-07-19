//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StatisticFormula.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	StatisticFormula, TTestResult, FTestResult, AnovaResult,
//              ZTestResult
//
//  Purpose:	StatisticFormula class provides helper methods for statistical
//              calculations like TTest, FTest, Anova, ZTest and others.
//              Actual calculations are made in the DataFormula class and 
//              the StatisticFormula class mange formula parameters, input and 
//              output series.
//
//              TTestResult, FTestResult, AnovaResult and ZTestResult
//              classes are used to store the results of the calculatiions.
//          
//              StatisticFormula class is exposed to the user through 
//              DataManipulator.StatisticFormula property. Here is an example of
//              using the Anova test:
//
//              AnovaResult result = Chart1.DataManipulator.StatisticFormula.Anova(0.6, "Group1,Group2,Group3");
//
//  NOTE:       First versions of the chart use single method to execute 
//              ALL formulas. Formula name and parameters were passed as
//              strings. Input and outpat data was passed through data
//              series.
//
//              This approach was hard to use by the end-user and was 
//              changed to a specific method for each formula. StatisticFormula
//              class provides that simplified interface for all statistics 
//              formulas. Internally it still uses the DataFormula.Formula
//              method with string parameters.
//
//	Reviewed:	AG - April 1, 2003
//              AG - Microsoft 14, 2007
//
//===================================================================

using System;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    /// <summary>
    /// The StatisticFormula class provides helper methods for statistical calculations.
    /// Actual calculations are made in the DataFormula class and the StatisticFormula
    /// class provide a simplified API which automatically prepares parameters and 
    /// deals with input and output series. 
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class StatisticFormula
	{
		#region Fields

        // Name used for temporary data series
		private string _tempOutputSeriesName = "Statistical Analyses Formula Temporary Output Series 2552003";

        // Reference to the class which describes calculation settings and 
        // provides access to chart common elements.
		private DataFormula _formulaData = null;

		#endregion // Fields
		
		#region Constructor

		/// <summary>
        /// StatisticFormula Constructor
		/// </summary>
		/// <param name="formulaData">Formula Data</param>
		internal StatisticFormula( DataFormula formulaData )
		{
			this._formulaData = formulaData;
		}

		#endregion // Constructor

		#region Tests
		
		/// <summary>
        /// This formula performs a Z Test using Normal distribution.
		/// </summary>
		/// <param name="hypothesizedMeanDifference">Hypothesized mean difference.</param>
		/// <param name="varianceFirstGroup">Variance first group.</param>
		/// <param name="varianceSecondGroup">Variance second group.</param>
		/// <param name="probability">Probability.</param>
		/// <param name="firstInputSeriesName">First input series name.</param>
		/// <param name="secondInputSeriesName">Second input series name.</param>
        /// <returns>ZTestResult object.</returns>
		public ZTestResult ZTest( 
			double hypothesizedMeanDifference, 
			double varianceFirstGroup, 
			double varianceSecondGroup, 
			double probability, 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");

			// Create output class
			ZTestResult zTestResult = new ZTestResult();

			// Make string with parameters
			string parameter = hypothesizedMeanDifference.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + varianceFirstGroup.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + varianceSecondGroup.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
            // Execute formula
            try
            {
                _formulaData.Formula("ZTest", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                zTestResult.firstSeriesMean = points[0].YValues[0];
                zTestResult.secondSeriesMean = points[1].YValues[0];
                zTestResult.firstSeriesVariance = points[2].YValues[0];
                zTestResult.secondSeriesVariance = points[3].YValues[0];
                zTestResult.zValue = points[4].YValues[0];
                zTestResult.probabilityZOneTail = points[5].YValues[0];
                zTestResult.zCriticalValueOneTail = points[6].YValues[0];
                zTestResult.probabilityZTwoTail = points[7].YValues[0];
                zTestResult.zCriticalValueTwoTail = points[8].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return zTestResult;
			
		}

		/// <summary>
        /// Perform a T Test using Students distribution (T distribution) with unequal variances.
		/// </summary>
		/// <param name="hypothesizedMeanDifference">Hypothesized mean difference.</param>
		/// <param name="probability">Probability.</param>
		/// <param name="firstInputSeriesName">First input series name.</param>
		/// <param name="secondInputSeriesName">Second input series name.</param>
        /// <returns>TTestResult object.</returns>
		public TTestResult TTestUnequalVariances( 
			double hypothesizedMeanDifference, 
			double probability, 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");

			// Create output class
			TTestResult tTestResult = new TTestResult();

			// Make string with parameters
			string parameter = hypothesizedMeanDifference.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
            try
            {
                string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);

                // Execute formula
                _formulaData.Formula("TTestUnequalVariances", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                tTestResult.firstSeriesMean = points[0].YValues[0];
                tTestResult.secondSeriesMean = points[1].YValues[0];
                tTestResult.firstSeriesVariance = points[2].YValues[0];
                tTestResult.secondSeriesVariance = points[3].YValues[0];
                tTestResult.tValue = points[4].YValues[0];
                tTestResult.degreeOfFreedom = points[5].YValues[0];
                tTestResult.probabilityTOneTail = points[6].YValues[0];
                tTestResult.tCriticalValueOneTail = points[7].YValues[0];
                tTestResult.probabilityTTwoTail = points[8].YValues[0];
                tTestResult.tCriticalValueTwoTail = points[9].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return tTestResult;
			
		}

		/// <summary>
        /// Perform a T Test using Students distribution (T distribution) with equal variances.
		/// </summary>
		/// <param name="hypothesizedMeanDifference">Hypothesized mean difference.</param>
		/// <param name="probability">Probability.</param>
		/// <param name="firstInputSeriesName">First input series name.</param>
		/// <param name="secondInputSeriesName">Second input series name.</param>
        /// <returns>TTestResult object.</returns>
		public TTestResult TTestEqualVariances( 
			double hypothesizedMeanDifference, 
			double probability, 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");

			// Create output class
			TTestResult tTestResult = new TTestResult();

			// Make string with parameters
			string parameter = hypothesizedMeanDifference.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Execute formula
            try
            {
                _formulaData.Formula("TTestEqualVariances", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                tTestResult.firstSeriesMean = points[0].YValues[0];
                tTestResult.secondSeriesMean = points[1].YValues[0];
                tTestResult.firstSeriesVariance = points[2].YValues[0];
                tTestResult.secondSeriesVariance = points[3].YValues[0];
                tTestResult.tValue = points[4].YValues[0];
                tTestResult.degreeOfFreedom = points[5].YValues[0];
                tTestResult.probabilityTOneTail = points[6].YValues[0];
                tTestResult.tCriticalValueOneTail = points[7].YValues[0];
                tTestResult.probabilityTTwoTail = points[8].YValues[0];
                tTestResult.tCriticalValueTwoTail = points[9].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return tTestResult;			
		}

		/// <summary>
        /// Performs a T Test using Students distribution (T distribution) with paired samples. 
        /// This is useful when there is a natural pairing of observations in samples.
		/// </summary>
        /// <param name="hypothesizedMeanDifference">Hypothesized mean difference.</param>
        /// <param name="probability">Probability.</param>
        /// <param name="firstInputSeriesName">First input series name.</param>
        /// <param name="secondInputSeriesName">Second input series name.</param>
        /// <returns>TTestResult object.</returns>
		public TTestResult TTestPaired( 
			double hypothesizedMeanDifference, 
			double probability, 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");

			// Create output class
			TTestResult tTestResult = new TTestResult();

			// Make string with parameters
			string parameter = hypothesizedMeanDifference.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Execute formula
            try
            {
                _formulaData.Formula("TTestPaired", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                tTestResult.firstSeriesMean = points[0].YValues[0];
                tTestResult.secondSeriesMean = points[1].YValues[0];
                tTestResult.firstSeriesVariance = points[2].YValues[0];
                tTestResult.secondSeriesVariance = points[3].YValues[0];
                tTestResult.tValue = points[4].YValues[0];
                tTestResult.degreeOfFreedom = points[5].YValues[0];
                tTestResult.probabilityTOneTail = points[6].YValues[0];
                tTestResult.tCriticalValueOneTail = points[7].YValues[0];
                tTestResult.probabilityTTwoTail = points[8].YValues[0];
                tTestResult.tCriticalValueTwoTail = points[9].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return tTestResult;
			
		}

        /// <summary>
        /// Removes empty points from series.
        /// </summary>
        /// <param name="seriesName">series name</param>
        private void RemoveEmptyPoints(string seriesName)
        {
            Series series = _formulaData.Common.DataManager.Series[seriesName];
            for (int pointIndex = 0; pointIndex < series.Points.Count; pointIndex++)
            {
                if (series.Points[pointIndex].IsEmpty)
                {
                    series.Points.RemoveAt(pointIndex--);
                }
            }
        }

        /// <summary>
        /// This formula performs a two-sample F Test using the F distribution, and is used to see if the samples have different variances.
        /// </summary>
        /// <param name="probability">Probability.</param>
        /// <param name="firstInputSeriesName">First input series name.</param>
        /// <param name="secondInputSeriesName">Second input series name.</param>
        /// <returns>FTestResult object.</returns>
        public FTestResult FTest( 
			double probability, 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");

			// Create output class
			FTestResult fTestResult = new FTestResult();

			// Make string with parameters
			string parameter = probability.ToString(System.Globalization.CultureInfo.InvariantCulture);

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

            
            // remove empty points from the collection.
            RemoveEmptyPoints(firstInputSeriesName);
            RemoveEmptyPoints(secondInputSeriesName);

			// Execute formula
            try
            {
                _formulaData.Formula("FTest", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                fTestResult.firstSeriesMean = points[0].YValues[0];
                fTestResult.secondSeriesMean = points[1].YValues[0];
                fTestResult.firstSeriesVariance = points[2].YValues[0];
                fTestResult.secondSeriesVariance = points[3].YValues[0];
                fTestResult.fValue = points[4].YValues[0];
                fTestResult.probabilityFOneTail = points[5].YValues[0];
                fTestResult.fCriticalValueOneTail = points[6].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return fTestResult;
			
		}


		/// <summary>
        /// An Anova test is used to determine the existence, or absence of a statistically 
        /// significant difference between the mean values of two or more groups of data.
		/// </summary>
		/// <param name="probability">Probability.</param>
        /// <param name="inputSeriesNames">Comma-delimited list of input series names.</param>
        /// <returns>AnovaResult object.</returns>
		public AnovaResult Anova( 
			double probability, 
			string inputSeriesNames)
		{
            // Check arguments
            if (inputSeriesNames == null)
                throw new ArgumentNullException("inputSeriesNames");

			// Create output class
			AnovaResult anovaResult = new AnovaResult();

			// Make string with parameters
			string parameter = probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Execute formula
            try
            {
                _formulaData.Formula("Anova", parameter, inputSeriesNames, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                anovaResult.sumOfSquaresBetweenGroups = points[0].YValues[0];
                anovaResult.sumOfSquaresWithinGroups = points[1].YValues[0];
                anovaResult.sumOfSquaresTotal = points[2].YValues[0];
                anovaResult.degreeOfFreedomBetweenGroups = points[3].YValues[0];
                anovaResult.degreeOfFreedomWithinGroups = points[4].YValues[0];
                anovaResult.degreeOfFreedomTotal = points[5].YValues[0];
                anovaResult.meanSquareVarianceBetweenGroups = points[6].YValues[0];
                anovaResult.meanSquareVarianceWithinGroups = points[7].YValues[0];
                anovaResult.fRatio = points[8].YValues[0];
                anovaResult.fCriticalValue = points[9].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return anovaResult;			
		}

		#endregion // Test

		#region Distributions

		/// <summary>
        /// This method returns the probability for the standard normal cumulative distribution function.
		/// </summary>
        /// <param name="zValue">The Z value for which the probability is required.</param>
		/// <returns>Returns value from the standard normal cumulative distribution function.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Z is a cartesian coordinate and well understood")]
        public double NormalDistribution(double zValue)
		{
			// Make string with parameters
			string parameter = zValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("NormalDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return result;
			
		}

		/// <summary>
        /// This method returns the inverse of the standard normal cumulative distribution.
		/// </summary>
		/// <param name="probability">Probability.</param>
		/// <returns>Returns value from the inverse standard normal cumulative distribution function.</returns>
		public double InverseNormalDistribution( double probability )
		{
			
			// Make string with parameters
			string parameter = probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("InverseNormalDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return result;			
		}

		/// <summary>
        /// This method returns the cumulative F distribution function probability.
		/// </summary>
		/// <param name="value">F Value.</param>
		/// <param name="firstDegreeOfFreedom">First degree of freedom.</param>
		/// <param name="secondDegreeOfFreedom">Second degree of freedom.</param>
		/// <returns>Returns value from the cumulative F distribution function.</returns>
		public double FDistribution( 
			double value,
			int firstDegreeOfFreedom,
			int secondDegreeOfFreedom )
		{
			
			// Make string with parameters
			string parameter = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + firstDegreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + secondDegreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("FDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return result;			
		}

		/// <summary>
        /// Returns the inverse of the F cumulative distribution.
		/// </summary>
		/// <param name="probability">Probability.</param>
		/// <param name="firstDegreeOfFreedom">First degree of freedom.</param>
		/// <param name="secondDegreeOfFreedom">Second degree of freedom.</param>
		/// <returns>Returns value from the inverse F distribution function.</returns>
		public double InverseFDistribution( 
			double probability,
			int firstDegreeOfFreedom,
			int secondDegreeOfFreedom )
		{
			
			// Make string with parameters
			string parameter = probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + firstDegreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + secondDegreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("InverseFDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return result;			
		}
		
		/// <summary>
        /// Returns the probability for the T distribution (student's distribution).
		/// </summary>
		/// <param name="value">T value</param>
		/// <param name="degreeOfFreedom">Degree of freedom</param>
        /// <param name="oneTail">If true, one-tailed distribution is used; otherwise two-tailed distribution is used.</param>
		/// <returns>Returns T Distribution cumulative function</returns>
		public double TDistribution( 
			double value,
			int degreeOfFreedom,
			bool oneTail )
		{
			
			// Make string with parameters
			string parameter = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + degreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
			if( oneTail )
			{
				parameter += ",1";
			}
			else
			{
				parameter += ",2";
			}
			
						
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

            // Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("TDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

        	// Return result class
			return result;
			
		}

		/// <summary>
        /// Returns the T-value of the T distribution as a function of probability and degrees of freedom.
		/// </summary>
		/// <param name="probability">Probability.</param>
		/// <param name="degreeOfFreedom">Degree of freedom.</param>
		/// <returns>Returns Inverse T distribution.</returns>
		public double InverseTDistribution( 
			double probability,
			int degreeOfFreedom )
		{
			
			// Make string with parameters
			string parameter = probability.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + degreeOfFreedom.ToString(System.Globalization.CultureInfo.InvariantCulture);
						
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

            // Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("InverseTDistribution", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output class
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result class
			return result;
			
		}

		#endregion // Distributions

		#region Correlation and Covariance

		/// <summary>
        /// This method gets the covariance value for two series of data.
		/// </summary>
		/// <param name="firstInputSeriesName">First input series name.</param>
		/// <param name="secondInputSeriesName">Second input series name.</param>
		/// <returns>Covariance.</returns>
		public double Covariance( 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");			

			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + secondInputSeriesName.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Execute formula
            double result = double.NaN;
            try
            {                
                _formulaData.Formula("Covariance", "", inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }
            
			// Return result
			return result;
			
		}

		/// <summary>
        /// This method gets the correlation value for two series of data.
		/// </summary>
		/// <param name="firstInputSeriesName">First input series name.</param>
		/// <param name="secondInputSeriesName">Second input series name.</param>
		/// <returns>Returns Correlation</returns>
		public double Correlation( 
			string firstInputSeriesName, 
			string secondInputSeriesName )
		{
            // Check arguments
            if (firstInputSeriesName == null)
                throw new ArgumentNullException("firstInputSeriesName");
            if (secondInputSeriesName == null)
                throw new ArgumentNullException("secondInputSeriesName");			

			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = firstInputSeriesName + "," + secondInputSeriesName;
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("Correlation", "", inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;
			
		}

		/// <summary>
        /// This method returns the average of all data points stored in the specified series.
		/// </summary>
		/// <param name="inputSeriesName">Input series name.</param>
		/// <returns>The average of all data points.</returns>
		public double Mean( 
			string inputSeriesName )
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = inputSeriesName;
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("Mean", "", inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;
			
		}

		/// <summary>
        /// This method returns the median of all data points in the specified series.
		/// </summary>
		/// <param name="inputSeriesName">Input series name.</param>
		/// <returns>Median.</returns>
		public double Median( 
			string inputSeriesName )
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");			

			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = inputSeriesName;
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("Median", "", inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;
			
		}

        /// <summary>
        /// This method returns the variance for a series.
        /// </summary>
        /// <param name="inputSeriesName">Input series name.</param>
        /// <param name="sampleVariance">If true, the data is a sample of the population.  If false, it is the entire population.</param>
        /// <returns>Variance.</returns>
		public double Variance( 
			string inputSeriesName,
			bool sampleVariance )
		{
            // Check arguments
            if (inputSeriesName == null)
                throw new ArgumentNullException("inputSeriesName");
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );

			// Set input series string
			string inputSeriesParameter = inputSeriesName;

			// Formula parameter
			string parameter = sampleVariance.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("Variance", parameter, inputSeriesParameter, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;
			
		}

		/// <summary>
        /// This method returns the beta function for two given values.
		/// </summary>
		/// <param name="m">First parameter for beta function</param>
		/// <param name="n">Second Parameter for beta function</param>
		/// <returns>Returns beta function for the two given values.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
             Justification = "The Beta Function is a mathematical function where arbitrary letters to indicate inputs are common")]  
        public double BetaFunction( 
			double m,
			double n )
		{
            // Fix for the VSTS 230829: The BetaFunction for the m=0,n=0 is double.NaN
            if (m == 0 && n == 0)
                return double.NaN;

			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Formula parameter
			string parameter = m.ToString(System.Globalization.CultureInfo.InvariantCulture);
			parameter += "," + n.ToString(System.Globalization.CultureInfo.InvariantCulture);
			
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("BetaFunction", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;			
		}

        /// <summary>
        /// This method returns the gamma function value for the given variable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns gamma function</returns>
		public double GammaFunction( 
			double value )
		{
			
			// Create temporary output series.
			_formulaData.Common.DataManager.Series.Add( new Series(_tempOutputSeriesName) );
			
			// Formula parameter
			string parameter = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
						
			// Execute formula
            double result = double.NaN;
            try
            {
                _formulaData.Formula("GammaFunction", parameter, _tempOutputSeriesName, _tempOutputSeriesName);

                DataPointCollection points = _formulaData.Common.DataManager.Series[_tempOutputSeriesName].Points;

                // Fill Output value
                result = points[0].YValues[0];
            }
            finally
            {
                // Remove Temporary output series
                _formulaData.Common.DataManager.Series.Remove(_formulaData.Common.DataManager.Series[_tempOutputSeriesName]);
            }

			// Return result
			return result;
			
		}
		
		#endregion
	}

    #region Output classes used to store statistical calculations results

    /// <summary>
    /// The TTestResult class stores the results of the TTest statistical calculations.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class TTestResult
    {
        #region Fields

        /// <summary>
        /// First series' mean.
        /// </summary>
        internal double firstSeriesMean = 0.0;

        /// <summary>
        /// Second series' mean.
        /// </summary>
        internal double secondSeriesMean = 0.0;

        /// <summary>
        /// First series' variance.
        /// </summary>
        internal double firstSeriesVariance = 0.0;

        /// <summary>
        /// Second series' variance.
        /// </summary>
        internal double secondSeriesVariance = 0.0;

        /// <summary>
        /// T value.
        /// </summary>
        internal double tValue = 0.0;

        /// <summary>
        /// Degree of freedom.
        /// </summary>
        internal double degreeOfFreedom = 0.0;

        /// <summary>
        /// Probability T one tail.
        /// </summary>
        internal double probabilityTOneTail = 0.0;

        /// <summary>
        /// Critical T one tail.
        /// </summary>
        internal double tCriticalValueOneTail = 0.0;

        /// <summary>
        /// Probability T two tails.
        /// </summary>
        internal double probabilityTTwoTail = 0.0;

        /// <summary>
        /// Critical T two tails.
        /// </summary>
        internal double tCriticalValueTwoTail = 0.0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the mean of the first series.
        /// </summary>
        public double FirstSeriesMean
        {
            get
            {
                return firstSeriesMean;
            }
        }

        /// <summary>
        /// Gets the mean of the second series.
        /// </summary>
        public double SecondSeriesMean
        {
            get
            {
                return secondSeriesMean;
            }
        }

        /// <summary>
        /// Gets the variance of the first series.
        /// </summary>
        public double FirstSeriesVariance
        {
            get
            {
                return firstSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the variance of the second series.
        /// </summary>
        public double SecondSeriesVariance
        {
            get
            {
                return secondSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the T value.
        /// </summary>
        public double TValue
        {
            get
            {
                return tValue;
            }
        }

        /// <summary>
        /// Gets the degree of freedom.
        /// </summary>
        public double DegreeOfFreedom
        {
            get
            {
                return degreeOfFreedom;
            }
        }

        /// <summary>
        /// Gets the probability T one tail value.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "T One Tail is a statistics term. 'Tone' is not the intended word here.")]   
        public double ProbabilityTOneTail
        {
            get
            {
                return probabilityTOneTail;
            }
        }

        /// <summary>
        /// Gets the critical T one tail value.
        /// </summary>
        public double TCriticalValueOneTail
        {
            get
            {
                return tCriticalValueOneTail;
            }
        }

        /// <summary>
        /// Gets the probability T two tails value.
        /// </summary>
        public double ProbabilityTTwoTail
        {
            get
            {
                return probabilityTTwoTail;
            }
        }

        /// <summary>
        /// Gets the critical T two tails value.
        /// </summary>
        public double TCriticalValueTwoTail
        {
            get
            {
                return tCriticalValueTwoTail;
            }
        }

        #endregion
    }

    /// <summary>
    /// The FTestResult class stores the results of the FTest statistical calculations.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class FTestResult
    {
        #region Fields

        /// <summary>
        /// First series' mean.
        /// </summary>
        internal double firstSeriesMean = 0.0;

        /// <summary>
        /// Second series' mean.
        /// </summary>
        internal double secondSeriesMean = 0.0;

        /// <summary>
        /// First series' variance.
        /// </summary>
        internal double firstSeriesVariance = 0.0;

        /// <summary>
        /// Second series' variance.
        /// </summary>
        internal double secondSeriesVariance = 0.0;

        /// <summary>
        /// F value.
        /// </summary>
        internal double fValue = 0.0;

        /// <summary>
        /// Probability F one tail.
        /// </summary>
        internal double probabilityFOneTail = 0.0;

        /// <summary>
        /// Critical F one tail.
        /// </summary>
        internal double fCriticalValueOneTail = 0.0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the mean of the first series.
        /// </summary>
        public double FirstSeriesMean
        {
            get
            {
                return firstSeriesMean;
            }
        }

        /// <summary>
        /// Gets the mean of the second series.
        /// </summary>
        public double SecondSeriesMean
        {
            get
            {
                return secondSeriesMean;
            }
        }

        /// <summary>
        /// Gets the variance of the first series.
        /// </summary>
        public double FirstSeriesVariance
        {
            get
            {
                return firstSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the variance of the second series.
        /// </summary>
        public double SecondSeriesVariance
        {
            get
            {
                return secondSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the F value.
        /// </summary>
        public double FValue
        {
            get
            {
                return fValue;
            }
        }

        /// <summary>
        /// Gets the probability F one tail.
        /// </summary>
        public double ProbabilityFOneTail
        {
            get
            {
                return probabilityFOneTail;
            }
        }

        /// <summary>
        /// Gets the critical F one tail.
        /// </summary>
        public double FCriticalValueOneTail
        {
            get
            {
                return fCriticalValueOneTail;
            }
        }

        #endregion
    }

    /// <summary>
    /// The AnovaResult class stores the results of the Anova statistical calculations.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AnovaResult
    {
        #region Fields

        /// <summary>
        /// Sum of squares between groups.
        /// </summary>
        internal double sumOfSquaresBetweenGroups = 0.0;

        /// <summary>
        /// Sum of squares within groups.
        /// </summary>
        internal double sumOfSquaresWithinGroups = 0.0;

        /// <summary>
        /// Total sum of squares.
        /// </summary>
        internal double sumOfSquaresTotal = 0.0;

        /// <summary>
        /// Degree of freedom between groups.
        /// </summary>
        internal double degreeOfFreedomBetweenGroups = 0.0;

        /// <summary>
        /// Degree of freedom within groups.
        /// </summary>
        internal double degreeOfFreedomWithinGroups = 0.0;

        /// <summary>
        /// Total degree of freedom.
        /// </summary>
        internal double degreeOfFreedomTotal = 0.0;

        /// <summary>
        /// Mean square variance between groups.
        /// </summary>
        internal double meanSquareVarianceBetweenGroups = 0.0;

        /// <summary>
        /// Mean square variance between groups.
        /// </summary>
        internal double meanSquareVarianceWithinGroups = 0.0;

        /// <summary>
        /// F ratio.
        /// </summary>
        internal double fRatio = 0.0;

        /// <summary>
        /// F critical value.
        /// </summary>
        internal double fCriticalValue = 0.0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the sum of squares between groups.
        /// </summary>
        public double SumOfSquaresBetweenGroups
        {
            get
            {
                return sumOfSquaresBetweenGroups;
            }
        }

        /// <summary>
        /// Gets the sum of squares within groups.
        /// </summary>
        public double SumOfSquaresWithinGroups
        {
            get
            {
                return sumOfSquaresWithinGroups;
            }
        }


        /// <summary>
        /// Gets the total sum of squares.
        /// </summary>
        public double SumOfSquaresTotal
        {
            get
            {
                return sumOfSquaresTotal;
            }
        }

        /// <summary>
        /// Gets the degree of freedom between groups.
        /// </summary>
        public double DegreeOfFreedomBetweenGroups
        {
            get
            {
                return degreeOfFreedomBetweenGroups;
            }
        }

        /// <summary>
        /// Gets the degree of freedom within groups.
        /// </summary>
        public double DegreeOfFreedomWithinGroups
        {
            get
            {
                return degreeOfFreedomWithinGroups;
            }
        }

        /// <summary>
        /// Gets the total degree of freedom.
        /// </summary>
        public double DegreeOfFreedomTotal
        {
            get
            {
                return degreeOfFreedomTotal;
            }
        }

        /// <summary>
        /// Gets the mean square variance between groups.
        /// </summary>
        public double MeanSquareVarianceBetweenGroups
        {
            get
            {
                return meanSquareVarianceBetweenGroups;
            }
        }

        /// <summary>
        /// Gets the mean square variance within groups.
        /// </summary>
        public double MeanSquareVarianceWithinGroups
        {
            get
            {
                return meanSquareVarianceWithinGroups;
            }
        }

        /// <summary>
        /// Gets the F ratio.
        /// </summary>
        public double FRatio
        {
            get
            {
                return fRatio;
            }
        }

        /// <summary>
        /// Gets the F critical value.
        /// </summary>
        public double FCriticalValue
        {
            get
            {
                return fCriticalValue;
            }
        }

        #endregion
    }

    /// <summary>
    /// The ZTestResult class stores the results of the ZTest statistical calculations.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ZTestResult
    {
        #region Constructor

        /// <summary>
        /// ZTestResult Constructor
        /// </summary>
        public ZTestResult()
        {
        }

        #endregion // Constructor

        #region Fields

        // Internal fields used for public properties
        internal double firstSeriesMean;
        internal double secondSeriesMean;
        internal double firstSeriesVariance;
        internal double secondSeriesVariance;
        internal double zValue;
        internal double probabilityZOneTail;
        internal double zCriticalValueOneTail;
        internal double probabilityZTwoTail;
        internal double zCriticalValueTwoTail;


        #endregion // Fields

        #region Properties

        /// <summary>
        /// Gets the mean of the first series.
        /// </summary>
        public double FirstSeriesMean
        {
            get
            {
                return firstSeriesMean;
            }
        }

        /// <summary>
        /// Gets the mean of the second series.
        /// </summary>
        public double SecondSeriesMean
        {
            get
            {
                return secondSeriesMean;
            }
        }

        /// <summary>
        /// Gets the variance of the first series.
        /// </summary>
        public double FirstSeriesVariance
        {
            get
            {
                return firstSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the variance of the second series.
        /// </summary>
        public double SecondSeriesVariance
        {
            get
            {
                return secondSeriesVariance;
            }
        }

        /// <summary>
        /// Gets the Z Value
        /// </summary>
        public double ZValue
        {
            get
            {
                return zValue;
            }
        }

        /// <summary>
        /// Gets the probability Z one tail value.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Z One Tail is a statistics term. 'Zone' is not the intended word here.")]  
        public double ProbabilityZOneTail
        {
            get
            {
                return probabilityZOneTail;
            }
        }

        /// <summary>
        /// Gets the Z critical value one tail value.
        /// </summary>
        public double ZCriticalValueOneTail
        {
            get
            {
                return zCriticalValueOneTail;
            }
        }

        /// <summary>
        /// Gets the probability Z two tail value.
        /// </summary>
        public double ProbabilityZTwoTail
        {
            get
            {
                return probabilityZTwoTail;
            }
        }

        /// <summary>
        /// Gets the Z critical value two tail value.
        /// </summary>
        public double ZCriticalValueTwoTail
        {
            get
            {
                return zCriticalValueTwoTail;
            }
        }

        #endregion // Properties
    }

    #endregion // Output Classes
}


