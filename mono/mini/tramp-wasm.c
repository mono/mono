#include "mini.h"

void wasm_interp_to_native_trampoline (void *target_func, InterpMethodArguments *margs);
void mono_sdb_single_step_trampoline (void);
void mono_sdb_single_step_trampoline (void);

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

gpointer
mono_arch_create_specific_trampoline (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, guint32 *code_len)
{
	g_error ("mono_arch_create_specific_trampoline");
}

guchar*
mono_arch_create_generic_trampoline (MonoTrampolineType tramp_type, MonoTrampInfo **info, gboolean aot)
{
	g_error ("mono_arch_create_generic_trampoline");
}

gpointer
mono_arch_create_rgctx_lazy_fetch_trampoline (guint32 slot, MonoTrampInfo **info, gboolean aot)
{
	g_error ("mono_arch_create_rgctx_lazy_fetch_trampoline");
}

void
mono_arch_patch_plt_entry (guint8 *code, gpointer *got, mgreg_t *regs, guint8 *addr)
{
	g_error ("mono_arch_patch_plt_entry");
}

void
mono_arch_patch_callsite (guint8 *method_start, guint8 *orig_code, guint8 *addr)
{
	g_error ("mono_arch_patch_callsite");
}

gpointer
mono_arch_get_unbox_trampoline (MonoMethod *m, gpointer addr)
{
	g_error ("mono_arch_get_unbox_trampoline");
	return NULL;
}

gpointer
mono_arch_get_static_rgctx_trampoline (gpointer arg, gpointer addr)
{
	g_error ("mono_arch_get_static_rgctx_trampoline");
	return NULL;
}

gpointer
mono_arch_get_interp_to_native_trampoline (MonoTrampInfo **info)
{
	return create_tramp_info (wasm_interp_to_native_trampoline, "interp_to_native_trampoline", info);
}

guint8*
mono_arch_create_sdb_trampoline (gboolean single_step, MonoTrampInfo **info, gboolean aot)
{
	g_assert (!aot);
	if (single_step)
		return create_tramp_info (mono_sdb_single_step_trampoline, "sdb_single_step_trampoline", info);
	else
		return create_tramp_info (mono_wasm_breakpoint_hit, "sdb_breakpoint_trampoline", info);
}
