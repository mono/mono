/*
 * pen.c
 * 
 * Copyright (c) 2003 Alexandre Pigolkine
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

void 
gdip_pen_init (GpPen *pen)
{
        pen->color = 0;
		pen->brush = 0;
        pen->width = 1;
        pen->miter_limit = 10;
        pen->line_join = LineJoinMiter;
	pen->dash_style = DashStyleSolid;
	pen->line_cap = LineCapFlat;
	pen->mode = PenAlignmentCenter;
	pen->dash_offset = 0;
	pen->dash_count = 0;
	pen->own_dash_array = 0;
	pen->dash_array = 0;
	pen->unit = UnitWorld;
        pen->matrix = cairo_matrix_create ();
}

GpPen*
gdip_pen_new (void)
{
        GpPen *result = (GpPen *) GdipAlloc (sizeof (GpPen));
        gdip_pen_init (result);
        return result;
}

static cairo_line_join_t
convert_line_join (GpLineJoin join)
{
        switch (join) {

        case LineJoinMiter:
                return CAIRO_LINE_JOIN_MITER;

        case LineJoinBevel:
                return CAIRO_LINE_JOIN_BEVEL;

        case LineJoinRound:
                return CAIRO_LINE_JOIN_ROUND;
 
        case LineJoinMiterClipped:
        default:
                return CAIRO_LINE_JOIN_MITER;
        }
}

static cairo_line_cap_t
convert_line_cap (GpLineCap cap)
{
        switch (cap) {
        
        case LineCapSquare:
                return CAIRO_LINE_CAP_SQUARE;

        case LineCapRound:
                return CAIRO_LINE_CAP_ROUND;                

        case LineCapFlat:
        case LineCapTriangle:
        case LineCapNoAnchor:
        case LineCapSquareAnchor:
        case LineCapRoundAnchor:
        case LineCapDiamondAnchor:
        case LineCapArrowAnchor:
        case LineCapCustom:
        default:
                return CAIRO_LINE_CAP_BUTT;
        }
}

static double *
convert_dash_array (float *f, int count)
{
        double *retval = malloc (sizeof (double) * count);
        int i;
        for (i = 0; i < count; i++, f++, retval++)
                *retval = (double) *f;

        return retval;
}

void 
gdip_pen_setup (GpGraphics *graphics, GpPen *pen)
{
        int R = (pen->color & 0x00FF0000 ) >> 16;
        int G = (pen->color & 0x0000FF00 ) >> 8;
        int B = (pen->color & 0x000000FF );

        cairo_set_rgb_color (graphics->ct, (double) R, (double) G, (double) B);
        cairo_set_line_width (graphics->ct, (double) pen->width);
        cairo_set_miter_limit (graphics->ct, (double) pen->miter_limit);
        cairo_set_line_join (graphics->ct, convert_line_join (pen->line_join));
        cairo_set_line_cap (graphics->ct, convert_line_cap (pen->line_cap));

        if (pen->matrix != NULL)
                cairo_set_matrix (graphics->ct, pen->matrix);

        if (pen->dash_array != NULL && pen->dash_count != 0)
                cairo_set_dash (graphics->ct,
                                convert_dash_array (pen->dash_array, pen->dash_count),
                                pen->dash_count, pen->dash_offset);
}

GpStatus 
GdipCreatePen1(int argb, float width, GpUnit unit, GpPen **pen)
{
        *pen = gdip_pen_new ();
        (*pen)->color = argb;
        (*pen)->width = width;
        return Ok; 
}

GpStatus
GdipCreatePen2 (GpBrush *brush, float width, GpUnit unit, GpPen **pen)
{
        int color;
        GpBrushType type;        
        *pen = gdip_pen_new ();
        (*pen)->width = width;

        GdipGetBrushType (brush, &type);

        switch (type) {

        case BrushTypeSolidColor:

                GdipGetSolidFillColor (brush, &color);                
                (*pen)->color = color;
                return Ok;

        case BrushTypeHatchFill:
        case BrushTypeTextureFill:
        case BrushTypePathGradient:
        case BrushTypeLinearGradient:
        default:
                return GenericError;
        }
}

static float *
clone_dash_array (float *clone, float *array, int size)
{
        int i;

        for (i = 0; i < size; i++)
                clone [i] = array [i];

        return clone;
}

GpStatus 
GdipClonePen (GpPen *pen, GpPen **clonepen)
{
        GpPen *result = gdip_pen_new ();
        int count = pen->dash_count;
        GpMatrix *matrix;       /* copy of pen->matrix */
        float dashes [count];   /* copy off pen->dash_array */

        GdipCloneMatrix (pen->matrix, &matrix);
        clone_dash_array (dashes, pen->dash_array, count);

        result->color = pen->color;
	result->brush = pen->brush;
        result->width = pen->width;
        result->miter_limit = pen->miter_limit;
        result->line_join = pen->line_join;
	result->dash_style = pen->dash_style;
        result->line_cap = pen->line_cap;
	result->mode = pen->mode;
        result->dash_offset = pen->dash_offset;
	result->dash_count = pen->dash_count;
	result->own_dash_array = 0;
	result->dash_array = dashes;
	result->unit = pen->unit;
        result->matrix = matrix;

        *clonepen = result;

        return Ok;
}       


