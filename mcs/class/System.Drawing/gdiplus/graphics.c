/*
 * graphics.c
 *
 * Copyright (c) 2003 Alexandre Pigolkine, Novell Inc.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
 * NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Authors:
 *   Alexandre Pigolkine(pigolkine@gmx.de)
 *   Duncan Mak (duncan@ximian.com)
 *
 */

#include "gdip.h"
#include "gdip_win32.h"
#include <math.h>

void
gdip_graphics_init (GpGraphics *graphics)
{
	graphics->ct = cairo_create ();
	graphics->copy_of_ctm = cairo_matrix_create ();
	cairo_matrix_set_identity (graphics->copy_of_ctm);
	graphics->hdc = 0;
	graphics->hdc_busy_count = 0;
	graphics->image = 0;
	graphics->type = gtUndefined;
        /* cairo_select_font (graphics->ct, "serif:12"); */
	cairo_select_font (graphics->ct, "serif:12", CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_NORMAL);
}

GpGraphics *
gdip_graphics_new ()
{
	GpGraphics *result = (GpGraphics *) GdipAlloc (sizeof (GpGraphics));
	gdip_graphics_init (result);
	return result;
}

void
gdip_graphics_attach_bitmap (GpGraphics *graphics, GpBitmap *image)
{
	cairo_set_target_image (graphics->ct, image->data.Scan0, image->cairo_format,
				image->data.Width, image->data.Height, image->data.Stride);
	graphics->image = image;
	graphics->type = gtMemoryBitmap;
}

void 
gdip_graphics_detach_bitmap (GpGraphics *graphics, GpBitmap *image)
{
	printf ("Implement graphics_detach_bitmap");
	/* FIXME: implement me */
}

#define C1 0.552285
static void 
make_ellipse (GpGraphics *graphics, float x, float y, float width, float height)
{
        double rx = width / 2;
        double ry = height / 2;
        double cx = x + rx;
        double cy = y + ry;

        cairo_move_to (graphics->ct, cx + rx, cy);

        /* an approximate of the ellipse by drawing a curve in each
         * quartrant */
        cairo_curve_to (graphics->ct,
                        cx + rx, cy - C1 * ry,
                        cx + C1 * rx, cy - ry,
                        cx, cy - ry);
        
        cairo_curve_to (graphics->ct,
                        cx - C1 * rx, cy - ry,
                        cx - rx, cy - C1 * ry,
                        cx - rx, cy);

        cairo_curve_to (graphics->ct,
                        cx - rx, cy + C1 * ry,
                        cx - C1 * rx, cy + ry,
                        cx, cy + ry);
                
        cairo_curve_to (graphics->ct,
                        cx + C1 * rx, cy + ry,
                        cx + rx, cy + C1 * ry,
                        cx + rx, cy);

        cairo_close_path (graphics->ct);
}

static void
make_polygon (GpGraphics *graphics, GpPointF *points, int count)
{
        int i;
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i++)
                cairo_line_to (graphics->ct, points [i].X, points [i].Y);

        /*
         * Draw a line from the last point back to the first point if
         * they're not the same
         */
        if (points [0].X != points [count].X && points [0].Y != points [count].Y)
                cairo_line_to (graphics->ct, points [0].X, points [0].Y);

        cairo_close_path (graphics->ct);
}

static void
make_polygon_from_integers (GpGraphics *graphics, GpPoint *points, int count)
{
        int i;
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i++)
                cairo_line_to (graphics->ct, points [i].X, points [i].Y);

        /*
         * Draw a line from the last point back to the first point if
         * they're not the same
         */
        if (points [0].X != points [count].X && points [0].Y != points [count].Y)
                cairo_line_to (graphics->ct, points [0].X, points [0].Y);

        cairo_close_path (graphics->ct);
}

/*
 * Based on the algorithm described in
 *      http://www.stillhq.com/ctpfaq/2002/03/c1088.html#AEN1212
 */
