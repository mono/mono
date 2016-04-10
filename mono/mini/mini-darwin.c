/*
 * mini-darwin.c: Darwin/MacOS support for Mono.
 *
 * Authors:
 *   Mono Team (mono-list@lists.ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc.
 * Copyright 2003-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

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
#include <mono/metadata/threads-types.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/attach.h>
#include <mono/metadata/gc-internals.h>
#include <mono/utils/mono-math.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/dtrace.h>

#include "mini.h"
#include <string.h>
#include <ctype.h>
#include "trace.h"
#include "version.h"

#include "jit-icalls.h"

/* MacOS includes */
#include <mach/mach.h>
#include <mach/mach_error.h>
#include <mach/exception.h>
#include <mach/task.h>
#include <pthread.h>
#include <dlfcn.h>
#include <AvailabilityMacros.h>

#if defined (TARGET_OSX) && (MAC_OS_X_VERSION_MIN_REQUIRED <= MAC_OS_X_VERSION_10_5)
#define NEEDS_EXCEPTION_THREAD
#endif

#ifdef NEEDS_EXCEPTION_THREAD

/*
 * This code disables the CrashReporter of MacOS X by installing
 * a dummy Mach exception handler.
 */

/*
 * http://darwinsource.opendarwin.org/10.4.3/xnu-792.6.22/osfmk/man/exc_server.html
 */
extern boolean_t exc_server (mach_msg_header_t *request_msg, mach_msg_header_t *reply_msg);

/*
 * The exception message
 */
typedef struct {
	mach_msg_base_t msg;  /* common mach message header */
	char payload [1024];  /* opaque */
} mach_exception_msg_t;

/* The exception port */
static mach_port_t mach_exception_port = VM_MAP_NULL;

kern_return_t
catch_exception_raise (
	mach_port_t exception_port,
	mach_port_t thread,
	mach_port_t task,
	exception_type_t exception,
	exception_data_t code,
	mach_msg_type_number_t code_count);

/*
 * Implicitly called by exc_server. Must be public.
 *
 * http://darwinsource.opendarwin.org/10.4.3/xnu-792.6.22/osfmk/man/catch_exception_raise.html
 */
kern_return_t
catch_exception_raise (
	mach_port_t exception_port,
	mach_port_t thread,
	mach_port_t task,
	exception_type_t exception,
	exception_data_t code,
	mach_msg_type_number_t code_count)
{
	/* consume the exception */
	return KERN_FAILURE;
}

/*
 * Exception thread handler.
 */
static
void *
mach_exception_thread (void *arg)
{
	for (;;) {
		mach_exception_msg_t request;
		mach_exception_msg_t reply;
		mach_msg_return_t result;

		/* receive from "mach_exception_port" */
		result = mach_msg (&request.msg.header,
				   MACH_RCV_MSG | MACH_RCV_LARGE,
				   0,
				   sizeof (request),
				   mach_exception_port,
				   MACH_MSG_TIMEOUT_NONE,
				   MACH_PORT_NULL);

		g_assert (result == MACH_MSG_SUCCESS);

		/* dispatch to catch_exception_raise () */
		exc_server (&request.msg.header, &reply.msg.header);

		/* send back to sender */
		result = mach_msg (&reply.msg.header,
				   MACH_SEND_MSG,
				   reply.msg.header.msgh_size,
				   0,
				   MACH_PORT_NULL,
				   MACH_MSG_TIMEOUT_NONE,
				   MACH_PORT_NULL);

		/*
		If we try to abort the thread while delivering an exception. The port will be gone since the kernel
		setup a send once port to deliver the resume message and thread_abort will consume it.
		*/
		g_assert (result == MACH_MSG_SUCCESS || result == MACH_SEND_INVALID_DEST);
	}
	return NULL;
}

static void
macosx_register_exception_handler (void)
{
	mach_port_t task;
	pthread_attr_t attr;
	pthread_t thread;

	if (mach_exception_port != VM_MAP_NULL)
		return;

	task = mach_task_self ();

	/* create the "mach_exception_port" with send & receive rights */
	g_assert (mach_port_allocate (task, MACH_PORT_RIGHT_RECEIVE,
				      &mach_exception_port) == KERN_SUCCESS);
	g_assert (mach_port_insert_right (task, mach_exception_port, mach_exception_port,
					  MACH_MSG_TYPE_MAKE_SEND) == KERN_SUCCESS);

	/* create the exception handler thread */
	g_assert (!pthread_attr_init (&attr));
	g_assert (!pthread_attr_setdetachstate (&attr, PTHREAD_CREATE_DETACHED));
	g_assert (!pthread_create (&thread, &attr, mach_exception_thread, NULL));
	pthread_attr_destroy (&attr);

	/*
	 * register "mach_exception_port" as a receiver for the
	 * EXC_BAD_ACCESS exception
	 *
	 * http://darwinsource.opendarwin.org/10.4.3/xnu-792.6.22/osfmk/man/task_set_exception_ports.html
	 */
	g_assert (task_set_exception_ports (task, EXC_MASK_BAD_ACCESS,
					    mach_exception_port,
					    EXCEPTION_DEFAULT,
					    MACHINE_THREAD_STATE) == KERN_SUCCESS);

	mono_gc_register_mach_exception_thread (thread);
}

