/**
 * \file
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_PROCESS_INTERNALS_H__
#define __MONO_METADATA_PROCESS_INTERNALS_H__

#include <config.h>
#include <glib.h>

// On platforms not using classic WIN API support the  implementation of below methods are in separate source file
// process-windows-*.c. On platforms using classic WIN API the implementation is still kept in process.c and still declared
// static and in some places even inlined.
#if !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)

void
mono_w32process_get_fileversion (MonoObjectHandle filever, const gunichar2 *filename, MonoError *error);

#endif  /* !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

#endif /* __MONO_METADATA_PROCESS_INTERNALS_H__ */
