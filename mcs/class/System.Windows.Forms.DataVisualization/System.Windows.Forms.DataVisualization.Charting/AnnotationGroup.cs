// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AnnotationGroup : Annotation
	{
		public AnnotationGroup ()
		{
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
