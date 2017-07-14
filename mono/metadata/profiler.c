/*
 * Licensed to the .NET Foundation under one or more agreements.
 * The .NET Foundation licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
 */

#include <mono/metadata/assembly.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/mono-config-dirs.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/profiler-private.h>
#include <mono/utils/mono-dl.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/mono-logger-internals.h>

MonoProfilerState mono_profiler_state;

typedef void (*MonoProfilerInitializer) (const char *);

#define OLD_INITIALIZER_NAME "mono_profiler_startup"
#define NEW_INITIALIZER_NAME "mono_profiler_init"

static gboolean
load_profiler (MonoDl *module, const char *desc, const char *suffix)
{
	if (!module)
		return FALSE;

	char *old_name;

	if (suffix)
		old_name = g_strdup_printf (OLD_INITIALIZER_NAME "_%s", suffix);
	else
		old_name = g_strdup_printf (OLD_INITIALIZER_NAME);

	MonoProfilerInitializer func;

	char *err;

	if (!(err = mono_dl_symbol (module, old_name, (gpointer) &func))) {
		mono_profiler_printf_err ("Found old-style startup symbol %s for %s; profiler has not been migrated to the new API.", old_name, desc);
		g_free (old_name);
		return FALSE;
	}

	g_free (err);
	g_free (old_name);

	char *new_name;

	if (suffix)
		new_name = g_strdup_printf (NEW_INITIALIZER_NAME "_%s", suffix);
	else
		new_name = g_strdup_printf (NEW_INITIALIZER_NAME);

	if ((err = mono_dl_symbol (module, new_name, (gpointer *) &func))) {
		g_free (err);
		g_free (new_name);
		return FALSE;
	}

	g_free (new_name);

	func (desc);

	return TRUE;
}

static gboolean
load_profiler_from_executable (const char *desc, const char *name)
{
	char *err;

	/*
	 * Some profilers (such as ours) may need to call back into the runtime
	 * from their sampling callback (which is called in async-signal context).
	 * They need to be able to know that all references back to the runtime
	 * have been resolved; otherwise, calling runtime functions may result in
	 * invoking the dynamic linker which is not async-signal-safe. Passing
	 * MONO_DL_EAGER will ask the dynamic linker to resolve everything upfront.
	 */
	MonoDl *module = mono_dl_open (NULL, MONO_DL_EAGER, &err);

	if (!module) {
		mono_profiler_printf_err ("Could not open main executable: %s", err);
		g_free (err);
		return FALSE;
	}

	return load_profiler (module, desc, name);
}

static gboolean
load_profiler_from_directory (const char *directory, const char *libname, const char *desc)
{
	char* path;
	void *iter = NULL;

	while ((path = mono_dl_build_path (directory, libname, &iter))) {
		// See the comment in load_embedded_profiler ().
		MonoDl *module = mono_dl_open (path, MONO_DL_EAGER, NULL);

		g_free (path);

		if (module)
			return load_profiler (module, desc, NULL);
	}

	return FALSE;
}

static gboolean
load_profiler_from_installation (const char *libname, const char *desc)
{
	char *err;
	MonoDl *module = mono_dl_open_runtime_lib (libname, MONO_DL_EAGER, &err);

	g_free (err);

	if (module)
		return load_profiler (module, desc, NULL);

	return FALSE;
}

void
mono_profiler_load (const char *desc)
{
	mono_gc_base_init ();

	if (!desc || !strcmp ("default", desc))
		desc = "log:report";

	const char *col = strchr (desc, ':');
	char *mname;

	if (col != NULL) {
		mname = (char *) g_memdup (desc, col - desc + 1);
		mname [col - desc] = 0;
	} else
		mname = g_strdup (desc);

	if (!load_profiler_from_executable (desc, mname)) {
		char *libname = g_strdup_printf ("mono-profiler-%s", mname);
		gboolean res = load_profiler_from_installation (libname, desc);

		if (!res && mono_config_get_assemblies_dir ())
			res = load_profiler_from_directory (mono_assembly_getrootdir (), libname, desc);

		if (!res)
			res = load_profiler_from_directory (NULL, libname, desc);

		if (!res)
			mono_profiler_printf_err ("The '%s' profiler wasn't found in the main executable nor could it be loaded from '%s'.", mname, libname);

		g_free (libname);
	}

	g_free (mname);
}

