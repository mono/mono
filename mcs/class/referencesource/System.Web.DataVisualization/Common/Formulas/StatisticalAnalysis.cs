//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StatisticalAnalysis.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Formulas
//
//	Classes:	StatisticalAnalysis
//
//  Purpose:	This class is used for Statistical Analysis
//
//	Reviewed:	AG - Apr 1, 2003
//
//===================================================================


using System;
using System.Collections;


#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Formulas
#else
	namespace System.Web.UI.DataVisualization.Charting.Formulas
#endif
{
	/// <summary>
	/// 
	/// </summary>
	internal class StatisticalAnalysis : IFormula
	{

		#region Error strings

		// Error strings
		//internal string inputArrayStart = "Formula requires";
		//internal string inputArrayEnd = "arrays";
		
		#endregion

		#region Parameters

		/// <summary>
		/// Formula Module name
		/// </summary>
        virtual public string Name { get { return SR.FormulaNameStatisticalAnalysis; } }

		#endregion // Parameters

		#region Methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public StatisticalAnalysis()
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
		
			outLabels = null;

			name = formulaName.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			try
			{
				switch( name )
				{
					case "TTESTEQUALVARIANCES":
						TTest( inputValues, out outputValues, parameterList, out outLabels, true );
						break;
					case "TTESTUNEQUALVARIANCES":
						TTest( inputValues, out outputValues, parameterList, out outLabels, false );
						break;
					case "TTESTPAIRED":
						TTestPaired( inputValues, out outputValues, parameterList, out outLabels );
						break;
					case "ZTEST":
						ZTest( inputValues, out outputValues, parameterList, out outLabels );
						break;
					case "FTEST":
						FTest( inputValues, out outputValues, parameterList, out outLabels );
						break;
					case "COVARIANCE":
						Covariance( inputValues, out outputValues, out outLabels );
						break;
					case "CORRELATION":
						Correlation( inputValues, out outputValues, out outLabels );
						break;
					case "ANOVA":
						Anova( inputValues, out outputValues, parameterList, out outLabels );
						break;
					case "TDISTRIBUTION":
						TDistribution( out outputValues, parameterList, out outLabels );
						break;
					case "FDISTRIBUTION":
						FDistribution( out outputValues, parameterList, out outLabels );
						break;
					case "NORMALDISTRIBUTION":
						NormalDistribution( out outputValues, parameterList, out outLabels );
						break;
					case "INVERSETDISTRIBUTION":
						TDistributionInverse( out outputValues, parameterList, out outLabels );
						break;
					case "INVERSEFDISTRIBUTION":
						FDistributionInverse( out outputValues, parameterList, out outLabels );
						break;
					case "INVERSENORMALDISTRIBUTION":
						NormalDistributionInverse( out outputValues, parameterList, out outLabels );
						break;
					case "MEAN":
						Average( inputValues, out outputValues, out outLabels );
						break;
					case "VARIANCE":
						Variance( inputValues, out outputValues, parameterList, out outLabels );
						break;
					case "MEDIAN":
						Median( inputValues, out outputValues, out outLabels );
						break;
					case "BETAFUNCTION":
						BetaFunction( out outputValues, parameterList, out outLabels );
						break;
					case "GAMMAFUNCTION":
						GammaFunction( out outputValues, parameterList, out outLabels );
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

		#endregion // Methods

		#region Statistical Tests		

        /// <summary>
        /// Anova test
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Anova(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// There is no enough input series
			 if( inputValues.Length < 3 )
                 throw new ArgumentException(SR.ExceptionStatisticalAnalysesNotEnoughInputSeries);
			
			outLabels = null;

			for( int index = 0; index < inputValues.Length - 1; index++ )
			{
				if( inputValues[index].Length != inputValues[index+1].Length )
                    throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAnovaTest);
			}

			// Alpha value
			double alpha;
			try
			{
				alpha = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			if( alpha < 0 || alpha > 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [10]; 

			// X
			outputValues[0] = new double [10];

			// Y
			outputValues[1] = new double [10];

			int m = inputValues.Length - 1;
			int n = inputValues[0].Length;

			double [] average = new double[ m ];
			double [] variance = new double[ m ];
			
			// Find averages
			for( int group = 0; group < m; group++ )
			{
				average[group] = Mean( inputValues[group+1] );
			}

			// Find variances
			for( int group = 0; group < m; group++ )
			{
				variance[group] = Variance( inputValues[group+1], true );
			}

			// Total Average ( for all groups )
			double averageTotal = Mean( average );

			// Total Sample Variance
			double totalS = 0;
			foreach( double avr in average )
			{
				totalS += ( avr - averageTotal ) * ( avr - averageTotal );
			}

			totalS /= ( m - 1 );

			// Group Sample Variance
			double groupS = Mean( variance );

			// F Statistica
			double f = totalS * ( n ) / groupS;

			// ****************************************
			// Sum of Squares
			// ****************************************

			// Grend Total Average
			double grandTotalAverage = 0;
			for( int group = 0; group < m; group++ )
			{
				foreach( double point in inputValues[group+1] )
				{
					grandTotalAverage += point;
				}
			}

			grandTotalAverage /= ( m * n );

			// Treatment Sum of Squares
			double trss = 0;
			for( int group = 0; group < m; group++ )
			{
				trss += ( average[group] - grandTotalAverage ) * ( average[group] - grandTotalAverage );
			}

			trss *= n;
			
			// Error Sum of Squares
			double erss = 0;
			for( int group = 0; group < m; group++ )
			{
				foreach( double point in inputValues[group+1] )
				{
					erss += ( point - average[group] ) * ( point - average[group] );
				}
			}

            outLabels[0][0] = SR.LabelStatisticalSumOfSquaresBetweenGroups; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = trss;

            outLabels[0][1] = SR.LabelStatisticalSumOfSquaresWithinGroups; 
			outputValues[0][1] = 2; 
			outputValues[1][1] = erss;

            outLabels[0][2] = SR.LabelStatisticalSumOfSquaresTotal; 
			outputValues[0][2] = 3; 
			outputValues[1][2] = trss + erss;

            outLabels[0][3] = SR.LabelStatisticalDegreesOfFreedomBetweenGroups; 
			outputValues[0][3] = 4; 
			outputValues[1][3] = m - 1;

            outLabels[0][4] = SR.LabelStatisticalDegreesOfFreedomWithinGroups; 
			outputValues[0][4] = 5; 
			outputValues[1][4] = m * ( n - 1 );

            outLabels[0][5] = SR.LabelStatisticalDegreesOfFreedomTotal; 
			outputValues[0][5] = 6; 
			outputValues[1][5] = m * n - 1;

            outLabels[0][6] = SR.LabelStatisticalMeanSquareVarianceBetweenGroups; 
			outputValues[0][6] = 7; 
			outputValues[1][6] = trss / ( m - 1 );

            outLabels[0][7] = SR.LabelStatisticalMeanSquareVarianceWithinGroups; 
			outputValues[0][7] = 8; 
			outputValues[1][7] = erss / ( m * ( n - 1 ) );

            outLabels[0][8] = SR.LabelStatisticalFRatio; 
			outputValues[0][8] = 9; 
			outputValues[1][8] = f;

            outLabels[0][9] = SR.LabelStatisticalFCriteria; 
			outputValues[0][9] = 10; 
			outputValues[1][9] = FDistributionInverse( alpha, m - 1, m * ( n - 1 ) ); 
		
		}




        /// <summary>
        /// Correlation measure the relationship between two data sets that 
        /// are scaled to be independent of the unit of measurement. The 
        /// population correlation calculation returns the covariance 
        /// of two data sets divided by the product of their standard 
        /// deviations: You can use the Correlation to determine whether two 
        /// ranges of data move together — that is, whether large values of 
        /// one set are associated with large values of the other 
        /// (positive correlation), whether small values of one set are 
        /// associated with large values of the other (negative correlation), 
        /// or whether values in both sets are unrelated (correlation 
        /// near zero).
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Correlation(double [][] inputValues, out double [][] outputValues, out string [][] outLabels )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

			// Find Covariance.
			double covar = Covar( inputValues[1], inputValues[2] );

			double varianceX = Variance( inputValues[1], false );
			double varianceY = Variance( inputValues[2], false );

			// Correlation
			double correl = covar / Math.Sqrt( varianceX * varianceY );

            outLabels[0][0] = SR.LabelStatisticalCorrelation; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = correl; 

		}

        /// <summary>
        /// Returns covariance, the average of the products of deviations 
        /// for each data point pair. Use covariance to determine the 
        /// relationship between two data sets. For example, you can 
        /// examine whether greater income accompanies greater 
        /// levels of education.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Covariance(double [][] inputValues, out double [][] outputValues, out string [][] outLabels )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

			// Find Covariance.
			double covar = Covar( inputValues[1], inputValues[2] );

            outLabels[0][0] = SR.LabelStatisticalCovariance; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = covar; 

		}

        /// <summary>
        /// Returns the result of an F-test. An F-test returns the one-tailed 
        /// probability that the variances in array1 and array2 are not 
        /// significantly different. Use this function to determine 
        /// whether two samples have different variances. For example, 
        /// given test scores from public and private schools, you can 
        /// test whether these schools have different levels of diversity.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void FTest(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			outLabels = null;

			double alpha;

			// The number of data points has to be > 1.
			CheckNumOfPoints( inputValues );
						
			// Alpha value
			try
			{
				alpha = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			if( alpha < 0 || alpha > 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}
			
			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [7]; 

			// X
			outputValues[0] = new double [7];

			// Y
			outputValues[1] = new double [7];

			// Find Variance of the first group
			double variance1 = Variance( inputValues[1], true );

			// Find Variance of the second group
			double variance2 = Variance( inputValues[2], true );

			// Find Mean of the first group
			double mean1 = Mean( inputValues[1] );

			// Find Mean of the second group
			double mean2 = Mean( inputValues[2] );
			
			// F Value
			double valueF = variance1 / variance2;

			if( variance2 == 0 )
			{
                throw new InvalidOperationException(SR.ExceptionStatisticalAnalysesZeroVariance);
			}

			// The way to find a left critical value is to reversed the degrees of freedom, 
			// look up the right critical value, and then take the reciprocal of this value. 
			// For example, the critical value with 0.05 on the left with 12 numerator and 15 
			// denominator degrees of freedom is found of taking the reciprocal of the critical 
			// value with 0.05 on the right with 15 numerator and 12 denominator degrees of freedom. 
			// Avoiding Left Critical Values. Since the left critical values are a pain to calculate, 
			// they are often avoided altogether. This is the procedure followed in the textbook. 
			// You can force the F test into a right tail test by placing the sample with the large 
			// variance in the numerator and the smaller variance in the denominator. It does not 
			// matter which sample has the larger sample size, only which sample has the larger 
			// variance. The numerator degrees of freedom will be the degrees of freedom for 
			// whichever sample has the larger variance (since it is in the numerator) and the 
			// denominator degrees of freedom will be the degrees of freedom for whichever sample 
			// has the smaller variance (since it is in the denominator). 
			bool lessOneF = valueF <= 1;

			double fDistInv;
			double fDist;

			if( lessOneF )
			{
				fDistInv = FDistributionInverse( 1 - alpha, inputValues[1].Length - 1, inputValues[2].Length - 1 );
				fDist = 1 - FDistribution( valueF, inputValues[1].Length - 1, inputValues[2].Length - 1 );
			}
			else
			{
				fDistInv = FDistributionInverse( alpha, inputValues[1].Length - 1, inputValues[2].Length - 1 );
				fDist = FDistribution( valueF, inputValues[1].Length - 1, inputValues[2].Length - 1 );
			}

            outLabels[0][0] = SR.LabelStatisticalTheFirstGroupMean; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = mean1;

            outLabels[0][1] = SR.LabelStatisticalTheSecondGroupMean; 
			outputValues[0][1] = 2; 
			outputValues[1][1] = mean2;

            outLabels[0][2] = SR.LabelStatisticalTheFirstGroupVariance; 
			outputValues[0][2] = 3; 
			outputValues[1][2] = variance1;

            outLabels[0][3] = SR.LabelStatisticalTheSecondGroupVariance; 
			outputValues[0][3] = 4; 
			outputValues[1][3] = variance2;

            outLabels[0][4] = SR.LabelStatisticalFValue; 
			outputValues[0][4] = 5; 
			outputValues[1][4] = valueF;

            outLabels[0][5] = SR.LabelStatisticalPFLessEqualSmallFOneTail; 
			outputValues[0][5] = 6; 
			outputValues[1][5] = fDist;

            outLabels[0][6] = SR.LabelStatisticalFCriticalValueOneTail; 
			outputValues[0][6] = 7; 
			outputValues[1][6] = fDistInv; 
		}


        /// <summary>
        /// Returns the two-tailed P-value of a z-test. The z-test 
        /// generates a standard score for x with respect to the data set, 
        /// array, and returns the two-tailed probability for the 
        /// normal distribution. You can use this function to assess 
        /// the likelihood that a particular observation is drawn 
        /// from a particular population.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void ZTest(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			// The number of data points has to be > 1.
			CheckNumOfPoints( inputValues );

			outLabels = null;

			double variance1;
			double variance2;
			double alpha;
			double HypothesizedMeanDifference;

			// Find Hypothesized Mean Difference parameter
			try
			{
				HypothesizedMeanDifference = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidMeanDifference);
			}

			if( HypothesizedMeanDifference < 0.0 ) 
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesNegativeMeanDifference);
			}

			// Find variance of the first group
			try
			{
				variance1 = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidVariance);
			}

			// Find variance of the second group
			try
			{
				variance2 = double.Parse( parameterList[2], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidVariance);
			}

			// Alpha value
			try
			{
				alpha = double.Parse( parameterList[3], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			if( alpha < 0 || alpha > 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}
			
			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [9]; 

			// X
			outputValues[0] = new double [9];

			// Y
			outputValues[1] = new double [9];

			// Find Mean of the first group
			double mean1 = Mean( inputValues[1] );

			// Find Mean of the second group
			double mean2 = Mean( inputValues[2] );
			
			double dev = Math.Sqrt( variance1 / inputValues[1].Length + variance2 / inputValues[2].Length );

			// Z Value
			double valueZ = ( mean1 - mean2 - HypothesizedMeanDifference ) / dev;

			double normalDistTwoInv = NormalDistributionInverse( 1 - alpha / 2 );
			double normalDistOneInv = NormalDistributionInverse( 1 - alpha);
			double normalDistOne;
			double normalDistTwo;

			if( valueZ < 0.0 )
			{
				normalDistOne = NormalDistribution( valueZ );
			}
			else
			{
				normalDistOne = 1.0 - NormalDistribution( valueZ );
			}

			normalDistTwo = 2.0 * normalDistOne;

            outLabels[0][0] = SR.LabelStatisticalTheFirstGroupMean; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = mean1;

            outLabels[0][1] = SR.LabelStatisticalTheSecondGroupMean; 
			outputValues[0][1] = 2; 
			outputValues[1][1] = mean2;

            outLabels[0][2] = SR.LabelStatisticalTheFirstGroupVariance; 
			outputValues[0][2] = 3; 
			outputValues[1][2] = variance1;

            outLabels[0][3] = SR.LabelStatisticalTheSecondGroupVariance; 
			outputValues[0][3] = 4; 
			outputValues[1][3] = variance2;

            outLabels[0][4] = SR.LabelStatisticalZValue; 
			outputValues[0][4] = 5; 
			outputValues[1][4] = valueZ;

            outLabels[0][5] = SR.LabelStatisticalPZLessEqualSmallZOneTail; 
			outputValues[0][5] = 6; 
			outputValues[1][5] = normalDistOne;

            outLabels[0][6] = SR.LabelStatisticalZCriticalValueOneTail; 
			outputValues[0][6] = 7; 
			outputValues[1][6] = normalDistOneInv;

            outLabels[0][7] = SR.LabelStatisticalPZLessEqualSmallZTwoTail; 
			outputValues[0][7] = 8; 
			outputValues[1][7] = normalDistTwo;

            outLabels[0][8] = SR.LabelStatisticalZCriticalValueTwoTail; 
			outputValues[0][8] = 9; 
			outputValues[1][8] = normalDistTwoInv; 
		}

        /// <summary>
        /// Returns the two-tailed P-value of a z-test. The z-test 
        /// generates a standard score for x with respect to the data set, 
        /// array, and returns the two-tailed probability for the 
        /// normal distribution. You can use this function to assess 
        /// the likelihood that a particular observation is drawn 
        /// from a particular population.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
        /// <param name="equalVariances">True if Variances are equal.</param>
		private void TTest(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels, bool equalVariances )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);

			outLabels = null;

			double variance1;
			double variance2;
			double alpha;
			double HypothesizedMeanDifference;

			// Find Hypothesized Mean Difference parameter
			try
			{
				HypothesizedMeanDifference = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidMeanDifference);
			}

			if( HypothesizedMeanDifference < 0.0 ) 
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesNegativeMeanDifference);
			}

			// Alpha value
			try
			{
				alpha = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			if( alpha < 0 || alpha > 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			// The number of data points has to be > 1.
			CheckNumOfPoints( inputValues );
						
			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [10]; 

			// X
			outputValues[0] = new double [10];

			// Y
			outputValues[1] = new double [10];

			// Find Mean of the first group
			double mean1 = Mean( inputValues[1] );

			// Find Mean of the second group
			double mean2 = Mean( inputValues[2] );

			variance1 = Variance( inputValues[1], true );

			variance2 = Variance( inputValues[2], true );

			double s;
			double T;
			int freedom;
			if( equalVariances )
			{
				freedom = inputValues[1].Length + inputValues[2].Length - 2;

				// S value
				s = ( ( inputValues[1].Length - 1 ) * variance1 + ( inputValues[2].Length - 1 ) * variance2 ) / ( inputValues[1].Length + inputValues[2].Length - 2 );

				// T value
				T = ( mean1 - mean2 - HypothesizedMeanDifference ) / ( Math.Sqrt( s * ( 1.0 / inputValues[1].Length + 1.0 / inputValues[2].Length ) ) );
		
			}
			else
			{
				double m = inputValues[1].Length;
				double n = inputValues[2].Length;
				double s1 = variance1;
				double s2 = variance2;
				double f = ( s1 / m + s2 / n ) * ( s1 / m + s2 / n ) / ( ( s1 / m ) * ( s1 / m ) / ( m - 1 ) + ( s2 / n ) * ( s2 / n ) / ( n - 1 ) );
				freedom = (int)Math.Round(f);

				s = Math.Sqrt( variance1 / inputValues[1].Length + variance2 / inputValues[2].Length );

				// Z Value
				T = ( mean1 - mean2 - HypothesizedMeanDifference ) / s;
			}
			
			double TDistTwoInv = StudentsDistributionInverse( alpha , freedom );

			bool more50 = alpha > 0.5;

			if( more50 )
			{
				alpha = 1 - alpha;
			}

			double TDistOneInv = StudentsDistributionInverse( alpha * 2.0, freedom );

			if( more50 )
			{
				TDistOneInv *= -1.0;
			}
			
			double TDistTwo = StudentsDistribution( T, freedom, false );
			double TDistOne = StudentsDistribution( T, freedom, true  );

            outLabels[0][0] = SR.LabelStatisticalTheFirstGroupMean; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = mean1;

            outLabels[0][1] = SR.LabelStatisticalTheSecondGroupMean; 
			outputValues[0][1] = 2; 
			outputValues[1][1] = mean2;

            outLabels[0][2] = SR.LabelStatisticalTheFirstGroupVariance; 
			outputValues[0][2] = 3; 
			outputValues[1][2] = variance1;

            outLabels[0][3] = SR.LabelStatisticalTheSecondGroupVariance; 
			outputValues[0][3] = 4; 
			outputValues[1][3] = variance2;

            outLabels[0][4] = SR.LabelStatisticalTValue; 
			outputValues[0][4] = 5; 
			outputValues[1][4] = T;

            outLabels[0][5] = SR.LabelStatisticalDegreeOfFreedom; 
			outputValues[0][5] = 6; 
			outputValues[1][5] = freedom;

            outLabels[0][6] = SR.LabelStatisticalPTLessEqualSmallTOneTail; 
			outputValues[0][6] = 7; 
			outputValues[1][6] = TDistOne;

            outLabels[0][7] = SR.LabelStatisticalSmallTCrititcalOneTail; 
			outputValues[0][7] = 8; 
			outputValues[1][7] = TDistOneInv;

            outLabels[0][8] = SR.LabelStatisticalPTLessEqualSmallTTwoTail; 
			outputValues[0][8] = 9; 
			outputValues[1][8] = TDistTwo;

            outLabels[0][9] = SR.LabelStatisticalSmallTCrititcalTwoTail; 
			outputValues[0][9] = 10; 
			outputValues[1][9] = TDistTwoInv; 
		}


        /// <summary>
        /// Returns the two-tailed P-value of a z-test. The z-test 
        /// generates a standard score for x with respect to the data set, 
        /// array, and returns the two-tailed probability for the 
        /// normal distribution. You can use this function to assess 
        /// the likelihood that a particular observation is drawn 
        /// from a particular population.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void TTestPaired(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// There is no enough input series
			if( inputValues.Length != 3 )
				throw new ArgumentException( SR.ExceptionPriceIndicatorsFormulaRequiresTwoArrays);
			
			if( inputValues[1].Length != inputValues[2].Length )
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidVariableRanges);

			outLabels = null;

			double variance;
			double alpha;
			double HypothesizedMeanDifference;
			int freedom;

			// Find Hypothesized Mean Difference parameter
			try
			{
				HypothesizedMeanDifference = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidMeanDifference);
			}

			if( HypothesizedMeanDifference < 0.0 ) 
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesNegativeMeanDifference);
			}
						
			// Alpha value
			try
			{
				alpha = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			if( alpha < 0 || alpha > 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}

			// The number of data points has to be > 1.
			CheckNumOfPoints( inputValues );
			
			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [10]; 

			// X
			outputValues[0] = new double [10];

			// Y
			outputValues[1] = new double [10];

			double [] difference = new double[inputValues[1].Length];
			
			for( int item = 0; item < inputValues[1].Length; item++ )
			{
				difference[item] = inputValues[1][item] - inputValues[2][item];
			}

			// Find Mean of the second group
			double mean = Mean( difference );

			variance = Math.Sqrt( Variance( difference, true ) );

			double T = ( Math.Sqrt( inputValues[1].Length ) * ( mean - HypothesizedMeanDifference ) ) / variance;

			freedom = inputValues[1].Length - 1;
			
			double TDistTwoInv = StudentsDistributionInverse( alpha , freedom );
            double TDistOneInv = alpha <= 0.5 ? StudentsDistributionInverse(2 * alpha, freedom) : double.NaN;
			double TDistTwo = StudentsDistribution( T, freedom, false );
			double TDistOne = StudentsDistribution( T, freedom, true  );

            outLabels[0][0] = SR.LabelStatisticalTheFirstGroupMean; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = Mean(inputValues[1]);

            outLabels[0][1] = SR.LabelStatisticalTheSecondGroupMean; 
			outputValues[0][1] = 2; 
			outputValues[1][1] = Mean(inputValues[2]);

            outLabels[0][2] = SR.LabelStatisticalTheFirstGroupVariance; 
			outputValues[0][2] = 3; 
			outputValues[1][2] = Variance(inputValues[1],true);

            outLabels[0][3] = SR.LabelStatisticalTheSecondGroupVariance; 
			outputValues[0][3] = 4; 
			outputValues[1][3] = Variance(inputValues[2],true);

            outLabels[0][4] = SR.LabelStatisticalTValue; 
			outputValues[0][4] = 5; 
			outputValues[1][4] = T;

            outLabels[0][5] = SR.LabelStatisticalDegreeOfFreedom; 
			outputValues[0][5] = 6; 
			outputValues[1][5] = freedom;

            outLabels[0][6] = SR.LabelStatisticalPTLessEqualSmallTOneTail; 
			outputValues[0][6] = 7; 
			outputValues[1][6] = TDistOne;

            outLabels[0][7] = SR.LabelStatisticalSmallTCrititcalOneTail; 
			outputValues[0][7] = 8; 
			outputValues[1][7] = TDistOneInv;

            outLabels[0][8] = SR.LabelStatisticalPTLessEqualSmallTTwoTail; 
			outputValues[0][8] = 9; 
			outputValues[1][8] = TDistTwo;

            outLabels[0][9] = SR.LabelStatisticalSmallTCrititcalTwoTail; 
			outputValues[0][9] = 10; 
			outputValues[1][9] = TDistTwoInv; 
		}


		#endregion // Statistical Tests

		#region Public distributions

        /// <summary>
        /// Returns the Percentage Points (probability) for the Student 
        /// t-distribution. The t-distribution is used in the hypothesis 
        /// testing of small sample data sets. Use this function in place 
        /// of a table of critical values for the t-distribution.
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void TDistribution(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// T value value
			double tValue;
			try
			{
				tValue = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidTValue);
			}

			// DegreeOfFreedom
			int freedom;
			try
			{
				freedom = int.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}

			// One Tailed distribution
			int oneTailed;
			try
			{
				oneTailed = int.Parse( parameterList[2], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidTailedParameter);
			}

			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = StudentsDistribution( tValue, freedom, oneTailed == 1 ); 

		}

        /// <summary>
        /// Returns the F probability distribution. You can use 
        /// this function to determine whether two data sets have 
        /// different degrees of diversity. For example, you can 
        /// examine test scores given to men and women entering 
        /// high school and determine if the variability in the 
        /// females is different from that found in the males.
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void FDistribution(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// F value value
			double fValue;
			try
			{
				fValue = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidTValue);
			}

			// Degree Of Freedom 1
			int freedom1;
			try
			{
				freedom1 = int.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}

			// Degree Of Freedom 2
			int freedom2;
			try
			{
				freedom2 = int.Parse( parameterList[2], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = FDistribution( fValue, freedom1, freedom2 ); 
		}

        /// <summary></summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void NormalDistribution(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// F value value
			double zValue;
			try
			{
				zValue = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidZValue);
			}
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = this.NormalDistribution( zValue ); 
		}

        /// <summary>
        /// Returns the t-value of the Student's t-distribution 
        /// as a function of the probability and the degrees 
        /// of freedom.
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void TDistributionInverse(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// T value value
			double probability;
			try
			{
				probability = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidProbabilityValue);
			}

			// DegreeOfFreedom
			int freedom;
			try
			{
				freedom = int.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}
		
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = StudentsDistributionInverse( probability, freedom ); 

		}

        /// <summary>
        /// Returns the inverse of the F probability distribution. 
        /// If p = FDIST(x,...), then FINV(p,...) = x. The F distribution 
        /// can be used in an F-test that compares the degree of 
        /// variability in two data sets. For example, you can analyze 
        /// income distributions in the United States and Canada to 
        /// determine whether the two ---- have a similar degree 
        /// of diversity.
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void FDistributionInverse(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// Probability value value
			double probability;
			try
			{
				probability = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidProbabilityValue);
			}

			// Degree Of Freedom 1
			int freedom1;
			try
			{
				freedom1 = int.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}

			// Degree Of Freedom 2
			int freedom2;
			try
			{
				freedom2 = int.Parse( parameterList[2], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = FDistributionInverse( probability, freedom1, freedom2 ); 
		}

        /// <summary>
        /// Returns the inverse of the standard normal 
        /// cumulative distribution. The distribution 
        /// has a mean of zero and a standard deviation 
        /// of one.
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void NormalDistributionInverse(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// Alpha value value
			double alpha;
			try
			{
				alpha = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidAlphaValue);
			}
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalProbability; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = this.NormalDistributionInverse( alpha ); 
		}

		#endregion

		#region Utility Statistical Functions


        /// <summary>
        /// Check number of data points. The number should be greater then 1.
        /// </summary>
        /// <param name="inputValues">Input series</param>
		private void CheckNumOfPoints( double [][] inputValues )
		{
			if( inputValues[1].Length < 2 )
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesNotEnoughDataPoints);
			}

			if( inputValues.Length > 2 )
			{
				if( inputValues[2].Length < 2 )
				{
                    throw new ArgumentException(SR.ExceptionStatisticalAnalysesNotEnoughDataPoints);
				}
			}
		}
	
		/// <summary>
		/// Returns covariance, the average of the products of deviations 
		/// for each data point pair. Use covariance to determine the 
		/// relationship between two data sets. For example, you can 
		/// examine whether greater income accompanies greater 
		/// levels of education.
		/// </summary>
		/// <param name="arrayX">First data set from X random variable.</param>
		/// <param name="arrayY">Second data set from Y random variable.</param>
		/// <returns>Returns covariance</returns>
		private double Covar( double [] arrayX, double [] arrayY )
		{
			// Check the number of data points
			if( arrayX.Length != arrayY.Length )
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesCovariance);
			}

			double [] arrayXY = new double[arrayX.Length];

			// Find XY
			for( int index = 0; index < arrayX.Length; index++ )
			{
				arrayXY[index] = arrayX[index] * arrayY[index];
			}

			// Find means
			double meanXY = Mean( arrayXY );
			double meanX = Mean( arrayX );
			double meanY = Mean( arrayY );

			// return covariance
			return meanXY - meanX * meanY;
		}

		/// <summary>
		/// Returns the natural logarithm of the gamma function, G(x).
		/// </summary>
		/// <param name="n">The value for which you want to calculate gamma function.</param>
		/// <returns>Returns the natural logarithm of the gamma function.</returns>
		private double GammLn( double n )
		{
			double x;
			double y;
			double tmp;
			double sum;
			double [] cof = {76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5};

			if( n < 0 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesGammaBetaNegativeParameters);
			}

			// Iterative method for Gamma function
			y = x = n;
			tmp = x + 5.5;
			tmp -= ( x + 0.5 ) * Math.Log( tmp );
			sum = 1.000000000190015;
			for( int item = 0; item <=5; item++ )
			{
				sum += cof[item] / ++y;
			}

			return -tmp + Math.Log( 2.5066282746310005 * sum / x );
		}

		/// <summary>
		/// Calculates Beta function
		/// </summary>
		/// <param name="m">First parameter for beta function</param>
		/// <param name="n">Second parameter for beta function</param>
		/// <returns>returns beta function</returns>
		private double BetaFunction( double m, double n )
		{
			return Math.Exp( GammLn( m ) + GammLn( n ) - GammLn( m + n ) );	
		}

        /// <summary>
        /// Used by betai: Evaluates continued fraction for 
        /// incomplete beta function by modified Lentz’s
        /// </summary>
        /// <param name="a">Beta incomplete parameter</param>
        /// <param name="b">Beta incomplete parameter</param>
        /// <param name="x">Beta incomplete parameter</param>
        /// <returns>Value used for Beta incomplete function</returns>
		private double BetaCF( double a, double b, double x )
		{
			int MAXIT = 100;
			double EPS = 3.0e-7;
			double FPMIN = 1.0e-30;

			int m,m2;
			double aa,c,d,del,h,qab,qam,qap;
			qab = a + b;
			qap= a + 1.0;
			qam = a - 1.0;
			c = 1.0;
			d = 1.0 - qab * x / qap;
			if ( Math.Abs(d) < FPMIN ) d=FPMIN;
			d = 1.0 / d;
			h = d;

			// Numerical approximation for Beta incomplete function
			for( m=1; m<=MAXIT; m++ ) 
			{
				m2 = 2*m;
				aa = m*(b-m)*x/((qam+m2)*(a+m2));

				// Find d coeficient
				d = 1.0 + aa*d;
				if( Math.Abs(d) < FPMIN ) d=FPMIN;

				// Find c coeficient
				c = 1.0 + aa / c;
				if( Math.Abs(c) < FPMIN ) c = FPMIN;

				// Find d coeficient
				d = 1.0 / d;

				// Find h coeficient
				h *= d*c;

				aa = -(a+m)*(qab+m)*x/((a+m2)*(qap+m2));

				// Recalc d coeficient
				d=1.0+aa*d;
				if (Math.Abs(d) < FPMIN) d=FPMIN;

				// Recalc c coeficient
				c=1.0+aa/c;
				if (Math.Abs(c) < FPMIN) c=FPMIN;

				// Recalc d coeficient
				d=1.0/d;
				del=d*c;

				// Recalc h coeficient
				h *= del;

				if (Math.Abs(del-1.0) < EPS) 
				{
					break;
				}
			}

			if (m > MAXIT)
			{
                throw new InvalidOperationException(SR.ExceptionStatisticalAnalysesIncompleteBetaFunction);
			}

			return h;
		}

		/// <summary>
		/// Standard normal density function
		/// </summary>
		/// <param name="t">T Value</param>
		/// <returns>Standard normal density</returns>
		private double NormalDistributionFunction(double t)
		{
			return 0.398942280401433 * Math.Exp( -t * t / 2 );
		}

		/// <summary>
		/// Returns the incomplete beta function Ix(a, b).
		/// </summary>
		/// <param name="a">Beta incomplete parameter</param>
		/// <param name="b">Beta incomplete parameter</param>
		/// <param name="x">Beta incomplete parameter</param>
		/// <returns>Beta Incomplete value</returns>
		private double BetaIncomplete( double a, double b, double x )
		{
			double bt;
            if (x < 0.0 || x > 1.0)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidInputParameter);
            if (x == 0.0 || x == 1.0)
            {
                bt = 0.0;
            }
            else
            {   // Factors in front of the continued fraction.
                bt = Math.Exp(GammLn(a + b) - GammLn(a) - GammLn(b) + a * Math.Log(x) + b * Math.Log(1.0 - x));
            }

            if (x < (a + 1.0) / (a + b + 2.0))
            {	//Use continued fraction directly.
                return bt * BetaCF(a, b, x) / a;
            }
            else
            {   // Use continued fraction after making the symmetry transformation. 
                return 1.0 - bt * BetaCF(b, a, 1.0 - x) / b;
            }
		}
		

		#endregion // Utility Statistical Functions
		
		#region Statistical Parameters

        /// <summary>
        /// Returns the average (arithmetic mean) of the arguments.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Average(double [][] inputValues, out double [][] outputValues, out string [][] outLabels )
		{
						
			outLabels = null;

			// Invalid number of data series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidSeriesNumber);

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];

			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalAverage; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = Mean( inputValues[1] ); 

		}

        /// <summary>
        /// Calculates variance
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Variance(double [][] inputValues, out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			
			// Sample Variance value
			bool sampleVariance;
			try
			{
				sampleVariance = bool.Parse( parameterList[0] );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidVariance);
			}	

			CheckNumOfPoints(inputValues);

			// Invalid number of data series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidSeriesNumber);

			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];
			
			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalVariance; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = Variance( inputValues[1], sampleVariance ); 

		}

        /// <summary>
        /// Calculates Median
        /// </summary>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void Median(double [][] inputValues, out double [][] outputValues, out string [][] outLabels )
		{
					
			outLabels = null;

			// Invalid number of data series
			if( inputValues.Length != 2 )
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidSeriesNumber);

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];
			
			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalMedian; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = Median( inputValues[1] ); 

		}

        /// <summary>
        /// Calculates Beta Function
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void BetaFunction(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// Degree of freedom
			double m;
			try
			{
				m = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}	

			// Degree of freedom
			double n;
			try
			{
				n = double.Parse( parameterList[1], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
			}	
					
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];
			
			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalBetaFunction; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = BetaFunction( m, n ); 

		}

        /// <summary>
        /// Calculates Gamma Function
        /// </summary>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Parameters</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		private void GammaFunction(out double [][] outputValues, string [] parameterList, out string [][] outLabels )
		{
			// Degree of freedom
			double m;
			try
			{
				m = double.Parse( parameterList[0], System.Globalization.CultureInfo.InvariantCulture );
			}
			catch(System.Exception)
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidInputParameter);
			}
				
			if( m < 0 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesGammaBetaNegativeParameters);
			}
			
			outLabels = null;

			// Output arrays
			outputValues = new double [2][];

			// Output Labels
			outLabels = new string [1][];

			// Parameters description
			outLabels[0] = new string [1]; 

			// X
			outputValues[0] = new double [1];
			
			// Y
			outputValues[1] = new double [1];

            outLabels[0][0] = SR.LabelStatisticalGammaFunction; 
			outputValues[0][0] = 1; 
			outputValues[1][0] = Math.Exp( GammLn( m ) ); 

		}


		/// <summary>
		/// Sort array of double values.
		/// </summary>
		/// <param name="values">Array of doubles which should be sorted.</param>
		private void Sort( ref double [] values )
		{
			
			double tempValue;
			for( int outLoop = 0; outLoop < values.Length; outLoop++ )
			{
				for( int inLoop = outLoop + 1; inLoop < values.Length; inLoop++ )
				{
					if( values[ outLoop ] > values[ inLoop ] )
					{
						tempValue = values[ outLoop ];
						values[ outLoop ] = values[ inLoop ];
						values[ inLoop ] = tempValue;
					}
				}
			}
		}

		/// <summary>
		/// Returns the median of the given numbers
		/// </summary>
		/// <param name="values">Array of double numbers</param>
		/// <returns>Median</returns>
		private double Median( double [] values )
		{
			// Exception for zero lenght of series.
			if( values.Length == 0 )
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidMedianConditions);
			}

			// Sort array
			Sort( ref values );

			int position = values.Length / 2;

			// if number of points is even
			if( values.Length % 2 == 0 )
			{
				return ( values[position-1] + values[position] ) / 2.0;
			}
			else
			{
				return values[position];
			}
		}
		
		/// <summary>
		/// Calculates a Mean for a series of numbers.
		/// </summary>
		/// <param name="values">series with double numbers</param>
		/// <returns>Returns Mean</returns>
		private double Mean( double [] values )
		{
			// Exception for zero lenght of series.
			if( values.Length == 0 )
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidMeanConditions);
			}

			// Find sum of values
			double sum = 0;
			foreach( double item in values )
			{
				sum += item;
			}

			// Calculate Mean
			return sum / values.Length;
		}

		/// <summary>
		/// Calculates a Variance for a series of numbers.
		/// </summary>
		/// <param name="values">double values</param>
		/// <param name="sampleVariance">If variance is calculated from sample sum has to be divided by n-1.</param>
		/// <returns>Variance</returns>
		private double Variance( double [] values, bool sampleVariance )
		{

			// Exception for zero lenght of series.
			if( values.Length < 1 )
			{
                throw new ArgumentException(SR.ExceptionStatisticalAnalysesInvalidVarianceConditions);
			}
			
			// Find sum of values
			double sum = 0;
			double mean = Mean( values );
			foreach( double item in values )
			{
				sum += (item - mean) * (item - mean);
			}

			// Calculate Variance
			if( sampleVariance )
			{
				return sum / ( values.Length - 1 );
			}
			else
			{
				return sum / values.Length;
			}
		}

		#endregion // Statistical Parameters
		
		# region Distributions

		/// <summary>
		/// Calculates the Percentage Points (probability) for the Student 
		/// t-distribution. The t-distribution is used in the hypothesis 
		/// testing of small sample data sets. Use this function in place 
		/// of a table of critical values for the t-distribution.
		/// </summary>
		/// <param name="tValue">The numeric value at which to evaluate the distribution.</param>
		/// <param name="n">An integer indicating the number of degrees of freedom.</param>
		/// <param name="oneTailed">Specifies the number of distribution tails to return.</param>
		/// <returns>Returns the Percentage Points (probability) for the Student t-distribution.</returns>
		private double StudentsDistribution(  double tValue, int n, bool oneTailed )
		{
			// Validation
			tValue = Math.Abs( tValue );
			if( n > 300 )
			{
				n = 300;
			}

			if( n < 1 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesStudentsNegativeFreedomDegree);
			}

			double result = 1 - BetaIncomplete( n / 2.0, 0.5, n / (n + tValue * tValue) );

			if( oneTailed )
				return  (  1.0 - result ) / 2.0;
			else
				return  1.0 - result;
		}

		/// <summary>
		/// Returns the standard normal cumulative distribution 
		/// function. The distribution has a mean of 0 (zero) and 
		/// a standard deviation of one. Use this function in place 
		/// of a table of standard normal curve areas.
		/// </summary>
		/// <param name="zValue">The value for which you want the distribution.</param>
		/// <returns>Returns the standard normal cumulative distribution.</returns>
		private double NormalDistribution( double zValue )
		{
			
			double [] a = {0.31938153,-0.356563782,1.781477937,-1.821255978,1.330274429};
			double result;
			if (zValue<-7.0)
			{
				result = NormalDistributionFunction(zValue)/Math.Sqrt(1.0+zValue*zValue);
			}
			else if (zValue>7.0)
			{
				result = 1.0 - NormalDistribution(-zValue);
			}
			else
			{
				result = 0.2316419;
				result=1.0/(1+result*Math.Abs(zValue));
				result=1-NormalDistributionFunction(zValue)*(result*(a[0]+result*(a[1]+result*(a[2]+result*(a[3]+result*a[4])))));
				if (zValue<=0.0) 
					result=1.0-result;
			}
			return result;
		}
		
		private double FDistribution( double x, int freedom1, int freedom2 )
		{
            if (x < 0)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidTValue);
            if (freedom1 <= 0)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
            if (freedom2 <= 0)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidDegreeOfFreedom);
            if (x == 0)
                return 1;
            if (x == double.PositiveInfinity)
                return 0;

    	    return BetaIncomplete( freedom2 / 2.0, freedom1 / 2.0, freedom2 / ( freedom2 + freedom1 * x ) );
		}

		#endregion // Distributions

		# region Inverse Distributions
				
		/// <summary>
		/// Calculates the t-value of the Student's t-distribution 
		/// as a function of the probability and the degrees of freedom.
		/// </summary>
		/// <param name="probability">The probability associated with the two-tailed Student's t-distribution.</param>
		/// <param name="n">The number of degrees of freedom to characterize the distribution.</param>
		/// <returns>Returns the t-value of the Student's t-distribution.</returns>
		private double StudentsDistributionInverse(  double probability, int n )
		{
            //Fix for boundary cases
            if (probability == 0)
                return double.PositiveInfinity;
            else if (probability == 1)
                return 0;
            else if (probability < 0 || probability > 1)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidProbabilityValue);

			int step  = 0;
			return StudentsDistributionSearch( probability, n, step, 0.0, 100000.0 );
		}

		/// <summary>
		/// Method for calculation of Inverse T Distribution (Binary tree)
		/// solution for non linear equations
		/// </summary>
		/// <param name="probability">Probability value</param>
		/// <param name="n">Degree of freedom</param>
		/// <param name="step">Step for Numerical solution for non linear equations</param>
		/// <param name="start">Start for numerical process</param>
		/// <param name="end">End for numerical process</param>
		/// <returns>Returns F ditribution inverse</returns>
		private double StudentsDistributionSearch( double probability, int n, int step, double start, double end )
		{
			step++;
			
			double mid = ( start + end ) / 2.0;
			double result = StudentsDistribution( mid, n, false );
			double resultX;

			if( step > 100 )
			{
				return mid;
			}
			
			if( result <= probability )
			{
				resultX = StudentsDistributionSearch( probability, n, step, start, mid );
			}
			else
			{
				resultX = StudentsDistributionSearch( probability, n, step, mid, end );
			}

			return resultX;
		}

		/// <summary>
		/// Returns the inverse of the standard normal cumulative distribution. 
		/// The distribution has a mean of zero and a standard deviation of one.
		/// </summary>
		/// <param name="probability">A probability corresponding to the normal distribution.</param>
		/// <returns>Returns the inverse of the standard normal cumulative distribution.</returns>
		private double NormalDistributionInverse( double probability )
		{
					
			// Validation
			if( probability < 0.00001 || probability > 0.99999 )
			{
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesNormalInvalidProbabilityValue);
			}

			double [] a = { 2.50662823884, -18.61500062529, 41.39119773534, -25.44106049637 };
			double [] b = { -8.47351093090, 23.08336743743, -21.06224101826, 3.13082909833 };
			double [] c = { 0.3374754822726147, 0.9761690190917186, 0.1607979714918209, 0.0276438810333863, 0.0038405729373609, 0.0003951896511919, 0.0000321767881768, 0.0000002888167364, 0.0000003960315187};

			double x,r;

			// Numerical Integration
			x = probability - 0.5;

			if ( Math.Abs(x) < 0.42 )
			{
				r = x * x;
				r = x * ( ( ( a[3] * r + a[2] ) * r + a[1] ) * r + a[0] ) / ( ( ( ( b[3] * r + b[2] ) * r + b[1] ) * r + b[0] ) * r + 1.0 );
				return( r );
			}
			r= probability;
			if( x > 0.0 ) 
			{
				r = 1.0 - probability;
			}

			r = Math.Log( -Math.Log( r ) );
			r = c[0] + r * ( c[1] + r * ( c[2] + r * ( c[3] + r * ( c[4] + r * ( c[5] + r * ( c[6] + r * ( c[7]+r * c[8] ) ) ) ) ) ) );
			if( x < 0.0 ) 
			{
				r = -r;
			}

			return r;
		}

		/// <summary>
		/// Calculates the inverse of the F probability distribution.
		/// The F distribution can be used in an F-test that compares 
		/// the degree of variability in two data sets.
		/// </summary>
		/// <param name="probability">A probability associated with the F cumulative distribution.</param>
		/// <param name="m">The numerator degrees of freedom.</param>
		/// <param name="n">The denominator degrees of freedom.</param>
		/// <returns>Returns the inverse of the F probability distribution.</returns>
		private double FDistributionInverse(  double probability, int m, int n )
		{
            //Fix for boundary cases
            if (probability == 0)
                return double.PositiveInfinity;
            else if (probability == 1)
                return 0;
            else if (probability < 0 || probability > 1)
                throw new ArgumentOutOfRangeException(SR.ExceptionStatisticalAnalysesInvalidProbabilityValue);

			int step  = 0;
			return FDistributionSearch( probability, m, n, step, 0.0, 10000.0 );
		}

		/// <summary>
		/// Method for calculation of Inverse F Distribution (Binary tree)
		/// solution for non linear equations
		/// </summary>
		/// <param name="probability">Probability value</param>
		/// <param name="m">Degree of freedom</param>
		/// <param name="n">Degree of freedom</param>
		/// <param name="step">Step for solution for non linear equations.</param>
		/// <param name="start">Start for numerical process</param>
		/// <param name="end">End for numerical process</param>
		/// <returns>Returns F ditribution inverse</returns>
		private double FDistributionSearch( double probability, int m, int n, int step, double start, double end )
		{
			step++;

			double mid = ( start + end ) / 2.0;
			double result = FDistribution( mid, m, n );
			double resultX;

			if( step > 30 )
			{
				return mid;
			}
			
			if( result <= probability )
			{
				resultX = FDistributionSearch( probability, m, n, step, start, mid );
			}
			else
			{
				resultX = FDistributionSearch( probability, m, n, step, mid, end );
			}

			return resultX;
		}


		#endregion // Inverse Distributions

	}
}



