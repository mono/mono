/*
 * graphics-path.c
 * 
 * Author: Duncan Mak (duncan@ximian.com)
 *
 * Copyright (C) 2003, Novell Inc.
 *
 */
 
#include <math.h>
#include "gdip.h"
#include "graphics-path.h"

static GArray *
array_to_g_array (const GpPointF *pt, int length)
{
        GArray *p = g_array_sized_new (FALSE, TRUE, sizeof (GpPointF), length);
        g_array_append_vals (p, pt, length);
        return p;
}

static GpPointF *
g_array_to_array (GArray *p)
{
        int length = p->len;
        GpPointF *pts = (GpPointF *) GdipAlloc (sizeof (GpPointF) * length);

        memcpy (pts, p->data, p->len * sizeof (GpPointF));        
        
        return pts;
}

static byte *
g_byte_array_to_array (GByteArray *p)
{
        int length = p->len;
        byte *types = (byte *) GdipAlloc (sizeof (byte) * length);

        memcpy (types, p->data, p->len * sizeof (byte));
        
        return types;
}

static GByteArray *
array_to_g_byte_array (const byte *types, int count)
{
        GByteArray *p = g_byte_array_sized_new (count);
        g_byte_array_append (p, types, count);
        return p;
}

static GpPoint *
float_to_int (const GpPointF *pts, int count)
{
        GpPoint *p = (GpPoint *) GdipAlloc (sizeof (GpPoint) * count);
        GpPointF *tmp = (GpPointF *) pts;
        int i;
        
        for (i = 0; i < count; i++, p++, tmp++) {
                p->X = (int) tmp->X;
                p->Y = (int) tmp->Y;
        }
        
        return p;
}

static GpPointF *
int_to_float (const GpPoint *pts, int count)
{
        GpPointF *p = (GpPointF *) GdipAlloc (sizeof (GpPointF) * count);
        GpPoint *tmp = (GpPoint *) pts;
        int i;

        for (i = 0; i < count; i++, p++, tmp++) {
                p->X = (float) tmp->X;
                p->Y = (float) tmp->Y;
        }

        return p;
}

static void
append (GpPath *path, float x, float y, GpPathPointType type)
{
        byte t = (byte) type;
        GpPointF pt;
        pt.X = x;
        pt.Y = y;
        g_array_append_val (path->points, pt);
        g_byte_array_append (path->types, &t, 1);
}

static void
append_point (GpPath *path, GpPointF pt, GpPathPointType type)
{
        byte t = (byte) type;
        g_array_append_val (path->points, pt);
        g_byte_array_append (path->types, &t, 1);
}

static void
append_bezier (GpPath *path, float x1, float y1, float x2, float y2, float x3, float y3)
{
        append (path, x1, y1, PathPointTypeBezier3);
        append (path, x2, y2, PathPointTypeBezier3);
        append (path, x3, y3, PathPointTypeBezier3);
}

GpStatus
GdipCreatePath (GpFillMode brushMode, GpPath **path)
{
        *path = (GpPath *) GdipAlloc (sizeof (GpPath));

        (*path)->fill_mode = brushMode;
        (*path)->points = NULL;
        (*path)->types = NULL;
        (*path)->count = 0;

        return Ok;
}

GpStatus
GdipCreatePath2 (const GpPointF *points, const byte *types,
                int count, GpFillMode fillMode, GpPath **path)
{
        GArray *pts = array_to_g_array (points, count);
        GByteArray *t = array_to_g_byte_array (types, count);
        
        *path = (GpPath *) GdipAlloc (sizeof (GpPath));
        (*path)->fill_mode = fillMode;
        (*path)->count = count;
        (*path)->points = pts;
        (*path)->types = t;
        
        return Ok;
}

GpStatus
GdipClonePath (GpPath *path, GpPath **clonePath)
{
        *clonePath = (GpPath *) GdipAlloc (sizeof (GpPath));
        (*clonePath)->fill_mode = path->fill_mode;
        (*clonePath)->count = path->count;
        (*clonePath)->points = path->points;
        (*clonePath)->types = path->types;
        
        return Ok;
}

GpStatus
GdipDeletePath (GpPath *path)
{
        if (path->count != 0) {
                if (path->points != NULL)
                        g_array_free (path->points, TRUE);
                
                if (path->types != NULL)
                        g_byte_array_free (path->types, TRUE);
        }
        
        GdipFree (path);
        return Ok;
}

GpStatus
GdipResetPath (GpPath *path)
{
        path->points = NULL;
        path->types = NULL;
        path->count = 0;
        
        return Ok;
}

GpStatus
GdipGetPointCount (GpPath *path, int *count)
{
        *count = path->count;
        return Ok;
}

GpStatus
GdipGetPathTypes (GpPath *path, byte *types, int *count)
{
        *count = path->count;
        types = g_byte_array_to_array (path->types);
        
        return Ok;
}

GpStatus
GdipGetPathPoints (GpPath *path, GpPointF *points, int *count)
{
        *count = path->count;
        points = g_array_to_array (path->points);
        
        return Ok;
}

GpStatus
GdipGetPathPointsI (GpPath *path, GpPoint *points, int *count)
{
        *count = path->count;
        PointF *tmp = g_array_to_array (path->points);

        points = float_to_int (tmp, path->count);

        GdipFree (tmp);
        
        return Ok;
}

GpStatus
GdipGetPathFillMode (GpPath *path, GpFillMode *fillmode)
{
        *fillmode = path->fill_mode;
        
        return Ok;
}

GpStatus
GdipSetPathFillMode (GpPath *path, GpFillMode fillmode)
{
        path->fill_mode = fillmode;
        
        return Ok;
}

GpStatus
GdipGetPathData (GpPath *path, GpPathData *pathData)
{
        pathData->Count = path->count;
        pathData->Points = g_array_to_array (path->points);
        pathData->Types = g_byte_array_to_array (path->types);
        
        return Ok;
}

GpStatus
GdipStartPathFigure (GpPath *path)
{
        return NotImplemented;
}

GpStatus
GdipClosePathFigure (GpPath *path)
{
        return NotImplemented;
}

GpStatus
GdipClosePathFigures (GpPath *path)
{
        return NotImplemented;
}

GpStatus
GdipSetPathMarker (GpPath *path)
{
        return NotImplemented;
}

GpStatus
GdipClearPathMarkers (GpPath *path)
{
        return NotImplemented;
}

GpStatus
GdipReversePath (GpPath *path)
{
        int length= path->count;
        GByteArray *types = g_byte_array_sized_new (length);
        GArray *points = g_array_sized_new (FALSE, TRUE, sizeof (GpPointF), length);
        int i;
        for (i = length; i > 0; i--) {
                byte t = g_array_index (path->types, byte, i);
                GpPointF pt = g_array_index (path->points, GpPointF, i);
                
                g_byte_array_append (types, &t, 1);
                g_array_append_val (points, pt);
        }
        path->points = points;
        path->types = types;
        
        return Ok;
}

GpStatus
GdipGetPathLastPoint (GpPath *path, GpPointF *lastPoint)
{
        *lastPoint = g_array_index (path->points, GpPointF, path->count);
        return Ok;
}

GpStatus
GdipAddPathLine (GpPath *path, float x1, float y1, float x2, float y2)
{
        append (path, x1, y1, PathPointTypeStart);
        append (path, x2, y2, PathPointTypeLine);

        return Ok;
}

GpStatus
GdipAddPathLine2 (GpPath *path, const GpPointF *points, int count)
{
        int i;
        GpPointF *tmp = (GpPointF *) points;

        for (i = 0; i < count; i++, tmp++)
                append_point (path, *tmp, PathPointTypeLine);
        
        return Ok;
}

GpStatus
GdipAddPathArc (GpPath *path, float x, float y, 
                float width, float height, float startAngle, float sweepAngle)
{
        float rx = width / 2;
        float ry = height / 2;
        
        /* center */
        int cx = x + rx;
        int cy = y + ry;

        /* angles in radians */        
        float alpha = startAngle * PI / 180;
        float beta = sweepAngle * PI / 180;

        float delta = beta - alpha;
        float bcp = 4.0 / 3 * (1 - cos (delta / 2)) / sin (delta /2);

        float sin_alpha = sin (alpha);
        float sin_beta = sin (beta);
        float cos_alpha = cos (alpha);
        float cos_beta = cos (beta);

        append (path,
                cx + rx * cos_alpha,
                cy + ry * sin_alpha,
                PathPointTypeStart);

        append_bezier (path, 
                       cx + rx * (cos_alpha - bcp * sin_alpha),
                       cy + ry * (sin_alpha + bcp * cos_alpha),
                       cx + rx * (cos_beta  + bcp * sin_beta),
                       cy + ry * (sin_beta  - bcp * cos_beta),
                       cx + rx *  cos_beta,
                       cy + ry *  sin_beta);

        return Ok;
}

GpStatus
GdipAddPathBezier (GpPath *path, 
        float x1, float y1, float x2, float y2, 
        float x3, float y3, float x4, float y4)
{
        append (path, x1, y1, PathPointTypeStart);
        append_bezier (path, x2, y2, x3, y3, x4, y4);
        
        return Ok;
}

GpStatus
GdipAddPathBeziers (GpPath *path, const GpPointF *points, int count)
{
        int i;
        GpPointF *tmp = (GpPointF *) points;
        
        append_point (path, *tmp, PathPointTypeStart);
        tmp++;

        for (i = 1; i < count; i++, tmp++)
                append_point (path, *tmp, PathPointTypeBezier3);

        return Ok;
}

GpStatus
GdipAddPathCurve (GpPath *path, const GpPointF *points, int count)
{
        return NotImplemented;
}

