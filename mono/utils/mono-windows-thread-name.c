// mono-windows-thread-name.c
//
// There several ways to set thread name on Windows.
//
// Historically: Raise an exception with an ASCII string.
//  This is visible only in a debugger. Only if a debugger
//  is running when the exception is raised. There is
//  no way to get the thread name and they do not appear in ETW traces.
//
// XboxOne: SetThreadName(thread handle, unicode)
//   This works with or without a debugger and can be retrieved with GetThreadName.
//   Sometimes a macro GetThreadDescription for source compat with next.
//
// Windows 10 1607 or newer (according to documentation, or Creators Update says https://randomascii.wordpress.com/2015/10/26/thread-naming-in-windows-time-for-something-better).
//  SetThreadDescription(thread handle, unicode)
//  This is like XboxOne -- works with or without debugger, can be retrieved
//  with GetThreadDescription, and appears in ETW traces.
//  See https://randomascii.wordpress.com/2015/10/26/thread-naming-in-windows-time-for-something-better.
//
//  This is not called SetThreadName to avoid breaking compilation of C (but not C++)
//  copied from http://msdn.microsoft.com/en-us/library/xcb2z8hs.aspx.
//
// UWP: Also SetThreadDescription, but running prior to 1607 (or Creators Update per above).
//   would require LoadLibrary / GetProcAddress, but LoadLibrary is not allowed.
//
// Author:
//  Jay Krell (jaykrell@microsoft.com)
//
// Copyright 2019 Microsoft
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
#include "config.h"
#include "mono-threads.h"

#if HOST_WIN32

// This is compiler specific because of the use of __try / __except.
#if _MSC_VER
const DWORD MS_VC_EXCEPTION = 0x406D1388;
#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO
{
	DWORD dwType;     // Must be 0x1000.
	PCSTR szName;     // Pointer to name (in user addr space).
	DWORD dwThreadID; // Thread ID (-1=caller thread).
	DWORD dwFlags;    // Reserved for future use, must be zero.
} THREADNAME_INFO;
#pragma pack(pop)
#endif

void
mono_native_thread_set_name (MonoNativeThreadId tid, const char *name)
{
// This is compiler specific because of the use of __try / __except.
#if _MSC_VER
	// http://msdn.microsoft.com/en-us/library/xcb2z8hs.aspx
	THREADNAME_INFO info = {0x1000, name, tid, 0};

	// Checking for IsDebuggerPresent here would be reasonable, for
	// efficiency, and would let this work with other compilers (without
	// structured exception handling support), however
	// there is a race condition then, in that a debugger
	// can be detached (and attached) at any time.
	//
	// A vectored exception handler could also be used to handle this exception.

	__try {
		RaiseException (MS_VC_EXCEPTION, 0, sizeof (info) / sizeof (ULONG_PTR), (ULONG_PTR*)&info);
	} __except (EXCEPTION_EXECUTE_HANDLER) {
	}
#endif
}

#ifdef SetThreadDescription

// If it is a macro, it works.

void
mono_thread_set_name_windows (HANDLE thread_handle, PCWSTR thread_name)
{
	SetThreadDescription (thread_hande, thread_name);
}

#elif HAVE_LOADLIBRARY

typedef
HRESULT
(__stdcall * MonoSetThreadDescription_t) (HANDLE thread_handle, PCWSTR thread_name);

static
HRESULT
__stdcall
mono_thread_set_name_windows_fallback_nop (HANDLE thread_handle, PCWSTR thread_name)
{
	// This function is called on older systems, when LoadLibrary / GetProcAddress fail.
}

static
HRESULT
__stdcall
mono_thread_set_name_windows_init (HANDLE thread_handle, PCWSTR thread_name);

static MonoSetThreadDescription_t set_thread_description = mono_thread_set_name_windows_init;

static
HRESULT
__stdcall
mono_thread_set_name_windows_init (HANDLE thread_handle, PCWSTR thread_name)
{
	// This function is called the first time mono_thread_set_name_windows is called
	// to LoadLibrary / GetProcAddress.
	//
	// Do not write NULL to global, that is racy.
	MonoSetThreadDescription_t local = NULL;
	const HMODULE kernel32 = LoadLibraryExW (L"kernel32.dll", NULL, LOAD_LIBRARY_SEARCH_SYSTEM32);
	if (kernel32)
		local = (MonoSetThreadDescription_t)GetProcAddress (kernel32, "SetThreadDescription");
	if (!local)
		local = mono_thread_set_name_windows_fallback_nop;
	set_thread_description = local;
	local (thread_handle, thread_name);
}

void
mono_thread_set_name_windows (HANDLE thread_handle, PCWSTR thread_name)
{
	set_thread_description (thread_hande, thread_name);
}

#else

void
mono_thread_set_name_windows (HANDLE thread_handle, PCWSTR thread_name)
{
	// When Windows 10 1607 is the minimum supported, just call it directly.
	// If LoadLibrary / GetProcAddress are made available to UWP, use that.
	// SetThreadDescription (thread_hande, thread_name);
}

#endif

#endif // Win32
