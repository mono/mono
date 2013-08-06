// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
using System.Drawing;
namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CustomLabel : ChartNamedElement
	{
		public CustomLabel() {
		}
		public CustomLabel(
			double fromPosition,
			double toPosition,
			string text,
			int labelRow,
			LabelMarkStyle markStyle
			){
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Text = text;
			RowIndex = labelRow;
			LabelMark = markStyle;
		}
		public CustomLabel(
			double fromPosition,
			double toPosition,
			string text,
			int labelRow,
			LabelMarkStyle markStyle,
			GridTickTypes gridTick
			){
			FromPosition = fromPosition;
			ToPosition = toPosition;
			Text = text;
			RowIndex = labelRow;
			LabelMark = markStyle;
			GridTicks = gridTick;
		}

		public Axis Axis { get; private set; } 
		public Color ForeColor { get; set; }
		public double FromPosition { get; set; }
		public GridTickTypes GridTicks { get; set; }
		public string Image { get; set; }
		public Color ImageTransparentColor { get; set; }
		public LabelMarkStyle LabelMark { get; set; }
		public Color MarkColor { get; set; }
		public override string Name { get; set; }
		public int RowIndex { get; set; }
		public string Text { get; set; }
		public string ToolTip { get; set; }
		public double ToPosition { get; set; }

		public CustomLabel Clone(){
			throw new NotImplementedException ();
		}
	}
}