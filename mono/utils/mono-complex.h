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

#if defined (__cplusplus) && !defined (HOST_ANDROID)
// This is portable ancient C++ but I do not know at the moment
// what Android looks like, so hold back there.
#define MONO_COMPLEX_CPLUSPLUS
#endif

#ifdef MONO_COMPLEX_CPLUSPLUS

#include <complex>

#else // __cplusplus

#if !defined (HAVE_COMPLEX_H) || (defined (ANDROID_UNIFIED_HEADERS) && __ANDROID_API__ < 23)
#include <../../support/libm/complex.h>
#else
#include <complex.h>
#endif

#define _USE_MATH_DEFINES // needed by MSVC to define math constants
#include <math.h>

#endif // MONO_COMPLEX_CPLUSPLUS

MONO_BEGIN_DECLS

#if defined (_MSC_VER) && !defined (__cplusplus)

#define double_complex _C_double_complex

static inline
double_complex mono_double_complex_make(gdouble re, gdouble im)
{
	return _Cbuild (re, im);
}

static inline
double_complex mono_double_complex_scalar_div(double_complex c, gdouble s)
{
	return mono_double_complex_make(creal(c) / s, cimag(c) / s);
}

static inline
double_complex mono_double_complex_scalar_mul(double_complex c, gdouble s)
{
	return mono_double_complex_make(creal(c) * s, cimag(c) * s);
}

static inline
double_complex mono_double_complex_div(double_complex left, double_complex right)
{
	double denom = creal(right) * creal(right) + cimag(right) * cimag(right);

	return mono_double_complex_make(
		(creal(left) * creal(right) + cimag(left) * cimag(right)) / denom,
		(-creal(left) * cimag(right) + cimag(left) * creal(right)) / denom);
}

static inline
double_complex mono_double_complex_sub(double_complex left, double_complex right)
{
	return mono_double_complex_make(creal(left) - creal(right), cimag(left)
		- cimag(right));
}

#else

#ifdef MONO_COMPLEX_CPLUSPLUS

typedef std::complex<double> double_complex;

#else

#define double_complex double complex

#endif

static inline
double_complex mono_double_complex_make(gdouble re, gdouble im)
{
#ifdef MONO_COMPLEX_CPLUSPLUS
	return double_complex (re, im);
#else
	return re + im * I;
#endif
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

MONO_END_DECLS

#endif
