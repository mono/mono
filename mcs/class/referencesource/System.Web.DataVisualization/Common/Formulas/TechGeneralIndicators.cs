//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		TechGeneralIndicators.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	TechGeneralIndicators
//
//  Purpose:	This class is used for calculations of 
//				general technical analyses indicators.
//
//	Reviewed:	GS - August 7, 2002
//				AG - August 7, 2002
//
//===================================================================


using System;
using System.Globalization;



#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Formulas
#else
	namespace System.Web.UI.DataVisualization.Charting.Formulas
#endif
{
	/// <summary>
	/// This class is used for calculations of general 
	/// technical analyses indicators.
	/// </summary>
	internal class GeneralTechnicalIndicators : PriceIndicators
	{
		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
        override public string Name { get { return SR.FormulaNameGeneralTechnicalIndicators; } }

		#endregion

		#region Formulas

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
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value.
		/// Output: 
		///		- 1 Y value Standard Deviation
		/// Parameters: 
		///		- Periods for standard deviation ( used for moving average )
		///	Extra Parameters: 
		///		-
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		/// <param name="extraParameterList">Array of strings - Extra parameters</param>
		private void StandardDeviation(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			int length = inputValues.Length;

			// Period for standard deviation ( used for moving average )
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

			// Not enough values for moving average in Standard deviation.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];

			StandardDeviation( inputValues[1], out outputValues[1], period, startFromFirst );

			// Set X values
			outputValues[0] = new double [outputValues[1].Length];
			for( int index = 0; index < outputValues[1].Length; index++ )
			{
				if( startFromFirst )
					outputValues[0][index] = inputValues[0][index];
				else
					outputValues[0][index] = inputValues[0][index+period-1];
			}
		}

		/// <summary>
		/// The Average True Range ("ATR") is a measure of volatility. It was introduced 
		/// by Welles Wilder in his book, New Concepts in Technical Trading Systems, and 
		/// has since been used as a component of many indicators and trading systems. Wilder 
		/// has found that high ATR values often occur at market bottoms following a "panic" 
		/// sell-off. Low Average True Range values are often found during extended sideways 
		/// periods, such as those found at tops and after consolidation periods. The Average 
		/// True Range can be interpreted using the same techniques that are used with 
		/// the other volatility indicators.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( High, Low, Close ).
		/// Output: 
		///		- 1 Y value AverageTrueRange
		/// Parameters: 
		///		- Periods (Default 14) = is used to configure the number of periods to calculate the ATR
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void AverageTrueRange(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
						
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 14;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// The distance from today's high to today's low
			double distanceOne;

			// The distance from yesterday's close to today's high
			double distanceTwo;

			// The distance from yesterday's close to today's low
			double distanceTree;

			double [] trueRange = new double [inputValues[0].Length - 1];

			// True Range
			for( int index = 1; index < inputValues[0].Length; index++ )
			{
				// The distance from today's high to today's low
				distanceOne = Math.Abs( inputValues[1][index] - inputValues[2][index] );

				// The distance from yesterday's close to today's high
				distanceTwo = Math.Abs( inputValues[3][index-1] - inputValues[1][index] );

				// The distance from yesterday's close to today's low
				distanceTree = Math.Abs( inputValues[3][index-1] - inputValues[2][index] );

				// True Range
				trueRange[index-1] = Math.Max( Math.Max( distanceOne, distanceTwo ), distanceTree );
			}

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length-period];

			// Moving average of true range
			MovingAverage( trueRange, out outputValues[1], period, false );

			// Set X values
			for( int index = period; index < inputValues[0].Length; index++ )
			{
				outputValues[0][index-period] = inputValues[0][index];
			}
		}

		/// <summary>
		/// The Ease of Movement indicator shows the relationship between volume and price 
		/// change. This indicator shows how much volume is required to move prices. The Ease 
		/// of Movement indicator was developed Richard W. Arms, Jr., the creator of Equivolume.
		/// High Ease of Movement values occur when prices are moving upward on lightStyle volume. 
		/// Low Ease of Movement values occur when prices are moving downward on lightStyle volume. 
		/// If prices are not moving, or if heavy volume is required to move prices, then 
		/// indicator will also be near zero.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( High, Low, Volume ).
		/// Output: 
		///		- 1 Y value Ease Of Movement
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		private void EaseOfMovement(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough input series
			if( inputValues.Length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
									
			double MidPointMove;
			double BoxRattio;

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - 1];
			outputValues[1] = new double [inputValues[0].Length - 1];

			// Ease Of Movement
			for( int index = 1; index < inputValues[0].Length; index++ )
			{
				// Set X values
				outputValues[0][index - 1] = inputValues[0][index];

				// Calculate the Mid-point Move for each day:                          
				MidPointMove = ( inputValues[1][index] + inputValues[2][index] ) / 2 - ( inputValues[1][index - 1] + inputValues[2][index - 1] ) / 2;

				// The Box Ratio determines the ratio between height and width of the Equivolume box:    
				BoxRattio = ( inputValues[3][index] ) / (( inputValues[1][index] - inputValues[2][index] ) );

				// Ease of Movement is then calculated as:
				outputValues[1][index - 1] = MidPointMove / BoxRattio;
			}
		}

		/// <summary>
		/// The Mass Index was designed to identify trend reversals by measuring the narrowing 
		/// and widening of the range between the high and low prices. As this range widens, the 
		/// Mass Index increases; as the range narrows the Mass Index decreases.
		/// The Mass Index was developed by Donald Dorsey. According to Mr. Dorsey, the most 
		/// significant pattern to watch for is a "reversal bulge." A reversal bulge occurs when 
		/// a 25-period Mass Index rises above 27.0 and subsequently falls below 26.5. A reversal 
		/// in price is then likely. The overall price trend (i.e., trending or trading range) 
		/// is unimportant.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( High, Low ).
		/// Output: 
		///		- 1 Y value Mass Index
		/// Parameters: 
		///		- Period = is used to calculate the accumulation, By default this property is set to 25.
		///		- AveragePeriod = is used to calculate Simple Moving Avg, By default this property is set to 9.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void MassIndex(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 25;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Average Period
			int averagePeriod;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out averagePeriod))
            {
                averagePeriod = 9;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodAverageParameterIsNegative);
						
			double [] highLow = new double [inputValues[0].Length];
			double [] average;
			double [] secondAverage;

			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				highLow[index] = inputValues[1][index] - inputValues[2][index];
			}

			// Find exponential moving average
			ExponentialMovingAverage( highLow, out average, averagePeriod, false );

			// Find exponential moving average of exponential moving average
			ExponentialMovingAverage( average, out secondAverage, averagePeriod, false );

			outputValues = new double [2][];

			outputValues[0] = new double [secondAverage.Length - period + 1];
			outputValues[1] = new double [secondAverage.Length - period + 1];

			// Mass Index
			int outIndex = 0;
			double sum = 0;
			for( int index = 2 * averagePeriod - 3 + period; index < inputValues[0].Length; index++ )
			{
				// Set X values
				outputValues[0][outIndex] = inputValues[0][index];

				sum = 0;
				for( int indexSum = index - period + 1; indexSum <= index; indexSum++ )
				{
					sum += average[indexSum - averagePeriod + 1] / secondAverage[indexSum - 2 * averagePeriod + 2];
				}

				// Set Y values
				outputValues[1][outIndex] = sum;

				outIndex++;
			}
		}

		/// <summary>
		/// The Performance indicator displays a security's price performance as 
		/// a percentage. This is sometimes called a "normalized" chart. The 
		/// Performance indicator displays the percentage that the security 
		/// has increased since the first period displayed. For example, if 
		/// the Performance indicator is 10, it means that the security's 
		/// price has increased 10% since the first period displayed on the 
		/// left side of the chart. Similarly, a value of -10% means that 
		/// the security's price has fallen by 10% since the first period 
		/// displayed.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value ( Close ).
		/// Output: 
		///		- 1 Y value Performance
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		private void Performance(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];
						
			// Performance indicator
			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set Y values
				outputValues[1][index] = ( inputValues[1][index] - inputValues[1][0] ) / inputValues[1][0] * 100;
			}
		}

		/// <summary>
		/// Rate of Change is used to monitor momentum by making direct comparisons between current 
		/// and past prices on a continual basis. The results can be used to determine the strength 
		/// of price trends. Note: This study is the same as the Momentum except that Momentum uses 
		/// subtraction in its calculations while Rate of Change uses division. The resulting lines 
		/// of these two studies operated over the same data will look exactly the same - only the 
		/// scale values will differ. The Price Rate-of-Change ("----") indicator displays the 
		/// difference between the current price and the price x-time periods ago. The difference 
		/// can be displayed in either points or as a percentage. The Momentum indicator displays 
		/// the same information, but expresses it as a ratio. When the Rate-of-Change displays 
		/// the price change in points, it subtracts the price x-time periods ago from today’s price.
		/// When the Rate-of-Change displays the price change as a percentage, it divides 
		/// the price change by price x-time period’s ago.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value ( Close ).
		/// Output: 
		///		- 1 Y value Rate of Change
		/// Parameters: 
		///		- Periods = is used to configure the number of periods to calculate the rate of Change. By default the Periods property is set to 10.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void RateOfChange(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
			
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 10;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - period];
			outputValues[1] = new double [inputValues[0].Length - period];
						
			// Rate Of Change
			for( int index = period; index < inputValues[0].Length; index++ )
			{
				// Set X values
				outputValues[0][index - period] = inputValues[0][index];

				// Set Y values
				outputValues[1][index - period] = ( inputValues[1][index] - inputValues[1][index - period] ) / inputValues[1][index - period] * 100;
			}
		}

		/// <summary>
		/// This indicator was developed by Welles Wilder Jr. Relative Strength is often 
		/// used to identify price tops and bottoms by keying on specific levels 
		/// (usually "30" and "70") on the RSI chart which is scaled from from 0-100. 
		/// The study is also useful to detect the following: 
		///		- Movement which might not be as readily apparent on the bar chart
		///		- Failure swings above 70 or below 30 which can warn of coming reversals
		///		- Support and resistance levels 
		///		- Divergence between the RSI and price which is often a useful reversal indicator 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value ( Close ).
		/// Output: 
		///		- 1 Y value RelativeStrengthIndex 
		/// Parameters: 
		///		- Periods = is used to configure the number of periods to calculate the RSI indicator. By default the Periods property is set to 10.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void RelativeStrengthIndex(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
			
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 10;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);
			
			double [] upward = new double[inputValues[0].Length-1];
			double [] downward = new double[inputValues[0].Length-1];

			for( int index = 1; index < inputValues[0].Length; index++ )
			{
				// Upward - price is going up
				if( inputValues[1][index - 1] < inputValues[1][index] )
				{
					upward[index-1] = inputValues[1][index] - inputValues[1][index - 1];
					downward[index-1] = 0.0;
				}
				// Downward - price is going down
				if( inputValues[1][index - 1] > inputValues[1][index] )
				{
					upward[index-1] = 0.0;
					downward[index-1] = inputValues[1][index - 1] - inputValues[1][index];
				}
			}

			double [] averageUpward = new double[inputValues[0].Length];
			double [] averageDownward = new double[inputValues[0].Length];

			ExponentialMovingAverage(downward, out averageDownward, period, false );
			ExponentialMovingAverage(upward, out averageUpward, period, false );

			outputValues = new double [2][];

			outputValues[0] = new double [averageDownward.Length];
			outputValues[1] = new double [averageDownward.Length];

			// Find RSI
			for( int index = 0; index < averageDownward.Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index + period];

				// Calculate the Relative Strength Index (RSI):
				outputValues[1][index] = 100 - 100 / ( 1 + averageUpward[index] / averageDownward[index] ); 
			}
		}

		/// <summary>
		/// TripleExponentialMovingAverage is a momentum indicator that displays the percent rate-of-change of a triple 
		/// exponentially smoothed moving average of the security's closing price. It is designed 
		/// to keep you in trends equal to or shorter than the number of periods you specify. 
		/// The TripleExponentialMovingAverage indicator oscillates around a zero line. Its triple exponential smoothing is 
		/// designed to filter out "insignificant" cycles (i.e., those that are shorter than 
		/// the number of periods you specify).	Trades should be placed when the indicator changes 
		/// direction (i.e., buy when it turns up and sell when it turns down). You may want to 
		/// plot a 9-period moving average of the TripleExponentialMovingAverage to create a "signal" line (similar to the 
		/// MovingAverageConvergenceDivergence indicator, and then buy when the TripleExponentialMovingAverage rises above its signal, and sell when it 
		/// falls below its signal. Divergences between the security and the TripleExponentialMovingAverage can also help 
		/// identify turning points.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y values ( Close ).
		/// Output: 
		///		- 1 Y value ( TripleExponentialMovingAverage ).
		/// Parameters: 
		///		- Period = is used to calculate the Exponential Moving Avg, By default this property is set to 12.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void Trix(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
						
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 12;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);
			
			double [] exp1; // Exponential Moving average of input values
			double [] exp2; // Exponential Moving average of exp1
			double [] exp3; // Exponential Moving average of exp2

			// Find exponential moving average
			ExponentialMovingAverage( inputValues[1], out exp1, period, false );

			// Find exponential moving average
			ExponentialMovingAverage( exp1, out exp2, period, false );

			// Find exponential moving average
			ExponentialMovingAverage( exp2, out exp3, period, false );

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - period * 3 + 2];
			outputValues[1] = new double [inputValues[0].Length - period * 3 + 2];

			// Calculate TripleExponentialMovingAverage
			int outIndex = 0;
			for( int index = period * 3 - 2; index < inputValues[0].Length; index++ )
			{
				// set X value
				outputValues[0][outIndex] = inputValues[0][index];

				// set Y value
				outputValues[1][outIndex] = ( exp3[outIndex+1] - exp3[outIndex] ) / exp3[outIndex];

				outIndex++;
			}
		}

		/// <summary>
		/// The MovingAverageConvergenceDivergence is used to determine overbought or oversold conditions in the market. Written 
		/// for stocks and stock indices, MovingAverageConvergenceDivergence can be used for commodities as well. The MovingAverageConvergenceDivergence line 
		/// is the difference between the long and short exponential moving averages of the chosen 
		/// item. The signal line is an exponential moving average of the MovingAverageConvergenceDivergence line. Signals are 
		/// generated by the relationship of the two lines. As with RSI and Stochastics, 
		/// divergences between the MovingAverageConvergenceDivergence and prices may indicate an upcoming trend reversal. The MovingAverageConvergenceDivergence  
		/// is a trend following momentum indicator that shows the relationship between two 
		/// moving averages of prices. The MovingAverageConvergenceDivergence is the difference between a 26-day and 12-day 
		/// exponential moving average. A 9-day exponential moving average, called the "signal" 
		/// (or "trigger") line is plotted on top of the MovingAverageConvergenceDivergence to show buy/sell opportunities. The 
		/// MovingAverageConvergenceDivergence is calculated by subtracting the value of a 26-day exponential moving average 
		/// from a 12-day exponential moving average. A 9-day dotted exponential moving average of 
		/// the MovingAverageConvergenceDivergence (the "signal" line) is then plotted on top of the MovingAverageConvergenceDivergence.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value ( Close ).
		/// Output: 
		///		- 1 Y value ( MovingAverageConvergenceDivergence ).
		/// Parameters: 
		///		- ShortPeriod = is used to configure the short Exponential Moving Average, By default this property is set to 12.
		///		- LongPeriod = is used to configure the Int64 Exponential Moving Average, By default this property is set to 26.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void Macd(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
									
			// Short Period
			int shortPeriod;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out shortPeriod))
            {
                shortPeriod = 12;
            }

			if( shortPeriod <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodShortParameterIsNegative);

			// Int64 Period
			int longPeriod;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longPeriod))
            {
                longPeriod = 26;
            }

			if( longPeriod <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodLongParameterIsNegative);

			if( longPeriod <= shortPeriod )
                throw new InvalidOperationException(SR.ExceptionIndicatorsLongPeriodLessThenShortPeriod);
			
			double [] longAverage; // Int64 Average
			double [] shortAverage; // Short Average
			
			// Find Int64 exponential moving average
			ExponentialMovingAverage( inputValues[1], out longAverage, longPeriod, false );

			// Find Short exponential moving average
			ExponentialMovingAverage( inputValues[1], out shortAverage, shortPeriod, false );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - longPeriod + 1];
			outputValues[1] = new double [inputValues[0].Length - longPeriod + 1];
			
			// Calculate MovingAverageConvergenceDivergence
			int outIndex = 0;
			for( int index = longPeriod - 1; index < inputValues[0].Length; index++ )
			{
				// set X value
				outputValues[0][outIndex] = inputValues[0][index];

				// set Y value
				outputValues[1][outIndex] = shortAverage[ outIndex + longPeriod - shortPeriod ] - longAverage[outIndex];

				outIndex++;
			}
		}

		/// <summary>
		/// The CCI is a timing system that is best applied to commodity contracts which 
		/// have cyclical or seasonal tendencies. CCI does not determine the length of 
		/// cycles - it is designed to detect when such cycles begin and end through 
		/// the use of a statistical analysis which incorporates a moving average and a divisor 
		/// reflecting both the possible and actual trading ranges. Although developed primarily 
		/// for commodities, the CCI could conceivably be used to analyze stocks as well. The 
		/// Commodity Channel Index ("CCI") measures the variation of a security’s price from 
		/// its statistical mean. High values show that prices are unusually high compared to 
		/// average prices whereas low values indicate that prices are unusually low. 
		/// 1. Calculate today's Typical Price (TP) = (H+L+C)/3 where H = high; L = low, and C = close. 
		/// 2. Calculate today's 20-day Simple Moving Average of the Typical Price (SMATP). 
		/// 3. Calculate today's Mean Deviation. First, calculate the absolute value of the difference 
		///    between today's SMATP and the typical price for each of the past 20 days. 
		///    Add all of these absolute values together and divide by 20 to find the Mean Deviation. 
		/// 4. The final step is to apply the Typical Price (TP), the Simple Moving Average of the 
		///    Typical Price (SMATP), the Mean Deviation and a Constant (.015).
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( Hi, Low, Close ).
		/// Output: 
		///		- 1 Y value ( CCI ).
		/// Parameters: 
		///		- Periods = is used to configure the number of periods to calculate the CCI. By default the Periods property is set to 10.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void CommodityChannelIndex(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
									
			// Period
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 10;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Typical Price
			double [] typicalPrice = new double[inputValues[0].Length];
			
			// Typical Price loop
			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				typicalPrice[index] = ( inputValues[1][index] + inputValues[2][index] + inputValues[3][index] ) / 3.0;
			}

			// Moving Average
			double [] movingAverage;
						
			// Simple Moving Average of the Typical Price 
			MovingAverage( typicalPrice, out movingAverage, period, false );

			// Calculate today's Mean Deviation. First, calculate the absolute value 
			// of the difference between today's SMATP and the typical price for each 
			// of the past 20 days. Add all of these absolute values together and 
			// divide by 20 to find the Mean Deviation. 

			// Mean Deviation
			double [] meanDeviation = new double[movingAverage.Length];

			double sum =0;
			for( int index = 0; index < movingAverage.Length; index++ )
			{
				sum = 0;
				for( int indexSum = index; indexSum < index + period; indexSum++ )
				{
					sum += Math.Abs( movingAverage[index] - typicalPrice[indexSum] );
				}
				meanDeviation[index] = sum / period;
			}

			outputValues = new double [2][];

			outputValues[0] = new double [meanDeviation.Length];
			outputValues[1] = new double [meanDeviation.Length];
			

			for( int index = 0; index < meanDeviation.Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index + period - 1];

				// Set Y values
				outputValues[1][index] = ( typicalPrice[index + period - 1] - movingAverage[index] ) / ( 0.015 * meanDeviation[index] );
				
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public GeneralTechnicalIndicators()
		{
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
		override public void Formula( string formulaName, double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList, out string [][] outLabels )
		{
			string name;
			outputValues = null;

			name = formulaName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			// Not used for these formulas.
			outLabels = null;

			try
			{
				switch( name )
				{
					case "STANDARDDEVIATION":
						StandardDeviation( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "AVERAGETRUERANGE":
						AverageTrueRange( inputValues, out outputValues, parameterList );
						break;
					case "EASEOFMOVEMENT":
						EaseOfMovement( inputValues, out outputValues );
						break;
					case "MASSINDEX":
						MassIndex( inputValues, out outputValues, parameterList );
						break;
					case "PERFORMANCE":
						Performance( inputValues, out outputValues );
						break;
					case "RATEOFCHANGE":
						RateOfChange( inputValues, out outputValues, parameterList );
						break;
					case "RELATIVESTRENGTHINDEX":
						RelativeStrengthIndex( inputValues, out outputValues, parameterList );
						break;
					case "TRIPLEEXPONENTIALMOVINGAVERAGE":
						Trix( inputValues, out outputValues, parameterList );
						break;
					case "MOVINGAVERAGECONVERGENCEDIVERGENCE":
						Macd( inputValues, out outputValues, parameterList );
						break;
					case "COMMODITYCHANNELINDEX":
						CommodityChannelIndex( inputValues, out outputValues, parameterList );
						break;
					default:
						outputValues = null;
						break;
				}
			}
			catch( IndexOutOfRangeException )
			{
				throw new InvalidOperationException( SR.ExceptionFormulaInvalidPeriod( name ) );
			}
			catch( OverflowException )
			{
				throw new InvalidOperationException( SR.ExceptionFormulaNotEnoughDataPoints( name ) );
			}
		}

		#endregion
	}
}
