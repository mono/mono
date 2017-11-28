#include "mixed_callstack_plugin.h"
#include "mono/metadata/mono-debug.h"
#include "mono/metadata/profiler.h"

static char *
pmip_pretty(MonoCompile* monoCompile)
{
	char* methodName;
	char* assemblyName;
	char* formattedPMIP;
	MonoDebugSourceLocation* debugSourceLocation;
	MonoDebugMethodInfo* debugMethodInfo;
	MonoDomain* domain;
	MonoMethod* method = monoCompile->method; 

	domain = mono_domain_get();
	if (!domain)
		domain = mono_get_root_domain();

	methodName = mono_method_full_name(method, TRUE);

	debugSourceLocation = mono_debug_lookup_source_location(method, 0, domain);
	debugMethodInfo = mono_debug_lookup_method(method);

	assemblyName = method->klass->image->module_name;

	formattedPMIP = g_strdup_printf("[%s] %s", assemblyName, methodName);

	mono_debug_free_source_location(debugSourceLocation);
	g_free(methodName);

	return formattedPMIP;
}

#if !defined(DISABLE_JIT) && defined(HOST_WIN32)

static gboolean enabled;
static mono_mutex_t mutex;
static HANDLE fileHandle;
int pmipFileNum;

#define mixed_callstack_plugin_lock() mono_os_mutex_lock (&mutex)
#define mixed_callstack_plugin_unlock() mono_os_mutex_unlock (&mutex)

void
create_next_pmip_file()
{
	char* file_name = g_strdup_printf("pmip_%d_%d.txt", GetCurrentProcessId(), pmipFileNum++);
	char* path = g_build_filename(g_get_tmp_dir(), file_name, NULL);
	char* version = "UnityMixedCallstacks:1.0\n";
	long bytesWritten = 0;

	mixed_callstack_plugin_lock ();

	if(fileHandle)
		CloseHandle(fileHandle);

	fileHandle = CreateFileA(path,
							GENERIC_WRITE,
							FILE_SHARE_DELETE | FILE_SHARE_READ,
							NULL,
							CREATE_ALWAYS,
							FILE_FLAG_DELETE_ON_CLOSE,
							NULL);

	if (INVALID_HANDLE_VALUE != fileHandle)
		enabled = TRUE;

	WriteFile(fileHandle, version, strlen(version), &bytesWritten, NULL);

	mixed_callstack_plugin_unlock ();

	g_free(file_name);
	g_free(path);
}

void
mixed_callstack_plugin_init (const char *options)
{
	pmipFileNum = 0;

	mono_os_mutex_init_recursive(&mutex);

	MonoProfilerHandle prof = mono_profiler_create(NULL);
	mono_profiler_set_domain_unloaded_callback(prof, mixed_callstack_plugin_on_domain_unload_end);

	create_next_pmip_file();
}

void
mixed_callstack_plugin_on_domain_unload_end()
{
	if(!enabled)
		return;

	create_next_pmip_file();
}

void
mixed_callstack_plugin_save_method_info (MonoCompile *cfg)
{
	char* pretty_name;
	char* frame;
	long bytesWritten = 0;

	if (!enabled)
		return;

	pretty_name = pmip_pretty(cfg);

	mixed_callstack_plugin_lock ();
	frame = g_strdup_printf("%p;%p;%s\n", cfg->native_code, ((char*)cfg->native_code) + cfg->code_size, pretty_name);
	WriteFile(fileHandle, frame, strlen(frame), &bytesWritten, NULL);
	FlushFileBuffers(fileHandle);

	mixed_callstack_plugin_unlock ();

	g_free(pretty_name);
	g_free(frame);
}

void
mixed_callstack_plugin_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

void
mixed_callstack_plugin_save_trampoline_info (MonoTrampInfo *info)
{
	char* frame;
	long bytesWritten = 0;

	if (!enabled)
		return;

	mixed_callstack_plugin_lock ();
	frame = g_strdup_printf ("%p;%p;%s\n", info->code, ((char*)info->code) + info->code_size, info->name ? info->name : "");
	WriteFile(fileHandle, frame, strlen(frame), &bytesWritten, NULL);
	FlushFileBuffers(fileHandle);
	mixed_callstack_plugin_unlock ();

	g_free(frame);
}

void
mixed_callstack_plugin_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len)
{

}

#else

void
mixed_callstack_plugin_init (const char *options)
{
	g_error ("Only Available On Windows With Jit Enabled");
}

void
mixed_callstack_plugin_on_domain_unload_end()
{
}

void
mixed_callstack_plugin_save_method_info (MonoCompile *cfg)
{
}

void
mixed_callstack_plugin_save_trampoline_info (MonoTrampInfo *info)
{
}

void
mixed_callstack_plugin_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

void
mixed_callstack_plugin_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len)
{
}

#endif
