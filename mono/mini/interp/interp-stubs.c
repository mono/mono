#include <config.h>

#ifndef ENABLE_INTERPRETER

#include <interp/interp.h>

/* Dummy versions of interpreter functions to avoid ifdefs at call sites */

MonoJitInfo*
mono_interp_find_jit_info (MonoDomain *domain, MonoMethod *method)
{
	return NULL;
}

void
mono_interp_set_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	g_assert_not_reached ();
}

void
mono_interp_clear_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	g_assert_not_reached ();
}

MonoJitInfo*
mono_interp_frame_get_jit_info (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

gpointer
mono_interp_frame_get_ip (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

gpointer
mono_interp_frame_get_arg (MonoInterpFrameHandle frame, int pos)
{
	g_assert_not_reached ();
	return NULL;
}

gpointer
mono_interp_frame_get_local (MonoInterpFrameHandle frame, int pos)
{
	g_assert_not_reached ();
	return NULL;
}

gpointer
mono_interp_frame_get_this (MonoInterpFrameHandle frame)
{
	g_assert_not_reached ();
	return NULL;
}

void
mono_interp_start_single_stepping (void)
{
	g_assert_not_reached ();
}

void
mono_interp_stop_single_stepping (void)
{
	g_assert_not_reached ();
}

#endif