static void
make_pie (GpGraphics *graphics, float x, float y, 
          float width, float height, float startAngle, float sweepAngle)
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
        cairo_move_to (graphics->ct, cx, cy);

        /* draw pie edge */
        cairo_line_to (graphics->ct,
                       cx + rx * cos_alpha, 
                       cy + ry * sin_alpha);
        
        cairo_curve_to (graphics->ct,
                        cx + rx * (cos_alpha - bcp * sin_alpha),
                        cy + ry * (sin_alpha + bcp * cos_alpha),
                        cx + rx * (cos_beta  + bcp * sin_beta),
                        cy + ry * (sin_beta  - bcp * cos_beta),
                        cx + rx *  cos_beta,
                        cy + ry *  sin_beta);

        /* draws line back to center */
        cairo_close_path (graphics->ct);
}

static void
make_arc (GpGraphics *graphics, float x, float y, float width,
                float height, float startAngle, float sweepAngle)
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

        /* move to pie edge */
        cairo_move_to (graphics->ct,
                       cx + rx * cos_alpha, 
                       cy + ry * sin_alpha);

        cairo_curve_to (graphics->ct,
                        cx + rx * (cos_alpha - bcp * sin_alpha),
                        cy + ry * (sin_alpha + bcp * cos_alpha),
                        cx + rx * (cos_beta  + bcp * sin_beta),
                        cy + ry * (sin_beta  - bcp * cos_beta),
                        cx + rx *  cos_beta,
                        cy + ry *  sin_beta);
}

static cairo_fill_rule_t
convert_fill_mode (GpFillMode fill_mode)
{
        if (fill_mode == FillModeAlternate) 
                return CAIRO_FILL_RULE_EVEN_ODD;
        else
                return CAIRO_FILL_RULE_WINDING;
}


GpStatus 
GdipCreateFromHDC (int hDC, GpGraphics **graphics)
{
	DC* dc = _get_DC_by_HDC (hDC);
	
	/* printf ("GdipCreateFromHDC. in %d, DC %p\n", hDC, dc); */
	if (dc == 0) return NotImplemented;
	
	*graphics = gdip_graphics_new ();
	cairo_set_target_drawable ((*graphics)->ct, GDIP_display, dc->physDev->drawable);
	_release_hdc (hDC);
	(*graphics)->hdc = (void*)hDC;
	(*graphics)->type = gtX11Drawable;
	/* printf ("GdipCreateFromHDC. graphics %p, ct %p\n", (*graphics), (*graphics)->ct); */
	return Ok;
}

GpStatus 
GdipDeleteGraphics (GpGraphics *graphics)
{
	/* FIXME: attention to surface (image, etc.) */
	/* printf ("GdipDeleteGraphics. graphics %p\n", graphics); */
	cairo_matrix_destroy (graphics->copy_of_ctm);
	cairo_destroy (graphics->ct);
	GdipFree (graphics);
	return Ok;
}

GpStatus 
GdipGetDC (GpGraphics *graphics, int *hDC)
{
	if (graphics->hdc == 0) {
		if (graphics->image != 0) {
			/* Create DC */
			graphics->hdc = gdip_image_create_Win32_HDC (graphics->image);
			if (graphics->hdc != 0) {
				++graphics->hdc_busy_count;
			}
		}
	}
	*hDC = (int)graphics->hdc;
	return Ok;
}

GpStatus 
GdipReleaseDC (GpGraphics *graphics, int hDC)
{
	if (graphics->hdc != (void *)hDC) return InvalidParameter;
	if (graphics->hdc_busy_count > 0) {
		--graphics->hdc_busy_count;
		if (graphics->hdc_busy_count == 0) {
			/* Destroy DC */
			gdip_image_destroy_Win32_HDC (graphics->image, (void*)hDC);
			graphics->hdc = 0;
		}
	}
	return Ok;
}

/* FIXME: the stack implementation is probably not suitable */
#define MAX_GRAPHICS_STATE_STACK 100

GpState saved_stack [MAX_GRAPHICS_STATE_STACK];
int current_stack_pos = 0;

GpStatus 
GdipRestoreGraphics (GpGraphics *graphics, unsigned int graphicsState)
{
	if (graphicsState < MAX_GRAPHICS_STATE_STACK) {
		cairo_matrix_copy (graphics->copy_of_ctm, saved_stack[graphicsState].matrix);
		cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
		current_stack_pos = graphicsState;
	}
	else {
		return InvalidParameter;
	}
	return Ok;
}