GpStatus 
GdipDeletePen (GpPen *pen)
{
        if (pen->matrix != NULL)
                cairo_matrix_destroy (pen->matrix);

        if (pen->dash_array != NULL && pen->own_dash_array)
                free (pen->dash_array);
        
        GdipFree (pen);
	return Ok;
}

GpStatus
GdipSetPenWidth (GpPen *pen, float width)
{
        pen->width = width;
        return Ok;
}

GpStatus
GdipGetPenWidth (GpPen *pen, float *width)
{
        *width = pen->width;
        return Ok;
}

GpStatus
GdipSetPenBrushFill (GpPen *pen, GpBrush *brush)
{
        GpStatus s;
        pen->brush = brush;
        int color;
        s = GdipGetSolidFillColor (brush, &color);

        if (s != Ok)
                return s;
        
        pen->color = color;
        return Ok;
}

GpStatus
GdipGetPenBrushFill (GpPen *pen, GpBrush **brush)
{
        *brush = pen->brush;
        return Ok;
}

GpStatus
GdipSetPenColor (GpPen *pen, int argb)
{
        pen->color = argb;

        return Ok;
}

GpStatus
GdipGetPenColor (GpPen *pen, int *argb)
{
        *argb = pen->color;
        return Ok;
}

GpStatus
GdipSetPenMiterLimit (GpPen *pen, float miterLimit)
{
        pen->miter_limit = miterLimit;
        return Ok;
}

GpStatus
GdipGetPenMiterLimit (GpPen *pen, float *miterLimit)
{
        *miterLimit = pen->miter_limit;

        return Ok;
}

GpStatus
GdipSetPenLineJoin (GpPen *pen, GpLineJoin lineJoin)
{
        pen->line_join = lineJoin;
        return Ok;
}

GpStatus
GdipGetPenLineJoin (GpPen *pen, GpLineJoin *lineJoin)
{
        *lineJoin = pen->line_join;
        return Ok;
}

GpStatus
GdipSetPenLineCap (GpPen *pen, GpLineCap lineCap)
{
        pen->line_cap = lineCap;
        return Ok;
}

GpStatus
GdipGetPenLineCap (GpPen *pen, GpLineCap *lineCap)
{
        *lineCap = pen->line_cap;
        return Ok;
}

GpStatus
GdipSetPenMode (GpPen *pen, GpPenAlignment penMode)
{
        pen->mode = penMode;
        return Ok;
}

GpStatus
GdipGetPenMode (GpPen *pen, GpPenAlignment *penMode)
{
        *penMode = pen->mode;
        return Ok;
}

GpStatus
GdipGetPenUnit (GpPen *pen, GpUnit *unit)
{
        *unit = pen->unit;
        return Ok;
}

GpStatus
GdipSetPenUnit (GpPen *pen, GpUnit unit)
{
        pen->unit = unit;
        return Ok;
}

