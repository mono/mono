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
		RectangleF rectangle;
		
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
				int count;
				Status status = GDIPlus.GdipGetLineBlendCount (nativeObject, out count);
				GDIPlus.CheckStatus (status);
				float [] factors = new float [count];
				float [] positions = new float [count];
				status = GDIPlus.GdipGetLineBlend (nativeObject, factors, positions, count);
				GDIPlus.CheckStatus (status);

				Blend blend = new Blend ();
				blend.Factors = factors;
				blend.Positions = positions;

				return blend;
			}
			set {
				int count;
				float [] factors = value.Factors;
				float [] positions = value.Positions;
				count = factors.Length;

				if (count != positions.Length)
					throw new ArgumentException ();
				if (positions [0] != 0.0F)
					throw new ArgumentException ();
				if (positions [count - 1] != 1.0F)
					throw new ArgumentException ();

				Status status = GDIPlus.GdipSetLineBlend (nativeObject, factors, positions, count);
				GDIPlus.CheckStatus (status);
			}
		}

		public bool GammaCorrection {
			get {
				bool gammaCorrection;
				Status status = GDIPlus.GdipGetLineGammaCorrection (nativeObject, out gammaCorrection);
				GDIPlus.CheckStatus (status);
				return gammaCorrection;
			}
			set {
				Status status = GDIPlus.GdipSetLineGammaCorrection (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public ColorBlend InterpolationColors {
			get {
				int count;
				Status status = GDIPlus.GdipGetLinePresetBlendCount (nativeObject, out count);
				GDIPlus.CheckStatus (status);
				int [] intcolors = new int [count];
				float [] positions = new float [count];
				status = GDIPlus.GdipGetLinePresetBlend (nativeObject, intcolors, positions, count);
				GDIPlus.CheckStatus (status);

				ColorBlend interpolationColors = new ColorBlend ();
				Color [] colors = new Color [count];
				for (int i = 0; i < count; i++)
					colors [i] = Color.FromArgb (intcolors [i]);
				interpolationColors.Colors = colors;
				interpolationColors.Positions = positions;

				return interpolationColors;
			}
			set {
				int count;
				Color [] colors = value.Colors;
				float [] positions = value.Positions;
				count = colors.Length;

				if (count == 0 || positions.Length == 0)
					throw new ArgumentException ("An invalid ColorBlend object was set. There must be at least 2 elements in the colors and positions array. The ColorBlend object must be constructed with the same number of positions and color values. With a position starting at 0.0 and ranging to 1.0. 1.0 being the last element in the array.");

				if (count != positions.Length)
					throw new ArgumentException ("An invalid ColorBlend object was set. The colors and positions do not have the same number of elements. The ColorBlend object must be constructed with the same number of positions and color values. With a position starting at 0.0 and ranging to 1.0. 1.0 being the last element in the array.");

				if (positions [0] != 0.0F)
					throw new ArgumentException ("An invalid ColorBlend object was set. The position's first element must be equal to 0. The ColorBlend object must be constructed with the same number of positions and color values. With a position starting at 0.0 and ranging to 1.0. 1.0 being the last element in the array.");

				if (positions [count - 1] != 1.0F)
					throw new ArgumentException ("An invalid ColorBlend object was set. The position's last element must be equal to 1.0. The ColorBlend object must be constructed with the same number of positions and color values. With a position starting at 0.0 and ranging to 1.0. 1.0 being the last element in the array.");

				int [] blend = new int [colors.Length];
				for (int i = 0; i < colors.Length; i++)
					blend [i] = colors [i].ToArgb ();

				Status status = GDIPlus.GdipSetLinePresetBlend (nativeObject, blend, positions, count);
				GDIPlus.CheckStatus (status);
			}
		}

		public Color [] LinearColors {
			get {
				int [] colors = new int [2];
				Status status = GDIPlus.GdipGetLineColors (nativeObject, colors);
				GDIPlus.CheckStatus (status);
				Color [] linearColors = new Color [2];
				linearColors [0] = Color.FromArgb (colors [0]);
				linearColors [1] = Color.FromArgb (colors [1]);

				return linearColors;
			}
			set {
				Status status = GDIPlus.GdipSetLineColors (nativeObject, value [0].ToArgb (), value [1].ToArgb ());
				GDIPlus.CheckStatus (status);
			}
		}

		public RectangleF Rectangle {
			get {
				return rectangle;
			}
		}

		public Matrix Transform {
			get {
				IntPtr matrix;
				Status status = GDIPlus.GdipGetLineTransform (nativeObject, out matrix);
				GDIPlus.CheckStatus (status);

				return new Matrix (matrix);
			}
			set {
				Status status = GDIPlus.GdipSetLineTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
			}
		}

		public WrapMode WrapMode {
			get {
				WrapMode wrapMode;
				Status status = GDIPlus.GdipGetLineWrapMode (nativeObject, out wrapMode);
				GDIPlus.CheckStatus (status);

				return wrapMode;
			}
			set {
				Status status = GDIPlus.GdipSetLineWrapMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		// Public Methods

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Status status = GDIPlus.GdipMultiplyLineTransform (nativeObject, matrix.nativeMatrix, order);
			GDIPlus.CheckStatus (status);
		}

		public void ResetTransform ()
		{
			Status status = GDIPlus.GdipResetLineTransform (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			Status status = GDIPlus.GdipRotateLineTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipScaleLineTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
		}

		public void SetBlendTriangularShape (float focus)
		{
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale)
		{
			Status status = GDIPlus.GdipSetLineLinearBlend (nativeObject, focus, scale);
			GDIPlus.CheckStatus (status);
		}

		public void SetSigmaBellShape (float focus)
		{
			SetSigmaBellShape (focus, 1.0F);
		}

		public void SetSigmaBellShape (float focus, float scale)
		{
			Status status = GDIPlus.GdipSetLineSigmaBlend (nativeObject, focus, scale);
			GDIPlus.CheckStatus (status);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			Status status = GDIPlus.GdipTranslateLineTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
		}

		public override object Clone ()
		{
			IntPtr clonePtr;
			Status status = GDIPlus.GdipCloneBrush (nativeObject, out clonePtr);
			GDIPlus.CheckStatus (status);

			LinearGradientBrush clone = new LinearGradientBrush (clonePtr);
			return clone;
		}
	}
}
