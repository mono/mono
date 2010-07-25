

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using awt = java.awt;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for PathGradientBrush.
	/// </summary>
	[MonoTODO]
	public sealed class PathGradientBrush : Brush {
		Brush _nativeObject;
		Blend _blend;
		Color _centerColor;
		PointF _center;
		PointF _focus;
		RectangleF _rectangle;
		Color [] _surroundColors;
		ColorBlend _interpolationColors;
		WrapMode _wrapMode;
		GraphicsPath _texturePath;
		bool _triangularShape = false;

		protected override java.awt.Paint NativeObject {
			get {
				return _nativeObject;
			}
		}

		#region initialize

		void Initialize(GraphicsPath path, WrapMode wrapMode, bool initColors, bool calcCenter) {
			
			_texturePath = path;
			_wrapMode = wrapMode;
			_rectangle = path.GetBounds();

			if (initColors) {
				_centerColor = Color.Black;
				_surroundColors = new Color []{ Color.White };
			}
			
			Bitmap texture = new Bitmap( (int)_rectangle.Width, (int)_rectangle.Height );
			Graphics g = Graphics.FromImage( texture );
			PointF [] pathPoints = path.PathPoints;

			if (calcCenter) {
				for (int i=0; i < pathPoints.Length; i++) {
					_center.X += pathPoints[i].X;
					_center.Y += pathPoints[i].Y;
				}
				_center.X /= pathPoints.Length;
				_center.Y /= pathPoints.Length;
			}

			int outerColor = 0;
			DrawSector( g, CenterPoint, pathPoints[pathPoints.Length-1], pathPoints[0], CenterColor, SurroundColors[outerColor] );
			for(int i=0; i < pathPoints.Length - 1; i++) {
				if (outerColor < SurroundColors.Length - 1)
					outerColor++;
				DrawSector( g, CenterPoint, pathPoints[i], pathPoints[i+1], CenterColor, SurroundColors[outerColor] );
			}

			_nativeObject = new TextureBrush( texture );
		}
		private void DrawSector(Graphics g, PointF center, PointF p1, PointF p2, Color innerColor, Color outerColor) {
			GraphicsPath pt = new GraphicsPath();
			pt.AddLine(p1, center);
			pt.AddLine(center, p2);
			LinearGradientBrush lgb = new LinearGradientBrush( GetVertical(center, p1, p2) , center, outerColor, innerColor );
			if (_triangularShape)
				lgb.SetBlendTriangularShape(0.5f);
			g.FillPath( lgb, pt );
		}
		private PointF GetVertical(PointF c, PointF p1, PointF p2) {
			if (p1.X == p2.X)
				return new PointF(p1.X, c.Y);
			if (p1.Y == p2.Y)
				return new PointF(c.X, p2.Y);

			float a = (float)(p2.Y - p1.Y) / (p2.X - p1.X);
			float av = - 1 / a;

			float b1 = p1.Y - a * p1.X;
			float b2 = c.Y - av * c.X;

			float ox = (b1 - b2) / (av - a);
			float oy = av * ox + b2;

			return new PointF(ox, oy);
		}

		#endregion

		#region ctors

		public PathGradientBrush (GraphicsPath path) {
			Initialize( path, WrapMode.Clamp, true, true );
		}

		public PathGradientBrush (Point [] points) : this (points, WrapMode.Clamp) {
		}

		public PathGradientBrush (PointF [] points) : this (points, WrapMode.Clamp) {
		}

		public PathGradientBrush (Point [] points, WrapMode wrapMode) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines( points );
			Initialize( path, wrapMode, true, true );
		}

		public PathGradientBrush (PointF [] points, WrapMode wrapMode) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines( points );
			Initialize( path, wrapMode, true, true );
		}

		#endregion

		#region Properties

		[MonoTODO]
		public Blend Blend {
			get {
				return _blend;
			}
			set {
				_blend = value;
			}
		}

		public Color CenterColor {
			get {
				return _centerColor;
			}
			set {
				_centerColor = value;
				Initialize(_texturePath, _wrapMode, false, false );
			}
		}

		public PointF CenterPoint {
			get {
				return _center;
			}
			set {
				_center = value;
				Initialize(_texturePath, _wrapMode, false, false );
			}
		}

		public PointF FocusScales {
			get {
				return _focus;
			}
			set {
				_focus = value;
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return _interpolationColors;
			}
			set {
				_interpolationColors = value;
			}
		}

		public RectangleF Rectangle {
			get {
				return _rectangle;
			}
		}

		public Color [] SurroundColors {
			get {
				return _surroundColors;
			}
			set {
				_surroundColors = value;
				Initialize(_texturePath, _wrapMode, false, false );
			}
		}

		public Matrix Transform {
			get {
				return BrushTransform;
			}
			set {
				BrushTransform = value;
			}
		}

		public WrapMode WrapMode {
			get {
				return _wrapMode;
			}
			set {
				_wrapMode = value;
			}
		}

		#endregion

		#region Methods

		public void MultiplyTransform (Matrix matrix) {
			base.BrushMultiplyTransform( matrix );
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order) {
			base.BrushMultiplyTransform( matrix, order );
		}

		public void ResetTransform () {
			base.BrushResetTransform();
		}

		public void RotateTransform (float angle) {
			base.BrushRotateTransform( angle );
		}

		public void RotateTransform (float angle, MatrixOrder order) {
			base.BrushRotateTransform( angle, order );
		}

		public void ScaleTransform (float sx, float sy) {
			base.BrushScaleTransform( sx, sy );
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order) {
			base.BrushScaleTransform( sx, sy, order );
		}

		public void SetBlendTriangularShape (float focus) {
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale) {
			_triangularShape = true;
			Initialize( _texturePath, _wrapMode, false, false );
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
			base.BrushTranslateTransform( dx, dy );
		}

		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			base.BrushTranslateTransform( dx, dy, order );
		}

		public override object Clone () {
			PathGradientBrush copy = (PathGradientBrush)InternalClone();

			if (copy._nativeObject != null)
				copy._nativeObject = (Brush)copy._nativeObject.Clone();
			
			if (copy._surroundColors != null)
				copy._surroundColors = (Color[])copy._surroundColors.Clone();
			
			if (copy._texturePath != null)
				copy._texturePath = (GraphicsPath)copy._texturePath.Clone();

			//TBD: clone _blend, _interpolationColors
			//copy._blend = copy._blend
			
			return copy;
		}

		#endregion
	}
}
