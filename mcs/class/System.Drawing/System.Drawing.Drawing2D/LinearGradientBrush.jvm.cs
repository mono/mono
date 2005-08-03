
using System;
using java.awt;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for LinearGradientBrush.
	/// </summary>
	public sealed class LinearGradientBrush : Brush
	{
		//TODO:this 3 entries nowbody need
		GradientPaint _nativeObject;
		protected override Paint NativeObject {
			get {
				return _nativeObject;
			}
		}

		//
		Matrix _transform;
		WrapMode _wrapmode;
		ColorBlend _colorblend;
		Blend _blend;
		float _focus = 0f;
		float _scale = 0f;
		float x1,y1,x2,y2;
		Matrix _internalTransform;
		LinearGradientMode _linMode = LinearGradientMode.Horizontal;
#if DEBUG_GRADIENT_BRUSH
		public PointF [] dPoints;
#endif
		

		/*
		internal LinearGradientBrush (GradientPaint native) : base (native)
		{
			_transform = new Matrix();
			_wrapmode = WrapMode.Clamp;
		}
		*/
		internal LinearGradientBrush (float x1,float y1, Color color1,float x2, float y2, Color color2,float angle)
		{
			this.x1=x1;
			this.x2=x2;
			this.y1=y1;
			this.y2=y2;
			_internalTransform = new Matrix();				
			_internalTransform.RotateAt(-angle,new PointF(x2-x1,y2-y1));
			PointF [] pts  = new PointF[2];
			//default to gorizontal vector
			pts[0] = new PointF(x1,y2-y1);
			pts[1] = new PointF(x2,y2-y1);
			_internalTransform.TransformPoints(pts);
			_nativeObject = new GradientPaint( pts[0].X,pts[0].Y,
				new java.awt.Color(color1.R,color1.G,color1.B,color1.A),
				pts[1].X,pts[1].Y,
				new java.awt.Color(color2.R,color2.G,color2.B,color2.A));

		}
		internal PointF [] GetMedianeEnclosingRect(float x1,float y1,float x2,float y2)
		{
			//get hypotenuse length 
			double hipLength = Math.Sqrt(Math.Pow(y2-y1,2)+Math.Pow(x2-x1,2));
			//mark linear point on the rectangle left edge: we will rotate 
			//it from there ...
			double yy = y1+hipLength;
			//... by this angle
			double rotationAngle = Math.Atan2(x2-x1,y2-y1);
			float  xxSemiWidth = (float)((x2-x1)*Math.Cos(rotationAngle));
			PointF rotationPoint=new PointF(x1,y1);
			Matrix mat  = new Matrix();
			//here the magic
			mat.RotateAt(-(float)(rotationAngle/Math.PI*180),rotationPoint);
			PointF []points = new PointF[]{	new PointF(x1-xxSemiWidth,y1),
											new PointF(x1+xxSemiWidth,y1),
											new PointF(x1+xxSemiWidth,(float)yy),
											new PointF(x1-xxSemiWidth,(float)yy)
										  };
			mat.TransformPoints(points);
			//now we have original rectangle withing another (enclosing)
			//rect that is reclined to left
			//the central points of new rectangle sides are exactly
			//the starting points of gradient in BackwardDiagonal
			//case. For ForwardDiagonal you can use the same points with
			//"y" or "x" coordinates swapped
#if DEBUG_GRADIENT_BRUSH
			dPoints = points;
#endif
			return points;
		}
		internal PointF[] GetDiagStartStopCoords(PointF []points)
		{

			return new PointF[]{
								   new PointF((points[3].X-points[0].X)/2+points[0].X,
								   (points[2].Y-points[1].Y)/2+points[1].Y),
								   new PointF((points[2].X-points[1].X)/2+points[1].X,
								   (points[3].Y-points[0].Y)/2+points[0].Y)};
		}

		internal void Init(float x1,float y1, Color color1,float x2, float y2, Color color2, LinearGradientMode mode, bool cycle)
		{

			float xx1;
			float yy1;
			float xx2;
			float yy2;				

			if(mode == LinearGradientMode.Vertical)
			{
				xx1 = x2-x1;
				yy1 = y1;
				xx2 = x2-x1;
				yy2 = y2;
			}
			else if(mode == LinearGradientMode.Horizontal)
			{
				xx1 = x1;
				yy1 = y2-y1;
				xx2 = x2;
				yy2 = y2-y1;				
			}
			else if(mode == LinearGradientMode.ForwardDiagonal)
			{
				if(y2-y1 != x2-x1)
				{
					PointF [] pts = GetDiagStartStopCoords(GetMedianeEnclosingRect(x1,y1,x2,y2));
					xx1 = pts[0].X;
					yy1 = pts[0].Y;
					xx2 = pts[1].X;
					yy2 = pts[1].Y;				
				}
				else
				{
					xx1 = x1;
					yy1 = y1;
					xx2 = x2;
					yy2 = y2;				
				}
			}
			else /*if(mode == LinearGradientMode.BackwardDiagonal)*/
			{
				if(y2-y1 != x2-x1)
				{
					PointF [] pts = GetDiagStartStopCoords(GetMedianeEnclosingRect(x1,y1,x2,y2));
					xx1 = pts[1].X;
					yy1 = pts[0].Y;
					xx2 = pts[0].X;
					yy2 = pts[1].Y;
				}
				else
				{
					xx1 = x2;
					yy1 = y1;
					xx2 = x1;
					yy2 = y2;				
				}
			}
			//move this into upper if's for better perfomance
			if(cycle)
			{
				if(xx2>xx1)
					xx2 = (xx2-xx1)/2+xx1;
				else
					xx2 = (xx1-xx2)/2+xx2;
				if(yy2>yy1)
					yy2 = (yy2-yy1)/2+yy1;
				else
					yy2 = (yy1-yy2)/2+yy2;
			}

			_nativeObject = new GradientPaint(xx1,yy1,
				new java.awt.Color(color1.R,color1.G,color1.B,color1.A),
				xx2,yy2,
				new java.awt.Color(color2.R,color2.G,color2.B,color2.A),cycle);
		}
		internal LinearGradientBrush (float x1,float y1, Color color1,float x2, float y2, Color color2, LinearGradientMode mode)
		{
			this.x1=x1;
			this.x2=x2;
			this.y1=y1;
			this.y2=y2;

			_linMode = mode;
			Init(x1,y1,color1,x2,y2,color2,mode,false);
		}

		public LinearGradientBrush (Point point1, Point point2, Color color1, Color color2):
			this((float)point1.X,(float)point1.Y,
			color1,
			(float)point2.X,(float)point2.Y,
			color2,LinearGradientMode.Horizontal)
		{
		}

		public LinearGradientBrush (PointF point1, PointF point2, Color color1, Color color2):
			this(point1.X,point1.Y,
			color1,
			point2.X,point2.Y,
			color2,LinearGradientMode.Horizontal)
		{			
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode):
			this(rect.Left,rect.Top,
			color1,
			rect.Right,rect.Bottom,
			color2,linearGradientMode)
		{
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle):
			this((float)rect.Left,(float)rect.Top,
			color1,
			(float)rect.Right,(float)rect.Bottom,
			color2,angle)
		{
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode):
			this(rect.Left,rect.Top,
			color1,
			rect.Right,rect.Bottom,
			color2,linearGradientMode)
		{			
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle) : 
			this (rect, color1, color2, angle, false)
		{
		}

		public LinearGradientBrush (Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable):
			this((float)rect.Left,(float)rect.Top,
			color1,
			(float)rect.Right,(float)rect.Bottom,
			color2,angle)
		{
			//TODO: angle, scalable
		}

		public LinearGradientBrush (RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable):
			this((float)rect.Left,(float)rect.Top,
			color1,
			(float)rect.Right,(float)rect.Bottom,
			color2,angle)
		{
			//TODO: angle, scalable
		}

		// Public Properties

		public Blend Blend {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public bool GammaCorrection {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public ColorBlend InterpolationColors {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
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
				RectangleF rect = this.Rectangle;
				_nativeObject = new GradientPaint( rect.X,rect.Y,
									value[0].NativeObject,
									rect.X + rect.Width,rect.Y + rect.Height,
									value[1].NativeObject);
			}
		}

		public RectangleF Rectangle {
			get {
				java.awt.geom.Point2D p1 = ((GradientPaint)NativeObject).getPoint1();
				java.awt.geom.Point2D p2 = ((GradientPaint)NativeObject).getPoint2();
				//return new RectangleF((float)p1.getX(),(float)p1.getY(),(float)(p2.getX()-p1.getX()),(float)(p2.getY() - p1.getY()));
				return new RectangleF(x1,y1,x2-x1,y2-y1);
			}
		}

		public Matrix Transform {
			get {
				return _transform;
			}
			set {
				_transform = value;
			}
		}

		public WrapMode WrapMode {
			get {
				return _wrapmode;
			}
			set {
				_wrapmode = value;
			}
		}

		// Public Methods

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			_transform.Multiply(matrix,order);			
		}

		public void ResetTransform ()
		{
			_transform = new Matrix();
		}

		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{
			_transform.Rotate(angle,order);
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			_transform.Scale(sx,sy,order);
		}

		public void SetBlendTriangularShape (float focus)
		{
			SetBlendTriangularShape (focus, 1.0F);
		}

		public void SetBlendTriangularShape (float focus, float scale)
		{
			Color []cl = LinearColors;
			Init(x1,y1,cl[0],x2,y2,cl[1],_linMode,true);
			//throw new NotImplementedException();
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
			_transform.Translate(dx,dy,order);
		}

		public override object Clone ()
		{
			Color [] clrs = this.LinearColors;
			LinearGradientBrush b = new LinearGradientBrush(this.Rectangle,clrs[0],clrs[1],_linMode);
			b.Transform = this.Transform;
			b.Blend = this.Blend;
			return b;
		}
	}
}
