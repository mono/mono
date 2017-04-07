//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		VolumeIndicator.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	VolumeIndicators
//
//  Purpose:	This class is used for calculations of 
//				technical analyses volume indicators.
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
	/// This class is used for calculations of 
	/// technical analyses volume indicators.
	/// </summary>
	internal class VolumeIndicators : PriceIndicators
	{
		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
        override public string Name { get { return SR.FormulaNameVolumeIndicators; } }

		#endregion

		#region Methods

		/// <summary>
		/// Default Constructor
		/// </summary>
		public VolumeIndicators()
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

			name = formulaName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			// Not used for these formulas.
			outLabels = null;

			try
			{
				switch( name )
				{
					case "MONEYFLOW":
						MoneyFlow( inputValues, out outputValues, parameterList );
						break;
					case "ONBALANCEVOLUME":
						OnBalanceVolume( inputValues, out outputValues );
						break;
					case "NEGATIVEVOLUMEINDEX":
						NegativeVolumeIndex( inputValues, out outputValues, parameterList );
						break;
					case "POSITIVEVOLUMEINDEX":
						PositiveVolumeIndex( inputValues, out outputValues, parameterList );
						break;
					case "PRICEVOLUMETREND":
						PriceVolumeTrend( inputValues, out outputValues );
						break;
					case "ACCUMULATIONDISTRIBUTION":
						AccumulationDistribution( inputValues, out outputValues );
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
		
		#region Formulas
		
		/// <summary>
		/// The Money Flow Index ("MFI") is a momentum indicator that 
		/// measures the strength of money flowing in and out of 
		/// a security. It is related to the Relative Strength Index, 
		/// but where the RSI only incorporates prices, the Money Flow 
		/// Index accounts for volume. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 4 Y values ( High, Low, Close, Volume ).
		/// Output: 
		///		- 1 Y value Money Flow Indicator.
		/// Parameters: 
		///		- Period
		/// </summary>
		/// <param name="inputValues">Arrays of doubles</param>
		/// <param name="outputValues">Arrays of doubles</param>
		/// <param name="parameterList">Array of strings</param>
		private void MoneyFlow(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			int length = inputValues.Length;
						
			// There is no enough series
			if( length != 5 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresFourArrays);

			// Different number of x and y values
			CheckNumOfValues( inputValues, 4 );
						
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

			// Not enough values for Money Flow.
			if( inputValues[0].Length < period )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsNotEnoughPoints);

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length - period + 1];
			outputValues[1] = new double [inputValues[0].Length - period + 1];
			double [] TypicalPrice = new double [inputValues[1].Length];
			double [] MoneyFlow = new double [inputValues[1].Length];
			double [] PositiveMoneyFlow = new double [inputValues[1].Length];
			double [] NegativeMoneyFlow = new double [inputValues[1].Length];

			// Find Money Flow
			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				// Find Typical Price
				TypicalPrice[index] = (inputValues[1][index] + inputValues[2][index] + inputValues[3][index])/3.0;		
				// Find Money Flow
				MoneyFlow[index] = (inputValues[1][index] + inputValues[2][index] + inputValues[3][index])/3.0 * inputValues[4][index];		
			}

			// Find Money Flow
			for( int index = 1; index < inputValues[1].Length; index++ )
			{
				// Positive Typical Price
				if( TypicalPrice[index] > TypicalPrice[index - 1] )
				{
					PositiveMoneyFlow[index] = MoneyFlow[index];
					NegativeMoneyFlow[index] = 0;
				}
				// Negative Typical Price
				if( TypicalPrice[index] < TypicalPrice[index - 1] )
				{
					NegativeMoneyFlow[index] = MoneyFlow[index];
					PositiveMoneyFlow[index] = 0;
				}
			}

			double PosMoney = 0;
			double NegMoney = 0;
			for( int index = period - 1; index < inputValues[1].Length; index++ )
			{
				PosMoney = 0;
				NegMoney = 0;
				// Find Money flow using period
				for( int periodIndex = index - period + 1; periodIndex <= index; periodIndex++ )
				{
					NegMoney += NegativeMoneyFlow[periodIndex];
					PosMoney += PositiveMoneyFlow[periodIndex];
				}

				// X value
				outputValues[0][index - period + 1] = inputValues[0][index];

				// Money Flow Index
				outputValues[1][index - period + 1] = 100.0 - 100.0 / ( 1.0 + (PosMoney / NegMoney) );

			}
		}

		/// <summary>
		/// The Price and Volume Trend ("PVT") is similar to 
		/// On Balance Volume ("OBV,") in that it is a cumulative 
		/// total of volume that is adjusted depending on changes 
		/// in closing prices. But where OBV adds all volume on days 
		/// when prices close higher and subtracts all volume on days 
		/// when prices close lower, the PVT adds/subtracts only 
		/// a portion of the daily volume. The amount of volume 
		/// added to the PVT is determined by the amount that prices 
		/// rose or fell relative to the previous day’s close. 
		/// The PVT is calculated by multiplying the day’s volume 
		/// by the percent that the security’s price changed, and 
		/// adding this value to a cumulative total.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( Close, Volume ).
		/// Output: 
		///		- 1 Y value Price Volume Trend Indicator.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		private void PriceVolumeTrend(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];
			
			// Set X and Y zero values
			outputValues[0][0] = inputValues[0][0];
			outputValues[1][0] = 0;

			double yesterdayClose;
			double todayClose;

			// Price Volume Trend Indicator
			for( int index = 1; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set Y values
				yesterdayClose = inputValues[1][index-1];
				todayClose = inputValues[1][index];

				// Price Volume Trend for one point
				outputValues[1][index] = ( todayClose - yesterdayClose ) / yesterdayClose * inputValues[2][index] + outputValues[1][index-1];
			}
		}

		/// <summary>
		/// On Balance Volume ("OBV") is a momentum indicator that 
		/// relates volume to price change. OBV is one of the most 
		/// popular volume indicators and was developed by 
		/// Joseph Granville. Constructing an OBV line is very 
		/// simple: The total volume for each day is assigned a 
		/// positive or negative value depending on whether prices 
		/// closed higher or lower that day. A higher close results 
		/// in the volume for that day to get a positive value, while 
		/// a lower close results in negative value. A running total 
		/// is kept by adding or subtracting each day's volume based 
		/// on the direction of the close. The direction of the OBV 
		/// line is the thing to watch, not the actual volume numbers. 
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( Close, Volume ).
		/// Output: 
		///		- 1 Y value On Balance Volume Indicator.
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		private void OnBalanceVolume(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];
			
			outputValues[0][0] = inputValues[0][0];
			outputValues[1][0] = inputValues[2][0];

			// Find On Balance Volume
			for( int index = 1; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Set Y Values
				// If today’s close is greater than yesterday’s close then
				if( inputValues[1][index - 1] < inputValues[1][index] )
					outputValues[1][index] = outputValues[1][index - 1] + inputValues[2][index];
				// If today’s close is less than yesterday’s close then
				else if( inputValues[1][index - 1] > inputValues[1][index] )
					outputValues[1][index] = outputValues[1][index - 1] - inputValues[2][index];
				// If today’s close is equal to yesterday’s close then
				else
					outputValues[1][index] = outputValues[1][index - 1];
			}
		}

		/// <summary>
		/// The Negative Volume Index ("NVI") focuses on days where 
		/// the volume decreases from the previous day. The premise 
		/// being that the "smart money" takes positions on days when 
		/// volume decreases.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( Close, Volume ).
		/// Output: 
		///		- 1 Y value Negative Volume index.
		/// Parameters: 
		///		- StartValue : double
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void NegativeVolumeIndex(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			// Start Value
			double startValue;
			try
				{startValue = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch(System.Exception)
            { throw new InvalidOperationException(SR.ExceptionVolumeIndicatorStartValueMissing); }

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];

			outputValues[0][0] = inputValues[0][0];
			outputValues[1][0] = startValue;
				
			// Find Negative Volume Index
			for( int index = 1; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// If today’s volume is less than yesterday’s volume then
				if( inputValues[2][index] < inputValues[2][index-1] )
				{
					double yesterdayClose = inputValues[1][index-1];
					double todayClose = inputValues[1][index];
					
					outputValues[1][index] = ( todayClose - yesterdayClose ) / yesterdayClose * outputValues[1][index-1] + outputValues[1][index-1];
				}
				// If today’s volume is greater than or equal to yesterday’s volume then:
				else
					outputValues[1][index] = outputValues[1][index-1];
				
			}
		}

		/// <summary>
		/// The Positive Volume Index ("PVI") focuses on days where 
		/// the volume increased from the previous day. The premise 
		/// being that the "crowd" takes positions on days when 
		/// volume increases. Interpretation of the PVI assumes that 
		/// on days when volume increases, the crowd-following 
		/// "uninformed" investors are in the market. Conversely, on 
		/// days with decreased volume, the "smart money" is quietly 
		/// taking positions. Thus, the PVI displays what the 
		/// not-so-smart-money is doing. (The Negative Volume Index, 
		/// displays what the smart money is doing.) Note, however, 
		/// that the PVI is not a contrarian indicator. Even though 
		/// the PVI is supposed to show what the not-so-smart-money 
		/// is doing, it still trends in the same direction as prices.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 2 Y values ( Close, Volume ).
		/// Output: 
		///		- 1 Y value On Positive Volume index.
		/// Parameters: 
		///		- StartValue : double
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		/// <param name="parameterList">Array of strings - Parameters</param>
		private void PositiveVolumeIndex(double [][] inputValues, out double [][] outputValues, string [] parameterList)
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 2 );
			
			// Start Value
			double startValue;
			try
			{startValue = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );}
			catch(System.Exception)
            { throw new InvalidOperationException(SR.ExceptionVolumeIndicatorStartValueMissing); }

			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];

			outputValues[0][0] = inputValues[0][0];
			outputValues[1][0] = startValue;
				
			// Find Negative Volume Index
			for( int index = 1; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// If today’s volume is greater than yesterday’s volume then
				if( inputValues[2][index] > inputValues[2][index-1] )
				{
					double yesterdayClose = inputValues[1][index-1];
					double todayClose = inputValues[1][index];
					
					outputValues[1][index] = ( todayClose - yesterdayClose ) / yesterdayClose * outputValues[1][index-1] + outputValues[1][index-1];
				}
				// If today’s volume is less than or equal to yesterday’s volume then:
				else
					outputValues[1][index] = outputValues[1][index-1];
			}
		}

		/// <summary>
		/// The Accumulation/Distribution is a momentum indicator that 
		/// associates changes in price and volume. The indicator is 
		/// based on the premise that the more volume that accompanies 
		/// a price move, the more significant the price move. A portion 
		/// of each day’s volume is added or subtracted from 
		/// a cumulative total. The nearer the closing price is to 
		/// the high for the day, the more volume added to 
		/// the cumulative total. The nearer the closing price is to 
		/// the low for the day, the more volume subtracted from the 
		/// cumulative total. If the close is exactly between the high 
		/// and low prices, nothing is added to the cumulative total.
		/// ---------------------------------------------------------
		/// Input: 
		///		- 4 Y values ( Hi, Low, Close, Volume ).
		/// Output: 
		///		- 1 Y value Accumulation Distribution
		/// </summary>
		/// <param name="inputValues">Arrays of doubles - Input values</param>
		/// <param name="outputValues">Arrays of doubles - Output values</param>
		internal void AccumulationDistribution(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough input series
			if( inputValues.Length != 5 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresFourArrays);
			
			// Different number of x and y values
			CheckNumOfValues( inputValues, 4 );
			
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[0].Length];

			double [] distribution = new double [inputValues[0].Length];
			
			// Set X and Y zero values
			outputValues[0][0] = inputValues[0][0];
			outputValues[1][0] = 0;
			

			// Accumulation Distribution
			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				// Set X values
				outputValues[0][index] = inputValues[0][index];

				// Distribution  {(Close - Low) - (High - Close)} / (High - Low) * Volume
				distribution[index] = ((inputValues[3][index] - inputValues[2][index])-(inputValues[1][index] - inputValues[3][index]))/(inputValues[1][index] - inputValues[2][index])*inputValues[4][index];
			}

			// The Accumulation Distribution Index is calculated as a cumulative total of each day's reading
			double sum = 0;
			for( int index = 0; index < inputValues[1].Length; index++ )
			{
				sum += distribution[index];
				outputValues[1][index] = sum;
			}
		}

		#endregion
	}
}
