// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class LegendItemsCollection : ChartElementCollection<LegendItem>
	{
		public int Add(
			Color color,
			string text
			){
			throw new NotImplementedException ();
		}
		public int Add(
			string image,
			string text
			){
			throw new NotImplementedException ();
		}

		public void Insert(
			int index,
			Color color,
			string text
			){
			throw new NotImplementedException ();
		}
		public void Insert(
			int index,
			string image,
			string text
			){
			throw new NotImplementedException ();
		}

		public void Reverse(){
		}
	}
}