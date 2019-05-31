/*
 * Licensed to the .NET Foundation under one or more agreements.
 * The .NET Foundation licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
 */

#include <config.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/object.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-error.h>
#include <mono/utils/mono-publib.h>
#include <mono/eglib/glib.h>

#ifndef __MONO_EE_H__
#define __MONO_EE_H__

#define MONO_EE_API_VERSION 0xa

typedef struct _MonoInterpStackIter MonoInterpStackIter;

/* Needed for stack allocation */
struct _MonoInterpStackIter {
	gpointer dummy [8];
};

typedef gpointer MonoInterpFrameHandle;

typedef struct _MonoEECallbacks {
	void (*entry_from_trampoline) (gpointer ccontext, gpointer imethod);
	void (*to_native_trampoline) (gpointer addr, gpointer ccontext);
	gpointer (*create_method_pointer) (MonoMethod *method, gboolean compile, MonoError *error);
	MonoFtnDesc *(*create_method_pointer_llvmonly) (MonoMethod *method, gboolean unbox, MonoError *error);
	MonoObject* (*runtime_invoke) (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error);
	void (*init_delegate) (MonoDelegate *del, MonoError *error);
	void (*delegate_ctor) (MonoObjectHandle this_obj, MonoObjectHandle target, gpointer addr, MonoError *error);
	gpointer (*get_remoting_invoke) (MonoMethod *method, gpointer imethod, MonoError *error);
	void (*set_resume_state) (MonoJitTlsData *jit_tls, MonoException *ex, MonoJitExceptionInfo *ei, MonoInterpFrameHandle interp_frame, gpointer handler_ip);
	gboolean (*run_finally) (StackFrameInfo *frame, int clause_index, gpointer handler_ip, gpointer handler_ip_end);
	gboolean (*run_filter) (StackFrameInfo *frame, MonoException *ex, int clause_index, gpointer handler_ip, gpointer handler_ip_end);
	void (*frame_iter_init) (MonoInterpStackIter *iter, gpointer interp_exit_data);
	gboolean (*frame_iter_next) (MonoInterpStackIter *iter, StackFrameInfo *frame);
	MonoJitInfo* (*find_jit_info) (MonoDomain *domain, MonoMethod *method);
	void (*set_breakpoint) (MonoJitInfo *jinfo, gpointer ip);
	void (*clear_breakpoint) (MonoJitInfo *jinfo, gpointer ip);
	MonoJitInfo* (*frame_get_jit_info) (MonoInterpFrameHandle frame);
	gpointer (*frame_get_ip) (MonoInterpFrameHandle frame);
	gpointer (*frame_get_arg) (MonoInterpFrameHandle frame, int pos);
	gpointer (*frame_get_local) (MonoInterpFrameHandle frame, int pos);
	gpointer (*frame_get_this) (MonoInterpFrameHandle frame);
	void (*frame_arg_to_data) (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer data);
	void (*data_to_frame_arg) (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer data);
	gpointer (*frame_arg_to_storage) (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index);
	void (*frame_arg_set_storage) (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer storage);
	MonoInterpFrameHandle (*frame_get_parent) (MonoInterpFrameHandle frame);
	void (*start_single_stepping) (void);
	void (*stop_single_stepping) (void);
} MonoEECallbacks;

// static const MonoEECallbacks foo_ee_callbacks = MONO_INIT_EE_CALLBACKS (foo);
#define MONO_INIT_EE_CALLBACKS(prefix) {           \
	prefix ## _entry_from_trampoline,          \
	prefix ## _to_native_trampoline,           \
	prefix ## _create_method_pointer,          \
	prefix ## _create_method_pointer_llvmonly, \
	prefix ## _runtime_invoke,                 \
	prefix ## _init_delegate,                  \
	prefix ## _delegate_ctor,                  \
	prefix ## _get_remoting_invoke,            \
	prefix ## _set_resume_state,               \
	prefix ## _run_finally,                    \
	prefix ## _run_filter,                     \
	prefix ## _frame_iter_init,                \
	prefix ## _frame_iter_next,                \
	prefix ## _find_jit_info,                  \
	prefix ## _set_breakpoint,                 \
	prefix ## _clear_breakpoint,               \
	prefix ## _frame_get_jit_info,             \
	prefix ## _frame_get_ip,                   \
	prefix ## _frame_get_arg,                  \
	prefix ## _frame_get_local,                \
	prefix ## _frame_get_this,                 \
	prefix ## _frame_arg_to_data,              \
	prefix ## _data_to_frame_arg,              \
	prefix ## _frame_arg_to_storage,           \
	prefix ## _frame_arg_set_storage,          \
	prefix ## _frame_get_parent,               \
	prefix ## _start_single_stepping,          \
	prefix ## _stop_single_stepping,           \
}

#endif /* __MONO_EE_H__ */