MonoProfilerHandle
mono_profiler_install (MonoProfiler *prof)
{
	MonoProfilerHandle handle = g_new0 (struct _MonoProfilerDesc, 1);

	handle->prof = prof;
	handle->next = mono_profiler_state.profilers;

	mono_profiler_state.profilers = handle;

	return handle;
}

void
mono_profiler_set_coverage_filter_callback (MonoProfilerHandle handle, MonoProfilerCoverageFilterCallback cb)
{
	InterlockedWritePointer (&handle->coverage_filter, (gpointer) cb);
}

static void
initialize_coverage (void)
{
	mono_os_mutex_init (&mono_profiler_state.coverage_mutex);
	mono_profiler_state.coverage_hash = g_hash_table_new (NULL, NULL);
}

static void
lazy_initialize_coverage (void)
{
	mono_lazy_initialize (&mono_profiler_state.coverage_status, initialize_coverage);
}

static void
coverage_lock (void)
{
	mono_os_mutex_lock (&mono_profiler_state.coverage_mutex);
}

static void
coverage_unlock (void)
{
	mono_os_mutex_unlock (&mono_profiler_state.coverage_mutex);
}

void
mono_profiler_get_coverage_data (MonoProfilerHandle handle, MonoMethod *method, MonoProfilerCoverageCallback cb)
{
	lazy_initialize_coverage ();

	coverage_lock ();

	MonoProfilerCoverageInfo *info = g_hash_table_lookup (mono_profiler_state.coverage_hash, method);

	coverage_unlock ();

	if (!info)
		return;

	MonoError error;
	MonoMethodHeader *header = mono_method_get_header_checked (method, &error);
	mono_error_assert_ok (&error);

	guint32 size;

	const unsigned char *start = mono_method_header_get_code (header, &size, NULL);
	const unsigned char *end = start - size;
	MonoDebugMethodInfo *minfo = mono_debug_lookup_method (method);

	for (guint32 i = 0; i < info->entries; i++) {
		guchar *cil_code = info->data [i].cil_code;

		if (cil_code && cil_code >= start && cil_code < end) {
			guint32 offset = cil_code - start;

			MonoProfilerCoverageData data = {
				.method = method,
				.il_offset = offset,
				.counter = info->data [i].count,
				.line = 1,
				.column = 1,
			};

			if (minfo) {
				MonoDebugSourceLocation *loc = mono_debug_method_lookup_location (minfo, offset);

				if (loc) {
					data.file_name = g_strdup (loc->source_file);
					data.line = loc->row;
					data.column = loc->column;

					mono_debug_free_source_location (loc);
				}
			}

			cb (handle->prof, &data);

			g_free ((char *) data.file_name);
		}
	}

	mono_metadata_free_mh (header);
}

MonoProfilerCoverageInfo *
mono_profiler_coverage_alloc (MonoMethod *method, guint32 entries)
{
	lazy_initialize_coverage ();

	gboolean cover = FALSE;

	for (MonoProfilerHandle handle = mono_profiler_state.profilers; handle; handle = handle->next) {
		MonoProfilerCoverageFilterCallback cb = handle->coverage_filter;

		if (cb)
			cover |= cb (handle->prof, method);
	}

	if (!cover)
		return NULL;

	coverage_lock ();

	MonoProfilerCoverageInfo *info = g_malloc0 (sizeof (MonoProfilerCoverageInfo) + SIZEOF_VOID_P * 2 * entries);

	info->entries = entries;

	g_hash_table_insert (mono_profiler_state.coverage_hash, method, info);

	coverage_unlock ();

	return info;
}

void
mono_profiler_coverage_free (MonoMethod *method)
{
	lazy_initialize_coverage ();

	coverage_lock ();

	MonoProfilerCoverageInfo *info = g_hash_table_lookup (mono_profiler_state.coverage_hash, method);

	if (info) {
		g_hash_table_remove (mono_profiler_state.coverage_hash, method);
		g_free (info);
	}

	coverage_unlock ();
}

mono_bool
mono_profiler_enable_sampling (MonoProfilerHandle handle)
{
	if (mono_profiler_state.startup_done)
		return FALSE;

	if (mono_profiler_state.sampling_owner)
		return TRUE;

	mono_profiler_state.sampling_owner = handle;
	mono_profiler_state.sample_mode = MONO_PROFILER_SAMPLE_MODE_NONE;
	mono_profiler_state.sample_freq = 100;
	mono_os_sem_init (&mono_profiler_state.sampling_semaphore, 0);

	return TRUE;
}

