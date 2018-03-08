/* gdebug.c
 *
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#include <config.h>
#include <glib.h>

#if defined (_WIN32)

#include <windows.h>

gboolean
g_is_usermode_native_debugger_present (void)
{
	// This is just a few instructions and no syscall. It is very fast.
	// Kernel debugger is detected otherwise and is also useful for usermode debugging.
	// Mono managed debugger is detected otherwise.
	return IsDebuggerPresent () != FALSE;
}

#elif defined (__APPLE__)

#include <errno.h>
#include <unistd.h>
#include <sys/sysctl.h>

static gboolean
g_is_usermode_native_debugger_present_slow (void)
// https://developer.apple.com/library/content/qa/qa1361/_index.html
// This is a syscall so it is very slow.
{
	int mib[4] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, getpid () };
	struct kinfo_proc info;

	memset (&info, 0, sizeof (info));
	size_t size = sizeof (info);

	sysctl (mib, sizeof (mib) / sizeof (*mib), &info, &size, NULL, 0);

	// We're being debugged if the P_TRACED flag is set.
	return (info.kp_proc.p_flag & P_TRACED) != 0;
}

static gchar g_is_usermode_native_debugger_present_cache; // 0:uninitialized 1:true 2:false

gboolean
g_is_usermode_native_debugger_present (void)
{
	if (g_is_usermode_native_debugger_present_cache == 0) {
		int er = errno;
		g_is_usermode_native_debugger_present_cache = g_is_usermode_native_debugger_present_slow () ? 1 : 2;
		er = errno;
	}
	return g_is_usermode_native_debugger_present_cache == 1;
}

#else

// FIXME Other operating systems.

gboolean
g_is_usermode_native_debugger_present (void)
{
	return FALSE;
}

#endif

#ifdef GDEBUG_TEST

int
#ifdef _MSC_VER
__cdecl
#endif
main ()
{
	printf ("g_is_usermode_native_debugger_present:%d\n", g_is_usermode_native_debugger_present ());
}

#endif
