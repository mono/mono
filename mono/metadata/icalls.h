/**
 * \file
 */

#ifndef __UTILS_MONO_ICALL_H__
#define __UTILS_MONO_ICALL_H__

#include <mono/utils/mono-publib.h>

#ifdef ENABLE_ICALL_EXPORT
#pragma GCC diagnostic ignored "-Wmissing-prototypes"
#define ICALL_DECL_EXPORT MONO_API
#define ICALL_EXPORT MONO_API
#else
#define ICALL_DECL_EXPORT /* nothing */
/* Can't be static as icall.c defines icalls referenced by icall-tables.c */
#define ICALL_EXPORT MONO_EXTERN_C
#endif

#endif // __UTILS_MONO_ICALL_H__
