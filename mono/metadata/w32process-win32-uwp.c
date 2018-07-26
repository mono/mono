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

gboolean
mono_process_win_enum_processes (DWORD *pids, DWORD count, DWORD *needed, MonoError *error)
{
	// FIXME UWP does support EnumProcesses.
	g_unsupported_api ("EnumProcesses");
	*needed = 0;
	mono_error_set_not_supported (error, "This system does not support EnumProcesses");
	//mono_error_set_not_supported (error, G_UNSUPPORTED_API, "EnumProcesses");
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

HANDLE
ves_icall_System_Diagnostics_Process_GetProcess_internal (guint32 pid, MonoError *error)
{
	// FIXME UWP does support OpenProcess.
	g_unsupported_api ("OpenProcess");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "OpenProcess");
	SetLastError (ERROR_NOT_SUPPORTED);
	return NULL;
}

void
mono_w32process_get_fileversion (MonoObject *filever, gunichar2 *filename, MonoError *error)
{
	g_unsupported_api ("GetFileVersionInfoSize, GetFileVersionInfo, VerQueryValue, VerLanguageName");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "GetFileVersionInfoSize, GetFileVersionInfo, VerQueryValue, VerLanguageName");
	SetLastError (ERROR_NOT_SUPPORTED);
}

MonoObjectHandle
process_add_module (HANDLE process, HMODULE mod, const gunichar2 *filename, const gunichar2 *modulename, MonoClass *proc_class,
	MonoArrayHandle array, gsize index, MonoError *error)
{
	// FIXME UWP does support GetModuleInformation.
	g_unsupported_api ("GetModuleInformation");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "GetModuleInformation");
	SetLastError (ERROR_NOT_SUPPORTED);
	return NULL_HANDLE;
}

MonoArrayHandle
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObjectHandle this_obj, gpointer process, MonoError *error)
{
	// UWP supports GetModuleBaseName, GetModuleFileNameEx but not EnumProcessModules.
	g_unsupported_api ("EnumProcessModules, GetModuleBaseName, GetModuleFileNameEx");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "EnumProcessModules, GetModuleBaseName, GetModuleFileNameEx");
	SetLastError (ERROR_NOT_SUPPORTED);
	return NULL_HANDLE_ARRAY;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (MonoW32ProcessStartInfoHandle proc_start_info, MonoW32ProcessInfo *process_info, MonoError *error)
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
	// FIXME Check process == GetCurrentProcess || GetProcessId() == GetCurrenrtProcessId.
	// FIXME UWP does support psapi.

	gunichar2 name[MAX_PATH]; // FIXME
	guint32 len = GetModuleFileName (NULL, name, G_N_ELEMENTS (name));
	if (len == 0)
		return NULL_HANDLE_STRING;

	return mono_string_new_utf16_handle (mono_domain_get (), name, len, error);
}

void
mono_process_init_startup_info (HANDLE stdin_handle, HANDLE stdout_handle, HANDLE stderr_handle, STARTUPINFO *startinfo)
{
	memset (startupinfo, 0, sizeof (*startupinfo));
	startinfo->cb = sizeof(STARTUPINFO);
	startinfo->dwFlags = 0;
	startinfo->hStdInput = INVALID_HANDLE_VALUE;
	startinfo->hStdOutput = INVALID_HANDLE_VALUE;
	startinfo->hStdError = INVALID_HANDLE_VALUE;
}

gboolean
mono_process_create_process (MonoCreateProcessCoop *coop, MonoW32ProcessInfo *mono_process_info,
	MonoStringHandle cmd, guint32 creation_flags, gunichar2 *env_vars, gunichar2 *dir, STARTUPINFO *start_info,
	PROCESS_INFORMATION *process_info, MonoError *error)
{
	// FIXME UWP supports CreateProcess but not CreateProcessWithLogon.
	const char * const api_name = mono_process_info->username ? "CreateProcessWithLogonW" : "CreateProcess";
	memset (&process_info, 0, sizeof (PROCESS_INFORMATION));
	g_unsupported_api (api_name);
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, api_name);
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

MonoBoolean
mono_icall_get_process_working_set_size (gpointer handle, gsize *min, gsize *max, MonoError *error)
{
	g_unsupported_api ("GetProcessWorkingSetSize");
	mono_error_set_not_supported(error, G_UNSUPPORTED_API, "GetProcessWorkingSetSize");
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

MonoBoolean
mono_icall_set_process_working_set_size (gpointer handle, gsize min, gsize max, MonoError *error)
{
	g_unsupported_api ("SetProcessWorkingSetSize");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "SetProcessWorkingSetSize");
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

gint32
mono_icall_get_priority_class (gpointer handle, MonoError *error)
{
	// FIXME UWP does support GetPriorityClass.
	g_unsupported_api ("GetPriorityClass");
	mono_error_set_not_supported (error, G_UNSUPPORTED_API, "GetPriorityClass");
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

MonoBoolean
mono_icall_set_priority_class (gpointer handle, gint32 priorityClass, MonoError *error)
{
	// FIXME UWP does support SetPriorityClass.
	g_unsupported_api ("SetPriorityClass");
	mono_error_set_not_supported(error, G_UNSUPPORTED_API, "SetPriorityClass");
	SetLastError (ERROR_NOT_SUPPORTED);
	return FALSE;
}

#else /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */

MONO_EMPTY_SOURCE_FILE (process_windows_uwp);
#endif /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */
