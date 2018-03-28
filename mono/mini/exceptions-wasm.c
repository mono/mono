#include "mini.h"

static gpointer
create_tramp_info (gpointer code, const char *name, MonoTrampInfo **info)
{
	if (info) {
		MonoTrampInfo *tinfo = g_new0 (MonoTrampInfo, 1);
		tinfo->code = code;
		tinfo->code_size = 1;
		tinfo->name = g_strdup (name);
		tinfo->ji = NULL;
		tinfo->unwind_ops = NULL;
		tinfo->uw_info = NULL;
		tinfo->uw_info_len = 0;
		tinfo->owns_uw_info = FALSE;

		*info = tinfo;
	}
	return code;
}

static void
wasm_restore_context (void)
{
	g_error ("wasm_restore_context");
}

static void
wasm_call_filter (void)
{
	g_error ("wasm_call_filter");
}

static void
wasm_throw_exception (void)
{
	g_error ("wasm_throw_exception");
}

static void
wasm_rethrow_exception (void)
{
	g_error ("wasm_rethrow_exception");
}

static void
wasm_throw_corlib_exception (void)
{
	g_error ("wasm_throw_corlib_exception");
}

gboolean
mono_arch_unwind_frame (MonoDomain *domain, MonoJitTlsData *jit_tls, 
							 MonoJitInfo *ji, MonoContext *ctx, 
							 MonoContext *new_ctx, MonoLMF **lmf,
							 mgreg_t **save_locations,
							 StackFrameInfo *frame)
{
	if (ji)
		g_error ("Can't unwind compiled code");

	if (*lmf) {
		if ((*lmf)->top_entry)
			return FALSE;
		g_error ("Can't handle non-top-entry LMFs\n");
	}

	return FALSE;
}

gpointer
mono_arch_get_call_filter (MonoTrampInfo **info, gboolean aot)
{
	return create_tramp_info (wasm_call_filter, "call_filter", info);
}

gpointer
mono_arch_get_restore_context (MonoTrampInfo **info, gboolean aot)
{
	return create_tramp_info (wasm_restore_context, "restore_context", info);
}
gpointer 
mono_arch_get_throw_corlib_exception (MonoTrampInfo **info, gboolean aot)
{
	return create_tramp_info (wasm_throw_corlib_exception, "throw_corlib_exception", info);
}

gpointer
mono_arch_get_rethrow_exception (MonoTrampInfo **info, gboolean aot)
{
	return create_tramp_info (wasm_rethrow_exception, "rethrow_exception", info);
}

gpointer
mono_arch_get_throw_exception (MonoTrampInfo **info, gboolean aot)
{
	return create_tramp_info (wasm_throw_exception, "throw_exception", info);
}

void
mono_arch_undo_ip_adjustment (MonoContext *ctx)
{
}
