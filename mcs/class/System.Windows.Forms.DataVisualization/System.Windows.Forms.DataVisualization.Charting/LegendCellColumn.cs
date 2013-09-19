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
	public class LegendCellColumn : ChartNamedElement
	{
		public virtual ContentAlignment Alignment { get; set; }
		public virtual Color BackColor { get; set; }
		public virtual LegendCellColumnType ColumnType { get; set; }
		public virtual Font Font { get; set; }
		public virtual Color ForeColor { get; set; }
		public StringAlignment HeaderAlignment { get; set; }
		public virtual Color HeaderBackColor { get; set; }
		public virtual Font HeaderFont { get; set; }
		public virtual Color HeaderForeColor { get; set; }
		public virtual string HeaderText { get; set; }
		public virtual Legend Legend { get; private set;}
		public virtual Margins Margins { get; set; }
		public virtual int MaximumWidth { get; set; }
		public virtual int MinimumWidth { get; set; }
		public override string Name { get; set; }
		public virtual Size SeriesSymbolSize { get; set; }
		public virtual string Text { get; set; }
		public virtual string ToolTip { get; set; }
	}
}
