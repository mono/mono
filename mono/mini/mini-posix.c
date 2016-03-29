/*
 * mini-posix.c: POSIX signal handling support for Mono.
 *
 * Authors:
 *   Mono Team (mono-list@lists.ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc.
 * Copyright 2003-2008 Ximian, Inc.
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 *
 * See LICENSE for licensing information.
 */
#include <config.h>
#include <signal.h>
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <math.h>
#ifdef HAVE_SYS_SYSCALL_H
#include <sys/syscall.h>
#endif
#include <errno.h>


#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/io-layer/io-layer.h>
#include "mono/metadata/profiler.h"
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/attach.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/dtrace.h>
#include <mono/utils/mono-signal-handler.h>
#include <mono/utils/mono-threads.h>

#include "mini.h"
#include <string.h>
#include <ctype.h>
#include "trace.h"
#include "version.h"
#include "debugger-agent.h"

#include "jit-icalls.h"

#if defined(__native_client__) || defined(HOST_WATCHOS)

void
mono_runtime_setup_stat_profiler (void)
{
	printf("WARNING: mono_runtime_setup_stat_profiler() called!\n");
}


void
mono_runtime_shutdown_stat_profiler (void)
{
}


gboolean
MONO_SIG_HANDLER_SIGNATURE (mono_chain_signal)
{
	return FALSE;
}

#ifndef PLATFORM_MACOSX
void
mono_runtime_install_handlers (void)
{
}
#endif

void
mono_runtime_posix_install_handlers(void)
{

}

void
mono_runtime_shutdown_handlers (void)
{
}

void
mono_runtime_cleanup_handlers (void)
{
}

#if !defined(PLATFORM_MACOSX)
pid_t
mono_runtime_syscall_fork (void)
{
	g_assert_not_reached();
	return 0;
}

void
mono_gdb_render_native_backtraces (pid_t crashed_pid)
{
}
#endif

#else

static GHashTable *mono_saved_signal_handlers = NULL;

static struct sigaction *
get_saved_signal_handler (int signo)
{
	if (mono_saved_signal_handlers)
		/* The hash is only modified during startup, so no need for locking */
		return (struct sigaction *)g_hash_table_lookup (mono_saved_signal_handlers, GINT_TO_POINTER (signo));
	return NULL;
}

static void
save_old_signal_handler (int signo, struct sigaction *old_action)
{
	struct sigaction *handler_to_save = (struct sigaction *)g_malloc (sizeof (struct sigaction));

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_CONFIG,
				"Saving old signal handler for signal %d.", signo);

	if (! (old_action->sa_flags & SA_SIGINFO)) {
		handler_to_save->sa_handler = old_action->sa_handler;
	} else {
#ifdef MONO_ARCH_USE_SIGACTION
		handler_to_save->sa_sigaction = old_action->sa_sigaction;
#endif /* MONO_ARCH_USE_SIGACTION */
	}
	handler_to_save->sa_mask = old_action->sa_mask;
	handler_to_save->sa_flags = old_action->sa_flags;
	
	if (!mono_saved_signal_handlers)
		mono_saved_signal_handlers = g_hash_table_new (NULL, NULL);
	g_hash_table_insert (mono_saved_signal_handlers, GINT_TO_POINTER (signo), handler_to_save);
}

static void
free_saved_sig_handler_func (gpointer key, gpointer value, gpointer user_data)
{
	g_free (value);
}

static void
free_saved_signal_handlers (void)
{
	if (mono_saved_signal_handlers) {
		g_hash_table_foreach (mono_saved_signal_handlers, free_saved_sig_handler_func, NULL);
		g_hash_table_destroy (mono_saved_signal_handlers);
		mono_saved_signal_handlers = NULL;
	}
}

/*
 * mono_chain_signal:
 *
 *   Call the original signal handler for the signal given by the arguments, which
 * should be the same as for a signal handler. Returns TRUE if the original handler
 * was called, false otherwise.
 */
