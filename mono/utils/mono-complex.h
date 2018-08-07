/**
 * \file
* C99 Complex math cross-platform support code
*
* Author:
*	Joao Matos (joao.matos@xamarin.com)
*
* Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
* Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#include <config.h>
#include <glib.h>
#include <math.h>

#if 1 // previously _MSC_VER

typedef struct double_complex {
	double real;
	double imag;
} double_complex;

static inline
double_complex mono_double_complex_make(gdouble re, gdouble im)
{
	double_complex const a = { re, im };
	return a;
}

static inline
double_complex mono_double_complex_scalar_div(double_complex c, gdouble s)
{
	return mono_double_complex_make(mono_creal(c) / s, mono_cimag(c) / s);
}

static inline
double_complex mono_double_complex_scalar_mul(double_complex c, gdouble s)
{
	return mono_double_complex_make(mono_creal(c) * s, mono_cimag(c) * s);
}

static inline
double_complex mono_double_complex_div(double_complex left, double_complex right)
{
	double denom = creal(right) * creal(right) + cimag(right) * cimag(right);

	return mono_double_complex_make(
		(mono_creal(left) * mono_creal(right) + mono_cimag(left) * mono_cimag(right)) / denom,
		(-mono_creal(left) * mono_cimag(right) + mono_cimag(left) * mono_creal(right)) / denom);
}

static inline
double_complex mono_double_complex_sub(double_complex left, double_complex right)
{
	return mono_double_complex_make(mono_creal(left) - mono_creal(right), mono_cimag(left)
		- mono_cimag(right));
}

#else // dead code

#define double_complex double complex

static inline
double_complex mono_double_complex_make(gdouble re, gdouble im)
{
	return re + im * I;
}

static inline
double_complex mono_double_complex_scalar_div(double_complex c, gdouble s)
{
	return c / s;
}

static inline
double_complex mono_double_complex_scalar_mul(double_complex c, gdouble s)
{
	return c * s;
}

static inline
double_complex mono_double_complex_div(double_complex left, double_complex right)
{
	return left / right;
}

static inline
double_complex mono_double_complex_sub(double_complex left, double_complex right)
{
	return left - right;
}

#endif
