//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		TimeSeriesAndForecasting.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	TimeSeriesAndForecasting
//
//  Purpose:	This class is used for calculations of 
//				time series and forecasting
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
	/// This class is used for calculations of 
	/// time series and forecasting
	/// </summary>
	internal class TimeSeriesAndForecasting : IFormula
	{
		#region Enumeration

		/// <summary>
		/// AxisName of regression
		/// </summary>
		internal enum RegressionType
		{
			/// <summary>
			/// Polynomial trend
			/// </summary>
			Polynomial,

			/// <summary>
			/// IsLogarithmic trend
			/// </summary>
			Logarithmic,

			/// <summary>
			/// Power trend
			/// </summary>
			Power,

			/// <summary>
			/// Exponential trend
			/// </summary>
			Exponential
		}

		#endregion

		#region Properties

		/// <summary>
		/// Formula Module name
		/// </summary>
        virtual public string Name { get { return SR.FormulaNameTimeSeriesAndForecasting; } }

		#endregion

		#region Methods
		
		/// <summary>
		/// Public constructor.
		/// </summary>
		public TimeSeriesAndForecasting()
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
					case "FORECASTING":
						Forecasting( inputValues, out outputValues, parameterList );
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
				throw new InvalidOperationException( SR.ExceptionFormulaNotEnoughDataPoints( name ) );
			}
		}

		#endregion
		
		#region Formulas

        /// <summary>
        /// Forecasting formula predicts future values of the time series variable. 
        /// Multiple regressions are used for this forecasting model. Any method 
        /// of fitting equations to data may be called regression. Such equations 
        /// are valuable for at least two purposes: making predictions and judging 
        /// the strength of relationships. Of the various methods of performing 
        /// regression, Last Square is the most widely used. This formula returns 
        /// two more series, which represents upper and lower bond of error. Error 
        /// is based on standard deviation and represents a linear combination of 
        /// approximation error and forecasting error.
        /// ---------------------------------------------------------
        /// Input: 
        /// 	- Y values.
        /// Output: 
        /// 	- Forecasting
        /// 	- upper bond error
        /// 	- lower bond error
        /// Parameters: 
        /// 	- Polynomial degree (Default: 2 - Linear regression )
        /// 	- Forecasting period (Default: Half of the series length )
        /// 	- Returns Approximation error (Default: true)
        /// 	- Returns Forecasting error (Default: true)
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void Forecasting(double[][] inputValues, out double[][] outputValues, string[] parameterList)
        {
            // Polynomial degree
            int degree;
            RegressionType regressionType = RegressionType.Polynomial;

            if (String.Equals(parameterList[0],"Exponential", StringComparison.OrdinalIgnoreCase))
            {
                regressionType = RegressionType.Exponential;
                degree = 2;
            }
            else if (String.Equals(parameterList[0],"Linear", StringComparison.OrdinalIgnoreCase))
            {
                regressionType = RegressionType.Polynomial;
                degree = 2;
            }
            else if (String.Equals(parameterList[0],"IsLogarithmic", StringComparison.OrdinalIgnoreCase) || 
                     String.Equals(parameterList[0],"Logarithmic",   StringComparison.OrdinalIgnoreCase))
            {
                regressionType = RegressionType.Logarithmic;
                degree = 2;
            }
            else if (String.Equals(parameterList[0],"Power", StringComparison.OrdinalIgnoreCase))
            {
                regressionType = RegressionType.Power;
                degree = 2;
            }
            else
            {
                if (parameterList.Length < 1 || 
                    !int.TryParse(parameterList[0], NumberStyles.Any, CultureInfo.InvariantCulture, out degree))
                {
                    degree = 2;
                }
            }


            if (degree > 5 || degree < 1)
                throw new InvalidOperationException(SR.ExceptionForecastingDegreeInvalid);

            if (degree > inputValues[0].Length)
                throw new InvalidOperationException(SR.ExceptionForecastingNotEnoughDataPoints(degree.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            // Forecasting period
            int period;
            if (parameterList.Length < 2 || 
                !int.TryParse(parameterList[1], NumberStyles.Any, CultureInfo.InvariantCulture, out period))
            {
                period = inputValues[0].Length / 2;
            }

            // Approximation error
            bool approximationError;
            if (parameterList.Length < 3 || 
                !bool.TryParse(parameterList[2], out approximationError))
            {
                approximationError = true;
            }

            // Forecasting error
            bool forecastingError;
            if (parameterList.Length < 4 || 
                !bool.TryParse(parameterList[3], out forecastingError))
            {
                forecastingError = true;
            }

            double[][] tempOut;
            // Find regresion
            Regression(regressionType, inputValues, out tempOut, degree, period);

            // If error disabled get out from procedure
            if (!forecastingError && !approximationError)
            {
                outputValues = tempOut;
                return;
            }

            double[][] inputErrorEst = new double[2][];
            double[][] outputErrorEst;
            inputErrorEst[0] = new double[inputValues[0].Length / 2];
            inputErrorEst[1] = new double[inputValues[0].Length / 2];

            for (int index = 0; index < inputValues[0].Length / 2; index++)
            {
                inputErrorEst[0][index] = inputValues[0][index];
                inputErrorEst[1][index] = inputValues[1][index];
            }

            Regression(regressionType, inputErrorEst, out outputErrorEst, degree, inputValues[0].Length / 2);

            // Find the average for forecasting error
            double error = 0;
            for (int index = inputValues[0].Length / 2; index < outputErrorEst[1].Length; index++)
            {
                error += (outputErrorEst[1][index] - inputValues[1][index]) * (outputErrorEst[1][index] - inputValues[1][index]);
            }
            error /= inputValues[0].Length - inputValues[0].Length / 2;
            error = Math.Sqrt(error);
            error /= (inputValues[0].Length / 4);

            // Find the standard deviation
            double dev = 0;
            for (int index = 0; index < inputValues[0].Length; index++)
            {
                dev += (tempOut[1][index] - inputValues[1][index]) * (tempOut[1][index] - inputValues[1][index]);
            }
            dev /= inputValues[0].Length;
            dev = Math.Sqrt(dev);

            outputValues = new double[4][];
            outputValues[0] = tempOut[0];
            outputValues[1] = tempOut[1];
            outputValues[2] = new double[tempOut[0].Length];
            outputValues[3] = new double[tempOut[0].Length];

            if (!approximationError)
                dev = 0;

            if (!forecastingError)
                error = 0;

            for (int index = 0; index < inputValues[0].Length; index++)
            {
                outputValues[2][index] = tempOut[1][index] + 2 * dev;
                outputValues[3][index] = tempOut[1][index] - 2 * dev;
            }
            double sumError = 0;
            for (int index = inputValues[0].Length; index < tempOut[0].Length; index++)
            {
                sumError += error;
                outputValues[2][index] = tempOut[1][index] + sumError + 2 * dev;
                outputValues[3][index] = tempOut[1][index] - sumError - 2 * dev;
            }
        }

        /// <summary>
        /// Any method of fitting equations to data may be called regression. 
        /// Such equations are valuable for at least two purposes: making 
        /// predictions and judging the strength of relationships. Of the 
        /// various methods of performing regression, Last Square is the 
        /// most widely used. 
        /// </summary>
        /// <param name="regressionType">AxisName of regression Polynomial, exponential, etc.</param>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="polynomialDegree">Polynomial degree (Default: 2 - Linear regression )</param>
        /// <param name="forecastingPeriod">Forecasting period (Default: Half of the series length )</param>
		private void Regression( RegressionType regressionType, double [][] inputValues, out double [][] outputValues, int polynomialDegree, int forecastingPeriod )
		{
			if( regressionType == RegressionType.Exponential )
			{
				double [] oldYValues = new double[ inputValues[1].Length ];
				for( int index = 0; index < inputValues[1].Length; index++ )
				{
					oldYValues[ index ] = inputValues[1][index];
					if( inputValues[1][index] <= 0 )
					{
                        throw new InvalidOperationException(SR.ExceptionForecastingExponentialRegressionHasZeroYValues);
					}
					inputValues[1][index] = Math.Log( inputValues[1][index] );
				}

				

				PolynomialRegression( regressionType, inputValues, out outputValues, 2, forecastingPeriod, 0 );

				inputValues[1] = oldYValues;
			}
			else if( regressionType == RegressionType.Logarithmic )
			{
				double interval;
				double first = inputValues[0][0];

				// Find Interval for X values
				interval = Math.Abs( inputValues[0][0] - inputValues[0][inputValues[0].Length - 1] ) / ( inputValues[0].Length - 1 );
			
				if( interval <= 0 )
					interval = 1;

				for( int index = 0; index < inputValues[0].Length; index++ )
				{
					inputValues[0][index] = Math.Log( inputValues[0][index] );
				}

				PolynomialRegression( regressionType, inputValues, out outputValues, 2, forecastingPeriod, interval );

				// Create Y values based on approximation.
				for( int i = 0; i < outputValues[0].Length; i++ )
				{
					// Set X value
					outputValues[0][i] = first + i * interval;
				}
			}
			else if( regressionType == RegressionType.Power )
			{
				double [] oldYValues = new double[ inputValues[1].Length ];
				double interval;
				double first = inputValues[0][0];

				for( int index = 0; index < inputValues[1].Length; index++ )
				{
					oldYValues[ index ] = inputValues[1][index];
					if( inputValues[1][index] <= 0 )
					{
                        throw new InvalidOperationException(SR.ExceptionForecastingPowerRegressionHasZeroYValues);
					}
				}

				// Find Interval for X values
				interval = Math.Abs( inputValues[0][0] - inputValues[0][inputValues[0].Length - 1] ) / ( inputValues[0].Length - 1 );
			
				if( interval <= 0 )
					interval = 1;

				PolynomialRegression( regressionType, inputValues, out outputValues, 2, forecastingPeriod, interval );

				inputValues[1] = oldYValues;

				// Create Y values based on approximation.
				for( int i = 0; i < outputValues[0].Length; i++ )
				{
					// Set X value
					outputValues[0][i] = first + i * interval;
				}
			}
			else
			{
				PolynomialRegression( regressionType, inputValues, out outputValues, polynomialDegree, forecastingPeriod, 0 );
			}
		}

        /// <summary>
        /// Any method of fitting equations to data may be called regression. 
        /// Such equations are valuable for at least two purposes: making 
        /// predictions and judging the strength of relationships. Of the 
        /// various methods of performing regression, Last Square is the 
        /// most widely used. 
        /// </summary>
        /// <param name="regressionType">AxisName of regression Polynomial, exponential, etc.</param>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="polynomialDegree">Polynomial degree (Default: 2 - Linear regression )</param>
        /// <param name="forecastingPeriod">Forecasting period (Default: Half of the series length )</param>
        /// <param name="logInterval">Interval for logarithmic scale</param>
		private void PolynomialRegression( RegressionType regressionType, double [][] inputValues, out double [][] outputValues, int polynomialDegree, int forecastingPeriod, double logInterval )
		{
			double [] coefficients = new double [polynomialDegree];
			int size = inputValues[0].Length;
			double minimumX = double.MaxValue;
			double interval = 1.0;

			// Find Interval for X values
			interval = Math.Abs( inputValues[0][0] - inputValues[0][inputValues[0].Length - 1] ) / ( inputValues[0].Length - 1 );
			
			if( interval <= 0 )
				interval = 1;

			if( regressionType != RegressionType.Logarithmic )
			{
				// Avoid Rounding error because of big X values.
				// Find Minimum X value 
				for( int xIndex = 0; xIndex < inputValues[0].Length; xIndex++ )
				{
					if( minimumX > inputValues[0][xIndex] )
						minimumX = inputValues[0][xIndex];
				}

				// Change X values.
				for( int xIndex = 0; xIndex < inputValues[0].Length; xIndex++ )
				{
					inputValues[0][xIndex] -= minimumX - 1;
				}
			}

			if( regressionType == RegressionType.Power )
			{
				for( int index = 0; index < inputValues[0].Length; index++ )
				{
					inputValues[0][index] = Math.Log( inputValues[0][index] );
					inputValues[1][index] = Math.Log( inputValues[1][index] );
				}
			}

			double [][] mainDeterminant = new double [polynomialDegree][];
			for(int arrayIndex = 0;  arrayIndex < polynomialDegree; arrayIndex++)
			{
				mainDeterminant[arrayIndex] = new double [polynomialDegree];
			}

			// Main determinant
			for( int k = 0; k < polynomialDegree; k++ )
			{
				for( int i = 0; i < polynomialDegree; i++ )
				{
					mainDeterminant[i][k] = 0;
					for( int j = 0; j < inputValues[0].Length; j++ )
					{
						mainDeterminant[i][k] += (double)Math.Pow( inputValues[0][j], (i+k) );
					}
				}
			}
			double mainValue = Determinant(mainDeterminant);
			
			// Coefficient determinant
			for( int i = 0; i < polynomialDegree; i++ )
			{
				double [][] coeffDeterminant = CopyDeterminant(mainDeterminant);
				for( int k = 0; k < polynomialDegree; k++ )
				{
					coeffDeterminant[i][k] = 0;
					for( int j = 0; j < inputValues[0].Length; j++ )
					{
						coeffDeterminant[i][k] += (double)inputValues[1][j] * (double)Math.Pow( inputValues[0][j], k );
					}
				}
				coefficients[i] = Determinant(coeffDeterminant) / mainValue;
			}

			// Create output arrays for approximation and forecasting
			outputValues = new double[2][];
			outputValues[0] = new double[size + forecastingPeriod];
			outputValues[1] = new double[size + forecastingPeriod];

			if( regressionType == RegressionType.Polynomial )
			{
				// Create Y values based on approximation.
				for( int i = 0; i < size + forecastingPeriod; i++ )
				{
					// Set X value
					outputValues[0][i] = inputValues[0][0] + i * interval;

					outputValues[1][i] = 0;
					for( int j = 0; j < polynomialDegree; j++ )
					{
						outputValues[1][i]+= (double)coefficients[j]*Math.Pow(outputValues[0][i],j);
					}
				}
			}
			else if( regressionType == RegressionType.Exponential )
			{
				// Create Y values based on approximation.
				for( int i = 0; i < size + forecastingPeriod; i++ )
				{
					// Set X value
					outputValues[0][i] = inputValues[0][0] + i * interval;

					outputValues[1][i]= Math.Exp( coefficients[0] ) * Math.Exp( coefficients[1] * outputValues[0][i] );
				}
			}
			else if( regressionType == RegressionType.Logarithmic )
			{
				// Create Y values based on approximation.
				for( int i = 0; i < size + forecastingPeriod; i++ )
				{
					// Set X value
					outputValues[0][i] = Math.Exp( inputValues[0][0] ) + i * logInterval;
					
					outputValues[1][i]= coefficients[1] * Math.Log( outputValues[0][i] ) + coefficients[0];
				}
			}
			else if( regressionType == RegressionType.Power )
			{
				// Create Y values based on approximation.
				for( int i = 0; i < size + forecastingPeriod; i++ )
				{
					// Set X value
					outputValues[0][i] = Math.Exp( inputValues[0][0] ) + i * logInterval;
					
					outputValues[1][i]= Math.Exp( coefficients[0] ) * Math.Pow( outputValues[0][i], coefficients[1] );
				}
			}

			if( regressionType != RegressionType.Logarithmic )
			{
				// Return X values.
				for( int xIndex = 0; xIndex < size + forecastingPeriod; xIndex++ )
				{
					outputValues[0][xIndex] += minimumX - 1;
				}
			}
		}

		/// <summary>
		/// This method recalculates determinant. This method is used for 
		/// recursive calls for sub determinants too.
		/// </summary>
		/// <param name="inputDeterminant">Input determinant</param>
		/// <returns>Result of determinant</returns>
		private double Determinant( double [][] inputDeterminant )
		{
			double sum = 0;
			double sign = 1.0;

			// Determinant is 2X2 - calculate value
			if( inputDeterminant.Length == 2 )
			{
				return inputDeterminant[0][0]*inputDeterminant[1][1] - inputDeterminant[0][1]*inputDeterminant[1][0];
			}

			// Determinant is biger than 2X2. Go to recursive 
			// call of Determinant method
			for( int column = 0; column < inputDeterminant.GetLength(0); column++ )
			{
				// Make sub determinant
				double [][] newDeterminant = MakeSubDeterminant( inputDeterminant, column );

				sum += sign * Determinant( newDeterminant ) * inputDeterminant[column][0];
				sign *= -1.0;
			}
			return sum;
		}

		/// <summary>
		/// This method will create a new determinant, which is 
		/// smaller by one rank (dimension). Specified column 
		/// and zero rows will be skipped.
		/// </summary>
		/// <param name="inputDeterminant">Input determinant</param>
		/// <param name="columnPos">Position of column, which has to be skipped</param>
		/// <returns>New determinant</returns>
		private double [][] MakeSubDeterminant( double [][] inputDeterminant, int columnPos )
		{
			// Get Determinant Size
			int size = inputDeterminant.GetLength(0);

			// Prepare sub Determinant
			double [][] newDeterminant = new double [size - 1][];
			for(int arrayIndex = 0;  arrayIndex < size - 1; arrayIndex++)
			{
				newDeterminant[arrayIndex] = new double [size - 1];
			}


			int newColumn = 0;
			// Copy columns
			for( int column = 0; column < size; column++ )
			{
				// Skeep this column
				if( column == columnPos )
					continue;

				// Copy rows
				for( int  row = 1; row < size; row++ )
				{
					newDeterminant[newColumn][row-1] = inputDeterminant[column][row];
				}

				// Go to new column for new determinant
				newColumn++;
			}

			// Return new determinant
			return newDeterminant;
		}

		/// <summary>
		/// This method will copy determinant
		/// </summary>
		/// <param name="inputDeterminant">Input determinant</param>
		/// <returns>New determinant</returns>
		private double [][] CopyDeterminant( double [][] inputDeterminant )
		{
			// Get Determinant Size
			int size = inputDeterminant.GetLength(0);

			// Prepare sub Determinant
			double [][] newDeterminant = new double [size][];
			for(int arrayIndex = 0;  arrayIndex < size; arrayIndex++)
			{
				newDeterminant[arrayIndex] = new double [size];
			}

			// Copy columns
			for( int column = 0; column < size; column++ )
			{
				// Copy rows
				for( int  row = 0; row < size; row++ )
				{
					newDeterminant[column][row] = inputDeterminant[column][row];
				}
			}

			// Return new determinant
			return newDeterminant;
		}

		#endregion
	}
}