GpStatus 
GdipSaveGraphics(GpGraphics *graphics, unsigned int *state)
{
	if (current_stack_pos < MAX_GRAPHICS_STATE_STACK) {
		saved_stack[current_stack_pos].matrix = cairo_matrix_create ();
		cairo_matrix_copy (saved_stack[current_stack_pos].matrix, graphics->copy_of_ctm);
		*state = current_stack_pos;
		++current_stack_pos;
	}
	else {
		return OutOfMemory;
	}
	return Ok;
}
GpStatus
GdipResetWorldTransform (GpGraphics *graphics)
{
        GpStatus s = cairo_matrix_set_identity (graphics->copy_of_ctm);

        if (s != Ok)
                return s;
        else {
                cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
                return Ok;
        }
}

GpStatus
GdipSetWorldTransform (GpGraphics *graphics, GpMatrix *matrix)
{
        graphics->copy_of_ctm = matrix;
        cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
        return Ok;
}

GpStatus 
GdipGetWorldTransform (GpGraphics *graphics, GpMatrix *matrix)
{
        cairo_current_matrix (graphics->ct, matrix);
        return Ok;
}

GpStatus
GdipMultiplyWorldTransform (GpGraphics *graphics, GpMatrix *matrix, GpMatrixOrder order)
{
        Status s = GdipMultiplyMatrix (graphics->copy_of_ctm, matrix, order);
        
        if (s != Ok)
                return s;

        else {
                cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
                return Ok;
        }
}

GpStatus 
GdipRotateWorldTransform (GpGraphics *graphics, float angle, GpMatrixOrder order)
{
	GpStatus s = GdipRotateMatrix (graphics->copy_of_ctm, angle, order);

        if (s != Ok)
                return s;
        else {
                cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
                return Ok;
        }
}

GpStatus 
GdipTranslateWorldTransform (GpGraphics *graphics, float dx, float dy, GpMatrixOrder order)
{
        GpStatus s = GdipTranslateMatrix (graphics->copy_of_ctm, dx, dy, order);

        if (s != Ok) 
                return s;
        else {
                cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
                return Ok;
        }
}

