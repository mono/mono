//
// gdip_main.h
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

#ifndef _GDIP_MAIN_H
#define _GDIP_MAIN_H

#include <stdlib.h>
#include <stdio.h>

#include <cairo.h>
#include <cairo-xlib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/object.h>

typedef struct {
	unsigned int Width;
	unsigned int Height;
	int          Stride;
	int          PixelFormat;
	void         *Scan0;
	unsigned int Reserved;
} GdipBitmapData;

typedef struct {
	int left, top, right, bottom;
} Rect;

typedef struct tagRectF{
	float left, top, right, bottom;
} RectF;

enum graphics_type {
	gtUndefined,
	gtX11Drawable,
	gtMemoryBitmap
};

typedef struct {
	cairo_t         *ct;
	cairo_matrix_t  *copy_of_ctm;
	void            *hdc;
	int             hdc_busy_count;
	void            *image;
	int             type; 
} gdip_graphics, *gdip_graphics_ptr;

enum ImageType {
	imageUndefined,
	imageBitmap,
	imageMetafile
};

typedef struct {
	enum ImageType     type;
	cairo_surface_t   *surface;
	gdip_graphics_ptr  graphics;		// created by GdipGetImageGraphicsContext
} gdip_image, *gdip_image_ptr;

typedef struct {
	gdip_image	image;
	int             cairo_format;
	GdipBitmapData	data;
	void		*hBitmapDC;
	void		*hInitialBitmap;
	void		*hBitmap;
} gdip_bitmap, * gdip_bitmap_ptr;

typedef struct {
	int 		color;
} gdip_brush, *gdip_brush_ptr;

typedef struct {
	int 		color;
	float 		width;
} gdip_pen, *gdip_pen_ptr;

void gdip_image_init              (gdip_image_ptr image);
void *gdip_image_create_Win32_HDC (gdip_image_ptr image);
void gdip_image_destroy_Win32_HDC (gdip_image_ptr image, void *hdc);

void            gdip_bitmap_init  (gdip_bitmap_ptr bitmap);
gdip_bitmap_ptr gdip_bitmap_new   (void);
void            gdip_bitmap_dispose (gdip_bitmap_ptr bitmap);

void *gdip_bitmap_create_Win32_HDC (gdip_bitmap_ptr bitmap);
void gdip_bitmap_destroy_Win32_HDC (gdip_bitmap_ptr bitmap, void *hdc);

void *_get_gdi32Handle (void);
void *_get_user32Handle (void);

void gdip_graphics_init (gdip_graphics_ptr graphics);
gdip_graphics_ptr gdip_graphics_new (void);
void gdip_graphics_attach_bitmap (gdip_graphics_ptr graphics, gdip_bitmap_ptr image);
void gdip_graphics_detach_bitmap (gdip_graphics_ptr graphics, gdip_bitmap_ptr image);

void gdip_brush_init (gdip_brush_ptr brush);
gdip_brush_ptr gdip_brush_new (void);
void gdip_brush_setup (gdip_graphics_ptr graphics, gdip_brush_ptr brush);

void _init_pen (gdip_pen_ptr pen);
gdip_pen_ptr _new_pen (void);
void _setup_pen (gdip_graphics_ptr graphics, gdip_pen_ptr pen);

typedef struct {
	cairo_matrix_t		*matrix;
} gdip_state, *gdip_start_ptr;

extern Display *GDIP_display;

void initializeGdipWin32 (void);

#include "gdip_defs.h"

#endif // _GDIP_MAIN_H
