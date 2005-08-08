using System;
using System.Drawing;
using System.Collections;
using java.awt.geom;
using java.awt;

namespace System.Drawing.Drawing2D
{
	public sealed class GraphicsPath : BasicShape, ICloneable
	{
		enum JPI {
			SEG_MOVETO = 0,
			SEG_LINETO = 1,
			SEG_QUADTO = 2,
			SEG_CUBICTO = 3,
			SEG_CLOSE = 4
		}

		#region Vars

		bool _isNewFigure = true;
		
		#endregion

		#region Internal
		internal GeneralPath NativeObject
		{
			get 
			{
				return (GeneralPath)Shape;
			}
		}

		GraphicsPath (GeneralPath ptr) : base(ptr)
		{
		}
		#endregion

		#region  C-tors.
		public GraphicsPath ():
			this(FillMode.Alternate)
		{
		}
                
		public GraphicsPath (FillMode fillMode) : this(new GeneralPath())
		{
			FillMode = fillMode;
		}
                
		public GraphicsPath (Point[] pts, byte[] types) : this(null)
		{
			throw new NotImplementedException();
		}
                
		public GraphicsPath (PointF[] pts, byte[] types) : this(null)
		{
			throw new NotImplementedException();
		}
                
		public GraphicsPath (Point[] pts, byte[] types, FillMode fillMode) : this(null)
		{
			throw new NotImplementedException();		
		}

		public GraphicsPath (PointF[] pts, byte[] types, FillMode fillMode) : this(null)
		{
			throw new NotImplementedException();
		}
	
		GraphicsPath (GeneralPath path, bool isNewFigure) : this(path)
		{
			_isNewFigure = isNewFigure;
		}

		#endregion

		#region Clone
		public object Clone ()
		{
			return new GraphicsPath ((GeneralPath)NativeObject.clone(), _isNewFigure);
		}
		#endregion

		#region Properties
		public FillMode FillMode 
		{
			get 
			{   if(NativeObject.getWindingRule() == GeneralPath.WIND_NON_ZERO)
					return FillMode.Alternate;
				else
					return FillMode.Winding;
			}

			set 
			{
				if(value == FillMode.Alternate)
					NativeObject.setWindingRule(GeneralPath.WIND_NON_ZERO);
				else
					NativeObject.setWindingRule(GeneralPath.WIND_EVEN_ODD);
			}
		}

		public PathData PathData 
		{
			get 
			{
				throw new NotImplementedException();
			}
		}

		public PointF [] PathPoints 
		{
			get 
			{
				return PathData.Points;
			}
		}

		public byte [] PathTypes 
		{
			get 
			{
				return PathData.Types;			
			}
		}
		#endregion

		#region PointCount [TODO]
		public int PointCount 
		{

			get 
			{
				//TODO
				throw new NotImplementedException();
			}
		}
		#endregion
                        
		#region AddArc
		public void AddArc (Rectangle rect, float startAngle, float sweepAngle)
		{
			AddArc(rect.X,rect.Y,rect.Width,rect.Height,startAngle,sweepAngle);			
		}

		public void AddArc (RectangleF rect, float startAngle, float sweepAngle)
		{
			AddArc(rect.X,rect.Y,rect.Width,rect.Height,startAngle,sweepAngle);
		}

		public void AddArc (int x, int y, int width, int height, float startAngle, float sweepAngle)
		{
			AddArc((float)x,(float)y,(float)width,(float)height,startAngle,sweepAngle);
		}

		public void AddArc (float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Shape shape = null;

			if (sweepAngle >= 360)
				shape = new Ellipse2D.Float(x, y, width, height);
			else {

				double d1Tod2 = width/height;
				double sqrd1Tod2 = d1Tod2*d1Tod2;
				double start = ConvertArcAngle(sqrd1Tod2, startAngle);
				double extent = ConvertArcAngle(sqrd1Tod2, startAngle+sweepAngle) - start;

				shape = new Arc2D.Double(x,y,width,height,-start,-extent,Arc2D.OPEN);
			}

			NativeObject.append(shape,!_isNewFigure);
			_isNewFigure = false;
		}

