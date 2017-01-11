/*
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef _MONO_METADATA_W32FILE_INTERNALS_H_
#define _MONO_METADATA_W32FILE_INTERNALS_H_

#include <config.h>
#include <glib.h>

#if !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)

gboolean
mono_w32file_move (gunichar2 *path, gunichar2 *dest, gint32 *error);

gboolean
mono_w32file_copy (gunichar2 *path, gunichar2 *dest, gboolean overwrite, gint32 *error);

gint64
mono_w32file_get_file_size (gpointer handle, gint32 *error);

gboolean
mono_w32file_lock (gpointer handle, gint64 position, gint64 length, gint32 *error);

gboolean
mono_w32file_replace (gunichar2 *destinationFileName, gunichar2 *sourceFileName,
			   gunichar2 *destinationBackupFileName, guint32 flags, gint32 *error);

gboolean
mono_w32file_unlock (gpointer handle, gint64 position, gint64 length, gint32 *error);

gpointer
mono_w32file_get_console_output (void);

gpointer
mono_w32file_get_console_error (void);

gpointer
mono_w32file_get_console_input (void);

#endif /* !defined(HOST_WIN32) || !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

#endif /* _MONO_METADATA_W32FILE_INTERNALS_H_ */
