//
// System.Drawing.Drawing2D.LinearGradientBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for LinearGradientBrush.
	/// </summary>
	public sealed class LinearGradientBrush : Brush
	{
		private Color[] linearColors_;
		
		//Constructors.
		public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2) {
			linearColors_ = new Color[] { color1, color2 };
		}

		public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2) {
			linearColors_ = new Color[] { color1, color2 };
		}

		public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode) {
			linearColors_ = new Color[] { color1, color2 };
		}


		//public Properties
		public Blend Blend {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public bool GammaCorrection {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public ColorBlend InterpolationColors {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public Color [] LinearColors {
			get {
				return linearColors_;
			}
			set {
				linearColors_[0] = value[0];
				linearColors_[1] = value[1];
			}
		}
		public RectangleF Rectange {
			get {
				throw new NotImplementedException ();
			}
		}
		public Matrix Transform {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public WrapMode WrapMode {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		// Public Methods
		
		public override object Clone(){
			throw new NotImplementedException ();
		}

		public void MultiplyTransform(Matrix matrix){
			throw new NotImplementedException ();
		}

		public void MultiplyTransform(Matrix matrix, MatrixOrder order){
			throw new NotImplementedException ();
		}

		public void ResetTransform(){
			throw new NotImplementedException ();
		}

		public void RotateTransform(float angle, MatrixOrder order){
			throw new NotImplementedException ();
		}

		public void RotateTransform(float angle){
			throw new NotImplementedException ();
		}

	}
}
