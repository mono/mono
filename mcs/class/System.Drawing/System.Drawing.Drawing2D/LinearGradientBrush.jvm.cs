using System;
using java.awt;
using awt = java.awt;
using geom = java.awt.geom;
using image = java.awt.image;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for LinearGradientBrush.
	/// </summary>
	public sealed class LinearGradientBrush : Brush {
		Blend _blend;
		bool _gammaCorrection;
		ColorBlend _interpolationColors;
		WrapMode _wrapmode;
		RectangleF _gradientRectangle;

		// gradient brush data
		float _x1 = 0;
		float _y1 = 0;

		float _x2 = 0;
		float _y2 = 0;

		Color _color1, _color2;
		bool _cyclic;

		#region NativeObject

		GradientPaint _nativeObject = null;
		protected override Paint NativeObject {
			get {
				if ( _nativeObject == null )
					_nativeObject = new GradientPaint(
						_x1, _y1,
						new java.awt.Color(_color1.R, _color1.G, _color1.B, _color1.A),
						_x2, _y2, 
						new java.awt.Color(_color2.R, _color2.G, _color2.B, _color2.A), _cyclic);

				return _nativeObject;
			}
		}

		#endregion

		#region Initialization

		private void Init(float x1, float y1, Color color1, float x2, float y2, Color color2, bool cyclic) {
			_x1 = x1;
			_y1 = y1;
			_color1 = color1;

			_x2 = x2;
			_y2 = y2;
			_color2 = color2;

			_cyclic = cyclic;
			_nativeObject = null;
		}

		private void Init(float x1, float y1, Color color1, float x2, float y2, Color color2, float angle) {
			_gradientRectangle = new RectangleF(x1, y1, x2-x1, y2-y1);
			PointF [] points = GetMedianeEnclosingRect(x1, y1, x2, y2, angle);
			Init(points[0].X, points[0].Y, color1, points[1].X, points[1].Y, color2, false);
		}

		private void Init(float x1, float y1, Color color1, float x2, float y2, Color color2, LinearGradientMode linearGradientMode) {
			_gradientRectangle = new RectangleF(x1, y1, x2-x1, y2-y1);
			PointF [] points;

			switch (linearGradientMode) {
				case LinearGradientMode.Horizontal :
					Init(x1, y1, color1, x2, y1, color2, false);
					break;

				case LinearGradientMode.Vertical :
					Init(x1, y1, color1, x1, y2, color2, false);
					break;

				case LinearGradientMode.BackwardDiagonal :
					points = GetMedianeEnclosingRect(x1, y1, x2, y2, false);
					Init(points[0].X, points[0].Y, color2, points[1].X, points[1].Y, color1, false);
					break;

				case LinearGradientMode.ForwardDiagonal :
					points = GetMedianeEnclosingRect(x1, y1, x2, y2, true);
					Init(points[0].X, points[0].Y, color1, points[1].X, points[1].Y, color2, false);
					break;

				default :
					throw new ArgumentException("LinearGradientMode");
			}
		}

		#endregion

		#region Constructors

		private LinearGradientBrush (float x1, float y1, Color color1, float x2, float y2, Color color2, bool cyclic) {
			Init(x1, y1, color1, x2, y2, color2, cyclic);
		}

		internal LinearGradientBrush (float x1, float y1, Color color1, float x2, float y2, Color color2, LinearGradientMode mode) {
			Init(x1, y1, color1, x2, y2, color2, mode);
		}
		internal LinearGradientBrush (float x1, float y1, Color color1, float x2, float y2, Color color2) {
			Init(x1, y2, color1, x1, y2, color2, false);
		}
		public LinearGradientBrush (Point point1, Point point2, Color color1, Color color2) {
			_gradientRectangle = new RectangleF(point1.X, point1.Y, point2.X - point1.X, point2.Y - point2.Y);
			Init(point1.X, point1.Y, color1, point2.X, point2.Y, color2, false);
		}
		public LinearGradientBrush (PointF point1, PointF point2, Color color1, Color color2) {	
			_gradientRectangle = new RectangleF(point1.X, point1.Y, point2.X - point1.X, point2.Y - point2.Y);
			Init(point1.X, point1.Y, color1, point2.X, point2.Y, color2, false);
		}
		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode) {
			Init(rect.X, rect.Y, color1, rect.X + rect.Width, rect.Y + rect.Height, color2, linearGradientMode);
		}
		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode) {	
			Init(rect.X, rect.Y, color1, rect.X + rect.Width, rect.Y + rect.Height, color2, linearGradientMode);
		}
		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle) {
			Init(rect.X, rect.Y, color1, rect.X + rect.Width, rect.Y + rect.Height, color2, angle);
		}
		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle) {
			Init(rect.X, rect.Y, color1, rect.X + rect.Width, rect.Y + rect.Height, color2, angle);
		}
		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable):
			this(rect, color1, color2, angle) {
		}
		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable):
			this(rect, color1, color2, angle) {
		}
		#endregion

		#region GetMedianeEnclosingRect

		internal PointF [] GetMedianeEnclosingRect(float x1, float y1, float x2, float y2, float rotateAngle) {
			float width = x2 - x1;
			float height = y2 - y1;
			PointF rectCenter = new PointF(x1 + width/2, y1 + height/2);
			float gradLen = width * Math.Abs((float)Math.Cos(rotateAngle * Math.PI / 180)) + 
				height * Math.Abs((float)Math.Sin(rotateAngle * Math.PI / 180));

			PointF [] points = new PointF []{	new PointF(rectCenter.X - gradLen/2, rectCenter.Y),
												new PointF(rectCenter.X + gradLen/2, rectCenter.Y) };

			Matrix mx = new Matrix();
			mx.RotateAt((float)rotateAngle, rectCenter);
			mx.TransformPoints(points);
			return points;
		}
		internal PointF [] GetMedianeEnclosingRect(float x1, float y1, float x2, float y2, bool forwardDiagonal) {
			float width = x2 - x1;
			float height = y2 - y1;
			PointF rectCenter = new PointF(x1 + width/2, y1 + height/2);
			float rotateAngle = (float)Math.Atan2(width, height);
			float gradLen = width * (float)Math.Cos(rotateAngle);

			if (!forwardDiagonal)
				rotateAngle = -rotateAngle;
			
			PointF [] points = new PointF []{	new PointF(rectCenter.X - gradLen, rectCenter.Y),
												new PointF(rectCenter.X + gradLen, rectCenter.Y) };

			Matrix mx = new Matrix();
			mx.RotateAt((float)rotateAngle * (float)(180/Math.PI), rectCenter);
			mx.TransformPoints(points);
			return points;
		}

		#endregion

		#region Public Properties

		// FALLBACK: no functionality implemented for this property
		[MonoTODO]
		public Blend Blend {
			get {
				return _blend;
			}
			set {
				_blend = value;
			}
		}

		// FALLBACK: no functionality implemented for this property
		[MonoTODO]
		public bool GammaCorrection {
			get {
				return _gammaCorrection;
			}
			set {
				_gammaCorrection = value;
			}
		}

		// FALLBACK: functionality of two color gradient is implemented
		[MonoTODO]
		public ColorBlend InterpolationColors {
			get {
				return _interpolationColors;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("ColorBlend");

				if ( (value.Colors == null) || (value.Colors.Length == 0) )
					throw new ArgumentException("ColorBlend");

				_interpolationColors = value;

				_color1 = value.Colors[0];
				_color2 = value.Colors[value.Colors.Length - 1];
				_nativeObject = null;
			}
		}

		public Color [] LinearColors {
			get {
				Color [] cl = new Color[2];
				cl[0] = _color1;
				cl[1] = _color2;
				return cl;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("colors");

				_color1 = value[0];
				_color2 = value[1];
				_nativeObject = null;
			}
		}

		public RectangleF Rectangle {
			get {
				return _gradientRectangle;
			}
		}

		public Matrix Transform {
			get { return BrushTransform; }
			set { BrushTransform = value; }
		}

		// FALLBACK: not functionality implemented for this property
		[MonoTODO]
		public WrapMode WrapMode {
			get {
				return _wrapmode;
			}
			set {
				_wrapmode = value;
			}
		}
		#endregion

		#region Public Methods

		public void MultiplyTransform (Matrix matrix) {
			BrushMultiplyTransform(matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order) {
			BrushMultiplyTransform(matrix, order);			
		}

		public void ResetTransform () {
			BrushResetTransform();
		}

		public void RotateTransform (float angle) {
			BrushRotateTransform(angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order) {
			BrushRotateTransform(angle, order);
		}

		public void ScaleTransform (float sx, float sy) {
			BrushScaleTransform(sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order) {
			BrushScaleTransform(sx, sy, order);
		}

		public void SetBlendTriangularShape (float focus) {
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale) {
			_x2 = (_x1 + _x2) / 2;
			_y2 = (_y1 + _y2) / 2;
			_cyclic = true;
			_nativeObject = null;
		}

		public void SetSigmaBellShape (float focus) {
			SetSigmaBellShape (focus, 1.0F);
		}

		[MonoTODO]
		public void SetSigmaBellShape (float focus, float scale) {
			// FALLBACK: Triangle shape used
			SetBlendTriangularShape (focus, scale);
		}

		public void TranslateTransform (float dx, float dy) {
			BrushTranslateTransform (dx, dy);
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			BrushTranslateTransform(dx, dy, order);
		}

		public override object Clone () {
			LinearGradientBrush copy = (LinearGradientBrush)InternalClone();
			
			if (copy._nativeObject != null)
				copy._nativeObject = null;

			if (_interpolationColors != null) {
				copy._interpolationColors = new ColorBlend();
				if (_interpolationColors.Colors != null)
					copy._interpolationColors.Colors = (Color[])_interpolationColors.Colors.Clone();
				if (_interpolationColors.Positions != null)
					copy._interpolationColors.Positions = (float[])_interpolationColors.Positions.Clone();
			}

			if (_blend != null) {
				copy._blend = new Blend();
				if (_blend.Factors != null)
					copy._blend.Factors = (float[])_blend.Factors.Clone();
				if (_blend.Positions != null)
					copy._blend.Positions = (float[])_blend.Positions.Clone();
			}

			copy.LinearColors = (Color[])LinearColors.Clone();
			return copy;
		}
		#endregion
	}
}
