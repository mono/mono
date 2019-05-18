/**
 * \file
 * Handle the differences between the llvm backend beeing embedded
 * or loaded at runtime.
 */

#include "mini.h"
#include "mini-llvm.h"

#ifdef MONO_LLVM_LOADED

typedef struct {
	void (*init)(void);
	void (*cleanup)(void);
	void (*emit_method)(MonoCompile *cfg);
	void (*emit_call)(MonoCompile *cfg, MonoCallInst *call);
	void (*create_aot_module)(MonoAssembly *assembly, const char *global_prefix, int initial_got_size, LLVMModuleFlags flags);
	void (*fixup_aot_module)(void);
	void (*emit_aot_module)(const char *filename, const char *cu_name);
	void (*check_method_supported)(MonoCompile *cfg);
	void (*emit_aot_file_info)(MonoAotFileInfo *info, gboolean has_jitted_code);
	void (*emit_aot_data)(const char *symbol, guint8 *data, int data_len);
	void (*free_domain_info)(MonoDomain *domain);
	void (*create_vars)(MonoCompile *cfg);
} LoadedBackend;

static LoadedBackend backend;

static
void
mono_llvm_fatal (void)
{
	fprintf (stderr, "Error: Mono LLVM support could not be loaded.\n");
	exit (1);
}

#define mono_llvm_check(p) ((p) ? (p) : (mono_llvm_fatal (), NULL))

void
mono_llvm_init (void)
{
	mono_llvm_check (backend.init) ();
}

void
mono_llvm_cleanup (void)
{
	mono_llvm_check (backend.cleanup) ();
}

void
mono_llvm_emit_method (MonoCompile *cfg)
{
	mono_llvm_check (backend.emit_method) (cfg);
}

void
mono_llvm_emit_call (MonoCompile *cfg, MonoCallInst *call)
{
	mono_llvm_check (backend.emit_call) (cfg, call);
}

void
mono_llvm_create_aot_module (MonoAssembly *assembly, const char *global_prefix, int initial_got_size, LLVMModuleFlags flags)
{
	mono_llvm_check (backend.create_aot_module) (assembly, global_prefix, initial_got_size, flags);
}

void
mono_llvm_emit_aot_module (const char *filename, const char *cu_name)
{
	mono_llvm_check (backend.emit_aot_module) (filename, cu_name);
}

void
mono_llvm_fixup_aot_module (void)
{
	mono_llvm_check (backend.fixup_aot_module) ();
}

void
mono_llvm_check_method_supported (MonoCompile *cfg)
{
	mono_llvm_check (backend.check_method_supported) (cfg);
}

void
mono_llvm_free_domain_info (MonoDomain *domain)
{
	/* This is called even when llvm is not enabled */
	if (backend.free_domain_info)
		backend.free_domain_info (domain);
}

void
mono_llvm_emit_aot_file_info (MonoAotFileInfo *info, gboolean has_jitted_code)
{
	mono_llvm_check (backend.emit_aot_file_info) (info, has_jitted_code);
}

void
mono_llvm_emit_aot_data (const char *symbol, guint8 *data, int data_len)
{
	mono_llvm_check (backend.emit_aot_data) (symbol, data, data_len);
}

void
mono_llvm_create_vars (MonoCompile *cfg)
{
	mono_llvm_check (backend.create_vars) (cfg);
}

int
mono_llvm_load (const char* bpath)
{
	char *err = NULL;
	MonoDl *llvm_lib = mono_dl_open_runtime_lib ("mono-llvm", MONO_DL_LAZY, &err);

	if (!llvm_lib) {
		g_warning ("llvm load failed: %s\n", err);
		g_free (err);
		return FALSE;
	}

	err = mono_dl_symbol (llvm_lib, "mono_llvm_init", (void**)&backend.init);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_cleanup", (void**)&backend.cleanup);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_emit_method", (void**)&backend.emit_method);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_emit_call", (void**)&backend.emit_call);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_create_aot_module", (void**)&backend.create_aot_module);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_emit_aot_module", (void**)&backend.emit_aot_module);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_fixup_aot_module", (void**)&backend.fixup_aot_module);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_check_method_supported", (void**)&backend.check_method_supported);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_free_domain_info", (void**)&backend.free_domain_info);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_emit_aot_file_info", (void**)&backend.emit_aot_file_info);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_emit_aot_data", (void**)&backend.emit_aot_data);
	if (err) goto symbol_error;
	err = mono_dl_symbol (llvm_lib, "mono_llvm_create_vars", (void**)&backend.create_vars);
	if (err) goto symbol_error;
	return TRUE;
symbol_error:
	g_warning ("llvm symbol load failed: %s\n", err);
	g_free (err);
	return FALSE;
}

#else

int
mono_llvm_load (const char* bpath)
{
	return TRUE;
}

#endif /* MONO_LLVM_LOADED */