mono_bool
mono_profiler_set_sample_mode (MonoProfilerHandle handle, MonoProfilerSampleMode mode, uint64_t freq)
{
	if (handle != mono_profiler_state.sampling_owner)
		return FALSE;

	mono_profiler_state.sample_mode = mode;
	mono_profiler_state.sample_freq = freq;

	mono_profiler_sampling_thread_post ();

	return TRUE;
}

mono_bool
mono_profiler_get_sample_mode (MonoProfilerHandle handle, MonoProfilerSampleMode *mode, uint64_t *freq)
{
	if (mode)
		*mode = mono_profiler_state.sample_mode;

	if (freq)
		*freq = mono_profiler_state.sample_freq;

	return handle == mono_profiler_state.sampling_owner;
}

gboolean
mono_profiler_sampling_enabled (void)
{
	return !!mono_profiler_state.sampling_owner;
}

void
mono_profiler_sampling_thread_post (void)
{
	mono_os_sem_post (&mono_profiler_state.sampling_semaphore);
}

void
mono_profiler_sampling_thread_wait (void)
{
	mono_os_sem_wait (&mono_profiler_state.sampling_semaphore, MONO_SEM_FLAGS_NONE);
}

mono_bool
mono_profiler_enable_allocations (void)
{
	if (mono_profiler_state.startup_done)
		return FALSE;

	mono_profiler_state.allocations = TRUE;

	return TRUE;
}

void
mono_profiler_set_call_instrumentation_filter_callback (MonoProfilerHandle handle, MonoProfilerCallInstrumentationFilterCallback cb)
{
	InterlockedWritePointer (&handle->call_instrumentation_filter, (gpointer) cb);
}

gboolean
mono_profiler_should_instrument_method (MonoMethod *method, gboolean entry)
{
	MonoProfilerCallInstrumentationFlags flags = MONO_PROFILER_CALL_INSTRUMENTATION_NONE;

	for (MonoProfilerHandle handle = mono_profiler_state.profilers; handle; handle = handle->next) {
		MonoProfilerCallInstrumentationFilterCallback cb = handle->call_instrumentation_filter;

		if (cb)
			flags |= cb (handle->prof, method);
	}

	if (entry)
		return flags & MONO_PROFILER_CALL_INSTRUMENTATION_PROLOGUE;
	else
		return flags & MONO_PROFILER_CALL_INSTRUMENTATION_EPILOGUE;
}

void
mono_profiler_started (void)
{
	mono_profiler_state.startup_done = TRUE;
}

void
mono_profiler_cleanup (void)
{
	for (MonoProfilerHandle handle = mono_profiler_state.profilers; handle; handle = handle->next) {
#define _MONO_PROFILER_EVENT(name) \
	mono_profiler_set_ ## name ## _callback (handle, NULL); \
	g_assert (!handle->name ## _cb);
#define MONO_PROFILER_EVENT_0(name, type) \
	_MONO_PROFILER_EVENT(name)
#define MONO_PROFILER_EVENT_1(name, type, arg1_type, arg1_name) \
	_MONO_PROFILER_EVENT(name)
#define MONO_PROFILER_EVENT_2(name, type, arg1_type, arg1_name, arg2_type, arg2_name) \
	_MONO_PROFILER_EVENT(name)
#define MONO_PROFILER_EVENT_3(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name) \
	_MONO_PROFILER_EVENT(name)
#define MONO_PROFILER_EVENT_4(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name, arg4_type, arg4_name) \
	_MONO_PROFILER_EVENT(name)
#include <mono/metadata/profiler-events.h>
#undef MONO_PROFILER_EVENT_0
#undef MONO_PROFILER_EVENT_1
#undef MONO_PROFILER_EVENT_2
#undef MONO_PROFILER_EVENT_3
#undef MONO_PROFILER_EVENT_4
#undef _MONO_PROFILER_EVENT
	}

#define _MONO_PROFILER_EVENT(name, type) \
	g_assert (!mono_profiler_state.name ## _count);
#define MONO_PROFILER_EVENT_0(name, type) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_1(name, type, arg1_type, arg1_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_2(name, type, arg1_type, arg1_name, arg2_type, arg2_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_3(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_4(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name, arg4_type, arg4_name) \
	_MONO_PROFILER_EVENT(name, type)
#include <mono/metadata/profiler-events.h>
#undef MONO_PROFILER_EVENT_0
#undef MONO_PROFILER_EVENT_1
#undef MONO_PROFILER_EVENT_2
#undef MONO_PROFILER_EVENT_3
#undef MONO_PROFILER_EVENT_4
#undef _MONO_PROFILER_EVENT
}

static void
update_callback (volatile gpointer *location, gpointer new_, volatile gint32 *counter)
{
	gpointer old;

	do {
		old = InterlockedReadPointer (location);
	} while (InterlockedCompareExchangePointer (location, new_, old) != old);

	/*
	 * At this point, we could have installed a NULL callback while the counter
	 * is still non-zero, i.e. setting the callback and modifying the counter
	 * is not a single atomic operation. This is fine as we make sure callbacks
	 * are non-NULL before invoking them (see the code below that generates the
	 * raise functions), and besides, updating callbacks at runtime is an
	 * inherently racy operation.
	 */

	if (old)
		InterlockedDecrement (counter);

	if (new_)
		InterlockedIncrement (counter);
}

#define _MONO_PROFILER_EVENT(name, type) \
	void \
	mono_profiler_set_ ## name ## _callback (MonoProfilerHandle handle, MonoProfiler ## type ## Callback cb) \
	{ \
		update_callback (&handle->name ## _cb, (gpointer) cb, &mono_profiler_state.name ## _count); \
	}
#define MONO_PROFILER_EVENT_0(name, type) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_1(name, type, arg1_type, arg1_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_2(name, type, arg1_type, arg1_name, arg2_type, arg2_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_3(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name) \
	_MONO_PROFILER_EVENT(name, type)
#define MONO_PROFILER_EVENT_4(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name, arg4_type, arg4_name) \
	_MONO_PROFILER_EVENT(name, type)
#include <mono/metadata/profiler-events.h>
#undef MONO_PROFILER_EVENT_0
#undef MONO_PROFILER_EVENT_1
#undef MONO_PROFILER_EVENT_2
#undef MONO_PROFILER_EVENT_3
#undef MONO_PROFILER_EVENT_4
#undef _MONO_PROFILER_EVENT

#define _MONO_PROFILER_EVENT(name, type, params, args) \
	void \
	mono_profiler_raise_ ## name params \
	{ \
		for (MonoProfilerHandle h = mono_profiler_state.profilers; h; h = h->next) { \
			MonoProfiler ## type ## Callback cb = h->name ## _cb; \
			if (cb) \
				cb args; \
		} \
	}
#define MONO_PROFILER_EVENT_0(name, type) \
	_MONO_PROFILER_EVENT(name, type, (void), (h->prof))
#define MONO_PROFILER_EVENT_1(name, type, arg1_type, arg1_name) \
	_MONO_PROFILER_EVENT(name, type, (arg1_type arg1_name), (h->prof, arg1_name))
#define MONO_PROFILER_EVENT_2(name, type, arg1_type, arg1_name, arg2_type, arg2_name) \
	_MONO_PROFILER_EVENT(name, type, (arg1_type arg1_name, arg2_type arg2_name), (h->prof, arg1_name, arg2_name))
#define MONO_PROFILER_EVENT_3(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name) \
	_MONO_PROFILER_EVENT(name, type, (arg1_type arg1_name, arg2_type arg2_name, arg3_type arg3_name), (h->prof, arg1_name, arg2_name, arg3_name))
#define MONO_PROFILER_EVENT_4(name, type, arg1_type, arg1_name, arg2_type, arg2_name, arg3_type, arg3_name, arg4_type, arg4_name) \
	_MONO_PROFILER_EVENT(name, type, (arg1_type arg1_name, arg2_type arg2_name, arg3_type arg3_name, arg4_type arg4_name), (h->prof, arg1_name, arg2_name, arg3_name, arg4_name))
#include <mono/metadata/profiler-events.h>
#undef MONO_PROFILER_EVENT_0
#undef MONO_PROFILER_EVENT_1
#undef MONO_PROFILER_EVENT_2
#undef MONO_PROFILER_EVENT_3
#undef MONO_PROFILER_EVENT_4
#undef _MONO_PROFILER_EVENT
