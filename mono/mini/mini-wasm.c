#include "mini.h"
#include "mini-runtime.h"
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/seq-points-data.h>
#include <mono/mini/aot-runtime.h>
#include <mono/mini/seq-points.h>

//XXX This is dirty, extend ee.h to support extracting info from MonoInterpFrameHandle
#include <mono/mini/interp/interp-internals.h>

#ifndef DISABLE_JIT

// include "ir-emit.h"
#include "cpu-wasm.h"

gboolean
mono_arch_have_fast_tls (void)
{
	return FALSE;
}

guint32
mono_arch_get_patch_offset (guint8 *code)
{
	g_error ("mono_arch_get_patch_offset");
	return 0;
}
gpointer
mono_arch_ip_from_context (void *sigctx)
{
	g_error ("mono_arch_ip_from_context");
}

gboolean
mono_arch_is_inst_imm (int opcode, int imm_opcode, gint64 imm)
{
	g_error ("mono_arch_is_inst_imm");
	return TRUE;
}

void
mono_arch_lowering_pass (MonoCompile *cfg, MonoBasicBlock *bb)
{
}

gboolean
mono_arch_opcode_supported (int opcode)
{
	return FALSE;
}

void
mono_arch_output_basic_block (MonoCompile *cfg, MonoBasicBlock *bb)
{
	g_error ("mono_arch_output_basic_block");
}

void
mono_arch_peephole_pass_1 (MonoCompile *cfg, MonoBasicBlock *bb)
{
}

void
mono_arch_peephole_pass_2 (MonoCompile *cfg, MonoBasicBlock *bb)
{
}

guint32
mono_arch_regalloc_cost (MonoCompile *cfg, MonoMethodVar *vmv)
{
	return 0;
}

GList *
mono_arch_get_allocatable_int_vars (MonoCompile *cfg)
{
	g_error ("mono_arch_get_allocatable_int_vars");
}

GList *
mono_arch_get_global_int_regs (MonoCompile *cfg)
{
	g_error ("mono_arch_get_global_int_regs");
}

void
mono_arch_allocate_vars (MonoCompile *cfg)
{
	g_error ("mono_arch_allocate_vars");
}

void
mono_arch_create_vars (MonoCompile *cfg)
{
	g_error ("mono_arch_create_vars");
}

void
mono_arch_emit_call (MonoCompile *cfg, MonoCallInst *call)
{
	g_error ("mono_arch_emit_call");
}

void
mono_arch_emit_epilog (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_epilog");
}

void
mono_arch_emit_exceptions (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_exceptions");
}

MonoInst*
mono_arch_emit_inst_for_method (MonoCompile *cfg, MonoMethod *cmethod, MonoMethodSignature *fsig, MonoInst **args)
{
	g_error ("mono_arch_emit_inst_for_method");
	return NULL;
}

void
mono_arch_emit_outarg_vt (MonoCompile *cfg, MonoInst *ins, MonoInst *src)
{
	g_error ("mono_arch_emit_outarg_vt");
}

guint8 *
mono_arch_emit_prolog (MonoCompile *cfg)
{
	g_error ("mono_arch_emit_prolog");
}

void
mono_arch_emit_setret (MonoCompile *cfg, MonoMethod *method, MonoInst *val)
{
	g_error ("mono_arch_emit_setret");
}

void
mono_arch_flush_icache (guint8 *code, gint size)
{
}

const char*
mono_arch_fregname (int reg)
{
	return "freg0";
}

const char*
mono_arch_regname (int reg)
{
	return "r0";
}

LLVMCallInfo*
mono_arch_get_llvm_call_info (MonoCompile *cfg, MonoMethodSignature *sig)
{
	g_error ("mono_arch_get_llvm_call_info");
}

gboolean
mono_arch_tailcall_supported (MonoCompile *cfg, MonoMethodSignature *caller_sig, MonoMethodSignature *callee_sig)
{
	g_error ("mono_arch_tailcall_supported");
	return FALSE;
}
#endif

