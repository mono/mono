/**
 * \file
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_PROCESS_INTERNALS_H__
#define __MONO_METADATA_PROCESS_INTERNALS_H__

#include <config.h>
#include <glib.h>

// On platforms not using classic WIN API support the  implementation of bellow methods are hosted in separate source file
// process-windows-*.c. On platforms using classic WIN API the implementation is still keept in process.c and still declared
// static and in some places even inlined.
#if !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
void
mono_w32process_get_fileversion (MonoObjectHandle filever, const gunichar2 *filename, MonoError *error);

void
mono_process_init_startup_info (HANDLE stdin_handle, HANDLE stdout_handle,
				HANDLE stderr_handle,STARTUPINFO *startinfo);

gboolean
mono_process_create_process (MonoW32ProcessInfo *mono_process_info, MonoString *cmd, guint32 creation_flags,
	gunichar2 *env_vars, gunichar2 *dir, STARTUPINFO *start_info, PROCESS_INFORMATION *process_info);

#endif  /* !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

#endif /* __MONO_METADATA_PROCESS_INTERNALS_H__ */
