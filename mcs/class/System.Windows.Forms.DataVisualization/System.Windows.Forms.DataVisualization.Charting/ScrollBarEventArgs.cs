// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ScrollBarEventArgs : EventArgs
	{
		public Axis Axis { get; private set; }
		public ScrollBarButtonType ButtonType { get; private set;}
		public ChartArea ChartArea { get; private set;}
		public bool IsHandled { get; set; }
		public int MousePositionX { get; private set; }
		public int MousePositionY { get; private set; }
	}
}