gboolean
MONO_SIG_HANDLER_SIGNATURE (mono_chain_signal)
{
	int signal = MONO_SIG_HANDLER_GET_SIGNO ();
	struct sigaction *saved_handler = (struct sigaction *)get_saved_signal_handler (signal);

	if (saved_handler && saved_handler->sa_handler) {
		if (!(saved_handler->sa_flags & SA_SIGINFO)) {
			saved_handler->sa_handler (signal);
		} else {
#ifdef MONO_ARCH_USE_SIGACTION
			saved_handler->sa_sigaction (MONO_SIG_HANDLER_PARAMS);
#endif /* MONO_ARCH_USE_SIGACTION */
		}
		return TRUE;
	}
	return FALSE;
}

MONO_SIG_HANDLER_FUNC (static, sigabrt_signal_handler)
{
	MonoJitInfo *ji = NULL;
	MONO_SIG_HANDLER_INFO_TYPE *info = MONO_SIG_HANDLER_GET_INFO ();
	MONO_SIG_HANDLER_GET_CONTEXT;

	if (mono_thread_internal_current ())
		ji = mono_jit_info_table_find_internal (mono_domain_get (), (char *)mono_arch_ip_from_context (ctx), TRUE, TRUE);
	if (!ji) {
        if (mono_chain_signal (MONO_SIG_HANDLER_PARAMS))
			return;
		mono_handle_native_sigsegv (SIGABRT, ctx, info);
	}
}

#if defined(__i386__) || defined(__x86_64__)
#define FULL_STAT_PROFILER_BACKTRACE 1
#define CURRENT_FRAME_GET_BASE_POINTER(f) (* (gpointer*)(f))
#define CURRENT_FRAME_GET_RETURN_ADDRESS(f) (* (((gpointer*)(f)) + 1))
#if MONO_ARCH_STACK_GROWS_UP
#define IS_BEFORE_ON_STACK <
#define IS_AFTER_ON_STACK >
#else
#define IS_BEFORE_ON_STACK >
#define IS_AFTER_ON_STACK <
#endif
#else
#define FULL_STAT_PROFILER_BACKTRACE 0
#endif

#ifdef SIGPROF

#if defined(__ia64__) || defined(__sparc__) || defined(sparc)

MONO_SIG_HANDLER_FUNC (static, sigprof_signal_handler)
{
	if (mono_chain_signal (MONO_SIG_HANDLER_PARAMS))
		return;

	NOT_IMPLEMENTED;
}

#else

