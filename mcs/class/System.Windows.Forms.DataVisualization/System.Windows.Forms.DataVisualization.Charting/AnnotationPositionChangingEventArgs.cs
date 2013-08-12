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

using System;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class AnnotationPositionChangingEventArgs : EventArgs
	{
		public AnnotationPositionChangingEventArgs ()
		{
		}

		public Annotation Annotation { get; set; }

		public PointF NewAnchorLocation { get; set; }
		public double NewAnchorLocationX { 
			get { return this.NewAnchorLocation.X; } 
			set { 
				PointF nal = this.NewAnchorLocation; 
				nal.X = (float)value;
				this.NewAnchorLocation = nal; 
			} 
		}
		public double NewAnchorLocationY { 
			get { return this.NewAnchorLocation.Y; } 
			set { 
				PointF nal = this.NewAnchorLocation; 
				nal.Y = (float)value;
				this.NewAnchorLocation = nal; 
			} 
		}

		public RectangleF NewPosition { get; set; }

		public double NewLocationX { 
			get { return this.NewPosition.X; } 
			set { 
				RectangleF np = this.NewPosition; 
				np.X = (float)value;
				this.NewPosition = np;
			}
		}
		public double NewLocationY { 
			get { return this.NewPosition.Y; } 
			set { 
				RectangleF np = this.NewPosition; 
				np.Y = (float)value; 
				this.NewPosition = np;
			} 
		}
		public double NewSizeWidth { 
			get { return this.NewPosition.Width; } 
			set { 
				RectangleF np = this.NewPosition; 
				np.Width = (float)value; 
				this.NewPosition = np;
			}
		}
		public double NewSizeHeight { 
			get { return this.NewPosition.Height; } 
			set { 
				RectangleF np = this.NewPosition; 
				np.Height = (float)value; 
				this.NewPosition = np;
			}
		}
	}
}
