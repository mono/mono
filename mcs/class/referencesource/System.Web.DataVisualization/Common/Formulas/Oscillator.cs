//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Oscillator.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	Oscillators
//
//  Purpose:	This class is used to calculate oscillator 
//				indicators used in Technical Analyses.
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
	/// This class is used to calculate oscillator 
	///	indicators used in Technical Analyses.
	/// </summary>
	internal class Oscillators : PriceIndicators
	{
		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
		override public string Name			{ get{ return "Oscillators";}}

		#endregion

		#region Formulas

		/// <summary>
		/// The Chaikin Oscillator is created by subtracting a 10 period 
		/// exponential moving average of the Accumulation/Distribution 
		/// line from a 3 period moving average of the 
		/// Accumulation/Distribution Line. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 4 Y values ( Hi, Low, Close, Volume ).
		/// Output: 
		///		- 1 Y value Chaikin Oscillator
		/// Parameters: 
		///		- Short Period for Exponential Moving average (default=3)
		///		- Int64 Period for Exponential Moving average (default=10)
		///	Extra Parameters: 
		///		- Start from First
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		/// <param name="extraParameterList">Array of strings - Extra parameters</param>
		private void ChaikinOscillator(double [][] inputValues, out double [][] outputValues, string [] parameterList, string [] extraParameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 5 )
			{
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresFourArrays);
			}
						
			// Different number of x and y values
			CheckNumOfValues( inputValues, 4 );

			// Short Period for Exp moving average
			int shortPeriod;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out shortPeriod))
            {
                shortPeriod = 3;
            }
			
			// Int64 Period for Exp moving average
			int longPeriod;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longPeriod))
            {
                longPeriod = 10;
            }

			if( shortPeriod > longPeriod || longPeriod <= 0 || shortPeriod <= 0 )
			{
                throw new ArgumentException(SR.ExceptionOscillatorObjectInvalidPeriod);
			}

			// Starting average from the first data point or after period.
			bool startFromFirst = bool.Parse( extraParameterList[0] );

			VolumeIndicators volume = new VolumeIndicators();

			double [][] outputDistribution = new double [2][];

			// Accumulation Distribution
			volume.AccumulationDistribution( inputValues, out outputDistribution );

			double [] ExpAvgDistribution;

			// Exponential Moving average of Accumulation Distribution
			ExponentialMovingAverage(outputDistribution[1],out ExpAvgDistribution,longPeriod,startFromFirst);

			double [] ExpAvg;

			// Exponential Moving average of close
			ExponentialMovingAverage(outputDistribution[1],out ExpAvg,shortPeriod,startFromFirst);

			outputValues = new double [2][];

			int period = Math.Min(ExpAvg.Length,ExpAvgDistribution.Length);

			outputValues[0] = new double [period];
			outputValues[1] = new double [period];

			// Accumulation Distribution
			int expIndex = 0;
			for( int index = inputValues[1].Length - period; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][expIndex] = inputValues[0][index];

				// Set Y values
				if(startFromFirst)
				{
					// Number of items in all arays is the same and they are aligned by time.
					outputValues[1][expIndex] = ExpAvg[expIndex] - ExpAvgDistribution[expIndex];
				}
				else if( (expIndex + longPeriod - shortPeriod) < ExpAvg.Length)
				{
					// Number of items in MovingAverages arrays is different and requires adjustment.
					outputValues[1][expIndex] = ExpAvg[expIndex + longPeriod - shortPeriod] - ExpAvgDistribution[expIndex];
				}
				else
				{
					outputValues[1][expIndex] = Double.NaN;
				}
				expIndex++;
			}
		}

		/// <summary>
		/// The Detrended Price Oscillator ("DPO") attempts to 
		/// eliminate the trend in prices. Detrended prices allow 
		/// you to more easily identify cycles and overbought/oversold 
		/// levels. To calculate the DPO, you specify a time period. 
		/// Cycles longer than this time period are removed from 
		/// prices, leaving the shorter-term cycles. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y value ( Close ).
		/// Output: 
		///		- 1 Y value Detrended Price Oscillator
		/// Parameters: 
		///		- Period
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void DetrendedPriceOscillator(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
			{
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			}
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
			
			// Short Period for Exp moving average
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

			double [] outputAverage;

			// Moving Average
			MovingAverage( inputValues[1], out outputAverage, period, false );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - period*3/2];
			outputValues[1] = new double [inputValues[1].Length - period*3/2];

			// Detrended Price Oscillator
			for( int index = 0; index < outputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index + period + period/2];

				// Set Y values
				outputValues[1][index] = inputValues[1][index + period + period/2] - outputAverage[index];
			}
		}

		/// <summary>
		/// Chaikin's Volatility indicator compares the spread 
		/// between a security's high and low prices. 
		/// It quantifies volatility as a widening of the range 
		/// between the high and the low price. There are two ways 
		/// to interpret this measure of volatility. One method 
		/// assumes that market tops are generally accompanied by 
		/// increased volatility (as investors get nervous and 
		/// indecisive) and that the latter stages of a market 
		/// bottom are generally accompanied by decreased volatility 
		/// (as investors get bored). Another method (Mr. Chaikin's) 
		/// assumes that an increase in the Volatility indicator over 
		/// a relatively short time period indicates that a bottom is 
		/// near (e.g., a panic sell-off) and that a decrease in 
		/// volatility over a longer time period indicates an 
		/// approaching top (e.g., a mature bull market). As with 
		/// almost all experienced investors, Mr. Chaikin recommends 
		/// that you do not rely on any one indicator. He suggests 
		/// using a moving average ----ion or trading band system 
		/// to confirm this (or any) indicator.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( Hi, Low ).
		/// Output: 
		///		- 1 Y value Volatility Chaikins
		/// Parameters: 
		///		- Periods (default 10)- is used to specify the Shift days, By default this property is set to 10.
		///     - SignalPeriod (default 10)- is used to calculate Exponential Moving Avg of the Signal line, By default this property is set to 10.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void VolatilityChaikins(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
			{
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			}
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
						
			// Period
			int period;
            if (parameterList.Length < 1 ||
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 10;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionOscillatorNegativePeriodParameter);

			// Signal Period for Exp moving average
			int signalPeriod;
            if (parameterList.Length < 2 ||
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out signalPeriod))
            {
                signalPeriod = 10;
            }

			if( signalPeriod <= 0 )
                throw new InvalidOperationException(SR.ExceptionOscillatorNegativeSignalPeriod);

			double [] outputAverage;

			double [] hiLowInput = new double[inputValues[1].Length];

			// Find Hi - Low
			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				hiLowInput[index] = inputValues[1][index] - inputValues[2][index];
			}
            
			// Exponential Moving Average
			ExponentialMovingAverage( hiLowInput, out outputAverage, signalPeriod, false );
			
			outputValues = new double [2][];

			outputValues[0] = new double [outputAverage.Length - period];
			outputValues[1] = new double [outputAverage.Length - period];

			// Volatility Chaikins
			for( int index = 0; index < outputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index + period + signalPeriod - 1];

				// Set Y values
				if( outputAverage[index] != 0.0 )
					outputValues[1][index] = ( outputAverage[index + period] - outputAverage[index] ) / outputAverage[index] * 100.0;
				else
					// Div with zero error.
					outputValues[1][index] = 0.0;
			}
		}

		/// <summary>
		/// The Volume Oscillator displays the difference between two 
		/// moving averages of a security's volume. The difference 
		/// between the moving averages can be expressed in either 
		/// points or percentages. You can use the difference between 
		/// two moving averages of volume to determine if the overall 
		/// volume trend is increasing or decreasing. When the Volume 
		/// Oscillator rises above zero, it signifies that the 
		/// shorter-term volume moving average has risen above 
		/// the longer-term volume moving average, and thus, that 
		/// the short-term volume trend is higher (i.e., more volume) 
		/// than the longer-term volume trend.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 1 Y values ( Volume ).
		/// Output: 
		///		- 1 Y value VolumeOscillator
		/// Parameters: 
		///		- ShortPeriod (Default 5)= is used to configure the short period.
		///		- LongPeriod (Default 10)= is used to configure the Int64 period.
		///		- Percentage (Default true)= The Volume Oscillator can display the difference between the two moving averages as either points or percentages.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void VolumeOscillator(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 2 )
			{
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			}
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
						
			// Short Period
			int shortPeriod;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out shortPeriod))
            {
                shortPeriod = 5;
            }

			// Int64 Period
			int longPeriod;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longPeriod))
            {
                longPeriod = 10;
            }

			if( shortPeriod > longPeriod || longPeriod <= 0 || shortPeriod <= 0 )
                throw new ArgumentException(SR.ExceptionOscillatorObjectInvalidPeriod);

			// percentage
			bool percentage;
            if (parameterList.Length < 3 || 
                !bool.TryParse(parameterList[2], out percentage))
            {
                percentage = true;
            }
			
			double [] shortAverage;
			double [] longAverage;

			// Find Short moving average
			MovingAverage( inputValues[1], out shortAverage, shortPeriod, false );

			// Find Int64 moving average
			MovingAverage( inputValues[1], out longAverage, longPeriod, false );

			outputValues = new double [2][];

			outputValues[0] = new double [longAverage.Length];
			outputValues[1] = new double [longAverage.Length];
			
			// Volume Oscillator
			for( int index = 0; index < longAverage.Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index + longPeriod-1];

				// Set Y values
				outputValues[1][index] = shortAverage[index + shortPeriod] - longAverage[index];

				// RecalculateAxesScale difference in %
				if( percentage )
				{
					// Div by zero error.
					if( longAverage[index] == 0.0 )
						outputValues[1][index]=0.0;
					else
						outputValues[1][index] = outputValues[1][index] / shortAverage[index + shortPeriod] * 100;
				}
			}
		}

		/// <summary>
		/// The Stochastic Indicator is based on the observation that 
		/// as prices increase, closing prices tend to accumulate ever 
		/// closer to the highs for the period. Conversely, as prices 
		/// decrease, closing prices tend to accumulate ever closer to 
		/// the lows for the period. Trading decisions are made with 
		/// respect to divergence between % of "D" (one of the two 
		/// lines generated by the study) and the item's price. For 
		/// example, when a commodity or stock makes a high, reacts, 
		/// and subsequently moves to a higher high while corresponding 
		/// peaks on the % of "D" line make a high and then a lower 
		/// high, a bearish divergence is indicated. When a commodity 
		/// or stock has established a new low, reacts, and moves to a 
		/// lower low while the corresponding low points on the % of 
		/// "D" line make a low and then a higher low, a bullish 
		/// divergence is indicated. Traders act upon this divergence 
		/// when the other line generated by the study (K) crosses on 
		/// the right-hand side of the peak of the % of "D" line in the 
		/// case of a top, or on the right-hand side of the low point 
		/// of the % of "D" line in the case of a bottom. The Stochastic 
		/// Oscillator is displayed as two lines. The main line is 
		/// called "%K." The second line, called "%D," is a moving 
		/// average of %K. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( Hi, Low, Close ).
		/// Output: 
		///		- 2 Y value ( %K, %D )
		/// Parameters: 
		///		- PeriodD (Default 10) = is used for %D calculation as SMA of %K.
		///     - PeriodK (Default 10) = is used to calculate %K.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		internal void StochasticIndicator(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 4 )
			{
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);
			}
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
			
			// PeriodD for moving average
			int periodD;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out periodD))
            {
                periodD = 10;
            }

			if( periodD <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// PeriodK for moving average
			int periodK;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out periodK))
            {
                periodK = 10;
            }

			if( periodK <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Output arrays
			outputValues = new double [3][];

			// X
			outputValues[0] = new double [inputValues[0].Length - periodK - periodD + 2];

			// K%
			outputValues[1] = new double [inputValues[0].Length - periodK - periodD + 2];

			// D%
			outputValues[2] = new double [inputValues[0].Length - periodK - periodD + 2];

			double [] K = new double [inputValues[0].Length - periodK + 1];

			// Find K%
			for( int index = periodK - 1; index < inputValues[0].Length; index++ )
			{
				// Find Lowest Low and Highest High
				double minLow = double.MaxValue;
				double maxHi = double.MinValue;
				for( int indexHL = index - periodK + 1; indexHL <= index; indexHL++ )
				{
					if( minLow > inputValues[2][indexHL] )
						minLow = inputValues[2][indexHL];

					if( maxHi < inputValues[1][indexHL] )
						maxHi = inputValues[1][indexHL];
				}
				// Find K%
				K[index - periodK + 1] = ( inputValues[3][index] - minLow ) / ( maxHi - minLow ) * 100;

				// Set X and Y K output
				if( index >= periodK + periodD - 2 )
				{
					outputValues[0][index - periodK - periodD + 2] = inputValues[0][index];
					outputValues[1][index - periodK - periodD + 2] = K[index - periodK + 1];
				}
			}

			// Find D%
			MovingAverage( K, out outputValues[2], periodD, false );
		}

		/// <summary>
		/// Williams’ %R (pronounced "percent R") is a momentum 
		/// indicator that measures overbought/oversold levels. 
		/// Williams’ %R was developed by Larry Williams. The 
		/// interpretation of Williams' %R is very similar to that 
		/// of the Stochastic Oscillator except that %R is plotted 
		/// upside-down and the Stochastic Oscillator has internal 
		/// smoothing. To display the Williams’ %R indicator on an 
		/// upside-down scale, it is usually plotted using negative 
		/// values (e.g., -20%). Readings in the range of 80 to 100% 
		/// indicate that the security is oversold while readings in 
		/// the 0 to 20% range suggest that it is overbought.
		/// As with all overbought/oversold indicators, it is best to 
		/// wait for the security's price to change direction before 
		/// placing your trades. For example, if an overbought/oversold 
		/// indicator (such as the Stochastic Oscillator or Williams' 
		/// %R) is showing an overbought condition, it is wise to wait 
		/// for the security's price to turn down before selling the 
		/// security. (The MovingAverageConvergenceDivergence is a good indicator to monitor change 
		/// in a security's price.) It is not unusual for 
		/// overbought/oversold indicators to remain in an 
		/// overbought/oversold condition for a long time period as 
		/// the security's price continues to climb/fall. Selling 
		/// simply because the security appears overbought may take 
		/// you out of the security long before its price shows signs 
		/// of deterioration.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 3 Y values ( Hi, Low, Close ).
		/// Output: 
		///		- 2 Y value ( %R )
		/// Parameters: 
		///		- Period (Default 14) = is used to configure the number of periods to calculate the WilliamsR
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		internal void WilliamsR(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 4 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresThreeArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 3 );
			
			// PeriodD for moving average
			int period;
            if (parameterList.Length < 1 || 
                !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = 14;
            }

			if( period <= 0 )
                throw new InvalidOperationException(SR.ExceptionPeriodParameterIsNegative);

			// Output arrays
			outputValues = new double [2][];

			// X
			outputValues[0] = new double [inputValues[0].Length - period + 1];

			// R%
			outputValues[1] = new double [inputValues[0].Length - period + 1];

			// Find R%
			for( int index = period - 1; index < inputValues[0].Length; index++ )
			{
				// Find Lowest Low and Highest High
				double minLow = double.MaxValue;
				double maxHi = double.MinValue;
				for( int indexHL = index - period + 1; indexHL <= index; indexHL++ )
				{
					if( minLow > inputValues[2][indexHL] )
						minLow = inputValues[2][indexHL];

					if( maxHi < inputValues[1][indexHL] )
						maxHi = inputValues[1][indexHL];
				}
				// Set X value
				outputValues[0][index - period + 1] = inputValues[0][index];

				// Find R%
				outputValues[1][index - period + 1] = ( maxHi - inputValues[3][index] ) / ( maxHi - minLow ) * (-100.0);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		public Oscillators()
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

			// Not used for these formulas.
			outLabels = null;

			name = formulaName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			try
			{
				switch( name )
				{
					case "STOCHASTICINDICATOR":
						StochasticIndicator( inputValues, out outputValues, parameterList );
						break;
					case "CHAIKINOSCILLATOR":
						ChaikinOscillator( inputValues, out outputValues, parameterList, extraParameterList );
						break;
					case "DETRENDEDPRICEOSCILLATOR":
						DetrendedPriceOscillator( inputValues, out outputValues, parameterList );
						break;
					case "VOLATILITYCHAIKINS":
						VolatilityChaikins( inputValues, out outputValues, parameterList );
						break;
					case "VOLUMEOSCILLATOR":
						VolumeOscillator( inputValues, out outputValues, parameterList );
						break;
					case "WILLIAMSR":
						WilliamsR( inputValues, out outputValues, parameterList );
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
