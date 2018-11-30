/**
 * \file
 * Support for verbose unmanaged crash dumps
 *
 * Author:
 *   Alexander Kyte (alkyte@microsoft.com)
 *
 * (C) 2018 Microsoft, Inc.
 *
 */
#include <config.h>
#include <glib.h>
#include <mono/utils/mono-state.h>
#include <mono/utils/mono-threads-coop.h>
#include <mono/metadata/object-internals.h>

#include <sys/param.h>
#include <sys/sysctl.h>
#include <fcntl.h>
#include <utils/mono-threads-debug.h>

#ifndef DISABLE_CRASH_REPORTING

extern GCStats mono_gc_stats;

// For AOT mode
#include <mono/mini/mini-runtime.h>
#include <mono/utils/mono-threads-debug.h>
#include <mono/utils/mono-merp.h>

#ifdef TARGET_OSX
#include <mach/mach.h>
#include <mach/task_info.h>
#endif

#include <sys/param.h>
#include <sys/sysctl.h>
#include <fcntl.h>

typedef struct {
	const char *directory;
	MonoSummaryStage level;
} MonoSummaryTimeline;

static const char *configured_timeline_dir;
static MonoSummaryTimeline log;

static void
file_for_summary_stage (const char *directory, MonoSummaryStage stage, gchar *buff, size_t sizeof_buff)
{
	g_snprintf (buff, sizeof_buff, "%s%scrash_stage_%d", directory, G_DIR_SEPARATOR_S, stage);
}

gboolean
mono_summarize_set_timeline_dir (const char *directory)
{
	if (directory) {
		configured_timeline_dir = strdup (directory);
		return g_ensure_directory_exists (directory);
	} else {
		configured_timeline_dir = NULL;
		return TRUE;
	}
}

void
mono_summarize_timeline_start (void)
{
	memset (&log, 0, sizeof (log));

	if (!configured_timeline_dir)
		return;

	log.level = MonoSummarySetup;
	log.directory = configured_timeline_dir;
}

void
mono_summarize_double_fault_log (void)
{
	char out_file [200];
	file_for_summary_stage (log.directory, MonoSummaryDoubleFault, out_file, sizeof(out_file));
	int handle = g_open (out_file, O_WRONLY | O_CREAT, S_IWUSR | S_IRUSR | S_IRGRP | S_IROTH);
	close(handle);
}

void
mono_summarize_timeline_phase_log (MonoSummaryStage next)
{
	if (log.level == MonoSummaryNone)
		return;

	if (!log.directory)
		return;

	MonoSummaryStage out_level;
	switch (log.level) {
		case MonoSummarySetup:
			out_level = MonoSummarySuspendHandshake;
			break;
		case MonoSummarySuspendHandshake:
			out_level = MonoSummaryDumpTraversal;
			break;
		case MonoSummaryDumpTraversal:
			out_level = MonoSummaryStateWriter;
			break;
		case MonoSummaryStateWriter:
#ifdef TARGET_OSX
			if (mono_merp_enabled ()) {
				out_level = MonoSummaryMerpWriter;
			} else
#endif
			{
				out_level = MonoSummaryCleanup;
			}
			break;
		case MonoSummaryMerpWriter:
			out_level = MonoSummaryMerpInvoke;
			break;
		case MonoSummaryMerpInvoke:
			out_level = MonoSummaryCleanup;
			break;
		case MonoSummaryCleanup:
			out_level = MonoSummaryDone;
			break;

		case MonoSummaryDone:
			MOSTLY_ASYNC_SAFE_PRINTF ("Trying to log crash reporter timeline, already at done %d\n", log.level);
			return;
		default:
			MOSTLY_ASYNC_SAFE_PRINTF ("Trying to log crash reporter timeline, illegal state %d\n", log.level);
			return;
	}

	g_assertf(out_level == next, "Log Error: Log transition to %d, actual expected next step is %d\n", next, out_level);

	char out_file [200];
	memset (out_file, 0, sizeof(out_file));
	file_for_summary_stage (log.directory, out_level, out_file, sizeof(out_file));

	int handle = g_open (out_file, O_WRONLY | O_CREAT, S_IWUSR | S_IRUSR | S_IRGRP | S_IROTH);
	close(handle);

	// To check, comment out normally
	// DO NOT MERGE UNCOMMENTED
	// As this does a lot of FILE io
	//
	// g_assert (out_level == mono_summarize_timeline_read_level (log.directory,  FALSE));

	log.level = out_level;

	if (out_level == MonoSummaryDone)
		memset (&log, 0, sizeof (log));

	return;
}

