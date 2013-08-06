// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataFormula
	{
		public bool IsEmptyPointIgnored { get; set; }
		public bool IsStartFromFirst { get; set; }
		public StatisticFormula Statistics { get; private set;}

		public void CopySeriesValues(string inputSeries,string outputSeries){
			throw new NotImplementedException();
		}

		public void FinancialFormula(FinancialFormula formulaName,string inputSeries){
			throw new NotImplementedException();
		}

		public void FinancialFormula(FinancialFormula formulaName,Series inputSeries){
			throw new NotImplementedException();
		}

		public void FinancialFormula(
			FinancialFormula formulaName,
			string inputSeries,
			string outputSeries
			){
			throw new NotImplementedException();
		}

		public void FinancialFormula(
			FinancialFormula formulaName,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException();
		}

		public void FinancialFormula(
			FinancialFormula formulaName,
			string parameters,
			string inputSeries,
			string outputSeries
			){
			throw new NotImplementedException();
		}

		public void FinancialFormula(
			FinancialFormula formulaName,
			string parameters,
			Series inputSeries,
			Series outputSeries
			){
			throw new NotImplementedException();
		}

	}
}