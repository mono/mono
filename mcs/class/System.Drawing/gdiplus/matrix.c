/*
 * matrix.c
 *
 * Author: Duncan Mak (duncan@ximian.com)
 * 
 * Copyright (C) Novell, Inc. 2003.
 *
 */

#include "gdip.h"

GpStatus
GdipCreateMatrix (GpMatrix **matrix)
{
        *matrix = cairo_matrix_create ();

        return Ok;
}

GpStatus
GdipCreateMatrix2 (float m11, float m12, float m21, float m22, float dx, float dy, GpMatrix **matrix)
{
        *matrix = cairo_matrix_create ();

        return gdip_get_status (
                cairo_matrix_set_affine (
                        *matrix, m11, m12, m21, m22, dx, dy));
}

GpStatus
GdipCreateMatrix3 (GpRectF *rect, GpPointF *dstplg, GpMatrix **matrix)
{
        return NotImplemented;
}

GpStatus
GdipCreateMatrix3I (GpRect *rect, GpPoint *dstplg, GpMatrix **matrix)
{
        return NotImplemented;
}

GpStatus
GdipCloneMatrix (GpMatrix *matrix, GpMatrix **cloneMatrix)
{
        return gdip_get_status (
                cairo_matrix_copy (matrix, *cloneMatrix));
}

GpStatus
GdipDeleteMatrix (GpMatrix *matrix)
{
        cairo_matrix_destroy (matrix);
        return Ok;
}

GpStatus
GdipSetMatrixElements (GpMatrix *matrix, float m11, float m12, float m21, float m22, float dx, float dy)
{
        return NotImplemented;
}

GpStatus
GdipMultiplyMatrix (GpMatrix *matrix, GpMatrix *matrix2, GpMatrixOrder order)
{
        return NotImplemented;
}

GpStatus
GdipTranslateMatrix (GpMatrix *matrix, float offsetX, float offsetY, GpMatrixOrder order)
{
        double x = (double) offsetX;
        double y = (double) offsetY;
        
        return gdip_get_status (
                cairo_matrix_transform_distance (matrix, &x, &y));
}

GpStatus
GdipScaleMatrix (GpMatrix *matrix, float scaleX, float scaleY, GpMatrixOrder order)
{
        return gdip_get_status (
                cairo_matrix_scale (matrix, scaleX, scaleY));
        
}

GpStatus
GdipRotateMatrix (GpMatrix *matrix, float angle, GpMatrixOrder order)
{
        return gdip_get_status (
                cairo_matrix_rotate (matrix, angle));
}

GpStatus
GdipShearMatrix (GpMatrix *matrix, float shearX, float shearY, GpMatrixOrder order)
{
        return NotImplemented;
}

GpStatus
GdipInvertMatrix (GpMatrix *matrix)
{
        return gdip_get_status (
                cairo_matrix_invert (matrix));
}

GpStatus
GdipTransformMatrixPoints (GpMatrix *matrix, GpPointF *pts, int count)
{
        return NotImplemented;
}

GpStatus
GdipTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count)
{
        return NotImplemented;
}

GpStatus
GdipVectorTransformMatrixPoints (GpMatrix *matrix, GpPointF *pts, int count)
{
        return NotImplemented;        
}

GpStatus
GdipVectorTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count)
{
        return NotImplemented;        
}

GpStatus 
GdipGetMatrixElements (GpMatrix *matrix, float *matrixOut)
{
        return NotImplemented;
}        

GpStatus 
GdipIsMatrixInvertible (GpMatrix *matrix, int *result)
{
        return NotImplemented;
}

GpStatus
GdipIsMatrixIdentity (GpMatrix *matrix, int *result)
{
        return NotImplemented;
}

GpStatus
GdipIsMatrixEqual (GpMatrix *matrix, GpMatrix *matrix2, int *result)
{
        return NotImplemented;
}
