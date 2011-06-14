/*
 * mono-threads.h: Low-level threading
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2011 Novell, Inc
 */

#ifndef __MONO_THREADS_H__
#define __MONO_THREADS_H__

#include <mono/utils/mono-semaphore.h>
#include <mono/utils/mono-stack-unwinding.h>
#include <mono/utils/mono-linked-list-set.h>

/* FIXME used for CRITICAL_SECTION replace with mono-mutex  */
#include <mono/io-layer/io-layer.h>

#include <glib.h>

#ifdef HOST_WIN32

#include <windows.h>

typedef DWORD MonoNativeThreadId;
typedef HANDLE MonoNativeThreadHandle;

#define mono_native_thread_id_get GetCurrentThreadId
#define mono_native_thread_id_equals(a,b) ((a) == ((b))

#else

#include <pthread.h>

#if defined(__MACH__)
#include <mono/utils/mach-support.h>

typedef thread_port_t MonoNativeThreadHandle;

#else
/*FIXME this should be pid_t on posix - to handle android borken signaling */
typedef pthread_t MonoNativeThreadHandle;

#endif /* defined(__MACH__) */

typedef pthread_t MonoNativeThreadId;

#define mono_native_thread_id_get pthread_self
#define mono_native_thread_id_equals(a,b) pthread_equal((a),(b))

#endif /* #ifdef HOST_WIN32 */

#ifndef THREAD_INFO_TYPE
#define THREAD_INFO_TYPE MonoThreadInfo
#endif

enum {
	STATE_STARTING,
	STATE_RUNNING,
	STATE_SHUTTING_DOWN,
	STATE_DEAD
};

typedef struct {
	MonoLinkedListSetNode node;
	guint32 small_id; /*Used by hazard pointers */
	MonoNativeThreadHandle native_handle;
	int thread_state;

	/* suspend machinery, fields protected by the suspend_lock */
	CRITICAL_SECTION suspend_lock;
	int suspend_count;

	/* only needed by the posix backend */ 
#if defined(_POSIX_VERSION) && !defined (__MACH__)
	MonoSemType suspend_semaphore;
	MonoSemType resume_semaphore; 
	MonoSemType finish_resume_semaphore;
	gboolean self_suspend;
#endif

	/*In theory, only the posix backend needs this, but having it on mach/win32 simplifies things a lot.*/
	MonoThreadUnwindState suspend_state;

	/*async call machinery, thread MUST be suspended before accessing those fields*/
	void (*async_target)(void*);
	void *user_data;
} MonoThreadInfo;

typedef struct {
	void* (*thread_register)(THREAD_INFO_TYPE *info, void *baseaddr);
	/*
	This callback is called after @info is removed from the thread list.
	SMR remains functional as its small_id has not been reclaimed.
	*/
	void (*thread_unregister)(THREAD_INFO_TYPE *info);
	void (*thread_attach)(THREAD_INFO_TYPE *info);
	gboolean (*mono_method_is_critical) (void *method);
#ifndef HOST_WIN32
	int (*mono_gc_pthread_create) (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg);
#endif
} MonoThreadInfoCallbacks;

typedef struct {
	void (*setup_async_callback) (MonoContext *ctx, void (*async_cb)(void *fun), gpointer user_data);
	gboolean (*thread_state_init_from_sigctx) (MonoThreadUnwindState *state, void *sigctx);
	gboolean (*thread_state_init_from_handle) (MonoThreadUnwindState *tctx, MonoNativeThreadId thread_id, MonoNativeThreadHandle thread_handle);
} MonoThreadInfoRuntimeCallbacks;

/*
Requires the world to be stoped
*/
#define FOREACH_THREAD(thread) MONO_LLS_FOREACH (mono_thread_info_list_head (), thread)
#define END_FOREACH_THREAD MONO_LLS_END_FOREACH

/*
Snapshot iteration.
*/
#define FOREACH_THREAD_SAFE(thread) MONO_LLS_FOREACH_SAFE (mono_thread_info_list_head (), thread)
#define END_FOREACH_THREAD_SAFE MONO_LLS_END_FOREACH_SAFE

#define mono_thread_info_get_tid(info) ((MonoNativeThreadId)((MonoThreadInfo*)info)->node.key)
#define mono_thread_info_set_tid(info, val) do { ((MonoThreadInfo*)(info))->node.key = (uintptr_t)(val); } while (0)

/*
 * @thread_info_size is sizeof (GcThreadInfo), a struct the GC defines to make it possible to have
 * a single block with info from both camps. 
 */
void
mono_threads_init (MonoThreadInfoCallbacks *callbacks, size_t thread_info_size) MONO_INTERNAL;

void
mono_threads_runtime_init (MonoThreadInfoRuntimeCallbacks *callbacks) MONO_INTERNAL;

MonoThreadInfoCallbacks *
mono_threads_get_callbacks (void) MONO_INTERNAL;

MonoThreadInfoRuntimeCallbacks *
mono_threads_get_runtime_callbacks (void) MONO_INTERNAL;

int
mono_thread_info_register_small_id (void) MONO_INTERNAL;

THREAD_INFO_TYPE *
mono_thread_info_attach (void *baseptr) MONO_INTERNAL;

void
mono_thread_info_dettach (void) MONO_INTERNAL;

THREAD_INFO_TYPE *
mono_thread_info_current (void) MONO_INTERNAL;

int
mono_thread_info_get_small_id (void) MONO_INTERNAL;

MonoLinkedListSet*
mono_thread_info_list_head (void) MONO_INTERNAL;

MonoThreadInfo*
mono_thread_info_lookup (MonoNativeThreadId id) MONO_INTERNAL;

MonoThreadInfo*
mono_thread_info_safe_suspend_sync (MonoNativeThreadId tid, gboolean interrupt_kernel) MONO_INTERNAL;

gboolean
mono_thread_info_resume (MonoNativeThreadId tid) MONO_INTERNAL;

void
mono_thread_info_self_suspend (void) MONO_INTERNAL;

gboolean
mono_thread_info_new_interrupt_enabled (void) MONO_INTERNAL;

void
mono_thread_info_setup_async_call (MonoThreadInfo *info, void (*target_func)(void*), void *user_data) MONO_INTERNAL;

void
mono_thread_info_suspend_lock (void) MONO_INTERNAL;

void
mono_thread_info_suspend_unlock (void) MONO_INTERNAL;

void
mono_threads_unregister_current_thread (THREAD_INFO_TYPE *info) MONO_INTERNAL;

#if !defined(HOST_WIN32)

int
mono_threads_pthread_create (pthread_t *new_thread, const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg) MONO_INTERNAL;

#endif /* !defined(HOST_WIN32) */

/* Plartform specific functions DON'T use them */
void mono_threads_init_platform (void) MONO_INTERNAL; //ok
gboolean mono_threads_core_suspend (MonoThreadInfo *info) MONO_INTERNAL;
gboolean mono_threads_core_resume (MonoThreadInfo *info) MONO_INTERNAL;
void mono_threads_platform_register (MonoThreadInfo *info) MONO_INTERNAL; //ok
void mono_threads_platform_free (MonoThreadInfo *info) MONO_INTERNAL;
void mono_threads_core_self_suspend (MonoThreadInfo *info) MONO_INTERNAL;
void mono_threads_core_interrupt (MonoThreadInfo *info) MONO_INTERNAL;

#endif /* __MONO_THREADS_H__ */