static gboolean 
timeline_has_level (const char *directory, char *log_file, size_t log_file_size, gboolean clear, MonoSummaryStage stage)
{
	memset (log_file, 0, log_file_size);
	file_for_summary_stage (directory, stage, log_file, log_file_size);
	gboolean exists = g_file_test (log_file, G_FILE_TEST_EXISTS);
	if (clear && exists) 
		remove (log_file);

	return exists;
}

MonoSummaryStage
mono_summarize_timeline_read_level (const char *directory, gboolean clear)
{
	char out_file [200];

	// Make sure that clear gets to erase all of these files if they exist
	gboolean has_level_done = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryDone);
	gboolean has_level_cleanup = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryCleanup);
	gboolean has_level_merp_invoke = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryMerpInvoke);
	gboolean has_level_merp_writer = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryMerpWriter);
	gboolean has_level_state_writer = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryStateWriter);
	gboolean has_level_dump_traversal = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummaryDumpTraversal);
	gboolean has_level_suspend_handshake = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummarySuspendHandshake);
	gboolean has_level_setup = timeline_has_level (directory, out_file, sizeof(out_file), clear, MonoSummarySetup);

	if (has_level_done)
		return MonoSummaryDone;
	else if (has_level_cleanup)
		return MonoSummaryCleanup;
	else if (has_level_merp_invoke)
		return MonoSummaryMerpInvoke;
	else if (has_level_merp_writer)
		return MonoSummaryMerpWriter;
	else if (has_level_state_writer)
		return MonoSummaryStateWriter;
	else if (has_level_dump_traversal)
		return MonoSummaryDumpTraversal;
	else if (has_level_suspend_handshake)
		return MonoSummarySuspendHandshake;
	else if (has_level_setup)
		return MonoSummarySetup;
	else
		return MonoSummaryNone;
}

#define MONO_MAX_SUMMARY_LEN 500000
static gchar output_dump_str [MONO_MAX_SUMMARY_LEN];

static JsonWriter writer;
static GString static_gstr;

static void 
mono_json_writer_init_memory (gchar *output_dump_str, int len)
{
	memset (&static_gstr, 0, sizeof (static_gstr));
	memset (&writer, 0, sizeof (writer));
	memset (output_dump_str, 0, len * sizeof (gchar));

	static_gstr.len = 0;
	static_gstr.allocated_len = len;
	static_gstr.str = output_dump_str;

	writer.indent = 0;
	writer.text = &static_gstr;
}

static void 
mono_json_writer_init_with_static (void) 
{
	return mono_json_writer_init_memory (output_dump_str, MONO_MAX_SUMMARY_LEN);
}

static void 
assert_has_space (void)
{
	// Each individual key/value append should be roughly less than this many characters
	const int margin = 35;

	// Not using static, exit
	if (static_gstr.allocated_len == 0)
		return;

	if (static_gstr.allocated_len - static_gstr.len < margin)
		g_error ("Ran out of memory to create crash dump json blob. Current state:\n%s\n", static_gstr.str);
}

static void
mono_native_state_add_ctx (JsonWriter *writer, MonoContext *ctx)
{
	// Context
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "ctx");
	mono_json_writer_object_begin(writer);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "IP");
	mono_json_writer_printf (writer, "\"%p\",\n", (gpointer) MONO_CONTEXT_GET_IP (ctx));

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "SP");
	mono_json_writer_printf (writer, "\"%p\",\n", (gpointer) MONO_CONTEXT_GET_SP (ctx));

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "BP");
	mono_json_writer_printf (writer, "\"%p\"\n", (gpointer) MONO_CONTEXT_GET_BP (ctx));

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
	mono_json_writer_printf (writer, ",\n");
}

