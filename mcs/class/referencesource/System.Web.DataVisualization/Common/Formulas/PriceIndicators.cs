//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		PriceIndicators.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	PriceIndicators
//
//  Purpose:	This class is used to calculate Price 
//				indicators used in Technical Analyses.
//
//	Reviewed:	GS - August 7, 2002
//				AG - August 7, 2002
//
//===================================================================


using System;


#if Microsoft_CONTROL
    namespace System.Windows.Forms.DataVisualization.Charting.Formulas
#else
	namespace System.Web.UI.DataVisualization.Charting.Formulas
#endif
{
	/// <summary>
	/// Price indicator is module with mathematical calculations 
	/// that apply to a security's price.
	/// </summary>
	internal class PriceIndicators : IFormula
	{
		#region Error strings

		// Error strings
		//internal string inputArrayStart = "Formula requires";
		//internal string inputArrayEnd = "arrays";
		//internal string SR.ExceptionPriceIndicatorsSameYNumber = "Formula requires the same number of Y values for each input data point";
		//internal string SR.ExceptionPriceIndicatorsFormulaRequiresFourArrays = "Formula requires the same number of X and Y values for each input data point";
		//internal string periodMissing = "Formula error - Period parameter is missing. ";
		//internal string SR.ExceptionPriceIndicatorsFormulaRequiresFourArrays = "Formula error - There are not enough data points for the Period. ";

		#endregion

		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
        virtual public string Name { get { return SR.FormulaNamePriceIndicators; } }

		#endregion

		#region Formulas

		/// <summary>
		/// A Moving Average is an indicator that shows the average 
		/// value of a security's price over a period of time. When 
		/// calculating a moving average, a mathematical analysis of 
		/// the security's average value over a predetermined time 
		/// period is made. As the security's price changes, 
		/// its average price moves up or down.
		/// A simple, or arithmetic, moving average is calculated by 
		/// adding the closing price of the security for a number of 
		/// time periods (e.g., 12 days) and then dividing this total 
		/// by the number of time periods. The result is the average 
		/// price of the security over the time period. Simple moving 
		/// averages give equal weight to each daily price.
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Array of doubles: Y values</param>
		/// <param name="outputValues">Arrays of doubles: Moving average</param>
		/// <param name="period">Period</param>
		/// <param name="FromFirst">Start from first value</param>
		internal void MovingAverage(double [] inputValues, out double [] outputValues, int period, bool FromFirst )
		{
			double [][] tempInput = new double [2][];
			double [][] tempOutput = new double [2][];
			string [] parList = new string [1];
			string [] extList = new string [1];

			parList[0] = period.ToString(System.Globalization.CultureInfo.InvariantCulture);
			extList[0] = FromFirst.ToString(System.Globalization.CultureInfo.InvariantCulture);

			tempInput[0] = new double[inputValues.Length];
			tempInput[1] = inputValues;
			
			MovingAverage( tempInput, out tempOutput, parList, extList );
		
			outputValues = tempOutput[1];
		}
	
		/// <summary>
		/// A Moving Average is an indicator that shows the average 
		/// value of a security's price over a period of time. When 
		/// calculating a moving average, a mathematical analysis of 
		/// the security's average value over a predetermined time 
		/// period is made. As the security's price changes, 
		/// its average price moves up or down.
		/// A simple, or arithmetic, moving average is calculated by 
		/// adding the closing price of the security for a number of 
		/// time periods (e.g., 12 days) and then dividing this total 
		/// by the number of time periods. The result is the average 
		/// price of the security over the time period. Simple moving 
		/// averages give equal weight to each daily price.
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		/// <param name="parameterList">Array of strings: 1. Period</param>
		/// <param name="extraParameterList">Array of strings: 1. Start from zero</param>
		private void MovingAverage(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0]);

			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			// Not enough values for moving average.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];

			if( startFromFirst )
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length];

				// Y values
				outputValues[1] = new double [inputValues[1].Length];
			
				for( int point = 0; point < inputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point];

					// Find sum of Y values
					double sum = 0;
					int startSum = 0;

					// Find the begining of the period
					if( point - period + 1 > 0 )
					{
						startSum = point - period + 1;
					}

					// Find sum fro real period.
					for( int pointSum = startSum; pointSum <= point; pointSum++ )
					{
						sum += inputValues[1][pointSum];
					}

					// Find real period if start from first data point.
					int realPeriod = period;
					if( period > point + 1 )
					{
						realPeriod = point + 1;
					}

					outputValues[1][point] = sum / realPeriod;
				}
			}
			else
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length - period + 1];

				// Y values
				outputValues[1] = new double [inputValues[1].Length - period + 1];
			
				// Find sum of Y values for the period
				double sum = 0;
				for( int pointSum = 0; pointSum < period; pointSum++ )
				{
					sum += inputValues[1][pointSum];
				}

				for( int point = 0; point < outputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point + period - 1];

					outputValues[1][point] = sum / period;

					// Change Sum
					if( point < outputValues[0].Length - 1 )
					{
						sum -= inputValues[1][point];
						sum += inputValues[1][point + period];
					}
				}
			}
		}

		/// <summary>
		/// An exponential (or exponentially weighted) moving average 
		/// is calculated by applying a percentage of today’s closing 
		/// price to yesterday’s moving average value. Exponential 
		/// moving averages place more weight on recent prices.	For 
		/// example, to calculate a 9% exponential moving average 
		/// of IBM, you would first take today’s closing price and 
		/// multiply it by 9%. Next, you would add this product to 
		/// the value of yesterday’s moving average multiplied by 
		/// 91% (100% - 9% = 91%).
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Exponential Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Array of doubles: Y values</param>
		/// <param name="outputValues">Arrays of doubles: Exponential Moving average</param>
		/// <param name="period">Period</param>
		/// <param name="startFromFirst">Start from first value</param>
		internal void ExponentialMovingAverage(double []inputValues, out double []outputValues, int period, bool startFromFirst)
		{
			double [][] tempInput = new double [2][];
			double [][] tempOutput = new double [2][];
			string [] parList = new string [1];
			string [] extList = new string [1];

			parList[0] = period.ToString(System.Globalization.CultureInfo.InvariantCulture);
			extList[0] = startFromFirst.ToString(System.Globalization.CultureInfo.InvariantCulture);

			tempInput[0] = new double[inputValues.Length];
			tempInput[1] = inputValues;
			
			ExponentialMovingAverage( tempInput, out tempOutput, parList, extList );
		
			outputValues = tempOutput[1];
		}

		/// <summary>
		/// An exponential (or exponentially weighted) moving average 
		/// is calculated by applying a percentage of today’s closing 
		/// price to yesterday’s moving average value. Exponential 
		/// moving averages place more weight on recent prices.	For 
		/// example, to calculate a 9% exponential moving average 
		/// of IBM, you would first take today’s closing price and 
		/// multiply it by 9%. Next, you would add this product to 
		/// the value of yesterday’s moving average multiplied by 
		/// 91% (100% - 9% = 91%).
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Exponential Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		/// <param name="parameterList">Array of strings: 1. Period</param>
		/// <param name="extraParameterList">Array of strings: 1. Start from zero</param>
		private void ExponentialMovingAverage(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Formula for converting period to percentage
			double exponentialPercentage = 2.0 / ( period + 1.0 );

			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0] );

			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			// Not enough values for moving average.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];

			if( startFromFirst )
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length];

				// Y values
				outputValues[1] = new double [inputValues[1].Length];
				
				for( int point = 0; point < inputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point];

					// Find sum of Y values
					double sum = 0;
					int startSum = 0;

					if( point - period + 1 > 0 )
					{
						startSum = point - period + 1;
					}

					for( int pointSum = startSum; pointSum < point; pointSum++ )
					{
						sum += inputValues[1][pointSum];
					}

					int realPeriod = period;
					if( period > point + 1 )
						realPeriod = point + 1;

					double movingAvr;

					// Find real period if start from first data point.
					if( realPeriod <= 1 )
						movingAvr = 0;
					else
						movingAvr = sum / ( realPeriod - 1 );

					// Formula for converting period to percentage
					exponentialPercentage = 2.0 / ( realPeriod + 1.0 );

					// Exponential influence
					outputValues[1][point] = movingAvr * (1 - exponentialPercentage ) + inputValues[1][point] * exponentialPercentage;

				}
			}
			else
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length - period + 1];

				// Y values
				outputValues[1] = new double [inputValues[1].Length - period + 1];
				
				for( int point = 0; point < outputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point + period - 1];

					double movingAvr;
					// if point is less than period calulate simple moving average
					if(  point == 0  )
					{
						// Find sum of Y values
						double sum = 0;
						for( int pointSum = point; pointSum < point + period; pointSum++ )
						{
							sum += inputValues[1][pointSum];
						}

						movingAvr = sum / ( period );
					}
						// else use previos day exponential moving average
					else
						movingAvr = outputValues[1][point-1];

					// Exponential influence
					outputValues[1][point] = movingAvr * (1 - exponentialPercentage ) + inputValues[1][point + period - 1] * exponentialPercentage;

				}
			}
		}

		/// <summary>
		/// Triangular moving averages place the majority of the weight 
		/// on the middle portion of the price series. They are actually 
		/// double-smoothed simple moving averages. The periods used 
		/// in the simple moving averages varies depending on if you 
		/// specify an odd or even number of time periods. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		/// <param name="parameterList">Array of strings: 1. Period</param>
		/// <param name="extraParameterList">Array of strings: 1. Start from zero</param>
		private void TriangularMovingAverage(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0] );
			
			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			// Not enough values for moving average.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];
		
			// Find triangular period
			double tempPeriod = ((double)period + 1.0) / 2.0;
			tempPeriod = Math.Round(tempPeriod);
			double [] tempOut;
			double [] tempIn = inputValues[1];

			// Call moving averages first time
			MovingAverage( tempIn, out tempOut, (int)tempPeriod, startFromFirst );
			// Call moving averages second time (Moving average of moving average)
			MovingAverage( tempOut, out tempOut, (int)tempPeriod, startFromFirst );

			outputValues[1] = tempOut;

			// X values
			outputValues[0] = new double [outputValues[1].Length];
			
			// Set X values
			if( startFromFirst )
				outputValues[0] = inputValues[0];
			else
			{
				for( int index = 0; index < outputValues[1].Length; index++ )
					outputValues[0][index] = inputValues[0][((int)(tempPeriod)-1) * 2 + index];
			}
		}

		/// <summary>
		/// A weighted moving average is designed to put more weight on 
		/// recent data and less weight on past data. A weighted moving 
		/// average is calculated by multiplying each of the previous 
		/// day’s data by a weight. The following table shows the calculation 
		/// of a 5-day weighted moving average.
		/// ---------------------------------------------------------
		/// Input: 
		///		- Y values.
		/// Output: 
		///		- Moving Average.
		/// Parameters: 
		///		- Period
		///	Extra Parameters: 
		///		- Start from First 
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		/// <param name="parameterList">Array of strings: 1. Period</param>
		/// <param name="extraParameterList">Array of strings: 1. Start from zero</param>
		private void WeightedMovingAverage(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0] );

			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			// Not enough values for moving average.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];

			if( startFromFirst )
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length];

				// Y values
				outputValues[1] = new double [inputValues[1].Length];
				
				for( int point = 0; point < inputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point];

					// Find sum of Y values
					double sum = 0;
					int startSum = 0;

					if( point - period + 1 > 0 )
					{
						startSum = point - period + 1;
					}

					int index = 1;
					int indexSum = 0;
					for( int pointSum = startSum; pointSum <= point; pointSum++ )
					{
						sum += inputValues[1][pointSum] * index;
						indexSum += index;
						index++;
					}

					double movingAvr;

					// Avoid division by zero.
					if( point == 0 )
						movingAvr = inputValues[1][0];
					else
						movingAvr = sum / indexSum;

					// Weighted average
					outputValues[1][point] = movingAvr;

				}
			}
			else
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length - period + 1];

				// Y values
				outputValues[1] = new double [inputValues[1].Length - period + 1];
				
				for( int point = 0; point < outputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point + period - 1];

					// Find sum of Y values
					double sum = 0;
					
					int index = 1;
					int indexSum = 0;
					for( int pointSum = point; pointSum < point + period; pointSum++ )
					{
						sum += inputValues[1][pointSum] * index;
						indexSum += index;
						index++;
					}

					double movingAvr = sum / indexSum;

					// Weighted average
					outputValues[1][point] = movingAvr;

				}
			}
		}

		/// <summary>
		/// Bollinger Bands plot trading bands above and below 
		/// a simple moving average. The standard deviation of 
		/// closing prices for a period equal to the moving 
		/// average employed is used to determine the band width. 
		/// This causes the bands to tighten in quiet markets and 
		/// loosen in volatile markets. The bands can be used to 
		/// determine overbought and oversold levels, locate 
		/// reversal areas, project targets for market moves, and 
		/// determine appropriate stop levels. The bands are used 
		/// in conjunction with indicators such as RSI, MovingAverageConvergenceDivergence 
		/// histogram, CCI and Rate of Change. Divergences between 
		/// Bollinger bands and other indicators show potential 
		/// action points. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value.
		/// Output: 
		///		- 2 Y values (Bollinger Band Hi and Low).
		/// Parameters: 
		///		- period
		///	Extra Parameters: 
		///		- startFromFirst
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Bollinger Band Up, 3. row - Bollinger Band Down</param>
		/// <param name="parameterList">Array of strings: 1. Period</param>
		/// <param name="extraParameterList">Array of strings: 1. Start from zero</param>
		private void BollingerBands(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Standard deviation
			double deviation;
			try
			{deviation = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );}
			catch(System.Exception)
            { throw new InvalidOperationException(SR.ExceptionIndicatorsDeviationMissing); }
			
			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0] );

			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			// Not enough values for moving average.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [3][];

			if( startFromFirst )
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length];

				// Y values
				outputValues[1] = new double [inputValues[1].Length];
				outputValues[2] = new double [inputValues[1].Length];

				// average
				double [] average = new double [inputValues[1].Length];

				MovingAverage( inputValues[1], out average, period, true );
		
				for( int point = 0; point < outputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point];

					// Find sum of Y values
					double sum = 0;
					int startSum = 0;

					// Find the begining of the period
					if( point - period + 1 > 0 )
					{
						startSum = point - period + 1;
					}
								
					for( int pointSum = startSum; pointSum <= point; pointSum++ )
					{
						sum += ((inputValues[1][pointSum] - average[point])*(inputValues[1][pointSum] - average[point]));
					}

					outputValues[1][point] = average[point] + Math.Sqrt(sum / period) * deviation;
					outputValues[2][point] = average[point] - Math.Sqrt(sum / period) * deviation;
				}
			}
			else
			{
				// X values
				outputValues[0] = new double [inputValues[0].Length - period + 1];

				// Y values
				outputValues[1] = new double [inputValues[1].Length - period + 1];
				outputValues[2] = new double [inputValues[1].Length - period + 1];

				// average
				double [] average = new double [inputValues[1].Length - period + 1];

				MovingAverage( inputValues[1], out average, period, false );
			
				for( int point = 0; point < outputValues[0].Length; point++ )
				{
					// Set X value
					outputValues[0][point] = inputValues[0][point + period - 1];

					// Find sum of Y values
					double sum = 0;
									
					for( int pointSum = point; pointSum < point + period; pointSum++ )
					{
						sum += ((inputValues[1][pointSum] - average[point])*(inputValues[1][pointSum] - average[point]));
					}

					outputValues[1][point] = average[point] + Math.Sqrt(sum / period) * deviation;
					outputValues[2][point] = average[point] - Math.Sqrt(sum / period) * deviation;
				}
			}
		}

		/// <summary>
		/// The Typical Price indicator is simply an average of each 
		/// day's price. The Median Price and Weighted Close are 
		/// similar indicators. The Typical Price indicator provides 
		/// a simple, single-line plot of the day's average price. 
		/// Some investors use the Typical Price rather than the 
		/// closing price when creating moving average ----ion 
		/// systems.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( Close, High, Low ).
		/// Output: 
		///		- 1 Y value Weighted Close Indicator.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values (Close), 3. row - Y values (High), 4. row - Y values (Low)</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Weighted Close</param>
		private void TypicalPrice(double [][] inputValues, out double [][] outputValues)
		{
			int length = inputValues.Length;
						
			// There is no enough series
			if( length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);

			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[1].Length];

			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set median price
				outputValues[1][index] = (inputValues[1][index] + inputValues[2][index] + inputValues[3][index])/3.0;
			}

		}

		/// <summary>
		/// The Median Price indicator is simply the midpoint of each 
		/// day's price. The Typical Price and Weighted Close are 
		/// similar indicators. The Median Price indicator provides 
		/// a simple, single-line chart of the day's "average price." 
		/// This average price is useful when you want a simpler 
		/// scaleView of prices.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( High, Low ).
		/// Output: 
		///		- 1 Y value Median Price Indicator.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values (High), 3. row - Y values (Low)</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Median Price</param>
		private void MedianPrice(double [][] inputValues, out double [][] outputValues)
		{
			int length = inputValues.Length;
						
			// There is no enough series
			if( length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);

			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[1].Length];

			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set median price
				outputValues[1][index] = (inputValues[1][index] + inputValues[2][index])/2.0;
			}

		}

		/// <summary>
		/// The Weighted Close indicator is simply an average of each day's 
		/// price. It gets its name from the fact that extra weight is 
		/// given to the closing price. The Median Price and Typical Price 
		/// are similar indicators. When plotting and back-testing moving 
		/// averages, indicators, trendlines, etc, some investors like 
		/// the simplicity that a line chart offers. However, line 
		/// charts that only show the closing price can be misleading 
		/// since they ignore the high and low price. A Weighted Close 
		/// chart combines the simplicity of the line chart with the 
		/// scope of a bar chart, by plotting a single point for each 
		/// day that includes the high, low, and closing price.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( Close, High, Low ).
		/// Output: 
		///		- 1 Y value Weighted Close Indicator.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values (Close), 3. row - Y values (High), 4. row - Y values (Low)</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Weighted Close</param>
		private void WeightedClose(double [][] inputValues, out double [][] outputValues)
		{
			int length = inputValues.Length;
						
			// There is no enough series
			if( length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);

			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[1].Length];

			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set median price
				outputValues[1][index] = (inputValues[1][index] + inputValues[2][index] + inputValues[3][index] * 2)/4.0;
			}

		}

		/// <summary>
		/// An envelope is comprised of two moving averages. One moving 
		/// average is shifted upward and the second moving average 
		/// is shifted downward. Envelopes define the upper and lower 
		/// boundaries of a security's normal trading range. A sell 
		/// signal is generated when the security reaches the upper 
		/// band whereas a buy signal is generated at the lower band. 
		/// The optimum percentage shift depends on the volatility of 
		/// the security--the more volatile, the larger the percentage.
		/// The logic behind envelopes is that overzealous buyers and 
		/// sellers push the price to the extremes (i.e., the upper 
		/// and lower bands), at which point the prices often stabilize 
		/// by moving to more realistic levels. This is similar to the 
		/// interpretation of Bollinger Bands.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value.
		/// Output: 
		///		- 2 Y values (Envelope Hi and Low).
		/// Parameters: 
		///		- period
		///		- shift in percentages
		///	Extra Parameters: 
		///		- startFromFirst
		/// 
		/// </summary>
		/// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
		/// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Envelopes Up, 3. row - Envelopes Down</param>
		/// <param name="parameterList">Array of strings: parameters</param>
		/// <param name="extraParameterList">Array of strings: Extra parameters </param>
		private void Envelopes(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for moving average
			int period;
			try
			{period = int.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch( Exception e )
			{
                if (e.Message == SR.ExceptionObjectReferenceIsNull)
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing);
				else
                    throw new InvalidOperationException(SR.ExceptionPriceIndicatorsPeriodMissing + e.Message);
			}

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Shift
			double shift;
			try
			{shift = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );}
			catch(System.Exception)
            { throw new InvalidOperationException(SR.ExceptionPriceIndicatorsShiftParameterMissing); }
						
			// There is no enough series
			if( length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);

			double [][] movingAverage;

			MovingAverage( inputValues, out movingAverage, parameterList, extraParameterList );

			outputValues = new double[3][];
			outputValues[0] = new double[movingAverage[0].Length];
			outputValues[1] = new double[movingAverage[0].Length];
			outputValues[2] = new double[movingAverage[0].Length];

			for( int index = 0; index < movingAverage[0].Length; index++ )
			{
				outputValues[0][index] = movingAverage[0][index];
				outputValues[1][index] = movingAverage[1][index] + shift*movingAverage[1][index]/100.0;
				outputValues[2][index] = movingAverage[1][index] - shift*movingAverage[1][index]/100.0;

			}
		}
		
		/// <summary>
		/// Standard Deviation is a statistical measure of volatility. 
		/// Standard Deviation is typically used as a component of 
		/// other indicators, rather than as a stand-alone indicator. 
		/// For example, Bollinger Bands are calculated by adding 
		/// a security's Standard Deviation to a moving average. 
		/// High Standard Deviation values occur when the data item 
		/// being analyzed (e.g., prices or an indicator) is changing 
		/// dramatically. Similarly, low Standard Deviation values 
		/// occur when prices are stable.
		/// </summary>
		/// <param name="inputValues">Input Y values</param>
		/// <param name="outputValues">Output standard deviation</param>
		/// <param name="period">Period</param>
		/// <param name="startFromFirst">Start calculation from the first Y value</param>
		internal void StandardDeviation(double [] inputValues, out double [] outputValues, int period, bool startFromFirst )
		{
			double [] movingOut;

			// Start calculation from the first Y value
			if( startFromFirst )
			{
				outputValues = new double[inputValues.Length];
				double sum;
				MovingAverage( inputValues, out movingOut, period, startFromFirst );
				int outIndex = 0;
				for( int index = 0; index < inputValues.Length; index++ )
				{
					sum = 0;
					int startSum = 0;

					// Find the begining of the period
					if( index - period + 1 > 0 )
					{
						startSum = index - period + 1;
					}
					for( int indexDev = startSum; indexDev <= index; indexDev++ )
					{
						sum += (inputValues[indexDev] - movingOut[outIndex])*(inputValues[indexDev] - movingOut[outIndex]);
					}
					outputValues[outIndex] = Math.Sqrt( sum / period );
					outIndex++;
				}
			}
				// Do not start calculation from the first Y value
			else
			{
				outputValues = new double[inputValues.Length - period + 1];
				double sum;
				MovingAverage( inputValues, out movingOut, period, startFromFirst );
				int outIndex = 0;
				for( int index = period - 1; index < inputValues.Length; index++ )
				{
					sum = 0;
					for( int indexDev = index - period + 1; indexDev <= index; indexDev++ )
					{
						sum += (inputValues[indexDev] - movingOut[outIndex])*(inputValues[indexDev] - movingOut[outIndex]);
					}
					outputValues[outIndex] = Math.Sqrt( sum / period );
					outIndex++;
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public PriceIndicators()
		{
		}
	
		/// <summary>
		/// This methods checks the number of X and Y values and 
		/// fire exception if the numbers are different.
		/// </summary>
		/// <param name="inputValues">Input X and Y values</param>
		/// <param name="numOfYValues">The number of Y values</param>
		public void CheckNumOfValues( double [][] inputValues, int numOfYValues )
		{
			// Different number of x and y values
			if( inputValues[0].Length != inputValues[1].Length )
			{
                throw new ArgumentException(SR.ExceptionPriceIndicatorsSameXYNumber);
			}

			// Different number of y values
			for( int index = 1; index < numOfYValues; index++ )
			{
				if( inputValues[index].Length != inputValues[index+1].Length )
				{
					throw new ArgumentException( SR.ExceptionPriceIndicatorsSameYNumber );
				}
			}
		}

        /// <summary>
        /// The first method in the module, which converts a formula 
        /// name to the corresponding private method.
        /// </summary>
        /// <param name="formulaName">String which represent a formula name</param>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Formula parameters</param>
        /// <param name="extraParameterList">Array of strings - Extra Formula parameters from DataManipulator object</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		virtual public void Formula( string formulaName, double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList, out string [][] outLabels )
		{
			string name;

			name = formulaName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			// Not used for these formulas.
			outLabels = null;

			try
			{
				switch( name )
				{
					case "MOVINGAVERAGE":
						MovingAverage( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "EXPONENTIALMOVINGAVERAGE":
						ExponentialMovingAverage( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "TRIANGULARMOVINGAVERAGE":
						TriangularMovingAverage( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "WEIGHTEDMOVINGAVERAGE":
						WeightedMovingAverage( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "BOLLINGERBANDS":
						BollingerBands( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "MEDIANPRICE":
						MedianPrice( inputValues, out outputValues );
						break;
					case "TYPICALPRICE":
						TypicalPrice( inputValues, out outputValues );
						break;
					case "WEIGHTEDCLOSE":
						WeightedClose( inputValues, out outputValues );
						break;
					case "ENVELOPES":
						Envelopes( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					default:
						outputValues = null; 
						break;
				}
			}
			catch( IndexOutOfRangeException )
			{
                throw new InvalidOperationException(SR.ExceptionFormulaInvalidPeriod( name ) );
			}
			catch( OverflowException )
			{
				throw new InvalidOperationException( SR.ExceptionFormulaNotEnoughDataPoints( name ) );
			}
		}

		#endregion
	}
}
