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
	public class LegendItem : ChartNamedElement
	{
		public LegendItem ()
		{
			Cells = new LegendCellCollection ();
		}
		public LegendItem (string name,Color color,string image)
		{
			this.Name = name;
			this.Color = color;
			this.Image = image;
			Cells = new LegendCellCollection ();
		}

		public GradientStyle BackGradientStyle { get; set; }
		public ChartHatchStyle BackHatchStyle { get; set; }
		public Color BackImageTransparentColor { get; set; }
		public Color BackSecondaryColor { get; set; }
		public Color BorderColor { get; set; }
		public ChartDashStyle BorderDashStyle { get; set; }
		public int BorderWidth { get; set; }
		public LegendCellCollection Cells { get; private set; }
		public Color Color { get; set; }
		public bool Enabled { get; set; }
		public string Image { get; set; }
		public LegendImageStyle ImageStyle { get; set; }
		public Legend Legend { get; private set;}
		public Color MarkerBorderColor { get; set; }
		public int MarkerBorderWidth { get; set; }
		public Color MarkerColor { get; set; }
		public string MarkerImage { get; set; }
		public Color MarkerImageTransparentColor { get; set; }
		public int MarkerSize { get; set; }
		public MarkerStyle MarkerStyle { get; set; }
		public override string Name { get; set; }
		public Color SeparatorColor { get; set; }
		public LegendSeparatorStyle SeparatorType { get; set; }
		public string SeriesName { get; set; }
		public int SeriesPointIndex { get; set; }
		public Color ShadowColor { get; set; }
		public int ShadowOffset { get; set; }
		public string ToolTip { get; set; }
	}
}