static void
per_thread_profiler_hit (void *ctx)
{
	int call_chain_depth = mono_profiler_stat_get_call_chain_depth ();
	MonoProfilerCallChainStrategy call_chain_strategy = mono_profiler_stat_get_call_chain_strategy ();

	if (call_chain_depth == 0) {
		mono_profiler_stat_hit ((guchar *)mono_arch_ip_from_context (ctx), ctx);
	} else {
		MonoJitTlsData *jit_tls = (MonoJitTlsData *)mono_native_tls_get_value (mono_jit_tls_id);
		int current_frame_index = 1;
		MonoContext mono_context;
		guchar *ips [call_chain_depth + 1];

		mono_sigctx_to_monoctx (ctx, &mono_context);
		ips [0] = (guchar *)MONO_CONTEXT_GET_IP (&mono_context);
		
		if (jit_tls != NULL) {
			if (call_chain_strategy == MONO_PROFILER_CALL_CHAIN_NATIVE) {
#if FULL_STAT_PROFILER_BACKTRACE
			guchar *current_frame;
			guchar *stack_bottom;
			guchar *stack_top;
			
			stack_bottom = (guchar *)jit_tls->end_of_stack;
			stack_top = (guchar *)MONO_CONTEXT_GET_SP (&mono_context);
			current_frame = (guchar *)MONO_CONTEXT_GET_BP (&mono_context);
			
			while ((current_frame_index <= call_chain_depth) &&
					(stack_bottom IS_BEFORE_ON_STACK (guchar*) current_frame) &&
					((guchar*) current_frame IS_BEFORE_ON_STACK stack_top)) {
				ips [current_frame_index] = (guchar *)CURRENT_FRAME_GET_RETURN_ADDRESS (current_frame);
				current_frame_index ++;
				stack_top = current_frame;
				current_frame = (guchar *)CURRENT_FRAME_GET_BASE_POINTER (current_frame);
			}
#else
				call_chain_strategy = MONO_PROFILER_CALL_CHAIN_GLIBC;
#endif
			}
			
			if (call_chain_strategy == MONO_PROFILER_CALL_CHAIN_GLIBC) {
#if GLIBC_PROFILER_BACKTRACE
				current_frame_index = backtrace ((void**) & ips [1], call_chain_depth);
#else
				call_chain_strategy = MONO_PROFILER_CALL_CHAIN_MANAGED;
#endif
			}

			if (call_chain_strategy == MONO_PROFILER_CALL_CHAIN_MANAGED) {
				MonoDomain *domain = mono_domain_get ();
				if (domain != NULL) {
					MonoLMF *lmf = NULL;
					MonoJitInfo *ji;
					MonoJitInfo res;
					MonoContext new_mono_context;
					int native_offset;
					ji = mono_find_jit_info (domain, jit_tls, &res, NULL, &mono_context,
							&new_mono_context, NULL, &lmf, &native_offset, NULL);
					while ((ji != NULL) && (current_frame_index <= call_chain_depth)) {
						ips [current_frame_index] = (guchar *)MONO_CONTEXT_GET_IP (&new_mono_context);
						current_frame_index ++;
						mono_context = new_mono_context;
						ji = mono_find_jit_info (domain, jit_tls, &res, NULL, &mono_context,
								&new_mono_context, NULL, &lmf, &native_offset, NULL);
					}
				}
			}
		}
		
		mono_profiler_stat_call_chain (current_frame_index, & ips [0], ctx);
	}
}

MONO_SIG_HANDLER_FUNC (static, sigprof_signal_handler)
{
	int old_errno = errno;
	int hp_save_index;
	MONO_SIG_HANDLER_GET_CONTEXT;

	if (mono_thread_info_get_small_id () == -1)
		return; //an non-attached thread got the signal

	if (!mono_domain_get () || !mono_native_tls_get_value (mono_jit_tls_id))
		return; //thread in the process of dettaching

	hp_save_index = mono_hazard_pointer_save_for_signal_handler ();

	mono_thread_info_set_is_async_context (TRUE);
	per_thread_profiler_hit (ctx);
	mono_thread_info_set_is_async_context (FALSE);

	mono_hazard_pointer_restore_for_signal_handler (hp_save_index);
	errno = old_errno;

	mono_chain_signal (MONO_SIG_HANDLER_PARAMS);
}

#endif
#endif

MONO_SIG_HANDLER_FUNC (static, sigquit_signal_handler)
{
	gboolean res;

	/* We use this signal to start the attach agent too */
	res = mono_attach_start ();
	if (res)
		return;

	mono_threads_request_thread_dump ();

	mono_chain_signal (MONO_SIG_HANDLER_PARAMS);
}

MONO_SIG_HANDLER_FUNC (static, sigusr2_signal_handler)
{
	gboolean enabled = mono_trace_is_enabled ();

	mono_trace_enable (!enabled);

	mono_chain_signal (MONO_SIG_HANDLER_PARAMS);
}

