/*
 * gdip.h
 *
 * Authors:
 *      Alexandre Pigolkine (pigolkine@gmx.de)
 *      Duncan Mak (duncan@ximian.com)
 *      Miguel de Icaza (miguel@ximian.com)
 *
 * Copyright (C) Novell, Inc. 2003.
 */

#ifndef _GDIP_H
#define _GDIP_H

#include <stdlib.h>
#include <stdio.h>

#include <cairo.h>
#include <cairo-xlib.h>
#include <mono/io-layer/uglify.h>

/*
 * Constants
 *
 */
#define PI 3.14159265358979323846
#define DEGTORAD PI / 180.0

/*
 * Enums
 *
 */

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
} GpUnit, Unit;

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

typedef enum {
        DashStyleSolid = 0,      /* solid line */
        DashStyleDash = 1,       /* dashed line */
        DashStyleDot = 2,        /* dotted line */
        DashStyleDashDot = 3,    /* alt. dash-dot */
        DashStyleDashDotDot = 4, /* alt. dash-dot-dot */
        DashStyleCustom = 5      /* user-defined */
} GpDashStyle;

typedef enum {
        LineJoinMiter = 0,       /* sharp corner */
        LineJoinBevel = 1,       /* round corner */
        LineJoinRound = 2,       /* circular, smooth, circular arc */
        LineJoinMiterClipped = 3 /* miter, sharp or beveled corner */
} GpLineJoin;

typedef enum  {
	imageUndefined,
	imageBitmap,
	imageMetafile
} ImageType;

typedef enum {
	gtUndefined,
	gtX11Drawable,
	gtMemoryBitmap
} GraphicsType;

typedef enum {
    LineCapFlat = 0,
    LineCapSquare = 1,
    LineCapRound = 2,
    LineCapTriangle = 3,
    LineCapNoAnchor = 0x10,
    LineCapSquareAnchor = 0x11,
    LineCapRoundAnchor = 0x12,
    LineCapDiamondAnchor = 0x13,
    LineCapArrowAnchor = 0x14,
    LineCapCustom = 0xff
} GpLineCap;

typedef enum {
        PenAlignmentCenter = 0,
        PenAlignmentInset = 1
} GpPenAlignment, PenAlignment;

typedef enum {
        BrushTypeSolidColor = 0,
        BrushTypeHatchFill = 1,
        BrushTypeTextureFill = 2,
        BrushTypePathGradient = 3,
        BrushTypeLinearGradient = 4
} GpBrushType, BrushType;

/*
 * Structures
 *
 */
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
} GpPoint, Point;

typedef struct {
        float X, Y;
} GpPointF, PointF;

typedef struct {
	cairo_t         *ct;
	cairo_matrix_t  *copy_of_ctm;
	void            *hdc;
	int             hdc_busy_count;
	void            *image;
	int             type; 
} GpGraphics;

typedef cairo_matrix_t GpMatrix;

typedef struct {
	int color;
        GpBrushType type;
} GpBrush, GpSolidFill;

typedef struct {
	int color;
        GpBrush *brush; 
	float width;
        float miter_limit;
        GpLineJoin line_join;
        GpDashStyle dash_style;
        GpLineCap line_cap;
        GpPenAlignment mode;
        float dash_offset;
        int dash_count;
        float *dash_array;
        GpUnit unit;
        GpMatrix *matrix;
} GpPen;

typedef struct {
	cairo_matrix_t		*matrix;
} GpState;

typedef struct {
	ImageType     type;
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
        GpFillMode fillMode;
        int count;
        GpPointF *float_points;
        GpPoint *int_points;
} GpPath;

/*
 * Functions
 * 
 */
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

/* Brush */
void gdip_brush_setup (GpGraphics *graphics, GpBrush *brush);
GpBrush *gdip_brush_new (void);

/* Solid fill */
void gdip_solidfill_init (GpSolidFill *brush);
void gdip_solidfill_setup (GpGraphics *graphics, GpBrush *brush);
GpStatus gdip_solidfill_clone (GpBrush *brush, GpBrush **clonedBrush);
GpSolidFill *gdip_solidfill_new (void);

/* Pen */
void gdip_pen_init (GpPen *pen);
GpPen *gdip_pen_new (void);
void gdip_pen_setup (GpGraphics *graphics, GpPen *pen);

extern Display *GDIP_display;

