//
// Enums, structs and functions
//

#ifndef _GDIP_DEFS_H
#define _GDIP_DEFS_H

enum StatusCode
{
    Ok = 0,
    GenericError = 1,
    InvalidParameter = 2,
    OutOfMemory = 3,
    ObjectBusy = 4,
    InsufficientBuffer = 5,
    NotImplemented = 6,
    Win32Error = 7,
    WrongState = 8,
    Aborted = 9,
    FileNotFound = 10,
    ValueOverflow = 11,
    AccessDenied = 12,
    UnknownImageFormat = 13,
    FontFamilyNotFound = 14,
    FontStyleNotFound = 15,
    NotTrueTypeFont = 16,
    UnsupportedGdiplusVersion = 17,
    GdiplusNotInitialized = 18,
    PropertyNotFound = 19,
    PropertyNotSupported = 20
};

#define Status int

enum MatrixOrder
{
    MatrixOrderPrepend    = 0,
    MatrixOrderAppend     = 1
};

enum Unit
{
    UnitWorld	 = 0,      
    UnitDisplay	 = 1,    
    UnitPixel	 = 2,      
    UnitPoint	 = 3,     
    UnitInch	 = 4,       
    UnitDocument = 5,   
    UnitMillimeter = 6 
};

enum PixelFormat {
	Alpha = 262144,
	Canonical = 2097152,
	DontCare = 0,
	Extended = 1048576,
	Format16bppArgb1555 = 397319,
	Format16bppGrayScale = 1052676,
	Format16bppRgb555 = 135173,
	Format16bppRgb565 = 135174,
	Format1bppIndexed = 196865,
	Format24bppRgb = 137224,
	Format32bppArgb = 2498570,
	Format32bppPArgb = 925707,
	Format32bppRgb = 139273,
	Format48bppRgb = 1060876, 
	Format4bppIndexed = 197634,
	Format64bppArgb = 3424269,
	Format64bppPArgb = 1851406,
	Format8bppIndexed = 198659,
	Gdi = 131072,
	Indexed = 65536,
	Max = 15,
	PAlpha = 524288,
	Undefined = 0
};

enum ImageLockMode {
	ReadOnly = 1,
	ReadWrite = 3,
	UserInputBuffer = 4,
	WriteOnly = 2
};

Status GdipCreateBitmapFromScan0 (int width, int height, int strideIn, int format, void * scan0, gdip_bitmap_ptr * bitmap);
Status GdipCreateBitmapFromGraphics (int width, int height, gdip_graphics_ptr graphics, gdip_bitmap_ptr * bitmap);
Status GdipBitmapLockBits (gdip_bitmap_ptr bmp, Rect *rc, int flags, int format, BitmapData * bmpData);
Status GdipBitmapUnlockBits (gdip_bitmap_ptr bmp, BitmapData * bmpData);

Status GdipCreateFromHDC(int hDC, gdip_graphics_ptr *graphics);
Status GdipDeleteGraphics(gdip_graphics_ptr graphics);
Status GdipGetDC(gdip_graphics_ptr graphics, int *hDC);
Status GdipReleaseDC(gdip_graphics_ptr graphics, int hDC);
Status GdipRestoreGraphics(gdip_graphics_ptr graphics, unsigned int graphicsState);
Status GdipSaveGraphics(gdip_graphics_ptr graphics, unsigned int * state);
Status GdipRotateWorldTransform (gdip_graphics_ptr graphics, float angle, int order);
Status GdipTranslateWorldTransform (gdip_graphics_ptr graphics, float dx, float dy, int order);
Status GdipDrawLine (gdip_graphics_ptr graphics, gdip_pen_ptr pen, float x1, float y1, float x2, float y2);
Status GdipFillRectangle (gdip_graphics_ptr graphics, gdip_brush_ptr brush, float x1, float y1, float x2, float y2);
Status GdipDrawString (gdip_graphics_ptr graphics, const char *string, int len, void *font, RectF *rc, void *format, gdip_brush_ptr brush);

Status GdipCloneBrush (gdip_brush_ptr brush, gdip_brush_ptr * clonedBrush);
Status GdipDeleteBrush (gdip_brush_ptr brush);

void * GdipAlloc (int size);
void GdipFree (void * ptr);


#endif // _GDIP_DEFS_H