		/// <summary>
		/// .Net computes an angle by intersection of ellipse with a ray
		/// java does the following: x1 = d1*cos(a), y1 = d2*sin(a)
		/// where: d1 = width/2, d2 = height/2
		/// we need to find angle x, which satisfies:
		/// x1 = m*cos(a) = d1*cos(x)
		/// y1 = m*sin(a) = d2*sin(x)
		/// (x1*x1)/(d1*d1) + (x2*x2)/(d2*d2) = 1
		/// </summary>
		/// <param name="sqrd1Tod2">(d1/d2)*(d1/d2)</param>
		/// <param name="angle">angle in degrees</param>
		/// <returns>converted angle in degrees</returns>
		static double ConvertArcAngle(double sqrd1Tod2, double angle) {
			double angleRad = java.lang.Math.toRadians(angle);
			double tan = Math.Tan(angleRad);
			double cosx = 1/Math.Sqrt( sqrd1Tod2 * (tan*tan) + 1);
			double xRad = Math.Acos(cosx);
			double x = java.lang.Math.toDegrees(xRad);
			int q = ((int)angle)/90;

			switch (q&3) {
				default:
					return x;
				case 1:
					return 180-x;
				case 2:
					return 180+x;
				case 3:
					return 360-x;
			}
		}

		#endregion
		
		#region AddBezier(s) [s - TODO]
		public void AddBezier (Point pt1, Point pt2, Point pt3, Point pt4)
		{
			AddBezier(pt1.X,pt1.Y,pt2.X,pt2.Y,pt3.X,pt3.Y,pt4.X,pt4.Y);
		}

		public void AddBezier (PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{			
			AddBezier(pt1.X,pt1.Y,pt2.X,pt2.Y,pt3.X,pt3.Y,pt4.X,pt4.Y);
		}

		public void AddBezier (int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
		{
			AddBezier((float)x1,(float)y1,(float)x2,(float)y2,(float)x3,(float)y3,(float)x4,(float)y4);
		}

		public void AddBezier (float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			CubicCurve2D cc = new CubicCurve2D.Float(x1,y1,x2,y2,x3,y3,x4,y4);
			NativeObject.append(cc,!_isNewFigure);
			_isNewFigure = false;
		}

		public void AddBeziers (Point [] pts)
		{
			throw new NotImplementedException();
		}

		public void AddBeziers (PointF [] pts)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region AdEllipse
		public void AddEllipse (float x, float y, float width, float height)
		{
			Ellipse2D e = new Ellipse2D.Float(x,y,width,height);
			NativeObject.append(e,false);
			_isNewFigure = true;
		}

		public void AddEllipse (RectangleF r)
		{
			AddEllipse(r.X,r.Y,r.Width,r.Height);
		}
                
		public void AddEllipse (Rectangle r)
		{
			AddEllipse(r.X,r.Y,r.Width,r.Height);
		}
                
		public void AddEllipse (int x, int y, int width, int height)
		{
			AddEllipse((float)x, (float)y, (float)width, (float)height);
		}
		#endregion
                
		#region AddLine
		public void AddLine (float x1, float y1, float x2, float y2)
		{
			Line2D l = new Line2D.Float(x1,y1,x2,y2);
			NativeObject.append(l,!_isNewFigure);
			_isNewFigure = false;
		}

		public void AddLine (Point a, Point b)
		{
			AddLine(a.X,a.Y,b.X,b.Y);
		}

		public void AddLine (PointF a, PointF b)
		{
			AddLine(a.X,a.Y,b.X,b.Y);
		}

		public void AddLine (int x1, int y1, int x2, int y2)
		{
			AddLine((float)x1,(float)y1,(float)x2,(float)y2);
		}

		public void AddLines (Point [] points)
		{			
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length == 0)
				return;

			if (_isNewFigure)
				NativeObject.moveTo(points[0].X, points[0].Y);
			else
				NativeObject.lineTo(points[0].X, points[0].Y);

			_isNewFigure = false;

			for (int i = 1; i < points.Length; i ++)
				NativeObject.lineTo(points[i].X, points[i].Y);
		}

		public void AddLines (PointF [] points)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length == 0)
				return;