static void
add_signal_handler (int signo, gpointer handler, int flags)
{
	struct sigaction sa;
	struct sigaction previous_sa;

#ifdef MONO_ARCH_USE_SIGACTION
	sa.sa_sigaction = (void (*)(int, siginfo_t *, void *))handler;
	sigemptyset (&sa.sa_mask);
	sa.sa_flags = SA_SIGINFO | flags;
#ifdef MONO_ARCH_SIGSEGV_ON_ALTSTACK

/*Apple likes to deliver SIGBUS for *0 */
#ifdef PLATFORM_MACOSX
	if (signo == SIGSEGV || signo == SIGBUS) {
#else
	if (signo == SIGSEGV) {
#endif
		sa.sa_flags |= SA_ONSTACK;

		/* 
		 * libgc will crash when trying to do stack marking for threads which are on
		 * an altstack, so delay the suspend signal after the signal handler has
		 * executed.
		 */
		if (mono_gc_get_suspend_signal () != -1)
			sigaddset (&sa.sa_mask, mono_gc_get_suspend_signal ());
	}
#endif
	if (signo == SIGSEGV) {
		/* 
		 * Delay abort signals while handling SIGSEGVs since they could go unnoticed.
		 */
		sigset_t block_mask;
     
		sigemptyset (&block_mask);
	}
#else
	sa.sa_handler = handler;
	sigemptyset (&sa.sa_mask);
	sa.sa_flags = flags;
#endif
	g_assert (sigaction (signo, &sa, &previous_sa) != -1);

	/* if there was already a handler in place for this signal, store it */
	if (! (previous_sa.sa_flags & SA_SIGINFO) &&
			(SIG_DFL == previous_sa.sa_handler)) { 
		/* it there is no sa_sigaction function and the sa_handler is default, we can safely ignore this */
	} else {
		if (mono_do_signal_chaining)
			save_old_signal_handler (signo, &previous_sa);
	}
}

static void
remove_signal_handler (int signo)
{
	struct sigaction sa;
	struct sigaction *saved_action = get_saved_signal_handler (signo);

	if (!saved_action) {
		sa.sa_handler = SIG_DFL;
		sigemptyset (&sa.sa_mask);
		sa.sa_flags = 0;

		sigaction (signo, &sa, NULL);
	} else {
		g_assert (sigaction (signo, saved_action, NULL) != -1);
	}
}

void
mono_runtime_posix_install_handlers (void)
{

	sigset_t signal_set;

	if (mini_get_debug_options ()->handle_sigint)
		add_signal_handler (SIGINT, mono_sigint_signal_handler, SA_RESTART);

	add_signal_handler (SIGFPE, mono_sigfpe_signal_handler, 0);
	add_signal_handler (SIGQUIT, sigquit_signal_handler, SA_RESTART);
	add_signal_handler (SIGILL, mono_sigill_signal_handler, 0);
	add_signal_handler (SIGBUS, mono_sigsegv_signal_handler, 0);
	if (mono_jit_trace_calls != NULL)
		add_signal_handler (SIGUSR2, sigusr2_signal_handler, SA_RESTART);

	/* it seems to have become a common bug for some programs that run as parents
	 * of many processes to block signal delivery for real time signals.
	 * We try to detect and work around their breakage here.
	 */
	sigemptyset (&signal_set);
	if (mono_gc_get_suspend_signal () != -1)
		sigaddset (&signal_set, mono_gc_get_suspend_signal ());
	if (mono_gc_get_restart_signal () != -1)
		sigaddset (&signal_set, mono_gc_get_restart_signal ());
	sigaddset (&signal_set, SIGCHLD);
	sigprocmask (SIG_UNBLOCK, &signal_set, NULL);

	signal (SIGPIPE, SIG_IGN);

	add_signal_handler (SIGABRT, sigabrt_signal_handler, 0);

	/* catch SIGSEGV */
	add_signal_handler (SIGSEGV, mono_sigsegv_signal_handler, 0);
}

#ifndef PLATFORM_MACOSX
void
mono_runtime_install_handlers (void)
{
	mono_runtime_posix_install_handlers ();
}
#endif

void
mono_runtime_cleanup_handlers (void)
{
	if (mini_get_debug_options ()->handle_sigint)
		remove_signal_handler (SIGINT);

	remove_signal_handler (SIGFPE);
	remove_signal_handler (SIGQUIT);
	remove_signal_handler (SIGILL);
	remove_signal_handler (SIGBUS);
	if (mono_jit_trace_calls != NULL)
		remove_signal_handler (SIGUSR2);

	remove_signal_handler (SIGABRT);

	remove_signal_handler (SIGSEGV);

	free_saved_signal_handlers ();
}

static MonoNativeThreadId sampling_thread;
static volatile gint32 sampling_thread_running;

static mono_native_thread_return_t
sampling_thread_func (void *data)
{
	/*
	 * This gives us access to lock-free data structures that use hazard
	 * pointers, e.g. the LLS (used for the thread list).
	 */
	mono_thread_info_register_small_id ();

	MonoNativeThreadId me = mono_native_thread_id_get ();
	guint64 us = 1000000 / mono_profiler_get_sampling_rate () - 1;

	while (InterlockedRead (&sampling_thread_running)) {
		FOREACH_THREAD_SAFE (info) {
			if (mono_thread_info_get_tid (info) == me)
				continue;

			mono_threads_pthread_kill (info, SIGPROF);
		} FOREACH_THREAD_SAFE_END

		mono_thread_info_usleep (us);
	}

	return NULL;
}

void
mono_runtime_shutdown_stat_profiler (void)
{
#ifdef SIGPROF
	InterlockedWrite (&sampling_thread_running, 0);
	pthread_join (sampling_thread, NULL);

	/*
	 * FIXME: We can't safely remove the signal handler because we have no
	 * guarantee that all pending signals have been delivered at this point.
	 */
	//remove_signal_handler (SIGPROF);
#endif
}

void
mono_runtime_setup_stat_profiler (void)
{
#ifdef SIGPROF
	add_signal_handler (SIGPROF, sigprof_signal_handler, SA_RESTART);

	InterlockedWrite (&sampling_thread_running, 1);
	mono_native_thread_create (&sampling_thread, sampling_thread_func, NULL);
#endif
}

#if !defined(PLATFORM_MACOSX)
pid_t
mono_runtime_syscall_fork ()
{
#if defined(PLATFORM_ANDROID)
	/* SYS_fork is defined to be __NR_fork which is not defined in some ndk versions */
	g_assert_not_reached ();
	return 0;
#elif defined(SYS_fork)
	return (pid_t) syscall (SYS_fork);
#else
	g_assert_not_reached ();
	return 0;
#endif
}

void
mono_gdb_render_native_backtraces (pid_t crashed_pid)
{
	const char *argv [9];
	char template_ [] = "/tmp/mono-lldb-commands.XXXXXX";
	char buf1 [128];
	FILE *commands;
	gboolean using_lldb = FALSE;

	argv [0] = g_find_program_in_path ("gdb");
	if (argv [0] == NULL) {
		argv [0] = g_find_program_in_path ("lldb");
		using_lldb = TRUE;
	}

	if (argv [0] == NULL)
		return;

	if (using_lldb) {
		if (mkstemp (template_) == -1)
			return;

		commands = fopen (template_, "w");

		fprintf (commands, "process attach --pid %ld\n", (long) crashed_pid);
		fprintf (commands, "thread list\n");
		fprintf (commands, "thread backtrace all\n");
		fprintf (commands, "detach\n");
		fprintf (commands, "quit\n");

		fflush (commands);
		fclose (commands);

		argv [1] = "--source";
		argv [2] = template_;
		argv [3] = 0;
	} else {
		argv [1] = "-ex";
		sprintf (buf1, "attach %ld", (long) crashed_pid);
		argv [2] = buf1;
		argv [3] = "--ex";
		argv [4] = "info threads";
		argv [5] = "--ex";
		argv [6] = "thread apply all bt";
		argv [7] = "--batch";
		argv [8] = 0;
	}

	execv (argv [0], (char**)argv);

	if (using_lldb)
		unlink (template_);
}
#endif
#endif /* __native_client__ */

#if !defined (__MACH__)

gboolean
mono_thread_state_init_from_handle (MonoThreadUnwindState *tctx, MonoThreadInfo *info)
{
	g_error ("Posix systems don't support mono_thread_state_init_from_handle");
	return FALSE;
}

#endif
