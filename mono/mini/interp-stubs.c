#include <config.h>

#include "interp/interp.h"

/* interpreter callback stubs */

static MonoJitInfo*
stub_find_jit_info (MonoDomain *domain, MonoMethod *method)
{
	return NULL;
}

static void
stub_set_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	g_assert_not_reached ();
}

static void
stub_clear_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	g_assert_not_reached ();
}

static MonoJitInfo*
stub_frame_get_jit_info (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

static gpointer
stub_frame_get_ip (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

static gpointer
stub_frame_get_arg (MonoInterpFrameHandle frame, int pos)
{
	g_assert_not_reached ();
	return NULL;
}

static gpointer
stub_frame_get_local (MonoInterpFrameHandle frame, int pos)
{
	g_assert_not_reached ();
	return NULL;
}

static gpointer
stub_frame_get_this (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

static MonoInterpFrameHandle
stub_frame_get_parent (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

static void
stub_start_single_stepping (void)
{
}

static void
stub_stop_single_stepping (void)
{
}

static void
stub_set_resume_state (MonoJitTlsData *jit_tls, MonoException *ex, MonoJitExceptionInfo *ei, MonoInterpFrameHandle interp_frame, gpointer handler_ip)
{
	g_assert_not_reached ();
}

static gboolean
stub_run_finally (StackFrameInfo *frame, int clause_index, gpointer handler_ip, gpointer handler_ip_end)
{
	g_assert_not_reached ();
}

static gboolean
stub_run_filter (StackFrameInfo *frame, MonoException *ex, int clause_index, gpointer handler_ip, gpointer handler_ip_end)
{
	g_assert_not_reached ();
	return FALSE;
}

static void
stub_frame_iter_init (MonoInterpStackIter *iter, gpointer interp_exit_data)
{
	g_assert_not_reached ();
}

static gboolean
stub_frame_iter_next (MonoInterpStackIter *iter, StackFrameInfo *frame)
{
	g_assert_not_reached ();
	return FALSE;
}

static gpointer
stub_create_method_pointer (MonoMethod *method, gboolean compile, MonoError *error)
{
	g_assert_not_reached ();
	return NULL;
}

static MonoFtnDesc*
stub_create_method_pointer_llvmonly (MonoMethod *method, gboolean compile, MonoError *error)
{
	g_assert_not_reached ();
	return NULL;
}

static MonoObject*
stub_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error)
{
	g_assert_not_reached ();
	return NULL;
}

static void
stub_init_delegate (MonoDelegate *del, MonoError *error)
{
	g_assert_not_reached ();
}

static gpointer
stub_get_remoting_invoke (MonoMethod *method, gpointer imethod, MonoError *error)
{
	g_assert_not_reached ();
	return NULL;
}

static void
stub_delegate_ctor (MonoObjectHandle this_obj, MonoObjectHandle target, gpointer addr, MonoError *error)
{
	g_assert_not_reached ();
}

void
mono_interp_stub_init (void)
{
	if (mini_get_interp_callbacks ())
		/* already initialized */
		return;

#define stub_entry_from_trampoline NULL
#define stub_to_native_trampoline NULL
#define stub_frame_arg_to_data NULL
#define stub_data_to_frame_arg NULL
#define stub_frame_arg_to_storage NULL
#define stub_frame_arg_set_storage NULL

	static const MonoEECallbacks c = MONO_INIT_EE_CALLBACKS (stub);

	mini_install_interp_callbacks (&c);
}
