/*
 * graphics-path.h
 *
 * Author:
 *      Duncan Mak (duncan@ximian.com)
 *
 * Copyright (C) Novell, Inc. 2003.
 */


#ifndef _GRAPHICS_PATH_H_
#define _GRAPHICS_PATH_H_

#include "gdip.h"

GpStatus GdipCreatePath (GpFillMode brushMode, GpPath **path);
GpStatus GdipCreatePath2 (const GpPointF *points, const byte *types, int count, GpFillMode fillMode, GpPath **path);
GpStatus GdipClonePath (GpPath *path, GpPath **clonePath);
GpStatus GdipDeletePath (GpPath *path);
GpStatus GdipResetPath (GpPath *path);
GpStatus GdipGetPointCount (GpPath *path, int *count);
GpStatus GdipGetPathTypes (GpPath *path, byte *types, int count);
GpStatus GdipGetPathPoints (GpPath *path, GpPointF *points, int count);
GpStatus GdipGetPathPointsI (GpPath *path, GpPoint *points, int count);
GpStatus GdipGetPathFillMode (GpPath *path, GpFillMode *fillmode);
GpStatus GdipSetPathFillMode (GpPath *path, GpFillMode fillmode);
GpStatus GdipGetPathData (GpPath *path, GpPathData *pathData);
GpStatus GdipStartPathFigure (GpPath *path);
GpStatus GdipClosePathFigure (GpPath *path);
GpStatus GdipClosePathFigures (GpPath *path);
GpStatus GdipSetPathMarker (GpPath *path);
GpStatus GdipClearPathMarker (GpPath *path);
GpStatus GdipReversePath (GpPath *path);
GpStatus GdipGetPathLastPoint (GpPath *path, GpPointF *lastPoint);
GpStatus GdipAddPathLine (GpPath *path, float x1, float y1, float x2, float y2);
GpStatus GdipAddPathLine2 (GpPath *path, const GpPointF *points, int count);
GpStatus GdipAddPathArc (GpPath *path, float x, float y, float width, float height, float startAngle, float sweepAngle);
GpStatus GdipAddPathBezier (GpPath *path, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
GpStatus GdipAddPathBeziers (GpPath *path, const GpPointF *points, int count);
GpStatus GdipAddPathCurve (GpPath *path, const GpPointF *points, int count);
GpStatus GdipAddPathCurve2 (GpPath *path, const GpPointF *points, int count, float tension);
GpStatus GdipAddPathCurve3 (GpPath *path, const GpPointF *points, int count, int offset, int numberOfSegments, float tension);
GpStatus GdipAddPathClosedCurve (GpPath *path, const GpPointF *points, int count);
GpStatus GdipAddPathClosedCurve2 (GpPath *path, const GpPointF *points, int count, float tension);
GpStatus GdipAddPathRectangle (GpPath *path, float x, float y, float width, float height);
GpStatus GdipAddPathRectangles (GpPath *path, const GpRectF *rects, int count);
GpStatus GdipAddPathEllipse (GpPath *path, float x, float y, float width, float height);
GpStatus GdipAddPathPie (GpPath *path, float x, float y, float width, float height, float startAngle, float sweepAngle);
GpStatus GdipAddPathPolygon (GpPath *path, const GpPointF *points, int count);
GpStatus GdipAddPathPath (GpPath *path, GpPath *addingPath, bool connect);

GpStatus GdipAddPathLineI (GpPath *path, int x1, int y1, int x2, int y2);
GpStatus GdipAddPathLine2I (GpPath *path, const GpPoint *points, int count);
GpStatus GdipAddPathArcI (GpPath *path, int x, int y, int width, int height, float startAngle, float sweepAngle);
GpStatus GdipAddPathBezierI (GpPath *path, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);
GpStatus GdipAddPathBeziersI (GpPath *path, const GpPoint *points, int count);
GpStatus GdipAddPathCurveI (GpPath *path, const GpPoint *points, int count);
GpStatus GdipAddPathCurve2I (GpPath *path, const GpPoint *points, int count, float tension);
GpStatus GdipAddPathCurve3I (GpPath *path, const GpPoint *points, int count, int offset, int numberOfSegments, float tension);
GpStatus GdipAddPathClosedCurveI (GpPath *path, const GpPoint *points, int count);
GpStatus GdipAddPathClosedCurve2I (GpPath *path, const GpPoint *points, int count, float tension);
GpStatus GdipAddPathRectangleI (GpPath *path, int x, int y, int width, int height);
GpStatus GdipAddPathRectanglesI (GpPath *path, const GpRect *rects, int count);
GpStatus GdipAddPathEllipseI (GpPath *path, int x, int y, int width, int height);
GpStatus GdipAddPathPieI (GpPath *path, int x, int y, int width, int height, float startAngle, float sweepAngle);
GpStatus GdipAddPathPolygonI (GpPath *path, const GpPoint *points, int count);
GpStatus GdipFlattenPath (GpPath *path, GpMatrix *matrix, float flatness);
GpStatus GdipWindingModeOutline (GpPath *path, GpMatrix *matrix, float flatness);
GpStatus GdipWidenPath (GpPath *nativePath, GpPen *pen, GpMatrix *matrix, float flatness);
GpStatus GdipWarpPath (GpPath *nativePath, GpMatrix *matrix, const GpPointF *points, int count, 
                float src, float srcy, float srcwidth, float srcheight, WarpMode warpMode, float flatness);
GpStatus GdipTransformPath (GpPath* path, GpMatrix *matrix);
GpStatus GdipGetPathWorldBounds (GpPath *path, GpRectF *bounds, const GpMatrix *matrix, const GpPen *pen);
GpStatus GdipGetPathWorldBoundsI (GpPath *path, GpRect *bounds, const GpMatrix *matrix, const GpPen *pen);
GpStatus GdipIsVisiblePathPoint (GpPath *path, float x, float y, GpGraphics *graphics, bool *result);
GpStatus GdipIsVisiblePathPointI (GpPath *path, int x, int y, GpGraphics *graphics, bool *result);
GpStatus GdipIsOutlineVisiblePathPoint (GpPath *path, float x, float y, GpGraphics *graphics, bool *result);
GpStatus GdipIsOutlineVisiblePathPointI (GpPath *path, int x, int y, GpGraphics *graphics, bool *result);



#endif /* _GRAPHICS_PATH_H_ */
