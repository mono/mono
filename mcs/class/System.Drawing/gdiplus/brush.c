/*
 * brush.c
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
gdip_brush_setup (GpGraphics *graphics, GpBrush *brush)
{
        GpBrushType type;
        GdipGetBrushType (brush, &type);

        if (type == BrushTypeSolidColor) {
                GpSolidFill *solid = brush;
                gdip_solidfill_setup (graphics, solid);
        }
}

GpBrush *
gdip_brush_new (void)
{
        GpBrush *result = (GpBrush *) GdipAlloc (sizeof (GpBrush));

        return result;
}

GpStatus 
GdipCloneBrush (GpBrush *brush, GpBrush **clonedBrush)
{
	GpBrushType type;
        GdipGetBrushType (brush, &type);

        if (type == BrushTypeSolidColor)
                return gdip_solidfill_clone (brush, clonedBrush);
        else
                return NotImplemented;
}

GpStatus 
GdipDeleteBrush (GpBrush *brush)
{
        GdipFree (brush);
	return Ok;
}

GpStatus
GdipGetBrushType (GpBrush *brush, GpBrushType *type)
{
        *type = brush->type;
        return Ok;
}