void initializeGdipWin32 (void);

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
GpStatus GdipRotateWorldTransform (GpGraphics *graphics, float angle, GpMatrixOrder order);
GpStatus GdipTranslateWorldTransform (GpGraphics *graphics, float dx, float dy, GpMatrixOrder order);
GpStatus GdipDrawBezier (GpGraphics *graphics, GpPen *pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
GpStatus GdipDrawBezierI (GpGraphics *graphics, GpPen *pen, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);
GpStatus GdipDrawBeziers (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count);
GpStatus GdipDrawBeziersI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count);
GpStatus GdipDrawEllipse (GpGraphics *graphics, GpPen *pen, float x, float y, float width, float height);
GpStatus GdipDrawLine (GpGraphics *graphics, GpPen *pen, float x1, float y1, float x2, float y2);
GpStatus GdipDrawLineI (GpGraphics *graphics, GpPen *pen, int x1, int y1, int x2, int y2);
GpStatus GdipDrawLines (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count);
GpStatus GdipDrawLinesI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count);
GpStatus GdipDrawPie (GpGraphics *graphics, GpPen *pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
GpStatus GdipDrawPieI (GpGraphics *graphics, GpPen *pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
GpStatus GdipDrawPolygon (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count);
GpStatus GdipDrawPolygonI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count);
GpStatus GdipDrawRectangle (GpGraphics *graphics, GpPen *pen, float x, float y, float width, float height);
GpStatus GdipDrawRectangleI (GpGraphics *graphics, GpPen *pen, int x, int y, int width, int height);
GpStatus GdipFillEllipse (GpGraphics *graphics, GpBrush *brush, float x, float y, float width, float height);
GpStatus GdipFillEllipseI (GpGraphics *graphics, GpBrush *brush, int x, int y, int width, int height);
GpStatus GdipFillRectangle (GpGraphics *graphics, GpBrush *brush, float x1, float y1, float x2, float y2);
GpStatus GdipFillPolygon (GpGraphics *graphics, GpBrush *brush, GpPointF *points, int count, GpFillMode fillMode);
GpStatus GdipFillPolygonI (GpGraphics *graphics, GpBrush *brush, GpPoint *points, int count, GpFillMode fillMode);
GpStatus GdipFillPolygon2 (GpGraphics *graphics, GpBrush *brush, GpPointF *points, int count);
GpStatus GdipFillPolygon2I (GpGraphics *graphics, GpBrush *brush, GpPoint *points, int count);
GpStatus GdipSetRenderingOrigin (GpGraphics *graphics, int x, int y);
GpStatus GdipGetRenderingOrigin (GpGraphics *graphics, int *x, int *y);

/* Status */
GpStatus gdip_get_status (cairo_status_t status);

/* Brush */
GpStatus GdipCloneBrush (GpBrush *brush, GpBrush **clonedBrush);
GpStatus GdipDeleteBrush (GpBrush *brush);
GpStatus GdipGetBrushType (GpBrush *brush, GpBrushType *brushType);

/* Solidfill Brush */
GpStatus GdipGetSolidFillColor (GpSolidFill *brush, int *color);

/* Pen */
GpStatus GdipCreatePen1 (int argb, float width, GpUnit unit, GpPen **pen);
GpStatus GdipCreatePen2 (GpBrush *brush, float width, GpUnit unit, GpPen **pen);
GpStatus GdipClonePen (GpPen *pen, GpPen **clonedpen);
GpStatus GdipSetPenWidth (GpPen *pen, float width);
GpStatus GdipGetPenWidth (GpPen *pen, float *width);
GpStatus GdipSetPenBrushFill (GpPen *pen, GpBrush *brush);
GpStatus GdipGetPenBrushFill (GpPen *pen, GpBrush **brush);
GpStatus GdipSetPenColor (GpPen *pen, int color);
GpStatus GdipGetPenColor (GpPen *pen, int *color);;
GpStatus GdipSetPenDashStyle (GpPen *pen, GpDashStyle dashStyle);
GpStatus GdipGetPenDashStyle (GpPen *pen, GpDashStyle *dashStyle);
GpStatus GdipSetPenDashOffset (GpPen *pen, float offset);
GpStatus GdipGetPenDashOffset (GpPen *pen, float *offset);
GpStatus GdipSetPenDashCount (GpPen *pen, int count);
GpStatus GdipGetPenDashCount (GpPen *pen, int *count);
GpStatus GdipSetPenDashArray (GpPen *pen, float *dash, int count);
GpStatus GdipGetPenDashArray (GpPen *pen, float **dash, int *count);
GpStatus GdipSetPenDashCompoundArray (GpPen *pen, float *dash, int count);
GpStatus GdipGetPenDashCompoundArray (GpPen *pen, float **dash, int *count);
GpStatus GdipGetPenDashCompoundCount (GpPen *pen, int *count);
GpStatus GdipSetPenMode (GpPen *pen, GpPenAlignment penMode);
GpStatus GdipGetPenMode (GpPen *pen, GpPenAlignment *penMode);
GpStatus GdipSetPenUnit (GpPen *pen, GpUnit unit);
GpStatus GdipGetPenUnit (GpPen *pen, GpUnit *unit);
GpStatus GdipDeletePen (GpPen *pen);
GpStatus GdipSetPenMiterLimit (GpPen *pen, float miterLimit);
GpStatus GdipGetPenMiterLimit (GpPen *pen, float *miterLimit);
GpStatus GdipSetPenLineCap (GpPen *pen, GpLineCap lineCap);
GpStatus GdipGetPenLineCap (GpPen *pen, GpLineCap *lineCap);
GpStatus GdipSetPenLineJoin (GpPen *pen, GpLineJoin lineJoin);
GpStatus GdipGetPenLineJoin (GpPen *pen, GpLineJoin *lineJoin);

/* Text */
GpStatus GdipDrawString (GpGraphics *graphics, const char *string, int len, void *font, RectF *rc, void *format, GpBrush *brush);

/* Matrix */
GpStatus GdipCreateMatrix (GpMatrix **matrix);
GpStatus GdipCreateMatrix2 (float m11, float m12, float m21, float m22, float dx, float dy, GpMatrix **matrix);
GpStatus GdipCreateMatrix3 (const GpRectF *rect, const GpPointF *dstplg, GpMatrix **matrix);
GpStatus GdipCreateMatrix3I (const GpRect *rect, const GpPoint *dstplg, GpMatrix **matrix);
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

/* Memory */
void *GdipAlloc (int size);
void GdipFree (void *ptr);

#endif /* _GDIP_H */