			if (_isNewFigure)
				NativeObject.moveTo(points[0].X, points[0].Y);
			else
				NativeObject.lineTo(points[0].X, points[0].Y);

			_isNewFigure = false;

			for (int i = 1; i < points.Length; i ++)
				NativeObject.lineTo(points[i].X, points[i].Y);
		}
		#endregion
        
		#region AddPie
		public void AddPie (float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Shape shape = null;

			if (sweepAngle >= 360)
				shape = new Ellipse2D.Float(x, y, width, height);
			else {

				double d1Tod2 = width/height;
				double sqrd1Tod2 = d1Tod2*d1Tod2;
				double start = ConvertArcAngle(sqrd1Tod2, startAngle);
				double extent = ConvertArcAngle(sqrd1Tod2, startAngle+sweepAngle) - start;

				shape = new Arc2D.Double(x,y,width,height,-start,-extent,Arc2D.PIE);
			}

			NativeObject.append(shape,false);
			_isNewFigure = true;
		}

		public void AddPie (Rectangle rect, float startAngle, float sweepAngle)
		{
			AddPie((float)rect.X, (float)rect.Y,(float)rect.Width,(float)rect.Height,startAngle,sweepAngle);		
		}

		public void AddPie (int x, int y, int width, int height, float startAngle, float sweepAngle)
		{
			AddPie((float)x,(float)y,(float)width,(float)height,startAngle,sweepAngle);		
		}
		#endregion

		#region AddPolygon
		public void AddPolygon (Point [] points)
		{
			if(points.Length < 2)
				return;
			NativeObject.moveTo((float)points[0].X,(float)points[0].Y);
			for(int i = 1; i< points.Length - 1;i++)
			{
				NativeObject.lineTo((float)points[i].X,(float)points[i].Y);
			}
			NativeObject.closePath();
			_isNewFigure = true;
		}

		public void AddPolygon (PointF [] points)
		{
			if(points.Length < 2)
				return;
			NativeObject.moveTo(points[0].X,points[0].Y);
			for(int i = 1; i< points.Length - 1;i++)
			{
				NativeObject.lineTo(points[i].X,points[i].Y);
			}
			NativeObject.closePath();
			_isNewFigure = true;
		}
		#endregion

		#region AddRectangle(s)
		internal void AddRectangle(float x,float y, float w, float h)
		{
			Rectangle2D r = new Rectangle2D.Float(x,y,w,h);
			NativeObject.append(r,false);
			_isNewFigure = true;
		}
		public void AddRectangle (RectangleF rect)
		{
			AddRectangle(rect.X,rect.Y,rect.Width,rect.Height);
		}

		public void AddRectangle (Rectangle rect)
		{
			AddRectangle(rect.X,rect.Y,rect.Width,rect.Height);
		}

		public void AddRectangles (Rectangle [] rects)
		{
			foreach(Rectangle rect in rects)
				AddRectangle(rect.X,rect.Y,rect.Width,rect.Height);
		}

		public void AddRectangles (RectangleF [] rects)
		{
			foreach(RectangleF rect in rects)
				AddRectangle(rect.X,rect.Y,rect.Width,rect.Height);
		}
		#endregion

		#region AddPath
		public void AddPath (GraphicsPath addingPath, bool connect)
		{
			NativeObject.append(addingPath.NativeObject,connect);
			_isNewFigure = false;
		}
		#endregion

		#region GetLastPoint
		public PointF GetLastPoint ()
		{
			java.awt.geom.Point2D p2d = NativeObject.getCurrentPoint();
			return new PointF((float)p2d.getX(),(float)p2d.getY());
		}
		#endregion

