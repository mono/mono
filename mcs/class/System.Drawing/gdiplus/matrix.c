/**
 * matrix.c
 *
 * Author: Duncan Mak (duncan@ximian.com)
 * 
 * Copyright (C) Novell, Inc. 2003.
 *
 **/

#include <math.h>
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
                cairo_matrix_copy (*cloneMatrix, matrix));
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
        return gdip_get_status (
                cairo_matrix_set_affine (matrix, m11, m12, m21, m22, dx, dy));
}

GpStatus 
GdipGetMatrixElements (GpMatrix *matrix, float *matrixOut)
{
        double a, b, c, d, tx, ty;
        matrixOut = malloc (6 * sizeof (float));

        cairo_matrix_get_affine (matrix, &a, &b, &c, &d, &tx, &ty);
        
        matrixOut [0] = (float) a;
        matrixOut [1] = (float) b;
        matrixOut [2] = (float) c;
        matrixOut [3] = (float) d;
        matrixOut [4] = (float) tx;
        matrixOut [5] = (float) ty;

        return Ok;
}

GpStatus
GdipMultiplyMatrix (GpMatrix *matrix, GpMatrix *matrix2, GpMatrixOrder order)
{
        GpMatrix *result = NULL;
        cairo_status_t status;

        if (order == MatrixOrderPrepend)
                status = cairo_matrix_multiply (result, matrix, matrix2);

        if (order == MatrixOrderAppend)
                status = cairo_matrix_multiply (result, matrix2, matrix);

        else
                return GenericError;

        matrix = result;
        return gdip_get_status (status);
}

static GpMatrix *
set_translate (float offsetX, float offsetY)
{
        GpMatrix *matrix = cairo_matrix_create ();
        cairo_matrix_set_affine (matrix, 1, 0, 0, 1, offsetX, offsetY);
        return matrix;
}

GpStatus
GdipTranslateMatrix (GpMatrix *matrix, float offsetX, float offsetY, GpMatrixOrder order)
{
        GpMatrix *tmp = set_translate (offsetX, offsetY);
        GpStatus s = GdipMultiplyMatrix (matrix, tmp, order);
        GdipDeleteMatrix (tmp);

        return s;
}

static GpMatrix *
set_scale (float scaleX, float scaleY)
{
        GpMatrix *matrix = cairo_matrix_create ();
        cairo_matrix_set_affine (matrix, scaleX, 0, 0, scaleY, 0, 0);
        return matrix;
}

GpStatus
GdipScaleMatrix (GpMatrix *matrix, float scaleX, float scaleY, GpMatrixOrder order)
{
        GpMatrix *tmp = set_scale (scaleX, scaleY);
        GpStatus s = GdipMultiplyMatrix (matrix, tmp, order);
        GdipDeleteMatrix (tmp);

        return s;
}

static GpMatrix *
set_rotate (float angle)
{
        float rad = angle * DEGTORAD;
        GpMatrix *matrix = cairo_matrix_create ();
        cairo_matrix_set_affine (matrix, cos (rad), sin (rad), -sin (rad), cos (rad), 0, 0);

        return matrix;
}

GpStatus
GdipRotateMatrix (GpMatrix *matrix, float angle, GpMatrixOrder order)
{
        GpMatrix *tmp = set_rotate (angle);
        GpStatus s = GdipMultiplyMatrix (matrix, tmp, order);
        GdipDeleteMatrix (tmp);

        return s;
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
        int i;
        cairo_status_t status;
        
        for (i = 0; i < count; i++, pts++) {
                double x = pts->X;
                double y = pts->Y;
                status = cairo_matrix_transform_point (matrix, &x, &y);
                if (status != CAIRO_STATUS_SUCCESS)
                        return gdip_get_status (status);
                pts->X = (float) x;
                pts->Y = (float) y;
        }

        return Ok;
}

GpStatus
GdipTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count)
{
        int i;
        cairo_status_t status;
        
        for (i = 0; i < count; i++, pts++) {
                double x = pts->X;
                double y = pts->Y;
                status = cairo_matrix_transform_point (matrix, &x, &y);
                if (status != CAIRO_STATUS_SUCCESS)
                        return gdip_get_status (status);
                pts->X = (int) x;
                pts->Y = (int) y;
        }

        return Ok;
}

GpStatus
GdipVectorTransformMatrixPoints (GpMatrix *matrix, GpPointF *pts, int count)
{
        int i;
        cairo_status_t status;
        
        for (i = 0; i < count; i++, pts++) {
                double x = pts->X;
                double y = pts->Y;
                status = cairo_matrix_transform_distance (matrix, &x, &y);
                if (status != CAIRO_STATUS_SUCCESS)
                        return gdip_get_status (status);
                pts->X = (float) x;
                pts->Y = (float) y;
        }

        return Ok;
}

GpStatus
GdipVectorTransformMatrixPointsI (GpMatrix *matrix, GpPoint *pts, int count)
{
        int i;
        cairo_status_t status;
        
        for (i = 0; i < count; i++, pts++) {
                double x = pts->X;
                double y = pts->Y;
                status = cairo_matrix_transform_distance (matrix, &x, &y);
                if (status != CAIRO_STATUS_SUCCESS)
                        return gdip_get_status (status);

                pts->X = (int) x;
                pts->Y = (int) y;
        }

        return Ok;
}

GpStatus 
GdipIsMatrixInvertible (GpMatrix *matrix, int *result)
{
        cairo_status_t status = cairo_matrix_invert (matrix);

        if (status == CAIRO_STATUS_INVALID_MATRIX)
                *result = 1;

        *result = 0;
        return Ok;
}

GpStatus
GdipIsMatrixIdentity (GpMatrix *matrix, int *result)
{
        cairo_matrix_t *identity = cairo_matrix_create ();

/*         if (cairo_matrix_equals (matrix, identity)) */
/*                 *result = 1; */

        *result = 0;

        cairo_matrix_destroy (identity);
        return Ok;
}

GpStatus
GdipIsMatrixEqual (GpMatrix *matrix, GpMatrix *matrix2, int *result)
{
/*         *result = cairo_matrix_equals (matrix, matrix2); */

        return Ok;
}
