#include <mono/utils/mono-threads.h>

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

#include "Thread-c-api.h"

void
mono_threads_suspend_init (void)
{
   g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gboolean
mono_threads_suspend_begin_async_suspend (MonoThreadInfo *info, gboolean interrupt_kernel)
{
   g_assert(0 && "This function is not yet implemented for the Unity platform.");
   return FALSE;
}

gboolean
mono_threads_suspend_check_suspend_result (MonoThreadInfo *info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_threads_suspend_abort_syscall (MonoThreadInfo *info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gboolean
mono_threads_suspend_needs_abort_syscall (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_threads_suspend_begin_async_resume (MonoThreadInfo *info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_threads_suspend_register (MonoThreadInfo *info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_threads_suspend_free (MonoThreadInfo *info)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_threads_suspend_init_signals (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gint
mono_threads_suspend_search_alternative_signal (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	g_assert_not_reached ();
}

gint
mono_threads_suspend_get_suspend_signal (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return -1;
}

gint
mono_threads_suspend_get_restart_signal (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return -1;
}

gint
mono_threads_suspend_get_abort_signal (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return -1;
}

int
mono_threads_platform_create_thread (MonoThreadStart thread_fn, gpointer thread_data, gsize* const stack_size, MonoNativeThreadId *out_tid)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return-1;
}


MonoNativeThreadId
mono_native_thread_id_get (void)
{
	return (MonoNativeThreadId)UnityPalGetCurrentThreadId();
}

gboolean
mono_native_thread_id_equals (MonoNativeThreadId id1, MonoNativeThreadId id2)
{
	return id1 == id2;
}

gboolean
mono_native_thread_create (MonoNativeThreadId *tid, gpointer func, gpointer arg)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_native_thread_join (MonoNativeThreadId tid)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_threads_platform_get_stack_bounds (guint8 **staddr, size_t *stsize)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_threads_platform_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gboolean
mono_threads_platform_in_critical_region (MonoNativeThreadId tid)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gboolean
mono_threads_platform_yield (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_threads_platform_exit (gsize exit_code)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

int
mono_threads_get_max_stack_size (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

void
mono_native_thread_set_name (MonoNativeThreadId tid, const char *name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

#endif //PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS
