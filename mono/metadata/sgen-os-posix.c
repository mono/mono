/*
 * sgen-os-posix.c: Posix support.
 *
 * Author:
 *	Paolo Molaro (lupus@ximian.com)
 *	Mark Probst (mprobst@novell.com)
 * 	Geoff Norton (gnorton@novell.com)
 *
 * Copyright 2010 Novell, Inc (http://www.novell.com)
 * Copyright (C) 2012 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

#include "config.h"

#ifdef HAVE_SGEN_GC
#if !defined(__MACH__) && !MONO_MACH_ARCH_SUPPORTED && defined(HAVE_PTHREAD_KILL)

#include <errno.h>
#include <glib.h>
#include "metadata/sgen-gc.h"
#include "metadata/gc-internal.h"
#include "metadata/sgen-archdep.h"
#include "metadata/object-internals.h"
#include "utils/mono-signal-handler.h"

#if defined(__APPLE__) || defined(__OpenBSD__) || defined(__FreeBSD__)
const static int suspend_signal_num = SIGXFSZ;
#else
const static int suspend_signal_num = SIGPWR;
#endif
const static int restart_signal_num = SIGXCPU;

static MonoSemType suspend_ack_semaphore;
static MonoSemType *suspend_ack_semaphore_ptr;

static sigset_t suspend_signal_mask;
static sigset_t suspend_ack_signal_mask;

static void
suspend_thread (SgenThreadInfo *info, void *context)
{
	int stop_count;
#ifndef USE_MONO_CTX
	gpointer regs [ARCH_NUM_REGS];
#endif
	gpointer stack_start;

	info->stopped_domain = mono_domain_get ();
	info->stopped_ip = context ? (gpointer) ARCH_SIGCTX_IP (context) : NULL;
	info->signal = 0;
	stop_count = sgen_global_stop_count;
	/* duplicate signal */
	if (0 && info->stop_count == stop_count)
		return;

	stack_start = context ? (char*) ARCH_SIGCTX_SP (context) - REDZONE_SIZE : NULL;
	/* If stack_start is not within the limits, then don't set it
	   in info and we will be restarted. */
	if (stack_start >= info->stack_start_limit && info->stack_start <= info->stack_end) {
		info->stack_start = stack_start;

#ifdef USE_MONO_CTX
		if (context) {
			mono_sigctx_to_monoctx (context, &info->ctx);
		} else {
			memset (&info->ctx, 0, sizeof (MonoContext));
		}
#else
		if (context) {
			ARCH_COPY_SIGCTX_REGS (regs, context);
			memcpy (&info->regs, regs, sizeof (info->regs));
		} else {
			memset (&info->regs, 0, sizeof (info->regs));
		}
#endif
	} else {
		g_assert (!info->stack_start);
	}

	/* Notify the JIT */
	if (mono_gc_get_gc_callbacks ()->thread_suspend_func)
		mono_gc_get_gc_callbacks ()->thread_suspend_func (info->runtime_data, context, NULL);

	SGEN_LOG (4, "Posting suspend_ack_semaphore for suspend from %p %p", info, (gpointer)mono_native_thread_id_get ());

	/*
	Block the restart signal. 
	We need to block the restart signal while posting to the suspend_ack semaphore or we race to sigsuspend,
	which might miss the signal and get stuck.
	*/
	pthread_sigmask (SIG_BLOCK, &suspend_ack_signal_mask, NULL);

	/* notify the waiting thread */
	MONO_SEM_POST (suspend_ack_semaphore_ptr);
	info->stop_count = stop_count;

	/* wait until we receive the restart signal */
	do {
		info->signal = 0;
		sigsuspend (&suspend_signal_mask);
	} while (info->signal != restart_signal_num);

	/* Unblock the restart signal. */
	pthread_sigmask (SIG_UNBLOCK, &suspend_ack_signal_mask, NULL);

	SGEN_LOG (4, "Posting suspend_ack_semaphore for resume from %p %p\n", info, (gpointer)mono_native_thread_id_get ());
	/* notify the waiting thread */
	MONO_SEM_POST (suspend_ack_semaphore_ptr);
}

/* LOCKING: assumes the GC lock is held (by the stopping thread) */
MONO_SIGNAL_HANDLER_FUNC (static, suspend_handler, (int sig, siginfo_t *siginfo, void *context))
{
	/*
	 * The suspend signal handler potentially uses syscalls that
	 * can set errno, and it calls functions that use the hazard
	 * pointer machinery.  Since we're interrupting other code we
	 * must restore those to the values they had when we
	 * interrupted.
	 */

	SgenThreadInfo *info;
	int old_errno = errno;
	int hp_save_index = mono_hazard_pointer_save_for_signal_handler ();

	info = mono_thread_info_current ();
	suspend_thread (info, context);

	mono_hazard_pointer_restore_for_signal_handler (hp_save_index);
	errno = old_errno;
}

MONO_SIGNAL_HANDLER_FUNC (static, restart_handler, (int sig))
{
	SgenThreadInfo *info;
	int old_errno = errno;

	info = mono_thread_info_current ();
	info->signal = restart_signal_num;
	SGEN_LOG (4, "Restart handler in %p %p", info, (gpointer)mono_native_thread_id_get ());
	errno = old_errno;
}

gboolean
sgen_resume_thread (SgenThreadInfo *info)
{
	return mono_threads_pthread_kill (info, restart_signal_num) == 0;
}

gboolean
sgen_suspend_thread (SgenThreadInfo *info)
{
	return mono_threads_pthread_kill (info, suspend_signal_num) == 0;
}

void
sgen_wait_for_suspend_ack (int count)
{
	int i, result;

	for (i = 0; i < count; ++i) {
		while ((result = MONO_SEM_WAIT (suspend_ack_semaphore_ptr)) != 0) {
			if (errno != EINTR) {
				g_error ("MONO_SEM_WAIT FAILED with %d errno %d (%s)", result, errno, strerror (errno));
			}
		}
	}
}

int
sgen_thread_handshake (BOOL suspend)
{
	int count, result;
	SgenThreadInfo *info;
	int signum = suspend ? suspend_signal_num : restart_signal_num;

	MonoNativeThreadId me = mono_native_thread_id_get ();

	count = 0;
	FOREACH_THREAD_SAFE (info) {
		if (mono_native_thread_id_equals (mono_thread_info_get_tid (info), me)) {
			continue;
		}
		if (info->gc_disabled)
			continue;
		/*if (signum == suspend_signal_num && info->stop_count == global_stop_count)
			continue;*/
		result = mono_threads_pthread_kill (info, signum);
		if (result == 0) {
			count++;
		} else {
			info->skip = 1;
		}
	} END_FOREACH_THREAD_SAFE

	sgen_wait_for_suspend_ack (count);

	SGEN_LOG (4, "%s handshake for %d threads\n", suspend ? "suspend" : "resume", count);

	return count;
}

void
sgen_os_init (void)
{
	struct sigaction sinfo;

	suspend_ack_semaphore_ptr = &suspend_ack_semaphore;
	MONO_SEM_INIT (&suspend_ack_semaphore, 0);

	sigfillset (&sinfo.sa_mask);
	sinfo.sa_flags = SA_RESTART | SA_SIGINFO;
	sinfo.sa_sigaction = suspend_handler;
	if (sigaction (suspend_signal_num, &sinfo, NULL) != 0) {
		g_error ("failed sigaction");
	}

	sinfo.sa_handler = (void*) restart_handler;
	if (sigaction (restart_signal_num, &sinfo, NULL) != 0) {
		g_error ("failed sigaction");
	}

	sigfillset (&suspend_signal_mask);
	sigdelset (&suspend_signal_mask, restart_signal_num);

	sigemptyset (&suspend_ack_signal_mask);
	sigaddset (&suspend_ack_signal_mask, restart_signal_num);
	
}

int
mono_gc_get_suspend_signal (void)
{
	return suspend_signal_num;
}

int
mono_gc_get_restart_signal (void)
{
	return restart_signal_num;
}
#endif
#endif
