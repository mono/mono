//
// System.Drawing.Drawing2D.LinearGradientBrush.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell, Inc. http://www.novell.com
//

using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for LinearGradientBrush.
	/// </summary>
	public sealed class LinearGradientBrush : Brush
	{
		Blend blend;
		Color [] linearColors;
		bool useGammaCorrection;
		RectangleF rectangle;
		ColorBlend interpolationColors;
		Matrix transform;
		WrapMode wrapMode;
		
		internal LinearGradientBrush (IntPtr native) : base (native)
		{
		}

		public LinearGradientBrush (Point point1, Point point2, Color color1, Color color2)
		{
			Status status = GDIPlus.GdipCreateLineBrushI (ref point1, ref point2, color1.ToArgb (), color2.ToArgb (), WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			Rectangle rect;
			status = GDIPlus.GdipGetLineRectI (nativeObject, out rect);
			GDIPlus.CheckStatus (status);
			rectangle = (RectangleF) rect;
		}

		public LinearGradientBrush (PointF point1, PointF point2, Color color1, Color color2)
		{
			Status status = GDIPlus.GdipCreateLineBrush (ref point1, ref point2, color1.ToArgb (), color2.ToArgb (), WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			status = GDIPlus.GdipGetLineRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
		{
			Status status = GDIPlus.GdipCreateLineBrushFromRectI (ref rect, color1.ToArgb (), color2.ToArgb (), linearGradientMode, WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			rectangle = (RectangleF) rect;
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle) : this (rect, color1, color2, angle, false)
		{
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
		{
			Status status = GDIPlus.GdipCreateLineBrushFromRect (ref rect, color1.ToArgb (), color2.ToArgb (), linearGradientMode, WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			rectangle = rect;
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle) : this (rect, color1, color2, angle, false)
		{
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
		{
			Status status = GDIPlus.GdipCreateLineBrushFromRectWithAngleI (ref rect, color1.ToArgb (), color2.ToArgb (), angle, isAngleScaleable, WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			rectangle = (RectangleF) rect;
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
		{
			Status status = GDIPlus.GdipCreateLineBrushFromRectWithAngle (ref rect, color1.ToArgb (), color2.ToArgb (), angle, isAngleScaleable, WrapMode.Tile, out nativeObject);
			GDIPlus.CheckStatus (status);

			rectangle = rect;
		}

		// Public Properties

		public Blend Blend {
			get {
				return blend;
			}
			set {
				Status status = GDIPlus.GdipSetLineBlend (nativeObject, value.Factors, value.Positions, value.Factors.Length);
				GDIPlus.CheckStatus (status);
				blend = value;
			}
		}

		public bool GammaCorrection {
			get {
				return useGammaCorrection;
			}
			set {
				Status status = GDIPlus.GdipSetLineGammaCorrection (nativeObject, value);
				GDIPlus.CheckStatus (status);
				useGammaCorrection = value;
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return interpolationColors;
			}
			set {
				Color [] colors = value.Colors;
				int [] blend = new int [colors.Length];
				for (int i = 0; i < colors.Length; i++)
					blend [i] = colors [i].ToArgb ();

				Status status = GDIPlus.GdipSetLinePresetBlend (nativeObject, blend, value.Positions, blend.Length);
				GDIPlus.CheckStatus (status);
				interpolationColors = value;
			}
		}

		public Color [] LinearColors {
			get {
				return linearColors;
			}
			set {
				Status status = GDIPlus.GdipSetLineColors (nativeObject, value [0].ToArgb (), value [1].ToArgb ());
				GDIPlus.CheckStatus (status);
				linearColors = value;
			}
		}

		public RectangleF Rectangle {
			get {
				return rectangle;
			}
		}

		public Matrix Transform {
			get {
				return transform;
			}
			set {
				Status status = GDIPlus.GdipSetLineTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
				transform = value;
			}
		}

		public WrapMode WrapMode {
			get {
				return wrapMode;
			}
			set {
				Status status = GDIPlus.GdipSetLineWrapMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
				wrapMode = value;
			}
		}

		// Public Methods

		[MonoTODO()]
		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void MultiplyTransform (Matrix matrix)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void ResetTransform ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void RotateTransform (float angle, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void RotateTransform (float angle)
		{
			throw new NotImplementedException ();
		}

	}
}
