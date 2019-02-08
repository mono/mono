/**
* \file
* Win32 OS wait wrappers and interrupt/abort APC handling.
*
* Author:
*   Johan Lorensson (lateralusx.github@gmail.com)
*
* Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#include <mono/utils/mono-os-wait.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-threads-debug.h>
#include "mono-logger-internals.h"

DWORD
mono_win32_sleep_ex (DWORD timeout, BOOL alertable)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = SleepEx (timeout, alertable);

	if (info)
		mono_win32_leave_alertable_wait (info);

	return result;
}

DWORD
mono_win32_wait_for_single_object_ex (HANDLE handle, DWORD timeout, BOOL alertable)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = WaitForSingleObjectEx (handle, timeout, alertable);

	if (info)
		mono_win32_leave_alertable_wait (info);

	return result;
}

DWORD
mono_win32_wait_for_multiple_objects_ex (DWORD count, CONST HANDLE *handles, BOOL waitAll, DWORD timeout, BOOL alertable)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = WaitForMultipleObjectsEx (count, handles, waitAll, timeout, alertable);

	if (info)
		mono_win32_leave_alertable_wait (info);

	// This is not perfect, but it is the best you can do in usermode and matches CoreCLR.
	// i.e. handle-based instead of object-based.

	if (result == WAIT_FAILED && waitAll &&
			count > 1 && count <= MAXIMUM_WAIT_OBJECTS
			&& GetLastError () == ERROR_INVALID_PARAMETER) {
		gpointer handles_sorted [MAXIMUM_WAIT_OBJECTS]; // 64
		memcpy (handles_sorted, handles, count * sizeof (handles [0]));
		qsort (handles_sorted, count, sizeof (handles_sorted [0]), g_direct_equal);
		for (DWORD i = 1; i < count; ++i) {
			if (handles_sorted [i - 1] == handles_sorted [i]) {
				mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER_HANDLE, "%s: handle %p is duplicated", __func__, handles_sorted [i]);
				// Preserve LastError, but reduce triggering write breakpoints.
				if (GetLastError () != ERROR_INVALID_PARAMETER)
					SetLastError (ERROR_INVALID_PARAMETER);
				break;
			}
		}
	}

	return result;
}

/* See comment above ves_icall_System_Threading_WaitHandle_SignalAndWait_Internal */
#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)

DWORD
mono_win32_signal_object_and_wait (HANDLE toSignal, HANDLE toWait, DWORD timeout, BOOL alertable)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = SignalObjectAndWait (toSignal, toWait, timeout, alertable);

	if (info)
		mono_win32_leave_alertable_wait (info);

	return result;
}

#endif

#if G_HAVE_API_SUPPORT(HAVE_CLASSIC_WINAPI_SUPPORT)
DWORD
mono_win32_msg_wait_for_multiple_objects_ex (DWORD count, CONST HANDLE *handles, DWORD timeout, DWORD wakeMask, DWORD flags)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();
	BOOL alertable = flags & MWMO_ALERTABLE;

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = MsgWaitForMultipleObjectsEx (count, handles, timeout, wakeMask, flags);

	if (info)
		mono_win32_leave_alertable_wait (info);

	return result;
}
#endif

DWORD
mono_win32_wsa_wait_for_multiple_events (DWORD count, const WSAEVENT FAR *handles, BOOL waitAll, DWORD timeout, BOOL alertable)
{
	DWORD result = WAIT_FAILED;
	MonoThreadInfo *info = mono_thread_info_current_unchecked ();

	if (info)
		mono_win32_enter_alertable_wait (info);

	result = WSAWaitForMultipleEvents (count, handles, waitAll, timeout, alertable);

	if (info)
		mono_win32_leave_alertable_wait (info);

	return result;
}