#endif

/* This is #define'd by Boehm GC to _GC_dlopen. */
#undef dlopen

void* dlopen(const char* path, int mode);

void
mono_runtime_install_handlers (void)
{
#ifdef NEEDS_EXCEPTION_THREAD
	macosx_register_exception_handler ();
#endif
	mono_runtime_posix_install_handlers ();

	/* Snow Leopard has a horrible bug: http://openradar.appspot.com/7209349
	 * This causes obscure SIGTRAP's for any application that comes across this built on
	 * Snow Leopard.  This is a horrible hack to ensure that the private __CFInitialize
	 * is run on the main thread, so that we don't get SIGTRAPs later
	 */
#if defined (__APPLE__) && (defined (__i386__) || defined (__x86_64__))
	{
		void *handle = dlopen ("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", RTLD_LAZY);
		if (handle == NULL)
			return;

		dlclose (handle);
	}
#endif
}

pid_t
mono_runtime_syscall_fork ()
{
#ifdef HAVE_FORK
	return (pid_t) fork ();
#else
	g_assert_not_reached ();
#endif
}

void
mono_gdb_render_native_backtraces (pid_t crashed_pid)
{
#ifdef HAVE_EXECV
	const char *argv [5];
	char template [] = "/tmp/mono-gdb-commands.XXXXXX";
	FILE *commands;
	gboolean using_lldb = FALSE;

	using_lldb = TRUE;

	argv [0] = g_find_program_in_path ("gdb");
	if (argv [0])
		using_lldb = FALSE;

	if (using_lldb)
		argv [0] = g_find_program_in_path ("lldb");

	if (argv [0] == NULL)
		return;

	if (mkstemp (template) == -1)
		return;

	commands = fopen (template, "w");
	if (using_lldb) {
		fprintf (commands, "process attach --pid %ld\n", (long) crashed_pid);
		fprintf (commands, "thread list\n");
		fprintf (commands, "thread backtrace all\n");
		fprintf (commands, "detach\n");
		fprintf (commands, "quit\n");
		argv [1] = "--source";
		argv [2] = template;
		argv [3] = 0;
		
	} else {
		fprintf (commands, "attach %ld\n", (long) crashed_pid);
		fprintf (commands, "info threads\n");
		fprintf (commands, " t a a info thread\n");
		fprintf (commands, "thread apply all bt\n");
		argv [1] = "-batch";
		argv [2] = "-x";
		argv [3] = template;
		argv [4] = 0;
	}
	fflush (commands);
	fclose (commands);

	fclose (stdin);

	execv (argv [0], (char**)argv);
	unlink (template);
#else
	fprintf (stderr, "mono_gdb_render_native_backtraces not supported on this platform\n");
#endif // HAVE_EXECV
}

gboolean
mono_thread_state_init_from_handle (MonoThreadUnwindState *tctx, MonoThreadInfo *info)
{
	kern_return_t ret;
	mach_msg_type_number_t num_state;
	thread_state_t state;
	ucontext_t ctx;
	mcontext_t mctx;
	MonoJitTlsData *jit_tls;
	void *domain;
	MonoLMF *lmf = NULL;
	gpointer *addr;

	g_assert (info);
	/*Zero enough state to make sure the caller doesn't confuse itself*/
	tctx->valid = FALSE;
	tctx->unwind_data [MONO_UNWIND_DATA_DOMAIN] = NULL;
	tctx->unwind_data [MONO_UNWIND_DATA_LMF] = NULL;
	tctx->unwind_data [MONO_UNWIND_DATA_JIT_TLS] = NULL;

	state = (thread_state_t) alloca (mono_mach_arch_get_thread_state_size ());
	mctx = (mcontext_t) alloca (mono_mach_arch_get_mcontext_size ());

	ret = mono_mach_arch_get_thread_state (info->native_handle, state, &num_state);
	if (ret != KERN_SUCCESS)
		return FALSE;

	mono_mach_arch_thread_state_to_mcontext (state, mctx);
	ctx.uc_mcontext = mctx;

	mono_sigctx_to_monoctx (&ctx, &tctx->ctx);

	/* mono_set_jit_tls () sets this */
	jit_tls = mono_thread_info_tls_get (info, TLS_KEY_JIT_TLS);
	/* SET_APPDOMAIN () sets this */
	domain = mono_thread_info_tls_get (info, TLS_KEY_DOMAIN);

	/*Thread already started to cleanup, can no longer capture unwind state*/
	if (!jit_tls || !domain)
		return FALSE;

	/*
	 * The current LMF address is kept in a separate TLS variable, and its hard to read its value without
	 * arch-specific code. But the address of the TLS variable is stored in another TLS variable which
	 * can be accessed through MonoThreadInfo.
	 */
	/* mono_set_lmf_addr () sets this */
	addr = mono_thread_info_tls_get (info, TLS_KEY_LMF_ADDR);
	if (addr)
		lmf = *addr;


	tctx->unwind_data [MONO_UNWIND_DATA_DOMAIN] = domain;
	tctx->unwind_data [MONO_UNWIND_DATA_JIT_TLS] = jit_tls;
	tctx->unwind_data [MONO_UNWIND_DATA_LMF] = lmf;
	tctx->valid = TRUE;

	return TRUE;
}
