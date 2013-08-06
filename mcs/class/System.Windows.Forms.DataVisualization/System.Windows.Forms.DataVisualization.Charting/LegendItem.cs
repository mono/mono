// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class LegendItem : ChartNamedElement
	{
		public LegendItem(){ 
			Cells = new LegendCellCollection ();
		}
		public LegendItem(string name,Color color,string image){
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