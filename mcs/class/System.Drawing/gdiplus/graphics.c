//
// graphics.c
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
//

#include "gdip_main.h"
#include "gdip_win32.h"

void gdip_graphics_init (gdip_graphics_ptr graphics)
{
	graphics->ct = cairo_create ();
	graphics->copy_of_ctm = cairo_matrix_create ();
	cairo_matrix_set_identity (graphics->copy_of_ctm);
	graphics->hdc = 0;
	graphics->hdc_busy_count = 0;
	graphics->image = 0;
	graphics->type = gtUndefined;
	cairo_select_font (graphics->ct, "serif:12");
}

gdip_graphics_ptr gdip_graphics_new ()
{
	gdip_graphics_ptr result = (gdip_graphics_ptr)GdipAlloc(sizeof(gdip_graphics));
	gdip_graphics_init (result);
	return result;
}

void gdip_graphics_attach_bitmap (gdip_graphics_ptr graphics, gdip_bitmap_ptr image)
{
	cairo_set_target_image (graphics->ct, image->data.Scan0, image->cairo_format,
				image->data.Width, image->data.Height, image->data.Stride);
	graphics->image = image;
	graphics->type = gtMemoryBitmap;
}

void gdip_graphics_detach_bitmap (gdip_graphics_ptr graphics, gdip_bitmap_ptr image)
{
	printf ("Implement graphics_detach_bitmap");
	//FIXME: implement me
}

Status GdipCreateFromHDC (int hDC, gdip_graphics_ptr *graphics)
{
	DC* dc = _get_DC_by_HDC (hDC);
	
	//printf ("GdipCreateFromHDC. in %d, DC %p\n", hDC, dc);
	if (dc == 0) return NotImplemented;
	
	*graphics = gdip_graphics_new ();
	cairo_set_target_drawable ((*graphics)->ct, GDIP_display, dc->physDev->drawable);
	_release_hdc (hDC);
	(*graphics)->hdc = (void*)hDC;
	(*graphics)->type = gtX11Drawable;
	//printf ("GdipCreateFromHDC. graphics %p, ct %p\n", (*graphics), (*graphics)->ct);
	return Ok;
}

Status GdipDeleteGraphics(gdip_graphics_ptr graphics)
{
	//FIXME: attention to surface (image, etc.)
	//printf ("GdipDeleteGraphics. graphics %p\n", graphics);
	cairo_matrix_destroy (graphics->copy_of_ctm);
	cairo_destroy (graphics->ct);
	GdipFree (graphics);
	return Ok;
}

Status GdipGetDC(gdip_graphics_ptr graphics, int *hDC)
{
	if (graphics->hdc == 0) {
		if (graphics->image != 0) {
			// Create DC
			graphics->hdc = gdip_image_create_Win32_HDC (graphics->image);
			if (graphics->hdc != 0) {
				++graphics->hdc_busy_count;
			}
		}
	}
	*hDC = (int)graphics->hdc;
	return Ok;
}

Status GdipReleaseDC(gdip_graphics_ptr graphics, int hDC)
{
	if (graphics->hdc != (void *)hDC) return InvalidParameter;
	if (graphics->hdc_busy_count > 0) {
		--graphics->hdc_busy_count;
		if (graphics->hdc_busy_count == 0) {
			// Destroy DC
			gdip_image_destroy_Win32_HDC (graphics->image, (void*)hDC);
			graphics->hdc = 0;
		}
	}
	return Ok;
}

#define MAX_GRAPHICS_STATE_STACK 100

gdip_state 	saved_stack [MAX_GRAPHICS_STATE_STACK];
int			current_stack_pos = 0;

Status GdipRestoreGraphics(gdip_graphics_ptr graphics, unsigned int graphicsState)
{
	if (graphicsState < MAX_GRAPHICS_STATE_STACK) {
		cairo_matrix_copy (graphics->copy_of_ctm, saved_stack[graphicsState].matrix);
		cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	}
	else {
		return InvalidParameter;
	}
	return Ok;
}

Status GdipSaveGraphics(gdip_graphics_ptr graphics, unsigned int * state)
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

#define PI 3.14159265358979323846
#define GRADTORAD PI / 180.0

Status GdipRotateWorldTransform (gdip_graphics_ptr graphics, float angle, int order)
{
	cairo_matrix_t *mtx = cairo_matrix_create ();
	cairo_matrix_rotate (mtx, angle * GRADTORAD);
	cairo_matrix_multiply (graphics->copy_of_ctm, mtx, graphics->copy_of_ctm );
	cairo_matrix_destroy ( mtx);
	cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	return Ok;
}

Status GdipTranslateWorldTransform (gdip_graphics_ptr graphics, float dx, float dy, int order)
{
	//FIXME: consider order here
	cairo_matrix_translate (graphics->copy_of_ctm, dx, dy);
	cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	return Ok;
}

Status GdipDrawLine (gdip_graphics_ptr graphics, gdip_pen_ptr pen, float x1, float y1, float x2, float y2)
{
	_setup_pen (graphics, pen);
	cairo_move_to (graphics->ct, x1, y1);
	cairo_line_to (graphics->ct, x2, y2);
	cairo_stroke (graphics->ct);
	return Ok;
}

Status GdipFillRectangle (gdip_graphics_ptr graphics, gdip_brush_ptr brush, float x1, float y1, float x2, float y2)
{
	gdip_brush_setup (graphics, brush);
	cairo_rectangle (graphics->ct, x1, y1, x2 - x1, y2 - y1);
	cairo_fill (graphics->ct);
	return Ok;
}

Status GdipDrawString (gdip_graphics_ptr graphics, const char *string, int len, void *font, RectF *rc, void *format, gdip_brush_ptr brush)
{
	cairo_save(graphics->ct);
	if (brush) {
		gdip_brush_setup (graphics, brush);
	} else {
		cairo_set_rgb_color (graphics->ct, 0., 0., 0.);
	}
	cairo_move_to (graphics->ct, rc->left, rc->top + 12);
	cairo_scale_font (graphics->ct, 12);
	cairo_show_text (graphics->ct, string);
	cairo_restore(graphics->ct);
	return Ok;
}