GpStatus
GdipDrawArc (GpGraphics *graphics, GpPen *pen, 
                float x, float y, float width, float height, 
                float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);

        float delta = sweepAngle - startAngle;

        if (delta < 180)
                make_arc (graphics, x, y, width, height, startAngle, sweepAngle);

        else {
                make_arc (graphics, x, y, width, height, startAngle, startAngle + 180);
                make_arc (graphics, x, y, width, height, startAngle + 180, sweepAngle);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawArcI (GpGraphics *graphics, GpPen *pen, 
                int x, int y, int width, int height, 
                float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);

        float delta = sweepAngle - startAngle;

        if (delta < 180)
                make_arc (graphics, x, y, width, height, startAngle, sweepAngle);

        else {
                make_arc (graphics, x, y, width, height, startAngle, startAngle + 180);
                make_arc (graphics, x, y, width, height, startAngle + 180, sweepAngle);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipDrawBezier (GpGraphics *graphics, GpPen *pen, 
                float x1, float y1, float x2, float y2,
                float x3, float y3, float x4, float y4)
{
        gdip_pen_setup (graphics, pen);        
        
        cairo_move_to (graphics->ct, x1, y1);
        cairo_curve_to (graphics->ct, x2, y2, x3, y3, x4, y4);
        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus GdipDrawBezierI (GpGraphics *graphics, GpPen *pen, 
                int x1, int y1, int x2, int y2,
                int x3, int y3, int x4, int y4)
{
        return GdipDrawBezier (graphics, pen,
                        x1, y1, x2, y2, x3, y3, x4, y4);
}

GpStatus 
GdipDrawBeziers (GpGraphics *graphics, GpPen *pen,
                GpPointF *points, int count)
{
        int i, j, k;
        
        if (count == 0)
                return Ok;

        gdip_pen_setup (graphics, pen);
        
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count - 3; i += 3) {
                j = i + 1;
                k = i + 2;
                cairo_curve_to (graphics->ct,
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y,
                                points [k].X, points [k].Y);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawBeziersI (GpGraphics *graphics, GpPen *pen,
                GpPoint *points, int count)
{
        int i, j, k;
        
        if (count == 0)
                return Ok;

        gdip_pen_setup (graphics, pen);

        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count - 3; i += 3) {
                j = i + 1;
                k = i + 2;
                cairo_curve_to (graphics->ct,
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y,
                                points [k].X, points [k].Y);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipDrawEllipse (GpGraphics *graphics, GpPen *pen, 
                float x, float y, float width, float height)
{
        gdip_pen_setup (graphics, pen);
        
        make_ellipse (graphics, x, y, width, height);
        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawEllipseI (GpGraphics *graphics, GpPen *pen,
                int x, int y, int width, int height)
{
        return GdipDrawEllipse (graphics, pen, x, y, width, height);
}

GpStatus
GdipDrawLine (GpGraphics *graphics, GpPen *pen,
                float x1, float y1, float x2, float y2)
{
	gdip_pen_setup (graphics, pen);

	cairo_move_to (graphics->ct, x1, y1);
	cairo_line_to (graphics->ct, x2, y2);
	cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipDrawLineI (GpGraphics *graphics, GpPen *pen, 
                int x1, int y1, int x2, int y2)
{
        return GdipDrawLine (graphics, pen, x1, y1, x2, y2);
}

GpStatus 
GdipDrawLines (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count)
{
        GpStatus s;
        int i, j;

        for (i = 0; i < count - 1; i++) {
                j = i + 1;
                s = GdipDrawLine (graphics, pen, 
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y);

                if (s != Ok) return s;
        }

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipDrawLinesI (GpGraphics *graphics, GpPen *pen,
                GpPoint *points, int count)
{
        GpStatus s;
        int i, j;

        for (i = 0; i < count - 1; i++) {
                j = i + 1;
                s = GdipDrawLineI (graphics, pen, 
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y);

                if (s != Ok) return s;
        }

        return Ok;
}

GpStatus
GdipDrawPath (GpGraphics *graphics, GpPen *pen, GpPath *path)
{
        int length = path->count;
        int i;

        gdip_pen_setup (graphics, pen);

        for (i = 0; i < length; ++i) {
                GpPointF pt = g_array_index (path->points, GpPointF, i);
                byte type = g_array_index (path->types, byte, i);
                GpPointF pts [3];
                int idx = 0;
                
                switch (type) {
                case PathPointTypeStart:
                        cairo_move_to (graphics->ct, pt.X, pt.Y);
                        break;

                case PathPointTypeLine:
                        cairo_line_to (graphics->ct, pt.X, pt.Y);
                        break;

                case PathPointTypeBezier:
                        if (idx < 3) {
                                pts [idx] = pt;
                                idx ++;

                        } else {
                                cairo_curve_to (graphics->ct,
                                                pts [0].X, pts [0].Y,
                                                pts [1].X, pts [1].Y,
                                                pts [2].X, pts [2].Y);
                                idx = 0;
                        }
                        break;
                        
                default:
                        return NotImplemented;
                }
        }

        cairo_stroke (graphics->ct);
        
        return Ok;
}

GpStatus
GdipDrawPie (GpGraphics *graphics, GpPen *pen, float x, float y, 
                float width, float height, float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);

        float delta = sweepAngle - startAngle;

        if (delta < 180)
                make_pie (graphics, x, y, width, height, startAngle, sweepAngle);
        else {
                make_pie (graphics, x, y, width, height, startAngle, startAngle + 180);
                make_pie (graphics, x, y, width, height, startAngle + 180, sweepAngle);
        }

        cairo_stroke (graphics->ct);

        cairo_close_path (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawPieI (GpGraphics *graphics, GpPen *pen, int x, int y, 
                int width, int height, float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);
        
        float delta = sweepAngle - startAngle;
        
        if (delta < 180)
                make_pie (graphics, x, y, width, height, startAngle, sweepAngle);
        else {
                make_pie (graphics, x, y, width, height, startAngle, startAngle + 180);
                make_pie (graphics, x, y, width, height, startAngle + 180, sweepAngle);
        }

        cairo_stroke (graphics->ct);

        cairo_close_path (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawPolygon (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count)
{
        gdip_pen_setup (graphics, pen);
        
        make_polygon (graphics, points, count);
        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawPolygonI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count)
{
        gdip_pen_setup (graphics, pen);
        
        make_polygon_from_integers (graphics, points, count);
        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawRectangle (GpGraphics *graphics, GpPen *pen,
                float x, float y, float width, float height)
{
        gdip_pen_setup (graphics, pen);

        cairo_rectangle (graphics->ct, x, y, width, height);
        cairo_stroke (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipDrawRectangleI (GpGraphics *graphics, GpPen *pen,
                int x, int y, int width, int height)
{
        return GdipDrawRectangle (graphics, pen, x, y, width, height);
}

GpStatus
GdipFillEllipse (GpGraphics *graphics, GpBrush *brush,
                float x, float y, float width, float height)
{
        gdip_brush_setup (graphics, brush);
        make_ellipse (graphics, x, y, width, height);
        cairo_fill (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipFillEllipseI (GpGraphics *graphics, GpBrush *brush,
                int x, int y, int width, int height)
{
        return GdipFillEllipse (graphics, brush, x, y, width, height);
}

GpStatus 
GdipFillRectangle (GpGraphics *graphics, GpBrush *brush, 
                float x, float y, float width, float height)
{
	gdip_brush_setup (graphics, brush);
	cairo_rectangle (graphics->ct, x, y, width, height);
	cairo_fill (graphics->ct);
        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipFillPolygon (GpGraphics *graphics, GpBrush *brush, 
                GpPointF *points, int count, GpFillMode fillMode)
{
        gdip_brush_setup (graphics, brush);
        make_polygon (graphics, points, count);

        cairo_set_fill_rule (
                graphics->ct,
                convert_fill_mode (fillMode));

        cairo_fill (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipFillPolygonI (GpGraphics *graphics, GpBrush *brush, 
                GpPoint *points, int count, GpFillMode fillMode)
{
        gdip_brush_setup (graphics, brush);
        make_polygon_from_integers (graphics, points, count);

        cairo_set_fill_rule (
                graphics->ct,
                convert_fill_mode (fillMode));
        
        cairo_fill (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus
GdipFillPolygon2 (GpGraphics *graphics, GpBrush *brush, GpPointF *points, int count)
{
        return GdipFillPolygon (graphics, brush, points, count, FillModeAlternate);
}

GpStatus
GdipFillPolygon2I (GpGraphics *graphics, GpBrush *brush, GpPoint *points, int count)
{
        return GdipFillPolygonI (graphics, brush, points, count, FillModeAlternate);
}

GpStatus 
GdipDrawString (GpGraphics *graphics, const char *string,// FIXME: The string is encode using Unicode, Jordi
                int len, void *font, RectF *rc, void *format, GpBrush *brush)
{
        cairo_save (graphics->ct);
	if (brush)
		gdip_brush_setup (graphics, brush);

        else
		cairo_set_rgb_color (graphics->ct, 0., 0., 0.);

	cairo_move_to (graphics->ct, rc->left, rc->top + 12);
	cairo_scale_font (graphics->ct, 12);
	cairo_show_text (graphics->ct, string);

        cairo_restore (graphics->ct);        
	return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipSetRenderingOrigin (GpGraphics *graphics, int x, int y)
{
        cairo_move_to (graphics->ct, x, y);
        cairo_close_path (graphics->ct);

        return gdip_get_status (cairo_status (graphics->ct));
}

GpStatus 
GdipGetRenderingOrigin (GpGraphics *graphics, int *x, int *y)
{
        double cx, cy;
        cairo_current_point (graphics->ct, &cx, &cy);

        *x = (int) cx;
        *y = (int) cy;

        return gdip_get_status (cairo_status (graphics->ct));
}

