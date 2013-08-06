// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CustomizeLegendEventArgs : EventArgs
	{
		public LegendItemsCollection LegendItems { get; private set; }
		public string LegendName { get; private set; }
	}
}
