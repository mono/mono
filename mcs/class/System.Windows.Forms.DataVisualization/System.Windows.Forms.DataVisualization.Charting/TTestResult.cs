// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class TTestResult
	{
		public double DegreeOfFreedom { get; private set;}
		public double FirstSeriesMean { get; private set;}
		public double FirstSeriesVariance { get; private set;}
		public double ProbabilityTOneTail { get; private set;}
		public double ProbabilityTTwoTail { get; private set;}
		public double SecondSeriesMean { get; private set;}
		public double SecondSeriesVariance { get; private set;}
		public double TCriticalValueOneTail { get; private set;}
		public double TCriticalValueTwoTail { get; private set;}
		public double TValue { get; private set;}
	}
}