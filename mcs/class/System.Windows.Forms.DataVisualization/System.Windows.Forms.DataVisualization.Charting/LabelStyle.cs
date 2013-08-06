// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class LabelStyle : ChartElement
	{
		public int Angle { get; set; }
		public bool Enabled { get; set; }
		public Font Font { get; set; }
		public Color ForeColor { get; set; }
		public string Format { get; set; }
		public double Interval { get; set; }
		public DateTimeIntervalType IntervalType { get; set; }
		public double IntervalOffset { get; set; }
		public DateTimeIntervalType IntervalOffsetType { get; set; }
		public bool IsEndLabelVisible { get; set; }
		public bool IsStaggered { get; set; }
		public bool TruncatedLabels { get; set; }
	}
}