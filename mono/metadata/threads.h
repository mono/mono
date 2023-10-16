/**
 * \file
 * Threading API
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *	Patrik Torstensson (patrik.torstensson@labs2.com)
 *
 * (C) 2001 Ximian, Inc
 */

#ifndef _MONO_METADATA_THREADS_H_
#define _MONO_METADATA_THREADS_H_

#include <mono/utils/mono-publib.h>
#include <mono/metadata/object.h>
#include <mono/metadata/appdomain.h>

MONO_BEGIN_DECLS

typedef struct MonoUnityStackFrameInfo
{
    const MonoMethod *method;
} MonoUnityStackFrameInfo;

/* This callback should return TRUE if the runtime must wait for the thread, FALSE otherwise */
typedef mono_bool (*MonoThreadManageCallback) (MonoThread* thread);
typedef void (*MonoThreadManageVisit) (MonoThread* thread);
typedef void (*MonoUnityStackFrameInfoWalk) (const MonoUnityStackFrameInfo *info, void *user_data);

MONO_API void mono_thread_init (MonoThreadStartCB start_cb,
			      MonoThreadAttachCB attach_cb);
MONO_API void mono_thread_cleanup (void);
MONO_API MONO_RT_EXTERNAL_ONLY
void mono_thread_manage(void);

MONO_API MonoThread *mono_thread_current (void);

MONO_API void        mono_thread_set_main (MonoThread *thread);
MONO_API MonoThread *mono_thread_get_main (void);

MONO_API MONO_RT_EXTERNAL_ONLY void mono_thread_stop (MonoThread *thread);

MONO_API void mono_thread_new_init (intptr_t tid, void* stack_start,
				  void* func);

MONO_API MONO_RT_EXTERNAL_ONLY void
mono_thread_create (MonoDomain *domain, void* func, void* arg);

MONO_API MONO_RT_EXTERNAL_ONLY MonoThread *
mono_thread_attach (MonoDomain *domain);
MONO_API MONO_RT_EXTERNAL_ONLY void
mono_thread_detach (MonoThread *thread);
MONO_API void mono_thread_exit (void);

MONO_API MONO_RT_EXTERNAL_ONLY void
mono_threads_attach_tools_thread (void);

MONO_API char   *mono_thread_get_name_utf8 (MonoThread *thread);
MONO_API int32_t mono_thread_get_managed_id (MonoThread *thread);

MONO_API void     mono_thread_set_manage_callback (MonoThread *thread, MonoThreadManageCallback func);

MONO_API void mono_threads_set_default_stacksize (uint32_t stacksize);
MONO_API uint32_t mono_threads_get_default_stacksize (void);

MONO_API void mono_threads_request_thread_dump (void);

MONO_API mono_bool mono_thread_is_foreign (MonoThread *thread);

MONO_API MONO_RT_EXTERNAL_ONLY mono_bool
mono_thread_detach_if_exiting (void);

MONO_API mono_bool mono_thread_has_sufficient_execution_stack (void);

MONO_API MONO_RT_EXTERNAL_ONLY
void
mono_unity_thread_get_all_attached_threads(MonoThreadManageVisit visitFunc);

MONO_API MONO_RT_EXTERNAL_ONLY
void mono_unity_current_thread_walk_frame_stack(MonoUnityStackFrameInfoWalk func, void* user_data);

MONO_API MONO_RT_EXTERNAL_ONLY
void mono_unity_thread_walk_frame_stack(MonoThread* thread, MonoUnityStackFrameInfoWalk func, void* user_data);

MONO_API MONO_RT_EXTERNAL_ONLY
void mono_unity_current_thread_get_top_frame(MonoUnityStackFrameInfo* frame);

MONO_API MONO_RT_EXTERNAL_ONLY
void 
mono_unity_thread_get_top_frame(MonoThread* thread, MonoUnityStackFrameInfo* frame);

MONO_API MONO_RT_EXTERNAL_ONLY
void mono_unity_current_thread_get_frame_at(int32_t offset, MonoUnityStackFrameInfo* frame);

MONO_API MONO_RT_EXTERNAL_ONLY
mono_bool mono_unity_thread_get_frame_at(MonoThread* thread, int32_t offset, MonoUnityStackFrameInfo* frame);

MONO_API MONO_RT_EXTERNAL_ONLY
int32_t mono_unity_current_thread_get_stack_depth();

MONO_API MONO_RT_EXTERNAL_ONLY
int32_t mono_unity_thread_get_stack_depth(MonoThread* thread);

MONO_END_DECLS

#endif /* _MONO_METADATA_THREADS_H_ */
