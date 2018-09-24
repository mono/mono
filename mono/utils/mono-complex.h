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
#define _USE_MATH_DEFINES // needed by MSVC to define math constants
#include <math.h>

typedef struct MonoComplex {
	double real;
	double imag;
} MonoComplex;

#define mono_creal(c) ((c).real)
#define mono_cimag(c) ((c).imag)

static inline
MonoComplex mono_complex_make (double re, double im)
{
	MonoComplex const a = { re, im };
	return a;
}

static inline
MonoComplex mono_complex_scalar_div (MonoComplex c, double s)
{
	return mono_complex_make (mono_creal (c) / s, mono_cimag (c) / s);
}

static inline
MonoComplex mono_complex_scalar_mul (MonoComplex c, double s)
{
	return mono_complex_make (mono_creal (c) * s, mono_cimag (c) * s);
}

static inline
MonoComplex mono_complex_div (MonoComplex left, MonoComplex right)
{
	double denom = mono_creal (right) * mono_creal (right) + mono_cimag (right) * mono_cimag (right);

	return mono_complex_make(
		(mono_creal (left) * mono_creal (right) + mono_cimag (left) * mono_cimag (right)) / denom,
		(-mono_creal (left) * mono_cimag (right) + mono_cimag (left) * mono_creal (right)) / denom);
}

static inline
MonoComplex mono_complex_sub(MonoComplex left, MonoComplex right)
{
	return mono_complex_make(creal(left) - mono_creal(right), mono_cimag(left)
		- mono_cimag(right));
}

#include "../../support/libm/complex.c"
