// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
namespace System.Windows.Forms.DataVisualization.Charting
{
	public class CursorEventArgs : EventArgs
	{
		public CursorEventArgs(	ChartArea chartArea,
		                       Axis axis,
		                       double newPosition)
		{
			this.ChartArea = chartArea;
			this.Axis = axis;
			this.NewPosition = newPosition;
		}

		public CursorEventArgs(ChartArea chartArea,
		                Axis axis,
		                double newSelectionStart,
		                double newSelectionEnd)
		{
			this.ChartArea = chartArea;
			this.Axis = axis;
			this.NewSelectionStart = newSelectionStart;
			this.NewSelectionEnd = newSelectionEnd;
		}

		public Axis Axis { get; private set; }

		public ChartArea ChartArea { get; private set; }

		public double NewPosition { get; set; }

		public double NewSelectionStart { get; set; }

		public double NewSelectionEnd { get; set; }
	}
}