		#region Reset
		public void Reset ()
		{
			NativeObject.reset();
			_isNewFigure = true;
		}
		#endregion

		#region GetBounds
		public RectangleF GetBounds ()
		{
			Rectangle2D rect = NativeObject.getBounds2D();
			return new RectangleF((float)rect.getX(),(float)rect.getY(),(float)rect.getWidth(),(float)rect.getHeight());
		}  		

		public RectangleF GetBounds (Matrix matrix)
		{
			Shape shape = matrix != null ? 
				NativeObject.createTransformedShape(matrix.NativeObject) : NativeObject;
			Rectangle2D rect = shape.getBounds2D();
			return new RectangleF((float)rect.getX(),(float)rect.getY(),(float)rect.getWidth(),(float)rect.getHeight());
		}

		public RectangleF GetBounds (Matrix matrix, Pen pen)
		{
			throw new NotImplementedException();
		}
		#endregion
        
		#region Transform
		public void Transform (Matrix matrix)
		{
			if(matrix == null)
				return;

			NativeObject.transform(matrix.NativeObject);
		}
		#endregion

		#region IsVisible
		public bool IsVisible (Point point)
		{
			return IsVisible (point.X, point.Y, null);
		}  		
                
		public bool IsVisible (PointF point)
		{
			return IsVisible (point.X, point.Y, null);
		}  		
                
		public bool IsVisible (int x, int y)
		{
			return IsVisible (x, y, null);
		}

		public bool IsVisible (float x, float y)
		{
			return IsVisible (x, y, null);
		}  		                
                
		public bool IsVisible (Point pt, Graphics graphics)
		{
			return IsVisible (pt.X, pt.Y, graphics);
		}  		
                
		public bool IsVisible (PointF pt, Graphics graphics)
		{
			return IsVisible (pt.X, pt.Y, graphics);
		}  		
                                
		public bool IsVisible (int x, int y, Graphics graphics)
		{
			return IsVisible((float)x,(float)y,null);
		}  		
                
		public bool IsVisible (float x, float y, Graphics graphics)
		{
			if (graphics != null && !graphics.IsVisible(x,y))
				return false;

			return NativeObject.contains(x,y);
		}
		#endregion
        
		#region Reverse [TODO]
		public void Reverse ()
		{
			throw new NotImplementedException();
		}
		#endregion
             
		#region AddClosedCurve [TODO]
		//this could be simply implemented using the same 
		//mechnizm as AddCurve. Simply use the last point for 
		//first point tangent calculation
		public void AddClosedCurve (Point [] points)
		{
			throw new NotImplementedException ();
		}

		public void AddClosedCurve (PointF [] points)
		{
			throw new NotImplementedException ();
		}

		public void AddClosedCurve (Point [] points, float tension)
		{
			throw new NotImplementedException ();
		}

		public void AddClosedCurve (PointF [] points, float tension)
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region AddCurve
		//we have now two approaches for drawing cardinal curves
		//the first one is to convert cardinals into approximate beziers
		//the second one - to draw curve ourself with all interpolation staff
		//here. I preffer the first one because we could utilize java antialiasing and
		//flattening features, otherwise curves will be more strict but less cool
		public void AddCurve (Point [] points)
		{
			AddCurve(points,0.3F);
		}
                
		public void AddCurve (PointF [] points)
		{
			AddCurve(points,0.3f);
		}
                
		public void AddCurve (Point [] points, float tension)
		{
			AddCurve(points, 0, points.Length-1, tension);
		}
                
		public void AddCurve (PointF [] points, float tension)
		{
			AddCurve(points, 0, points.Length-1, tension);
		}

		public void AddCurve (Point [] points, int offset, int numberOfSegments, float tension)
		{
			int nPoints = numberOfSegments + 1;
			int length = nPoints*2 + 4;
			float[] pts = new float[length];

			int lastP = offset + nPoints;
			if (lastP == points.Length) {
				lastP--;
				pts[--length] = points[lastP].Y;
				pts[--length] = points[lastP].X;
			}

			for (; length > 0 && lastP >= 0; lastP--) {
				pts[--length] = points[lastP].Y;
				pts[--length] = points[lastP].X;
			}

			if (length > 0) {
				pts[1] = points[0].Y;
				pts[0] = points[0].X;
			}

			AddCurve(pts, !_isNewFigure, tension);
			_isNewFigure = false;
		}
                
