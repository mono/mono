// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AxisScrollBar : IDisposable
	{
		public Axis Axis { get; private set; }
		public Color BackColor { get; set; }
		public Color ButtonColor { get; set; }
		public ScrollBarButtonStyles ButtonStyle { get; set; }
		public ChartArea ChartArea { get; private set; } 
		public bool Enabled { get; set; }
		public bool IsPositionedInside { get; set; }
		public bool IsVisible { get; private set; } 
		public Color LineColor { get; set; }
		public double Size { get; set; }

		public void Dispose()
		{
			throw new NotImplementedException ();
		}

		protected virtual void Dispose(	bool disposing )
		{
			throw new NotImplementedException ();
		}
	}
}

