/*
 * mono-threads.c: Coop threading
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Copyright 2015 Xamarin, Inc (http://www.xamarin.com)
 */

#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-semaphore.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-tls.h>
#include <mono/utils/hazard-pointer.h>
#include <mono/utils/mono-memory-model.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/atomic.h>
#include <mono/utils/mono-time.h>

#ifdef USE_COOP_BACKEND

void
mono_threads_core_abort_syscall (MonoThreadInfo *info)
{
	g_error ("FIXME");
}

gboolean
mono_threads_core_begin_async_resume (MonoThreadInfo *info)
{
	g_error ("FIXME");
	return FALSE;
}

gboolean
mono_threads_core_begin_async_suspend (MonoThreadInfo *info, gboolean interrupt_kernel)
{
	mono_threads_add_to_pending_operation_set (info);
	/* There's nothing else to do after we async request the thread to suspend */
	return TRUE;
}

gboolean
mono_threads_core_check_suspend_result (MonoThreadInfo *info)
{
	/* Async suspend can't async fail on coop */
	return TRUE;
}

gboolean
mono_threads_core_needs_abort_syscall (void)
{
	/*
	Small digression.
	Syscall abort can't be handled by the suspend machinery even though it's kind of implemented
	in a similar way (with, like, signals).

	So, having it here is wrong, it should be on mono-threads-(mach|posix|windows).
	Ideally we would slice this in (coop|preemp) and target. Then have this file set:
	mono-threads-mach, mono-threads-mach-preempt and mono-threads-mach-coop.
	More files, less ifdef hell.
	*/
	return FALSE;
}

void
mono_threads_init_platform (void)
{
	//See the above for what's wrong here.
}

void
mono_threads_platform_free (MonoThreadInfo *info)
{
	//See the above for what's wrong here.
}

void
mono_threads_platform_register (MonoThreadInfo *info)
{
	//See the above for what's wrong here.
}

void
mono_threads_core_begin_global_suspend (void)
{
	mono_polling_required = 1;
}

void
mono_threads_core_end_global_suspend (void)
{
	mono_polling_required = 0;
}


#endif