int
mono_arch_get_argument_info (MonoMethodSignature *csig, int param_count, MonoJitArgumentInfo *arg_info)
{
	g_error ("mono_arch_get_argument_info");
}

GSList*
mono_arch_get_delegate_invoke_impls (void)
{
	g_error ("mono_arch_get_delegate_invoke_impls");
}

gpointer
mono_arch_get_gsharedvt_call_info (gpointer addr, MonoMethodSignature *normal_sig, MonoMethodSignature *gsharedvt_sig, gboolean gsharedvt_in, gint32 vcall_offset, gboolean calli)
{
	g_error ("mono_arch_get_gsharedvt_call_info");
	return NULL;
}

gpointer
mono_arch_get_delegate_invoke_impl (MonoMethodSignature *sig, gboolean has_target)
{
	g_error ("mono_arch_get_delegate_invoke_impl");
}

#ifdef HOST_WASM
#include <emscripten.h>
//functions exported to be used by JS
EMSCRIPTEN_KEEPALIVE void mono_set_timeout_exec (int id);
//JS functions imported that we use
extern void mono_set_timeout (int t, int d);
#endif

gpointer
mono_arch_get_this_arg_from_call (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_get_this_arg_from_call");
}

gpointer
mono_arch_get_delegate_virtual_invoke_impl (MonoMethodSignature *sig, MonoMethod *method, int offset, gboolean load_imt_reg)
{
	g_error ("mono_arch_get_delegate_virtual_invoke_impl");
}


void
mono_arch_cpu_init (void)
{
	// printf ("mono_arch_cpu_init\n");
}

void
mono_arch_finish_init (void)
{
	// printf ("mono_arch_finish_init\n");
}

void
mono_arch_init (void)
{
	// printf ("mono_arch_init\n");
}

void
mono_arch_cleanup (void)
{
}

void
mono_arch_register_lowlevel_calls (void)
{
}

void
mono_arch_flush_register_windows (void)
{
}

void
mono_arch_free_jit_tls_data (MonoJitTlsData *tls)
{
}


MonoMethod*
mono_arch_find_imt_method (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_find_static_call_vtable");
	return (MonoMethod*) regs [MONO_ARCH_IMT_REG];
}

MonoVTable*
mono_arch_find_static_call_vtable (mgreg_t *regs, guint8 *code)
{
	g_error ("mono_arch_find_static_call_vtable");
	return (MonoVTable*) regs [MONO_ARCH_RGCTX_REG];
}

gpointer
mono_arch_build_imt_trampoline (MonoVTable *vtable, MonoDomain *domain, MonoIMTCheckItem **imt_entries, int count, gpointer fail_tramp)
{
	g_error ("mono_arch_build_imt_trampoline");
}

guint32
mono_arch_cpu_enumerate_simd_versions (void)
{
	return 0;
}

guint32
mono_arch_cpu_optimizations (guint32 *exclude_mask)
{
	return 0;
}

mgreg_t
mono_arch_context_get_int_reg (MonoContext *ctx, int reg)
{
	g_error ("mono_arch_context_get_int_reg");
	return 0;
}

#ifdef HOST_WASM

void
mono_runtime_setup_stat_profiler (void)
{
	g_error ("mono_runtime_setup_stat_profiler");
}


void
mono_runtime_shutdown_stat_profiler (void)
{
	g_error ("mono_runtime_shutdown_stat_profiler");
}


gboolean
MONO_SIG_HANDLER_SIGNATURE (mono_chain_signal)
{
	g_error ("mono_chain_signal");
	
	return FALSE;
}

void
mono_runtime_install_handlers (void)
{
}

void
mono_runtime_cleanup_handlers (void)
{
}

gboolean
mono_thread_state_init_from_handle (MonoThreadUnwindState *tctx, MonoThreadInfo *info, void *sigctx)
{
	g_error ("WASM systems don't support mono_thread_state_init_from_handle");
	return FALSE;
}

