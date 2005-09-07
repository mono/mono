using System;
using java.awt;
using geom = java.awt.geom;

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

		GradientPaint _nativeObject;
		protected override Paint NativeObject {
			get {
				return _nativeObject;
			}
		}

		#region Initialization

		internal void Init(float x1, float y1, Color color1, float x2, float y2, Color color2, bool cyclic) {
			_nativeObject = new GradientPaint(
				x1, y1,
				new java.awt.Color(color1.R,color1.G,color1.B,color1.A),
				x2, y2,
				new java.awt.Color(color2.R,color2.G,color2.B,color2.A), cyclic);
		}

		internal void Init(float x1, float y1, Color color1, float x2, float y2, Color color2) {
			Init(x1, y1, color1, x2, y2, color2, false);
		}
		
		internal void Init(float x1, float y1, Color color1, float x2, float y2, Color color2, float angle) {
			_gradientRectangle = new RectangleF(x1, y1, x2-x1, y2-y1);
			PointF [] points = GetMedianeEnclosingRect(x1, y1, x2, y2, angle);
			Init(points[0].X, points[0].Y, color1, points[1].X, points[1].Y, color2);
		}
		internal void Init(float x1,float y1, Color color1,float x2, float y2, Color color2, LinearGradientMode linearGradientMode) {
			_gradientRectangle = new RectangleF(x1, y1, x2-x1, y2-y1);

			if (linearGradientMode == LinearGradientMode.Horizontal) {
				Init(x1, y1, color1, x2, y1, color2);
			}
			if (linearGradientMode == LinearGradientMode.Vertical) {
				Init(x1, y1, color1, x1, y2, color2);
			}
			if (linearGradientMode == LinearGradientMode.BackwardDiagonal) {
				PointF [] points = GetMedianeEnclosingRect(x1, y1, x2, y2, false);
				Init(points[0].X, points[0].Y, color2, points[1].X, points[1].Y, color1);
			}
			if (linearGradientMode == LinearGradientMode.ForwardDiagonal) {
				PointF [] points = GetMedianeEnclosingRect(x1, y1, x2, y2, true);
				Init(points[0].X, points[0].Y, color1, points[1].X, points[1].Y, color2);
			}
		}

		#endregion

		#region Constructors

		internal LinearGradientBrush (geom.Point2D p1, java.awt.Color color1, geom.Point2D p2, java.awt.Color color2, bool cyclic) {
			_nativeObject = new GradientPaint(p1, color1, p2, color2, cyclic);
		}
		internal LinearGradientBrush (float x1,float y1, Color color1,float x2, float y2, Color color2, LinearGradientMode mode) {
			Init(x1, y1, color1, x2, y2, color2, mode);
		}
		internal LinearGradientBrush (float x1, float y1, Color color1, float x2, float y2, Color color2) {
			Init(x1, y2, color1, x1, y2, color2);
		}
		public LinearGradientBrush (Point point1, Point point2, Color color1, Color color2) {
			_gradientRectangle = new RectangleF(point1.X, point1.Y, point2.X - point1.X, point2.Y - point2.Y);
			Init(point1.X, point1.Y, color1, point2.X, point2.Y, color2);
		}
		public LinearGradientBrush (PointF point1, PointF point2, Color color1, Color color2) {	
			_gradientRectangle = new RectangleF(point1.X, point1.Y, point2.X - point1.X, point2.Y - point2.Y);
			Init(point1.X, point1.Y, color1, point2.X, point2.Y, color2);
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
			float gradLen = width * ((float)Math.Cos(rotateAngle * Math.PI / 180)) + 
				height * ((float)Math.Sin(rotateAngle * Math.PI / 180));

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
		public Blend Blend {
			get {
				return _blend;
			}
			set {
				_blend = value;
			}
		}

		// FALLBACK: no functionality implemented for this property
		public bool GammaCorrection {
			get {
				return _gammaCorrection;
			}
			set {
				_gammaCorrection = value;
			}
		}

		// FALLBACK: no functionality implemented for this property
		public ColorBlend InterpolationColors {
			get {
				return _interpolationColors;
			}
			set {
				_interpolationColors = value;
			}
		}

		public Color [] LinearColors {
			get {
				Color [] cl = new Color[2];
				java.awt.Color c1 = ((GradientPaint)NativeObject).getColor1();
				java.awt.Color c2 = ((GradientPaint)NativeObject).getColor2();
				cl[0] = Color.FromArgb(c1.getAlpha(),c1.getRed(),c1.getGreen(),c1.getBlue());
				cl[1] = Color.FromArgb(c2.getAlpha(),c2.getRed(),c2.getGreen(),c2.getBlue());
				return cl;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("colors");

				geom.Point2D p1 = ((GradientPaint)NativeObject).getPoint1();
				geom.Point2D p2 = ((GradientPaint)NativeObject).getPoint2();
				Init(
					(float)p1.getX(), (float)p1.getY(), value[0], 
					(float)p2.getX(), (float)p2.getY(), value[1]);
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
			Color [] cl = LinearColors;
			geom.Point2D p1 = ((GradientPaint)NativeObject).getPoint1();
			geom.Point2D p2 = ((GradientPaint)NativeObject).getPoint2();
			geom.Point2D.Double po = new geom.Point2D.Double(
				(p2.getX() + p1.getX()) / 2,
				(p2.getY() + p1.getY()) / 2);
			Init(
				(float)p1.getX(), (float)p1.getY(), cl[0], 
				(float)po.getX(), (float)po.getY(), cl[1], true);
		}

		public void SetSigmaBellShape (float focus) {
			SetSigmaBellShape (focus, 1.0F);
		}

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
			LinearGradientBrush b = new LinearGradientBrush(
				((GradientPaint)NativeObject).getPoint1(),
				((GradientPaint)NativeObject).getColor1(),
				((GradientPaint)NativeObject).getPoint2(),
				((GradientPaint)NativeObject).getColor2(),
				((GradientPaint)NativeObject).isCyclic());
			b.Transform = this.Transform;
			b.Blend = this.Blend;
			return b;
		}
		#endregion
	}
}
