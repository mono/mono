#include "mixed_callstack_plugin.h"
#include "mono/metadata/mono-debug.h"
#include "mono/metadata/profiler.h"
#include "mono/mini/seq-points.h"

#if !defined(DISABLE_JIT) && defined(HOST_WIN32)

static gboolean enabled;
static mono_mutex_t mutex;
static GPtrArray* domainHandles;
static gboolean legacyMode;

#define mixed_callstack_plugin_lock() mono_os_mutex_lock (&mutex)
#define mixed_callstack_plugin_unlock() mono_os_mutex_unlock (&mutex)

static void mixed_callstack_plugin_on_domain_unload_end (MonoProfiler *prof, MonoDomain *domain);

typedef struct {
	HANDLE fileHandle;
	gint32 domain_id;
	gint32 count;
} DomainHandle;

DomainHandle*
get_domain_handle(gint32 domain_id)
{
	DomainHandle* dHandle = NULL;
	domain_id = legacyMode ? mono_get_root_domain()->domain_id : domain_id;
	for (int i = 0; i < domainHandles->len; i++)
	{
		if (((DomainHandle*)domainHandles->pdata[i])->domain_id == domain_id)
		{
			dHandle = (DomainHandle*)domainHandles->pdata[i];
			break;
		}
	}

	return dHandle;
}

DomainHandle*
create_next_pmip_file(gint32 domain_id)
{
	char* file_name;
	char* path;
	char* version = "UnityMixedCallstacks:2.0\n";
	long bytesWritten = 0;
	int pmipFileNum = 1;

	mixed_callstack_plugin_lock ();

	HANDLE fileHandle = NULL;
	DomainHandle* dHandle = get_domain_handle(domain_id);

	if (dHandle)
	{
		fileHandle = dHandle->fileHandle;
		pmipFileNum = ++dHandle->count;
	}
	if (legacyMode)
	{
		version = "UnityMixedCallstacks:1.0\n";
		file_name = g_strdup_printf("pmip_%d_%d.txt", GetCurrentProcessId(), pmipFileNum);
	}
	else
		file_name = g_strdup_printf("pmip_%d_%d_%d.txt", GetCurrentProcessId(), pmipFileNum, domain_id);
	path = path = g_build_filename(g_get_tmp_dir(), file_name, NULL);

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
	
	if (dHandle)
		dHandle->fileHandle = fileHandle;
	else
	{
		dHandle = g_new0(DomainHandle, 1);
		dHandle->domain_id = domain_id;
		dHandle->fileHandle = fileHandle;
		dHandle->count = pmipFileNum;
		g_ptr_array_add(domainHandles, dHandle);
	}

	mixed_callstack_plugin_unlock ();

	g_free(file_name);
	g_free(path);
	return dHandle;
}

void
mixed_callstack_plugin_init (const guint options, MonoDomain *domain)
{
	mono_os_mutex_init_recursive(&mutex);

	MonoProfilerHandle prof = mono_profiler_create(NULL);
	mono_profiler_set_domain_unloaded_callback(prof, mixed_callstack_plugin_on_domain_unload_end);

	// TODO: Clean this thing up somewhere on close
	domainHandles = g_ptr_array_new();

	// 1 is legacy 2 is file per domain
	legacyMode = options == 1;
	create_next_pmip_file(domain->domain_id);
}

void
mixed_callstack_plugin_on_domain_unload_end(MonoProfiler *prof, MonoDomain *domain)
{
	if(!enabled)
		return;

	create_next_pmip_file(domain->domain_id);
}

