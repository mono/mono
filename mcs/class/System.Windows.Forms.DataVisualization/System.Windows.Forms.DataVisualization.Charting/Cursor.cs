// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class Cursor : IDisposable
	{
		public Cursor(){}

		public bool AutoScroll { get; set; }
		public AxisType AxisType { get; set; }
		public double Interval { get; set; }
		public double IntervalOffset { get; set; }
		public DateTimeIntervalType IntervalOffsetType { get; set; }
		public DateTimeIntervalType IntervalType { get; set; }
		public bool IsUserEnabled { get; set; }
		public bool IsUserSelectionEnabled { get; set; }
		public Color LineColor { get; set; }
		public ChartDashStyle LineDashStyle { get; set; }
		public int LineWidth { get; set; }
		public double Position { get; set; }
		public Color SelectionColor { get; set; }
		public double SelectionEnd { get; set; }
		public double SelectionStart { get; set; }


		public void Dispose(){
			throw new NotImplementedException ();
		}
		protected virtual void Dispose (bool disposing){
			throw new NotImplementedException ();
		}
		public void SetCursorPixelPosition(PointF point,bool roundToBoundary){
			throw new NotImplementedException ();
		}
		public void SetCursorPosition(double newPosition){
			throw new NotImplementedException ();
		}
		public void SetSelectionPixelPosition(
			PointF startPoint,
			PointF endPoint,
			bool roundToBoundary
			){
			throw new NotImplementedException ();
		}
		public void SetSelectionPosition(
			double newStart,
			double newEnd
			){
			throw new NotImplementedException ();
		}
	}
}