/*
 * Enums, structs and functions
 */

#ifndef _GDIP_DEFS_H
#define _GDIP_DEFS_H

typedef enum {
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
} GpStatus;

typedef enum  {
    MatrixOrderPrepend    = 0,
    MatrixOrderAppend     = 1
} MatrixOrder, GpMatrixOrder;

typedef enum {
    UnitWorld	 = 0,      
    UnitDisplay	 = 1,    
    UnitPixel	 = 2,      
    UnitPoint	 = 3,     
    UnitInch	 = 4,       
    UnitDocument = 5,   
    UnitMillimeter = 6 
} Unit;

typedef enum {
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
} PixelFormat;

typedef enum {
	ReadOnly = 1,
	ReadWrite = 3,
	UserInputBuffer = 4,
	WriteOnly = 2
} ImageLockMode;

typedef enum {
        FillModeAlternate,
        FillModeWinding
} GpFillMode;

/* Bitmap */
GpStatus GdipCreateBitmapFromScan0 (int width, int height, int strideIn, int format, void *scan0, GpBitmap **bitmap);
GpStatus GdipCreateBitmapFromGraphics (int width, int height, GpGraphics *graphics, GpBitmap **bitmap);
GpStatus GdipBitmapLockBits (GpBitmap *bmp, Rect *rc, int flags, int format, GdipBitmapData *result);
GpStatus GdipBitmapUnlockBits (GpBitmap *bmp, GdipBitmapData *bmpData);

/* Graphics */
GpStatus GdipCreateFromHDC (int hDC, GpGraphics **graphics);
GpStatus GdipDeleteGraphics (GpGraphics *graphics);
GpStatus GdipGetDC (GpGraphics *graphics, int *hDC);
GpStatus GdipReleaseDC (GpGraphics *graphics, int hDC);
GpStatus GdipRestoreGraphics (GpGraphics *graphics, unsigned int graphicsState);
GpStatus GdipSaveGraphics(GpGraphics *graphics, unsigned int * state);
GpStatus GdipRotateWorldTransform (GpGraphics *graphics, float angle, int order);
GpStatus GdipTranslateWorldTransform (GpGraphics *graphics, float dx, float dy, int order);
GpStatus GdipDrawLine (GpGraphics *graphics, GpPen *pen, float x1, float y1, float x2, float y2);
GpStatus GdipDrawLineI (GpGraphics *graphics, GpPen *pen, int x1, int y1, int x2, int y2);
GpStatus GdipDrawLines (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count);
GpStatus GdipDrawLinesI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count);

GpStatus GdipFillRectangle (GpGraphics *graphics, GpBrush *brush, float x1, float y1, float x2, float y2);

/* Brush */
GpStatus GdipCloneBrush (GpBrush *brush, GpBrush **clonedBrush);
GpStatus GdipDeleteBrush (GpBrush *brush);

/* Text */
GpStatus GdipDrawString (GpGraphics *graphics, const char *string, int len, void *font, RectF *rc, void *format, GpBrush *brush);

/* Matrix */
GpStatus GdipCreateMatrix (GpMatrix **matrix);
GpStatus GdipCreateMatrix2 (float m11, float m12, float m21, float m22, float dx, float dy, GpMatrix **matrix);
GpStatus GdipCreateMatrix3 (GpRectF *rect, GpPointF *dstplg, GpMatrix **matrix);
GpStatus GdipCreateMatrix3I (GpRect *rect, GpPoint *dstplg, GpMatrix **matrix);
GpStatus GdipCloneMatrix (GpMatrix *matrix, GpMatrix **cloneMatrix);
GpStatus GdipDeleteMatrix (GpMatrix *matrix);
GpStatus GdipSetMatrixElements (GpMatrix *matrix, float m11, float m12, float m21, float m22, float dx, float dy);
GpStatus GdipMultiplyMatrix (GpMatrix *matrix, GpMatrix *matrix2, GpMatrixOrder order);
GpStatus GdipTranslateMatrix (GpMatrix *matrix, float offsetX, float offsetY, GpMatrixOrder order);
GpStatus GdipScaleMatrix (GpMatrix *matrix, float scaleX, float scaleY, GpMatrixOrder order);
GpStatus GdipRotateMatrix(GpMatrix *matrix, float angle, GpMatrixOrder order);
GpStatus GdipShearMatrix (GpMatrix *matrix, float shearX, float shearY, GpMatrixOrder order);
GpStatus GdipInvertMatrix (GpMatrix *matrix);
GpStatus GdipTransformMatrixPoints (GpMatrix *matrix, GpPointF *pts, int count);
GpStatus GdipTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count);
GpStatus GdipVectorTransformMatrixPoints (GpMatrix *matrix, GpPointF *pts, int count);
GpStatus GdipVectorTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count);
GpStatus GdipGetMatrixElements (GpMatrix *matrix, float *matrixOut);
GpStatus GdipIsMatrixInvertible (GpMatrix *matrix, int *result);
GpStatus GdipIsMatrixIdentity (GpMatrix *matrix, int *result);
GpStatus GdipIsMatrixEqual (GpMatrix *matrix, GpMatrix *matrix2, int *result);

GpStatus gdip_get_status (cairo_t *ct);

/* Memory */
void * GdipAlloc (int size);
void GdipFree (void *ptr);

#endif /* _GDIP_DEFS_H */
