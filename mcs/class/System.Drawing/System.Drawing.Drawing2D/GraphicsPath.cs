//
// System.Drawing.Drawing2D.GraphicsPath.cs
//
// Authors:
//
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2004 Novell, Inc
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
        public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable {

                internal IntPtr nativePath = IntPtr.Zero;

                GraphicsPath (IntPtr ptr)
                {
                        nativePath = ptr;
                }
                           		
                public GraphicsPath ()
                {
                        Status status = GDIPlus.GdipCreatePath (FillMode.Alternate, out nativePath);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                [MonoTODO]
                public GraphicsPath (FillMode fillMode)
                {
                	throw new NotImplementedException ();
                }
                
                [MonoTODO]
                public GraphicsPath (Point[] pts, byte[] types)
                {
                	throw new NotImplementedException ();
                }
                
                [MonoTODO]
                public GraphicsPath (PointF[] pts, byte[] types)
                {
                	throw new NotImplementedException ();
                }
                
                [MonoTODO]
		public GraphicsPath (Point[] pts,  byte[] types,  FillMode fillMode)
		{
                	throw new NotImplementedException ();	
                }

		[MonoTODO]
		public GraphicsPath(PointF[] pts,  byte[] types,   FillMode fillMode)
		{
                	throw new NotImplementedException ();
                }
	
		
                public object Clone ()
                {
                        IntPtr clone;

                        Status status = GDIPlus.GdipClonePath (nativePath, out clone);
                        GDIPlus.CheckStatus (status);                      	

                        return new GraphicsPath (clone);
                }

                public void Dispose ()
                {
                        Dispose (true);
                        System.GC.SuppressFinalize (this);
                }

                ~GraphicsPath ()
                {
                        Dispose (false);
                }
                
                void Dispose (bool disposing)
                {
		
                }


                public FillMode FillMode {
                        get {

                                FillMode mode;
                                Status status = GDIPlus.GdipGetPathFillMode (nativePath, out mode);
                                GDIPlus.CheckStatus (status);                      	
                                
                                return mode;
                        }

                        set {
                                Status status = GDIPlus.GdipSetPathFillMode (nativePath, value);
                                GDIPlus.CheckStatus (status);                      	
                        }
                }

                public PathData PathData {

                        get {
                                IntPtr tmp;
                                Status status = GDIPlus.GdipGetPathData (nativePath, out tmp);
                                GDIPlus.CheckStatus (status);                      	

                                throw new Exception ();
                        }
                }

                public PointF [] PathPoints {

                        get {
                                int count;
                        
                                Status status = GDIPlus.GdipGetPointCount (nativePath, out count);
                                GDIPlus.CheckStatus (status);                      	

                                PointF [] points = new PointF [count];

                                status = GDIPlus.GdipGetPathPoints (nativePath, points, count); 
                                GDIPlus.CheckStatus (status);                      	

                                return points;
                        }
                }

                public byte [] PathTypes {

                        get {
                                int count;
                                Status status = GDIPlus.GdipGetPointCount (nativePath, out count);
                                GDIPlus.CheckStatus (status);                      	

                                byte [] types = new byte [count];
                                status = GDIPlus.GdipGetPathTypes (nativePath, types, count);
                                GDIPlus.CheckStatus (status);                      	

                                return types;
                        }
                }

                public int PointCount {

                        get {
                                int count;

                                Status status = GDIPlus.GdipGetPointCount (nativePath, out count);
                                GDIPlus.CheckStatus (status);                      	

                                return count;
                        }
                }
                
                internal IntPtr NativeObject{
                
					get{
							return nativePath;
					}
					set	{
							nativePath = value;
					}
				}
        
                //
                // AddArc
                //
                public void AddArc (Rectangle rect, float start_angle, float sweep_angle)
                {
                        Status status = GDIPlus.GdipAddPathArcI (nativePath, rect.X, rect.Y, rect.Width, rect.Height, start_angle, sweep_angle);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddArc (RectangleF rect, float start_angle, float sweep_angle)
                {
                        Status status = GDIPlus.GdipAddPathArc (nativePath, rect.X, rect.Y, rect.Width, rect.Height, start_angle, sweep_angle);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddArc (int x, int y, int width, int height, float start_angle, float sweep_angle)
                {
                        Status status = GDIPlus.GdipAddPathArcI (nativePath, x, y, width, height, start_angle, sweep_angle);                
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddArc (float x, float y, float width, float height, float start_angle, float sweep_angle)
                {
                        Status status = GDIPlus.GdipAddPathArc (nativePath, x, y, width, height, start_angle, sweep_angle);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddBezier
                //
                public void AddBezier (Point pt1, Point pt2, Point pt3, Point pt4)
                {
                        Status status = GDIPlus.GdipAddPathBezierI (nativePath, pt1.X, pt1.Y,
                                        pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                                        
			GDIPlus.CheckStatus (status);                      		                                      
                }

                public void AddBezier (PointF pt1, PointF pt2, PointF pt3, PointF pt4)
                {
                        Status status = GDIPlus.GdipAddPathBezier (nativePath, pt1.X, pt1.Y,
                                        pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                                        
			GDIPlus.CheckStatus (status);                      	                                       
                }

                public void AddBezier (int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
                {
                        Status status = GDIPlus.GdipAddPathBezierI (nativePath, x1, y1, x2, y2, x3, y3, x4, y4);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddBezier (float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
                {
                        Status status = GDIPlus.GdipAddPathBezier (nativePath, x1, y1, x2, y2, x3, y3, x4, y4);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddBeziers
                //
                public void AddBeziers (Point [] pts)
                {
                        Status status = GDIPlus.GdipAddPathBeziersI (nativePath, pts, pts.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddBeziers (PointF [] pts)
                {
                        Status status = GDIPlus.GdipAddPathBeziers (nativePath, pts, pts.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddEllipse
                //
                public void AddEllipse (RectangleF r)
                {
                        Status status = GDIPlus.GdipAddPathEllipse (nativePath, r.X, r.Y, r.Width, r.Height);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                public void AddEllipse (float x, float y, float width, float height)
                {
                        Status status = GDIPlus.GdipAddPathEllipse (nativePath, x, y, width, height);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddEllipse (Rectangle r)
                {
                        Status status = GDIPlus.GdipAddPathEllipseI (nativePath, r.X, r.Y, r.Width, r.Height);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                public void AddEllipse (int x, int y, int width, int height)
                {
                        Status status = GDIPlus.GdipAddPathEllipseI (nativePath, x, y, width, height);
                        GDIPlus.CheckStatus (status);                      	
                }
                

                //
                // AddLine
                //
                public void AddLine (Point a, Point b)
                {
                        Status status = GDIPlus.GdipAddPathLineI (nativePath, a.X, a.Y, b.X, b.Y);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddLine (PointF a, PointF b)
                {
                        Status status = GDIPlus.GdipAddPathLine (nativePath, a.X, a.Y, b.X,
                                        b.Y);
                                        
			GDIPlus.CheckStatus (status);                      	                                       
                }

                public void AddLine (int x1, int y1, int x2, int y2)
                {
                        Status status = GDIPlus.GdipAddPathLineI (nativePath, x1, y1, x2, y2);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddLine (float x1, float y1, float x2, float y2)
                {
                        Status status = GDIPlus.GdipAddPathLine (nativePath, x1, y1, x2,
                                        y2);                
                                        
			GDIPlus.CheckStatus (status);                      	                                       
                }

                //
                // AddLines
                //
                public void AddLines (Point [] points)
                {
                        int length = points.Length;

                        for (int i = 0; i < length - 2; i += 2) {
                                int j = i + 1;
                                Status status = GDIPlus.GdipAddPathLineI (nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                                GDIPlus.CheckStatus (status);                      	
                        }
                }

                public void AddLines (PointF [] points)
                {
                        int length = points.Length;

                        for (int i = 0; i < length - 2; i += 2) {
                                int j = i + 1;
                                Status status = GDIPlus.GdipAddPathLine (nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                                GDIPlus.CheckStatus (status);                      	
                        }
                }
        
                //
                // AddPie
                //
                public void AddPie (Rectangle rect, float startAngle, float sweepAngle)
                {
                        Status status = GDIPlus.GdipAddPathPie (nativePath, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddPie (int x, int y, int width, int height, float startAngle, float sweepAngle)
                {
                        Status status = GDIPlus.GdipAddPathPie (nativePath, x, y, width, height, startAngle, sweepAngle);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddPie (float x, float y, float width, float height, float startAngle, float sweepAngle)
                {
                        Status status = GDIPlus.GdipAddPathPie (nativePath, x, y, width, height, startAngle, sweepAngle);                
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddPolygon
                //
                public void AddPolygon (Point [] points)
                {
                        Status status = GDIPlus.GdipAddPathPolygonI (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddPolygon (PointF [] points)
                {
                        Status status = GDIPlus.GdipAddPathPolygon (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddRectangle
                //
                public void AddRectangle (Rectangle rect)
                {
                        Status status = GDIPlus.GdipAddPathRectangleI (nativePath, rect.X, rect.Y, rect.Width, rect.Height);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddRectangle (RectangleF rect)
                {
                        Status status = GDIPlus.GdipAddPathRectangle (nativePath, rect.X, rect.Y, rect.Width, rect.Height);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddRectangles
                //
                public void AddRectangles (Rectangle [] rects)
                {
                        Status status = GDIPlus.GdipAddPathRectanglesI (nativePath, rects, rects.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddRectangles (RectangleF [] rects)
                {
                        Status status = GDIPlus.GdipAddPathRectangles (nativePath, rects, rects.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddPath
                //
                public void AddPath (GraphicsPath addingPath, bool connect)
                {
                        Status status = GDIPlus.GdipAddPathPath (nativePath, addingPath.nativePath, connect);
                        GDIPlus.CheckStatus (status);                      	
                }

                public PointF GetLastPoint ()
                {
                        PointF pt;
                        Status status = GDIPlus.GdipGetPathLastPoint (nativePath, out pt);
                        GDIPlus.CheckStatus (status);                      	

                        return pt;
                }

                //
                // AddClosedCurve
                //
                public void AddClosedCurve (Point [] points)
                {
                        Status status = GDIPlus.GdipAddPathClosedCurveI (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddClosedCurve (PointF [] points)
                {
                        Status status = GDIPlus.GdipAddPathClosedCurve (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddClosedCurve (Point [] points, float tension)
                {
                        Status status = GDIPlus.GdipAddPathClosedCurve2I (nativePath, points, points.Length, tension);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddClosedCurve (PointF [] points, float tension)
                {
                        Status status = GDIPlus.GdipAddPathClosedCurve2 (nativePath, points, points.Length, tension);
                        GDIPlus.CheckStatus (status);                      	
                }

                //
                // AddCurve
                //
                public void AddCurve (Point [] points)
                {
                        Status status = GDIPlus.GdipAddPathCurveI (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                public void AddCurve (PointF [] points)
                {
                        Status status = GDIPlus.GdipAddPathCurve (nativePath, points, points.Length);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                public void AddCurve (Point [] points, float tension)
                {
                        Status status = GDIPlus.GdipAddPathCurve2I (nativePath, points, points.Length, tension);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                public void AddCurve (PointF [] points, float tension)
                {
                        Status status = GDIPlus.GdipAddPathCurve2 (nativePath, points, points.Length, tension);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddCurve (Point [] points, int offset, int numberOfSegments, float tension)
                {
                        Status status = GDIPlus.GdipAddPathCurve3I (nativePath, points, points.Length,
                                        offset, numberOfSegments, tension);
                                        
			GDIPlus.CheckStatus (status);                      	                                       
                }
                
                public void AddCurve (PointF [] points, int offset, int numberOfSegments, float tension)
                {
                        Status status = GDIPlus.GdipAddPathCurve3 (nativePath, points, points.Length,
                                        offset, numberOfSegments, tension);
                                        
			GDIPlus.CheckStatus (status);                      	                                       
                }
                        
                public void Reset ()
                {
                        Status status = GDIPlus.GdipResetPath (nativePath);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void Reverse ()
                {
                        Status status = GDIPlus.GdipReversePath (nativePath);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void Transform (Matrix matrix)
                {
                        Status status = GDIPlus.GdipTransformPath (nativePath, matrix.nativeMatrix);
                        GDIPlus.CheckStatus (status);                      	
                }
                
                [MonoTODO]
                public void AddString (string s, FontFamily family, int style,  float emSize,  Point origin,   StringFormat format)
                {
                	throw new NotImplementedException ();
                }  	
                
                [MonoTODO]
  		public void AddString (string s,  FontFamily family,  int style,  float emSize,  PointF origin,   StringFormat format)
  		{
                	throw new NotImplementedException ();
                }  	
  		
  		[MonoTODO]
  		public void AddString (string s, FontFamily family, int style, float emSize,  Rectangle layoutRect, StringFormat format)
  		{
                	throw new NotImplementedException ();
                }  	
  		
  		[MonoTODO]
  		public void AddString (string s, FontFamily family, int style, float emSize,  RectangleF layoutRect,   StringFormat format)
  		{
                	throw new NotImplementedException ();
                }  	
                
                [MonoTODO]
		public void ClearMarkers()               
		{
                	throw new NotImplementedException ();
                }  	
                
                [MonoTODO]
		public void CloseAllFigures()
		{
                	throw new NotImplementedException ();
                }  	
                
                [MonoTODO]
                public void Flatten ()
                {
                	throw new NotImplementedException ();
                }  	
  
  		[MonoTODO]
		public void Flatten (Matrix matrix)
		{
                	throw new NotImplementedException ();
                }  	
		
		[MonoTODO]
		public void Flatten (Matrix matrix, float flatness)
		{
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public RectangleF GetBounds ()
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public RectangleF GetBounds (Matrix matrix)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public RectangleF GetBounds (Matrix matrix, Pen pen)
                {
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (Point point,  Pen pen)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (PointF point,  Pen pen)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (int x, int y, Pen pen)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (Point pt, Pen pen, Graphics graphics)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (PointF pt, Pen pen, Graphics graphics)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (float x, float y, Pen pen)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (int x, int y, Pen pen, Graphics graphics)
		{
                	throw new NotImplementedException ();
                }  		
		
		[MonoTODO]
		public bool IsOutlineVisible (float x, float y, Pen pen, Graphics graphics)
		{
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (Point point)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (PointF point)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (int x, int y)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (Point pt, Graphics graphics)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (PointF pt, Graphics graphics)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (float x, float y)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (int x, int y, Graphics graphics)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public bool IsVisible (float x, float y, Graphics graphics)
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public void SetMarkers()
                {
                	throw new NotImplementedException ();
                }
                
                [MonoTODO]
                public void StartFigure()
                {
                	throw new NotImplementedException ();
                }  		
                
                [MonoTODO]
                public void Warp (PointF[] destPoints,  RectangleF srcRect)
                {
                	throw new NotImplementedException ();
                }  		

		[MonoTODO]
		public void Warp (PointF[] destPoints, RectangleF srcRect,  Matrix matrix)
		{
                	throw new NotImplementedException ();
                }  		

		[MonoTODO]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix, WarpMode warpMode)
		{
                	throw new NotImplementedException ();
                }  		

		[MonoTODO]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix,  WarpMode warpMode, float flatness)
		{
                	throw new NotImplementedException ();
                }  		                
                
                [MonoTODO]
                public void Widen (Pen pen)
		{
                	throw new NotImplementedException ();
                }  		
                
		[MonoTODO]
		public void Widen (Pen pen,  Matrix matrix)
		{	
                	throw new NotImplementedException ();
                }  		
                
		[MonoTODO]
		public void Widen (Pen pen, Matrix matrix, float flatness)
                {
                	throw new NotImplementedException ();
                }  		            

        }
}


