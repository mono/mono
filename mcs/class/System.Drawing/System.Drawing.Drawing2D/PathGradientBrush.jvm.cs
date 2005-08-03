

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using awt = java.awt;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for PathGradientBrush.
	/// </summary>
	public sealed class PathGradientBrush : Brush
	{
		awt.GradientPaint _nativeObject;

		Blend blend;
		Color centerColor;
		PointF center;
		PointF focus;
		RectangleF rectangle;
		Color [] surroundColors;
		ColorBlend interpolationColors;
		Matrix transform;
		WrapMode wrapMode;

		protected override java.awt.Paint NativeObject {
			get {
				return _nativeObject;
			}
		}

		PathGradientBrush (awt.GradientPaint native)
		{
			_nativeObject = native;
		}

		public PathGradientBrush (GraphicsPath path)
		{
			throw new NotImplementedException();
		}

		public PathGradientBrush (Point [] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (PointF [] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (Point [] points, WrapMode wrapMode)
		{
			throw new NotImplementedException();
		}

		public PathGradientBrush (PointF [] points, WrapMode wrapMode)
		{
			throw new NotImplementedException();
		}

		// Properties

		public Blend Blend {
			get {
				return blend;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public Color CenterColor {
			get {
				return centerColor;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public PointF CenterPoint {
			get {
				return center;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public PointF FocusScales {
			get {
				return focus;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return interpolationColors;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public RectangleF Rectangle {
			get {
				return rectangle;
			}
		}

		public Color [] SurroundColors {
			get {
				return surroundColors;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public Matrix Transform {
			get {
				return transform;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public WrapMode WrapMode {
			get {
				return wrapMode;
			}
			set {
				throw new NotImplementedException();
			}
		}

		// Methods

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void ResetTransform ()
		{
			throw new NotImplementedException();
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public void SetBlendTriangularShape (float focus)
		{
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale)
		{
			throw new NotImplementedException();
		}

		public void SetSigmaBellShape (float focus)
		{
			SetSigmaBellShape (focus, 1.0F);
		}

		public void SetSigmaBellShape (float focus, float scale)
		{
			throw new NotImplementedException();
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			throw new NotImplementedException();
		}

		public override object Clone ()
		{
			throw new NotImplementedException();
		}
	}
}