EMSCRIPTEN_KEEPALIVE void
mono_set_timeout_exec (int id)
{
	ERROR_DECL (error);
	MonoClass *klass = mono_class_load_from_name (mono_defaults.corlib, "System.Threading", "WasmRuntime");
	g_assert (klass);

	MonoMethod *method = mono_class_get_method_from_name_checked (klass, "TimeoutCallback", -1, 0, error);
	mono_error_assert_ok (error);
	g_assert (method);

	gpointer params[1] = { &id };
	MonoObject *exc = NULL;

	mono_runtime_try_invoke (method, NULL, params, &exc, error);

	//YES we swallow exceptions cuz there's nothing much we can do from here.
	//FIXME Maybe call the unhandled exception function?
	if (!is_ok (error)) {
		printf ("timeout callback failed due to %s\n", mono_error_get_message (error));
		mono_error_cleanup (error);
	}

	if (exc) {
		char *type_name = mono_type_get_full_name (mono_object_get_class (exc));
		printf ("timeout callback threw a %s\n", type_name);
		g_free (type_name);
	}
}

void
mono_wasm_set_timeout (int timeout, int id)
{
	mono_set_timeout (timeout, id);
}

#endif

void
mono_arch_register_icall (void)
{
#ifdef HOST_WASM
	mono_add_internal_call ("System.Threading.WasmRuntime::SetTimeout", mono_wasm_set_timeout);
#endif
}

void
mono_arch_patch_code_new (MonoCompile *cfg, MonoDomain *domain, guint8 *code, MonoJumpInfo *ji, gpointer target)
{
	g_error ("mono_arch_patch_code_new");
}

#ifdef HOST_WASM

/*
The following functions don't belong here, but are due to laziness.
*/
gboolean mono_w32file_get_volume_information (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize);
void * getgrnam (const char *name);
void * getgrgid (gid_t gid);
int inotify_init (void);
int inotify_rm_watch (int fd, int wd);
int inotify_add_watch (int fd, const char *pathname, uint32_t mask);
int sem_timedwait (sem_t *sem, const struct timespec *abs_timeout);


//w32file-wasm.c
gboolean
mono_w32file_get_volume_information (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize)
{
	glong len;
	gboolean status = FALSE;

	gunichar2 *ret = g_utf8_to_utf16 ("memfs", -1, NULL, &len, NULL);
	if (ret != NULL && len < fsbuffersize) {
		memcpy (fsbuffer, ret, len * sizeof (gunichar2));
		fsbuffer [len] = 0;
		status = TRUE;
	}
	if (ret != NULL)
		g_free (ret);

	return status;
}


//llvm builtin's that we should not have used in the first place


//libc / libpthread missing bits from musl or shit we didn't detect :facepalm:
int pthread_getschedparam (pthread_t thread, int *policy, struct sched_param *param)
{
	g_error ("pthread_getschedparam");
	return 0;
}

int
pthread_setschedparam(pthread_t thread, int policy, const struct sched_param *param)
{
	return 0;
}


int
pthread_attr_getstacksize (const pthread_attr_t *restrict attr, size_t *restrict stacksize)
{
	return 65536; //wasm page size
}

int
pthread_sigmask (int how, const sigset_t * restrict set, sigset_t * restrict oset)
{
	return 0;
}


int
sigsuspend(const sigset_t *sigmask)
{
	g_error ("sigsuspend");
	return 0;
}

int
getdtablesize (void)
{
	return 256; //random constant that is the fd limit
}

void *
getgrnam (const char *name)
{
	return NULL;
}

void *
getgrgid (gid_t gid)
{
	return NULL;
}

int
inotify_init (void)
{
	g_error ("inotify_init");
}

int
inotify_rm_watch (int fd, int wd)
{
	g_error ("inotify_rm_watch");
	return 0;
}

int
inotify_add_watch (int fd, const char *pathname, uint32_t mask)
{
	g_error ("inotify_add_watch");
	return 0;
}

int
sem_timedwait (sem_t *sem, const struct timespec *abs_timeout)
{
	g_error ("sem_timedwait");
	return 0;
	
}
#endif