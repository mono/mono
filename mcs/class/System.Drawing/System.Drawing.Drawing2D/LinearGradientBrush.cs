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
		private Color[] linear_colors;
		
		//Constructors.
		public LinearGradientBrush (Point point1, Point point2, Color color1, Color color2) {
			linear_colors = new Color[] { color1, color2 };
		}

		public LinearGradientBrush (PointF point1, PointF point2, Color color1, Color color2) {
			linear_colors = new Color[] { color1, color2 };
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode) {
			linear_colors = new Color[] { color1, color2 };
		}


		//public Properties
		[MonoTODO()]
		public Blend Blend {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		[MonoTODO()]
		public bool GammaCorrection {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		[MonoTODO()]
		public ColorBlend InterpolationColors {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public Color [] LinearColors {
			get {
				return linear_colors;
			}
			set {
				linear_colors[0] = value[0];
				linear_colors[1] = value[1];
			}
		}

		[MonoTODO()]
		public RectangleF Rectangle {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO()]
		public Matrix Transform {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		[MonoTODO()]
		public WrapMode WrapMode {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		// Public Methods

		[MonoTODO()]
		public override object Clone (){
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void MultiplyTransform (Matrix matrix){
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void MultiplyTransform (Matrix matrix, MatrixOrder order){
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void ResetTransform (){
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void RotateTransform (float angle, MatrixOrder order){
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void RotateTransform (float angle){
			throw new NotImplementedException ();
		}

	}
}