		public void AddCurve (PointF [] points, int offset, int numberOfSegments, float tension)
		{
			int nPoints = numberOfSegments + 1;
			int length = nPoints*2 + 4;
			float[] pts = new float[length];

			int lastP = offset + nPoints;
			if (lastP == points.Length) {
				lastP--;
				pts[--length] = points[lastP].Y;
				pts[--length] = points[lastP].X;
			}

			for (; length > 0 && lastP >= 0; lastP--) {
				pts[--length] = points[lastP].Y;
				pts[--length] = points[lastP].X;
			}

			if (length > 0) {
				pts[1] = points[0].Y;
				pts[0] = points[0].X;
			}

			AddCurve(pts, !_isNewFigure, tension);
			_isNewFigure = false;
		}

		/// <summary>
		/// Based on http://pubpages.unh.edu/~cs770/a5/cardinal.html
		/// </summary>
		/// <param name="pts">point array (x1,y1,x2,y2 ...).
		/// The first and last points considered only for calculations, but are not added.</param>
		void AddCurve(float[] pts, bool connect, float tension) {
			tension /= 3f; //looks like a good pick

			if (connect)
				NativeObject.lineTo(pts[2],pts[3]);
			else
				NativeObject.moveTo(pts[2],pts[3]);

			float dx = pts[4] - pts[0];
			float dy = pts[5] - pts[1];

			float sx = pts[2] + tension*dx;
			float sy = pts[3] + tension*dy;

			for (int offset = 2, total = pts.Length-4; offset < total; offset += 2) {
				int cur_offset = offset;
				int pX = cur_offset++;
				int pY = cur_offset++;
				int X = cur_offset++;
				int Y = cur_offset++;
				int nX = cur_offset++;
				int nY = cur_offset++;

				dx = pts[nX] - pts[pX];
				dy = pts[nY] - pts[pY];

				float rx = pts[X] - tension*dx;
				float ry = pts[Y] - tension*dy;
				
				NativeObject.curveTo(sx, sy, rx, ry, pts[X], pts[Y]);

				sx = pts[X] + tension*dx;
				sy = pts[Y] + tension*dy;
			}
		}
		#endregion

		#region AddString [TODO]
		public void AddString (string s, FontFamily family, int style,  float emSize,  Point origin,   StringFormat format)
		{
			throw new NotImplementedException ();
		}  	
                
		public void AddString (string s,  FontFamily family,  int style,  float emSize,  PointF origin,   StringFormat format)
		{
			throw new NotImplementedException ();
		}  	
  		
		public void AddString (string s, FontFamily family, int style, float emSize,  Rectangle layoutRect, StringFormat format)
		{
			throw new NotImplementedException ();
		}  	
  		
		public void AddString (string s, FontFamily family, int style, float emSize,  RectangleF layoutRect,   StringFormat format)
		{
			throw new NotImplementedException ();
		}
		#endregion
                
		#region ClearMarkers [TODO]
		public void ClearMarkers()               
		{
			throw new NotImplementedException ();
		}
		#endregion
        
		#region Close(All) [REVIEW-EXTEND]
		

