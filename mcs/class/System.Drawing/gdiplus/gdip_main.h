/*
 * gdip_main.h
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

#ifndef _GDIP_MAIN_H
#define _GDIP_MAIN_H

#include <stdlib.h>
#include <stdio.h>

#include <cairo.h>
#include <cairo-xlib.h>
#include <mono/io-layer/uglify.h>

typedef struct {
	unsigned int Width;
	unsigned int Height;
	int          Stride;
	int          PixelFormat;
	void         *Scan0;
	unsigned int Reserved;
} GdipBitmapData, BitmapData;

typedef struct {
	int left, top, right, bottom;
} GpRect, Rect;

typedef struct tagRectF{
	float left, top, right, bottom;
} GpRectF, RectF;

typedef struct {
        int X, Y;
} GpPoint;

typedef struct {
        float X, Y;
} GpPointF;

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
} GpGraphics;

enum ImageType {
	imageUndefined,
	imageBitmap,
	imageMetafile
};

typedef struct {
	enum ImageType     type;
	cairo_surface_t   *surface;
	GpGraphics  *graphics;		/* created by GdipGetImageGraphicsContext */
} GpImage;

typedef struct {
	GpImage	image;
	int cairo_format;
	BitmapData	data;
	void *hBitmapDC;
	void *hInitialBitmap;
	void *hBitmap;
} GpBitmap;

typedef struct {
	int color;
} GpBrush;

typedef struct {
	int color;
	float width;
} GpPen;

typedef cairo_matrix_t GpMatrix;

void gdip_image_init              (GpImage *image);
void *gdip_image_create_Win32_HDC (GpImage *image);
void gdip_image_destroy_Win32_HDC (GpImage *image, void *hdc);

void gdip_bitmap_init  (GpBitmap *bitmap);
GpBitmap *gdip_bitmap_new   (void);
void gdip_bitmap_dispose (GpBitmap *bitmap);

void *gdip_bitmap_create_Win32_HDC (GpBitmap *bitmap);
void gdip_bitmap_destroy_Win32_HDC (GpBitmap *bitmap, void *hdc);

void *_get_gdi32Handle (void);
void *_get_user32Handle (void);

void gdip_graphics_init (GpGraphics *graphics);
GpGraphics *gdip_graphics_new (void);
void gdip_graphics_attach_bitmap (GpGraphics *graphics, GpBitmap *image);
void gdip_graphics_detach_bitmap (GpGraphics *graphics, GpBitmap *image);

void gdip_brush_init (GpBrush *brush);
GpBrush *gdip_brush_new (void);
void gdip_brush_setup (GpGraphics *graphics, GpBrush *brush);

void gdip_pen_init (GpPen *pen);
GpPen *gdip_pen_new (void);
void gdip_pen_setup (GpGraphics *graphics, GpPen *pen);

typedef struct {
	cairo_matrix_t		*matrix;
} GpState;

extern Display *GDIP_display;

void initializeGdipWin32 (void);

#include "gdip_defs.h"

#endif /* _GDIP_MAIN_H */
