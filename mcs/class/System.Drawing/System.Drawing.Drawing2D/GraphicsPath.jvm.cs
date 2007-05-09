//
// System.Drawing.Drawing2D.GraphicsPath.cs
//
// Author:
// Konstantin Triger <kostat@mainsoft.com>
// Bors Kirzner <boris@mainsoft.com>	
//
// Copyright (C) 2005 Mainsoft Corporation, (http://www.mainsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Collections;
using java.awt.geom;
using java.awt;

namespace System.Drawing.Drawing2D
{
	public sealed class GraphicsPath : BasicShape, ICloneable
	{
		internal enum JPI {
			SEG_MOVETO = ExtendedGeneralPath.SEG_MOVETO,
			SEG_LINETO = ExtendedGeneralPath.SEG_LINETO,
			SEG_QUADTO = ExtendedGeneralPath.SEG_QUADTO,
			SEG_CUBICTO = ExtendedGeneralPath.SEG_CUBICTO,
			SEG_CLOSE = ExtendedGeneralPath.SEG_CLOSE
		}

		#region Internal

		internal ExtendedGeneralPath NativeObject
		{
			get 
			{
				return (ExtendedGeneralPath)Shape;
			}
		}

		GraphicsPath (ExtendedGeneralPath ptr) : base(ptr)
		{
		}
		#endregion

		#region  C-tors.
		public GraphicsPath ():
			this(FillMode.Alternate)
		{
		}
                
		public GraphicsPath (FillMode fillMode) : this(new ExtendedGeneralPath ())
		{
			FillMode = fillMode;
		}
                
		public GraphicsPath (Point[] pts, byte[] types) : this(pts, types, FillMode.Alternate)
		{
		}
                
		public GraphicsPath (PointF [] pts, byte [] types) : this(pts, types, FillMode.Alternate)
		{
		}
                
		public GraphicsPath (Point [] pts, byte [] types, FillMode fillMode) : this(new ExtendedGeneralPath ())
		{
			FillMode = fillMode;
			SetPath (pts, types);
		}

		public GraphicsPath (PointF [] pts, byte [] types, FillMode fillMode) : this(new ExtendedGeneralPath ())
		{
			FillMode = fillMode;
			SetPath (pts, types);
		}

		#endregion

		#region Clone
		public object Clone ()
		{
			return new GraphicsPath ((ExtendedGeneralPath) NativeObject.Clone ());
		}
		#endregion

		#region Properties
		public FillMode FillMode 
		{
			get 
			{   if(NativeObject.getWindingRule() == GeneralPath.WIND_EVEN_ODD)
					return FillMode.Alternate;
				else
					return FillMode.Winding;
			}

			set 
			{
				if (value == FillMode.Alternate)
					NativeObject.setWindingRule (GeneralPath.WIND_EVEN_ODD);
				else
					NativeObject.setWindingRule (GeneralPath.WIND_NON_ZERO);
			}
		}

		public PathData PathData 
		{
			get { return NativeObject.PathData; }
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

		#region PointCount
		public int PointCount 
		{
			get 
			{
				return NativeObject.PointCount;
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

			NativeObject.append(shape);
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
			int q = (Math.Abs((int)angle))/90;

			switch (q&3) {
				case 1:
					x = 180-x;
					break;
				case 2:
					x = 180+x;
					break;
				case 3:
					x = 360-x;
					break;
			}

			if (angle < 0)
				x = -x;

			x += (((int)angle)/360)*360;

			return x;
		}

		#endregion
		
		#region AddBezier(s)
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
			NativeObject.append(cc);
		}

		public void AddBeziers (Point [] pts)
		{
			if (pts == null)
				throw new ArgumentNullException("points");

			AddBezier(pts [0].X,pts [0].Y,
					pts [1].X,pts [1].Y,
					pts [2].X,pts [2].Y,
					pts [3].X,pts [3].Y);

			for (int i = 4; i < pts.Length; i += 3) {
				NativeObject.curveTo(	
					pts [i].X,pts [i].Y,
					pts [i+1].X,pts [i+1].Y,
					pts [i+2].X,pts [i+2].Y);
			}
		}

		public void AddBeziers (PointF [] pts)
		{
			if (pts == null)
				throw new ArgumentNullException("points");

			AddBezier(pts [0].X,pts [0].Y,
				pts [1].X,pts [1].Y,
				pts [2].X,pts [2].Y,
				pts [3].X,pts [3].Y);

			for (int i = 4; i < pts.Length; i += 3) {
				NativeObject.curveTo(	
					pts [i].X,pts [i].Y,
					pts [i+1].X,pts [i+1].Y,
					pts [i+2].X,pts [i+2].Y);
			}
		}
		#endregion

		#region AddEllipse
		public void AddEllipse (float x, float y, float width, float height)
		{
			Ellipse2D e = new Ellipse2D.Float(x,y,width,height);
			NativeObject.append(e,false);
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
			NativeObject.append(l);
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

			if (NativeObject.LastFigureClosed)
				NativeObject.moveTo(points[0].X, points[0].Y);
			else
				NativeObject.lineTo(points[0].X, points[0].Y);

			for (int i = 1; i < points.Length; i ++)
				NativeObject.lineTo(points[i].X, points[i].Y);
		}

		public void AddLines (PointF [] points)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length == 0)
				return;