static void
mono_native_state_add_frame (JsonWriter *writer, MonoFrameSummary *frame)
{
	mono_json_writer_indent (writer);
	mono_json_writer_object_begin(writer);

	if (frame->is_managed) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "is_managed");
		mono_json_writer_printf (writer, "\"%s\",\n", frame->is_managed ? "true" : "false");
	}

	if (frame->unmanaged_data.is_trampoline) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "is_trampoline");
		mono_json_writer_printf (writer, "\"true\",");
	}

	if (frame->is_managed) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "guid");
		mono_json_writer_printf (writer, "\"%s\",\n", frame->managed_data.guid);

		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "token");
		mono_json_writer_printf (writer, "\"0x%05x\",\n", frame->managed_data.token);

		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "native_offset");
		mono_json_writer_printf (writer, "\"0x%x\",\n", frame->managed_data.native_offset);

#ifndef MONO_PRIVATE_CRASHES
		if (frame->managed_data.name != NULL) {
			assert_has_space ();
			mono_json_writer_indent (writer);
			mono_json_writer_object_key(writer, "method_name");
			mono_json_writer_printf (writer, "\"%s\",\n", frame->managed_data.name);
		}
#endif

		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "il_offset");
		mono_json_writer_printf (writer, "\"0x%05x\"\n", frame->managed_data.il_offset);

	} else {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key (writer, "native_address");
		if (frame->unmanaged_data.ip) {
			mono_json_writer_printf (writer, "\"0x%" PRIx64 "\"", frame->unmanaged_data.ip);
		} else
			mono_json_writer_printf (writer, "\"unregistered\"");

		if (frame->unmanaged_data.ip) {
			mono_json_writer_printf (writer, ",\n");

			assert_has_space ();
			mono_json_writer_indent (writer);
			mono_json_writer_object_key (writer, "native_offset");
			mono_json_writer_printf (writer, "\"0x%05x\"", frame->unmanaged_data.offset);
		}

		if (frame->unmanaged_data.module) {
			mono_json_writer_printf (writer, ",\n");

			assert_has_space ();
			mono_json_writer_indent (writer);
			mono_json_writer_object_key (writer, "native_module");
			mono_json_writer_printf (writer, "\"%s\"", frame->unmanaged_data.module);
		}

		if (frame->unmanaged_data.has_name) {
			mono_json_writer_printf (writer, ",\n");

			assert_has_space ();
			mono_json_writer_indent (writer);
			mono_json_writer_object_key(writer, "unmanaged_name");
			mono_json_writer_printf (writer, "\"%s\"\n", frame->str_descr);
		} else {
			mono_json_writer_printf (writer, "\n");
		}
	}

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
}

static void
mono_native_state_add_frames (JsonWriter *writer, int num_frames, MonoFrameSummary *frames, const char *label)
{
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, label);

	mono_json_writer_array_begin (writer);

	mono_native_state_add_frame (writer, &frames [0]);
	for (int i = 1; i < num_frames; ++i) {
		mono_json_writer_printf (writer, ",\n");
		mono_native_state_add_frame (writer, &frames [i]);
	}
	mono_json_writer_printf (writer, "\n");

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_array_end (writer);
}

