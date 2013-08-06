// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class LegendCell : ChartNamedElement
	{
		public LegendCell(){
		}

		public LegendCell(
			string text
			){
		}
		public LegendCell(
			LegendCellType cellType,
			string text
			){
		}
		public LegendCell(
			LegendCellType cellType,
			string text,
			ContentAlignment alignment
			){
		}

		public virtual ContentAlignment Alignment { get; set; }
		public virtual Color BackColor { get; set; }
		public virtual int CellSpan { get; set; }
		public virtual LegendCellType CellType { get; set; }
		public virtual Font Font { get; set; }
		public virtual Color ForeColor { get; set; }
		public virtual string Image { get; set; }
		public virtual Size ImageSize { get; set; }
		public virtual Color ImageTransparentColor { get; set; }
		public virtual Legend Legend { get; private set;}
		public virtual LegendItem LegendItem { get; private set;}
		public virtual Margins Margins { get; set; }
		public override string Name { get; set; }
		public virtual Size SeriesSymbolSize { get; set; }
		public virtual string Text { get; set; }
		public virtual string ToolTip { get; set; }
	}
}