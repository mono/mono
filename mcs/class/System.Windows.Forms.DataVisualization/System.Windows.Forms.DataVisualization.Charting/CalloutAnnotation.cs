// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CalloutAnnotation : TextAnnotation
	{
		public override ContentAlignment AnchorAlignment { get; set; }
		public override double AnchorOffsetX { get; set; }
		public override double AnchorOffsetY { get; set; }
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
		public override Color BackColor { get; set; }
		public override GradientStyle BackGradientStyle { get; set; }
		public override ChartHatchStyle BackHatchStyle { get; set; }
		public override Color BackSecondaryColor { get; set; }
		public virtual LineAnchorCapStyle CalloutAnchorCap { get; set; }
		public virtual CalloutStyle CalloutStyle { get; set; }
		public override Color LineColor { get; set; }
		public override ChartDashStyle LineDashStyle { get; set; }
		public override int LineWidth { get; set; }
	}
}