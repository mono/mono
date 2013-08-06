// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

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