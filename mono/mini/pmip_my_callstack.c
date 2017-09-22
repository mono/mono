#include "pmip_my_callstack.h"
#include "mono/metadata/mono-debug.h"

static char *
pmip_pretty(MonoMethod* method)
{
	char *lineNumber;
	char* filePath;
	char* methodName;
	char* assemblyName;
	char* formattedPMIP;
	MonoDebugSourceLocation* debugSourceLocation;
	MonoDebugMethodInfo* debugMethodInfo;
	MonoDomain *domain;

	domain = mono_domain_get();
	if (!domain)
		domain = mono_get_root_domain();

	methodName = mono_method_full_name(method, TRUE);

	debugSourceLocation = mono_debug_lookup_source_location(method, 0, domain);
	debugMethodInfo = mono_debug_lookup_method(method);

	assemblyName = method->klass->image->module_name;
	lineNumber = debugSourceLocation ? g_strdup_printf("%d", debugSourceLocation->row) : g_strdup("<UNKNOWN>");
	filePath = debugSourceLocation ? g_strdup(debugSourceLocation->source_file) : g_strdup("<UNKNOWN>");

	formattedPMIP = g_strdup_printf("[%s] %s Line %s File %s", assemblyName, methodName, lineNumber, filePath);

	mono_debug_free_source_location(debugSourceLocation);
	g_free(methodName);
	g_free(lineNumber);
	g_free(filePath);

	return formattedPMIP;
}

#if !defined(DISABLE_JIT) && defined(HOST_WIN32)

static gboolean enabled;
static mono_mutex_t mutex;
static FILE* fd;

#define pmip_my_callstack_lock() mono_os_mutex_lock (&mutex)
#define pmip_my_callstack_unlock() mono_os_mutex_unlock (&mutex)

void
mono_pmip_my_callstack_init (const char *options)
{
	char* file_name = g_strdup_printf("pmip.%d", GetCurrentProcessId());
	char* path = g_build_filename(g_get_tmp_dir(), file_name, NULL);

	mono_os_mutex_init_recursive(&mutex);

	fd = _fsopen(path, "w", _SH_DENYNO);

	g_free(file_name);
	g_free(path);

	if (fd)
		enabled = TRUE;
}

void
mono_pmip_my_callstack_save_method_info (MonoCompile *cfg)
{
	char* pretty_name;

	if (!enabled)
		return;

	pretty_name = pmip_pretty(cfg->method);

	pmip_my_callstack_lock ();
	fprintf(fd, "%p;%p;%s\n", cfg->native_code, ((char*)cfg->native_code) + cfg->code_size, pretty_name);
	fflush (fd);
	pmip_my_callstack_unlock ();

	g_free(pretty_name);
}

void
mono_pmip_my_callstack_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

void
mono_pmip_my_callstack_save_trampoline_info (MonoTrampInfo *info)
{
	if (!enabled)
		return;

	pmip_my_callstack_lock ();
	fprintf (fd, "%p;%p;%s\n", info->code, ((char*)info->code) + info->code_size, info->name ? info->name : "");
	fflush (fd);
	pmip_my_callstack_unlock ();
}

void
mono_pmip_my_callstack_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len)
{

}

#else

void
mono_pmip_my_callstack_init (const char *options)
{
	g_error ("Only Available On Windows With Jit Enabled");
}

void
mono_pmip_my_callstack_save_method_info (MonoCompile *cfg)
{
}

void
mono_pmip_my_callstack_save_trampoline_info (MonoTrampInfo *info)
{
}

void
mono_pmip_my_callstack_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

void
mono_pmip_my_callstack_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len)
{
}

#endif
