//
// System.Drawing.Drawing2D.PathGradientBrush.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc. http://www.ximian.com
// (C) 2004, Novell, Inc. http://www.novell.com
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for PathGradientBrush.
	/// </summary>
	public sealed class PathGradientBrush : Brush
	{
		Blend blend;
		Color centerColor;
		PointF center;
		PointF focus;
		RectangleF rectangle;
		Color[] surroundColors;
		ColorBlend interpolationColors;
		Matrix transform;
		WrapMode wrapMode;
		
		internal PathGradientBrush (IntPtr native) : base (native)
		{
		}

		public PathGradientBrush (GraphicsPath path)
		{
			Status status = GDIPlus.GdipCreatePathGradientFromPath (path.NativeObject, out nativeObject);
			GDIPlus.CheckStatus (status);
 
			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
		}

		public PathGradientBrush (Point[] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (PointF[] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (Point[] points, WrapMode wrapMode)
		{
			Status status = GDIPlus.GdipCreatePathGradientI (points, points.Length, wrapMode, out nativeObject);
			GDIPlus.CheckStatus (status);

			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
		}

		public PathGradientBrush (PointF[] points, WrapMode wrapMode)
		{
			Status status = GDIPlus.GdipCreatePathGradient (points, points.Length, wrapMode, out nativeObject);
			GDIPlus.CheckStatus (status);

			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
		}

		// Properties

		public Blend Blend {
			get {
				return blend;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientBlend (nativeObject, value.Factors, value.Positions, value.Factors.Length);
				GDIPlus.CheckStatus (status);
				blend = value;
			}
		}

		public Color CenterColor {
			get {
				return centerColor;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientCenterColor (nativeObject, value.ToArgb ());
				GDIPlus.CheckStatus (status);
				centerColor = value;
			}
		}

		public PointF CenterPoint {
			get {
				return center;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientCenterPoint (nativeObject, ref value);
				GDIPlus.CheckStatus (status);
				center = value;
			}
		}

		public PointF FocusScales {
			get {
				return focus;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientFocusScales (nativeObject, value.X, value.Y);
				GDIPlus.CheckStatus (status);
				focus = value;
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return interpolationColors;
			}
			set {
				Color[] colors = value.Colors;
				int[] blend = new int [colors.Length];
				for (int i = 0; i < colors.Length; i++)
					blend [i] = colors [i].ToArgb ();

				Status status = GDIPlus.GdipSetPathGradientPresetBlend (nativeObject, blend, value.Positions, blend.Length);
				GDIPlus.CheckStatus (status);
				interpolationColors = value;
			}
		}

		public RectangleF Rectangle {
			get {
				
				return rectangle;
			}
		}

		public Color[] SurroundColors {
			get {
				return surroundColors;
			}
			set {
				int length = value.Length;
				int[] colors = new int [length];
				for (int i = 0; i < length; i++)
					colors [i] = value [i].ToArgb ();

				Status status = GDIPlus.GdipSetPathGradientSurroundColorsWithCount (nativeObject, colors, ref length);
				GDIPlus.CheckStatus (status);
				surroundColors = value;
			}
		}

		public Matrix Transform {
			get {
				return transform;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
				transform = value;
			}
		}

		public WrapMode WrapMode {
			get {
				return wrapMode;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientWrapMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
				wrapMode = value;
			}
		}

		// Methods

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Status status = GDIPlus.GdipMultiplyPathGradientTransform (nativeObject, matrix.nativeMatrix, order);
			GDIPlus.CheckStatus (status);
		}

		public void ResetTransform ()
		{
			Status status = GDIPlus.GdipResetPathGradientTransform (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			Status status = GDIPlus.GdipRotatePathGradientTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipScalePathGradientTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
		}

		public void SetBlendTriangularShape (float focus)
		{
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale)
		{
			Status status = GDIPlus.GdipSetPathGradientLinearBlend (nativeObject, focus, scale);
			GDIPlus.CheckStatus (status);
		}

		public void SetSigmaBellShape (float focus)
		{
			SetSigmaBellShape (focus, 1.0F);
		}

		public void SetSigmaBellShape (float focus, float scale)
		{
			Status status = GDIPlus.GdipSetPathGradientSigmaBlend (nativeObject, focus, scale);
			GDIPlus.CheckStatus (status);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipTranslatePathGradientTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
		}

		public override object Clone ()
		{
			IntPtr clonePtr;
			Status status = GDIPlus.GdipCloneBrush (nativeObject, out clonePtr);
			GDIPlus.CheckStatus (status);

			PathGradientBrush clone = new PathGradientBrush (clonePtr);
			return clone;
		}
	}
}