		public void CloseAllFigures()
		{
			GeneralPath p = new GeneralPath();
			PathIterator pi = NativeObject.getPathIterator(null);
			JPI lastSeg = JPI.SEG_CLOSE;
			float [] points = new float[6];
 
			p.setWindingRule(pi.getWindingRule());
			while(!pi.isDone())
			{
				JPI curSeg = (JPI)pi.currentSegment(points);
				switch(curSeg)
				{
					case JPI.SEG_CLOSE:
						p.closePath();
						break;
					case JPI.SEG_MOVETO:
						if(lastSeg != JPI.SEG_CLOSE)
							p.closePath();
						p.moveTo(points[0],points[1]);
						break;
					case JPI.SEG_LINETO:
						p.lineTo(points[0],points[1]);
						break;
					case JPI.SEG_QUADTO:
						p.quadTo(points[0],points[1],points[2],points[3]);
						break;
					case JPI.SEG_CUBICTO:
						p.curveTo(points[0],points[1],points[2],points[3],points[4],points[5]);
						break;
					default:
						break;
				}				
				lastSeg = curSeg;
				pi.next();
			}

			Shape = p;
			//_isNewFigure = (lastSeg == PathIterator.SEG_CLOSE);
		}  	
                
		public void CloseFigure()
		{
			NativeObject.closePath();
		} 
		#endregion

		#region Flatten [REVIEW]
		public void Flatten ()
		{
			// 1/4 is the FlatnessDefault as defined in GdiPlusEnums.h
			Flatten (null, 1.0f / 4.0f); 
		}  	
  
		public void Flatten (Matrix matrix)
		{
			Flatten (matrix, 1.0f / 4.0f);
		}
		
		public void Flatten (Matrix matrix, float flatness)
		{
			AffineTransform tr = null;
			if(matrix != null)			
				tr = matrix.NativeObject;

			//REVIEW. Perfomance reasons.
			PathIterator pi = NativeObject.getPathIterator(tr,flatness);
			GeneralPath newPath = new GeneralPath();
			newPath.append(pi,false);
			Shape = newPath;
		}
		#endregion
        
		#region GetOutlineVisible [TODO]
		public bool IsOutlineVisible (Point point, Pen pen)
		{
			throw new NotImplementedException();
		}  		
		
		public bool IsOutlineVisible (PointF point, Pen pen)
		{
			throw new NotImplementedException();
		} 
		
		public bool IsOutlineVisible (int x, int y, Pen pen)
		{
			throw new NotImplementedException();
		}

		public bool IsOutlineVisible (float x, float y, Pen pen)
		{
			throw new NotImplementedException();
		}  		
		
		public bool IsOutlineVisible (Point pt, Pen pen, Graphics graphics)
		{
			throw new NotImplementedException();
		}  		
		
		public bool IsOutlineVisible (PointF pt, Pen pen, Graphics graphics)
		{
			throw new NotImplementedException();
		}  		
				
		public bool IsOutlineVisible (int x, int y, Pen pen, Graphics graphics)
		{
			throw new NotImplementedException();
		}  		
				
		public bool IsOutlineVisible (float x, float y, Pen pen, Graphics graphics)
		{
			throw new NotImplementedException();
		}  		
		#endregion
        
		#region SetMarkers [TODO]
		public void SetMarkers ()
		{
			throw new NotImplementedException();
		}
		#endregion
                
		#region StartFigure
		public void StartFigure()
		{
			_isNewFigure = true;
		}
		#endregion
  		        
		#region Warp [TODO]
		public void Warp (PointF[] destPoints, RectangleF srcRect)
		{
			Warp (destPoints, srcRect, null, WarpMode.Perspective, 1.0f / 4.0f);
		}  		

		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix)
		{
			Warp (destPoints, srcRect, matrix, WarpMode.Perspective, 1.0f / 4.0f);
		}  		

		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode)
		{
			Warp (destPoints, srcRect, matrix, warpMode, 1.0f / 4.0f);
		}  		

		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix,  WarpMode warpMode, float flatness)
		{
			throw new NotImplementedException();
		}
		#endregion
        
		#region Widen [TODO]
		public void Widen (Pen pen)
		{
			Widen (pen, null, 1.0f / 4.0f);
		}  		
                
		public void Widen (Pen pen, Matrix matrix)
		{	
			Widen (pen, matrix, 1.0f / 4.0f);
		}  		
                		
		public void Widen (Pen pen, Matrix matrix, float flatness)
		{
			throw new NotImplementedException();
		} 
		#endregion
	}
}
