// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AnnotationGroup : Annotation
	{
		public AnnotationGroup(){
			Annotations = new AnnotationCollection ();
		}

		public override ContentAlignment Alignment { get; set; }
		public override bool AllowAnchorMoving { get; set; }
		public override bool AllowMoving { get; set; }
		public override bool AllowPathEditing { get; set; }
		public override bool AllowResizing { get; set; }
		public override bool AllowSelecting { get; set; }
		public override bool AllowTextEditing { get; set; }
		public AnnotationCollection Annotations { get; private set; }
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
		public override Color BackColor { get; set; }
		public override GradientStyle BackGradientStyle { get; set; }
		public override ChartHatchStyle BackHatchStyle { get; set; }
		public override Color BackSecondaryColor { get; set; }
		public override string ClipToChartArea { get; set; }
		public override Font Font { get; set; }
		public override Color ForeColor { get; set; }
		public override bool IsSelected { get; set; }
		public override bool IsSizeAlwaysRelative { get; set; }
		public override Color LineColor { get; set; }
		public override ChartDashStyle LineDashStyle { get; set; }
		public override int LineWidth { get; set; }
		public override Color ShadowColor { get; set; }
		public override int ShadowOffset { get; set; }
		public override TextStyle TextStyle { get; set; }
		public override bool Visible { get; set; }
	}
}