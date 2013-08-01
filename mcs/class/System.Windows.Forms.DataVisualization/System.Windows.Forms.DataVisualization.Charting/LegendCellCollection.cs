// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class LegendCellCollection : ChartNamedElementCollection<LegendCell>
	{
		public int Add(
			LegendCellType cellType,
			string text,
			ContentAlignment alignment
			){
			throw new NotImplementedException ();
		}

		public void Insert(
			int index,
			LegendCellType cellType,
			string text,
			ContentAlignment alignment
			){
			throw new NotImplementedException ();
		}
	}
}