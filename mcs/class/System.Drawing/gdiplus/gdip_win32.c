/*
 * gdip_win32.c
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

#include "gdip_win32.h"
#include <dlfcn.h>

static void * gdi32Handle = 0;
static void * user32Handle = 0;

static void _load_gdi32 (void)
{
	if (gdi32Handle == 0) {
		gdi32Handle = dlopen ("libgdi32.dll.so", 1);
		if (gdi32Handle == 0) {
			gdi32Handle = dlopen ("/usr/local/lib/libgdi32.dll.so", 1);
		}
		if (gdi32Handle == 0) {
			gdi32Handle = dlopen ("/usr/lib/libgdi32.dll.so", 1);
		}
	}
}

static void _load_user32 (void)
{
	if (user32Handle == 0) {
		user32Handle = dlopen ("libuser32.dll.so", 1);
		if (user32Handle == 0) {
			user32Handle = dlopen ("/usr/local/lib/libuser32.dll.so", 1);
		}
		if (user32Handle == 0) {
			user32Handle = dlopen ("/usr/lib/libuser32.dll.so", 1);
		}
	}
}

void *_get_gdi32Handle (void)
{
	_load_gdi32 ();
	return gdi32Handle;
}

void *_get_user32Handle (void)
{
	_load_user32 ();
	return user32Handle;
}

DC* DC_GetDCPtr_gdip (int hdc)
{
	return 0;
}

void GDI_ReleaseObj_gdip (int hdc)
{
}

void* __stdcall CreateCompatibleDC_gdip (void * hdc)
{
	return 0;
}

void* __stdcall CreateCompatibleBitmap_gdip (void * hdc, int width, int height)
{
	return 0;
}

void* __stdcall GetDC_gdip (void * hwnd)
{
	return 0;
}

void* __stdcall SelectObject_gdip (void * hdc, void *object)
{
	return 0;
}

void __stdcall DeleteDC_gdip (void * hdc)
{
}

int __stdcall DeleteObject_gdip (void * obj)
{
	return 0;
}

void __stdcall ReleaseDC_gdip (void *hwnd, void * hdc)
{
}

int __stdcall GetDIBits_gdip (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse)
{
	return 0;
}

int __stdcall SetDIBits_gdip (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse)
{
	return 0;
}

void* (__stdcall *CreateCompatibleDC_pfn) (void * hdc);
void* (__stdcall *CreateCompatibleBitmap_pfn) (void * hdc, int width, int height);
void* (__stdcall *GetDC_pfn) (void * hwnd);

void* (__stdcall *SelectObject_pfn) (void * hdc, void *object);

void (__stdcall *DeleteDC_pfn) (void * hdc);
int (__stdcall *DeleteObject_pfn) (void * obj);
void (__stdcall *ReleaseDC_pfn) (void *hwnd, void * hdc);

int (__stdcall *GetDIBits_pfn) (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse);
int (__stdcall *SetDIBits_pfn) (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse);

DC* (*DC_GetDCPtr_pfn) (int hdc);
void (*GDI_ReleaseObj_pfn) (int hdc);

#define CHECK_FUNCTION(name) if (name##_pfn == 0) name##_pfn = name##_gdip;
void initializeGdipWin32 (void)
{
	void * gdi32Handle = _get_gdi32Handle ();
	void * user32Handle = _get_user32Handle ();
	
	if (gdi32Handle != 0 && user32Handle != 0) {
		CreateCompatibleDC_pfn = dlsym (gdi32Handle,"CreateCompatibleDC");
		CreateCompatibleBitmap_pfn = dlsym (gdi32Handle,"CreateCompatibleBitmap");
		SelectObject_pfn = dlsym (gdi32Handle,"SelectObject");
		DeleteDC_pfn = dlsym (gdi32Handle,"DeleteDC");
		DeleteObject_pfn = dlsym (gdi32Handle,"DeleteObject");
		SetDIBits_pfn = dlsym (gdi32Handle,"SetDIBits");
		GetDIBits_pfn = dlsym (gdi32Handle,"GetDIBits");
		
		GetDC_pfn = dlsym (user32Handle,"GetDC");
		ReleaseDC_pfn = dlsym (user32Handle, "ReleaseDC");
		
		DC_GetDCPtr_pfn = dlsym(gdi32Handle,"DC_GetDCPtr");
		GDI_ReleaseObj_pfn = dlsym(gdi32Handle,"GDI_ReleaseObj");
	}
	CHECK_FUNCTION (CreateCompatibleDC);
	CHECK_FUNCTION (CreateCompatibleBitmap);
	CHECK_FUNCTION (SelectObject);
	CHECK_FUNCTION (DeleteDC);
	CHECK_FUNCTION (DeleteObject);
	CHECK_FUNCTION (SetDIBits);
	CHECK_FUNCTION (GetDIBits);
	CHECK_FUNCTION (GetDC);
	CHECK_FUNCTION (ReleaseDC);
	
	CHECK_FUNCTION (DC_GetDCPtr);
	CHECK_FUNCTION (GDI_ReleaseObj);
}

DC *_get_DC_by_HDC (int hDC)
{
	return DC_GetDCPtr_pfn (hDC);
}

void _release_hdc (int hdc)
{
	return GDI_ReleaseObj_pfn (hdc);
}
