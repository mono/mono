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
 *
 */

#include "gdip.h"

void 
gdip_pen_init (GpPen *pen)
{
        pen->color = 0;
        pen->width = 1.0F;
        pen->miter_limit = 0;
        pen->line_join = LineJoinMiter;
        pen->matrix = NULL;
}

GpPen*
gdip_pen_new (void)
{
        GpPen *result = (GpPen *) GdipAlloc (sizeof (GpPen));
        gdip_pen_init (result);
        return result;
}

cairo_line_join_t
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
                printf ("We don't support MiterClipped LineJoins yet.\n");
                return CAIRO_LINE_JOIN_MITER;
        }
}

cairo_line_cap_t
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
GdipDeletePen (GpPen *pen)
{
        return NotImplemented; 
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
GdipGetPenLineCap (GpPen *pen, GpLineJoin *lineCap)
{
        *lineCap = pen->line_cap;
        return Ok;
}

GpStatus
GdipSetPenTransform (GpPen *pen, GpMatrix *matrix)
{
        pen->matrix = matrix;
        return Ok;
}

GpStatus
GdipGetPenTransform (GpPen *pen, GpMatrix *matrix)
{
        matrix = pen->matrix;
        return Ok;
}
