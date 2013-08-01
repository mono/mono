// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class StripLine : ChartElement
	{
		public Color BackColor { get; set; }
		public GradientStyle BackGradientStyle { get; set; }
		public ChartHatchStyle BackHatchStyle { get; set; }
		public string BackImage { get; set; }
		public ChartImageAlignmentStyle BackImageAlignment { get; set; }
		public Color BackImageTransparentColor { get; set; }
		public ChartImageWrapMode BackImageWrapMode { get; set; }
		public Color BackSecondaryColor { get; set; }
		public Color BorderColor { get; set; }
		public ChartDashStyle BorderDashStyle { get; set; }
		public int BorderWidth { get; set; }
		public Font Font { get; set; }
		public Color ForeColor { get; set; }
		public double Interval { get; set; }
		public double IntervalOffset { get; set; }
		public DateTimeIntervalType IntervalOffsetType { get; set; }
		public DateTimeIntervalType IntervalType { get; set; }
		public string Name { get; private set; }
		public double StripWidth { get; set; }
		public DateTimeIntervalType StripWidthType { get; set; }
		public string Text { get; set; }
		public StringAlignment TextAlignment { get; set; }
		public StringAlignment TextLineAlignment { get; set; }
		public TextOrientation TextOrientation { get; set; }
		public string ToolTip { get; set; }
	}
}