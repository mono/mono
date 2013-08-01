// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ImageAnnotation : Annotation
	{
		public override ContentAlignment Alignment { get; set; }
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
		public override Color BackColor { get; set; }
		public override GradientStyle BackGradientStyle { get; set; }
		public override ChartHatchStyle BackHatchStyle { get; set; }
		public override Color BackSecondaryColor { get; set; }
		public override Font Font { get; set; }
		public override Color ForeColor { get; set; }
		public virtual string Image { get; set; }
		public Color ImageTransparentColor { get; set; }
		public ChartImageWrapMode ImageWrapMode { get; set; }
		public override Color LineColor { get; set; }
		public override ChartDashStyle LineDashStyle { get; set; }
		public override int LineWidth { get; set; }
		public override TextStyle TextStyle { get; set; }
	}
}