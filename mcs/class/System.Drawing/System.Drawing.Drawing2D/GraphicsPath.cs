//
// System.Drawing.Drawing2D.GraphicsPath.cs
//
// Authors:
//
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2004 Novell, Inc
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
        public sealed class GraphicsPath : MarshalByRefObject, ICloneable, IDisposable {

                internal IntPtr nativePath;

                GraphicsPath (IntPtr ptr)
                {
                        nativePath = ptr;
                }
                           		
                public GraphicsPath ()
                {
                        GDIPlus.GdipCreatePath (FillMode.Alternate, out nativePath);
                }
	
                public object Clone ()
                {
                        IntPtr clone;

                        GDIPlus.GdipClonePath (nativePath, out clone);

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
                                GDIPlus.GdipGetPathFillMode (nativePath, out mode);
                                return mode;
                        }

                        set {
                                GDIPlus.GdipSetPathFillMode (nativePath, value);
                        }
                }

                public PathData PathData {

                        get {
                                IntPtr tmp;
                                GDIPlus.GdipGetPathData (nativePath, out tmp);

                                throw new Exception ();
                        }
                }

                public PointF [] PathPoints {

                        get {
                                int count;
                        
                                GDIPlus.GdipGetPointCount (nativePath, out count);

                                PointF [] points = new PointF [count];

                                GDIPlus.GdipGetPathPoints (nativePath, points, count); 

                                return points;
                        }
                }

                public byte [] PathTypes {

                        get {
                                int count;
                                GDIPlus.GdipGetPointCount (nativePath, out count);

                                byte [] types = new byte [count];
                                GDIPlus.GdipGetPathTypes (nativePath, types, count);

                                return types;
                        }
                }

                public int PointCount {

                        get {
                                int count;

                                GDIPlus.GdipGetPointCount (nativePath, out count);

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
                        GDIPlus.GdipAddPathArcI (nativePath, rect.X, rect.Y, rect.Width, rect.Height, start_angle, sweep_angle);
                }

                public void AddArc (RectangleF rect, float start_angle, float sweep_angle)
                {
                        GDIPlus.GdipAddPathArc (nativePath, rect.X, rect.Y, rect.Width, rect.Height, start_angle, sweep_angle);
                }

                public void AddArc (int x, int y, int width, int height, float start_angle, float sweep_angle)
                {
                        GDIPlus.GdipAddPathArcI (nativePath, x, y, width, height, start_angle, sweep_angle);                
                }

                public void AddArc (float x, float y, float width, float height, float start_angle, float sweep_angle)
                {
                        GDIPlus.GdipAddPathArc (nativePath, x, y, width, height, start_angle, sweep_angle);
                }

                //
                // AddBezier
                //
                public void AddBezier (Point pt1, Point pt2, Point pt3, Point pt4)
                {
                        GDIPlus.GdipAddPathBezierI (nativePath, pt1.X, pt1.Y,
                                        pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                }

                public void AddBezier (PointF pt1, PointF pt2, PointF pt3, PointF pt4)
                {
                        GDIPlus.GdipAddPathBezier (nativePath, pt1.X, pt1.Y,
                                        pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                }

                public void AddBezier (int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
                {
                        GDIPlus.GdipAddPathBezierI (nativePath, x1, y1, x2, y2, x3, y3, x4, y4);
                }

                public void AddBezier (float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
                {
                        GDIPlus.GdipAddPathBezier (nativePath, x1, y1, x2, y2, x3, y3, x4, y4);
                }

                //
                // AddBeziers
                //
                public void AddBeziers (Point [] pts)
                {
                        GDIPlus.GdipAddPathBeziersI (nativePath, pts, pts.Length);
                }

                public void AddBeziers (PointF [] pts)
                {
                        GDIPlus.GdipAddPathBeziers (nativePath, pts, pts.Length);
                }

                //
                // AddEllipse
                //
                public void AddEllipse (RectangleF r)
                {
                        GDIPlus.GdipAddPathEllipse (nativePath, r.X, r.Y, r.Width, r.Height);
                }
                
                public void AddEllipse (float x, float y, float width, float height)
                {
                        GDIPlus.GdipAddPathEllipse (nativePath, x, y, width, height);
                }

                public void AddEllipse (Rectangle r)
                {
                        GDIPlus.GdipAddPathEllipseI (nativePath, r.X, r.Y, r.Width, r.Height);
                }
                
                public void AddEllipse (int x, int y, int width, int height)
                {
                        GDIPlus.GdipAddPathEllipseI (nativePath, x, y, width, height);
                }
                

                //
                // AddLine
                //
                public void AddLine (Point a, Point b)
                {
                        GDIPlus.GdipAddPathLineI (nativePath, a.X, a.Y, b.X, b.Y);
                }

                public void AddLine (PointF a, PointF b)
                {
                        GDIPlus.GdipAddPathLine (nativePath, a.X, a.Y, b.X,
                                        b.Y);
                }

                public void AddLine (int x1, int y1, int x2, int y2)
                {
                        GDIPlus.GdipAddPathLineI (nativePath, x1, y1, x2, y2);
                }

                public void AddLine (float x1, float y1, float x2, float y2)
                {
                        GDIPlus.GdipAddPathLine (nativePath, x1, y1, x2,
                                        y2);                
                }

                //
                // AddLines
                //
                public void AddLines (Point [] points)
                {
                        int length = points.Length;

                        for (int i = 0; i < length - 2; i += 2) {
                                int j = i + 1;
                                GDIPlus.GdipAddPathLineI (nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                        }
                }

                public void AddLines (PointF [] points)
                {
                        int length = points.Length;

                        for (int i = 0; i < length - 2; i += 2) {
                                int j = i + 1;
                                GDIPlus.GdipAddPathLine (nativePath, points [i].X, points [i].Y, points [j].X, points [j].Y);
                        }
                }
        
                //
                // AddPie
                //
                public void AddPie (Rectangle rect, float startAngle, float sweepAngle)
                {
                        GDIPlus.GdipAddPathPie (nativePath, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
                }

                public void AddPie (int x, int y, int width, int height, float startAngle, float sweepAngle)
                {
                        GDIPlus.GdipAddPathPie (nativePath, x, y, width, height, startAngle, sweepAngle);
                }

                public void AddPie (float x, float y, float width, float height, float startAngle, float sweepAngle)
                {
                        GDIPlus.GdipAddPathPie (nativePath, x, y, width, height, startAngle, sweepAngle);                
                }

                //
                // AddPolygon
                //
                public void AddPolygon (Point [] points)
                {

                        GDIPlus.GdipAddPathPolygonI (nativePath, points, points.Length);
                }

                public void AddPolygon (PointF [] points)
                {
                        GDIPlus.GdipAddPathPolygon (nativePath, points, points.Length);
                }

                //
                // AddRectangle
                //
                public void AddRectangle (Rectangle rect)
                {
                        GDIPlus.GdipAddPathRectangleI (nativePath, rect.X, rect.Y, rect.Width, rect.Height);
                }

                public void AddRectangle (RectangleF rect)
                {
                        GDIPlus.GdipAddPathRectangle (nativePath, rect.X, rect.Y, rect.Width, rect.Height);
                }

                //
                // AddRectangles
                //
                public void AddRectangles (Rectangle [] rects)
                {
                        GDIPlus.GdipAddPathRectanglesI (nativePath, rects, rects.Length);
                }

                public void AddRectangles (RectangleF [] rects)
                {
                        GDIPlus.GdipAddPathRectangles (nativePath, rects, rects.Length);
                }

                //
                // AddPath
                //
                public void AddPath (GraphicsPath addingPath, bool connect)
                {
                        GDIPlus.GdipAddPathPath (nativePath, addingPath.nativePath, connect);
                }

                public PointF GetLastPoint ()
                {
                        PointF pt;
                        GDIPlus.GdipGetPathLastPoint (nativePath, out pt);

                        return pt;
                }

                //
                // AddClosedCurve
                //
                public void AddClosedCurve (Point [] points)
                {
                        GDIPlus.GdipAddPathClosedCurveI (nativePath, points, points.Length);
                }

                public void AddClosedCurve (PointF [] points)
                {
                        GDIPlus.GdipAddPathClosedCurve (nativePath, points, points.Length);
                }

                public void AddClosedCurve (Point [] points, float tension)
                {
                        GDIPlus.GdipAddPathClosedCurve2I (nativePath, points, points.Length, tension);
                }

                public void AddClosedCurve (PointF [] points, float tension)
                {
                        GDIPlus.GdipAddPathClosedCurve2 (nativePath, points, points.Length, tension);
                }

                //
                // AddCurve
                //
                public void AddCurve (Point [] points)
                {
                        GDIPlus.GdipAddPathCurveI (nativePath, points, points.Length);
                }
                
                public void AddCurve (PointF [] points)
                {
                        GDIPlus.GdipAddPathCurve (nativePath, points, points.Length);
                }
                
                public void AddCurve (Point [] points, float tension)
                {
                        GDIPlus.GdipAddPathCurve2I (nativePath, points, points.Length, tension);
                }
                
                public void AddCurve (PointF [] points, float tension)
                {
                        GDIPlus.GdipAddPathCurve2 (nativePath, points, points.Length, tension);
                }

                public void AddCurve (Point [] points, int offset, int numberOfSegments, float tension)
                {
                        GDIPlus.GdipAddPathCurve3I (nativePath, points, points.Length,
                                        offset, numberOfSegments, tension);
                }
                
                public void AddCurve (PointF [] points, int offset, int numberOfSegments, float tension)
                {
                        GDIPlus.GdipAddPathCurve3 (nativePath, points, points.Length,
                                        offset, numberOfSegments, tension);
                }
                        
                public void Reset ()
                {
                        GDIPlus.GdipResetPath (nativePath);
                }

                public void Reverse ()
                {
                        GDIPlus.GdipReversePath (nativePath);
                }

                public void Transform (Matrix matrix)
                {
                        GDIPlus.GdipTransformPath (nativePath, matrix.nativeMatrix);
                }
        }
}