void
mono_native_state_add_thread (JsonWriter *writer, MonoThreadSummary *thread, MonoContext *ctx, gboolean first_thread, gboolean crashing_thread)
{
	assert_has_space ();

	if (!first_thread) {
		mono_json_writer_printf (writer, ",\n");
	}

	mono_json_writer_indent (writer);
	mono_json_writer_object_begin(writer);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "is_managed");
	mono_json_writer_printf (writer, "%s,\n", thread->is_managed ? "true" : "false");

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "crashed");
	mono_json_writer_printf (writer, "%s,\n", crashing_thread ? "true" : "false");

	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "managed_thread_ptr");
	mono_json_writer_printf (writer, "\"0x%x\",\n", (gpointer) thread->managed_thread_ptr);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "thread_info_addr");
	mono_json_writer_printf (writer, "\"0x%x\",\n", (gpointer) thread->info_addr);

	if (thread->error_msg != NULL) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "dumping_error");
		mono_json_writer_printf (writer, "\"%s\",\n", thread->error_msg);
	}

	if (thread->name [0] != '\0') {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "thread_name");
		mono_json_writer_printf (writer, "\"%s\",\n", thread->name);
	}

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "native_thread_id");
	mono_json_writer_printf (writer, "\"0x%x\",\n", (gpointer) thread->native_thread_id);

	if (thread->managed_exc_type) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "managed_exception_type");
		mono_json_writer_printf (writer, "\"%s.%s\",\n", m_class_get_name_space (thread->managed_exc_type), m_class_get_name (thread->managed_exc_type));
	}

	if (ctx)
		mono_native_state_add_ctx (writer, ctx);

	if (thread->num_managed_frames > 0) {
		mono_native_state_add_frames (writer, thread->num_managed_frames, thread->managed_frames, "managed_frames");
	}
	if (thread->num_unmanaged_frames > 0) {
		if (thread->num_managed_frames > 0)
			mono_json_writer_printf (writer, ",\n");
		mono_native_state_add_frames (writer, thread->num_unmanaged_frames, thread->unmanaged_frames, "unmanaged_frames");
	}
	mono_json_writer_printf (writer, "\n");

	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
}

static void
mono_native_state_add_ee_info  (JsonWriter *writer)
{
	// FIXME: setup callbacks to enable
	/*const char *aot_mode;*/
	/*MonoAotMode mono_aot_mode = mono_jit_get_aot_mode ();*/
	/*switch (mono_aot_mode) {*/
		/*case MONO_AOT_MODE_NONE:*/
			/*aot_mode = "none";*/
			/*break;*/
		/*case MONO_AOT_MODE_NORMAL:*/
			/*aot_mode = "normal";*/
			/*break;*/
		/*case MONO_AOT_MODE_HYBRID:*/
			/*aot_mode = "hybrid";*/
			/*break;*/
		/*case MONO_AOT_MODE_FULL:*/
			/*aot_mode = "full";*/
			/*break;*/
		/*case MONO_AOT_MODE_LLVMONLY:*/
			/*aot_mode = "llvmonly";*/
			/*break;*/
		/*case MONO_AOT_MODE_INTERP:*/
			/*aot_mode = "interp";*/
			/*break;*/
		/*case MONO_AOT_MODE_INTERP_LLVMONLY:*/
			/*aot_mode = "interp_llvmonly";*/
			/*break;*/
		/*default:*/
			/*aot_mode = "error";*/
	/*}*/

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "execution_context");
	mono_json_writer_object_begin(writer);

	/*mono_json_writer_indent (writer);*/
	/*mono_json_writer_object_key(writer, "aot_mode");*/
	/*mono_json_writer_printf (writer, "\"%s\",\n", aot_mode);*/

	/*mono_json_writer_indent (writer);*/
	/*mono_json_writer_object_key(writer, "mono_use_llvm");*/
	/*mono_json_writer_printf (writer, "\"%s\",\n", mono_use_llvm ? "true" : "false");*/

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "coop-enabled");
	mono_json_writer_printf (writer, "\"%s\"\n", mono_threads_is_cooperative_suspension_enabled () ? "true" : "false");

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
	mono_json_writer_printf (writer, ",\n");
}

// Taken from driver.c
#if defined(MONO_ARCH_ARCHITECTURE)
/* Redefine MONO_ARCHITECTURE to include more information */
#undef MONO_ARCHITECTURE
#define MONO_ARCHITECTURE MONO_ARCH_ARCHITECTURE
#endif

static char *mono_runtime_build_info;

