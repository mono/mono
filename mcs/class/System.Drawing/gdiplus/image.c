//
// image.c
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

#include <math.h>

void gdip_image_init (gdip_image_ptr image)
{
	image->type = imageUndefined;
	image->surface = 0;
	image->graphics = 0;
}

void *gdip_image_create_Win32_HDC (gdip_image_ptr image)
{
	void *result = 0;
	switch (image->type) {
	case imageBitmap:
		result = gdip_bitmap_create_Win32_HDC ((gdip_bitmap_ptr)image);
		break;
	case imageMetafile:
		break;
	case imageUndefined:
		break;
	}
	return result;
}

void gdip_image_destroy_Win32_HDC (gdip_image_ptr image, void *hdc)
{
	switch (image->type) {
	case imageBitmap:
		gdip_bitmap_destroy_Win32_HDC ((gdip_bitmap_ptr)image, hdc);
		break;
	case imageMetafile:
		break;
	case imageUndefined:
		break;
	}
}

Status GdipDisposeImage (gdip_image_ptr image)
{
	switch (image->type){
	case imageBitmap:
		gdip_bitmap_dispose ((gdip_bitmap_ptr) image);
		break;
	case imageMetafile:
		break;
	case imageUndefined:
		break;
	}
	cairo_surface_destroy (image->surface);
	image->surface = 0;
	GdipFree (image);
	
	return Ok;
}

Status GdipGetImageGraphicsContext ( gdip_image_ptr image, gdip_graphics_ptr * graphics)
{
	if (image->graphics == 0) {
		image->graphics = gdip_graphics_new ();
		if (image->type == imageBitmap) {
			gdip_graphics_attach_bitmap (image->graphics, (gdip_bitmap_ptr)image);
		}
		else if (image->type == imageMetafile) {
		}
	}
	*graphics = image->graphics;
	return Ok;
}

Status GdipDrawImageI (gdip_graphics_ptr graphics, gdip_image_ptr image, int x, int y)
{
	printf("GdipDrawImageI. %p (type %d), %p, (%d,%d)\n", graphics, graphics->type, image, x, y);
	return NotImplemented;
}

Status GdipDrawImageRectI (gdip_graphics_ptr graphics, gdip_image_ptr image, int x, int y, int width, int height)
{
	gdip_graphics_ptr image_graphics = 0;
	cairo_surface_t *image_surface = 0;

	if (image->type != imageBitmap)
		return InvalidParameter;
	
	//printf("GdipDrawImageRectI. %p (type %d), %p, (%d,%d) (%d,%d)\n", graphics, graphics->type, image, x, y, width, height);
	
	GdipGetImageGraphicsContext (image, &image_graphics);
	if (image_graphics == 0) {
		printf("GdipDrawImageRectI. Error : cannot get graphics\n");
		return GenericError;
	}
	image_surface = cairo_current_target_surface (image_graphics->ct);
	if (image_surface == 0) {
		printf("GdipDrawImageRectI. Error : cannot get surface\n");
		return GenericError;
	}
	cairo_move_to (graphics->ct, x, y);
	cairo_set_pattern (graphics->ct, image_surface);
	cairo_rectangle (graphics->ct, x, y, width, height);
	cairo_fill (graphics->ct);
	
	return Ok;
}

