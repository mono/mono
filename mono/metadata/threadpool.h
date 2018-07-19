/**
 * \file
 */

#ifndef _MONO_METADATA_THREADPOOL_H_
#define _MONO_METADATA_THREADPOOL_H_

#include <config.h>
#include <glib.h>

#include <mono/metadata/exception.h>
#include <mono/metadata/object-internals.h>

typedef struct _MonoNativeOverlapped MonoNativeOverlapped;

void
mono_threadpool_cleanup (void);

MonoAsyncResult *
mono_threadpool_begin_invoke (MonoDomain *domain, MonoObject *target, MonoMethod *method, gpointer *params, MonoError *error);
MonoObject *
mono_threadpool_end_invoke (MonoAsyncResult *ares, MonoArray **out_args, MonoObject **exc, MonoError *error);

gboolean
mono_threadpool_remove_domain_jobs (MonoDomain *domain, int timeout);

void
mono_threadpool_suspend (void);
void
mono_threadpool_resume (void);

void
ves_icall_System_Threading_ThreadPool_GetAvailableThreadsNative (gint32 *worker_threads, gint32 *completion_port_threads, MonoError *error);
void
ves_icall_System_Threading_ThreadPool_GetMinThreadsNative (gint32 *worker_threads, gint32 *completion_port_threads, MonoError *error);
void
ves_icall_System_Threading_ThreadPool_GetMaxThreadsNative (gint32 *worker_threads, gint32 *completion_port_threads, MonoError *error);
MonoBoolean
ves_icall_System_Threading_ThreadPool_SetMinThreadsNative (gint32 worker_threads, gint32 completion_port_threads, MonoError *error);
MonoBoolean
ves_icall_System_Threading_ThreadPool_SetMaxThreadsNative (gint32 worker_threads, gint32 completion_port_threads, MonoError *error);
void
ves_icall_System_Threading_ThreadPool_InitializeVMTp (MonoBoolean *enable_worker_tracking, MonoError *error);
MonoBoolean
ves_icall_System_Threading_ThreadPool_NotifyWorkItemComplete (MonoError *error);
void
ves_icall_System_Threading_ThreadPool_NotifyWorkItemProgressNative (MonoError *error);
void
ves_icall_System_Threading_ThreadPool_NotifyWorkItemQueued (MonoError *error);
void
ves_icall_System_Threading_ThreadPool_ReportThreadStatus (MonoBoolean is_working, MonoError *error);
MonoBoolean
ves_icall_System_Threading_ThreadPool_RequestWorkerThread (MonoError *error);

MonoBoolean
ves_icall_System_Threading_ThreadPool_PostQueuedCompletionStatus (MonoNativeOverlapped *native_overlapped, MonoError *error);

MonoBoolean
ves_icall_System_Threading_ThreadPool_BindIOCompletionCallbackNative (gpointer file_handle, MonoError *error);

MonoBoolean
ves_icall_System_Threading_ThreadPool_IsThreadPoolHosted (MonoError *error);

/* Internals */

gboolean
mono_threadpool_enqueue_work_item (MonoDomain *domain, MonoObject *work_item, MonoError *error);

#endif // _MONO_METADATA_THREADPOOL_H_
