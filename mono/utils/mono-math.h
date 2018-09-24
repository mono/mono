/**
 * \file
 */

#ifndef __MONO_SIGNBIT_H__
#define __MONO_SIGNBIT_H__

#include <math.h>
#include <mono/utils/mono-publib.h>

#ifdef HAVE_SIGNBIT
#define mono_signbit signbit
#else
#define mono_signbit(x) (sizeof (x) == sizeof (float) ? mono_signbit_float (x) : mono_signbit_double (x))

MONO_API int
mono_signbit_double (double x);

MONO_API int
mono_signbit_float (float x);

#endif

// Instead of isfinite, isinf, isnan, etc.,
// use mono_isfininite, mono_isinf, mono_isnan, etc.
// These functions are implemented in C in order to avoid
// a C++ runtime dependency and for more portable binding
// from C++, esp. across Android versions/architectures.
// WebAssembly, and Win32/gcc.
// SEe https://github.com/mono/mono/pull/10701.

#if defined (__cplusplus) || defined (MONO_MATH_DECLARE_ALL)

#ifdef HAVE_ISFINITE
G_EXTERN_C gboolean mono_isfinite_float (float);
G_EXTERN_C gboolean mono_isfinite_double (double);
#endif
#ifdef HAVE_ISINF
G_EXTERN_C gboolean mono_isinf_float (float);
G_EXTERN_C gboolean mono_isinf_double (double);
#endif
G_EXTERN_C gboolean mono_isnan_float (float);
G_EXTERN_C gboolean mono_isnan_double (double);
G_EXTERN_C gboolean mono_isnormal_float (float);
G_EXTERN_C gboolean mono_isnormal_double (double);
G_EXTERN_C gboolean mono_isunordered_float (float, float);
G_EXTERN_C gboolean mono_isunordered_double (double, double);
G_EXTERN_C gboolean mono_trunc_float (float);
G_EXTERN_C gboolean mono_trunc_double (double);

#endif

#ifdef __cplusplus

#ifdef HAVE_ISFINITE
inline gboolean mono_isfinite (float a)                 { return mono_isfinite_float (a); }
inline gboolean mono_isfinite (double a)                { return mono_isfinite_double (a); }
#endif
#ifdef HAVE_ISINF
inline gboolean mono_isinf (float a)                    { return mono_isinf_float (a); }
inline gboolean mono_isinf (double a)                   { return mono_isinf_double (a); }
#endif
inline gboolean mono_isnan (float a)                    { return mono_isnan_float (a); }
inline gboolean mono_isnan (double a)                   { return mono_isnan_double (a); }
inline gboolean mono_isnormal (float a)                 { return mono_isnormal_float (a); }
inline gboolean mono_isnormal (double a)                { return mono_isnormal_double (a); }
inline gboolean mono_isunordered (float a, float b)     { return mono_isunordered_float (a, b); }
inline gboolean mono_isunordered (double a, double b)   { return mono_isunordered_double (a, b); }
inline gboolean mono_trunc (float a)                    { return mono_trunc_float (a); }
inline gboolean mono_trunc (double a)                   { return mono_trunc_double (a); }

#else

// Direct macros for C.
#ifdef HAVE_ISFINITE
#define mono_isfinite(x)        (isfinite ((x)))
#endif
#ifdef HAVE_ISINF
#define mono_isinf(x)           (isinf ((x)))
#endif
#define mono_isnan(x)           (isnan ((x)))
#define mono_isnormal(x)        (isnormal ((x)))
#define mono_isunordered(x, y)  (isunordered ((x), (y)))
#define mono_trunc(x)           (trunc ((x)))

#endif

#endif