static void
mono_native_state_add_version (JsonWriter *writer)
{

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "configuration");
	mono_json_writer_object_begin(writer);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "version");
	if (!mono_runtime_build_info)
		mono_runtime_build_info = mono_get_runtime_callbacks ()->get_runtime_build_info ();
	mono_json_writer_printf (writer, "\"%s\",\n", mono_runtime_build_info);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "tlc");
#ifdef MONO_KEYWORD_THREAD
	mono_json_writer_printf (writer, "\"__thread\",\n");
#else
	mono_json_writer_printf (writer, "\"normal\",\n");
#endif /* MONO_KEYWORD_THREAD */

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "sigsgev");
#ifdef MONO_ARCH_SIGSEGV_ON_ALTSTACK
	mono_json_writer_printf (writer, "\"altstack\",\n");
#else
	mono_json_writer_printf (writer, "\"normal\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "notifications");
#ifdef HAVE_EPOLL
	mono_json_writer_printf (writer, "\"epoll\",\n");
#elif defined(HAVE_KQUEUE)
	mono_json_writer_printf (writer, "\"kqueue\",\n");
#else
	mono_json_writer_printf (writer, "\"thread+polling\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "architecture");
	mono_json_writer_printf (writer, "\"%s\",\n", MONO_ARCHITECTURE);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "disabled_features");
	mono_json_writer_printf (writer, "\"%s\",\n", DISABLED_FEATURES);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "smallconfig");
#ifdef MONO_SMALL_CONFIG
	mono_json_writer_printf (writer, "\"enabled\",\n");
#else
	mono_json_writer_printf (writer, "\"disabled\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "bigarrays");
#ifdef MONO_BIG_ARRAYS
	mono_json_writer_printf (writer, "\"enabled\",\n");
#else
	mono_json_writer_printf (writer, "\"disabled\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "softdebug");
#if !defined(DISABLE_SDB)
	mono_json_writer_printf (writer, "\"enabled\",\n");
#else
	mono_json_writer_printf (writer, "\"disabled\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "interpreter");
#ifndef DISABLE_INTERPRETER
	mono_json_writer_printf (writer, "\"enabled\",\n");
#else
	mono_json_writer_printf (writer, "\"disabled\",\n");
#endif

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "llvm_support");
#ifdef MONO_ARCH_LLVM_SUPPORTED
#ifdef ENABLE_LLVM
	mono_json_writer_printf (writer, "\"%d\",\n", LLVM_API_VERSION);
#else
	mono_json_writer_printf (writer, "\"disabled\",\n");
#endif
#endif

	const char *susp_policy = mono_threads_suspend_policy_name ();
	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key (writer, "suspend");
	mono_json_writer_printf (writer, "\"%s\"\n", susp_policy);


	assert_has_space ();
	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
	mono_json_writer_printf (writer, ",\n");
}

static void
mono_native_state_add_memory (JsonWriter *writer)
{
	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "memory");
	mono_json_writer_object_begin(writer);

#ifdef TARGET_OSX
	struct task_basic_info t_info;
	memset (&t_info, 0, sizeof (t_info));
	mach_msg_type_number_t t_info_count = TASK_BASIC_INFO_COUNT;
	task_name_t task = mach_task_self ();
	task_info(task, TASK_BASIC_INFO, (task_info_t) &t_info, &t_info_count);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "Resident Size");
	mono_json_writer_printf (writer, "\"%lu\",\n", t_info.resident_size);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "Virtual Size");
	mono_json_writer_printf (writer, "\"%lu\",\n", t_info.virtual_size);
#endif

	GCStats stats;
	memcpy (&stats, &mono_gc_stats, sizeof (GCStats));

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "minor_gc_time");
	mono_json_writer_printf (writer, "\"%lu\",\n", stats.minor_gc_time);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "major_gc_time");
	mono_json_writer_printf (writer, "\"%lu\",\n", stats.major_gc_time);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "minor_gc_count");
	mono_json_writer_printf (writer, "\"%lu\",\n", stats.minor_gc_count);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "major_gc_count");
	mono_json_writer_printf (writer, "\"%lu\",\n", stats.major_gc_count);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "major_gc_time_concurrent");
	mono_json_writer_printf (writer, "\"%lu\"\n", stats.major_gc_time_concurrent);

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
	mono_json_writer_printf (writer, ",\n");
}