			if (NativeObject.LastFigureClosed)
				NativeObject.moveTo(points[0].X, points[0].Y);
			else
				NativeObject.lineTo(points[0].X, points[0].Y);

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
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length < 3)
				throw new ArgumentException("Invalid parameter used.");

			NativeObject.moveTo((float)points[0].X,(float)points[0].Y);
			for (int i = 1; i< points.Length; i++)
			{
				NativeObject.lineTo((float)points[i].X,(float)points[i].Y);
			}
			NativeObject.closePath();
		}

		public void AddPolygon (PointF [] points)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length < 3)
				throw new ArgumentException("Invalid parameter used.");

			NativeObject.moveTo(points[0].X,points[0].Y);
			for (int i = 1; i < points.Length; i++)
			{
				NativeObject.lineTo(points[i].X,points[i].Y);
			}
			NativeObject.closePath();
		}
		#endregion

		#region AddRectangle(s)
		internal void AddRectangle(float x,float y, float w, float h)
		{
			NativeObject.moveTo(x, y);
			NativeObject.lineTo (x + w, y);
			NativeObject.lineTo (x + w, y + h);
			NativeObject.lineTo (x, y + h);
			NativeObject.closePath ();
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
			if (NativeObject.LastFigureClosed || addingPath.NativeObject.LastFigureClosed)
				connect = false;

			NativeObject.append(addingPath.NativeObject,connect);
		}
		#endregion

		#region GetLastPoint
		public PointF GetLastPoint ()
		{
			return NativeObject.GetLastPoint ();
		}
		#endregion

		#region Reset
		public void Reset ()
		{
			NativeObject.reset();
		}
		#endregion

		#region GetBounds
		public RectangleF GetBounds ()
		{
			return GetBounds (null, null);
		}  		

		public RectangleF GetBounds (Matrix matrix)
		{
			return GetBounds (matrix, null);
		}

		public RectangleF GetBounds (Matrix matrix, Pen pen)
		{
			// FIXME : we do not know exacly how the bounding rectangle 
			// is calculated so this implementation obtains different bounds
			// that still contains the path widened by oen and transformed by matrix
			// the order of operations is similar to widening, as .Net does.

			// first get original shape bounds
			//Shape shape = NativeObject.getBounds2D();
			Shape shape = NativeObject;

			// stroke bounds
			if (pen != null)
				shape = ((Stroke)pen).createStrokedShape (shape);

			Rectangle2D rect = shape.getBounds2D ();

			// transform bounds			
			if (matrix != null)
				rect = matrix.NativeObject.createTransformedShape(rect).getBounds2D();

			return new RectangleF (rect);
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
			// LAMESPEC : .Net is currently ignorig Graphics object
			//if (graphics != null && !graphics.IsVisible(x,y))
			//	return false;

			return NativeObject.contains(x,y);
		}
		#endregion
        
		#region Reverse
		public void Reverse ()
		{
			NativeObject.Reverse ();
		}
		#endregion
             
		#region AddClosedCurve
		public void AddClosedCurve (Point [] points)
		{
			AddClosedCurve(points, 0.5f);
		}

		public void AddClosedCurve (PointF [] points)
		{
			AddClosedCurve(points, 0.5f);
		}

		public void AddClosedCurve (Point [] points, float tension)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length < 3)
				throw new ArgumentException("Invalid parameter used.");

			int length = (points.Length + 3)*2;

			float[] pts = new float[length];
			pts[--length] = points[1].Y;
			pts[--length] = points[1].X;
			pts[--length] = points[0].Y;
			pts[--length] = points[0].X;

			for (int i = points.Length-1; i >= 0; i--) {
				pts[--length] = points[i].Y;
				pts[--length] = points[i].X;
			}

			pts[--length] = points[points.Length-1].Y;
			pts[--length] = points[points.Length-1].X;

			AddCurve(pts, !NativeObject.LastFigureClosed, tension);
			CloseFigure ();
		}

		public void AddClosedCurve (PointF [] points, float tension)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			if (points.Length < 3)
				throw new ArgumentException("Invalid parameter used.");

			int length = (points.Length + 3)*2;

			float[] pts = new float[length];
			pts[--length] = points[1].Y;
			pts[--length] = points[1].X;
			pts[--length] = points[0].Y;
			pts[--length] = points[0].X;

			for (int i = points.Length-1; i >= 0; i--) {
				pts[--length] = points[i].Y;
				pts[--length] = points[i].X;
			}

			pts[--length] = points[points.Length-1].Y;
			pts[--length] = points[points.Length-1].X;

			AddCurve(pts, !NativeObject.LastFigureClosed, tension);
			CloseFigure ();
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
			AddCurve(points,0.5F);
		}
                
		public void AddCurve (PointF [] points)
		{
			AddCurve(points,0.5f);
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

			AddCurve(pts, !NativeObject.LastFigureClosed, tension);
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

			AddCurve(pts, !NativeObject.LastFigureClosed, tension);
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

		#region AddString
		public void AddString (string s, FontFamily family, int style,  float emSize,  Point origin,   StringFormat format)
		{
			AddString(s, new Font(family, emSize, (FontStyle)style, GraphicsUnit.World), origin.X, origin.Y, float.PositiveInfinity, float.PositiveInfinity,
				format);
		}  	
                
		public void AddString (string s,  FontFamily family,  int style,  float emSize,  PointF origin,   StringFormat format)
		{
			AddString(s, new Font(family, emSize, (FontStyle)style, GraphicsUnit.World), origin.X, origin.Y, float.PositiveInfinity, float.PositiveInfinity,
				format);
		}  	
  		
		public void AddString (string s, FontFamily family, int style, float emSize,  Rectangle layoutRect, StringFormat format)
		{
			AddString(s, new Font(family, emSize, (FontStyle)style, GraphicsUnit.World),
				layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height,
				format);
		}  	
  		
		public void AddString (string s, FontFamily family, int style, float emSize,  RectangleF layoutRect,   StringFormat format)
		{
			AddString(s, new Font(family, emSize, (FontStyle)style, GraphicsUnit.World),
				layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height,
				format);
		}

		void AddString (string s, Font font,
			float x, float y, float width, float height, 
			StringFormat format) {

			TextLineIterator iter = new TextLineIterator(s, font,
				new java.awt.font.FontRenderContext(null, false, false),
				format, width, height);

			int coordsCount = NativeObject.CoordsCount;

			for (LineLayout layout = iter.NextLine(); layout != null; layout = iter.NextLine()) {
				NativeObject.append(layout.GetOutline(x, y), false);
			}

			AffineTransform lineAlignT = iter.CalcLineAlignmentTransform();
			if (lineAlignT != null)
				NativeObject.transform(lineAlignT, coordsCount, NativeObject.CoordsCount - coordsCount);
		}
		#endregion
                
		#region ClearMarkers
		public void ClearMarkers()               
		{
			NativeObject.ClearMarkers ();
		}
		#endregion
        
		#region Close
		public void CloseAllFigures()
		{
			ExtendedGeneralPath p = new ExtendedGeneralPath();
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

			p.closePath();
			Shape = p;
		}  	
                
		public void CloseFigure() {
			if (!NativeObject.LastFigureClosed)
				NativeObject.closePath();
		}
		#endregion

		#region Flatten
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

			//FIXME : Review (perfomance reasons).
			PathIterator pi = NativeObject.getPathIterator(tr,flatness);
			ExtendedGeneralPath newPath = new ExtendedGeneralPath();
			newPath.append(pi,false);
			Shape = newPath;
		}
		#endregion
        
		#region GetOutlineVisible
		public bool IsOutlineVisible (Point point, Pen pen)
		{
			return IsOutlineVisible (point.X, point.Y, pen, null);
		}  		
		
		public bool IsOutlineVisible (PointF point, Pen pen)
		{
			return IsOutlineVisible (point.X, point.Y, pen, null);
		} 
		
		public bool IsOutlineVisible (int x, int y, Pen pen)
		{
			return IsOutlineVisible (x, y, pen, null);
		}

		public bool IsOutlineVisible (float x, float y, Pen pen)
		{
			return IsOutlineVisible (x, y, pen, null);
		}  		
		
		public bool IsOutlineVisible (Point pt, Pen pen, Graphics graphics)
		{
			return IsOutlineVisible (pt.X, pt.Y, pen, graphics);
		}  		
		
		public bool IsOutlineVisible (PointF pt, Pen pen, Graphics graphics)
		{
			return IsOutlineVisible (pt.X, pt.Y, pen, graphics);
		}  		
				
		public bool IsOutlineVisible (int x, int y, Pen pen, Graphics graphics)
		{
			// LAMESPEC : .Net is currently ignorig Graphics object
			//if (graphics != null) {
			//	if (!graphics.IsVisible (x, y))
			//		return false;				
			//}

			return ((Stroke)pen).createStrokedShape (NativeObject).contains (x, y);
		}  		
				
		public bool IsOutlineVisible (float x, float y, Pen pen, Graphics graphics)
		{
			return ((Stroke)pen).createStrokedShape (NativeObject).contains (x, y);
		}  		
		#endregion
        
		#region SetMarkers 
		public void SetMarkers ()
		{
			NativeObject.SetMarkers ();
		}
		#endregion
                
		#region StartFigure
		public void StartFigure()
		{
			NativeObject.StartFigure ();
		}
		#endregion
  		        
		#region Warp
		[MonoNotSupported ("")]
		public void Warp (PointF[] destPoints, RectangleF srcRect)
		{
			Warp (destPoints, srcRect, null, WarpMode.Perspective, 1.0f / 4.0f);
		}  		

		[MonoNotSupported ("")]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix)
		{
			Warp (destPoints, srcRect, matrix, WarpMode.Perspective, 1.0f / 4.0f);
		}  		

		[MonoNotSupported ("")]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode)
		{
			Warp (destPoints, srcRect, matrix, warpMode, 1.0f / 4.0f);
		}  		

		[MonoNotSupported ("")]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix,  WarpMode warpMode, float flatness)
		{
			throw new NotImplementedException();
		}
		#endregion
        
		#region Widen
		public void Widen (Pen pen)
		{
			Widen (pen, null);
		}  		
                
		public void Widen (Pen pen, Matrix matrix)
		{	
			Widen (pen, matrix, 2f/3f);
		}  		
                		
		public void Widen (Pen pen, Matrix matrix, float flatness)
		{
			if (pen == null)
				throw new ArgumentNullException("pen");

			Shape = new ExtendedGeneralPath(((Stroke)pen).createStrokedShape(this));
			Flatten(matrix, flatness);
		} 
		#endregion

		private void SetPath (Point [] pts, byte [] types)
		{
			NativeObject.Clear ();
			if (((PathPointType)types [0] & PathPointType.PathTypeMask) != PathPointType.Start)
				NativeObject.moveTo (pts [0].X, pts [0].Y);

			for (int i=0; i < pts.Length; i++) {
				switch (((PathPointType)types [i] & PathPointType.PathTypeMask)) {
					case PathPointType.Start :
						NativeObject.moveTo (pts [i].X, pts [i].Y);
						break;
					case PathPointType.Line :
						NativeObject.lineTo (pts [i].X, pts [i].Y);
						break;
					case PathPointType.Bezier3 :
						float x1 = pts [i].X;
						float y1 = pts [i].Y;
						i++;
						float x2 = pts [i].X;
						float y2 = pts [i].Y;
						i++;
						float x3 = pts [i].X;
						float y3 = pts [i].Y;
						NativeObject.curveTo (x1,y1, x2, y2, x3, y3);
						break;						
				}
				if (((PathPointType)types [i] & PathPointType.CloseSubpath) != 0)
					NativeObject.closePath();

				if (((PathPointType)types [i] & PathPointType.PathMarker) != 0)
					NativeObject.SetMarkers ();
			}
		}

		internal void SetPath (PointF [] pts, byte [] types)
		{
			NativeObject.Clear ();
			if (((PathPointType)types [0] & PathPointType.PathTypeMask) != PathPointType.Start)
				NativeObject.moveTo (pts [0].X, pts [0].Y);
			for (int i=0; i < pts.Length; i++) {
				switch (((PathPointType)types [i] & PathPointType.PathTypeMask)) {
					case PathPointType.Start :
						NativeObject.moveTo (pts [i].X, pts [i].Y);
						break;
					case PathPointType.Line :
						NativeObject.lineTo (pts [i].X, pts [i].Y);
						break;
					case PathPointType.Bezier3 :
						float x1 = pts [i].X;
						float y1 = pts [i].Y;
						i++;
						float x2 = pts [i].X;
						float y2 = pts [i].Y;
						i++;
						float x3 = pts [i].X;
						float y3 = pts [i].Y;
						NativeObject.curveTo (x1,y1, x2, y2, x3, y3);
						break;						
				}
				if (((PathPointType)types [i] & PathPointType.CloseSubpath) != 0)
					NativeObject.closePath();

				if (((PathPointType)types [i] & PathPointType.PathMarker) != 0)
					NativeObject.SetMarkers ();
			}
		}
	}
}
