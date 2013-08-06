// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Border3DAnnotation : RectangleAnnotation
	{
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
		public BorderSkin BorderSkin { get; set; }
	}
}