// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class StatisticFormula
	{
		public AnovaResult Anova(
			double probability,
			string inputSeriesNames
			){
			throw new NotImplementedException ();
		}

		public double BetaFunction(
			double m,
			double n
			){
			throw new NotImplementedException ();
		}

		public double Correlation(
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double Covariance(
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double FDistribution(
			double value,
			int firstDegreeOfFreedom,
			int secondDegreeOfFreedom
			){
			throw new NotImplementedException ();
		}

		public FTestResult FTest(
			double probability,
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double GammaFunction(
			double value
			){
			throw new NotImplementedException ();
		}

		public double InverseFDistribution(
			double probability,
			int firstDegreeOfFreedom,
			int secondDegreeOfFreedom
			){
			throw new NotImplementedException ();
		}

		public double InverseNormalDistribution(
			double probability
			){
			throw new NotImplementedException ();
		}

		public double InverseTDistribution(
			double probability,
			int degreeOfFreedom
			){
			throw new NotImplementedException ();
		}

		public double Mean(
			string inputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double Median(
			string inputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double NormalDistribution(
			double zValue
			){
			throw new NotImplementedException ();
		}

		public double TDistribution(
			double value,
			int degreeOfFreedom,
			bool oneTail
			){
			throw new NotImplementedException ();
		}

		public TTestResult TTestEqualVariances(
			double hypothesizedMeanDifference,
			double probability,
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public TTestResult TTestPaired(
			double hypothesizedMeanDifference,
			double probability,
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public TTestResult TTestUnequalVariances(
			double hypothesizedMeanDifference,
			double probability,
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}

		public double Variance(
			string inputSeriesName,
			bool sampleVariance
			){
			throw new NotImplementedException ();
		}

		public ZTestResult ZTest(
			double hypothesizedMeanDifference,
			double varianceFirstGroup,
			double varianceSecondGroup,
			double probability,
			string firstInputSeriesName,
			string secondInputSeriesName
			){
			throw new NotImplementedException ();
		}
	}
}