// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class DataFormula
	{
		public bool IsEmptyPointIgnored { get; set; }
		public bool IsStartFromFirst { get; set; }
		public StatisticFormula Statistics { get; private set;}

		[MonoTODO]
		public void CopySeriesValues (string inputSeries,string outputSeries)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula (FinancialFormula formulaName,string inputSeries)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula (FinancialFormula formulaName,Series inputSeries)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula (FinancialFormula formulaName, string inputSeries, string outputSeries) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula(FinancialFormula formulaName, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula(FinancialFormula formulaName, string parameters, string inputSeries, string outputSeries)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void FinancialFormula(FinancialFormula formulaName, string parameters, Series inputSeries, Series outputSeries)
		{
			throw new NotImplementedException();
		}
	}
}
