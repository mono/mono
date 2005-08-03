using System;
using System.Drawing;
using System.Collections;
using java.awt.geom;
using java.awt;

namespace System.Drawing.Drawing2D
{
	public enum JPI
	{
		SEG_MOVETO = 0,
		SEG_LINETO = 1,
		SEG_QUADTO = 2,
		SEG_CUBICTO = 3,
		SEG_CLOSE = 4
	}

	public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable 
	{
#if OLDCODE
		internal class SegmentWrapper
		{
			internal GeneralPath segment = new GeneralPath();
			Point2D startPoint = null;
			Point2D endPoint = null;
			bool isFirstAdd = true;
			bool nonTrackable = false;
			bool bLastWasArc = false;
			//ArrayList concats = new ArrayList();

			public SegmentWrapper()
			{
			}

			public SegmentWrapper(GeneralPath path)
			{
				nonTrackable = true;
				segment=(GeneralPath)path.clone();
			}

			public void flatten (AffineTransform tr, float flat)
			{
				if(tr != null)
					transform(tr);
				//TODO
				//after this start and stop points probably could be invalid
				//we should check this later
				GeneralPath gp = new GeneralPath();
			    gp.append(segment.getPathIterator(null,(double)flat),false);
				segment = gp;
			}
			public void transform(AffineTransform tr)
			{
				segment.transform(tr);
				if(startPoint != null)
					tr.transform(startPoint,startPoint);
			}

			public void generalappend(Shape s,bool connect)
			{
				segment.append(s,connect);
			}

			public void append(PathIterator s)
			{
				nonTrackable = true;
				NativeObject.append(s,false);
				//???
				bLastWasArc = false;
			}

			public void append(GeneralPath s, bool connect)
			{
				//REVIEW
				isFirstAdd = false;
				NativeObject.append(s,connect);
				//endPoint= s.getCurrentPoint();
				bLastWasArc = false;
			}
			
			public Point2D LastPoint
			{
				get
				{
						return NativeObject.getCurrentPoint();
				}
			}
			public Point2D FirstPoint
			{
				get
				{
					return startPoint;
				}
				set
				{
					startPoint = value;
					isFirstAdd = false;
				}
			}

			public void append(Shape s)
			{
				throw new NotSupportedException("Add support for this shape explicitly");
			}

			public void append(Line2D s)
			{
				bool append = true;
				Point2D startPoint = s.getP1();
				if(startPoint.equals(segment.getCurrentPoint()))
					append = true;
//				endPoint = s.getP2();
				if(isFirstAdd)
					this.startPoint = startPoint;
				isFirstAdd = false;
				generalappend(s,append);
			}
			public void append(Rectangle2D s)
			{
				bool append = true;
				Point2D startPoint = new Point2D.Double(s.getX(),s.getY());
				if(startPoint.equals(segment.getCurrentPoint()))
					append = true;
//				endPoint   = new Point2D.Double(s.getMaxX(),s.getMaxY());
				if(isFirstAdd)
					this.startPoint = startPoint;
				isFirstAdd = false;
				generalappend(s,append);
			}
			public void append(Arc2D s)
			{
				bool append = true;
				Point2D startPoint = s.getStartPoint();
				if(startPoint.equals(segment.getCurrentPoint()))
					append = true;
//				endPoint = s.getEndPoint();
				if(isFirstAdd)
					this.startPoint = startPoint;				
				isFirstAdd = false;
				generalappend(s,append);
			}
			public void append(Ellipse2D s)
			{
				//REVIEW
				isFirstAdd = true;
				generalappend(s,false);
			}
			public void append(CubicCurve2D s)
			{
				bool append = true;
				Point2D startPoint = s.getP1();
				if(startPoint.equals(segment.getCurrentPoint()))
					append = true;
//				endPoint = s.getP2();
				if(isFirstAdd)
					this.startPoint = startPoint;				
				isFirstAdd = false;
				generalappend(s,append);
			}
			public void append(QuadCurve2D s)
			{
				bool append = true;
				Point2D startPoint = s.getP1();
				if(startPoint.equals(segment.getCurrentPoint()))
					append = true;
//				endPoint = s.getP2();				
				if(isFirstAdd)
					this.startPoint = startPoint;				
				isFirstAdd = false;
				generalappend(s,append);
			}
			public GeneralPath NativeObject
			{
				get
				{
					return segment;
				}
			}
			public void close()
			{
				if(startPoint == null)
					NativeObject.closePath();
				else
					generalappend(new Line2D.Float(NativeObject.getCurrentPoint(),startPoint),true);
				startPoint = null;
				isFirstAdd = true;
			}
		}
#endif 
		#region Vars

		GeneralPath _nativePath = null;
//		internal PathData _pathData;		
		bool isNewFigure = true;
		
		#endregion

		#region Internal
		internal GeneralPath NativeObject
		{
			get 
			{
				return _nativePath;
			}
		}

		GraphicsPath (GeneralPath ptr)
		{
			_nativePath = ptr;
		}
		#endregion

		#region  C-tors.
		public GraphicsPath ():
			this(FillMode.Alternate)
		{
		}
                
		public GraphicsPath (FillMode fillMode)
		{
			_nativePath = new GeneralPath();
			if(fillMode == FillMode.Alternate)
				_nativePath.setWindingRule(GeneralPath.WIND_NON_ZERO);
			else
				_nativePath.setWindingRule(GeneralPath.WIND_EVEN_ODD);
		}
                
		public GraphicsPath (Point[] pts, byte[] types)
		{
			throw new NotImplementedException();
		}
                
		public GraphicsPath (PointF[] pts, byte[] types)
		{
			throw new NotImplementedException();
		}
                
		public GraphicsPath (Point[] pts, byte[] types, FillMode fillMode)
		{
			throw new NotImplementedException();		
		}

		public GraphicsPath (PointF[] pts, byte[] types, FillMode fillMode)
		{
			throw new NotImplementedException();
		}
	
		GraphicsPath (GraphicsPath gp)
		{
			_nativePath = (GeneralPath)gp.NativeObject.clone();
			_nativePath.setWindingRule(gp.NativeObject.getWindingRule());
		}

		#endregion

		#region Clone
		public object Clone ()
		{
			return new GraphicsPath (this);
		}
		#endregion

		#region Dispose
		public void Dispose ()
		{
		}
		
		void Dispose (bool disposing)
		{		
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
		public void AddArc (Rectangle rect, float start_angle, float sweep_angle)
		{
			AddArc(rect.X,rect.Y,rect.Width,rect.Height,start_angle,sweep_angle);			
		}

		public void AddArc (RectangleF rect, float start_angle, float sweep_angle)
		{
			AddArc(rect.X,rect.Y,rect.Width,rect.Height,start_angle,sweep_angle);
		}

		public void AddArc (int x, int y, int width, int height, float start_angle, float sweep_angle)
		{
			AddArc((float)x,(float)y,(float)width,(float)height,start_angle,sweep_angle);
		}

		public void AddArc (float x, float y, float width, float height, float start_angle, float sweep_angle)
		{
			Arc2D a = new Arc2D.Float(x, y,width,height,-start_angle,-sweep_angle,0/*OPEN*/);
			NativeObject.append(a,!isNewFigure);
			isNewFigure = false;
			//LastFigure.append(a); 
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
			NativeObject.append(cc,!isNewFigure);
			isNewFigure = false;
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
			isNewFigure = true;
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
			NativeObject.append(l,!isNewFigure);
			isNewFigure = false;
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
			for (int i = 0; i < points.Length - 2; i += 2) 
				AddLine(points[i].X, points [i].Y, points [i+1].X, points [i+1].Y);
		}

		public void AddLines (PointF [] points)
		{
			for (int i = 0; i < points.Length - 2; i += 2) 
				AddLine(points[i].X, points [i].Y, points [i+1].X, points [i+1].Y);
		}
		#endregion
        
		#region AddPie
		public void AddPie (float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Arc2D a = new Arc2D.Float(x,y,width,height,-startAngle,-sweepAngle,2/*PIE*/);
			NativeObject.append(a,false);
			isNewFigure = true;
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
			isNewFigure = true;
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
			isNewFigure = true;
		}
		#endregion

		#region AddRectangle(s)
		internal void AddRectangle(float x,float y, float w, float h)
		{
			Rectangle2D r = new Rectangle2D.Float(x,y,w,h);
			NativeObject.append(r,false);
			isNewFigure = true;
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
			isNewFigure = false;
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
			_nativePath.reset();
		}
		#endregion

		#region GetBounds
		public RectangleF GetBounds ()
		{
			//TBD: use getBounds2D
			java.awt.Rectangle rect = NativeObject.getBounds();
			return new RectangleF((float)rect.getX(),(float)rect.getY(),(float)rect.getWidth(),(float)rect.getHeight());
		}  		

		public RectangleF GetBounds (Matrix matrix)
		{
			return GetBounds (matrix, null);
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
			AffineTransform tr = new AffineTransform(matrix.Elements[0],matrix.Elements[1],matrix.Elements[2],matrix.Elements[3],matrix.Elements[4],matrix.Elements[5]);
			NativeObject.transform(tr);
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
			bool gCont = true;
			if(graphics != null)
				gCont = graphics.IsVisible(x,y);
			return NativeObject.contains(x,y) && gCont;
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
#if !AUTONOMOUS_CURVE_DRAW		
		internal void AppendCurve (GeneralPath path, Point [] points,float tension)
		{
			int i, length = points.Length;
			

			if (points.Length <= 0)
				return;

			float coefficient = tension / 3.0f;			

			PointF []tangents = new PointF[points.Length];

			/* initialize everything to zero to begin with */
			for (i = 0; i < length; i++) 
			{
				tangents [i].X = 0;
				tangents [i].Y = 0;
			}

			if (length > 2)
			{
				for (i = 1; i < length - 1; i++) 
				{
					int r = i + 1;
					int s = i - 1;

					if (r >= length) r = length - 1;
					if (s < 0) s = 0;

					tangents [i].X += (coefficient * ((float)points [r].X - (float)points [s].X));
					tangents [i].Y += (coefficient * ((float)points [r].Y - (float)points [s].Y));
				}
			}			
			length = points.Length - 1;
			for (i = 1; i <= length; i++) 
			{

				int j = i - 1;
				int k = (i < points.Length) ? i : 0;

				double x0 = (double)points [j].X;
				double y0 = (double)points [j].Y;

				double x1 = (double)points [j].X + tangents [j].X;
				double y1 = (double)points [j].Y + tangents [j].Y;

				double x2 = (double)points [k].X - tangents [k].X;
				double y2 = (double)points [k].Y - tangents [k].Y;

				double x3 = (double)points [k].X;
				double y3 = (double)points [k].Y;
				CubicCurve2D cc = new CubicCurve2D.Double(x0,y0,x1,y1,x2,y2,x3,y3);
				if(i==1)
					path.append(cc,false);
				else
					path.append(cc,true);
			}
		}

		internal void AppendCurve (GeneralPath path, PointF [] points,float tension)
		{
			int i, length = points.Length;

			if (points.Length <= 0)
				return;

			float coefficient = tension / 3.0f;			

			PointF []tangents = new PointF[points.Length];

			/* initialize everything to zero to begin with */
			for (i = 0; i < length; i++) 
			{
				tangents [i].X = 0;
				tangents [i].Y = 0;
			}

			if (length > 2)
			{
				for (i = 1; i < length - 1; i++) 
				{
					int r = i + 1;
					int s = i - 1;

					if (r >= length) r = length - 1;
					if (s < 0) s = 0;

					tangents [i].X += (coefficient * (points [r].X - points [s].X));
					tangents [i].Y += (coefficient * (points [r].Y - points [s].Y));
				}
			}
			length = points.Length - 1;
			for (i = 1; i <= length; i++) 
			{

				int j = i - 1;
				int k = (i < points.Length) ? i : 0;

				double x0 = (double)points [j].X;
				double y0 = (double)points [j].Y;

				double x1 = (double)points [j].X + tangents [j].X;
				double y1 = (double)points [j].Y + tangents [j].Y;

				double x2 = (double)points [k].X - tangents [k].X;
				double y2 = (double)points [k].Y - tangents [k].Y;

				double x3 = (double)points [k].X;
				double y3 = (double)points [k].Y;
				CubicCurve2D cc = new CubicCurve2D.Double(x0,y0,x1,y1,x2,y2,x3,y3);
				if(i==1)
					path.append(cc,false);
				else
					path.append(cc,true);
			}
		}

#else
		internal static void InternalAddCurve(GeneralPath path,PointF []pts,float tension)		
		{
			float jtension = tension/3;
			int segLen = 13;
			int n = pts.Length;
			int n1 = n + 1;
			int n2 = n + 2;
			float []Px = new float[n2];
			float []Py = new float[n2];
			for(int i = 0; i < n; i++)
			{
				Px[i + 1] = pts[i].X;
				Py[i + 1] = pts[i].Y;
			}

			float t = 0.0F;
			float []B0 = new float[segLen];
			float []B1 = new float[segLen];
			float []B2 = new float[segLen];
			float []B3 = new float[segLen];
			for(int i = 0; i < segLen; i++)
			{
				float t1 = (float)1 - t;
				float t12 = t1 * t1;
				float t2 = t * t;
				B0[i] = t1 * t12;
				B1[i] = (float)3 * t * t12;
				B2[i] = (float)3 * t2 * t1;
				B3[i] = t * t2;
				t = (float)((double)t + 0.080000000000000002D);
				//t = (float)((double)t + 0.040000000000000001D);
			}

			float Xo = Px[1];
			float Yo = Py[1];
			float Xold = Xo;
			float Yold = Yo;
			Px[0] = Px[1] - (Px[2] - Px[1]);
			Py[0] = Py[1] - (Py[2] - Py[1]);
			Px[n1] = Px[n] + (Px[n] - Px[n - 1]);
			Py[n1] = Py[n] + (Py[n] - Py[n - 1]);
			path.moveTo(Xold, Yold);
			for(int i = 1; i < n; i++)
			{
				for(int k = 0; k < segLen; k++)
				{
					float X = (float)Px[i] * B0[k] + ((float)Px[i] + (float)(Px[i + 1] - Px[i - 1]) * jtension) * B1[k] + ((float)Px[i + 1] - (float)(Px[i + 2] - Px[i]) * jtension) * B2[k] + (float)Px[i + 1] * B3[k];
					float Y = (float)Py[i] * B0[k] + ((float)Py[i] + (float)(Py[i + 1] - Py[i - 1]) * jtension) * B1[k] + ((float)Py[i + 1] - (float)(Py[i + 2] - Py[i]) * jtension) * B2[k] + (float)Py[i + 1] * B3[k];
					path.lineTo(X, Y);
				}

			}
		}

		internal void InternalAddCurve(GeneralPath path,Point []pts,float tension)		
		{
			float jtention = tension / 3;
			int segLen = 13;
			int n = pts.Length;
			int n1 = n + 1;
			int n2 = n + 2;
			float []Px = new float[n2];
			float []Py = new float[n2];
			for(int i = 0; i < n; i++)
			{
				Px[i + 1] = (float)pts[i].X;
				Py[i + 1] = (float)pts[i].Y;
			}

			float t = 0.0F;
			float []B0 = new float[segLen];
			float []B1 = new float[segLen];
			float []B2 = new float[segLen];
			float []B3 = new float[segLen];
			for(int i = 0; i < segLen; i++)
			{
				float t1 = (float)1 - t;
				float t12 = t1 * t1;
				float t2 = t * t;
				B0[i] = t1 * t12;
				B1[i] = (float)3 * t * t12;
				B2[i] = (float)3 * t2 * t1;
				B3[i] = t * t2;
				t = (float)((double)t + 0.080000000000000002D);
				//t = (float)((double)t + 0.040000000000000001D);
			}

			float Xo = Px[1];
			float Yo = Py[1];
			float Xold = Xo;
			float Yold = Yo;
			Px[0] = Px[1] - (Px[2] - Px[1]);
			Py[0] = Py[1] - (Py[2] - Py[1]);
			Px[n1] = Px[n] + (Px[n] - Px[n - 1]);
			Py[n1] = Py[n] + (Py[n] - Py[n - 1]);
			path.moveTo(Xold, Yold);
			for(int i = 1; i < n; i++)
			{
				float x = (float)Px[i];
				float y = (float)Py[i];

				float x1 = (float)Px[i+1];
				float y1 = (float)Py[i+1];
				float x2 = (float)Px[i+1];
				float y2 = (float)Py[i+1];

				float xe = (float)Px[i+1];
				float ye = (float)Py[i+1];


//				for(int k = 0; k < segLen; k++)
//				{
//					float X = (float)Px[i] * B0[k] + ((float)Px[i] + (float)(Px[i + 1] - Px[i - 1]) * jtention) * B1[k] + ((float)Px[i + 1] - (float)(Px[i + 2] - Px[i]) * jtention) * B2[k] + (float)Px[i + 1] * B3[k];
//					float Y = (float)Py[i] * B0[k] + ((float)Py[i] + (float)(Py[i + 1] - Py[i - 1]) * jtention) * B1[k] + ((float)Py[i + 1] - (float)(Py[i + 2] - Py[i]) * jtention) * B2[k] + (float)Py[i + 1] * B3[k];
//					path.lineTo(X, Y);
//				}

			}
		}
#endif

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
			//REVIEW
			//maybe last point of current figure should be prepended to array
			GeneralPath gp = new GeneralPath();
			AppendCurve(gp,points,tension);			
			NativeObject.append(gp,!isNewFigure);
			isNewFigure = false;
//			LastFigure.append(new FlatteningPathIterator(gp.getPathIterator(null),1.0)/*,false*/);
		}
                
		public void AddCurve (PointF [] points, float tension)
		{
			GeneralPath gp = new GeneralPath();
			AppendCurve(gp,points,tension);
			NativeObject.append(gp,!isNewFigure);
			isNewFigure = false;
//			LastFigure.append(new FlatteningPathIterator(gp.getPathIterator(null),1.0)/*,false*/);
		}

		public void AddCurve (Point [] points, int offset, int numberOfSegments, float tension)
		{
			throw new NotImplementedException ();
		}
                
		public void AddCurve (PointF [] points, int offset, int numberOfSegments, float tension)
		{
			throw new NotImplementedException ();
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

			_nativePath = p;
			//isNewFigure = (lastSeg == PathIterator.SEG_CLOSE);
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
			PathIterator pi = _nativePath.getPathIterator(tr,flatness);
			GeneralPath newPath = new GeneralPath();
			newPath.append(pi,false);
			_nativePath = newPath;
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
			isNewFigure = true;
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