GpStatus
GdipSetPenTransform (GpPen *pen, GpMatrix *matrix)
{
        pen->matrix = matrix;
        return Ok;
}

GpStatus
GdipGetPenTransform (GpPen *pen, GpMatrix **matrix)
{
        *matrix = pen->matrix;
        return Ok;
}

GpStatus
GdipResetPenTransform (GpPen *pen)
{
        pen->matrix = cairo_matrix_create ();
        return Ok;
}

GpStatus
GdipMultiplyPenTransform (GpPen *pen, GpMatrix *matrix, GpMatrixOrder order)
{
        return GdipMultiplyMatrix (pen->matrix, matrix, order);
}

GpStatus
GdipTranslatePenTransform (GpPen *pen, float dx, float dy, GpMatrixOrder order)
{
        return GdipTranslateMatrix (pen->matrix, dx, dy, order);
}

GpStatus
GdipScalePenTransform (GpPen *pen, float sx, float sy, GpMatrixOrder order)
{
        return GdipScaleMatrix (pen->matrix, sx, sy, order);
}

GpStatus
GdipRotatePenTransform (GpPen *pen, float angle, GpMatrixOrder order)
{
        return GdipRotateMatrix (pen->matrix, angle, order);
}

GpStatus
GdipGetPenDashStyle (GpPen *pen, GpDashStyle *dashStyle)
{
        *dashStyle = pen->dash_style;
        return Ok;
}

static float Custom [] = { 1.0 };
static float Dot []  = { 1.0, 1.0 };
static float Dash []  = { 3.0, 1.0 };
static float DashDot [] = { 3.0, 1.0, 1.0, 1.0 };
static float DashDotDot [] = { 3.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

GpStatus
GdipSetPenDashStyle (GpPen *pen, GpDashStyle dashStyle)
{
        pen->dash_style = dashStyle;

        switch (dashStyle) {
        case DashStyleSolid:
                pen->dash_array = NULL;
                return Ok;

        case DashStyleDashDot:
                pen->dash_array = DashDot;
                pen->dash_count = 4;
                return Ok;
                
        case DashStyleDashDotDot:
                pen->dash_array = DashDotDot;
                pen->dash_count = 6;
                return Ok;

        case DashStyleDot:
                pen->dash_array = Dot;
                pen->dash_count = 2;
                return Ok;

        case DashStyleDash:
                pen->dash_array = Dash;
                pen->dash_count = 2;
                return Ok;

        case DashStyleCustom:
                pen->dash_array = Custom;
                pen->dash_count = 1;
                return Ok;

        default:
                return GenericError;
        }
}

GpStatus
GdipGetPenDashOffset (GpPen *pen, float *offset)
{
        *offset = pen->dash_offset;
        return Ok;
}

GpStatus
GdipSetPenDashOffset (GpPen *pen, float offset)
{
        pen->dash_offset = offset;
        return Ok;
}

GpStatus
GdipGetPenDashCount (GpPen *pen, int *count)
{
        *count = pen->dash_count;

        return Ok;
}

GpStatus
GdipSetPenDashCount (GpPen *pen, int count)
{
        pen->dash_count = count;

        return Ok;
}

/*
 * This is the DashPattern property in Pen
 */
GpStatus
GdipGetPenDashArray (GpPen *pen, float **dash, int *count)
{
        *dash = pen->dash_array;
        *count = pen->dash_count;

        return Ok;
}

GpStatus
GdipSetPenDashArray (GpPen *pen, float *dash, int count)
{
        if (count == 0 || dash == NULL)
                return Ok;

        GdipSetPenDashStyle (pen, DashStyleCustom);
        pen->dash_array = dash;
        pen->dash_count = count;

        return Ok;
}

/*
 * MonoTODO: Find out what the difference is between CompoundArray and DashArray
 */
GpStatus
GdipGetPenCompoundCount (GpPen *pen, int *count)
{
        return NotImplemented;
}

GpStatus
GdipSetPenCompoundArray (GpPen *pen, const float *dash, int count)
{
        return NotImplemented;
}

GpStatus
GdipGetPenCompoundArray (GpPen *pen, float **dash, int count)
{
        return NotImplemented;
}
