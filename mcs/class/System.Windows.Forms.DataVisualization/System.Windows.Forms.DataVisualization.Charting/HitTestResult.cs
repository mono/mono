// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class HitTestResult
	{
		public Axis Axis { get; set; }
		public ChartArea ChartArea { get; set; }
		public ChartElementType ChartElementType { get; set; }
		public int PointIndex { get; set; }
		public Series Series { get; set; }
		public Object Object { get; set; }
		public Object SubObject { get; set; }
	}
}
	
