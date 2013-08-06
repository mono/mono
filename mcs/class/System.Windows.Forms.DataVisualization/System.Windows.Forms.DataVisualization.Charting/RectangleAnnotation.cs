// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class RectangleAnnotation : TextAnnotation
	{
		public override string AnnotationType { get { throw new NotImplementedException (); } } //FIXME - find out what MS implementation returns here
	}
}