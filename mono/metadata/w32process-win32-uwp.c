/**
 * \file
 * UWP process support for Mono.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>
#include "mono/utils/mono-compiler.h"

#if G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT)
#include <windows.h>
#include <mono/metadata/object-internals.h>
#include "mono/metadata/w32process.h"
#include "mono/metadata/w32process-internals.h"
#include "mono/metadata/w32process-win32-internals.h"
#include "icall-decl.h"

void
mono_w32process_get_fileversion (MonoObjectHandle filever, MonoStringHandle str, const gunichar2 *filename, MonoError *error)
{
	g_unsupported_api ("GetFileVersionInfoSize, GetFileVersionInfo, VerQueryValue, VerLanguageName");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "GetFileVersionInfoSize, GetFileVersionInfo, VerQueryValue, VerLanguageName");
	SetLastError (ERROR_NOT_SUPPORTED);
}

MonoObjectHandle
process_add_module (MonoObjectHandle item, MonoObjectHandle filever, MonoStringHandle str,
	HANDLE process, HMODULE mod, const gunichar2 *filename, const gunichar2 *modulename,
	MonoClass *proc_class, MonoError *error)
{
	g_unsupported_api ("GetModuleInformation");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "GetModuleInformation");
	SetLastError (ERROR_NOT_SUPPORTED);
	return NULL_HANDLE;
}

MonoArrayHandle
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObjectHandle this_obj, HANDLE process, MonoError *error)
{
	g_unsupported_api ("EnumProcessModules, GetModuleBaseName, GetModuleFileNameEx");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "EnumProcessModules, GetModuleBaseName, GetModuleFileNameEx");
	SetLastError (ERROR_NOT_SUPPORTED);
	return NULL_HANDLE_ARRAY;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (MonoW32ProcessStartInfo *proc_start_info, MonoW32ProcessInfo *process_info, MonoError *error)
{
	g_unsupported_api ("ShellExecuteEx");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "ShellExecuteEx");
	process_info->pid = (guint32)(-ERROR_NOT_SUPPORTED);
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

MonoStringHandle
ves_icall_System_Diagnostics_Process_ProcessName_internal (HANDLE process, MonoError *error)
{
	gunichar2 name [MAX_PATH]; // FIXME MAX_PATH
	guint32 len = GetModuleFileNameW (NULL, name, G_N_ELEMENTS (name));
	if (len == 0)
		return NULL_HANDLE_STRING;
	return mono_string_new_utf16_handle (mono_domain_get (), name, len, error);
}

void
mono_process_init_startup_info (HANDLE stdin_handle, HANDLE stdout_handle, HANDLE stderr_handle, STARTUPINFO *startinfo)
{
	startinfo->cb = sizeof(STARTUPINFO);
	startinfo->dwFlags = 0;
	startinfo->hStdInput = INVALID_HANDLE_VALUE;
	startinfo->hStdOutput = INVALID_HANDLE_VALUE;
	startinfo->hStdError = INVALID_HANDLE_VALUE;
}

gboolean
mono_process_create_process (MonoW32ProcessInfo *mono_process_info, MonoString *cmd, guint32 creation_flags,
	gunichar2 *env_vars, gunichar2 *dir, STARTUPINFO *start_info, PROCESS_INFORMATION *process_info,
	MonoError *error)
{
	char *api_name = mono_process_info->username ? "CreateProcessWithLogonW" : "CreateProcess";
	memset (process_info, 0, sizeof (PROCESS_INFORMATION));
	g_unsupported_api (api_name);
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, api_name);
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

#else /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */

MONO_EMPTY_SOURCE_FILE (process_windows_uwp);
#endif /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */
