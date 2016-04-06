/*
 * mono-threads-posix-signals.c: Shared facility for Posix signals support
 *
 * Author:
 *	Ludovic Henry (ludovic@gmail.com)
 *
 * (C) 2015 Xamarin, Inc
 */

#include <config.h>
#include <glib.h>

#include "mono-threads.h"

#if defined(USE_POSIX_BACKEND)

#include <errno.h>
#include <signal.h>

#if defined(__APPLE__) || defined(__OpenBSD__) || defined(__FreeBSD__) || defined(__FreeBSD_kernel__)
#define DEFAULT_SUSPEND_SIGNAL SIGXFSZ
#else
#define DEFAULT_SUSPEND_SIGNAL SIGPWR
#endif
#define DEFAULT_RESTART_SIGNAL SIGXCPU

static int abort_signal_num;

static sigset_t suspend_signal_mask;
static sigset_t suspend_ack_signal_mask;

int
mono_threads_posix_signal_search_alternative (void)
{
#if !defined (SIGRTMIN)
	g_error ("signal search only works with RTMIN");
#else
	int i;
	/* we try to avoid SIGRTMIN and any one that might have been set already, see bug #75387 */
	for (i = SIGRTMIN + 1; i < SIGRTMAX; ++i) {
		struct sigaction sinfo;
		sigaction (i, NULL, &sinfo);
		if (sinfo.sa_handler == SIG_DFL && (void*)sinfo.sa_sigaction == (void*)SIG_DFL) {
			return i;
		}
	}
	g_error ("Could not find an available signal");
#endif
}

static void
signal_add_handler (int signo, gpointer handler, int flags)
{
#if defined(__native_client__)
	g_assert_not_reached ();
#else
	/*FIXME, move the code from mini to utils and do the right thing!*/
	struct sigaction sa;
	struct sigaction previous_sa;
	int ret;

	sa.sa_sigaction = (void (*)(int, siginfo_t *, void *))handler;
	sigfillset (&sa.sa_mask);

	sa.sa_flags = SA_SIGINFO | flags;
	ret = sigaction (signo, &sa, &previous_sa);

	g_assert (ret != -1);
#endif
}

static int
abort_signal_get (void)
{
#if defined(PLATFORM_ANDROID)
	return SIGTTIN;
#elif defined (SIGRTMIN)
	static int abort_signum = -1;
	if (abort_signum == -1)
		abort_signum = mono_threads_posix_signal_search_alternative ();
	return abort_signum;
#elif defined (SIGTTIN)
	return SIGTTIN;
#else
	return -1;
#endif
}

static void
restart_signal_handler (int _dummy, siginfo_t *_info, void *context)
{
#if defined(__native_client__)
	g_assert_not_reached ();
#else
	MonoThreadInfo *info;
	int old_errno = errno;

	info = mono_thread_info_current ();
	info->signal = DEFAULT_RESTART_SIGNAL;
	errno = old_errno;
#endif
}

static void
suspend_signal_handler (int _dummy, siginfo_t *info, void *context)
{
#if defined(__native_client__)
	g_assert_not_reached ();
#else
	int old_errno = errno;
	int hp_save_index = mono_hazard_pointer_save_for_signal_handler ();


	MonoThreadInfo *current = mono_thread_info_current ();
	gboolean ret;

	THREADS_SUSPEND_DEBUG ("SIGNAL HANDLER FOR %p [%p]\n", mono_thread_info_get_tid (current), (void*)current->native_handle);
	if (current->syscall_break_signal) {
		current->syscall_break_signal = FALSE;
		THREADS_SUSPEND_DEBUG ("\tsyscall break for %p\n", mono_thread_info_get_tid (current));
		mono_threads_notify_initiator_of_abort (current);
		goto done;
	}

	/* Have we raced with self suspend? */
	if (!mono_threads_transition_finish_async_suspend (current)) {
		current->suspend_can_continue = TRUE;
		THREADS_SUSPEND_DEBUG ("\tlost race with self suspend %p\n", mono_thread_info_get_tid (current));
		goto done;
	}

	ret = mono_threads_get_runtime_callbacks ()->thread_state_init_from_sigctx (&current->thread_saved_state [ASYNC_SUSPEND_STATE_INDEX], context);

	/* thread_state_init_from_sigctx return FALSE if the current thread is detaching and suspend can't continue. */
	current->suspend_can_continue = ret;

	/* This thread is doomed, all we can do is give up and let the suspender recover. */
	if (!ret) {
		THREADS_SUSPEND_DEBUG ("\tThread is starting or detaching, failed to capture state %p\n", mono_thread_info_get_tid (current));

		mono_threads_transition_async_suspend_compensation (current);

		/* We're done suspending */
		mono_threads_notify_initiator_of_suspend (current);

		goto done;
	}

	/*
	Block the restart signal.
	We need to block the restart signal while posting to the suspend_ack semaphore or we race to sigsuspend,
	which might miss the signal and get stuck.
	*/
	pthread_sigmask (SIG_BLOCK, &suspend_ack_signal_mask, NULL);

	/* We're done suspending */
	mono_threads_notify_initiator_of_suspend (current);

	do {
		current->signal = 0;
		sigsuspend (&suspend_signal_mask);
	} while (current->signal != DEFAULT_RESTART_SIGNAL);

	/* Unblock the restart signal. */
	pthread_sigmask (SIG_UNBLOCK, &suspend_ack_signal_mask, NULL);

	if (current->async_target) {
#if MONO_ARCH_HAS_MONO_CONTEXT
		MonoContext tmp = current->thread_saved_state [ASYNC_SUSPEND_STATE_INDEX].ctx;
		mono_threads_get_runtime_callbacks ()->setup_async_callback (&tmp, current->async_target, current->user_data);
		current->user_data = NULL;
		current->async_target = NULL;
		mono_monoctx_to_sigctx (&tmp, context);
#else
		g_error ("The new interruption machinery requires a working mono-context");
#endif
	}

	/* We're done resuming */
	mono_threads_notify_initiator_of_resume (current);

done:
	mono_hazard_pointer_restore_for_signal_handler (hp_save_index);
	errno = old_errno;
#endif
}

void
mono_threads_posix_init_signals (void)
{
	sigset_t signal_set;

	sigemptyset (&signal_set);

	sigfillset (&suspend_signal_mask);
	sigdelset (&suspend_signal_mask, DEFAULT_RESTART_SIGNAL);

	sigemptyset (&suspend_ack_signal_mask);
	sigaddset (&suspend_ack_signal_mask, DEFAULT_RESTART_SIGNAL);

	signal_add_handler (DEFAULT_SUSPEND_SIGNAL, suspend_signal_handler, SA_RESTART);
	signal_add_handler (DEFAULT_RESTART_SIGNAL, restart_signal_handler, SA_RESTART);

	sigaddset (&signal_set, DEFAULT_SUSPEND_SIGNAL);
	sigaddset (&signal_set, DEFAULT_RESTART_SIGNAL);

	abort_signal_num = abort_signal_get ();

	/* the difference between abort and suspend here is made by not
	 * passing SA_RESTART, meaning we won't restart the syscall when
	 * receiving a signal */
	signal_add_handler (abort_signal_num, suspend_signal_handler, 0);

	sigaddset (&signal_set, abort_signal_num);

	/* ensure all the new signals are unblocked */
	sigprocmask (SIG_UNBLOCK, &signal_set, NULL);
}

gint
mono_threads_posix_get_suspend_signal (void)
{
	return DEFAULT_SUSPEND_SIGNAL;
}

gint
mono_threads_posix_get_restart_signal (void)
{
	return DEFAULT_RESTART_SIGNAL;
}

gint
mono_threads_posix_get_abort_signal (void)
{
	return abort_signal_num;
}

void
mono_threads_core_abort_syscall (MonoThreadInfo *info)
{
	/* We signal a thread to break it from the current syscall.
	 * This signal should not be interpreted as a suspend request. */
	info->syscall_break_signal = TRUE;
	if (mono_threads_pthread_kill (info, mono_threads_posix_get_abort_signal ()) == 0) {
		mono_threads_add_to_pending_operation_set (info);
	}
}

gboolean
mono_threads_core_needs_abort_syscall (void)
{
	return TRUE;
}

#else /* !defined(USE_POSIX_BACKEND) */

gint
mono_threads_posix_get_suspend_signal (void)
{
	return -1;
}

gint
mono_threads_posix_get_restart_signal (void)
{
	return -1;
}

gint
mono_threads_posix_get_abort_signal (void)
{
	return -1;
}

#endif /* defined(USE_POSIX_BACKEND) */
