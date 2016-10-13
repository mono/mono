/*
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_ICALL_INTERNALS_H__
#define __MONO_METADATA_ICALL_INTERNALS_H__

#include <config.h>
#include <glib.h>
#include <mono/metadata/object-internals.h>

// On Windows platform implementation of bellow methods are hosted in separate source file
// icall-windows.c or icall-windows-*.c. On other platforms the implementation is still keept
// in icall.c still declared as static and in some places even inlined.
#ifdef HOST_WIN32
void
mono_icall_make_platform_path (gchar *path);

const gchar *
mono_icall_get_file_path_prefix (const gchar *path);

gpointer
mono_icall_module_get_hinstance (MonoReflectionModule *module);

MonoString *
mono_icall_get_machine_name (void);

int
mono_icall_get_platform (void);

MonoString *
mono_icall_get_new_line (void);

MonoBoolean
mono_icall_is_64bit_os (void);

MonoArray *
mono_icall_get_environment_variable_names (void);

void
mono_icall_set_environment_variable (MonoString *name, MonoString *value);

MonoString *
mono_icall_get_windows_folder_path (int folder);

void
mono_icall_broadcast_setting_change (void);

void
mono_icall_write_windows_debug_string (MonoString *message);

MonoBoolean
mono_icall_close_process (gpointer handle);

gint32
mono_icall_wait_for_input_idle (gpointer handle, gint32 milliseconds);
#endif  /* HOST_WIN32 */

// On platforms not using classic WIN API support the  implementation of bellow methods are hosted in separate source file
// icall-windows-*.c. On platforms using classic WIN API the implementation is still keept in icall.c and still declared
// static and in some places even inlined.
#if !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
MonoArray *
mono_icall_get_logical_drives (void);

guint32
mono_icall_drive_info_get_drive_type (MonoString *root_path_name);

MonoBoolean
mono_icall_get_process_working_set_size (gpointer handle, gsize *min, gsize *max);

MonoBoolean
mono_icall_set_process_working_set_size (gpointer handle, gsize min, gsize max);

gint32
mono_icall_get_priority_class (gpointer handle);

MonoBoolean
mono_icall_set_priority_class (gpointer handle, gint32 priorityClass);
#endif  /* !G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT) */

#endif /* __MONO_METADATA_ICALL_INTERNALS_H__ */
