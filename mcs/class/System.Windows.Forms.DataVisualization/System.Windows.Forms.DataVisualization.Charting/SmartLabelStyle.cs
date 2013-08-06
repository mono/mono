// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class SmartLabelStyle
	{
		public virtual LabelOutsidePlotAreaStyle AllowOutsidePlotArea { get; set; }
		public virtual Color CalloutBackColor { get; set; }
		public virtual LineAnchorCapStyle CalloutLineAnchorCapStyle { get; set; }
		public virtual Color CalloutLineColor { get; set; }
		public virtual ChartDashStyle CalloutLineDashStyle { get; set; }
		public virtual int CalloutLineWidth { get; set; }
		public virtual LabelCalloutStyle CalloutStyle { get; set; }
		public virtual bool Enabled { get; set; }
		public virtual bool IsMarkerOverlappingAllowed { get; set; }
		public virtual bool IsOverlappedHidden { get; set; }
		public virtual double MaxMovingDistance { get; set; }
		public virtual double MinMovingDistance { get; set; }
		public virtual LabelAlignmentStyles MovingDirection { get; set; }
	}
}