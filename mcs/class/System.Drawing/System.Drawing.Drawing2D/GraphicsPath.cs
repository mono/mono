//
// System.Drawing.Drawing2D.GraphicsPath.cs
//
// Authors:
//
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//   Ravindra (rkumar@novell.com)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
	public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable
	{
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

		public GraphicsPath (FillMode fillMode)
		{
                        Status status = GDIPlus.GdipCreatePath (fillMode, out nativePath);
                        GDIPlus.CheckStatus (status);
		}

		public GraphicsPath (Point[] pts, byte[] types)
		{
			Status status;
			if (pts.Length != types.Length)
				throw new ArgumentException ("Invalid parameter passed. Number of points and types must be same.");

			status = GDIPlus.GdipCreatePath2I (pts, types, pts.Length, FillMode.Alternate, out nativePath);
			GDIPlus.CheckStatus (status);
		}

		public GraphicsPath (PointF[] pts, byte[] types)
		{
			Status status;
			if (pts.Length != types.Length)
				throw new ArgumentException ("Invalid parameter passed. Number of points and types must be same.");

			status = GDIPlus.GdipCreatePath2 (pts, types, pts.Length, FillMode.Alternate, out nativePath);
			GDIPlus.CheckStatus (status);
		}

		public GraphicsPath (Point[] pts, byte[] types, FillMode fillMode)
		{
			Status status;
			if (pts.Length != types.Length)
				throw new ArgumentException ("Invalid parameter passed. Number of points and types must be same.");

			status = GDIPlus.GdipCreatePath2I (pts, types, pts.Length, fillMode, out nativePath);
			GDIPlus.CheckStatus (status);
		}

		public GraphicsPath (PointF[] pts, byte[] types, FillMode fillMode)
		{
			Status status;
			if (pts.Length != types.Length)
				throw new ArgumentException ("Invalid parameter passed. Number of points and types must be same.");

			status = GDIPlus.GdipCreatePath2 (pts, types, pts.Length, fillMode, out nativePath);
			GDIPlus.CheckStatus (status);
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
			Status status;
			if (nativePath != IntPtr.Zero) {
				status = GDIPlus.GdipDeletePath (nativePath);
				GDIPlus.CheckStatus (status);

				nativePath = IntPtr.Zero;
			}
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
				PathData pdata = new PathData ();
				pdata.Points = PathPoints;
				pdata.Types = PathTypes;
				return pdata;
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

		internal IntPtr NativeObject {
			get {
				return nativePath;
			}
			set {
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

                        for (int i = 0; i < length - 1; i++) {
                                int j = i + 1;
                                Status status = GDIPlus.GdipAddPathLineI (
                                        nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                                GDIPlus.CheckStatus (status);                      	
                        }
                }

                public void AddLines (PointF [] points)
                {
                        int length = points.Length;

                        for (int i = 0; i < length - 1; i++) {
                                int j = i + 1;
                                Status status = GDIPlus.GdipAddPathLine (
                                        nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                                GDIPlus.CheckStatus (status);                      	
                        }
                }
        
                //
                // AddPie
                //
                public void AddPie (Rectangle rect, float startAngle, float sweepAngle)
                {
                        Status status = GDIPlus.GdipAddPathPie (
                                nativePath, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
                        GDIPlus.CheckStatus (status);                      	
                }

                public void AddPie (int x, int y, int width, int height, float startAngle, float sweepAngle)
                {
                        Status status = GDIPlus.GdipAddPathPieI (nativePath, x, y, width, height, startAngle, sweepAngle);
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
                
		public void ClearMarkers()               
		{
                	Status s = GDIPlus.GdipClearPathMarkers (nativePath);

                        GDIPlus.CheckStatus (s);
                }
                
		public void CloseAllFigures()
		{
                	Status s = GDIPlus.GdipClosePathFigures (nativePath);

                        GDIPlus.CheckStatus (s);
                }  	
                
		public void CloseFigure()
		{
                	Status s = GDIPlus.GdipClosePathFigure (nativePath);

                        GDIPlus.CheckStatus (s);
                } 

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
                	Status status = GDIPlus.GdipFlattenPath (nativePath, matrix.nativeMatrix, flatness);

                        GDIPlus.CheckStatus (status);
                }  		
                
                public RectangleF GetBounds ()
                {
                	return GetBounds (null, null);
                }  		

                public RectangleF GetBounds (Matrix matrix)
                {
                	return GetBounds (matrix, null);
                }

                [MonoTODO]
                public RectangleF GetBounds (Matrix matrix, Pen pen)
                {
                        RectangleF retval;
                        IntPtr m = (matrix == null) ? IntPtr.Zero : matrix.nativeMatrix;
                        IntPtr p = (pen == null) ? IntPtr.Zero : pen.nativeObject;
                        
                        Status s = GDIPlus.GdipGetPathWorldBounds (nativePath, out retval, m, p);

                        GDIPlus.CheckStatus (s);

                        return retval;
                }

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
				
		[MonoTODO]
		public bool IsOutlineVisible (int x, int y, Pen pen, Graphics graphics)
		{
                        bool result;
                        IntPtr g = (graphics == null) ? IntPtr.Zero : graphics.nativeObject;
                        
                	Status s = GDIPlus.GdipIsOutlineVisiblePathPointI (nativePath, x, y, g, out result);
                        GDIPlus.CheckStatus (s);

                        return result;
                }  		

		[MonoTODO]
		public bool IsOutlineVisible (float x, float y, Pen pen, Graphics graphics)
		{
                        bool result;
                        IntPtr g = (graphics == null) ? IntPtr.Zero : graphics.nativeObject;
                        
                	Status s = GDIPlus.GdipIsOutlineVisiblePathPoint (nativePath, x, y, g, out result);
                        GDIPlus.CheckStatus (s);

                        return result;
                }  		
                
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
                                
                [MonoTODO]
                public bool IsVisible (int x, int y, Graphics graphics)
                {
                        bool retval;

                	IntPtr g = (graphics == null) ? IntPtr.Zero : graphics.nativeObject;

                        Status s = GDIPlus.GdipIsVisiblePathPointI (nativePath, x, y, g, out retval);

                        GDIPlus.CheckStatus (s);

                        return retval;
                }  		
                
                [MonoTODO]
                public bool IsVisible (float x, float y, Graphics graphics)
                {
                        bool retval;

                	IntPtr g = (graphics == null) ? IntPtr.Zero : graphics.nativeObject;

                        Status s = GDIPlus.GdipIsVisiblePathPoint (nativePath, x, y, g, out retval);

                        GDIPlus.CheckStatus (s);

                        return retval;
                }  		
                
                public void SetMarkers ()
                {
                	Status s = GDIPlus.GdipSetPathMarker (nativePath);

                        GDIPlus.CheckStatus (s);
                }
                
                public void StartFigure()
                {
                	Status s = GDIPlus.GdipStartPathFigure (nativePath);

                        GDIPlus.CheckStatus (s);
                }  		
                
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

		[MonoTODO]
		public void Warp (PointF[] destPoints, RectangleF srcRect, Matrix matrix,  WarpMode warpMode, float flatness)
		{
                	IntPtr m = (matrix == null) ? IntPtr.Zero : matrix.nativeMatrix;

                        Status s = GDIPlus.GdipWarpPath (nativePath, m, destPoints, destPoints.Length,
                                        srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, warpMode, flatness);

                        GDIPlus.CheckStatus (s);
                }
                
                public void Widen (Pen pen)
		{
                	Widen (pen, null, 1.0f / 4.0f);
                }  		
                
		public void Widen (Pen pen, Matrix matrix)
		{	
                	Widen (pen, matrix, 1.0f / 4.0f);
                }  		
                
		[MonoTODO]
		public void Widen (Pen pen, Matrix matrix, float flatness)
                {
                        IntPtr p = (pen == null) ? IntPtr.Zero : pen.nativeObject;
                	IntPtr m = (matrix == null) ? IntPtr.Zero : matrix.nativeMatrix;

                        Status s = GDIPlus.GdipWidenPath (nativePath, p, m, flatness);

                        GDIPlus.CheckStatus (s);
                } 
        }
}


