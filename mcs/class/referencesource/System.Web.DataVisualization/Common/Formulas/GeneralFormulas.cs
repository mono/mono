//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		GeneralFormulas.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	GeneralFormulas
//
//  Purpose:	This class calculates Running total and average.
//				Could be used for Pareto chart.
//
//	Reviewed:	GS - August 6, 2002
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
	/// This class calculates Running total and average.
	/// Could be used for Pareto chart
	/// </summary>
	internal class GeneralFormulas : PriceIndicators
	{
		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
        override public string Name { get { return SR.FormulaNameGeneralFormulas; } }

		#endregion

		#region Formulas

        /// <summary>
        /// Formula which calculates cumulative total.
        /// ---------------------------------------------------------
        /// Input: 
        /// 	- Y values.
        /// Output: 
        /// 	- Running Total.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
        /// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		private void RuningTotal(double [][] inputValues, out double [][] outputValues)
		{
			// There is not enough series
			if( inputValues.Length != 2 )
			{
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
			}

			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
						
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[1].Length];

			// Cumulative total
			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				outputValues[0][index] = inputValues[0][index];

				if( index > 0 )
				{
					outputValues[1][index] = inputValues[1][index] + outputValues[1][index-1];
				}
				else
				{
					outputValues[1][index] = inputValues[1][index];
				}
			}
		}

        /// <summary>
        /// Running Average Formula
        /// ---------------------------------------------------------
        /// Input: 
        /// 	- Y values.
        /// Output: 
        /// 	- Running Average.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
        /// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		private void RunningAverage(double [][] inputValues, out double [][] outputValues)
		{
			// There is no enough series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

			// Different number of x and y values
			CheckNumOfValues( inputValues, 1 );
						
			outputValues = new double [2][];

			outputValues[0] = new double [inputValues[0].Length];
			outputValues[1] = new double [inputValues[1].Length];

			// Total
			double total = 0;
			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				total += inputValues[1][index];
			}

			// Runing Average
			for( int index = 0; index < inputValues[0].Length; index++ )
			{
				outputValues[0][index] = inputValues[0][index];

				if( index > 0 )
					outputValues[1][index] = inputValues[1][index] / total * 100 + outputValues[1][index-1];
				else
					outputValues[1][index] = inputValues[1][index] / total * 100;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor.
		/// </summary>
		public GeneralFormulas()
		{
		}

        /// <summary>
        /// The first method in the module, which converts a formula 
        /// name to the corresponding private method.
        /// </summary>
        /// <param name="formulaName">String which represent a formula name.</param>
        /// <param name="inputValues">Arrays of doubles - Input values.</param>
        /// <param name="outputValues">Arrays of doubles - Output values.</param>
        /// <param name="parameterList">Array of strings - Formula parameters.</param>
        /// <param name="extraParameterList">Array of strings - Extra Formula parameters from DataManipulator object.</param>
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
					case "RUNINGTOTAL":
						RuningTotal( inputValues, out outputValues );
						break;
					case "RUNINGAVERAGE":
						RunningAverage( inputValues, out outputValues );
						break;
					default:
						outputValues = null;
						break;
				}
			}
			catch( IndexOutOfRangeException )
			{
				throw new InvalidOperationException( SR.ExceptionFormulaInvalidPeriod(name) );
			}
			catch( OverflowException )
			{
				throw new InvalidOperationException( SR.ExceptionFormulaNotEnoughDataPoints(name) );
			}
		}
		#endregion
		
	}
}