void
mixed_callstack_plugin_save_method_info (MonoCompile *cfg)
{
	char* method_name;
	long bytesWritten = 0;
	char frame[1024];
	int bytes;
	MonoSeqPointInfo* seq_points;
	gboolean wroteSomething = FALSE;

	if (!enabled)
		return;

	DomainHandle* domain_handle = get_domain_handle(cfg->domain->domain_id);
	if (!domain_handle)
		domain_handle = create_next_pmip_file(cfg->domain->domain_id);

	method_name = mono_method_full_name (cfg->method, TRUE);

	// No need to fetch sequence points if we're in legacy mode
	seq_points = legacyMode ? NULL : mono_get_seq_points (cfg->domain, cfg->method);

	// we can dump line numbers if we successfully access seq points
	if (seq_points)
	{
		// First dump range so we still have old value just in case we screw up in the plugin
		bytes = snprintf(frame, sizeof(frame), "---%p;%p;[%s] %s\n", cfg->native_code, ((char*)cfg->native_code) + cfg->code_size, cfg->method->klass->image->module_name, method_name);
		/* negative value is encoding error */
		if (bytes < 0 || bytes > sizeof(frame))
			return;

		mixed_callstack_plugin_lock();
		WriteFile(domain_handle->fileHandle, frame, bytes, &bytesWritten, NULL);
		mixed_callstack_plugin_unlock();

		SeqPointIterator it;
		MonoDebugSourceLocation* location;
		mono_seq_point_iterator_init(&it, seq_points);
		while (mono_seq_point_iterator_next(&it))
		{
			location = mono_debug_lookup_source_location(cfg->method, it.seq_point.native_offset, cfg->domain);
			
			if (!location)
				continue;

			bytes = snprintf(frame, sizeof(frame), "%p;%p;[%s] %s : %d;%s\n", cfg->native_code + it.seq_point.native_offset, cfg->native_code + it.seq_point.native_offset, cfg->method->klass->image->module_name, method_name, location->row, location->source_file);

			/* negative value is encoding error */
			if (bytes < 0 || bytes > sizeof(frame))
				return;

			mixed_callstack_plugin_lock();
			WriteFile(domain_handle->fileHandle, frame, bytes, &bytesWritten, NULL);
			mixed_callstack_plugin_unlock();
			mono_debug_free_source_location(location);
			wroteSomething = TRUE;
		}
	}
	if (!wroteSomething)
	{
		// Old behavior
		bytes = snprintf(frame, sizeof(frame), "%p;%p;[%s] %s\n", cfg->native_code, ((char*)cfg->native_code) + cfg->code_size, cfg->method->klass->image->module_name, method_name);
		/* negative value is encoding error */
		if (bytes < 0 || bytes > sizeof(frame))
			return;

		mixed_callstack_plugin_lock();
		WriteFile(domain_handle->fileHandle, frame, bytes, &bytesWritten, NULL);
		mixed_callstack_plugin_unlock();
	}

	g_free(method_name);
}

void
mixed_callstack_plugin_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

void
mixed_callstack_plugin_save_trampoline_info (MonoTrampInfo *info, MonoDomain *domain)
{
	char* frame;
	long bytesWritten = 0;

	if (!enabled)
		return;

	DomainHandle* domain_handle = get_domain_handle(domain->domain_id);
	if (!domain_handle)
		domain_handle = create_next_pmip_file(domain->domain_id);

	mixed_callstack_plugin_lock ();
	frame = g_strdup_printf ("%p;%p;%s\n", info->code, ((char*)info->code) + info->code_size, info->name ? info->name : "");
	WriteFile(domain_handle->fileHandle, frame, strlen(frame), &bytesWritten, NULL);
	mixed_callstack_plugin_unlock ();

	g_free(frame);
}

void
mixed_callstack_plugin_save_specific_trampoline_info (gpointer arg1, MonoTrampolineType tramp_type, MonoDomain *domain, gpointer code, guint32 code_len)
{

}

#else

void
mixed_callstack_plugin_init (const guint options, MonoDomain *domain)
{
	g_error ("Only Available On Windows With Jit Enabled");
}

void
mixed_callstack_plugin_on_domain_unload_end(MonoProfiler *prof, MonoDomain *domain)
{
}

void
mixed_callstack_plugin_save_method_info (MonoCompile *cfg)
{
}

void
mixed_callstack_plugin_save_trampoline_info (MonoTrampInfo *info, MonoDomain *domain)
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
