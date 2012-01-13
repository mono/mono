/*
 * sgen-os-posix.c: Simple generational GC.
 *
 * Author:
 *	Paolo Molaro (lupus@ximian.com)
 *	Mark Probst (mprobst@novell.com)
 * 	Geoff Norton (gnorton@novell.com)
 *
 * Copyright 2010 Novell, Inc (http://www.novell.com)
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#include "config.h"
#ifdef HAVE_SGEN_GC
#include <glib.h>
#include "metadata/gc-internal.h"
#include "metadata/sgen-gc.h"
#include "metadata/sgen-archdep.h"
#include "metadata/object-internals.h"

#if defined(__MACH__)
#include "utils/mach-support.h"
#endif

#if defined(PLATFORM_ANDROID)
#include <errno.h>

extern int tkill (pid_t tid, int signal);
#endif

#if !(defined(__MACH__) && defined (MONO_MACH_ARCH_SUPPORTED))

int
mono_sgen_pthread_kill (SgenThreadInfo *info, int signum)
{
#if defined(PLATFORM_ANDROID)
	int  ret;
	int  old_errno = errno;

	ret = tkill ((pid_t) info->android_tid, signum);
	if (ret < 0) {
		ret = errno;
		errno = old_errno;
	}
	return ret;
#else
	return pthread_kill (info->id, signum);
#endif
}

int
mono_sgen_thread_handshake (int signum)
{
	int count, i, result;
	SgenThreadInfo **thread_table;
	SgenThreadInfo *info;
	pthread_t me = pthread_self ();

	thread_table = mono_sgen_get_thread_table ();
	count = 0;
	for (i = 0; i < THREAD_HASH_SIZE; ++i) {
		for (info = thread_table [i]; info; info = info->next) {
			if (ARCH_THREAD_EQUALS (info->id, me)) {
				continue;
			}
			if (info->gc_disabled)
				continue;
			/*if (signum == suspend_signal_num && info->stop_count == global_stop_count)
				continue;*/
			result = mono_sgen_pthread_kill (info, signum);
			if (result == 0) {
				count++;
			} else {
				info->skip = 1;
			}
		}
	}

	mono_sgen_wait_for_suspend_ack (count);

	return count;
}
#endif
#endif
