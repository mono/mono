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
	public class Title : ChartNamedElement, IDisposable
	{
		public Title ()
		{
	       	}
		public Title (string text)
		{ 
			this.Text = text; 
		}
		public Title (string text, Docking docking)
		{
			this.Text = text;
			this.Docking = docking;
		}
		public Title (string text,
				Docking docking,
				Font font,
				Color color)
		{
			this.Text = text;
			this.Docking = docking;
			this.Font = font;
			this.ForeColor = color;
		}

		public ContentAlignment Alignment { get; set; }
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
		public string DockedToChartArea { get; set; }
		public Docking Docking { get; set; }
		public int DockingOffset { get; set; }
		public Font Font { get; set; }
		public Color ForeColor { get; set; }
		public bool IsDockedInsideChartArea { get; set; }
		public override string Name { get; set; }
		public ElementPosition Position { get; set; }
		public Color ShadowColor { get; set; }
		public int ShadowOffset { get; set; }
		public string Text { get; set; }
		public TextOrientation TextOrientation { get; set; }
		public TextStyle TextStyle { get; set; }
		public string ToolTip { get; set; }
		public virtual bool Visible { get; set; }

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
	}
}
