/*
 * solidbrush.c
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
 */

#include "gdip.h"

void 
gdip_solidfill_init (GpSolidFill *brush)
{
	brush->color = 0;
        brush->type = BrushTypeSolidColor;
}

GpSolidFill *
gdip_solidfill_new (void)
{
	GpSolidFill *result = (GpSolidFill *) gdip_brush_new ();
	gdip_solidfill_init (result);
	return result;
}

void
gdip_solidfill_setup (GpGraphics *graphics, GpBrush *brush)
{
        int R = (brush->color & 0x00FF0000 ) >> 16;
        int G = (brush->color & 0x0000FF00 ) >> 8;
        int B = (brush->color & 0x000000FF );
        cairo_set_rgb_color (graphics->ct, (double) R, (double) G, (double) B);
}

GpStatus
gdip_solidfill_clone (GpBrush *brush, GpBrush **clonedBrush)
{
        *clonedBrush = gdip_brush_new ();
        (*clonedBrush)->color = brush->color;

        return Ok;
}

GpStatus 
GdipCreateSolidFill (int color, GpBrush **brush)
{
	*brush = gdip_brush_new ();
	(*brush)->color = color;
	return NotImplemented;
}

GpStatus
GdipSetSolidFillColor (GpSolidFill *brush, int color)
{
        brush->color = color;

        return Ok;
}

GpStatus
GdipGetSolidFillColor (GpSolidFill *brush, int *color)
{
        *color = brush->color;

        return Ok;
}