GpStatus
GdipAddPathCurve2 (GpPath *path, const GpPointF *points, int count, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathCurve3 (GpPath *path, const GpPointF *points, int count, 
        int offset, int numberOfSegments, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathClosedCurve (GpPath *path, const GpPointF *points, int count)
{
        return GdipAddPathClosedCurve2 (path, points, count, 0.5);
}

GpStatus
GdipAddPathClosedCurve2 (GpPath *path, const GpPointF *points, int count, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathRectangle (GpPath *path, float x, float y, float width, float height)
{
        append (path, x, y, PathPointTypeLine);
        append (path, x + width, y, PathPointTypeLine);
        append (path, x + width, y + height, PathPointTypeLine);
        append (path, x, y + height, PathPointTypeLine);
        
        return Ok;
}

GpStatus
GdipAddPathRectangles (GpPath *path, const GpRectF *rects, int count)
{
        int i;
        for (i = 0; i < count; i++, rects++) {
                float x = rects->left;
                float y = rects->top;
                float width = rects->right - rects->left;
                float height = rects->bottom - rects->top;
                GdipAddPathRectangle (path, x, y, width, height);
        }
        
        return Ok;
}

GpStatus
GdipAddPathEllipse (GpPath *path, float x, float y, float width, float height)
{
        float C1 = 0.552285;
        double rx = width / 2;
        double ry = height / 2;
        double cx = x + rx;
        double cy = y + ry;

        /* origin */
        append (path, cx + rx, cy, PathPointTypeStart);

        /* quadrant I */
        append_bezier (path, 
                       cx + rx, cy - C1 * ry, 
                       cx + C1 * rx, cy - ry, 
                       cx, cy - ry);

        /* quadrant II */
        append_bezier (path,
                       cx - C1 * rx, cy - ry, 
                       cx - rx, cy - C1 * ry, 
                       cx - rx, cy);

        /* quadrant III */
        append_bezier (path,
                       cx - rx, cy + C1 * ry, 
                       cx - C1 * rx, cy + ry, 
                       cx, cy + ry);

        /* quadrant IV */
        append_bezier (path,
                       cx + C1 * rx, cy + ry, 
                       cx + rx, cy + C1 * ry, 
                       cx + rx, cy);
        
        return Ok;
}

GpStatus
GdipAddPathPie (GpPath *path, float x, float y, float width, float height, float startAngle, float sweepAngle)
{
        float rx = width / 2;
        float ry = height / 2;
        int cx = x + rx;
        int cy = y + ry;

        /* angles in radians */        
        float alpha = startAngle * PI / 180;
        float beta = sweepAngle * PI / 180;

        float delta = beta - alpha;
        float bcp = 4.0 / 3 * (1 - cos (delta / 2)) / sin (delta /2);

        float sin_alpha = sin (alpha);
        float sin_beta = sin (beta);
        float cos_alpha = cos (alpha);
        float cos_beta = cos (beta);

        /* move to center */
        append (path, cx, cy, PathPointTypeStart);
        

        /* draw pie edge */
        append (path, cx + rx * cos_alpha, cy + ry * sin_alpha,
                PathPointTypeLine);

        /* draw arc */
        append_bezier (path,
                       cx + rx * (cos_alpha - bcp * sin_alpha),
                       cy + ry * (sin_alpha + bcp * cos_alpha),
                       cx + rx * (cos_beta  + bcp * sin_beta),
                       cy + ry * (sin_beta  - bcp * cos_beta),
                       cx + rx *  cos_beta,
                       cy + ry *  sin_beta);
        
        /* draw pie edge */
        append (path, cx, cy, PathPointTypeLine);

        return Ok;
}

GpStatus
GdipAddPathPolygon (GpPath *path, const GpPointF *points, int count)
{
        int i;
        GpPointF *tmp = (GpPointF *) points;
        
        append_point (path, *tmp, PathPointTypeStart);
        tmp ++;

        for (i = 1; i < count; i++, tmp++)
                append_point (path, *tmp, PathPointTypeLine);

        return Ok;
}

GpStatus
GdipAddPathPath (GpPath *path, GpPath *addingPath, bool connect)
{
        /* XXX:need to understand the connect argument */

        return NotImplemented;
}

/* XXX: This one is really hard. They really translate a string into bezier points and what not */
/*
 * GpStatus 
 * GdipAddString (GpPath *path, const char *string, int length, 
 *                const GpFontFamily *family, int style, float emSize, const GpRectF *layoutRect,
 *                const GpStringFormat *format)
 * { 
 *         return NotImplemented; 
 * }
 */

/*
 * GpStatus
 * GdipAddString (GpPath *path, const char *string, int length,
 *                const GpFontFamily *family, int style, float emSize, const GpRect *layoutRect,
 *                const GpStringFormat *format)
 * {
 *          return NotImplemented;
 * }
 */

GpStatus
GdipAddPathLineI (GpPath *path, int x1, int y1, int x2, int y2)
{
        append (path, x1, y1, PathPointTypeStart);
        append (path, x2, y2, PathPointTypeLine);

        return Ok;
}

GpStatus
GdipAddPathLine2I (GpPath* path, const GpPoint *points, int count)
{
        int i;
        GpPointF *tmp = int_to_float (points, count);
        
        append_point (path, *tmp, PathPointTypeStart);
        tmp++;

        for (i = 1; i < count; i++, tmp++)
                append_point (path, *tmp, PathPointTypeLine);

        GdipFree (tmp);

        return Ok;
}

GpStatus
GdipAddPathArcI (GpPath *path, int x, int y, int width, int height, float startAngle, float sweepAngle)
{
        return GdipAddPathArc (path, x, y, width, height, startAngle, sweepAngle);
}

GpStatus
GdipAddPathBezierI (GpPath *path, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
{
	return GdipAddPathBezier (path, x1, y1, x2, y2, x3, y3, x4, y4);
}

GpStatus
GdipAddPathBeziersI (GpPath *path, const GpPoint *points, int count)
{
        GpPointF *tmp = int_to_float (points, count);
        Status s = GdipAddPathBeziers (path, tmp, count);

        GdipFree (tmp);

        return s;
}

GpStatus
GdipAddPathCurveI (GpPath *path, const GpPoint *points, int count)
{
        return GdipAddPathCurve2I (path, points, count, 0.5);
}

GpStatus
GdipAddPathCurve2I (GpPath *path, const GpPoint *points, int count, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathCurve3I (GpPath *path, const GpPoint *points, 
                    int count, int offset, int numberOfSegments, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathClosedCurveI (GpPath *path, const GpPoint *points, int count)
{
        return GdipAddPathClosedCurve2I (path, points, count, 0.5);
}

GpStatus
GdipAddPathClosedCurve2I (GpPath *path, const GpPoint *points, int count, float tension)
{
        return NotImplemented;
}

GpStatus
GdipAddPathRectangleI (GpPath *path, int x, int y, int width, int height)
{
        return GdipAddPathRectangle (path, x, y, width, height);
}

GpStatus
GdipAddPathRectanglesI (GpPath *path, const GpRect *rects, int count)
{
        int i;
        for (i = 0; i < count; i++, rects++) {
                float x = (float) rects->left;
                float y = (float) rects->top;
                float width =  (float) (rects->right - rects->left);
                float height =  (float) (rects->bottom - rects->top);
                GdipAddPathRectangle (path, x, y, width, height);
        }

        return Ok;
}

GpStatus
GdipAddPathEllipseI (GpPath *path, int x, int y, int width, int height)
{
        return GdipAddPathEllipse (path, x, y, width, height);
}

GpStatus
GdipAddPathPieI (GpPath *path, int x, int y, int width, int height, float startAngle, float sweepAngle)
{
        return GdipAddPathPie (path, x, y, width, height, startAngle, sweepAngle);
}

GpStatus
GdipAddPathPolygonI (GpPath *path, const GpPoint *points, int count)
{
        GpPointF *tmp = int_to_float (points, count);

        Status s = GdipAddPathPolygon (path, tmp, count);

        GdipFree (tmp);

        return s;
}

GpStatus 
GdipFlattenPath (GpPath *path, GpMatrix *matrix, float flatness)
{
        return NotImplemented;
}

GpStatus 
GdipWindingModeOutline (GpPath *path, GpMatrix *matrix, float flatness)
{
        return NotImplemented;
}

GpStatus 
GdipWidenPath (GpPath *nativePath, GpPen *pen, GpMatrix *matrix, float flatness)
{
        return NotImplemented;
}

GpStatus 
GdipWarpPath (GpPath *nativePath, GpMatrix *matrix, const GpPointF *points, int count, 
                float src, float srcy, float srcwidth, float srcheight, WarpMode warpMode, float flatness)
{
        return NotImplemented;
}

GpStatus 
GdipTransformPath (GpPath* path, GpMatrix *matrix)
{
        PointF *points = g_array_to_array (path->points);
        int count = path->count;

        Status s = GdipTransformMatrixPoints (matrix, points, count);

        path->points = array_to_g_array (points, count);

        GdipFree (points);

        return s;
}

GpStatus 
GdipGetPathWorldBounds (GpPath *path, GpRectF *bounds, const GpMatrix *matrix, const GpPen *pen)
{
        return NotImplemented;
}

GpStatus 
GdipGetPathWorldBoundsI (GpPath *path, GpRect *bounds, const GpMatrix *matrix, const GpPen *pen)
{
        return NotImplemented;
}

GpStatus 
GdipIsVisiblePathPoint (GpPath *path, float x, float y, GpGraphics *graphics, bool *result)
{
        return NotImplemented;
}

GpStatus 
GdipIsVisiblePathPointI (GpPath *path, int x, int y, GpGraphics *graphics, bool *result)
{
        return NotImplemented;
}

GpStatus 
GdipIsOutlineVisiblePathPoint (GpPath *path, float x, float y, GpGraphics *graphics, bool *result)
{
        return NotImplemented;
}

GpStatus 
GdipIsOutlineVisiblePathPointI (GpPath *path, int x, int y, GpGraphics *graphics, bool *result)
{
        return NotImplemented;
}
