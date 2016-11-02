/*
 * icall-windows-uwp.c: UWP icall support for Mono.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>

#if G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT)
#include <Windows.h>
#include "mono/metadata/icall-windows-internals.h"

MonoString *
mono_icall_get_machine_name (void)
{
	g_unsupported_api ("GetComputerName");
	return mono_string_new (mono_domain_get (), "mono");
}

MonoString *
mono_icall_get_windows_folder_path (int folder)
{
	g_unsupported_api ("SHGetFolderPath");
	return mono_string_new (mono_domain_get (), "");
}

MonoArray *
mono_icall_get_logical_drives (void)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetLogicalDriveStrings");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetLogicalDriveStrings");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return NULL;
}

void
mono_icall_broadcast_setting_change (void)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("SendMessageTimeout");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "SendMessageTimeout");
	mono_error_set_pending_exception (&mono_error);

	SetLastError (ERROR_NOT_SUPPORTED);

	return;
}

guint32
mono_icall_drive_info_get_drive_type (MonoString *root_path_name)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("GetDriveType");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "GetDriveType");
	mono_error_set_pending_exception (&mono_error);

	return DRIVE_UNKNOWN;
}

gint32
mono_icall_wait_for_input_idle (gpointer handle, gint32 milliseconds)
{
	MonoError mono_error;
	mono_error_init (&mono_error);

	g_unsupported_api ("WaitForInputIdle");

	mono_error_set_not_supported (&mono_error, G_UNSUPPORTED_API, "WaitForInputIdle");
	mono_error_set_pending_exception (&mono_error);

	return WAIT_TIMEOUT;
}

#else /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
void __mono_win32_icall_windows_uwp_quiet_lnk4221(void) {}
#endif
#endif /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */
