//
// gdip_win32.h
//
// Authors:
//   Alexandre Pigolkine(pigolkine@gmx.de)
//

#ifndef _GDIP_WIN32_H
#define _GDIP_WIN32_H

#include <cairo.h>
#include <cairo-xlib.h>
#include <mono/jit/jit.h>

// sizeof (GDIOBJHDR) = 12 (2 + 2 + 4 + 4)
// offsetof (DC, physDev) = 20 (12 + 4 + 4)
typedef struct tagGDIOBJHDR {
	short 	next;
	short 	wMagic;
	long 	dwCount;
	void*	funcs;
} GDIOBJHDR;

typedef struct tagX11DRV_PDEVICE
{
    void*         hdc;
    void          *dc;          /* direct pointer to DC, should go away */
    GC            gc;          /* X Window GC */
    Drawable      drawable;
} X11DRV_PDEVICE;

typedef struct tagDC {
    GDIOBJHDR    	header;
    void*        	hSelf;          /* Handle to this DC */
    void 		 	*funcs; 		/* DC function table */
    X11DRV_PDEVICE 	*physDev;       /* Physical device (driver-specific) */
} DC;

typedef struct {
  BYTE rgbBlue;
  BYTE rgbGreen;
  BYTE rgbRed;
  BYTE rgbReserved;
} RGBQUAD, *LPRGBQUAD;

typedef struct
{
    DWORD 	biSize;
    LONG  	biWidth;
    LONG  	biHeight;
    WORD 	biPlanes;
    WORD 	biBitCount;
    DWORD 	biCompression;
    DWORD 	biSizeImage;
    LONG  	biXPelsPerMeter;
    LONG  	biYPelsPerMeter;
    DWORD 	biClrUsed;
    DWORD 	biClrImportant;
} BITMAPINFOHEADER, *PBITMAPINFOHEADER, *LPBITMAPINFOHEADER;

  /* biCompression */
#define BI_RGB           0
#define BI_RLE8          1
#define BI_RLE4          2
#define BI_BITFIELDS     3

typedef struct {
	BITMAPINFOHEADER bmiHeader;
	RGBQUAD	bmiColors[1];
} BITMAPINFO, *PBITMAPINFO, *LPBITMAPINFO;

#    pragma pack(2)
typedef struct
{
    WORD    bfType;
    DWORD   bfSize;
    WORD    bfReserved1;
    WORD    bfReserved2;
    DWORD   bfOffBits;
} BITMAPFILEHEADER, *PBITMAPFILEHEADER, *LPBITMAPFILEHEADER;
#    pragma pack()

#define BFT_BITMAP 0x4d42


#  define __stdcall __attribute__((__stdcall__))

extern void* (__stdcall *CreateCompatibleDC_pfn) (void * hdc);
extern void* (__stdcall *CreateCompatibleBitmap_pfn) (void * hdc, int width, int height);
extern void* (__stdcall *GetDC_pfn) (void * hwnd);

extern void* (__stdcall *SelectObject_pfn) (void * hdc, void *object);

extern void (__stdcall *DeleteDC_pfn) (void * hdc);
extern int (__stdcall *DeleteObject_pfn) (void * obj);
extern void (__stdcall *ReleaseDC_pfn) (void *hwnd, void * hdc);

extern int (__stdcall *GetDIBits_pfn) (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse);
extern int (__stdcall *SetDIBits_pfn) (void *hdc, void *hbitmap, unsigned startScan, unsigned scanLines, void *bitmapBits, PBITMAPINFO pbmi, unsigned int colorUse);


DC *_get_DC_by_HDC (int hDC);
void _release_hdc (int hdc);

#endif // _GDIP_WIN32_H
