/*
 * thread-private.h:  Private definitions for thread handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_THREAD_PRIVATE_H_
#define _WAPI_THREAD_PRIVATE_H_

#include <config.h>
#include <glib.h>
#include <pthread.h>
#include <mono/utils/mono-semaphore.h>

/* There doesn't seem to be a defined symbol for this */
#define _WAPI_THREAD_CURRENT (gpointer)0xFFFFFFFE

extern struct _WapiHandleOps _wapi_thread_ops;

#define INTERRUPTION_REQUESTED_HANDLE (gpointer)0xFFFFFFFE

struct _WapiHandle_thread
{
	pthread_t id;
	pid_t pid;
	GPtrArray *owned_mutexes;
	/* 
     * Handle this thread waits on. If this is INTERRUPTION_REQUESTED_HANDLE,
	 * it means the thread is interrupted by another thread, and shouldn't enter
	 * a wait.
	 * This also acts as a reference for the handle.
	 */
	gpointer wait_handle;
};

typedef struct _WapiHandle_thread WapiHandle_thread;

extern gboolean _wapi_thread_apc_pending (gpointer handle);
extern gboolean _wapi_thread_cur_apc_pending (void);
extern void _wapi_thread_own_mutex (gpointer mutex);
extern void _wapi_thread_disown_mutex (gpointer mutex);
extern void _wapi_thread_cleanup (void);

#endif /* _WAPI_THREAD_PRIVATE_H_ */
