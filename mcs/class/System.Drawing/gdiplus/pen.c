//
// pen.c
//
// Copyright (c) 2003 Alexandre Pigolkine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//   Alexandre Pigolkine(pigolkine@gmx.de)
//

#include "gdip_main.h"

void _init_pen (gdip_pen_ptr pen)
{
	pen->color = 0;
	pen->width = 1.0F;
}

gdip_pen_ptr _new_pen (void)
{
	gdip_pen_ptr result = (gdip_pen_ptr)GdipAlloc(sizeof(gdip_pen));
	_init_pen (result);
	return result;
}

void _setup_pen (gdip_graphics_ptr graphics, gdip_pen_ptr pen)
{
	int R = (pen->color & 0x00FF0000 ) >> 16;
	int G = (pen->color & 0x0000FF00 ) >> 8;
	int B = (pen->color & 0x000000FF );
	cairo_set_rgb_color (graphics->ct, (double) R, (double) G, (double) B);
	cairo_set_line_width (graphics->ct, (double) pen->width);
}

Status GdipCreatePen1(int argb, float width, int unit, gdip_pen_ptr *pen)
{
	*pen = _new_pen ();
	(*pen)->color = argb;
	(*pen)->width = width;
	return Ok; 
}

Status GdipDeletePen(gdip_pen_ptr pen)
{
	return NotImplemented; 
}
