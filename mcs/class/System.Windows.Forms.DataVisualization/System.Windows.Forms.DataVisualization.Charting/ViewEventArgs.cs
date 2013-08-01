// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ViewEventArgs : EventArgs
	{
		public Axis Axis { get; private set;}
		public ChartArea ChartArea { get; private set;}
		public double NewPosition { get; set; }
		public double NewSize { get; set; }
		public DateTimeIntervalType NewSizeType { get; set; }
	}
}