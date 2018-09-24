// This file bridges C++ to C runtime math functions
// without any chance of a C++ library dependency.
// In time it can probably be removed.
#include "config.h"
#include "glib.h"
#define MONO_MATH_DECLARE_ALL 1
#include "mono-math.h"

#if defined (__cplusplus) && !defined (_MSC_VER)
#error This file should be compiled as C.
#endif

#ifdef HAVE_ISFINITE
gboolean mono_isfinite_float (float a)			{ return isfinite (a); }
gboolean mono_isfinite_double (double a)		{ return isfinite (a); }
#endif
#ifdef HAVE_ISINF
gboolean mono_isinf_float (float a)			{ return isinf (a); }
gboolean mono_isinf_double (double a)			{ return isinf (a); }
#endif
gboolean mono_isnan_float (float a)			{ return isnan (a); }
gboolean mono_isnan_double (double a)			{ return isnan (a); }
gboolean mono_isnormal_float (float a)			{ return isnormal (a); }
gboolean mono_isnormal_double (double a)		{ return isnormal (a); }
gboolean mono_isunordered_float (float a, float b)	{ return isunordered (a, b); }
gboolean mono_isunordered_double (double a, double b)	{ return isunordered (a, b); }
gboolean mono_trunc_float (float a)			{ return trunc (a); }
gboolean mono_trunc_double (double a)			{ return trunc (a); }
