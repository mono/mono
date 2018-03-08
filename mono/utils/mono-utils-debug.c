/* mono-utils-debug.c
 *
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#include <config.h>
#include <glib.h>
#include "mono-utils-debug.h"

#if defined (_WIN32)

#include <windows.h>

gboolean
mono_is_usermode_native_debugger_present (void)
{
	// This is just a few instructions and no syscall. It is very fast.
	// Kernel debugger is detected otherwise and is also useful for usermode debugging.
	// Mono managed debugger is detected otherwise.
	return IsDebuggerPresent () != FALSE;
}

#elif defined (__APPLE__)

#include <unistd.h>
#include <errno.h>
#include <sys/sysctl.h>

static gboolean
mono_is_usermode_native_debugger_present_slow (void)
// https://developer.apple.com/library/content/qa/qa1361/_index.html
// This is a syscall so it is very slow.
{
	int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, getpid () };
	struct kinfo_proc info;

	memset (&info, 0, sizeof (info));
	size_t size = sizeof (info);

	sysctl (mib, sizeof (mib) / sizeof (*mib), &info, &size, NULL, 0);

	// Return the traced flag.
	return (info.kp_proc.p_flag & P_TRACED) != 0;
}

static gchar mono_is_usermode_native_debugger_present_cache; // 0:uninitialized 1:true 2:false

gboolean
mono_is_usermode_native_debugger_present (void)
{
	if (mono_is_usermode_native_debugger_present_cache == 0) {
		int er = errno;
		mono_is_usermode_native_debugger_present_cache = mono_is_usermode_native_debugger_present_slow () ? 1 : 2;
		errno = er;
	}
	return mono_is_usermode_native_debugger_present_cache == 1;
}

#else

// FIXME Other operating systems.

gboolean
mono_is_usermode_native_debugger_present (void)
{
	return FALSE;
}

#endif

#if 0 // test

int
#ifdef _MSC_VER
__cdecl
#endif
main ()
{
	printf ("mono_usermode_native_debugger_present:%d\n", mono_is_usermode_native_debugger_present ());
}

#endif
