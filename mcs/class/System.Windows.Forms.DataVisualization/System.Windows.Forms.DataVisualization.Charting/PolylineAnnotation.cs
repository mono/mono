// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class PolylineAnnotation : Annotation
	{
		public PolylineAnnotation(){
			GraphicsPathPoints = new AnnotationPathPointCollection ();
		}
		public override ContentAlignment Alignment { get; set; }
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
		public override Color BackColor { get; set; }
		public override GradientStyle BackGradientStyle { get; set; }
		public override ChartHatchStyle BackHatchStyle { get; set; }
		public override Color BackSecondaryColor { get; set; }
		public virtual LineAnchorCapStyle EndCap { get; set; }
		public override Font Font { get; set; }
		public override Color ForeColor { get; set; }
		public virtual GraphicsPath GraphicsPath { get; set; }
		public AnnotationPathPointCollection GraphicsPathPoints { get; private set; }
		public virtual bool IsFreeDrawPlacement { get; set; }
		public virtual LineAnchorCapStyle StartCap { get; set; }
		public override TextStyle TextStyle { get; set; }
	}
}