static void
mono_native_state_add_prologue (JsonWriter *writer)
{
	mono_json_writer_object_begin(writer);

	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "protocol_version");
	mono_json_writer_printf (writer, "\"%s\",\n", MONO_NATIVE_STATE_PROTOCOL_VERSION);

	mono_native_state_add_version (writer);

#ifndef MONO_PRIVATE_CRASHES
	mono_native_state_add_ee_info (writer);
#endif

	mono_native_state_add_memory (writer);

	const char *assertion_msg = g_get_assertion_message ();
	if (assertion_msg != NULL) {
		assert_has_space ();
		mono_json_writer_indent (writer);
		mono_json_writer_object_key(writer, "assertion_message");

		size_t length;
		const char *pos;
		if ((pos = strchr (assertion_msg, '\n')) != NULL)
			length = (size_t)(pos - assertion_msg);
		else
			length = strlen (assertion_msg);
		length = MIN (length, INT_MAX);

		mono_json_writer_printf (writer, "\"%.*s\",\n", (int)length, assertion_msg);
	}

	// Start threads array
	assert_has_space ();
	mono_json_writer_indent (writer);
	mono_json_writer_object_key(writer, "threads");
	mono_json_writer_array_begin (writer);
}

static void
mono_native_state_add_epilogue (JsonWriter *writer)
{
	mono_json_writer_indent_pop (writer);
	mono_json_writer_printf (writer, "\n");
	mono_json_writer_indent (writer);
	mono_json_writer_array_end (writer);

	mono_json_writer_indent_pop (writer);
	mono_json_writer_indent (writer);
	mono_json_writer_object_end (writer);
}

void
mono_native_state_init (JsonWriter *writer)
{
	mono_native_state_add_prologue (writer);
}

char *
mono_native_state_emit (JsonWriter *writer)
{
	mono_native_state_add_epilogue (writer);
	return writer->text->str;
}

char *
mono_native_state_free (JsonWriter *writer, gboolean free_data)
{
	mono_native_state_add_epilogue (writer);
	char *output = NULL;

	// Make this interface work like the g_string free does
	if (!free_data)
		output = g_strdup (writer->text->str);

	mono_json_writer_destroy (writer);
	return output;
}

void
mono_summarize_native_state_begin (gchar *mem, int size)
{
	// Shared global mutable memory, only use when VM crashing
	if (!mem)
		mono_json_writer_init_with_static ();
	else
		mono_json_writer_init_memory (mem, size);

	mono_native_state_init (&writer);
}

char *
mono_summarize_native_state_end (void)
{
	return mono_native_state_emit (&writer);
}

void
mono_summarize_native_state_add_thread (MonoThreadSummary *thread, MonoContext *ctx, gboolean crashing_thread)
{
	static gboolean not_first_thread = FALSE;
	mono_native_state_add_thread (&writer, thread, ctx, !not_first_thread, crashing_thread);
	not_first_thread = TRUE;
}

void
mono_crash_dump (const char *jsonFile, MonoStackHash *hashes)
{
	size_t size = strlen (jsonFile);

	gboolean success = FALSE;

	// Save up to 100 dump files for a given stacktrace hash
	for (int increment = 0; increment < 100; increment++) {
		char name [100]; 
		name [0] = '\0';
		g_snprintf (name, sizeof (name), "mono_crash.%" PRIx64 ".%d.json", hashes->offset_free_hash, increment);

		int handle = g_open (name, O_WRONLY | O_CREAT | O_EXCL);
		if (handle == -1) {
			MOSTLY_ASYNC_SAFE_PRINTF ("Couldn't create crash file %s, name may be used. \n", name);
		} else {
			g_write (handle, jsonFile, (guint32) size);
			success = TRUE;
		}

		/*cleanup*/
		if (handle)
			close (handle);

		if (success)
			return;
	}

	return;
}

#endif // DISABLE_CRASH_REPORTING
