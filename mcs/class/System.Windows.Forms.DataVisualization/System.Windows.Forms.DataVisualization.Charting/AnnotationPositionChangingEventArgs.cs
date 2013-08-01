// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

using System;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AnnotationPositionChangingEventArgs : EventArgs
	{
		public AnnotationPositionChangingEventArgs() {}

		public Annotation Annotation {get;set;}

		public PointF NewAnchorLocation { get; set;}
		public double NewAnchorLocationX { 
			get { return this.NewAnchorLocation.X; } 
			set { PointF nal = this.NewAnchorLocation; nal.X = (float)value; this.NewAnchorLocation = nal; } }
		public double NewAnchorLocationY { 
			get { return this.NewAnchorLocation.Y; } 
			set { PointF nal = this.NewAnchorLocation; nal.Y = (float)value; this.NewAnchorLocation = nal; } }

		public RectangleF NewPosition { get; set; }

		public double NewLocationX { 
			get { return this.NewPosition.X;} 
			set{ RectangleF np = this.NewPosition; np.X = (float)value; this.NewPosition = np;} }
		public double NewLocationY { 
			get { return this.NewPosition.Y;} 
			set{ RectangleF np = this.NewPosition; np.Y = (float)value; this.NewPosition = np;} }
		public double NewSizeWidth { 
			get { return this.NewPosition.Width;} 
			set{ RectangleF np = this.NewPosition; np.Width = (float)value; this.NewPosition = np;} }
		public double NewSizeHeight { 
			get { return this.NewPosition.Height;} 
			set{ RectangleF np = this.NewPosition; np.Height = (float)value; this.NewPosition = np;} }
	}
}
