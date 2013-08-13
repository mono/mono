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
	public class Legend : ChartNamedElement
	{
		public Legend ()
		{
		}
		public Legend (string name)
		{
			Name = name;
		}

		public StringAlignment Alignment { get; set; }
		public int AutoFitMinFontSize { get; set; }
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
		public LegendCellColumnCollection CellColumns { get; private set; }
		public LegendItemsCollection CustomItems { get; private set; }
		public string DockedToChartArea { get; set; }
		public Docking Docking { get; set; }
		public bool Enabled { get; set; }
		public Font Font { get; set; }
		public Color ForeColor { get; set; }
		public LegendSeparatorStyle HeaderSeparator { get; set; }
		public Color HeaderSeparatorColor { get; set; }
		public string InsideChartArea { get; set; }
		public bool InterlacedRows { get; set; }
		public Color InterlacedRowsColor { get; set; }
		public bool IsDockedInsideChartArea { get; set; }
		public bool IsEquallySpacedItems { get; set; }
		public bool IsTextAutoFit { get; set; }
		public LegendSeparatorStyle ItemColumnSeparator { get; set; }
		public Color ItemColumnSeparatorColor { get; set; }
		public int ItemColumnSpacing { get; set; }
		public LegendItemOrder LegendItemOrder { get; set; }
		public LegendStyle LegendStyle { get; set; }
		public float MaximumAutoSize { get; set; }
		public override string Name { get; set; }
		public ElementPosition Position { get; set; }
		public Color ShadowColor { get; set; }
		public int ShadowOffset { get; set; }
		public LegendTableStyle TableStyle { get; set; }
		public int TextWrapThreshold { get; set; }
		public string Title { get; set; }
		public StringAlignment TitleAlignment { get; set; }
		public Color TitleBackColor { get; set; }
		public Font TitleFont { get; set; }
		public Color TitleForeColor { get; set; }
		public LegendSeparatorStyle TitleSeparator { get; set; }
		public Color TitleSeparatorColor { get; set; }

		[MonoTODO]
		protected override void Dispose(bool disposing){
			throw new NotImplementedException ();
		}
	}
}
